using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using System;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXWaterfallFFTDisplay : UserControl
    {
        Mutex FFTLock = new Mutex();
        private FFTTransformer FFT = new FFTTransformer(256);
        private int _FFTSize = 256;
        private double[] FFTResult = new double[256];

        double IMax = -100;
        double IMin = 100;

        public DirectXWaterfallFFTDisplay()
        {
            InitializeComponent();

            /* we work with already squared FFT values for performance reasons */
            fftDisplay.SquaredFFTData = true;
            waterfallDisplay.SquaredFFTData = true;

            /* handle X Zoom and X Offset ourselves */
            fftDisplay.UserEventCallback = UserEventCallback;
            waterfallDisplay.UserEventCallback = UserEventCallback;

            fftDisplay.ActionMousePosX = eUserAction.UserCallback;
            fftDisplay.ActionMouseDragY = eUserAction.UserCallback;
            fftDisplay.ActionMouseWheel = eUserAction.UserCallback;

            waterfallDisplay.ActionMousePosX = eUserAction.UserCallback;
        }


        public double SamplingRate
        {
            set { fftDisplay.SamplingRate = value; }
        }

        public double Averaging
        {
            get { return fftDisplay.Averaging; }
            set { fftDisplay.Averaging = value; }
        }

        public FFTTransformer.eWindowingFunction WindowingFunction
        {
            get { return FFT.WindowingFunction; }
            set { FFT.WindowingFunction = value; }
        }

        public void UserEventCallback(eUserEvent evt, double delta)
        {
            switch (evt)
            {
                    
                case eUserEvent.MouseWheel:
                    if (delta > 0)
                        fftDisplay.FFTPrescaler *= 1.1f;
                    else
                        fftDisplay.FFTPrescaler /= 1.1f;
                    fftDisplay.AxisUpdated = true;
                    break;

                case eUserEvent.MouseDragY:
                    if (Math.Abs(delta) < 5)
                    {
                        fftDisplay.FFTOffset += delta;
                        fftDisplay.AxisUpdated = true;
                    }
                    break;

                case eUserEvent.MousePosX:
                    fftDisplay.ProcessUserAction(eUserAction.XPos, delta);
                    waterfallDisplay.ProcessUserAction(eUserAction.XPos, delta);
                    break;
            }
        }

        public int FFTSize
        {
            get { return _FFTSize; }
            set
            {
                lock (FFTLock)
                {
                    _FFTSize = value;
                    FFTResult = new double[_FFTSize];
                    FFT = new FFTTransformer(value);

                    fftDisplay.FFTSize = value;
                    waterfallDisplay.FFTSize = value;
                }
            }
        }

        public void ProcessRawData(byte[] dataBuffer)
        {
            const int bytePerSample = 2;
            const int channels = 2;

            lock (FFTLock)
            {
                int samplePairs = dataBuffer.Length / (channels * bytePerSample);

                for (int samplePair = 0; samplePair < samplePairs; samplePair++)
                {
                    int samplePairPos = samplePair * bytePerSample * channels;
                    double I = ByteUtil.getDoubleFromBytes(dataBuffer, samplePairPos);
                    double Q = ByteUtil.getDoubleFromBytes(dataBuffer, samplePairPos + bytePerSample);


                    FFT.AddSample(I, Q);

                    if (FFT.ResultAvailable)
                    {
                        FFT.GetResultSquared(FFTResult);

                        fftDisplay.ProcessFFTData(FFTResult);
                        waterfallDisplay.ProcessFFTData(FFTResult);
                    }
                }
            }
        }

        public void ProcessIQData(double[] iSamples, double[] qSamples)
        {
            lock (FFTLock)
            {
                int samplePairs = iSamples.Length;

                for (int samplePair = 0; samplePair < samplePairs; samplePair++)
                {
                    double I = iSamples[samplePair];
                    double Q = iSamples[samplePair];

                    FFT.AddSample(I, Q);

                    if (FFT.ResultAvailable)
                    {
                        FFT.GetResultSquared(FFTResult);

                        fftDisplay.ProcessFFTData(FFTResult);
                        waterfallDisplay.ProcessFFTData(FFTResult);
                    }
                }
            }
        }


        public void ProcessIQSample(double I, double Q)
        {
            lock (FFTLock)
            {
                FFT.AddSample(I, Q);

                if (FFT.ResultAvailable)
                {
                    FFT.GetResultSquared(FFTResult);

                    fftDisplay.ProcessFFTData(FFTResult);
                    waterfallDisplay.ProcessFFTData(FFTResult);
                }
            }
        }
    }
}