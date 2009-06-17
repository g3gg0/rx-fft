using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public enum eOversamplingType
    {
        None,
        Linear,
        SinX
    }

    public class Oversampler
    {
        private double HighVal = 1.0;
        private double LowVal = -1.0;
        public eOversamplingType Type = eOversamplingType.SinX;
        private double[] DeltaTable;


        public Oversampler()
        {
        }

        public Oversampler(double highVal, double lowVal)
        {
            HighVal = highVal;
            LowVal = lowVal;
        }

        public void PrepareDeltaTable(int oversampling)
        {
            DeltaTable = new double[oversampling];

            for(int pos =0; pos < DeltaTable.Length; pos++)
            {
                DeltaTable[pos] = Math.Sin(pos * Math.PI) / (pos * Math.PI);
            }
        }

        public double[] Oversample(byte[] sourceData, double oversampling)
        {
            double[] outData = new double[(int)(sourceData.Length * 8 * oversampling)];

            for (int outPos = 0; outPos < outData.Length; outPos++)
            {
                int bitPos = (int)(outPos / oversampling);
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

        public double[] Oversample(double[] srcData, double oversampling)
        {
            double[] outData = new double[(int)(srcData.Length * oversampling)];

            switch (Type)
            {
                case eOversamplingType.None:
                    for (int outPos = 0; outPos < outData.Length; outPos++)
                        outData[outPos] = srcData[(int)(outPos / oversampling)];
                    break;

                case eOversamplingType.Linear:
                    for (int outPos = 0; outPos < outData.Length; outPos++)
                    {
                        double samplePos = outPos / oversampling;

                        double delta = samplePos - Math.Floor(samplePos);
                        int samplePos1 = (int)samplePos;
                        int samplePos2 = (int)samplePos + 1;

                        double sampleValue1;
                        double sampleValue2;

                        if (samplePos1 >= 0 && samplePos1 < srcData.Length)
                            sampleValue1 = srcData[samplePos1];
                        else
                            sampleValue1 = 0;

                        if (samplePos2 >= 0 && samplePos2 < srcData.Length)
                            sampleValue2 = srcData[samplePos2];
                        else
                            sampleValue2 = sampleValue1;


                        outData[outPos] = sampleValue1 * (1 - delta) + sampleValue2 * delta;
                    }
                    break;

                case eOversamplingType.SinX:
                    for (int outPos = 0; outPos < outData.Length; outPos++)
                    {
                        int windowStart = (int)(outPos / oversampling - 8);
                        int windowEnd = (int)(outPos / oversampling + 8);

                        windowStart = Math.Max(0, windowStart);
                        windowEnd = Math.Min(outData.Length, windowEnd);

                        for (int windowPos = windowStart; windowPos < windowEnd; windowPos++)
                        {
                            double delta = windowPos - outPos / oversampling;
                            double sampleValue;

                            if (windowPos >= 0 && windowPos < srcData.Length)
                                sampleValue = srcData[windowPos];
                            else
                                sampleValue = 0;

                            delta *= Math.PI;

                            if (delta != 0)
                                outData[outPos] += sampleValue * Math.Sin(delta) / delta;
                            else
                                outData[outPos] += sampleValue;
                        }
                    }
                    break;

            }


            return outData;
        }

        public double[] Oversample(bool[] sourceData, double oversampling)
        {
            double[] outData = new double[(int)(sourceData.Length * oversampling)];

            for (int outPos = 0; outPos < outData.Length; outPos++)
                outData[outPos] = sourceData[(int)(outPos / oversampling)] ? 1.0f : -1.0f;

            return outData;
        }
    }
}
