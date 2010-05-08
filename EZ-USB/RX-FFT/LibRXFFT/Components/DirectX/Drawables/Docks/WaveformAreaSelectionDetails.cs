

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using System.Drawing;
using SlimDX.Direct3D9;
using LibRXFFT.Libraries.Misc;

namespace LibRXFFT.Components.DirectX.Drawables.Docks
{
    public class WaveformAreaSelectionDetails : Dock
    {
        public Color4 TextColor = Color.Cyan;
        public Color4 ShadowColor = Color.Black;

        protected WaveformAreaSelection Selection;

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



        public WaveformAreaSelectionDetails(WaveformAreaSelection selection, DockPanel panel)
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
            if (State == eDockState.Hidden)
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

            string text = "Selected: ";
            DisplayFontBold.DrawString(null, text, TextShadowRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)ShadowColor);
            DisplayFontBold.DrawString(null, text, TextRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)TextColor);
            TextRect.Y += FontSizeRect.Height;
            TextShadowRect.Y += FontSizeRect.Height;

            text = "  from " + FrequencyFormatter.TimeToString(Selection.SelectionStart);
            DisplayFontNormal.DrawString(null, text, TextShadowRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)ShadowColor);
            DisplayFontNormal.DrawString(null, text, TextRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)TextColor);
            TextRect.Y += FontSizeRect.Height;
            TextShadowRect.Y += FontSizeRect.Height;

            text = "    to " + FrequencyFormatter.TimeToString(Selection.SelectionEnd);
            DisplayFontNormal.DrawString(null, text, TextShadowRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)ShadowColor);
            DisplayFontNormal.DrawString(null, text, TextRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)TextColor);
            TextRect.Y += FontSizeRect.Height;
            TextShadowRect.Y += FontSizeRect.Height;

            text = "Width: ";
            DisplayFontBold.DrawString(null, text, TextShadowRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)ShadowColor);
            DisplayFontBold.DrawString(null, text, TextRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)TextColor);
            TextRect.Y += FontSizeRect.Height;
            TextShadowRect.Y += FontSizeRect.Height;

            text = "       " + FrequencyFormatter.TimeToString(Selection.SelectionWidth);
            DisplayFontNormal.DrawString(null, text, TextShadowRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)ShadowColor);
            DisplayFontNormal.DrawString(null, text, TextRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)TextColor);
            TextRect.Y += FontSizeRect.Height;
            TextShadowRect.Y += FontSizeRect.Height;

            text = "Information: ";
            DisplayFontBold.DrawString(null, text, TextShadowRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)ShadowColor);
            DisplayFontBold.DrawString(null, text, TextRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)TextColor);
            TextRect.Y += FontSizeRect.Height;
            TextShadowRect.Y += FontSizeRect.Height;

            text = "       " + String.Format("{0:0.##} Hz", 1/Selection.SelectionWidth);
            DisplayFontNormal.DrawString(null, text, TextShadowRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)ShadowColor);
            DisplayFontNormal.DrawString(null, text, TextRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)TextColor);
            TextRect.Y += FontSizeRect.Height;
            TextShadowRect.Y += FontSizeRect.Height;

        }
    }
}
