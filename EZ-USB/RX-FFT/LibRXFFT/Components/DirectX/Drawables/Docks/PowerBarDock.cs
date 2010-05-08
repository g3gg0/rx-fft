using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using System.Drawing;
using SlimDX.Direct3D9;

namespace LibRXFFT.Components.DirectX.Drawables.Docks
{
    public class PowerBarDock : Dock
    {
        public Color4 TextColor = Color.Red;
        public Color4 ShadowColor = Color.Black;

        public string Unit = "dB";
        public double Power
        {
            get { return _Power; }
            set
            {
                _Power = value;
                PowerUpdated = true;
            }
        }

        public override float Height
        {
            get { return Private.TitleHitRect.Height; }
        }
        public override bool Sticky
        {
            get { return true; }
        }
        public override bool HideOthers
        {
            get { return false; }
        }

        protected double _Power = 0;
        protected bool PowerUpdated = false;
        protected SlimDX.Direct3D9.Font DisplayFont;
        protected int UpdateCounter = 0;
        protected Vertex[] BorderVertexes = new Vertex[128];
        protected int BorderVertexesUsed = 0;
        protected Vertex[] BodyVertexes = new Vertex[128];
        protected int BodyVertexesUsed = 0;
        protected Rectangle TextRect = new Rectangle();
        protected Rectangle TextShadowRect = new Rectangle();


        public PowerBarDock(DockPanel panel)
            : base(panel)
        {
            Title = "RSSI";
            Width = 100;
        }


        public override void AllocateResources()
        {
            DisplayFont = new SlimDX.Direct3D9.Font(Panel.MainPlot.Device, new System.Drawing.Font("Courier New", 15, FontStyle.Bold));
            PositionUpdated = true;
        }

        public override void ReleaseResources()
        {
            if (DisplayFont != null)
                DisplayFont.Dispose();
            DisplayFont = null;
        }

        public override void Render()
        {
            if (State == eDockState.Hidden)
            {
                return;
            }

            int xPos = XPosition;
            int yPos = YPosition;
            double pct = Math.Min(1, Math.Max(0, 1 - (-Power / 60)));

            if (PositionUpdated || PowerUpdated)
            {
                PositionUpdated = false;
                PowerUpdated = false;

                BodyVertexesUsed = 0;

                /* dark background */
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, xPos, (int)(xPos + Width), yPos, yPos + Height, 0x7F000000);

                /* background gradient */
                double relPos = 0;
                double delta = 0.33f;

                /* gradient blue -> cyan */
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + Width * relPos), (int)(xPos + Width * (relPos + delta)), yPos, yPos + Height, 0x3F0000FF, 0x3F00FFFF);
                relPos += delta;

                /* gradient cyan -> yellow */
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + Width * relPos), (int)(xPos + Width * (relPos + delta)), yPos, yPos + Height, 0x3F00FFFF, 0x3FFFFF00);
                relPos += delta;

                /* gradient yellow -> red */
                BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + Width * relPos), (int)(xPos + Width * (relPos + delta)), yPos, yPos + Height, 0x3FFFFF00, 0x3FFF0000);
                relPos = 0;

                /* bar gradient */
                double maxDelta = 0;
                double colorPct = 0;

                if (pct > 0)
                {
                    /* gradient blue -> cyan */
                    maxDelta = Math.Max(0, Math.Min(delta, pct));
                    colorPct = maxDelta / delta;
                    BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + Width * relPos), (int)(xPos + Width * (relPos + maxDelta)), yPos, yPos + Height, 0xFF0000FF, 0xFF0000FF | (((uint)(255.0f * colorPct)) << 8));
                    relPos += delta;
                }

                if (pct > 0.33f)
                {
                    /* gradient cyan -> yellow */
                    maxDelta = Math.Max(0, Math.Min(delta, pct - relPos));
                    colorPct = maxDelta / delta;
                    BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + Width * relPos), (int)(xPos + Width * (relPos + maxDelta)), yPos, yPos + Height, 0xFF00FFFF, 0xFF00FF00 | (((uint)(255.0f * colorPct)) << 16) | ((uint)(255.0f * (1 - colorPct))));
                    relPos += delta;
                }

                if (pct > 0.66f)
                {
                    /* gradient yellow -> red */
                    maxDelta = Math.Max(0, Math.Min(delta, pct - relPos));
                    colorPct = maxDelta / delta;
                    BodyVertexesUsed = BuildFilledRectangle(BodyVertexes, BodyVertexesUsed, (int)(xPos + Width * relPos), (int)(xPos + Width * (relPos + maxDelta)), yPos, yPos + Height, 0xFFFFFF00, 0xFFFF0000 | (((uint)(255.0f * (1 - colorPct))) << 8));
                }
            }

            if (BodyVertexesUsed - 2 > 0)
                Panel.MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, BodyVertexesUsed - 2, BodyVertexes);

            TextRect.X = XPosition;
            TextRect.Y = YPosition;
            TextRect.Width = (int)Width;
            TextRect.Height = (int)Height;
            TextShadowRect.X = XPosition + 1;
            TextShadowRect.Y = YPosition + 1;
            TextShadowRect.Width = (int)Width;
            TextShadowRect.Height = (int)Height;

            string text = Power.ToString("#0.0 ") + Unit;

            DisplayFont.DrawString(null, text, TextShadowRect, DrawTextFormat.Center | DrawTextFormat.VerticalCenter, (int)ShadowColor);
            DisplayFont.DrawString(null, text, TextRect, DrawTextFormat.Center | DrawTextFormat.VerticalCenter, (int)TextColor);
        }

    }
}
