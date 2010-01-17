using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using LibRXFFT.Libraries;

using Timer = System.Timers.Timer;


namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXWaveformDisplayOld : DirectXPlot
    {
        readonly Thread DisplayThread;
        readonly ArrayList SampleValues = new ArrayList();
        private bool NeedsUpdate = false;
        
        public int MaxSamples { get; set; }


        public DirectXWaveformDisplayOld()
        {
            MaxSamples = 10000;
            YZoomFactor = 1.0f;
            XZoomFactor = 2.0f;
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;
            ColorFont = Color.DarkCyan;

            EventActions[eUserEvent.MouseDragY] = eUserAction.None;

            InitializeComponent();
            try
            {
                InitializeDirectX();
            }
            catch (SlimDX.Direct3D9.Direct3D9Exception e)
            {
                MessageBox.Show("Failed initializing DirectX." + Environment.NewLine + e.ToString());
            }

            DisplayThread = new Thread(DisplayFunc);
            DisplayThread.Name = "WaveformDisplay Display Thread";
            DisplayThread.Start();
        }


        public void ClearProcessData(double[] samples)
        {
            lock (SampleValues)
            {
                SampleValues.Clear();
                for (int pos = 0; pos < samples.Length; pos++)
                    SampleValues.Add(samples[pos]);

                NeedsUpdate = true;
            }
        }

        public void ProcessData(double power)
        {
            lock (SampleValues)
            {
                SampleValues.Add(power);
                NeedsUpdate = true;
            }
        }

        public void ProcessData(double[] samples)
        {
            lock (SampleValues)
            {
                for (int pos = 0; pos < samples.Length; pos++)
                    SampleValues.Add(samples[pos]);

                NeedsUpdate = true;
            }
        }

        public void ProcessData(byte[] dataBuffer, int channels, int channel)
        {
            if (channels == 0 || channel == 0 || channel > channels)
                return;

            lock (SampleValues)
            {
                int bytePerSample = channels * 2;
                int byteOffset = (channel - 1) * 2;

                for (int pos = 0; pos < dataBuffer.Length / bytePerSample; pos++)
                {
                    SampleValues.Add(ByteUtil.getDoubleFromBytes(dataBuffer, byteOffset + bytePerSample * pos));
                }
                NeedsUpdate = true;
            }
        }



        protected override void RenderOverlay()
        {
            if (UserEventCallback != null)
                UserEventCallback(eUserEvent.RenderOverlay, 0);
        }

        private void DisplayFunc()
        {
            while (true)
            {
                lock (SampleValues)
                {
                    if (NeedsUpdate)
                    {
                        NeedsUpdate = false;

                        if (SampleValues.Count > 0)
                        {
                            int samples = SampleValues.Count;

                            lock (LinePointsLock)
                            {
                                if (LinePoints == null || LinePoints.Length < samples)
                                    LinePoints = new Point[samples];

                                for (int pos = 0; pos < samples; pos++)
                                {
                                    double sampleValue = (double)SampleValues[pos];
                                    double posX = pos;
                                    double posY = sampleValue * DirectXHeight;

                                    LinePoints[pos].X = posX;
                                    LinePoints[pos].Y = posY;
                                }

                                if (SampleValues.Count > MaxSamples)
                                {
                                    int removeCount = SampleValues.Count - MaxSamples;
                                    SampleValues.RemoveRange(0, removeCount);
                                }

                                LinePointEntries = samples;
                                LinePointsUpdated = true;
                            }
                        }
                    }
                }
                Render();
                Thread.Sleep(10);
            }
        }

        public void Clear()
        {
            lock (SampleValues)
            {
                SampleValues.Clear();
                NeedsUpdate = true;
            }
        }

    }
}