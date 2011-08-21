using System;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.Demodulators;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SignalProcessing;
using System.Drawing;
using System.Collections;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.SoundSinks;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Components.GDI
{
    public partial class DemodulationDialog : Form
    {
        private DemodulationState DemodState;
        private Timer UpdateTimer;
        public event EventHandler FrequencyChanged = null;
        public bool FrequencyFixed = false;
        public string[] SourceFrequencyDesc = new[] { "Band Center", "Custom", "Follow Cursor", "Select Marker", "Area Selection" };

        public DemodulationDialog(DemodulationState demod)
        {
            InitializeComponent();
            DemodState = demod;

            foreach (var v in Enum.GetValues(typeof(DemodulationState.eSourceFrequency)))
            {
                this.cmbSourceFrequency.Items.Add(SourceFrequencyDesc[(int)v]);
            }

            UpdateTimer = new Timer();
            UpdateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            UpdateTimer.Interval = 500;
            UpdateTimer.Start();

            DemodState.DataUpdated += new EventHandler(Demod_DataUpdated);

            lock (DemodState.SoundSinkInfos)
            {
                foreach (SoundSinkInfo info in DemodState.SoundSinkInfos)
                {
                    tabSoundOut.Controls.Add(info.Page);
                }
            }

            UpdateFromConfig();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            foreach (SoundSinkInfo info in DemodState.SoundSinkInfos)
            {
                tabSoundOut.Controls.Remove(info.Page);
            }
            base.OnClosing(e);
        }

        public void Shutdown()
        {
            DemodState.RemoveSinks();
        }

        public void UpdateFromConfig()
        {
            UpdateFrequencyInternal();
            chkEnableDemod.Checked = DemodState.DemodulationEnabled;
            chkEnableLimiter.Checked = DemodState.CursorPositionWindowEnabled;
            chkAmplify.Checked = DemodState.AudioAmplificationEnabled;
            chkNative.Checked = FIRFilter.UseNative;

            txtAmplify.Text = (DemodState.AudioAmplification*100).ToString();
            txtDecim.Text = DemodState.AudioDecimation.ToString();
            txtSquelchLimit.Text = DemodState.SquelchLowerLimit.ToString();

            radioAM.Checked = false;
            radioFMFast.Checked = false;
            radioFMAccurate.Checked = false;

            radioFilter8.Checked = false;
            radioFilter4.Checked = false;
            radioFilter2.Checked = false;
            radioFilter16.Checked = false;
            radioFilter32.Checked = false;
            radioFilter64.Checked = false;
            radioFilter128.Checked = false;
            radioFilter256.Checked = false;

            radioLowPass2.Checked = false;
            radioLowPass4.Checked = false;
            radioLowPass8.Checked = false;
            radioLowPass16.Checked = false;
            radioLowPass32.Checked = false;
            radioLowPass64.Checked = false;
            radioLowPass128.Checked = false;
            radioLowPass256.Checked = false;

            if (DemodState.SignalDemodulator.GetType() == typeof(AMDemodulator))
            {
                radioAM.Checked = true;
            }
            else if (DemodState.SignalDemodulator.GetType() == typeof(FMDemodulator))
            {
                if (((FMDemodulator)DemodState.SignalDemodulator).Accurate)
                {
                    radioFMAccurate.Checked = true;
                }
                else
                {
                    radioFMFast.Checked = true;
                }
            }

            switch (DemodState.CursorWindowFilterWidthFract)
            {
                case 256:
                    radioFilter256.Checked = true;
                    break;
                case 128:
                    radioFilter128.Checked = true;
                    break;
                case 64:
                    radioFilter64.Checked = true;
                    break;
                case 32:
                    radioFilter32.Checked = true;
                    break;
                case 16:
                    radioFilter16.Checked = true;
                    break;
                case 8:
                    radioFilter8.Checked = true;
                    break;
                case 4:
                    radioFilter4.Checked = true;
                    break;
                case 2:
                    radioFilter2.Checked = true;
                    break;
            }

            switch (DemodState.AudioLowPassWidthFract)
            {
                case 256:
                    radioLowPass256.Checked = true;
                    break;
                case 128:
                    radioLowPass128.Checked = true;
                    break;
                case 64:
                    radioLowPass64.Checked = true;
                    break;
                case 32:
                    radioLowPass32.Checked = true;
                    break;
                case 16:
                    radioLowPass16.Checked = true;
                    break;
                case 8:
                    radioLowPass8.Checked = true;
                    break;
                case 4:
                    radioLowPass4.Checked = true;
                    break;
                case 2:
                    radioLowPass2.Checked = true;
                    break;
            }

            chkEnableSquelch.Checked = DemodState.SquelchEnabled;

            UpdateInformationInternal();
            UpdatePowerBarInternal();
        }

        private void cmbSourceFrequency_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lastFreq = DemodState.DemodulationFrequency;

            try
            {
                int pos = Array.FindIndex<string>(SourceFrequencyDesc, item => cmbSourceFrequency.Text == item );

                DemodState.SourceFrequency = (DemodulationState.eSourceFrequency)pos;
            }
            catch(ArgumentOutOfRangeException ex)
            {
                DemodState.SourceFrequency = DemodulationState.eSourceFrequency.Fixed;
            }

            bool selectorFixed = false;

            switch (DemodState.SourceFrequency)
            {
                /* when selecting "custom", use the last active frequency */
                case DemodulationState.eSourceFrequency.Fixed:
                    DemodState.DemodulationFrequencyFixed = lastFreq;
                    selectorFixed = false;
                    break;

                case DemodulationState.eSourceFrequency.Selection:
                    selectorFixed = false;
                    break;

                case DemodulationState.eSourceFrequency.Center:
                case DemodulationState.eSourceFrequency.Cursor:
                case DemodulationState.eSourceFrequency.Marker:
                    selectorFixed = true;
                    break;

                default:
                    Log.AddMessage("Did not care about DemodState.SourceFrequency = " + DemodState.SourceFrequency);
                    break;
            }

            frequencySelector.ReadOnly = selectorFixed;
        }

        private void frequencySelector_FrequencyChanged(object sender, EventArgs e)
        {
            switch (DemodState.SourceFrequency)
            {
                case DemodulationState.eSourceFrequency.Fixed:
                    if (DemodState.DemodulationFrequencyFixed != frequencySelector.Frequency)
                    {
                        DemodState.DemodulationFrequencyFixed = frequencySelector.Frequency;

                        if (FrequencyChanged != null)
                            FrequencyChanged(this, null);
                    }
                    break;

                case DemodulationState.eSourceFrequency.Selection:
                    if (DemodState.DemodulationFrequencySelection != frequencySelector.Frequency)
                    {
                        DemodState.DemodulationFrequencySelection = frequencySelector.Frequency;

                        if (FrequencyChanged != null)
                            FrequencyChanged(this, null);
                    }
                    break;

                case DemodulationState.eSourceFrequency.Center:
                case DemodulationState.eSourceFrequency.Cursor:
                case DemodulationState.eSourceFrequency.Marker:
                    break;
            }
        }

        public double Frequency
        {
            get
            {
                return (double)DemodState.DemodulationFrequency;
            }
            set
            {
                try
                {
                    BeginInvoke(new Action(() => frequencySelector.Frequency = (long)value));
                }
                catch (Exception)
                {
                }
            }
        }

        private DateTime LastDataUpdate = DateTime.Now;

        void Demod_DataUpdated(object sender, EventArgs e)
        {
            lock (this)
            {
                if ((DateTime.Now - LastDataUpdate).TotalMilliseconds > 500)
                {
                    UpdateInformation();
                    UpdatePowerBar();
                    LastDataUpdate = DateTime.Now;
                }
            }

        }

        void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateInformationInternal(false);
        }

        public void UpdateInformation()
        {
            try
            {
                BeginInvoke(new MethodInvoker(UpdateInformationInternal));
            }
            catch (Exception e)
            {
            }
        }

        public void UpdateFrequency()
        {
            try
            {
                BeginInvoke(new MethodInvoker(UpdateFrequencyInternal));
            }
            catch (Exception e)
            {
            }
        }

        private void UpdateFrequencyInternal()
        {
            this.cmbSourceFrequency.Text = SourceFrequencyDesc[(int)DemodState.SourceFrequency];
            frequencySelector.Frequency = DemodState.DemodulationFrequency;
        }

        public void UpdatePowerBar()
        {
            try
            {
                BeginInvoke(new MethodInvoker(UpdatePowerBarInternal));
            }
            catch (Exception e)
            {
            }
        }

        private void UpdatePowerBarInternal()
        {
            barSquelchPower.Amplitude = (DemodState.SquelchAverage + 100) / 100;
            barSquelchPower.LinePosition = (DemodState.SquelchLowerLimit + 100) / 100;
            barSquelchPower.Enabled = DemodState.SquelchEnabled;
            barSquelchPower.Invalidate();
        }

        private void UpdateInformationInternal()
        {
            UpdateInformationInternal(false);
        }

        private void UpdateInformationInternal(bool notify)
        {
            txtDemodRate.Text = FrequencyFormatter.FreqToString(DemodState.AudioRate);
            txtDecim.Value = DemodState.AudioDecimation;

            /* update title bar */
            if (DemodState.Description != null)
            {
                Text = "Demodulation: " + DemodState.Description;
            }

            /* colorize frequency */
            if (DemodState.DemodulationPossible)
            {
                frequencySelector.ForeColor = Color.Cyan;
            }
            else
            {
                frequencySelector.ForeColor = Color.Red;
            }

            if (DemodState.SoundSinkInfos.Count == 0)
            {
                txtStatus.Text = "No output opened";
            }
            else
            {
                txtStatus.Text = "";
                lock (DemodState.SoundSinkInfos)
                {
                    foreach (SoundSinkInfo info in DemodState.SoundSinkInfos)
                    {
                        txtStatus.Text += info.Sink.Status + " ";
                    }
                }
            }

            if (!DemodState.SquelchEnabled)
            {
                DemodState.SquelchAverage = -50;
            }

            /*
            if (Demod.SoundDevice == null)
            {
                txtSamplingRate.Text = FrequencyFormatter.FreqToString(Demod.InputRate / Demod.InputSignalDecimation / Demod.AudioDecimation);
            }
            else
            {
                txtSamplingRate.Text = FrequencyFormatter.FreqToString(Demod.SoundDevice.Rate);
            }
            */

            if (DemodState.InputRate != 0)
            {
                radioFilter2.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 2);
                radioFilter4.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 4);
                radioFilter8.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 8);
                radioFilter16.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 16);
                radioFilter32.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 32);
                radioFilter64.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 64);
                radioFilter128.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 128);
                radioFilter256.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 256);
            }
            else
            {
                radioFilter2.Text = "/2";
                radioFilter4.Text = "/4";
                radioFilter8.Text = "/8";
                radioFilter16.Text = "/16";
                radioFilter32.Text = "/32";
                radioFilter64.Text = "/64";
                radioFilter128.Text = "/128";
                radioFilter256.Text = "/256";
            }

            if (DemodState.InputRate != 0)
            {
                radioLowPass2.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 2 / DemodState.InputSignalDecimation);
                radioLowPass4.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 4 / DemodState.InputSignalDecimation);
                radioLowPass8.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 8 / DemodState.InputSignalDecimation);
                radioLowPass16.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 16 / DemodState.InputSignalDecimation);
                radioLowPass32.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 32 / DemodState.InputSignalDecimation);
                radioLowPass64.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 64 / DemodState.InputSignalDecimation);
                radioLowPass128.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 128 / DemodState.InputSignalDecimation);
                radioLowPass256.Text = FrequencyFormatter.FreqToString(DemodState.InputRate / 256 / DemodState.InputSignalDecimation);
            }
            else
            {
                radioLowPass2.Text = "/2";
                radioLowPass4.Text = "/4";
                radioLowPass8.Text = "/8";
                radioLowPass16.Text = "/16";
                radioLowPass32.Text = "/32";
                radioLowPass64.Text = "/64";
                radioLowPass128.Text = "/128";
                radioLowPass256.Text = "/256";
            }

            txtSquelchAvg.Text = String.Format("{0:0.00}", DemodState.SquelchAverage);
            txtSquelchMax.Text = String.Format("{0:0.00}", DemodState.SquelchMax);

            if (notify)
            {
                DemodState.UpdateListeners();
            }
        }

        private void chkEnableDemod_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.DemodulationEnabled = chkEnableDemod.Checked;

                DemodState.UpdateSinks();
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void chkEnableCursorWin_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.CursorPositionWindowEnabled = chkEnableLimiter.Checked;
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioAM_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.SignalDemodulator = new AMDemodulator();
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioLSB_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.SignalDemodulator = new SSBDemodulator(eSsbType.Lsb);
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioUSB_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.SignalDemodulator = new SSBDemodulator(eSsbType.Usb);
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioFMFast_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.SignalDemodulator = new FMDemodulator();
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);                
            }
        }

        private void radioFMAccurate_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.SignalDemodulator = new FMDemodulator();
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);                
            }
        }

        private void radioFilter2_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter2.Checked)
                return;
            lock (DemodState)
            {
                DemodState.CursorWindowFilterWidthFract = 2;
//                Demod.CursorWindowFilterI = new FIRFilter(FIRCoefficients.FIRLowPass_2_Low);
//                Demod.CursorWindowFilterQ = new FIRFilter(FIRCoefficients.FIRLowPass_2_Low);
                DemodState.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_2);
                DemodState.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_2);
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioFilter4_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter4.Checked)
                return;
            lock (DemodState)
            {
                DemodState.CursorWindowFilterWidthFract = 4;
                DemodState.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_4);
                DemodState.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_4);
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioFilter8_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter8.Checked)
                return;
            lock (DemodState)
            {
                DemodState.CursorWindowFilterWidthFract = 8;
                DemodState.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_8);
                DemodState.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_8);
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioFilter16_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter16.Checked)
                return;
            lock (DemodState)
            {
                DemodState.CursorWindowFilterWidthFract = 16;
                DemodState.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_16);
                DemodState.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_16);
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioFilter32_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter32.Checked)
                return;
            lock (DemodState)
            {
                DemodState.CursorWindowFilterWidthFract = 32;
                DemodState.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_32);
                DemodState.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_32);
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioFilter64_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter64.Checked)
                return;
            lock (DemodState)
            {
                DemodState.CursorWindowFilterWidthFract = 64;
                DemodState.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_64);
                DemodState.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_64);
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioFilter128_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter128.Checked)
                return;
            lock (DemodState)
            {
                DemodState.CursorWindowFilterWidthFract = 128;
                DemodState.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_128);
                DemodState.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_128);
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void radioFilter256_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter256.Checked)
                return;
            lock (DemodState)
            {
                DemodState.CursorWindowFilterWidthFract = 256;
                DemodState.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_256);
                DemodState.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_256);
                DemodState.ReinitSound = true;
                UpdateInformationInternal(true);
            }
        }

        private void chkEnableLowpass_CheckedChanged(object sender, EventArgs e)
        {
            DemodState.AudioLowPassEnabled = chkEnableLowpass.Checked;
            UpdateInformationInternal(true); 
        }

        private void radioLowPass2_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.AudioLowPassWidthFract = 2;
                DemodState.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_2_Low);
                UpdateInformationInternal(true);                
            }
        }

        private void radioLowPass4_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.AudioLowPassWidthFract = 4;
                DemodState.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_4_Low);
                UpdateInformationInternal(true);                
            }
        }

        private void radioLowPass8_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.AudioLowPassWidthFract = 8;
                DemodState.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_8_Low);
                UpdateInformationInternal(true);                
            }
        }

        private void radioLowPass16_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.AudioLowPassWidthFract = 16;
                DemodState.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_16_Low);
                UpdateInformationInternal(true);                
            }
        }

        private void radioLowPass32_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.AudioLowPassWidthFract = 32;
                DemodState.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_32_Low);
                UpdateInformationInternal(true);                
            }
        }
        
        private void radioLowPass64_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.AudioLowPassWidthFract = 64;
                DemodState.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_64_Low);
                UpdateInformationInternal(true);                
            }
        }

        private void radioLowPass128_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.AudioLowPassWidthFract = 128;
                DemodState.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_128_Low);
                UpdateInformationInternal(true);                
            }
        }

        private void radioLowPass256_CheckedChanged(object sender, EventArgs e)
        {
            lock (DemodState)
            {
                DemodState.AudioLowPassWidthFract = 256;
                DemodState.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_256_Low);
                UpdateInformationInternal(true);                
            }
        }

        private void chkAmplify_CheckedChanged(object sender, EventArgs e)
        {
            DemodState.AudioAmplificationEnabled = chkAmplify.Checked;
            txtAmplify.ReadOnly = !DemodState.AudioAmplificationEnabled;
            UpdateInformationInternal(true);
        }

        private void txtAmplify_TextChanged(object sender, EventArgs e)
        {
            double ampl;

            if (!double.TryParse(txtAmplify.Text, out ampl))
                return;

            DemodState.AudioAmplification = Math.Exp(ampl / 20);
            UpdateInformationInternal(true);
        }

        private void txtDecim_TextChanged(object sender, EventArgs e)
        {
            int decim;

            if (!int.TryParse(txtDecim.Text, out decim))
                return;

            DemodState.AudioDecimation = decim;
            DemodState.ReinitSound = true;
            UpdateInformationInternal(true);
        }

        private void chkNative_CheckedChanged(object sender, EventArgs e)
        {
            FIRFilter.UseNative = chkNative.Checked;
            IIRFilter.UseNative = chkNative.Checked;
            Demodulator.UseNative = chkNative.Checked;
            Downmixer.UseNative = chkNative.Checked;
            ByteUtil.UseNative = chkNative.Checked;
            UpdateInformationInternal(true);
            //FFTTransformer.UseFFTW = !chkNative.Checked;
        }

        private void chkEnableSquelch_CheckedChanged(object sender, EventArgs e)
        {
            DemodState.SquelchEnabled = chkEnableSquelch.Checked;
            barSquelchPower.Enabled = chkEnableSquelch.Checked;
            
            if(!DemodState.SquelchEnabled)
            {
                DemodState.SquelchAverage = -50;
            }

            barSquelchPower.Invalidate();
            UpdateInformationInternal(true);
        }

        private void txtSquelchLimit_TextChanged(object sender, EventArgs e)
        {
            double level;

            if (!double.TryParse(txtSquelchLimit.Text, out level))
                return;

            DemodState.SquelchLowerLimit = level;
            UpdateInformationInternal(true);
        }

        private void btnDemodFft_Click(object sender, EventArgs e)
        {
            if (DemodState.DemodView == null)
            {
                DemodState.DemodView = new DemodFFTView();
                DemodState.DemodView.Show();
            }
            else
            {
                DemodState.DemodView.Close();
                DemodState.DemodView = null;
            }
        }

        private void btnSound_Click(object sender, EventArgs e)
        {
            SoundSinkInfo info = new SoundSinkInfo();

            info.Page = new TabPage("Sound");
            info.Sink = new SoundCardSink(info.Page);

            PrepareSinkTab(info);
            DemodState.AddSink(info);
        }

        private void btnWav_Click(object sender, EventArgs e)
        {
            /*
            SoundSinkInfo info = new SoundSinkInfo();
            info.Page = new TabPage("WAV");

            PrepareSinkTab(info);
            DemodState.AddSink(info);
             * */
        }

        private void btnMp3_Click(object sender, EventArgs e)
        {
            SoundSinkInfo info = new SoundSinkInfo();
            info.Page = new TabPage("Stream");
            info.Sink = new ShoutcastSink(info.Page);

            PrepareSinkTab(info);
            DemodState.AddSink(info);
        }

        private void btnMp3File_Click(object sender, EventArgs e)
        {
            SoundSinkInfo info = new SoundSinkInfo();
            info.Page = new TabPage("MP3");
            info.Sink = new SoundFileSink(info.Page);

            PrepareSinkTab(info);
            DemodState.AddSink(info);
        }

        private void btnShmem_Click(object sender, EventArgs e)
        {
            SoundSinkInfo info = new SoundSinkInfo();
            info.Page = new TabPage("Shared Mem");
            info.Sink = new SharedMemSink(info.Page);

            PrepareSinkTab(info);
            DemodState.AddSink(info);
        }

        private void PrepareSinkTab(SoundSinkInfo info)
        {
            tabSoundOut.Controls.Add(info.Page);

            Label closeLabel = new Label();
            closeLabel.Text = "X";
            closeLabel.Dock = DockStyle.Right | DockStyle.Top;
            //closeLabel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            //closeLabel.Location = new System.Drawing.Point(info.Page.Size.Width - 15, 0);
            closeLabel.MouseClick += (object s, MouseEventArgs a) =>
            {
                if (a.Button == MouseButtons.Left)
                {
                    DemodState.RemoveSink(info);
                    //tabSoundOut.Controls.Remove(info.Page);
                }
            };
            closeLabel.MouseEnter += (object s, EventArgs a) =>
            {
                closeLabel.ForeColor = Color.Gray;
            };
            closeLabel.MouseLeave += (object s, EventArgs a) =>
            {
                closeLabel.ForeColor = Color.Black;
            };
            info.Page.Controls.Add(closeLabel);
        }
    }
}
