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
using LibRXFFT.Libraries.Misc;
using RX_FFT.Dialogs;
using LibRXFFT.Libraries;
using System.IO;
using LibRXFFT.Libraries.ShmemChain;
using System.Collections;

namespace RX_FFT.DeviceControls
{
    public partial class USBRXDeviceControl : Form, DeviceControl
    {
        private USBRXDevice USBRX;
        private SampleSource _SampleSource;
        private int BytesPerSamplePair = 4;
        private bool ClosingAllowed = false;
        private long CurrentFrequency = 0;
        public bool _Connected = false;

        private double BoardAttenuation = 0;
        private double BoardAmplification = 0;

        public int ShmemChannel
        {
            get { return USBRX.ShmemChannel; }
        }

        public int ShmemNode
        {
            get { return USBRX.ShmemNode; }
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
                FilterList.NCOFreq = USBRX.Atmel.TCXOFreq;
                FilterList.UpdateFilters("Filter");
                FilterList.AddFilters("..\\..\\..\\Filter");
                FilterList.FilterSelected += new EventHandler(FilterList_FilterSelected);

                USBRX.Tuner.SamplingRateChanged += new EventHandler(AD6636_FilterRateChanged);
                USBRX.Tuner.FilterWidthChanged += new EventHandler(AD6636_FilterWidthChanged);

                frequencySelector1.UpperLimit = USBRX.Tuner.HighestFrequency;
                frequencySelector1.LowerLimit = USBRX.Tuner.LowestFrequency;
                CurrentFrequency = USBRX.Tuner.GetFrequency();
                SetFreqTextbox(CurrentFrequency);

                _SampleSource = new ShmemSampleSource("USB-RX Device Control", USBRX.ShmemChannel, 1, 0);
                _SampleSource.InvertedSpectrum = InvertedSpectrum;
                _SampleSource.DataFormat = LibRXFFT.Libraries.ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;

                ToolTip ttFreq = new ToolTip();
                ttFreq.SetToolTip(lblFrequency, "Min Freq: " + FrequencyFormatter.FreqToStringAccurate(USBRX.Tuner.LowestFrequency) + Environment.NewLine + "Max Freq: " + FrequencyFormatter.FreqToStringAccurate(USBRX.Tuner.HighestFrequency));
                ttFreq.AutomaticDelay = 500;

                UpdateAtmelFilters();
                SelectFiles(true);

                _Connected = true;

                /* small hack to select first (widest) filter */
                FilterList.FilterSelect(FilterList.FirstFilter);

                Show();
            }

            /* close wait dialog and show ours */
            waitDlg.Close();

            radioAcqOff.Checked = true;
            radioTuner.Checked = true;
            radioAgcOff.Checked = true;
            chkAtt.Checked = false;
            chkPreAmp.Checked = false;
        }

        void UpdateAtmelFilters()
        {
            int filters = USBRX.Atmel.GetFilterCount();

            for (int pos = 0; pos < filters; pos++)
            {
                USBRX.Atmel.ReadFilter(pos);
                long width = USBRX.Atmel.GetFilterWidth();
                long rate = USBRX.Atmel.GetFilterClock();

                FilterList.AddFilter(new AtmelFilter(pos, width, rate));
            }
        }

        void AD6636_FilterWidthChanged(object sender, EventArgs e)
        {
            /* update UI */
            txtFilterWidth.Text = FrequencyFormatter.FreqToStringAccurate(FilterWidth);

            /* inform listeners */
            if (FilterWidthChanged != null)
                FilterWidthChanged(this, null);
        }

        void AD6636_FilterRateChanged(object sender, EventArgs e)
        {
            /* update UI */
            txtFilterRate.Text = FrequencyFormatter.FreqToStringAccurate(SamplingRate);

            /* set sample source frequency */
            _SampleSource.ForceInputRate(SamplingRate);

            /* inform listeners */
            if (SamplingRateChanged != null)
                SamplingRateChanged(this, null);
        }

        void FilterList_FilterSelected(object sender, EventArgs e)
        {
            if (!Connected)
                return;

            if (USBRX != null)
            {
                if (sender is AD6636FilterFile)
                {
                    radioAgcSlow.Enabled = false;
                    radioAgcMedium.Enabled = false;
                    radioAgcFast.Enabled = false;
                    if (radioAgcSlow.Checked || radioAgcMedium.Checked || radioAgcFast.Checked)
                    {
                        radioAgcOff.Checked = true;
                    }
                    USBRX.SetFilter((AD6636FilterFile)sender);
                    SharedMemNative.shmemchain_set_rate(ShmemNode, ((AD6636FilterFile)sender).Rate * 2);
                }
                else if (sender is AtmelFilter)
                {
                    radioAgcSlow.Enabled = true;
                    radioAgcMedium.Enabled = true;
                    radioAgcFast.Enabled = true;
                    USBRX.SetFilter((AtmelFilter)sender);
                    SharedMemNative.shmemchain_set_rate(ShmemNode, ((AtmelFilter)sender).Rate * 2);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!ClosingAllowed)
            {
                e.Cancel = true;
                return;
            }

            if (Connected)
            {
                USBRX.CurrentMode = LibRXFFT.Libraries.eTransferMode.Stopped;
                USBRX.Close();
                _SampleSource.Close();
            }
        }

        private void frequencySelector1_FrequencyChanged(object sender, EventArgs e)
        {
            if (!Connected)
                return;
            CurrentFrequency = frequencySelector1.Frequency;

            if (!USBRX.Tuner.SetFrequency(CurrentFrequency))
                frequencySelector1.BackColor = Color.Red;
            else
                frequencySelector1.BackColor = Color.White;

            if (FrequencyChanged != null)
                FrequencyChanged(this, null);
        }

        private void SelectFiles(bool state)
        {
            if (state)
            {
                FilterList.ShowFiles(true);
                btnFiles.ForeColor = Color.Red;
                btnAtmel.ForeColor = new Button().ForeColor;
            }
            else
            {
                FilterList.ShowFiles(false);
                btnAtmel.ForeColor = Color.Red;
                btnFiles.ForeColor = new Button().ForeColor;
            }
        }

        private void btnFiles_Click(object sender, EventArgs e)
        {
            SelectFiles(true);
        }

        private void btnAtmel_Click(object sender, EventArgs e)
        {
            SelectFiles(false);
        }

        #region DigitalTuner Member

        public event EventHandler SamplingRateChanged;

        public long SamplingRate
        {
            get
            {
                return USBRX.Tuner.SamplingRate;
            }
        }

        #endregion

        #region Tuner Member

        public event EventHandler FrequencyChanged; 
        public event EventHandler FilterWidthChanged;
        public event EventHandler InvertedSpectrumChanged;

        public double Amplification
        {
            get { return USBRX.Tuner.Amplification + BoardAmplification - BoardAttenuation; }
        }

        public long LowestFrequency
        {
            get { return USBRX.Tuner.LowestFrequency; }
        }

        public long HighestFrequency
        {
            get { return USBRX.Tuner.HighestFrequency; }
        }

        public long UpperFilterMargin
        {
            get { return USBRX.Tuner.UpperFilterMargin; }
        }

        public long LowerFilterMargin
        {
            get { return USBRX.Tuner.LowerFilterMargin; }
        }

        public long FilterWidth
        {
            get
            {
                return USBRX.Tuner.FilterWidth;
            }
        }

        public string UpperFilterMarginDescription
        {
            get
            {
                return USBRX.Tuner.UpperFilterMarginDescription;
            }
        }

        public string LowerFilterMarginDescription
        {
            get
            {
                return USBRX.Tuner.LowerFilterMarginDescription;
            }
        }

        public string FilterWidthDescription
        {
            get
            {
                return USBRX.Tuner.FilterWidthDescription;
            }
        }

        string[] Tuner.Name
        {
            get 
            {
                ArrayList lines = new ArrayList();

                lines.Add("BO-35digi with tuners:");
                foreach (string line in USBRX.Tuner.Name)
                {
                    lines.Add("    " + line);
                }

                return (string[])lines.ToArray(typeof(string));
            }
        }

        string[] Tuner.Description
        {
            get { return USBRX.Tuner.Description; }
        }

        string[] Tuner.Details
        {
            get
            {
                ArrayList lines = new ArrayList();

                lines.Add("Hardware details:");
                lines.Add("    Serial: " + USBRX.Atmel.SerialNumber);
                lines.Add("    TCXOFreq: " + USBRX.Atmel.TCXOFreq);
                
                lines.Add("Tuner details:");
                foreach (string line in USBRX.Tuner.Details)
                {
                    lines.Add("    " + line);
                }

                return (string[])lines.ToArray(typeof(string));
            }
        }
        #endregion

        #region DeviceControl Member

        public event EventHandler TransferModeChanged;

        public eTransferMode TransferMode
        {
            get
            {
                return USBRX.CurrentMode;
            }
            set
            {
                USBRX.CurrentMode = value;
                if (TransferModeChanged != null)
                {
                    TransferModeChanged(this, null);
                }
            }
        }
        
        public int SamplesPerBlock
        {
            set
            {
                if (!Connected)
                    return;
                USBRX.SamplesPerBlock = (uint)value;
                USBRX.ReadBlockSize = (uint)(value * BytesPerSamplePair);
                SampleSource.SamplesPerBlock = value;
            }
            get
            {
                if (!Connected)
                    return 0;
                return (int)(USBRX.ReadBlockSize / BytesPerSamplePair);
            }
        }

        public double BlocksPerSecond
        {
            get
            {
                return USBRX.BlocksPerSecond;
            }
            set
            {
                USBRX.BlocksPerSecond = value;
            }
        }

        delegate void setFreqDelegate(double freq);
        void SetFreqTextbox(double freq)
        {
            frequencySelector1.Frequency = (long)freq;
        }

        public bool SetFrequency(long frequency)
        {
            if (!Connected)
                return false;
            if (USBRX.Tuner.SetFrequency(frequency))
            {
                CurrentFrequency = frequency;
                this.BeginInvoke(new setFreqDelegate(SetFreqTextbox), frequency);
                if (FrequencyChanged != null)
                    FrequencyChanged(this, null);
                return true;
            }

            return false;
        }

        public long GetFrequency()
        {
            if (!Connected)
                return 0;

            return CurrentFrequency;

            //return USBRX.Tuner.GetFrequency();
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

        public bool ReadBlock()
        {
            bool ret;

            ret = SampleSource.Read();

            /* transfer done, if needed start next one */
            if(TransferMode == eTransferMode.Block)
            {
                USBRX.ReadBlockReceived();
            }

            return ret;
        }

        public bool Connected
        {
            get
            {
                return _Connected;
            }
        }


        #endregion


        private void radioAcqOff_CheckedChanged(object sender, EventArgs e)
        {
            SampleSource.Flush();
            TransferMode = eTransferMode.Stopped;
        }

        private void radioAcqBlock_CheckedChanged(object sender, EventArgs e)
        {
            SampleSource.Flush();
            TransferMode = eTransferMode.Block;
        }

        private void radioAcqStream_CheckedChanged(object sender, EventArgs e)
        {
            SampleSource.Flush();
            TransferMode = eTransferMode.Stream;
        }

        private void radioRf1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioRf1.Checked)
            {
                USBRX.SetRfSource(USBRXDevice.eRfSource.RF1);
            }
        }

        private void radioRf2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioRf2.Checked)
            {
                USBRX.SetRfSource(USBRXDevice.eRfSource.RF2);
            }
        }

        private void radioRf3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioRf3.Checked)
            {
                USBRX.SetRfSource(USBRXDevice.eRfSource.RF3);
            }
        }

        private void radioRf4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioRf4.Checked)
            {
                USBRX.SetRfSource(USBRXDevice.eRfSource.RF4);
            }
        }

        private void radioTuner_CheckedChanged(object sender, EventArgs e)
        {
            if (radioTuner.Checked)
            {
                USBRX.SetRfSource(USBRXDevice.eRfSource.Tuner);
            }
        }

        private void chkAtt_CheckedChanged(object sender, EventArgs e)
        {
            USBRX.SetAtt(chkAtt.Checked);
            if (chkAtt.Checked)
            {
                BoardAttenuation = 20;
            }
            else
            {
                BoardAttenuation = 0;
            }
        }

        private void chkPreAmp_CheckedChanged(object sender, EventArgs e)
        {
            USBRX.SetPreAmp(chkPreAmp.Checked);
            if (chkPreAmp.Checked)
            {
                BoardAmplification = 20;
            }
            else
            {
                BoardAmplification = 0;
            }
        }

        private void radioAgcOff_CheckedChanged(object sender, EventArgs e)
        {
            USBRX.SetAgc(USBRXDevice.eAgcType.Off);
        }

        private void radioAgcSlow_CheckedChanged(object sender, EventArgs e)
        {
            USBRX.SetAgc(USBRXDevice.eAgcType.Slow);
        }

        private void radioAgcMedium_CheckedChanged(object sender, EventArgs e)
        {
            USBRX.SetAgc(USBRXDevice.eAgcType.Medium);
        }

        private void radioAgcFast_CheckedChanged(object sender, EventArgs e)
        {
            USBRX.SetAgc(USBRXDevice.eAgcType.Fast);
        }

        private void radioAgcManual_CheckedChanged(object sender, EventArgs e)
        {
            txtMgcValue.ReadOnly = !radioAgcManual.Checked;
            if (radioAgcManual.Checked)
            {
                txtMgcValue_ValueChanged(null, null);
            }
        }

        private void txtMgcValue_ValueChanged(object sender, EventArgs e)
        {
            USBRX.SetMgc((int)txtMgcValue.Value);
        }
    }
}
