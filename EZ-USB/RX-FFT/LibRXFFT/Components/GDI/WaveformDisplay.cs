using System;
using System.Collections;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using Timer=System.Timers.Timer;

namespace LibRXFFT.Components.GDI
{
    public partial class WaveformDisplay : UserControl
    {
        Bitmap Image;
        Bitmap ImageBuffer;
        Graphics ImageGraph;
        Graphics ImageBufferGraph;

        readonly Color colorFG;
        readonly Color colorBG;
        readonly Pen LinePen;
        readonly Timer DisplayTimer;
        readonly ArrayList SampleValues = new ArrayList();
        private DisplayFuncState DisplayTimerState;
        private bool needsUpdate = false;

        public string DisplayName { get; set; }
        public double ZoomFactor { get; set; }
        public bool ShowFPS { get; set; }
        public bool UseLines { get; set; }
        public int StartSample { get; set; }
        public int MaxSamples { get; set; }

        public WaveformDisplay()
        {
            UseLines = true;
            MaxSamples = 10000;
            ZoomFactor = 1.0f;
            colorFG = Color.Cyan;
            colorBG = Color.Black;
            LinePen = new Pen(colorFG);

            InitializeComponent();
            Image = new Bitmap(Width, Height);
            ImageGraph = Graphics.FromImage(Image);
            ImageBuffer = new Bitmap(Width, Height);
            ImageBufferGraph = Graphics.FromImage(ImageBuffer);

            DisplayTimerState = new DisplayFuncState();
            DisplayTimer = new Timer();
            DisplayTimer.Elapsed += DisplayFunc;
            DisplayTimer.Interval = 20;
            DisplayTimer.Start();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ( e.Delta > 0 && ZoomFactor < 20.0f)
                ZoomFactor *= 1.1f;

            if (e.Delta < 0 && ZoomFactor > 0.01f)
                ZoomFactor /= 1.1f;
        }

        protected override void OnResize(EventArgs e)
        {
            lock (SampleValues)
            {
                Image = new Bitmap(Width, Height);
                ImageGraph = Graphics.FromImage(Image);
                ImageBuffer = new Bitmap(Width, Height);
                ImageBufferGraph = Graphics.FromImage(ImageBuffer);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            lock (SampleValues)
            {
                Image = new Bitmap(Width, Height);
                ImageGraph = Graphics.FromImage(Image);
                ImageBuffer = new Bitmap(Width, Height);
                ImageBufferGraph = Graphics.FromImage(ImageBuffer);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            lock (SampleValues)
            {
                e.Graphics.DrawImage(Image, 0, 0);
            }
        }

        public void ClearProcessData(double[] samples)
        {
            lock (SampleValues)
            {
                SampleValues.Clear();
                for (int pos = 0; pos < samples.Length; pos++)
                    SampleValues.Add(samples[pos]);

                needsUpdate = true;
            }
        }

        public void ProcessData(double[] samples)
        {
            lock (SampleValues)
            {
                for (int pos = 0; pos < samples.Length; pos++)
                    SampleValues.Add(samples[pos]);

                needsUpdate = true;
            }
        }

        public void ProcessData(byte[] dataBuffer, int channels, int channel)
        {
            if (channels == 0 || channel == 0 || channel > channels)
                return;

            lock (SampleValues)
            {
                int bytePerSample = channels*2;
                int byteOffset = (channel-1)*2;

                for (int pos = 0; pos < dataBuffer.Length / bytePerSample; pos++)
                {
                    SampleValues.Add(ByteUtil.getDoubleFromBytes(dataBuffer, byteOffset + bytePerSample * pos));
                }
                needsUpdate = true;
            }
        }

        class DisplayFuncState
        {
            protected internal Font TextFont;
            protected internal DateTime StartTime;
            protected internal long FrameNumber;
            protected internal double FPS;

            public DisplayFuncState()
            {
                FPS = 0;
                FrameNumber = 0;
                TextFont = new Font("Arial", 16);
                StartTime = DateTime.Now;
            }
        }

        private void DisplayFunc(object state, ElapsedEventArgs e)
        {
            DisplayFuncState s = DisplayTimerState;

            if (!needsUpdate)
                return;

            lock (SampleValues)
            {
                needsUpdate = false;

                if (SampleValues.Count > 0)
                {
                    ImageBufferGraph.Clear(colorBG);

                    int startPos = StartSample;
                    int samples = Width;

                    if (startPos + samples > SampleValues.Count)
                        startPos = SampleValues.Count - samples;

                    if (startPos < 0)
                    {
                        startPos = 0;
                        samples = SampleValues.Count;
                    }

                    int lastX = 0;
                    int lastY = 0;

                    for (int pos = 0; pos < samples; pos++)
                    {
                        double sampleValue = (double) SampleValues[startPos + pos];
                        int posX = pos;
                        int posY = (int)(Height - (sampleValue * ZoomFactor * Height))/2;
                        posY = Math.Min(posY, Height - 1);
                        posY = Math.Max(posY, 0);

                        if (UseLines && pos > 0)
                            ImageBufferGraph.DrawLine(LinePen, lastX, lastY, posX, posY);
                        else
                            ImageBuffer.SetPixel(posX, posY, colorFG);

                        lastX = posX;
                        lastY = posY;
                    }

                    if (SampleValues.Count > MaxSamples)
                    {
                        int removeCount = SampleValues.Count - MaxSamples;
                        SampleValues.RemoveRange(0, removeCount);
                    }

                    if (ShowFPS)
                    {
                        if (s.FrameNumber++ > 500 && s.FrameNumber % 20 == 0)
                            s.FPS = s.FrameNumber/DateTime.Now.Subtract(s.StartTime).TotalSeconds;
                        ImageBufferGraph.DrawString("FPS: " + (int) s.FPS, s.TextFont, Brushes.Cyan, 10, 10);
                    }
                    if (!string.IsNullOrEmpty(DisplayName))
                        ImageBufferGraph.DrawString(DisplayName, s.TextFont, Brushes.Cyan, 10, 10);

                    ImageGraph.DrawImageUnscaled(ImageBuffer, 0, 0);
                    Invalidate();
                }
            }
        }

        public void Clear()
        {
            lock (SampleValues)
            {
                SampleValues.Clear();
                needsUpdate = true;
            }
        }


    }
}