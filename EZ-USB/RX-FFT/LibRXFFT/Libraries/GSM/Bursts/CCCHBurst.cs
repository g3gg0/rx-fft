using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    public class CCCHBurst : NormalBurst
    {
        public CCCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "CCCH";
            InitArrays();
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            if (IsDummy(decodedBurst, 3))
                return true;

            Array.Copy(decodedBurst, 3, BurstBuffer[sequence], 0, 57);
            Array.Copy(decodedBurst, 88, BurstBuffer[sequence], 57, 57);

            if (sequence == 3)
            {
                InterleaveCoder.Deinterleave(BurstBuffer, DataDeinterleaved);

                if (ConvolutionalCoder.Decode(DataDeinterleaved[0], DataDecoded) == null)
                {
                    ErrorMessage = "(Error in ConvolutionalCoder)";
                    return false;
                }

                CRC.Calc(DataDecoded, 0, 224, CRC.PolynomialFIRE, CRCBuffer);
                if (!CRC.Matches(CRCBuffer))
                {
                    ErrorMessage = "(Error in CRC)";
                    return false;
                }

                ByteUtil.BitsToBytesRev(DataDecoded, Data, 0, 184);

                if ((Data[0] & 3) != 1)
                {
                    ErrorMessage = "(Error in L2 Pseudo Length)";
                    return false;
                }

                L2.Handle(this, L3, Data);
            }
            else
                StatusMessage = null;


            return true;
        }
    }
}