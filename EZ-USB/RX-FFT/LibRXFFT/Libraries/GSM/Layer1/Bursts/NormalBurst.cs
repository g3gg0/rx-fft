using System;
using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;
using LibRXFFT.Libraries.GSM.Layer2;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class NormalBurst : Burst
    {
        internal enum eCRCState
        {
            Correct,
            Fixed,
            Failed
        }

        internal class BurstData
        {
            /* encrypted e[] bits */
            internal bool[] BurstBufferE = new bool[114];

            /* interleaved i[] bits */
            internal bool[] BurstBufferI = new bool[114];

            /* frame number */
            internal long FN;

            /* A5 related frame number */
            internal uint Count;
        }

        public const double Data1Bits = 57;
        public const double HLBits = 1;
        public const double SyncBits = 26;
        public const double HUBits = 1;
        public const double Data2Bits = 57;

        public const double Data1BitsPos = LeadingTailBits;
        public const double HLBitsPos = Data1BitsPos + Data1Bits;
        public const double SyncBitsPos = HLBitsPos + HLBits;
        public const double HUBitsPos = SyncBitsPos + SyncBits;
        public const double Data2BitsPos = HUBitsPos + HUBits;

        public long DummyBursts = 0;
        public long DataBursts = 0;
        public long CryptedBursts = 0;


        public static bool[] DummyBurstBits = new[] { true, true, true, true, true, false, true, true, false, true, true, true, false, true, true, false, false, false, false, false, true, false, true, false, false, true, false, false, true, true, true, false, false, false, false, false, true, false, false, true, false, false, false, true, false, false, false, false, false, false, false, true, true, true, true, true, false, false, false, true, true, true, false, false, false, true, false, true, true, true, false, false, false, true, false, true, true, true, false, false, false, true, false, true, false, true, true, true, false, true, false, false, true, false, true, false, false, false, true, true, false, false, true, true, false, false, true, true, true, false, false, true, true, true, true, false, true, false, false, true, true, true, true, true, false, false, false, true, false, false, true, false, true, true, true, true, true, false, true, false, true, false };

        internal bool[] FireCRCBuffer;


        /* Burstblock contains 4 or 8 bursts and their context */
        internal BurstData[] BurstBlock;


        /* interleaved i[] bits, references to BurstBlock */
        internal bool[][] BurstBufferI;

        /* convolutional coded c[] bits */
        internal bool[] BurstBufferC;

        /* data bits u[] */
        internal bool[] BurstBufferU;

        /* resulting data byte array d[] */
        internal byte[] BurstBufferD;


        internal void InitBuffers(int burstCount)
        {
            BurstBufferI = new bool[burstCount][];
            BurstBufferC = new bool[456];
            BurstBufferU = new bool[228];
            BurstBufferD = new byte[23];

            BurstBlock = new BurstData[burstCount];

            for (int pos = 0; pos < BurstBlock.Length; pos++)
            {
                BurstBlock[pos] = new BurstData();
                BurstBufferI[pos] = BurstBlock[pos].BurstBufferI;
            }

            FireCRCBuffer = new bool[CRC.PolynomialFIRE.Length - 1];
        }


        internal bool IsDummy(bool[] decodedBurst)
        {
            for (int pos = 0; pos < DummyBurstBits.Length; pos++)
                if (DummyBurstBits[pos] != decodedBurst[3 + pos])
                    return false;

            return true;
        }

        internal void StoreBurstContext(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            /* get 114 e[] bits from burst into our buffer. two bits are just stealing flags - skip them */
            UnmapToE(decodedBurst, sequence);

            /* store context */
            BurstBlock[sequence].FN = param.FN;
            BurstBlock[sequence].Count = param.Count;
        }

        internal bool AllBurstsReceived()
        {
            for (int pos = 1; pos < BurstBlock.Length; pos++)
            {
                if ((BurstBlock[pos - 1].FN + 1) != BurstBlock[pos].FN)
                {
                    return false;
                }
            }

            return true;
        }

        internal void ClearBurstContext()
        {
            for (int pos = 0; pos < BurstBlock.Length; pos++)
            {
                BurstBlock[pos].FN = 0;
            }
        }

        internal void UnmapToI(bool[] bits, int dstBurst)
        {
            Array.Copy(bits, (int)Data1BitsPos, BurstBlock[dstBurst].BurstBufferI, 0, (int)Data1Bits);
            Array.Copy(bits, (int)Data2BitsPos, BurstBlock[dstBurst].BurstBufferI, (int)Data1Bits, (int)Data2Bits);
        }

        internal void UnmapToE(bool[] bits, int dstBurst)
        {
            Array.Copy(bits, (int)Data1BitsPos, BurstBlock[dstBurst].BurstBufferE, 0, (int)Data1Bits);
            Array.Copy(bits, (int)Data2BitsPos, BurstBlock[dstBurst].BurstBufferE, (int)Data1Bits, (int)Data2Bits);
        }

        internal void CopyEToI()
        {
            for (int dstBurst = 0; dstBurst < BurstBlock.Length; dstBurst++)
            {
                Array.Copy(BurstBlock[dstBurst].BurstBufferE, BurstBlock[dstBurst].BurstBufferI, (int)(Data1Bits + Data2Bits));
            }
        }

        internal void Deinterleave()
        {
            InterleaveCoder.Deinterleave(BurstBufferI, new[] { BurstBufferC });
        }

        internal bool Deconvolution()
        {
            return ConvolutionalCoder.Decode(BurstBufferC, BurstBufferU) != null;
        }

        internal eCRCState CRCCheck()
        {
            CRC.Calc(BurstBufferU, 0, 224, CRC.PolynomialFIRE, FireCRCBuffer);
            if (!CRC.Matches(FireCRCBuffer))
            {
                bool[] DataRepaired = new bool[224];

                FireCode fc = new FireCode(40, 184);
                if (!fc.FC_check_crc(BurstBufferU, DataRepaired))
                    return eCRCState.Failed;

                Array.Copy(DataRepaired, BurstBufferU, DataRepaired.Length);

                return eCRCState.Fixed;
            }
            return eCRCState.Correct;
        }

        internal void PackBytes()
        {
            ByteUtil.BitsToBytesRev(BurstBufferU, BurstBufferD, 0, 184);
        }

        internal bool IsHL(bool[] decodedBurst)
        {
            return decodedBurst[(int)HLBitsPos];
        }

        internal bool IsHU(bool[] decodedBurst)
        {
            return decodedBurst[(int)HUBitsPos];
        }


        /* for burst types which allow encryption */
        public bool ChannelEncrypted = false;
        public int EncryptionType = 0;
        public static bool ShowEncryptedMessage = false;
        public static bool DumpEncryptedMessage = false;
        public string EncryptionBitString = "";
        

        internal bool HandleEncryption(GSMParameters param)
        {
            /* this channel was flagged as encrypted */
            if (ChannelEncrypted)
            {
                CryptedBursts++;

                /* do we have an A5 decryptor? */
                if (param.A5AlgorithmAvailable)
                {
                    /* now decrypt all 4 bursts */
                    for (int dstBurst = 0; dstBurst < BurstBlock.Length; dstBurst++)
                    {
                        if (DumpEncryptedMessage)
                        {
                            EncryptionBitString += "Burst #" + dstBurst + " (Encrypted) e[]: " + DumpBits(BurstBlock[dstBurst].BurstBufferE);
                        }

                        /* update COUNT and let it decrypt our burst */
                        param.A5Algorithm.CryptDownlink(BurstBlock[dstBurst].BurstBufferE, BurstBlock[dstBurst].Count);

                        if (DumpEncryptedMessage)
                        {
                            EncryptionBitString += " (Decrypted) e[]: " + DumpBits(BurstBlock[dstBurst].BurstBufferE) + Environment.NewLine;
                        }
                    }

                    return true;
                }
                else if (DumpEncryptedMessage)
                {
                    StatusMessage = "";

                    /* no decryption available - just dump if requested */
                    for (int dstBurst = 0; dstBurst < BurstBlock.Length; dstBurst++)
                    {
                        StatusMessage += "Burst #" + dstBurst + " (Encrypted) e[]: " + DumpBits(BurstBlock[dstBurst].BurstBufferE) + Environment.NewLine;
                    }
                }
                return false;
            }

            return true;
        }
    }
}