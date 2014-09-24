using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SignalProcessing;
using SlimDX.Direct3D9;
using Timer = LibRXFFT.Libraries.Timers.AccurateTimer;
using LibRXFFT.Components.DirectX.Drawables;
using RX_FFT.Components.GDI;
using System.Collections;
using System.Threading.Tasks;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXFFTDisplay : DirectXPlot
    {
        /* DirectX related graphic stuff */
        protected Vertex[] CursorVertexesVert = new Vertex[4];
        protected Vertex[] CursorVertexesHor = new Vertex[3];
        protected Vertex[] CursorVertexesHorSide = new Vertex[8];

        protected Vertex[] ScaleVertexes = new Vertex[512];
        protected Vertex[] OverlayVertexes = new Vertex[100];
        protected ScaleLineInfo[] ScaleLineInfos = new ScaleLineInfo[1024];
        protected int ScaleLineInfosUsed = 0;
        protected struct ScaleLineInfo
        {
            public double Frequency;
            public string Text;
            public uint Color;
        }

        /* the cursor rectangle that contains signal information */
        protected Vertex[] CursorRectVertexes = new Vertex[4];
        protected Vertex[] CursorRectBorderVertexes = new Vertex[4];

        public Color CursorBoxColor = Color.Blue;
        public Color ColorGrid = Color.FromArgb(0x3F, Color.Gray);

        public string ScaleUnit = "dBm";

        protected Vertex[] LimiterVertexesLeft = new Vertex[4];
        protected Vertex[] LimiterVertexesRight = new Vertex[4];
        protected Vertex[] LimiterLines = new Vertex[4];

        public double LimiterLowerLimit = 0;
        public double LimiterUpperLimit = 0;
        public bool LimiterDisplayEnabled = true;
        public Color LimiterColor = Color.Green;
        public string LimiterUpperDescription = "";
        public string LimiterLowerDescription = "";

        protected int ScaleVertexesUsed = 0;
        protected int OverlayVertexesUsed = 0;

        protected Thread ScreenRefreshTimer;
        protected Thread LinePointUpdateTimer;
        protected Thread DisplayThread;
        public object LinePointUpdateSignal = new object();
        protected bool _NeedsUpdate = false;
        protected bool NeedsUpdate
        {
            get
            {
                return _NeedsUpdate;
            }
            set
            {
                if (value)
                {
                    lock (LinePointUpdateSignal)
                    {
                        Monitor.Pulse(LinePointUpdateSignal);
                    }
                }
                _NeedsUpdate = value;
            }
        }
        public bool EnoughData = false;
        public bool EnoughDataReset = false;

        /* variable horizontal cursor width - unit is Hz */
        public double HorLineStart = 0.0f;
        public double HorLineEnd = 0.0f;
        public bool HorLineFixed = false;


        /* channel displaying */
        public bool ChannelMode = false;
        public FrequencyBand ChannelBandDetails = null;

        /* sample value buffer */
        public double[] SampleValues = new double[0];
        protected long SampleValuesAveraged = 0;
        public long SamplesToAverage = 2;
        /* instead of averaging, get the peaks */
        public bool SampleValuesTrackPeaks = false;

        /* processing related */
        protected Mutex FFTLock = new Mutex();
        protected FFTTransformer FFT;
        protected int _FFTSize = 256;

        protected double BaseAmplification = 0;

        public double FFTPrescaler = 1.0f;
        protected double FFTPrescalerMin = 0.2f;
        protected double FFTPrescalerMax = 10.0f;

        public double FFTOffset = 0.0f;
        protected double FFTOffsetMin = -300.0f;
        protected double FFTOffsetMax = 300.0f;

        /* if the fft data provided is already squared, set to true */
        public bool SquaredFFTData = false;

        /* the averaging value to smooth the displayed lines */
        public double VerticalSmooth = 1;

        private double _SamplingRate = 100;
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
                    //ScreenRefreshTimer.Interval = (uint)(1000 / MinRefreshRate);// ((value < MinRefreshRate) ? (1000 / MinRefreshRate) : RenderSleepDelay);
                }
            }
        }


        public DirectXFFTDisplay()
            : this(false)
        {
        }

        public DirectXFFTDisplay(bool slaveMode)
            : base(slaveMode)
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
                    if (numPoints > PlotVerts.Length)
                    {
                        Array.Resize(ref PlotVerts, numPoints);
                        Array.Resize(ref PlotVertsOverview, numPoints);
                        //PlotVerts = new Vertex[numPoints];
                        //PlotVertsOverview = new Vertex[numPoints];
                    }

                    PlotVertsEntries = numPoints - 1;

                    for (int pos = 0; pos < numPoints; pos++)
                    {
                        double xPos = ((double)points[pos].X / (double)numPoints) * DirectXWidth;

                        PlotVerts[pos].PositionRhw.X = (float)Math.Min(DirectXWidth, Math.Max(0, ((XAxisSampleOffset + xPos) * XZoomFactor - DisplayXOffset)));
                        PlotVerts[pos].PositionRhw.Y = (float)Math.Min(DirectXHeight, Math.Max(0, (-sampleToDBScale(points[pos].Y - BaseAmplification))));
                        PlotVerts[pos].PositionRhw.Z = 0.5f;
                        PlotVerts[pos].PositionRhw.W = 1;
                        PlotVerts[pos].Color = 0x9F000000 | colorFG;

                        if (OverviewModeEnabled)
                        {
                            PlotVertsOverview[pos].PositionRhw.X = Math.Min(DirectXWidth, Math.Max(0, (float)(XAxisSampleOffset + xPos)));
                            PlotVertsOverview[pos].PositionRhw.Y = PlotVerts[pos].PositionRhw.Y;
                            PlotVertsOverview[pos].PositionRhw.Z = PlotVerts[pos].PositionRhw.Z;
                            PlotVertsOverview[pos].PositionRhw.W = PlotVerts[pos].PositionRhw.W;
                            PlotVertsOverview[pos].Color = PlotVerts[pos].Color;
                        }
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
            ProcessFFTData(amplitudes, 0, 0);
        }

        public void ProcessFFTData(double[] amplitudes, int spectPart, double baseAmp)
        {
            if (EnoughData || (EnoughDataReset && spectPart != 0))
                return;

            EnoughDataReset = false;

            /* to prevent that the value changes during processing */
            int spectSize = amplitudes.Length;
            int spectParts = SpectParts;
            if (spectPart >= spectParts)
            {
                return;
            }

            lock (SampleValues)
            {
                BaseAmplification = baseAmp;

                if (SampleValues.Length != spectSize * spectParts)
                {
                    Array.Resize(ref SampleValues, spectSize * spectParts);
                    //SampleValues = new double[spectSize * spectParts];
                }

                if (SamplesToAverage == 0)
                {
                    /* no preference made, just average all samples we get */
                    if (SampleValuesAveraged == 0)
                    {
                        for (int pos = 0; pos < spectSize; pos++)
                            SampleValues[spectPart * spectSize + pos] = amplitudes[pos];
                    }
                    else
                    {
                        /* track peaks if configured */
                        if (SampleValuesTrackPeaks)
                        {
                            for (int pos = 0; pos < spectSize; pos++)
                            {
                                SampleValues[spectPart * spectSize + pos] = Math.Max(SampleValues[spectPart * spectSize + pos], amplitudes[pos]);
                            }
                        }
                        else
                        {
                            for (int pos = 0; pos < spectSize; pos++)
                            {
                                SampleValues[spectPart * spectSize + pos] += amplitudes[pos];
                            }
                        }
                    }

                    if (spectPart + 1 == spectParts)
                    {
                        SampleValuesAveraged++;
                        NeedsUpdate = true;
                    }
                }
                else
                {
                    /* dont average more as requested */
                    if (SampleValuesAveraged < SamplesToAverage)
                    {
                        if (SampleValuesAveraged == 0)
                        {
                            for (int pos = 0; pos < spectSize; pos++)
                            {
                                SampleValues[spectPart * spectSize + pos] = amplitudes[pos];
                            }
                        }
                        else
                        {
                            /* track peaks if configured */
                            if (SampleValuesTrackPeaks)
                            {
                                for (int pos = 0; pos < spectSize; pos++)
                                {
                                    int valPos = spectPart * spectSize + pos;
                                    SampleValues[valPos] = Math.Max(SampleValues[valPos], amplitudes[pos]);
                                }
                            }
                            else
                            {
                                for (int pos = 0; pos < spectSize; pos++)
                                {
                                    SampleValues[spectPart * spectSize + pos] += amplitudes[pos];
                                }
                            }
                        }

                        SampleValuesAveraged++;
                    }

                    /* to reduce CPU load */
                    lock (SampleValues)
                    {
                        if (SampleValuesAveraged >= SamplesToAverage)
                        {
                            if (spectPart + 1 == spectParts)
                            {
                                EnoughData = true;
                                NeedsUpdate = true;
                            }
                            else
                            {
                                SampleValuesAveraged = 0;
                            }
                        }
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

            EnoughDataReset = false;

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

        protected virtual void RefreshLinePoints()
        {
            LinePointsUpdated = true;
        }


        public override void ProcessUserAction(eUserAction action, double param)
        {
            RefreshLinePoints();

            switch (action)
            {
                case eUserAction.YZoomIn:
                    if (FFTPrescaler < FFTPrescalerMax)
                    {
                        FFTOffset = ((LastMousePos.Y + FFTOffset) * YZoomStep) - LastMousePos.Y;
                        FFTPrescaler *= YZoomStep;
                    }

                    UpdateAxis = true;
                    UpdateCursor = true;
                    UpdateOverlays = true;
                    break;

                case eUserAction.YZoomOut:
                    if (FFTPrescaler > FFTPrescalerMin)
                    {
                        FFTOffset = ((LastMousePos.Y + FFTOffset) / YZoomStep) - LastMousePos.Y;
                        FFTPrescaler /= YZoomStep;
                    }

                    UpdateAxis = true;
                    UpdateCursor = true;
                    UpdateOverlays = true;
                    break;

                case eUserAction.YOffset:
                    if (Math.Abs(param) < 5)
                    {
                        FFTOffset += param;
                        FFTOffset = Math.Max(FFTOffsetMin, Math.Min(FFTOffsetMax, FFTOffset));
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
            float stubWidth = (float)DirectXWidth / 30.0f;

            float horLineHeight = (float)DirectXHeight / 10.0f;

            float xPos = (float)LastMousePos.X;
            float yPos = (float)LastMousePos.Y;
            float dB = (float)sampleFromDBScale(-yPos);

            if (UpdateCursor)
            {
                UpdateCursor = false;

                float stubOffsetX = ((xPos / DirectXWidth) - 0.5f) * 2 * stubWidth;
                float stubStart = Math.Min(yPos - stubLength, 20);

                /* draw vertical cursor line */
                CursorVertexesVert[0].PositionRhw.X = xPos - stubOffsetX;
                CursorVertexesVert[0].PositionRhw.Y = stubStart;
                CursorVertexesVert[1].PositionRhw.X = xPos;
                CursorVertexesVert[1].PositionRhw.Y = stubStart + stubLength;
                CursorVertexesVert[2].PositionRhw.X = xPos;
                CursorVertexesVert[2].PositionRhw.Y = DirectXHeight - stubLength;
                CursorVertexesVert[3].PositionRhw.X = xPos;
                CursorVertexesVert[3].PositionRhw.Y = DirectXHeight;


                /* recalc lines (this is needed just once btw.) */
                CursorVertexesVert[0].PositionRhw.Z = 0.5f;
                CursorVertexesVert[0].PositionRhw.W = 1;
                CursorVertexesVert[0].Color = colorCursor & 0x00FFFFFF;

                CursorVertexesVert[1].PositionRhw.Z = 0.5f;
                CursorVertexesVert[1].PositionRhw.W = 1;
                CursorVertexesVert[1].Color = colorCursor;

                CursorVertexesVert[2].PositionRhw.Z = 0.5f;
                CursorVertexesVert[2].PositionRhw.W = 1;
                CursorVertexesVert[2].Color = colorCursor;

                CursorVertexesVert[3].PositionRhw.Z = 0.5f;
                CursorVertexesVert[3].PositionRhw.W = 1;
                CursorVertexesVert[3].Color = colorCursor;

                if (HorLineFixed)
                {
                    float absoluteWidthStart = (float)(XPosFromFrequency(CenterFrequency + HorLineStart) - XPosFromFrequency(CenterFrequency));
                    float absoluteWidthEnd = (float)(XPosFromFrequency(CenterFrequency + HorLineEnd) - XPosFromFrequency(CenterFrequency));

                    /* horizontal line */
                    CursorVertexesHor[0].PositionRhw.X = xPos + absoluteWidthStart;
                    CursorVertexesHor[0].PositionRhw.Y = yPos;
                    CursorVertexesHor[0].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[0].PositionRhw.W = 1;
                    CursorVertexesHor[0].Color = colorCursor;

                    CursorVertexesHor[1].PositionRhw.X = xPos;
                    CursorVertexesHor[1].PositionRhw.Y = yPos;
                    CursorVertexesHor[1].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[1].PositionRhw.W = 1;
                    CursorVertexesHor[1].Color = colorCursor;

                    CursorVertexesHor[2].PositionRhw.X = xPos + absoluteWidthEnd;
                    CursorVertexesHor[2].PositionRhw.Y = yPos;
                    CursorVertexesHor[2].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[2].PositionRhw.W = 1;
                    CursorVertexesHor[2].Color = colorCursor;

                    /* 1 */
                    CursorVertexesHorSide[0].PositionRhw.X = xPos + absoluteWidthStart;
                    CursorVertexesHorSide[0].PositionRhw.Y = yPos - horLineHeight;
                    CursorVertexesHorSide[0].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[0].PositionRhw.W = 1;
                    CursorVertexesHorSide[0].Color = colorCursor & 0x00FFFFFF;

                    CursorVertexesHorSide[1].PositionRhw.X = xPos + absoluteWidthStart;
                    CursorVertexesHorSide[1].PositionRhw.Y = yPos;
                    CursorVertexesHorSide[1].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[1].PositionRhw.W = 1;
                    CursorVertexesHorSide[1].Color = colorCursor;

                    /* 2 */
                    CursorVertexesHorSide[2].PositionRhw.X = xPos + absoluteWidthStart;
                    CursorVertexesHorSide[2].PositionRhw.Y = yPos;
                    CursorVertexesHorSide[2].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[2].PositionRhw.W = 1;
                    CursorVertexesHorSide[2].Color = colorCursor;

                    CursorVertexesHorSide[3].PositionRhw.X = xPos + absoluteWidthStart;
                    CursorVertexesHorSide[3].PositionRhw.Y = yPos + horLineHeight;
                    CursorVertexesHorSide[3].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[3].PositionRhw.W = 1;
                    CursorVertexesHorSide[3].Color = colorCursor & 0x00FFFFFF;

                    /* 3 */
                    CursorVertexesHorSide[4].PositionRhw.X = xPos + absoluteWidthEnd;
                    CursorVertexesHorSide[4].PositionRhw.Y = yPos - horLineHeight;
                    CursorVertexesHorSide[4].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[4].PositionRhw.W = 1;
                    CursorVertexesHorSide[4].Color = colorCursor & 0x00FFFFFF;

                    CursorVertexesHorSide[5].PositionRhw.X = xPos + absoluteWidthEnd;
                    CursorVertexesHorSide[5].PositionRhw.Y = yPos;
                    CursorVertexesHorSide[5].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[5].PositionRhw.W = 1;
                    CursorVertexesHorSide[5].Color = colorCursor;

                    /* 4 */
                    CursorVertexesHorSide[6].PositionRhw.X = xPos + absoluteWidthEnd;
                    CursorVertexesHorSide[6].PositionRhw.Y = yPos;
                    CursorVertexesHorSide[6].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[6].PositionRhw.W = 1;
                    CursorVertexesHorSide[6].Color = colorCursor;

                    CursorVertexesHorSide[7].PositionRhw.X = xPos + absoluteWidthEnd;
                    CursorVertexesHorSide[7].PositionRhw.Y = yPos + horLineHeight;
                    CursorVertexesHorSide[7].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[7].PositionRhw.W = 1;
                    CursorVertexesHorSide[7].Color = colorCursor & 0x00FFFFFF;
                }
                else
                {
                    /* horizontal line */
                    CursorVertexesHor[0].PositionRhw.X = xPos - DirectXWidth / 20;
                    CursorVertexesHor[0].PositionRhw.Y = yPos;
                    CursorVertexesHor[0].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[0].PositionRhw.W = 1;

                    CursorVertexesHor[0].Color = colorCursor & 0x00FFFFFF;
                    CursorVertexesHor[1].PositionRhw.X = xPos;
                    CursorVertexesHor[1].PositionRhw.Y = yPos;
                    CursorVertexesHor[1].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[1].PositionRhw.W = 1;
                    CursorVertexesHor[1].Color = colorCursor;

                    CursorVertexesHor[2].PositionRhw.X = xPos + DirectXWidth / 20;
                    CursorVertexesHor[2].PositionRhw.Y = yPos;
                    CursorVertexesHor[2].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[2].PositionRhw.W = 1;
                    CursorVertexesHor[2].Color = colorCursor & 0x00FFFFFF;
                }
            }

            if ((MouseHovering || ShowVerticalCursor) && !HideCursor)
            {
                string signalPower = dB + " " + ScaleUnit;
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
                    if (HorLineFixed)
                    {
                        Device.DrawUserPrimitives(PrimitiveType.LineList, 4, CursorVertexesHorSide);
                    }
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

            float limiterLower = (float)XPosFromFrequency(LimiterLowerLimit);
            float limiterUpper = (float)XPosFromFrequency(LimiterUpperLimit);
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


                /* draw scale lines */
                for (int dBLevel = 0; dBLevel <= 150; dBLevel += 10)
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

                /* draw frequency markers */
                double width = SamplingRate;
                double startFreq = CenterFrequency - width / 2;

                double scaleDistance = Math.Pow(10, Math.Floor(Math.Log10(width) - 0.5f));
                int scaleCount = (int)(width / scaleDistance) + 1;

                if (scaleCount == 0)
                {
                    scaleCount = 1;
                    scaleDistance = width;
                }

                int pixelsPerRange = DirectXWidth / scaleCount;
                int interScales = 1;

                for (int tries = 0; tries < 5; tries++)
                {
                    if (pixelsPerRange < 80 && pixelsPerRange < DirectXWidth / 4)
                    {
                        scaleDistance *= 2;
                        interScales *= 2;
                        scaleCount = (int)(width / scaleDistance) + 1;
                        pixelsPerRange = DirectXWidth / scaleCount;
                    }
                }


                double interScaleDistance = scaleDistance / interScales;
                double firstScaleFreq = Math.Floor(startFreq / scaleDistance) * scaleDistance;

                /* generate a list of frequencies to draw text for */
                lock (ScaleLineInfos)
                {
                    ScaleLineInfosUsed = 0;
                    for (int pos = 0; pos < scaleCount; pos++)
                    {
                        double freq = firstScaleFreq + (pos * interScales) * interScaleDistance;

                        float scaleXPos = (int)XPosFromFrequency(freq);
                        float alpha = Math.Min((Math.Min(scaleXPos, DirectXWidth - scaleXPos) / 100), 1.0f) * 255.0f;

                        if (alpha > 0)
                        {
                            ScaleLineInfos[ScaleLineInfosUsed].Frequency = freq;
                            ScaleLineInfos[ScaleLineInfosUsed].Text = FrequencyFormatter.FreqToStringAccurate(freq);
                            ScaleLineInfos[ScaleLineInfosUsed].Color = (uint)((colorCursor & 0x00FFFFFF) | ((int)alpha << 24));
                            ScaleLineInfosUsed++;
                        }
                    }
                }

                /* generate lines for the same frequencies */
                for (int pos = 0; pos < scaleCount; pos++)
                {
                    for (int interScale = 0; interScale < interScales; interScale++)
                    {
                        if (ScaleVertexesUsed + 4 > ScaleVertexes.Length)
                        {
                            return;
                        }

                        float scaleXPos = (int)XPosFromFrequency(firstScaleFreq + (pos * interScales + interScale) * interScaleDistance);
                        float alpha = Math.Min((Math.Min(scaleXPos, DirectXWidth - scaleXPos) / 100), 1.0f) * 255.0f;

                        if (alpha > 0)
                        {
                            /* the grey line */
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = scaleXPos;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = 0;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                            ScaleVertexes[ScaleVertexesUsed].Color = (uint)((ColorGrid.ToArgb() & 0x00FFFFFF) | ((int)(alpha / 4) << 24));
                            ScaleVertexesUsed++;

                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = scaleXPos;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = DirectXHeight;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                            ScaleVertexes[ScaleVertexesUsed].Color = (uint)((ColorGrid.ToArgb() & 0x00FFFFFF) | ((int)(alpha / 4) << 24));
                            ScaleVertexesUsed++;

                            /* the red line stub */
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = scaleXPos;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = 0;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                            ScaleVertexes[ScaleVertexesUsed].Color = (uint)((colorCursor & 0x00FFFFFF) | ((int)alpha << 24));
                            ScaleVertexesUsed++;

                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = scaleXPos;
                            if (interScale == 0)
                            {
                                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = 25;
                            }
                            else
                            {
                                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = 10;
                            }
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                            ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                            ScaleVertexes[ScaleVertexesUsed].Color = colorCursor & 0x00FFFFFF;
                            ScaleVertexesUsed++;
                        }
                    }
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
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)-sampleToDBScale(line.Position);
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                        OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                        OverlayVertexesUsed++;

                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = DirectXWidth;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (float)-sampleToDBScale(line.Position);
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                        OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                        OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                        OverlayVertexesUsed++;
                    }
                }

                foreach (LabelledLine line in LabelledVertLines)
                {
                    if (OverlayVertexesUsed < OverlayVertexes.Length - 4)
                    {
                        double freq = XPosFromFrequency(line.Position);

                        if (freq >= 0 && freq < DirectXWidth)
                        {
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 10 + (float)XPosFromFrequency(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = 0;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)XPosFromFrequency(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = 10;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)XPosFromFrequency(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = 10;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = (float)XPosFromFrequency(line.Position);
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = DirectXHeight;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                            OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                            OverlayVertexes[OverlayVertexesUsed].Color = line.Color;
                            OverlayVertexesUsed++;

                            /* add the labels to the label list to draw */
                            OverlayTextLabels.AddLast(new StringLabel(line.Label, (int)XPosFromFrequency(line.Position) + 5, 15, line.Color));
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

            SmallFont.DrawString(null, "   0 " + ScaleUnit, 10, (int)-sampleToDBScale(0), (int)(colorCursor & 0x80FFFFFF));
            SmallFont.DrawString(null, " -50 " + ScaleUnit, 10, (int)-sampleToDBScale(-50), (int)(colorCursor & 0x80FFFFFF));
            SmallFont.DrawString(null, "-100 " + ScaleUnit, 10, (int)-sampleToDBScale(-100), (int)(colorCursor & 0x80FFFFFF));
            SmallFont.DrawString(null, "-150 " + ScaleUnit, 10, (int)-sampleToDBScale(-150), (int)(colorCursor & 0x80FFFFFF));

            lock (ScaleLineInfos)
            {
                for (int pos = 0; pos < ScaleLineInfosUsed; pos++)
                {
                    SmallFont.DrawString(null, ScaleLineInfos[pos].Text, (int)XPosFromFrequency(ScaleLineInfos[pos].Frequency), (int)20, (int)ScaleLineInfos[pos].Color);
                }
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

            if (OverviewMode)
                return ((offset + 0.5f + XAxisSampleOffset) * (1 * DirectXWidth)) - 0;
            else
                return ((offset + 0.5f + XAxisSampleOffset) * (XZoomFactor * DirectXWidth)) - DisplayXOffset;
        }

        public double YPosFromStrength(double strength)
        {
            return -sampleToDBScale(strength);
        }

        public double XRelativeCoordFromCursorPos()
        {
            return XRelativeCoordFromCursorPos(LastMousePos.X);
        }

        public override string XLabelFromCursorPos(double xPos)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = ((DisplayXOffset + xPos) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;

            if (!ChannelMode)
            {
                double frequency = CenterFrequency + offset * SamplingRate;

                return FrequencyFormatter.FreqToStringAccurate(frequency) + "  (Δ " + FrequencyFormatter.FreqToString(offset * SamplingRate) + ")";
            }
            else
            {
                long channels = 1 + ChannelBandDetails.ChannelEnd - ChannelBandDetails.ChannelStart;
                long channel = (long)(ChannelBandDetails.ChannelStart + (offset + 0.5f) * channels);
                return "Ch. " + channel;
            }
        }

        public long CursorFrequency
        {
            get
            {
                return FrequencyFromCursorPos();
            }
        }

        public double CursorStrength
        {
            get
            {
                return sampleFromDBScale(-LastMousePos.Y);
            }
        }

        public long FrequencyFromCursorPos()
        {
            /* offset (-0.5 ... 0.5) */
            double offset = ((DisplayXOffset + LastMousePos.X) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;

            if (!ChannelMode)
            {
                long frequency = (long)(CenterFrequency + offset * SamplingRate);
                return frequency;
            }
            else
            {
                long channels = 1 + ChannelBandDetails.ChannelEnd - ChannelBandDetails.ChannelStart;
                long channel = (long)((offset + 0.5f) * channels);
                return ChannelBandDetails.BaseFrequency + channel * ChannelBandDetails.ChannelDistance;
            }
        }

        public long FrequencyFromCursorPosOffset(double xOffset)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = ((DisplayXOffset + LastMousePos.X + xOffset) / (XZoomFactor * DirectXWidth)) - 0.5f - XAxisSampleOffset;

            if (!ChannelMode)
            {
                long frequency = (long)(CenterFrequency + offset * SamplingRate);
                return frequency;
            }
            else
            {
                long channels = 1 + ChannelBandDetails.ChannelEnd - ChannelBandDetails.ChannelStart;
                long channel = (long)((offset + 0.5f) * channels);
                return ChannelBandDetails.BaseFrequency + channel * ChannelBandDetails.ChannelDistance;
            }
        }


        public override string XLabelFromSampleNum(double pos)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = pos / FFTSize - 0.5f;

            if (!ChannelMode)
            {
                double frequency = CenterFrequency + offset * SamplingRate;
                return FrequencyFormatter.FreqToStringAccurate(frequency) + "  (Δ " + FrequencyFormatter.FreqToString(offset * SamplingRate) + ")";
            }
            else
            {
                long channels = 1 + ChannelBandDetails.ChannelEnd - ChannelBandDetails.ChannelStart;
                long channel = (long)(ChannelBandDetails.ChannelStart + (offset + 0.5f) * channels);
                return "Ch. " + channel;
            }
        }

        public double FrequencyFromSampleNum(double pos)
        {
            /* offset (-0.5 ... 0.5) */
            double offset = pos / FFTSize - 0.5f;

            if (!ChannelMode)
            {
                double frequency = CenterFrequency + offset * SamplingRate;
                return frequency;
            }
            else
            {
                long channels = 1 + ChannelBandDetails.ChannelEnd - ChannelBandDetails.ChannelStart;
                long channel = (long)(ChannelBandDetails.ChannelStart + (offset + 0.5f) * channels);
                return channel;
            }
        }

        protected override void KeyPressed(Keys key)
        {
            LinePointsUpdated = true;
        }

        public override void PrepareLinePoints()
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
                            if (LinePoints == null || LinePoints.Length != samples)
                            {
                                Array.Resize(ref LinePoints, samples);
                                //LinePoints = new Point[samples];
                            }

                            for (int pos = 0; pos < samples; pos++)
                            {
                                double posX = pos;
                                double posY = (double)sampleArray[pos];

                                LinePoints[pos].X = posX;

                                /* if configured to track peaks, dont average values */
                                if (SampleValuesTrackPeaks)
                                {
                                    SampleValuesAveraged = 1;
                                }

                                if (VerticalSmooth != 1.0f)
                                {
                                    /* some simple averaging */
                                    LinePoints[pos].Y *= (VerticalSmooth - 1);
                                    LinePoints[pos].Y += posY / SampleValuesAveraged;
                                    LinePoints[pos].Y /= VerticalSmooth;
                                }
                                else
                                {
                                    LinePoints[pos].Y = posY / SampleValuesAveraged;
                                }

                                if (double.IsNaN(LinePoints[pos].Y))
                                {
                                    LinePoints[pos].Y = 0;
                                }
                            }
                            LinePointEntries = samples;
                            LinePointsUpdated = true;
                        }
                    }
                    SampleValuesAveraged = 0;
                }
                EnoughDataReset = true;
                EnoughData = false;
            }
        }

        private void LinePointUpdateTimer_Func(object sender, EventArgs e)
        {
            lock (SampleValues)
            {
                if (NeedsUpdate)
                {
                    try
                    {
                        NeedsUpdate = false;
                        NeedsRender = true;

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
}