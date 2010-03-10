using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX.Direct3D9;
using SlimDX;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public class PowerBar : DirectXDrawable
    {
        public Color4 TextColor = Color.Red;
        public Color4 ShadowColor = Color.DarkRed;
        public bool Active = true;
        public double Power
        {
            get { return _Power; }
            set
            {
                _Power = value;
                PowerUpdated = true;
            }
        }

        protected double _Power = 0;
        protected bool PowerUpdated = false;
        protected SlimDX.Direct3D9.Font DisplayFont;
        protected int UpdateCounter = 0;
        protected Vertex[] BorderVertexes = new Vertex[64];
        protected int BorderVertexesUsed = 0;
        protected Vertex[] BodyVertexes = new Vertex[64];
        protected int BodyVertexesUsed = 0;


        public PowerBar(DirectXPlot mainPlot)
            : base(mainPlot)
        {
            AbsoluteWidth = 60;
            AbsoluteHeight = 10;
        }

        public override void AllocateResources()
        {
            DisplayFont = new SlimDX.Direct3D9.Font(MainPlot.Device, new System.Drawing.Font("Arial", 8));
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

            if (xPos < 0)
            {
                xPos += MainPlot.DirectXWidth - AbsoluteWidth;
            }

            if (yPos < 0)
            {
                yPos += MainPlot.DirectXHeight - AbsoluteHeight;
            }

            if (PositionUpdated || PowerUpdated)
            {
                PositionUpdated = false;
                PowerUpdated = false;

                BorderVertexesUsed = 0;
                BorderVertexesUsed = BuildRectangle(BorderVertexes, BorderVertexesUsed, xPos, xPos + AbsoluteWidth, yPos, yPos + AbsoluteHeight, 0xFF00FFFF);
                BorderVertexesUsed = BuildRectangle(BorderVertexes, BorderVertexesUsed, xPos - 1, xPos + AbsoluteWidth + 1, yPos - 1, yPos + 1 + AbsoluteHeight, 0x7F00FFFF);

                BodyVertexesUsed = 0;
                // BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, xPos, (int)(xPos + Width * Power), yPos, yPos + Height, 0x3F00FFFF);

                /* dark background */
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, xPos, (int)(xPos + AbsoluteWidth), yPos, yPos + AbsoluteHeight, 0x7F000000);

                /* background gradient */
                double relPos = 0;
                double delta = 0.33f;

                /* gradient blue -> cyan */
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + AbsoluteWidth * relPos), (int)(xPos + AbsoluteWidth * (relPos + delta)), yPos, yPos + AbsoluteHeight, 0x3F0000FF, 0x3F00FFFF);
                relPos += delta;

                /* gradient cyan -> yellow */
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + AbsoluteWidth * relPos), (int)(xPos + AbsoluteWidth * (relPos + delta)), yPos, yPos + AbsoluteHeight, 0x3F00FFFF, 0x3FFFFF00);
                relPos += delta;

                /* gradient yellow -> red */
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + AbsoluteWidth * relPos), (int)(xPos + AbsoluteWidth * (relPos + delta)), yPos, yPos + AbsoluteHeight, 0x3FFFFF00, 0x3FFF0000);
                relPos = 0;

                /* bar gradient */
                double maxDelta = 0;
                double colorPct = 0;

                if (Power > 0)
                {
                    /* gradient blue -> cyan */
                    maxDelta = Math.Max(0, Math.Min(delta, Power));
                    colorPct = maxDelta / delta;
                    BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + AbsoluteWidth * relPos), (int)(xPos + AbsoluteWidth * (relPos + maxDelta)), yPos, yPos + AbsoluteHeight, 0xFF0000FF, 0xFF0000FF | (((uint)(255.0f * colorPct)) << 8));
                    relPos += delta;

                }
                if (Power > 0.33f)
                {
                    /* gradient cyan -> yellow */
                    maxDelta = Math.Max(0, Math.Min(delta, Power - relPos));
                    colorPct = maxDelta / delta;
                    BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + AbsoluteWidth * relPos), (int)(xPos + AbsoluteWidth * (relPos + maxDelta)), yPos, yPos + AbsoluteHeight, 0xFF00FFFF, 0xFF00FF00 | (((uint)(255.0f * colorPct)) << 16) | ((uint)(255.0f * (1 - colorPct))));
                    relPos += delta;
                }

                if (Power > 0.66f)
                {
                    /* gradient yellow -> red */
                    maxDelta = Math.Max(0, Math.Min(delta, Power - relPos));
                    colorPct = maxDelta / delta;
                    BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + AbsoluteWidth * relPos), (int)(xPos + AbsoluteWidth * (relPos + maxDelta)), yPos, yPos + AbsoluteHeight, 0xFFFFFF00, 0xFFFF0000 | (((uint)(255.0f * (1 - colorPct))) << 8));
                }
            }

            if (BodyVertexesUsed - 2 > 0)
                MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, BodyVertexesUsed - 2, BodyVertexes);
            if (BorderVertexesUsed > 0)
                MainPlot.Device.DrawUserPrimitives(PrimitiveType.LineList, BorderVertexesUsed / 2, BorderVertexes);
        }

        public override void ReleaseResources()
        {
            if (DisplayFont != null)
                DisplayFont.Dispose();
            DisplayFont = null;
        }
    }
}
