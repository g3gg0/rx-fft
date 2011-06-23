using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Components.GDI;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;
using LibRXFFT.Libraries.GSM.Layer1.GMSK;
using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.SignalProcessing;
using Timer = System.Windows.Forms.Timer;
using LibRXFFT.Libraries.USB_RX.Tuners;
using RX_FFT.Components.GDI;
using LibRXFFT.Libraries.GSM.Layer1.PacketDump;
using System.IO;
using System.ServiceModel;
using LibRXFFT.Components.DeviceControls;
using LuaInterface;

namespace GSM_Analyzer
{
    public partial class GSMAnalyzer : Form
    {
        public DeviceControl Source;
        private Tuner _Device;
        public Tuner Device
        {
            get { return _Device; }
            set
            {
                _Device = value;
                ChannelHandler.Device = value;

                txtArfcn.Enabled = (Device != null);
                btnScan.Enabled = (Device != null);

                splitContainer5.SplitterDistance = (Device != null) ? 115 : 0;

            }
        }
        private RadioChannelHandler ChannelHandler;
        private Thread ChannelScanThread;
        public ChannelSplitter Splitter;

        private Thread ReadThread;
        private bool ThreadActive;

        public double[] BurstLengthJitter = new[] { 0.75d, -0.25d, -0.25d, -0.25d };

        private Semaphore SingleStepSem = new Semaphore(0, 1, "SingleStepSemaphore");
        private bool SingleStep;

        internal TimeSlotHandler Handler;
        internal GSMParameters Parameters;
        internal GMSKDemodulator[] Demodulator;
        private FilterDialog FilterWindow;
        private BurstVisualizer BurstWindow;
        private SpectrumVisualizer SpectrumWindow;
        private OptionsDialog OptionsWindow;
        private StationListDialog StationDialog;

        private Object BurstWindowLock = new Object();
        private Object SpectrumWindowLock = new Object();

        private DateTime LastTextBoxUpdate = DateTime.Now;
        private StringBuilder TextBoxBuffer = new StringBuilder(32768);
        private Timer TextBoxCommitTimer = new Timer();

        private bool ShowSlotUsage = false;
        private DateTime LastUiUpdate = DateTime.Now;
        private int UiUpdateTime = 250; /* ms */
        public bool Subsampling = false;
        public int InternalOversampling = 1;
        public double SubSampleOffset = 0;

        internal double DefaultSamplingRate = 2184533;
        internal double SymbolRate = 270833;
        private double BT = 0.3d;

        public string AuthHostAddress = "";
        private AuthService AuthHost = null;

        public string KrakenHostAddress = "";

        public class KrakenCracker : CipherCracker
        {
            private GSMAnalyzer Analyzer = null;
            public KrakenClient Kraken = null;
            private int JobNumber = 0;
            private int JobCount = 0;
            private DateTime LastConnect = DateTime.MinValue;

            public KrakenCracker(GSMAnalyzer analyzer)
            {
                Analyzer = analyzer;
            }

            #region CipherCracker Member

            public void SetJobInfo(int jobNumber, int jobCount)
            {
                JobNumber = jobNumber;
                JobCount = jobCount;
            }

            public void GetJobInfo(out int jobNumber, out int jobCount)
            {
                jobNumber = JobNumber;
                jobCount = JobCount;
            }

            public byte[] Crack(bool[] key1, uint count1, bool[] key2, uint count2)
            {
                if (!Available)
                {
                    return null;
                }

                return Kraken.RequestResult(key1, count1, key2, count2);
            }

            public bool Available
            {
                get
                {
                    if (Kraken == null)
                    {
                        /* first time and host configured */
                        if (Analyzer.KrakenHostAddress != null && Analyzer.KrakenHostAddress.Length > 0)
                        {
                            Kraken = new KrakenClient(Analyzer.KrakenHostAddress);
                            Kraken.Connect();
                        }
                        else
                        {
                            /* no host configured */
                            return false;
                        }
                    }
                    else
                    {
                        /* no host configured anymore */
                        if (Analyzer.KrakenHostAddress == null || Analyzer.KrakenHostAddress.Length == 0)
                        {
                            Kraken.Disconnect();
                            Kraken = null;
                            return false;
                        }

                        /* host changed */
                        if (Analyzer.KrakenHostAddress != Kraken.Hostname)
                        {
                            Kraken.Disconnect();
                            Kraken = new KrakenClient(Analyzer.KrakenHostAddress);
                            Kraken.Connect();
                        }
                    }

                    /* check if still connected */
                    if (!Kraken.Connected)
                    {
                        /* dont flood */
                        if ((DateTime.Now - LastConnect).TotalSeconds > 10)
                        {
                            Log.AddMessage("KrakenCracker", "Trying to reconnect to Kraken server (" + Analyzer.KrakenHostAddress + ") ...");
                            LastConnect = DateTime.Now;
                            Kraken.Connect();

                            if (Kraken.Connected)
                            {
                                Log.AddMessage("KrakenCracker", "    Connected!");
                                return true;
                            }
                            else
                            {
                                Log.AddMessage("KrakenCracker", "    Failed!");
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }

                    return false;
                }
            }


            public int SearchDuration
            {
                get { return Kraken.SearchDuration; }
            }

            public void Close()
            {
                if (!Available)
                {
                    return;
                }

                Kraken.Disconnect();
            }

            #endregion
        }


        internal double CurrentSampleRate
        {
            get
            {
                if (Source != null)
                    return Source.SamplingRate;

                return DefaultSamplingRate;
            }
        }

        internal double Oversampling
        {
            get
            {
                return CurrentSampleRate / SymbolRate;
            }
        }

        public bool _InvertedSpectrum = false;
        public bool InvertedSpectrum
        {
            get
            {
                if (Source != null)
                {
                    _InvertedSpectrum = Source.SampleSource.InvertedSpectrum;
                }

                return _InvertedSpectrum;
            }
            set
            {
                if (Source != null)
                {
                    Source.SampleSource.InvertedSpectrum = value;
                }

                _InvertedSpectrum = value;
            }
        }

        public GSMAnalyzer()
        {
            InitializeComponent();

            Log.Init();

            try
            {
                Splitter = new ChannelSplitter();
                Splitter.Config.BaseFrequency = 0;

                if (false)
                {
                    Splitter.Config.Channels = new ChannelSplitter.ChannelConfig[2];
                    Splitter.Config.Channels[0] = new ChannelSplitter.ChannelConfig();
                    Splitter.Config.Channels[1] = new ChannelSplitter.ChannelConfig();
                    Splitter.Config.Channels[0].ChannelWidth = SymbolRate; /* 270 kHz channel width */
                    Splitter.Config.Channels[1].ChannelWidth = SymbolRate; /* 270 kHz channel width */

                    /* hack: one channel aside */
                    Splitter.Config.Channels[0].FrequencyOffset = 500000;
                    Splitter.Config.Channels[1].FrequencyOffset = -500000;
                }
                else
                {
                    Splitter.Config.Bypass = true;
                    Splitter.Config.Channels = new ChannelSplitter.ChannelConfig[1];
                    Splitter.Config.Channels[0] = new ChannelSplitter.ChannelConfig();
                }
                Splitter.UpdateConfig();


                Parameters = new GSMParameters();
                Parameters.CipherCracker = new KrakenCracker(this);
                krakenStatusBox1.SetCracker((KrakenCracker)Parameters.CipherCracker);

                InitLua();

                Demodulator = new GMSKDemodulator[Splitter.Config.Channels.Length];
                for (int chan = 0; chan < Demodulator.Length; chan++)
                {
                    Demodulator[chan] = new GMSKDemodulator();
                }

                /* set up a channel handler for resolving ARFCN to frequencies */
                ChannelHandler = new RadioChannelHandler(Device);
                ChannelHandler.FrequencyOffset = 0;

                /* from: http://de.wikipedia.org/wiki/ARFCN or GSM-05.05 Chapter 2 */
                ChannelHandler.AddBand(new FrequencyBand("E-GSM a", 935000000, 200000, 0, 124));
                ChannelHandler.AddBand(new FrequencyBand("E-GSM b", 925200000, 200000, 975, 1023)); /* sure? */
                ChannelHandler.AddBand(new FrequencyBand("T-GSM 900", 915600000, 200000, 1024, 1052));
                ChannelHandler.AddBand(new FrequencyBand("DCS 1800", 1805200000, 200000, 512, 885));

                txtArfcn.Value = ChannelHandler.LowestChannel;

                /* disable controls */
                Device = null;

                /* already init here to load XML files */
                InitTimeSlotHandler();

                TextBoxCommitTimer.Tick += new EventHandler(TextBoxCommitTimer_Tick);
                TextBoxCommitTimer.Interval = 100;

                slotUsageControl.Visible = ShowSlotUsage;

                CryptA5.SelfCheck();
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to initialize: " + e.GetType().ToString());
            }
        }

        private void InitLua()
        {
            try
            {
                Parameters.LuaVm = new Lua();

                LuaHelpers.RegisterLuaFunctions(Parameters.LuaVm, new LuaHelpers());
                Parameters.LuaVm.DoFile("GSM Analyzer.lua");
                try
                {
                    LuaHelpers.CallFunction(Parameters.LuaVm, "Init", this);
                }
                catch (Exception e)
                {
                    Log.AddMessage("Failed to init LUA Script: " + e.ToString());
                }
            }
            catch (TypeInitializationException e)
            {
                Log.AddMessage("Failed to init LUA Script - Running under linux?");
            }
            catch (Exception e)
            {
                Log.AddMessage("Failed to init LUA Script: " + e.ToString());
            }
        }


        void TextBoxCommitTimer_Tick(object sender, EventArgs e)
        {
            /* just call the AddMessage routine to commit the text in buffers */
            AddMessage("");
        }

        public void RegisterTriggers(L3Handler L3Handler)
        {
            L3Handler.PDUDataTriggers.Add("LAIUpdate", TriggerLAIUpdate);
            L3Handler.PDUDataTriggers.Add("RANDreceived", TriggerRandReceived);
        }


        private void TriggerRandReceived(L3Handler L3Handler)
        {
            byte[] rand = null;

            lock (L3Handler.PDUDataRaw)
            {
                if (L3Handler.PDUDataRaw.ContainsKey("RAND"))
                {
                    rand = L3Handler.PDUDataRaw["RAND"];
                }
            }

            if (rand == null)
            {
                return;
            }

            /* try to connect to an auth host */
            if (AuthHost == null && AuthHostAddress.Length > 0)
            {
                try
                {
                    AuthHost = ConnectAuthHost();
                }
                catch (Exception ex)
                {
                    AuthHost = null;
                    Log.AddMessage("Failed to connect to auth host '" + AuthHostAddress + "'");
                }
            }

            if (rand.Length != 0 && AuthHost != null)
            {
                byte[] resp = AuthHost.RunGsmAlgo(rand);
                if (resp == null)
                {
                    Log.AddMessage("Failed retrieving Kc from '" + AuthHostAddress + "'");
                    return;
                }
                byte[] key = new byte[8];
                Array.Copy(resp, 4, key, 0, 8);

                Parameters.AddA5Key(key);
            }
        }

        private AuthService ConnectAuthHost()
        {
            AuthService service = null;

            ChannelFactory<AuthService> scf = new ChannelFactory<AuthService>(new NetTcpBinding(), "net.tcp://" + AuthHostAddress + ":8005");

            service = scf.CreateChannel();
            if (!service.Available())
            {
                Log.AddMessage("Auth host '" + AuthHostAddress + "' rejected - not available.");
                return null;
            }

            Log.AddMessage("Connected to auth host");
            (service as ICommunicationObject).Faulted += new EventHandler((object s, EventArgs ev) =>
            {
                AuthHost = null;
                Log.AddMessage("Disconnected from auth host");
            });

            return service;
        }

        private void TriggerLAIUpdate(L3Handler L3Handler)
        {
            string mccMncString = "-1";
            string lacString = "-1";
            string cellIdentString = "-1";

            lock (L3Handler.PDUDataFields)
            {
                if (L3Handler.PDUDataFields.ContainsKey("MCC/MNC"))
                    mccMncString = L3Handler.PDUDataFields["MCC/MNC"];

                if (L3Handler.PDUDataFields.ContainsKey("LAC"))
                    lacString = L3Handler.PDUDataFields["LAC"];

                if (L3Handler.PDUDataFields.ContainsKey("CellIdent"))
                    cellIdentString = L3Handler.PDUDataFields["CellIdent"];
            }

            int mnc = -1;
            int mcc = -1;
            int lac = -1;
            int cellIdent = -1;

            if (mccMncString.Length == 6)
            {
                int.TryParse(mccMncString.Substring(0, 3), out mcc);
                int.TryParse(mccMncString.Substring(3, 3), out mnc);
            }

            int.TryParse(lacString, out lac);
            int.TryParse(cellIdentString, out cellIdent);

            Parameters.MCC = mcc;
            Parameters.MNC = mnc;
            Parameters.LAC = lac;
            Parameters.CellIdent = cellIdent;

            UpdateCellInfo(mcc, mnc, lac, cellIdent, Parameters.CBCH);
        }


        private void UpdateCellInfoFunc(int mcc, int mnc, long lac, long cellident, eTriState hasCBCH)
        {
            if (mcc > 0 && mnc > 0)
                lblMCCMNC.Text = "" + string.Format("{0:000}", mcc) + " " + string.Format("{0:000}", mnc);
            else
                lblMCCMNC.Text = "---";

            if (lac > 0)
                lblLAC.Text = "" + lac;
            else
                lblLAC.Text = "---";

            if (cellident > 0)
                lblCellIdent.Text = "" + cellident;
            else
                lblCellIdent.Text = "---";

            if (hasCBCH == eTriState.Yes)
                lblCellBroadcast.Text = "yes";
            else if (hasCBCH == eTriState.No)
                lblCellBroadcast.Text = "no";
            else
                lblCellBroadcast.Text = "---";
        }

        public void AddMessageFunc(String msg)
        {
            if (msg == null)
            {
                txtLog.Clear();
                TextBoxBuffer.Length = 0;
                return;
            }

            if (DateTime.Now.Subtract(LastTextBoxUpdate).TotalMilliseconds < 100)
            {
                TextBoxBuffer.Append(msg);
                TextBoxCommitTimer.Start();
            }
            else
            {
                TextBoxCommitTimer.Stop();
                if (TextBoxBuffer.Length > 0)
                {
                    txtLog.AppendText(TextBoxBuffer.ToString());
                    TextBoxBuffer.Length = 0;
                }
                txtLog.AppendText(msg);
                LastTextBoxUpdate = DateTime.Now;
            }
        }


        void UpdateFreqOffset(double offset)
        {
            lblFreqOffset.Text = FrequencyFormatter.FreqToString(offset);
        }

        void UpdateUIStatus(GSMParameters param)
        {
            if (ShowSlotUsage)
            {
                slotUsageControl.UpdateSlots(param.ActiveBursts);
            }

            switch (param.State)
            {
                case eGSMState.Idle:
                case eGSMState.Reset:
                    statusSearch.State = eLampState.Grayed;
                    statusTrain.State = eLampState.Grayed;
                    statusLock.State = eLampState.Grayed;

                    lock (Handler.L3.PDUDataFields)
                    {
                        Handler.L3.PDUDataFields.Remove("MCC/MNC");
                        Handler.L3.PDUDataFields.Remove("LAC");
                        Handler.L3.PDUDataFields.Remove("CellIdent");
                    }
                    param.CBCH = eTriState.Unknown;
                    UpdateCellInfo(-1, -1, -1, -1, param.CBCH);

                    break;

                case eGSMState.FCCHSearch:
                    statusSearch.State = eLampState.Green;
                    statusTrain.State = eLampState.Grayed;
                    statusLock.State = eLampState.Grayed;

                    lock (Handler.L3.PDUDataFields)
                    {
                        Handler.L3.PDUDataFields.Remove("MCC/MNC");
                        Handler.L3.PDUDataFields.Remove("LAC");
                        Handler.L3.PDUDataFields.Remove("CellIdent");
                    }
                    param.CBCH = eTriState.Unknown;
                    UpdateCellInfo(-1, -1, -1, -1, param.CBCH);
                    break;

                case eGSMState.SCHSearch:
                    statusSearch.State = eLampState.Green;
                    statusTrain.State = eLampState.Green;
                    statusLock.State = eLampState.Grayed;

                    lock (Handler.L3.PDUDataFields)
                    {
                        Handler.L3.PDUDataFields.Remove("MCC/MNC");
                        Handler.L3.PDUDataFields.Remove("LAC");
                        Handler.L3.PDUDataFields.Remove("CellIdent");
                    }
                    param.CBCH = eTriState.Unknown;
                    UpdateCellInfo(-1, -1, -1, -1, param.CBCH);
                    break;

                case eGSMState.Lock:
                    statusSearch.State = eLampState.Green;
                    statusTrain.State = eLampState.Green;
                    statusLock.State = param.Errors > 0 ? eLampState.Red : eLampState.Green;
                    break;
            }
        }

        private void UpdatePowerDetails(double averagePower, double averageIdlePower)
        {
            double avg = DBTools.SampleTodB(averagePower);
            if (double.IsNaN(avg) || double.IsInfinity(avg))
            {
                lblPower.Text = "--.-- dB";
            }
            else
            {
                lblPower.Text = avg.ToString("#0.0 dB");
            }

            double avgIdle = DBTools.SampleTodB(averageIdlePower);
            if (double.IsNaN(avgIdle) || double.IsInfinity(avgIdle))
            {
                lblIdlePower.Text = "--.-- dB";
            }
            else
            {
                lblIdlePower.Text = avgIdle.ToString("#0.0 dB");
            }

            double snr = DBTools.SampleTodB(averagePower) - DBTools.SampleTodB(averageIdlePower);
            if (double.IsNaN(snr) || double.IsInfinity(snr))
            {
                lblSnr.Text = "--.-- dB";
            }
            else
            {
                lblSnr.Text = DBTools.SampleTodB(snr).ToString("#0.0 dB");
            }
        }

        /* intentionally dont pass GSMParameters due to thread safeness */
        void UpdateErrorSuccess(long err, long succ, long TN, long T1, long T2, long T3)
        {
            if (err >= 0)
                lblErrors.Text = "" + err;
            else
                lblErrors.Text = "---";

            if (succ >= 0)
                lblSucess.Text = "" + succ;
            else
                lblSucess.Text = "---";

            if (err + succ > 0)
                lblRate.Text = "" + ((succ * 100) / (succ + err)) + " %";
            else
                lblRate.Text = "--- %";

            if (T1 >= 0)
                lblT1.Text = "" + T1;
            else
                lblT1.Text = "---";

            if (T2 >= 0)
                lblT2.Text = "" + T2;
            else
                lblT2.Text = "---";

            if (T3 >= 0)
                lblT3.Text = "" + T3;
            else
                lblT3.Text = "---";

            if (TN >= 0)
                lblTN.Text = "" + TN;
            else
                lblTN.Text = "---";

            lblDate.Text = Parameters.TimeStamp.ToShortDateString() + " " + Parameters.TimeStamp.ToShortTimeString();
        }


        public void AddMessage(String msg)
        {
            try
            {
                this.BeginInvoke(new Action(() => AddMessageFunc(msg)));
            }
            catch (Exception)
            {
            }
        }

        public void UpdateCellInfo(int mcc, int mnc, long lac, long cellIdent, eTriState hasCBCH)
        {
            try
            {
                BeginInvoke(new Action(() => UpdateCellInfoFunc(mcc, mnc, lac, cellIdent, hasCBCH)));
            }
            catch (Exception)
            {
            }
        }

        void UpdateStats(GSMParameters param)
        {
            try
            {
                BeginInvoke(new Action(() => UpdatePowerDetails(param.AveragePower, param.AverageIdlePower)));
                BeginInvoke(new Action(() => UpdateErrorSuccess(param.TotalErrors, param.TotalSuccess, param.TN, param.T1, param.T2, param.T3)));
                BeginInvoke(new Action(() => UpdateFreqOffset(param.PhaseOffsetFrequency)));
            }
            catch (Exception)
            {
            }
        }

        void ResetStats()
        {
            try
            {
                BeginInvoke(new Action(() => UpdateErrorSuccess(-1, -1, -1, -1, -1, -1)));
            }
            catch (Exception)
            {
            }
        }

        private void btnOpen_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    InitLua();
                }
            }
            catch (Exception ex)
            {
                Log.AddMessage("Failed to init LUA Script: " + e.ToString());
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (ReadThread != null)
            {
                CloseSource();
            }
            else
            {
                InitLua();

                ContextMenu menu = new ContextMenu();

                menu.MenuItems.Add(new MenuItem("Shared Memory", new EventHandler(btnOpen_SharedMemory)));
                menu.MenuItems.Add(new MenuItem("IQ Wave File", new EventHandler(btnOpen_IQFile)));
                menu.MenuItems.Add(new MenuItem("Network Source", new EventHandler(btnOpen_NetworkSource)));
                menu.MenuItems.Add(new MenuItem("-"));
                menu.MenuItems.Add(new MenuItem("Dumped Traffic", new EventHandler(btnOpen_DumpFile)));
                menu.MenuItems.Add(new MenuItem("Dumped Traffic (multi)", new EventHandler(btnOpen_DumpFileMulti)));
                menu.MenuItems.Add(new MenuItem("-"));
                menu.MenuItems.Add(new MenuItem("Osmocon Bitstream", new EventHandler(btnOpen_OsmoconBitstream)));
                menu.MenuItems.Add(new MenuItem("-"));
                menu.MenuItems.Add(new MenuItem("LUA Script", new EventHandler(btnOpen_LuaScript)));

                menu.Show(this, new Point(10, 10));
            }
        }

        private void CloseSource()
        {
            ThreadActive = false;

            if (ReadThread != null)
            {
                if (!ReadThread.Join(500))
                {
                    ReadThread.Abort();
                }

                ReadThread = null;
            }

            if (ChannelScanThread != null)
            {
                ChannelScanThread.Abort();
                ChannelScanThread = null;
                btnScan.Text = "Scan";
            }

            if (Source != null)
            {
                Source.CloseControl();
                Source = null;
            }

            /* finally kill any connection (may be blocking) */
            if (Parameters.CipherCracker != null)
            {
                Parameters.CipherCracker.Close();
            }

            btnOpen.Text = "Open";
            SetDataSource("");
        }

        private void btnOpen_LuaScript(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "LUA Scripts (*.lua)|*.lua|All files (*.*)|*.*";
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in dlg.FileNames)
                {
                    try
                    {
                        Parameters.LuaVm.DoFile(file);
                    }
                    catch (LuaScriptException ex)
                    {
                        MessageBox.Show("Failed to load '" + file + "'. Reason: " + ex.Message);
                    }
                    catch (LuaException ex)
                    {
                        MessageBox.Show("Failed to load '" + file + "'. Reason: " + ex.Message);
                    }
                }
            }
        }

        private void btnOpen_IQFile(object sender, EventArgs e)
        {
            Source = new FileSourceDeviceControl(InternalOversampling);

            if (!Source.Connected)
            {
                return;
            }
            txtLog.Clear();
            ThreadActive = true;
            ReadThread = new Thread(SampleReadFunc);
            ReadThread.Start();

            btnOpen.Text = "Close";
            SetDataSource("IQ-File");
        }

        public void btnOpen_NetworkSource(object sender, EventArgs e)
        {
            Source = new NetworkDeviceControl();

            txtLog.Clear();
            ThreadActive = true;
            ReadThread = new Thread(SampleReadFunc);
            ReadThread.Start();

            btnOpen.Text = "Close";
            SetDataSource("NetworkSource");
        }

        public void btnOpen_OsmoconBitstream(object sender, EventArgs e)
        {
        }

        public void OpenSharedMem(int srcChan)
        {
            Source = new SharedMemDeviceControl(srcChan);

            txtLog.Clear();
            ThreadActive = true;
            ReadThread = new Thread(SampleReadFunc);
            ReadThread.Start();

            btnOpen.Text = "Close";
            SetDataSource("Shared Memory channel " + srcChan);
        }

        private void SetDataSource(string source)
        {
            if (source != null && source != "")
            {
                Text = "GSM Analyzer - [Input: " + source + "]";
            }
            else
            {
                Text = "GSM Analyzer";
            }
        }

        private MenuItem btnOpen_SharedMemoryCreateMenuItem(string name, int srcChan)
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

        private void btnOpen_DumpFileMulti(object sender, EventArgs e)
        {
            int fileNum = 1;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "GSM Analyzer Dump Files (*.gad)|*.gad|All files (*.*)|*.*";
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ThreadActive = true;
                ReadThread = new Thread(() =>
                {
                    foreach (string file in dlg.FileNames)
                    {
                        bool done = false;
                        BeginInvoke(new Action(() =>
                        {
                            txtLog.Clear();
                            SetDataSource("Dump File '" + file + "' (" + fileNum + "/" + dlg.FileNames .Length+ ")");
                            done = true;
                        }));

                        while (!done)
                        {
                            Thread.Sleep(100);
                        }
                        DumpReadFunc(file);

                        /* save messages */
                        try
                        {
                            done = false;

                            BeginInvoke(new Action(() =>
                            {
                                FileStream stream = File.Open(file + ".txt", FileMode.OpenOrCreate, FileAccess.Write);
                                TextWriter writer = new StreamWriter(stream);

                                string messages = "";
                                messages = txtLog.Text;
                                writer.WriteLine(messages);
                                writer.Close();

                                done = true;
                            }));

                            while (!done)
                            {
                                Thread.Sleep(100);
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                        fileNum++;
                    }

                    BeginInvoke(new Action(() => { CloseSource(); }));
                });
                ReadThread.Start();

                btnOpen.Text = "Close";
            }
        }

        private void btnOpen_DumpFile(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "GSM Analyzer Dump Files (*.gad)|*.gad|All files (*.*)|*.*";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtLog.Clear();
                ThreadActive = true;
                ReadThread = new Thread(() =>
                {
                    DumpReadFunc(dlg.FileName);
                    BeginInvoke(new Action(() => { CloseSource(); }));
                });
                ReadThread.Start();

                btnOpen.Text = "Close";
                SetDataSource("Dump File '" + dlg.FileName + "'");
            }
        }

        private void btnOpen_SharedMemory(object sender, EventArgs e)
        {
            ContextMenu menu = new ContextMenu();
            NodeInfo[] infos = SharedMem.GetNodeInfos();

            foreach (NodeInfo info in infos)
            {
                MenuItem item = btnOpen_SharedMemoryCreateMenuItem(info.name, info.dstChan);
                menu.MenuItems.Add(item);
            }

            if (infos.Length == 0)
            {
                MenuItem item = new MenuItem("(No nodes found)");
                item.Enabled = false;
                menu.MenuItems.Add(item);
            }

            menu.Show(this, new Point(10, 10));
        }

        void SampleReadFunc()
        {
            FCCHFinder normalFinder = new FCCHFinder(Oversampling);
            FCCHFinder invertedFinder = new FCCHFinder(Oversampling);

            long frameStartPosition = 0;
            long currentPosition = 0;
            int displayedChannel = 0;

            double oldSamplingRate = Source.SamplingRate;


            long burstSamples = (long)Math.Ceiling(Burst.TotalBitCount * Oversampling);
            double burstSamplesAccurate = Burst.TotalBitCount * Oversampling;
            double deltaSamplesPerBurst = burstSamples - burstSamplesAccurate;
            double skipSampleEvery = 1 / deltaSamplesPerBurst;
            int sampleDelta = 0;


            double burstCount = 0;
            long burstBufferPos = 0;

            /* for every channel a set of buffers */
            int channels = Splitter.Config.Channels.Length;
            double[][] burstBuffer = new double[channels][];
            double[][] burstStrengthBuffer = new double[channels][];
            double[][] sourceSignal = new double[channels][];
            double[][] sourceStrength = new double[channels][];

            for (int chan = 0; chan < channels; chan++)
            {
                burstBuffer[chan] = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];
                burstStrengthBuffer[chan] = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];
                sourceSignal[chan] = new double[Source.SampleSource.OutputBlockSize];
                sourceStrength[chan] = new double[Source.SampleSource.OutputBlockSize];
            }

            /* update sampling rate in spectrum window */
            lock (SpectrumWindowLock)
            {
                if (SpectrumWindow != null)
                    SpectrumWindow.SamplingRate = Source.SamplingRate;
            }

            /* increase shared mem read timeout */
            if (Source is ShmemSampleSource)
            {
                ((ShmemSampleSource)Source).ShmemChannel.ReadTimeout = 1000;
            }

            try
            {
                while (ThreadActive)
                {
                    /* check if sample source was not able to read a whole block */
                    if (!Source.ReadBlock() || Source.SampleSource.SamplesRead != Source.SampleSource.OutputBlockSize)
                    {
                        if (Source.SampleSource.SamplesRead != Source.SampleSource.OutputBlockSize && Source.SampleSource.SamplesRead != 0)
                        {
                            AddMessage("-------------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine);
                            AddMessage("  Important: Input buffer inconsistency. Your computer might be too slow. Please close some applications and/or visualizations" + Environment.NewLine);
                            AddMessage("-------------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine);
                            AddMessage(Environment.NewLine);
                            Source.SampleSource.Flush();
                        }
                        Thread.Sleep(50);
                    }
                    else
                    {
                        if (Source.SampleSource.BufferOverrun)
                        {
                            AddMessage("------------------------------------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine);
                            AddMessage("  Important: Input buffer overrun. Your computer might be too slow. Please close some applications and/or visualizations or reduce sampling rate" + Environment.NewLine);
                            AddMessage("------------------------------------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine);
                            AddMessage(Environment.NewLine);
                            Source.SampleSource.Flush();
                        }

                        /* handle external rate change */
                        if (Source.SampleSource.SamplingRateHasChanged)
                        {
                            Source.SampleSource.SamplingRateHasChanged = false;

                            /* update channel splitter */
                            Splitter.Config.SamplingRate = Source.SamplingRate;
                            Splitter.UpdateConfig();

                            if (Oversampling > 1)
                            {
                                AddMessage("[GSM] Sampling Rate changed from " + oldSamplingRate + " to " + Source.SamplingRate + ", Oversampling factor: " + Oversampling + Environment.NewLine);
                                oldSamplingRate = Source.SamplingRate;
                                normalFinder = new FCCHFinder(Oversampling);
                                invertedFinder = new FCCHFinder(Oversampling);

                                InitTimeSlotHandler();

                                burstSamples = (long)Math.Ceiling(Burst.TotalBitCount * Oversampling);
                                burstSamplesAccurate = Burst.TotalBitCount * Oversampling;
                                deltaSamplesPerBurst = burstSamples - burstSamplesAccurate;
                                skipSampleEvery = 1 / deltaSamplesPerBurst;
                                burstCount = 0;
                                sampleDelta = 0;

                                for (int chan = 0; chan < channels; chan++)
                                {
                                    burstBuffer[chan] = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];
                                    burstStrengthBuffer[chan] = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];

                                    sourceSignal[chan] = new double[Source.SampleSource.OutputBlockSize];
                                    sourceStrength[chan] = new double[Source.SampleSource.OutputBlockSize];
                                }

                                Parameters.Reset();
                                Parameters.Oversampling = Oversampling;
                                Parameters.BT = BT;
                                Parameters.SampleStartPosition = Oversampling * Handler.SpareBits;

                                InitTimeSlotHandler();
                                UpdateUIStatus(Parameters);

                                lock (SpectrumWindowLock)
                                {
                                    if (SpectrumWindow != null)
                                        SpectrumWindow.SamplingRate = Source.SamplingRate;
                                }
                            }
                        }

                        /* split signal into channels */
                        Splitter.ProcessData(Source.SampleSource.SourceSamplesI, Source.SampleSource.SourceSamplesQ);

                        /* demodulate every channel */
                        for (int num = 0; num < channels; num++)
                        {
                            ChannelSplitter.ChannelConfig chan = Splitter.Config.Channels[num];

                            Demodulator[num].ProcessData(chan.SampleBufferI, chan.SampleBufferQ, sourceSignal[num], sourceStrength[num]);
                        }

                        /* go through every sample */
                        for (int pos = 0; pos < Source.SampleSource.OutputBlockSize; pos++)
                        {
                            double signal = sourceSignal[0][pos] + Parameters.PhaseOffsetValue;
                            double strength = sourceStrength[0][pos];

                            bool burstSampled = false;

                            /* write this sample into the burst buffer */
                            for (int chan = 0; chan < channels; chan++)
                            {
                                if (burstBufferPos < burstBuffer[chan].Length && burstBufferPos > 0)
                                {
                                    burstBuffer[chan][burstBufferPos] = sourceSignal[chan][pos] + Parameters.PhaseOffsetValue;
                                    burstStrengthBuffer[chan][burstBufferPos] = sourceStrength[chan][pos];
                                }
                            }
                            burstBufferPos++;

                            /* when we should skip one sample, decrease sampleDelta */
                            if (burstCount >= skipSampleEvery)
                            {
                                sampleDelta -= 1;
                                burstCount -= skipSampleEvery;
                            }

                            /* have enough samples for one burst? */
                            if (burstBufferPos >= (burstSamples + sampleDelta))
                            {
                                /* reset the delta. it will get set later again */
                                sampleDelta = 0;
                                burstSampled = true;
                            }

                            /* feed every sample to FFT window */
                            lock (SpectrumWindowLock)
                            {
                                if (SpectrumWindow != null)
                                {
                                    lock (Source.SampleSource.SampleBufferLock)
                                    {
                                        SpectrumWindow.ProcessIQSample(Splitter.Config.Channels[displayedChannel].SampleBufferI[pos], Splitter.Config.Channels[displayedChannel].SampleBufferQ[pos]);
                                    }

                                    if (!SpectrumWindow.Visible)
                                    {
                                        SpectrumWindow = null;
                                    }
                                }
                            }

                            switch (Parameters.State)
                            {
                                case eGSMState.Idle:
                                    break;

                                case eGSMState.Reset:
                                    AddMessage("[GSM] Reset" + Environment.NewLine);

                                    L3Handler.ReloadFiles();
                                    currentPosition = 0;
                                    normalFinder.Reset();
                                    invertedFinder.Reset();

                                    Parameters.Reset();
                                    Parameters.ResetError();
                                    Parameters.State = eGSMState.FCCHSearch;

                                    InitTimeSlotHandler();
                                    ResetStats();
                                    UpdateUIStatus(Parameters);
                                    break;

                                case eGSMState.FCCHSearch:

                                    /* let the FCCH finders search the FCCH burst */
                                    try
                                    {
                                        bool normalFound = normalFinder.ProcessData(signal, strength);
                                        bool invertedFound = invertedFinder.ProcessData(-signal, strength);

                                        if (normalFound | invertedFound)
                                        {
                                            Parameters.State = eGSMState.SCHSearch;
                                            UpdateUIStatus(Parameters);

                                            /* if the inverted FCCH finder found one, invert spectrum */
                                            if (invertedFound)
                                            {
                                                AddMessage("[GSM] FCCH found (inverting spectrum)" + Environment.NewLine);
                                                InvertedSpectrum ^= true;

                                                Parameters.CurrentPower = invertedFinder.AveragePower;
                                                /* save the position where the frame started */
                                                frameStartPosition = invertedFinder.BurstStartPosition;
                                            }
                                            else
                                            {
                                                AddMessage("[GSM] FCCH found" + Environment.NewLine);

                                                Parameters.CurrentPower = invertedFinder.AveragePower;
                                                /* save the position where the frame started */
                                                frameStartPosition = normalFinder.BurstStartPosition;
                                            }

                                            frameStartPosition -= (long)(Oversampling * Handler.SpareBits);

                                            /* update the burst buffer pointer */
                                            burstBufferPos = currentPosition - frameStartPosition;

                                            /* this is TN 0 */
                                            Parameters.FN = 0;
                                            Parameters.TN = 0;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        AddMessage("[GSM] FCCH Exception: " + e + Environment.NewLine);
                                        return;
                                    }

                                    /* if enough samples processed, update burst window */
                                    if (burstSampled)
                                    {
                                        burstBufferPos = 0;

                                        lock (BurstWindowLock)
                                        {
                                            /* update the burst visualizer */
                                            if (BurstWindow != null)
                                            {
                                                BurstWindow.XAxisGridOffset = 0;
                                                BurstWindow.ProcessBurst(burstBuffer[displayedChannel], burstStrengthBuffer[displayedChannel]);
                                            }
                                        }

                                        if (SingleStep)
                                            SingleStepSem.WaitOne();
                                    }
                                    break;


                                case eGSMState.SCHSearch:
                                    /* if one burst was sampled */
                                    if (burstSampled)
                                    {
                                        /* if this is the eighth burst, its the first timeslot of the next frame - SCH */
                                        if (Parameters.TN == 8)
                                        {
                                            /* set TN to 7, since handler will increase */
                                            Parameters.TN = 7;
                                            Parameters.FirstSCH = true;

                                            /* let the handler process this packet */
                                            Parameters.SampleOffset = 0;
                                            Parameters.SubSampleOffset = 0;
                                            Handler.Handle(burstBuffer[0], burstStrengthBuffer[0]);

                                            if (Parameters.Errors > 0)
                                            {
                                                AddMessage("[GSM] SCH failed -> Reset" + Environment.NewLine);
                                                Parameters.State = eGSMState.Reset;
                                                UpdateUIStatus(Parameters);
                                            }
                                            else
                                            {
                                                AddMessage("[GSM] SCH found, locked" + Environment.NewLine);
                                                Parameters.State = eGSMState.Lock;
                                                UpdateUIStatus(Parameters);
                                            }
                                        }
                                        else
                                        {
                                            Parameters.TN++;
                                        }


                                        burstBufferPos = 0;

                                        lock (BurstWindowLock)
                                        {
                                            /* update the burst visualizer */
                                            if (BurstWindow != null)
                                            {
                                                BurstWindow.XAxisGridOffset = Parameters.SampleOffset;
                                                BurstWindow.ProcessBurst(burstBuffer[displayedChannel], burstStrengthBuffer[displayedChannel]);
                                            }
                                        }

                                        if (SingleStep)
                                            SingleStepSem.WaitOne();
                                    }
                                    break;

                                case eGSMState.Lock:

                                    /* when we are already in frame sync and one burst was sampled */
                                    if (burstSampled)
                                    {
                                        if (Subsampling)
                                        {
                                            /* start offset estimation at the 5th bit transition */
                                            int startPos = (int)(Parameters.SampleOffset + 5.5f * Oversampling);
                                            int samples = (int)((Burst.NetBitCount - 5) * Oversampling);

                                            Parameters.SubSampleOffset = OffsetEstimator.EstimateOffset(burstBuffer[0], startPos, samples, Oversampling, Oversampling / 2);
                                        }
                                        else
                                        {
                                            Parameters.SubSampleOffset = 0;
                                        }

                                        /* add constant defined by user */
                                        Parameters.SubSampleOffset += SubSampleOffset;

                                        //Statistics.Add(Parameters.SubSampleOffset);

                                        lock (BurstWindowLock)
                                        {
                                            if (BurstWindow != null)
                                            {
                                                BurstWindow.SampleDisplay.DirectXLock.WaitOne();
                                                BurstWindow.XAxisGridOffset = ((int)(Oversampling * Handler.SpareBits) + Oversampling / 2);
                                                BurstWindow.XAxisSampleOffset = -Parameters.SubSampleOffset;
                                                BurstWindow.Oversampling = Oversampling;
                                                BurstWindow.SampleDisplay.UpdateAxis = true;
                                                BurstWindow.SampleDisplay.DirectXLock.ReleaseMutex();
                                                BurstWindow.ProcessBurst(burstBuffer[displayedChannel], burstStrengthBuffer[displayedChannel]);

                                                if (!BurstWindow.Visible)
                                                    BurstWindow = null;
                                            }
                                        }

                                        Handler.Handle(burstBuffer[0], burstStrengthBuffer[0]);

                                        lock (BurstWindowLock)
                                        {
                                            if (BurstWindow != null)
                                            {
                                                BurstWindow.SampleDisplay.DirectXLock.WaitOne();
                                                BurstWindow.SampleDisplay.DecisionPowerLine.Position = Handler.Decoder.DecisionPower;
                                                BurstWindow.BurstBits = Handler.BurstBits;
                                                BurstWindow.SampleDisplay.UpdateAxis = true;
                                                BurstWindow.SampleDisplay.UpdateOverlays = true;
                                                BurstWindow.SampleDisplay.DirectXLock.ReleaseMutex();
                                            }
                                        }

                                        if (Parameters.ErrorLimit)
                                        {
                                            AddMessage("[GSM] Packet handling failed -> Reset" + Environment.NewLine);
                                            Parameters.State = eGSMState.Reset;
                                            UpdateUIStatus(Parameters);
                                        }

                                        /* 
                                         * tricky! the BTS sends the bursts with 156 bits instead of 156.25
                                         * but it delays one bit after 4 bursts. compensate this here.
                                         * we do that for the next timeslot
                                         */
                                        long burstNumber = ((Parameters.TN + 1) % 4);
                                        sampleDelta += (int)(BurstLengthJitter[burstNumber] * Oversampling);

                                        /* update counters and apply offset correction */
                                        burstCount++;

                                        /* the next buffer destination depends on the sample offset we have */
                                        burstBufferPos = (long)-(Parameters.SampleOffset + Parameters.SubSampleOffset);
                                        Parameters.SampleOffset = 0;
                                        Parameters.SubSampleOffset = 0;

                                        /* update UI if necessary */
                                        if (SingleStep || (DateTime.Now - LastUiUpdate).TotalMilliseconds > UiUpdateTime)
                                        {
                                            LastUiUpdate = DateTime.Now;

                                            UpdateUIStatus(Parameters);
                                            UpdateStats(Parameters);
                                        }

                                        if (SingleStep)
                                            SingleStepSem.WaitOne();
                                    }
                                    break;
                            }
                            currentPosition++;
                        }
                    }
                }
            }
            catch (ThreadAbortException e)
            {

            }
            catch (SystemException e)
            {
                MessageBox.Show("There was an unhandled SystemException." + Environment.NewLine + Environment.NewLine + "Exception:" + Environment.NewLine + e);
                AddMessage("   [GSM] SystemException: " + e + Environment.NewLine);
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show("There was an unhandled Exception." + Environment.NewLine + Environment.NewLine + "Exception:" + Environment.NewLine + e);
                AddMessage("   [GSM] Exception: " + e + Environment.NewLine);
                return;
            }

            Parameters.State = eGSMState.Idle;

            /* show statistics/information */
            DumpStatistics();
            UpdateUIStatus(Parameters);
        }

        void DumpReadFunc(string fileName)
        {
            bool[] burstBits = new bool[148];
            GsmAnalyzerDumpReader reader = new GsmAnalyzerDumpReader(Parameters, fileName);

            L3Handler.ReloadFiles();

            Parameters.Reset();
            Parameters.ResetError();
            Parameters.State = eGSMState.Lock;
            Parameters.AverageIdlePower = 0;
            Parameters.AveragePower = 0;
            Parameters.CurrentPower = 0;

            InitTimeSlotHandler();
            ResetStats();
            UpdateUIStatus(Parameters);

            try
            {
                while (reader.HasData)
                {
                    /* get the next burst from the reader */
                    reader.Read(burstBits);

                    /* let timeslot handler process the burst bits. 
                     * passing the burst number to handler so its able to display the burst number.
                     * used to track e.g. faulty bursts in source file 
                     */
                    Handler.Handle(burstBits, reader.BurstNumber);

                    /* update UI if necessary */
                    if (SingleStep || (DateTime.Now - LastUiUpdate).TotalMilliseconds > UiUpdateTime)
                    {
                        LastUiUpdate = DateTime.Now;

                        UpdateUIStatus(Parameters);
                        UpdateStats(Parameters);
                    }

                    if (SingleStep)
                        SingleStepSem.WaitOne();
                }
            }
            catch (ThreadAbortException e)
            {
            }
            catch (SystemException e)
            {
                MessageBox.Show("There was an unhandled SystemException." + Environment.NewLine + Environment.NewLine + "Exception:" + Environment.NewLine + e);
                AddMessage("   [GSM] SystemException: " + e + Environment.NewLine);
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show("There was an unhandled Exception." + Environment.NewLine + Environment.NewLine + "Exception:" + Environment.NewLine + e);
                AddMessage("   [GSM] Exception: " + e + Environment.NewLine);
                return;
            }

            Parameters.State = eGSMState.Idle;

            reader.Close();

            /* show statistics/information */
            DumpStatistics();
            UpdateUIStatus(Parameters);
        }

        private void DumpStatistics()
        {
            StringBuilder stats = new StringBuilder();

            stats.Append(Environment.NewLine);
            stats.Append(Parameters.GetSlotUsage());

            stats.Append(Environment.NewLine);
            stats.Append(Parameters.GetTimeslotDetails());

            AddMessage(stats.ToString());
        }

        private void InitTimeSlotHandler()
        {
            Handler = new TimeSlotHandler(Parameters, AddMessage);
            RegisterTriggers(Handler.L3);
            if (Handler.L3.StatusMessage != null)
                MessageBox.Show("   [L3] " + Handler.L3.StatusMessage);
        }

        private void chkSingleStep_CheckedChanged(object sender, EventArgs e)
        {
            SingleStep = chkSingleStep.Checked;
        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            try
            {
                SingleStepSem.Release(1);
            }
            catch (SemaphoreFullException ex)
            {
                /* someone clicked too fast? :) */
            }
        }

        void txtArfcn_ValueChanged(object sender, System.EventArgs e)
        {
            long chan = txtArfcn.Value;

            if (ChannelHandler.HasChannel(chan))
            {
                txtArfcn.BackColor = Color.White;
                ChannelHandler.Channel = txtArfcn.Value;
                Source.SampleSource.Flush();
                Parameters.ARFCN = txtArfcn.Value;
                Parameters.State = eGSMState.Reset;
            }
            else
            {
                if (chan < ChannelHandler.LowestChannel)
                {
                    txtArfcn.Value = ChannelHandler.LowestChannel;
                }
                else if (chan > ChannelHandler.HighestChannel)
                {
                    txtArfcn.Value = ChannelHandler.HighestChannel;
                }
                else
                {
                    txtArfcn.BackColor = Color.Red;
                }
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (ChannelScanThread == null)
            {
                if (Device != null && Source != null)
                {
                    StationDialog = new StationListDialog(ChannelHandler);
                    StationDialog.Show();

                    ChannelScanThread = new Thread(ChannelScanThreadMain);
                    ChannelScanThread.Start();
                    btnScan.Text = "Stop";
                }
            }
            else
            {
                ChannelScanThread.Abort();
                ChannelScanThread = null;
                btnScan.Text = "Scan";
            }
        }

        private void ChannelScanThreadMain()
        {
            ChannelHandler.Channel = ChannelHandler.LowestChannel;

            while (true)
            {
                Source.SampleSource.Flush();
                Parameters.State = eGSMState.Reset;

                /* wait until reset done */
                int waitForSearch = 10;
                while ((Parameters.State == eGSMState.Reset) && (--waitForSearch > 0))
                {
                    Thread.Sleep(100);
                }

                /* make sure buffers are empty and wait 250ms */
                Source.SampleSource.Flush();
                Thread.Sleep(250);

                /* wait for FCCH searching */
                int waitForLock = 10;
                while ((Parameters.State == eGSMState.FCCHSearch) && (--waitForLock > 0))
                {
                    Thread.Sleep(100);
                }

                /* when FCCH found, wait until CBCH found */
                if (Parameters.State != eGSMState.FCCHSearch)
                {
                    int waitForDetails = 20;
                    bool detailsfound = false;

                    while (!detailsfound && --waitForDetails > 0)
                    {
                        Thread.Sleep(100);

                        /* exit loop if all details were received */
                        detailsfound = true;
                        detailsfound &= Parameters.CBCH != eTriState.Unknown;
                        detailsfound &= Handler.L3.PDUDataFields.ContainsKey("MCC/MNC");
                        detailsfound &= Handler.L3.PDUDataFields.ContainsKey("LAC");
                        detailsfound &= Handler.L3.PDUDataFields.ContainsKey("CellIdent");
                    }

                    /* check if any detail was found */
                    detailsfound = false;
                    detailsfound |= Parameters.CBCH != eTriState.Unknown;
                    detailsfound |= Handler.L3.PDUDataFields.ContainsKey("MCC/MNC");
                    detailsfound |= Handler.L3.PDUDataFields.ContainsKey("LAC");
                    detailsfound |= Handler.L3.PDUDataFields.ContainsKey("CellIdent");

                    /* if so, channel is used */
                    if (detailsfound)
                    {
                        string mccMncString = "?";
                        string lacString = "?";
                        string cellIdentString = "?";

                        if (Handler.L3.PDUDataFields.ContainsKey("MCC/MNC"))
                            mccMncString = Handler.L3.PDUDataFields["MCC/MNC"];

                        if (Handler.L3.PDUDataFields.ContainsKey("LAC"))
                            lacString = Handler.L3.PDUDataFields["LAC"];

                        if (Handler.L3.PDUDataFields.ContainsKey("CellIdent"))
                            cellIdentString = Handler.L3.PDUDataFields["CellIdent"];

                        StationDialog.AddStation(ChannelHandler.Channel, "0x" + Parameters.BSIC.ToString("X2"), mccMncString, lacString, cellIdentString, Parameters.CBCH.ToString(), DBTools.SampleTodB(Parameters.AveragePower).ToString("0.00"));

                        Log.AddMessage("Channel " + ChannelHandler.Channel + " used.");
                        Log.AddMessage("  MCC/MNC  : " + mccMncString);
                        Log.AddMessage("  LAC      : " + lacString);
                        Log.AddMessage("  CellIdent: " + cellIdentString);
                        Log.AddMessage("  CBCH     : " + Parameters.CBCH);
                        Log.AddMessage("  Power    : " + DBTools.SampleTodB(Parameters.AveragePower));
                        Log.AddMessage("  Idle Pwr : " + DBTools.SampleTodB(Parameters.AverageIdlePower));
                        Log.AddMessage("  SNR      : " + (DBTools.SampleTodB(Parameters.AveragePower) - DBTools.SampleTodB(Parameters.AverageIdlePower)));
                    }
                }

                if (ChannelHandler.Channel == ChannelHandler.HighestChannel)
                {
                    Log.AddMessage("Scan finished");
                    BeginInvoke(new MethodInvoker(() => btnScan.Text = "Scan"));
                    return;
                }

                ChannelHandler.Channel = ChannelHandler.NextChannel;
                BeginInvoke(new MethodInvoker(() => txtArfcn.Value = ChannelHandler.Channel));
            }
        }

        private void btnDumpStatistics_Click(object sender, EventArgs e)
        {
            DumpStatistics();
        }

        private void btnToggleBurst_Click(object sender, EventArgs e)
        {
            lock (BurstWindowLock)
            {
                if (BurstWindow == null || !BurstWindow.Visible)
                {
                    try
                    {
                        BurstWindow = new BurstVisualizer(Oversampling);
                        BurstWindow.Show();
                    }
                    catch (Exception ex)
                    {
                        AddMessage("Exception while initializing Burst Window:" + Environment.NewLine + ex.ToString());
                    }
                }
                else
                {
                    BurstWindow.Close();
                    BurstWindow = null;
                }
            }
        }

        private void btnToggleSpectrum_Click(object sender, EventArgs e)
        {
            lock (SpectrumWindowLock)
            {
                if (SpectrumWindow == null || !SpectrumWindow.Visible)
                {
                    try
                    {
                        SpectrumWindow = new SpectrumVisualizer();
                        SpectrumWindow.Show();
                        if (Source != null)
                            SpectrumWindow.SamplingRate = Source.SamplingRate;
                    }
                    catch (Exception ex)
                    {
                        AddMessage("Exception while initializing Spectrum Window:" + Environment.NewLine + ex.ToString());
                    }
                }
                else
                {
                    SpectrumWindow.Close();
                    SpectrumWindow = null;
                }
            }
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {

            if (OptionsWindow == null || !OptionsWindow.Visible)
            {
                OptionsWindow = new OptionsDialog(this);
                OptionsWindow.Show();
            }
            else
            {
                OptionsWindow.Close();
                OptionsWindow = null;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        private void btnL3Filter_Click(object sender, EventArgs e)
        {
            if (FilterWindow == null || !FilterWindow.Visible)
            {
                FilterWindow = new FilterDialog();
                FilterWindow.Show();
            }
            else
            {
                FilterWindow.Close();
                FilterWindow = null;
            }
        }

        private void btnDump_Click(object sender, EventArgs e)
        {
            btnDump.CheckState = CheckState.Unchecked;

            if (Parameters.PacketDumper != null)
            {
                Parameters.PacketDumper.Close();
                Parameters.PacketDumper = null;
            }
            else
            {
                ContextMenu contextMenu = new ContextMenu();
                MenuItem menuItem;

                menuItem = new MenuItem("Dump to XML...");
                menuItem.Click += (object s, EventArgs ev) =>
                {
                    FileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "GSM Analyzer Dumps (*.gad)|*.gad|All files (*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            File.Delete(dlg.FileName);
                            Parameters.PacketDumper = new GsmAnalyzerDumpWriter(Parameters, dlg.FileName);
                            btnDump.CheckState = CheckState.Checked;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Could not create dump file: " + ex.GetType().ToString());
                        }
                    }
                };
                contextMenu.MenuItems.Add(menuItem);

                menuItem = new MenuItem("Dump via GSMTAP");
                contextMenu.MenuItems.Add(menuItem);
                menuItem.Click += (object s, EventArgs ev) =>
                {
                    Parameters.PacketDumper = new GsmTapWriter(Parameters);
                    btnDump.CheckState = CheckState.Checked;
                };

                menuItem = new MenuItem("Forward to Master...");
                menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);

                menuItem = new MenuItem("-");
                menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);

                menuItem = new MenuItem("Skip L2 processing during dump");
                menuItem.Checked = Parameters.SkipL2Parsing;
                menuItem.Click += (object s, EventArgs ev) => { Parameters.SkipL2Parsing ^= true; };
                contextMenu.MenuItems.Add(menuItem);

                Point popupPos = this.PointToClient(MousePosition);

                popupPos.X -= 20;
                popupPos.Y -= 20;

                contextMenu.Show(this, popupPos);
            }
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chkSlotUsage_CheckedChanged(object sender, EventArgs e)
        {
            ShowSlotUsage = chkSlotUsage.Checked;

            slotUsageControl.Visible = ShowSlotUsage;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            string text = txtLog.Text;

            Clipboard.SetDataObject(text, true);
        }
    }
}
