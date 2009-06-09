using System;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Components.GraphicalElements;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM;
using LibRXFFT.Libraries.ShmemChain;

namespace GSM_Analyzer
{
    public partial class GSMAnalyzer : Form
    {
        private BurstVisualizer visualizer;
        private SharedMem ShmemChannel;
        private Thread ReadThread;
        private bool ThreadActive;
        private const int BlockSize = 512;

        private TimeSlotHandler handler;

        private double Rate = 2184533;
        private double MinPowerFact = 0.25;

        private Semaphore SingleStepSem = new Semaphore(0, 1, "SingleStepSemaphore");
        private bool SingleStep;

        private double Oversampling
        {
            get { return Rate / 270833; }
        }


        public GSMAnalyzer()
        {
            InitializeComponent();
            handler = new TimeSlotHandler(Oversampling, 0.3);
            /*
            bool[] dat = new bool[] {true, true, false, true, false, true, true, false, true, true};
            bool[] pol = new bool[] {true, false, false, true, true};

            CRC.Calc(dat, null, pol);
            bool[] srcData = new bool[]{ true, false, false, false, true, true, true, true, false, false, false, true, false, true, true, true, true, false, false, false, false, true, false, true, true };

            CRC.Calc(srcData, null, CRC.PolynomialSCH);
             * */

            /*
             * thats for testing the en/decoders
             * 
            string msg = "";

            byte[] inData = new byte[]{ 0xDE, 0xAD, 0xBE, 0xEF};

            for (int pos = 0; pos < inData.Length; pos++)
                msg += " " + inData[pos];


            msg += Environment.NewLine;

            bool[] boolData = ByteUtil.BytesToBits(inData);

            for (int pos = 0; pos < boolData.Length; pos++)
                msg += " " + (boolData[pos]?"1":"0");


            msg += Environment.NewLine;

            boolData = DifferenceCode.Encode(boolData);

            for (int pos = 0; pos < boolData.Length; pos++)
                msg += " " + (boolData[pos] ? "1" : "0");



            msg += Environment.NewLine;

            boolData = DifferenceCode.Decode(boolData);

            for (int pos = 0; pos < boolData.Length; pos++)
                msg += " " + (boolData[pos] ? "1" : "0");

            byte[] outData = ByteUtil.BitsToBytes(boolData);

            for (int pos = 0; pos < outData.Length; pos++)
                msg += " " + outData[pos];


            txtLog.AppendText(msg);
             * */
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

        delegate void addMessageDelegate(String msg);
        delegate void updateErrorSuccessDelegate(long err, long succ, long timeslot);

        public void addMessageFunc(String msg)
        {
            if (msg != null)
                txtLog.AppendText(msg);
            else
                txtLog.Clear();
        }

        void UpdateErrorSuccess(long err, long succ, long timeslot)
        {
            lblErrors.Text = "Errors: " + err;
            lblSucess.Text = "Successful: " + succ;
            if (err + succ > 0)
                lblRate.Text = "Rate: " + ((succ * 100) / (succ + err)) + " %";
            else
                lblRate.Text = "Rate: 0 %";
            lblTS.Text = "TS: " + timeslot;
        }

        void UpdateStats(GSMParameters param)
        {
            BeginInvoke(new updateErrorSuccessDelegate(UpdateErrorSuccess), new object[] { param.TotalErrors, param.TotalSuccess, param.CurrentTimeSlot });
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (ShmemChannel != null)
            {
                ThreadActive = false;
                Thread.Sleep(10);
                ReadThread.Abort();
                ShmemChannel.Unregister();
                ShmemChannel = null;

                btnOpen.Text = "Open";

                /*
                AddMessage(Environment.NewLine);
                AddMessage("-----------------------------" + Environment.NewLine);
                AddMessage("   Dumping all BCCH Frames:" + Environment.NewLine);
                AddMessage("-----------------------------" + Environment.NewLine);
                AddMessage(Environment.NewLine);

                foreach (KeyValuePair<int, long> occurence in handler.BCCH.Occurences)
                {
                    if (handler.BCCH.pktTypes.Map.ContainsKey(occurence.Key))
                        AddMessage("  " + String.Format("{0,5}", occurence.Value) + " x " + handler.BCCH.pktTypes.Map[occurence.Key].description);
                    else
                        AddMessage("  " + String.Format("{0,5}", occurence.Value) + " x Unknown Type " + occurence.Key);

                    AddMessage(Environment.NewLine);
                }
                */
                /*
                byte[] srcBytes = new byte[57];
                srcBytes[0] = 0xDE;
                srcBytes[1] = 0xAD;
                srcBytes[2] = 0xBE;
                srcBytes[3] = 0xEF;

                bool[][] srcData = new bool[1][];
                srcData[0] = ByteUtil.BytesToBits(srcBytes);

                bool[][] dstData = InterleaveCoder.Interleave(srcData, null);
                bool[][] decData = InterleaveCoder.Deinterleave(dstData, null);
                */
                /*
                AddMessage(null);
                for (int pos = 0; pos < handler.Decoders.Length; pos++ )
                    AddMessage(handler.Decoders[pos].MinPowerFact + " " + handler.DecoderSuccess[pos] + " " + handler.DecoderErrors[pos] + Environment.NewLine);
                
                AddMessage(null);
                for (int pos = 0; pos < handler.Decoders.Length; pos++)
                    AddMessage(handler.Decoders[pos].DCOffsetFact + " " + handler.DecoderSuccess[pos] + " " + handler.DecoderErrors[pos] + Environment.NewLine);
                 * */
            }
            else
            {
                ShmemChannel = new SharedMem(0, -1, "GSM Analyzer", 64 * 1024 * 1024);
                ThreadActive = true;

                txtLog.Clear();
                ReadThread = new Thread(FFTReadFunc);
                ReadThread.Start();

                btnOpen.Text = "Close";
            }
        }


        void SetStatusLamps(GSMParameters param)
        {
            eGSMState state = param.State;

            switch (state)
            {
                case eGSMState.Idle:
                case eGSMState.Reset:
                    statusSearch.State = eLampState.Grayed;
                    statusTrain.State = eLampState.Grayed;
                    statusLock.State = eLampState.Grayed;
                    break;

                case eGSMState.FCCHSearch:
                    statusSearch.State = eLampState.Green;
                    statusTrain.State = eLampState.Grayed;
                    statusLock.State = eLampState.Grayed;
                    break;

                case eGSMState.SCHSearch:
                    statusSearch.State = eLampState.Green;
                    statusTrain.State = eLampState.Green;
                    statusLock.State = eLampState.Grayed;
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
            byte[] inBuffer = new byte[BlockSize * 4];
            double[] gsmSignal = new double[BlockSize];
            double[] gsmStrength = new double[BlockSize];

            long frameStartPosition = 0;
            long currentPosition = 0;
            long absoluteTimeSlotNumber = 0;

            int updateLoops = 0;

            double[] burstBuffer = new double[(int)(Burst.TotalBitCount * Oversampling) + 2];
            double[] burstStrengthBuffer = new double[(int)(Burst.TotalBitCount * Oversampling) + 2];

            double burstSamplesAccurate = Burst.TotalBitCount * Oversampling;
            long burstSamples = (long)Math.Ceiling(burstSamplesAccurate);
            double deltaSamplesPerBurst = burstSamples - burstSamplesAccurate;
            double skipSampleEvery = 1 / deltaSamplesPerBurst;
            long burstBufferPos = 0;
            double burstCount = 0;

            GSMParameters parameters = new GSMParameters();
            GMSKDemodulator demod = new GMSKDemodulator();
            FCCHFinder finder = new FCCHFinder(Oversampling);

            handler.AddMessage = new AddMessageDelegate(AddMessage);
            handler.Parameters = parameters;
            handler.SharedMemory = ShmemChannel;

            parameters.State = eGSMState.Reset;
            SetStatusLamps(parameters);

            ShmemChannel.ReadTimeout = 10;
            ShmemChannel.ReadMode = eReadMode.TimeLimited;

            while (ThreadActive)
            {
                int read = ShmemChannel.Read(inBuffer, 0, inBuffer.Length);
                if (read != 0 && read != inBuffer.Length)
                {
                    AddMessage("Timeout while reading data!" + Environment.NewLine);
                    return;
                }

                if (read == inBuffer.Length)
                {
                    /* when the rate has changed */
                    if (ShmemChannel.Rate != 0 && Rate != ShmemChannel.Rate / 2)
                    {
                        Rate = ShmemChannel.Rate / 2;

                        finder = new FCCHFinder(Oversampling);
                        handler = new TimeSlotHandler(Oversampling, 0.3);
                        handler.AddMessage = new AddMessageDelegate(AddMessage);
                        handler.Parameters = parameters;
                        handler.SharedMemory = ShmemChannel;

                        burstSamples = (long)Math.Ceiling(Burst.TotalBitCount * Oversampling);
                        burstBuffer = new double[burstSamples + 2];
                        burstStrengthBuffer = new double[burstSamples + 2];
                    }

                    handler.Decoder.MinPowerFact = MinPowerFact;
                    demod.ProcessData(inBuffer, gsmSignal, gsmStrength, null);

                    for (int pos = 0; pos < read / 4; pos++)
                    {
                        double signal = gsmSignal[pos];
                        double strength = gsmStrength[pos];

                        /* write this sample into the burst buffer */
                        if (burstBufferPos < burstBuffer.Length && burstBufferPos > 0)
                        {
                            burstBuffer[burstBufferPos] = signal;
                            burstStrengthBuffer[burstBufferPos] = strength;
                        }
                        burstBufferPos++;


                        bool burstSampled = false;

                        bool timeToSkip = burstCount > skipSampleEvery;
                        if (timeToSkip && burstBufferPos >= (burstSamples - 1))
                        {
                            burstSampled = true;
                            burstCount -= skipSampleEvery;
                        }
                        else if (!timeToSkip && burstBufferPos >= burstSamples)
                            burstSampled = true;

                        switch (parameters.State)
                        {
                            case eGSMState.Idle:
                                break;

                            case eGSMState.Reset:
                                AddMessage("[GSM] Reset" + Environment.NewLine);
                                currentPosition = 0;
                                finder.Reset();
                                parameters.State = eGSMState.FCCHSearch;
                                SetStatusLamps(parameters);
                                break;

                            case eGSMState.FCCHSearch:
                                /* let the FCCH finder detect an FCCH burst */
                                if (finder.ProcessData(signal, strength))
                                {
                                    parameters.State = eGSMState.SCHSearch;
                                    SetStatusLamps(parameters);
                                    absoluteTimeSlotNumber = 0;

                                    //AddMessage("Frame " + String.Format("{0,3}", currentFrame) + " TS " + String.Format("{0,3}", currentTimeslot) + " FCCH found at " + finder.BurstStartPosition + Environment.NewLine);
                                    AddMessage("   [FCCH]" + Environment.NewLine);

                                    /* save the position where the frame started */
                                    frameStartPosition = finder.BurstStartPosition;
                                    frameStartPosition -= (long)(Oversampling);

                                    /* update the burst buffer pointer */
                                    burstBufferPos = currentPosition - frameStartPosition;
                                }
                                break;

                            case eGSMState.SCHSearch:

                                /* if one burst was sampled */
                                if (burstSampled)
                                {
                                    /* update the burst visualizer */
                                    if (visualizer != null)
                                        visualizer.ProcessBurst(burstBuffer, burstStrengthBuffer);

                                    /* output it to shared memory */
                                    //ShmemChannel.Write(ByteUtil.convertToBytesInterleaved(burstBuffer,
                                    //                                                      burstStrengthBuffer));
                                    //ShmemChannel.Write(ByteUtil.convertToBytes(new double[256]));

                                    if (SingleStep)
                                        SingleStepSem.WaitOne();

                                    /* look for the first SCH */
                                    if (absoluteTimeSlotNumber == 8)
                                    {

                                        /* let the handler process this packet */
                                        parameters.CurrentControlFrame = 0;
                                        parameters.CurrentTimeSlot = 7;
                                        handler.Handle(burstBuffer);

                                        if (parameters.Error)
                                        {
                                            AddMessage("[GSM] SCH failed -> Reset" + Environment.NewLine);
                                            parameters.State = eGSMState.Reset;
                                            SetStatusLamps(parameters);
                                            parameters.Error = false;
                                        }
                                        else
                                        {
                                            AddMessage("[GSM] SCH found -> Lock" + Environment.NewLine);
                                            parameters.State = eGSMState.Lock;
                                            SetStatusLamps(parameters);
                                        }
                                    }
                                    burstBufferPos = 0;
                                    absoluteTimeSlotNumber++;
                                }
                                break;

                            case eGSMState.Lock:

                                /* when we are already in frame sync and one burst was sampled */
                                if (burstSampled)
                                {
                                    if (visualizer != null)
                                        visualizer.ProcessBurst(burstBuffer, burstStrengthBuffer);

                                    handler.Handle(burstBuffer);
                                    if (parameters.Error)
                                    {
                                        parameters.Error = false;
                                        AddMessage("[GSM] Packet handling failed -> Reset" + Environment.NewLine);
                                        parameters.State = eGSMState.Reset;
                                    }

                                    if (SingleStep)
                                        SingleStepSem.WaitOne();

                                    burstCount++;
                                    burstBufferPos = -parameters.SampleOffset;
                                    parameters.SampleOffset = 0;

                                    if (SingleStep || updateLoops++ >= 11)
                                    {
                                        SetStatusLamps(parameters);
                                        UpdateStats(parameters);
                                        updateLoops = 0;
                                    }
                                }
                                break;
                        }
                        currentPosition++;
                    }
                }
            }
        }

        private void btnBurst_Click(object sender, EventArgs e)
        {
            if (visualizer == null)
            {
                visualizer = new BurstVisualizer();
                visualizer.Show();
            }
            else
            {
                visualizer.Close();
                visualizer = null;
            }
        }

        private void txtEstimationFact_TextChanged(object sender, EventArgs e)
        {
            try
            {
                MinPowerFact = double.Parse(txtEstimationFact.Text);
            }
            catch (Exception ex)
            {
                MinPowerFact = 0.1;
            }
        }

        private void chkSingleStep_CheckedChanged(object sender, EventArgs e)
        {
            SingleStep = chkSingleStep.Checked;
        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            SingleStepSem.Release(1);
        }
    }

}
