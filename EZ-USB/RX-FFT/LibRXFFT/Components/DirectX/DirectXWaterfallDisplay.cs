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
    public partial class DirectXWaterfallDisplay : UserControl
    {
        private readonly Thread DisplayThread;
        private bool NeedsUpdate = false;

        readonly ArrayList SampleValues = new ArrayList();
        private DisplayFuncState DisplayTimerState;
        private Vertex[] CursorVertexes = new Vertex[4];

        private Mutex FFTLock = new Mutex();
        private FFTTransformer FFT;

        public bool ShowFPS { get; set; }
        public bool UseLines { get; set; }

        private int _FFTSize = 256;
        public double FFTPrescaler = 0.05f;
        public double FFTOffset = 0.3f;
        private double fftPrescalerDefault = 0.05f;
        private double fftOffsetDefault = 0.3f;

        public double Averaging = 1;

        /* if the fft data provided is already squared, set to true */
        public bool SquaredFFTData = false;

        public double LeveldBWhite = -10;
        public double LeveldBBlack = -100;
        public double LeveldBMax = -150;

        internal Font SmallFont = null;
        internal bool ResetScaleBar = true;

        Vertex[] ScaleBarVertexes = new Vertex[4];
        Vertex[] ScaleBarVertexesLeft = new Vertex[3];
        Vertex[] ScaleBarVertexesRight = new Vertex[3];
        Vertex[] ScaleBarPos = new Vertex[4];
        


        public DirectXWaterfallDisplay()
        {
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;
            ColorFont = Color.DarkCyan;

            ShowFPS = true;
            UseLines = true;
            YAxisCentered = false;
            YZoomFactor = 0.01f;
            XZoomFactor = 1.0f;


            InitializeComponent();
            InitializeDirectX();

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
                ScaleBarVertexes[0].PositionRhw.X = 20;
                ScaleBarVertexes[0].PositionRhw.Y = barTop;
                ScaleBarVertexes[0].PositionRhw.Z = 0.5f;
                ScaleBarVertexes[0].PositionRhw.W = 1;
                ScaleBarVertexes[0].Color = 0x80FF8030;

                ScaleBarVertexes[1].PositionRhw.X = 20;
                ScaleBarVertexes[1].PositionRhw.Y = barTop + 10;
                ScaleBarVertexes[1].PositionRhw.Z = 0.5f;
                ScaleBarVertexes[1].PositionRhw.W = 2;
                ScaleBarVertexes[1].Color = 0xFFFF8030;

                ScaleBarVertexes[2].PositionRhw.X = 20;
                ScaleBarVertexes[2].PositionRhw.Y = barBottom - 10;
                ScaleBarVertexes[2].PositionRhw.Z = 0.5f;
                ScaleBarVertexes[2].PositionRhw.W = 2;
                ScaleBarVertexes[2].Color = 0xFFFF8030;

                ScaleBarVertexes[3].PositionRhw.X = 20;
                ScaleBarVertexes[3].PositionRhw.Y = barBottom;
                ScaleBarVertexes[3].PositionRhw.Z = 0.5f;
                ScaleBarVertexes[3].PositionRhw.W = 1;
                ScaleBarVertexes[3].Color = 0x80FF8030;

                ScaleBarVertexesLeft[0].PositionRhw.X = 19;
                ScaleBarVertexesLeft[0].PositionRhw.Y = barTop;
                ScaleBarVertexesLeft[0].PositionRhw.Z = 0.5f;
                ScaleBarVertexesLeft[0].PositionRhw.W = 1;
                ScaleBarVertexesLeft[0].Color = 0x00FF8030;

                ScaleBarVertexesLeft[1].PositionRhw.X = 19;
                ScaleBarVertexesLeft[1].PositionRhw.Y = (barTop + barBottom) / 2;
                ScaleBarVertexesLeft[1].PositionRhw.Z = 0.5f;
                ScaleBarVertexesLeft[1].PositionRhw.W = 1;
                ScaleBarVertexesLeft[1].Color = 0xFFFF8030;

                ScaleBarVertexesLeft[2].PositionRhw.X = 19;
                ScaleBarVertexesLeft[2].PositionRhw.Y = barBottom;
                ScaleBarVertexesLeft[2].PositionRhw.Z = 0.5f;
                ScaleBarVertexesLeft[2].PositionRhw.W = 1;
                ScaleBarVertexesLeft[2].Color = 0x00FF8030;

                ScaleBarVertexesRight[0].PositionRhw.X = 21;
                ScaleBarVertexesRight[0].PositionRhw.Y = barTop;
                ScaleBarVertexesRight[0].PositionRhw.Z = 0.5f;
                ScaleBarVertexesRight[0].PositionRhw.W = 1;
                ScaleBarVertexesRight[0].Color = 0x00FF8030;

                ScaleBarVertexesRight[1].PositionRhw.X = 21;
                ScaleBarVertexesRight[1].PositionRhw.Y = (barTop + barBottom) / 2;
                ScaleBarVertexesRight[1].PositionRhw.Z = 0.5f;
                ScaleBarVertexesRight[1].PositionRhw.W = 1;
                ScaleBarVertexesRight[1].Color = 0xFFFF8030;

                ScaleBarVertexesRight[2].PositionRhw.X = 21;
                ScaleBarVertexesRight[2].PositionRhw.Y = barBottom;
                ScaleBarVertexesRight[2].PositionRhw.Z = 0.5f;
                ScaleBarVertexesRight[2].PositionRhw.W = 1;
                ScaleBarVertexesRight[2].Color = 0x00FF8030;

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


        public UserEventCallbackDelegate UserEventCallback;

        internal int DirectXWidth = 1280;
        internal int DirectXHeight = 1024;

        internal bool YAxisCentered = true;

        private bool DirectXAvailable = false;

        private Direct3D Direct3D;
        private PresentParameters PresentParameters;
        internal Texture LastWaterfallTexture;
        internal Texture TempWaterfallTexture;
        internal Device Device;
        internal Sprite Sprite;
        internal Surface DefaultRenderTarget;
        internal SlimDX.Direct3D9.Font DisplayFont;

        /* add some interface to set the YAxisLines - for now external code is locking and modifying member variables */
        public readonly Mutex DirectXLock = new Mutex();

        private Vertex[][] PlotVerts = new Vertex[0][];
        private Vertex[] XAxisVerts = new Vertex[0];
        private Vertex[] YAxisVerts = new Vertex[0];
        private int PlotVertsCount = 0;
        private int PlotVertsWritePos = 0;
        private int PlotVertsReadPos = 0;


        protected Point[] LinePoints;
        public ArrayList YAxisLines = new ArrayList();
        public ArrayList YAxisNames = new ArrayList();

        internal bool UpdateOverlays = false;
        public bool LinePointsUpdated = true;
        public bool AxisUpdated = true;

        protected int LinePointEntries;
        private bool SizeHasChanged;

        internal bool ShiftPressed;
        internal bool AltPressed;
        internal bool ControlPressed;

        public eUserAction ActionMousePosX = eUserAction.XPos;
        public eUserAction ActionMousePosY = eUserAction.YPos;

        public eUserAction ActionMouseWheel = eUserAction.None;
        public eUserAction ActionMouseWheelShift = eUserAction.YOffset;
        public eUserAction ActionMouseWheelControl = eUserAction.YZoom;
        public eUserAction ActionMouseWheelAlt = eUserAction.None;

        public eUserAction ActionMouseDragX = eUserAction.None;
        public eUserAction ActionMouseDragXShift = eUserAction.None;
        public eUserAction ActionMouseDragXControl = eUserAction.None;
        public eUserAction ActionMouseDragXAlt = eUserAction.None;

        public eUserAction ActionMouseDragY = eUserAction.None;
        public eUserAction ActionMouseDragYShift = eUserAction.None;
        public eUserAction ActionMouseDragYControl = eUserAction.None;
        public eUserAction ActionMouseDragYAlt = eUserAction.None;


        /* values are in pixels and set by the DragX/Y functions */
        internal double DisplayXOffset = 0;
        internal double DisplayYOffset = 0;

        internal Point LastMousePos = new Point();

        /* distance of X Axis lines */
        public double XAxisUnit = 100;
        public double XAxisGridOffset = 0;
        public double XAxisSampleOffset = 0;
        public double XMaximum = 0;
        public int XAxisLines = 0;

        public double YZoomFactor { get; set; }
        public double XZoomFactor { get; set; }

        public Color ColorFG { get; set; }
        public Color ColorBG { get; set; }
        public Color ColorFont { get; set; }


        protected void CreateVertexBufferForPoints(Point[] points)
        {
            CreateVertexBufferForPoints(points, points.Length);
        }

        protected void CreateVertexBufferForPoints(Point[] points, int numPoints)
        {
            if (points == null)
                return;

            try
            {
                DirectXLock.WaitOne();

                if (Device != null)
                {
                    int color = ColorFG.ToArgb();

                    if (numPoints > 0)
                    {
                        if (DirectXHeight != PlotVerts.Length || numPoints != PlotVerts[PlotVertsWritePos].Length)
                        {
                            PlotVerts = new Vertex[DirectXHeight][];
                            for (int pos = 0; pos < PlotVerts.Length; pos++)
                                PlotVerts[pos] = new Vertex[numPoints];

                            PlotVertsWritePos = 0;
                            PlotVertsReadPos = 0;
                            PlotVertsCount = 0;
                        }

                        for (int pos = 0; pos < numPoints; pos++)
                        {
                            float dB = (float)(points[pos].Y);
                            double ampl = 1 - ((dB - LeveldBWhite) / (LeveldBBlack - LeveldBWhite));

                            ampl = Math.Max(0, ampl);
                            ampl = Math.Min(1, ampl);

                            int colorCode = (int)(0xFF * ampl);

                            PlotVerts[PlotVertsWritePos][pos].PositionRhw.X = (float)(XAxisSampleOffset * XZoomFactor - DisplayXOffset + points[pos].X * XZoomFactor);
                            PlotVerts[PlotVertsWritePos][pos].PositionRhw.Y = 0;
                            PlotVerts[PlotVertsWritePos][pos].PositionRhw.Z = 0.5f;
                            PlotVerts[PlotVertsWritePos][pos].PositionRhw.W = 1;
                            PlotVerts[PlotVertsWritePos][pos].Color = (uint)(0xFF000000 + (colorCode << 8) + colorCode);
                        }

                        PlotVertsWritePos = (PlotVertsWritePos + 1) % PlotVerts.Length;
                        PlotVertsCount++;
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


        protected void CreateVertexBufferForAxis()
        {
            try
            {
                DirectXLock.WaitOne();

                if (Device != null)
                {
                    uint color1 = 0xFF101010;
                    uint color2 = 0xFF404040;
                    uint color3 = 0xFFFFFFFF;

                    if (YAxisCentered)
                    {
                        YAxisVerts = new Vertex[4 + YAxisLines.Count * 2];

                        YAxisVerts[0].PositionRhw.X = 0;
                        YAxisVerts[0].PositionRhw.Y = DirectXHeight / 2;
                        YAxisVerts[0].PositionRhw.Z = 0.5f;
                        YAxisVerts[0].PositionRhw.W = 1;
                        YAxisVerts[0].Color = color1;

                        YAxisVerts[1].PositionRhw.X = DirectXWidth / 2;
                        YAxisVerts[1].PositionRhw.Y = DirectXHeight / 2;
                        YAxisVerts[1].PositionRhw.Z = 0.5f;
                        YAxisVerts[1].PositionRhw.W = 1;
                        YAxisVerts[1].Color = color3;

                        YAxisVerts[2].PositionRhw.X = DirectXWidth / 2;
                        YAxisVerts[2].PositionRhw.Y = DirectXHeight / 2;
                        YAxisVerts[2].PositionRhw.Z = 0.5f;
                        YAxisVerts[2].PositionRhw.W = 1;
                        YAxisVerts[2].Color = color3;

                        YAxisVerts[3].PositionRhw.X = DirectXWidth;
                        YAxisVerts[3].PositionRhw.Y = DirectXHeight / 2;
                        YAxisVerts[3].PositionRhw.Z = 0.5f;
                        YAxisVerts[3].PositionRhw.W = 1;
                        YAxisVerts[3].Color = color1;

                        for (int pos = 0; pos < YAxisLines.Count; pos++)
                        {
                            double yPos = (double)YAxisLines[pos];

                            YAxisVerts[4 + pos * 2 + 0].PositionRhw.X = 0;
                            YAxisVerts[4 + pos * 2 + 0].PositionRhw.Y = (float)(DirectXHeight - (yPos * YZoomFactor * DirectXHeight)) / 2;
                            YAxisVerts[4 + pos * 2 + 0].PositionRhw.Z = 0.5f;
                            YAxisVerts[4 + pos * 2 + 0].PositionRhw.W = 1;
                            YAxisVerts[4 + pos * 2 + 0].Color = color2;

                            YAxisVerts[4 + pos * 2 + 1].PositionRhw.X = DirectXWidth;
                            YAxisVerts[4 + pos * 2 + 1].PositionRhw.Y = (float)(DirectXHeight - (yPos * YZoomFactor * DirectXHeight)) / 2;
                            YAxisVerts[4 + pos * 2 + 1].PositionRhw.Z = 0.5f;
                            YAxisVerts[4 + pos * 2 + 1].PositionRhw.W = 1;
                            YAxisVerts[4 + pos * 2 + 1].Color = color2;
                        }
                    }
                    else
                        YAxisVerts = new Vertex[0];


                    XAxisVerts = new Vertex[XAxisLines * 4];
                    for (int pos = 0; pos < XAxisLines; pos++)
                    {
                        float xPos = (float)(XAxisGridOffset * XZoomFactor - DisplayXOffset + (pos * XAxisUnit * XZoomFactor));

                        XAxisVerts[pos * 4 + 0].PositionRhw.X = xPos;
                        XAxisVerts[pos * 4 + 0].PositionRhw.Y = 0;
                        XAxisVerts[pos * 4 + 0].PositionRhw.Z = 0.5f;
                        XAxisVerts[pos * 4 + 0].PositionRhw.W = 1;
                        XAxisVerts[pos * 4 + 0].Color = color1;

                        XAxisVerts[pos * 4 + 1].PositionRhw.X = xPos;
                        XAxisVerts[pos * 4 + 1].PositionRhw.Y = DirectXHeight / 2;
                        XAxisVerts[pos * 4 + 1].PositionRhw.Z = 0.5f;
                        XAxisVerts[pos * 4 + 1].PositionRhw.W = 1;
                        XAxisVerts[pos * 4 + 1].Color = color2;

                        XAxisVerts[pos * 4 + 2].PositionRhw.X = xPos;
                        XAxisVerts[pos * 4 + 2].PositionRhw.Y = DirectXHeight / 2;
                        XAxisVerts[pos * 4 + 2].PositionRhw.Z = 0.5f;
                        XAxisVerts[pos * 4 + 2].PositionRhw.W = 1;
                        XAxisVerts[pos * 4 + 2].Color = color2;

                        XAxisVerts[pos * 4 + 3].PositionRhw.X = xPos;
                        XAxisVerts[pos * 4 + 3].PositionRhw.Y = DirectXHeight;
                        XAxisVerts[pos * 4 + 3].PositionRhw.Z = 0.5f;
                        XAxisVerts[pos * 4 + 3].PositionRhw.W = 1;
                        XAxisVerts[pos * 4 + 3].Color = color1;
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

        public delegate void ResetDirectXDelegate();

        protected void ResetDirectX()
        {
            ReleaseDirectX();
            InitializeDirectX();
        }

        protected void ReleaseDirectX()
        {
            DirectXLock.WaitOne();

            DirectXAvailable = false;

            if (DisplayFont != null)
                DisplayFont.Dispose();

            if (Device != null)
                Device.Dispose();

            if (Direct3D != null)
                Direct3D.Dispose();

            if (Sprite != null)
                Sprite.Dispose();

            if (TempWaterfallTexture != null)
                TempWaterfallTexture.Dispose();

            if (LastWaterfallTexture != null)
                LastWaterfallTexture.Dispose();

            if (DefaultRenderTarget != null)
                DefaultRenderTarget.Dispose();

            if (SmallFont != null)
                SmallFont.Dispose();


            Direct3D = null;
            Device = null;
            DefaultRenderTarget = null;
            DisplayFont = null;
            SmallFont = null;
            Sprite = null;
            LastWaterfallTexture = null;
            TempWaterfallTexture = null;

            DirectXLock.ReleaseMutex();
        }


        protected void InitializeDirectX()
        {
            try
            {
                DirectXLock.WaitOne();

                DirectXHeight = Height;
                DirectXWidth = Width;

                /* deciding between soft and hard initialization */
                if (Direct3D == null)
                {
                    Direct3D = new Direct3D();

                    /* we dont need to allocate that all the time. once is enough */
                    if (PresentParameters == null)
                    {
                        PresentParameters = new PresentParameters();
                        PresentParameters.BackBufferHeight = DirectXHeight;
                        PresentParameters.BackBufferWidth = DirectXWidth;
                        PresentParameters.DeviceWindowHandle = Handle;
                        PresentParameters.BackBufferFormat = Format.A8R8G8B8;
                    }

                    Device = new Device(Direct3D, 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing, PresentParameters);
                    Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                    Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                    Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

                    DefaultRenderTarget = Device.GetRenderTarget(0);

                    DisplayFont = new Font(Device, new System.Drawing.Font("Arial", 20));
                    SmallFont = new Font(Device, new System.Drawing.Font("Arial", 8));

                    LastWaterfallTexture = new Texture(Device, PresentParameters.BackBufferWidth, PresentParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                    TempWaterfallTexture = new Texture(Device, PresentParameters.BackBufferWidth, PresentParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                    Sprite = new Sprite(Device);

                    AllocateResources();
                }
                else
                {
                    PresentParameters.BackBufferHeight = DirectXHeight;
                    PresentParameters.BackBufferWidth = DirectXWidth;

                    DisplayFont.Dispose();
                    SmallFont.Dispose();
                    Sprite.Dispose();
                    TempWaterfallTexture.Dispose();
                    LastWaterfallTexture.Dispose();
                    DefaultRenderTarget.Dispose();

                    ReleaseResources();

                    Device.Reset(PresentParameters);
                    Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                    Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                    Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

                    DefaultRenderTarget = Device.GetRenderTarget(0);

                    DisplayFont = new Font(Device, new System.Drawing.Font("Arial", 20));
                    SmallFont = new Font(Device, new System.Drawing.Font("Arial", 8));

                    LastWaterfallTexture = new Texture(Device, PresentParameters.BackBufferWidth, PresentParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                    TempWaterfallTexture = new Texture(Device, PresentParameters.BackBufferWidth, PresentParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                    Sprite = new Sprite(Device);

                    AllocateResources();
                }

                DirectXAvailable = true;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to initialize DirectX", e);
            }
            finally
            {
                DirectXLock.ReleaseMutex();
            }

            return;
        }

        internal virtual void AllocateResources()
        {
        }

        internal virtual void ReleaseResources()
        {
        }


        protected void Render()
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
                    CreateVertexBufferForPoints(LinePoints, LinePointEntries);
                }

                Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
                Device.SetRenderTarget(0, TempWaterfallTexture.GetSurfaceLevel(0));
                Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
                Device.BeginScene();

                for (int line = 0; line < PlotVertsCount; line++)
                {
                    /* clear temp buffer */
                    Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);

                    /* draw the old waterfall image */
                    Sprite.Begin(SpriteFlags.None);
                    Sprite.Draw(LastWaterfallTexture, Vector3.Zero, new Vector3(0, 1, 0), new Color4(Color.White));
                    Sprite.End();

                    /* paint first line */
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, PlotVerts[PlotVertsReadPos].Length - 1, PlotVerts[PlotVertsReadPos]);

                    /* now write the temp buffer into the real image buffer */
                    Device.SetRenderTarget(0, LastWaterfallTexture.GetSurfaceLevel(0));

                    Sprite.Begin(SpriteFlags.None);
                    Sprite.Draw(TempWaterfallTexture, new Color4(Color.White));
                    Sprite.End();

                    PlotVertsReadPos = (PlotVertsReadPos + 1) % PlotVerts.Length;
                }

                PlotVertsCount = 0;
                Device.EndScene();


                /* paint overlay */
                Device.SetRenderTarget(0, TempWaterfallTexture.GetSurfaceLevel(0));
                Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1.0f, 0);

                Device.BeginScene();

                if (XAxisVerts.Length > 0)
                    Device.DrawUserPrimitives(PrimitiveType.LineList, XAxisVerts.Length / 2, XAxisVerts);
                if (YAxisVerts.Length > 0)
                    Device.DrawUserPrimitives(PrimitiveType.LineList, YAxisVerts.Length / 2, YAxisVerts);

                DisplayFont.DrawString(null, Name, 20, 30, ColorBG);
                DisplayFont.DrawString(null, Name, 21, 31, ColorFont);

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

                int loops = 10;
                while (!DirectXAvailable && loops-- > 0)
                {
                    Thread.Sleep(250);
                    BeginInvoke(new ResetDirectXDelegate(ResetDirectX), null);
                    Thread.Sleep(750);
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


        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((e.KeyData & System.Windows.Forms.Keys.Shift) != 0)
                ShiftPressed = true;
            if ((e.KeyData & System.Windows.Forms.Keys.Alt) != 0)
                AltPressed = true;
            if ((e.KeyData & System.Windows.Forms.Keys.Control) != 0)
                ControlPressed = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if ((e.KeyData & System.Windows.Forms.Keys.Shift) == 0)
                ShiftPressed = false;
            if ((e.KeyData & System.Windows.Forms.Keys.Alt) == 0)
                AltPressed = false;
            if ((e.KeyData & System.Windows.Forms.Keys.Control) == 0)
                ControlPressed = false;
        }

        public void ProcessUserEvent(eUserEvent evt, double delta)
        {
            eUserAction action = eUserAction.None;

            switch (evt)
            {
                case eUserEvent.MousePosX:
                    action = ActionMousePosX;
                    break;
                case eUserEvent.MousePosY:
                    action = ActionMousePosY;
                    break;

                case eUserEvent.MouseWheel:
                    action = ActionMouseWheel;
                    break;
                case eUserEvent.MouseWheelShift:
                    action = ActionMouseWheelShift;
                    break;
                case eUserEvent.MouseWheelControl:
                    action = ActionMouseWheelControl;
                    break;
                case eUserEvent.MouseWheelAlt:
                    action = ActionMouseWheelAlt;
                    break;

                case eUserEvent.MouseDragX:
                    action = ActionMouseDragX;
                    break;
                case eUserEvent.MouseDragXShift:
                    action = ActionMouseDragXShift;
                    break;
                case eUserEvent.MouseDragXControl:
                    action = ActionMouseDragXControl;
                    break;
                case eUserEvent.MouseDragXAlt:
                    action = ActionMouseDragXAlt;
                    break;

                case eUserEvent.MouseDragY:
                    action = ActionMouseDragY;
                    break;
                case eUserEvent.MouseDragYShift:
                    action = ActionMouseDragYShift;
                    break;
                case eUserEvent.MouseDragYControl:
                    action = ActionMouseDragYControl;
                    break;
                case eUserEvent.MouseDragYAlt:
                    action = ActionMouseDragYAlt;
                    break;
            }

            if (action == eUserAction.UserCallback)
                UserEventCallback(evt, delta);
            else
                ProcessUserAction(action, delta);
        }

        public void ProcessUserAction(eUserAction action, double delta)
        {
            if (delta == 0)
                return;

            switch (action)
            {
                case eUserAction.XPos:
                    LastMousePos.X = delta;
                    break;

                case eUserAction.YPos:
                    LastMousePos.Y = delta;
                    break;

                case eUserAction.YOffset:
                    LeveldBWhite = Math.Max(LeveldBBlack, Math.Min(0, LeveldBWhite + 2 * Math.Sign(delta)));
                    AxisUpdated = true;
                    break;

                case eUserAction.YZoom:
                    LeveldBBlack = Math.Max(LeveldBMax, Math.Min(LeveldBWhite, LeveldBBlack + 2 * Math.Sign(delta)));
                    AxisUpdated = true;
                    break;
            }
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                double xDelta = LastMousePos.X - e.X;
                double yDelta = LastMousePos.Y - e.Y;

                if (AltPressed)
                {
                    ProcessUserEvent(eUserEvent.MouseDragXAlt, xDelta);
                    ProcessUserEvent(eUserEvent.MouseDragYAlt, yDelta);
                }
                else if (ControlPressed)
                {
                    ProcessUserEvent(eUserEvent.MouseDragXControl, xDelta);
                    ProcessUserEvent(eUserEvent.MouseDragYControl, yDelta);
                }
                else if (ShiftPressed)
                {
                    ProcessUserEvent(eUserEvent.MouseDragXShift, xDelta);
                    ProcessUserEvent(eUserEvent.MouseDragYShift, yDelta);
                }
                else
                {
                    ProcessUserEvent(eUserEvent.MouseDragX, xDelta);
                    ProcessUserEvent(eUserEvent.MouseDragY, yDelta);
                }
            }

            ProcessUserEvent(eUserEvent.MousePosX, e.X);
            ProcessUserEvent(eUserEvent.MousePosY, e.Y);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (AltPressed)
                ProcessUserEvent(eUserEvent.MouseWheelAlt, e.Delta);
            else if (ControlPressed)
                ProcessUserEvent(eUserEvent.MouseWheelControl, e.Delta);
            else if (ShiftPressed)
                ProcessUserEvent(eUserEvent.MouseWheelShift, e.Delta);
            else
                ProcessUserEvent(eUserEvent.MouseWheel, e.Delta);

        }

        protected override void OnSizeChanged(EventArgs e)
        {
            InitializeDirectX();
            LinePointsUpdated = true;
            AxisUpdated = true;
        }

        protected override void OnResize(EventArgs e)
        {
            LinePointsUpdated = true;
            AxisUpdated = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
        }
    }
}