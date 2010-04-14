using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using SlimDX.Direct3D9;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXPhaseDisplay : DirectXPlot
    {
        readonly Thread DisplayThread;
        readonly ArrayList SampleValues = new ArrayList();


        double lastPhase = 0;

        public string DisplayName { get; set; }

        public double ZoomFactor { get; set; }
        public bool ShowFPS { get; set; }
        public bool UseLines { get; set; }
        public int StartSample { get; set; }
        public int MaxSamples { get; set; }

        public DirectXPhaseDisplay() : this(false)
        {
        }

        public DirectXPhaseDisplay(bool slaveMode)
        {
            UseLines = true;
            MaxSamples = 10000;
            ZoomFactor = 1.0f;
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;

            InitializeComponent();
            try
            {
                InitializeDirectX();
            }
            catch (Direct3D9Exception e)
            {
                MessageBox.Show("Failed initializing DirectX." + Environment.NewLine + e.ToString());
            }
            InitFields();

            if (!slaveMode)
            {
                DisplayThread = new Thread(DisplayFunc);
                DisplayThread.Name = "PhaseDisplay Display Thread";
                DisplayThread.Start();
            }
        }

        private void InitFields()
        {
            lock (SampleValues)
            {
                LinePoints = new Point[DirectXWidth];
                for (int pos = 0; pos < LinePoints.Length; pos++)
                    LinePoints[pos] = new Point();
            }
        }



        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0 && ZoomFactor < 20.0f)
                ZoomFactor *= 1.1f;

            if (e.Delta < 0 && ZoomFactor > 0.01f)
                ZoomFactor /= 1.1f;
        }

        protected override void OnResize(EventArgs e)
        {
            InitFields();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            InitFields();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
        }

        public void ProcessData(double[] samples)
        {
            lock (SampleValues)
            {
                for (int pos = 0; pos < samples.Length; pos++)
                {
                    SampleValues.Add(samples[pos]);
                }
            }
        }

        public void ProcessData(byte[] dataBuffer)
        {

            lock (SampleValues)
            {
                int bytePerSamplePair = 4;

                for (int pos = 0; pos < dataBuffer.Length / bytePerSamplePair; pos++)
                {
                    double I = ByteUtil.getDoubleFromBytes(dataBuffer, bytePerSamplePair * pos);
                    double Q = ByteUtil.getDoubleFromBytes(dataBuffer, bytePerSamplePair * pos + 2);

                    double phase = Math.Atan2(I, Q);
                    double lastPhaseEffective = lastPhase%(2*Math.PI);
                    double fullRotationAngle = lastPhase - lastPhaseEffective;

                    if (phase - lastPhaseEffective < -(Math.PI / 2))
                        phase += Math.PI;

                    if (phase - lastPhaseEffective > Math.PI / 2)
                        phase -= Math.PI;

                    phase += fullRotationAngle;
                    lastPhase = phase;

                    SampleValues.Add(phase);
                }
            }
        }

        public override void PrepareLinePoints()
        {
            lock (SampleValues)
            {
                if (SampleValues.Count > 0)
                {
                    int startPos = StartSample;
                    int samples = DirectXWidth;

                    if (startPos + samples > SampleValues.Count)
                        startPos = SampleValues.Count - samples;

                    if (startPos < 0)
                    {
                        startPos = 0;
                        samples = SampleValues.Count;
                    }

                    for (int pos = 0; pos < samples; pos++)
                    {
                        double sampleValue = (double)SampleValues[startPos + pos];
                        int posX = pos;
                        int posY = (int)(DirectXHeight - (sampleValue * ZoomFactor * DirectXHeight)) / 2;

                        posY = Math.Min(posY, DirectXHeight - 1);
                        posY = Math.Max(posY, 0);

                        LinePoints[pos].X = posX;
                        LinePoints[pos].Y = posY;
                    }

                    if (SampleValues.Count > MaxSamples)
                    {
                        int removeCount = SampleValues.Count - MaxSamples;
                        SampleValues.RemoveRange(0, removeCount);
                    }

                    CreateVertexBufferForPoints(LinePoints);
                }
            }
        }

        private void DisplayFunc()
        {
            while (true)
            {
                if (SlavePlot != null)
                    SlavePlot.PrepareLinePoints();
                PrepareLinePoints();

                if (SlavePlot != null)
                    SlavePlot.Render();
                Render();

                Thread.Sleep(1000/60);
            }
        }
    }
}