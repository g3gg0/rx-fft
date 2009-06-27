using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GMSK;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    class CBCHBurst : NormalBurst
    {
        private long[] FN;
        private CBCHandler CBCHandler;
        public static eTriState CBCHEnabled = eTriState.Unknown;
        public static int CBCHTimeSlot = 0;
        public static int CBCHSubChannel;
        private int SubChannel;

        public CBCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "CBCH";
            ShortName = "CB ";
            FN = new long[4];

            CBCHandler = new CBCHandler();

            InitArrays();
        }

        public CBCHBurst(L3Handler l3, int subChannel)
        {
            L3 = l3;
            Name = "CBCH " + subChannel;
            ShortName = "CB" + subChannel;
            SubChannel = subChannel;
            FN = new long[4];

            CBCHandler = new CBCHandler();

            InitArrays();
        }

        public CBCHBurst(L3Handler l3, string name, int subChannel)
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
            {
                if (param.DumpPackets)
                    StatusMessage = "Dummy Burst";
                return true;
            }

            Array.Copy(decodedBurst, 3, BurstBuffer[sequence], 0, 57);
            Array.Copy(decodedBurst, 88, BurstBuffer[sequence], 57, 57);

            FN[sequence] = param.FN;

            if (FN[0] + 1 == FN[1] && FN[1] + 1 == FN[2] && FN[2] + 1 == FN[3])
            {
                InterleaveCoder.Deinterleave(BurstBuffer, DataDeinterleaved);

                if (ConvolutionalCoder.DecodeViterbi(DataDeinterleaved[0], DataDecoded) == null)
                {
                    ErrorMessage = "(Error in ConvolutionalCoder)";
                    return false;
                }

                CRC.Calc(DataDecoded, 0, 224, CRC.PolynomialFIRE, CRCBuffer);
                if (!CRC.Matches(CRCBuffer))
                {
                    bool[] DataRepaired = new bool[224];

                    FireCode fc = new FireCode(40, 184);
                    if (!fc.FC_check_crc(DataDecoded, DataRepaired))
                    {
                        ErrorMessage = "(Error in CRC)";
                        return false;
                    }

                    StatusMessage = "(CRC Error recovered)";
                    Array.Copy(DataRepaired, DataDecoded, DataRepaired.Length);
                }


                ByteUtil.BitsToBytesRev(DataDecoded, Data, 0, 184);


                /* do we have a CBCH channel and thats this subchannel? */
                if (CBCHandler.Handle(Data))
                    StatusMessage = CBCHandler.StatusMessage;

                Array.Clear(FN, 0, 4);
            }
            else
                StatusMessage = null;

            return true;
        }
    }
}
