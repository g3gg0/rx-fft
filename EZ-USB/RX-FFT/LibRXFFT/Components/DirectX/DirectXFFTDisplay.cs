using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using Timer = LibRXFFT.Libraries.Timers.AccurateTimer;
using Font = SlimDX.Direct3D9.Font;
using SlimDX.Direct3D9;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries.GSM.Misc;
using LibRXFFT.Libraries.Misc;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXFFTDisplay : DirectXPlot
    {

        /* DirectX related graphic stuff */
        protected Vertex[] CursorVertexesVert = new Vertex[4];
        protected Vertex[] CursorVertexesHor = new Vertex[3];
        protected Vertex[] ScaleVertexes = new Vertex[100];
        protected int ScaleVertexesUsed = 0;

        protected Timer ScreenRefreshTimer;
        protected Timer LinePointUpdateTimer;
        protected Thread DisplayThread;
        protected bool NeedsUpdate = false;
        public bool EnoughData = false;

        /* sample value buffer */
        protected double[] SampleValues = new double[0];
        protected long SampleValuesAveraged = 0;
        public long SamplesToAverage = 2;

        /* processing related */
        protected Mutex FFTLock = new Mutex();
        protected FFTTransformer FFT;

        protected int _FFTSize = 256;
        public double FFTPrescaler = 1.0f;
        public double FFTOffset = 0.0f;
        protected double fftPrescalerDefault = 1.0f;
        protected double fftOffsetDefault = 0.0f;

        /* if the fft data provided is already squared, set to true */
        public bool SquaredFFTData = false;

        private double MaxPower = -99999.0f;

        /* the averaging value to smooth the displayed lines */
        public double Averaging = 1;

        public double _SamplingRate = 100;
        public double SamplingRate
        {
            get { return _SamplingRate; }
            set
            {
                _SamplingRate = value;
                UpdateAxis = true;
                UpdateOverlays = true;
            }
        }
        public double _CenterFrequency = 0;
        public double CenterFrequency
        {
            get { return _CenterFrequency; }
            set 
            { 
                _CenterFrequency = value;
                UpdateAxis = true;
                UpdateOverlays = true;
            }
        }

        public double UpdateRate
        {
            get { return 1000 / RenderSleepDelay; }
            set 
            { 
                RenderSleepDelay = 1000 / value;
                if (!double.IsNaN(RenderSleepDelay) && !double.IsInfinity(RenderSleepDelay) && LinePointUpdateTimer != null)
                {
                    LinePointUpdateTimer.Interval = (uint)RenderSleepDelay;
                    ScreenRefreshTimer.Interval = (uint)((value < 60) ? (1000 / 60) : RenderSleepDelay);
                }
            }
        }


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

            EventActions[eUserEvent.MouseDragX] = eUserAction.XOffset;
            EventActions[eUserEvent.MouseWheelUp] = eUserAction.YZoomIn;
            EventActions[eUserEvent.MouseWheelDown] = eUserAction.YZoomOut;
            EventActions[eUserEvent.MouseWheelUpShift] = eUserAction.XZoomIn;
            EventActions[eUserEvent.MouseWheelDownShift] = eUserAction.XZoomOut;

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
                ScreenRefreshTimer = new Timer();
                ScreenRefreshTimer.Interval = 1000 / 60;
                ScreenRefreshTimer.Timer += new EventHandler(ScreenRefreshTimer_Func);
                ScreenRefreshTimer.Start();

                LinePointUpdateTimer = new Timer();
                LinePointUpdateTimer.Interval = (uint) RenderSleepDelay;
                LinePointUpdateTimer.Timer += new EventHandler(LinePointUpdateTimer_Func);
                LinePointUpdateTimer.Start();
            }
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


        protected Cursor CreateEmptyCursor()
        {
            Bitmap b = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(b);
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
                        EnoughData = false;
                        FFT = new FFTTransformer(value);
                    }
                }
            }
        }

        public void ProcessFFTData(double[] amplitudes)
        {
            if (EnoughData)
                return;

            lock (SampleValues)
            {
                if (SampleValues.Length != amplitudes.Length)
                    SampleValues = new double[amplitudes.Length];

                if (SamplesToAverage == 0)
                {
                    /* no preference made, just average all samples we get */
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
                    NeedsUpdate = true;
                }
                else
                {
                    /* dont average more as requested */
                    if (SampleValuesAveraged < SamplesToAverage)
                    {
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
                    }
                    // to reduce CPU load
                    if (SampleValuesAveraged >= SamplesToAverage)
                    {
                        EnoughData = true;
                        NeedsUpdate = true;
                    }
                }
            }
        }

        public void ProcessRawData(byte[] dataBuffer)
        {
            const int bytePerSample = 2;
            const int channels = 2;

            if (EnoughData)
                return;

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

        protected double sampleToDBScale(double sampleValue)
        {
            return FFTPrescaler * sampleValue + FFTOffset;
        }

        protected double sampleFromDBScale(double scaleValue)
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
                    UpdateAxis = true;
                    break;

                case eUserAction.YZoomOut:
                    FFTOffset = ((LastMousePos.Y + FFTOffset) / YZoomStep) - LastMousePos.Y;
                    FFTPrescaler /= YZoomStep;

                    LinePointsUpdated = true;
                    UpdateAxis = true;
                    break;

                case eUserAction.YOffset:
                    if (Math.Abs(param) < 5)
                    {
                        FFTOffset += param;
                        UpdateAxis = true;
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
            uint colorCursor = (uint)ColorCursor.ToArgb();

            /* only recalc scale lines when axis need to get updated */
            if (UpdateOverlays)
            {
                TextLabels.Clear();
                ScaleVertexesUsed = 0;

                for (int dBLevel = 0; dBLevel <= 150; dBLevel+=10)
                {
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 0;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel);
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                    ScaleVertexes[ScaleVertexesUsed].Color = colorCursor;
                    ScaleVertexesUsed++;

                    if (dBLevel % 100 == 0)
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 50;
                    else if (dBLevel % 50 == 0)
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 20;
                    else
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 10;

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel);
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                    ScaleVertexes[ScaleVertexesUsed].Color = colorCursor & 0x00FFFFFF;
                    ScaleVertexesUsed++;

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = DirectXWidth;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel);
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                    ScaleVertexes[ScaleVertexesUsed].Color = colorCursor;
                    ScaleVertexesUsed++;

                    if (dBLevel % 100 == 0)
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = DirectXWidth - 50;
                    else if (dBLevel % 50 == 0)
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = DirectXWidth - 20;
                    else
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = DirectXWidth - 10;

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (float)-sampleToDBScale(-dBLevel);
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                    ScaleVertexes[ScaleVertexesUsed].Color = colorCursor & 0x00FFFFFF;
                    ScaleVertexesUsed++;
                }

                foreach (LabelledLine line in LabelledHorLines)
                {
                    if (ScaleVertexesUsed < ScaleVertexes.Length - 2)
                    {
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 0;
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (float)-sampleToDBScale(line.Position);
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                        ScaleVertexes[ScaleVertexesUsed].Color = line.Color;
                        ScaleVertexesUsed++;

                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = DirectXWidth;
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (float)-sampleToDBScale(line.Position);
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                        ScaleVertexes[ScaleVertexesUsed].Color = line.Color;
                        ScaleVertexesUsed++;
                    }
                }
                
                foreach (LabelledLine line in LabelledVertLines)
                {
                    if (ScaleVertexesUsed < ScaleVertexes.Length - 4)
                    {
                        double freq = XPosFromFrequency(line.Position);

                        if (freq >= 0 && freq < DirectXWidth)
                        {
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 10 + (float)XPosFromFrequency(line.Position);
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = 0;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                            ScaleVertexes[ScaleVertexesUsed].Color = line.Color;
                            ScaleVertexesUsed++;

                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = (float)XPosFromFrequency(line.Position);
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = 10;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                            ScaleVertexes[ScaleVertexesUsed].Color = line.Color;
                            ScaleVertexesUsed++;

                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = (float)XPosFromFrequency(line.Position);
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = 10;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                            ScaleVertexes[ScaleVertexesUsed].Color = line.Color;
                            ScaleVertexesUsed++;

                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = (float)XPosFromFrequency(line.Position);
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = DirectXHeight;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                            ScaleVertexes[ScaleVertexesUsed].Color = line.Color;
                            ScaleVertexesUsed++;

                            /* add the labels to the label list to draw */
                            TextLabels.AddLast(new StringLabel(line.Label, (int)XPosFromFrequency(line.Position) + 5, 15, line.Color));
                        }
                    }
                }
            }

            /* draw scale */
            Device.DrawUserPrimitives(PrimitiveType.LineList, ScaleVertexesUsed / 2, ScaleVertexes);
            SmallFont.DrawString(null, "   0 dB", 10, (int)-sampleToDBScale(0), (int)(colorCursor & 0x80FFFFFF));
            SmallFont.DrawString(null, " -50 dB", 10, (int)-sampleToDBScale(-50), (int)(colorCursor & 0x80FFFFFF));
            SmallFont.DrawString(null, "-100 dB", 10, (int)-sampleToDBScale(-100), (int)(colorCursor & 0x80FFFFFF));
            SmallFont.DrawString(null, "-150 dB", 10, (int)-sampleToDBScale(-150), (int)(colorCursor & 0x80FFFFFF));

            /* draw strings for labels and such things */
            foreach (StringLabel label in TextLabels)
            {
                SmallFont.DrawString(null, label.Label, label.X, label.Y, (int)label.Color);
            }

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

            /* horizontal line */
            CursorVertexesHor[0].PositionRhw.X = xPos - 30;
            CursorVertexesHor[0].PositionRhw.Y = yPos;

            CursorVertexesHor[1].PositionRhw.X = xPos;
            CursorVertexesHor[1].PositionRhw.Y = yPos;

            CursorVertexesHor[2].PositionRhw.X = xPos + 30;
            CursorVertexesHor[2].PositionRhw.Y = yPos;

            /* if axis changed, recalc lines */
            if (UpdateOverlays)
            {
                CursorVertexesVert[0].PositionRhw.Y = 20;
                CursorVertexesVert[0].PositionRhw.Z = 0.5f;
                CursorVertexesVert[0].PositionRhw.W = 1;
                CursorVertexesVert[0].Color = colorCursor & 0x00FFFFFF;

                CursorVertexesVert[1].PositionRhw.Y = 20 + stubLength;
                CursorVertexesVert[1].PositionRhw.Z = 0.5f;
                CursorVertexesVert[1].PositionRhw.W = 1;
                CursorVertexesVert[1].Color = colorCursor;

                CursorVertexesVert[2].PositionRhw.Y = DirectXHeight - stubLength;
                CursorVertexesVert[2].PositionRhw.Z = 0.5f;
                CursorVertexesVert[2].PositionRhw.W = 1;
                CursorVertexesVert[2].Color = colorCursor;

                CursorVertexesVert[3].PositionRhw.Y = DirectXHeight;
                CursorVertexesVert[3].PositionRhw.Z = 0.5f;
                CursorVertexesVert[3].PositionRhw.W = 1;
                CursorVertexesVert[3].Color = colorCursor;

                CursorVertexesHor[0].PositionRhw.Z = 0.5f;
                CursorVertexesHor[0].PositionRhw.W = 1;
                CursorVertexesHor[0].Color = colorCursor & 0x00FFFFFF;

                CursorVertexesHor[1].PositionRhw.Z = 0.5f;
                CursorVertexesHor[1].PositionRhw.W = 1;
                CursorVertexesHor[1].Color = colorCursor;

                CursorVertexesHor[2].PositionRhw.Z = 0.5f;
                CursorVertexesHor[2].PositionRhw.W = 1;
                CursorVertexesHor[2].Color = colorCursor & 0x00FFFFFF;
            }

            if (MouseHovering || ShowVerticalCursor /*&& !ShiftPressed*/)
            {
                Device.DrawUserPrimitives(PrimitiveType.LineStrip, 3, CursorVertexesVert);
                if (MouseHovering)
                {
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, CursorVertexesHor);

                    /* draw the strength at the current position */
                    SmallFont.DrawString(null, dB + " dB", (int)xPos + 20, (int)yPos, ColorFG.ToArgb());
                }

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

            }
        }

        public double XRelativeCoordFromCursorPos(double xPos)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = ((DisplayXOffset + xPos) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;

            return offset;
        }

        public double XPosFromFrequency(double frequency)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = (frequency - CenterFrequency) / SamplingRate;

            if(OverviewMode)
                return ((offset + 0.5f + XAxisSampleOffset) * (1 * DirectXWidth)) - 0;
            else
                return ((offset + 0.5f + XAxisSampleOffset) * (XZoomFactor * DirectXWidth)) - DisplayXOffset;
        }

        public double XRelativeCoordFromCursorPos()
        {
            return XRelativeCoordFromCursorPos(LastMousePos.X);
        }

        protected override string XLabelFromCursorPos(double xPos)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = ((DisplayXOffset + xPos) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;
            double frequency = CenterFrequency + offset * SamplingRate;

            return FrequencyFormatter.FreqToString(frequency);
        }

        public long FrequencyFromCursorPos()
        {
            /* offset (-0.5 ... 0.5) */
            double offset = ((DisplayXOffset + LastMousePos.X) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;
            long frequency = (long)(CenterFrequency + offset * SamplingRate);

            return frequency;
        }

        public long FrequencyFromCursorPosOffset(double xOffset)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = ((DisplayXOffset + LastMousePos.X + xOffset) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;
            long frequency = (long)(CenterFrequency + offset * SamplingRate);

            return frequency;
        }


        protected override string XLabelFromSampleNum(double pos)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = pos / DirectXWidth - 0.5f;
            double frequency = CenterFrequency + offset * SamplingRate;

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
                    EnoughData = false;
                }
            }
        }

        private void LinePointUpdateTimer_Func(object sender, EventArgs e)
        {
            if (NeedsUpdate)
            {
                NeedsUpdate = false;
                if (SlavePlot != null)
                    SlavePlot.PrepareLinePoints();
                PrepareLinePoints();
            }
        }

        private void ScreenRefreshTimer_Func(object sender, EventArgs e)
        {
            if (SlavePlot != null)
                SlavePlot.Render();
            Render();
        }

    }
}