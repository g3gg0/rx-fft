using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Components.GDI;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Bursts;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.SignalProcessing;

namespace GSM_Analyzer
{
    public partial class GSMAnalyzer : Form
    {
        private Stream InputStream;
        private SharedMem ShmemChannel;
        private Thread ReadThread;
        private bool ThreadActive;
        private int BlockSize = 512;

        private double[] BurstLengthCompensation = new[] { -0.75, 0.25, 0.25, 0.25 };

        private Semaphore SingleStepSem = new Semaphore(0, 1, "SingleStepSemaphore");
        private bool SingleStep;

        internal TimeSlotHandler Handler;
        internal readonly GMSKDemodulator Demodulator;

        private FilterDialog FilterWindow;
        private BurstVisualizer BurstWindow;
        private OptionsDialog OptionsWindow;

        internal double SamplingRate = 2184533;
        internal bool SamplingRateChanged = false;
        public static bool Subsampling = false;

        internal double Oversampling
        {
            get { return SamplingRate / 270833; }
        }


        public GSMAnalyzer()
        {
            InitializeComponent();
            Handler = new TimeSlotHandler(Oversampling, 0.3, AddMessage);
            Demodulator = new GMSKDemodulator();

            if (Handler.L3.StatusMessage != null)
                MessageBox.Show("   [L3] " + Handler.L3.StatusMessage);

            RegisterTriggers();

        }

        public void RegisterTriggers()
        {
            L3Handler.PDUDataTriggers.Add("LAIUpdate", TriggerLAIUpdate);
            L3Handler.PDUDataTriggers.Add("CBCHReset", TriggerCBCHReset);
            L3Handler.PDUDataTriggers.Add("CBCHUpdate", TriggerCBCHUpdate);
        }

        private void TriggerCBCHReset()
        {
            lock (L3Handler.PDUDataFields)
            {
                if (L3Handler.PDUDataFields.ContainsKey("ChannelType"))
                    L3Handler.PDUDataFields.Remove("ChannelType");
                if (L3Handler.PDUDataFields.ContainsKey("SubChannel"))
                    L3Handler.PDUDataFields.Remove("SubChannel");
                if (L3Handler.PDUDataFields.ContainsKey("TimeSlot"))
                    L3Handler.PDUDataFields.Remove("TimeSlot");
            }

        }

        private void TriggerCBCHUpdate()
        {
            string channelTypeString = "";
            string subChannelString = "";
            string timeSlotString = "";

            lock (L3Handler.PDUDataFields)
            {
                if (L3Handler.PDUDataFields.ContainsKey("ChannelType"))
                    channelTypeString = L3Handler.PDUDataFields["ChannelType"];
                if (L3Handler.PDUDataFields.ContainsKey("SubChannel"))
                    subChannelString = L3Handler.PDUDataFields["SubChannel"];
                if (L3Handler.PDUDataFields.ContainsKey("TimeSlot"))
                    timeSlotString = L3Handler.PDUDataFields["TimeSlot"];
            }

            int subChannel = -1;
            int timeSlot = -1;

            int.TryParse(subChannelString, out subChannel);
            int.TryParse(timeSlotString, out timeSlot);

            if (subChannel < 0 || timeSlot < 0 || !channelTypeString.StartsWith("SDCCH"))
            {
                SDCCHBurst.CBCHEnabled = eTriState.No;
                return;
            }

            SDCCHBurst.CBCHEnabled = eTriState.Yes;
            SDCCHBurst.CBCHTimeSlot = timeSlot;
            SDCCHBurst.CBCHSubChannel = subChannel;
        }

        private void TriggerLAIUpdate()
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

            UpdateCellInfo(mcc, mnc, lac, cellIdent, SDCCHBurst.CBCHEnabled);
        }


        public void AddMessage(String msg)
        {
            try
            {
                this.BeginInvoke(new addMessageDelegate(addMessageFunc), new object[] { msg });
            }
            catch (Exception)
            {
            }
        }

        public void UpdateCellInfo(int mcc, int mnc, long lac, long cellIdent, eTriState hasCBCH)
        {
            try
            {
                this.BeginInvoke(new updateCellInfoDelegate(UpdateCellInfoFunc), new object[] { mcc, mnc, lac, cellIdent, hasCBCH });
            }
            catch (Exception)
            {
            }
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

        delegate void addMessageDelegate(String msg);
        delegate void updateErrorSuccessDelegate(long err, long succ, long TN, long T1, long T2, long T3);
        delegate void updateCellInfoDelegate(int mcc, int mnc, long lac, long cellIdent, eTriState hasCBCH);

        public void addMessageFunc(String msg)
        {
            if (msg != null)
                txtLog.AppendText(msg);
            else
                txtLog.Clear();
        }

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

        void UpdateStats(GSMParameters param)
        {
            BeginInvoke(new updateErrorSuccessDelegate(UpdateErrorSuccess), new object[] { param.TotalErrors, param.TotalSuccess, param.TN, param.T1, param.T2, param.T3 });
        }

        void ResetStats()
        {
            BeginInvoke(new updateErrorSuccessDelegate(UpdateErrorSuccess), new object[] { -1, -1, -1, -1, -1, -1 });
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (InputStream != null)
            {
                ThreadActive = false;
                Thread.Sleep(10);
                ReadThread.Abort();
                InputStream.Close();

                if (ShmemChannel != null)
                {
                    ShmemChannel.Unregister();
                    ShmemChannel = null;
                }
                InputStream = null;

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
                CFileDecimationDialog dec = new CFileDecimationDialog();

                dec.StartPosition = FormStartPosition.Manual;
                dec.Location = MousePosition;
                dec.ShowDialog();

                if (dec.Decimation < 1)
                    return;

                /* calculate sampling rate from USRPs decimation rate */
                SamplingRate = 64000000f / dec.Decimation;
                SamplingRateChanged = true;

                /* USRP has an inverted spectrum */
                GMSKDemodulator.InvertedSpectrum = true;

                try
                {
                    InputStream = new FileStream(dlg.FileName, FileMode.Open);
                    BlockSize = 32768;

                    Demodulator.DataFormat = eDataFormat.Direct64BitIQFloat64k;
                    txtLog.Clear();

                    ThreadActive = true;
                    ReadThread = new Thread(FFTReadFunc);
                    ReadThread.Start();

                    btnOpen.Text = "Close";
                }
                catch (Exception ex)
                {
                    AddMessage("Exception: " + ex);
                }
            }
        }

        private void btnOpen_SharedMemory(object sender, EventArgs e)
        {
            ShmemChannel = new SharedMem(0, -1, "GSM Analyzer", 64 * 1024 * 1024);
            ShmemChannel.ReadTimeout = 10;
            ShmemChannel.ReadMode = eReadMode.TimeLimited;
            BlockSize = 512;

            InputStream = ShmemChannel;

            Demodulator.DataFormat = eDataFormat.Direct16BitIQFixedPoint;
            txtLog.Clear();
            ThreadActive = true;
            ReadThread = new Thread(FFTReadFunc);
            ReadThread.Start();

            btnOpen.Text = "Close";
        }


        void UpdateUIStatus(GSMParameters param)
        {
            eGSMState state = param.State;

            switch (state)
            {
                case eGSMState.Idle:
                case eGSMState.Reset:
                    statusSearch.State = eLampState.Grayed;
                    statusTrain.State = eLampState.Grayed;
                    statusLock.State = eLampState.Grayed;

                    lock (L3Handler.PDUDataFields)
                    {
                        L3Handler.PDUDataFields.Remove("MCC/MNC");
                        L3Handler.PDUDataFields.Remove("LAC");
                        L3Handler.PDUDataFields.Remove("CellIdent");
                    }
                    SDCCHBurst.CBCHEnabled = eTriState.Unknown;
                    UpdateCellInfo(-1, -1, -1, -1, SDCCHBurst.CBCHEnabled);

                    break;

                case eGSMState.FCCHSearch:
                    statusSearch.State = eLampState.Green;
                    statusTrain.State = eLampState.Grayed;
                    statusLock.State = eLampState.Grayed;

                    lock (L3Handler.PDUDataFields)
                    {
                        L3Handler.PDUDataFields.Remove("MCC/MNC");
                        L3Handler.PDUDataFields.Remove("LAC");
                        L3Handler.PDUDataFields.Remove("CellIdent");
                    }
                    SDCCHBurst.CBCHEnabled = eTriState.Unknown;
                    UpdateCellInfo(-1, -1, -1, -1, SDCCHBurst.CBCHEnabled);
                    break;

                case eGSMState.SCHSearch:
                    statusSearch.State = eLampState.Green;
                    statusTrain.State = eLampState.Green;
                    statusLock.State = eLampState.Grayed;

                    lock (L3Handler.PDUDataFields)
                    {
                        L3Handler.PDUDataFields.Remove("MCC/MNC");
                        L3Handler.PDUDataFields.Remove("LAC");
                        L3Handler.PDUDataFields.Remove("CellIdent");
                    }
                    SDCCHBurst.CBCHEnabled = eTriState.Unknown;
                    UpdateCellInfo(-1, -1, -1, -1, SDCCHBurst.CBCHEnabled);
                    break;

                case eGSMState.Lock:
                    statusSearch.State = eLampState.Green;
                    statusTrain.State = eLampState.Green;
                    statusLock.State = param.Errors > 0 ? eLampState.Red : eLampState.Green;
                    break;
            }
        }

        void FFTReadFunc()
        {
            long frameStartPosition = 0;
            long currentPosition = 0;
            long updateLoops = 0;

            double oldSamplingRate = SamplingRate;

            byte[] inBuffer = new byte[BlockSize * Demodulator.BytesPerSamplePair];
            double[] gsmSignal = new double[BlockSize];
            double[] gsmStrength = new double[BlockSize];

            double[] burstBuffer = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];
            double[] burstStrengthBuffer = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];

            long burstSamples = (long)Math.Ceiling(Burst.TotalBitCount * Oversampling);
            double burstSamplesAccurate = Burst.TotalBitCount * Oversampling;
            double deltaSamplesPerBurst = burstSamples - burstSamplesAccurate;
            double skipSampleEvery = 1 / deltaSamplesPerBurst;
            int sampleDelta = 0;


            double burstCount = 0;
            long burstBufferPos = 0;

            GSMParameters parameters = new GSMParameters();
            FCCHFinder finder = new FCCHFinder(Oversampling);

            Handler.Parameters = parameters;

            parameters.State = eGSMState.Reset;
            UpdateUIStatus(parameters);

            while (ThreadActive)
            {
                int read = InputStream.Read(inBuffer, 0, inBuffer.Length);

                if (read != 0 && read != inBuffer.Length)
                {
                    parameters.State = eGSMState.Reset;
                    UpdateUIStatus(parameters);
                    AddMessage("(Timeout while reading data)" + Environment.NewLine);
                    Thread.Sleep(500);
                }
                else
                {
                    int samplesRead = read / Demodulator.BytesPerSamplePair;

                    /* if read an even sample count */
                    if (samplesRead * Demodulator.BytesPerSamplePair == read)
                    {
                        /* when the rate has changed */
                        if (ShmemChannel != null && ShmemChannel.Rate != 0 && SamplingRate != ShmemChannel.Rate / 2)
                        {
                            SamplingRateChanged = true;
                            SamplingRate = ShmemChannel.Rate / 2;
                        }

                        /* to allow external rate change */
                        if (SamplingRateChanged)
                        {
                            SamplingRateChanged = false;
                            if (Oversampling > 1)
                            {
                                AddMessage("   [GMSK] Sampling Rate changed from " + oldSamplingRate + " to " + SamplingRate);
                                oldSamplingRate = SamplingRate;
                                finder = new FCCHFinder(Oversampling);
                                Handler = new TimeSlotHandler(Oversampling, 0.3, AddMessage);
                                Handler.Parameters = parameters;

                                burstBuffer = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];
                                burstStrengthBuffer = new double[(int)((Handler.SpareBits + Burst.TotalBitCount) * Oversampling)];
                                burstSamples = (long)Math.Ceiling(Burst.TotalBitCount * Oversampling);
                                burstSamplesAccurate = Burst.TotalBitCount * Oversampling;
                                deltaSamplesPerBurst = burstSamples - burstSamplesAccurate;
                                skipSampleEvery = 1 / deltaSamplesPerBurst;
                                burstCount = 0;
                                sampleDelta = 0;
                            }
                        }

                        try
                        {
                            Demodulator.ProcessData(inBuffer, read, gsmSignal, gsmStrength);
                        }
                        catch (Exception e)
                        {
                            AddMessage("   [GMSK] Exception: " + e + Environment.NewLine);
                            return;
                        }


                        for (int pos = 0; pos < samplesRead; pos++)
                        {
                            double signal = gsmSignal[pos];
                            double strength = gsmStrength[pos];

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
                                burstSampled = true;
                                /* reset the delta. it will get set later again */
                                sampleDelta = 0;
                            }

                            switch (parameters.State)
                            {
                                case eGSMState.Idle:
                                    break;

                                case eGSMState.Reset:
                                    AddMessage("[GSM] Reset" + Environment.NewLine);
                                    L3Handler.ReloadFiles();
                                    currentPosition = 0;
                                    finder.Reset();
                                    parameters.State = eGSMState.FCCHSearch;
                                    ResetStats();
                                    UpdateUIStatus(parameters);
                                    break;

                                case eGSMState.FCCHSearch:
                                    /* let the FCCH finder detect an FCCH burst */
                                    bool fcchFound = false;
                                    try
                                    {
                                        fcchFound = finder.ProcessData(signal, strength);
                                    }
                                    catch (Exception e)
                                    {
                                        AddMessage("   [FCCH] Exception: " + e + Environment.NewLine);
                                        return;
                                    }

                                    if (fcchFound)
                                    {
                                        parameters.State = eGSMState.SCHSearch;
                                        UpdateUIStatus(parameters);

                                        AddMessage("   [FCCH]" + Environment.NewLine);

                                        /* save the position where the frame started */
                                        frameStartPosition = finder.BurstStartPosition;
                                        frameStartPosition -= (long)(Oversampling * Handler.SpareBits);

                                        /* update the burst buffer pointer */
                                        burstBufferPos = currentPosition - frameStartPosition;

                                        /* this is TN 0 */
                                        parameters.FN = 0;
                                        parameters.TN = 0;
                                    }

                                    break;


                                case eGSMState.SCHSearch:
                                    /* if one burst was sampled */
                                    if (burstSampled)
                                    {
                                        /* if this is the eighth burst, its the first timeslot of the next frame - SCH */
                                        if (parameters.TN == 8)
                                        {
                                            /* set TN to 7, since handler will increase */
                                            parameters.TN = 7;
                                            parameters.FirstSCH = true;

                                            /* let the handler process this packet */
                                            Handler.Decoder.StartOffset = (int)(Oversampling * Handler.SpareBits);
                                            Handler.Decoder.SubSampleOffset = 0;
                                            Handler.Handle(burstBuffer);

                                            if (parameters.Error)
                                            {
                                                AddMessage("[GSM] SCH failed -> Reset" + Environment.NewLine);
                                                parameters.State = eGSMState.Reset;
                                                UpdateUIStatus(parameters);
                                                parameters.Error = false;
                                            }
                                            else
                                            {
                                                AddMessage("[GSM] SCH found -> Lock" + Environment.NewLine);
                                                parameters.State = eGSMState.Lock;
                                                UpdateUIStatus(parameters);
                                            }
                                        }
                                        else
                                            parameters.TN++;


                                        burstBufferPos = 0;

                                        /* update the burst visualizer */
                                        if (BurstWindow != null)
                                        {
                                            BurstWindow.XAxisOffset = Handler.Decoder.StartOffset;
                                            BurstWindow.ProcessBurst(burstBuffer, burstStrengthBuffer);
                                        }

                                        if (SingleStep)
                                            SingleStepSem.WaitOne();

                                    }
                                    break;

                                case eGSMState.Lock:

                                    /* when we are already in frame sync and one burst was sampled */
                                    if (burstSampled)
                                    {
                                        Handler.Decoder.StartOffset = (int)(Oversampling * Handler.SpareBits);
                                        if (Subsampling)
                                            Handler.Decoder.SubSampleOffset = OffsetEstimator.EstimateOffset(burstBuffer,
                                                                           (int)(Handler.Decoder.StartOffset + Oversampling / 2 - 5 * Oversampling),
                                                                           (int)((Burst.NetBitCount - 5) * Oversampling),
                                                                           Oversampling);
                                        else
                                            Handler.Decoder.SubSampleOffset = 0;

                                        if (BurstWindow != null)
                                        {
                                            BurstWindow.XAxisOffset = (int)(Handler.Decoder.StartOffset + Handler.Decoder.SubSampleOffset + Oversampling / 2);
                                            BurstWindow.Oversampling = Oversampling;
                                            BurstWindow.ProcessBurst(burstBuffer, burstStrengthBuffer);
                                        }


                                        Handler.Handle(burstBuffer);
                                        if (parameters.Error)
                                        {
                                            parameters.Error = false;
                                            AddMessage("[GSM] Packet handling failed -> Reset" + Environment.NewLine);
                                            parameters.State = eGSMState.Reset;
                                        }

                                        /* 
                                         * tricky! the BTS sends the bursts with 156 bits instead of 156.25
                                         * but it delays one bit after 4 bursts. compensate this here.
                                         * we do that for the next timeslot
                                         */
                                        long burstNumber = ((parameters.TN + 1) % 4);
                                        sampleDelta -= (int)(BurstLengthCompensation[burstNumber] * Oversampling);

                                        /* update counters and reset offset correction */
                                        burstCount++;
                                        burstBufferPos = -parameters.SampleOffset;
                                        parameters.SampleOffset = 0;

                                        /* update UI if necessary */
                                        if (SingleStep || updateLoops++ >= 20 * 9)
                                        {
                                            UpdateUIStatus(parameters);
                                            UpdateStats(parameters);
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
                    else
                    {
                        Thread.Sleep(500);
                    }
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

        private void btnStats_Click(object sender, EventArgs e)
        {
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

        private void btnBurst_Click(object sender, EventArgs e)
        {
            if (BurstWindow == null || !BurstWindow.Visible)
            {
                BurstWindow = new BurstVisualizer(Oversampling);
                BurstWindow.Show();
            }
            else
            {
                BurstWindow.Close();
                BurstWindow = null;
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
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }
    }

}
