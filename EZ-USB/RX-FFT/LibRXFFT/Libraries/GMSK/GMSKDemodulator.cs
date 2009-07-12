using System;
using LibRXFFT.Libraries.ShmemChain;

namespace LibRXFFT.Libraries.GMSK
{

    public class GMSKDemodulator
    {
        private double LastPhase;
        public bool UseFastAtan2 = false;
        public bool InvertedSpectrum = false;

        private SharedMem ShmemOutChan;
        public static bool UseFastAtan2Default = false;
        public static bool InvertedSpectrumDefault = false;


        public GMSKDemodulator()
        {
            UseFastAtan2 = UseFastAtan2Default;
            InvertedSpectrum = InvertedSpectrumDefault;

            //ShmemOutChan = new SharedMem(-2, -1, "GMSKDemod");

        }

        private double FastAtan2b(double y, double x)
        {
            const double ONEQTR_PI = Math.PI / 4.0f;
            const double THRQTR_PI = 3.0f * Math.PI / 4.0f;
            double r;
            double angle;
            double abs_y = Math.Abs(y);

            if (x < 0.0f)
            {
                r = (x + abs_y) / (abs_y - x);
                angle = THRQTR_PI;
            }
            else
            {
                r = (x - abs_y) / (x + abs_y);
                angle = ONEQTR_PI;
            }

            angle += (0.1963f * r * r - 0.9817f) * r;

            return y < 0.0f ? -angle : angle;
        }

        public double[] ProcessData(double[] samplesI, double[] samplesQ, double[] sampleArray, double[] strengthArray)
        {
            byte[] outBuffer = null;
            int outBufferPos = 0;
            int samplePos = 0;
            int samplePairs = samplesI.Length;

            /* if caller provided no array, create one */
            if (sampleArray == null)
                sampleArray = new double[samplePairs];

            /* shmem output buffer */
            if (ShmemOutChan != null)
                outBuffer = new byte[samplePairs * 4];

            for (int pos = 0; pos < samplePairs; pos++)
            {
                double I = samplesI[pos];
                double Q = samplesQ[pos];

                if (InvertedSpectrum)
                    I = -I;

                /* 
                 * this strength calculation is incorrect!
                 * the Math.Sqrt() is missing, but that would consume
                 * too much CPU power. 
                 * thats okay since the strength is just used qualitative.
                 * the *exact* value doesnt matter.
                 */
                double strength = 100 * (I * I + Q * Q);
                double phase;

                phase = UseFastAtan2 ? FastAtan2b(I, Q) : Math.Atan2(I, Q);

                while (phase - LastPhase < -(Math.PI / 2))
                    phase += Math.PI;

                while (phase - LastPhase > Math.PI / 2)
                    phase -= Math.PI;

                /* catch the case where I and Q are zero */
                if (double.IsNaN(phase))
                    phase = LastPhase;

                double diff = phase - LastPhase;

                /* only provide strength if array is passed to function */
                if (strengthArray != null)
                    strengthArray[samplePos] = strength;

                sampleArray[samplePos++] = diff;

                if (outBuffer != null)
                {
                    /*
                    ByteUtil.putBytesFromDouble(outBuffer, 2 * (outBufferPos++), I);
                    ByteUtil.putBytesFromDouble(outBuffer, 2 * (outBufferPos++), Q);
                    */
                    ByteUtil.putBytesFromDouble(outBuffer, 2 * (outBufferPos++), diff * strength);
                    ByteUtil.putBytesFromDouble(outBuffer, 2 * (outBufferPos++), strength);
                }

                LastPhase = phase % (2 * Math.PI);
            }

            if (ShmemOutChan != null)
                ShmemOutChan.Write(outBuffer, 0, outBuffer.Length);

            return sampleArray;
        }

    }
}
