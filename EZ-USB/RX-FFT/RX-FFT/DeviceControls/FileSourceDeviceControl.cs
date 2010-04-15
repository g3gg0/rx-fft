using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries;
using System.Threading;
using LibRXFFT.Libraries.Timers;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.USB_RX.Tuners;

namespace RX_FFT.DeviceControls
{
    public partial class FileSourceDeviceControl : Form, DeviceControl
    {
        private FileSampleSource Source;
        private bool Repeat = false;
        private bool Closing = false;
        private double LastPosition = 0;
        private object ReadTick = new object();

        private string FileName = "none";

        private AccurateTimer Timer;

        public FileSourceDeviceControl()
        {
            InitializeComponent();

            FileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileName = dlg.FileName;
                Source = new FileSampleSource(FileName, 1);
                Source.ForwardEnabled = true;

                trackBar.Maximum = (int)(Source.GetTotalTime() / 0.01f);

                Timer = new AccurateTimer();
                Timer.Periodic = true;
                Timer.Interval = (uint)((double)(Source.OutputBlockSize * 1000.0f) / Source.OutputSamplingRate);
                Timer.Timer += new EventHandler(Timer_Timer);
                Timer.Start();

                UpdateDisplay();

                Show();
            }
        }

        void Timer_Timer(object sender, EventArgs e)
        {
            lock (ReadTick)
            {
                Monitor.Pulse(ReadTick);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Closing = true;

            e.Cancel = false;
            TransferMode = eTransferMode.Stopped;
            Timer.Stop();

            if (DeviceDisappeared != null)
            {
                DeviceDisappeared(this, null);
            }
            base.OnClosing(e);
        }

        #region DeviceControl Member

        public bool AllowsMultipleReaders
        {
            get
            {
                return true;
            }
        }

        public int SamplesPerBlock
        {
            get
            {
                if (Source != null)
                {
                    return Source.SamplesPerBlock;
                }
                return 0;
            }
            set
            {
                if (Source != null)
                {
                    Source.SamplesPerBlock = value;
                }
            }
        }

        public event EventHandler TransferModeChanged;
        private eTransferMode CurrentTransferMode;


        public eTransferMode TransferMode
        {
            get
            {
                return CurrentTransferMode;
            }
            set
            {
                switch (value)
                {
                    case eTransferMode.Stopped:
                        BeginInvoke(new Action(() => { btnPlayPause.Text = "Play"; }));
                        break;
                    case eTransferMode.Stream:
                    case eTransferMode.Block:
                        BeginInvoke(new Action(() => { btnPlayPause.Text = "Pause"; }));
                        break;
                }

                CurrentTransferMode = value;
                if (TransferModeChanged != null)
                {
                    TransferModeChanged(this, null);
                }
            }
        }

        public LibRXFFT.Libraries.SampleSources.SampleSource SampleSource
        {
            get { return Source; }
        }

        public bool Connected
        {
            get { return (Source != null); }
        }

        public string ErrorMessage
        {
            get { return "None"; }
        }

        public double BlocksPerSecond
        {
            get;
            set;
        }

        public bool ScanFrequenciesEnabled
        {
            get;
            set;
        }

        public int ShmemChannel
        {
            get 
            {
                if (Source != null)
                {
                    return Source.OutputShmemChannel.DstChan;
                }
                return 0;
            }
        }

        public bool ReadBlock()
        {
            bool success;

            do
            {
                lock (ReadTick)
                {
                    Monitor.Wait(ReadTick);
                }

                if (Closing)
                {
                    return false;
                }

            } while (TransferMode == eTransferMode.Stopped);

            success = SampleSource.Read();

            /* if failed, maybe we reached the end */
            if (!success)
            {
                if (Repeat)
                {
                    SampleSource.Restart();
                    success = SampleSource.Read();
                }
                else
                {
                    /* seems to have reached the end. stop playback */
                    TransferMode = eTransferMode.Stopped;
                }
            }

            if (Source.GetPosition() != LastPosition)
            {
                LastPosition = Source.GetPosition();
                BeginInvoke(new Action(() => { UpdateDisplay(); }));
            }

            return success;
        }

        private void UpdateDisplay()
        {
            int newPos = (int)(trackBar.Maximum * Source.GetPosition());
            if(newPos!= trackBar.Value)
            {
                trackBar.Value = newPos;
            }

            lblStartPos.Text = "0 s";
            lblEndPos.Text = FrequencyFormatter.TimeToString(Source.GetTotalTime());
            lblCurrentPos.Text = FrequencyFormatter.TimeToString(Source.GetTotalTime() * Source.GetPosition());
        }

        #endregion

        #region DigitalTuner Member

        public long SamplingRate
        {
            get
            {
                if (Source != null)
                {
                    return (long)Source.OutputSamplingRate;
                }

                return 0;
            }
        }

        public event EventHandler SamplingRateChanged;

        #endregion

        #region Tuner Member

        public event EventHandler FilterWidthChanged;

        public event EventHandler FrequencyChanged;

        public event EventHandler InvertedSpectrumChanged;

        public event EventHandler DeviceDisappeared;

        public bool OpenTuner()
        {
            return true;
        }

        public void CloseTuner()
        {
            TransferMode = eTransferMode.Stopped;
            Timer.Stop();
        }

        public bool SetFrequency(long frequency)
        {
            return true;
        }

        public long GetFrequency()
        {
            return 0;
        }

        public long FilterWidth
        {
            get 
            {
                if (Source != null)
                {
                    return (long)Source.OutputSamplingRate;
                }
                return 0;
            }
        }

        public string FilterWidthDescription
        {
            get { return "Source file"; }
        }

        public bool InvertedSpectrum
        {
            get
            {
                if (Source != null)
                {
                    return Source.InvertedSpectrum;
                }
                return false;
            }
        }

        string[] Tuner.Name
        {
            get { return new[] { "File source" }; }
        }

        public string[] Description
        {
            get { return new[] { "Reads data from a file" }; }
        }

        public string[] Details
        {
            get { return new[] { FileName }; }
        }

        public long IntermediateFrequency
        {
            get { return 0; }
        }

        public long LowestFrequency
        {
            get { return -FilterWidth / 2; }
        }

        public long HighestFrequency
        {
            get { return FilterWidth / 2; }
        }

        public long UpperFilterMargin
        {
            get { return 0; }
        }

        public string UpperFilterMarginDescription
        {
            get { return "Source file"; }
        }

        public long LowerFilterMargin
        {
            get { return 0; }
        }

        public string LowerFilterMarginDescription
        {
            get { return "Source file"; }
        }

        public double Amplification
        {
            get;
            set;
        }

        public double Attenuation
        {
            get
            {
                return 0;
            }
        }

        #endregion

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            Seek((double)trackBar.Value / trackBar.Maximum);
            UpdateDisplay();
        }

        private void Seek(double pos)
        {
            Source.Seek(pos);

            /* read one block */
            if (TransferMode == eTransferMode.Stopped)
            {
                Source.Read();
            }

            Source.Seek(pos);
        }

        private void btnPlayPause_Click(object sender, EventArgs e)
        {
            switch (TransferMode)
            {
                case eTransferMode.Stopped:
                    TransferMode = eTransferMode.Stream;
                    break;
                case eTransferMode.Stream:
                case eTransferMode.Block:
                    TransferMode = eTransferMode.Stopped;
                    break;
            }
        }

        private void chkRepeat_CheckedChanged(object sender, EventArgs e)
        {
            Repeat = chkRepeat.Checked;
        }

        private void btnRewind_Click(object sender, EventArgs e)
        {
            Seek(0);
            UpdateDisplay();
        }
    }
}
