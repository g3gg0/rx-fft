using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.ShmemChain;
using System.Threading;
using LibRXFFT.Libraries.FFTW;
using LibRXFFT.Libraries.Demodulators;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Components.DirectX;

namespace RX_FFT
{
    public partial class MainScreen : Form
    {
        bool processPaused;
        bool ThreadActive;
        Thread ReadThread;
        SampleSource SampleSource;

        double[] DecimatedInputI = new double[0];
        double[] DecimatedInputQ = new double[0];
        double[] AudioSampleBuffer = new double[0];
        double[] AudioSampleBufferDecim = new double[0];
        Object SpinLock = new Object();

        Demodulation DemodOptions = new Demodulation();
        DemodulationDialog DemodDialog;


        public MainScreen()
        {
            InitializeComponent();
            FFTDisplay.UserEventCallback = UserEventCallbackFunc;

            FFTSize = 2048;

            string[] windowingTypes = Enum.GetNames(typeof(FFTTransformer.eWindowingFunction));
            cmbWindowFunc.Items.AddRange(windowingTypes);
            cmbWindowFunc.Text = FFTDisplay.WindowingFunction.ToString();
            cmbAverage.Text = FFTDisplay.Averaging.ToString();
            cmbFFTSize.Text = FFTSize.ToString();
            txtUpdatesPerSecond.Text = FFTDisplay.UpdateRate.ToString();
            txtAverageSamples.Text = FFTDisplay.SamplesToAverage.ToString();
        }

        int FFTSize
        {
            get { return FFTDisplay.FFTSize; }
            set
            {
                lock (SpinLock)
                {
                    FFTDisplay.FFTSize = value;
                }
            }
        }

        delegate void setRateDelegate(double rate);
        void setRate(double rate)
        {
            txtSamplingRate.Text = rate.ToString();
        }

        public void UserEventCallbackFunc(eUserEvent evt, double param)
        {
            if (evt == eUserEvent.MousePosX)
            {
                double relative = (2 * param / FFTDisplay.Width) - 1;

                DemodOptions.DemodulationDownmixer.TimeStep = -relative * Math.PI;
            }
        }

        void FFTReadFunc()
        {
            double lastRate = 0;
            int lastAudioDecim = 1;
            int lastInputDecim = 1;

            double[] inputI;
            double[] inputQ;

            while (ThreadActive)
            {
                //dev.Read(inBuffer);
                lock (SpinLock)
                {
                    double rate = SampleSource.InputSamplingRate;


                    SampleSource.Read();
                    inputI = SampleSource.SourceSamplesI;
                    inputQ = SampleSource.SourceSamplesQ;

                    bool blockSizeChanged = AudioSampleBuffer.Length != (inputI.Length / lastInputDecim);
                    bool rateChanged = Math.Abs(lastRate - rate) > 0;

                    if (blockSizeChanged || DemodOptions.ReinitSound || rateChanged)
                    {
                        DemodOptions.ReinitSound = false;

                        FFTDisplay.SamplingRate = rate;
                        DemodOptions.InputRate = rate;
                        DemodOptions.AudioRate = rate / DemodOptions.AudioDecimation / DemodOptions.InputSignalDecimation;
                        DemodOptions.SoundDevice.SetRate((int)DemodOptions.AudioRate);

                        if (DemodDialog != null && DemodDialog.Visible)
                            DemodDialog.UpdateInformation();

                        try
                        {
                            this.BeginInvoke(new setRateDelegate(setRate), rate);
                        }
                        catch (Exception)
                        {
                        }

                        lastRate = rate;
                        lastAudioDecim = DemodOptions.AudioDecimation;
                        lastInputDecim = DemodOptions.InputSignalDecimation;

                        AudioSampleBuffer = new double[inputI.Length / lastInputDecim];
                        AudioSampleBufferDecim = new double[inputI.Length / lastAudioDecim / lastInputDecim];

                        DecimatedInputI = new double[inputI.Length / lastInputDecim];
                        DecimatedInputQ = new double[inputQ.Length / lastInputDecim];
                    }

                    if (!processPaused)
                    {
                        /* first display on screen */
                        if (!DemodOptions.DisplayDemodulationSignal)
                            FFTDisplay.ProcessData(inputI, inputQ);

                        lock (DemodOptions)
                        {
                            if (DemodOptions.DemodulationEnabled)
                            {
                                if (DemodOptions.CursorPositionWindowEnabled)
                                {
                                    /* frequency translation */
                                    DemodOptions.DemodulationDownmixer.ProcessData(inputI, inputQ, inputI, inputQ);

                                    /* lowpass */
                                    DemodOptions.CursorWindowFilterI.Process(inputI, inputI);
                                    DemodOptions.CursorWindowFilterQ.Process(inputQ, inputQ);

                                    /* decimate input signal */
                                    if (lastInputDecim > 1)
                                    {
                                        for (int pos = 0; pos < DecimatedInputI.Length; pos++)
                                        {
                                            DecimatedInputI[pos] = inputI[pos * lastInputDecim];
                                            DecimatedInputQ[pos] = inputQ[pos * lastInputDecim];
                                        }

                                        inputI = DecimatedInputI;
                                        inputQ = DecimatedInputQ;
                                    }
                                }

                                /* demodulate signal */
                                DemodOptions.Demod.ProcessData(inputI, inputQ, AudioSampleBuffer);

                                if (DemodOptions.AudioLowPassEnabled)
                                {
                                    DemodOptions.AudioLowPass.Process(AudioSampleBuffer, AudioSampleBuffer);
                                }

                                if (DemodOptions.AudioDecimation > 1)
                                {
                                    double ampl = 1;
                                    if (DemodOptions.AudioAmplificationEnabled)
                                        ampl = DemodOptions.AudioAmplification;

                                    for (int pos = 0; pos < AudioSampleBufferDecim.Length; pos++)
                                    {
                                        AudioSampleBufferDecim[pos] = ampl * AudioSampleBuffer[pos * lastAudioDecim];
                                    }
                                    DemodOptions.SoundDevice.Write(AudioSampleBufferDecim);
                                }
                                else
                                {
                                    if (DemodOptions.AudioAmplificationEnabled)
                                    {
                                        for (int pos = 0; pos < AudioSampleBuffer.Length; pos++)
                                        {
                                            AudioSampleBuffer[pos] *= DemodOptions.AudioAmplification;
                                        }
                                    } 
                                    DemodOptions.SoundDevice.Write(AudioSampleBuffer);
                                }
                            }
                        }

                        if (DemodOptions.DisplayDemodulationSignal)
                            FFTDisplay.ProcessData(inputI, inputQ);
                    }
                }
            }

            if (DemodDialog != null)
                DemodDialog.UpdateInformation();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (SampleSource != null)
            {
                ThreadActive = false;
                Thread.Sleep(10);
                ReadThread.Abort();

                SampleSource.Close();
                SampleSource = null;

                lock (DemodOptions)
                {
                    DemodOptions.SoundDevice.Close();
                    DemodOptions.SoundDevice = null;
                }

                btnOpen.Text = "Open";
            }
            else
            {
                SampleSource = new ShmemSampleSource("FFT Display", 1, 48000);

                lock (DemodOptions)
                {
                    DemodOptions.SoundDevice = new DXSoundDevice(Handle);
                }

                ThreadActive = true;
                ReadThread = new Thread(new ThreadStart(FFTReadFunc));
                ReadThread.Name = "MainScreen Data Read Thread";

                ReadThread.Start();

                btnOpen.Text = "Close";
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            processPaused = !processPaused;
        }

        private void cmbWindowFunc_TextChanged(object sender, EventArgs e)
        {
            string typeString = cmbWindowFunc.Text;

            try
            {
                FFTTransformer.eWindowingFunction type = (FFTTransformer.eWindowingFunction)Enum.Parse(typeof(FFTTransformer.eWindowingFunction), typeString);
                FFTDisplay.WindowingFunction = type;
            }
            catch (Exception ex)
            {
            }
        }

        private void cmbAverage_TextChanged(object sender, EventArgs e)
        {
            double avg;

            if (!double.TryParse(cmbAverage.Text, out avg))
                return;
            FFTDisplay.Averaging = avg;
        }

        private void cmbFFTSize_TextChanged(object sender, EventArgs e)
        {
            int size;

            if (!int.TryParse(cmbFFTSize.Text, out size))
                return;

            FFTSize = size;
            FFTDisplay.FFTSize = size;
        }

        private void txtSamplingRate_TextChanged(object sender, EventArgs e)
        {
            long rate;

            if (!long.TryParse(txtSamplingRate.Text, out rate))
                return;

            FFTDisplay.SamplingRate = rate;

            lock (DemodOptions)
            {
                DemodOptions.InputRate = rate;
                DemodOptions.AudioRate = rate / DemodOptions.AudioDecimation / DemodOptions.InputSignalDecimation;
                if (DemodOptions.SoundDevice != null)
                {
                    DemodOptions.SoundDevice.SetRate((int)DemodOptions.AudioRate);
                }
            }

            if (DemodDialog != null)
                DemodDialog.UpdateInformation();
        }

        private void txtUpdatesPerSecond_TextChanged(object sender, EventArgs e)
        {
            double rate;

            if (!double.TryParse(txtUpdatesPerSecond.Text, out rate))
                return;

            FFTDisplay.UpdateRate = rate;
        }

        private void txtAverageSamples_TextChanged(object sender, EventArgs e)
        {
            long samples;

            if (!long.TryParse(txtAverageSamples.Text, out samples))
                return;

            FFTDisplay.SamplesToAverage = samples;
        }

        private void chkRecording_CheckedChanged(object sender, EventArgs e)
        {
            FFTDisplay.SavingEnabled = chkRecording.Checked;
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            FFTDisplay.SavingName = txtFileName.Text;
        }

        private void btnDemod_Click(object sender, EventArgs e)
        {
            if (DemodDialog != null)
            {
                DemodDialog.Close();
                DemodDialog = null;
            }
            else
            {
                DemodDialog = new DemodulationDialog(DemodOptions);
                DemodDialog.Show();
            }
        }

    }
}
