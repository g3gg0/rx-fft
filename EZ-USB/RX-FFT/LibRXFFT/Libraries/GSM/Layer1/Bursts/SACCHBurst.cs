using System;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class SACCHBurst : NormalBurst
    {
        public TCHBurst AssociatedTCH = null;

        public bool TCHType = false;
        private int TCHSeq = 0;
        private int SubChannel;

        public SACCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "SACCH";
            ShortName = "SA ";

            InitBuffers(4);
        }

        public SACCHBurst(L3Handler l3, int subChan)
        {
            L3 = l3;
            Name = "SACCH " + subChan;
            ShortName = "SA" + subChan;
            SubChannel = subChan;

            InitBuffers(4);
        }

        public SACCHBurst(L3Handler l3, string name, int subChan)
        {
            L3 = l3;
            Name = name;
            ShortName = "SA" + subChan;
            SubChannel = subChan;

            InitBuffers(4);
        }

        public SACCHBurst(L3Handler l3, string name, int subChan, bool tchType)
        {
            L3 = l3;
            Name = name;
            ShortName = "SA" + subChan;
            SubChannel = subChan;
            TCHType = tchType;

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
                State = eBurstState.Idle;
                DummyBursts++;

                /* don't treat SACCHs as a reliable source for end-of-connection detection */
                //DummyBurstReceived(param);

                if (DumpRawData)
                {
                    StatusMessage = "Dummy Burst";
                }

                TCHSeq = 0;
                return eSuccessState.Succeeded;
            }

            bool isComplete;

            /* decide between normal SACCH and SACCH/TCH */
            if (!TCHType)
            {
                /* thats a SACCH */

                /* use the normal sequence parameter */
                StoreBurstContext(param, decodedBurst, sequence);

                /* the block is complete when 4 bursts of 4 consecutive FN were buffered */
                isComplete = AllBurstsReceived();
            }
            else
            {
                /* thats a SACCH/TCH */

                /* use an own counter */
                StoreBurstContext(param, decodedBurst, TCHSeq);

                /* when we caught four bursts, the block is complete */
                TCHSeq++;
                isComplete = TCHSeq > 3;
            }

            if (isComplete)
            {
                /* clean up */
                TCHSeq = 0;
                ClearBurstContext();

                /* try to decrypt buffer if this is enabled */
                if (!HandleEncryption(param))
                {
                    State = eBurstState.CryptedTraffic;

                    if (param.ReportL1EncryptionErrors)
                    {
                        StatusMessage = "(Error in decryption)";
                    }

                    /* encrypted but no decryptor available, silently return */
                    return eSuccessState.Unknown;
                }

                /* deinterleave the 4 bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                if (Deconvolution() == eCorrectionResult.Failed)
                {
                    if (!ChannelEncrypted)
                    {
                        StatusMessage = "(Error in ConvolutionalCoder - not encrypted)";
                    }
                    else
                    {
                        StatusMessage = "(Error in ConvolutionalCoder - encrypted, wrong keystream?)";
                    }

                    State = eBurstState.Failed;
                    return eSuccessState.Unknown;
                }

                /* CRC check/fix */
                switch (CRCCheck())
                {
                    case eCorrectionResult.Fixed:
                        StatusMessage = "(CRC Error recovered)";
                        break;

                    case eCorrectionResult.Failed:
                        State = eBurstState.Failed;
                        ErrorMessage = "(CRC Error)";
                        return eSuccessState.Failed;
                }

                if (ChannelEncrypted)
                {
                    State = eBurstState.DecryptedTraffic;
                }
                else
                {
                    State = eBurstState.PlainTraffic;
                }

                if (ChannelEncrypted && DumpEncryptedMessageBits)
                {
                    if (StatusMessage != null)
                    {
                        StatusMessage += EncryptionBitString;
                    }
                    else
                    {
                        StatusMessage = EncryptionBitString;
                    }
                }

                /* convert u[] to d[] bytes */
                PackBytes();

                DataBursts++;

                /* payload starts at octet 3 (GSM 04.04 7.1) */
                L2.Handle(param, this, L3, BurstBufferD, 2);

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