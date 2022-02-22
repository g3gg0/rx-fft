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
using RX_FFT.Components.GDI;

namespace LibRXFFT.Components.DeviceControls
{
    public partial class HiQSDRDeviceControl : Form, DeviceControl
    {
        private enum eStatus
        {
            Ready,
            Connecting,
            Connected,
            Disconnecting
        }

        private ByteUtil.eSampleFormat DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPointLE;

        private EndPoint Endpoint;
        private ShmemSampleSource _SampleSource;
        private SharedMem NetShmemSink;
        private Thread TransferThread;

        private HiQSDRControl HiQControl;
        private bool StopTransfers = false;

        private long CurrentRate = 960000;

        private long SamplesReceived = 0;
        private long AverageRate = 0;
        private DateTime LastTime = DateTime.Now;
        private DateTime LastPacketTime = DateTime.Now;

        private bool FlushData = false;
        
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
            byte[] receiveBuffer = null;
            byte[] outBuffer = new byte[1440 * 4];
            int received = 0;

            try
            {
                while (!StopTransfers)
                {
                    lock (this)
                    {
                        if (HiQControl != null)
                        {
                            received = HiQControl.Receive(ref receiveBuffer, ref Endpoint);
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
                            /* sample count depends on byte mode of the FPGA */
                            int byteMode = HiQControl.ByteMode;
                            int samples = 1440 / (byteMode * 2);

                            for (int sample = 0; sample < samples; sample++)
                            {
                                double I = 50*ByteUtil.getDoubleFromBytes(receiveBuffer, 2 + sample * (byteMode * 2), byteMode, true);
                                double Q = 50*ByteUtil.getDoubleFromBytes(receiveBuffer, 2 + sample * (byteMode * 2) + byteMode, byteMode, true);

                                ByteUtil.putBytesFromDouble(outBuffer, sample * 4, I);
                                ByteUtil.putBytesFromDouble(outBuffer, sample * 4 + 2, Q);

                                SamplesReceived++;
                            }

                            /* writing 16 bit I/Q samples, so 4 byte per sample */
                            NetShmemSink.Write(outBuffer, 0, samples * 4);

                            DateTime now = DateTime.Now;
                            double delta = (now - LastTime).TotalMilliseconds;
                            if (delta > 250)
                            {
                                long rate = (long)((double)SamplesReceived / (delta / 1000.0f));
                                if (AverageRate != 0)
                                {
                                    AverageRate = (9 * AverageRate + rate) / 10;
                                }
                                else
                                {
                                    AverageRate = rate;
                                }
                                LastTime = now;
                                SamplesReceived = 0;

                                BeginInvoke(new Action(() =>
                                {
                                    lblRate.Text = AverageRate.ToString();
                                }));
                            }

                            //double packetDelta = (now - LastPacketTime).TotalMilliseconds;
                            //Log.AddMessage("HiQSDRDeviceControl", "Received Block: " + samples + " samples (" + packetDelta.ToString("0.000") + " ms)");
                            //LastPacketTime = now;
                        }
                        else
                        {
                            NetShmemSink.Write(receiveBuffer, 2, received - 2);
                        }
                    }
                    else if (received > 0)
                    {
                        Thread.Sleep(10);
                    }
                    else 
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            TransferThread = null;
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

        private void txtFreqRx_FrequencyChanged(object sender, EventArgs e)
        {
            if (!Connected || ScanFrequenciesEnabled)
                return;

            SetFrequency(txtFreqRx.Frequency);
        }

        private void txtFreqTx_FrequencyChanged(object sender, EventArgs e)
        {
            if (!Connected || ScanFrequenciesEnabled)
                return;

            SetTxFrequency(txtFreqTx.Frequency);
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
                return SampleSource.SamplesPerBlock;
            }
            set
            {
                SampleSource.SamplesPerBlock = value;
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
            if (SampleSource.SamplesAvailable > SamplesPerBlock * 50)
            {
                FlushData = true;
            }

            if (FlushData)
            {
                SampleSource.Flush();
                FlushData = false;
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
                    FlushData = true;
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
                        HiQControl.SetRxRate((int)CurrentRate);
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
                FlushData = true;
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
            get { return 0; }
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

                txtFreqRx.Frequency = frequency;
                HiQControl.RxFrequency = frequency;

                if (FrequencyChanged != null)
                    FrequencyChanged(this, null);

                FlushData = true;
                return true;
            }
        }

        public bool SetTxFrequency(long frequency)
        {
            lock (this)
            {
                if (HiQControl == null)
                {
                    return false;
                }

                txtFreqTx.Frequency = frequency;
                HiQControl.TxFrequency = frequency;

                if (FrequencyChanged != null)
                    FrequencyChanged(this, null);

                FlushData = true;
                return true;
            }
        }

        public long GetFrequency()
        {
            return HiQControl.RxFrequency;
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

        private void cmbRate_TextChanged(object sender, EventArgs e)
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
                SetStatus(eStatus.Connecting);

                Thread t = new Thread(() =>
                {
                    IPAddress host;

                    try
                    {
                        if (!IPAddress.TryParse(txtHost.Text, out host))
                        {
                            host = Dns.GetHostEntry(txtHost.Text).AddressList[0];
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception: " + ex.Message);
                        SetStatus(eStatus.Ready);
                        return;
                    }

                    try
                    {
                        lock (this)
                        {
                            HiQControl = new HiQSDRControl(host);
                            HiQControl.StopTransfer();
                        }

                        if (HiQControl.FirmwareVersion < 0)
                        {
                            HiQControl.Close();
                            HiQControl = null;

                            MessageBox.Show("Device does not respond");
                            SetStatus(eStatus.Ready);
                            return;
                        }

                        SetStatus(eStatus.Connected); ;

                        if (FrequencyChanged != null)
                            FrequencyChanged(this, null); ;

                        if (SamplingRateChanged != null)
                            SamplingRateChanged(this, null);

                        BeginInvoke(new Action(() =>
                        {
                            radioAcqStream.Checked = true;
                        }));
                        StartStreamRead();
                    }
                    catch (Exception ex)
                    {
                        HiQControl.Close();
                        HiQControl = null;

                        MessageBox.Show("Exception: " + ex.Message);
                        SetStatus(eStatus.Ready);
                        return;
                    }

                });

                t.Start();
            }
            else
            {
                SetStatus(eStatus.Disconnecting);

                lock (this)
                {
                    HiQControl.Close();
                    HiQControl = null;
                }

                SetStatus(eStatus.Ready);
            }
        }

        private void SetStatus(eStatus eStatus)
        {
            BeginInvoke(new Action(() =>
            {
                switch (eStatus)
                {
                    case HiQSDRDeviceControl.eStatus.Ready:
                        radioAcqOff.Checked = true;
                        lblFirmware.Text = "N/A";
                        btnConnect.Text = "Connect";
                        Enabled = true;
                        break;

                    case HiQSDRDeviceControl.eStatus.Connecting:
                        radioAcqOff.Checked = true;
                        lblFirmware.Text = "N/A";
                        btnConnect.Text = "Connecting";
                        Enabled = false;
                        break;

                    case HiQSDRDeviceControl.eStatus.Connected:
                        lblName.Text = "\"" + HiQControl.DeviceInfo.Name + "\"";
                        lblSerial.Text = "\"" + HiQControl.DeviceInfo.Serial + "\"";
                        lblFirmware.Text = "v1." + HiQControl.FirmwareVersion;
                        txtFreqRx.Frequency = HiQControl.RxFrequency;
                        txtFreqTx.Frequency = HiQControl.TxFrequency;
                        btnConnect.Text = "Disconnect";
                        Enabled = true;
                        break;

                    case HiQSDRDeviceControl.eStatus.Disconnecting:
                        radioAcqOff.Checked = true;
                        lblFirmware.Text = "N/A";
                        btnConnect.Text = "Disconnecting";
                        Enabled = false;
                        break;
                }
            }));
        }

        private void txtHost_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                btnConnect_Click(null, null);
            }
        }

        private void chkTxPtt_CheckedChanged(object sender, EventArgs e)
        {
            HiQControl.SetTxPtt(chkTxPTT.Checked);
        }

        private void chkTxCw_CheckedChanged(object sender, EventArgs e)
        {
            HiQControl.SetTxCw(chkTxCw.Checked);
        }

        private void chkTxOther_CheckedChanged(object sender, EventArgs e)
        {
            HiQControl.SetTxOther(chkTxOther.Checked);
        }

        private void txtTxLevel_ValueChanged(object sender, EventArgs e)
        {
            HiQControl.SetTxLevel((int)txtTxLevel.Value);
        }

        private void txtAttenuation_ValueChanged(object sender, EventArgs e)
        {
            HiQControl.SetAttenuation((int)txtAttenuation.Value);
        }
    }
}
