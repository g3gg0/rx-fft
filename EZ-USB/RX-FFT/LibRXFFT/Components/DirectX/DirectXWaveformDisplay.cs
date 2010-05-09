using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.Misc;
using SlimDX.Direct3D9;
using Timer = LibRXFFT.Libraries.Timers.AccurateTimer;
using LibRXFFT.Components.DirectX.Drawables;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXWaveformDisplay : DirectXPlot
    {
        /* DirectX related graphic stuff */
        protected Vertex[] CursorVertexesVert = new Vertex[4];
        protected Vertex[] CursorVertexesHor = new Vertex[3];
        protected Vertex[] ScaleVertexes = new Vertex[100];
        protected Vertex[] OverlayVertexes = new Vertex[100];


        public bool RealTimeMode = false;

        /* the cursor rectangle that contains signal information */
        protected Vertex[] CursorRectVertexes = new Vertex[4];
        protected Vertex[] CursorRectBorderVertexes = new Vertex[4];
        public Color CursorBoxColor = Color.Blue;

        public double ScaleUnitFactor = 1;
        public string ScaleUnit = "dB";

        protected Vertex[] LimiterVertexesLeft = new Vertex[4];
        protected Vertex[] LimiterVertexesRight = new Vertex[4];
        protected Vertex[] LimiterLines = new Vertex[4];

        public double LimiterLowerLimit = 0;
        public double LimiterUpperLimit = 0;
        public bool LimiterDisplayEnabled = false;
        public Color LimiterColor = Color.Green;
        public string LimiterUpperDescription = "";
        public string LimiterLowerDescription = "";

        protected int ScaleVertexesUsed = 0;
        protected int OverlayVertexesUsed = 0;

        protected Timer ScreenRefreshTimer;
        protected Timer LinePointUpdateTimer;
        protected Thread DisplayThread;
        public bool NeedsUpdate = false;
        public bool EnoughData = false;

        public double ScalePosMax = 0;
        public double ScalePosMin = -120;
        public double ScaleBarDistance = 10;
        public double ScaleTextDistance = 20;

        /* sample value buffer */
        public int MaxSamples { get; set; }
        readonly ArrayList SampleValues = new ArrayList();

        /* processing related */

        private double _SamplingRate = 0;
        public double SamplingRate
        {
            get { return _SamplingRate; }
            set
            {
                _SamplingRate = value;
                UpdateAxis = true;
                UpdateCursor = true;
                UpdateOverlays = true;
            }
        }
        private double _CenterFrequency = 0;
        public double CenterFrequency
        {
            get { return _CenterFrequency; }
            set
            {
                _CenterFrequency = value;
                UpdateAxis = true;
                UpdateCursor = true;
                UpdateOverlays = true;
            }
        }

        private int _SpectParts = 1;
        public int SpectParts
        {
            get { return _SpectParts; }
            set
            {
                _SpectParts = value;
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
                    ScreenRefreshTimer.Interval = (uint)(1000/MinRefreshRate);// ((value < MinRefreshRate) ? (1000 / MinRefreshRate) : RenderSleepDelay);
                }
            }
        }


        public DirectXWaveformDisplay()
            : this(false)
        {
        }

        public DirectXWaveformDisplay(bool slaveMode) : base(slaveMode)
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

            if (!slaveMode)
            {
                ScreenRefreshTimer = new Timer();
                ScreenRefreshTimer.Interval = (uint)(1000 / DefaultRefreshRate);
                ScreenRefreshTimer.Timer += new EventHandler(ScreenRefreshTimer_Func);
                ScreenRefreshTimer.Start();

                LinePointUpdateTimer = new Timer();
                LinePointUpdateTimer.Interval = (uint)RenderSleepDelay;
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
                uint colorFG = ((uint)ColorFG.ToArgb()) & 0xFFFFFF;

                if (numPoints > 0)
                {
                    if (PlotVerts == null || numPoints > PlotVerts.Length)
                    {
                        PlotVerts = new Vertex[numPoints];
                    }
                    if (numPoints > PlotVertsOverview.Length)
                    {
                        PlotVertsOverview = new Vertex[numPoints];
                    }

                    PlotVertsEntries = numPoints - 1;

                    for (int pos = 0; pos < numPoints; pos++)
                    {
                        double yVal = points[pos].Y;
                        double xPos = ((double)points[pos].X / (double)PlotVertsEntries) * DirectXWidth;

                        PlotVerts[pos].PositionRhw.X = (float)Math.Min(DirectXWidth, Math.Max(0, ((XAxisSampleOffset + xPos) * XZoomFactor - DisplayXOffset)));
                        PlotVerts[pos].PositionRhw.Y = (float)Math.Min(DirectXHeight, Math.Max(0, yVal));
                        PlotVerts[pos].PositionRhw.Z = 0.5f;
                        PlotVerts[pos].PositionRhw.W = 1;
                        PlotVerts[pos].Color = 0x9F000000 | colorFG;

                        if (OverviewModeEnabled)
                        {
                            PlotVertsOverview[pos].PositionRhw.X = (float)Math.Min(DirectXWidth, Math.Max(0, (XAxisSampleOffset + xPos)));
                            PlotVertsOverview[pos].PositionRhw.Y = PlotVerts[pos].PositionRhw.Y;
                            PlotVertsOverview[pos].PositionRhw.Z = PlotVerts[pos].PositionRhw.Z;
                            PlotVertsOverview[pos].PositionRhw.W = PlotVerts[pos].PositionRhw.W;
                            PlotVertsOverview[pos].Color = PlotVerts[pos].Color;
                        }
                    }

                    try
                    {
                        foreach (DirectXDrawable drawable in Drawables)
                        {
                            if (drawable is PlotVertsSink)
                            {
                                ((PlotVertsSink)drawable).ProcessPlotVerts(PlotVerts, PlotVertsEntries);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            catch (Exception e)
            {
                return;
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

        public void Clear()
        {
            lock (SampleValues)
            {
                SampleValues.Clear();
                NeedsUpdate = true;
            }
        }

        public void ClearProcessData(double[] samples)
        {
            lock (SampleValues)
            {
                SampleValues.Clear();
                for (int pos = 0; pos < samples.Length; pos++)
                    SampleValues.Add(samples[pos]);

                NeedsUpdate = true;
            }
        }

        public void ProcessData(double level, bool forcePlot)
        {
            try
            {
                lock (SampleValues)
                {
                    SampleValues.Add(level);

                    if (RealTimeMode && SampleValues.Count >= MaxSamples)
                    {
                        if (SlavePlot != null)
                            SlavePlot.PrepareLinePoints();
                        PrepareLinePoints();
                        SampleValues.Clear();
                        lock (LinePointsLock)
                        {
                            CreateVertexBufferForPoints(LinePoints, LinePointEntries);
                        }
                    }
                    else
                    {
                        NeedsUpdate = true;
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        public void ProcessData(double power)
        {
            if (SampleValues.Count > 2 * MaxSamples)
                return;

            ProcessData(power, false);
        }

        public void ProcessData(double[] samples)
        {
            if (SampleValues.Count > 2 * MaxSamples)
                return;

            try
            {
                lock (SampleValues)
                {
                    for (int pos = 0; pos < samples.Length; pos++)
                    {
                        SampleValues.Add(samples[pos]);

                        if (RealTimeMode && SampleValues.Count >= MaxSamples)
                        {
                            if (SlavePlot != null)
                                SlavePlot.PrepareLinePoints();
                            PrepareLinePoints();
                            SampleValues.Clear();
                            lock (LinePointsLock)
                            {
                                CreateVertexBufferForPoints(LinePoints, LinePointEntries);
                            }
                        }
                        else
                        {
                            NeedsUpdate = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        public void ProcessData(byte[] dataBuffer, int channels, int channel)
        {
            if (SampleValues.Count > 2 * MaxSamples)
                return;

            if (channels == 0 || channel == 0 || channel > channels)
                return;

            lock (SampleValues)
            {
                int bytePerSample = channels * 2;
                int byteOffset = (channel - 1) * 2;

                for (int pos = 0; pos < dataBuffer.Length / bytePerSample; pos++)
                {
                    SampleValues.Add(ByteUtil.getDoubleFromBytes(dataBuffer, byteOffset + bytePerSample * pos));

                    if (RealTimeMode && SampleValues.Count >= MaxSamples)
                    {
                        if (SlavePlot != null)
                            SlavePlot.PrepareLinePoints();
                        PrepareLinePoints();
                        SampleValues.Clear();
                        lock (LinePointsLock)
                        {
                            CreateVertexBufferForPoints(LinePoints, LinePointEntries);
                        }
                    }
                    else
                    {
                        NeedsUpdate = true;
                    }
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Focus();
            base.OnMouseEnter(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            XMaximum = DirectXWidth;
        }

        protected double SampleToYPos(double sampleValue)
        {
            return -(sampleValue * DirectXHeight * YZoomFactor) - DisplayYOffset;
        }

        protected double SampleFromYPos(double scaleValue)
        {
            return -(scaleValue + DisplayYOffset) / (DirectXHeight * YZoomFactor);
        }

        public override void ProcessUserAction(eUserAction action, double param)
        {
            NeedsUpdate = true;
            double oldOffset;

            switch (action)
            {

                case eUserAction.YOffset:
                    oldOffset = DisplayYOffset;

                    param += oldOffset;
                    param = Math.Min(10 * DirectXHeight, param);
                    DisplayYOffset = Math.Max(-10 * DirectXHeight, param);

                    if (oldOffset != DisplayYOffset)
                    {
                        UpdateAxis = true;
                        UpdateCursor = true;
                        UpdateOverlays = true;
                    }

                    break;

                case eUserAction.YZoomIn:
                    if (YZoomFactor < YZoomFactorMax)
                    {
                        DisplayYOffset = (LastMousePos.Y + DisplayYOffset) * YZoomStep - LastMousePos.Y;
                        YZoomFactor *= YZoomStep;

                        /* call ourselves again for min/max fitting */
                        ProcessUserAction(eUserAction.YOffset, 0);

                        UpdateAxis = true;
                        UpdateCursor = true;
                        UpdateOverlays = true;
                    }

                    break;

                case eUserAction.YZoomOut:
                    if (YZoomFactor > YZoomFactorMin)
                    {
                        DisplayYOffset = (LastMousePos.Y + DisplayYOffset) / YZoomStep - LastMousePos.Y;
                        YZoomFactor /= YZoomStep;

                        /* call ourselves again for min/max fitting */
                        ProcessUserAction(eUserAction.YOffset, 0);

                        UpdateAxis = true;
                        UpdateCursor = true;
                        UpdateOverlays = true;
                    }

                    break;

                default:
                    base.ProcessUserAction(action, param);
                    break;
            }
        }

        protected override void RenderCursor()
        {
            uint colorCursor = (uint)ColorCursor.ToArgb();
            uint colorCursorBox = (uint)CursorBoxColor.ToArgb();

            float stubLength = (float)DirectXHeight / 10.0f;
            float xPos = (float)LastMousePos.X;
            float yPos = (float)LastMousePos.Y;
            float unit = (float)(SampleFromYPos(yPos) * ScaleUnitFactor);

            if (UpdateCursor)
            {
                UpdateCursor = false;

                /* draw vertical cursor line */
                if (xPos > DirectXWidth / 2)
                    CursorVertexesVert[0].PositionRhw.X = xPos - 30;
                else
                    CursorVertexesVert[0].PositionRhw.X = xPos + 30;
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


                /* recalc lines (this is needed just once btw.) */
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

            if (MouseHovering || ShowVerticalCursor)
            {
                string signalPower = unit + " " + ScaleUnit;
                string positionLabel;
                if (OverviewMode)
                    positionLabel = XLabelFromSampleNum(xPos);
                else
                    positionLabel = XLabelFromCursorPos(xPos);

                int signalPowerWidth = SmallFont.MeasureString(null, signalPower, DrawTextFormat.Center).Width;
                int positionLabelWidth = SmallFont.MeasureString(null, positionLabel, DrawTextFormat.Center).Width;
                int signalPowerHeight = SmallFont.MeasureString(null, signalPower, DrawTextFormat.Center).Height;
                int positionLabelHeight = SmallFont.MeasureString(null, positionLabel, DrawTextFormat.Center).Height;

                int boxWidth = 10 + Math.Max(signalPowerWidth, positionLabelWidth);
                int boxHeight = 10 + signalPowerHeight + positionLabelHeight;
                int boxY = 20;
                int boxX = (int)xPos;

                if (xPos > DirectXWidth / 2)
                {
                    boxX -= boxWidth + 25;
                }
                else
                {
                    boxX += 25;
                }

                BuildStripRectangle(CursorRectVertexes, colorCursorBox & 0x3FFFFFFF, boxX, boxX + boxWidth, boxY + boxHeight, boxY);

                Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, CursorRectVertexes);
                Device.DrawUserPrimitives(PrimitiveType.LineStrip, 3, CursorVertexesVert);

                if (MouseHovering)
                {
                    /* draw the strength into box */
                    SmallFont.DrawString(null, signalPower, boxX + 5, boxY + 5, ColorFG.ToArgb());
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, CursorVertexesHor);
                }

                /* draw the horizontal into box */
                SmallFont.DrawString(null, positionLabel, boxX + 5, boxY + 5 + signalPowerHeight, ColorFG.ToArgb());
            }
        }

        protected override void RenderAxis()
        {
            uint colorCursor = (uint)ColorCursor.ToArgb();
            uint colorLimiter = (uint)LimiterColor.ToArgb();
            int xPos = (int)LastMousePos.X;
            int yPos = (int)LastMousePos.Y;

            float limiterLower = (float)XPosFromSampleNum(LimiterLowerLimit);
            float limiterUpper = (float)XPosFromSampleNum(LimiterUpperLimit);
            bool mouseInLowerLimit = (xPos <= limiterLower);
            bool mouseInUpperLimit = (xPos >= limiterUpper);

            /* only recalc scale lines when axis need to get updated */
            if (UpdateAxis)
            {
                UpdateAxis = false;
                ScaleVertexesUsed = 0;

                /* update bandwith limiters */
                LimiterLines[0].PositionRhw.X = limiterLower;
                LimiterLines[0].PositionRhw.Y = 0;
                LimiterLines[0].PositionRhw.Z = 0.5f;
                LimiterLines[0].PositionRhw.W = 1;
                LimiterLines[0].Color = colorLimiter;
                LimiterLines[1].PositionRhw.X = limiterLower;
                LimiterLines[1].PositionRhw.Y = DirectXHeight;
                LimiterLines[1].PositionRhw.Z = 0.5f;
                LimiterLines[1].PositionRhw.W = 1;
                LimiterLines[1].Color = colorLimiter;
                LimiterLines[2].PositionRhw.X = limiterUpper;
                LimiterLines[2].PositionRhw.Y = 0;
                LimiterLines[2].PositionRhw.Z = 0.5f;
                LimiterLines[2].PositionRhw.W = 1;
                LimiterLines[2].Color = colorLimiter;
                LimiterLines[3].PositionRhw.X = limiterUpper;
                LimiterLines[3].PositionRhw.Y = DirectXHeight;
                LimiterLines[3].PositionRhw.Z = 0.5f;
                LimiterLines[3].PositionRhw.W = 1;
                LimiterLines[3].Color = colorLimiter;

                BuildStripRectangle(LimiterVertexesLeft, colorLimiter & 0x2FFFFFFF, 0, limiterLower);
                BuildStripRectangle(LimiterVertexesRight, colorLimiter & 0x2FFFFFFF, limiterUpper, DirectXWidth);

                int scaleCount = (int)Math.Round((ScalePosMax - ScalePosMin) / ScaleBarDistance);

                /* draw scale lines */
                for (int scaleNum = 0; scaleNum <= scaleCount; scaleNum++)
                {
                    float scalePos = (float)(ScalePosMin + scaleNum * ScaleBarDistance);

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 0;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (float)SampleToYPos(scalePos);
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                    ScaleVertexes[ScaleVertexesUsed].Color = colorCursor;
                    ScaleVertexesUsed++;

                    if (scaleNum == 0 || scaleNum == scaleCount || Math.Round(scalePos / ScaleBarDistance) == 0)
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 50;
                    else if (scaleNum % 2 == 0)
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 20;
                    else
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 10;

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (float)SampleToYPos(scalePos);
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                    ScaleVertexes[ScaleVertexesUsed].Color = colorCursor & 0x00FFFFFF;
                    ScaleVertexesUsed++;

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = DirectXWidth;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (float)SampleToYPos(scalePos);
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                    ScaleVertexes[ScaleVertexesUsed].Color = colorCursor;
                    ScaleVertexesUsed++;

                    if (scalePos % 100 == 0)
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = DirectXWidth - 50;
                    else if (scalePos % 50 == 0)
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = DirectXWidth - 20;
                    else
                        ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = DirectXWidth - 10;

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (float)SampleToYPos(scalePos);
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                    ScaleVertexes[ScaleVertexesUsed].Color = colorCursor & 0x00FFFFFF;
                    ScaleVertexesUsed++;

                }
            }

            if (LimiterDisplayEnabled)
            {
                Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, LimiterVertexesLeft);
                Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, LimiterVertexesRight);
                Device.DrawUserPrimitives(PrimitiveType.LineList, 2, LimiterLines);

                if (mouseInLowerLimit)
                {
                    if (xPos > DirectXWidth / 2)
                    {
                        int stringWidth = SmallFont.MeasureString(null, LimiterLowerDescription, DrawTextFormat.Center).Width;
                        SmallFont.DrawString(null, LimiterLowerDescription, xPos - 30 - stringWidth, yPos + 20, (int)(colorLimiter));
                    }
                    else
                    {
                        SmallFont.DrawString(null, LimiterLowerDescription, xPos + 20, yPos + 20, (int)(colorLimiter));
                    }
                }
                if (mouseInUpperLimit)
                {
                    if (xPos > DirectXWidth / 2)
                    {
                        int stringWidth = SmallFont.MeasureString(null, LimiterUpperDescription, DrawTextFormat.Center).Width;
                        SmallFont.DrawString(null, LimiterUpperDescription, xPos - 30 - stringWidth, yPos + 20, (int)(colorLimiter));
                    }
                    else
                    {
                        SmallFont.DrawString(null, LimiterUpperDescription, xPos + 20, yPos + 20, (int)(colorLimiter));
                    }
                }
            }

            if (XAxisVerts.Length > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineList, XAxisVerts.Length / 2, XAxisVerts);
            if (YAxisVerts.Length > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineList, YAxisVerts.Length / 2, YAxisVerts);
        }

        private void BuildStripRectangle(Vertex[] vertexBuffer, uint color, float startX, float endX)
        {
            BuildStripRectangle(vertexBuffer, color, startX, endX, DirectXHeight, 0);
        }

        private void BuildStripRectangle(Vertex[] vertexBuffer, uint color, float startX, float endX, float startY, float endY)
        {
            vertexBuffer[0].PositionRhw.X = startX;
            vertexBuffer[0].PositionRhw.Y = startY;
            vertexBuffer[0].PositionRhw.Z = 0.5f;
            vertexBuffer[0].PositionRhw.W = 1;
            vertexBuffer[0].Color = color;
            vertexBuffer[1].PositionRhw.X = startX;
            vertexBuffer[1].PositionRhw.Y = endY;
            vertexBuffer[1].PositionRhw.Z = 0.5f;
            vertexBuffer[1].PositionRhw.W = 1;
            vertexBuffer[1].Color = color;
            vertexBuffer[2].PositionRhw.X = endX;
            vertexBuffer[2].PositionRhw.Y = startY;
            vertexBuffer[2].PositionRhw.Z = 0.5f;
            vertexBuffer[2].PositionRhw.W = 1;
            vertexBuffer[2].Color = color;
            vertexBuffer[3].PositionRhw.X = endX;
            vertexBuffer[3].PositionRhw.Y = endY;
            vertexBuffer[3].PositionRhw.Z = 0.5f;
            vertexBuffer[3].PositionRhw.W = 1;
            vertexBuffer[3].Color = color;
        }

        protected override void RenderOverlay()
        {
            uint colorCursor = (uint)ColorCursor.ToArgb();

            if (UpdateOverlays)
            {
                UpdateOverlays = false;
                UpdateDrawablePositions();
                OverlayTextLabels.Clear();
                OverlayVertexesUsed = 0;

                foreach (LabelledLine line in LabelledHorLines)
                {
                    if (OverlayVertexesUsed < OverlayVertexes.Length - 2)
                    {
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 0;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)SampleToYPos(line.Position);
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                        OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                        OverlayVertexesUsed++;

                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = DirectXWidth;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)SampleToYPos(line.Position);
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                        OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                        OverlayVertexesUsed++;

                        /* add the labels to the label list to draw */
                        OverlayTextLabels.AddLast(new StringLabel(line.Label, 60, (int)SampleToYPos(line.Position) + 5, line.Color));
                    }
                }

                foreach (LabelledLine line in LabelledVertLines)
                {
                    if (OverlayVertexesUsed < OverlayVertexes.Length - 4)
                    {
                        double freq = XPosFromSampleNum(line.Position);

                        if (freq >= 0 && freq < DirectXWidth)
                        {
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 10 + (float)XPosFromSampleNum(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = 0;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)XPosFromSampleNum(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = 10;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)XPosFromSampleNum(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = 10;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)XPosFromSampleNum(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = DirectXHeight;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            /* add the labels to the label list to draw */
                            OverlayTextLabels.AddLast(new StringLabel(line.Label, (int)XPosFromSampleNum(line.Position) + 5, 15, line.Color));
                        }
                    }
                }
            }

            /* draw overlay */
            if (OverlayVertexesUsed > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineList, OverlayVertexesUsed / 2, OverlayVertexes);
            foreach (StringLabel label in OverlayTextLabels)
            {
                SmallFont.DrawString(null, label.Label, label.X, label.Y, (int)label.Color);
            }

            /* draw scale lines and text */
            if (ScaleVertexesUsed > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineList, ScaleVertexesUsed / 2, ScaleVertexes);

            for (double scalePos = ScalePosMin; scalePos <= ScalePosMax; scalePos += ScaleTextDistance)
            {
                SmallFont.DrawString(null, " " + (int)(scalePos * ScaleUnitFactor) + " " + ScaleUnit, 10, (int)SampleToYPos(scalePos), (int)(colorCursor & 0x80FFFFFF));
            }
        }

        public double CursorTime
        {
            get
            {
                if (SamplingRate != 0)
                {
                    double offset = ((DisplayXOffset + LastMousePos.X) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;

                    int sampleNum = (int)((offset + 0.5) * LinePointEntries);
                    return (sampleNum / SamplingRate);
                }

                return 0;
            }
        }


        public double XPosFromTime(double time)
        {
            return XPosFromSampleNum(time * SamplingRate);
        }


        public double XPosFromSampleNum(double sampleNum)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = sampleNum / LinePointEntries - 0.5f;

            if (OverviewMode)
                return ((offset + 0.5f + XAxisSampleOffset) * (1 * DirectXWidth)) - 0;
            else
                return ((offset + 0.5f + XAxisSampleOffset) * (XZoomFactor * DirectXWidth)) - DisplayXOffset;
        }

        protected override string XLabelFromCursorPos(double xPos)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = ((DisplayXOffset + xPos) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;

            int sampleNum = (int)((offset + 0.5) * LinePointEntries);

            if (SamplingRate != 0)
            {
                return "Sample: " + sampleNum + "  + " + FrequencyFormatter.TimeToString((sampleNum / SamplingRate));
            }
            else
            {
                return "Sample: " + sampleNum;
            }
        }


        protected override string XLabelFromSampleNum(double pos)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = pos / DirectXWidth - 0.5f;

            int sampleNum = (int)((offset + 0.5) * LinePointEntries);

            if (SamplingRate != 0)
            {
                return "Sample: " + sampleNum + "  + " + FrequencyFormatter.TimeToString((sampleNum / SamplingRate));
            }
            else
            {
                return "Sample: " + sampleNum;
            }
        }

        protected override void KeyPressed(Keys key)
        {
            NeedsUpdate = true;
        }

        public override void PrepareLinePoints()
        {
            lock (SampleValues)
            {
                if (SampleValues.Count > 0)
                {
                    if (SampleValues.Count > MaxSamples)
                    {
                        int removeCount = SampleValues.Count - MaxSamples;
                        SampleValues.RemoveRange(0, removeCount);
                    }

                    int samples = SampleValues.Count;

                    lock (LinePointsLock)
                    {
                        if (LinePoints == null || LinePoints.Length < samples)
                            LinePoints = new Point[samples];

                        for (int pos = 0; pos < samples; pos++)
                        {
                            double sampleValue = (double)SampleValues[pos];
                            double posX = pos;
                            double posY = SampleToYPos(sampleValue);

                            LinePoints[pos].X = posX;
                            LinePoints[pos].Y = posY;
                        }

                        LinePointEntries = samples;
                        LinePointsUpdated = true;
                    }
                }
            }
        }

        private void LinePointUpdateTimer_Func(object sender, EventArgs e)
        {
            if (NeedsUpdate && !RealTimeMode)
            {
                try
                {
                    NeedsUpdate = false;
                    if (SlavePlot != null)
                        SlavePlot.PrepareLinePoints();
                    PrepareLinePoints();
                }
                catch (Exception ex)
                {
                    Log.AddMessage(ex.ToString());
                }
            }
        }

    }
}