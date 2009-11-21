using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Devices;
using LibRXFFT.Libraries.USB_RX.Tuners;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.USB_RX.Misc;

namespace RX_FFT
{
    public partial class USBRXDeviceControl : Form, DeviceControl
    {
        private USBRXDevice USBRX;
        private SampleSource _SampleSource;
        private int BytesPerSamplePair = 8;
        private bool ClosingAllowed = false;

        public bool _Connected = false;
        public int ShmemChannel
        {
            get { return USBRX.ShmemChannel; }
        }

        public USBRXDeviceControl()
        {
            /* display the wait message */
            WaitDialog waitDlg = new WaitDialog();
            waitDlg.Show();
            waitDlg.Refresh();

            /* do our own stuff */
            InitializeComponent();

            USBRX = new USBRXDevice();
            USBRX.ShowConsole(false);

            if (USBRX.Init())
            {
//                aD6636FilterList1.UpdateFilters("D:\\cygwin\\home\\g3gg0\\EZ-USB\\CD\\HyperFFT Dream 8\\Filter", USBRX.Atmel.TCXOFreq);
                aD6636FilterList1.UpdateFilters("..\\..\\..\\Filter", USBRX.Atmel.TCXOFreq);
                aD6636FilterList1.FilterSelected += new EventHandler(aD6636FilterList1_FilterSelected);

                _SampleSource = new ShmemSampleSource("FFT Display", USBRX.ShmemChannel, 1, 0);
                _SampleSource.InvertedSpectrum = InvertedSpectrum;

                _Connected = true;
                Show();
            }

            /* close wait dialog and show ours */
            waitDlg.Close();

            radioAcqOff.Checked = true;
            radioAcqOff_CheckedChanged(null, null);
        }

        void aD6636FilterList1_FilterSelected(object sender, EventArgs e)
        {
            if (!_Connected)
                return;

            FilterFile filter = (FilterFile)sender;
            if (USBRX != null)
            {
                USBRX.AD6636.SetFilter(filter);
            }
            txtFilterRate.Text = (filter.OutputFrequency).ToString();
            _SampleSource.ForceInputRate(filter.OutputFrequency);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!ClosingAllowed)
            {
                e.Cancel = true;
                return;
            }

            if (_Connected)
            {
                USBRX.StopRead();
                USBRX.Close();
                _SampleSource.Close();
            }
        }

        private void frequencySelector1_TextChanged(object sender, EventArgs e)
        {
            if (!_Connected)
                return;
            long freq = frequencySelector1.Frequency;

            if (!USBRX.Tuner.SetFrequency(freq))
                frequencySelector1.BackColor = Color.Red;
            else
                frequencySelector1.BackColor = Color.White;


            if (FrequencyChanged != null)
                FrequencyChanged(this, null);
        }

        private void btnFilterSelectDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            DialogResult result = d.ShowDialog();
            if (result == DialogResult.OK)
            {
                string folderName = d.SelectedPath;
                aD6636FilterList1.UpdateFilters(folderName);
            }
        }

        #region DeviceControl Member

        public event EventHandler FrequencyChanged;

        public int SamplesPerBlock
        {
            set
            {
                if (!_Connected)
                    return;
                USBRX.ReadBlockSize = (uint)(value * BytesPerSamplePair);
                _SampleSource.SamplesPerBlock = value;
            }
            get
            {
                if (!_Connected)
                    return 0;
                return (int)(USBRX.ReadBlockSize / BytesPerSamplePair);
            }
        }

        
        delegate void setFreqDelegate(double freq);
        void setFreqTextbox(double freq)
        {
            frequencySelector1.Frequency = (long)freq;
        }

        public bool SetFrequency(long frequency)
        {
            if (!_Connected)
                return false;
            if (USBRX.Tuner.SetFrequency(frequency))
            {
                this.BeginInvoke(new setFreqDelegate(setFreqTextbox), frequency);
                return true;
            }

            return false;
        }

        public long GetFrequency()
        {
            if (!_Connected)
                return 0;
            return USBRX.Tuner.GetFrequency();
        }


        public bool InvertedSpectrum
        {
            get
            {
                return USBRX.Tuner.InvertedSpectrum;
            }
        }

        public SampleSource SampleSource
        {
            get
            {
                return _SampleSource;
            }
        }

        public void Close()
        {
            ClosingAllowed = true;
            base.Close();
        }

        public bool Connected
        {
            get
            {
                return _Connected;
            }
        }

        public void StartRead()
        {
            if (!_Connected)
                return;

            USBRX.StartRead();
        }
        
        public void StopRead()
        {
            if (!_Connected)
                return;

            USBRX.StopRead();
        }
        
        public void StartStreamRead()
        {
            if (!_Connected)
                return;

            USBRX.StartStreamRead();
        }

        public void StopStreamRead()
        {
            if (!_Connected)
                return;

            USBRX.StopStreamRead();
        }

        #endregion

        private void radioAcqOff_CheckedChanged(object sender, EventArgs e)
        {
            StopRead();
            StopStreamRead();
        }

        private void radioAcqBlock_CheckedChanged(object sender, EventArgs e)
        {
            StopRead();
            StartRead();
        }

        private void radioAcqStream_CheckedChanged(object sender, EventArgs e)
        {
            StopRead();
            StartStreamRead();
        }
    }
}
