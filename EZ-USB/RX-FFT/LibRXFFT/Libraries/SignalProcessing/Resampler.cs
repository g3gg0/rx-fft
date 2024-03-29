﻿using System;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public enum eResamplingType
    {
        None,
        Linear,
        SinC
    }

    public class Resampler
    {
        public eResamplingType Type = eResamplingType.SinC;
        public int SinCDepth
        {
            get { return _SinCDepth; }
            set
            {
                lock (this)
                {
                    _SinCDepth = value;
                }
            }
        }
        public decimal Oversampling
        {
            get { return _Oversampling; }
            set
            {
                lock (this)
                {
                    _Oversampling = value;
                }
            }
        }

        private decimal _Oversampling = 1;
        private int _SinCDepth = 4;

        private double HighVal = 1.0;
        private double LowVal = -1.0;
        private double LastSampleValue = 0;
        private double[] SinCBuffer = new double[0];
        private double[] SinCTable = new double[0];


        public Resampler(decimal oversampling)
        {
            Oversampling = oversampling;
        }

        public Resampler(double highVal, double lowVal)
        {
            HighVal = highVal;
            LowVal = lowVal;
        }

        public double[] Resample(bool[] sourceData)
        {
            lock (this)
            {
                double[] outData = new double[(int)(sourceData.Length * Oversampling)];

                for (int outPos = 0; outPos < outData.Length; outPos++)
                    outData[outPos] = sourceData[(int)(outPos / Oversampling)] ? 1.0f : -1.0f;

                return outData;
            }
        }

        public double[] Resample(byte[] sourceData)
        {
            lock (this)
            {
                double[] outData = new double[(int)(sourceData.Length * 8 * Oversampling)];

                for (int outPos = 0; outPos < outData.Length; outPos++)
                {
                    int bitPos = (int)(outPos / Oversampling);
                    int bitNum = 7 - (bitPos % 8);
                    int byteNum = bitPos / 8;

                    double sampleVal;

                    if (((sourceData[byteNum] >> bitNum) & 1) == 1)
                        sampleVal = HighVal;
                    else
                        sampleVal = LowVal;

                    outData[outPos] = sampleVal;
                }

                return outData;
            }
        }

        public double[] Resample(double[] srcData)
        {
            double[] outData = null;
            
            Resample(srcData, ref outData);

            return outData;
        }

        /*
         * new SinC-design
         * 
         *  |------------------|  buffer content
         *      |----------|      data that is returned
         *             |-------|  data that is saved for next frame
         * 
         *   000|0000000000|000
         *      |AAAAAAAAAA      <- samples in
         *   000|000AAAAAAA|AAA  -> samples out
         *      |BBBBBBBBBB
         *   AAA|AAABBBBBBB|BBB
         * 
         */

        private decimal LastRemainder = 0;

        public void Resample(double[] srcData, ref double[] outData)
        {
            lock (this)
            {
                decimal thisBlockOutSize = LastRemainder + (decimal)srcData.Length * Oversampling;
                int outLength = (int)(Math.Ceiling(thisBlockOutSize));
                decimal remaining = thisBlockOutSize - outLength;

                if (outData == null)
                {
                    outData = new double[outLength];
                }
                else if (outData.Length != outLength)
                {
                    Array.Resize(ref outData, outLength);
                }

                for (int outPos = 0; outPos < outLength; outPos++)
                {
                    int inPos = (int)((outPos - LastRemainder) / Oversampling);
                    outData[outPos] = srcData[inPos];
                }

                LastRemainder = remaining;
                return;

                switch (Type)
                {
                    case eResamplingType.None:
                        for (int outPos = 0; outPos < outData.Length; outPos++)
                            outData[outPos] = srcData[(int)(outPos / Oversampling)];
                        break;

                    case eResamplingType.Linear:

                        double sampleValue1 = 0;
                        double sampleValue2 = 0;

                        for (int outPos = 0; outPos < outData.Length; outPos++)
                        {
                            decimal samplePos = outPos / Oversampling;
                            double delta = (double)(samplePos - Math.Floor(samplePos));

                            int samplePos2 = (int)Math.Max(0, Math.Min(srcData.Length - 1, samplePos));
                            int samplePos1 = samplePos2 - 1;

                            sampleValue2 = srcData[samplePos2];

                            if (samplePos1 < 0)
                                sampleValue1 = LastSampleValue;
                            else
                                sampleValue1 = srcData[samplePos1];

                            outData[outPos] = sampleValue1 * (1 - delta) + sampleValue2 * delta;
                        }

                        LastSampleValue = sampleValue2;

                        break;

                    case eResamplingType.SinC:

                        /* allocate buffer with two extra blocks before and after */
                        if (SinCBuffer.Length != 2 * SinCDepth + srcData.Length)
                        {
                            Array.Resize<double>(ref SinCBuffer, 2 * SinCDepth + srcData.Length);
                            Array.Clear(SinCBuffer, 0, SinCBuffer.Length);
                        }

                        int width = (int)(1 + Oversampling * (SinCDepth+1));
                        if (SinCTable.Length != width )
                        {
                            Array.Resize<double>(ref SinCTable, width);

                            /* center is always 1.0 */
                            SinCTable[0] = 1.0f;

                            for (int pos = 1; pos < width; pos++)
                            {
                                double delta = pos / (double)Oversampling;
                                double value = Math.Sin(delta * Math.PI) / (delta * Math.PI);

                                SinCTable[pos] = value;
                            }
                        }

                        /* move the last 2*depth samples we saved now into the start of our buffer */
                        Array.Copy(SinCBuffer, SinCBuffer.Length - 2 * SinCDepth, SinCBuffer, 0, 2 * SinCDepth);
                        /* now the new samples */
                        Array.Copy(srcData, 0, SinCBuffer, 2 * SinCDepth, srcData.Length);

                        /* now process every output sample */
                        for (int outPos = 0; outPos < outData.Length; outPos++)
                        {
                            int windowStart = (int)(outPos / Oversampling) - SinCDepth;
                            int windowEnd = (int)(outPos / Oversampling) + SinCDepth;

                            /* default is zero */
                            outData[outPos] = 0;

                            for (int windowPos = windowStart; windowPos <= windowEnd; windowPos++)
                            {
                                int srcPos = windowPos + SinCDepth;
                                int sincPos = (int)Math.Abs((outPos + SinCDepth * Oversampling) - (srcPos * Oversampling));

                                outData[outPos] += SinCBuffer[srcPos] * SinCTable[sincPos];
                            }
                        }

                        break;
                }
            }
        }

    }
}
