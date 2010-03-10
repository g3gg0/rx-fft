using System;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries
{
    public class ByteUtil
    {
        public static bool UseNative = true;

        [DllImport("libRXFFT_native.dll", EntryPoint = "SamplesFromBinary")]
        public static unsafe extern void SamplesFromBinaryNative(byte[] dataBuffer, int bytesRead, int destSize, double[] samplesI, double[] samplesQ, int dataFormat, bool invertedSpectrum);
        [DllImport("libRXFFT_native.dll", EntryPoint = "SamplesToBinary")]
        public static unsafe extern void SamplesToBinaryNative(byte[] dataBuffer, int samplePairs, double[] samplesI, double[] samplesQ, int dataFormat, bool invertedSpectrum);
        

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
            return BitsToBytes(srcData, null, 8, 0, srcData.Length);
        }

        public static byte[] BitsToBytes(bool[] srcData, int bitsPerByte)
        {
            return BitsToBytes(srcData, null, bitsPerByte, 0, srcData.Length);
        }

        public static byte[] BitsToBytes(bool[] srcData, int startPos, int bits)
        {
            return BitsToBytes(srcData, null, 8, startPos, bits);
        }

        public static byte[] BitsToBytes(bool[] srcData, byte[] dstData)
        {
            return BitsToBytes(srcData, dstData, 8, 0, srcData.Length);
        }

        public static byte[] BitsToBytes(bool[] srcData, byte[] dstData, int bitsPerByte, int startPos, int bits)
        {
            int byteCount = (bits + (bitsPerByte-1)) / bitsPerByte;
            int leadingBits = 0;// (byteCount * 8) - srcData.Length;

            if (dstData == null)
                dstData = new byte[byteCount];

            for (int bytePos = 0; bytePos < (bits / bitsPerByte); bytePos++)
            {
                byte outByte = 0;

                for (int bitPos = 0; bitPos < bitsPerByte; bitPos++)
                {
                    /* when handling the first bit, skip some bits if not aligned properly 
                    if (bitPos == 0 && bytePos == 0)
                        bitPos = leadingBits;

                    byte bitValue = (byte)(1 << (7-bitPos));
                    bool bitSet = srcData[startPos + bytePos * bitsPerByte + bitPos];

                    if (bitSet)
                        outByte |= bitValue;
                     * */

                    bool bitSet = srcData[startPos + bytePos * bitsPerByte + bitPos];

                    outByte <<= 1;
                    if (bitSet)
                        outByte |= 1;
                }

                dstData[bytePos] = outByte;
            }

            return dstData;
        }


        public static long BitsToLong(bool[] srcData)
        {
            return BitsToLong(srcData, 0, srcData.Length);
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

        public enum eSampleFormat
        {
            Direct16BitIQFixedPoint = 0,
            Direct64BitIQFloat = 1,
            Direct64BitIQFloat64k = 2
        }
        
        public static void SamplesFromBinary(byte[] dataBuffer, double[] samplesI, double[] samplesQ, eSampleFormat dataFormat, bool InvertedSpectrum)
        {
            SamplesFromBinary(dataBuffer, dataBuffer.Length, samplesI, samplesQ, dataFormat, InvertedSpectrum);
        }

        public static void SamplesFromBinary(byte[] dataBuffer, int bytesRead, double[] samplesI, double[] samplesQ, eSampleFormat dataFormat, bool invertedSpectrum)
        {
            if (UseNative)
            {
                SamplesFromBinaryNative(dataBuffer, bytesRead, samplesI.Length, samplesI, samplesQ, (int)dataFormat, invertedSpectrum);
            }
            else
            {
                int bytesPerSample;
                int bytesPerSamplePair;

                switch (dataFormat)
                {
                    case eSampleFormat.Direct16BitIQFixedPoint:
                        bytesPerSamplePair = 4;
                        bytesPerSample = 2;
                        break;

                    case eSampleFormat.Direct64BitIQFloat:
                    case eSampleFormat.Direct64BitIQFloat64k:
                        bytesPerSamplePair = 8;
                        bytesPerSample = 4;
                        break;

                    default:
                        bytesPerSamplePair = 0;
                        bytesPerSample = 0;
                        break;
                }

                int samplePos = 0;
                int samplePairs = bytesRead / bytesPerSamplePair;

                if (samplesI.Length < samplePairs || samplesQ.Length < samplePairs)
                    return;

                for (int pos = 0; pos < samplePairs; pos++)
                {
                    double I;
                    double Q;
                    switch (dataFormat)
                    {
                        case eSampleFormat.Direct16BitIQFixedPoint:
                            I = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos);
                            Q = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos + bytesPerSample);
                            break;

                        case eSampleFormat.Direct64BitIQFloat:
                            I = BitConverter.ToSingle(dataBuffer, bytesPerSamplePair * pos);
                            Q = BitConverter.ToSingle(dataBuffer, bytesPerSamplePair * pos + bytesPerSample);
                            break;

                        case eSampleFormat.Direct64BitIQFloat64k:
                            I = BitConverter.ToSingle(dataBuffer, bytesPerSamplePair * pos) / 65536;
                            Q = BitConverter.ToSingle(dataBuffer, bytesPerSamplePair * pos + bytesPerSample) / 65536;
                            break;

                        default:
                            return;
                    }

                    if (invertedSpectrum)
                        I = -I;

                    samplesI[pos] = I;
                    samplesQ[pos] = Q;
                }

                return;
            }
        }
        
        public static void SamplesToBinary(byte[] dataBuffer, double[] samplesI, double[] samplesQ, eSampleFormat dataFormat, bool InvertedSpectrum)
        {
            SamplesToBinary(dataBuffer, dataBuffer.Length, samplesI, samplesQ, dataFormat, InvertedSpectrum);
        }

        public static void SamplesToBinary(byte[] dataBuffer, int samplePairs, double[] samplesI, double[] samplesQ, eSampleFormat dataFormat, bool invertedSpectrum)
        {
            if (UseNative)
            {
                SamplesToBinaryNative(dataBuffer, samplePairs, samplesI, samplesQ, (int)dataFormat, invertedSpectrum);
            }
            else
            {
                int bytesPerSample;
                int bytesPerSamplePair;

                switch (dataFormat)
                {
                    case eSampleFormat.Direct16BitIQFixedPoint:
                        bytesPerSamplePair = 4;
                        bytesPerSample = 2;
                        break;

                    case eSampleFormat.Direct64BitIQFloat:
                    case eSampleFormat.Direct64BitIQFloat64k:
                        bytesPerSamplePair = 8;
                        bytesPerSample = 4;
                        break;

                    default:
                        bytesPerSamplePair = 0;
                        bytesPerSample = 0;
                        break;
                }

                int samplePos = 0;

                if (samplesI.Length < samplePairs || samplesQ.Length < samplePairs)
                    return;

                for (int pos = 0; pos < samplePairs; pos++)
                {
                    double I = samplesI[pos];
                    double Q = samplesQ[pos];

                    if (invertedSpectrum)
                        I = -I;

                    switch (dataFormat)
                    {
                        case eSampleFormat.Direct16BitIQFixedPoint:
                            putBytesFromDouble(dataBuffer, bytesPerSamplePair * pos, I);
                            putBytesFromDouble(dataBuffer, bytesPerSamplePair * pos + bytesPerSample, Q);
                            break;

                        case eSampleFormat.Direct64BitIQFloat:
                            byte[] bufI = BitConverter.GetBytes((float)I);
                            byte[] bufQ = BitConverter.GetBytes((float)Q);
                            Array.Copy(bufI, 0, dataBuffer, bytesPerSamplePair * pos, bytesPerSample);
                            Array.Copy(bufQ, 0, dataBuffer, bytesPerSamplePair * pos + bytesPerSample, bytesPerSample);
                            break;

                        case eSampleFormat.Direct64BitIQFloat64k:
                            byte[] bufI64k = BitConverter.GetBytes((float)(I / 65536));
                            byte[] bufQ64k = BitConverter.GetBytes((float)(Q / 65536));
                            Array.Copy(bufI64k, 0, dataBuffer, bytesPerSamplePair * pos, bytesPerSample);
                            Array.Copy(bufQ64k, 0, dataBuffer, bytesPerSamplePair * pos + bytesPerSample, bytesPerSample);
                            break;

                        default:
                            return;
                    }
                }

                return;
            }
        }
    }
}