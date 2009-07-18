using System;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.FFTW
{
    public class FFTTransformer
    {
        internal double[] InSamples;
        internal double[] OutSamples;
        internal GCHandle hin;
        internal GCHandle hout;
        internal IntPtr fplan;

        internal int FFTPos = 0;
        internal int FFTSize = 0;
        public bool ResultAvailable = false;
        public bool Available = false;

        internal double WindowingConstant = 1.0f;
        internal double WindowingCorrection = 1.0f;
        internal double WindowingCorrectionSquared = 1.0f;
        internal double[] WindowFuncTable;
        internal eWindowingFunction _WindowingFunction = eWindowingFunction.BlackmanHarris;

        public FFTTransformer(int FFTSize)
        {
            this.FFTSize = FFTSize;
            BuildWindowFuncTable(WindowingFunction);

            InSamples = new double[FFTSize * 2];
            OutSamples = new double[FFTSize * 2];
            hin = GCHandle.Alloc(InSamples, GCHandleType.Pinned);
            hout = GCHandle.Alloc(OutSamples, GCHandleType.Pinned);

            try
            {
                fplan = fftw.dft_1d(FFTSize, hin.AddrOfPinnedObject(), hout.AddrOfPinnedObject(), fftw_direction.Forward, fftw_flags.Estimate);
                Available = true;
            }
            catch (DllNotFoundException ex)
            {
            }
        }

        public eWindowingFunction WindowingFunction
        { 
            get { return _WindowingFunction; }
            set
            {
                _WindowingFunction = value;
                BuildWindowFuncTable(value);
            }
        }

        /* 
         * Windowing Functions
         * 
         * Signalverarbeitung analoge und digitale Signale, Systeme und Filter
         * Pg. 195
         * 
         * http://books.google.com/books?id=Q-_aeIGOgBYC&pg=PA188&lpg=PA188&dq=blackman+window+correction&source=bl&ots=OCZ04l_cg8&sig=2telnf7LZZIiHGNyk2MmtOonFjw&hl=de&ei=w7NfSvzGC5jEmwPN6bmrBw&sa=X&oi=book_result&ct=result&resnum=10
         * 
         */

        public enum eWindowingFunction
        {
            None,
            Hamming,
            RaisedCosine,
            Blackman,
            BlackmanHarris,
            BlackmanNuttal,
            FlatTop
        }

        internal void BuildWindowFuncTable(eWindowingFunction function)
        {
            WindowFuncTable = new double[FFTSize];

            switch (function)
            {
                case eWindowingFunction.Hamming:
                    WindowingConstant = 0.54f;
                    break;

                case eWindowingFunction.RaisedCosine:
                    WindowingConstant = 0.5f;
                    break;

                case eWindowingFunction.Blackman:
                    WindowingConstant = 0.42f;
                    break;

                case eWindowingFunction.BlackmanHarris:
                    WindowingConstant = 0.35875f;
                    break;

                case eWindowingFunction.BlackmanNuttal:
                    WindowingConstant = 0.3635819f;
                    break;

                case eWindowingFunction.FlatTop:
                    WindowingConstant = 1.0f;
                    break;

                case eWindowingFunction.None:
                default:
                    WindowingConstant = 1.0f;
                    break;
            }

            WindowingCorrection = 1 / WindowingConstant;
            WindowingCorrectionSquared = WindowingCorrection * WindowingCorrection;

            for(int n = 0; n < FFTSize; n++)
            {
                double M = FFTSize - 1;
                double value = 0;

                switch(function)
                {
                    case eWindowingFunction.Hamming:
                        value = WindowingConstant - 0.46f * Math.Cos(2 * n * Math.PI / M);
                        break;

                    case eWindowingFunction.RaisedCosine:
                        value = WindowingConstant - 0.5f * Math.Cos(2 * n * Math.PI / M);
                        break;

                    case eWindowingFunction.Blackman:
                        value = WindowingConstant - 0.5f * Math.Cos(2 * n * Math.PI / M) + 0.08f * Math.Cos(4 * n * Math.PI / M);
                        break;

                    case eWindowingFunction.BlackmanHarris:
                        value = WindowingConstant - 0.48829f * Math.Cos(2 * n * Math.PI / M) + 0.14128f * Math.Cos(4 * n * Math.PI / M) - 0.01168f * Math.Cos(6 * n * Math.PI / M);
                        break;

                    case eWindowingFunction.BlackmanNuttal:
                        value = WindowingConstant - 0.4891775f * Math.Cos(2 * n * Math.PI / M) + 0.1365995f * Math.Cos(4 * n * Math.PI / M) - 0.0106411f * Math.Cos(6 * n * Math.PI / M);
                        break;

                    case eWindowingFunction.FlatTop:
                        value = WindowingConstant - 1.93f * Math.Cos(2 * n * Math.PI / M) + 1.29f * Math.Cos(4 * n * Math.PI / M) - 0.388f * Math.Cos(6 * n * Math.PI / M) + 0.032f * Math.Cos(8 * n * Math.PI / M);
                        break;

                    case eWindowingFunction.None:
                    default:
                        value = 1.0f;
                        break;

                }

                WindowFuncTable[n] = value;
            }
        }



        public void Execute()
        {
            fftw.execute(fplan);
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
                double I = OutSamples[2 * pos] / FFTSize;
                double Q = OutSamples[2 * pos + 1] / FFTSize;

                /* the output is not properly aligned, so start in the middle */
                amplitudes[(pos + FFTSize / 2) % FFTSize] = WindowingCorrection * Math.Sqrt(I * I + Q * Q);
            }

            return amplitudes;
        }

        /* a more optimized version, that returns the amplitudes still squared. useful if the data later would get square-rooted. */
        public double[] GetResultSquared(double[] amplitudes)
        {
            ResultAvailable = false;

            if (amplitudes == null)
                amplitudes = new double[FFTSize];

            for (int pos = 0; pos < FFTSize; pos++)
            {
                double I = OutSamples[2 * pos] / FFTSize;
                double Q = OutSamples[2 * pos + 1] / FFTSize;

                /* the output is not properly aligned, so start in the middle */
                amplitudes[(pos + FFTSize / 2) % FFTSize] = WindowingCorrectionSquared * (I * I + Q * Q);
            }

            return amplitudes;
        }

        public void AddSample(double I, double Q)
        {
            InSamples[2 * FFTPos] = I * WindowFuncTable[FFTPos];
            InSamples[2 * FFTPos + 1] = Q * WindowFuncTable[FFTPos];

            FFTPos++;
            if (FFTPos == FFTSize)
            {
                FFTPos = 0;
                /*
                for (int pos = 0; pos < InSamples.Length / 4; pos++)
                {
                    InSamples[4*pos] = 1.0f;
                    InSamples[4*pos + 1] = 0f;
                    InSamples[4 * pos + 2] = -1.0f;
                    InSamples[4 * pos + 3] = 0f;
                }
                */
                Execute();
                ResultAvailable = true;
            }
        }

    }
}