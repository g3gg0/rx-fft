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
using LibRXFFT.Components.DirectX.Drawables;
using LibRXFFT.Components.DirectX.Drawables.Docks;
using LibRXFFT.Components.Generic;
using System.IO;
using LuaInterface;
using LibRXFFT.Libraries.Demodulators;

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
        bool ProcessPaused;
        bool ReadThreadRun;
        bool AudioThreadRun;
        bool Exiting = false;

        RemoteControl Remote;

        private LinkedListNode<FrequencyMarker> CurrentScanFreq;

        /* update signal strength every n miliseconds */
        private const int StrengthUpdateTime = 125;

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

        public Thread ReadThread;
        public Thread AudioThread;
        public DeviceControl Device;
        public double LastSamplingRate = 48000;
        public long MouseDragStartFreq;

        private FrequencyMarkerList MarkerList = new FrequencyMarkerList();
        private LinkedList<FrequencyMarker> ScanFrequencies = new LinkedList<FrequencyMarker>();

        ShmemSampleSource AudioShmem;
        SharedMem AudioOutShmem;

        double[] DecimatedInputI = new double[0];
        double[] DecimatedInputQ = new double[0];
        double[] AudioSampleBuffer = new double[0];
        double[] AudioSampleBufferDecim = new double[0];
        Object FFTSizeSpinLock = new Object();

        public Demodulation DemodOptions = new Demodulation();
        public DemodulationDialog DemodDialog;
        public MarkerListDialog MarkerDialog;
        public PerformaceStatsDialog StatsDialog;
        public PerformanceEnvelope PerformanceCounters = new PerformanceEnvelope();

        public RXOscilloscope OscilloscopeWindow = new RXOscilloscope();
        public DemodulatorDialog DemodulatorWindow = new DemodulatorDialog();
        public GSMAnalyzer GsmAnalyzerWindow = new GSMAnalyzer();

        public StaticText SignalOverLoad;
        public DockPanel RightDockPanel;
        public DockPanel LeftDockPanel;
        public DockPanel BottomDockPanel;
        public DockPanel TopDockPanel;
        public PowerBarDock SignalPowerBar;
        public StaticTextDock StatusTextDock;
        public FFTAreaSelection AreaSelection;

        bool _DeviceOpened = false;
        private bool ScanStrongestFrequency;
        private long _currentFrequency;
        private double CurrentMaxSignalDb = double.NegativeInfinity;

        /* try to set amplification to get a input power of -10dB */
        private double DesiredInputSignalPower = -10.0;

        private Timer StatusUpdateTimer;
        private bool AGCEnabled;
        private Lua LuaVm;

        public MainScreen()
        {
            InitializeComponent();
            
            Log.Init();
            Log.Enabled = false;

            this.Icon = Icon.FromHandle(Icons.imgRhythmbox.GetHicon());

            this.fftSizeOtherMenu.KeyPress += new KeyPressEventHandler(this.fftSizeOtherMenu_KeyPress);
            this.updateRateText.KeyPress += new KeyPressEventHandler(this.updateRateText_KeyPress);
            this.averageSamplesText.KeyPress += new KeyPressEventHandler(this.averageSamplesText_KeyPress);
            this.verticalSmoothMenuText.KeyPress += new KeyPressEventHandler(this.verticalSmoothMenuText_KeyPress);


            DragEnter += new DragEventHandler(MainScreen_DragEnter);
            DragDrop += new DragEventHandler(MainScreen_DragDrop);

            updateRateText_KeyPress(null, null);
            averageSamplesText_KeyPress(null, null);
            verticalSmoothMenuText_KeyPress(null, null);

            MarkerList.MarkersChanged += new EventHandler(MarkerDialog_MarkersChanged);
            FFTDisplay.Markers = MarkerList.Markers;

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

            SignalOverLoad = new StaticText(FFTDisplay.FFTDisplay, "OVERDRIVE");
            SignalOverLoad.XPosition = -0.04;
            SignalOverLoad.YPosition = -0.06;
            SignalOverLoad.Pulsing = true;
            SignalOverLoad.Active = false;

            RightDockPanel = new DockPanel(FFTDisplay.FFTDisplay, eOrientation.RightBorder);
            LeftDockPanel = new DockPanel(FFTDisplay.FFTDisplay, eOrientation.LeftBorder);
            TopDockPanel = new DockPanel(FFTDisplay.FFTDisplay, eOrientation.TopBorder);
            BottomDockPanel = new DockPanel(FFTDisplay.FFTDisplay, eOrientation.BottomBorder);
            SignalPowerBar = new PowerBarDock(RightDockPanel);

            AreaSelection = new FFTAreaSelection(FFTDisplay.FFTDisplay);
            FFTAreaSelectionDetails d = new FFTAreaSelectionDetails(AreaSelection, RightDockPanel);

            AreaSelection.Draggable = true;
            AreaSelection.SelectionUpdated += new EventHandler(FFTAreaSelection_SelectionUpdated);

            DemodOptions.CursorWindowFilterChanged += new EventHandler(DemodOptions_CursorWindowFilterChanged);
            DemodOptions.DataUpdated += new EventHandler(DemodOptions_CursorWindowFilterChanged);

            RightDockPanel.FadeOutByDistance = true;
            RightDockPanel.FadeOutDistance = 90;

            StatusTextDock = new StaticTextDock(LeftDockPanel);
            StatusTextDock.Title = "Status";
            StatusTextDock.Text = "";
            StatusTextDock.XOffset = 25;
            StatusTextDock.YOffset = 25;

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

            Remote = new RemoteControl();
            Remote.FFTSizeChanged += new EventHandler(Remote_FFTSizeChanged);
            FFTDisplay.Remote = Remote;

            dynamicWaterfallMenu.Checked = true;
            displayFilterMarginsMenu.Checked = true;

            FFTDisplay.DynamicLimits = true;
            DisplayFilterMargins = true;

            InitLua();
        }

        void AreaSelectionUpdate()
        {
            FFTAreaSelection sel = AreaSelection;

            string text = "";

            if (!DeviceOpened || !DemodOptions.DemodulationEnabled || !DemodOptions.CursorPositionWindowEnabled)
            {
                sel.Text = text;
                return;
            }

            if (DemodOptions.Demod is AMDemodulator)
            {
                text = "AM:  ";
            }
            else if (DemodOptions.Demod is FMDemodulator)
            {
                text = "FM:  ";
            }
            else if (DemodOptions.Demod is SSBDemodulator)
            {
                switch (((SSBDemodulator)DemodOptions.Demod).Type)
                {
                    case eSsbType.Usb:
                        text = "USB: ";
                        break;
                    case eSsbType.Lsb:
                        text = "LSB: ";
                        break;
                }
            }

            double filterWidth = Device.SamplingRate / DemodOptions.CursorWindowFilterWidthFract;

            sel.Text = text + FrequencyFormatter.FreqToStringAccurate(filterWidth);

            if (sel.Visible)
            {
                FFTDisplay.FFTDisplay.HorLineFixed = false;
            }
            else
            {
                FFTDisplay.FFTDisplay.HorLineFixed = true;
                FFTDisplay.FFTDisplay.HorLineWidth = filterWidth;
            }
        }

        void DemodOptions_CursorWindowFilterChanged(object sender, EventArgs e)
        {
            double filterWidth = Device.SamplingRate / DemodOptions.CursorWindowFilterWidthFract;

            if (DemodOptions == null)
            {
                return;
            }

            if (DemodOptions.Demod is AMDemodulator || DemodOptions.Demod is FMDemodulator)
            {
                double centerFreq = (AreaSelection.StartFreq + AreaSelection.EndFreq) / 2;

                AreaSelection.StartFreq = (long)(centerFreq - filterWidth / 2);
                AreaSelection.EndFreq = (long)(centerFreq + filterWidth / 2);
            }
            else if (DemodOptions.Demod is SSBDemodulator)
            {
                filterWidth /= 2;
                switch (((SSBDemodulator)DemodOptions.Demod).Type)
                {
                    case eSsbType.Lsb:
                        AreaSelection.StartFreq = (long)(AreaSelection.EndFreq - filterWidth);
                        break;
                    case eSsbType.Usb:
                        AreaSelection.EndFreq = (long)(AreaSelection.StartFreq + filterWidth);
                        break;
                }
            }

            AreaSelectionUpdate();
            AreaSelection.UpdatePositions();
        }

        void FFTAreaSelection_SelectionUpdated(object sender, EventArgs e)
        {
            FFTAreaSelection sel = AreaSelection;

            /* in any case update its text */
            AreaSelectionUpdate();

            /* not being used, so dont do anything */
            if (!DeviceOpened || !DemodOptions.DemodulationEnabled || !DemodOptions.CursorPositionWindowEnabled)
            {
                return;
            }

            double baseFreq = 0;

            if (DemodOptions.Demod is AMDemodulator || DemodOptions.Demod is FMDemodulator)
            {
                /* want center as baseband */
                baseFreq = (sel.StartFreq + sel.EndFreq) / 2;
            }
            else if (DemodOptions.Demod is SSBDemodulator)
            {
                /* depending on USB or LSB choose the baseband freq */
                switch (((SSBDemodulator)DemodOptions.Demod).Type)
                {
                    case eSsbType.Usb:
                        baseFreq = AreaSelection.StartFreq;
                        break;
                    case eSsbType.Lsb:
                        baseFreq = AreaSelection.EndFreq;
                        break;
                }
            }

            /* calculate downmix frequency */
            double filterWidth = Device.SamplingRate / DemodOptions.CursorWindowFilterWidthFract;
            double xPos = FFTDisplay.FFTDisplay.XPosFromFrequency(baseFreq);
            double relative = FFTDisplay.FFTDisplay.XRelativeCoordFromCursorPos(xPos);

            /* only if within the visible window */
            if (relative >= -0.5f && relative <= 0.5f)
            {
                DemodOptions.DemodulationDownmixer.TimeStep = relative * (2 * Math.PI);
            }
        }

        private void InitLua()
        {
            LuaVm = new Lua();

            LuaHelpers.RegisterNamespace("DemodulatorCollection.Demodulators");
            LuaHelpers.RegisterNamespace("DemodulatorCollection.BitClockSinks");

            LuaHelpers.RegisterLuaFunctions(LuaVm, new LuaHelpers());
            LuaHelpers.RegisterLuaFunctions(LuaVm, this);

            try
            {
                LuaVm.DoFile("startup.lua");
                try
                {
                    LuaHelpers.CallFunction(LuaVm, "Init", this);
                }
                catch (Exception e)
                {
                    Log.AddMessage(e.ToString());
                }
            }
            catch (Exception e)
            {
            }
        }

        private void DeinitLua()
        {
            try
            {
                LuaHelpers.CallFunction(LuaVm, "Deinit", this);
            }
            catch (Exception e)
            {
            }
        }


        void MainScreen_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 1)
            {
                if (DeviceOpened)
                {
                    if (Device is FileSourceDeviceControl)
                    {
                        ((FileSourceDeviceControl)Device).LoadFile(files[0]);
                    }
                    else
                    {
                        StopThreads();
                        CloseDevice();
                        OpenFileDevice(files[0]);
                    }
                }
                else
                {
                    OpenFileDevice(files[0]);
                }
            }
        }

        void MainScreen_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        void Remote_FFTSizeChanged(object sender, EventArgs e)
        {
            //FFTSize = Remote.FFTSize;
        }

        private void StatusUpdateTimerFunc(object state)
        {
            if (CurrentMaxSignalDb > -5)
            {
                SignalOverLoad.Active = true;
            }
            else
            {
                SignalOverLoad.Active = false;
            }

            if (!IsDisposed)
            {
                try
                {
                    BeginInvoke(
                        new MethodInvoker(
                            () =>
                            {
                                if (!double.IsNegativeInfinity(CurrentMaxSignalDb))
                                {
                                    maxDbLabel.Text = CurrentMaxSignalDb.ToString("#.# dB");
                                }
                                else
                                {
                                    maxDbLabel.Text = "--- dB";
                                }

                                fpsLabel.Text =
                                    string.Format("{0:0.##}", FFTDisplay.FpsProcessed) + " of " +
                                    string.Format("{0:0.##}", FFTDisplay.FpsReceived);
                            }));
                }
                catch (Exception e)
                {
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Exiting = true;
            DeinitLua();
            Remote.Abort();
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
                {
                    Device.SamplesPerBlock = Math.Min(32, Math.Max(1, SamplesToAverage)) * value;
                }

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
                {
                    Device.BlocksPerSecond = value;
                }
                FFTDisplay.UpdateRate = value;
            }
        }

        int SamplesToAverage
        {
            get { return (int)FFTDisplay.SamplesToAverage; }
            set
            {
                if (Device != null)
                {
                    Device.SamplesPerBlock = Math.Min(32, Math.Max(1, value)) * FFTSize;
                }
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

        void setRateTextbox(double rate)
        {
            samplingRateLabel.Text = FrequencyFormatter.FreqToStringAccurate(rate);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            WindowActivated = true;
            FFTDisplay.FocusHovered();
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
                        if (!AreaSelection.Visible)
                        {
                            double relative = FFTDisplay.RelativeCursorXPos;

                            if (relative >= -0.5f && relative <= 0.5f)
                            {
                                DemodOptions.DemodulationDownmixer.TimeStep = relative * (2 * Math.PI);
                            }
                        }
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
                        if (DeviceOpened)
                        {
                            long mouseFreq = FFTDisplay.Frequency;

                            long currentFreq = Device.GetFrequency();
                            long newFreq = currentFreq - (mouseFreq - MouseDragStartFreq);

                            //FFTDisplay.CenterFrequency = newFreq;
                            CurrentFrequency = newFreq;
                        }
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
                        MenuItem menuItem1 = new MenuItem("Frequency: " + FrequencyFormatter.FreqToStringAccurate(freq));
                        MenuItem menuItem2 = new MenuItem("-");
                        MenuItem menuItem3 = new MenuItem("Send to Locator");
                        MenuItem menuItem4 = new MenuItem("Add Marker...");
                        MenuItem menuItem5 = new MenuItem("-");
                        MenuItem menuItem6 = new MenuItem("Gradient");
                        MenuItem menuItem7 = new MenuItem("TriColor");
                        MenuItem menuItem8 = new MenuItem("Pseudocolors");
                        menuItem1.Enabled = false;

                        contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem1, menuItem2, menuItem3, menuItem4, menuItem5, menuItem6, menuItem7, menuItem8 });

                        menuItem4.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            AddMarker(freq);
                        });

                        menuItem6.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            ColorDialog dlg = new ColorDialog();
                            dlg.ShowDialog();

                            FFTDisplay.WaterfallDisplay.ColorTable = new ColorLookupTable(dlg.Color);
                        });

                        menuItem7.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            FFTDisplay.WaterfallDisplay.ColorTable = new MultiColorMap(8192, Color.Black, Color.FromArgb(0, 0, 128), Color.FromArgb(192, 0, 255), Color.White);
                        });

                        menuItem8.Click += new EventHandler(delegate(object sender, EventArgs e)
                        {
                            FFTDisplay.WaterfallDisplay.ColorTable = new HeatColors(8192);
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

            MarkerList.Add(marker);
            FFTDisplay.Markers = MarkerList.Markers;
        }

        void AudioReadFunc()
        {
            try
            {
                double rate = 0;
                int lastAudioDecim = 1;
                int lastInputDecim = 1;
                bool lastCursorWinEnabled = false;

                double[] inputI;
                double[] inputQ;
                byte[] AudioOutBinary = null;

                AudioShmem.SamplesPerBlock = 512;


                PerformanceCounters.Reset();

                PerformanceCounters.CounterRuntime.Start();

                while (AudioThreadRun)
                {
                    //dev.Read(inBuffer);
                    PerformanceCounters.CounterRuntime.Update();

                    if (DeviceOpened && Device.TransferMode == eTransferMode.Stream)
                    {
                        lock (AudioShmem)
                        {
                            PerformanceCounters.CounterReading.Start();
                            AudioShmem.Read();
                            PerformanceCounters.CounterReading.Stop();

                            PerformanceCounters.CounterProcessing.Start();
                            lock (AudioShmem.SampleBufferLock)
                            {
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
                                        DemodOptions.SoundDevice.SetInputRate((int)DemodOptions.AudioRate);

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

                                                DemodOptions.UpdateListeners();
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
                                                {
                                                    ampl = DemodOptions.AudioAmplification;
                                                }

                                                for (int pos = 0; pos < AudioSampleBufferDecim.Length; pos++)
                                                {
                                                    AudioSampleBufferDecim[pos] = ampl * AudioSampleBuffer[pos * lastAudioDecim];
                                                }
                                                PerformanceCounters.CounterDemodDecimate.Stop();

                                                DemodOptions.SoundDevice.Write(AudioSampleBufferDecim);

                                                /* shmem output of demodulated signal */
                                                if (AudioOutBinary == null || AudioOutBinary.Length != AudioSampleBufferDecim.Length * 4)
                                                {
                                                    AudioOutBinary = new byte[AudioSampleBufferDecim.Length * 4];
                                                }
                                                ByteUtil.SamplesToBinary(AudioOutBinary, AudioSampleBufferDecim, AudioSampleBufferDecim, ByteUtil.eSampleFormat.Direct16BitIQFixedPoint, false);
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

                                                /* shmem output of demodulated signal */
                                                if (AudioOutBinary == null || AudioOutBinary.Length != AudioSampleBuffer.Length * 4)
                                                {
                                                    AudioOutBinary = new byte[AudioSampleBuffer.Length * 4];
                                                }
                                                ByteUtil.SamplesToBinary(AudioOutBinary, AudioSampleBuffer, AudioSampleBuffer, ByteUtil.eSampleFormat.Direct16BitIQFixedPoint, false);
                                            }

                                            AudioOutShmem.Rate = (long)(rate / lastAudioDecim);
                                            AudioOutShmem.Write(AudioOutBinary);
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
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                /*
                if (DemodDialog != null)
                    DemodDialog.UpdateInformation();
                */
                PerformanceCounters.CounterRuntime.Stop();
            }
            catch (ThreadAbortException e)
            {
            }
            catch (Exception e)
            {
                Log.AddMessage("Exception in Audio Thread: " + e.ToString());
            }
        }


        void FFTReadFunc()
        {
            try
            {
                double lastRate = 0;

                double[] inputI;
                double[] inputQ;

                DateTime lastStrengthUpdate = DateTime.Now;

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
                                        // Log.AddMessage("FFTReadFunc: avail " + avail + " Bytes. Thats okay when FFT size was changed.");
                                    }
                                    //Device.SampleSource.Flush();
                                }

                                lock (Device.SampleSource.SampleBufferLock)
                                {
                                    inputI = Device.SampleSource.SourceSamplesI;
                                    inputQ = Device.SampleSource.SourceSamplesQ;

                                    /* update strength display every some ms */
                                    if (DateTime.Now.Subtract(lastStrengthUpdate).TotalMilliseconds > StrengthUpdateTime)
                                    {
                                        lastStrengthUpdate = DateTime.Now;

                                        double maxDb = DBTools.MaximumDb(inputI, inputQ);
                                        double fact = CurrentMaxSignalDb / maxDb;
                                        double maxFact = 1.5f;
                                        double smoothFact = 5;

                                        /* check if 
                                         *  a) had no signal before
                                         *  b) have no signal now
                                         *  c) old and new strength differ by factor 'maxFact'
                                         */
                                        if (CurrentMaxSignalDb == 0 || maxDb == 0 || fact > maxFact || fact < (1.0f / maxFact))
                                        {
                                            /* if so, use current strength */
                                            CurrentMaxSignalDb = maxDb;
                                        }
                                        else
                                        {
                                            /* if not, smooth value */
                                            CurrentMaxSignalDb = ((smoothFact - 1) * CurrentMaxSignalDb + maxDb) / smoothFact;
                                        }

                                        SignalPowerBar.Power = CurrentMaxSignalDb;

                                        /* allow a maximum of -10dB input signal power */
                                        double desired = -CurrentMaxSignalDb + DesiredInputSignalPower + Device.Amplification;

                                        if (AGCEnabled)
                                        {
                                            Device.Amplification = desired;
                                        }
                                    }

                                    //Log.AddMessage("FFTReadFunc: Read " + inputI.Length + " Samples");

                                    if (rateChanged)
                                    {
                                        lock (DemodOptions)
                                        {
                                            LastSamplingRate = rate;

                                            FFTDisplay.SamplingRate = rate;

                                            DemodOptions.InputRate = rate;
                                            DemodOptions.ReinitSound = true;

                                            if (DemodDialog != null && DemodDialog.Visible)
                                                DemodDialog.UpdateInformation();
                                        }

                                        try
                                        {
                                            BeginInvoke(new Action(() => setRateTextbox(rate)));
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
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException e)
            {
            }
            catch (Exception e)
            {
                Log.AddMessage("Exception in Transfer Thread: " + e.ToString());
            }
        }

        private void OpenBO35Device(USBRXDevice.eCombinationType type)
        {
            try
            {
                USBRXDeviceControl dev = new USBRXDeviceControl();

                dev.TunerCombination = type;

                if (!dev.OpenTuner())
                {
                    MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                    return;
                }

                Device = dev;
                Remote.Tuner = dev;

                dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
                dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
                dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
                dev.TransferModeChanged += new EventHandler(Device_TransferModeChanged);
                dev.InvertedSpectrumChanged += new EventHandler(Device_InvertedSpectrumChanged);
                dev.DeviceDisappeared += new EventHandler(Device_DeviceDisappeared);
                dev.DeviceClosed += new EventHandler(Device_DeviceClosed);
                dev.SamplesPerBlock = Math.Min(32, Math.Max(1, SamplesToAverage)) * FFTSize;

                StartThreads();
                CurrentFrequency = dev.GetFrequency();
                DeviceOpened = true;
            }
            catch (Exception e)
            {
                DeviceOpened = false;
            }
        }

        public void OpenFileDevice()
        {
            OpenFileDevice(null);
        }

        public void OpenFileDevice(string fileName)
        {
            FileSourceDeviceControl dev = new FileSourceDeviceControl(fileName);

            if (!dev.Connected)
            {
                MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                return;
            }

            Device = dev;

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.DeviceDisappeared += new EventHandler(Device_DeviceDisappeared);
            dev.DeviceClosed += new EventHandler(Device_DeviceClosed);
            dev.SamplesPerBlock = Math.Min(32, Math.Max(1, SamplesToAverage)) * FFTSize;

            StartThreads();

            DeviceOpened = true;
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
            dev.DeviceClosed += new EventHandler(Device_DeviceClosed);
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
            Remote.Tuner = dev;

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.DeviceClosed += new EventHandler(Device_DeviceClosed);

            StartThreads();

            DeviceOpened = true;
        }

        private void StartThreads()
        {
            /* create an extra shmem channel for audio decoding */
            AudioShmem = new ShmemSampleSource("RX-FFT Audio Decoder", Device.ShmemChannel, 1, 0);
            AudioShmem.InvertedSpectrum = Device.SampleSource.InvertedSpectrum;

            AudioOutShmem = new SharedMem(-2, 1, "RX-FFT Demodulated Audio");

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
                    DemodOptions.SoundDevice.Stop();
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
                if (!ReadThread.Join(100))
                {
                    ReadThread.Abort();
                }
                ReadThread = null;
            }

            if (AudioThread != null)
            {
                if (!AudioThread.Join(100))
                {
                    AudioThread.Abort();
                }
                AudioThread = null;
            }
            
            if (AudioShmem != null)
            {
                AudioShmem.Close();
                AudioShmem = null;
            }

            if (AudioOutShmem != null)
            {
                AudioOutShmem.Close();
                AudioOutShmem = null;
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
                DeviceOpened = false;
                Device.Close();
                Device = null;
            }
        }

        void Device_DeviceClosed(object sender, EventArgs e)
        {
            StopThreads();
            CloseDevice();
        }

        void Device_DeviceDisappeared(object sender, EventArgs e)
        {
            Tuner dev = (Tuner)sender;

            BeginInvoke(new MethodInvoker(() =>
            {
                DeviceControl oldDevice = Device;
                StopThreads();

                DeviceOpened = false;
                // Device = null;

                dev.CloseTuner();

                if (MessageBox.Show("The device '" + dev.Name[0] + "' seems to have disappeared. Reconnect?", "Device failure", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (dev.OpenTuner())
                    {
                        StartThreads();
                        //Device = oldDevice;
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

            /* try to find optimal decimation rate */
            if (DemodOptions.SoundDevice != null && DemodOptions.SoundDevice.Rate < Device.SamplingRate)
            {
                double exactDecim = (double)Device.SamplingRate / (double)DemodOptions.SoundDevice.Rate;

                DemodOptions.AudioDecimation = (int)Math.Ceiling(exactDecim);
            }
            else
            {
                DemodOptions.AudioDecimation = 1;
            }

            DemodOptions.ReinitSound = true;
            if (DemodDialog != null)
            {
                DemodDialog.UpdateInformation();
            }
        }

        void Device_FrequencyChanged(object sender, EventArgs e)
        {
            /* give FFT area selection the chance to update its downmix parameters */
            FFTAreaSelection_SelectionUpdated(null, null);

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
            bool animateStatus = true;
            new Thread(() =>
            {
                string dotString = "........";
                int dots = 0;
                int loops = 100;

                while (animateStatus)
                {
                    StatusTextDock.Text = "Opening Device" + dotString.Substring(0, dots);
                    Thread.Sleep(500);
                    dots++;
                    dots %= 4;
                    loops--;
                    if (loops == 0)
                    {
                        return;
                    }
                }
                StatusTextDock.Hide();
            }).Start();

            OpenBO35Device(USBRXDevice.eCombinationType.BO35);

            animateStatus = false;
            StatusTextDock.Hide();
        }

        private void openBO35PlainMenu_Click(object sender, EventArgs e)
        {
            bool animateStatus = true;
            new Thread(() =>
            {
                string dotString = "........";
                int dots = 0;
                int loops = 100;

                while (animateStatus)
                {
                    StatusTextDock.Text = "Opening Device" + dotString.Substring(0, dots);
                    Thread.Sleep(500);
                    dots++;
                    dots %= 4;
                    loops--;
                    if (loops == 0)
                    {
                        return;
                    }
                }
                StatusTextDock.Hide();
            }).Start();
            OpenBO35Device(USBRXDevice.eCombinationType.Automatic);

            StatusTextDock.Hide();
            animateStatus = false;
        }

        private void openFileMenu_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDevice();
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show("File format not supported. (" + ex.Message + ")");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the file. (" + ex.Message + ")");
            }
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
                MarkerDialog = new MarkerListDialog(MarkerList);
                MarkerDialog.GetTuner = MarkerDialog_GetTuner;
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
            FFTDisplay.Markers = MarkerList.Markers;
        }

        private void EnableSaving(string fileName)
        {
            Device.SampleSource.SavingFileName = fileName;
            Device.SampleSource.SavingEnabled = true;
            saveMenu.Text = "Stop saving";
        }
    }
}
