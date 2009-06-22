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
        Point[] LinePoints;

        private readonly Thread DisplayThread;
        readonly ArrayList SampleValues = new ArrayList();

        private DisplayFuncState DisplayState = new DisplayFuncState();
        private Mutex FFTLock = new Mutex();
        private FFTTransformer FFT;

        public double ZoomFactor { get; set; }
        public bool ShowFPS { get; set; }
        public bool UseLines { get; set; }
        public int StartSample { get; set; }
        public int MaxSamples { get; set; }

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
            MaxSamples = 10000;

            InitializeComponent();
            InitializeDirectX();
            InitFields();

            DisplayThread = new Thread(DisplayFunc);
            DisplayThread.Start();
        }

        private void InitFields()
        {
            lock (SampleValues)
            {
                LinePoints = new Point[DirectXWidth];
                for (int pos = 0; pos < LinePoints.Length; pos++)
                    LinePoints[pos] = new Point(0, 0);
            }
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
                        InitFields();
                    }
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0 && FFTPrescaler < 10.0f)
                FFTPrescaler += 0.05f;

            if (e.Delta < 0 && FFTPrescaler > 0.05f)
                FFTPrescaler -= 0.05f;
        }

        protected override void OnResize(EventArgs e)
        {
            InitFields();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            InitFields();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
        }

        public void ProcessData(double[] amplitudes)
        {
            lock (SampleValues)
            {
                SampleValues.Add(amplitudes);
            }
        }


        public void ProcessData(byte[] dataBuffer)
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

                        ProcessData(amplitudes);
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
            DisplayFuncState s = DisplayState;

            while (true)
            {
                lock (SampleValues)
                {
                    if (SampleValues.Count > 0)
                    {
                        double[] samples = (double[])SampleValues[0];
                        double ratioX = (double)DirectXWidth / samples.Length;

                        if (ratioX >= 1.0f)
                        {
                            for (int pos = 0; pos < samples.Length; pos++)
                            {
                                double sampleValue = sampleToDBScale(samples[pos]);
                                int posX = (int)(pos * ratioX);
                                int posY = (int)(DirectXHeight - sampleValue * DirectXHeight) / 2;

                                posX = Math.Min(posX, DirectXWidth - 1);
                                posX = Math.Max(posX, 0);
                                posY = Math.Min(posY, DirectXHeight - 1);
                                posY = Math.Max(posY, 0);

                                LinePoints[pos].X = posX;
                                LinePoints[pos].Y = posY;
                            }
                            CreateVertexBufferForPoints(LinePoints, samples.Length);
                        }
                        else
                        {
                            for (int posX = 0; posX < DirectXWidth; posX++)
                            {
                                int pos = (int)(posX / ratioX);
                                pos = Math.Min(pos, samples.Length - 1);
                                pos = Math.Max(pos, 0);

                                double sampleValue = sampleToDBScale(samples[pos]);
                                int posY = (int)(DirectXHeight - sampleValue * DirectXHeight) / 2;

                                posY = Math.Min(posY, DirectXHeight - 1);
                                posY = Math.Max(posY, 0);

                                LinePoints[posX].X = posX;
                                LinePoints[posX].Y = posY;
                            }
                            CreateVertexBufferForPoints(LinePoints, DirectXWidth);
                        }


                        SampleValues.Clear();
                    }
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