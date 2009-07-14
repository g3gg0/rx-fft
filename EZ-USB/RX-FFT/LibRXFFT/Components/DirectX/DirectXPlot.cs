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
        public enum eUserAction
        {
            None,
            YZoom,
            XZoom,
            YOffset,
            XOffset,
            UserCallback
        }

        public enum eUserEvent
        {
            None,
            MouseDragX,
            MouseDragXShift,
            MouseDragXAlt,
            MouseDragXControl,
            MouseDragY,
            MouseDragYShift,
            MouseDragYAlt,
            MouseDragYControl,
            MouseWheel,
            MouseWheelShift,
            MouseWheelAlt,
            MouseWheelControl,
            RenderOverlay
        }

        public struct Point
        {
            public double X;
            public double Y;

            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Vertex
        {
            public Vector4 PositionRhw;
            public long Color;
        }

        public delegate void UserEventCallbackDelegate(eUserEvent evt, double delta);
        public UserEventCallbackDelegate UserEventCallback;

        internal int DirectXWidth = 1280;
        internal int DirectXHeight = 1024;

        internal bool YAxisCentered = true;

        private bool DirectXAvailable = false;

        private Direct3D Direct3D;
        private PresentParameters PresentParameters;

        internal Device Device;
        internal Font DisplayFont;

        /* add some interface to set the YAxisLines - for now external code is locking and modifying member variables */
        public readonly Mutex DirectXLock = new Mutex();

        private int PlotVertsEntries = 0;

        private Vertex[] PlotVerts;
        private Vertex[] XAxisVerts;
        private Vertex[] YAxisVerts;

        protected Point[] LinePoints;
        public ArrayList YAxisLines = new ArrayList();
        public ArrayList YAxisNames = new ArrayList();

        public bool DataUpdated;
        public bool AxisUpdated;

        protected int LinePointEntries;
        private bool SizeHasChanged;

        internal bool ShiftPressed;
        internal bool AltPressed;
        internal bool ControlPressed;

        public eUserAction ActionMouseWheel = eUserAction.YZoom;
        public eUserAction ActionMouseWheelShift = eUserAction.XZoom;
        public eUserAction ActionMouseWheelControl = eUserAction.None;
        public eUserAction ActionMouseWheelAlt = eUserAction.None;

        public eUserAction ActionMouseDragX = eUserAction.XOffset;
        public eUserAction ActionMouseDragXShift = eUserAction.XOffset;
        public eUserAction ActionMouseDragXControl = eUserAction.XOffset;
        public eUserAction ActionMouseDragXAlt = eUserAction.XOffset;

        public eUserAction ActionMouseDragY = eUserAction.YOffset;
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


        public DirectXPlot()
        {
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;

            InitializeDirectX();
        }

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
                        if (numPoints > PlotVerts.Length)
                            PlotVerts = new Vertex[numPoints];

                        PlotVertsEntries = numPoints - 1;

                        double maximum = 0;
                        if (YAxisCentered)
                        {
                            for (int pos = 0; pos < numPoints; pos++)
                            {
                                PlotVerts[pos].PositionRhw.X = (float)(XAxisSampleOffset * XZoomFactor - DisplayXOffset + points[pos].X * XZoomFactor);
                                PlotVerts[pos].PositionRhw.Y = (float)(DirectXHeight - (DisplayYOffset + points[pos].Y * YZoomFactor)) / 2;
                                PlotVerts[pos].PositionRhw.Z = 0.5f;
                                PlotVerts[pos].PositionRhw.W = 1;
                                PlotVerts[pos].Color = color;

                                maximum = (int)Math.Max(points[pos].X, maximum);
                            }
                        }
                        else
                        {
                            for (int pos = 0; pos < numPoints; pos++)
                            {
                                PlotVerts[pos].PositionRhw.X = (float)(XAxisSampleOffset * XZoomFactor - DisplayXOffset + points[pos].X * XZoomFactor);
                                PlotVerts[pos].PositionRhw.Y = (float)(DirectXHeight - (DisplayYOffset + points[pos].Y * YZoomFactor));
                                PlotVerts[pos].PositionRhw.Z = 0.5f;
                                PlotVerts[pos].PositionRhw.W = 1;
                                PlotVerts[pos].Color = color;

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
                    int color1 = 0x7F101010;
                    int color2 = 0x7F404040;
                    int color3 = 0x7FFFFFFF;

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

            DisplayFont = null;
            Device = null;
            Direct3D = null;

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
                    }

                    if (PlotVerts == null)
                        PlotVerts = new Vertex[0];
                    if (XAxisVerts == null)
                        XAxisVerts = new Vertex[0];
                    if (YAxisVerts == null)
                        YAxisVerts = new Vertex[0];

                    Device = new Device(Direct3D, 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing, PresentParameters);
                    DisplayFont = new Font(Device, new System.Drawing.Font("Arial", 20));

                    AllocateResources();
                }
                else
                {
                    PresentParameters.BackBufferHeight = DirectXHeight;
                    PresentParameters.BackBufferWidth = DirectXWidth;

                    DisplayFont.Dispose();
                    ReleaseResources();
                    Device.Reset(PresentParameters);
                    DisplayFont = new Font(Device, new System.Drawing.Font("Arial", 20));
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
                if (AxisUpdated)
                {
                    AxisUpdated = false;
                    CreateVertexBufferForAxis();
                }

                if (DataUpdated)
                {
                    CreateVertexBufferForPoints(LinePoints, LinePointEntries);
                    DataUpdated = false;
                }

                DirectXLock.WaitOne();

                Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
                Device.BeginScene();
                Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;

                if (XAxisVerts.Length > 0)
                    Device.DrawUserPrimitives(PrimitiveType.LineList, XAxisVerts.Length / 2, XAxisVerts);
                if (YAxisVerts.Length > 0)
                    Device.DrawUserPrimitives(PrimitiveType.LineList, YAxisVerts.Length / 2, YAxisVerts);
                if (PlotVerts.Length > 0)
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, PlotVertsEntries, PlotVerts);

                DisplayFont.DrawString(null, Name, 20, 30, 0x7F00FFFF);

                RenderOverlay();

                Device.EndScene();
                Device.Present();
                DirectXLock.ReleaseMutex();
            }
            catch (Direct3D9Exception e)
            {
                DirectXLock.ReleaseMutex();
                DirectXAvailable = false;

                int loops = 10;
                while (!DirectXAvailable && loops-- > 0)
                {
                    Thread.Sleep(50);
                    BeginInvoke(new ResetDirectXDelegate(ResetDirectX), null);
                    Thread.Sleep(450);
                }

                if (!DirectXAvailable)
                    MessageBox.Show("Failed to re-init DirectX ater 5 seconds");
            }
            catch (Exception e)
            {
                DirectXLock.ReleaseMutex();
            }
        }

        internal virtual void RenderOverlay()
        {
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((e.KeyData & System.Windows.Forms.Keys.Shift) != 0)
                ShiftPressed = true;
            if ((e.KeyData & System.Windows.Forms.Keys.Alt) != 0)
                AltPressed = true;
            if ((e.KeyData & System.Windows.Forms.Keys.Control) != 0)
                ControlPressed = true;

            DataUpdated = true;
            AxisUpdated = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if ((e.KeyData & System.Windows.Forms.Keys.Shift) == 0)
                ShiftPressed = false;
            if ((e.KeyData & System.Windows.Forms.Keys.Alt) == 0)
                AltPressed = false;
            if ((e.KeyData & System.Windows.Forms.Keys.Control) == 0)
                ControlPressed = false;

            DataUpdated = true;
            AxisUpdated = true;
        }

        public void ProcessUserEvent(eUserEvent evt, double delta)
        {
            eUserAction action = eUserAction.None;

            switch (evt)
            {
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
                case eUserAction.XOffset:
                    double delta1 = delta + DisplayXOffset;
                    double delta2 = Math.Min(XMaximum * XZoomFactor - DirectXWidth, delta1);
                    DisplayXOffset = Math.Max(0, delta2);

                    DataUpdated = true;
                    AxisUpdated = true;

                    break;

                case eUserAction.YOffset:
                    delta += DisplayYOffset;

                    delta = Math.Min(DirectXHeight * YZoomFactor, delta);
                    DisplayYOffset = Math.Max(-DirectXHeight * YZoomFactor, delta);

                    DataUpdated = true;
                    AxisUpdated = true;

                    break;

                case eUserAction.XZoom:
                    if (delta > 0 && XZoomFactor < 20.0f)
                        XZoomFactor *= 1.1f;

                    if (delta < 0 && XZoomFactor > 0.01f)
                        XZoomFactor /= 1.1f;

                    /* call ourselves again for min/max fitting */
                    ProcessUserAction(eUserAction.XOffset, 0);

                    DataUpdated = true;
                    AxisUpdated = true;

                    break;

                case eUserAction.YZoom:
                    if (delta > 0 && YZoomFactor < 50.0f)
                        YZoomFactor *= 1.1f;

                    if (delta < 0 && YZoomFactor > 0.001f)
                        YZoomFactor /= 1.1f;

                    /* call ourselves again for min/max fitting */
                    ProcessUserAction(eUserAction.YOffset, 0);

                    DataUpdated = true;
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

            LastMousePos.X = e.X;
            LastMousePos.Y = e.Y;
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
            DataUpdated = true;
            AxisUpdated = true;
        }

        protected override void OnResize(EventArgs e)
        {
            DataUpdated = true;
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
