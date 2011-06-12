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
using SDR4All_DLL;

namespace LibRXFFT.Components.DeviceControls
{
    public partial class USRPDeviceControl : Form, DeviceControl
    {
        SDR4All Driver = null;

        public USRPDeviceControl()
        {
            InitializeComponent();

            try
            {
                Driver = new SDR4All();
                int devices = Driver.GetNumberOfUSRP();

                MessageBox.Show("Devices found: " + devices + ". Did not implement more for now.");
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to instanciate SDR4All. " + e.GetType().ToString());
            }

            Hide();
        }

        void txtChannel_ValueChanged(object sender, EventArgs e)
        {
        }

        void radioAcqOff_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
            }
        }

        public int ShmemChannel
        {
            get { return 0; }
        }

        public int ShmemNode
        {
            get { return 0; }
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
                //_SampleSource.ForceInputRate(SamplingRate);

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

            TransferMode = TransferMode;
        }

        protected override void OnClosing(CancelEventArgs e)
        {

            CloseTuner();
        }

        private void frequencySelector1_FrequencyChanged(object sender, EventArgs e)
        {
            if (!Connected || ScanFrequenciesEnabled)
                return;


            if (FrequencyChanged != null)
                FrequencyChanged(this, null);
        }

        private void SelectFiles(bool state)
        {
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


            try
            {
                
            }
            catch (BadImageFormatException e)
            {
                ErrorMessage = "Unsupported architecture.";


                base.Close();
                return false;
            }
            catch (Exception e)
            {
                ErrorMessage = "Unhandled exception." + Environment.NewLine + e;

                base.Close();
                return false;
            }
            ErrorMessage = "";

            SelectFiles(true);

            Connected = true;


            /* close wait dialog and show ours */
#if false
                    waitDlg.Close();
#endif

            Show();

            radioAcqOff.Checked = true;
            radioTuner.Checked = true;
            radioAgcOff.Checked = true;
            chkAtt.Checked = false;
            chkPreAmp.Checked = false;

            radioTuner_CheckedChanged(null, null);
            chkAtt_CheckedChanged(null, null);
            chkPreAmp_CheckedChanged(null, null);
            radioAgcOff_CheckedChanged(null, null);

            radioAcqBlock.Checked = true;

            return true;
        }

        public virtual void CloseTuner()
        {
            if (Connected)
            {

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
            get { return 0; }
            set
            {
                double val = value;

                if (val > 30 && AttEnabled)
                {
                    BeginInvoke(new MethodInvoker(() => chkAtt.Checked = false));
                    val -= 30;
                }

                if (false)
                {
                    if (val > 20)
                    {
                        if (!PreampEnabled)
                        {
                            BeginInvoke(new MethodInvoker(() => chkPreAmp.Checked = true));
                        }
                        val -= 20;
                    }
                }

            }
        }

        public double Attenuation
        {
            get { return 0; }
        }

        public long LowestFrequency
        {
            get { return 0; }
        }

        public long HighestFrequency
        {
            get { return 0; }
        }

        public long UpperFilterMargin
        {
            get { return 0; }
        }

        public long LowerFilterMargin
        {
            get { return 0; }
        }

        public long FilterWidth
        {
            get
            {
                return 0;
            }
        }

        public string UpperFilterMarginDescription
        {
            get
            {
                return "";
            }
        }

        public string LowerFilterMarginDescription
        {
            get
            {
                return "";
            }
        }

        public string FilterWidthDescription
        {
            get
            {
                return "";
            }
        }

        string[] Tuner.Name
        {
            get
            {
                ArrayList lines = new ArrayList();

                
                return (string[])lines.ToArray(typeof(string));
            }
        }

        string[] Tuner.Description
        {
            get
            {
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

        public eTransferMode TransferMode
        {
            get
            {
                    return eTransferMode.Stopped;

            }
            set
            {
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

                TransferMode = TransferMode;
            }
            get
            {
                    return 0;
            }
        }

        public double BlocksPerSecond
        {
            get
            {
                return 0;
            }
            set
            {
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

            return 0;
        }

        public bool InvertedSpectrum
        {
            get
            {

                return false;
            }
        }

        public SampleSource SampleSource
        {
            get
            {
                return null;
            }
        }

        public bool ScanFrequenciesEnabled { get; set; }

        public void Close()
        {

            TransferMode = eTransferMode.Stopped;

            CloseTuner();
            base.Close();
        }

        public bool ReadBlock()
        {
            return false;
        }

        public bool Connected
        {
            get
            {
                return false;
            }
            private set
            {
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
            }
        }

        private void radioRf2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioRf2.Checked)
            {
            }
        }

        private void radioRf3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioRf3.Checked)
            {
            }
        }

        private void radioRf4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioRf4.Checked)
            {
            }
        }

        private void radioTuner_CheckedChanged(object sender, EventArgs e)
        {
            if (radioTuner.Checked)
            {
            }
        }

        void txtAtt_ValueChanged(object sender, System.EventArgs e)
        {
        }

        private void chkAtt_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void chkPreAmp_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void radioAgcOff_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAgcOff.Checked)
            {
            }
        }

        private void radioAgcSlow_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAgcSlow.Checked)
            {
            }
        }

        private void radioAgcMedium_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAgcMedium.Checked)
            {
            }
        }

        private void radioAgcFast_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAgcFast.Checked)
            {
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
            }
        }
    }
}
