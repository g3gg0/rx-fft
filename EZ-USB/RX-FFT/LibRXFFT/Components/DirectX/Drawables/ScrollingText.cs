using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public class ScrollingText : DirectXDrawable
    {        
        private SlimDX.Direct3D9.Font DisplayFont;
        private int XPosition = 0;
        private string Text;

        public ScrollingText(DirectXPlot mainPlot, string text)
            : base(mainPlot)
        {
            Text = text;
        }

        public override void AllocateResources()
        {
            DisplayFont = new SlimDX.Direct3D9.Font(MainPlot.Device, new System.Drawing.Font("Arial", 20));
        }

        public override void Render()
        {
            DisplayFont.DrawString(null, Text, XPosition, 30, Color.Red);
            XPosition++;
            XPosition += (int)Math.Sin(XPosition)*2;
            XPosition %= MainPlot.DirectXWidth;
        }

        public override void ReleaseResources()
        {
            if (DisplayFont != null)
                DisplayFont.Dispose();
            DisplayFont = null;
        }
    }
}
