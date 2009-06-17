using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;
using Device=SlimDX.Direct3D9.Device;

namespace LibRXFFT.Components.DirectX
{

    public class DirectXPlot : UserControl
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        {
            public Vector4 PositionRhw;
            public int Color;
        }

        protected int DirectXWidth = 1280;
        protected int DirectXHeight = 1024;

        private readonly Semaphore DirectXSemaphore = new Semaphore(1, 100);

        private Vertex[] PlotVerts;
        private Vertex[] XAxisVerts;

        protected Point[] LinePoints;
        protected int LinePointEntries;
        protected bool LinePointsUpdated;


        private Device device;
        private int NumLines = 0;
        public double XAxisUnit = 100;
        public int XAxisOffset = 0;
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

        protected void CreateVertexBuffer(Point[] points)
        {
            CreateVertexBuffer(points, points.Length);
        }

        protected void CreateVertexBuffer(Point[] points, int numPoints)
        {
            try
            {
                DirectXSemaphore.WaitOne();

                if (device != null)
                {
                    int color = ColorFG.ToArgb();

                    NumLines = numPoints-1;
                    for (int pos = 0; pos < numPoints; pos++)
                    {
                        PlotVerts[pos].PositionRhw.X = (float) (points[pos].X * XZoomFactor);
                        PlotVerts[pos].PositionRhw.Y = points[pos].Y;
                        PlotVerts[pos].PositionRhw.Z = 0.5f;
                        PlotVerts[pos].PositionRhw.W = 1;
                        PlotVerts[pos].Color = color;
                    }



                    int color1 = 0x7F101010;
                    int color2 = 0x7F404040;

                    XAxisVerts = new Vertex[XAxisLines * 4];
                    for (int pos = 0; pos < XAxisLines; pos++)
                    {
                        float xPos = XAxisOffset + (float) (pos*XAxisUnit*XZoomFactor);

                        XAxisVerts[pos * 4 + 0].PositionRhw.X = xPos;
                        XAxisVerts[pos * 4 + 0].PositionRhw.Y = 0;
                        XAxisVerts[pos * 4 + 0].PositionRhw.Z = 0.5f;
                        XAxisVerts[pos * 4 + 0].PositionRhw.W = 1;
                        XAxisVerts[pos * 4 + 0].Color = color1;

                        XAxisVerts[pos * 4 + 1].PositionRhw.X = xPos;
                        XAxisVerts[pos * 4 + 1].PositionRhw.Y = DirectXHeight/2;
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
                DirectXSemaphore.Release();
            }
        }


        protected void InitializeDirectX()
        {
            try
            {
                DirectXSemaphore.WaitOne();

                // Now let's setup our D3D stuff
                ClientSize = new Size(DirectXWidth, DirectXHeight);

                Direct3D direct3D = new Direct3D();

                PresentParameters presentParams = new PresentParameters();
                presentParams.BackBufferHeight = DirectXHeight;
                presentParams.BackBufferWidth = DirectXWidth;
                presentParams.DeviceWindowHandle = Handle;

                device = new Device(direct3D, 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing, presentParams);
                PlotVerts = new Vertex[DirectXWidth];
                XAxisVerts = new Vertex[0];

            }
            catch (Exception e)
            {
                return;
            }
            finally
            {
                DirectXSemaphore.Release();
            }

            return;
        }


        protected void Render()
        {
            if (device == null)
                return;

            try
            {
                if (LinePointsUpdated)
                {
                    CreateVertexBuffer(LinePoints, LinePointEntries);
                    LinePointsUpdated = false;
                }

                DirectXSemaphore.WaitOne();

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);

                device.BeginScene();

                device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;

                if (XAxisVerts.Length > 0)
                    device.DrawUserPrimitives(PrimitiveType.LineList, XAxisVerts.Length / 2, XAxisVerts);

                device.DrawUserPrimitives(PrimitiveType.LineStrip, NumLines, PlotVerts);

                device.EndScene();

                device.Present();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
            }
            finally
            {
                DirectXSemaphore.Release();
            }
        }

    }
}
