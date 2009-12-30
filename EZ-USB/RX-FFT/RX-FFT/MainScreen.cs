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
using LibRXFFT.Libraries.Misc;
using System.Collections.Generic;
using RX_FFT.Dialogs;
using GSM_Analyzer;
using RX_FFT.DeviceControls;
using LibRXFFT.Libraries.USB_RX.Tuners;


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

        bool WindowActivated = true;
        bool processPaused;
        bool ReadThreadRun;
        bool AudioThreadRun;
        Thread ReadThread;
        Thread AudioThread;
        DeviceControl Device;
        double LastSamplingRate = 48000;
        long MouseDragStartFreq;

        private LinkedList<FrequencyMarker> Markers = new LinkedList<FrequencyMarker>();
        /*
        private FrequencyMarker UpperFilterMarginMarker = new FrequencyMarker("Upper Limit", "Upper Frequency Limit", 0);
        private FrequencyMarker LowerFilterMarginMarker = new FrequencyMarker("Lower Limit", "Lower Frequency Limit", 0);
        */

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


        public MainScreen()
        {
            InitializeComponent();

            this.fftSizeOtherMenu.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.fftSizeOtherMenu_KeyPress);
            this.updateRateText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.updateRateText_KeyPress);
            this.averageSamplesText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.averageSamplesText_KeyPress);
            this.verticalSmoothMenuText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.verticalSmoothMenuText_KeyPress);

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

            /*
            Markers.AddLast(UpperFilterMarginMarker);
            Markers.AddLast(LowerFilterMarginMarker);
            */

            /* build the windowing function list dropdown menu */
            foreach (string windowName in Enum.GetNames(typeof(FFTTransformer.eWindowingFunction)))
            {
                ToolStripMenuItem menu = new ToolStripMenuItem();
                this.windowingFunctionMenu.DropDownItems.Add(menu);
                this.FormClosing += new FormClosingEventHandler(MainScreen_FormClosing);

                menu.Name = windowName;
                menu.Text = windowName;
                menu.Click += new System.EventHandler(delegate(object s, EventArgs e) 
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

            Log.Init();
        }

        private void MainScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            ToolStripManager.SaveSettings(this);
        }

        bool _DeviceOpened = false;
        bool DeviceOpened
        {
            get { return _DeviceOpened; }
            set 
            { 
                _DeviceOpened = value;

                closeMenu.Enabled = DeviceOpened;
                pauseMenu.Enabled = DeviceOpened;
                openMenu.Enabled = !DeviceOpened;
            }
        }

        int FFTSize
        {
            get { return FFTDisplay.FFTSize; }
            set
            {
                lock (FFTSizeSpinLock)
                {
                    if (Device != null)
                        Device.SamplesPerBlock = value * Math.Max(1, SamplesToAverage);
                    FFTDisplay.FFTSize = value;
                }
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

                        DemodOptions.DemodulationDownmixer.TimeStep = -relative * (2 * Math.PI);
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

                        FFTDisplay.CenterFrequency = newFreq;
                        Device.SetFrequency(newFreq);
                    }
                    break;

                /* jump to frequency under cursor */
                case eUserEvent.MouseDoubleClickLeft:
                    if (Device != null)
                    {
                        long freq = FFTDisplay.Frequency;
                        Device.SetFrequency(freq);
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

                        contextMenu.Show(this, this.PointToClient(MousePosition));
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

                    if (!processPaused)
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
                            FFTDisplay.ProcessData(inputI, inputQ);
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

            //Log.AddMessage("FFTReadFunc: Started");

            while (ReadThreadRun)
            {
                lock (FFTSizeSpinLock)
                {
                    double rate = Device.SampleSource.InputSamplingRate;
                    bool rateChanged = Math.Abs(lastRate - rate) > 0;

                    lock (Device.SampleSource)
                    {
                        if (!processPaused)
                        {
                            Device.SampleSource.Read();

                            inputI = Device.SampleSource.SourceSamplesI;
                            inputQ = Device.SampleSource.SourceSamplesQ;

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

                            if (!DemodOptions.DisplayDemodulationSignal)
                            {
                                PerformanceCounters.CounterVisualization.Start();
                                FFTDisplay.ProcessData(inputI, inputQ);
                                PerformanceCounters.CounterVisualization.Stop();
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
            if (dev.Connected)
            {
                Device = dev;

                /* create an extra shmem channel for audio decoding */
                AudioShmem = new ShmemSampleSource("RX-FFT Audio Decoder", dev.ShmemChannel, 1, 0);
                AudioShmem.InvertedSpectrum = dev.SampleSource.InvertedSpectrum;

                dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
                dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
                dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
                dev.SampleSource.SamplingRateChanged += new EventHandler(SampleSource_SamplingRateChangedEvent);

                dev.SetFrequency(100200000);
                dev.SamplesPerBlock = Math.Max(1, SamplesToAverage) * FFTSize;

                lock (DemodOptions)
                {
                    DemodOptions.SoundDevice = new DXSoundDevice(Handle);
                }

                ReadThreadRun = true;
                ReadThread = new Thread(new ThreadStart(FFTReadFunc));
                ReadThread.Name = "MainScreen Data Read Thread";
                ReadThread.Start();

                AudioThreadRun = true;
                AudioThread = new Thread(new ThreadStart(AudioReadFunc));
                AudioThread.Name = "Audio Decoder Thread";
                AudioThread.Start();

                DeviceOpened = true;
            }
            else
            {
                dev.Close();
                dev = null;
                MessageBox.Show("Could not find any compatible USB device.");
            }
        }

        public void OpenSharedMem(int srcChan)
        {
            SharedMemDeviceControl dev = new SharedMemDeviceControl(srcChan);
            Device = dev;

            /* create an extra shmem channel for audio decoding */
            AudioShmem = new ShmemSampleSource("RX-FFT Audio Decoder", dev.ShmemChannel, 1, 0);
            AudioShmem.InvertedSpectrum = dev.SampleSource.InvertedSpectrum;

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.SampleSource.SamplingRateChanged += new EventHandler(SampleSource_SamplingRateChangedEvent);

            dev.SamplesPerBlock = Math.Max(1, SamplesToAverage) * FFTSize;

            lock (DemodOptions)
            {
                DemodOptions.SoundDevice = new DXSoundDevice(Handle);
            }

            ReadThreadRun = true;
            ReadThread = new Thread(new ThreadStart(FFTReadFunc));
            ReadThread.Name = "MainScreen Data Read Thread";
            ReadThread.Start();

            AudioThreadRun = true;
            AudioThread = new Thread(new ThreadStart(AudioReadFunc));
            AudioThread.Name = "Audio Decoder Thread";
            AudioThread.Start();

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
            mainMenu.ContextMenu.Show(mainMenu, new System.Drawing.Point(10, 10));
        }

        private void OpenRandomDevice()
        {
            RandomDataDeviceControl dev = new RandomDataDeviceControl();
            Device = dev;

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.SampleSource.SamplingRateChanged += new EventHandler(SampleSource_SamplingRateChangedEvent);

            lock (DemodOptions)
            {
                DemodOptions.SoundDevice = new DXSoundDevice(Handle);
            }

            ReadThreadRun = true;
            ReadThread = new Thread(new ThreadStart(FFTReadFunc));
            ReadThread.Name = "MainScreen Data Read Thread";
            ReadThread.Start();

            DeviceOpened = true;
        }

        void Device_FilterWidthChanged(object sender, EventArgs e)
        {
        }

        void Device_RateChanged(object sender, EventArgs e)
        {
            double rate = Device.SamplingRate;
            FFTDisplay.SamplingRate = rate;
        }

        void Device_FrequencyChanged(object sender, EventArgs e)
        {
            double freq = Device.GetFrequency();
            /*
            LowerFilterMarginMarker.Frequency = Device.LowerFilterMargin;
            UpperFilterMarginMarker.Frequency = Device.UpperFilterMargin;
            */
            FFTDisplay.CenterFrequency = freq;
            FFTDisplay.Markers = Markers;
        }

        void SampleSource_SamplingRateChangedEvent(object sender, EventArgs e)
        {
            /*
            LowerFilterMarginMarker.Frequency = Device.LowerFilterMargin;
            UpperFilterMarginMarker.Frequency = Device.UpperFilterMargin;
            */

            FFTDisplay.Markers = Markers;
        }

        private void openBO35Menu_Click(object sender, EventArgs e)
        {
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

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }
    }
}
