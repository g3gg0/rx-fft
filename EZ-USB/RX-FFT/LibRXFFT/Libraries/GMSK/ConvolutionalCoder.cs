using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.GMSK
{
    public class ConvolutionalCoder
    {
        private static bool Bit(bool[] srcData, int pos)
        {
            if (pos < 0)
                return false;

            return srcData[pos];
        }

        public static bool[] Decode(bool[] srcData, bool[] dstData)
        {
            if (dstData == null)
                dstData = new bool[srcData.Length / 2];

            for (int bitPos = 0; bitPos < srcData.Length / 2; bitPos++)
            {
                bool bit1 = Bit(srcData, 2 * bitPos) ^ Bit(dstData, bitPos - 3) ^ Bit(dstData, bitPos - 4);
                bool bit2 = Bit(srcData, 2 * bitPos + 1) ^ Bit(dstData, bitPos - 1) ^ Bit(dstData, bitPos - 3) ^ Bit(dstData, bitPos - 4);
                
                if (bit1 != bit2)
                    return null;
                
                dstData[bitPos] = bit1;
            }

            return dstData;
        }

        public static bool[] Decode(bool[] srcData)
        {
            return Decode(srcData, null);
        }
    }
}
