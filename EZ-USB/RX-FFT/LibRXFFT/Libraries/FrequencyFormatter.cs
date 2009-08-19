using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.Misc
{
    public class FrequencyFormatter
    {
        public static string FreqToString(double frequency)
        {
            string[] scale = { "", "k", "M", "G", "T" };
            int fact = 0;

            while (Math.Abs(frequency) > 1000)
            {
                frequency /= 1000;
                fact++;
            }

            return String.Format("{0:0.00}", frequency) + " " + scale[fact] + "Hz";
        }
    }
}
