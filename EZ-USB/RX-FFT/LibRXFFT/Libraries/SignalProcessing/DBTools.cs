using System;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class DBTools
    {
        public static double SampleTodB(double sampleValue)
        {
            return 20 * Math.Log10(sampleValue);
        }

        public static double SampleFromdB(double dBValue)
        {
            return Math.Pow(10, (dBValue/20));
        }

        public static double SquaredSampleTodB(double sampleValue)
        {
            return 10 * Math.Log10(sampleValue);
        }

        public static double SquaredSampleFromdB(double dBValue)
        {
            return Math.Pow(10, (dBValue / 10));
        }

        public static double MaximumDb(double[] iSamples, double[] qSamples)
        {
            if (iSamples.Length != qSamples.Length)
                return 0;

            double maxVal = 0;

            for(int pos = 0; pos < iSamples.Length;pos++)
            {
                maxVal = Math.Max(maxVal, iSamples[pos]*iSamples[pos] + qSamples[pos]*qSamples[pos]);
            }

            return SquaredSampleTodB(maxVal);
        }
    }
}
