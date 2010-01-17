using System;
using System.Drawing;
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

        private Brush BrushOver = Brushes.Red;
        private Brush BrushSignal = Brushes.Green;
        private Pen PenLine = Pens.Yellow;
        private Pen PenBorder = Pens.Black;

        public PowerBar()
        {
            InitializeComponent();
            SetEnabledColors(Enabled);
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

        protected override void OnEnabledChanged(EventArgs e)
        {
            SetEnabledColors(Enabled);
            base.OnEnabledChanged(e);
        }

        private void SetEnabledColors(bool enabled)
        {
            if (enabled)
            {
                BrushOver = Brushes.Red;
                BrushSignal = Brushes.Green;
                PenLine = Pens.Yellow;
                PenBorder = Pens.Black;
            }
            else
            {
                BrushOver = Brushes.DarkGray;
                BrushSignal = Brushes.Gray;
                PenLine = Pens.White;
                PenBorder = Pens.Black;
            }

            NeedsUpdate = true;
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
            BarGraphics.FillRectangle(BrushOver, 0, 0, BarBitmap.Width, BarBitmap.Height);
            BarGraphics.FillRectangle(BrushSignal, 0, 0, (float)(BarBitmap.Width * Amplitude), (float)BarBitmap.Height);
            BarGraphics.DrawLine(PenLine, (float)(BarBitmap.Width * LinePosition), 0, (float)(BarBitmap.Width * LinePosition), (float)BarBitmap.Height);
            BarGraphics.DrawRectangle(PenBorder, 0, 0, BarBitmap.Width - 1, BarBitmap.Height - 1);
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
