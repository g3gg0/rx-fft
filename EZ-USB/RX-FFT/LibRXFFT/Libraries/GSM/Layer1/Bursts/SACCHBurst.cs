using System;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class SACCHBurst : NormalBurst
    {
        public bool EncryptionState = false;
        public int EncryptionType = 0;
        public TCHBurst AssociatedTCH = null;

        public static bool ShowEncryptedMessage = false;
        public static bool DumpEncryptedMessage = false;

        public bool TCHType = false;

        private long[] FN;
        private int TCHSeq = 0;
        private int SubChannel;

        public SACCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "SACCH";
            ShortName = "SA ";
            FN = new long[4];

            InitBuffers(4);
        }

        public SACCHBurst(L3Handler l3, int subChan)
        {
            L3 = l3;
            Name = "SACCH " + subChan;
            ShortName = "SA" + subChan;
            SubChannel = subChan;
            FN = new long[4];

            InitBuffers(4);
        }

        public SACCHBurst(L3Handler l3, string name, int subChan)
        {
            L3 = l3;
            Name = name;
            ShortName = "SA" + subChan;
            SubChannel = subChan;
            FN = new long[4];

            InitBuffers(4);
        }

        public SACCHBurst(L3Handler l3, string name, int subChan, bool tchType)
        {
            L3 = l3;
            Name = name;
            ShortName = "SA" + subChan;
            SubChannel = subChan;
            FN = new long[4];
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
                DummyBursts++;

                if (DumpRawData)
                    StatusMessage = "Dummy Burst";

                TCHSeq = 0;
                return eSuccessState.Succeeded;
            }

            /* get the e[] bits from burst - without stealing flags */
            UnmapToE(decodedBurst);
            //Array.Copy(decodedBurst, 3, BurstBufferE, 0, 57);
            //Array.Copy(decodedBurst, 88, BurstBufferE, 57, 57);

            if (EncryptionState && DumpEncryptedMessage)
            {
                StatusMessage = "(Encrypted) e[]: ";
                DumpBits(BurstBufferE);
                return eSuccessState.Unknown;
            }            

            bool isComplete;

            /* decide between normal SACCH and SACCH/TCH */
            if (!TCHType)
            {
                /* thats a normal SACCH. use the 114 e[] bits as i[] */
                CopyEToI(sequence);
                //Array.Copy(BurstBufferE, BurstBufferI[sequence], 114);
                //UnmapToI(decodedBurst, sequence);

                FN[sequence] = param.FN;

                /* the block is complete when 4 bursts of 4 consecutive FN were buffered */
                isComplete = (FN[0] + 1 == FN[1]) && (FN[1] + 1 == FN[2]) && (FN[2] + 1 == FN[3]);
            }
            else
            {
                /* thats a SACCH/TCH. use the 114 e[] bits as i[] */
                CopyEToI(TCHSeq);
                //Array.Copy(BurstBufferE, BurstBufferI[TCHSeq], 114);
                //Array.Copy(decodedBurst, 3, BurstBufferI[TCHSeq], 0, 57);
                //Array.Copy(decodedBurst, 88, BurstBufferI[TCHSeq], 57, 57);

                /* when we caught four bursts, the block is complete */
                TCHSeq++;
                isComplete = TCHSeq > 3;
            }

            if (isComplete)
            {
                /* clean up */
                TCHSeq = 0;
                Array.Clear(FN, 0, 4);

                /* deinterleave the 4 bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                if (!Deconvolution())
                {
                    if (ShowEncryptedMessage || DumpEncryptedMessage)
                    {
                        if (DumpEncryptedMessage)
                        {
                            StatusMessage = "(Encrypted) De-Interleaved bits: ";
                            DumpBits(BurstBufferC);
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

                /* payload starts at octet 3 (GSM 04.04 7.1) */
                L2.Handle(this, L3, BurstBufferD, 2);

                return eSuccessState.Succeeded;
            }
            else
                StatusMessage = null;

            return eSuccessState.Unknown;
        }
    }
}