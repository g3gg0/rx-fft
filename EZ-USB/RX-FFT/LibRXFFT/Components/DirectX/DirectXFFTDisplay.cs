using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using Timer = System.Timers.Timer;
using Font = SlimDX.Direct3D9.Font;
using SlimDX.Direct3D9;
using LibRXFFT.Libraries.SignalProcessing;

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

        Vertex[] CursorVertexesVert = new Vertex[4];
        Vertex[] CursorVertexesHor = new Vertex[3];
        Vertex[] ScaleVertexes = new Vertex[100];

        public bool ShowFPS { get; set; }
        public bool UseLines { get; set; }

        private int _FFTSize = 256;
        public double FFTPrescaler = 1.0f;
        public double FFTOffset = 0.0f;
        private double fftPrescalerDefault = 1.0f;
        private double fftOffsetDefault = 0.0f;

        public double Averaging = 1;

        /* if the fft data provided is already squared, set to true */
        public bool SquaredFFTData = false;

        private double MaxPower = -99999.0f;

        internal Font SmallFont = null;

        internal override void AllocateResources()
        {
            SmallFont = new Font(Device, new System.Drawing.Font("Arial", 8));
        }

        internal override void ReleaseResources()
        {
            if (SmallFont != null)
                SmallFont.Dispose();
            SmallFont = null;
        }

        protected override void CreateVertexBufferForPoints(Point[] points, int numPoints)
        {
            if (points == null)
                return;

            try
            {
                DirectXLock.WaitOne();

                if (Device != null)
                {
                    uint color = 0x9F00FFFF;//(uint)ColorFG.ToArgb();

                    if (numPoints > 0)
                    {
                        if (numPoints > PlotVerts.Length)
                            PlotVerts = new Vertex[numPoints];

                        PlotVertsEntries = numPoints - 1;


                        for (int pos = 0; pos < numPoints; pos++)
                        {
                            PlotVerts[pos].PositionRhw.X = (float)(XAxisSampleOffset * XZoomFactor - DisplayXOffset + points[pos].X * XZoomFactor);
                            PlotVerts[pos].PositionRhw.Y = (float)(-sampleToDBScale(points[pos].Y));
                            PlotVerts[pos].PositionRhw.Z = 0.5f;
                            PlotVerts[pos].PositionRhw.W = 1;
                            PlotVerts[pos].Color = color;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return;
            }
            finally
            {
                DirectXLock.ReleaseMutex();
            }
        }

        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }


        private Cursor CreateEmptyCursor()
        {
            Bitmap b = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(b);
            // do whatever you wish
            //g.DrawString("arya", this.Font, Brushes.Blue, 0, 0);
            // this is the trick!
            IntPtr ptr = b.GetHicon();
            return new Cursor(ptr);
        }

        public DirectXFFTDisplay()
        {
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;
            ColorFont = Color.DarkCyan;

            ShowFPS = true;
            UseLines = true;
            YAxisCentered = false;

            YZoomFactor = 1.0f;
            XZoomFactor = 1.0f;

            ActionMouseDragX = eUserAction.None;
            ActionMouseWheelShift = eUserAction.None;

            InitializeComponent();
            InitializeDirectX();

            this.Cursor = CreateEmptyCursor();

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
                if (SampleValues.Count == 0)
                {
                    SampleValues.Add(amplitudes);
                    NeedsUpdate = true;
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
                        double[] amplitudes = FFT.GetResult();

                        ProcessFFTData(amplitudes);
                    }
                }
            }
        }

        private double sampleToDBScale(double sampleValue)
        {
            return FFTPrescaler * sampleValue + FFTOffset;
        }

        private double sampleFromDBScale(double scaleValue)
        {
            return (scaleValue - FFTOffset) / FFTPrescaler;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            FFTOffset = 0;
            FFTPrescaler = (double)Height / 150;

        }

        internal override void RenderOverlay ()
        {
            /* only recalc scale lines when axis need to get updated */
            if (UpdateOverlays)
            {
                for (int dBLevel = 0; dBLevel <= 15; dBLevel++)
                {
                    int pos = 4 * dBLevel;

                    ScaleVertexes[pos].PositionRhw.X = 0;
                    ScaleVertexes[pos].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel * 10);
                    ScaleVertexes[pos].PositionRhw.Z = 0.5f;
                    ScaleVertexes[pos].PositionRhw.W = 1;
                    ScaleVertexes[pos].Color = 0xFFFF3030;

                    if(dBLevel % 10 == 0)
                        ScaleVertexes[pos + 1].PositionRhw.X = 50;
                    else if(dBLevel % 5 == 0)
                        ScaleVertexes[pos + 1].PositionRhw.X = 20;
                    else
                        ScaleVertexes[pos + 1].PositionRhw.X = 10;

                    ScaleVertexes[pos + 1].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel * 10);
                    ScaleVertexes[pos + 1].PositionRhw.Z = 0.5f;
                    ScaleVertexes[pos + 1].PositionRhw.W = 1;
                    ScaleVertexes[pos + 1].Color = 0x00FF3030;

                    ScaleVertexes[pos + 2].PositionRhw.X = DirectXWidth;
                    ScaleVertexes[pos + 2].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel * 10);
                    ScaleVertexes[pos + 2].PositionRhw.Z = 0.5f;
                    ScaleVertexes[pos + 2].PositionRhw.W = 1;
                    ScaleVertexes[pos + 2].Color = 0xFFFF3030;

                    if (dBLevel % 10 == 0)
                        ScaleVertexes[pos + 3].PositionRhw.X = DirectXWidth - 50;
                    else if (dBLevel % 5 == 0)
                        ScaleVertexes[pos + 3].PositionRhw.X = DirectXWidth - 20;
                    else
                        ScaleVertexes[pos + 3].PositionRhw.X = DirectXWidth - 10;


                    ScaleVertexes[pos + 3].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel * 10);
                    ScaleVertexes[pos + 3].PositionRhw.Z = 0.5f;
                    ScaleVertexes[pos + 3].PositionRhw.W = 1;
                    ScaleVertexes[pos + 3].Color = 0x00FF3030;
                }
            }

            /* draw scale */
            Device.DrawUserPrimitives(PrimitiveType.LineList, 16*2, ScaleVertexes);
            SmallFont.DrawString(null, "0 dB", 10, (int)-sampleToDBScale(0), 0x7FFF3030);
            SmallFont.DrawString(null, "-50 dB", 10, (int)-sampleToDBScale(-50), 0x7FFF3030);
            SmallFont.DrawString(null, "-100 dB", 10, (int)-sampleToDBScale(-100), 0x7FFF3030);
            SmallFont.DrawString(null, "-150 dB", 10, (int)-sampleToDBScale(-150), 0x7FFF3030);
            //SmallFont.DrawString(null, "-200 dB", 10, (int)-sampleToDBScale(-200), 0x7FFF3030);


            /* draw vertical line */
            float stubLength = (float)DirectXHeight / 10.0f;
            float xPos = (float)LastMousePos.X;
            float yPos = (float)LastMousePos.Y;
            float dB = (float)sampleFromDBScale(-yPos);

            if(xPos > DirectXWidth/2)
                CursorVertexesVert[0].PositionRhw.X = xPos - 20;
            else
                CursorVertexesVert[0].PositionRhw.X = xPos + 20;
            CursorVertexesVert[1].PositionRhw.X = xPos;
            CursorVertexesVert[2].PositionRhw.X = xPos;
            CursorVertexesVert[3].PositionRhw.X = xPos;

            /* if axis changed, recalc lines */
            if (UpdateOverlays)
            {
                CursorVertexesVert[0].PositionRhw.Y = 20;
                CursorVertexesVert[0].PositionRhw.Z = 0.5f;
                CursorVertexesVert[0].PositionRhw.W = 1;
                CursorVertexesVert[0].Color = 0x00FF3030;

                CursorVertexesVert[1].PositionRhw.Y = 20 + stubLength;
                CursorVertexesVert[1].PositionRhw.Z = 0.5f;
                CursorVertexesVert[1].PositionRhw.W = 1;
                CursorVertexesVert[1].Color = 0xFFFF3030;

                CursorVertexesVert[2].PositionRhw.Y = DirectXHeight - stubLength;
                CursorVertexesVert[2].PositionRhw.Z = 0.5f;
                CursorVertexesVert[2].PositionRhw.W = 1;
                CursorVertexesVert[2].Color = 0xFFFF3030;

                CursorVertexesVert[3].PositionRhw.Y = DirectXHeight;
                CursorVertexesVert[3].PositionRhw.Z = 0.5f;
                CursorVertexesVert[3].PositionRhw.W = 1;
                CursorVertexesVert[3].Color = 0x00FF3030;
            }

            Device.DrawUserPrimitives(PrimitiveType.LineStrip, 3, CursorVertexesVert);

            /* horizontal line */
            CursorVertexesHor[0].PositionRhw.X = xPos - 15;
            CursorVertexesHor[0].PositionRhw.Y = yPos;

            CursorVertexesHor[1].PositionRhw.X = xPos;
            CursorVertexesHor[1].PositionRhw.Y = yPos;

            CursorVertexesHor[2].PositionRhw.X = xPos + 15;
            CursorVertexesHor[2].PositionRhw.Y = yPos;

            /* if axis changed, recalc lines (this is needed just once btw.) */
            if (UpdateOverlays)
            {
                CursorVertexesHor[0].PositionRhw.Z = 0.5f;
                CursorVertexesHor[0].PositionRhw.W = 1;
                CursorVertexesHor[0].Color = 0x00FF3030;

                CursorVertexesHor[1].PositionRhw.Z = 0.5f;
                CursorVertexesHor[1].PositionRhw.W = 1;
                CursorVertexesHor[1].Color = 0xFFFF3030;

                CursorVertexesHor[2].PositionRhw.Z = 0.5f;
                CursorVertexesHor[2].PositionRhw.W = 1;
                CursorVertexesHor[2].Color = 0x00FF3030;
            }

            Device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, CursorVertexesHor);

            /* draw the horizontal position (preliminary) */
            if (xPos > DirectXWidth / 2)
                SmallFont.DrawString(null, ((xPos * 100) / DirectXWidth).ToString(), (int)xPos - 40, 20, ColorFG.ToArgb());
            else
                SmallFont.DrawString(null, ((xPos * 100) / DirectXWidth).ToString(), (int)xPos + 20, 20, ColorFG.ToArgb());

            /* and the strength at the current position */
            SmallFont.DrawString(null, dB + " dB", (int)xPos + 20, (int)yPos, ColorFG.ToArgb());
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
                                    double posY = SquaredFFTData ? DBTools.SquaredSampleTodB(sampleValue) : DBTools.SampleTodB(sampleValue);

                                    LinePoints[pos].X = posX;
                                    //LinePoints[pos].Y = posY;

                                    /* some simple averaging */
                                    unchecked
                                    {
                                        LinePoints[pos].Y *= (Averaging - 1) / Averaging;
                                        LinePoints[pos].Y += posY / Averaging;
                                    }

                                    if (double.IsNaN(LinePoints[pos].Y))
                                        LinePoints[pos].Y = 0;
                                }

                                LinePointEntries = samples;
                                LinePointsUpdated = true;
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
            protected internal DateTime StartTime;
            protected internal long FrameNumber;
            protected internal double FPS;

            public DisplayFuncState()
            {
                FPS = 0;
                FrameNumber = 0;
                StartTime = DateTime.Now;
            }
        }

    }
}