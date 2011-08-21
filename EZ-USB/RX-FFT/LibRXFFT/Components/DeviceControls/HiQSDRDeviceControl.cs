using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.USB_RX.Devices;
using LibRXFFT.Libraries.USB_RX.Misc;
using LibRXFFT.Libraries.USB_RX.Tuners;
using LibRXFFT.Components.DeviceControls;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LibRXFFT.Libraries.HiQ_SDR;

namespace LibRXFFT.Components.DeviceControls
{
    public partial class HiQSDRDeviceControl : Form, DeviceControl
    {
        private ByteUtil.eSampleFormat DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPointLE;

        private EndPoint Endpoint;
        private ShmemSampleSource _SampleSource;
        private SharedMem NetShmemSink;
        private Thread TransferThread;

        private HiQSDRControl HiQControl;
        private bool StopTransfers = false;

        private long CurrentFrequency = 0;
        private long CurrentRate = 960000;

        private long SamplesReceived = 0;
        private DateTime LastTime = DateTime.Now;
        
        public HiQSDRDeviceControl()
        {
            InitializeComponent();

            NetShmemSink = new SharedMem(-2, 0, "Network Output Node");

            _SampleSource = new ShmemSampleSource("NetworkReader", NetShmemSink.DstChan, 1, 0);
            _SampleSource.InvertedSpectrum = false;
            _SampleSource.SamplingRateChanged += new EventHandler(_SampleSource_SamplingRateChanged);
            _SampleSource.DataFormat = DataFormat;
            _SampleSource.ForceInputRate(CurrentRate);

            Endpoint = new IPEndPoint(IPAddress.Any, 0);

            TransferThread = new Thread(TransferThreadMain);
            TransferThread.Start();
            
            radioAcqOff.Checked = true;

            Show();
        }

        public int ShmemChannel
        {
            get { return _SampleSource.ShmemChannel.SrcChan; }
        }

        void _SampleSource_SamplingRateChanged(object sender, EventArgs e)
        {
            if (SamplingRateChanged != null)
            {
                SamplingRateChanged(this, null);
            }
        }

        void TransferThreadMain()
        {
            byte[] receiveBuffer = new byte[8192];
            byte[] outBuffer = new byte[1440/6 * 4];
            int received = 0;

            try
            {
                while (!StopTransfers)
                {
                    lock (this)
                    {
                        if (HiQControl != null)
                        {
                            received = HiQControl.Receive(receiveBuffer, ref Endpoint);
                        }
                        else
                        {
                            received = 0;
                        }
                    }

                    if (received == 1442)
                    {
                        if (true)
                        {
                            for (int sample = 0; sample < 1440 / 6; sample++)
                            {
                                double I = ByteUtil.getDoubleFromBytes(receiveBuffer, 2 + sample * 6, 3, true);
                                double Q = ByteUtil.getDoubleFromBytes(receiveBuffer, 2 + sample * 6 + 3, 3, true);

                                ByteUtil.putBytesFromDouble(outBuffer, sample * 4, I);
                                ByteUtil.putBytesFromDouble(outBuffer, sample * 4 + 2, Q);

                                SamplesReceived++;
                            }
                            NetShmemSink.Write(outBuffer, 0, outBuffer.Length);

                            DateTime now = DateTime.Now;
                            double delta = (now - LastTime).TotalMilliseconds;
                            if (delta > 250)
                            {
                                string text = (long)(((double)SamplesReceived / (delta / 1000))) + " Sample/s";
                                BeginInvoke(new Action(() =>
                                {
                                    lblRate.Text = text;
                                }));
                                LastTime = now;
                                SamplesReceived = 0;
                            }
                        }
                        else
                        {
                            NetShmemSink.Write(receiveBuffer, 2, received - 2);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            TransferThread = null;
            BeginInvoke(new Action(() =>
            {
                radioAcqOff.Checked = true;
            }));
        }

        private void CloseConnection()
        {
            if (TransferThread != null)
            {
                StopTransfers = true;

                if (!TransferThread.Join(500))
                {
                    TransferThread.Abort();
                }
                TransferThread = null;
            }
        }

        void Tuner_InvertedSpectrumChanged(object sender, EventArgs e)
        {
            SampleSource.InvertedSpectrum = InvertedSpectrum;
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            CloseTuner();
        }

        private void frequencySelector1_FrequencyChanged(object sender, EventArgs e)
        {
            if (!Connected || ScanFrequenciesEnabled)
                return;

            SetFrequency(frequencySelector1.Frequency);
        }


        #region DeviceControl Member
        
        public bool AllowsMultipleReaders
        {
            get
            {
                return true;
            }
        }

        public event EventHandler TransferModeChanged;
        private eTransferMode CurrentMode = eTransferMode.Stopped;
        public eTransferMode TransferMode
        {
            get
            {
                return CurrentMode;
            }
            set
            {
                CurrentMode = value;
                if (TransferModeChanged != null)
                {
                    TransferModeChanged(this, null);
                }
            }
        }

        public string ErrorMessage
        {
            get { return "None"; }
        }

        public double BlocksPerSecond
        {
            get
            {
                return CurrentRate / SamplesPerBlock;
            }
            set
            {
            }
        }

        public int SamplesPerBlock
        {
            get
            {
                return 1440 / 6;
            }
            set
            {
            }
        }

        public SampleSource SampleSource
        {
            get { return _SampleSource; }
        }

        public bool ReadBlock()
        {
            bool ret;

            ret = SampleSource.Read();
            if (SampleSource.SamplesRead != 0)
            {
                SampleSource.Flush();
            }

            return ret;
        }

        public bool Connected
        {
            get { return true; }
        }

        public void CloseControl()
        {
            CloseConnection();
            NetShmemSink.Close();
            SampleSource.Close();
            CloseTuner();
            Close();
        }

        public void StartRead()
        {
            lock (this)
            {
                if (HiQControl != null)
                {
                    HiQControl.StartTransfer();
                }
            }
        }

        public void StartStreamRead()
        {
            lock (this)
            {
                if (HiQControl != null)
                {
                    HiQControl.StartTransfer();
                    SampleSource.Flush();
                    TransferMode = eTransferMode.Stream;
                }
            }
        }

        public void StopRead()
        {
            lock (this)
            {
                if (HiQControl != null)
                {
                    HiQControl.StopTransfer();
                }
            }
        }

        #endregion

        #region DigitalTuner Member

        public long SamplingRate
        {
            set
            {
                CurrentRate = value;
                lock (this)
                {
                    if (HiQControl != null)
                    {
                        HiQControl.SetSampleRate((int)CurrentRate);
                    }
                }
                _SampleSource.DataFormat = DataFormat;
                _SampleSource.ForceInputRate(CurrentRate);
                NetShmemSink.Rate = CurrentRate * 2;

                _SampleSource.ForceInputRate(CurrentRate);

                /* inform listeners */
                if (SamplingRateChanged != null)
                    SamplingRateChanged(this, null);

                /* inform listeners */
                if (FilterWidthChanged != null)
                    FilterWidthChanged(this, null);
            }
            get
            {
                return CurrentRate;
            }
        }

        public event EventHandler SamplingRateChanged;

        #endregion

        #region Tuner Member

        public event EventHandler FilterWidthChanged;
        public event EventHandler FrequencyChanged;
        public event EventHandler InvertedSpectrumChanged;
        public event EventHandler DeviceDisappeared;
        public event EventHandler DeviceClosed;

        public bool OpenTuner()
        {
            return true;
        }

        public void CloseTuner()
        {
            lock (this)
            {
                if (HiQControl != null)
                {
                    HiQControl.Close();
                }
            }
        }

        public double Amplification
        {
            get { return 0; }
            set { }
        }

        public double Attenuation
        {
            get { return 0; }
        }

        public long IntermediateFrequency
        {
            get { return 0; }
        }

        public long LowestFrequency
        {
            get { return 100000; }
        }

        public long HighestFrequency
        {
            get { return HiQSDRControl.RX_CLOCK / 2; }
        }

        public long UpperFilterMargin
        {
            get { return HighestFrequency; }
        }

        public long LowerFilterMargin
        {
            get { return LowestFrequency; }
        }

        public string UpperFilterMarginDescription
        {
            get
            {
                return "Highest frequency";
            }
        }

        public string LowerFilterMarginDescription
        {
            get
            {
                return "Lowest frequency";
            }
        }

        public string FilterWidthDescription
        {
            get
            {
                return "Sampling rate range";
            }
        }

        string[] Tuner.Name
        {
            get { return new[] { "HiQSDR" }; }
        }

        public string[] Description
        {
            get { return new[] { "HiQSDR" }; }
        }

        public string[] Details
        {
            get
            {
                string msg = "Etherned and FPGA based SDR at IP Address ";
                lock (this)
                {
                    if (HiQControl == null)
                    {
                        msg += "<not connected>";
                    }
                    else
                    {
                        msg += HiQControl.Address.ToString();
                    }
                    return new[] { msg };
                }
            }
        }

        public bool SetFrequency(long frequency)
        {
            lock (this)
            {
                if (HiQControl == null)
                {
                    return false;
                }

                CurrentFrequency = frequency;
                frequencySelector1.Frequency = frequency;
                HiQControl.SetRXFreq((int)frequency);

                if (FrequencyChanged != null)
                    FrequencyChanged(this, null);

                return true;
            }
        }

        public long GetFrequency()
        {
            return CurrentFrequency;
        }

        public long FilterWidth
        {
            get
            {
                return CurrentRate;
            }
        }

        public bool InvertedSpectrum
        {
            get { return false; }
        }
        public bool ScanFrequenciesEnabled { get; set; }

        #endregion

        private void radioAcqOff_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAcqOff.Checked)
            {
                StopRead();
            }
        }

        private void radioAcqStream_CheckedChanged(object sender, EventArgs e)
        {
            if (radioAcqStream.Checked)
            {
                StartStreamRead();
            }
        }

        private void cmbRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            int rate = 0;

            if (int.TryParse(cmbRate.Text, out rate))
            {
                SamplingRate = rate;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (HiQControl == null)
            {
                btnConnect.Text = "Connecting";
                btnConnect.Enabled = false;

                Thread t = new Thread(() =>
                {
                    IPAddress host;

                    try
                    {
                        if (!IPAddress.TryParse(txtHost.Text, out host))
                        {
                            host = Dns.GetHostEntry(txtHost.Text).AddressList[0];
                        }

                        lock (this)
                        {
                            HiQControl = new HiQSDRControl(host);
                            HiQControl.StopTransfer();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to resolve host name: " + ex.GetType().ToString());
                        BeginInvoke(new Action(() =>
                        {
                            btnConnect.Text = "Connect";
                            btnConnect.Enabled = true;
                        }));
                        return;
                    }
                    BeginInvoke(new Action(() =>
                    {
                        btnConnect.Text = "Disconnect";
                        btnConnect.Enabled = true;
                    }));
                });

                t.Start();
            }
            else
            {
                lock (this)
                {
                    HiQControl.StopTransfer();
                    HiQControl = null;
                }
                btnConnect.Text = "Connect";
                btnConnect.Enabled = true;
            }
        }

        private void txtHost_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                btnConnect_Click(null, null);
            }
        }
    }
}
