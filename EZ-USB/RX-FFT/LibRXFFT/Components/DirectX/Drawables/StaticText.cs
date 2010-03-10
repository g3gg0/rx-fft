using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX.Direct3D9;
using SlimDX;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public class StaticText : DirectXDrawable
    {
        public Color4 TextColor = Color.Red;
        public Color4 ShadowColor = Color.DarkRed;
        public bool Pulsing = false;
        public bool Active = true;
        private string Text
        {
            get { return _Text; }
            set
            {
                _Text = value;
                PositionUpdated = true;
            }
        }


        protected SlimDX.Direct3D9.Font DisplayFont;
        protected string _Text = "";
        protected int UpdateCounter = 0;
        protected Rectangle SizeRect = new Rectangle();
        protected Vertex[] BorderVertexes = new Vertex[32];
        protected int BorderVertexesUsed = 0;
        protected Vertex[] BodyVertexes = new Vertex[32]; /* 4 */
        protected int BodyVertexesUsed = 0;


        public StaticText(DirectXPlot mainPlot, string text)
            : base(mainPlot)
        {
            Text = text;
        }

        public override void AllocateResources()
        {
            DisplayFont = new SlimDX.Direct3D9.Font(MainPlot.Device, new System.Drawing.Font("Arial", 14));
            PositionUpdated = true;
        }

        public override void Render()
        {
            if (!Active)
            {
                return;
            }

            int xPos = AbsoluteXPosition;
            int yPos = AbsoluteYPosition;

            if (PositionUpdated)
            {
                DisplayFont.MeasureString(null, Text, DrawTextFormat.Center, ref SizeRect);
                AbsoluteHeight = SizeRect.Height;
                AbsoluteWidth = SizeRect.Width;
            }

            if (xPos < 0)
            {
                xPos += MainPlot.DirectXWidth - AbsoluteWidth;
            }

            if (yPos < 0)
            {
                yPos += MainPlot.DirectXHeight - AbsoluteHeight;
            }

            if (PositionUpdated)
            {
                PositionUpdated = false;

                BorderVertexesUsed = 0;
                BorderVertexesUsed = BuildRectangle(BorderVertexes, BorderVertexesUsed, xPos - 7, xPos + 7 + AbsoluteWidth, yPos - 4, yPos + 4 + AbsoluteHeight, 0x7FFF0000);
                BorderVertexesUsed = BuildRectangle(BorderVertexes, BorderVertexesUsed, xPos - 6, xPos + 6 + AbsoluteWidth, yPos - 3, yPos + 3 + AbsoluteHeight, 0xFFFF0000);
                BorderVertexesUsed = BuildRectangle(BorderVertexes, BorderVertexesUsed, xPos - 5, xPos + 5 + AbsoluteWidth, yPos - 2, yPos + 2 + AbsoluteHeight, 0x7FFF0000);

                BodyVertexesUsed = 0;
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, xPos - 6, xPos + 6 + AbsoluteWidth, yPos - 3, yPos + 3 + AbsoluteHeight, 0x1FFF0000);
            }

            if (Pulsing)
            {
                double maxAngle = 10 * Math.PI;
                /* -1 .. 1 -> 0.5 .. 1 */
                double sinVal = (Math.Sin(UpdateCounter++ / maxAngle) + 1) / 4 + 0.5;

                TextColor.Alpha = (float)Math.Max(0, Math.Min(1, sinVal));
                ShadowColor.Alpha = (float)Math.Max(0, TextColor.Alpha - 0.5);
            }
            else
            {
                TextColor.Alpha = 1;
                ShadowColor.Alpha = 1;
            }

            MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, BodyVertexes);
            if (BorderVertexesUsed > 0)
                MainPlot.Device.DrawUserPrimitives(PrimitiveType.LineList, BorderVertexesUsed / 2, BorderVertexes);

            DisplayFont.DrawString(null, Text, xPos + 2, yPos + 2, ShadowColor);
            DisplayFont.DrawString(null, Text, xPos, yPos, TextColor);
        }

        public override void ReleaseResources()
        {
            if (DisplayFont != null)
                DisplayFont.Dispose();
            DisplayFont = null;
        }
    }
}
