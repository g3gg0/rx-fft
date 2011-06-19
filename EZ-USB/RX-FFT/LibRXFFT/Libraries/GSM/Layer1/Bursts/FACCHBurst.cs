using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class FACCHBurst : NormalBurst
    {
        public static bool ShowEncryptedMessage = false;
        public static bool DumpEncryptedMessage = false;

        private int FACCHSeq = 0;
        private int SubChannel;

        public FACCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "FACCH";
            ShortName = "FA ";

            InitBuffers(4);
        }

        public FACCHBurst(L3Handler l3, int subChan)
        {
            L3 = l3;
            Name = "FACCH " + subChan;
            ShortName = "FA" + subChan;
            SubChannel = subChan;

            InitBuffers(4);
        }

        public FACCHBurst(L3Handler l3, string name, int subChan)
        {
            L3 = l3;
            Name = name;
            ShortName = "FA" + subChan;
            SubChannel = subChan;

            InitBuffers(4);
        }

        public FACCHBurst(L3Handler l3, string name, int subChan, bool tchType)
        {
            L3 = l3;
            Name = name;
            ShortName = "FA" + subChan;
            SubChannel = subChan;

            InitBuffers(4);
        }

        /* define our own since we want to use private bool[] for c[] bits */
        internal new eCorrectionResult Deconvolution(bool[] burstBufferC)
        {
            int failures = ConvolutionalCoder.Decode(burstBufferC, ref BurstBufferU);

            if (failures == 0)
            {
                return eCorrectionResult.Correct;
            }
            else if (failures < 10)
            {
                return eCorrectionResult.Fixed;
            }
            else
            {
                return eCorrectionResult.Failed;
            }
        }

        public eSuccessState ParseFACCHData(GSMParameters param, bool[] burstBufferC)
        {
            /* decode */
            if (Deconvolution(burstBufferC) == eCorrectionResult.Failed)
            {
                ErrorMessage = "(FACCH: Error in ConvolutionalCoder, maybe encrypted)";
                CryptedFrames++;
                return eSuccessState.Unknown;
            }

            /* CRC check/fix */
            switch (CRCCheck())
            {
                case eCorrectionResult.Fixed:
                    StatusMessage = "(FACCH: CRC Error recovered)";
                    break;

                case eCorrectionResult.Failed:
                    ErrorMessage = "(FACCH: CRC Error)";
                    return eSuccessState.Failed;
            }

            /* convert u[] to d[] bytes */
            PackBytes();
            DataBursts++;

            L2.Handle(param, this, L3, BurstBufferD);

            return eSuccessState.Succeeded;
        }
    }
}
