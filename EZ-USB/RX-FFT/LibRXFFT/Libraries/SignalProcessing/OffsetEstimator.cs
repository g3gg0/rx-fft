using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class OffsetEstimator
    {
        public static double EstimateOffset ( double[] srcData, int startPos, int samples, double oversampling )
        {
            double lastSampleValue = 0;
            double sampleValue = 0;
            ArrayList averages = new ArrayList();

            for (int pos = startPos; pos < samples; pos++)
            {
                if(startPos+pos > 0 && startPos + pos < srcData.Length)
                    sampleValue = srcData[startPos + pos];
                else
                    sampleValue = 0;

                if (( lastSampleValue<0 && sampleValue>0) || (lastSampleValue > 0 && sampleValue < 0))
                {
                    /* low->high or high->low */
                    double delta = lastSampleValue/(lastSampleValue - sampleValue);

                    double transition = startPos + (pos-1) + delta;
                    transition += oversampling / 2;
                    transition %= oversampling;
                    transition -= oversampling / 2;

                    averages.Add(transition);

                }

                lastSampleValue = sampleValue;
            }

            double average = 0;

            foreach (double value in averages)
                average += value;

            average /= averages.Count;

            if (Math.Abs(average) > oversampling/4)
                return 0;

            return average;
        }
    }
}
