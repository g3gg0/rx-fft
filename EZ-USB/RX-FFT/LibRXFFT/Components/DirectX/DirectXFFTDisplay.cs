using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using Timer = System.Timers.Timer;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXFFTDisplay : DirectXPlot
    {
        private readonly Thread DisplayThread;
        private bool NeedsUpdate = false;

        readonly ArrayList SampleValues = new ArrayList();
        private DisplayFuncState DisplayTimerState;

        private Mutex FFTLock = new Mutex();
        private FFTTransformer FFT;

        public bool ShowFPS { get; set; }
        public bool UseLines { get; set; }

        private int _FFTSize = 512;
        private double FFTPrescaler = 0.05f;
        private double FFTOffset = 0.3f;
        private double fftPrescalerDefault = 0.05f;
        private double fftOffsetDefault = 0.3f;



        public DirectXFFTDisplay()
        {
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;

            ShowFPS = true;
            UseLines = true;
            YAxisCentered = false;
            YZoomFactor = 1.0f;
            XZoomFactor = 1.0f;

            ActionMouseDragX = eUserAction.None;
            ActionMouseWheelShift = eUserAction.None;

            InitializeComponent();
            InitializeDirectX();

            DisplayTimerState = new DisplayFuncState();
            DisplayThread = new Thread(DisplayFunc);
            DisplayThread.Start();
        }


        public int FFTSize
        {
            get { return _FFTSize; }
            set
            {
                lock (FFTLock)
                {
                    lock (SampleValues)
                    {
                        _FFTSize = value;
                        SampleValues.Clear();
                        FFT = new FFTTransformer(value);
                    }
                }
            }
        }

        public void ProcessFFTData(double[] amplitudes)
        {
            lock (SampleValues)
            {
                SampleValues.Add(amplitudes);

                NeedsUpdate = true;
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
                        double[] amplitudes = FFT.GetResult();

                        ProcessFFTData(amplitudes);
                    }
                }
            }
        }

        private double sampleToDBScale(double sampleValue)
        {
            double dbValue = Math.Log(sampleValue * sampleValue);
            return FFTPrescaler * dbValue + FFTOffset;
        }



        private void DisplayFunc()
        {
            DisplayFuncState s = DisplayTimerState;


            while (true)
            {
                lock (SampleValues)
                {
                    if (NeedsUpdate)
                    {
                        NeedsUpdate = false;

                        if (SampleValues.Count > 0)
                        {
                            foreach (double[] sampleArray in SampleValues)
                            {
                                int samples = sampleArray.Length;

                                if (LinePoints == null || LinePoints.Length < samples)
                                    LinePoints = new Point[samples];

                                for (int pos = 0; pos < samples; pos++)
                                {
                                    double sampleValue = (double)sampleArray[pos];
                                    double posX = ((double)pos / (double)samples) * DirectXWidth;
                                    double posY = sampleToDBScale(sampleValue) * DirectXHeight;

                                    LinePoints[pos].X = posX;

                                    /* some simple averaging */
                                    LinePoints[pos].Y *= 5 / 6;
                                    LinePoints[pos].Y += posY / 6;
                                }



                                LinePointEntries = samples;
                                DataUpdated = true;
                            }
                        }
                    }
                    SampleValues.Clear();
                }
                Render();
                Thread.Sleep(10);
            }
        }

        class DisplayFuncState
        {
            protected internal Font TextFont;
            protected internal DateTime StartTime;
            protected internal long FrameNumber;
            protected internal double FPS;

            public DisplayFuncState()
            {
                FPS = 0;
                FrameNumber = 0;
                TextFont = new Font("Arial", 16);
                StartTime = DateTime.Now;
            }
        }

    }
}