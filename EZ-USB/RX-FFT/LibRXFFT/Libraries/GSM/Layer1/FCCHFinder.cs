using System;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.GSM.Layer1
{
    public class FCCHFinder
    {
        private int ConsecutiveHighBits = 0;
        private double MaxSampleValue = 0;
        private double AccumulatedPower = 0;

        /* 
         * relative to the highest sample value.
         * to detect logical 0 (high signal) of the FCH.
         * normally, checking for >0 should be enough, but there might be a offset.
         * //for now lets check if the samples are all > 20% of the highest value.
         */
        public static double SampleHighMark = 0.00;
        private double OversampleBitCount = 0;
        private double Oversampling = 1;

        public long BurstStartPosition { get; set; }
        public long CurrentPosition { get; set; }


        private double MinHighSampleValue
        {
            get { return MaxSampleValue * SampleHighMark; }
        }

        public double AveragePower
        {
            get { return AccumulatedPower / OversampleBitCount; }
        }

        public FCCHFinder(double oversampling)
        {
            Oversampling = oversampling;
            OversampleBitCount = (int)((FCHBurst.NetBitCount - 2) * Oversampling);
        }

        public void Reset()
        {
            CurrentPosition = 0;
            ConsecutiveHighBits = 0;
            BurstStartPosition = 0;
            AccumulatedPower = 0;
        }

        public bool ProcessData(double sampleValueIn, double strength)
        {
            /* if !->strength<-! gets low, phase is varying due to noise. 
             * compensate this by multiplying sample value with the strength. */
            double sampleValue = sampleValueIn * strength;

            /* continuously decrease highest bit value */
            MaxSampleValue *= 0.99f;

            /* if the sample value is higher than the highest known, update highest value */
            MaxSampleValue = Math.Max(sampleValue, MaxSampleValue);
            if (double.IsNaN(MaxSampleValue))
                MaxSampleValue = sampleValue;

            if (sampleValue > MinHighSampleValue)
            {
                AccumulatedPower += strength;
                ConsecutiveHighBits++;
            }
            else
            {
                AccumulatedPower = 0;
                BurstStartPosition = CurrentPosition;
                ConsecutiveHighBits = 0;
            }

            CurrentPosition++;

            if (ConsecutiveHighBits >= OversampleBitCount)
            {
                ConsecutiveHighBits = 0;
                return true;
            }

            return false;
        }
    }
}