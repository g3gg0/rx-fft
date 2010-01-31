using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DemodulatorCollection;
using GSM_Analyzer;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries.Timers;
using LibRXFFT.Libraries.USB_RX.Devices;
using LibRXFFT.Libraries.USB_RX.Tuners;
using RX_FFT.DeviceControls;
using RX_FFT.Dialogs;
using RX_Oscilloscope;
using Log = RX_FFT.Components.GDI.Log;
using Point = System.Drawing.Point;
using Timer = System.Threading.Timer;

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

        public delegate DigitalTuner delegateGetTuner();

        string BO35DeviceName = "BO-35 Digital Tuner";
        bool WindowActivated = true;
        bool ProcessPaused;
        bool ReadThreadRun;
        bool AudioThreadRun;

        private LinkedListNode<FrequencyMarker> CurrentScanFreq;

        private double ScanFrequenciesPowerMin = 66;
        bool _ScanFrequenciesEnabled = false;
        bool ScanFrequenciesEnabled
        {
            get
            {
                return _ScanFrequenciesEnabled;
            }
            set
            {
                _ScanFrequenciesEnabled = value;
                if (DeviceOpened)
                {
                    Device.ScanFrequenciesEnabled = value;
                }

                if (ScanFrequenciesEnabled && !ScanStrongestFrequency)
                {
                    FFTDisplay.SpectParts = ScanFrequencies.Count;
                    FFTDisplay.ChannelMode = true;
                }
                else
                {
                    FFTDisplay.SpectParts = 1;
                    FFTDisplay.ChannelMode = false;
                }
            }
        }

        public FrequencyBand ChannelBandDetails
        {
            get { return FFTDisplay.ChannelBandDetails; }
            set { FFTDisplay.ChannelBandDetails = value; }
        }

        Thread ReadThread;
        Thread AudioThread;
        DeviceControl Device;
        double LastSamplingRate = 48000;
        long MouseDragStartFreq;

        private LinkedList<FrequencyMarker> Markers = new LinkedList<FrequencyMarker>();
        private LinkedList<FrequencyMarker> ScanFrequencies = new LinkedList<FrequencyMarker>();

        ShmemSampleSource AudioShmem;
        double[] DecimatedInputI = new double[0];
        double[] DecimatedInputQ = new double[0];
        double[] AudioSampleBuffer = new double[0];
        double[] AudioSampleBufferDecim = new double[0];
        Object FFTSizeSpinLock = new Object();

        Demodulation DemodOptions = new Demodulation();
        DemodulationDialog DemodDialog;
        MarkerListDialog MarkerDialog;
        PerformaceStatsDialog StatsDialog;
        PerformanceEnvelope PerformanceCounters = new PerformanceEnvelope();

        RXOscilloscope OscilloscopeWindow = new RXOscilloscope();
        DemodulatorDialog DemodulatorWindow = new DemodulatorDialog();
        GSMAnalyzer GsmAnalyzerWindow = new GSMAnalyzer();

        bool _DeviceOpened = false;
        private bool ScanStrongestFrequency;
        private long _currentFrequency;
        private double CurrentMaxSignalDb = 0;

        /* try to set amplification to get a input power of -10dB */
        private double DesiredInputSignalPower = -10.0;

        private Timer StatusUpdateTimer;


        public MainScreen()
        {
            InitializeComponent();
            Log.Init();

            this.Icon = Icon.FromHandle(Icons.imgRhythmbox.GetHicon());

            this.fftSizeOtherMenu.KeyPress += new KeyPressEventHandler(this.fftSizeOtherMenu_KeyPress);
            this.updateRateText.KeyPress += new KeyPressEventHandler(this.updateRateText_KeyPress);
            this.averageSamplesText.KeyPress += new KeyPressEventHandler(this.averageSamplesText_KeyPress);
            this.verticalSmoothMenuText.KeyPress += new KeyPressEventHandler(this.verticalSmoothMenuText_KeyPress);

            updateRateText_KeyPress(null, null);
            averageSamplesText_KeyPress(null, null);
            verticalSmoothMenuText_KeyPress(null, null);


            /* register callback functions */
            FFTDisplay.UserEventCallback = UserEventCallbackFunc;

            FFTDisplay.AddUserEventCallback(eUserEvent.MouseEnter);
            /* options menu */
            FFTDisplay.AddUserEventCallback(eUserEvent.MouseClickRight);
            /* jump to frequency when doubleclicking */
            FFTDisplay.AddUserEventCallback(eUserEvent.MouseDoubleClickLeft);
            /* get feedback when mouse-frequency changed */
            FFTDisplay.AddUserEventCallback(eUserEvent.MousePosX);
            /* when starting to drag with control, remember drag start frequency */
            FFTDisplay.AddUserEventCallback(eUserEvent.MouseDownLeftControl);
            /* when dragging with CTRL pressed, "move" frequency */
            FFTDisplay.AddUserEventCallback(eUserEvent.MouseDragXControl);


            DeviceOpened = false;

            /* build the windowing function list dropdown menu */
            foreach (string windowName in Enum.GetNames(typeof(FFTTransformer.eWindowingFunction)))
            {
                ToolStripMenuItem menu = new ToolStripMenuItem();
                this.windowingFunctionMenu.DropDownItems.Add(menu);
                this.FormClosing += new FormClosingEventHandler(MainScreen_FormClosing);

                menu.Name = windowName;
                menu.Text = windowName;
                menu.Click += new EventHandler(delegate(object s, EventArgs e)
                {
                    ToolStripMenuItem sender = (ToolStripMenuItem)s;
                    string typeString = sender.Name;

                    try
                    {
                        FFTTransformer.eWindowingFunction type = (FFTTransformer.eWindowingFunction)Enum.Parse(typeof(FFTTransformer.eWindowingFunction), typeString);
                        FFTDisplay.WindowingFunction = type;
                    }
                    catch (Exception ex)
                    {
                    }
                });
            }

            ToolStripManager.LoadSettings(this);

            StatusUpdateTimer = new Timer(StatusUpdateTimerFunc, null, 500, 500);

        }

        private void StatusUpdateTimerFunc(object state)
        {
            if (!IsDisposed)
            {
                BeginInvoke(
                    new MethodInvoker(
                        () =>
                        {
                            maxDbLabel.Text = CurrentMaxSignalDb.ToString("#.# dB");
                            fpsLabel.Text =
                                string.Format("{0:0.##}", FFTDisplay.FpsProcessed) + " of " +
                                string.Format("{0:0.##}", FFTDisplay.FpsReceived);
                        }));
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopThreads();

            if (GsmAnalyzerWindow != null && !GsmAnalyzerWindow.IsDisposed)
            {
                GsmAnalyzerWindow.Close();
            }
            if (DemodulatorWindow != null && !DemodulatorWindow.IsDisposed)
            {
                DemodulatorWindow.Close();
            }
            if (OscilloscopeWindow != null && !OscilloscopeWindow.IsDisposed)
            {
                OscilloscopeWindow.Close();
            }

            CloseDevice();
            base.OnFormClosing(e);
        }

        private void MainScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            ToolStripManager.SaveSettings(this);
        }


        bool DeviceOpened
        {
            get { return _DeviceOpened; }
            set
            {
                _DeviceOpened = value;

                closeMenu.Enabled = DeviceOpened;
                pauseMenu.Enabled = DeviceOpened;
                openMenu.Enabled = !DeviceOpened;

                if (value)
                {
                    UpdateRate = UpdateRate;
                    FFTSize = FFTSize;
                    CurrentFrequency = CurrentFrequency;
                    statusLabel.Text = "Connected";
                }
                else
                {
                    statusLabel.Text = "Idle";
                }

                /* update transfer mode string */
                Device_TransferModeChanged(null, null);
            }
        }

        protected long CurrentFrequency
        {
            get
            {
                return _currentFrequency;
            }
            set
            {
                _currentFrequency = value;
                if (DeviceOpened)
                {
                    Device.SetFrequency(CurrentFrequency);
                }
            }
        }

        int FFTSize
        {
            get { return FFTDisplay.FFTSize; }
            set
            {
                if (Device != null)
                    Device.SamplesPerBlock = value * Math.Max(1, SamplesToAverage);

                if (FFTDisplay.FFTSize != value)
                {
                    lock (FFTSizeSpinLock)
                    {
                        FFTDisplay.FFTSize = value;
                    }
                }
            }
        }

        double UpdateRate
        {
            get { return FFTDisplay.UpdateRate; }
            set
            {
                if (Device != null)
                    Device.BlocksPerSecond = value;
                FFTDisplay.UpdateRate = value;
            }
        }

        int SamplesToAverage
        {
            get { return (int)FFTDisplay.SamplesToAverage; }
            set
            {
                if (Device != null)
                    Device.SamplesPerBlock = Math.Max(1, value) * FFTSize;
                FFTDisplay.SamplesToAverage = value;
            }
        }

        bool FitSpectrum
        {
            get
            {
                return FFTDisplay.FitSpectrumEnabled;
            }
            set
            {
                FFTDisplay.FitSpectrumEnabled = value;
                if (value)
                {
                    FFTDisplay.LimiterDisplayEnabled = false;
                }
            }
        }

        bool DisplayFilterMargins
        {
            get
            {
                return FFTDisplay.LimiterDisplayEnabled;
            }
            set
            {
                /* don't enable this when the spectrum is fit to filter width */
                if (FitSpectrum)
                {
                    FFTDisplay.LimiterDisplayEnabled = false;
                    return;
                }

                FFTDisplay.LimiterDisplayEnabled = value;
            }
        }

        delegate void setRateDelegate(double rate);
        void setRateTextbox(double rate)
        {
            samplingRateLabel.Text = FrequencyFormatter.FreqToStringAccurate(rate);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            WindowActivated = true;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            WindowActivated = false;
        }

        public void UserEventCallbackFunc(eUserEvent evt, double param)
        {
            switch (evt)
            {
                case eUserEvent.MousePosX:
                    {
                        double relative = FFTDisplay.RelativeCursorXPos;

                        if (Device != null && Device.InvertedSpectrum)
                        {
                            relative -= relative;
                        }

                        DemodOptions.DemodulationDownmixer.TimeStep = relative * (2 * Math.PI);
                    }
                    break;

                /* when mouse is moved into a plot and we are in foreground, update focus to hovered plot */
                case eUserEvent.MouseEnter:
                    if (WindowActivated)
                    {
                        FFTDisplay.FocusHovered();
                    }
                    break;

                /* when mouse is pressed with control down, user eventually starts to drag. save frequency */
                case eUserEvent.MouseDownLeftControl:
                    MouseDragStartFreq = FFTDisplay.Frequency;
                    break;

                /* dragged with control down. "move" radio band */
                case eUserEvent.MouseDragXControl:
                    {
                        long mouseFreq = FFTDisplay.Frequency;

                        long currentFreq = Device.GetFrequency();
                        long newFreq = currentFreq - (mouseFreq - MouseDragStartFreq);

                        //FFTDisplay.CenterFrequency = newFreq;
                        CurrentFrequency = newFreq;
                    }
                    break;

                /* jump to frequency under cursor */
                case eUserEvent.MouseDoubleClickLeft:
                    if (Device != null)
                    {
                        long freq = FFTDisplay.Frequency;

                        if (ScanFrequenciesEnabled)
                        {
                            ScanFrequenciesEnabled = false;
                        }

                        CurrentFrequency = freq;
                        //FFTDisplay.CenterFrequency = freq;
                    }
                    break;

                /* bring up popup menu. has to be improved */
                case eUserEvent.MouseClickRight:
                    {
                        long freq = FFTDisplay.Frequency;


                        ContextMenu contextMenu = new ContextMenu();
                        MenuItem menuItem1 = new MenuItem();
                        MenuItem menuItem2 = new MenuItem();
                        MenuItem menuItem3 = new MenuItem();
                        MenuItem menuItem4 = new MenuItem();
                        menuItem1.Enabled = false;

                        contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem1, menuItem2, menuItem3, menuItem4 });
                        menuItem1.Index = 0;
                        menuItem1.Text = "Frequency: " + FrequencyFormatter.FreqToStringAccurate(freq);
                        menuItem2.Index = 1;
                        menuItem2.Text = "-";
                        menuItem3.Index = 2;
                        menuItem3.Text = "Send to Locator";

                        menuItem4.Index = 3;
                        menuItem4.Text = "Add Marker...";
                        menuItem4.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            AddMarker(freq);
                        });

                        Point popupPos = this.PointToClient(MousePosition);

                        popupPos.X -= 20;
                        popupPos.Y -= 20;
                        contextMenu.Show(this, popupPos);
                    }
                    break;
            }
        }

        private void AddMarker(long freq)
        {
            FrequencyMarker marker = new FrequencyMarker(freq);
            MarkerDetailsDialog dlg = new MarkerDetailsDialog("Add Marker...", marker);

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Markers.AddLast(marker);
            FFTDisplay.Markers = Markers;

            /* notify marker window */
            if (MarkerDialog != null && !MarkerDialog.IsDisposed)
            {
                MarkerDialog.UpdateMarkerList();
            }
        }

        void AudioReadFunc()
        {
            double rate = 0;
            int lastAudioDecim = 1;
            int lastInputDecim = 1;
            bool lastCursorWinEnabled = false;

            double[] inputI;
            double[] inputQ;

            AudioShmem.SamplesPerBlock = 512;

            PerformanceCounters.Reset();

            PerformanceCounters.CounterRuntime.Start();

            while (AudioThreadRun)
            {
                //dev.Read(inBuffer);
                PerformanceCounters.CounterRuntime.Update();

                lock (AudioShmem)
                {
                    PerformanceCounters.CounterReading.Start();
                    AudioShmem.Read();
                    PerformanceCounters.CounterReading.Stop();

                    PerformanceCounters.CounterProcessing.Start();

                    inputI = AudioShmem.SourceSamplesI;
                    inputQ = AudioShmem.SourceSamplesQ;

                    lock (DemodOptions)
                    {
                        bool blockSizeChanged = AudioSampleBuffer.Length != (inputI.Length / lastInputDecim);
                        bool rateChanged = Math.Abs(rate - LastSamplingRate) > 0;
                        lastCursorWinEnabled = DemodOptions.CursorPositionWindowEnabled;

                        if (blockSizeChanged || DemodOptions.ReinitSound || rateChanged)
                        {
                            rate = LastSamplingRate;

                            DemodOptions.ReinitSound = false;
                            DemodOptions.SoundDevice.SetRate((int)DemodOptions.AudioRate);

                            lastAudioDecim = DemodOptions.AudioDecimation;
                            lastInputDecim = DemodOptions.InputSignalDecimation;

                            AudioSampleBuffer = new double[inputI.Length / lastInputDecim];
                            AudioSampleBufferDecim = new double[inputI.Length / lastAudioDecim / lastInputDecim];

                            DecimatedInputI = new double[inputI.Length / lastInputDecim];
                            DecimatedInputQ = new double[inputQ.Length / lastInputDecim];
                        }
                    }

                    if (!ProcessPaused)
                    {
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

                                /* in this block are some samples that can be demodulated. used for squelch */
                                bool haveSamplesToDemodulate = true;

                                /* squelch */
                                if (DemodOptions.SquelchEnabled)
                                {
                                    double totalStrength = 0;
                                    double maxStrength = 0;
                                    double limit = DBTools.SquaredSampleFromdB(DemodOptions.SquelchLowerLimit);

                                    /* default: nothing to demodulate */
                                    haveSamplesToDemodulate = false;

                                    for (int pos = 0; pos < inputI.Length; pos++)
                                    {
                                        double strength = inputI[pos] * inputI[pos] + inputQ[pos] * inputQ[pos];

                                        totalStrength += strength;
                                        maxStrength = Math.Max(maxStrength, strength);

                                        if (strength < limit)
                                        {
                                            /* below limit, close squelch? */
                                            if (DemodOptions.SquelchState == Demodulation.eSquelchState.Open)
                                            {
                                                DemodOptions.SquelchSampleCounter++;
                                                if (DemodOptions.SquelchSampleCounter > DemodOptions.SquelchSampleCount)
                                                {
                                                    DemodOptions.SquelchSampleCounter = 0;
                                                    DemodOptions.SquelchState = Demodulation.eSquelchState.Closed;
                                                }
                                            }
                                            else
                                            {
                                                DemodOptions.SquelchSampleCounter = 0;
                                            }
                                        }
                                        else
                                        {
                                            /* over limit, open squelch? */
                                            if (DemodOptions.SquelchState == Demodulation.eSquelchState.Closed)
                                            {
                                                DemodOptions.SquelchSampleCounter++;
                                                if (DemodOptions.SquelchSampleCounter > DemodOptions.SquelchSampleCount)
                                                {
                                                    DemodOptions.SquelchSampleCounter = 0;
                                                    DemodOptions.SquelchState = Demodulation.eSquelchState.Open;
                                                }
                                            }
                                            else
                                            {
                                                DemodOptions.SquelchSampleCounter = 0;
                                            }
                                        }

                                        if (DemodOptions.SquelchState == Demodulation.eSquelchState.Closed)
                                        {
                                            inputI[pos] = 0;
                                            inputQ[pos] = 0;
                                        }
                                        else
                                        {
                                            /* demodulate this block since there are some usable samples */
                                            haveSamplesToDemodulate = true;
                                        }
                                    }

                                    DemodOptions.SquelchAverage = DBTools.SquaredSampleTodB(totalStrength / inputI.Length);
                                    DemodOptions.SquelchMax = DBTools.SquaredSampleTodB(maxStrength);
                                    if (DemodDialog != null)
                                    {
                                        DemodDialog.UpdatePowerBar();
                                    }
                                }

                                /* demodulate signal */
                                if (haveSamplesToDemodulate)
                                {
                                    PerformanceCounters.CounterDemod.Start();
                                    DemodOptions.Demod.ProcessData(inputI, inputQ, AudioSampleBuffer);
                                    PerformanceCounters.CounterDemod.Stop();

                                    if (DemodOptions.AudioLowPassEnabled)
                                    {
                                        PerformanceCounters.CounterDemodLowpass.Start();
                                        DemodOptions.AudioLowPass.Process(AudioSampleBuffer, AudioSampleBuffer);
                                        PerformanceCounters.CounterDemodLowpass.Stop();
                                    }
                                }
                                else
                                {
                                    Array.Clear(AudioSampleBuffer, 0, AudioSampleBuffer.Length);
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
                        PerformanceCounters.CounterProcessing.Stop();

                        if (DemodOptions.DisplayDemodulationSignal)
                        {
                            PerformanceCounters.CounterVisualization.Start();
                            FFTDisplay.ProcessData(inputI, inputQ, 0, Device.Amplification - Device.Attenuation);
                            PerformanceCounters.CounterVisualization.Stop();
                        }
                    }
                    else
                    {
                        PerformanceCounters.CounterProcessing.Stop();
                    }
                }
            }

            if (DemodDialog != null)
                DemodDialog.UpdateInformation();

            PerformanceCounters.CounterRuntime.Stop();
        }

        void FFTReadFunc()
        {
            double lastRate = 0;

            double[] inputI;
            double[] inputQ;

            int dbUpdateDivisor = 10;
            int dbUpdateCounter = 0;
            int spectPart = 0;
            //FFTDisplay.SpectParts = ScanFrequencies.Count;

            //Log.AddMessage("FFTReadFunc: Started");

            while (ReadThreadRun)
            {
                lock (FFTSizeSpinLock)
                {
                    double rate = Device.SampleSource.InputSamplingRate;
                    bool rateChanged = Math.Abs(lastRate - rate) > 0;

                    lock (Device.SampleSource)
                    {
                        if (!ProcessPaused)
                        {
                            Device.ReadBlock();

                            if (Device.TransferMode == eTransferMode.Block)
                            {
                                long avail = ((ShmemSampleSource)Device.SampleSource).ShmemChannel.Length;

                                if (avail > 0)
                                {
                                    Log.AddMessage("FFTReadFunc: avail " + avail +
                                                   " Bytes. Thats okay when FFT size was changed.");
                                }
                                Device.SampleSource.Flush();
                            }

                            inputI = Device.SampleSource.SourceSamplesI;
                            inputQ = Device.SampleSource.SourceSamplesQ;

                            if (dbUpdateCounter++ > dbUpdateDivisor)
                            {
                                double maxDb = DBTools.MaximumDb(inputI, inputQ);

                                if (CurrentMaxSignalDb == 0 || maxDb == 0 || CurrentMaxSignalDb / maxDb > 1.5f || maxDb / CurrentMaxSignalDb > 1.5f)
                                {
                                    CurrentMaxSignalDb = maxDb;
                                }
                                else
                                {
                                    CurrentMaxSignalDb = (9 * CurrentMaxSignalDb + maxDb) / 10;
                                }

                                /* allow a maximum of -10dB input signal power */
                                double desired = -CurrentMaxSignalDb + DesiredInputSignalPower + Device.Amplification;

                                Device.Amplification = desired;

                                dbUpdateCounter = 0;
                            }

                            //Log.AddMessage("FFTReadFunc: Read " + inputI.Length + " Samples");

                            if (rateChanged)
                            {
                                lock (DemodOptions)
                                {
                                    LastSamplingRate = rate;

                                    FFTDisplay.SamplingRate = rate;

                                    DemodOptions.InputRate = rate;
                                    DemodOptions.AudioRate = rate / DemodOptions.AudioDecimation / DemodOptions.InputSignalDecimation;
                                    DemodOptions.ReinitSound = true;

                                    if (DemodDialog != null && DemodDialog.Visible)
                                        DemodDialog.UpdateInformation();
                                }

                                try
                                {
                                    this.BeginInvoke(new setRateDelegate(setRateTextbox), rate);
                                }
                                catch (Exception)
                                {
                                }

                                lastRate = rate;
                            }

                            if (ScanFrequenciesEnabled)
                            {
                                PerformanceCounters.CounterVisualization.Start();
                                if (ScanStrongestFrequency)
                                {
                                    FFTDisplay.ProcessData(inputI, inputQ, 0, Device.Amplification - Device.Attenuation);
                                }
                                else
                                {
                                    FFTDisplay.ProcessData(inputI, inputQ, spectPart, Device.Amplification - Device.Attenuation);
                                }

                                PerformanceCounters.CounterVisualization.Stop();

                                if (!ScanStrongestFrequency || DemodOptions.SquelchState == Demodulation.eSquelchState.Closed)
                                {
                                    if (CurrentScanFreq.Next != null)
                                    {
                                        spectPart++;
                                        CurrentScanFreq = CurrentScanFreq.Next;
                                    }
                                    else
                                    {
                                        spectPart = 0;
                                        CurrentScanFreq = ScanFrequencies.First;
                                    }

                                    CurrentFrequency = CurrentScanFreq.Value.Frequency;
                                }
                            }
                            else
                            {
                                if (!DemodOptions.DisplayDemodulationSignal)
                                {
                                    PerformanceCounters.CounterVisualization.Start();
                                    FFTDisplay.ProcessData(inputI, inputQ, 0, Device.Amplification - Device.Attenuation);
                                    PerformanceCounters.CounterVisualization.Stop();
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
            }
        }


        private void OpenBO35Device()
        {
            USBRXDeviceControl dev = new USBRXDeviceControl();

            if (!dev.OpenTuner())
            {
                MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                return;
            }

            Device = dev;

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.TransferModeChanged += new EventHandler(Device_TransferModeChanged);
            dev.InvertedSpectrumChanged += new EventHandler(Device_InvertedSpectrumChanged);
            dev.DeviceDisappeared += new EventHandler(Device_DeviceDisappeared);
            dev.SamplesPerBlock = Math.Max(1, SamplesToAverage) * FFTSize;

            StartThreads();

            DeviceOpened = true;

            CurrentFrequency = dev.GetFrequency();
        }


        public void OpenSharedMem(int srcChan)
        {
            SharedMemDeviceControl dev = new SharedMemDeviceControl(srcChan);

            if (!dev.Connected)
            {
                MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                return;
            }

            Device = dev;

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.SamplesPerBlock = Math.Max(1, SamplesToAverage) * FFTSize;

            StartThreads();

            DeviceOpened = true;
        }

        private MenuItem OpenSharedMemoryDeviceCreateMenuItem(string name, int srcChan)
        {
            MenuItem item;

            if (srcChan < 0)
            {
                item = new MenuItem("No data from <" + name + ">");
                item.Enabled = false;
            }
            else
            {
                item = new MenuItem("Channel " + srcChan + " from <" + name + ">",
                new EventHandler(delegate(object sender, EventArgs e)
                {
                    OpenSharedMem(srcChan);
                }));
            }

            return item;
        }

        private void OpenSharedMemoryDevice()
        {
            ContextMenu menu = new ContextMenu();
            NodeInfo[] infos = SharedMem.GetNodeInfos();

            foreach (NodeInfo info in infos)
            {
                MenuItem item = OpenSharedMemoryDeviceCreateMenuItem(info.name, info.dstChan);
                menu.MenuItems.Add(item);
            }

            if (infos.Length == 0)
            {
                MenuItem item = new MenuItem("(No nodes found)");
                item.Enabled = false;
                menu.MenuItems.Add(item);
            }

            mainMenu.ContextMenu = menu;
            mainMenu.ContextMenu.Show(mainMenu, new Point(10, 10));
        }

        private void OpenRandomDevice()
        {
            RandomDataDeviceControl dev = new RandomDataDeviceControl();

            if (!dev.Connected)
            {
                MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                return;
            }

            Device = dev;

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);

            StartThreads();

            DeviceOpened = true;
        }

        private void StartThreads()
        {
            /* create an extra shmem channel for audio decoding */
            AudioShmem = new ShmemSampleSource("RX-FFT Audio Decoder", Device.ShmemChannel, 1, 0);
            AudioShmem.InvertedSpectrum = Device.SampleSource.InvertedSpectrum;

            lock (DemodOptions)
            {
                DemodOptions.SoundDevice = new DXSoundDevice(Handle);
            }

            ReadThreadRun = true;
            ReadThread = new Thread(FFTReadFunc);
            ReadThread.Name = "MainScreen Data Read Thread";
            ReadThread.Start();

            AudioThreadRun = true;
            AudioThread = new Thread(AudioReadFunc);
            AudioThread.Name = "Audio Decoder Thread";
            AudioThread.Start();
        }

        private void StopThreads()
        {
            /* pause transfers and finish threads */
            ProcessPaused = true;
            ReadThreadRun = false;
            AudioThreadRun = false;

            lock (DemodOptions)
            {
                if (DemodOptions.SoundDevice != null)
                {
                    DemodOptions.SoundDevice.Close();
                    DemodOptions.SoundDevice = null;
                }
            }

            if (DemodDialog != null)
            {
                DemodDialog.Close();
                DemodDialog = null;
            }

            if (ReadThread != null)
            {
                ReadThread.Abort();
                ReadThread = null;
            }
            if (AudioThread != null)
            {
                AudioThread.Abort();
                AudioThread = null;
            }

            if (AudioShmem != null)
            {
                AudioShmem.Close();
                AudioShmem = null;
            }

            /* un-pause again */
            ProcessPaused = false;
            if (!Disposing)
            {
                pauseMenu.Checked = ProcessPaused;
            }
        }

        private void CloseDevice()
        {
            if (DeviceOpened)
            {
                Device.Close();
                Device = null;
                DeviceOpened = false;
            }
        }

        void Device_DeviceDisappeared(object sender, EventArgs e)
        {
            Tuner dev = (Tuner)sender;
            StopThreads();

            BeginInvoke(new MethodInvoker(() =>
            {
                dev.CloseTuner();
                if (MessageBox.Show("The device '" + dev.Name[0] + "' seems to have disappeared. Reconnect?", "Device failure", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (dev.OpenTuner())
                    {
                        StartThreads();
                        DeviceOpened = true;
                    }
                }
            }));

        }

        void Device_InvertedSpectrumChanged(object sender, EventArgs e)
        {
        }

        void Device_TransferModeChanged(object sender, EventArgs e)
        {
            statusLabel.Text = statusLabel.Text.Replace(" (" + eTransferMode.Stopped.ToString() + ")", "");
            statusLabel.Text = statusLabel.Text.Replace(" (" + eTransferMode.Block.ToString() + ")", "");
            statusLabel.Text = statusLabel.Text.Replace(" (" + eTransferMode.Stream.ToString() + ")", "");

            if (Device != null)
            {
                statusLabel.Text += " (" + Device.TransferMode.ToString() + ")";
            }
        }

        void Device_FilterWidthChanged(object sender, EventArgs e)
        {
            FFTDisplay.FitSpectrumWidth = ((double)Device.FilterWidth / (double)Device.SamplingRate);

            FFTDisplay.LimiterUpperLimit = Device.UpperFilterMargin;
            FFTDisplay.LimiterLowerLimit = Device.LowerFilterMargin;
            FFTDisplay.LimiterUpperDescription = Device.UpperFilterMarginDescription;
            FFTDisplay.LimiterLowerDescription = Device.LowerFilterMarginDescription;
        }

        void Device_RateChanged(object sender, EventArgs e)
        {
            FFTDisplay.FitSpectrumWidth = ((double)Device.FilterWidth / (double)Device.SamplingRate);
            FFTDisplay.SamplingRate = Device.SamplingRate;

            FFTDisplay.LimiterUpperLimit = Device.UpperFilterMargin;
            FFTDisplay.LimiterLowerLimit = Device.LowerFilterMargin;
            FFTDisplay.LimiterUpperDescription = Device.UpperFilterMarginDescription;
            FFTDisplay.LimiterLowerDescription = Device.LowerFilterMarginDescription;
        }

        void Device_FrequencyChanged(object sender, EventArgs e)
        {
            if (!ScanFrequenciesEnabled)
            {
                _currentFrequency = Device.GetFrequency();

                FFTDisplay.CenterFrequency = CurrentFrequency;
                FFTDisplay.LimiterUpperLimit = Device.UpperFilterMargin;
                FFTDisplay.LimiterLowerLimit = Device.LowerFilterMargin;
                FFTDisplay.LimiterUpperDescription = Device.UpperFilterMarginDescription;
                FFTDisplay.LimiterLowerDescription = Device.LowerFilterMarginDescription;
            }
        }

        private void openBO35Menu_Click(object sender, EventArgs e)
        {
            MT2131.DeviceTypeDisabled = false;
            OpenBO35Device();
        }

        private void openBO35PlainMenu_Click(object sender, EventArgs e)
        {
            MT2131.DeviceTypeDisabled = true;
            OpenBO35Device();
        }

        private void openShMemMenu_Click(object sender, EventArgs e)
        {
            OpenSharedMemoryDevice();
        }

        private void openRandomDataMenu_Click(object sender, EventArgs e)
        {
            OpenRandomDevice();
        }

        private void markersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MarkerDialog == null || MarkerDialog.IsDisposed)
            {
                MarkerDialog = new MarkerListDialog(Markers);
                MarkerDialog.GetTuner = MarkerDialog_GetTuner;
                MarkerDialog.MarkersChanged += new EventHandler(MarkerDialog_MarkersChanged);
                MarkerDialog.Show();
            }
            else
            {
                MarkerDialog.Visible = !MarkerDialog.Visible;
            }
        }

        private DigitalTuner MarkerDialog_GetTuner()
        {
            return Device;
        }

        private void MarkerDialog_MarkersChanged(object sender, EventArgs e)
        {
            /* the markers were changed. update this in FFT */
            FFTDisplay.Markers = Markers;
        }

        private void EnableSaving(string fileName)
        {
            Device.SampleSource.SavingFileName = fileName;
            Device.SampleSource.SavingEnabled = true;
            saveMenu.Text = "Stop saving";
        }
    }
}
