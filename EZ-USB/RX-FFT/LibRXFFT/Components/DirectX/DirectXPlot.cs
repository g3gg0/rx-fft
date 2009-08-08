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

namespace LibRXFFT.Components.DirectX
{

    public class DirectXPlot : UserControl
    {
        public double UpdateRate
        {
            get { return 1000 / RenderSleepDelay; }
            set { RenderSleepDelay = 1000 / value; }
        }
        public double RenderSleepDelay = 1000 / 25;
        public DirectXPlot SlavePlot = null;

        public string MainText = "";
        public string MainTextPrev = "";
 
        public UserEventCallbackDelegate UserEventCallback;

        protected int DirectXWidth = 1280;
        protected int DirectXHeight = 1024;

        protected bool YAxisCentered = true;

        protected bool DirectXAvailable = false;

        protected Direct3D Direct3D;
        protected PresentParameters PresentParameters;

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
        protected Vertex[] OverviewVertexes = new Vertex[14];
        

        protected Point[] LinePoints;
        protected Object LinePointsLock = new Object();
        public ArrayList YAxisLines = new ArrayList();
        public ArrayList YAxisNames = new ArrayList();

        protected bool UpdateOverlays = false;
        public bool LinePointsUpdated = true;
        public bool AxisUpdated = true;

        protected int LinePointEntries;
        private bool SizeHasChanged;

        protected bool ShiftPressed;
        protected bool AltPressed;
        protected bool ControlPressed;

        protected bool OverviewMode;
        protected bool OverviewModeEnabled = true;

        public eUserAction ActionMousePosX = eUserAction.XPos;
        public eUserAction ActionMousePosY = eUserAction.YPos;

        public eUserAction ActionMouseWheelUp = eUserAction.YZoomIn;
        public eUserAction ActionMouseWheelUpShift = eUserAction.XZoomIn;
        public eUserAction ActionMouseWheelUpControl = eUserAction.None;
        public eUserAction ActionMouseWheelUpAlt = eUserAction.None;

        public eUserAction ActionMouseWheelDown = eUserAction.YZoomOut;
        public eUserAction ActionMouseWheelDownShift = eUserAction.XZoomOut;
        public eUserAction ActionMouseWheelDownControl = eUserAction.None;
        public eUserAction ActionMouseWheelDownAlt = eUserAction.None;

        public eUserAction ActionMouseDragX = eUserAction.XOffset;
        public eUserAction ActionMouseDragXShift = eUserAction.XOffsetOverview;
        public eUserAction ActionMouseDragXControl = eUserAction.XOffset;
        public eUserAction ActionMouseDragXAlt = eUserAction.XOffset;

        public eUserAction ActionMouseDragY = eUserAction.YOffset;
        public eUserAction ActionMouseDragYShift = eUserAction.None;
        public eUserAction ActionMouseDragYControl = eUserAction.None;
        public eUserAction ActionMouseDragYAlt = eUserAction.None;

        /* mouse click actions */
        public eUserAction ActionMouseClickLeft = eUserAction.None;
        public eUserAction ActionMouseClickLeftShift = eUserAction.None;
        public eUserAction ActionMouseClickLeftControl = eUserAction.None;
        public eUserAction ActionMouseClickLeftAlt = eUserAction.None;
        public eUserAction ActionMouseClickMiddle = eUserAction.None;
        public eUserAction ActionMouseClickMiddleShift = eUserAction.None;
        public eUserAction ActionMouseClickMiddleControl = eUserAction.None;
        public eUserAction ActionMouseClickMiddleAlt = eUserAction.None;
        public eUserAction ActionMouseClickRight = eUserAction.None;
        public eUserAction ActionMouseClickRightShift = eUserAction.None;
        public eUserAction ActionMouseClickRightControl = eUserAction.None;
        public eUserAction ActionMouseClickRightAlt = eUserAction.None;

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

        public Color ColorOverview { get; set; }
        public Color ColorFont { get; set; }
        public Color ColorFG { get; set; }
        public Color ColorBG { get; set; }
        public Color ColorCursor { get; set; }


        public DirectXPlot() : this(false)
        { 
        }

        public DirectXPlot(bool slaveMode)
        {
            ColorOverview = Color.Red;
            ColorFont = Color.DarkCyan;
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;
            ColorCursor = Color.Red;

            try
            {
                InitializeDirectX();
            }
            catch (Direct3D9Exception e)
            {
                MessageBox.Show("Failed initializing DirectX." + Environment.NewLine + e.ToString());
            }
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

            if (DisplayFont != null)
                DisplayFont.Dispose();

            if (FixedFont != null)
                FixedFont.Dispose();

            if (SmallFont != null)
                SmallFont.Dispose();

            if (Device != null)
                Device.Dispose();

            if (Direct3D != null)
                Direct3D.Dispose();

            DisplayFont = null;
            Device = null;
            Direct3D = null;
            SmallFont = null;
            FixedFont = null;

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
                        PresentParameters.Multisample = MultisampleType.TwoSamples;
                    }

                    Device = new Device(Direct3D, 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing, PresentParameters);

                    Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                    Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                    Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

                    DisplayFont = new Font(Device, new System.Drawing.Font("Arial", 20));
                    SmallFont = new Font(Device, new System.Drawing.Font("Arial", 8));
                    FixedFont = new Font(Device, new System.Drawing.Font("Courier Newo", 8));

                    AllocateResources();
                }
                else
                {
                    PresentParameters.BackBufferHeight = DirectXHeight;
                    PresentParameters.BackBufferWidth = DirectXWidth;

                    DisplayFont.Dispose();
                    SmallFont.Dispose();
                    ReleaseResources();
                    Device.Reset(PresentParameters);
                    Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                    Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                    Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

                    DisplayFont = new Font(Device, new System.Drawing.Font("Arial", 20));
                    SmallFont = new Font(Device, new System.Drawing.Font("Arial", 8));
                    FixedFont = new Font(Device, new System.Drawing.Font("Courier Newo", 8));

                    AllocateResources();
                }

                DirectXAvailable = true;
                LinePointsUpdated = true;
                AxisUpdated = true;

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

        protected virtual void AllocateResources()
        {
        }

        protected virtual void ReleaseResources()
        {
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
                int pos = 0;
                uint colorOverview = ((uint)ColorOverview.ToArgb()) & 0xFFFFFF;

                /* draw left margin */
                OverviewVertexes[pos].PositionRhw.X = (float)(DisplayXOffset / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = 0;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000;
                pos++;

                OverviewVertexes[pos].PositionRhw.X = (float)(DisplayXOffset / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000 | colorOverview;
                pos++;

                OverviewVertexes[pos].PositionRhw.X = (float)(DisplayXOffset / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000 | colorOverview;
                pos++;

                OverviewVertexes[pos].PositionRhw.X = (float)(DisplayXOffset / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000;
                pos++;

                /* draw right margin */
                OverviewVertexes[pos].PositionRhw.X = (float)((DisplayXOffset + DirectXWidth) / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = 0;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000;
                pos++;

                OverviewVertexes[pos].PositionRhw.X = (float)((DisplayXOffset + DirectXWidth) / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000 | colorOverview;
                pos++;

                OverviewVertexes[pos].PositionRhw.X = (float)((DisplayXOffset + DirectXWidth) / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000 | colorOverview;
                pos++;

                OverviewVertexes[pos].PositionRhw.X = (float)((DisplayXOffset + DirectXWidth) / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000;
                pos++;

                /* horizonal line */
                OverviewVertexes[pos].PositionRhw.X = (float)(DisplayXOffset / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000 | colorOverview;
                pos++;

                OverviewVertexes[pos].PositionRhw.X = (float)((DisplayXOffset + DirectXWidth) / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000 | colorOverview;
                pos++;

                /* draw center line */
                OverviewVertexes[pos].PositionRhw.X = (float)((DisplayXOffset + DirectXWidth / 2) / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight / 4;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000;
                pos++;

                OverviewVertexes[pos].PositionRhw.X = (float)((DisplayXOffset + DirectXWidth / 2) / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000 | colorOverview;
                pos++;

                OverviewVertexes[pos].PositionRhw.X = (float)((DisplayXOffset + DirectXWidth / 2) / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = DirectXHeight / 2;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000 | colorOverview;
                pos++;

                OverviewVertexes[pos].PositionRhw.X = (float)((DisplayXOffset + DirectXWidth / 2) / XZoomFactor);
                OverviewVertexes[pos].PositionRhw.Y = 3 * DirectXHeight / 4;
                OverviewVertexes[pos].PositionRhw.Z = 0.5f;
                OverviewVertexes[pos].PositionRhw.W = 1;
                OverviewVertexes[pos].Color = 0x9F000000;
                pos++;
            }

            int startPos = (int)(DisplayXOffset / XZoomFactor);
            int endPos = (int)(((DisplayXOffset + DirectXWidth) / XZoomFactor));
            int centerPos = (int)((startPos + endPos) / 2);

            Device.DrawUserPrimitives(PrimitiveType.LineList, 7, OverviewVertexes);
            SmallFont.DrawString(null, XLabelFromSampleNum(startPos), startPos + 10, 60, ColorFont.ToArgb());
            SmallFont.DrawString(null, XLabelFromSampleNum(endPos), endPos + 10, 60, ColorFont.ToArgb());
            SmallFont.DrawString(null, XLabelFromSampleNum(centerPos), centerPos, 60, ColorFont.ToArgb());
        }

        internal virtual void PrepareLinePoints()
        {
        }

        internal virtual void Render()
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

                Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
                Device.BeginScene();
                Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;

                if (XAxisVerts.Length > 0)
                    Device.DrawUserPrimitives(PrimitiveType.LineList, XAxisVerts.Length / 2, XAxisVerts);
                if (YAxisVerts.Length > 0)
                    Device.DrawUserPrimitives(PrimitiveType.LineList, YAxisVerts.Length / 2, YAxisVerts);
                if (PlotVertsEntries > 0)
                {
                    if (ShiftPressed)
                        RenderOverview(PlotVertsEntries, PlotVertsOverview);
                    else
                        Device.DrawUserPrimitives(PrimitiveType.LineStrip, PlotVertsEntries, PlotVerts);
                }

                DisplayFont.DrawString(null, MainText, 20, 30, ColorBG);
                DisplayFont.DrawString(null, MainText, 21, 31, ColorFont);

                RenderOverlay();

                Device.EndScene();
                Device.Present();

                UpdateOverlays = false;

            }
            catch (Direct3D9Exception e)
            {
                DirectXAvailable = false;
                DirectXLock.ReleaseMutex();

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

        protected virtual void RenderOverlay()
        {
        }

        protected virtual void KeyPressed(Keys key)
        {
        }

        protected virtual void KeyReleased(Keys key)
        {
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
                    MainTextPrev = MainText;
                    MainText = "Zoom Selection";
                }
            }

            //LinePointsUpdated = true;
            //AxisUpdated = true;
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
                    OverviewMode = false;
                    MainText = MainTextPrev;
                    MainTextPrev = "";
                }
            }

            //LinePointsUpdated = true;
            //AxisUpdated = true;
        }

        public void ProcessUserEvent(eUserEvent evt, double param)
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

                case eUserEvent.MouseWheelUp:
                    action = ActionMouseWheelUp;
                    break;
                case eUserEvent.MouseWheelUpShift:
                    action = ActionMouseWheelUpShift;
                    break;
                case eUserEvent.MouseWheelUpControl:
                    action = ActionMouseWheelUpControl;
                    break;
                case eUserEvent.MouseWheelUpAlt:
                    action = ActionMouseWheelUpAlt;
                    break;

                case eUserEvent.MouseWheelDown:
                    action = ActionMouseWheelDown;
                    break;
                case eUserEvent.MouseWheelDownShift:
                    action = ActionMouseWheelDownShift;
                    break;
                case eUserEvent.MouseWheelDownControl:
                    action = ActionMouseWheelDownControl;
                    break;
                case eUserEvent.MouseWheelDownAlt:
                    action = ActionMouseWheelDownAlt;
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

                case eUserEvent.MouseClickLeft:
                    action = ActionMouseClickLeft;
                    break;
                case eUserEvent.MouseClickLeftShift:
                    action = ActionMouseClickLeftShift;
                    break;
                case eUserEvent.MouseClickLeftControl:
                    action = ActionMouseClickLeftControl;
                    break;
                case eUserEvent.MouseClickLeftAlt:
                    action = ActionMouseClickLeftAlt;
                    break;

                case eUserEvent.MouseClickMiddle:
                    action = ActionMouseClickMiddle;
                    break;
                case eUserEvent.MouseClickMiddleShift:
                    action = ActionMouseClickMiddleShift;
                    break;
                case eUserEvent.MouseClickMiddleControl:
                    action = ActionMouseClickMiddleControl;
                    break;
                case eUserEvent.MouseClickMiddleAlt:
                    action = ActionMouseClickMiddleAlt;
                    break;

                case eUserEvent.MouseClickRight:
                    action = ActionMouseClickRight;
                    break;
                case eUserEvent.MouseClickRightShift:
                    action = ActionMouseClickRightShift;
                    break;
                case eUserEvent.MouseClickRightControl:
                    action = ActionMouseClickRightControl;
                    break;
                case eUserEvent.MouseClickRightAlt:
                    action = ActionMouseClickRightAlt;
                    break;
            }

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
                    UpdateOverlays = true;
                    LastMousePos.X = param;
                    break;

                case eUserAction.YPos:
                    UpdateOverlays = true;
                    LastMousePos.Y = param;
                    break;

                case eUserAction.XOffsetOverview:
                    oldOffset = DisplayXOffset;
                    delta1 = -param * XZoomFactor + DisplayXOffset;
                    delta2 = Math.Min(XMaximum * XZoomFactor - DirectXWidth, delta1);
                    DisplayXOffset = Math.Round(Math.Max(0, delta2));

                    if (oldOffset != DisplayXOffset)
                    {
                        LinePointsUpdated = true;
                        AxisUpdated = true;
                    }

                    break;
                    
                case eUserAction.XOffset:
                    oldOffset = DisplayXOffset;
                    delta1 = param + DisplayXOffset;
                    delta2 = Math.Min(XMaximum * XZoomFactor - DirectXWidth, delta1);
                    DisplayXOffset = Math.Max(0, delta2);

                    if (oldOffset != DisplayXOffset)
                    {
                        LinePointsUpdated = true;
                        AxisUpdated = true;
                    }

                    break;

                case eUserAction.YOffset:
                    oldOffset = DisplayYOffset;

                    param += DisplayYOffset;
                    param = Math.Min(DirectXHeight * YZoomFactor, param);
                    DisplayYOffset = Math.Max(-DirectXHeight * YZoomFactor, param);

                    if (oldOffset != DisplayYOffset)
                    {
                        LinePointsUpdated = true;
                        AxisUpdated = true;
                    }

                    break;

                case eUserAction.XZoomIn:
                    if (XZoomFactor < 20.0f)
                    {
                        DisplayXOffset = (DisplayXOffset + LastMousePos.X) * XZoomStep - LastMousePos.X;
                        XZoomFactor *= XZoomStep;

                        /* call ourselves again for min/max fitting */
                        ProcessUserAction(eUserAction.XOffset, 0);

                        LinePointsUpdated = true;
                        AxisUpdated = true;
                    }

                    if (XMaximum * XZoomFactor < DirectXWidth)
                        XZoomFactor = DirectXWidth / XMaximum;

                    break;

                case eUserAction.XZoomOut:
                    if (XZoomFactor > 0.01f)
                    {
                        DisplayXOffset = (DisplayXOffset + LastMousePos.X) / XZoomStep - LastMousePos.X;
                        XZoomFactor /= XZoomStep;

                        /* call ourselves again for min/max fitting */
                        ProcessUserAction(eUserAction.XOffset, 0);

                        LinePointsUpdated = true;
                        AxisUpdated = true;
                    }

                    if (XMaximum * XZoomFactor < DirectXWidth)
                        XZoomFactor = DirectXWidth / XMaximum;

                    break;

                case eUserAction.YZoomIn:
                    if (YZoomFactor < 50.0f)
                    {
                        YZoomFactor *= YZoomStep;

                        /* call ourselves again for min/max fitting */
                        ProcessUserAction(eUserAction.YOffset, 0);

                        LinePointsUpdated = true;
                        AxisUpdated = true;
                    }

                    break;

                case eUserAction.YZoomOut:
                    if (YZoomFactor > 0.001f)
                    {
                        YZoomFactor /= YZoomStep;

                        /* call ourselves again for min/max fitting */
                        ProcessUserAction(eUserAction.YOffset, 0);

                        LinePointsUpdated = true;
                        AxisUpdated = true;
                    }

                    break;
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
