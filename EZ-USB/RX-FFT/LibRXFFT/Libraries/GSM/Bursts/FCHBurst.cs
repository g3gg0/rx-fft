using LibRXFFT.Libraries.GSM.Layer1;
using System;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    public class FCHBurst : Burst
    {
        public FCHBurst(GSMParameters Parameters)
        {
            Name = "FCH";
            ShortName = "FC ";
        }

        public override eSuccessState ParseRawBurst(GSMParameters Parameters, double[] rawBurst)
        {
            double startOffset = Parameters.SampleStartPosition;

            /* dont use all bits. skip 4 bits at the start and 4 at the end */
            int bits = (int)PayloadBits - 8;
            int startPos = (int)(Parameters.Oversampling * 4);
            int samples = (int)(Parameters.Oversampling * bits);

            double avg = 0;
            for (int pos = startPos; pos < samples + startPos; pos++)
                avg += rawBurst[(int)(startOffset + pos)];
            avg /= bits;

            /* should have +PI/2 per high bit. calculate phase correction value per sample */
            double phaseOffset = (Math.PI / 2 - avg) / Parameters.Oversampling;

            /* set offset */
            if (Parameters.PhaseAutoOffset)
                Parameters.PhaseOffsetValue += phaseOffset;

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