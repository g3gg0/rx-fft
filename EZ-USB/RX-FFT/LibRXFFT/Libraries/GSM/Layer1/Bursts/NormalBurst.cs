using System;
using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;
using LibRXFFT.Libraries.GSM.Layer2;

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


        public bool[] DummyBurstBits = new[] { true, true, true, true, true, false, true, true, false, true, true, true, false, true, true, false, false, false, false, false, true, false, true, false, false, true, false, false, true, true, true, false, false, false, false, false, true, false, false, true, false, false, false, true, false, false, false, false, false, false, false, true, true, true, true, true, false, false, false, true, true, true, false, false, false, true, false, true, true, true, false, false, false, true, false, true, true, true, false, false, false, true, false, true, false, true, true, true, false, true, false, false, true, false, true, false, false, false, true, true, false, false, true, true, false, false, true, true, true, false, false, true, true, true, true, false, true, false, false, true, true, true, true, true, false, false, false, true, false, false, true, false, true, true, true, true, true, false, true, false, true, false };

        internal bool[] FireCRCBuffer;

        /* interleaved i[] bits */
        internal bool[][] BurstBufferI;

        /* convolutional coded c[] bits */
        internal bool[] BurstBufferC;

        /* data bits u[] */
        internal bool[] BurstBufferU;

        /* resulting data byte array d[] */
        internal byte[] BurstBufferD;


        internal void InitBuffers(int BurstCount)
        {
            BurstBufferI = new bool[BurstCount][];
            BurstBufferC = new bool[456];
            BurstBufferU = new bool[228];
            BurstBufferD = new byte[23];

            for (int pos = 0; pos < BurstBufferI.Length; pos++)
                BurstBufferI[pos] = new bool[114];

            FireCRCBuffer = new bool[CRC.PolynomialFIRE.Length - 1];
        }


        internal bool IsDummy(bool[] decodedBurst)
        {
            for (int pos = 0; pos < DummyBurstBits.Length; pos++)
                if (DummyBurstBits[pos] != decodedBurst[3 + pos])
                    return false;

            return true;
        }

        internal void UnmapToI(bool[] decodedBurst, int dstBurst)
        {
            Array.Copy(decodedBurst, (int)Data1BitsPos, BurstBufferI[dstBurst], 0, (int)Data1Bits);
            Array.Copy(decodedBurst, (int)Data2BitsPos, BurstBufferI[dstBurst], (int)Data1Bits, (int)Data2Bits);
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
    }
}