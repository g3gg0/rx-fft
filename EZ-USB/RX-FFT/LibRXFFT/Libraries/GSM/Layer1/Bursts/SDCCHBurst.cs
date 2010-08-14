using System;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class SDCCHBurst : NormalBurst
    {
        public bool EncryptionState = false;
        public int EncryptionType = 0;
        public static bool ShowEncryptedMessage = false;
        public static bool DumpEncryptedMessage = false;
        
        private long[] FN;
        private int SubChannel;

        public SDCCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "SDCCH";
            ShortName = "SD ";
            FN = new long[4];

            InitBuffers(4);
        }

        public SDCCHBurst(L3Handler l3, int subChannel)
        {
            L3 = l3;
            Name = "SDCCH " + subChannel;
            ShortName = "SD" + subChannel;
            SubChannel = subChannel;
            FN = new long[4];

            InitBuffers(4);
        }

        public SDCCHBurst(L3Handler l3, string name, int subChannel)
        {
            L3 = l3;
            Name = name;
            ShortName = "SD" + subChannel;
            SubChannel = subChannel;
            FN = new long[4];

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

            /* get 114 e[] bits from burst into our buffer. two bits are just stealing flags */
            UnmapToE(decodedBurst);

            if (EncryptionState && DumpEncryptedMessage)
            {
                StatusMessage = "(Encrypted) e[]: ";
                DumpBits(BurstBufferE);
                //return eSuccessState.Unknown;
            }

            CopyEToI(sequence);
            //UnmapToI(decodedBurst, sequence);

            FN[sequence] = param.FN;

            if (FN[0] + 1 == FN[1] && FN[1] + 1 == FN[2] && FN[2] + 1 == FN[3])
            {
                Array.Clear(FN, 0, 4);

                /* deinterleave the 4 bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                /* undo convolutional coding c[] to u[] */
                if (!Deconvolution())
                {
                    if (ShowEncryptedMessage || DumpEncryptedMessage)
                    {
                        if (DumpEncryptedMessage)
                        {
                            if (!EncryptionState)
                            {
                                StatusMessage = "(Encrypted?) De-Interleaved bits: ";
                                DumpBits(BurstBufferC);
                            }
                            else
                            {
                                // we expected this to happen
                            }
                        }
                        else
                        {
                            StatusMessage = "(Error in ConvolutionalCoder, maybe encrypted)";
                        }
                    }
                    CryptedBursts++;
                    return eSuccessState.Unknown;
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

                DataBursts++;
                L2.Handle(this, L3, BurstBufferD);

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