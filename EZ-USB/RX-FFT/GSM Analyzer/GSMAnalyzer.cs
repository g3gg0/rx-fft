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

namespace GSM_Analyzer
{
    public partial class GSMAnalyzer : Form
    {
        public SampleSource Source;
        private Tuner _Device;
        public Tuner Device
        {
            get { return _Device; }
            set { 
                _Device = value;
                ChannelHandler.Device = value; 
            }
        }
        private RadioChannelHandler ChannelHandler;
        private Thread ChannelScanThread;

        private Thread ReadThread;
        private bool ThreadActive;

        public double[] BurstLengthJitter = new[] { 0.75d, -0.25d, -0.25d, -0.25d };

        private Semaphore SingleStepSem = new Semaphore(0, 1, "SingleStepSemaphore");
        private bool SingleStep;

        internal TimeSlotHandler Handler;
        internal GSMParameters Parameters;
        internal GMSKDemodulator Demodulator;
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

        public bool Subsampling = true;
        public int InternalOversampling = 1;
        public double SubSampleOffset = 0;

        internal double DefaultSamplingRate = 2184533;
        private double BT = 0.3d;


        internal double CurrentSampleRate
        {
            get
            {
                if (Source != null)
                    return Source.OutputSamplingRate;

                return DefaultSamplingRate;
            }
        }

        internal double Oversampling
        {
            get
            {
                return CurrentSampleRate / 270833;
            }
        }


        public GSMAnalyzer()
        {
            InitializeComponent();
            Parameters = new GSMParameters();
            Demodulator = new GMSKDemodulator();

            Log.Init();
            ChannelHandler = new RadioChannelHandler(Device);
            ChannelHandler.FrequencyOffset = 0;

            /* from: http://de.wikipedia.org/wiki/ARFCN */
            ChannelHandler.AddBand(new FrequencyBand("E-GSM a", 935000000, 200000, 0, 124));
            ChannelHandler.AddBand(new FrequencyBand("E-GSM b", 925200000, 200000, 975, 1023));
            ChannelHandler.AddBand(new FrequencyBand("T-GSM 900", 915600000, 200000, 1024, 1052));
            ChannelHandler.AddBand(new FrequencyBand("DCS 1800", 1805200000, 200000, 512, 885));

            txtArfcn.Value = ChannelHandler.LowestChannel;


            /* already init here to load XML files */
            InitTimeSlotHandler();

            TextBoxCommitTimer.Tick += new EventHandler(TextBoxCommitTimer_Tick);
            TextBoxCommitTimer.Interval = 100;

            /*
            double lg;
            double bg;
            GaussKrueger.ConvertToLatLong(3634110, 5391840, out lg, out bg);
            txtLog.Text += (" " + bg + ", " + lg + Environment.NewLine);
            GaussKrueger.HelmertTransformation(lg, bg, out lg, out bg);
            txtLog.Text += (" " + bg + ", " + lg + Environment.NewLine);
            */
        }

        void TextBoxCommitTimer_Tick(object sender, EventArgs e)
        {
            /* just call the AddMessage routine to commit the text in buffers */
            AddMessage("");
        }

        public void RegisterTriggers(L3Handler L3Handler)
        {
            L3Handler.PDUDataTriggers.Add("LAIUpdate", TriggerLAIUpdate);
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
            lblPower.Text = DBTools.SampleTodB(averagePower).ToString("#0.0 dB");
            lblIdlePower.Text = DBTools.SampleTodB(averageIdlePower).ToString("#0.0 dB");
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

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (Source != null)
            {
                ThreadActive = false;

                if (!ReadThread.Join(1000))
                    ReadThread.Abort();

                if (ChannelScanThread != null)
                {
                    ChannelScanThread.Abort();
                    ChannelScanThread = null;
                    btnScan.Text = "Scan";
                }

                Source.Close();
                Source = null;

                btnOpen.Text = "Open";
            }
            else
            {
                ContextMenu menu = new ContextMenu();

                menu.MenuItems.Add(new MenuItem("Shared Memory", new EventHandler(btnOpen_SharedMemory)));
                menu.MenuItems.Add(new MenuItem("USRP CFile", new EventHandler(btnOpen_CFile)));
                btnOpen.ContextMenu = menu;
                btnOpen.ContextMenu.Show(btnOpen, new Point(10, 10));
            }
        }

        private void btnOpen_CFile(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "USRP CFiles (*.cfile)|*.cfile|All files (*.*)|*.*";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Source = new USRPSampleSource(dlg.FileName, InternalOversampling);

                txtLog.Clear();
                ThreadActive = true;
                ReadThread = new Thread(SampleReadFunc);
                ReadThread.Start();

                btnOpen.Text = "Close";
            }
        }

        public void OpenSharedMem(int srcChan)
        {
            Source = new ShmemSampleSource("GSM Analyzer", srcChan, InternalOversampling, DefaultSamplingRate);

            txtLog.Clear();
            ThreadActive = true;
            ReadThread = new Thread(SampleReadFunc);
            ReadThread.Start();

            btnOpen.Text = "Close";
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

            btnOpen.ContextMenu = menu;
            btnOpen.ContextMenu.Show(btnOpen, new Point(10, 10));
        }

        void SampleReadFunc()
        {
            FCCHFinder finder = new FCCHFinder(Oversampling);

            long frameStartPosition = 0;
            long currentPosition = 0;
            long updateLoops = 0;

            double oldSamplingRate = Source.OutputSamplingRate;

            double[] burstBuffer = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];
            double[] burstStrengthBuffer = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];

            long burstSamples = (long)Math.Ceiling(Burst.TotalBitCount * Oversampling);
            double burstSamplesAccurate = Burst.TotalBitCount * Oversampling;
            double deltaSamplesPerBurst = burstSamples - burstSamplesAccurate;
            double skipSampleEvery = 1 / deltaSamplesPerBurst;
            int sampleDelta = 0;


            double burstCount = 0;
            long burstBufferPos = 0;
            long lastSampleOffset = 0;

            double[] sourceSignal = new double[Source.OutputBlockSize];
            double[] sourceStrength = new double[Source.OutputBlockSize];

            /* update sampling rate in spectrum window */
            lock (SpectrumWindowLock)
            {
                if (SpectrumWindow != null)
                    SpectrumWindow.SamplingRate = Source.OutputSamplingRate;
            }

            try
            {
                while (ThreadActive)
                {
                    if (!Source.Read())
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        if(Source.BufferOverrun)
                        {
                            AddMessage("----------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine);
                            AddMessage("  Important: Input buffer overrun. Your computer might be too slow. Please close some applications and/or visualizations" + Environment.NewLine);
                            AddMessage("----------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine);
                            AddMessage(Environment.NewLine);
                            Source.Flush();
                        }
                        Demodulator.ProcessData(Source.SourceSamplesI, Source.SourceSamplesQ, sourceSignal, sourceStrength);

                        /* to allow external rate change */
                        if (Source.SamplingRateHasChanged)
                        {
                            Source.SamplingRateHasChanged = false;

                            if (Oversampling > 1)
                            {
                                AddMessage("[GSM] Sampling Rate changed from " + oldSamplingRate + " to " + Source.OutputSamplingRate + ", Oversampling factor: " + Oversampling + Environment.NewLine);
                                oldSamplingRate = Source.OutputSamplingRate;
                                finder = new FCCHFinder(Oversampling);

                                InitTimeSlotHandler();

                                burstBuffer = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];
                                burstStrengthBuffer = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];
                                burstSamples = (long)Math.Ceiling(Burst.TotalBitCount * Oversampling);
                                burstSamplesAccurate = Burst.TotalBitCount * Oversampling;
                                deltaSamplesPerBurst = burstSamples - burstSamplesAccurate;
                                skipSampleEvery = 1 / deltaSamplesPerBurst;
                                burstCount = 0;
                                sampleDelta = 0;

                                sourceSignal = new double[Source.OutputBlockSize];
                                sourceStrength = new double[Source.OutputBlockSize];

                                Parameters.Reset();
                                Parameters.Oversampling = Oversampling;
                                Parameters.BT = BT;
                                Parameters.SampleStartPosition = Oversampling * Handler.SpareBits;

                                InitTimeSlotHandler();
                                UpdateUIStatus(Parameters);

                                lock (SpectrumWindowLock)
                                {
                                    if (SpectrumWindow != null)
                                        SpectrumWindow.SamplingRate = Source.OutputSamplingRate;
                                }
                            }
                        }


                        for (int pos = 0; pos < Source.OutputBlockSize; pos++)
                        {
                            double signal = sourceSignal[pos] + Parameters.PhaseOffsetValue;
                            double strength = sourceStrength[pos];

                            bool burstSampled = false;

                            /* write this sample into the burst buffer */
                            if (burstBufferPos < burstBuffer.Length && burstBufferPos > 0)
                            {
                                burstBuffer[burstBufferPos] = signal;
                                burstStrengthBuffer[burstBufferPos] = strength;
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
                                    lock (Source.SampleBufferLock)
                                    {
                                        SpectrumWindow.ProcessIQSample(Source.SourceSamplesI[pos], Source.SourceSamplesQ[pos]);
                                    }
                                    if (!SpectrumWindow.Visible)
                                        SpectrumWindow = null;
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
                                    finder.Reset();

                                    Parameters.Reset();
                                    Parameters.ResetError();
                                    Parameters.State = eGSMState.FCCHSearch;

                                    InitTimeSlotHandler(); 
                                    ResetStats();
                                    UpdateUIStatus(Parameters);
                                    break;

                                case eGSMState.FCCHSearch:

                                    /* let the FCCH finder search the FCCH burst */
                                    try
                                    {
                                        if (finder.ProcessData(signal, strength))
                                        {
                                            Parameters.State = eGSMState.SCHSearch;
                                            UpdateUIStatus(Parameters);

                                            AddMessage("[GSM] FCCH found" + Environment.NewLine);

                                            /* save the position where the frame started */
                                            frameStartPosition = finder.BurstStartPosition;
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
                                                BurstWindow.ProcessBurst(burstBuffer, burstStrengthBuffer);
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
                                            Handler.Handle(burstBuffer, burstStrengthBuffer);

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
                                            Parameters.TN++;


                                        burstBufferPos = 0;

                                        lock (BurstWindowLock)
                                        {
                                            /* update the burst visualizer */
                                            if (BurstWindow != null)
                                            {
                                                BurstWindow.XAxisGridOffset = Parameters.SampleOffset;
                                                BurstWindow.ProcessBurst(burstBuffer, burstStrengthBuffer);
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
                                            /* start at the 5th bit transition */
                                            int startPos = (int)(Parameters.SampleOffset + 5.5f * Oversampling);
                                            int samples = (int)((Burst.NetBitCount - 5) * Oversampling);

                                            Parameters.SubSampleOffset = OffsetEstimator.EstimateOffset(burstBuffer, startPos, samples, Oversampling);
                                        }
                                        else
                                            Parameters.SubSampleOffset = 0;

                                        /* add constant defined by user */
                                        Parameters.SubSampleOffset += SubSampleOffset;

                                        //Statistics.Add(Parameters.SubSampleOffset);

                                        lock (BurstWindowLock)
                                        {
                                            if (BurstWindow != null)
                                            {
                                                BurstWindow.SampleDisplay.DirectXLock.WaitOne();
                                                BurstWindow.SampleDisplay.YAxisLines.Clear();
                                                BurstWindow.XAxisGridOffset = ((int)(Oversampling * Handler.SpareBits) + Oversampling / 2);
                                                BurstWindow.XAxisSampleOffset = -Parameters.SubSampleOffset;
                                                BurstWindow.Oversampling = Oversampling;
                                                BurstWindow.SampleDisplay.UpdateAxis = true;
                                                BurstWindow.SampleDisplay.DirectXLock.ReleaseMutex();
                                                BurstWindow.ProcessBurst(burstBuffer, burstStrengthBuffer);

                                                if (!BurstWindow.Visible)
                                                    BurstWindow = null;
                                            }
                                        }

                                        Handler.Handle(burstBuffer, burstStrengthBuffer);

                                        lock (BurstWindowLock)
                                        {
                                            if (BurstWindow != null)
                                            {
                                                BurstWindow.SampleDisplay.DirectXLock.WaitOne();
                                                BurstWindow.SampleDisplay.YAxisLines.Add(Handler.Decoder.MaxPower);
                                                BurstWindow.SampleDisplay.YAxisLines.Add(Handler.Decoder.DecisionPower);
                                                BurstWindow.BurstBits = Handler.BurstBits;
                                                BurstWindow.SampleDisplay.UpdateAxis = true;
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
                                        if (SingleStep || updateLoops++ >= 50 * 9)
                                        {
                                            UpdateUIStatus(Parameters);
                                            UpdateStats(Parameters);
                                            updateLoops = 0;
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


            /* show statistics/information */
            DumpStatistics();

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


        private void btnStats_Click(object sender, EventArgs e)
        {
            DumpStatistics();

            /*
            if (FilterWindow == null || !FilterWindow.Visible)
            {
                FilterWindow = new FilterDialog(Handler);
                FilterWindow.Show();
            }
            else
            {
                FilterWindow.Close();
                FilterWindow = null;
            }
             * */
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

        private void btnSpectrum_Click(object sender, EventArgs e)
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
                            SpectrumWindow.SamplingRate = Source.OutputSamplingRate;
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

        private void btnBurst_Click(object sender, EventArgs e)
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

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        void txtArfcn_ValueChanged(object sender, System.EventArgs e)
        {
            long chan = txtArfcn.Value;

            if (ChannelHandler.HasChannel(chan))
            {
                txtArfcn.BackColor = Color.White;
                ChannelHandler.Channel = txtArfcn.Value;
                Source.Flush();
                Parameters.ARFCN = txtArfcn.Value;
                Parameters.State = eGSMState.Reset;
            }
            else
            {
                if(chan < ChannelHandler.LowestChannel)
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
                Source.Flush();
                Parameters.State = eGSMState.Reset;

                /* wait until reset done */
                int waitForSearch = 100;
                while (Parameters.State == eGSMState.Reset && --waitForSearch > 0)
                {
                    Thread.Sleep(100);
                }

                /* wait for FCCH searching */
                int waitForLock = 8;
                while (Parameters.State == eGSMState.FCCHSearch && --waitForLock > 0)
                {
                    Thread.Sleep(100);
                }

                /* when FCCH found, wait until CBCH found */
                if (Parameters.State != eGSMState.FCCHSearch)
                {
                    int waitForDetails = 50;
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

                    string mccMncString = "Unknown";
                    string lacString = "Unknown";
                    string cellIdentString = "Unknown";

                    if (Handler.L3.PDUDataFields.ContainsKey("MCC/MNC"))
                        mccMncString = Handler.L3.PDUDataFields["MCC/MNC"];

                    if (Handler.L3.PDUDataFields.ContainsKey("LAC"))
                        lacString = Handler.L3.PDUDataFields["LAC"];

                    if (Handler.L3.PDUDataFields.ContainsKey("CellIdent"))
                        cellIdentString = Handler.L3.PDUDataFields["CellIdent"];

                    StationDialog.AddStation(ChannelHandler.Channel, mccMncString, lacString,cellIdentString, Parameters.CBCH.ToString());

                    Log.AddMessage("Channel " + ChannelHandler.Channel + " used.");
                    Log.AddMessage("  MCC/MNC  : " + mccMncString);
                    Log.AddMessage("  LAC      : " + lacString);
                    Log.AddMessage("  CellIdent: " + cellIdentString);
                    Log.AddMessage("  CBCH     : " + Parameters.CBCH);
                }

                if (ChannelHandler.Channel == ChannelHandler.HighestChannel)
                {
                    Log.AddMessage("Scan finished");
                    BeginInvoke(new MethodInvoker(() => btnScan.Text="Scan"));
                    return;
                }

                ChannelHandler.Channel = ChannelHandler.NextChannel;
                BeginInvoke(new MethodInvoker(() => txtArfcn.Value = ChannelHandler.Channel));
            }
        }
    }
}
