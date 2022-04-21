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
using LibRXFFT.Libraries.Aaronia.Spectran;

namespace LibRXFFT.Components.DeviceControls
{
    public partial class SpectranDeviceControl : Form, DeviceControl
    {
        private enum eStatus
        {
            Ready,
            Connecting,
            Connected,
            Disconnecting
        }

        private ByteUtil.eSampleFormat DataFormat = ByteUtil.eSampleFormat.Direct32BitIQFloat;

        private EndPoint Endpoint;
        private ShmemSampleSource _SampleSource;
        private SharedMem NetShmemSink;
        private Thread TransferThread;

        private SpectranDevice Spectran;
        private bool StopTransfers = false;

        private long CurrentRate = 960000;

        private long SamplesReceived = 0;
        private long AverageRate = 0;
        private DateTime LastTime = DateTime.Now;
        private DateTime LastPacketTime = DateTime.Now;

        private bool FlushData = false;
        
        public SpectranDeviceControl()
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
            int received = 0;
            byte[] floatBuffer = null;

            try
            {
                while (!StopTransfers)
                {
                    lock (this)
                    {
                        if (Spectran != null)
                        {
                            received = Spectran.GetPacketRaw(ref floatBuffer);
                            //Spectran.Flush();
                        }
                        else
                        {
                            received = 0;
                        }
                    }

                    if (received > 0)
                    {
                        NetShmemSink.Write(floatBuffer, 0, received);

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
                if (Spectran != null)
                {
                    Spectran.CenterFrequency = 1000000;
                    Spectran.ReceiverChannel = "Rx1";
                    Spectran.SpanFrequency = 50000000;
                    Spectran.DemodCenterFrequency = 1000000;
                    Spectran.DemodSpanFrequency = 2000000;
                    Spectran.TransmitterGain = 0;
                    Spectran.Start();
                }
            }
        }

        public void StartStreamRead()
        {
            lock (this)
            {
                if (Spectran != null)
                {
                    Spectran.CenterFrequency = 1000000;
                    Spectran.ReceiverChannel = "Rx1";
                    Spectran.SpanFrequency = 50000000;
                    Spectran.DemodCenterFrequency = 1000000;
                    Spectran.DemodSpanFrequency = 2000000;
                    Spectran.TransmitterGain = 0;
                    Spectran.Decimation = 4;

                    Spectran.Start();
                    FlushData = true;
                    TransferMode = eTransferMode.Stream;
                }
            }
        }

        public void StopRead()
        {
            lock (this)
            {
                if (Spectran != null)
                {
                    Spectran.Stop();
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
                    if (Spectran != null)
                    {
                        Spectran.SpanFrequency = ((int)CurrentRate);
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
        public event EventHandler DeviceOpened;
        public event EventHandler DeviceClosed;

        public bool OpenTuner()
        {
            Show();
            return true;
        }

        public void CloseTuner()
        {
            DeviceClosed?.Invoke(this, EventArgs.Empty);

            lock (this)
            {
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
                string msg = "Spectran V6 ";
                lock (this)
                {
                    if (Spectran == null)
                    {
                        msg += "<not connected>";
                    }
                    else
                    {
                        msg += Spectran.DeviceInfo.serialNumber;
                    }
                    return new[] { msg };
                }
            }
        }

        public bool SetFrequency(long frequency)
        {
            lock (this)
            {
                if (Spectran == null)
                {
                    return false;
                }

                txtFreqRx.Frequency = frequency;
                Spectran.CenterFrequency = frequency;

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
                if (Spectran == null)
                {
                    return false;
                }

                txtFreqTx.Frequency = frequency;
                Spectran.CenterFrequency = frequency;

                if (FrequencyChanged != null)
                    FrequencyChanged(this, null);

                FlushData = true;
                return true;
            }
        }

        public long GetFrequency()
        {
            return (long)Spectran.CenterFrequency;
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
            if (Spectran == null)
            {
                SetStatus(eStatus.Connecting);

                Thread t = new Thread(() =>
                {

                    try
                    {
                        lock (this)
                        {
                            Spectran = new SpectranDevice();
                            Spectran.TryOpen();
                        }

                        SetStatus(eStatus.Connected);
                        DeviceOpened?.Invoke(this, EventArgs.Empty);

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
                        Spectran.Stop();
                        Spectran = null;

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
                    Spectran.Close();
                    Spectran = null;
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
                    case SpectranDeviceControl.eStatus.Ready:
                        radioAcqOff.Checked = true;
                        lblFirmware.Text = "N/A";
                        btnConnect.Text = "Connect";
                        Enabled = true;
                        break;

                    case SpectranDeviceControl.eStatus.Connecting:
                        radioAcqOff.Checked = true;
                        lblFirmware.Text = "N/A";
                        btnConnect.Text = "Connecting";
                        Enabled = false;
                        break;

                    case SpectranDeviceControl.eStatus.Connected:
                        lblName.Text = "Spectran V6";
                        lblSerial.Text = "\"" + Spectran.DeviceInfo.serialNumber + "\"";
                        txtFreqRx.Frequency = (long)Spectran.CenterFrequency;
                        txtFreqTx.Frequency = (long)Spectran.CenterFrequency;
                        btnConnect.Text = "Disconnect";
                        Enabled = true;
                        break;

                    case SpectranDeviceControl.eStatus.Disconnecting:
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

        private void txtTxLevel_ValueChanged(object sender, EventArgs e)
        {
            Spectran.TransmitterGain = ((int)txtTxLevel.Value);
        }

        private void txtAttenuation_ValueChanged(object sender, EventArgs e)
        {
        }
    }
}
