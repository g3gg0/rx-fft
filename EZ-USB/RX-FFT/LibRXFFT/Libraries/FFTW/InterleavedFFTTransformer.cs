using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.FFTW
{
    public class InterleavedFFTTransformer : FFTTransformer
    {
        protected int SampleNum = 0;
        protected FFTTransformer[] Transformers;

        public InterleavedFFTTransformer(int fftSize, int interleaves)
        {
            Transformers = new FFTTransformer[interleaves];
            FFTSize = fftSize;

            for (int pos = 0; pos < interleaves; pos++)
            {
                Transformers[pos] = new FFTTransformer(FFTSize);
            }
        }

        public eWindowingFunction WindowingFunction
        {
            get { return Transformers[0].WindowingFunction; }
            set
            {
                for (int pos = 0; pos < Transformers.Length; pos++)
                {
                    Transformers[pos].WindowingFunction = value;
                }
            }
        }

        public override void AddSample(double I, double Q)
        {
            int transformers = Math.Min(SampleNum / Transformers.Length + 1, Transformers.Length);

            if(SampleNum < FFTSize)
            {
                SampleNum++;
            }

            for (int pos = 0; pos < transformers; pos++)
            {
                Transformers[pos].AddSample(I, Q);

                if (Transformers[pos].ResultAvailable)
                {
                    ResultAvailable = true;
                }
            }
        }

        public override double[] GetResult(double[] amplitudes)
        {
            for (int pos = 0; pos < Transformers.Length; pos++)
            {
                if (Transformers[pos].ResultAvailable)
                {
                    ResultAvailable = false;
                    return Transformers[pos].GetResult(amplitudes);
                }
            }

            return null;
        }

        public override double[] GetResultSquared(double[] amplitudes)
        {
            for (int pos = 0; pos < Transformers.Length; pos++)
            {
                if (Transformers[pos].ResultAvailable)
                {
                    ResultAvailable = false;
                    return Transformers[pos].GetResultSquared(amplitudes);
                }
            }

            return null;
        }
    }
}
