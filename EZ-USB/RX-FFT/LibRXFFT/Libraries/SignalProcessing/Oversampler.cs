using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class Oversampler
    {
        private double HighVal = 1.0;
        private double LowVal = -1.0;

        public Oversampler()
        {
        }

        public Oversampler(double highVal, double lowVal)
        {
            HighVal = highVal;
            LowVal = lowVal;
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

        public double[] Oversample(double[] sourceData, double oversampling)
        {
            double[] outData = new double[(int)(sourceData.Length * oversampling)];

            for (int outPos = 0; outPos < outData.Length; outPos++)
                outData[outPos] = sourceData[(int)(outPos / oversampling)];

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
