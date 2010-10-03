using System;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class SDCCHBurst : NormalBurst
    {
        private int SubChannel;
        

        public SDCCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "SDCCH";
            ShortName = "SD ";

            InitBuffers(4);
        }

        public SDCCHBurst(L3Handler l3, int subChannel)
        {
            L3 = l3;
            Name = "SDCCH " + subChannel;
            ShortName = "SD" + subChannel;
            SubChannel = subChannel;

            InitBuffers(4);
        }

        public SDCCHBurst(L3Handler l3, string name, int subChannel)
        {
            L3 = l3;
            Name = name;
            ShortName = "SD" + subChannel;
            SubChannel = subChannel;

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
                ClearBurstContext();

                /* try to decrypt buffer if this is enabled */
                if (!HandleEncryption(param))
                {
                    /* encrypted but no decryptor available, silently return */
                    return eSuccessState.Unknown;
                }

                /* deinterleave the 4 bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                /* undo convolutional coding c[] to u[] */
                if (!Deconvolution())
                {
                    if (ShowEncryptedMessage || DumpEncryptedMessage)
                    {
                        if (DumpEncryptedMessage)
                        {
                            if (!ChannelEncrypted)
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

                if (ChannelEncrypted && DumpEncryptedMessage)
                {
                    if (StatusMessage != null)
                    {
                        StatusMessage += EncryptionBitString;
                    }
                    else
                    {
                        StatusMessage = EncryptionBitString;
                    }

                    EncryptionBitString = "";
                }

                /* convert u[] to d[] bytes */
                PackBytes();

                DataBursts++;
                L2.Handle(param, this, L3, BurstBufferD);

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