using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using Timer=System.Timers.Timer;

namespace LibRXFFT.Components.GDI
{
    public partial class WaterfallDisplay : UserControl
    {
        Bitmap Image;
        Bitmap ImageBuffer;
        Bitmap ImageBufferTmp;
        Graphics ImageGraph;
        Graphics ImageBufferGraph;
        Graphics ImageBufferTmpGraph;
        Rectangle ImageScrollRect;


        Color[] ColorTable;
        Pen[] PenTable;
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

        private int WaterfallLine = 0;

        private int _FFTSize = 512;
        private double FFTPrescaler = 0.05f;
        private double FFTOffset = 0.3f;

        public WaterfallDisplay()
        {
            UseLines = true;
            MaxSamples = 10000;

            InitColorTable(Color.Cyan);

            InitializeComponent();

            Image = new Bitmap(Width, Height);
            ImageGraph = Graphics.FromImage(Image);
            ImageBuffer = new Bitmap(Width, Height);
            ImageBufferGraph = Graphics.FromImage(ImageBuffer);
            ImageBufferTmp = new Bitmap(Width, Height);
            ImageBufferTmpGraph = Graphics.FromImage(ImageBufferTmp);
            ImageScrollRect = new Rectangle(0, 0, Width, Height - 1);
            ImageBufferGraph.SmoothingMode = SmoothingMode.HighSpeed;

            DisplayTimer.Elapsed += DisplayFunc;
            DisplayTimer.Interval = 20;
            DisplayTimer.Start();
        }

        public void InitColorTable(Color color)
        {
            double R = color.R;
            double G = color.G;
            double B = color.B;

            ColorTable = new Color[256];
            PenTable = new Pen[256];

            for (int pos = 0; pos < 256; pos++)
            {
                int curR = (int)((R * pos) / 256);
                int curG = (int)((G * pos) / 256);
                int curB = (int)((B * pos) / 256);

                ColorTable[pos] = Color.FromArgb(curR, curG, curB);
                PenTable[pos] = new Pen(ColorTable[pos]);
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
            lock (SampleValues)
            {
                Image = new Bitmap(Width, Height);
                ImageGraph = Graphics.FromImage(Image);
                ImageBuffer = new Bitmap(Width, Height);
                ImageBufferGraph = Graphics.FromImage(ImageBuffer);
                ImageBufferTmp = new Bitmap(Width, Height);
                ImageBufferTmpGraph = Graphics.FromImage(ImageBufferTmp);
                ImageScrollRect = new Rectangle(0, 0, Width, Height - 1);
                ImageBufferGraph.SmoothingMode = SmoothingMode.HighSpeed;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            lock (SampleValues)
            {
                Image = new Bitmap(Width, Height);
                ImageGraph = Graphics.FromImage(Image);
                ImageBuffer = new Bitmap(Width, Height);
                ImageBufferGraph = Graphics.FromImage(ImageBuffer);
                ImageBufferTmp = new Bitmap(Width, Height);
                ImageBufferTmpGraph = Graphics.FromImage(ImageBufferTmp);
                ImageScrollRect = new Rectangle(0, 0, Width, Height - 1);

                ImageBufferGraph.SmoothingMode = SmoothingMode.HighSpeed;
            }
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
//                ImageBufferGraph.Clear(Color.Black);

                if (SampleValues.Count > 0)
                {
                    double[] samples = (double[])SampleValues[0];
                    double ratioX = (double)Width / samples.Length;
                    int lastX = 0;
                    int lastY = 0;

                    if (ratioX >= 1.0f)
                    {
                        for (int pos = 0; pos < samples.Length; pos++)
                        {
                            int posX = (int)(pos * ratioX);
                            int posY = WaterfallLine;

                            double sampleValue = sampleToDBScale(samples[pos]);
                            sampleValue *= 256;
                            sampleValue = Math.Min(sampleValue, 255);
                            sampleValue = Math.Max(sampleValue, 0);
                            Color color = ColorTable[(int)sampleValue];
                            Pen pen = PenTable[(int)sampleValue];

                            posX = Math.Min(posX, Width - 1);
                            posX = Math.Max(posX, 0);
                            
                            if (pos > 0)
                                ImageBufferGraph.DrawLine(pen, lastX, lastY, posX, posY);
                            else
                                ImageBuffer.SetPixel(posX, posY, color);
                            
                            lastX = posX;
                            lastY = posY;
                        }
                    }
                    else
                    {
                        for (int posX = 0; posX < Width; posX++)
                        {
                            int posY = WaterfallLine;

                            int pos = (int)(posX / ratioX);
                            pos = Math.Min(pos, samples.Length - 1);
                            pos = Math.Max(pos, 0);

                            double sampleValue = sampleToDBScale(samples[pos]);
                            sampleValue *= 256;
                            sampleValue = Math.Min(sampleValue, 255);
                            sampleValue = Math.Max(sampleValue, 0);
                            Color color = ColorTable[(int)sampleValue];
                            Pen pen = PenTable[(int)sampleValue];
                            
                            if (pos > 0)
                                ImageBufferGraph.DrawLine(pen, lastX, lastY, posX, posY);
                            else
                                ImageBuffer.SetPixel(posX, posY, color);
                            
                            lastX = posX;
                            lastY = posY;
                        }
                    }

                    SampleValues.Clear();
                    
                    if (ShowFPS)
                    {
                        if (s.FrameNumber++ > 100 && s.FrameNumber % 20 == 0)
                            s.FPS = s.FrameNumber / DateTime.Now.Subtract(s.StartTime).TotalSeconds;
                        ImageBufferGraph.DrawString("FPS: " + (int)s.FPS, s.TextFont, Brushes.Cyan, 10, 10);
                    }

                    ImageGraph.DrawImage(ImageBuffer, 0, 0);
                    Invalidate();

                    /* scroll up for the next waterfall line */
                    if (++WaterfallLine > Height - 1)
                    {
                        WaterfallLine = Height - 1;
                        
                        ImageBufferTmpGraph.DrawImage(ImageBuffer, ImageScrollRect, 0, 1, Width, Height - 1, GraphicsUnit.Pixel);
                        ImageBufferGraph.Clear(Color.Black);
                        ImageBufferGraph.DrawImage(ImageBufferTmp, 0, 0);
                    }
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