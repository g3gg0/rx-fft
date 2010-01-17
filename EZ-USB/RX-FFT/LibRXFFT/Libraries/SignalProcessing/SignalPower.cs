using System;

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
            double refFactor = Max(srcData, startPosition, refData.Length/4);

            for (int pos = startPosition; pos < endPosition; pos++)
            {
                double strength = ProcessDiff(srcData, pos, refData, refFactor);

                if (strength > maxStrength)
                {
                    maxStrength = strength;
                    maxPos = pos;
                }
            }

            return maxPos;
        }

        public static double ProcessMult(double[] srcData, int startPos, double[] refData, double refFactor)
        {
            double power = 0;

            for (int pos = 0; pos < refData.Length; pos++)
                if (startPos >= 0 && startPos + pos < srcData.Length)
                    power += srcData[startPos + pos] * (refData[pos] * refFactor);

            return power;
        }

        public static double ProcessDiff(double[] srcData, int startPos, double[] refData, double refFactor)
        {
            double power = 0;

            for (int pos = 0; pos < refData.Length; pos++)
            {
                double diff = 0;
                if (startPos >= 0 && startPos + pos < srcData.Length)
                    diff = srcData[startPos + pos] - (refData[pos] * refFactor);
                power -= diff * diff;
            }

            return power;
        }

        public static double Calculate(double[] srcData, int startPos, int samples)
        {
            double strength = 0;

            for (int pos = 0; pos < samples; pos++)
                if (startPos >= 0 && startPos + pos < srcData.Length)
                    strength += srcData[startPos + pos];

            return strength;
        }

        public static double Max(double[] srcData, int startPos, int samples)
        {
            double strength = double.MinValue;

            for (int pos = 0; pos < samples; pos++)
                if (startPos >= 0 && startPos + pos < srcData.Length)
                    strength = Math.Max(strength, srcData[startPos + pos]);

            return strength;
        }

        public static double Min(double[] srcData, int startPos, int samples)
        {
            double strength = double.MaxValue;

            for (int pos = 0; pos < samples; pos++)
                if (startPos >= 0 && startPos + pos < srcData.Length)
                    strength = Math.Min(strength, srcData[startPos + pos]);

            return strength;
        }

        public static double Average(double[] srcData, int startPos, int samples)
        {
            double strength = 0;

            for (int pos = 0; pos < samples; pos++)
                if (startPos >= 0 && startPos + pos < srcData.Length)
                    strength += srcData[startPos + pos];


            return strength / samples;
        }
    }
}
