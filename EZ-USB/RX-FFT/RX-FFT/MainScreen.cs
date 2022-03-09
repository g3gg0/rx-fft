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
using LibRXFFT.Components.GDI;
using LibRXFFT.Components.DeviceControls;
using LibRXFFT.Libraries.HiQ_SDR;

namespace RX_FFT
{
    public partial class MainScreen : Form
    {
        public static MainScreen Instance = null;
        public delegate DigitalTuner delegateGetTuner();

        DCOffsetCorrection corr = new DCOffsetCorrection();
        private bool CorrectDCOffset = true;
        private bool WindowActivated = true;
        private bool ProcessPaused;
        private bool ReadThreadRun;

        private RemoteControl Remote;

        private LinkedListNode<FrequencyMarker> CurrentScanFreq;

        /* update signal strength every n miliseconds */
        private const int StrengthUpdateTime = 1000;

        private long ScanStartFreq = 0;
        private long ScanEndFreq = 0;
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

                if (ScanFrequenciesEnabled || ScanStrongestFrequency)
                {
                    if (ScanEndFreq - ScanStartFreq > 0)
                    {
                        FFTDisplay.CenterFrequency = (ScanEndFreq + ScanStartFreq) / 2;
                        if (FFTDisplay.FitSpectrumEnabled)
                        {
                            FFTDisplay.SamplingRate = (ScanEndFreq - ScanStartFreq) / FFTDisplay.FitSpectrumWidth;
                        }
                        else
                        {
                            FFTDisplay.SamplingRate = ScanEndFreq - ScanStartFreq;
                        }
                        FFTDisplay.ChannelMode = false;
                    }
                    else
                    {
                        FFTDisplay.CenterFrequency = 0;
                        FFTDisplay.SamplingRate = 0;
                        FFTDisplay.ChannelMode = true;
                    }
                    FFTDisplay.SpectParts = ScanFrequencies.Count;
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
        public Thread UpdateRateThread;
        

        public DeviceControl Device;
        public double LastSamplingRate = 48000;
        public long MouseDragStartFreq;

        private LinkedList<FrequencyMarker> ScanFrequencies = new LinkedList<FrequencyMarker>();

        public ShmemSampleSource AudioShmem;

        double[] DecimatedInputI = new double[0];
        double[] DecimatedInputQ = new double[0];
        double[] AudioSampleBuffer = new double[0];
        double[] AudioSampleBufferDecim = new double[0];
        Object FFTSizeSpinLock = new Object();

        private FrequencyMarkerList MarkerList = new FrequencyMarkerList();
        public Dictionary<FrequencyMarker, AudioDemodulator> MarkerDemodulators = new Dictionary<FrequencyMarker, AudioDemodulator>();
        public LinkedList<AudioDemodulator> AudioDemodulators = new LinkedList<AudioDemodulator>();
        public DemodulationState DemodState = null;

        public MarkerListDialog MarkerDialog;
        public PerformaceStatsDialog StatsDialog;
        public AudioDemodulator.PerformanceEnvelope PerformanceCounters = new LibRXFFT.Libraries.SignalProcessing.AudioDemodulator.PerformanceEnvelope();

        public DemodulatorDialog DemodulatorWindow = new DemodulatorDialog();
        public RXOscilloscope OscilloscopeWindow = null;
        public GSMAnalyzer GsmAnalyzerWindow = null;

        public LuaShell LuaShellWindow = null;

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

        public MainScreen()
        {
            Instance = this;

            InitializeComponent();
            
            Log.Init();
            //Log.Enabled = false;

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

            AudioDemodulator demod = new AudioDemodulator(PerformanceCounters);
            AudioDemodulators.AddLast(demod);
            DemodState = demod.DemodState;
            DemodState.CursorWindowFilterChanged += new EventHandler(DemodOptions_CursorWindowFilterChanged);
            DemodState.DataUpdated += new EventHandler(DemodOptions_CursorWindowFilterChanged);

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
            menuButtons.Show();

            StatusUpdateTimer = new Timer(StatusUpdateTimerFunc, null, 500, 500);

            Remote = new RemoteControl();
            Remote.FFTSizeChanged += new EventHandler(Remote_FFTSizeChanged);
            FFTDisplay.Remote = Remote;

            dynamicWaterfallMenu.Checked = true;
            displayFilterMarginsMenu.Checked = true;
            dCOffsetCorrectionToolStripMenuItem.Checked = true;

            FFTDisplay.DynamicLimits = true;
            DisplayFilterMargins = true;

            try
            {
                LuaShellWindow = new LuaShell();
                LuaShellWindow.RunCommand = LuaRunCommand;
                LuaHelpers.RegisterNamespace("DemodulatorCollection.Demodulators");
                LuaHelpers.RegisterNamespace("DemodulatorCollection.BitClockSinks");
                RegisterScript("startup.lua", true);
            }
            catch(Exception ex)
            {
            }

            if(SNDP.Instance.Devices.Length > 0)
            {
                openHiQSDRMenuItem_Click(null, null);
            }
        }

        void AreaSelectionUpdate()
        {
            FFTAreaSelection sel = AreaSelection;
            double cursorStartFreq = 0;
            double cursorEndFreq = 0;

            string text = "";

            if (!DeviceOpened || !DemodState.DemodulationEnabled || !DemodState.BandwidthLimiter)
            {
                sel.Text = text;
                return;
            }

            double filterWidth = Device.SamplingRate / DemodState.BandwidthLimiterFract;

            if (DemodState.SignalDemodulator is AMDemodulator)
            {
                text = "AM:  ";
                sel.AreaMode = FFTAreaSelection.eAreaMode.Normal;
            }
            else if (DemodState.SignalDemodulator is FMDemodulator)
            {
                text = "FM:  ";
                sel.AreaMode = FFTAreaSelection.eAreaMode.Normal;
            }
            else if (DemodState.SignalDemodulator is SSBDemodulator)
            {
                switch (((SSBDemodulator)DemodState.SignalDemodulator).Type)
                {
                    case eSsbType.Usb:
                        text = "USB: ";
                        sel.AreaMode = FFTAreaSelection.eAreaMode.USB;
                        cursorStartFreq = 0;
                        cursorEndFreq = filterWidth / 2;
                        break;
                    case eSsbType.Lsb:
                        text = "LSB: ";
                        sel.AreaMode = FFTAreaSelection.eAreaMode.LSB;
                        cursorStartFreq = -filterWidth / 2;
                        cursorEndFreq = 0;
                        break;
                }
            }

            sel.Text = text + FrequencyFormatter.FreqToStringAccurate(filterWidth);

            switch (DemodState.SourceFrequency)
            {
                case DemodulationState.eSourceFrequency.Selection:
                    FFTDisplay.FFTDisplay.HorLineFixed = false;
                    break;
                case DemodulationState.eSourceFrequency.Cursor:
                    FFTDisplay.FFTDisplay.HorLineFixed = true;
                    FFTDisplay.FFTDisplay.HorLineStart = cursorStartFreq;
                    FFTDisplay.FFTDisplay.HorLineEnd = cursorEndFreq;
                    break;
                case DemodulationState.eSourceFrequency.Center:
                    FFTDisplay.FFTDisplay.HorLineFixed = true;
                    FFTDisplay.FFTDisplay.HorLineStart = cursorStartFreq;
                    FFTDisplay.FFTDisplay.HorLineEnd = cursorEndFreq;
                    break;
            }
        }

        void DemodOptions_CursorWindowFilterChanged(object sender, EventArgs e)
        {
            if (!DeviceOpened || DemodState == null || !DemodState.DemodulationEnabled || !DemodState.BandwidthLimiter)
            {
                return;
            }

            double filterWidth = Device.SamplingRate / DemodState.BandwidthLimiterFract;

 
            if (DemodState.SignalDemodulator is AMDemodulator || DemodState.SignalDemodulator is FMDemodulator)
            {
                double centerFreq = (AreaSelection.StartFreq + AreaSelection.EndFreq) / 2;

                AreaSelection.StartFreq = (long)(centerFreq - filterWidth / 2);
                AreaSelection.EndFreq = (long)(centerFreq + filterWidth / 2);
            }
            else if (DemodState.SignalDemodulator is SSBDemodulator)
            {
                switch (((SSBDemodulator)DemodState.SignalDemodulator).Type)
                {
                    case eSsbType.Lsb:
                        AreaSelection.StartFreq = (long)(AreaSelection.EndFreq - filterWidth);
                        break;
                    case eSsbType.Usb:
                        AreaSelection.EndFreq = (long)(AreaSelection.StartFreq + filterWidth);
                        break;
                }
                /* width has changed - let audio demodulator recalc xlat */
                DemodState.ReinitSound = true;
            }

            AreaSelectionUpdate();
            AreaSelection.UpdatePositions();
        }

        private void UpdateDemodFrequency()
        {
            /* not being used, so dont do anything */
            if (!DeviceOpened || DemodState.Dialog == null || DemodState == null)
            {
                return;
            }

            AreaSelectionUpdate();
            DemodState.Dialog.UpdateFrequency();
            CallScript("DemodFrequencyChanged", DemodState.DemodulationFrequency, DemodState.SignalDemodulator);

            /* update selection if frequency was changed */
            if (AreaSelection.Visible && DemodState.SourceFrequency == DemodulationState.eSourceFrequency.Selection)
            {
                long delta = 0;

                if (DemodState.SignalDemodulator is AMDemodulator || DemodState.SignalDemodulator is FMDemodulator)
                {
                    /* want center as baseband */
                    double baseFreq = (AreaSelection.StartFreq + AreaSelection.EndFreq) / 2;
                    delta = (long)(DemodState.DemodulationFrequency - baseFreq);
                }
                else if (DemodState.SignalDemodulator is SSBDemodulator)
                {
                    /* depending on USB or LSB choose the baseband freq */
                    switch (((SSBDemodulator)DemodState.SignalDemodulator).Type)
                    {
                        case eSsbType.Usb:
                            delta = (long)(DemodState.DemodulationFrequency - AreaSelection.StartFreq);
                            break;
                        case eSsbType.Lsb:
                            delta = (long)(DemodState.DemodulationFrequency - AreaSelection.EndFreq);
                            break;
                    }
                }

                if (delta != 0)
                {
                    AreaSelection.StartFreq += delta;
                    AreaSelection.EndFreq += delta;
                    AreaSelection.UpdatePositions();
                }
            }
        }

        void FFTAreaSelection_SelectionUpdated(object sender, EventArgs e)
        {
            FFTAreaSelection sel = AreaSelection;

            /* in any case update its text */
            AreaSelectionUpdate();

            /* not being used, so dont do anything */
            if (!DeviceOpened || !DemodState.DemodulationEnabled || !DemodState.BandwidthLimiter)
            {
                return;
            }

            if (AreaSelection.Visible)
            {
                double baseFreq = 0;

                if (DemodState.SignalDemodulator is AMDemodulator || DemodState.SignalDemodulator is FMDemodulator)
                {
                    /* want center as baseband */
                    baseFreq = (sel.StartFreq + sel.EndFreq) / 2;
                }
                else if (DemodState.SignalDemodulator is SSBDemodulator)
                {
                    /* depending on USB or LSB choose the baseband freq */
                    switch (((SSBDemodulator)DemodState.SignalDemodulator).Type)
                    {
                        case eSsbType.Usb:
                            baseFreq = AreaSelection.StartFreq;
                            break;
                        case eSsbType.Lsb:
                            baseFreq = AreaSelection.EndFreq;
                            break;
                    }
                }
                DemodState.DemodulationFrequencySelection = (long)baseFreq;
                UpdateDemodFrequency();
            }
        }



        void MainScreen_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            CallScript("FilesDropped", files);
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
            UnregisterScripts();
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

            if (DemodState != null)
            {
                if (DemodState.Dialog != null)
                {
                    DemodState.Dialog.Close();
                }

                DemodState.RemoveSinks();
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
                btnCloseDevice.Enabled = DeviceOpened;
                btnOpenDevice.Enabled = !DeviceOpened;

                if (value)
                {
                    UpdateRate = UpdateRate;
                    FFTSize = FFTSize;
                    CurrentFrequency = CurrentFrequency;
                    statusLabel.Text = "Connected";
                    CallScript("DeviceOpened", Device);
                }
                else
                {
                    statusLabel.Text = "Idle";
                    CallScript("DeviceClosed");
                }

                /* update transfer mode string */
                Device_TransferModeChanged(null, null);
            }
        }

        protected long FilterWidth
        {
            get
            {
                if (!DeviceOpened)
                {
                    return 0;
                }
                return Device.FilterWidth;
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

        object UpdateRateSignal = new object();
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
        
        public bool SampleValuesTrackPeaks
        {
            get { return FFTDisplay.SampleValuesTrackPeaks; }
            set
            {
                FFTDisplay.SampleValuesTrackPeaks = value;
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
                        if (DeviceOpened)
                        {
                            DemodState.DemodulationFrequencyCursor = FFTDisplay.Frequency;
                            UpdateDemodFrequency();
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
                            long freq = Math.Min(Device.HighestFrequency, Math.Max(0, newFreq));

                            //FFTDisplay.CenterFrequency = newFreq;
                            CurrentFrequency = freq;
                            CallScript("FrequencyChanged", freq);
                        }
                    }
                    break;

                /* jump to frequency under cursor */
                case eUserEvent.MouseDoubleClickLeft:
                    if (Device != null)
                    {
                        long freq = Math.Min(Device.HighestFrequency, Math.Max(0, FFTDisplay.Frequency));

                        if (ScanFrequenciesEnabled)
                        {
                            ScanFrequenciesEnabled = false;
                        }

                        CurrentFrequency = freq;
                        CallScript("FrequencyChanged", freq);
                        //FFTDisplay.CenterFrequency = freq;
                    }
                    break;

                /* bring up popup menu. has to be improved */
                case eUserEvent.MouseClickRight:
                    {
                        long freq = FFTDisplay.Frequency;

                        ContextMenu contextMenu = new ContextMenu();
                        MenuItem menuItem;
                        menuItem = new MenuItem("Frequency: " + FrequencyFormatter.FreqToStringAccurate(freq));
                        menuItem.Enabled = false;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("-");
                        menuItem.Enabled = false;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("Jump there");
                        menuItem.Click += (object sender, EventArgs e) => 
                        {
                            if (Device != null)
                            {
                                if (ScanFrequenciesEnabled)
                                {
                                    ScanFrequenciesEnabled = false;
                                }

                                CurrentFrequency = freq;
                                CallScript("FrequencyChanged", freq);
                            }
                        };
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("Add Marker...");
                        menuItem.Click += (object sender, EventArgs e) => { AddMarker(freq); };
                        contextMenu.MenuItems.Add(menuItem);

                        if (LuaContextEntries.Count > 0)
                        {
                            menuItem = new MenuItem("-");
                            menuItem.Enabled = false;
                            contextMenu.MenuItems.Add(menuItem);

                            menuItem = new MenuItem("LUA scripts:");
                            menuItem.Enabled = false;
                            contextMenu.MenuItems.Add(menuItem);

                            foreach (KeyValuePair<string, string> entry in LuaContextEntries)
                            {
                                string func = entry.Value;
                                menuItem = new MenuItem("    " + entry.Key);
                                menuItem.Click += (object sender, EventArgs e) => { CallScript(func, freq); };
                                contextMenu.MenuItems.Add(menuItem);
                            }
                        }

                        menuItem = new MenuItem("-");
                        menuItem.Enabled = false;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("Waterfall colors:");
                        menuItem.Enabled = false;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("     Gradient");
                        menuItem.Click += (object sender, EventArgs e) => 
                        {
                            ColorDialog dlg = new ColorDialog();
                            dlg.ShowDialog();
                            FFTDisplay.WaterfallDisplay.ColorTable = new ColorLookupTable(dlg.Color);
                        };
                        menuItem.Checked = (FFTDisplay.WaterfallDisplay.ColorTable.GetType() == typeof(ColorLookupTable));
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("     TriColor");
                        menuItem.Click += (object sender, EventArgs e) => { FFTDisplay.WaterfallDisplay.ColorTable = new MultiColorMap(8192, Color.Black, Color.FromArgb(0, 0, 128), Color.FromArgb(192, 0, 255), Color.White); };
                        menuItem.Checked = (FFTDisplay.WaterfallDisplay.ColorTable.GetType() == typeof(MultiColorMap));
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("     Pseudocolors");
                        menuItem.Click += (object sender, EventArgs e) => { FFTDisplay.WaterfallDisplay.ColorTable = new HeatColors(8192); };
                        menuItem.Checked = (FFTDisplay.WaterfallDisplay.ColorTable.GetType() == typeof(HeatColors));
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("-");
                        menuItem.Enabled = false;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("Filter correction profile:");
                        menuItem.Enabled = false;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("    Add current view to profile");
                        menuItem.Click += (object sender, EventArgs e) => { ProfileApply(false); };
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("    Load profile...");
                        menuItem.Click += (object sender, EventArgs e) => { ProfileLoad(false); };
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("    Save profile...");
                        menuItem.Click += (object sender, EventArgs e) => { ProfileSave(false); }; 
                        menuItem.Enabled = !FFTDisplay.FilterCorrection.Empty;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("    Clear profile");
                        menuItem.Click += (object sender, EventArgs e) => { FFTDisplay.FilterCorrection = new AttenuationCorrection(); };
                        menuItem.Enabled = !FFTDisplay.FilterCorrection.Empty;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("-");
                        menuItem.Enabled = false;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("Receiver correction profile:");
                        menuItem.Enabled = false;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("    Add current view to profile");
                        menuItem.Click += (object sender, EventArgs e) => { ProfileApply(true); };
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("    Load profile...");
                        menuItem.Click += (object sender, EventArgs e) => { ProfileLoad(true); };
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("    Save profile...");
                        menuItem.Click += (object sender, EventArgs e) => { ProfileSave(true); };
                        menuItem.Enabled = !FFTDisplay.ReceiverCorrection.Empty;
                        contextMenu.MenuItems.Add(menuItem);

                        menuItem = new MenuItem("    Clear profile");
                        menuItem.Click += (object sender, EventArgs e) => { FFTDisplay.ReceiverCorrection = new AttenuationCorrection(); };
                        menuItem.Enabled = !FFTDisplay.ReceiverCorrection.Empty;
                        contextMenu.MenuItems.Add(menuItem);

                        Point popupPos = this.PointToClient(MousePosition);

                        popupPos.X -= 20;
                        popupPos.Y -= 20;
                        contextMenu.Show(this, popupPos);
                    }
                    break;
            }
        }

        private void ProfileApply(bool receiver)
        {
            CorrectionProfile profile = new CorrectionProfile();

            var points = FFTDisplay.FFTDisplay.LinePoints;
            double rate = FFTDisplay.FFTDisplay.SamplingRate;
            double freqStep = rate / points.Length;
            double maxValue = 0;

            for (int pos = 0; pos < points.Length; pos++)
            {
                maxValue = Math.Min(maxValue, points[pos].Y);
            }

            if (receiver)
            {
                for (int pos = 0; pos < points.Length; pos++)
                {
                    profile.Add(new CorrectionProfilePoint((long)(FFTDisplay.CenterFrequency + (pos * freqStep - rate / 2)), maxValue - points[pos].Y));
                }

                CorrectionProfile oldProfile = FFTDisplay.ReceiverCorrection.GetProfile();

                CorrectionProfile merged = new CorrectionProfile(oldProfile, profile);
                FFTDisplay.ReceiverCorrection = new AttenuationCorrection(merged);
            }
            else
            {
                for (int pos = 0; pos < points.Length; pos++)
                {
                    profile.Add(new CorrectionProfilePoint((long)(pos * freqStep - rate / 2), maxValue - points[pos].Y));
                }

                FFTDisplay.FilterCorrection = new AttenuationCorrection(profile);
            }
        }

        private void ProfileSave(bool receiver)
        {
            if (FFTDisplay.FilterCorrection != null)
            {
                SaveFileDialog dlg = new SaveFileDialog();

                if (receiver)
                {
                    dlg.Filter = "Receiver profiles (*.rpr)|*.rpr|All files (*.*)|*.*";
                }
                else
                {
                    dlg.Filter = "Filter profiles (*.fpr)|*.fpr|All files (*.*)|*.*";
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        CorrectionProfile profile = null;
                        if (receiver)
                        {
                            profile = FFTDisplay.ReceiverCorrection.GetProfile();
                        }
                        else
                        {
                            profile = FFTDisplay.FilterCorrection.GetProfile();
                        }
                        profile.Save(dlg.FileName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error occurred: " + e.GetType().ToString());
                    }
                }
            }
        }

        private void ProfileLoad(bool receiver)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            if (receiver)
            {
                dlg.Filter = "Receiver profiles (*.rpr)|*.rpr|All files (*.*)|*.*";
            }
            else
            {
                dlg.Filter = "Filter profiles (*.fpr)|*.fpr|All files (*.*)|*.*";
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (receiver)
                    {
                        FFTDisplay.ReceiverCorrection = new AttenuationCorrection(dlg.FileName);
                    }
                    else
                    {
                        FFTDisplay.FilterCorrection = new AttenuationCorrection(dlg.FileName);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error occurred: " + e.GetType().ToString());
                }
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
            CallScript("MarkerAdded", marker);
        }

        void FFTReadFunc()
        {
            try
            {
                double lastRate = 0;

                double[] inputI;
                double[] inputQ;

                DateTime lastStrengthUpdate = DateTime.Now;
                DateTime lastDisplayUpdate = DateTime.Now;

                int spectPart = 0;
                //FFTDisplay.SpectParts = ScanFrequencies.Count;

                //Log.AddMessage("FFTReadFunc: Started");

                while (ReadThreadRun)
                {
                    /* when device uses stream transfer */
                    if (Device.TransferMode == eTransferMode.Stream)
                    {
                        /* sleep half the update rate before reading again.
                         * else we will read much too often and burn CPU time.
                         */
                        if (UpdateRate > 0)
                        {
                            /*
                            if (UpdateRate < 100)
                            {
                                lock (UpdateRateSignal)
                                {
                                    Monitor.Wait(UpdateRateSignal, (int)(1000 / UpdateRate));
                                }
                            }*/
                        }
                        else
                        {
                            //Thread.Sleep(500);
                        }
                    }

                    lock (FFTSizeSpinLock)
                    {
                        double rate = Device.SampleSource.InputSamplingRate;
                        bool rateChanged = Math.Abs(lastRate - rate) > 0;

                        lock (Device.SampleSource)
                        {
                            if (!ProcessPaused && Device.ReadBlock())
                            {
                                DateTime now = DateTime.Now;

                                if (Device.TransferMode == eTransferMode.Block)
                                {
                                    long avail = ((ShmemSampleSource)Device.SampleSource).ShmemChannel.Length;

                                    if (avail > 0)
                                    {
                                        // Log.AddMessage("FFTReadFunc: avail " + avail + " Bytes. Thats okay when FFT size was changed.");
                                    }
                                    //Device.SampleSource.Flush();
                                }

                                if (Device.SampleSource.BufferOverrun)
                                {
                                    Log.AddMessage("FFTReadFunc: Buffer overrun. Flushing.");
                                    Device.SampleSource.Flush();
                                }

                                /* only proceed if it is time to update the FFT display */
                                if (UpdateRate > 0 && (now - lastDisplayUpdate).TotalMilliseconds > (1000 / UpdateRate))
                                {
                                    lock (Device.SampleSource.SampleBufferLock)
                                    {
                                        inputI = Device.SampleSource.SourceSamplesI;
                                        inputQ = Device.SampleSource.SourceSamplesQ;

                                        if (CorrectDCOffset)
                                        {
                                            corr.PerformDCDetection(ref inputI, ref inputQ);
                                            corr.PerformDCCorrection(ref inputI, ref inputQ);
                                        }

                                        /* update strength display every some ms */
                                        if (now.Subtract(lastStrengthUpdate).TotalMilliseconds > StrengthUpdateTime)
                                        {
                                            lastStrengthUpdate = now;

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
                                            lock (DemodState)
                                            {
                                                LastSamplingRate = rate;

                                                FFTDisplay.SamplingRate = rate;

                                                /*
                                                DemodState.InputRate = rate;
                                                DemodState.ReinitSound = true;

                                                if (DemodState.Dialog != null && DemodState.Dialog.Visible)
                                                    DemodState.Dialog.UpdateInformation();
                                                */
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

                                            if (!ScanStrongestFrequency || DemodState.SquelchState == DemodulationState.eSquelchState.Closed)
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
                                            lastDisplayUpdate = now;
                                            PerformanceCounters.CounterVisualization.Start();
                                            FFTDisplay.ProcessData(inputI, inputQ, 0, Device.Amplification - Device.Attenuation);
                                            PerformanceCounters.CounterVisualization.Stop();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Thread.Sleep(1);
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
        
        private void OpenUSRPDevice()
        {
            try
            {
                USRPDeviceControl dev = new USRPDeviceControl();
            }
            catch (Exception e)
            {
                DeviceOpened = false;
            }
        }

        private void OpenBO35Device(USBRXDevice.eCombinationType type)
        {
            try
            {
                USBRXDeviceControl dev = new USBRXDeviceControl();

                dev.TunerCombination = type;

                dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
                dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
                dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
                dev.TransferModeChanged += new EventHandler(Device_TransferModeChanged);
                dev.InvertedSpectrumChanged += new EventHandler(Device_InvertedSpectrumChanged);
                dev.DeviceDisappeared += new EventHandler(Device_DeviceDisappeared);
                dev.DeviceOpened += new EventHandler(Device_DeviceOpened);
                dev.DeviceClosed += new EventHandler(Device_DeviceClosed);
                dev.SamplesPerBlock = Math.Min(32, Math.Max(1, SamplesToAverage)) * FFTSize;

                if (!dev.OpenTuner())
                {
                    MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                    return;
                }

                CurrentFrequency = dev.GetFrequency();
                Device = dev;
                Remote.Tuner = dev;
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

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.DeviceDisappeared += new EventHandler(Device_DeviceDisappeared);
            dev.DeviceOpened += new EventHandler(Device_DeviceOpened);
            dev.DeviceClosed += new EventHandler(Device_DeviceClosed);
            dev.SamplesPerBlock = Math.Min(32, Math.Max(1, SamplesToAverage)) * FFTSize;


            if (!dev.Connected)
            {
                MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                return;
            }

            Device = dev;
        }


        public void OpenSharedMem(int srcChan)
        {
            SharedMemDeviceControl dev = new SharedMemDeviceControl(srcChan);

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.DeviceOpened += new EventHandler(Device_DeviceOpened);
            dev.DeviceClosed += new EventHandler(Device_DeviceClosed);
            dev.SamplesPerBlock = Math.Max(1, SamplesToAverage) * FFTSize;

            if (!dev.Connected)
            {
                MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                return;
            }

            Device = dev;
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

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.DeviceClosed += new EventHandler(Device_DeviceClosed);
            dev.DeviceOpened += new EventHandler(Device_DeviceOpened);

            dev.OpenTuner();

            if (!dev.Connected)
            {
                MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                return;
            }

            Device = dev;
            Remote.Tuner = dev;
        }

        private void OpenNetworkDevice()
        {
            NetworkDeviceControl dev = new NetworkDeviceControl();

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.DeviceClosed += new EventHandler(Device_DeviceClosed);
            dev.DeviceOpened += new EventHandler(Device_DeviceOpened);

            dev.OpenTuner();

            if (!dev.Connected)
            {
                MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                return;
            }

            Device = dev;
            Remote.Tuner = dev;
        }

        private void OpenHiQSDR()
        {
            HiQSDRDeviceControl dev = new HiQSDRDeviceControl();

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.DeviceOpened += new EventHandler(Device_DeviceOpened);
            dev.DeviceClosed += new EventHandler(Device_DeviceClosed);

            dev.OpenTuner();

            if (!dev.Connected)
            {
                MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                return;
            }

            Device = dev;
            Remote.Tuner = dev;
        }

        void Device_DeviceOpened(object sender, EventArgs e)
        {
            StartThreads();

            DeviceOpened = true;
        }

        private void StartThreads()
        {
            /* create an extra shmem channel for audio decoding */
            AudioShmem = new ShmemSampleSource("RX-FFT Audio Decoder", Device.ShmemChannel, 1, 0);
            AudioShmem.InvertedSpectrum = Device.SampleSource.InvertedSpectrum;
            AudioShmem.DataFormat = Device.SampleSource.DataFormat;

            ReadThreadRun = true;
            ReadThread = new Thread(FFTReadFunc);
            ReadThread.Name = "MainScreen Data Read Thread";
            ReadThread.Start();

            UpdateRateThread = new Thread(UpdateRateThreadFunc);
            UpdateRateThread.Name = "UpdateRateThread";
            UpdateRateThread.Start();

            foreach (AudioDemodulator demod in AudioDemodulators)
            {
                demod.Start(AudioShmem);
            }
            foreach (KeyValuePair<FrequencyMarker, AudioDemodulator> pair in MarkerDemodulators)
            {
                pair.Value.Start(AudioShmem);
            }
        }

        private void UpdateRateThreadFunc(object obj)
        {
            while(UpdateRateThread != null)
            {
                double rate = UpdateRate;

                if (rate < 100 && rate > 0)
                {
                    lock (UpdateRateThread)
                    {
                        Monitor.Wait(UpdateRateThread, (int)(1000 / rate));
                    }

                    lock (UpdateRateSignal)
                    {
                        Monitor.Pulse(UpdateRateSignal);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void StopThreads()
        {
            /* pause transfers and finish threads */
            ProcessPaused = true;
            ReadThreadRun = false;

            if (DemodState.Dialog != null)
            {
                DemodState.Dialog.Close();
            }

            if (ReadThread != null)
            {
                if (!ReadThread.Join(100))
                {
                    ReadThread.Abort();
                }
                ReadThread = null;
            }

            if (UpdateRateThread != null)
            {
                UpdateRateThread.Abort();
                UpdateRateThread = null;
            }

            foreach (AudioDemodulator demod in AudioDemodulators)
            {
                demod.Stop();
            }
            foreach (KeyValuePair<FrequencyMarker, AudioDemodulator> pair in MarkerDemodulators)
            {
                pair.Value.Stop();
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
                DeviceOpened = false;
                Device.CloseControl();
                Device = null;
                if(DemodState.Dialog != null)
                {
                    DemodState.Dialog.Close();
                    DemodState.Dialog = null;
                }
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
                    }
                }
            }));

        }

        void Device_InvertedSpectrumChanged(object sender, EventArgs e)
        {
            Device.SampleSource.SavingInvertedSpectrum = Device.InvertedSpectrum;
        }

        void Device_TransferModeChanged(object sender, EventArgs e)
        {
            if (DeviceOpened)
            {
                statusLabel.Text = statusLabel.Text.Replace(" (" + eTransferMode.Stopped.ToString() + ")", "");
                statusLabel.Text = statusLabel.Text.Replace(" (" + eTransferMode.Block.ToString() + ")", "");
                statusLabel.Text = statusLabel.Text.Replace(" (" + eTransferMode.Stream.ToString() + ")", "");

                if (Device != null)
                {
                    statusLabel.Text += " (" + Device.TransferMode.ToString() + ")";
                }
            }
        }

        void Device_FilterWidthChanged(object sender, EventArgs e)
        {
            if (DeviceOpened)
            {
                FFTDisplay.FitSpectrumWidth = ((double)Device.FilterWidth / (double)Device.SamplingRate);

                FFTDisplay.LimiterUpperLimit = Device.UpperFilterMargin;
                FFTDisplay.LimiterLowerLimit = Device.LowerFilterMargin;
                FFTDisplay.LimiterUpperDescription = Device.UpperFilterMarginDescription;
                FFTDisplay.LimiterLowerDescription = Device.LowerFilterMarginDescription;
            }
        }

        void Device_RateChanged(object sender, EventArgs e)
        {
            if (DeviceOpened)
            {
                FFTDisplay.FitSpectrumWidth = ((double)Device.FilterWidth / (double)Device.SamplingRate);
                FFTDisplay.SamplingRate = Device.SamplingRate;

                FFTDisplay.LimiterUpperLimit = Device.UpperFilterMargin;
                FFTDisplay.LimiterLowerLimit = Device.LowerFilterMargin;
                FFTDisplay.LimiterUpperDescription = Device.UpperFilterMarginDescription;
                FFTDisplay.LimiterLowerDescription = Device.LowerFilterMarginDescription;

                foreach (AudioDemodulator demod in AudioDemodulators)
                {
                    demod.DemodState.InputRate = Device.SamplingRate;
                    demod.DemodState.ReinitSound = true;
                    if (demod.DemodState.Dialog != null && demod.DemodState.Dialog.Visible)
                        demod.DemodState.Dialog.UpdateInformation();
                }
                foreach (KeyValuePair<FrequencyMarker, AudioDemodulator> pair in MarkerDemodulators)
                {
                    pair.Value.DemodState.InputRate = Device.SamplingRate;
                    pair.Value.DemodState.ReinitSound = true;
                    if (pair.Value.DemodState.Dialog != null && pair.Value.DemodState.Dialog.Visible)
                        pair.Value.DemodState.Dialog.UpdateInformation();
                }


                /* TODO: try to find optimal decimation rate 
                foreach (AudioDemodulator demod in AudioDemodulators)
                {
                    if (demod.DemodState.SoundDevice != null && demod.DemodState.SoundDevice.Rate < Device.SamplingRate)
                    {
                        double exactDecim = (double)Device.SamplingRate / (double)demod.DemodState.SoundDevice.Rate;

                        demod.DemodState.AudioDecimation = (int)Math.Ceiling(exactDecim);
                    }
                    else
                    {
                        demod.DemodState.AudioDecimation = 1;
                    }

                    DemodState.ReinitSound = true;
                    if (DemodState.Dialog != null)
                    {
                        DemodState.Dialog.UpdateInformation();
                    }
                }
                */

            }
        }

        void Device_FrequencyChanged(object sender, EventArgs e)
        {
            if (DeviceOpened)
            {
                long freq = Device.GetFrequency();

                /* give FFT area selection the chance to update its downmix parameters */
                FFTAreaSelection_SelectionUpdated(null, null);
                DemodState.DemodulationFrequencyCenter = freq;

                foreach (AudioDemodulator demod in AudioDemodulators)
                {
                    demod.DemodState.BaseFrequency = freq;
                }
                foreach (KeyValuePair<FrequencyMarker, AudioDemodulator> pair in MarkerDemodulators)
                {
                    pair.Value.DemodState.BaseFrequency = Device.GetFrequency();
                }

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
        }

        private void openUSRPMenu_Click(object sender, EventArgs e)
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

            OpenUSRPDevice();

            animateStatus = false;
            StatusTextDock.Hide();
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

            OpenBO35Device(USBRXDevice.eCombinationType.None);

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

        private void openNetworkDeviceMenu_Click(object sender, EventArgs e)
        {
            OpenNetworkDevice();
        }

        private void openHiQSDRMenuItem_Click(object sender, EventArgs e)
        {
            OpenHiQSDR();
        }

        private void markersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MarkerDialog == null || MarkerDialog.IsDisposed)
            {
                MarkerDialog = new MarkerListDialog(MarkerList, this);
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
            Device.SampleSource.SavingInvertedSpectrum = Device.InvertedSpectrum;
            Device.SampleSource.SavingFileName = fileName;
            Device.SampleSource.SavingEnabled = true;
            saveMenu.Text = "Stop saving";
        }

        private Dictionary<string, string> LuaContextEntries = new Dictionary<string, string>();
        private LinkedList<Script> RegisteredScripts = new LinkedList<Script>();
        private struct Script
        {
            public Lua luaVm;
            public string fileName;
            public ToolStripMenuItem menuItem;
            public bool hidden;
        }


        [AttrLuaFunc("print", "Print string into log window", new[] { "Text to print" })]
        public static void LuaPrint(string text)
        {
            Instance.LuaShellWindow.AddMessage(text);
        }

        private Script RegisterScript(string fileName)
        {
            return RegisterScript(fileName, false);
        }

        private Script RegisterScript(string fileName, bool hide)
        {
            Script script = new Script();
            script.luaVm = new Lua();
            script.fileName = fileName;
            script.hidden = hide;

            if (!script.hidden)
            {
                script.menuItem = new System.Windows.Forms.ToolStripMenuItem();
                script.menuItem.Text = "Unload '" + fileName + "'";
                script.menuItem.Click += (object sender, EventArgs e) => { UnregisterScript(script); };
                unloadScriptToolStripMenuItem.DropDownItems.Add(script.menuItem);
            }
            RegisteredScripts.AddLast(script);

            LuaHelpers.RegisterLuaFunctions(script.luaVm, new LuaHelpers());
            /* override e.g. print */
            LuaHelpers.RegisterLuaFunctions(script.luaVm, this);

            try
            {
                script.luaVm.DoFile(script.fileName);
                try
                {
                    LuaHelpers.CallFunction(script.luaVm, "Init", this);
                }
                catch (Exception e)
                {
                    LuaShellWindow.AddMessage("Failed to init LUA Script: " + e.ToString());
                }
            }
            catch (Exception e)
            {
                LuaShellWindow.AddMessage("Failed to load LUA Script: " + e.ToString());
            }

            return script;
        }


        private void ReloadScriptsMenu_Click(object sender, EventArgs e)
        {
            LinkedList<Script> tempList = new LinkedList<Script>();

            foreach (Script script in RegisteredScripts)
            {
                if (!script.hidden)
                {
                    tempList.AddLast(script);
                }
            }

            foreach (Script script in tempList)
            {
                UnregisterScript(script);
            }

            foreach (Script script in tempList)
            {
                RegisterScript(script.fileName, script.hidden);
            }
        }

        private void UnregisterScripts()
        {
            LinkedList<Script> tempList = new LinkedList<Script>();

            foreach (Script script in RegisteredScripts)
            {
                if (!script.hidden)
                {
                    tempList.AddLast(script);
                }
            }

            foreach (Script script in tempList)
            {
                UnregisterScript(script);
            }
        }

        private bool UnregisterScript(Script script)
        {
            try
            {
                LuaFunction func = script.luaVm.GetFunction("Deinit");
                if (func != null)
                {
                    func.Call();
                }
                unloadScriptToolStripMenuItem.DropDownItems.Remove(script.menuItem);
                RegisteredScripts.Remove(script);

                return true;
            }
            catch (Exception e)
            {
                LuaShellWindow.AddMessage("Failed to unregister LUA Script '" + script.fileName + "': " + e.ToString());
            }

            return false;
        }

        private bool LuaRunCommand(string command)
        {
            foreach (Script script in RegisteredScripts)
            {
                try
                {
                    script.luaVm.DoString(command);
                    return true;
                }
                catch (Exception e)
                {
                }
            }
            return false;
        }

        private bool CallScript(string function, params object[] parameters)
        {
            foreach (Script script in RegisteredScripts)
            {
                try
                {
                    LuaFunction func = script.luaVm.GetFunction(function);
                    if (func != null)
                    {
                        func.Call(parameters);
                    }
                }
                catch (Exception e)
                {
                    LuaShellWindow.AddMessage("Failed to call '" + function + "' in script '" + script.fileName + "': " + e.ToString());
                }
            }
            return false;
        }

        public void LuaAddContextMenuScript(string name, string function)
        {
            if (LuaContextEntries.ContainsKey(name))
            {
                LuaContextEntries[name] = function;
            }
            else
            {
                LuaContextEntries.Add(name, function);
            }
        }

        private void loadScriptMenu_Click(object sender, EventArgs e)
        {
            FileDialog dlg = new OpenFileDialog();

            dlg.Filter = "LUA Scripts (*.lua)|*.lua|All files (*.*)|*.*";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    RegisterScript(dlg.FileName);
                }
                catch (Exception ex)
                {
                    Log.AddMessage("Failed to load LUA file: " + e.ToString());
                }
            }
        }

        private void unloadAllMenu_Click(object sender, EventArgs e)
        {
            UnregisterScripts();
        }


        private void ScriptShellMenu_Click(object sender, EventArgs e)
        {
            if (LuaShellWindow != null )
            {
                if (LuaShellWindow.IsDisposed)
                {
                    LuaShellWindow = null;
                }
                else
                {
                    LuaShellWindow.Show();
                    return;
                }
            }
            
            if (LuaShellWindow == null)
            {
                LuaShellWindow = new LuaShell();
                LuaShellWindow.RunCommand = LuaRunCommand;
                LuaShellWindow.Show();
            }
        }

        private void menuFft512_Click(object sender, EventArgs e)
        {
            FFTSize = 512;
        }

        private void menuFft1024_Click(object sender, EventArgs e)
        {
            FFTSize = 1024;
        }

        private void menuFft2048_Click(object sender, EventArgs e)
        {
            FFTSize = 2048;
        }

        private void menuFft4096_Click(object sender, EventArgs e)
        {
            FFTSize = 4096;
        }

        private void menuFft8192_Click(object sender, EventArgs e)
        {
            FFTSize = 8192;
        }

        private void btnStartScope_Click(object sender, EventArgs e)
        {
            oscilloscopeMenu_Click(sender, e);
        }

        private void btnStartGsmAnalyzer_Click(object sender, EventArgs e)
        {
            gsmAnalyzerMenu_Click(sender, e);
        }

        private void btnCloseDevice_Click(object sender, EventArgs e)
        {
            closeMenu_Click(sender, e);
        }

        private void btnOpenDevice_Click(object sender, EventArgs e)
        {
            openBO35PlainMenu_Click(sender, e);
        }

        private void dCOffsetCorrectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dCOffsetCorrectionToolStripMenuItem.Checked ^= true;
            CorrectDCOffset = dCOffsetCorrectionToolStripMenuItem.Checked;
        }
    }
}
