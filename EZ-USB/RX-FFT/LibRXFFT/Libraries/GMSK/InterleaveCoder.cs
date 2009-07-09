using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.GMSK
{
    public class InterleaveCoder
    {

        public static bool[][] Deinterleave(bool[][] srcData, bool[][] dstData)
        {
            int numBits = 456;
            int numBlocks = srcData.Length/8;

            if (dstData == null)
            {
                dstData = new bool[numBlocks][];
                for (int pos = 0; pos < dstData.Length; pos++)
                    dstData[pos] = new bool[456];
            }
            else
                numBlocks = dstData.Length;

            for (int outBlock = 0; outBlock < numBlocks; outBlock++)
            {
                for (int outBit = 0; outBit < numBits; outBit++)
                {
                    int inBlock = outBlock * 4 + outBit % srcData.Length;
                    int inBit = 2 * ((49 * outBit) % 57) + ((outBit % 8) / 4);

                    dstData[outBlock][outBit] = srcData[inBlock][inBit];
                }
            }

            return dstData;
        }

        public static bool[][] Interleave(bool[][] srcData, bool[][] dstData)
        {
            int numBits = 456;
            int numBlocks = srcData.Length;

            if (dstData == null)
            {
                dstData = new bool[8 * numBlocks][];
                for (int pos = 0; pos < dstData.Length; pos++)
                    dstData[pos] = new bool[114];
            }

            for (int inBlock = 0; inBlock < numBlocks; inBlock++)
            {
                for (int inBit = 0; inBit < numBits; inBit++)
                {
                    int outBlock = inBlock * 4 + inBit % dstData.Length;
                    int outBit = 2 * ((49 * inBit) % 57) + ((inBit % 8) / 4);

                    dstData[outBlock][outBit] = srcData[inBlock][inBit];
                }
            }

            return dstData;
        }
    }
}
