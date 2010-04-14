using System;

namespace LibRXFFT.Libraries.Misc
{
    public class FrequencyFormatter
    {
        public static string FreqToString(long frequency)
        {
            return FreqToString((decimal)frequency);
        }

        public static string FreqToString(double frequency)
        {
            return FreqToString((decimal)frequency);
        }

        public static string FreqToString(decimal frequency)
        {
            string[] scale = { "", "k", "M", "G", "T" };
            int fact = 0;

            while (Math.Abs(frequency) > 1000)
            {
                frequency /= 1000;
                fact++;
            }

            return String.Format("{0:0.##}", frequency) + " " + scale[fact] + "Hz";
        }

        public static string FreqToStringAccurate(long frequency)
        {
            return FreqToStringAccurate((decimal)frequency);
        }

        public static string FreqToStringAccurate(double frequency)
        {
            return FreqToStringAccurate((decimal)frequency);
        }

        public static string FreqToStringAccurate(decimal frequency)
        {
            string[] scale = { "", "k", "M", "G", "T" };
            int fact = 0;

            if (Math.Abs(frequency) > 0)
            {
                /* get the highest scale without decimals */
                while (Math.Abs(frequency) / 1000 == ((long)(Math.Abs(frequency) / 1000)))
                {
                    frequency /= 1000;
                    fact++;
                }

                /* if still above 1000, choose the next higher scale */
                if (Math.Abs(frequency) > 1000)
                {
                    frequency /= 1000;
                    fact++;
                }

                /* dont allow fractionals of 1 Hz */
                if (fact == 0)
                {
                    frequency = (long)frequency;
                }
            }

            return String.Format("{0:0.###}", frequency) + " " + scale[fact] + "Hz";
        }

        public static string FreqToStringSimple(decimal frequency)
        {
            return String.Format("{0:0,000,000,000} Hz", frequency);
        }

        public static string TimeToString(long time)
        {
            return TimeToString((decimal)time);
        }

        public static string TimeToString(double time)
        {
            return TimeToString((decimal)time);
        }

        public static string TimeToString(decimal time)
        {
            int fact = 5;
            string[] scale = { "f", "p", "n", "µ", "m", "", "k", "M", "G", "T" };

            while (Math.Abs(time) > 1000)
            {
                time /= 1000;
                fact++;
            }

            while (Math.Abs(time) < 1 && Math.Abs(time) > 0)
            {
                time *= 1000;
                fact--;
            }

            return String.Format("{0:0.##}", time) + " " + scale[fact] + "s";
        }
    }
}
