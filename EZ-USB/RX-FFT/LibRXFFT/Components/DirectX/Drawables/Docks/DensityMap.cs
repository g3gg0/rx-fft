using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using LibRXFFT.Components.Generic;
using System.Drawing;
using LibRXFFT.Components.DirectX.Drawables.Docks;

namespace LibRXFFT.Components.DirectX.Drawables.Docks
{
    public class DensityMap : Dock, PlotVertsSink
    {
        public int Granularity = 4;
        public bool Active = true;
        public Color4 MapColor = Color.Red;

        protected ColorLookupTable Colors = new ColorLookupTable(Color.Red);
        protected Vertex[] DensityVerts = new Vertex[256];
        protected int DensityVertsUsed = 0;
        protected double[] DensityBlocks = new double[1];

        public DensityMap(DockPanel panel)
            : base(panel)
        {
            Title = "Density Map (#" + Granularity + ")";
            HideOthers = true;
            HideBackTitle = true;

            UpdateSize();
        }

        private void UpdateSize()
        {
            switch (Panel.Orientation)
            {
                case eOrientation.LeftBorder:
                case eOrientation.RightBorder:
                    Width = 30;
                    Height = MainPlot.DirectXHeight;
                    YPosition = 0;
                    break;

                case eOrientation.TopBorder:
                case eOrientation.BottomBorder:
                    Height = 30;
                    Width= MainPlot.DirectXWidth;
                    XPosition = 0;
                    break;
            }
            PositionUpdated = true;
        }

        public override void AllocateResources()
        {
            UpdateSize();
        }

        public override void Render()
        {
            if (!Active)
            {
                return;
            }

            if (DensityVertsUsed > 0)
                MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, DensityVertsUsed - 2, DensityVerts);
        }

        public override void ReleaseResources()
        {
        }

        public override bool ProcessUserEvent(InputEvent evt)
        {
            switch (evt.Type)
            {
                case eInputEventType.MouseEnter:
                case eInputEventType.MouseMoved:
                    return true;

                case eInputEventType.MouseWheel:
                    Granularity = Math.Max(2, Math.Min(1024, Granularity + Math.Sign(evt.MouseWheelDelta)));
                    Title = "Density Map (#" + Granularity + ")";
                    Panel.DocksChanged = true;
                    return true;
            }

            return false;
        }


        #region PlotVertsSink Member


        public void ProcessPlotVerts(Vertex[][] verts, int[] vertsCount)
        {
            /* dont update when hidden */
            if (State == eDockState.Collapsed)
            {
                return;
            }

            if (DensityBlocks.Length != Granularity)
            {
                DensityBlocks = new double[Granularity];
            }

            if (DensityVerts.Length != (DensityBlocks.Length + 1) * 4)
            {
                DensityVerts = new Vertex[(DensityBlocks.Length + 1) * 4];
            }

            /* clear all blocks */
            for (int pos = 0; pos < DensityBlocks.Length; pos++)
            {
                DensityBlocks[pos] = 0;
            }

            float blockHeight = Height / DensityBlocks.Length;
            float blockWidth = Width;

            switch (Panel.Orientation)
            {
                case eOrientation.LeftBorder:
                case eOrientation.RightBorder:
                    {
                        blockHeight = Height / DensityBlocks.Length;
                        blockWidth = Width;

                        /* count the points in the block */
                        int maxCount = 0;
                        for (int vertsArray = 0; vertsArray < vertsCount.Length; vertsArray++)
                        {
                            for (int pos = 0; pos < vertsCount[vertsArray]; pos++)
                            {
                                int block = (int)(verts[vertsArray][pos].PositionRhw.Y / blockHeight);

                                block = Math.Min(DensityBlocks.Length - 1, Math.Max(0, block));
                                maxCount = Math.Max(maxCount, (int)++DensityBlocks[block]);
                            }
                        }
                        /* normalize to 0 .. 1.0 */
                        for (int pos = 0; pos < DensityBlocks.Length; pos++)
                        {
                            DensityBlocks[pos] /= maxCount;
                        }

                        DensityVertsUsed = 0;
                        DensityVertsUsed = BuildFilledRectangle(DensityVerts, DensityVertsUsed, XPosition, XPosition + blockWidth, 0, Height, 0xFF000000);
                        for (int block = 0; block < DensityBlocks.Length; block++)
                        {
                            float yPos = block * blockHeight;
                            DensityVertsUsed = BuildFilledRectangle(DensityVerts, DensityVertsUsed, XPosition, XPosition + blockWidth, yPos, yPos + blockHeight, 0xFF000000 | Colors.Lookup(DensityBlocks[block]));
                        }
                    }
                    break;

                case eOrientation.TopBorder:
                case eOrientation.BottomBorder:
                    {
                        blockHeight = Height;
                        blockWidth = Width / DensityBlocks.Length;

                        /* count the points in the block */
                        int maxCount = 0;
                        for (int vertsArray = 0; vertsArray < vertsCount.Length; vertsArray++)
                        {
                            for (int pos = 0; pos < vertsCount[vertsArray]; pos++)
                            {
                                int block = (int)(verts[vertsArray][pos].PositionRhw.X / blockWidth);

                                block = Math.Min(DensityBlocks.Length - 1, Math.Max(0, block));
                                maxCount = Math.Max(maxCount, (int)++DensityBlocks[block]);
                            }
                        }

                        /* normalize to 0 .. 1.0 */
                        for (int pos = 0; pos < DensityBlocks.Length; pos++)
                        {
                            DensityBlocks[pos] /= maxCount;
                        }


                        DensityVertsUsed = 0;
                        DensityVertsUsed = BuildFilledRectangle(DensityVerts, DensityVertsUsed, 0, Width, YPosition, YPosition + Height, 0xFF000000);
                        for (int block = 0; block < DensityBlocks.Length; block++)
                        {
                            float xPos = block * blockWidth;
                            DensityVertsUsed = BuildFilledRectangle(DensityVerts, DensityVertsUsed, xPos, xPos + blockWidth, YPosition, YPosition + blockHeight, 0xFF000000 | Colors.Lookup(DensityBlocks[block]));
                        }
                    }
                    break;

                default:
                    return;
            }
        }

        public void ProcessPlotVerts(Vertex[] verts, int vertsCount)
        {
            /* dont update when hidden */
            if (State == eDockState.Collapsed)
            {
                return;
            }

            if (DensityBlocks.Length != Granularity)
            {
                DensityBlocks = new double[Granularity];
            }

            if (DensityVerts.Length != (DensityBlocks.Length + 1) * 4)
            {
                DensityVerts = new Vertex[(DensityBlocks.Length + 1) * 4];
            }

            /* clear all blocks */
            for (int pos = 0; pos < DensityBlocks.Length; pos++)
            {
                DensityBlocks[pos] = 0;
            }

            float blockHeight = Height / DensityBlocks.Length;
            float blockWidth = Width;

            switch (Panel.Orientation)
            {
                case eOrientation.LeftBorder:
                case eOrientation.RightBorder:
                    {
                        blockHeight = Height / DensityBlocks.Length;
                        blockWidth = Width;

                        /* count the points in the block */
                        int maxCount = 0;
                        for (int pos = 0; pos < vertsCount; pos++)
                        {
                            int block = (int)(verts[pos].PositionRhw.Y / blockHeight);

                            block = Math.Min(DensityBlocks.Length - 1, Math.Max(0, block));
                            maxCount = Math.Max(maxCount, (int)++DensityBlocks[block]);
                        }

                        /* normalize to 0 .. 1.0 */
                        for (int pos = 0; pos < DensityBlocks.Length; pos++)
                        {
                            DensityBlocks[pos] /= maxCount;
                        }

                        DensityVertsUsed = 0;
                        DensityVertsUsed = BuildFilledRectangle(DensityVerts, DensityVertsUsed, XPosition, XPosition + blockWidth, 0, Height, 0xFF000000);
                        for (int block = 0; block < DensityBlocks.Length; block++)
                        {
                            float yPos = block * blockHeight;
                            DensityVertsUsed = BuildFilledRectangle(DensityVerts, DensityVertsUsed, XPosition, XPosition + blockWidth, yPos, yPos + blockHeight, 0xFF000000 | Colors.Lookup(DensityBlocks[block]));
                        }
                    }
                    break;

                case eOrientation.TopBorder:
                case eOrientation.BottomBorder:
                    {
                        blockHeight = Height;
                        blockWidth = Width / DensityBlocks.Length;

                        /* count the points in the block */
                        int maxCount = 0;
                        for (int pos = 0; pos < vertsCount; pos++)
                        {
                            int block = (int)(verts[pos].PositionRhw.X / blockWidth);

                            block = Math.Min(DensityBlocks.Length - 1, Math.Max(0, block));
                            maxCount = Math.Max(maxCount, (int)++DensityBlocks[block]);
                        }

                        /* normalize to 0 .. 1.0 */
                        for (int pos = 0; pos < DensityBlocks.Length; pos++)
                        {
                            DensityBlocks[pos] /= maxCount;
                        }


                        DensityVertsUsed = 0;
                        DensityVertsUsed = BuildFilledRectangle(DensityVerts, DensityVertsUsed, 0, Width, YPosition, YPosition + Height, 0xFF000000);
                        for (int block = 0; block < DensityBlocks.Length; block++)
                        {
                            float xPos = block * blockWidth;
                            DensityVertsUsed = BuildFilledRectangle(DensityVerts, DensityVertsUsed, xPos, xPos + blockWidth, YPosition, YPosition + blockHeight, 0xFF000000 | Colors.Lookup(DensityBlocks[block]));
                        }
                    }
                    break;

                default:
                    return;
            }

        }

        #endregion

    }
}
