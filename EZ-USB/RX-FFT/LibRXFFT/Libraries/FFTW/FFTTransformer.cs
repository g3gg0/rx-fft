using System;
using System.Runtime.InteropServices;
using LibRXFFT.Libraries.Timers;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.FFTW
{
    public class FFTTransformer
    {
        internal class FFTServerVars<T>
        {
            internal T[] InSamples;
            internal T[] OutSamples;

            internal GCHandle hin;
            internal GCHandle hout;

            internal IntPtr hinAddr;
            internal IntPtr houtAddr;

            internal IntPtr fplan;

            internal double totalTime;

            internal FFTServerVars(int fftSize)
            {
                InSamples = new T[fftSize * 2];
                OutSamples = new T[fftSize * 2];
                hin = GCHandle.Alloc(InSamples, GCHandleType.Pinned);
                hout = GCHandle.Alloc(OutSamples, GCHandleType.Pinned);
                hinAddr = hin.AddrOfPinnedObject();
                houtAddr = hout.AddrOfPinnedObject();
            }
        }

        FFTServerVars<double> VarsFFTW;
        FFTServerVars<float> VarsKISS;

        HighPerformanceCounter Counter;

        internal int FFTPos = 0;
        internal int FFTSize = 0;
        public bool ResultAvailable = false;
        public bool Available = false;

        public static bool UseFFTW = true;

        /* differences in signal are at about -200dB */
        public static bool DiffFFTWKISS = false;

        internal double WindowingConstant = 1.0f;
        internal double WindowingCorrection = 1.0f;
        internal double WindowingCorrectionSquared = 1.0f;
        internal double[] WindowFuncTable;
        internal eWindowingFunction _WindowingFunction = eWindowingFunction.BlackmanHarris;



        [DllImport("libRXFFT_native.dll", EntryPoint = "FFTInit", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern IntPtr FFTInit(int size, IntPtr inData, IntPtr outData);
        [DllImport("libRXFFT_native.dll", EntryPoint = "FFTProcess", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void FFTProcess(IntPtr ctx);
        [DllImport("libRXFFT_native.dll", EntryPoint = "FFTFree", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void FFTFree(IntPtr ctx);

        public FFTTransformer()
        { 
        }

        public FFTTransformer(int FFTSize)
        {
            this.FFTSize = FFTSize;
            BuildWindowFuncTable(WindowingFunction);

            Counter = new HighPerformanceCounter("FFTW/KISS Performance Measurement");

            VarsFFTW = new FFTServerVars<double>(FFTSize);
            VarsKISS = new FFTServerVars<float>(FFTSize);

            /* try to use FFTW, fallback to KISS */
            try
            {
                VarsFFTW.fplan = fftw.dft_1d(FFTSize, VarsFFTW.hinAddr, VarsFFTW.houtAddr, fftw_direction.Forward, fftw_flags.Estimate);
                Available = true;
            }
            catch (DllNotFoundException ex)
            {
                UseFFTW = false;
            }
            catch (BadImageFormatException ex)
            {
                UseFFTW = false;
            }

            try
            {
                VarsKISS.fplan = FFTInit(FFTSize, VarsKISS.hinAddr, VarsKISS.houtAddr);
                Available = true;
            }
            catch (DllNotFoundException ex)
            {
            }
            catch (BadImageFormatException ex)
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

            for (int n = 0; n < FFTSize; n++)
            {
                double M = FFTSize - 1;
                double value = 0;

                switch (function)
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

        public double FFTWKissRatio
        {
            get
            {
                return (VarsFFTW.totalTime / VarsKISS.totalTime) * 100;
            }
        }

        virtual protected void Execute()
        {
            if (DiffFFTWKISS)
            {
                Counter.Start();
                FFTProcess(VarsKISS.fplan);
                Counter.Stop();
                VarsKISS.totalTime += Counter.Duration;

                Counter.Start();
                fftw.execute(VarsFFTW.fplan);
                Counter.Stop();
                VarsFFTW.totalTime += Counter.Duration;
            }

            if (UseFFTW)
            {
                fftw.execute(VarsFFTW.fplan);
            }
            else
            {
                FFTProcess(VarsKISS.fplan);
            }
        }

        virtual public double[] GetResult()
        {
            return GetResult(null);
        }

        virtual public double[] GetResult(double[] amplitudes)
        {
            ResultAvailable = false;

            if (amplitudes == null)
            {
                amplitudes = new double[FFTSize];
            }

            if (amplitudes.Length != FFTSize)
            {
                Array.Resize(ref amplitudes, FFTSize);
            }

            if (DiffFFTWKISS)
            {
                for (int pos = 0; pos < FFTSize; pos++)
                {
                    double I = 0;
                    double Q = 0;

                    I = VarsFFTW.OutSamples[2 * pos] - VarsKISS.OutSamples[2 * pos];
                    Q = VarsFFTW.OutSamples[2 * pos + 1] - VarsKISS.OutSamples[2 * pos + 1];

                    I /= FFTSize;
                    Q /= FFTSize;

                    /* the output is not properly aligned, so start in the middle */
                    amplitudes[(pos + FFTSize / 2) % FFTSize] = DBTools.SampleTodB(WindowingCorrection * Math.Sqrt(I * I + Q * Q));
                }
            }
            else
            {
                if (UseFFTW)
                {
                    for (int pos = 0; pos < FFTSize; pos++)
                    {
                        double I = 0;
                        double Q = 0;

                        I = VarsFFTW.OutSamples[2 * pos];
                        Q = VarsFFTW.OutSamples[2 * pos + 1];

                        I /= FFTSize;
                        Q /= FFTSize;

                        /* the output is not properly aligned, so start in the middle */
                        amplitudes[(pos + FFTSize / 2) % FFTSize] = DBTools.SampleTodB(WindowingCorrection * Math.Sqrt(I * I + Q * Q));
                    }
                }
                else
                {
                    for (int pos = 0; pos < FFTSize; pos++)
                    {
                        double I = 0;
                        double Q = 0;

                        I = VarsKISS.OutSamples[2 * pos];
                        Q = VarsKISS.OutSamples[2 * pos + 1];

                        I /= FFTSize;
                        Q /= FFTSize;

                        /* the output is not properly aligned, so start in the middle */
                        amplitudes[(pos + FFTSize / 2) % FFTSize] = DBTools.SampleTodB(WindowingCorrection * Math.Sqrt(I * I + Q * Q));
                    }
                }
            }
            return amplitudes;
        }



        /* a more optimized version, that returns the amplitudes still squared. useful if the data later would get square-rooted. 
         * update: will behave the same as the other - both return dB.
         */
        virtual public double[] GetResultSquared(double[] amplitudes)
        {
            ResultAvailable = false;

            if (amplitudes == null)
            {
                amplitudes = new double[FFTSize];
            }

            if (amplitudes.Length != FFTSize)
            {
                Array.Resize(ref amplitudes, FFTSize);
            }

#if false
            if (DiffFFTWKISS)
            {
                for (int pos = 0; pos < FFTSize; pos++)
                {
                    double I = 0;
                    double Q = 0;

                    I = VarsFFTW.OutSamples[2 * pos] - VarsKISS.OutSamples[2 * pos];
                    Q = VarsFFTW.OutSamples[2 * pos + 1] - VarsKISS.OutSamples[2 * pos + 1];

                    I /= FFTSize;
                    Q /= FFTSize;

                    /* the output is not properly aligned, so start in the middle */
                    amplitudes[(pos + FFTSize / 2) % FFTSize] = DBTools.SquaredSampleTodB(WindowingCorrection * (I * I + Q * Q));
                }
            }
            else
#endif
            {
                int startPos = amplitudes.Length - 1 ;

                if (UseFFTW)
                {
                    for (int pos = 0; pos < FFTSize; pos++)
                    {
                        double I = 0;
                        double Q = 0;

                        I = VarsFFTW.OutSamples[2 * pos];
                        Q = VarsFFTW.OutSamples[2 * pos + 1];

                        I /= FFTSize;
                        Q /= FFTSize;

                        /* the output is not properly aligned, so start in the middle */
                        amplitudes[(pos + FFTSize / 2) % FFTSize] = DBTools.SquaredSampleTodB(WindowingCorrection * (I * I + Q * Q));
                    }
                }
                else
                {
                    for (int pos = 0; pos < FFTSize; pos++)
                    {
                        double I = 0;
                        double Q = 0;

                        I = VarsKISS.OutSamples[2 * pos];
                        Q = VarsKISS.OutSamples[2 * pos + 1];

                        I /= FFTSize;
                        Q /= FFTSize;

                        /* the output is not properly aligned, so start in the middle */
                        amplitudes[(pos + FFTSize / 2) % FFTSize] = DBTools.SquaredSampleTodB(WindowingCorrection * (I * I + Q * Q));
                    }
                }
            }
            return amplitudes;
        }

        virtual public void AddSample(double I, double Q)
        {
            if (UseFFTW || DiffFFTWKISS)
            {
                VarsFFTW.InSamples[2 * FFTPos] = I * WindowFuncTable[FFTPos];
                VarsFFTW.InSamples[2 * FFTPos + 1] = Q * WindowFuncTable[FFTPos];
            }

            if (!UseFFTW || DiffFFTWKISS)
            {
                VarsKISS.InSamples[2 * FFTPos] = (float)(I * WindowFuncTable[FFTPos]);
                VarsKISS.InSamples[2 * FFTPos + 1] = (float)(Q * WindowFuncTable[FFTPos]);
            }

            FFTPos++;
            if (FFTPos == FFTSize)
            {
                FFTPos = 0;

                Execute();
                ResultAvailable = true;
            }
        }
    }
}