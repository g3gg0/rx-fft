using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
