using System;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Components.DeviceControls;
using LibRXFFT.Libraries.ShmemChain;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using LibRXFFT.Components.GDI;

namespace LibRXFFT.Components.DeviceControls
{
    public partial class NetworkDeviceControl : Form, DeviceControl
    {
        private ShmemSampleSource _SampleSource;
        private double _BlocksPerSecond = 20;
        private SharedMem NetShmemSink;
        private Thread TransferThread;

        private EndPoint Endpoint;

        private Socket ReceiveSocket = null;
        private Socket ServerSocket = null;
        
        private bool StopTransfers = false;
        private eConnType ConnectionType = eConnType.None;
        private ByteUtil.eSampleFormat SampleFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;

        private enum eConnType
        {
            None,
            UdpClient,
            TcpClient,
            TcpServer
        };

        public int ShmemChannel
        {
            get { return _SampleSource.ShmemChannel.SrcChan; }
        }

        public NetworkDeviceControl()
        {
            InitializeComponent();

            NetShmemSink = new SharedMem(-2, 0);

            _SampleSource = new ShmemSampleSource("NetworkReader", NetShmemSink.DstChan, 1, 0);
            _SampleSource.InvertedSpectrum = false;
            _SampleSource.SamplingRateChanged += new EventHandler(_SampleSource_SamplingRateChanged);
            _SampleSource.DataFormat = SampleFormat;

            Show();
            UpdateDisplay();
        }


        void SetupConnection(eConnType type, IPAddress addr, int port)
        {
            CloseConnection();

            switch (type)
            {
                case eConnType.UdpClient:
                    Endpoint = new IPEndPoint(addr, port);
                    ReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    ReceiveSocket.Bind(Endpoint);
                    ReceiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5);
                    break;

                case eConnType.TcpClient:
                    Endpoint = new IPEndPoint(addr, port);
                    ReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    ReceiveSocket.Connect(Endpoint);
                    break;

                case eConnType.TcpServer:
                    Endpoint = new IPEndPoint(addr, port);
                    ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    ServerSocket.Bind(Endpoint);
                    ServerSocket.Listen(1);
                    ServerSocket.BeginAccept(new AsyncCallback(AcceptConnection), ServerSocket);

                    break;
            }

            ConnectionType = type;
            StopTransfers = false;
            TransferThread = new Thread(TransferThreadMain);
            TransferThread.Start();
        }

        private void AcceptConnection(IAsyncResult iar)
        {
            try
            {
                ReceiveSocket = ((Socket)iar.AsyncState).EndAccept(iar);
            }
            catch
            {
            }
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

            if (ServerSocket != null)
            {
                ServerSocket.Close();
                ServerSocket = null;
            }

            if (ReceiveSocket != null)
            {
                ReceiveSocket.Close();
                ReceiveSocket = null;
            }
        }

        void TransferThreadMain()
        {
            byte[] receiveBuffer = new byte[8192];
            int received = 0;

            try
            {
                while (!StopTransfers)
                {
                    switch (ConnectionType)
                    {
                        case eConnType.UdpClient:
                            try
                            {
                                received = ReceiveSocket.ReceiveFrom(receiveBuffer, receiveBuffer.Length, SocketFlags.Partial, ref Endpoint);
                            }
                            catch (SocketException se)
                            {
                                /* if the reception just timed out, continue */
                                if (se.ErrorCode == 10060)
                                {
                                    received = 0;
                                }
                                else
                                {
                                    throw se;
                                }
                            }
                            break;

                        case eConnType.TcpClient:
                        case eConnType.TcpServer:
                            if (ReceiveSocket != null)
                            {
                                received = ReceiveSocket.Receive(receiveBuffer);
                            }
                            else
                            {
                                received = 0;
                            }
                            break;
                    }

                    if (received > 0)
                    {
                        NetShmemSink.Write(receiveBuffer, 0, received);
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
                btnStartStop.Text = "Start";
            }));
        }

        void _SampleSource_SamplingRateChanged(object sender, EventArgs e)
        {
            if (SamplingRateChanged != null)
            {
                SamplingRateChanged(this, null);
            }
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

        public eTransferMode TransferMode
        {
            get
            {
                return eTransferMode.Stream;
            }
            set
            {
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
                return _BlocksPerSecond;
            }
            set
            {
                _BlocksPerSecond = value;
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

        }

        public void StartStreamRead()
        {

        }

        public void StopRead()
        {

        }

        #endregion

        #region DigitalTuner Member

        public long SamplingRate
        {
            get
            {
                return (long)SampleSource.OutputSamplingRate;
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
            get { return 1000000000; }
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
                return "artificial limit";
            }
        }

        public string LowerFilterMarginDescription
        {
            get
            {
                return "artificial limit";
            }
        }

        public string FilterWidthDescription
        {
            get
            {
                return "artificial limit";
            }
        }

        string[] LibRXFFT.Libraries.USB_RX.Tuners.Tuner.Name
        {
            get { return new[] {"Shared Memory data source"}; }
        }

        public string[] Description
        {
            get { return new[] {"(none)"}; }
        }

        public string[] Details
        {
            get { return new[] {"(none)"}; }
        }

        public bool SetFrequency(long frequency)
        {
            return true;
        }

        public long GetFrequency()
        {
            return 0;
        }

        public long FilterWidth
        {
            get
            {
                return 0;
            }
        }

        public bool InvertedSpectrum
        {
            get { return false; }
        }
        public bool ScanFrequenciesEnabled { get; set; }

        #endregion

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (TransferThread == null)
            {
                try
                {
                    int port;
                    IPAddress host;

                    try
                    {
                        port = int.Parse(txtPort.Text);
                        host = Dns.GetHostEntry(txtHost.Text).AddressList[0];
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to resolve host name or port: " + ex.GetType().ToString());
                        return;
                    }

                    if (radioTCPClient.Checked)
                    {
                        SetupConnection(eConnType.TcpClient, host, port);
                    }
                    else if (radioTCPServer.Checked)
                    {
                        SetupConnection(eConnType.TcpServer, IPAddress.Any, port);
                    }
                    else if (radioUDPListener.Checked)
                    {
                        SetupConnection(eConnType.UdpClient, IPAddress.Any, port);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to connect: " + ex.GetType().ToString());
                    return;
                }
                btnStartStop.Text = "Stop";
            }
            else
            {
                CloseConnection();
                btnStartStop.Text = "Start";
            }
        }

        private void radioUDPListener_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void radioTCPClient_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void radioTCPServer_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (radioTCPClient.Checked)
            {
                txtHost.Enabled = true;
                txtPort.Enabled = true;
            }
            else if (radioTCPServer.Checked)
            {
                txtHost.Enabled = false;
                txtPort.Enabled = true;
            }
            else if (radioUDPListener.Checked)
            {
                txtHost.Enabled = false;
                txtPort.Enabled = true;
            }
        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            DataFormatDialog dlg = new DataFormatDialog();

            dlg.SamplingRate = SamplingRate;
            dlg.SampleFormat = SampleFormat;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SampleFormat = dlg.SampleFormat;

                _SampleSource.DataFormat = SampleFormat;
                _SampleSource.ForceInputRate(dlg.SamplingRate);
                NetShmemSink.Rate = dlg.SamplingRate * 2;
            }
        }
    }
}
