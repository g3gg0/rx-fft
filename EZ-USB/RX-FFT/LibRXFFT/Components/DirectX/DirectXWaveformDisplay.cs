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
    public partial class DirectXWaveformDisplay : DirectXPlot
    {
        public bool fullScreen = false;

        readonly Thread DisplayThread;
        readonly ArrayList SampleValues = new ArrayList();
        private DisplayFuncState DisplayTimerState;
        private bool needsUpdate = false;


        public string DisplayName { get; set; }
        public bool ShowFPS { get; set; }
        public bool UseLines { get; set; }
        public int MaxSamples { get; set; }


        public DirectXWaveformDisplay()
        {
            UseLines = true;
            MaxSamples = 10000;
            YZoomFactor = 1.0f;
            XZoomFactor = 2.0f;
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;

            InitializeComponent();
            InitializeDirectX();
            InitFields();

            DisplayTimerState = new DisplayFuncState();
            DisplayThread = new Thread(DisplayFunc);
            DisplayThread.Start();
        }

        private void InitFields()
        {/*
            lock (SampleValues)
            {
                LinePoints = new Point[DirectXWidth];
                for (int pos = 0; pos < LinePoints.Length; pos++)
                    LinePoints[pos] = new Point(0, 0);
            }*/
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
                int bytePerSample = channels * 2;
                int byteOffset = (channel - 1) * 2;

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



        private void DisplayFunc()
        {
            DisplayFuncState s = DisplayTimerState;


            while (true)
            {
                lock (SampleValues)
                {
                    if (needsUpdate)
                    {
                        needsUpdate = false;

                        if (SampleValues.Count > 0)
                        {
                            int samples = SampleValues.Count;

                            if (LinePoints == null || LinePoints.Length < samples)
                                LinePoints = new Point[samples];

                            for (int pos = 0; pos < samples; pos++)
                            {
                                double sampleValue = (double) SampleValues[pos];
                                double posX = pos;
                                double posY = sampleValue;

                                LinePoints[pos].X = posX;
                                LinePoints[pos].Y = posY;
                            }

                            if (SampleValues.Count > MaxSamples)
                            {
                                int removeCount = SampleValues.Count - MaxSamples;
                                SampleValues.RemoveRange(0, removeCount);
                            }

                            LinePointEntries = samples;
                            DataUpdated = true;
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
                needsUpdate = true;
            }
        }


    }
}