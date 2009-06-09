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
            int numBlocks = srcData.Length/4;

            if (dstData == null)
            {
                dstData = new bool[numBlocks][];
                for (int pos = 0; pos < dstData.Length; pos++)
                    dstData[pos] = new bool[456];
            }

            for (int inBlock = 0; inBlock < numBlocks; inBlock++)
            {
                for (int bitPos = 0; bitPos < numBits; bitPos++)
                {
                    int outBlock = inBlock * 4 + bitPos % 4;
                    int dstPos = 2 * ((49 * bitPos) % 57) + ((bitPos % 8) / 4);

                    dstData[inBlock][bitPos] = srcData[outBlock][dstPos];
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
                dstData = new bool[4 * numBlocks][];
                for (int pos = 0; pos < dstData.Length; pos++)
                    dstData[pos] = new bool[114];
            }

            for (int inBlock = 0; inBlock < numBlocks; inBlock++)
            {
                for (int bitPos = 0; bitPos < numBits; bitPos++)
                {
                    int outBlock = inBlock * 4 + bitPos % 4;
                    int dstPos = 2 * ((49 * bitPos) % 57) + ((bitPos % 8) / 4);

                    dstData[outBlock][dstPos] = srcData[inBlock][bitPos];
                }
            }

            return dstData;
        }
    }
}
