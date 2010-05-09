using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SlimDX.Direct3D9;
using Font = SlimDX.Direct3D9.Font;
//using Mutex = LibRXFFT.Libraries.Misc.TraceMutex;
using LibRXFFT.Components.DirectX.Drawables;
using LibRXFFT.Components.DirectX.Drawables.Docks;
using RX_FFT.Components.GDI;
using LibRXFFT.Libraries.Misc;


namespace LibRXFFT.Components.DirectX
{
    public class DirectXPlot : UserControl
    {
        public double UpdateRate
        {
            get { return 1000 / RenderSleepDelay; }
            set { RenderSleepDelay = 1000 / value; }
        }

        public double DefaultRefreshRate = 60;
        public double MinRefreshRate = 60;
        public double RenderSleepDelay;

        public DirectXPlot SlavePlot = null;

        protected bool NeedsRender = true;
        public int NeedsRenderClients = 0;
        public bool KeepText = false;
        public string MainText = "";
        public string MainTextPrev = "";

        public UserEventCallbackDelegate UserEventCallback;

        public int DirectXWidth = 1280;
        public int DirectXHeight = 1024;

        protected bool YAxisCentered = true;

        protected int DirectXResetTries = 0;
        protected int DirectXResetTriesMax = 500;
        protected bool DirectXAvailable = false;

        protected Direct3D Direct3D;
        protected PresentParameters PresentParameters;
        protected virtual MultisampleType SuggestedMultisample { get { return MultisampleType.TwoSamples; } }


        protected int DefaultAdapter;
        protected Capabilities DeviceCaps;
        protected CreateFlags DeviceCreateFlags;
        public Device Device;
        protected Font DisplayFont;
        protected Font SmallFont;
        protected Font FixedFont;
        protected System.Windows.Forms.Timer ResizeTimer = new System.Windows.Forms.Timer();


        protected System.Drawing.Font DisplayFontSource = new System.Drawing.Font("Arial", 20);
        protected System.Drawing.Font SmallFontSource = new System.Drawing.Font("Arial", 8);
        protected System.Drawing.Font FixedFontSource = new System.Drawing.Font("Courier New", 8);


        /* add some interface to set the YAxisLines - for now external code is locking and modifying member variables */
        public readonly Mutex DirectXLock = new Mutex();

        protected int PlotVertsEntries = 0;

        protected Vertex[] PlotVerts = new Vertex[0];
        protected Vertex[] PlotVertsOverview = new Vertex[0];
        protected Vertex[] XAxisVerts = new Vertex[0];
        protected Vertex[] YAxisVerts = new Vertex[0];
        protected Vertex[] OverviewVertexes = new Vertex[100];
        protected int OverviewVertexCount = 0;

        public LinkedList<LabelledLine> LabelledVertLines = new LinkedList<LabelledLine>();
        public LinkedList<LabelledLine> LabelledHorLines = new LinkedList<LabelledLine>();
        public LinkedList<StringLabel> OverlayTextLabels = new LinkedList<StringLabel>();


        protected Point[] LinePoints;
        protected Object LinePointsLock = new Object();
        protected bool LinePointsUpdated = true;

        public ArrayList YAxisLines = new ArrayList();
        public ArrayList YAxisNames = new ArrayList();

        public bool UpdateOverlays = false;
        public bool UpdateAxis = true;
        public bool UpdateCursor = true;

        protected int LinePointEntries;
        private bool SizeHasChanged;

        protected bool Dragging;
        protected bool ShiftPressed;
        protected bool AltPressed;
        protected bool ControlPressed;
        public bool MouseHovering;
        public bool ShowVerticalCursor;
        public bool HideCursor;

        protected bool OverviewMode;
        protected bool OverviewModeEnabled = true;

        public Dictionary<eUserEvent, eUserAction> EventActions = new Dictionary<eUserEvent, eUserAction>();


        /* values are in pixels and set by the DragX/Y functions */
        public double DisplayXOffset = 0;
        public double DisplayYOffset = 0;

        protected Point LastMousePos = new Point();

        /* distance of X Axis lines */
        public double XAxisUnit = 100;
        public double XAxisGridOffset = 0;
        public double XAxisSampleOffset = 0;
        public double XMaximum = 0;
        public int XAxisLines = 0;

        public double XZoomStep = 1.05f;
        public double YZoomStep = 1.05f;

        public double YZoomFactor { get; set; }
        public double XZoomFactor { get; set; }
        public double YZoomFactorMax { get; set; }
        public double YZoomFactorMin { get; set; }
        public double XZoomFactorMax { get; set; }
        public double XZoomFactorMin { get; set; }

        public Color ColorOverview { get; set; }
        public Color ColorFont { get; set; }
        public Color ColorFG { get; set; }
        public Color ColorBG { get; set; }
        public Color ColorCursor { get; set; }

        private Cursor EmptyCursor;
        private Cursor DefaultCursor;

        protected LinkedList<DirectXDrawable> Drawables = new LinkedList<DirectXDrawable>();
        private InputEvent DrawableInputEvent = new InputEvent();

        public DirectXPlot()
            : this(false)
        {
        }

        public DirectXPlot(bool slaveMode)
        {
            SlimDX.Configuration.EnableObjectTracking = true;
            SlimDX.Configuration.DetectDoubleDispose = true;

            SetDefaultActions();

            ColorOverview = Color.Red;
            ColorFont = Color.DarkCyan;
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;
            ColorCursor = Color.Red;

            YZoomFactorMax = 50;
            YZoomFactorMin = 0.01d;
            XZoomFactorMax = 20;
            XZoomFactorMin = 1;

            DefaultCursor = this.Cursor;
            EmptyCursor = CreateEmptyCursor();

            RenderSleepDelay = 1000 / DefaultRefreshRate;

            try
            {
                InitializeDirectX();
            }
            catch (Direct3D9Exception e)
            {
                MessageBox.Show("Failed initializing DirectX." + Environment.NewLine + e.ToString());
            }
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseResources();
            ReleaseDevices();
            ReleaseDirectX();

            base.Dispose(disposing);
        }

        public string[] DisplayInformation
        {
            get
            {
                ArrayList lines = new ArrayList();

                lines.Add("Name:              " + Direct3D.Adapters.DefaultAdapter.Details.DeviceName);
                lines.Add("Driver:            " + Direct3D.Adapters.DefaultAdapter.Details.DriverName);
                lines.Add("MaxTextureHeight:  " + DeviceCaps.MaxTextureHeight);
                lines.Add("MaxTextureWidth:   " + DeviceCaps.MaxTextureWidth);
                lines.Add("Multisample:       " + PresentParameters.Multisample);
                lines.Add("DeviceCaps:        " + DeviceCaps.DeviceCaps);

                return (string[])lines.ToArray(typeof(string));
            }
        }

        protected Cursor CreateEmptyCursor()
        {
            Bitmap b = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(b);
            IntPtr ptr = b.GetHicon();

            return new Cursor(ptr);
        }

        protected void SetDefaultActions()
        {
            EventActions[eUserEvent.MousePosX] = eUserAction.XPos;
            EventActions[eUserEvent.MousePosY] = eUserAction.YPos;

            EventActions[eUserEvent.MouseWheelUp] = eUserAction.YZoomIn;
            EventActions[eUserEvent.MouseWheelUpShift] = eUserAction.XZoomIn;

            EventActions[eUserEvent.MouseWheelDown] = eUserAction.YZoomOut;
            EventActions[eUserEvent.MouseWheelDownShift] = eUserAction.XZoomOut;

            EventActions[eUserEvent.MouseDragX] = eUserAction.XOffset;
            EventActions[eUserEvent.MouseDragXShift] = eUserAction.XOffsetOverview;
            EventActions[eUserEvent.MouseDragXControl] = eUserAction.XOffset;
            EventActions[eUserEvent.MouseDragXAlt] = eUserAction.XOffset;

            EventActions[eUserEvent.MouseDragY] = eUserAction.YOffset;
        }

        protected void CreateVertexBufferForPoints(Point[] points)
        {
            CreateVertexBufferForPoints(points, points.Length);
        }

        protected virtual void CreateVertexBufferForPoints(Point[] points, int numPoints)
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
                        PlotVerts = new Vertex[numPoints];
                        PlotVertsOverview = new Vertex[numPoints];
                    }

                    PlotVertsEntries = numPoints - 1;

                    double maximum = 0;
                    if (YAxisCentered)
                    {
                        for (int pos = 0; pos < numPoints; pos++)
                        {
                            PlotVerts[pos].PositionRhw.X = (float)Math.Min(DirectXWidth, Math.Max(0, ((XAxisSampleOffset + points[pos].X) * XZoomFactor - DisplayXOffset)));
                            PlotVerts[pos].PositionRhw.Y = (float)Math.Min(DirectXHeight, Math.Max(0, (DirectXHeight - (DisplayYOffset + points[pos].Y * YZoomFactor)) / 2));
                            PlotVerts[pos].PositionRhw.Z = 0.5f;
                            PlotVerts[pos].PositionRhw.W = 1;
                            PlotVerts[pos].Color = 0xFF000000 | colorFG;

                            if (OverviewModeEnabled)
                            {
                                PlotVertsOverview[pos].PositionRhw.X = (float)Math.Min(DirectXWidth, Math.Max(0, (XAxisSampleOffset + points[pos].X)));
                                PlotVertsOverview[pos].PositionRhw.Y = PlotVerts[pos].PositionRhw.Y;
                                PlotVertsOverview[pos].PositionRhw.Z = PlotVerts[pos].PositionRhw.Z;
                                PlotVertsOverview[pos].PositionRhw.W = PlotVerts[pos].PositionRhw.W;
                                PlotVertsOverview[pos].Color = PlotVerts[pos].Color;
                            }

                            maximum = (int)Math.Max(points[pos].X, maximum);
                        }
                    }
                    else
                    {
                        for (int pos = 0; pos < numPoints; pos++)
                        {
                            PlotVerts[pos].PositionRhw.X = (float)((XAxisSampleOffset + points[pos].X) * XZoomFactor - DisplayXOffset);
                            PlotVerts[pos].PositionRhw.Y = (float)(DirectXHeight - (DisplayYOffset + points[pos].Y * YZoomFactor));
                            PlotVerts[pos].PositionRhw.Z = 0.5f;
                            PlotVerts[pos].PositionRhw.W = 1;
                            PlotVerts[pos].Color = 0xFF000000 | colorFG;

                            if (OverviewModeEnabled)
                            {
                                PlotVertsOverview[pos].PositionRhw.X = (float)(XAxisSampleOffset + points[pos].X);
                                PlotVertsOverview[pos].PositionRhw.Y = PlotVerts[pos].PositionRhw.Y;
                                PlotVertsOverview[pos].PositionRhw.Z = PlotVerts[pos].PositionRhw.Z;
                                PlotVertsOverview[pos].PositionRhw.W = PlotVerts[pos].PositionRhw.W;
                                PlotVertsOverview[pos].Color = PlotVerts[pos].Color;
                            }

                            maximum = (int)Math.Max(points[pos].X, maximum);
                        }
                    }

                    XMaximum = maximum;
                }
            }
            catch (Exception e)
            {
                return;
            }
        }



        protected void ResetDirectX()
        {
            ReleaseDirectX();
            try
            {
                InitializeDirectX();
            }
            catch (Direct3D9Exception e)
            {
            }
        }

        protected void ReleaseDirectX()
        {
            DirectXLock.WaitOne();

            DirectXAvailable = false;

            /* release resources */
            ReleaseResources();

            /* release devices */
            ReleaseDevices();

            /* release Direct3D */
            if (Direct3D != null)
                Direct3D.Dispose();
            Direct3D = null;

            DirectXLock.ReleaseMutex();
        }


        protected void InitializeDirectX()
        {
            try
            {
                DirectXLock.WaitOne();

                DirectXResetTries = 0;
                DirectXHeight = Height;
                DirectXWidth = Width;

                if (DeviceCaps != null)
                {
                    DirectXHeight = Math.Min(DirectXHeight, DeviceCaps.MaxTextureHeight);
                    DirectXWidth = Math.Min(DirectXWidth, DeviceCaps.MaxTextureWidth);
                }

                /* deciding between soft and hard initialization */
                if (Direct3D == null)
                {
                    /* Direct3D init */
                    Direct3D = new Direct3D();

                    /* we dont need to allocate that all the time. once is enough */
                    AllocateDevices();

                    /* ressource allocations */
                    AllocateResources();

                    /* set up the window resize timer */
                    ResizeTimer.Interval = 50;
                    ResizeTimer.Tick += (object sender, EventArgs ev) =>
                    {
                        try
                        {
                            ResizeTimer.Stop();
                            InitializeDirectX();
                        }
                        catch (Direct3D9Exception ex)
                        {
                        }
                        OnSizeChanged(null);
                    };
                }
                else
                {
                    /* release resources */
                    ReleaseResources();

                    /* reset device */
                    ResetDevices();

                    /* reallocate resources */
                    AllocateResources();
                }

                DirectXAvailable = true;
                LinePointsUpdated = true;
                UpdateAxis = true;
                UpdateCursor = true;
                UpdateOverlays = true;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                DirectXLock.ReleaseMutex();
            }

            return;
        }



        protected virtual void AllocateDevices()
        {
            DefaultAdapter = Direct3D.Adapters.DefaultAdapter.Adapter;
            DeviceCaps = Direct3D.GetDeviceCaps(DefaultAdapter, DeviceType.Hardware);

            DeviceCreateFlags = CreateFlags.Multithreaded;

            DirectXHeight = Math.Min(DirectXHeight, DeviceCaps.MaxTextureHeight);
            DirectXWidth = Math.Min(DirectXWidth, DeviceCaps.MaxTextureWidth);


            // supports hardware vertex processing?
            if ((DeviceCaps.DeviceCaps & SlimDX.Direct3D9.DeviceCaps.HWTransformAndLight) != 0)
            {
                DeviceCreateFlags |= CreateFlags.HardwareVertexProcessing;
            }
            else
            {
                DeviceCreateFlags |= CreateFlags.SoftwareVertexProcessing;
            }

            if (PresentParameters == null)
            {
                PresentParameters = new PresentParameters();
                PresentParameters.BackBufferHeight = DirectXHeight;
                PresentParameters.BackBufferWidth = DirectXWidth;
                PresentParameters.DeviceWindowHandle = Handle;
                PresentParameters.Multisample = MultisampleType.None;
                PresentParameters.BackBufferFormat = Format.A8R8G8B8;

                if (Direct3D.CheckDeviceMultisampleType(DefaultAdapter, DeviceType.Hardware, PresentParameters.BackBufferFormat, true, SuggestedMultisample))
                {
                    PresentParameters.Multisample = SuggestedMultisample;
                }
            }

            Device = new Device(Direct3D, DefaultAdapter, DeviceType.Hardware, PresentParameters.DeviceWindowHandle, DeviceCreateFlags, PresentParameters);
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);

            Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        }

        protected virtual void ResetDevices()
        {
            PresentParameters.BackBufferHeight = DirectXHeight;
            PresentParameters.BackBufferWidth = DirectXWidth;
            Device.Reset(PresentParameters);
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
            Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        }

        protected virtual void ReleaseDevices()
        {
            if (Device != null)
                Device.Dispose();
            Device = null;
        }

        protected virtual void AllocateResources()
        {
            DisplayFont = new Font(Device, DisplayFontSource);
            SmallFont = new Font(Device, SmallFontSource);
            FixedFont = new Font(Device, FixedFontSource);

            lock (Drawables)
            {
                try
                {
                    foreach (DirectXDrawable drawable in Drawables)
                    {
                        drawable.AllocateResources();
                    }
                }
                catch (Exception e)
                {
                    Log.AddMessage("Drawable Exception: " + e);
                }
            }
        }

        protected virtual void ReleaseResources()
        {
            if (DisplayFont != null)
                DisplayFont.Dispose();
            if (SmallFont != null)
                SmallFont.Dispose();
            if (FixedFont != null)
                FixedFont.Dispose();

            DisplayFont = null;
            SmallFont = null;
            FixedFont = null;

            lock (Drawables)
            {
                try
                {
                    foreach (DirectXDrawable drawable in Drawables)
                    {
                        drawable.ReleaseResources();
                    }
                }
                catch (Exception e)
                {
                    Log.AddMessage("Drawable Exception: " + e);
                }
            }
        }

        protected virtual string XLabelFromCursorPos(double xPos)
        {
            double offset = ((DisplayXOffset + xPos) / (XZoomFactor)) - XAxisSampleOffset;
            return "Sample " + offset.ToString();
        }

        protected virtual string XLabelFromSampleNum(double num)
        {
            return "Sample " + num.ToString();
        }

        protected virtual void RenderOverview(int vertexCount, Vertex[] vertexes)
        {
            if (vertexCount > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineStrip, vertexCount, vertexes);

            /* only recalc scale lines when axis need to get updated */
            if (UpdateOverlays)
            {
                uint colorOverview = ((uint)ColorOverview.ToArgb()) & 0xFFFFFF;
                double leftMargin = DisplayXOffset / XZoomFactor;
                double rightMargin = (DisplayXOffset + DirectXWidth) / XZoomFactor;
                double center = (DisplayXOffset + DirectXWidth / 2) / XZoomFactor;

                OverviewVertexCount = 0;

                /* draw left margin */
                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)leftMargin;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = 0;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000;
                OverviewVertexCount++;

                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)leftMargin;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000 | colorOverview;
                OverviewVertexCount++;

                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)leftMargin;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000 | colorOverview;
                OverviewVertexCount++;

                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)leftMargin;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000;
                OverviewVertexCount++;

                /* draw right margin */
                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)rightMargin;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = 0;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000;
                OverviewVertexCount++;

                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)rightMargin;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000 | colorOverview;
                OverviewVertexCount++;

                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)rightMargin;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000 | colorOverview;
                OverviewVertexCount++;

                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)rightMargin;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000;
                OverviewVertexCount++;

                /* horizonal line */
                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)leftMargin;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000 | colorOverview;
                OverviewVertexCount++;

                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)rightMargin;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000 | colorOverview;
                OverviewVertexCount++;

                /* draw center line */
                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)center;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight / 4;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000;
                OverviewVertexCount++;

                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)center;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000 | colorOverview;
                OverviewVertexCount++;

                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)center;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000 | colorOverview;
                OverviewVertexCount++;

                OverviewVertexes[OverviewVertexCount].PositionRhw.X = (float)center;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Y = 3 * DirectXHeight / 4;
                OverviewVertexes[OverviewVertexCount].PositionRhw.Z = 0.5f;
                OverviewVertexes[OverviewVertexCount].PositionRhw.W = 1;
                OverviewVertexes[OverviewVertexCount].Color = 0x9F000000;
                OverviewVertexCount++;
            }

            int startPos = (int)(DisplayXOffset / XZoomFactor);
            int endPos = (int)(((DisplayXOffset + DirectXWidth) / XZoomFactor));
            int centerPos = (int)((startPos + endPos) / 2);

            if (OverviewVertexCount > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineList, OverviewVertexCount / 2, OverviewVertexes);
            SmallFont.DrawString(null, XLabelFromSampleNum(startPos), startPos + 10, 40, ColorFont.ToArgb());
            SmallFont.DrawString(null, XLabelFromSampleNum(centerPos), centerPos, 50, ColorFont.ToArgb());
            SmallFont.DrawString(null, XLabelFromSampleNum(endPos), endPos + 10, 40, ColorFont.ToArgb());
        }

        public virtual void PrepareLinePoints()
        {
        }


        protected virtual void RenderCore()
        {
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
            Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;

            RenderAxis();
            RenderPlotVerts();
            RenderOverlay();
            RenderCursor();

            DisplayFont.DrawString(null, MainText, 20, 30, ColorBG);
            DisplayFont.DrawString(null, MainText, 21, 31, ColorFont);
        }


        public virtual void Render()
        {
            if (!DirectXAvailable)
            {
                if (++DirectXResetTries >= DirectXResetTriesMax)
                {
                    if (!DirectXAvailable)
                    {
                        if (MessageBox.Show("Failed to re-init DirectX within " + (DirectXResetTriesMax / 10) + " seconds. Retry again?", "Reset DirectX failed", MessageBoxButtons.RetryCancel) != DialogResult.Retry)
                        {
                            throw new Exception("Failed to initialize DirectX");
                        }
                    }
                }
                try
                {
                    BeginInvoke((MethodInvoker)delegate()
                    {
                        ResetDirectX();
                    });
                }
                catch (Exception e)
                {
                }
                Thread.Sleep(100);

                return;
            }

            DirectXLock.WaitOne();
            try
            {
                Device.BeginScene();
                RenderCore();

                lock (Drawables)
                {
                    try
                    {
                        foreach (DirectXDrawable drawable in Drawables)
                        {
                            drawable.Render();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.AddMessage("Drawable Exception: " + e);
                    }
                }

                Device.EndScene();
                Device.Present();
            }
            catch (Direct3D9Exception e)
            {
                DirectXAvailable = false;
            }
            catch (Exception e)
            {
            }

            DirectXLock.ReleaseMutex();
        }

        protected virtual void RenderPlotVerts()
        {
            if (LinePointsUpdated)
            {
                LinePointsUpdated = false;
                lock (LinePointsLock)
                {
                    CreateVertexBufferForPoints(LinePoints, LinePointEntries);
                }
            }

            if (PlotVertsEntries > 0)
            {
                if (ShiftPressed)
                    RenderOverview(PlotVertsEntries, PlotVertsOverview);
                else
                {
                    if (PlotVertsEntries > 0)
                        Device.DrawUserPrimitives(PrimitiveType.LineStrip, PlotVertsEntries, PlotVerts);
                }
            }
        }

        protected virtual void RenderOverlay()
        {
        }

        protected virtual void RenderCursor()
        {
        }

        protected virtual void RenderAxis()
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
                        if (YAxisVerts == null || YAxisVerts.Length < (4 + YAxisLines.Count * 2))
                        {
                            YAxisVerts = new Vertex[4 + YAxisLines.Count * 2];
                        }

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
                    {
                        YAxisVerts = null;
                    }

                    if (XAxisVerts == null || XAxisVerts.Length < XAxisLines * 4)
                    {
                        XAxisVerts = new Vertex[XAxisLines * 4];
                    }

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

        protected virtual void KeyPressed(Keys key)
        {
            NeedsRender = true;
        }

        protected virtual void KeyReleased(Keys key)
        {
            NeedsRender = true;
        }

        public void ProcessUserEvent(eUserEvent evt, double param)
        {
            eUserAction action = eUserAction.None;

            if (EventActions.ContainsKey(evt))
                action = EventActions[evt];

            if (action == eUserAction.UserCallback)
            {
                if (UserEventCallback != null)
                {
                    UserEventCallback(evt, param);
                }
                else
                {
                    throw new Exception();
                }
            }
            else
            {
                ProcessUserAction(action, param);
            }
        }

        public virtual void ProcessUserAction(eUserAction action, double param)
        {
            double delta1 = 0;
            double delta2 = 0;
            double oldOffset = 0;

            switch (action)
            {
                case eUserAction.XPos:
                    UpdateCursor = true;
                    LastMousePos.X = param;
                    break;

                case eUserAction.YPos:
                    UpdateCursor = true;
                    LastMousePos.Y = param;
                    break;

                case eUserAction.XOffsetOverview:
                    oldOffset = DisplayXOffset;
                    delta1 = -param * XZoomFactor + DisplayXOffset;
                    delta2 = Math.Min(XMaximum * XZoomFactor - DirectXWidth, delta1);
                    DisplayXOffset = Math.Round(Math.Max(0, delta2));

                    if (oldOffset != DisplayXOffset)
                    {
                        UpdateAxis = true;
                        UpdateCursor = true;
                        UpdateOverlays = true;
                    }

                    break;

                case eUserAction.XOffset:
                    oldOffset = DisplayXOffset;

                    delta1 = param + oldOffset;
                    delta2 = Math.Min(XMaximum * XZoomFactor - DirectXWidth, delta1);
                    DisplayXOffset = Math.Max(0, delta2);

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

                case eUserAction.XZoomIn:
                    if (XZoomFactor < XZoomFactorMax)
                    {
                        DisplayXOffset = (DisplayXOffset + LastMousePos.X) * XZoomStep - LastMousePos.X;
                        XZoomFactor *= XZoomStep;

                        /* call ourselves again for min/max fitting */
                        ProcessUserAction(eUserAction.XOffset, 0);

                        UpdateAxis = true;
                        UpdateCursor = true;
                        UpdateOverlays = true;
                    }

                    if (XMaximum * XZoomFactor < DirectXWidth)
                        XZoomFactor = DirectXWidth / XMaximum;

                    break;

                case eUserAction.XZoomOut:
                    if (XZoomFactor > XZoomFactorMin)
                    {
                        //DisplayXOffset = (DisplayXOffset + LastMousePos.X) / XZoomStep - LastMousePos.X;
                        XZoomFactor /= XZoomStep;

                        /* call ourselves again for min/max fitting */
                        ProcessUserAction(eUserAction.XOffset, 0);

                        UpdateAxis = true;
                        UpdateCursor = true;
                        UpdateOverlays = true;
                    }

                    if (XMaximum * XZoomFactor < DirectXWidth)
                        XZoomFactor = DirectXWidth / XMaximum;

                    break;

                case eUserAction.YZoomIn:
                    if (YZoomFactor < YZoomFactorMax)
                    {
                        //DisplayYOffset = (LastMousePos.Y + DisplayYOffset) * YZoomStep - LastMousePos.Y;
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
            }
        }

        public void AddDrawable(DirectXDrawable directXDrawable)
        {
            lock (Drawables)
            {
                Drawables.AddLast(directXDrawable);
            }

            NeedsRender = true;
        }

        protected bool UpdateDrawablePositions()
        {
            lock (Drawables)
            {
                try
                {
                    foreach (DirectXDrawable drawable in Drawables)
                    {
                        drawable.UpdatePositions();
                    }
                }
                catch (Exception e)
                {
                }
            }

            return false;
        }

        protected bool HandleDrawableInput(InputEvent evt)
        {
            lock (Drawables)
            {
                try
                {
                    foreach (DirectXDrawable drawable in Drawables)
                    {
                        if (drawable.ProcessInputEvent(evt))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }

            return false;
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            /* check if drawable wants to handle input event */
            DrawableInputEvent.Type = eInputEventType.KeyDown;
            DrawableInputEvent.KeyData = e.KeyData;
            if (HandleDrawableInput(DrawableInputEvent))
            {
                return;
            }

            if ((e.KeyData & Keys.Shift) != 0)
            {
                if (!ShiftPressed)
                {
                    ShiftPressed = true;
                    KeyPressed(Keys.Shift);
                }
            }

            if ((e.KeyData & Keys.Alt) != 0)
            {
                if (!AltPressed)
                {
                    AltPressed = true;
                    KeyPressed(Keys.Alt);
                }
            }

            if ((e.KeyData & Keys.Control) != 0)
            {
                if (!ControlPressed)
                {
                    ControlPressed = true;
                    KeyPressed(Keys.Control);
                }
            }

            if (OverviewModeEnabled)
            {
                if (ShiftPressed && !OverviewMode)
                {
                    OverviewMode = true;
                    UpdateOverlays = true;
                    UpdateCursor = true;
                    MainTextPrev = MainText;
                    MainText = "Zoom Selection";
                }
            }

            NeedsRender = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            /* check if drawable wants to handle input event */
            DrawableInputEvent.Type = eInputEventType.KeyUp;
            DrawableInputEvent.KeyData = e.KeyData;
            if (HandleDrawableInput(DrawableInputEvent))
            {
                return;
            }

            if ((e.KeyData & Keys.Shift) == 0)
            {
                if (ShiftPressed)
                {
                    ShiftPressed = false;
                    KeyReleased(Keys.Shift);
                }
            }
            if ((e.KeyData & Keys.Alt) == 0)
            {
                if (AltPressed)
                {
                    AltPressed = false;
                    KeyReleased(Keys.Alt);
                }
            }
            if ((e.KeyData & Keys.Control) == 0)
            {
                if (ControlPressed)
                {
                    ControlPressed = false;
                    KeyReleased(Keys.Control);
                }
            }

            if (OverviewModeEnabled)
            {
                if (!ShiftPressed && OverviewMode)
                {
                    ResetModifiers(false);
                }
            }

            NeedsRender = true;
        }
        protected virtual void ResetModifiers(bool forceUnhover)
        {
            ResetModifiers(forceUnhover, false);
        }

        protected virtual void ResetModifiers(bool forceUnhover, bool keepText)
        {
            ShiftPressed = false;
            AltPressed = false;
            ControlPressed = false;
            OverviewMode = false;
            UpdateOverlays = true;
            UpdateCursor = true;
            MouseHovering = ClientRectangle.Contains(PointToClient(MousePosition));
            if (MouseHovering && !forceUnhover)
            {
                Cursor = EmptyCursor;
            }
            else
            {
                Cursor = DefaultCursor;
            }

            if (!keepText)
            {
                MainText = MainTextPrev;
                MainTextPrev = "";
            }

            if (UserEventCallback != null)
                UserEventCallback(eUserEvent.StatusUpdated, 0);
        }

        public void CursorType(bool standard)
        {
            if (standard)
            {
                HideCursor = true;
                Cursor = DefaultCursor;
            }
            else
            {
                HideCursor = false;
                Cursor = EmptyCursor;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            /* check if drawable wants to handle input event */
            DrawableInputEvent.Type = eInputEventType.MouseEnter;
            if (HandleDrawableInput(DrawableInputEvent))
            {
                return;
            }

            //Focus();
            ProcessUserEvent(eUserEvent.StatusUpdated, 0);
            Cursor = EmptyCursor;
            MouseHovering = true;
            UpdateCursor = true;
            ProcessUserEvent(eUserEvent.MouseEnter, 0);

            NeedsRender = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            /* check if drawable wants to handle input event */
            DrawableInputEvent.Type = eInputEventType.MouseLeave;
            if (HandleDrawableInput(DrawableInputEvent))
            {
                return;
            }

            ResetModifiers(true, true);
            UpdateCursor = true;
            ProcessUserEvent(eUserEvent.MouseLeave, 0);

            NeedsRender = true;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            /* check if drawable wants to handle input event */
            DrawableInputEvent.Type = eInputEventType.MouseDoubleClick;
            DrawableInputEvent.MouseButtons = e.Button;
            if (HandleDrawableInput(DrawableInputEvent))
            {
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                if (AltPressed)
                    ProcessUserEvent(eUserEvent.MouseDoubleClickRightAlt, 0);
                else if (ControlPressed)
                    ProcessUserEvent(eUserEvent.MouseDoubleClickRightControl, 0);
                else if (ShiftPressed)
                    ProcessUserEvent(eUserEvent.MouseDoubleClickRightShift, 0);
                else
                    ProcessUserEvent(eUserEvent.MouseDoubleClickRight, 0);
            }

            if (e.Button == MouseButtons.Left)
            {
                if (AltPressed)
                    ProcessUserEvent(eUserEvent.MouseDoubleClickLeftAlt, 0);
                else if (ControlPressed)
                    ProcessUserEvent(eUserEvent.MouseDoubleClickLeftControl, 0);
                else if (ShiftPressed)
                    ProcessUserEvent(eUserEvent.MouseDoubleClickLeftShift, 0);
                else
                    ProcessUserEvent(eUserEvent.MouseDoubleClickLeft, 0);
            }

            if (e.Button == MouseButtons.Middle)
            {
                if (AltPressed)
                    ProcessUserEvent(eUserEvent.MouseDoubleClickMiddleAlt, 0);
                else if (ControlPressed)
                    ProcessUserEvent(eUserEvent.MouseDoubleClickMiddleControl, 0);
                else if (ShiftPressed)
                    ProcessUserEvent(eUserEvent.MouseDoubleClickMiddleShift, 0);
                else
                    ProcessUserEvent(eUserEvent.MouseDoubleClickMiddle, 0);
            }

            NeedsRender = true;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            /* check if drawable wants to handle input event */
            DrawableInputEvent.Type = eInputEventType.MouseClick;
            DrawableInputEvent.MouseButtons = e.Button;
            if (HandleDrawableInput(DrawableInputEvent))
            {
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                if (AltPressed)
                    ProcessUserEvent(eUserEvent.MouseClickRightAlt, 0);
                else if (ControlPressed)
                    ProcessUserEvent(eUserEvent.MouseClickRightControl, 0);
                else if (ShiftPressed)
                    ProcessUserEvent(eUserEvent.MouseClickRightShift, 0);
                else
                    ProcessUserEvent(eUserEvent.MouseClickRight, 0);
            }

            if (e.Button == MouseButtons.Left)
            {
                if (AltPressed)
                    ProcessUserEvent(eUserEvent.MouseClickLeftAlt, 0);
                else if (ControlPressed)
                    ProcessUserEvent(eUserEvent.MouseClickLeftControl, 0);
                else if (ShiftPressed)
                    ProcessUserEvent(eUserEvent.MouseClickLeftShift, 0);
                else
                    ProcessUserEvent(eUserEvent.MouseClickLeft, 0);
            }

            if (e.Button == MouseButtons.Middle)
            {
                if (AltPressed)
                    ProcessUserEvent(eUserEvent.MouseClickMiddleAlt, 0);
                else if (ControlPressed)
                    ProcessUserEvent(eUserEvent.MouseClickMiddleControl, 0);
                else if (ShiftPressed)
                    ProcessUserEvent(eUserEvent.MouseClickMiddleShift, 0);
                else
                    ProcessUserEvent(eUserEvent.MouseClickMiddle, 0);
            }

            NeedsRender = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            /* check if drawable wants to handle input event */
            DrawableInputEvent.Type = eInputEventType.MouseButtonDown;
            DrawableInputEvent.MouseButtons = e.Button;
            if (HandleDrawableInput(DrawableInputEvent))
            {
                return;
            }

            if (AltPressed)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ProcessUserEvent(eUserEvent.MouseDownLeftAlt, 0);
                    Dragging = true;
                }
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseDownRightAlt, 0);
            }
            else if (ControlPressed)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ProcessUserEvent(eUserEvent.MouseDownLeftControl, 0);
                    Dragging = true;
                }
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseDownRightControl, 0);
            }
            else if (ShiftPressed)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ProcessUserEvent(eUserEvent.MouseDownLeftShift, 0);
                    Dragging = true;
                }
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseDownRightShift, 0);
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    ProcessUserEvent(eUserEvent.MouseDownLeft, 0);
                    Dragging = true;
                }
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseDownRight, 0);
            }

            NeedsRender = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            /* check if drawable wants to handle input event */
            DrawableInputEvent.Type = eInputEventType.MouseButtonUp;
            DrawableInputEvent.MouseButtons = e.Button;
            if (HandleDrawableInput(DrawableInputEvent))
            {
                return;
            }

            if (AltPressed)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ProcessUserEvent(eUserEvent.MouseUpLeftAlt, 0);
                    Dragging = false;
                }
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseUpRightAlt, 0);
            }
            else if (ControlPressed)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ProcessUserEvent(eUserEvent.MouseUpLeftControl, 0);
                    Dragging = false;
                }
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseUpRightControl, 0);
            }
            else if (ShiftPressed)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ProcessUserEvent(eUserEvent.MouseUpLeftShift, 0);
                    Dragging = false;
                }
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseUpRightShift, 0);
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    ProcessUserEvent(eUserEvent.MouseUpLeft, 0);
                    Dragging = false;
                }
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseUpRight, 0);
            }

            NeedsRender = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            /* check if drawable wants to handle input event */
            DrawableInputEvent.Type = eInputEventType.MouseMoved;
            DrawableInputEvent.MouseButtons = e.Button;
            DrawableInputEvent.MousePosition.X = e.X;
            DrawableInputEvent.MousePosition.Y = e.Y;
            if (HandleDrawableInput(DrawableInputEvent))
            {
                return;
            }

            if (e.Button == MouseButtons.Left && Dragging)
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

            NeedsRender = true;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            /* check if drawable wants to handle input event */
            DrawableInputEvent.Type = eInputEventType.MouseWheel;
            DrawableInputEvent.MouseWheelDelta = e.Delta;
            if (HandleDrawableInput(DrawableInputEvent))
            {
                return;
            }

            if (e.Delta > 0)
            {
                if (AltPressed)
                    ProcessUserEvent(eUserEvent.MouseWheelUpAlt, e.Delta);
                else if (ControlPressed)
                    ProcessUserEvent(eUserEvent.MouseWheelUpControl, e.Delta);
                else if (ShiftPressed)
                    ProcessUserEvent(eUserEvent.MouseWheelUpShift, e.Delta);
                else
                    ProcessUserEvent(eUserEvent.MouseWheelUp, e.Delta);
            }
            else
            {
                if (AltPressed)
                    ProcessUserEvent(eUserEvent.MouseWheelDownAlt, e.Delta);
                else if (ControlPressed)
                    ProcessUserEvent(eUserEvent.MouseWheelDownControl, e.Delta);
                else if (ShiftPressed)
                    ProcessUserEvent(eUserEvent.MouseWheelDownShift, e.Delta);
                else
                    ProcessUserEvent(eUserEvent.MouseWheelDown, e.Delta);
            }

            NeedsRender = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (DirectXHeight != Height || DirectXWidth != Width)
            {
                ResizeTimer.Stop();
                ResizeTimer.Start();
            }
            NeedsRender = true;
        }

        protected override void OnResize(EventArgs e)
        {
            NeedsRender = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            NeedsRender = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            NeedsRender = true;
        }

        private Dictionary<object, bool> NeedRenderObjects = new Dictionary<object, bool>();
        public void NeedRender(object client, bool state)
        {
            lock (NeedRenderObjects)
            {
                if (!state && NeedRenderObjects.ContainsKey(client))
                {
                    NeedRenderObjects.Remove(client);
                }

                if (state && !NeedRenderObjects.ContainsKey(client))
                {
                    NeedRenderObjects.Add(client, state);
                }

                NeedsRenderClients = NeedRenderObjects.Count;
            }
        }

        protected void ScreenRefreshTimer_Func(object sender, EventArgs e)
        {
            if (NeedsRender || NeedsRenderClients > 0)
            {
                try
                {
                    NeedsRender = false;

                    if (SlavePlot != null)
                        SlavePlot.Render();

                    Render();
                }
                catch (Exception ex)
                {
                    Log.AddMessage(ex.ToString());
                }
            }
        }
    }
}
