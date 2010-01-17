using System;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
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

            InitBuffers(4);
        }

        public CBCHBurst(L3Handler l3, int subChannel)
        {
            L3 = l3;
            Name = "CBCH " + subChannel;
            ShortName = "CB" + subChannel;
            SubChannel = subChannel;
            FN = new long[4];

            CBCHandler = new CBCHandler();

            InitBuffers(4);
        }

        public CBCHBurst(L3Handler l3, string name, int subChannel)
        {
            L3 = l3;
            Name = name;
            SubChannel = subChannel;
            FN = new long[4];

            CBCHandler = new CBCHandler();

            InitBuffers(4);
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            if (IsDummy(decodedBurst))
            {
                if (DumpRawData)
                    StatusMessage = "Dummy Burst";
                return eSuccessState.Succeeded;
            }

            UnmapToI(decodedBurst, sequence);

            FN[sequence] = param.FN;

            if (FN[0] + 1 == FN[1] && FN[1] + 1 == FN[2] && FN[2] + 1 == FN[3])
            {
                Array.Clear(FN, 0, 4);

                /* deinterleave the 4 bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                if (!Deconvolution())
                {
                    ErrorMessage = "(Error in ConvolutionalCoder)";
                    return eSuccessState.Failed;
                }

                /* CRC check/fix */
                switch (CRCCheck())
                {
                    case eCRCState.Fixed:
                        StatusMessage = "(CRC Error recovered)";
                        break;

                    case eCRCState.Failed:
                        ErrorMessage = "(CRC Error)";
                        return eSuccessState.Failed;
                }

                /* convert u[] to d[] bytes */
                PackBytes();

                /* do we have a CBCH channel and thats this subchannel? */
                if (CBCHandler.Handle(BurstBufferD))
                    StatusMessage = CBCHandler.StatusMessage;

                return eSuccessState.Succeeded;
            }
            else
                StatusMessage = null;

            return eSuccessState.Unknown;
        }
    }
}
