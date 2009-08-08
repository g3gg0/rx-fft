using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using Timer = System.Timers.Timer;
using SlimDX.Direct3D9;
using SlimDX;
using Font = SlimDX.Direct3D9.Font;
using System.Runtime.InteropServices;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXWaterfallDisplay : DirectXPlot
    {

        /* DirectX related graphic stuff */
        protected Sprite Sprite;
        protected Surface DefaultRenderTarget;
        protected Texture LastWaterfallTexture;
        protected Texture TempWaterfallTexture;
        protected Color ColorFaderBG = Color.Orange;

        protected Vertex[] CursorVertexes = new Vertex[4];
        protected double DisplayXOffsetPrev = 0;
        protected Object DisplayXOffsetLock = new Object();
        protected bool ResetScaleBar = true;

        Vertex[] ScaleBarVertexes = new Vertex[4];
        Vertex[] ScaleBarVertexesLeft = new Vertex[3];
        Vertex[] ScaleBarVertexesRight = new Vertex[3];
        Vertex[] ScaleBarPos = new Vertex[4];

        /* timestamp related */
        protected DateTime TimeStamp = DateTime.Now;
        protected int LinesWithoutTimestamp = 0;
        protected int LinesWithoutTimestampMin = 16;
        public double TimeStampEveryMiliseconds = 1000;

        /* the averaging value to smooth the displayed lines */
        public double Averaging = 1;

        /* sample value buffer */
        protected double[] SampleValues = new double[0];
        protected long SampleValuesAveraged = 0;
        public long SamplesToAverage = 0;

        /* processing related */
        private readonly Thread DisplayThread;
        private bool NeedsUpdate = false;

        /* FFT related */
        private Mutex FFTLock = new Mutex();
        private FFTTransformer FFT;
        private int _FFTSize = 256;
        public double FFTPrescaler = 0.05f;
        public double FFTOffset = 0.3f;
        private double fftPrescalerDefault = 0.05f;
        private double fftOffsetDefault = 0.3f;

        /* if the fft data provided is already squared, set to true */
        public bool SquaredFFTData = false;

        public double LeveldBWhite = -10;
        public double LeveldBBlack = -100;
        public double LeveldBMax = -150;


        public DirectXWaterfallDisplay() : this(false)
        {
        }

        public DirectXWaterfallDisplay(bool slaveMode)
        {
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;
            ColorFont = Color.DarkCyan;
            ColorCursor = Color.Red;

            YAxisCentered = false;
            YZoomFactor = 0.01f;
            XZoomFactor = 1.0f;

            ActionMouseDragY = eUserAction.None;
            ActionMouseWheelUp = eUserAction.None;
            ActionMouseWheelDown = eUserAction.None;
            ActionMouseWheelUpShift = eUserAction.YOffset;
            ActionMouseWheelDownShift = eUserAction.YOffset;
            ActionMouseWheelUpControl = eUserAction.YZoomIn;
            ActionMouseWheelDownControl = eUserAction.YZoomOut;

            /* thats not possible with this display */
            OverviewModeEnabled = false;

            InitializeComponent();
            try
            {
                InitializeDirectX();
            }
            catch (Direct3D9Exception e)
            {
                MessageBox.Show("Failed initializing DirectX." + Environment.NewLine + e.ToString());
            }

            if (!slaveMode)
            {
                DisplayThread = new Thread(DisplayFunc);
                DisplayThread.Start();
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
                /* did the number of samples change? */
                if (SampleValues.Length != amplitudes.Length)
                {
                    SampleValues = new double[amplitudes.Length];
                    SampleValuesAveraged = 0;
                }

                /* the first samples are just copied, the others get added */
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

                /* if SampleValuesAverageCount is set and we summed up that many samples, we're done */
                if (SamplesToAverage == 0 || SampleValuesAveraged == SamplesToAverage)
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

        internal virtual void RenderOverlay()
        {

            /* draw white/black bar */
            int barLength = 50;
            int barTop = DirectXHeight - barLength - 20;
            int barBottom = barTop + barLength;
            int whiteYPos = (int)((LeveldBWhite / LeveldBMax) * barLength);
            int blackYPos = (int)((LeveldBBlack / LeveldBMax) * barLength);


            if (UpdateOverlays)
            {
                uint color = (uint)ColorFaderBG.ToArgb();

                ScaleBarVertexes[0].PositionRhw.X = 20;
                ScaleBarVertexes[0].PositionRhw.Y = barTop;
                ScaleBarVertexes[0].PositionRhw.Z = 0.5f;
                ScaleBarVertexes[0].PositionRhw.W = 1;
                ScaleBarVertexes[0].Color = color & 0x80FFFFFF;

                ScaleBarVertexes[1].PositionRhw.X = 20;
                ScaleBarVertexes[1].PositionRhw.Y = barTop + 10;
                ScaleBarVertexes[1].PositionRhw.Z = 0.5f;
                ScaleBarVertexes[1].PositionRhw.W = 2;
                ScaleBarVertexes[1].Color = color;

                ScaleBarVertexes[2].PositionRhw.X = 20;
                ScaleBarVertexes[2].PositionRhw.Y = barBottom - 10;
                ScaleBarVertexes[2].PositionRhw.Z = 0.5f;
                ScaleBarVertexes[2].PositionRhw.W = 2;
                ScaleBarVertexes[2].Color = color;

                ScaleBarVertexes[3].PositionRhw.X = 20;
                ScaleBarVertexes[3].PositionRhw.Y = barBottom;
                ScaleBarVertexes[3].PositionRhw.Z = 0.5f;
                ScaleBarVertexes[3].PositionRhw.W = 1;
                ScaleBarVertexes[3].Color = color & 0x80FFFFFF;

                ScaleBarVertexesLeft[0].PositionRhw.X = 19;
                ScaleBarVertexesLeft[0].PositionRhw.Y = barTop;
                ScaleBarVertexesLeft[0].PositionRhw.Z = 0.5f;
                ScaleBarVertexesLeft[0].PositionRhw.W = 1;
                ScaleBarVertexesLeft[0].Color = color & 0x00FFFFFF;

                ScaleBarVertexesLeft[1].PositionRhw.X = 19;
                ScaleBarVertexesLeft[1].PositionRhw.Y = (barTop + barBottom) / 2;
                ScaleBarVertexesLeft[1].PositionRhw.Z = 0.5f;
                ScaleBarVertexesLeft[1].PositionRhw.W = 1;
                ScaleBarVertexesLeft[1].Color = color;

                ScaleBarVertexesLeft[2].PositionRhw.X = 19;
                ScaleBarVertexesLeft[2].PositionRhw.Y = barBottom;
                ScaleBarVertexesLeft[2].PositionRhw.Z = 0.5f;
                ScaleBarVertexesLeft[2].PositionRhw.W = 1;
                ScaleBarVertexesLeft[2].Color = color & 0x00FFFFFF;

                ScaleBarVertexesRight[0].PositionRhw.X = 21;
                ScaleBarVertexesRight[0].PositionRhw.Y = barTop;
                ScaleBarVertexesRight[0].PositionRhw.Z = 0.5f;
                ScaleBarVertexesRight[0].PositionRhw.W = 1;
                ScaleBarVertexesRight[0].Color = color & 0x00FFFFFF;

                ScaleBarVertexesRight[1].PositionRhw.X = 21;
                ScaleBarVertexesRight[1].PositionRhw.Y = (barTop + barBottom) / 2;
                ScaleBarVertexesRight[1].PositionRhw.Z = 0.5f;
                ScaleBarVertexesRight[1].PositionRhw.W = 1;
                ScaleBarVertexesRight[1].Color = color;

                ScaleBarVertexesRight[2].PositionRhw.X = 21;
                ScaleBarVertexesRight[2].PositionRhw.Y = barBottom;
                ScaleBarVertexesRight[2].PositionRhw.Z = 0.5f;
                ScaleBarVertexesRight[2].PositionRhw.W = 1;
                ScaleBarVertexesRight[2].Color = color & 0x00FFFFFF;

                /* draw white/black limiter */

                ScaleBarPos[0].PositionRhw.X = 15;
                ScaleBarPos[0].PositionRhw.Y = barTop + whiteYPos;
                ScaleBarPos[0].PositionRhw.Z = 0.5f;
                ScaleBarPos[0].PositionRhw.W = 1;
                ScaleBarPos[0].Color = 0xFFFFFFFF;

                ScaleBarPos[1].PositionRhw.X = 25;
                ScaleBarPos[1].PositionRhw.Y = barTop + whiteYPos;
                ScaleBarPos[1].PositionRhw.Z = 0.5f;
                ScaleBarPos[1].PositionRhw.W = 2;
                ScaleBarPos[1].Color = 0xFFFFFFFF;

                ScaleBarPos[2].PositionRhw.X = 15;
                ScaleBarPos[2].PositionRhw.Y = barTop + blackYPos;
                ScaleBarPos[2].PositionRhw.Z = 0.5f;
                ScaleBarPos[2].PositionRhw.W = 1;
                ScaleBarPos[2].Color = 0xFFFFFFFF;

                ScaleBarPos[3].PositionRhw.X = 25;
                ScaleBarPos[3].PositionRhw.Y = barTop + blackYPos;
                ScaleBarPos[3].PositionRhw.Z = 0.5f;
                ScaleBarPos[3].PositionRhw.W = 2;
                ScaleBarPos[3].Color = 0xFFFFFFFF;
            }

            Device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, ScaleBarVertexesLeft);
            Device.DrawUserPrimitives(PrimitiveType.LineStrip, 3, ScaleBarVertexes);
            Device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, ScaleBarVertexesRight);
            Device.DrawUserPrimitives(PrimitiveType.LineList, 2, ScaleBarPos);



            SmallFont.DrawString(null, LeveldBWhite + " dB", 27, barTop + whiteYPos - 6, Color.White.ToArgb());
            SmallFont.DrawString(null, LeveldBBlack + " dB", 27, barTop + blackYPos - 6, Color.White.ToArgb());


            /* draw vertical line */
            float xPos = (float)LastMousePos.X;
            float stubLength = (float)DirectXHeight / 10.0f;

            CursorVertexes[0].PositionRhw.X = xPos;
            CursorVertexes[1].PositionRhw.X = xPos;
            CursorVertexes[2].PositionRhw.X = xPos;
            CursorVertexes[3].PositionRhw.X = xPos;

            if (UpdateOverlays)
            {
                CursorVertexes[0].PositionRhw.Y = 0;
                CursorVertexes[0].PositionRhw.Z = 0.5f;
                CursorVertexes[0].PositionRhw.W = 1;
                CursorVertexes[0].Color = 0x00FF3030;

                CursorVertexes[1].PositionRhw.Y = stubLength;
                CursorVertexes[1].PositionRhw.Z = 0.5f;
                CursorVertexes[1].PositionRhw.W = 1;
                CursorVertexes[1].Color = 0xFFFF3030;

                CursorVertexes[2].PositionRhw.Y = DirectXHeight - stubLength;
                CursorVertexes[2].PositionRhw.Z = 0.5f;
                CursorVertexes[2].PositionRhw.W = 1;
                CursorVertexes[2].Color = 0xFFFF3030;

                CursorVertexes[3].PositionRhw.Y = DirectXHeight;
                CursorVertexes[3].PositionRhw.Z = 0.5f;
                CursorVertexes[3].PositionRhw.W = 1;
                CursorVertexes[3].Color = 0x00FF3030;
            }

            Device.DrawUserPrimitives(PrimitiveType.LineStrip, 3, CursorVertexes);

        }

        internal override void PrepareLinePoints()
        {
            bool resetAverage = !LinePointsUpdated;

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
                                double sampleValue = (double)sampleArray[pos];
                                double posX = pos;
                                double posY = sampleValue;

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
                            resetAverage = false;
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

                if (NeedsUpdate && curTime.Subtract(lastUpdate).TotalMilliseconds >= RenderSleepDelay)
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
                    int delay = (int)Math.Min(1000 / 60, RenderSleepDelay - curTime.Subtract(lastUpdate).TotalMilliseconds);
                    if (delay >= 0)
                        Thread.Sleep(delay);
                    else
                        Thread.Sleep(1000 / 60);
                }

                if (SlavePlot != null)
                    SlavePlot.Render();
                Render();
            }
        }



        protected override void AllocateResources()
        {
            DefaultRenderTarget = Device.GetRenderTarget(0);

            SmallFont = new Font(Device, new System.Drawing.Font("Arial", 8));

            LastWaterfallTexture = new Texture(Device, PresentParameters.BackBufferWidth, PresentParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            TempWaterfallTexture = new Texture(Device, PresentParameters.BackBufferWidth, PresentParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            Sprite = new Sprite(Device);
        }

        protected override void ReleaseResources()
        {
            Sprite.Dispose();
            TempWaterfallTexture.Dispose();
            LastWaterfallTexture.Dispose();

            SmallFont.Dispose();

            DefaultRenderTarget.Dispose();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            XMaximum = DirectXWidth;
        }

        protected override void KeyPressed(Keys key)
        {
            if (key == Keys.Shift)
            {
                MainTextPrev = MainText;
                MainText = "Change Upper Limit (White)";
            }

            if (key == Keys.Control)
            {
                MainTextPrev = MainText;
                MainText = "Change Lower Limit (Black)";
            }
        }

        protected override void KeyReleased(Keys key)
        {
            if (key == Keys.Shift)
            {
                MainText = MainTextPrev;
                MainTextPrev = "";
            }

            if (key == Keys.Control)
            {
                MainText = MainTextPrev;
                MainTextPrev = "";
            }
        }

        public override void ProcessUserAction(eUserAction action, double param)
        {
            switch (action)
            {
                case eUserAction.YOffset:
                    LeveldBWhite = Math.Max(LeveldBBlack, Math.Min(0, LeveldBWhite + 2 * Math.Sign(param)));
                    AxisUpdated = true;
                    break;

                case eUserAction.YZoomIn:
                    LeveldBBlack = Math.Max(LeveldBMax, Math.Min(LeveldBWhite, LeveldBBlack + 2));
                    AxisUpdated = true;
                    break;

                case eUserAction.YZoomOut:
                    LeveldBBlack = Math.Max(LeveldBMax, Math.Min(LeveldBWhite, LeveldBBlack - 2));
                    AxisUpdated = true;
                    break;

                default:
                    /* in any other case lock the DisplayXOffset variable */
                    lock (DisplayXOffsetLock)
                    {
                        base.ProcessUserAction(action, param);
                    }
                    break;
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
                    if (numPoints > 0)
                    {
                        /* get density */
                        int density = 0;
                        for (int pos = 0; (pos < numPoints) && (((double)points[pos].X / (double)numPoints) * DirectXWidth * XZoomFactor < 1); pos++)
                            density++;

                        /* calculate average on high density */
                        if (density > 1)
                        {

                            int newNumPoints = (int)(((double)points[numPoints - 1].X / (double)numPoints) * DirectXWidth * XZoomFactor);
                            double ratio = (double)numPoints / (double)newNumPoints;

                            int startPos = 0;
                            for (int pos = 0; (pos < numPoints) && (points[pos].X * XZoomFactor < 0); pos++)
                                startPos++;

                            if (newNumPoints != PlotVerts.Length)
                            {
                                PlotVerts = new Vertex[newNumPoints];
                                PlotVertsOverview = new Vertex[newNumPoints];
                            }

                            PlotVertsEntries = newNumPoints - 1;

                            for (int pos = 0; pos < newNumPoints; pos++)
                            {
                                double maxAmpl = 0;

                                for (int sample = (int)(pos * ratio); sample < (pos + 1) * ratio; sample++)
                                {
                                    double yVal = points[startPos + sample].Y;
                                    float dB = (float)(SquaredFFTData ? DBTools.SquaredSampleTodB(yVal) : DBTools.SampleTodB(yVal));
                                    double ampl = 1 - ((dB - LeveldBWhite) / (LeveldBBlack - LeveldBWhite));

                                    ampl = Math.Max(0, ampl);
                                    ampl = Math.Min(1, ampl);

                                    maxAmpl = Math.Max(ampl, maxAmpl);
                                }

                                int colorCode = 0;
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.R * maxAmpl);
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.G * maxAmpl);
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.B * maxAmpl);

                                PlotVerts[pos].PositionRhw.X = (float)(pos - DisplayXOffset);
                                PlotVerts[pos].PositionRhw.Y = 0;
                                PlotVerts[pos].PositionRhw.Z = 0.5f;
                                PlotVerts[pos].PositionRhw.W = 1;
                                PlotVerts[pos].Color = (uint)(0xFF000000 | colorCode);
                            }
                        }
                        else
                        {
                            if (numPoints != PlotVerts.Length)
                            {
                                PlotVerts = new Vertex[numPoints];
                                PlotVertsOverview = new Vertex[numPoints];
                            }

                            PlotVertsEntries = numPoints - 1;

                            for (int pos = 0; pos < numPoints; pos++)
                            {
                                double yVal = points[pos].Y;
                                float dB = (float)(SquaredFFTData ? DBTools.SquaredSampleTodB(yVal) : DBTools.SampleTodB(yVal));
                                double ampl = 1 - ((dB - LeveldBWhite) / (LeveldBBlack - LeveldBWhite));

                                ampl = Math.Max(0, ampl);
                                ampl = Math.Min(1, ampl);

                                int colorCode = 0;
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.R * ampl);
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.G * ampl);
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.B * ampl);

                                double xPos = ((double)points[pos].X / (double)numPoints) * DirectXWidth;
                                PlotVerts[pos].PositionRhw.X = (float)((XAxisSampleOffset + xPos) * XZoomFactor - DisplayXOffset);
                                PlotVerts[pos].PositionRhw.Y = 0;
                                PlotVerts[pos].PositionRhw.Z = 0.5f;
                                PlotVerts[pos].PositionRhw.W = 1;
                                PlotVerts[pos].Color = (uint)(0xFF000000 | colorCode);
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

        internal override void Render()
        {
            if (!DirectXAvailable)
                return;

            try
            {
                DirectXLock.WaitOne();

                if (AxisUpdated)
                {
                    AxisUpdated = false;
                    UpdateOverlays = true;
                    CreateVertexBufferForAxis();
                }

                if (LinePointsUpdated)
                {
                    LinePointsUpdated = false;
                    lock (LinePointsLock)
                    {
                        CreateVertexBufferForPoints(LinePoints, LinePointEntries);
                    }
                }

                Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;

                if (PlotVertsEntries > 0)
                {
                    Device.SetRenderTarget(0, TempWaterfallTexture.GetSurfaceLevel(0));
                    Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                    Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                    Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
                    Device.BeginScene();

                    /* clear temp buffer */
                    Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);

                    /* move the old image when XOffset has changed */
                    float delta = 0;
                    lock (DisplayXOffsetLock)
                    {
                        delta = (float)(DisplayXOffsetPrev - DisplayXOffset);
                        DisplayXOffsetPrev = DisplayXOffset;
                    }

                    /* draw the old waterfall image */
                    Sprite.Begin(SpriteFlags.None);
                    Sprite.Draw(LastWaterfallTexture, Vector3.Zero, new Vector3(delta, 1, 0), new Color4(Color.White));
                    Sprite.End();

                    /* paint first line */
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, PlotVertsEntries, PlotVerts);
                    PlotVertsEntries = 0;

                    LinesWithoutTimestamp++;
                    /* draw a new timestamp */
                    DateTime newTimeStamp = DateTime.Now;
                    if (newTimeStamp.Subtract(TimeStamp).TotalMilliseconds > TimeStampEveryMiliseconds && LinesWithoutTimestamp > LinesWithoutTimestampMin)
                    {
                        TimeStamp = newTimeStamp;
                        LinesWithoutTimestamp = 0; 
                        
                        Vertex[] lineVertexes = new Vertex[4];
                        lineVertexes[0].PositionRhw.X = 0;
                        lineVertexes[0].PositionRhw.Y = 0;
                        lineVertexes[0].PositionRhw.Z = 0.5f;
                        lineVertexes[0].PositionRhw.W = 1;
                        lineVertexes[0].Color = (uint)ColorCursor.ToArgb();
                        lineVertexes[1].PositionRhw.X = 20;
                        lineVertexes[1].PositionRhw.Y = 0;
                        lineVertexes[1].PositionRhw.Z = 0.5f;
                        lineVertexes[1].PositionRhw.W = 1;
                        lineVertexes[1].Color = (uint)ColorCursor.ToArgb();

                        lineVertexes[2].PositionRhw.X = 20;
                        lineVertexes[2].PositionRhw.Y = 0;
                        lineVertexes[2].PositionRhw.Z = 0.5f;
                        lineVertexes[2].PositionRhw.W = 1;
                        lineVertexes[2].Color = (uint)ColorBG.ToArgb();
                        lineVertexes[3].PositionRhw.X = 22;
                        lineVertexes[3].PositionRhw.Y = 0;
                        lineVertexes[3].PositionRhw.Z = 0.5f;
                        lineVertexes[3].PositionRhw.W = 1;
                        lineVertexes[3].Color = (uint)ColorBG.ToArgb();

                        Device.DrawUserPrimitives(PrimitiveType.LineList, 2, lineVertexes);

                        //FixedFont.DrawString(null, TimeStamp.ToString(), 6, 2, ColorBG);
                        FixedFont.DrawString(null, TimeStamp.ToString(), 5, 1, (int)(ColorCursor.ToArgb() & 0xFF8F8F8F));
                    }

                    /* now write the temp buffer into the real image buffer */
                    Device.SetRenderTarget(0, LastWaterfallTexture.GetSurfaceLevel(0));

                    Sprite.Begin(SpriteFlags.None);
                    Sprite.Draw(TempWaterfallTexture, new Color4(Color.White));
                    Sprite.End();

                    Device.EndScene();
                }


                /* paint overlay */
                Device.SetRenderTarget(0, TempWaterfallTexture.GetSurfaceLevel(0));
                Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1.0f, 0);

                Device.BeginScene();

                if (XAxisVerts.Length > 0)
                    Device.DrawUserPrimitives(PrimitiveType.LineList, XAxisVerts.Length / 2, XAxisVerts);
                if (YAxisVerts.Length > 0)
                    Device.DrawUserPrimitives(PrimitiveType.LineList, YAxisVerts.Length / 2, YAxisVerts);

                DisplayFont.DrawString(null, MainText, 20, 30, ColorBG);
                DisplayFont.DrawString(null, MainText, 21, 31, ColorFont);

                RenderOverlay();
                Device.EndScene();
                /* end paint overlay */


                /* paint waterfall + overlay */
                Device.SetRenderTarget(0, DefaultRenderTarget);
                Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);

                Device.BeginScene();

                Sprite.Begin(SpriteFlags.AlphaBlend);
                Sprite.Draw(LastWaterfallTexture, Color.White);
                Sprite.Draw(TempWaterfallTexture, Color.White);
                Sprite.End();
                Device.EndScene();
                /* end paint waterfall*/


                Device.Present();

                UpdateOverlays = false;
            }
            catch (Direct3D9Exception e)
            {
                DirectXLock.ReleaseMutex();
                DirectXAvailable = false;

                int loops = 50;
                while (!DirectXAvailable && loops-- > 0)
                {
                    BeginInvoke(new ResetDirectXDelegate(ResetDirectX), null);
                    Thread.Sleep(100);
                }

                if (!DirectXAvailable)
                {
                    MessageBox.Show("Failed to re-init DirectX ater 10 seconds");
                    System.Console.WriteLine(e.ToString());
                }

                DirectXLock.WaitOne();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
            finally
            {
                DirectXLock.ReleaseMutex();
            }
        }
    }
}