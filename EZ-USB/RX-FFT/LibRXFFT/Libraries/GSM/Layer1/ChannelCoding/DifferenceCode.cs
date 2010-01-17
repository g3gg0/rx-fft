namespace LibRXFFT.Libraries.GSM.Layer1.ChannelCoding
{
    public static class DifferenceCode
    {
        public static bool[] Encode(bool[] srcData)
        {
            return Encode(srcData, null);
        }

        public static bool[] Encode(bool[] srcData, bool[] dstData)
        {
            return Encode(srcData, dstData, false);
        }

        public static bool[] Encode(bool[] srcData, bool[] dstData, bool lastBit)
        {
            if (dstData == null)
                dstData = new bool[srcData.Length];

            for (int bitPos = 0; bitPos < srcData.Length; bitPos++)
            {
                bool bitSet = srcData[bitPos];

                if (lastBit == bitSet)
                    dstData[bitPos] = true;
                else
                    dstData[bitPos] = false;

                lastBit = bitSet;
            }

            return dstData;
        }

        public static bool[] Decode(bool[] srcData)
        {
            return Decode(srcData, null);
        }

        public static bool[] Decode(bool[] srcData, bool[] dstData)
        {
            return Decode(srcData, dstData, false);
        }

        public static bool[] Decode(bool[] srcData, bool[] dstData, bool lastBit)
        {
            if (dstData == null)
                dstData = new bool[srcData.Length];

            for (int pos = 0; pos < srcData.Length; pos++)
            {
                if (!srcData[pos])
                    lastBit = !lastBit;

                dstData[pos] = lastBit;
            }


            return dstData;
        }
    }
}
