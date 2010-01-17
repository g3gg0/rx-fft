using System;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.Demodulators;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SignalProcessing;

namespace RX_FFT.Dialogs
{
    public partial class DemodulationDialog : Form
    {
        private Demodulation Demod;
        private Timer UpdateTimer;

        public DemodulationDialog(Demodulation demod)
        {
            InitializeComponent();
            Demod = demod;

            chkEnableDemod.Checked = Demod.DemodulationEnabled;
            chkEnableCursorWin.Checked = Demod.CursorPositionWindowEnabled;
            chkAmplify.Checked = Demod.AudioAmplificationEnabled;
            chkNative.Checked = FIRFilter.UseNative;
            
            txtAmplify.Text = Demod.AudioAmplification.ToString();
            txtDecim.Text = Demod.AudioDecimation.ToString();
            txtSquelchLimit.Text = Demod.SquelchLowerLimit.ToString();

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


            if (Demod.Demod.GetType() == typeof(AMDemodulator))
            {
                radioAM.Checked = true;
            }
            else if (Demod.Demod.GetType() == typeof(FMDemodulator))
            {
                if (((FMDemodulator)Demod.Demod).Accurate)
                    radioFMAccurate.Checked = true;
                else
                    radioFMFast.Checked = true;
            }

            switch (Demod.CursorWindowFilterWidthFract)
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

            switch (Demod.AudioLowPassWidthFract)
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

            chkEnableSquelch.Checked = Demod.SquelchEnabled;
            if (!Demod.SquelchEnabled)
            {
                Demod.SquelchAverage = -50;
            }

            UpdateInformationInternal();
            UpdatePowerBarInternal();

            UpdateTimer = new Timer();
            UpdateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            UpdateTimer.Interval = 500;
            UpdateTimer.Start();
        }

        void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateInformationInternal();
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
            barSquelchPower.Amplitude = (Demod.SquelchAverage + 100) / 100;
            barSquelchPower.LinePosition = (Demod.SquelchLowerLimit + 100) / 100;
            barSquelchPower.Enabled = Demod.SquelchEnabled;
            barSquelchPower.Invalidate();
        }

        private void UpdateInformationInternal()
        {
            Demod.AudioRate = Demod.InputRate / Demod.AudioDecimation / Demod.InputSignalDecimation;

            if (Demod.SoundDevice == null)
                txtStatus.Text = "No Device opened";
            else 
                txtStatus.Text = Demod.SoundDevice.Status;

            if (Demod.SoundDevice == null)
                txtSamplingRate.Text = FrequencyFormatter.FreqToString(Demod.InputRate / Demod.InputSignalDecimation / Demod.AudioDecimation);
            else
                txtSamplingRate.Text = FrequencyFormatter.FreqToString(Demod.SoundDevice.Rate);

            if (Demod.InputRate != 0)
            {
                radioFilter2.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 2);
                radioFilter4.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 4);
                radioFilter8.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 8);
                radioFilter16.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 16);
                radioFilter32.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 32);
                radioFilter64.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 64);
                radioFilter128.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 128);
                radioFilter256.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 256);
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

            if (Demod.InputRate != 0)
            {
                radioLowPass2.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 2 / Demod.InputSignalDecimation);
                radioLowPass4.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 4 / Demod.InputSignalDecimation);
                radioLowPass8.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 8 / Demod.InputSignalDecimation);
                radioLowPass16.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 16 / Demod.InputSignalDecimation);
                radioLowPass32.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 32 / Demod.InputSignalDecimation);
                radioLowPass64.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 64 / Demod.InputSignalDecimation);
                radioLowPass128.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 128 / Demod.InputSignalDecimation);
                radioLowPass256.Text = FrequencyFormatter.FreqToString(Demod.InputRate / 256 / Demod.InputSignalDecimation);
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

            txtSquelchAvg.Text = String.Format("{0:0.00}", Demod.SquelchAverage);
            txtSquelchMax.Text = String.Format("{0:0.00}", Demod.SquelchMax);
        }

        private void chkEnableDemod_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.DemodulationEnabled = chkEnableDemod.Checked;
                if (!Demod.DemodulationEnabled && Demod.SoundDevice != null)
                    Demod.SoundDevice.Stop();
            }
        }

        private void chkEnableCursorWin_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.CursorPositionWindowEnabled = chkEnableCursorWin.Checked;
                Demod.ReinitSound = true;
                UpdateInformationInternal();
            }
        }

        private void radioAM_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.Demod = new AMDemodulator();
                UpdateInformationInternal();                
            }
        }

        private void radioFMFast_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.Demod = new FMDemodulator();
                UpdateInformationInternal();                
            }
        }

        private void radioFMAccurate_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.Demod = new FMDemodulator();
                UpdateInformationInternal();                
            }
        }

        private void radioFilter2_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter2.Checked)
                return;
            lock (Demod)
            {
                Demod.CursorWindowFilterWidthFract = 2;
//                Demod.CursorWindowFilterI = new FIRFilter(FIRCoefficients.FIRLowPass_2_Low);
//                Demod.CursorWindowFilterQ = new FIRFilter(FIRCoefficients.FIRLowPass_2_Low);
                Demod.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_2);
                Demod.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_2);
                Demod.ReinitSound = true;
                UpdateInformationInternal();
            }
        }

        private void radioFilter4_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter4.Checked)
                return;
            lock (Demod)
            {
                Demod.CursorWindowFilterWidthFract = 4;
                Demod.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_4);
                Demod.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_4);
                Demod.ReinitSound = true;
                UpdateInformationInternal();
            }
        }

        private void radioFilter8_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter8.Checked)
                return;
            lock (Demod)
            {
                Demod.CursorWindowFilterWidthFract = 8;
                Demod.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_8);
                Demod.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_8);
                Demod.ReinitSound = true;
                UpdateInformationInternal();
            }
        }

        private void radioFilter16_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter16.Checked)
                return;
            lock (Demod)
            {
                Demod.CursorWindowFilterWidthFract = 16;
                Demod.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_16);
                Demod.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_16);
                Demod.ReinitSound = true;
                UpdateInformationInternal();
            }
        }

        private void radioFilter32_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter32.Checked)
                return;
            lock (Demod)
            {
                Demod.CursorWindowFilterWidthFract = 32;
                Demod.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_32);
                Demod.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_32);
                Demod.ReinitSound = true;
                UpdateInformationInternal();
            }
        }

        private void radioFilter64_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter64.Checked)
                return;
            lock (Demod)
            {
                Demod.CursorWindowFilterWidthFract = 64;
                Demod.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_64);
                Demod.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_64);
                Demod.ReinitSound = true;
                UpdateInformationInternal();
            }
        }

        private void radioFilter128_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter128.Checked)
                return;
            lock (Demod)
            {
                Demod.CursorWindowFilterWidthFract = 128;
                Demod.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_128);
                Demod.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_128);
                Demod.ReinitSound = true;
                UpdateInformationInternal();
            }
        }

        private void radioFilter256_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioFilter256.Checked)
                return;
            lock (Demod)
            {
                Demod.CursorWindowFilterWidthFract = 256;
                Demod.CursorWindowFilterI = new IIRFilter(IIRCoefficients.IIRLowPass_256);
                Demod.CursorWindowFilterQ = new IIRFilter(IIRCoefficients.IIRLowPass_256);
                Demod.ReinitSound = true;
                UpdateInformationInternal();
            }
        }

        private void chkEnableLowpass_CheckedChanged(object sender, EventArgs e)
        {
            Demod.AudioLowPassEnabled = chkEnableLowpass.Checked;
        }

        private void radioLowPass2_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.AudioLowPassWidthFract = 2;
                Demod.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_2_Low);
                UpdateInformationInternal();                
            }
        }

        private void radioLowPass4_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.AudioLowPassWidthFract = 4;
                Demod.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_4_Low);
                UpdateInformationInternal();                
            }
        }

        private void radioLowPass8_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.AudioLowPassWidthFract = 8;
                Demod.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_8_Low);
                UpdateInformationInternal();                
            }
        }

        private void radioLowPass16_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.AudioLowPassWidthFract = 16;
                Demod.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_16_Low);
                UpdateInformationInternal();                
            }
        }

        private void radioLowPass32_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.AudioLowPassWidthFract = 32;
                Demod.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_32_Low);
                UpdateInformationInternal();                
            }
        }
        
        private void radioLowPass64_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.AudioLowPassWidthFract = 64;
                Demod.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_64_Low);
                UpdateInformationInternal();                
            }
        }

        private void radioLowPass128_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.AudioLowPassWidthFract = 128;
                Demod.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_128_Low);
                UpdateInformationInternal();                
            }
        }

        private void radioLowPass256_CheckedChanged(object sender, EventArgs e)
        {
            lock (Demod)
            {
                Demod.AudioLowPassWidthFract = 256;
                Demod.AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_256_Low);
                UpdateInformationInternal();                
            }
        }

        private void chkShowDemod_CheckedChanged(object sender, EventArgs e)
        {
            Demod.DisplayDemodulationSignal = chkShowDemod.Checked;
        }

        private void chkAmplify_CheckedChanged(object sender, EventArgs e)
        {
            Demod.AudioAmplificationEnabled = chkAmplify.Checked;
            txtAmplify.ReadOnly = !Demod.AudioAmplificationEnabled;
        }

        private void txtAmplify_TextChanged(object sender, EventArgs e)
        {
            double ampl;

            if (!double.TryParse(txtAmplify.Text, out ampl))
                return;

            Demod.AudioAmplification = ampl / 100;
            UpdateInformationInternal();
        }

        private void txtDecim_TextChanged(object sender, EventArgs e)
        {
            int decim;

            if (!int.TryParse(txtDecim.Text, out decim))
                return;

            Demod.AudioDecimation = decim;
            Demod.ReinitSound = true;
            UpdateInformationInternal();
        }

        private void chkNative_CheckedChanged(object sender, EventArgs e)
        {
            FIRFilter.UseNative = chkNative.Checked;
            IIRFilter.UseNative = chkNative.Checked;
            Demodulator.UseNative = chkNative.Checked;
            Downmixer.UseNative = chkNative.Checked;
            ByteUtil.UseNative = chkNative.Checked;

            //FFTTransformer.UseFFTW = !chkNative.Checked;
        }

        private void chkEnableSquelch_CheckedChanged(object sender, EventArgs e)
        {
            Demod.SquelchEnabled = chkEnableSquelch.Checked;
            barSquelchPower.Enabled = chkEnableSquelch.Checked;
            
            if(!Demod.SquelchEnabled)
            {
                Demod.SquelchAverage = -50;
            }

            barSquelchPower.Invalidate();
        }

        private void txtSquelchLimit_TextChanged(object sender, EventArgs e)
        {
            double level;

            if (!double.TryParse(txtSquelchLimit.Text, out level))
                return;

            Demod.SquelchLowerLimit = level;
            UpdatePowerBarInternal();
        }
    }
}
