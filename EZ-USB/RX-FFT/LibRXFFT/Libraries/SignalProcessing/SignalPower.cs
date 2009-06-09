using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class SignalPower
    {
        public static bool Debug = false;

        public static int Locate(double[] srcData, int startPos, double[] refData, int variation)
        {
            int startPosition = Math.Max(startPos - variation, 0);
            int endPosition = Math.Min(startPos + variation, srcData.Length - refData.Length);
            int maxPos = int.MinValue;
            double maxStrength = double.MinValue;

            for (int pos = startPosition; pos < endPosition; pos++)
            {
                double strength = ProcessDiff(srcData, pos, refData);

                if (strength > maxStrength)
                {
                    maxStrength = strength;
                    maxPos = pos;
                }
            }

            return maxPos;
        }

        public static double ProcessMult(double[] srcData, int startPos, double[] refData)
        {
            double power = 0;

            for (int pos = 0; pos < refData.Length; pos++)
                power += srcData[startPos + pos] * refData[pos];

            return power;
        }

        public static double ProcessDiff(double[] srcData, int startPos, double[] refData)
        {
            double power = 0;

            for (int pos = 0; pos < refData.Length; pos++)
            {
                double diff = srcData[startPos + pos] - refData[pos];
                power -= diff * diff;
            }

            return power;
        }

        public static double Calculate(double[] srcData, int startPos, int samples)
        {
            double strength = 0;

            for (int pos = 0; pos < samples; pos++)
                strength += srcData[startPos + pos];

            return strength;
        }

        public static double Max(double[] srcData, int startPos, int samples)
        {
            double strength = double.MinValue;

            for (int pos = 0; pos < samples; pos++)
                strength = Math.Max(strength, srcData[startPos + pos]);

            return strength;
        }

        public static double Min(double[] srcData, int startPos, int samples)
        {
            double strength = double.MaxValue;

            for (int pos = 0; pos < samples; pos++)
                strength = Math.Min(strength, srcData[startPos + pos]);

            return strength;
        }

        public static double Average(double[] srcData, int startPos, int samples)
        {
            double strength = 0;

            for (int pos = 0; pos < samples; pos++)
                strength += srcData[startPos + pos];

            return strength / samples;
        }
    }
}
