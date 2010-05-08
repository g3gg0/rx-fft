using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public class WaveformAreaSelection : DirectXDrawable
    {
        protected new DirectXWaveformDisplay MainPlot;
        protected Vertex[] BorderVertexes = new Vertex[16];
        protected int BorderVertexesUsed = 0;
        protected Vertex[] BodyVertexes = new Vertex[16];
        protected int BodyVertexesUsed = 0;

        protected System.Windows.Forms.Keys Modifier = System.Windows.Forms.Keys.Alt;
        protected bool ModifierPressed = false;
        protected bool Dragging = false;

        public bool Selected = false;
        public bool Visible = false;

        public double StartTime = 0;
        public double EndTime = 0;

        public double SelectionWidth
        {
            get
            {
                return Math.Abs(EndTime - StartTime);
            }
        }
        public double SelectionStart
        {
            get
            {
                return Math.Min(StartTime, EndTime);
            }
        }
        public double SelectionEnd
        {
            get
            {
                return Math.Max(StartTime, EndTime);
            }
        }

        public event EventHandler SelectionUpdated;


        public WaveformAreaSelection(DirectXWaveformDisplay mainPlot)
            : base(mainPlot)
        {
            MainPlot = mainPlot;

            AllocateResources();
            MainPlot.AddDrawable(this);
        }

        public override bool ProcessInputEvent(InputEvent evt)
        {
            bool handled = false;
            bool modifierPressed = (evt.KeyData & Modifier) == Modifier;

            switch (evt.Type)
            {
                case eInputEventType.KeyDown:
                    if (modifierPressed)
                    {
                        ModifierPressed = true;
                    }
                    break;

                case eInputEventType.KeyUp:
                    if (!modifierPressed)
                    {
                        ModifierPressed = false;
                    }
                    break;

                case eInputEventType.MouseButtonUp:
                    if (Dragging)
                    {
                        UpdatePositions();

                        if (StartTime == EndTime)
                        {
                            StartTime = 0;
                            EndTime = 0;
                            Visible = false;
                            Selected = false;
                        }

                        Dragging = false;
                        MainPlot.CursorType(false);

                        if (SelectionUpdated != null)
                        {
                            SelectionUpdated(this, null);
                        }
                    }

                    break;

                case eInputEventType.MouseButtonDown:
                    if (ModifierPressed)
                    {
                        Visible = true;
                        Selected = true;
                        Dragging = true;

                        handled = true;

                        StartTime = MainPlot.CursorTime;
                        EndTime = MainPlot.CursorTime;

                        UpdatePositions();

                        MainPlot.CursorType(true);

                        if (SelectionUpdated != null)
                        {
                            SelectionUpdated(this, null);
                        }
                    }
                    else
                    {
                        /* if area selected */
                        if (SelectionWidth > 0)
                        {
                            /* and cursor within area */
                            if (MainPlot.CursorTime >= StartTime && MainPlot.CursorTime <= EndTime)
                            {
                                Visible = true;
                                Selected = true;
                            }
                            else
                            {
                                /* else only display */
                                Visible = true;
                                Selected = false;
                            }
                        }

                        UpdatePositions();

                        if (SelectionUpdated != null)
                        {
                            SelectionUpdated(this, null);
                        }
                    }
                    break;

                case eInputEventType.MouseMoved:
                    if (Dragging)
                    {
                        EndTime = MainPlot.CursorTime;
                        UpdatePositions();

                        if (SelectionUpdated != null)
                        {
                            SelectionUpdated(this, null);
                        }
                    }
                    else if (!Selected && (SelectionWidth > 0 && MainPlot.CursorTime >= StartTime && MainPlot.CursorTime <= EndTime))
                    {
                        Selected = true;
                        if (SelectionUpdated != null)
                        {
                            SelectionUpdated(this, null);
                        }
                    }
                    break;

            }
            return handled;
        }

        public override void UpdatePositions()
        {
            int xPos1 = (int)MainPlot.XPosFromTime(StartTime);
            int xPos2 = (int)MainPlot.XPosFromTime(EndTime);
            int yPos1 = 0;
            int yPos2 = (int)MainPlot.DirectXHeight;

            int xPosStart = Math.Min(xPos1, xPos2);
            int xPosEnd = Math.Max(xPos1, xPos2);
            int yPosStart = Math.Min(yPos1, yPos2);
            int yPosEnd = Math.Max(yPos1, yPos2);

            AbsoluteXPosition = xPosStart;
            AbsoluteWidth = xPosEnd - xPosStart;

            AbsoluteYPosition = yPosStart;
            AbsoluteHeight = yPosEnd - yPosStart;

            PositionUpdated = true;
        }

        public override void AllocateResources()
        {
            base.AllocateResources();
        }

        public override void Render()
        {
            if (!Visible)
            {
                return;
            }

            int xPos = AbsoluteXPosition;
            int yPos = AbsoluteYPosition;

            if (PositionUpdated)
            {
                PositionUpdated = false;

                BorderVertexesUsed = 0;
                BorderVertexesUsed = BuildRectangle(BorderVertexes, BorderVertexesUsed, xPos, xPos + AbsoluteWidth, yPos, yPos + AbsoluteHeight, 0x7F00FFFF);

                BodyVertexesUsed = 0;
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, xPos, xPos + AbsoluteWidth, yPos, yPos + AbsoluteHeight, 0x1F00FFFF);
            }
            if (BodyVertexesUsed - 2 > 0)
                MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, BodyVertexesUsed - 2, BodyVertexes);
            if (BorderVertexesUsed > 0)
                MainPlot.Device.DrawUserPrimitives(PrimitiveType.LineList, BorderVertexesUsed / 2, BorderVertexes);
        }

        public override void ReleaseResources()
        {
        }
    }
}
