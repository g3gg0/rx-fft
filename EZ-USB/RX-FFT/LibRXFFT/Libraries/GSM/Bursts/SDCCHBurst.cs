using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    public class SDCCHBurst : NormalBurst
    {
        private long[] FN;
        private CBCHandler CBCHandler;
        public static eTriState CBCHEnabled = eTriState.Unknown;
        public static int CBCHTimeSlot = 0;
        public static int CBCHSubChannel;
        private int SubChannel;

        public SDCCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "SDCCH";
            FN = new long[4];

            CBCHandler = new CBCHandler();

            InitArrays();
        }

        public SDCCHBurst(L3Handler l3, int subChannel)
        {
            L3 = l3;
            Name = "SDCCH " + subChannel;
            SubChannel = subChannel;
            FN = new long[4];

            CBCHandler = new CBCHandler();

            InitArrays();
        }

        public SDCCHBurst(L3Handler l3, string name, int subChannel)
        {
            L3 = l3;
            Name = name;
            SubChannel = subChannel;
            FN = new long[4];

            CBCHandler = new CBCHandler();

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

            FN[sequence] = param.FN;

            if (FN[0] + 1 == FN[1] && FN[1] + 1 == FN[2] && FN[2] + 1 == FN[3])
            {
                InterleaveCoder.Deinterleave(BurstBuffer, DataDeinterleaved);

                if (ConvolutionalCoder.Decode(DataDeinterleaved[0], DataDecoded) == null)
                {
                    ErrorMessage = "(Error in ConvolutionalCoder, maybe an encrypted packet)";
                    return true;
                }

                CRC.Calc(DataDecoded, 0, 224, CRC.PolynomialFIRE, CRCBuffer);
                if (!CRC.Matches(CRCBuffer))
                {
                    ErrorMessage = "(Error in CRC)";
                    return false;
                }

                ByteUtil.BitsToBytesRev(DataDecoded, Data, 0, 184);


                /* do we have a CBCH channel and thats this subchannel? */
                if (CBCHEnabled == eTriState.Yes && SubChannel == CBCHSubChannel)
                {
                    if (CBCHandler.Handle(Data))
                        StatusMessage = CBCHandler.StatusMessage;

                    return true;
                }


                L2.Handle(this, L3, Data);

                Array.Clear(FN, 0, 4);
            }
            else
                StatusMessage = null;

            return true;
        }
    }
}