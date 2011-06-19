using System;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    class CBCHBurst : NormalBurst
    {
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

            CBCHandler = new CBCHandler();

            InitBuffers(4);
        }

        public CBCHBurst(L3Handler l3, int subChannel)
        {
            L3 = l3;
            Name = "CBCH " + subChannel;
            ShortName = "CB" + subChannel;
            SubChannel = subChannel;

            CBCHandler = new CBCHandler();

            InitBuffers(4);
        }

        public CBCHBurst(L3Handler l3, string name, int subChannel)
        {
            L3 = l3;
            Name = name;
            SubChannel = subChannel;

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
                DummyBursts++;
                if (DumpRawData)
                    StatusMessage = "Dummy Burst";
                return eSuccessState.Succeeded;
            }

            StoreBurstContext(param, decodedBurst, sequence);

            /* if FNs are consecutive */
            if (AllBurstsReceived())
            {
                /* clean up */
                ClearBurstContext();

                /* get all e[] bits and place in i[] */
                CopyEToI();

                /* deinterleave the 4 bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                if (Deconvolution() == eCorrectionResult.Failed)
                {
                    ErrorMessage = "(Error in ConvolutionalCoder)";
                    return eSuccessState.Failed;
                }

                /* CRC check/fix */
                switch (CRCCheck())
                {
                    case eCorrectionResult.Fixed:
                        StatusMessage = "(CRC Error recovered)";
                        break;

                    case eCorrectionResult.Failed:
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
            {
                StatusMessage = null;
            }

            return eSuccessState.Unknown;
        }
    }
}
