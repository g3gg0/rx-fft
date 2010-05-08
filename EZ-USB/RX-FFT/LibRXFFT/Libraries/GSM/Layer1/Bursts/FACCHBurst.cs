using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    class FACCHBurst : NormalBurst
    {
        public static bool ShowEncryptedMessage = false;
        public static bool DumpEncryptedMessage = false;

        private long[] FN;
        private int FACCHSeq = 0;
        private int SubChannel;

        public FACCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "FACCH";
            ShortName = "FA ";
            FN = new long[4];

            InitBuffers(4);
        }

        public FACCHBurst(L3Handler l3, int subChan)
        {
            L3 = l3;
            Name = "FACCH " + subChan;
            ShortName = "FA" + subChan;
            SubChannel = subChan;
            FN = new long[4];

            InitBuffers(4);
        }

        public FACCHBurst(L3Handler l3, string name, int subChan)
        {
            L3 = l3;
            Name = name;
            ShortName = "FA" + subChan;
            SubChannel = subChan;
            FN = new long[4];

            InitBuffers(4);
        }

        public FACCHBurst(L3Handler l3, string name, int subChan, bool tchType)
        {
            L3 = l3;
            Name = name;
            ShortName = "FA" + subChan;
            SubChannel = subChan;
            FN = new long[4];

            InitBuffers(4);
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] burstBufferC, int sequence)
        {
            /* decode */
            if (ConvolutionalCoder.Decode(burstBufferC, BurstBufferU) == null)
            {
                ErrorMessage = "(FACCH: Error in ConvolutionalCoder, maybe encrypted)";
                CryptedBursts++;
                return eSuccessState.Unknown;
            }

            /* CRC check/fix */
            switch (CRCCheck())
            {
                case eCRCState.Fixed:
                    StatusMessage = "(FACCH: CRC Error recovered)";
                    break;

                case eCRCState.Failed:
                    ErrorMessage = "(FACCH: CRC Error)";
                    return eSuccessState.Failed;
            }

            /* convert u[] to d[] bytes */
            PackBytes();
            DataBursts++;

            L2.Handle(this, L3, BurstBufferD);

            return eSuccessState.Succeeded;
        }
    }
}
