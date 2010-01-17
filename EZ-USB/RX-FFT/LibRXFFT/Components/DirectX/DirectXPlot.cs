using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;
using Device = SlimDX.Direct3D9.Device;
using Font = SlimDX.Direct3D9.Font;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Components.DirectX
{

    public class DirectXPlot : UserControl
    {
        public double UpdateRate
        {
            get { return 1000 / RenderSleepDelay; }
            set { RenderSleepDelay = 1000 / value; }
        }
        public double RenderSleepDelay = 1000 / 60;
        public DirectXPlot SlavePlot = null;

        public bool KeepText = false;
        public string MainText = "";
        public string MainTextPrev = "";
 
        public UserEventCallbackDelegate UserEventCallback;

        protected int DirectXWidth = 1280;
        protected int DirectXHeight = 1024;

        protected bool YAxisCentered = true;

        protected int DirectXResetTries = 0;
        protected int DirectXResetTriesMax = 500;
        protected bool DirectXAvailable = false;

        protected Direct3D Direct3D;
        protected PresentParameters PresentParameters;

        protected int DefaultAdapter;
        protected Capabilities DeviceCaps;
        protected CreateFlags DeviceCreateFlags;
        protected Device Device;
        protected Font DisplayFont;
        protected Font SmallFont;
        protected Font FixedFont;

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

        protected bool ShiftPressed;
        protected bool AltPressed;
        protected bool ControlPressed;
        public bool MouseHovering;
        public bool ShowVerticalCursor;

        protected bool OverviewMode;
        protected bool OverviewModeEnabled = true;

        public Dictionary<eUserEvent, eUserAction> EventActions = new Dictionary<eUserEvent, eUserAction>();


        /* values are in pixels and set by the DragX/Y functions */
        protected double DisplayXOffset = 0;
        protected double DisplayYOffset = 0;

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

        public DirectXPlot() : this(false)
        { 
        }

        public DirectXPlot(bool slaveMode)
        {
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

            try
            {
                InitializeDirectX();
            }
            catch (Direct3D9Exception e)
            {
                MessageBox.Show("Failed initializing DirectX." + Environment.NewLine + e.ToString());
            }
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

                        double maximum = 0;
                        if (YAxisCentered)
                        {
                            for (int pos = 0; pos < numPoints; pos++)
                            {
                                PlotVerts[pos].PositionRhw.X = (float)((XAxisSampleOffset + points[pos].X) * XZoomFactor - DisplayXOffset);
                                PlotVerts[pos].PositionRhw.Y = (float)(DirectXHeight - (DisplayYOffset + points[pos].Y * YZoomFactor)) / 2;
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
                PresentParameters.Multisample = MultisampleType.TwoSamples;
                PresentParameters.BackBufferFormat = Format.A8R8G8B8;

                if (Direct3D.CheckDeviceMultisampleType(DefaultAdapter, DeviceType.Hardware, PresentParameters.BackBufferFormat, true, MultisampleType.TwoSamples))
                {
                    PresentParameters.Multisample = MultisampleType.TwoSamples;
                }
                else
                {
                    PresentParameters.Multisample = MultisampleType.None;
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
            DisplayFont = new Font(Device, new System.Drawing.Font("Arial", 20));
            SmallFont = new Font(Device, new System.Drawing.Font("Arial", 8));
            FixedFont = new Font(Device, new System.Drawing.Font("Courier New", 8));
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

            Device.DrawUserPrimitives(PrimitiveType.LineList, OverviewVertexCount/2, OverviewVertexes);
            SmallFont.DrawString(null, XLabelFromSampleNum(startPos), startPos + 10, 40, ColorFont.ToArgb());
            SmallFont.DrawString(null, XLabelFromSampleNum(centerPos), centerPos, 50, ColorFont.ToArgb());
            SmallFont.DrawString(null, XLabelFromSampleNum(endPos), endPos + 10, 40, ColorFont.ToArgb());
        }

        internal virtual void PrepareLinePoints()
        {
        }


        protected virtual void RenderCore()
        {
            Device.BeginScene();
            
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
            Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
            
            RenderAxis();
            RenderPlotVerts();
            RenderOverlay();
            RenderCursor();

            DisplayFont.DrawString(null, MainText, 20, 30, ColorBG);
            DisplayFont.DrawString(null, MainText, 21, 31, ColorFont);
            
            Device.EndScene();
            Device.Present();
        }


        internal virtual void Render()
        {

            if (!DirectXAvailable)
            {
                if (++DirectXResetTries >= DirectXResetTriesMax)
                {
                    if (!DirectXAvailable)
                    {
                        MessageBox.Show("Failed to re-init DirectX within " + (DirectXResetTriesMax /10) + " seconds.");
                        Thread.Sleep(1000);
                    }
                }
                try
                {
                    BeginInvoke(new ResetDirectXDelegate(ResetDirectX), null);
                }
                catch (Exception e)
                {
                }
                Thread.Sleep(100);

                return;
            }

            try
            {
                DirectXLock.WaitOne();
                RenderCore();
                DirectXLock.ReleaseMutex();
            }
            catch (Direct3D9Exception e)
            {
                DirectXAvailable = false;
                DirectXLock.ReleaseMutex();
            }
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
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, PlotVertsEntries, PlotVerts);
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

        protected virtual void KeyPressed(Keys key)
        {
        }

        protected virtual void KeyReleased(Keys key)
        {
        }



        public void ProcessUserEvent(eUserEvent evt, double param)
        {
            eUserAction action = eUserAction.None;

            if (EventActions.ContainsKey(evt))
                action = EventActions[evt];

            if (action == eUserAction.UserCallback)
                UserEventCallback(evt, param);
            else
                ProcessUserAction(action, param);
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
                        DisplayXOffset = (DisplayXOffset + LastMousePos.X) / XZoomStep - LastMousePos.X;
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
            }
        }



        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((e.KeyData & System.Windows.Forms.Keys.Shift) != 0)
            {
                if (!ShiftPressed)
                {
                    ShiftPressed = true;
                    KeyPressed(Keys.Shift);
                }
            }

            if ((e.KeyData & System.Windows.Forms.Keys.Alt) != 0)
            {
                if (!AltPressed)
                {
                    AltPressed = true;
                    KeyPressed(Keys.Alt);
                }
            }

            if ((e.KeyData & System.Windows.Forms.Keys.Control) != 0)
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
        }


        protected override void OnKeyUp(KeyEventArgs e)
        {
            if ((e.KeyData & System.Windows.Forms.Keys.Shift) == 0)
            {
                if (ShiftPressed)
                {
                    ShiftPressed = false;
                    KeyReleased(Keys.Shift);
                }
            }
            if ((e.KeyData & System.Windows.Forms.Keys.Alt) == 0)
            {
                if (AltPressed)
                {
                    AltPressed = false;
                    KeyReleased(Keys.Alt);
                }
            }
            if ((e.KeyData & System.Windows.Forms.Keys.Control) == 0)
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
            MouseHovering = ClientRectangle.Contains(PointToClient(Control.MousePosition));
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

        protected override void OnMouseEnter(EventArgs e)
        {
            //Focus();
            ProcessUserEvent(eUserEvent.StatusUpdated, 0);
            Cursor = EmptyCursor;
            MouseHovering = true;
            UpdateCursor = true;
            ProcessUserEvent(eUserEvent.MouseEnter, 0);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            ResetModifiers(true, true);
            UpdateCursor = true;
            ProcessUserEvent(eUserEvent.MouseLeave, 0);            
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
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
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
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
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (AltPressed)
            {
                if (e.Button == MouseButtons.Left)
                    ProcessUserEvent(eUserEvent.MouseDownLeftAlt, 0);
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseDownRightAlt, 0);
            }
            else if (ControlPressed)
            {
                if (e.Button == MouseButtons.Left)
                    ProcessUserEvent(eUserEvent.MouseDownLeftControl, 0);
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseDownRightControl, 0);
            }
            else if (ShiftPressed)
            {
                if (e.Button == MouseButtons.Left)
                    ProcessUserEvent(eUserEvent.MouseDownLeftShift, 0);
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseDownRightShift, 0);
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                    ProcessUserEvent(eUserEvent.MouseDownLeft, 0);
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseDownRight, 0);
            }             
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (AltPressed)
            {
                if (e.Button == MouseButtons.Left)
                    ProcessUserEvent(eUserEvent.MouseUpLeftAlt, 0);
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseUpRightAlt, 0);
            }
            else if (ControlPressed)
            {
                if (e.Button == MouseButtons.Left)
                    ProcessUserEvent(eUserEvent.MouseUpLeftControl, 0);
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseUpRightControl, 0);
            }
            else if (ShiftPressed)
            {
                if (e.Button == MouseButtons.Left)
                    ProcessUserEvent(eUserEvent.MouseUpLeftShift, 0);
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseUpRightShift, 0);
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                    ProcessUserEvent(eUserEvent.MouseUpLeft, 0);
                if (e.Button == MouseButtons.Right)
                    ProcessUserEvent(eUserEvent.MouseUpRight, 0);
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

        }

        protected override void OnSizeChanged(EventArgs e)
        {
            try
            {
                InitializeDirectX();
            }
            catch (Direct3D9Exception ex)
            {
            }
        }

        protected override void OnResize(EventArgs e)
        {
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
        }
    }
}
