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
        struct Vertex
        {
            public Vector4 PositionRhw;
            public int Color;
        }

        protected int DirectXWidth = 1280;
        protected int DirectXHeight = 1024;

        private Direct3D Direct3D;
        private Device Device;
        private Font DisplayFont;
        private PresentParameters PresentParameters;
        /* add some interface to set the YAxisLines - for now external code is locking and modifying member variables */
        public readonly Mutex DirectXLock = new Mutex();

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

        private bool ShiftPressed;


        private int NumLines = 0;
        public double XAxisUnit = 100;
        public double XAxisGridOffset = 0;
        public double XAxisSampleOffset = 0;
        public int XAxisLines = 0;
        private double DisplayStartOffset;
        private int LastMousePosX;

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

                        NumLines = numPoints - 1;
                        for (int pos = 0; pos < numPoints; pos++)
                        {
                            PlotVerts[pos].PositionRhw.X = (float)(XAxisSampleOffset * XZoomFactor - DisplayStartOffset + points[pos].X * XZoomFactor);
                            PlotVerts[pos].PositionRhw.Y = (float)(DirectXHeight - (points[pos].Y * YZoomFactor * DirectXHeight)) / 2;
                            PlotVerts[pos].PositionRhw.Z = 0.5f;
                            PlotVerts[pos].PositionRhw.W = 1;
                            PlotVerts[pos].Color = color;
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


                    XAxisVerts = new Vertex[XAxisLines * 4];
                    for (int pos = 0; pos < XAxisLines; pos++)
                    {
                        float xPos = (float)(XAxisGridOffset * XZoomFactor - DisplayStartOffset + (pos * XAxisUnit * XZoomFactor));

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


        protected void InitializeDirectX()
        {
            try
            {
                DirectXLock.WaitOne();
                DirectXHeight = Height;
                DirectXWidth = Width;

                if (Direct3D == null)
                {
                    Direct3D = new Direct3D();

                    PresentParameters = new PresentParameters();
                    PresentParameters.BackBufferHeight = DirectXHeight;
                    PresentParameters.BackBufferWidth = DirectXWidth;
                    PresentParameters.DeviceWindowHandle = Handle;

                    PlotVerts = new Vertex[0];
                    XAxisVerts = new Vertex[0];
                    YAxisVerts = new Vertex[0];
                }
                else
                {
                    PresentParameters.BackBufferHeight = DirectXHeight;
                    PresentParameters.BackBufferWidth = DirectXWidth;
                }

                Device = new Device(Direct3D, 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing, PresentParameters);
                DisplayFont = new Font(Device, new System.Drawing.Font("Arial", 20));

            }
            catch (Exception e)
            {
                return;
            }
            finally
            {
                DirectXLock.ReleaseMutex();
            }

            return;
        }


        protected void Render()
        {
            if (Device == null)
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
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, NumLines, PlotVerts);

                DisplayFont.DrawString(null, Name, 20, 30, 0x7F00FFFF);

                Device.EndScene();

                Device.Present();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
            }
            finally
            {
                DirectXLock.ReleaseMutex();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                ShiftPressed = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                ShiftPressed = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int xDelta = LastMousePosX - e.X;

                double val = DisplayStartOffset + xDelta * XZoomFactor;
                val = Math.Min(XZoomFactor * DirectXWidth, val);
                val = Math.Max(0, val);

                DisplayStartOffset = val;
                DataUpdated = true;
                AxisUpdated = true;
            }

            LastMousePosX = e.X;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!ShiftPressed)
            {
                if (e.Delta > 0 && YZoomFactor < 20.0f)
                    YZoomFactor *= 1.1f;

                if (e.Delta < 0 && YZoomFactor > 0.01f)
                    YZoomFactor /= 1.1f;
            }
            else
            {
                if (e.Delta > 0 && XZoomFactor < 20.0f)
                    XZoomFactor *= 1.1f;

                if (e.Delta < 0 && XZoomFactor > 0.01f)
                    XZoomFactor /= 1.1f;
            }

            DataUpdated = true;
            AxisUpdated = true;
        }


        protected override void OnResize(EventArgs e)
        {
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            InitializeDirectX();
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
