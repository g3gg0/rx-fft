using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;
using SlimDX;
using System.Drawing;
using System.Windows.Forms;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public class FFTAreaSelection : DirectXDrawable
    {
        public enum eOperationMode
        {
            Area,
            Carriers,
            USB,
            LSB
        }
        public enum eAreaMode
        {
            Normal,
            USB,
            LSB
        }
        protected enum DragType
        {
            None,
            Center,
            UpperLeft,
            Upper,
            UpperRight,
            Right,
            LowerRight,
            Lower,
            LowerLeft,
            Left
        }

        public Color4 BodyColor = Color.FromArgb(0x1F, Color.Cyan);
        public Color4 BorderColor = Color.FromArgb(0x7F, Color.Cyan);
        public Color4 CarrierColor = Color.FromArgb(0x7F, Color.Cyan);

        public string Text = "";
        public Color4 TextColor = Color.Red; 
        protected Rectangle TextRect = new Rectangle();
        protected SlimDX.Direct3D9.Font DisplayFontNormal;

        public double DragBorderWidth = 10;
        public int Carriers = 0;
        public eOperationMode Mode = eOperationMode.Area;
        public eAreaMode AreaMode = eAreaMode.Normal;
        

        protected new DirectXFFTDisplay MainPlot;
        protected Vertex[] BorderVertexes = new Vertex[16];
        protected int BorderVertexesUsed = 0;
        protected Vertex[] BodyVertexes = new Vertex[16];
        protected int BodyVertexesUsed = 0;
        protected Vertex[] CarrierVertexes = new Vertex[1024];
        protected int CarrierVertexesUsed = 0;

        protected Keys Modifier = Keys.Alt;
        protected bool ModifierPressed = false;

        public bool Draggable = true;

        protected DragType DragMode = DragType.None;
        protected long DragStartFrequency = 0;
        protected double DragStartStrength = 0;        

        public bool Selected = false;
        public bool SelectionChanged = false;
        public bool Visible = false;

        public long StartFreq = 0;
        public double StartStrength = 0;
        public long EndFreq = 0;
        public double EndStrength = 0;

        public event EventHandler SelectionUpdated;

        public double CarrierWidth
        {
            get
            {
                return FreqWidth / Carriers;
            }
        }
        public double StrengthStart
        {
            get
            {
                return Math.Max(StartStrength, EndStrength);
            }
        }
        public double StrengthEnd
        {
            get
            {
                return Math.Min(StartStrength, EndStrength);
            }
        }
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
                    if (DragMode != DragType.None)
                    {
                        SelectionChanged = true;


                        if (StartFreq == EndFreq)
                        {
                            StartFreq = 0;
                            StartStrength = 0;
                            EndFreq = 0;
                            EndStrength = 0;
                            Visible = false;
                            Selected = false;
                        }

                        Reorder();
                        UpdatePositions();

                        DragMode = DragType.None;
                        MainPlot.CursorType(false);
                    }


                    break;

                case eInputEventType.MouseButtonDown:
                    if (ModifierPressed)
                    {
                        SelectionChanged = true;

                        Visible = true;
                        Selected = true;
                        DragMode = DragType.LowerRight;

                        handled = true;

                        StartFreq = MainPlot.CursorFrequency;
                        StartStrength = MainPlot.CursorStrength;
                        EndFreq = MainPlot.CursorFrequency;
                        EndStrength = MainPlot.CursorStrength;
                        UpdatePositions();

                        MainPlot.CursorType(true);
                    }
                    else
                    {
                        /* if area selected */
                        if (FreqWidth > 0)
                        {
                            bool withinHor = (MainPlot.CursorFrequency >= Math.Min(StartFreq, EndFreq) && MainPlot.CursorFrequency <= Math.Max(StartFreq, EndFreq));
                            bool withinVert = (MainPlot.CursorStrength >= Math.Min(StartStrength, EndStrength) && MainPlot.CursorStrength <= Math.Max(StartStrength, EndStrength));

                            /* and cursor within area */
                            if (withinHor)
                            {
                                Visible = true;
                                Selected = true;

                                if (Draggable && withinVert)
                                {
                                    int xPosCursor = (int)MainPlot.XPosFromFrequency(MainPlot.CursorFrequency);
                                    int yPosCursor = (int)MainPlot.YPosFromStrength(MainPlot.CursorStrength);
                                    int xPos1 = (int)MainPlot.XPosFromFrequency(StartFreq);
                                    int xPos2 = (int)MainPlot.XPosFromFrequency(EndFreq);
                                    int yPos1 = (int)MainPlot.YPosFromStrength(StartStrength);
                                    int yPos2 = (int)MainPlot.YPosFromStrength(EndStrength);

                                    double topDelta = Math.Abs(yPosCursor - Math.Max(yPos1, yPos2));
                                    double bottomDelta = Math.Abs(yPosCursor - Math.Min(yPos1, yPos2));
                                    double leftDelta = Math.Abs(xPosCursor - Math.Min(xPos1, xPos2));
                                    double rightDelta = Math.Abs(xPosCursor - Math.Max(xPos1, xPos2));

                                    bool topDrag = false;
                                    bool bottomDrag = false;
                                    bool leftDrag = false;
                                    bool rightDrag = false;

                                    if (topDelta < DragBorderWidth)
                                    {
                                        topDrag |= true;
                                    }
                                    if (bottomDelta < DragBorderWidth)
                                    {
                                        bottomDrag |= true;
                                    }
                                    if (leftDelta < DragBorderWidth)
                                    {
                                        leftDrag |= true;
                                    }
                                    if (rightDelta < DragBorderWidth)
                                    {
                                        rightDrag |= true;
                                    }

                                    if (topDrag)
                                    {
                                        if (leftDrag)
                                        {
                                            DragMode = DragType.UpperLeft;
                                        }
                                        else if (rightDrag)
                                        {
                                            DragMode = DragType.UpperRight;
                                        }
                                        else
                                        {
                                            DragMode = DragType.Upper;
                                        }
                                    }
                                    else if (bottomDrag)
                                    {
                                        if (leftDrag)
                                        {
                                            DragMode = DragType.LowerLeft;
                                        }
                                        else if (rightDrag)
                                        {
                                            DragMode = DragType.LowerRight;
                                        }
                                        else
                                        {
                                            DragMode = DragType.Lower;
                                        }
                                    }
                                    else
                                    {
                                        if (leftDrag)
                                        {
                                            DragMode = DragType.Left;
                                        }
                                        else if (rightDrag)
                                        {
                                            DragMode = DragType.Right;
                                        }
                                        else
                                        {
                                            DragMode = DragType.Center;
                                        }
                                    }

                                    SelectionChanged = true;

                                    DragStartFrequency = MainPlot.CursorFrequency;
                                    DragStartStrength = MainPlot.CursorStrength;
                                    handled = true;
                                }
                            }
                            else
                            {
                                /* else only display */
                                Visible = true;
                                Selected = false;
                            }
                        }

                        UpdatePositions();
                    }
                    break;

                case eInputEventType.MouseMoved:
                    switch (DragMode)
                    {
                        case DragType.LowerRight:
                            EndFreq = MainPlot.CursorFrequency;
                            EndStrength = MainPlot.CursorStrength;
                            break;
                        case DragType.LowerLeft:
                            StartFreq = MainPlot.CursorFrequency;
                            EndStrength = MainPlot.CursorStrength;
                            break;
                        case DragType.Lower:
                            EndStrength = MainPlot.CursorStrength;
                            break;
                        case DragType.Upper:
                            StartStrength = MainPlot.CursorStrength;
                            break;
                        case DragType.UpperLeft:
                            StartFreq = MainPlot.CursorFrequency;
                            StartStrength = MainPlot.CursorStrength;
                            break;
                        case DragType.UpperRight:
                            EndFreq = MainPlot.CursorFrequency;
                            StartStrength = MainPlot.CursorStrength;
                            break;
                        case DragType.Right:
                            EndFreq = MainPlot.CursorFrequency;
                            break;
                        case DragType.Left:
                            StartFreq = MainPlot.CursorFrequency;
                            break;
                        case DragType.Center:
                            long curFreq = MainPlot.CursorFrequency;
                            double curStrength = MainPlot.CursorStrength;

                            StartFreq -= (DragStartFrequency - curFreq);
                            StartStrength -= (DragStartStrength - curStrength);
                            EndFreq -= (DragStartFrequency - curFreq);
                            EndStrength -= (DragStartStrength - curStrength);

                            DragStartFrequency = curFreq;
                            DragStartStrength = curStrength;
                            break;
                    }

                    if (DragMode != DragType.None)
                    {
                        SelectionChanged = true;
                    }

                    if (!Selected && (FreqWidth > 0 && (MainPlot.CursorFrequency >= Math.Min(StartFreq, EndFreq) && MainPlot.CursorFrequency <= Math.Max(StartFreq, EndFreq))))
                    {
                        Selected = true;
                        SelectionChanged = true;
                    }


                    UpdatePositions();


                    break;

            }
            return handled;
        }

        private void Reorder()
        {
            long startFreq = Math.Min(StartFreq, EndFreq);
            long endFreq = Math.Max(StartFreq, EndFreq);
            double startStrength = Math.Min(StartStrength, EndStrength);
            double endStrength = Math.Max(StartStrength, EndStrength);

            StartFreq = startFreq;
            EndFreq = endFreq;
            StartStrength = startStrength;
            EndStrength = endStrength;
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

            if (SelectionChanged && SelectionUpdated != null)
            {
                SelectionUpdated(this, null);
            }

            SelectionChanged = false;
            PositionUpdated = true;
        }

        public override void AllocateResources()
        {
            if (MainPlot != null)
            {
                DisplayFontNormal = new SlimDX.Direct3D9.Font(MainPlot.Device, new System.Drawing.Font("Lucida Console", 7));
            }

            base.AllocateResources();
        }

        public override void ReleaseResources()
        {
            if (DisplayFontNormal != null)
                DisplayFontNormal.Dispose();
            DisplayFontNormal = null;

            base.ReleaseResources();
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
                BorderVertexesUsed = BuildRectangle(BorderVertexes, BorderVertexesUsed, xPos, xPos + AbsoluteWidth, yPos, yPos + AbsoluteHeight, (uint)BorderColor.ToArgb());

                switch (AreaMode)
                {
                    case eAreaMode.LSB:
                        BodyVertexesUsed = 0;
                        BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, xPos, xPos + AbsoluteWidth, yPos, yPos + AbsoluteHeight, (uint)BodyColor.ToArgb() & 0x1F000000, (uint)BodyColor.ToArgb());
                        break;
                    case eAreaMode.USB:
                        BodyVertexesUsed = 0;
                        BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, xPos, xPos + AbsoluteWidth, yPos, yPos + AbsoluteHeight, (uint)BodyColor.ToArgb(), (uint)BodyColor.ToArgb() & 0x1F000000);
                        break;
                    case eAreaMode.Normal:
                        BodyVertexesUsed = 0;
                        BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, xPos, xPos + AbsoluteWidth, yPos, yPos + AbsoluteHeight, (uint)BodyColor.ToArgb());
                        break;
                }


                /* we dont care about the maximum size anymore */
                TextRect.X = xPos + 10;
                TextRect.Y = yPos + 3;
                TextRect.Width = 200;// (int)AbsoluteWidth - 10;
                TextRect.Height = 200;// (int)AbsoluteHeight - 3;

                double carrierWidth = (double)AbsoluteWidth / Carriers;

                CarrierVertexesUsed = 0;
                for (int carrier = 0; carrier < Carriers; carrier++)
                {
                    CarrierVertexes[CarrierVertexesUsed].Color = (uint)CarrierColor.ToArgb();
                    CarrierVertexes[CarrierVertexesUsed].PositionRhw.X = (float)(xPos + carrier * carrierWidth + carrierWidth / 2);
                    CarrierVertexes[CarrierVertexesUsed].PositionRhw.Y = (float)(yPos);
                    CarrierVertexes[CarrierVertexesUsed].PositionRhw.Z = 0.5f;
                    CarrierVertexes[CarrierVertexesUsed].PositionRhw.W = 1;
                    CarrierVertexesUsed++;

                    CarrierVertexes[CarrierVertexesUsed].Color = (uint)CarrierColor.ToArgb();
                    CarrierVertexes[CarrierVertexesUsed].PositionRhw.X = (float)(xPos + carrier * carrierWidth + carrierWidth / 2);
                    CarrierVertexes[CarrierVertexesUsed].PositionRhw.Y = (float)(yPos + AbsoluteHeight);
                    CarrierVertexes[CarrierVertexesUsed].PositionRhw.Z = 0.5f;
                    CarrierVertexes[CarrierVertexesUsed].PositionRhw.W = 1;
                    CarrierVertexesUsed++;
                }

            }

            switch (Mode)
            {
                case eOperationMode.Area:
                    if (BodyVertexesUsed - 2 > 0)
                        MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, BodyVertexesUsed - 2, BodyVertexes);
                    if (BorderVertexesUsed > 0)
                        MainPlot.Device.DrawUserPrimitives(PrimitiveType.LineList, BorderVertexesUsed / 2, BorderVertexes);
                    break;

                case eOperationMode.Carriers:
                    if (BodyVertexesUsed - 2 > 0)
                        MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, BodyVertexesUsed - 2, BodyVertexes);
                    if (CarrierVertexesUsed > 0)
                        MainPlot.Device.DrawUserPrimitives(PrimitiveType.LineList, CarrierVertexesUsed / 2, CarrierVertexes);
                    break;

            }

            if (Text != "")
            {
                DisplayFontNormal.DrawString(null, Text, TextRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)TextColor);
            }
        }
    }
}
