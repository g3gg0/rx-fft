using System;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.FFTW
{
    class FFTTransformer
    {
        internal float[] InSamples;
        internal float[] OutSamples;
        GCHandle hin;
        GCHandle hout;
        IntPtr fplan;
        private int FFTPos;
        private int FFTSize;
        public bool ResultAvailable { get; set; }


        public FFTTransformer(int FFTSize)
        {
            this.FFTSize = FFTSize;
            FFTPos = 0;
            InSamples = new float[FFTSize * 2];
            OutSamples = new float[FFTSize * 2];
            hin = GCHandle.Alloc(InSamples, GCHandleType.Pinned);
            hout = GCHandle.Alloc(OutSamples, GCHandleType.Pinned);

            fplan = fftwf.dft_1d(FFTSize, hin.AddrOfPinnedObject(), hout.AddrOfPinnedObject(), fftw_direction.Forward, fftw_flags.Estimate);
        }

        public double[] GetResult()
        {
            return GetResult(null);
        }

        public double[] GetResult(double[] amplitudes)
        {
            ResultAvailable = false;

            if (amplitudes == null)
                amplitudes = new double[FFTSize];

            for (int pos = 0; pos < FFTSize; pos++)
            {
                double I = OutSamples[2 * pos];
                double Q = OutSamples[2 * pos + 1];

                /* the output is not properly aligned, so start in the middle */
                amplitudes[(pos + FFTSize / 2) % FFTSize] = Math.Sqrt(I * I + Q * Q);
            }

            return amplitudes;
        }

        public void AddSample(double I, double Q)
        {
            InSamples[2 * FFTPos] = (float)I;
            InSamples[2 * FFTPos + 1] = (float)Q;

            FFTPos++;
            if (FFTPos == FFTSize)
            {
                FFTPos = 0;
                Execute();
                ResultAvailable = true;
            }
        }

        public void Execute()
        {
            fftwf.execute(fplan);
        }
    }
}