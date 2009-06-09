using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.GSM
{
    public class FCCHFinder
    {
        private int ConsecutiveHighBits = 0;
        private double MaxSampleValue = 0;

        /* relative to the highest sample value here are the */
        private double SampleHighMark = 0.60; /* to detect logical 0 (hight signal) of the FCH */
        private double OversampleBitCount = 0;

        public long BurstStartPosition { get; set; }
        public long CurrentPosition { get; set; }


        private double MinHighSampleValue
        {
            get { return MaxSampleValue * SampleHighMark; }
        }

        public FCCHFinder(double oversampling)
        {
            OversampleBitCount = (int)(FCHBurst.NetBitCount * oversampling);
        }

        public void Reset ()
        {
            CurrentPosition = 0;
            ConsecutiveHighBits = 0;
            BurstStartPosition = 0;
        }

        public bool ProcessData(double sampleValue, double strength)
        {
            /* if strength gets low, phase is varying due to noise. compensate this. */
            sampleValue *= strength;

            /* continuously decrease highest bit value */
            MaxSampleValue *= 0.99f;

            /* if the sample value is higher than the highest known, update highest value */
            MaxSampleValue = Math.Max(sampleValue, MaxSampleValue);

            if (sampleValue > MinHighSampleValue)
                ConsecutiveHighBits++;
            else
            {
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
