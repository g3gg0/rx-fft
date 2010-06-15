using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;

namespace LibRXFFT.Components.DirectX.Drawables.Docks
{
    public class StaticTextDock : Dock
    {
        public int XOffset = 0;
        public int YOffset = 0;
        public Color4 TextColor = Color.Red;
        public Color4 ShadowColor = Color.DarkRed;
        protected SlimDX.Direct3D9.Font DisplayFont;
        protected Vertex[] BorderVertexes = new Vertex[128];
        protected int BorderVertexesUsed = 0;
        protected Vertex[] BodyVertexes = new Vertex[128];
        protected int BodyVertexesUsed = 0;
        protected Rectangle TextRect = new Rectangle();
        protected Rectangle TextShadowRect = new Rectangle();
        protected Rectangle FontSizeRect = new Rectangle();

        public string _Text = "Test";
        public string Text
        {
            get { return _Text; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _Text = "";
                    Hide();
                }
                else
                {
                    _Text = value;
                    Expand();
                }

                PositionUpdated = true;
            }
        }


        public StaticTextDock(DockPanel panel)
            : base(panel)
        {
        } 

        public override void AllocateResources()
        {
            DisplayFont = new SlimDX.Direct3D9.Font(Panel.MainPlot.Device, new System.Drawing.Font("Lucida Console", 16));
            DisplayFont.MeasureString(null, "X", DrawTextFormat.Center, ref FontSizeRect);

            PositionUpdated = true;
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
            {
                Panel.MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, BodyVertexesUsed - 2, BodyVertexes);
            }

            TextRect.X = XPosition + 3 + XOffset;
            TextRect.Y = YPosition + 3 + YOffset;
            TextRect.Width = (int)Width;
            TextRect.Height = (int)Height;
            TextShadowRect.X = TextRect.X + 1;
            TextShadowRect.Y = TextRect.Y + 1;
            TextShadowRect.Width = (int)Width;
            TextShadowRect.Height = (int)Height;

            foreach (string line in Text.Split('\n'))
            {
                AddLine(line);
            }
        }

        public override void ReleaseResources()
        {
            if (DisplayFont != null)
                DisplayFont.Dispose();
            DisplayFont = null;
        }

        private void AddLine(string text)
        {
            Rectangle stringSizeRect = new Rectangle();

            DisplayFont.MeasureString(null, text, DrawTextFormat.Center, ref stringSizeRect);

            DisplayFont.DrawString(null, text, TextShadowRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)ShadowColor);
            DisplayFont.DrawString(null, text, TextRect, DrawTextFormat.Left | DrawTextFormat.Top, (int)TextColor);

            TextRect.Y += FontSizeRect.Height;
            TextShadowRect.Y += FontSizeRect.Height;

            if (stringSizeRect.Width + 10 > Width)
            {
                Width = stringSizeRect.Width + 10;
                Panel.ExpandDock(this);
            }

            if (TextRect.Y + 10 > Height)
            {
                Height = TextRect.Y + 10;
                Panel.ExpandDock(this);
            }
        }
    }
}
