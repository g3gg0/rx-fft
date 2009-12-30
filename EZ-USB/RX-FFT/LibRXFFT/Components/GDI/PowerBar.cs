using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LibRXFFT.Components.GDI
{
    public partial class PowerBar : Control
    {
        private bool NeedsUpdate = true;
        private double _Amplitude = 0;
        private double _LinePosition = 0;
        
        private Bitmap BarBitmap;
        private Graphics BarGraphics;

        public PowerBar()
        {
            InitializeComponent();
            //CreateGraphics();
        }

        protected override void OnResize(EventArgs e)
        {
            CreateGraphics();
            base.OnResize(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            CreateGraphics();
            base.OnSizeChanged(e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (NeedsUpdate)
            {
                UpdateGraphics();
            }
            pe.Graphics.DrawImageUnscaled(BarBitmap, 0, 0); 
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected void CreateGraphics()
        {
            BarBitmap = new Bitmap(Width, Height);
            BarGraphics = Graphics.FromImage(BarBitmap);
        }

        protected void UpdateGraphics()
        {
            BarGraphics.FillRectangle(Brushes.Red, 0, 0, BarBitmap.Width, BarBitmap.Height);
            BarGraphics.FillRectangle(Brushes.Green, 0, 0, (float)(BarBitmap.Width * Amplitude), (float)BarBitmap.Height);
            BarGraphics.DrawLine(Pens.Yellow, (float)(BarBitmap.Width * LinePosition), 0, (float)(BarBitmap.Width * LinePosition), (float)BarBitmap.Height);
            BarGraphics.DrawRectangle(Pens.Black, 0, 0, BarBitmap.Width - 1, BarBitmap.Height - 1);
        }

        public double Amplitude
        {
            get { return _Amplitude; }
            set 
            { 
                _Amplitude = Math.Min(1, Math.Max(0, value));
                NeedsUpdate = true;
            }
        }

        public double LinePosition
        {
            get { return _LinePosition; }
            set
            {
                _LinePosition = Math.Min(1, Math.Max(0, value));
                NeedsUpdate = true;
            }
        }
    }
}
