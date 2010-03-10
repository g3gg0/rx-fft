using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public class FFTAreaSelection : DirectXDrawable
    {
        protected new DirectXFFTDisplay MainPlot;
        protected Vertex[] BorderVertexes = new Vertex[16];
        protected int BorderVertexesUsed = 0;
        protected Vertex[] BodyVertexes = new Vertex[16];
        protected int BodyVertexesUsed = 0;

        protected System.Windows.Forms.Keys Modifier = System.Windows.Forms.Keys.Alt;
        protected bool ModifierPressed = false;
        protected bool Dragging = false;

        public bool Selected = false;
        public bool Visible = false;

        public long StartFreq = 0;
        public double StartStrength = 0;
        public long EndFreq = 0;
        public double EndStrength = 0;

        public double FreqWidth
        {
            get
            {
                return Math.Abs(EndFreq - StartFreq);
            }
        }
        public double FreqStart
        {
            get
            {
                return Math.Min(StartFreq, EndFreq);
            }
        }
        public double FreqEnd
        {
            get
            {
                return Math.Max(StartFreq, EndFreq);
            }
        }

        public event EventHandler SelectionUpdated;


        public FFTAreaSelection(DirectXFFTDisplay mainPlot)
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

                        if (StartFreq == EndFreq)
                        {
                            StartFreq = 0;
                            StartStrength = 0;
                            EndFreq = 0;
                            EndStrength = 0;
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

                        StartFreq = MainPlot.CursorFrequency;
                        StartStrength = MainPlot.CursorStrength;
                        EndFreq = MainPlot.CursorFrequency;
                        EndStrength = MainPlot.CursorStrength;
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
                        if (FreqWidth > 0)
                        {
                            /* and cursor within area */
                            if (MainPlot.CursorFrequency >= StartFreq && MainPlot.CursorFrequency <= EndFreq)
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
                        EndFreq = MainPlot.CursorFrequency;
                        EndStrength = MainPlot.CursorStrength;
                        UpdatePositions();

                        if (SelectionUpdated != null)
                        {
                            SelectionUpdated(this, null);
                        }
                    }
                    else if (!Selected && (FreqWidth > 0 && MainPlot.CursorFrequency >= StartFreq && MainPlot.CursorFrequency <= EndFreq))
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
            int xPos1 = (int)MainPlot.XPosFromFrequency(StartFreq);
            int xPos2 = (int)MainPlot.XPosFromFrequency(EndFreq);
            int yPos1 = (int)MainPlot.YPosFromStrength(StartStrength);
            int yPos2 = (int)MainPlot.YPosFromStrength(EndStrength);

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

            MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, BodyVertexesUsed - 2, BodyVertexes);
            MainPlot.Device.DrawUserPrimitives(PrimitiveType.LineList, BorderVertexesUsed / 2, BorderVertexes);

        }

        public override void ReleaseResources()
        {
        }
    }
}
