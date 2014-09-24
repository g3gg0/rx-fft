using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.Misc;
using SlimDX.Direct3D9;
using Timer = LibRXFFT.Libraries.Timers.AccurateTimer;
using RX_FFT.Components.GDI;
using LibRXFFT.Components.DirectX.Drawables;


namespace LibRXFFT.Components.DirectX
{
    public partial class DirectX2DPlot : DirectXPlot
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
        public string ScaleUnit = "u";

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

        protected Thread ScreenRefreshTimer;
        protected Thread LinePointUpdateTimer;
        protected Thread DisplayThread;
        public object LinePointUpdateSignal = new object();
        protected bool NeedsUpdate
        {
            set
            {
                if (value)
                {
                    lock (LinePointUpdateSignal)
                    {
                        Monitor.Pulse(LinePointUpdateSignal);
                    }
                }
            }
        }
        public bool EnoughData = false;

        public double ScalePosMax = -1;
        public double ScalePosMin = 1;
        public double ScaleBarDistance = 0.10;
        public double ScaleTextDistance = 0.20;

        /* sample value buffer */
        public int MaxSamples { get; set; }
        double[,] SampleValues = new double[0, 2];
        private int SamplesWritten;

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
                    //LinePointUpdateTimer.Interval = (uint)RenderSleepDelay;
                    //ScreenRefreshTimer.Interval = (uint)(1000/MinRefreshRate);// ((value < MinRefreshRate) ? (1000 / MinRefreshRate) : RenderSleepDelay);
                }
            }
        }


        public DirectX2DPlot()
            : this(false)
        {
        }

        public DirectX2DPlot(bool slaveMode) : base(slaveMode)
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
            /*
            try
            {
                InitializeDirectX();
            }
            catch (Direct3D9Exception e)
            {
                MessageBox.Show("Failed initializing DirectX." + Environment.NewLine + e.ToString());
            }
            */

            if (!slaveMode)
            {
                ScreenRefreshTimer = new Thread(ScreenRefreshTask);
                ScreenRefreshTimer.Start();

                LinePointUpdateTimer = new Thread(LinePointUpdateTask);
                LinePointUpdateTimer.Start();
            }
        }

        void ScreenRefreshTask()
        {
            while (ScreenRefreshTimer != null)
            {
                lock (NeedsRenderSignal)
                {
                    Monitor.Wait(NeedsRenderSignal);
                }
                ScreenRefreshTimer_Func(ScreenRefreshTimer, null);
            }
        }

        void LinePointUpdateTask()
        {
            while (LinePointUpdateTimer != null)
            {
                lock (LinePointUpdateSignal)
                {
                    Monitor.Wait(LinePointUpdateSignal);
                }
                LinePointUpdateTimer_Func(LinePointUpdateTimer, null);
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
                    double xDist = 1;

                    if (RenderAsLines)
                    {
                        if (numPoints > PlotVerts.Length)
                        {
                            PlotVerts = new Vertex[numPoints];
                            PlotVertsOverview = new Vertex[numPoints];
                        }
                        PlotVertsEntries = numPoints - 1;
                        xDist = DirectXWidth / ((double)PlotVertsEntries);
                    }
                    else
                    {
                        if (numPoints * 2 > PlotVerts.Length)
                        {
                            PlotVerts = new Vertex[numPoints * 2];
                            PlotVertsOverview = new Vertex[numPoints * 2];
                        }
                        PlotVertsEntries = 2 * numPoints - 1;
                        xDist = DirectXWidth / ((double)PlotVertsEntries / 2.0f);
                    }
                    

                    int outPos = 0;
                    for (int pos = 0; pos < numPoints; pos++)
                    {
                        double xPos = DirectXWidth / 2 * (1 + points[pos].X / 2 * XZoomFactor);
                        double yPos = DirectXHeight / 2 * (1 + points[pos].Y / 2 * YZoomFactor);

                        PlotVerts[outPos].PositionRhw.X = (float)Math.Min(DirectXWidth, Math.Max(0, ((float)xPos - DisplayXOffset)));
                        PlotVerts[outPos].PositionRhw.Y = (float)Math.Min(DirectXHeight, Math.Max(0, ((float)yPos - DisplayYOffset)));
                        PlotVerts[outPos].PositionRhw.Z = 0.5f;
                        PlotVerts[outPos].PositionRhw.W = 1;
                        PlotVerts[outPos].Color = 0x9F000000 | colorFG;

                        outPos++;

                        if (!RenderAsLines)
                        {
                            PlotVerts[outPos] = PlotVerts[outPos - 1];
                            PlotVerts[outPos].PositionRhw.Z += 0.1f;
                            outPos++;
                        }
                    }

                    foreach (DirectXDrawable drawable in Drawables)
                    {
                        if (drawable is PlotVertsSink)
                        {
                            ((PlotVertsSink)drawable).ProcessPlotVerts(PlotVerts, PlotVertsEntries);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return;
            }
        }


        public void ProcessData(double iValue, double qValue)
        {
            lock (SampleValues)
            {
                if (MaxSamples * 2 != SampleValues.Length)
                {
                    SampleValues = new double[MaxSamples, 2];
                    SamplesWritten = 0;
                }

                if (SamplesWritten >= MaxSamples)
                {
                    if (RealTimeMode)
                    {
                        if (SlavePlot != null)
                            SlavePlot.PrepareLinePoints();
                        PrepareLinePoints();
                        SamplesWritten = 0;
                        lock (LinePointsLock)
                        {
                            CreateVertexBufferForPoints(LinePoints, LinePointEntries);
                        }

                        NeedsRender = true;
                    }
                    else
                    {
                        NeedsUpdate = true;
                        return;
                    }
                }
                else
                {
                    NeedsUpdate = true;
                }

                SampleValues[SamplesWritten, 0] = iValue;
                SampleValues[SamplesWritten, 1] = qValue;
                SamplesWritten++;
            }
        }


        protected override void OnMouseEnter(EventArgs e)
        {
            //Focus();
            base.OnMouseEnter(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            XMaximum = DirectXWidth;
        }


        public override void ProcessUserAction(eUserAction action, double param)
        {
            NeedsUpdate = true;
            double oldOffset;

            switch (action)
            {
                case eUserAction.XOffset:
                    oldOffset = DisplayXOffset;

                    param += oldOffset;
                    param = Math.Min(DirectXWidth * XZoomFactor, param);
                    DisplayXOffset = Math.Max(-DirectXWidth * XZoomFactor, param);

                    if (oldOffset != DisplayXOffset)
                    {
                        UpdateAxis = true;
                        UpdateCursor = true;
                        UpdateOverlays = true;
                    }

                    break;

                case eUserAction.YOffset:
                    oldOffset = DisplayYOffset;

                    param += oldOffset;
                    param = Math.Min(DirectXHeight * YZoomFactor, param);
                    DisplayYOffset = Math.Max(-DirectXHeight * YZoomFactor, param);

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
                        YZoomFactor *= YZoomStep;
                        XZoomFactor = YZoomFactor;

                        ProcessUserAction(eUserAction.XOffset, 0);
                        ProcessUserAction(eUserAction.YOffset, 0);

                        UpdateAxis = true;
                        UpdateCursor = true;
                        UpdateOverlays = true;
                    }

                    break;

                case eUserAction.YZoomOut:
                    if (YZoomFactor > YZoomFactorMin)
                    {
                        YZoomFactor /= YZoomStep;
                        XZoomFactor = YZoomFactor;

                        ProcessUserAction(eUserAction.XOffset, 0);
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

            NeedsRender = true;
        }

        protected override void RenderCursor()
        {
            uint colorCursor = (uint)ColorCursor.ToArgb();
            uint colorCursorBox = (uint)CursorBoxColor.ToArgb();

            float stubLength = (float)DirectXHeight / 10.0f;
            float xPos = (float)LastMousePos.X;
            float yPos = (float)LastMousePos.Y;
            float unit = (float)(yPos * ScaleUnitFactor);

            if (UpdateCursor)
            {
                UpdateCursor = false;

                /* draw vertical cursor line */
                CursorVertexesVert[0].PositionRhw.X = xPos;
                CursorVertexesVert[0].PositionRhw.Y = yPos - 30;

                CursorVertexesVert[1].PositionRhw.X = xPos;
                CursorVertexesVert[1].PositionRhw.Y = yPos;

                CursorVertexesVert[2].PositionRhw.X = xPos;
                CursorVertexesVert[2].PositionRhw.Y = yPos + 30;

                /* horizontal line */
                CursorVertexesHor[0].PositionRhw.X = xPos - 30;
                CursorVertexesHor[0].PositionRhw.Y = yPos;

                CursorVertexesHor[1].PositionRhw.X = xPos;
                CursorVertexesHor[1].PositionRhw.Y = yPos;

                CursorVertexesHor[2].PositionRhw.X = xPos + 30;
                CursorVertexesHor[2].PositionRhw.Y = yPos;


                /* recalc lines (this is needed just once btw.) */
                CursorVertexesVert[0].PositionRhw.Z = 0.5f;
                CursorVertexesVert[0].PositionRhw.W = 1;
                CursorVertexesVert[0].Color = colorCursor & 0x00FFFFFF;

                CursorVertexesVert[1].PositionRhw.Z = 0.5f;
                CursorVertexesVert[1].PositionRhw.W = 1;
                CursorVertexesVert[1].Color = colorCursor;

                CursorVertexesVert[2].PositionRhw.Z = 0.5f;
                CursorVertexesVert[2].PositionRhw.W = 1;
                CursorVertexesVert[2].Color = colorCursor & 0x00FFFFFF;


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

                //Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, CursorRectVertexes);

                if (MouseHovering)
                {
                    /* draw the strength into box */
                    //SmallFont.DrawString(null, signalPower, boxX + 5, boxY + 5, ColorFG.ToArgb());
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, CursorVertexesVert);
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, CursorVertexesHor);
                }

                /* draw the horizontal into box */
                //SmallFont.DrawString(null, positionLabel, boxX + 5, boxY + 5 + signalPowerHeight, ColorFG.ToArgb());
            }
        }

        protected override void RenderAxis()
        {
            uint colorCursor = (uint)ColorCursor.ToArgb();
            uint colorLimiter = (uint)LimiterColor.ToArgb();
            int xPos = (int)LastMousePos.X;
            int yPos = (int)LastMousePos.Y;

            float limiterLower = (float)(LimiterLowerLimit);
            float limiterUpper = (float)(LimiterUpperLimit);
            bool mouseInLowerLimit = (xPos <= limiterLower);
            bool mouseInUpperLimit = (xPos >= limiterUpper);

            /* only recalc scale lines when axis need to get updated */
            if (UpdateAxis)
            {
                UpdateAxis = false;
                ScaleVertexesUsed = 0;

                BuildStripRectangle(LimiterVertexesLeft, colorLimiter & 0x2FFFFFFF, 0, limiterLower);
                BuildStripRectangle(LimiterVertexesRight, colorLimiter & 0x2FFFFFFF, limiterUpper, DirectXWidth);

                int scaleCount = (int)Math.Round((ScalePosMax - ScalePosMin) / ScaleBarDistance);

                /* draw scale lines */
                for (int scaleNum = 0; scaleNum <= scaleCount; scaleNum++)
                {
                    float scalePos = (float)(ScalePosMin + scaleNum * ScaleBarDistance);

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 0;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = scalePos;
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

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = scalePos;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                    ScaleVertexes[ScaleVertexesUsed].Color = colorCursor & 0x00FFFFFF;
                    ScaleVertexesUsed++;

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = DirectXWidth;
                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = scalePos;
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

                    ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = scalePos;
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

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(DirectXWidth / 2 * (1 + 0.5 * XZoomFactor) - DisplayXOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)(DirectXHeight / 2 * (1 + 0.5 * YZoomFactor) - DisplayYOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = colorCursor;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(DirectXWidth / 2 * (1 - 0.5 * XZoomFactor) - DisplayXOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)(DirectXHeight / 2 * (1 + 0.5 * YZoomFactor) - DisplayYOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = colorCursor;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(DirectXWidth / 2 * (1 - 0.5 * XZoomFactor) - DisplayXOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)(DirectXHeight / 2 * (1 + 0.5 * YZoomFactor) - DisplayYOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = colorCursor;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(DirectXWidth / 2 * (1 - 0.5 * XZoomFactor) - DisplayXOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)(DirectXHeight / 2 * (1 - 0.5 * YZoomFactor) - DisplayYOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = colorCursor;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(DirectXWidth / 2 * (1 - 0.5 * XZoomFactor) - DisplayXOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)(DirectXHeight / 2 * (1 - 0.5 * YZoomFactor) - DisplayYOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = colorCursor;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(DirectXWidth / 2 * (1 + 0.5 * XZoomFactor) - DisplayXOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)(DirectXHeight / 2 * (1 - 0.5 * YZoomFactor) - DisplayYOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = colorCursor;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(DirectXWidth / 2 * (1 + 0.5 * XZoomFactor) - DisplayXOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)(DirectXHeight / 2 * (1 - 0.5 * YZoomFactor) - DisplayYOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = colorCursor;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(DirectXWidth / 2 * (1 + 0.5 * XZoomFactor) - DisplayXOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)(DirectXHeight / 2 * (1 + 0.5 * YZoomFactor) - DisplayYOffset);
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = colorCursor;
                OverlayVertexesUsed++;

                foreach (LabelledLine line in LabelledHorLines)
                {
                    if (OverlayVertexesUsed < OverlayVertexes.Length - 2)
                    {
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 0;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)(line.Position);
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                        OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                        OverlayVertexesUsed++;

                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = DirectXWidth;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)(line.Position);
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                        OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                        OverlayVertexesUsed++;

                        /* add the labels to the label list to draw */
                        OverlayTextLabels.AddLast(new StringLabel(line.Label, 60, (int)(line.Position) + 5, line.Color));
                    }
                }

                foreach (LabelledLine line in LabelledVertLines)
                {
                    if (OverlayVertexesUsed < OverlayVertexes.Length - 4)
                    {
                        double freq = line.Position;

                        if (freq >= 0 && freq < DirectXWidth)
                        {
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 10 + (float)(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = 0;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = 10;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = 10;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = DirectXHeight;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            /* add the labels to the label list to draw */
                            OverlayTextLabels.AddLast(new StringLabel(line.Label, (int)(line.Position) + 5, 5, line.Color));
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
                SmallFont.DrawString(null, " " + (int)(scalePos * ScaleUnitFactor) + " " + ScaleUnit, 10, (int)(scalePos), (int)(colorCursor & 0x80FFFFFF));
            }
        }


        public override string XLabelFromCursorPos(double xPos)
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


        protected override void KeyPressed(Keys key)
        {
            NeedsUpdate = true;
        }

        public override void PrepareLinePoints()
        {
            lock (SampleValues)
            {
                if (SamplesWritten > 0)
                {
                    int samples = SamplesWritten;

                    lock (LinePointsLock)
                    {
                        if (LinePoints == null || LinePoints.Length != samples)
                        {
                            LinePoints = new Point[samples];
                        }

                        for (int pos = 0; pos < SamplesWritten; pos++)
                        {
                            double posX = SampleValues[pos, 0];
                            double posY = SampleValues[pos, 1];

                            LinePoints[pos].X = posX;
                            LinePoints[pos].Y = posY;
                        }

                        SamplesWritten = 0;
                        LinePointEntries = samples;
                        LinePointsUpdated = true;
                    }
                }
            }
        }

        int NestingDepth = 0;

        private void LinePointUpdateTimer_Func(object sender, EventArgs e)
        {
            NestingDepth++;

            if (NestingDepth > 1)
            {
                Log.AddMessage("Nesting");
            }
            lock (SampleValues)
            {
                try
                {
                    if (!RealTimeMode)
                    {
                        if (SlavePlot != null)
                            SlavePlot.PrepareLinePoints();
                        PrepareLinePoints();

                        NeedsUpdate = false;
                        NeedsRender = true;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            NestingDepth--;
        }
    }
}
