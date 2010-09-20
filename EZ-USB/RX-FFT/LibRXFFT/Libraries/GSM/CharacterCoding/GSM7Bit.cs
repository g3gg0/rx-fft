namespace LibRXFFT.Libraries.GSM.CharacterCoding
{
    public class GSM7Bit
    {
        public static string Decode(byte[] srcData)
        {
            return Decode(srcData, 0, srcData.Length);
        }

        public static string Decode(byte[] srcData, int start, int length)
        {
            int byteCount = (length * 8) / 7;
            string message = "";


            for (int pos = 0; pos < byteCount; pos++)
            {
                int bitPos = pos * 7;
                int bytePos = bitPos / 8;
                int bitOffset = bitPos % 8;

                int character = (srcData[start + bytePos] >> bitOffset);
                if (bitOffset > 1)
                {
                    int shiftAmount = (8 - bitOffset);
                    int nextPart = srcData[start + bytePos + 1];
                    int bitMask = (1 << (7 - shiftAmount)) - 1;

                    nextPart &= bitMask;
                    nextPart <<= shiftAmount;

                    character |= nextPart;
                }

                character &= 0x7F;

                if (character == 0x0d)
                    message += "\\r";
                else if (character == 0x0a)
                    message += "\\n";
                else
                    message += (char)character;
            }

            return message;
        }

    }
}
