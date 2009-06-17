using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    public class SACCHBurst : NormalBurst
    {
        private readonly bool[][] SACCHBuffer;
        private long[] FN;
        private int tchSeq = 0;
        private int SubChannel;

        public SACCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "SACCH";
            FN = new long[4];
            SACCHBuffer = new bool[4][];
            for (int pos = 0; pos < SACCHBuffer.Length; pos++)
                SACCHBuffer[pos] = new bool[114];
        }

        public SACCHBurst(L3Handler l3, int subChan)
        {
            L3 = l3;
            Name = "SACCH " + subChan;
            SubChannel = subChan;
            FN = new long[4];
            SACCHBuffer = new bool[4][];
            for (int pos = 0; pos < SACCHBuffer.Length; pos++)
                SACCHBuffer[pos] = new bool[114];
        }

        public SACCHBurst(L3Handler l3, string name, int subChan)
        {
            L3 = l3;
            Name = name;
            SubChannel = subChan;
            FN = new long[4];
            SACCHBuffer = new bool[4][];
            for (int pos = 0; pos < SACCHBuffer.Length; pos++)
                SACCHBuffer[pos] = new bool[114];
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            if (IsDummy(decodedBurst, 3))
            {
                tchSeq = 0;
                return true;
            }

            bool isComplete;

            /* decide between normal SACCH and SACCH/TCH */
            if (param.TN < 2)
            {
                Array.Copy(decodedBurst, 3, SACCHBuffer[sequence], 0, 57);
                Array.Copy(decodedBurst, 88, SACCHBuffer[sequence], 57, 57);

                FN[sequence] = param.FN;

                isComplete = FN[0] + 1 == FN[1] && FN[1] + 1 == FN[2] && FN[2] + 1 == FN[3];
            }
            else
            {
                Array.Copy(decodedBurst, 3, SACCHBuffer[tchSeq], 0, 57);
                Array.Copy(decodedBurst, 88, SACCHBuffer[tchSeq], 57, 57);

                tchSeq++;
                isComplete = tchSeq>3;
            }

            if (isComplete)
            {
                tchSeq = 0;
                bool[][] SACCHData = InterleaveCoder.Deinterleave(SACCHBuffer, null);


                bool[] SACCHDataDeinterleaved = ConvolutionalCoder.Decode(SACCHData[0], null);
                if (SACCHDataDeinterleaved == null)
                {
                    ErrorMessage = "(Error in ConvolutionalCoder)";
                    return true;
                }

                bool[] crc = CRC.Calc(SACCHDataDeinterleaved, 0, 224, CRC.PolynomialFIRE);
                if (!CRC.Matches(crc))
                {
                    ErrorMessage = "(Error in CRC)";
                    return true;
                }

                byte[] data = ByteUtil.BitsToBytesRev(SACCHDataDeinterleaved, 0, 184);

                L2.Handle(this, L3, data);

                Array.Clear(FN, 0, 4);
                Array.Clear(SACCHBuffer[0], 0, SACCHBuffer[0].Length);
                Array.Clear(SACCHBuffer[1], 0, SACCHBuffer[1].Length);
                Array.Clear(SACCHBuffer[2], 0, SACCHBuffer[2].Length);
                Array.Clear(SACCHBuffer[3], 0, SACCHBuffer[3].Length);
            }
            else
                StatusMessage = null;

            return true;
        }
    }
}