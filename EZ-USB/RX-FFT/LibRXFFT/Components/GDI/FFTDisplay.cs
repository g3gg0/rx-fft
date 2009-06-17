using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using Timer = System.Timers.Timer;

namespace LibRXFFT.Components.GDI
{
    public partial class FFTDisplay : UserControl
    {
        Bitmap Image;
        Bitmap ImageBuffer;
        Graphics ImageGraph;
        Graphics ImageBufferGraph;
        Point[] LinePoints;

        readonly Color colorFG;
        readonly Color colorBG;
        readonly Pen LinePen;
        readonly Timer DisplayTimer = new Timer();
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

        public FFTDisplay()
        {
            ShowFPS = true;
            UseLines = true;
            MaxSamples = 10000;
            colorFG = Color.Cyan;
            colorBG = Color.Black;
            LinePen = new Pen(colorFG);

            InitializeComponent();

            InitFields();

            DisplayTimer.Elapsed += DisplayFunc;
            DisplayTimer.Interval = 20;
            DisplayTimer.Start();
        }

        private void InitFields ()
        {
            lock (SampleValues)
            {
                Image = new Bitmap(Width, Height);
                ImageGraph = Graphics.FromImage(Image);
                ImageBuffer = new Bitmap(Width, Height);
                ImageBufferGraph = Graphics.FromImage(ImageBuffer);

                LinePoints = new Point[Math.Min(FFTSize, Width)];
                for(int pos =0; pos < LinePoints.Length; pos++)
                    LinePoints[pos] = new Point(0,0);
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
            lock (SampleValues)
            {
                e.Graphics.DrawImage(Image, 0, 0);
            }
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

                        lock (SampleValues)
                        {
                            SampleValues.Add(amplitudes);
                        }
                    }
                }
            }
        }

        private double sampleToDBScale(double sampleValue)
        {
            double dbValue = Math.Log(sampleValue * sampleValue);
            return FFTPrescaler * dbValue + FFTOffset;
        }


        private void DisplayFunc(object state, ElapsedEventArgs e)
        {
            DisplayFuncState s = DisplayState;

            lock (SampleValues)
            {
                if (SampleValues.Count > 0)
                {
                    ImageBufferGraph.Clear(colorBG);

                    double[] samples = (double[])SampleValues[0];
                    double ratioX = (double)Width / samples.Length;

                    if (ratioX >= 1.0f)
                    {
                        for (int pos = 0; pos < samples.Length; pos++)
                        {
                            double sampleValue = sampleToDBScale(samples[pos]);
                            int posX = (int)(pos * ratioX);
                            int posY = (int)(Height - sampleValue * Height) / 2;

                            posX = Math.Min(posX, Width - 1);
                            posX = Math.Max(posX, 0);
                            posY = Math.Min(posY, Height - 1);
                            posY = Math.Max(posY, 0);

                            LinePoints[pos].X = posX;
                            LinePoints[pos].Y = posY;
                            /*
                            if (UseLines && pos > 0)
                                ImageBufferGraph.DrawLine(LinePen, lastX, lastY, posX, posY);
                            else
                                ImageBuffer.SetPixel(posX, posY, colorFG);
                            
                            lastX = posX;
                            lastY = posY;*/
                        }
                    }
                    else
                    {
                        for (int posX = 0; posX < Width; posX++)
                        {
                            int pos = (int)(posX / ratioX);
                            pos = Math.Min(pos, samples.Length - 1);
                            pos = Math.Max(pos, 0);

                            double sampleValue = sampleToDBScale(samples[pos]);
                            int posY = (int)(Height - sampleValue * Height) / 2;

                            posY = Math.Min(posY, Height - 1);
                            posY = Math.Max(posY, 0);

                            LinePoints[posX].X = posX;
                            LinePoints[posX].Y = posY;
                            /*
                            if (UseLines && pos > 0)
                                ImageBufferGraph.DrawLine(LinePen, lastX, lastY, posX, posY);
                            else
                                ImageBuffer.SetPixel(posX, posY, colorFG);
                            
                            lastX = posX;
                            lastY = posY;
                             * */
                        }
                    }

                    ImageBufferGraph.DrawLines(LinePen, LinePoints);



                    SampleValues.Clear();

                    if (ShowFPS)
                    {
                        if (s.FrameNumber++ > 100 && s.FrameNumber % 20 == 0)
                            s.FPS = s.FrameNumber / DateTime.Now.Subtract(s.StartTime).TotalSeconds;
                        ImageBufferGraph.DrawString("FPS: " + (int)s.FPS, s.TextFont, Brushes.Cyan, 10, 10);
                    }

                    ImageGraph.DrawImageUnscaled(ImageBuffer, 0, 0);

                    Invalidate();
                }
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