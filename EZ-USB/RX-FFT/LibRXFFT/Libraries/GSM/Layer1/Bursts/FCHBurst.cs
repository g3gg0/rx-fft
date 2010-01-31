
using System;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class FCHBurst : Burst
    {
        public FCHBurst(GSMParameters Parameters)
        {
            Name = "FCH";
            ShortName = "FC ";
        }

        public override eSuccessState ParseRawBurst(GSMParameters Parameters, double[] rawBurst, double[] rawBurstStrength)
        {
            double startOffset = Parameters.SampleStartPosition;

            /* dont use all bits. skip 4 bits at the start and 4 at the end */
            int bits = (int)PayloadBits - 8;
            int startPos = (int)(Parameters.Oversampling * 4);
            int samples = (int)(Parameters.Oversampling * bits);

            double avgPhase = 0;
            double avgPower = 0;
            for (int pos = startPos; pos < samples + startPos; pos++)
            {
                avgPhase += rawBurst[(int)(startOffset + pos)];
                avgPower += Math.Sqrt(rawBurstStrength[(int)(startOffset + pos)]);
            }

            double avgIdlePower = 0;
            for (int pos = 1; pos < startOffset / 4 + 1; pos++)
            {
                avgIdlePower += Math.Sqrt(rawBurstStrength[pos]);
            }

            avgPhase /= bits;
            avgPower /= samples;
            avgIdlePower /= (startOffset / 4);

            /* should have +PI/2 per high bit. calculate phase correction value per sample */
            double phaseOffset = (Math.PI / 2 - avgPhase) / Parameters.Oversampling;

            /* set offset */
            if (Parameters.PhaseAutoOffset)
                Parameters.PhaseOffsetValue += phaseOffset;

            int ratio = 10;
            Parameters.AveragePower = ((ratio - 1) * Parameters.AveragePower + avgPower) / ratio;
            Parameters.AverageIdlePower = ((ratio - 1) * Parameters.AverageIdlePower + avgIdlePower) / ratio;

            return eSuccessState.Unknown;
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return eSuccessState.Unknown;
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            return eSuccessState.Unknown;
        }
    }
}