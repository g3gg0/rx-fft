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
using LibRXFFT.Libraries.GSM.Misc;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXFFTDisplay : DirectXPlot
    {

        /* DirectX related graphic stuff */
        Vertex[] CursorVertexesVert = new Vertex[4];
        Vertex[] CursorVertexesHor = new Vertex[3];
        Vertex[] ScaleVertexes = new Vertex[100];


        private readonly Thread DisplayThread;
        private bool NeedsUpdate = false;

        /* sample value buffer */
        protected double[] SampleValues = new double[0];
        protected long SampleValuesAveraged = 0;
        public long SamplesToAverage = 0;

        /* processing related */
        private Mutex FFTLock = new Mutex();
        private FFTTransformer FFT;

        private int _FFTSize = 256;
        public double FFTPrescaler = 1.0f;
        public double FFTOffset = 0.0f;
        private double fftPrescalerDefault = 1.0f;
        private double fftOffsetDefault = 0.0f;

        public double Averaging = 1;

        public double SamplingRate = 100;
        

        /* if the fft data provided is already squared, set to true */
        public bool SquaredFFTData = false;

        private double MaxPower = -99999.0f;

        public DirectXFFTDisplay()
            : this(false)
        {
        }

        public DirectXFFTDisplay(bool slaveMode)
        {
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;
            ColorFont = Color.DarkCyan;
            ColorCursor = Color.Red;

            YAxisCentered = false;

            YZoomFactor = 1.0f;
            XZoomFactor = 1.0f;

            ActionMouseDragX = eUserAction.XOffset;
            ActionMouseWheelUp = eUserAction.YZoomIn;
            ActionMouseWheelDown = eUserAction.YZoomOut;
            ActionMouseWheelUpShift = eUserAction.XZoomIn;
            ActionMouseWheelDownShift = eUserAction.XZoomOut;

            InitializeComponent();
            try
            {
                InitializeDirectX();
            }
            catch (Direct3D9Exception e)
            {
                MessageBox.Show("Failed initializing DirectX." + Environment.NewLine + e.ToString());
            }

            this.Cursor = CreateEmptyCursor();

            if (!slaveMode)
            {
                DisplayThread = new Thread(DisplayFunc);
                DisplayThread.Start();
            }
        }


        protected override void AllocateResources()
        {
        }

        protected override void ReleaseResources()
        {
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
                    uint colorFG = ((uint)ColorFG.ToArgb()) & 0xFFFFFF;

                    if (numPoints > 0)
                    {
                        if (numPoints > PlotVerts.Length)
                        {
                            PlotVerts = new Vertex[numPoints];
                            PlotVertsOverview = new Vertex[numPoints];
                        }

                        PlotVertsEntries = numPoints - 1;

                        for (int pos = 0; pos < numPoints; pos++)
                        {
                            double yVal = points[pos].Y;
                            double xPos = ((double)points[pos].X / (double)numPoints) * DirectXWidth;
                            PlotVerts[pos].PositionRhw.X = (float)((XAxisSampleOffset + xPos) * XZoomFactor - DisplayXOffset);
                            PlotVerts[pos].PositionRhw.Y = (float)(-sampleToDBScale(SquaredFFTData ? DBTools.SquaredSampleTodB(yVal) : DBTools.SampleTodB(yVal)));
                            PlotVerts[pos].PositionRhw.Z = 0.5f;
                            PlotVerts[pos].PositionRhw.W = 1;
                            PlotVerts[pos].Color = 0x9F000000 | colorFG;

                            if (OverviewModeEnabled)
                            {
                                PlotVertsOverview[pos].PositionRhw.X = (float)(XAxisSampleOffset + xPos);
                                PlotVertsOverview[pos].PositionRhw.Y = PlotVerts[pos].PositionRhw.Y;
                                PlotVertsOverview[pos].PositionRhw.Z = PlotVerts[pos].PositionRhw.Z;
                                PlotVertsOverview[pos].PositionRhw.W = PlotVerts[pos].PositionRhw.W;
                                PlotVertsOverview[pos].Color = PlotVerts[pos].Color;
                            }
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

            //g.DrawString("Test", this.Font, Brushes.Blue, 0, 0);

            IntPtr ptr = b.GetHicon();
            return new Cursor(ptr);
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
                        SampleValuesAveraged = 0;
                        FFT = new FFTTransformer(value);
                    }
                }
            }
        }

        public void ProcessFFTData(double[] amplitudes)
        {
            lock (SampleValues)
            {
                if (SampleValues.Length != amplitudes.Length)
                    SampleValues = new double[amplitudes.Length];

                if (SampleValuesAveraged == 0)
                {
                    for (int pos = 0; pos < amplitudes.Length; pos++)
                        SampleValues[pos] = amplitudes[pos];
                }
                else
                {
                    for (int pos = 0; pos < amplitudes.Length; pos++)
                        SampleValues[pos] += amplitudes[pos];
                }

                SampleValuesAveraged++;
                if (SampleValuesAveraged > SamplesToAverage)
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
            return FFTPrescaler * sampleValue + FFTOffset;
        }

        private double sampleFromDBScale(double scaleValue)
        {
            return (scaleValue - FFTOffset) / FFTPrescaler;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            XMaximum = DirectXWidth;
            FFTOffset = 0;
            FFTPrescaler = (double)Height / 150;
        }


        public override void ProcessUserAction(eUserAction action, double param)
        {
            switch (action)
            {
                case eUserAction.YZoomIn:
                    FFTOffset = ((LastMousePos.Y + FFTOffset) * YZoomStep) - LastMousePos.Y;
                    FFTPrescaler *= YZoomStep;

                    LinePointsUpdated = true;
                    AxisUpdated = true;
                    break;

                case eUserAction.YZoomOut:
                    FFTOffset = ((LastMousePos.Y + FFTOffset) / YZoomStep) - LastMousePos.Y;
                    FFTPrescaler /= YZoomStep;

                    LinePointsUpdated = true;
                    AxisUpdated = true;
                    break;

                case eUserAction.YOffset:
                    if (Math.Abs(param) < 5)
                    {
                        FFTOffset += param;
                        AxisUpdated = true;
                        LinePointsUpdated = true;
                    }
                    break;

                default:
                    base.ProcessUserAction(action, param);
                    break;
            }
        }

        protected override void RenderOverlay()
        {
            uint color = (uint)ColorCursor.ToArgb();

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
                    ScaleVertexes[pos].Color = color;

                    if (dBLevel % 10 == 0)
                        ScaleVertexes[pos + 1].PositionRhw.X = 50;
                    else if (dBLevel % 5 == 0)
                        ScaleVertexes[pos + 1].PositionRhw.X = 20;
                    else
                        ScaleVertexes[pos + 1].PositionRhw.X = 10;

                    ScaleVertexes[pos + 1].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel * 10);
                    ScaleVertexes[pos + 1].PositionRhw.Z = 0.5f;
                    ScaleVertexes[pos + 1].PositionRhw.W = 1;
                    ScaleVertexes[pos + 1].Color = color & 0x00FFFFFF;

                    ScaleVertexes[pos + 2].PositionRhw.X = DirectXWidth;
                    ScaleVertexes[pos + 2].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel * 10);
                    ScaleVertexes[pos + 2].PositionRhw.Z = 0.5f;
                    ScaleVertexes[pos + 2].PositionRhw.W = 1;
                    ScaleVertexes[pos + 2].Color = color;

                    if (dBLevel % 10 == 0)
                        ScaleVertexes[pos + 3].PositionRhw.X = DirectXWidth - 50;
                    else if (dBLevel % 5 == 0)
                        ScaleVertexes[pos + 3].PositionRhw.X = DirectXWidth - 20;
                    else
                        ScaleVertexes[pos + 3].PositionRhw.X = DirectXWidth - 10;
                    
                    ScaleVertexes[pos + 3].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel * 10);
                    ScaleVertexes[pos + 3].PositionRhw.Z = 0.5f;
                    ScaleVertexes[pos + 3].PositionRhw.W = 1;
                    ScaleVertexes[pos + 3].Color = color & 0x00FFFFFF; ;
                }
            }

            /* draw scale */
            Device.DrawUserPrimitives(PrimitiveType.LineList, 16 * 2, ScaleVertexes);
            SmallFont.DrawString(null, "0 dB", 10, (int)-sampleToDBScale(0), (int)(color & 0x80FFFFFF));
            SmallFont.DrawString(null, "-50 dB", 10, (int)-sampleToDBScale(-50), (int)(color & 0x80FFFFFF));
            SmallFont.DrawString(null, "-100 dB", 10, (int)-sampleToDBScale(-100), (int)(color & 0x80FFFFFF));
            SmallFont.DrawString(null, "-150 dB", 10, (int)-sampleToDBScale(-150), (int)(color & 0x80FFFFFF));

            /* draw vertical cursor line */
            float stubLength = (float)DirectXHeight / 10.0f;
            float xPos = (float)LastMousePos.X;
            float yPos = (float)LastMousePos.Y;
            float dB = (float)sampleFromDBScale(-yPos);

            if (xPos > DirectXWidth / 2)
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
                CursorVertexesVert[0].Color = color & 0x00FFFFFF;

                CursorVertexesVert[1].PositionRhw.Y = 20 + stubLength;
                CursorVertexesVert[1].PositionRhw.Z = 0.5f;
                CursorVertexesVert[1].PositionRhw.W = 1;
                CursorVertexesVert[1].Color = color;

                CursorVertexesVert[2].PositionRhw.Y = DirectXHeight - stubLength;
                CursorVertexesVert[2].PositionRhw.Z = 0.5f;
                CursorVertexesVert[2].PositionRhw.W = 1;
                CursorVertexesVert[2].Color = color;

                CursorVertexesVert[3].PositionRhw.Y = DirectXHeight;
                CursorVertexesVert[3].PositionRhw.Z = 0.5f;
                CursorVertexesVert[3].PositionRhw.W = 1;
                CursorVertexesVert[3].Color = color & 0x00FFFFFF;
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
                CursorVertexesHor[0].Color = color & 0x00FFFFFF;

                CursorVertexesHor[1].PositionRhw.Z = 0.5f;
                CursorVertexesHor[1].PositionRhw.W = 1;
                CursorVertexesHor[1].Color = color;

                CursorVertexesHor[2].PositionRhw.Z = 0.5f;
                CursorVertexesHor[2].PositionRhw.W = 1;
                CursorVertexesHor[2].Color = color & 0x00FFFFFF;
            }

            Device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, CursorVertexesHor);

            /* draw the horizontal position (preliminary) */
            string label;
            if (OverviewMode)
                label = XLabelFromSampleNum(xPos);
            else
                label = XLabelFromCursorPos(xPos);

            if (xPos > DirectXWidth / 2)
                SmallFont.DrawString(null, label, (int)xPos - 40, 20, ColorFG.ToArgb());
            else
                SmallFont.DrawString(null, label, (int)xPos + 20, 20, ColorFG.ToArgb());

            /* and the strength at the current position */
            SmallFont.DrawString(null, dB + " dB", (int)xPos + 20, (int)yPos, ColorFG.ToArgb());
        }

        protected override string XLabelFromCursorPos(double xPos)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = ((DisplayXOffset + xPos) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;
            double frequency = offset * SamplingRate;

            return FrequencyFormatter.FreqToString(frequency);
        }

        public double FrequencyFromCursorPos()
        {
            /* offset (-0.5 ... 0.5) */
            double offset = ((DisplayXOffset + LastMousePos.X) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;
            double frequency = offset * SamplingRate;

            return frequency;
        }

        protected override string XLabelFromSampleNum(double pos)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = pos / DirectXWidth - 0.5f;
            double frequency = offset * SamplingRate;

            return FrequencyFormatter.FreqToString(frequency);
        }

        internal override void PrepareLinePoints()
        {
            lock (SampleValues)
            {
                if (SampleValuesAveraged > 0)
                {
                    double[] sampleArray = SampleValues;
                    {
                        int samples = sampleArray.Length;

                        lock (LinePointsLock)
                        {
                            if (LinePoints == null || LinePoints.Length < samples)
                                LinePoints = new Point[samples];

                            for (int pos = 0; pos < samples; pos++)
                            {
                                double posX = pos;
                                double posY = (double)sampleArray[pos];

                                LinePoints[pos].X = posX;

                                /* some simple averaging */
                                unchecked
                                {
                                    LinePoints[pos].Y *= (Averaging - 1);
                                    LinePoints[pos].Y += posY / SampleValuesAveraged;
                                    LinePoints[pos].Y /= Averaging;
                                }

                                if (double.IsNaN(LinePoints[pos].Y))
                                    LinePoints[pos].Y = 0;
                            }
                            LinePointEntries = samples;
                            LinePointsUpdated = true;
                        }
                    }
                    SampleValuesAveraged = 0;
                }
            }
        }

        private void DisplayFunc()
        {
            DateTime lastUpdate = DateTime.Now;
            DateTime curTime = DateTime.Now;

            while (true)
            {
                curTime = DateTime.Now;

                if (NeedsUpdate && curTime.Subtract(lastUpdate).TotalMilliseconds > RenderSleepDelay)
                {
                    lastUpdate = curTime;
                    NeedsUpdate = false;

                    if (SlavePlot != null)
                        SlavePlot.PrepareLinePoints();
                    PrepareLinePoints();
                }
                else
                {
                    // to have approx. 60 FPS
                    Thread.Sleep(1000 / 60);
                }


                if (SlavePlot != null)
                    SlavePlot.Render();
                Render();
            }
        }


    }
}