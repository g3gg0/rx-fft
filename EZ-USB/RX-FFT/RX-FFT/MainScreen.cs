using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.ShmemChain;
using System.Threading;
using LibRXFFT.Libraries.FFTW;
using LibRXFFT.Libraries.Demodulators;
using LibRXFFT.Libraries.Timers;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Components.DirectX;
using System.Threading;
using LibRXFFT.Libraries.USB_RX.Devices;
using LibRXFFT.Libraries.USB_RX.Misc;


namespace RX_FFT
{
    public partial class MainScreen : Form
    {
        public class PerformanceEnvelope
        {
            public HighPerformanceCounter CounterRuntime = new HighPerformanceCounter("Runtime");
            public HighPerformanceCounter CounterReading = new HighPerformanceCounter("Reading");
            public HighPerformanceCounter CounterProcessing = new HighPerformanceCounter("Processing");
            public HighPerformanceCounter CounterXlat = new HighPerformanceCounter("Translation");
            public HighPerformanceCounter CounterXlatLowpass = new HighPerformanceCounter("Translation LowPass");
            public HighPerformanceCounter CounterXlatDecimate = new HighPerformanceCounter("Translation Decim");
            public HighPerformanceCounter CounterDemod = new HighPerformanceCounter("Demodulation");
            public HighPerformanceCounter CounterDemodLowpass = new HighPerformanceCounter("Demodulation LowPass");
            public HighPerformanceCounter CounterDemodDecimate = new HighPerformanceCounter("Demodulation Decim");
            public HighPerformanceCounter CounterVisualization = new HighPerformanceCounter("Visualization");

            internal void Reset()
            {
                CounterRuntime.Reset();
                CounterReading.Reset();
                CounterProcessing.Reset();
                CounterXlat.Reset();
                CounterXlatLowpass.Reset();
                CounterXlatDecimate.Reset();
                CounterDemod.Reset();
                CounterDemodLowpass.Reset();
                CounterDemodDecimate.Reset();
                CounterVisualization.Reset();
            }
        }

        bool processPaused;
        bool ThreadActive;
        Thread ReadThread;
        DeviceControl Device;
        double LastSamplingRate = 48000;

        double[] DecimatedInputI = new double[0];
        double[] DecimatedInputQ = new double[0];
        double[] AudioSampleBuffer = new double[0];
        double[] AudioSampleBufferDecim = new double[0];
        Object SpinLock = new Object();

        Demodulation DemodOptions = new Demodulation();
        DemodulationDialog DemodDialog;
        PerformaceStatsWindow StatsDialog;

        PerformanceEnvelope PerformanceCounters = new PerformanceEnvelope();


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
                    if (Device != null)
                        Device.SamplesPerBlock = value * (int)Math.Max(1,FFTDisplay.SamplesToAverage);
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
                //double relative = (param / FFTDisplay.Width) - 1;
                double relative = FFTDisplay.RelativeCursrorXPos;

                DemodOptions.DemodulationDownmixer.TimeStep = -relative * (2 * Math.PI);
            }
            else if (evt == eUserEvent.MouseClickRight)
            {
                if (Device != null)
                {
                    long freq = FFTDisplay.Frequency;
                    Device.SetFrequency(freq);
                    FFTDisplay.CenterFrequency = freq;
                }
            }
        }

        void FFTReadFunc()
        {
            double lastRate = 0;
            int lastAudioDecim = 1;
            int lastInputDecim = 1;
            bool lastCursorWinEnabled = false;

            double[] inputI;
            double[] inputQ;

            PerformanceCounters.Reset();

            PerformanceCounters.CounterRuntime.Start();

            while (ThreadActive)
            {
                //dev.Read(inBuffer);
                lock (SpinLock)
                {
                    PerformanceCounters.CounterRuntime.Update();

                    PerformanceCounters.CounterReading.Start();
                    double rate = Device.SampleSource.InputSamplingRate;
                    lock (Device.SampleSource)
                    {
                        Device.SampleSource.Read();
                        PerformanceCounters.CounterReading.Stop();

                        PerformanceCounters.CounterProcessing.Start();

                        inputI = Device.SampleSource.SourceSamplesI;
                        inputQ = Device.SampleSource.SourceSamplesQ;

                        lock (DemodOptions)
                        {
                            bool blockSizeChanged = AudioSampleBuffer.Length != (inputI.Length / lastInputDecim);
                            bool rateChanged = Math.Abs(lastRate - rate) > 0;
                            lastCursorWinEnabled = DemodOptions.CursorPositionWindowEnabled;

                            if (blockSizeChanged || DemodOptions.ReinitSound || rateChanged)
                            {
                                DemodOptions.ReinitSound = false;

                                FFTDisplay.SamplingRate = rate;
                                DemodOptions.InputRate = rate;
                                DemodOptions.AudioRate = rate / DemodOptions.AudioDecimation / DemodOptions.InputSignalDecimation;
                                DemodOptions.SoundDevice.SetRate((int)DemodOptions.AudioRate);

                                if (DemodDialog != null && DemodDialog.Visible)
                                    DemodDialog.UpdateInformation();

                                LastSamplingRate = rate;

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
                        }

                        if (!processPaused)
                        {
                            /* first display on screen */
                            if (!DemodOptions.DisplayDemodulationSignal)
                            {
                                PerformanceCounters.CounterVisualization.Start();
                                FFTDisplay.ProcessData(inputI, inputQ);
                                PerformanceCounters.CounterVisualization.Stop();
                            }

                            lock (DemodOptions)
                            {
                                if (DemodOptions.DemodulationEnabled)
                                {
                                    if (lastCursorWinEnabled)
                                    {
                                        /* frequency translation */
                                        PerformanceCounters.CounterXlat.Start();
                                        DemodOptions.DemodulationDownmixer.ProcessData(inputI, inputQ, inputI, inputQ);
                                        PerformanceCounters.CounterXlat.Stop();

                                        /* lowpass */
                                        PerformanceCounters.CounterXlatLowpass.Start();
#if false
                                    // 51% CPU time with 79% CPU load
                                    DemodOptions.CursorWindowFilterI.Process(inputI, inputI);
                                    DemodOptions.CursorWindowFilterQ.Process(inputQ, inputQ);
#else
                                        // 43% (with 53% CPU load)
                                        DemodOptions.CursorWindowFilterThreadI.Process(inputI, inputI);
                                        DemodOptions.CursorWindowFilterThreadQ.Process(inputQ, inputQ);
                                        WaitHandle.WaitAll(DemodOptions.CursorWindowFilterEvents);
#endif
                                        PerformanceCounters.CounterXlatLowpass.Stop();

                                        /* decimate input signal */
                                        PerformanceCounters.CounterXlatDecimate.Start();
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
                                        PerformanceCounters.CounterXlatDecimate.Stop();
                                    }

                                    /* demodulate signal */
                                    PerformanceCounters.CounterDemod.Start();
                                    DemodOptions.Demod.ProcessData(inputI, inputQ, AudioSampleBuffer);
                                    PerformanceCounters.CounterDemod.Stop();

                                    if (DemodOptions.AudioLowPassEnabled)
                                    {
                                        PerformanceCounters.CounterDemodLowpass.Start();
                                        DemodOptions.AudioLowPass.Process(AudioSampleBuffer, AudioSampleBuffer);
                                        PerformanceCounters.CounterDemodLowpass.Stop();
                                    }

                                    /* audio decimation */
                                    if (lastAudioDecim > 1)
                                    {
                                        PerformanceCounters.CounterDemodDecimate.Start();
                                        double ampl = 1;
                                        if (DemodOptions.AudioAmplificationEnabled)
                                            ampl = DemodOptions.AudioAmplification;

                                        for (int pos = 0; pos < AudioSampleBufferDecim.Length; pos++)
                                        {
                                            AudioSampleBufferDecim[pos] = ampl * AudioSampleBuffer[pos * lastAudioDecim];
                                        }
                                        PerformanceCounters.CounterDemodDecimate.Stop();

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
                            {
                                PerformanceCounters.CounterVisualization.Start();
                                FFTDisplay.ProcessData(inputI, inputQ);
                                PerformanceCounters.CounterVisualization.Stop();
                            }
                        }
                        PerformanceCounters.CounterProcessing.Stop();
                    }
                }
            }

            if (DemodDialog != null)
                DemodDialog.UpdateInformation();

            PerformanceCounters.CounterRuntime.Stop();

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (Device != null)
            {
                if (ReadThread != null)
                {
                    ThreadActive = false;
                    Thread.Sleep(10);

                    ReadThread.Abort();
                }

                Device.Close();
                Device = null;

                lock (DemodOptions)
                {
                    DemodOptions.SoundDevice.Close();
                    DemodOptions.SoundDevice = null;
                }

                btnOpen.Text = "Open";
            }
            else
            {
                Device = new USBRXDeviceControl();
                if (Device.Connected)
                {
                    Device.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
                    Device.SampleSource.SamplingRateChangedEvent += new EventHandler(SampleSource_SamplingRateChangedEvent);

                    Device.SetFrequency(103000000);
                    Device.StartRead();


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
                else
                {
                    Device.Close();
                    Device = null;
                }
            }
        }

        void Device_FrequencyChanged(object sender, EventArgs e)
        {
            double freq = Device.GetFrequency();
            FFTDisplay.CenterFrequency = freq;
        }

        void SampleSource_SamplingRateChangedEvent(object sender, EventArgs e)
        {

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

            if (Device != null)
            {
                lock (Device.SampleSource)
                {
                    Device.SampleSource.SamplesPerBlock = size;
                }
            }
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
                if (Device.SampleSource != null)
                    Device.SampleSource.InputSamplingRate = rate;
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
            if (Device != null)
                Device.SamplesPerBlock = FFTSize * (int)Math.Max(1, FFTDisplay.SamplesToAverage);
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

        private void btnStats_Click(object sender, EventArgs e)
        {
            if (StatsDialog != null)
            {
                StatsDialog.Close();
                StatsDialog = null;
            }
            else
            {
                StatsDialog = new PerformaceStatsWindow(PerformanceCounters);
                StatsDialog.Show();
            }
        }

    }
}
