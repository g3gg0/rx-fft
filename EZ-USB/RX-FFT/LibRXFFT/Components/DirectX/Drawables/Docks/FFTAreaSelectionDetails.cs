using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using System.Drawing;
using SlimDX.Direct3D9;
using LibRXFFT.Libraries.Misc;
using System.Windows.Forms;

namespace LibRXFFT.Components.DirectX.Drawables.Docks
{
    public class FFTAreaSelectionDetails : Dock
    {
        public Color4 TextColor = Color.Cyan;
        public Color4 ShadowColor = Color.Black;


        protected FFTAreaSelection Selection;

        protected bool DataUpdated = false;
        protected SlimDX.Direct3D9.Font DisplayFontNormal;
        protected SlimDX.Direct3D9.Font DisplayFontBold;
        protected Vertex[] BorderVertexes = new Vertex[128];
        protected int BorderVertexesUsed = 0;
        protected Vertex[] BodyVertexes = new Vertex[128];
        protected int BodyVertexesUsed = 0;
        protected Rectangle TextRect = new Rectangle();
        protected Rectangle TextShadowRect = new Rectangle();

        protected Rectangle FontSizeRect = new Rectangle();

        protected FFTAreaSelection.OperationMode Mode
        {
            get
            {
                return Selection.Mode;
            }
            set
            {
                Selection.Mode = value;
            }
        }


        public FFTAreaSelectionDetails(FFTAreaSelection selection, DockPanel panel)
            : base(panel)
        {
            Title = "Selection";
            Selection = selection;
            Width = 150;
            Height = 80;

            selection.SelectionUpdated += new EventHandler(SelectionUpdated);
        }

        private void SelectionUpdated(object sender, EventArgs e)
        {
            if (Selection.Selected)
            {
                Panel.ExpandDock(this);
            }
            else
            {
                Panel.CollapseDock(this);
            }
        }

        public override void StateChanged()
        {
            switch (State)
            {
                case eDockState.Expanded:
                case eDockState.Expanding:
                    Selection.Selected = true;
                    break;
                case eDockState.Collapsed:
                case eDockState.Collapsing:
                    Selection.Selected = false;
                    break;
            }
        }

        public override bool ProcessUserEvent(InputEvent evt)
        {
            switch (evt.Type)
            {
                case eInputEventType.MouseClick:
                    if (evt.MouseButtons == System.Windows.Forms.MouseButtons.Right)
                    {
                        ContextMenu contextMenu = new ContextMenu();
                        MenuItem menuItem1 = new MenuItem("FFT Area Selection");
                        MenuItem menuItem2 = new MenuItem("-");
                        MenuItem menuItem3 = new MenuItem("Dragable");
                        MenuItem menuItem4 = new MenuItem("-");
                        MenuItem menuItem5 = new MenuItem("Area mode");
                        MenuItem menuItem6 = new MenuItem("Carrier mode");

                        menuItem1.Enabled = false;
                        menuItem3.Checked = Selection.Draggable;
                        menuItem5.Checked = (Mode == FFTAreaSelection.OperationMode.Area);
                        menuItem6.Checked = (Mode == FFTAreaSelection.OperationMode.Carriers);

                        contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem1, menuItem2, menuItem3, menuItem4, menuItem5, menuItem6 });

                        menuItem3.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            Selection.Draggable = !Selection.Draggable;
                        });

                        menuItem5.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            Mode = FFTAreaSelection.OperationMode.Area;
                        });

                        menuItem6.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            Mode = FFTAreaSelection.OperationMode.Carriers;
                        });


                        System.Drawing.Point mousePos = new System.Drawing.Point();
                        mousePos.X = (int)evt.MousePosition.X;
                        mousePos.Y = (int)evt.MousePosition.Y;

                        System.Drawing.Point popupPos = MainPlot.PointToClient(mousePos);

                        popupPos.X -= 20;
                        popupPos.Y -= 20;
                        contextMenu.Show(MainPlot, popupPos);
                        return true;
                    }
                    break;

                case eInputEventType.MouseButtonDown:
                case eInputEventType.MouseButtonUp:
                case eInputEventType.KeyDown:
                case eInputEventType.MouseMoved:
                    return true;

                case eInputEventType.MouseWheel:
                    if (Mode == FFTAreaSelection.OperationMode.Carriers)
                    {
                        Selection.Carriers += Math.Sign(evt.MouseWheelDelta);
                        Selection.Carriers = Math.Max(0, Math.Min(256, Selection.Carriers));

                        Selection.UpdatePositions();
                        PositionUpdated = true;
                    }
                    return true;
            }

            return base.ProcessUserEvent(evt);
        }

        public override void AllocateResources()
        {
            DisplayFontNormal = new SlimDX.Direct3D9.Font(Panel.MainPlot.Device, new System.Drawing.Font("Lucida Console", 7));
            DisplayFontBold = new SlimDX.Direct3D9.Font(Panel.MainPlot.Device, new System.Drawing.Font("Lucida Console", 7, FontStyle.Bold | FontStyle.Underline));
            DisplayFontBold.MeasureString(null, "X", DrawTextFormat.Center, ref FontSizeRect);

            PositionUpdated = true;
        }

        public override void ReleaseResources()
        {
            if (DisplayFontNormal != null)
                DisplayFontNormal.Dispose();
            DisplayFontNormal = null;

            if (DisplayFontBold != null)
                DisplayFontBold.Dispose();
            DisplayFontBold = null;
        }


        public override void Render()
        {
            if (State == eDockState.Hidden || State == eDockState.Collapsed)
            {
                return;
            }

            int xPos = XPosition;
            int yPos = YPosition;

            if (PositionUpdated)
            {
                PositionUpdated = false;
                BodyVertexesUsed = 0;

                /* dark background */
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, xPos, (int)(xPos + Width), yPos, yPos + Height, 0xBF000000);
            }

            if (BodyVertexesUsed - 2 > 0)
                Panel.MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, BodyVertexesUsed - 2, BodyVertexes);

            TextRect.X = XPosition + 10;
            TextRect.Y = YPosition + 3;
            TextRect.Width = (int)Width;
            TextRect.Height = (int)Height;
            TextShadowRect.X = TextRect.X + 1;
            TextShadowRect.Y = TextRect.Y + 1;
            TextShadowRect.Width = (int)Width;
            TextShadowRect.Height = (int)Height;

            AddLine("Selected");
            AddLine("   from    " + FrequencyFormatter.FreqToStringAccurate(Selection.FreqStart));
            AddLine("     to    " + FrequencyFormatter.FreqToStringAccurate(Selection.FreqEnd));
            AddLine("  width    " + FrequencyFormatter.FreqToStringAccurate(Selection.FreqWidth));

            if (Selection.Carriers > 0)
            {
                AddLine("");
                AddLine("Carriers");
                AddLine("  count    " + Selection.Carriers);
                if (Selection.Carriers > 1)
                {
                    AddLine("  delta    " + FrequencyFormatter.FreqToStringAccurate(Selection.CarrierWidth));
                }
            }
        }

        private void AddLine(string text)
        {
            Rectangle stringSizeRect = new Rectangle();

            DisplayFontBold.MeasureString(null, text, DrawTextFormat.Center, ref stringSizeRect);

            DisplayFontNormal.DrawString(null, text, TextShadowRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)ShadowColor);
            DisplayFontNormal.DrawString(null, text, TextRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)TextColor);

            TextRect.Y += FontSizeRect.Height;
            TextShadowRect.Y += FontSizeRect.Height;

            if (stringSizeRect.Width > Width)
            {
                Width = stringSizeRect.Width;
                Panel.ExpandDock(this);
            }

            if (TextRect.Y > Height)
            {
                Height = TextRect.Y;
                Panel.ExpandDock(this);
            }
        }
    }
}
