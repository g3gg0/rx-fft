using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LibRXFFT.Libraries
{
    public class ByteUtil
    {
        public static bool UseNative = true;

        [DllImport("libRXFFT_native.dll", EntryPoint = "SamplesFromBinary", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void SamplesFromBinaryNative(byte[] dataBuffer, int bytesRead, int destSize, double[] samplesI, double[] samplesQ, int dataFormat, bool invertedSpectrum);
        [DllImport("libRXFFT_native.dll", EntryPoint = "SamplesToBinary", CallingConvention = CallingConvention.Cdecl)]
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

        /// <summary>
        /// Convert a given byte array to a bool array. The first bit in array will be (srcData[0] & 0x80).
        /// </summary>
        /// <param name="srcData">source byte data.</param> 
        /// <param name="dstData">destination bool data.</param> 
        /// <param name="bitsPerByte">bits per byte.</param> 
        /// <param name="startBitPos">number of bits to skip in input data.</param> 
        /// <param name="bitCount">number of bits to get.</param> 
        /// <returns>either dstData or (if it was null) a newly allocated bool[]</param> 
        /// 
        public static bool[] BitsFromBytesRev(byte[] srcData, bool[] dstData, int bitsPerByte, int startBitPos, int bitCount)
        {
            if (dstData == null)
                dstData = new bool[bitCount];

            int inBitPos = startBitPos;
            for (int outBitPos = 0; outBitPos < bitCount; outBitPos++)
            {
                int inBytePos = inBitPos / bitsPerByte;
                int bitNum = inBitPos % bitsPerByte;
                byte bitValue = (byte)(1 << bitNum);
                bool bitSet = (srcData[inBytePos] & bitValue) != 0;

                dstData[outBitPos] = bitSet;
                inBitPos++;
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

            if (dstData == null)
                dstData = new byte[byteCount];

            byte outByte = 0;
            int bitCount = 0;
            int outPos = 0;
            for (int bitNum = 0; bitNum < bits; bitNum++)
            {
                bool bitSet = srcData[bitNum];

                outByte <<= 1;
                if (bitSet)
                {
                    outByte |= 1;
                }

                bitCount++;
                /* have enough bits to write a byte */
                if (bitCount >= bitsPerByte)
                {
                    /* write the byte */
                    dstData[outPos] = outByte;

                    /* reset counters */
                    bitCount = 0;
                    outByte = 0;
                    outPos++;
                }
            }

            /* write remaining bits */
            if (bitCount!= 0)
            {
                /* shift to align it to MSB */
                outByte <<= (bitsPerByte - bitCount);
                dstData[outPos] = outByte;
            }
            /*
            for (int bytePos = 0; bytePos < (bits / bitsPerByte); bytePos++)
            {
                for (int bitPos = 0; bitPos < bitsPerByte; bitPos++)
                {
                    bool bitSet = srcData[startPos + bytePos * bitsPerByte + bitPos];

                    outByte <<= 1;
                    if (bitSet)
                        outByte |= 1;
                }

                dstData[bytePos] = outByte;
            }
            */
            return dstData;
        }

        public static bool[] BitsFromBytes(byte[] srcData)
        {
            return BitsFromBytes(srcData, null, 8, 0, srcData.Length * 8);
        }

        /// <summary>
        /// Convert a given byte array to a bool array MSB. The first bit in array will be (srcData[0] & 0x80).
        /// </summary>
        /// <param name="srcData">source byte data.</param> 
        /// <param name="dstData">destination bool data.</param> 
        /// <param name="bitsPerByte">bits per byte.</param> 
        /// <param name="startBitPos">number of bits to skip in input data.</param> 
        /// <param name="bitCount">number of bits to get.</param> 
        /// <returns>either dstData or (if it was null) a newly allocated bool[]</param> 
        /// 
        public static bool[] BitsFromBytes(byte[] srcData, bool[] dstData, int bitsPerByte, int startBitPos, int bitCount)
        {
            if (dstData == null)
                dstData = new bool[bitCount];

            int inBitPos = startBitPos;
            for (int bitNum = 0; bitNum < bitCount; bitNum++)
            {
                int inBytePos = inBitPos / bitsPerByte;
                int bitPos = inBitPos % bitsPerByte;
                byte bitValue = (byte)(1 << ((bitsPerByte - 1) - bitPos));
                bool bitSet = (srcData[inBytePos] & bitValue) != 0;

                dstData[bitNum] = bitSet;
                inBitPos++;
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

        public static bool[] BytesToBitsRev(byte[] srcData, ref bool[] dstData)
        {
            if (dstData == null)
                dstData = new bool[srcData.Length * 8];

            for (int bytePos = 0; bytePos < srcData.Length; bytePos++)
            {
                byte inByte = srcData[bytePos];

                for (int bitPos = 0; bitPos < 8; bitPos++)
                {
                    byte bitValue = (byte)(1 << bitPos);
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
            return getDoubleFromBytes(readBuffer, pos, 2, true);
        }

        public static double getDoubleFromBytes(byte[] readBuffer, int pos, int numBytes, bool isLittle)
        {
            long maxVal = 1 << (numBytes * 8) - 1;
            return (double)getIntFromBytes(readBuffer, pos, numBytes, isLittle) / maxVal;
        }

        public static void putBytesFromDouble(byte[] readBuffer, int pos, double sampleValue)
        {
            putBytesFromInt(readBuffer, pos, (int)(sampleValue * 0x7FFF));
        }

        public static long getIntFromBytes(byte[] readBuffer, int pos, int numBytes, bool isLittle)
        {
            if (readBuffer == null)
                return 0;

            long value = 0;

            for (int byteNum = 0; byteNum < numBytes; byteNum++)
            {
                value <<= 8;
                if (!isLittle)
                {
                    value |= readBuffer[pos + byteNum];
                }
                else
                {
                    value |= readBuffer[pos + (numBytes - byteNum - 1)];
                }
            }

            long negBitVal = 1 << (numBytes * 8 - 1);
            long maxVal = 1 << (numBytes * 8);

            if ((value & negBitVal) != 0)
            {
                value = value - maxVal;
            }

            value = Math.Max(value, -(maxVal / 2 - 1));
            value = Math.Min(value, (maxVal / 2 - 1));

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
            Direct16BitIQFixedPointLE = 0,
            Direct16BitIQFixedPointBE = 1,
            Direct24BitIQFixedPointLE = 2,
            Direct24BitIQFixedPointBE = 3,
            Direct32BitIQFloat = 4,
            Direct32BitIQFloat64k = 5,
            Unknown = 6
        }
        
        public static void SamplesFromBinary(byte[] dataBuffer, double[] samplesI, double[] samplesQ, eSampleFormat dataFormat, bool InvertedSpectrum)
        {
            SamplesFromBinary(dataBuffer, dataBuffer.Length, samplesI, samplesQ, dataFormat, InvertedSpectrum);
        }

        public static void SamplesFromBinary(byte[] dataBuffer, int bytesRead, double[] samplesI, double[] samplesQ, eSampleFormat dataFormat, bool invertedSpectrum)
        {
            if (UseNative)
            {
                try
                {
                    SamplesFromBinaryNative(dataBuffer, bytesRead, samplesI.Length, samplesI, samplesQ, (int)dataFormat, invertedSpectrum);
                }
                catch (Exception)
                {
                    UseNative = false;
                    SamplesFromBinary(dataBuffer, bytesRead, samplesI, samplesQ, dataFormat, invertedSpectrum);
                    return;
                }
            }
            else
            {
                int bytesPerSample;
                int bytesPerSamplePair;

                bytesPerSamplePair = GetBytePerSamplePair(dataFormat);
                bytesPerSample = GetBytePerSample(dataFormat);


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
                        case eSampleFormat.Direct16BitIQFixedPointLE:
                            I = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos, 2, true);
                            Q = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos + bytesPerSample, 2, true);
                            break;

                        case eSampleFormat.Direct16BitIQFixedPointBE:
                            I = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos, 2, false);
                            Q = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos + bytesPerSample, 2, false);
                            break;

                        case eSampleFormat.Direct24BitIQFixedPointLE:
                            I = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos, 3, true);
                            Q = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos + bytesPerSample, 3, true);
                            break;

                        case eSampleFormat.Direct24BitIQFixedPointBE:
                            I = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos, 3, false);
                            Q = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos + bytesPerSample, 3, false);
                            break;

                        case eSampleFormat.Direct32BitIQFloat:
                            I = BitConverter.ToSingle(dataBuffer, bytesPerSamplePair * pos);
                            Q = BitConverter.ToSingle(dataBuffer, bytesPerSamplePair * pos + bytesPerSample);
                            break;

                        case eSampleFormat.Direct32BitIQFloat64k:
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
            SamplesToBinary(dataBuffer, samplesI.Length, samplesI, samplesQ, dataFormat, InvertedSpectrum);
        }

        public static int GetBytePerSamplePair(eSampleFormat format)
        {
            switch (format)
            {
                case eSampleFormat.Direct16BitIQFixedPointLE:
                case eSampleFormat.Direct16BitIQFixedPointBE:
                    return 4;

                case eSampleFormat.Direct24BitIQFixedPointLE:
                case eSampleFormat.Direct24BitIQFixedPointBE:
                    return 6;

                case eSampleFormat.Direct32BitIQFloat:
                case eSampleFormat.Direct32BitIQFloat64k:
                    return 8;

                default:
                    return 0;
            }
        }

        public static int GetBytePerSample(eSampleFormat format)
        {
            switch (format)
            {
                case eSampleFormat.Direct16BitIQFixedPointLE:
                case eSampleFormat.Direct16BitIQFixedPointBE:
                    return 2;

                case eSampleFormat.Direct24BitIQFixedPointLE:
                case eSampleFormat.Direct24BitIQFixedPointBE:
                    return 3;

                case eSampleFormat.Direct32BitIQFloat:
                case eSampleFormat.Direct32BitIQFloat64k:
                    return 4;

                default:
                    return 0;

            }
        }

        public static void SamplesToBinary(byte[] dataBuffer, int samplePairs, double[] samplesI, double[] samplesQ, eSampleFormat dataFormat, bool invertedSpectrum)
        {
            if (UseNative)
            {
                try
                {
                    SamplesToBinaryNative(dataBuffer, samplePairs, samplesI, samplesQ, (int)dataFormat, invertedSpectrum);
                }
                catch (Exception)
                {
                    UseNative = false;
                    SamplesToBinary(dataBuffer, samplePairs, samplesI, samplesQ, dataFormat, invertedSpectrum);
                    return;
                }
            }
            else
            {
                int bytesPerSample;
                int bytesPerSamplePair;

                bytesPerSamplePair = GetBytePerSamplePair(dataFormat);
                bytesPerSample = GetBytePerSample(dataFormat);

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
                        case eSampleFormat.Direct16BitIQFixedPointLE:
                            putBytesFromDouble(dataBuffer, bytesPerSamplePair * pos, I);
                            putBytesFromDouble(dataBuffer, bytesPerSamplePair * pos + bytesPerSample, Q);
                            break;

                        case eSampleFormat.Direct24BitIQFixedPointLE:
                            break;

                        case eSampleFormat.Direct32BitIQFloat:
                            byte[] bufI = BitConverter.GetBytes((float)I);
                            byte[] bufQ = BitConverter.GetBytes((float)Q);
                            Array.Copy(bufI, 0, dataBuffer, bytesPerSamplePair * pos, bytesPerSample);
                            Array.Copy(bufQ, 0, dataBuffer, bytesPerSamplePair * pos + bytesPerSample, bytesPerSample);
                            break;

                        case eSampleFormat.Direct32BitIQFloat64k:
                            byte[] bufI64k = BitConverter.GetBytes((float)(I * 65536));
                            byte[] bufQ64k = BitConverter.GetBytes((float)(Q * 65536));
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

        public static string BitsToString(bool[] bits)
        {
            StringBuilder builder = new StringBuilder();

            foreach (bool value in bits)
            {
                if (value)
                {
                    builder.Append("1");
                }
                else
                {
                    builder.Append("0");
                }
            }

            return builder.ToString();
        }

        public static bool[] BitsFromString(string data)
        {
            bool[] bits = new bool[data.Length];

            for (int pos = 0; pos < data.Length; pos++)
            {
                if (data[pos] == '1')
                {
                    bits[pos] = true;
                }
                else
                {
                    bits[pos] = false;
                }
            }

            return bits;
        }
        public static string BytesToString(byte[] data)
        {
            StringBuilder builder = new StringBuilder();

            for (int pos = 0; pos < data.Length; pos++)
            {
                builder.AppendFormat("{0:X2}", data[pos]);
            }

            return builder.ToString();
        }

        public static bool BytesFromString(string inData, ref byte[] outData)
        {
            int charsPerByte = 2;

            /* white space detection */
            if (inData.Length >= 3 && inData[2] == ' ')
                charsPerByte = 3;

            for (int pos = 0; pos < (inData.Length+1)/charsPerByte; pos++)
            {
                string byteStr = inData.Substring(pos * charsPerByte, 2);

                if (!byte.TryParse(byteStr, System.Globalization.NumberStyles.HexNumber, null, out outData[pos]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool[] InvertBits(bool[] data)
        {
            if (data == null)
                return null;

            for (int i = 0; i < data.Length; i++)
                data[i] ^= true;

            return data;
        }
    }
}