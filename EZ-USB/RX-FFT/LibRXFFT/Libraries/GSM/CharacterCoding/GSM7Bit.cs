using System;
namespace LibRXFFT.Libraries.GSM.CharacterCoding
{
    public class GSM7Bit
    {
        private static char[] GsmCharacters = "@£$¥èéùìòÇ\nØø\rÅå∆_ΦΓΛΩΠΨΣΘΞ\x1bÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ`¿abcdefghijklmnopqrstuvwxyzäöñüà".ToCharArray();
        private static char[] GsmCharactersExt = "``````````®`````````^``````℗````````````{}`````\\````````````[~]`|````````````````````````````````````€``````````````````````````".ToCharArray();

        public static string Decode(byte[] srcData)
        {
            return Decode(srcData, 0, (srcData.Length * 8) / 7, 0);
        }

        public static string Decode(byte[] srcData, int start, int charCount, int skipChars)
        {
            try
            {
                string message = "";
                char translated = ' ';

                for (int pos = skipChars; pos < charCount; pos++)
                {
                    int bitPos = pos * 7;
                    int bytePos = bitPos / 8;
                    int bitOffset = bitPos % 8;

                    int character = (srcData[start + bytePos] >> bitOffset) & 0x7F;
                    if (bitOffset > 1)
                    {
                        int shiftAmount = (8 - bitOffset);
                        int nextPart = srcData[start + bytePos + 1];
                        int bitMask = (1 << (7 - shiftAmount)) - 1;

                        nextPart &= bitMask;
                        nextPart <<= shiftAmount;

                        character |= nextPart;
                    }

                    /* last character was some translated? */
                    if (translated == '\x1B')
                    {
                        translated = GsmCharactersExt[character];
                    }
                    else
                    {
                        translated = GsmCharacters[character];
                    }

                    if (translated != '\x1B')
                    {
                        if (translated == '\r')
                        {
                            message += "\\r";
                        }
                        else if (translated == '\n')
                        {
                            message += "\\n";
                        }
                        else
                        {
                            message += translated;
                        }
                    }
                }

                return message;
            }
            catch (Exception e)
            {
                return "(7 bit decode failed)";
            }
        }
    }
}
