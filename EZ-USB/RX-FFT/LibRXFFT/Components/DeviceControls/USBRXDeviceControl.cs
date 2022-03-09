using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.USB_RX.Devices;
using LibRXFFT.Libraries.USB_RX.Misc;
using LibRXFFT.Libraries.USB_RX.Tuners;
using LibRXFFT.Components.DeviceControls;

namespace LibRXFFT.Components.DeviceControls
{
    public partial class USBRXDeviceControl : Form, DeviceControl
    {
        public USBRXDevice USBRX;
        private SampleSource _SampleSource;
        private int BytesPerSamplePair = 4;
        private bool ClosingAllowed = false;
        private long CurrentFrequency = 0;
        public bool _Connected = false;

        private double BoardAttenuation = 0;
        private double BoardAmplification = 0;
        private FilterInformation CurrentFilter;
        protected int SamplesThisBlock = 0;


        public USBRXDevice.eCombinationType TunerCombination = USBRXDevice.eCombinationType.None;


        public USBRXDeviceControl()
        {
            InitializeComponent();

            radioAcqOff.MouseUp += new MouseEventHandler(radioAcqOff_MouseUp);
            txtChannel.ValueChanged += new EventHandler(txtChannel_ValueChanged);
            Hide();
        }

        void txtChannel_ValueChanged(object sender, EventArgs e)
        {
            USBRX.AD6636.SelectChannel((int)txtChannel.Value);
        }

        void radioAcqOff_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //USBRX.ShowConsole(true);
            }
        }

        public int ShmemChannel
        {
            get { return USBRX.ShmemChannel; }
        }

        public int ShmemNode
        {
            get { return USBRX.ShmemNode; }
        }


        void UpdateAtmelFilters()
        {
            int filters = USBRX.Atmel.GetFilterCount();

            for (int pos = 0; pos < filters; pos++)
            {
                USBRX.Atmel.ReadFilter(pos);
                long width = USBRX.Atmel.GetFilterWidth();
                long rate = USBRX.Atmel.GetFilterClock();

                FilterList.AddFilter(new AtmelFilter(USBRX.Atmel, pos, width, rate));
            }
        }

        void Tuner_DeviceDisappeared(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    Tuner_DeviceDisappeared(sender, e);
                }));
            }
            else
            {
                if (DeviceDisappeared != null)
                    DeviceDisappeared(sender, e);
            }
        }

        void Tuner_InvertedSpectrumChanged(object sender, EventArgs e)
        {
            SampleSource.InvertedSpectrum = InvertedSpectrum;
        }

        void Tuner_FilterWidthChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    Tuner_FilterWidthChanged(sender, e);
                }));
            }
            else
            {
                txtFilterWidth.Text = FrequencyFormatter.FreqToStringAccurate(FilterWidth);

                /* inform listeners */
                if (FilterWidthChanged != null)
                    FilterWidthChanged(this, null);
            }
        }

        void Tuner_FilterRateChanged(object sender, EventArgs e)
        {
            /* update UI */
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    Tuner_FilterRateChanged(sender, e);
                }));
            }
            else
            {
                txtFilterRate.Text = FrequencyFormatter.FreqToStringAccurate(SamplingRate);

                /* set sample source frequency */
                _SampleSource.ForceInputRate(SamplingRate);

                /* update related parameters */
                SamplesPerBlock = SamplesPerBlock;

                /* inform listeners */
                if (SamplingRateChanged != null)
                    SamplingRateChanged(this, null);
            }
        }

        void FilterList_FilterSelected(object sender, EventArgs e)
        {
            if (!Connected)
                return;

            if (USBRX != null)
            {
                CurrentFilter = (FilterInformation)sender;
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

            TransferMode = TransferMode;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!ClosingAllowed)
            {
                e.Cancel = true;
                return;
            }

            CloseTuner();
        }

        private void frequencySelector1_FrequencyChanged(object sender, EventArgs e)
        {
            if (!Connected || ScanFrequenciesEnabled)
                return;

            CurrentFrequency = frequencySelector1.Frequency;

            if (!USBRX.Tuner.SetFrequency(CurrentFrequency))
                frequencySelector1.ForeColor = Color.Red;
            else
                frequencySelector1.ForeColor = Color.Cyan;

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
                if (USBRX != null && USBRX.Tuner != null)
                {
                    return USBRX.Tuner.SamplingRate;
                }

                return 0;
            }
        }

        #endregion

        #region Tuner Member

        public event EventHandler FrequencyChanged;
        public event EventHandler FilterWidthChanged;
        public event EventHandler InvertedSpectrumChanged;
        public event EventHandler DeviceDisappeared;
        public event EventHandler DeviceClosed;

        public virtual bool OpenTuner()
        {
            /* display the wait message */
#if false
            WaitDialog waitDlg = new WaitDialog();
            waitDlg.Show();
            waitDlg.Refresh();
#endif

            USBRX = new USBRXDevice();
            //USBRX.ShowConsole(true);
            USBRX.TunerCombination = TunerCombination;

            try
            {
                if (!USBRX.Init())
                {
                    ErrorMessage = "Could not find USB-RX on USB.";
#if false
                    waitDlg.Close();
#endif
                    base.Close();
                    return false;
                }
            }
            catch (BadImageFormatException e)
            {
                ErrorMessage = "Unsupported architecture.";
#if false
                    waitDlg.Close();
#endif
                base.Close();
                return false;
            }
            catch (Exception e)
            {
                ErrorMessage = "Unhandled exception." + Environment.NewLine + e;
#if false
                    waitDlg.Close();
#endif
                base.Close();
                return false;
            }
            ErrorMessage = "";

            FilterList.NCOFreq = USBRX.Atmel.TCXOFreq;
            FilterList.UpdateFilters("Filter");
            FilterList.AddFilters("..\\..\\..\\Filter");
            FilterList.FilterSelected += new EventHandler(FilterList_FilterSelected);

            USBRX.Tuner.SamplingRateChanged += new EventHandler(Tuner_FilterRateChanged);
            USBRX.Tuner.FilterWidthChanged += new EventHandler(Tuner_FilterWidthChanged);
            USBRX.Tuner.InvertedSpectrumChanged += new EventHandler(Tuner_InvertedSpectrumChanged);
            USBRX.Tuner.DeviceDisappeared += new EventHandler(Tuner_DeviceDisappeared);

            frequencySelector1.UpperLimit = USBRX.Tuner.HighestFrequency;
            frequencySelector1.LowerLimit = USBRX.Tuner.LowestFrequency;
            CurrentFrequency = USBRX.Tuner.GetFrequency();
            SetFreqTextbox(CurrentFrequency);

            _SampleSource = new ShmemSampleSource("USB-RX Device Control", USBRX.ShmemChannel, 1, 0);
            _SampleSource.InvertedSpectrum = InvertedSpectrum;
            _SampleSource.DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPointLE;

            ToolTip ttFreq = new ToolTip();
            ttFreq.SetToolTip(frequencySelector1, "Min Freq: " + FrequencyFormatter.FreqToStringAccurate(USBRX.Tuner.LowestFrequency) + Environment.NewLine + "Max Freq: " + FrequencyFormatter.FreqToStringAccurate(USBRX.Tuner.HighestFrequency));
            ttFreq.AutomaticDelay = 500;

            UpdateAtmelFilters();
            SelectFiles(true);

            Connected = true;

            /* small hack to select first (widest) filter */
            FilterList.FilterSelect(FilterList.FirstFilter);

            /* close wait dialog and show ours */
#if false
                    waitDlg.Close();
#endif

            Show();

            radioAcqOff.Checked = true;
            radioTunerInt.Checked = true;
            radioAgcOff.Checked = true;
            chkAtt.Checked = false;
            chkPreAmp.Checked = false;

            radioTunerInt_CheckedChanged(null, null);
            chkAtt_CheckedChanged(null, null);
            chkPreAmp_CheckedChanged(null, null);
            radioAgcOff_CheckedChanged(null, null);

            radioAcqBlock.Checked = true;


            DeviceOpened?.Invoke(this, EventArgs.Empty);

            return true;
        }

        public virtual void CloseTuner()
        {
            DeviceClosed?.Invoke(this, EventArgs.Empty);

            if (Connected)
            {
                USBRX.Tuner.CloseTuner();
                USBRX.CurrentMode = eTransferMode.Stopped;
                USBRX.Close();
                USBRX = null;
                _SampleSource.Close();

                Connected = false;
            }

            Hide();
        }

        public long IntermediateFrequency
        {
            get { return 0; }
        }

        public double Amplification
        {
            get { return USBRX.Tuner.Amplification + BoardAmplification; }
            set
            {
                double val = value;

                if (val > 30 && AttEnabled)
                {
                    USBRX.SetAtt(false);
                    BeginInvoke(new MethodInvoker(() => chkAtt.Checked = false));
                    val -= 30;
                }

                if (false)
                {
                    if (val > 20)
                    {
                        if (!PreampEnabled)
                        {
                            USBRX.SetPreAmp(true);
                            BeginInvoke(new MethodInvoker(() => chkPreAmp.Checked = true));
                        }
                        val -= 20;
                    }
                }

                /* try to set main tuner amplification */
                USBRX.Tuner.Amplification = val;

                /* try to handle the remaining amplification */
                val -= USBRX.Tuner.Amplification;
            }
        }

        public double Attenuation
        {
            get { return BoardAttenuation + USBRX.Tuner.Attenuation; }
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

                lines.Add("USB-RX");
                if (USBRX != null && USBRX.Tuner != null)
                {
                    lines.Add("with tuners:");
                    foreach (string line in USBRX.Tuner.Name)
                    {
                        lines.Add("    " + line);
                    }
                }

                return (string[])lines.ToArray(typeof(string));
            }
        }

        string[] Tuner.Description
        {
            get
            {
                if (USBRX != null && USBRX.Tuner != null)
                {
                    return USBRX.Tuner.Description;
                }
                else
                {
                    return new[] { "No description available" };
                }
            }
        }

        string[] Tuner.Details
        {
            get
            {
                ArrayList lines = new ArrayList();

                if (USBRX != null)
                {
                    if (USBRX.Atmel != null)
                    {
                        lines.Add("Hardware details:");
                        lines.Add("    Temperature:    " + USBRX.Atmel.Temperature);
                        lines.Add("    Serial:         " + USBRX.Atmel.SerialNumber);
                        lines.Add("    InternalSerial: " + USBRX.Atmel.InternalSerialNumber);
                        lines.Add("    TCXOFreq:       " + USBRX.Atmel.TCXOFreq);
                    }

                    if (USBRX.Tuner != null)
                    {
                        lines.Add("Tuner details:");
                        foreach (string line in USBRX.Tuner.Details)
                        {
                            lines.Add("    " + line);
                        }
                    }
                }

                return (string[])lines.ToArray(typeof(string));
            }
        }


        #endregion

        #region DeviceControl Member

        public string _ErrorMessage;
        private bool AttEnabled = false;
        private bool PreampEnabled = false;

        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            private set { _ErrorMessage = value; }

        }

        public bool AllowsMultipleReaders
        {
            get
            {
                return true;
            }
        }


        public event EventHandler TransferModeChanged;
        public event EventHandler DeviceOpened;

        public eTransferMode TransferMode
        {
            get
            {
                if (!Connected)
                    return eTransferMode.Stopped;

                return USBRX.CurrentMode;
            }
            set
            {
                SamplesThisBlock = 0;
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

                int fragmentSamples = value;

                /* read data from shmen in smaller fragments */
                if (SamplingRate <= 48000)
                {
                    /* transfer in 512 byte blocks */
                    fragmentSamples = 512 / BytesPerSamplePair;
                }

                USBRX.SamplesPerBlock = (uint)value;
                USBRX.ReadBlockSize = (uint)(value * BytesPerSamplePair);
                USBRX.ReadFragmentSize = (uint)(fragmentSamples * BytesPerSamplePair);
                SampleSource.SamplesPerBlock = fragmentSamples;

                TransferMode = TransferMode;
            }
            get
            {
                if (!Connected)
                    return 0;
                return (int)USBRX.SamplesPerBlock;
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
                if (!ScanFrequenciesEnabled)
                {
                    this.BeginInvoke(new Action(() => SetFreqTextbox(frequency)));
                }
                if (FrequencyChanged != null)
                    FrequencyChanged(this, null);
                return true;
            }

            return false;
        }

        public void CloseControl()
        {
            Close();
        }

        public long GetFrequency()
        {
            if (!Connected)
                return 0;

            return CurrentFrequency;
        }

        public bool InvertedSpectrum
        {
            get
            {
                if (USBRX != null && USBRX.Tuner != null)
                {
                    return USBRX.Tuner.InvertedSpectrum;
                }

                return false;
            }
        }

        public SampleSource SampleSource
        {
            get
            {
                return _SampleSource;
            }
        }

        public bool ScanFrequenciesEnabled { get; set; }

        public void Close()
        {
            ClosingAllowed = true;

            TransferMode = eTransferMode.Stopped;

            CloseTuner();
            base.Close();
        }

        public bool ReadBlock()
        {
            bool success;

            do
            {
                /* will loop every 100ms (timeout set up in SharedMem object) */
                success = SampleSource.Read();
            } while (success && SampleSource.SamplesRead == 0);

            /* collect as many samples as the current read block size has */
            if (success)
            {
                SamplesThisBlock += SampleSource.SamplesPerBlock;
            }

            if (SamplesThisBlock >= SamplesPerBlock)
            {
                //RX_FFT.Components.GDI.Log.AddMessage("ShmemSampleSource", "Full block");
                SamplesThisBlock = 0;
                SampleSource.Flush();

                /* transfer done, if needed start next one */
                if (success)
                {
                    if (TransferMode == eTransferMode.Block)
                    {
                        USBRX.ReadBlockReceived();
                    }
                }
                else
                {
                    if (USBRX.DeviceLost)
                    {
                        RX_FFT.Components.GDI.Log.AddMessage("USBRX -> DeviceLost");
                        if (DeviceDisappeared != null)
                        {
                            DeviceDisappeared(this, null);
                        }
                    }
                }
            }

            return success;
        }

        public bool Connected
        {
            get
            {
                return _Connected;
            }
            private set
            {
                _Connected = value;
            }
        }


        #endregion


        private void radioAcqOff_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAcqOff.Checked)
            {
                SampleSource.Flush();
                TransferMode = eTransferMode.Stopped;
            }
        }

        private void radioAcqBlock_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAcqBlock.Checked)
            {
                TransferMode = eTransferMode.Stopped;
                SampleSource.Flush();
                TransferMode = eTransferMode.Block;
            }
        }

        private void radioAcqStream_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAcqStream.Checked)
            {
                TransferMode = eTransferMode.Stopped;
                SampleSource.Flush();
                TransferMode = eTransferMode.Stream;
            }
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

        private void radioTunerInt_CheckedChanged(object sender, EventArgs e)
        {
            if (radioTunerInt.Checked)
            {
                USBRX.SetRfSource(USBRXDevice.eRfSource.InternalTuner);
            }
        }

        void txtAtt_ValueChanged(object sender, System.EventArgs e)
        {
            BoardAttenuation = txtAtt.Value;
            USBRX.SetAtt((int)BoardAttenuation);
        }

        private void chkAtt_CheckedChanged(object sender, EventArgs e)
        {
            AttEnabled = chkAtt.Checked;
            USBRX.SetAtt(AttEnabled);
            if (AttEnabled)
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
            PreampEnabled = chkPreAmp.Checked;
            USBRX.SetPreAmp(PreampEnabled);
            if (PreampEnabled)
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
            if (radioAgcOff.Checked)
            {
                USBRX.SetAgc(USBRXDevice.eAgcType.Off);
            }
        }

        private void radioAgcSlow_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAgcSlow.Checked)
            {
                USBRX.SetAgc(USBRXDevice.eAgcType.Slow);
            }
        }

        private void radioAgcMedium_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAgcMedium.Checked)
            {
                USBRX.SetAgc(USBRXDevice.eAgcType.Medium);
            }
        }

        private void radioAgcFast_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAgcFast.Checked)
            {
                USBRX.SetAgc(USBRXDevice.eAgcType.Fast);
            }
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
            if (radioAgcManual.Checked)
            {
                USBRX.SetMgc((int)txtMgcValue.Value);
            }
        }
    }
}
