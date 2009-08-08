using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using System;
using LibRXFFT.Libraries.GSM.Misc;

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

            fftDisplay.ActionMouseClickRight = eUserAction.UserCallback;
            fftDisplay.ActionMousePosX = eUserAction.UserCallback;
            fftDisplay.ActionMouseDragX = eUserAction.UserCallback;
            fftDisplay.ActionMouseDragY = eUserAction.YOffset;
            fftDisplay.ActionMouseDragXShift = eUserAction.UserCallback;
            fftDisplay.ActionMouseDragYShift = eUserAction.YOffset;
            fftDisplay.ActionMouseWheelUp = eUserAction.UserCallback;
            fftDisplay.ActionMouseWheelDown = eUserAction.UserCallback;
            fftDisplay.ActionMouseWheelUpShift = eUserAction.UserCallback;
            fftDisplay.ActionMouseWheelDownShift = eUserAction.UserCallback;

            waterfallDisplay.ActionMouseClickRight = eUserAction.UserCallback;
            waterfallDisplay.ActionMousePosX = eUserAction.UserCallback;
            waterfallDisplay.ActionMouseDragX = eUserAction.UserCallback;
            waterfallDisplay.ActionMouseDragXShift = eUserAction.UserCallback;
            waterfallDisplay.ActionMouseWheelUpAlt = eUserAction.UserCallback;
            waterfallDisplay.ActionMouseWheelDownAlt = eUserAction.UserCallback;
        }


        public double SamplingRate
        {
            set { fftDisplay.SamplingRate = value; }
            get { return fftDisplay.SamplingRate; }
        }


        public double UpdateRate
        {
            set 
            { 
                fftDisplay.UpdateRate = value;
                waterfallDisplay.UpdateRate = value;
            }
            get { return fftDisplay.UpdateRate; }
        }

        public double Averaging
        {
            get { return fftDisplay.Averaging; }
            set 
            { 
                fftDisplay.Averaging = value;
                waterfallDisplay.Averaging = value;
            }
        }

        public FFTTransformer.eWindowingFunction WindowingFunction
        {
            get { return FFT.WindowingFunction; }
            set { FFT.WindowingFunction = value; }
        }

        public void UserEventCallback(eUserEvent evt, double param)
        {
            switch (evt)
            {
                case eUserEvent.MouseClickRight:
                    double freq = fftDisplay.FrequencyFromCursorPos();
                    break;

                case eUserEvent.MouseWheelUp:
                    fftDisplay.ProcessUserAction(eUserAction.YZoomIn, param);
                    break;

                case eUserEvent.MouseWheelDown:
                    fftDisplay.ProcessUserAction(eUserAction.YZoomOut, param);
                    break;


                case eUserEvent.MousePosX:
                    fftDisplay.ProcessUserAction(eUserAction.XPos, param);
                    waterfallDisplay.ProcessUserAction(eUserAction.XPos, param);
                    break;

                case eUserEvent.MouseDragXShift:
                    fftDisplay.ProcessUserAction(eUserAction.XOffsetOverview, param);
                    waterfallDisplay.ProcessUserAction(eUserAction.XOffsetOverview, param);
                    break;

                case eUserEvent.MouseDragX:
                    fftDisplay.ProcessUserAction(eUserAction.XOffset, param);
                    waterfallDisplay.ProcessUserAction(eUserAction.XOffset, param);
                    break;

                case eUserEvent.MouseWheelUpShift:
                    fftDisplay.ProcessUserAction(eUserAction.XZoomIn, param);
                    waterfallDisplay.ProcessUserAction(eUserAction.XZoomIn, param);
                    break;

                case eUserEvent.MouseWheelDownShift:
                    fftDisplay.ProcessUserAction(eUserAction.XZoomOut, param);
                    waterfallDisplay.ProcessUserAction(eUserAction.XZoomOut, param);
                    break;

                case eUserEvent.MouseWheelUpAlt:
                    if (waterfallDisplay.UpdateRate < 512)
                    {
                        fftDisplay.UpdateRate *= 2;
                        waterfallDisplay.UpdateRate *= 2;
                    }
                    break;

                case eUserEvent.MouseWheelDownAlt:
                    if (waterfallDisplay.UpdateRate > 1)
                    {
                        fftDisplay.UpdateRate /= 2;
                        waterfallDisplay.UpdateRate /= 2;
                    }
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