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
using System.IO;
using LibRXFFT.Components.DeviceControls;

namespace LibRXFFT.Components.DeviceControls
{
    public partial class FileSourceDeviceControl : Form, DeviceControl
    {

        private FileSampleSource Source;
        private bool Repeat = false;
        private double LastPosition = 0;
        private object ReadTick = new object();
        private object AccessLock = new object();
        private string FileName = "none";
        private AccurateTimer Timer;
        private int InternalOversampling = 1;

        public double ReadFrameRate = 0.05f;

        public FileSourceDeviceControl() : this(null, 1) { }
        public FileSourceDeviceControl(string fileName) : this(fileName, 1) { }
        public FileSourceDeviceControl(int oversampling) : this(null, oversampling) { }

        public FileSourceDeviceControl(string fileName, int oversampling)
        {
            InitializeComponent();

            InternalOversampling = oversampling;

            if (fileName == null)
            {
                FileDialog dlg = new OpenFileDialog();

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    fileName = dlg.FileName;
                }
            }

            Timer = new AccurateTimer();
            Timer.Periodic = true;
            Timer.Timer += new EventHandler(Timer_Timer);

            if (fileName != null)
            {
                FileName = fileName;
                Source = new FileSampleSource(FileName, InternalOversampling);
                Source.ForwardEnabled = true;
                Source.SamplesPerBlock = (int)Math.Min(ReadFrameRate * Source.InputSamplingRate, 32768);

                trackBar.Maximum = (int)(Source.GetTotalTime() / 0.01f);

                if(trackBar.Maximum < 0)
                {
                    trackBar.Maximum = int.MaxValue;
                }

                Timer.Interval = (uint)((double)(Source.OutputBlockSize * 1000.0f) / Source.OutputSamplingRate);

                UpdateDisplay();
                Timer.Start();
                Show();
            }
        }

        public void LoadFile(string fileName)
        {
            try
            {
                /* try to open the file */
                FileSampleSource source = new FileSampleSource(fileName);
                source.ForwardEnabled = true;
                source.SamplesPerBlock = (int)(ReadFrameRate * source.OutputSamplingRate);

                /* succeeded, use this filesource */
                lock (AccessLock)
                {
                    eTransferMode lastMode = TransferMode;

                    TransferMode = eTransferMode.Stopped;
                    Source.Close();
                    FileName = fileName;
                    Source = source;
                    TransferMode = lastMode;

                    /* update trackbar */
                    trackBar.Maximum = (int)(Source.GetTotalTime() / 0.01f);

                    /* reconfigure timer */
                    Timer.Stop();
                    Timer.Interval = (uint)((double)(Source.OutputBlockSize * 1000.0f) / Source.InputSamplingRate);
                    Timer.Start();
                }
                UpdateDisplay();
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show("File format not supported. (" + ex.Message + ")");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the file. (" + ex.Message + ")");
            }
        }


        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            FileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadFile(dlg.FileName);
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
            e.Cancel = false;
            TransferMode = eTransferMode.Stopped;
            Timer.Stop();

            if (DeviceClosed != null)
            {
                DeviceClosed(this, null);
            }
            base.OnClosing(e);
        }


        public void Close()
        {
            CloseTuner();
            base.Close();
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
                lock (AccessLock)
                {
                    if (Source != null)
                    {
                        return Source.SamplesPerBlock;
                    }
                    return 0;
                }
            }
            set
            {
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
                try
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
                }
                catch (Exception e)
                {
                }

                CurrentTransferMode = value;
                if (TransferModeChanged != null)
                {
                    TransferModeChanged(this, null);
                }
                lock (ReadTick)
                {
                    Monitor.Pulse(ReadTick);
                }
            }
        }

        public SampleSource SampleSource
        {
            get { return Source; }
        }

        public void CloseControl()
        {
            CloseTuner();
            Close();
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

                if (Source == null)
                {
                    return false;
                }

            } while (TransferMode == eTransferMode.Stopped);

            lock (AccessLock)
            {
                success = Source.Read();

                /* if failed, maybe we reached the end */
                if (!success)
                {
                    if (Repeat)
                    {
                        Source.Restart();
                        success = Source.Read();
                    }
                    else
                    {
                        /* seems to have reached the end. stop playback */
                        TransferMode = eTransferMode.Stopped;

                        string[] ext = FileName.Split('.');
                        string[] parts = ext[0].Split('_');

                        if (parts.Length > 1 && ext.Length > 1)
                        {
                            string counter = parts[parts.Length - 1];
                            int num = 0;

                            if(int.TryParse(counter, out num))
                            {
                                num++;
                                string newName = string.Join("_", parts.SubArray(0,parts.Length-1)) + "_" + num.ToString().PadLeft(counter.Length, '0') + "." + ext[1];
                                if (File.Exists(newName))
                                {
                                    BeginInvoke(new Action(() =>
                                    {
                                        LoadFile(newName);
                                        TransferMode = eTransferMode.Stream;
                                    }));
                                }
                            }
                        }
                    }
                }

                if (Source.GetPosition() != LastPosition)
                {
                    LastPosition = Source.GetPosition();
                    BeginInvoke(new Action(() => { UpdateDisplay(); }));
                }
            }

            return success;
        }


        private void UpdateDisplay()
        {
            lock (AccessLock)
            {
                int newPos = (int)(trackBar.Maximum * Source.GetPosition());
                if (newPos != trackBar.Value)
                {
                    trackBar.Value = newPos;
                }

                lblStartPos.Text = "0 s";
                lblEndPos.Text = String.Format("{0:0.000} s", Source.GetTotalTime());
                lblCurrentPos.Text = String.Format("{0:0.000} s", Source.GetTotalTime() * Source.GetPosition());
            }
        }

        #endregion

        #region DigitalTuner Member

        public long SamplingRate
        {
            get
            {
                if (Source != null)
                {
                    lock (AccessLock)
                    {
                        return (long)Source.OutputSamplingRate;
                    }
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
        public event EventHandler DeviceClosed;

        public bool OpenTuner()
        {
            return true;
        }

        public void CloseTuner()
        {
            if (Source != null)
            {
                Source.Close();
                Source = null;
            }

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
            lock (AccessLock)
            {
                Source.Seek(pos);
            }
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
    public static class Extension
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }

}
