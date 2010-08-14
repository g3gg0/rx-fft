using System;
using System.Collections;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class OffsetEstimator
    {
        /* estimate offset by checking the zero-line crossings */
               
        public static double EstimateOffset(double[] srcData, int startPos, int samples, double oversampling)
        {
            return EstimateOffset(srcData, startPos, samples, oversampling, 0);
        }

        public static double EstimateOffset(double[] srcData, int startPos, int samples, double oversampling, double transitionOffset)
        {
            double lastSampleValue = 0;
            double sampleValue = 0;
            ArrayList averages = new ArrayList();

            for (int pos = startPos; pos < samples; pos++)
            {
                if (startPos + pos > 0 && startPos + pos < srcData.Length)
                    sampleValue = srcData[startPos + pos];
                else
                    sampleValue = 0;

                if ((lastSampleValue < 0 && sampleValue > 0) || (lastSampleValue > 0 && sampleValue < 0))
                {
                    /* low->high or high->low */

                    /* interpolate zero position */
                    double delta = lastSampleValue / (lastSampleValue - sampleValue);

                    /* get the offset where we have this transition */
                    double transition = startPos + (pos - 1) + delta;

                    transition += transitionOffset;
                    transition %= oversampling;
                    transition -= transitionOffset;

                    averages.Add(transition);
                }

                lastSampleValue = sampleValue;
            }

            double average = 0;

            foreach (double value in averages)
                average += value;

            /* division by zero is checked later */
            unchecked
            {
                average /= averages.Count;
            }

            /* offset too high or we didnt have any zero-line crossings */
            if (/*Math.Abs(average) > oversampling / 4 ||*/ averages.Count == 0)
                return 0;

            return average;
        }
    }
}
