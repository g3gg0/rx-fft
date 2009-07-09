using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries
{
    public class ByteUtil
    {


        public static byte[] BitsToBytesRev(bool[] srcData)
        {
            return BitsToBytesRev(srcData, null, 0, srcData.Length);
        }

        public static byte[] BitsToBytesRev(bool[] srcData, int startPos, int bits)
        {
            return BitsToBytesRev(srcData, null, startPos, bits);
        }

        public static byte[] BitsToBytesRev(bool[] srcData, byte[] dstData)
        {
            return BitsToBytesRev(srcData, dstData, 0, srcData.Length);
        }

        public static byte[] BitsToBytesRev(bool[] srcData, byte[] dstData, int startPos, int bits)
        {
            int byteCount = (bits + 7) / 8;
            int leadingBits = 0;// (byteCount * 8) - srcData.Length;

            if (dstData == null)
                dstData = new byte[byteCount];

            for (int bytePos = 0; bytePos < bits / 8; bytePos++)
            {
                byte outByte = 0;

                for (int bitPos = 0; bitPos < 8; bitPos++)
                {
                    /* when handling the first bit, skip some bits if not aligned properly */
                    if (bitPos == 0 && bytePos == 0)
                        bitPos = leadingBits;

                    byte bitValue = (byte)(1 << bitPos);
                    bool bitSet = srcData[startPos + bytePos * 8 + bitPos];

                    if (bitSet)
                        outByte |= bitValue;
                }

                dstData[bytePos] = outByte;
            }

            return dstData;
        }


        public static byte[] BitsToBytes(bool[] srcData)
        {
            return BitsToBytes(srcData, null, 0, srcData.Length);
        }

        public static byte[] BitsToBytes(bool[] srcData, int startPos, int bits)
        {
            return BitsToBytes(srcData, null, startPos, bits);
        }

        public static byte[] BitsToBytes(bool[] srcData, byte[] dstData)
        {
            return BitsToBytes(srcData, dstData, 0, srcData.Length);
        }

        public static byte[] BitsToBytes(bool[] srcData, byte[] dstData, int startPos, int bits)
        {
            int byteCount = (bits + 7) / 8;
            int leadingBits = 0;// (byteCount * 8) - srcData.Length;

            if (dstData == null)
                dstData = new byte[byteCount];

            for (int bytePos = 0; bytePos < bits / 8; bytePos++)
            {
                byte outByte = 0;

                for (int bitPos = 0; bitPos < 8; bitPos++)
                {
                    /* when handling the first bit, skip some bits if not aligned properly */
                    if (bitPos == 0 && bytePos == 0)
                        bitPos = leadingBits;

                    byte bitValue = (byte)(1 << (7-bitPos));
                    bool bitSet = srcData[startPos + bytePos * 8 + bitPos];

                    if (bitSet)
                        outByte |= bitValue;
                }

                dstData[bytePos] = outByte;
            }

            return dstData;
        }


        public static long BitsToLong(bool[] srcData, int start, int length)
        {
            long retVal = 0;

            for (int bitPos = 0; bitPos < length; bitPos++)
            {
                retVal <<= 1;
                if (srcData[start + bitPos])
                    retVal |= 1;
            }

            return retVal;
        }

        public static long BitsToLongRev(bool[] srcData, int start, int length)
        {
            long retVal = 0;

            for (int bitPos = 0; bitPos < length; bitPos++)
            {
                retVal <<= 1;
                if (srcData[start + (length-bitPos-1)])
                    retVal |= 1;
            }

            return retVal;
        }

        public static long BitsToLong(bool[] bools, int[][] start)
        {
            long retVal = 0;

            foreach (int[] pos in start)
            {
                retVal <<= pos[1];
                retVal |= BitsToLong(bools, pos[0], pos[1]);
            }

            return retVal;
        }

        public static long BitsToLongRev(bool[] bools, int[][] start)
        {
            long retVal = 0;

            foreach (int[] pos in start)
            {
                retVal <<= pos[1];
                retVal |= BitsToLongRev(bools, pos[0], pos[1]);
            }

            return retVal;
        }


        public static bool[] BytesToBits(byte[] srcData)
        {
            return BytesToBits(srcData, null);
        }

        public static bool[] BytesToBits(byte[] srcData, bool[] dstData)
        {
            if (dstData == null)
                dstData = new bool[srcData.Length * 8];

            for (int bytePos = 0; bytePos < srcData.Length; bytePos++)
            {
                byte inByte = srcData[bytePos];

                for (int bitPos = 0; bitPos < 8; bitPos++)
                {
                    byte bitValue = (byte)(1 << (7 - bitPos));
                    bool bitSet = ((inByte & bitValue) == bitValue);

                    dstData[bytePos * 8 + bitPos] = bitSet;
                }
            }

            return dstData;
        }


        /*
        * Converts two Little Endian bytes to a double and back.
        * The two LE bytes are a fixed point integer.
        * 
        * Range:
        *  double   -1.0    0      1.0
        *  bytes  0x8001 0x0000  0x7FFF
        * 
        */

        public static byte[] convertToBytes(double[] sampleData)
        {
            return convertToBytes(sampleData, null, 0, sampleData.Length);
        }

        public static byte[] convertToBytes(double[] sampleData, int startPos, int samples)
        {
            return convertToBytes(sampleData, null, startPos, samples);
        }

        public static byte[] convertToBytes(double[] sampleData, byte[] outBuffer)
        {
            return convertToBytes(sampleData, outBuffer, 0, sampleData.Length);
        }

        public static byte[] convertToBytes(double[] sampleData, byte[] outBuffer, int startPos, int samples)
        {
            if (outBuffer == null)
                outBuffer = new byte[2 * samples];

            for (int outPos = 0; outPos < samples; outPos++)
                putBytesFromDouble(outBuffer, outPos * 2, sampleData[startPos + outPos]);

            return outBuffer;
        }


        public static byte[] convertToBytesInterleaved(double[] sample1Data, double[] sample2Data)
        {
            return convertToBytesInterleaved(sample1Data, sample2Data, null);
        }

        public static byte[] convertToBytesInterleaved(double[] sample1Data, double[] sample2Data, byte[] outBuffer)
        {
            if (sample1Data.Length != sample2Data.Length)
                return null;

            int samples = sample1Data.Length;

            if (outBuffer == null)
                outBuffer = new byte[4 * samples];

            for (int outPos = 0; outPos < samples; outPos++)
            {
                putBytesFromDouble(outBuffer, outPos * 4, sample1Data[outPos]);
                putBytesFromDouble(outBuffer, outPos * 4 + 2, sample2Data[outPos]);
            }

            return outBuffer;
        }


        public static double getDoubleFromBytes(byte[] readBuffer, int pos)
        {
            return (double)getIntFromBytes(readBuffer, pos) / 0x7FFF;
        }

        public static void putBytesFromDouble(byte[] readBuffer, int pos, double sampleValue)
        {
            putBytesFromInt(readBuffer, pos, (int)(sampleValue * 0x7FFF));
        }

        public static int getIntFromBytes(byte[] readBuffer, int pos)
        {
            if (readBuffer == null)
                return 0;

            int value = (readBuffer[pos + 1] << 8) | readBuffer[pos];

            if (value > 0x7FFF)
                value = value - 0x10000;

            value = Math.Max(value, -0x7FFF);
            value = Math.Min(value, 0x7FFF);

            return value;
        }

        public static void putBytesFromInt(byte[] writeBuffer, int pos, int value)
        {
            if (writeBuffer == null || writeBuffer.Length < 2)
                return;

            value = Math.Max(value, -0x7FFF);
            value = Math.Min(value, 0x7FFF);

            if (value < 0)
                value = 0x10000 + value;

            writeBuffer[pos + 0] = (byte)(value & 0xFF);
            writeBuffer[pos + 1] = (byte)(value >> 8);
        }


    }
}