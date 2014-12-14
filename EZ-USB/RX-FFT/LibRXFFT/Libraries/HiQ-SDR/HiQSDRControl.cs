using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Threading;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.HiQ_SDR
{
    public class HiQSDRControl
    {
        public const int HIQSDR_RX_PORT = 0xbc77;
        public const int HIQSDR_CTL_PORT = 0xbc78;
        public const int HIQSDR_TX_PORT = 0xbc79;

        public const int RX_CLOCK = 122880000;

        private Semaphore TransferControlData = new Semaphore(0, 100);
        private bool Running = false;

        public int ByteMode = 3;
        private int Preamp = 0;

        private byte[] TransmitBuffer = new byte[22];

        private EndPoint LocalEndpoint = null;
        private EndPoint ReceiveEndpoint = null;
        private EndPoint ControlEndpoint = null;
        private Socket RemoteSocket = null;
        private IPAddress RemoteAddress = null;

        private Thread ControlThread = null;
        public int FirmwareVersion = -1;


        public HiQSDRControl(IPAddress host)
        {
            RemoteAddress = host;

            TransmitBuffer[0] = (byte)'S';
            TransmitBuffer[1] = (byte)'t';

            LocalEndpoint = new IPEndPoint(IPAddress.Any, HIQSDR_RX_PORT);
            ReceiveEndpoint = new IPEndPoint(host, HIQSDR_RX_PORT);
            ControlEndpoint = new IPEndPoint(host, HIQSDR_CTL_PORT);

            RemoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            RemoteSocket.Bind(LocalEndpoint);
            RemoteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5);
            //RemoteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, 0);

            Running = true;
            ControlThread = new Thread(ControlThreadMain);
            ControlThread.Start();

            StopTransfer();

            /* set some default values */
            RxFrequency = 10700000;
            TxFrequency = 10701000;
            
            SetRxRate(960000);
            SetTxRate(8000);

            SetTxLevel(0);
            SetPresel(0);
            SetTxPtt(false);

            /* do a receive to get information about the device */
            byte[] receiveBuffer = new byte[8192];
            EndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);

            for (int loop = 0; loop < 20; loop++)
            {
                int ret = Receive(receiveBuffer, ref endpoint);

                if (ret >= 1442)
                {
                    loop = 0;
                }
                else if (ret > 0)
                {
                    if(FirmwareVersion >= 0)
                    {
                        break;
                    }
                    TransmitControl();
                    Thread.Sleep(20);
                }
                else
                {
                    TransmitControl();
                    Thread.Sleep(20);
                }
            }
        }

        internal void Close()
        {
            if (!Running)
            {
                return;
            }

            try
            {
                /* make sure the device is silent */
                StopTransfer();

                /* stop network threads */
                Running = false;
                TransferControlData.Release();
                if (!ControlThread.Join(50))
                {
                    ControlThread.Abort();
                }

                RemoteSocket.Shutdown(SocketShutdown.Both);
                RemoteSocket.Close();
            }
            catch (Exception e)
            {
                Log.AddMessage(e.Message);
            }
        }

        void ControlThreadMain()
        {
            while (Running)
            {
                TransferControlData.WaitOne();
                
                try
                {
                    RemoteSocket.SendTo(TransmitBuffer, ControlEndpoint);
                }
                catch (Exception e)
                {
                    Log.AddMessage("Exception: " + e.Message);
                }
            }
        }

        private long _RxFrequency = 0;
        public long RxFrequency
        {
            get
            {
                return _RxFrequency;
            }

            set
            {
                _RxFrequency = value;

                uint ph = ((uint)((float)(_RxFrequency) / RX_CLOCK * 0x100000000LU + 0.5)) & 0xffffffff;

                TransmitBuffer[2] = (byte)((ph >> 0) & 0xff);
                TransmitBuffer[3] = (byte)((ph >> 8) & 0xff);
                TransmitBuffer[4] = (byte)((ph >> 16) & 0xff);
                TransmitBuffer[5] = (byte)((ph >> 24) & 0xff);

                TransmitControl();
            }
        }

        private long _TxFrequency = 0;
        public long TxFrequency
        {
            get
            {
                return _TxFrequency;
            }

            set
            {
                _TxFrequency = value;

                uint ph = ((uint)((float)(_TxFrequency) / RX_CLOCK * 0x100000000LU + 0.5)) & 0xffffffff;

                TransmitBuffer[6] = (byte)((ph >> 0) & 0xff);
                TransmitBuffer[7] = (byte)((ph >> 8) & 0xff);
                TransmitBuffer[8] = (byte)((ph >> 16) & 0xff);
                TransmitBuffer[9] = (byte)((ph >> 24) & 0xff);

                TransmitControl();
            }
        }

        public IPAddress Address
        {
            get
            {
                return RemoteAddress;
            }
        }

        void TransmitControl()
        {
            try
            {
                TransferControlData.Release();
            }
            catch (Exception e)
            {
                Log.AddMessage(e.Message);
            }
        }

        public void SetRxRate(long rate)
        {
            switch (rate)
            {
                case 8000:
                case 9600:
                case 12000:
                case 16000:
                case 19200:
                case 24000:
                case 38400:
                    ByteMode = 3;
                    TransmitBuffer[12] = (byte)((2 << 6) | ((RX_CLOCK / 320 / rate) - 1));
                    break;

                case 48000:
                case 60000:
                case 96000:
                case 120000:
                case 192000:
                case 240000:
                case 320000:
                case 384000:
                case 480000:
                case 640000:
                case 960000:
                    ByteMode = 3;
                    TransmitBuffer[12] = (byte)((RX_CLOCK / 64 / rate) - 1);
                    break;

                case 1280000:
                case 1536000:
                case 1920000:
                    ByteMode = 2;
                    TransmitBuffer[12] = (byte)((1 << 6) | ((RX_CLOCK / 16 / rate) - 1));
                    break;

                case 2560000:
                case 3840000:
                    ByteMode = 1;
                    TransmitBuffer[12] = (byte)((3 << 6) | ((RX_CLOCK / 16 / rate) - 1));
                    break;
                default:
                    return;
            }
            TransmitControl();
        }

        private void SetTxRate(long rate)
        {
            TransmitBuffer[11] &= 0x0f;
            switch (rate)
            {
                case 192000:
                    TransmitBuffer[11] |= 0x10;
                    break;
                case 480000:
                    TransmitBuffer[11] |= 0x20;
                    break;
                case 8000:
                    TransmitBuffer[11] |= 0x30;
                    break;
                default:
                    break;
            }
            TransmitControl();
        }

        public void SetTxLevel(int level)
        {
            TransmitBuffer[10] = (byte)level;
            TransmitControl();
        }

        private void SetPresel(int presel)
        {
            TransmitBuffer[14] = (byte)(presel | (Preamp << 4)); 
            TransmitControl();
        }

        public void SetTxCw(bool enabled)
        {
            TransmitBuffer[11] &= 0xFE;
            TransmitBuffer[11] = (byte)(enabled ? 0x01 : 0);
            TransmitControl();
        }

        public void SetTxOther(bool enabled)
        {
            TransmitBuffer[11] &= 0xFD;
            TransmitBuffer[11] = (byte)(enabled ? 0x02 : 0);
            TransmitControl();
        }

        public void SetTxPtt(bool enabled)
        {
            TransmitBuffer[11] &= 0xF7;
            TransmitBuffer[11] = (byte)(enabled ? 0x08 : 0);
            TransmitControl();
        }

        public void SetAnt(bool enabled)
        {
            TransmitBuffer[16] = (byte)(enabled ? 1 : 0);
            TransmitControl();
        }

        public void SetTxDelay(int delay)
        {
            TransmitBuffer[20] = (byte)((delay << 4) & 0xff);
            TransmitControl();
        }

        public void SetAttenuation(int att)
        {
            TransmitBuffer[15] = 0;

            if (att < 0)
            {
                Preamp = 1;
            }
            else
            {
                Preamp = 0;

                if (att >= 20)
                {
                    att -= 20;
                    TransmitBuffer[15] |= 0x10;
                }
                if (att >= 10)
                {
                    att -= 10;
                    TransmitBuffer[15] |= 0x08;
                }
                if (att >= 8)
                {
                    att -= 8;
                    TransmitBuffer[15] |= 0x04;
                }
                if (att >= 4)
                {
                    att -= 4;
                    TransmitBuffer[15] |= 0x02;
                }
                if (att >= 2)
                {
                    att -= 2;
                    TransmitBuffer[15] |= 0x01;
                }
            }
            TransmitControl();
        }

        internal void StopTransfer()
        {
            if(!Running)
            {
                return;
            }
            RemoteSocket.SendTo(new byte[] { (byte)'s', (byte)'s' }, ReceiveEndpoint);
        }

        internal void StartTransfer()
        {
            if (!Running)
            {
                return;
            }
            RemoteSocket.SendTo(new byte[] { (byte)'r', (byte)'r' }, ReceiveEndpoint);
        }

        internal int Receive(byte[] receiveBuffer, ref EndPoint Endpoint)
        {
            if (!Running)
            {
                return 0;
            }

            try
            {
                int ret = RemoteSocket.ReceiveFrom(receiveBuffer, receiveBuffer.Length, SocketFlags.Partial, ref Endpoint);

                if(ret >= 14 && ret <= 100)
                {
                    if (receiveBuffer[0] == 'S' && receiveBuffer[1] == 't')
                    {
                        double rxFreq = receiveBuffer[2] + (receiveBuffer[3] << 8) + (receiveBuffer[4] << 16) + (receiveBuffer[5] << 24);
                        double txFreq = receiveBuffer[6] + (receiveBuffer[7] << 8) + (receiveBuffer[8] << 16) + (receiveBuffer[9] << 24);

                        _RxFrequency = (long)(rxFreq * RX_CLOCK / 0x100000000 + 0.5f);
                        _TxFrequency = (long)(txFreq * RX_CLOCK / 0x100000000 + 0.5f);

                        if (FirmwareVersion < 0)
                        {
                            FirmwareVersion = receiveBuffer[13];
                        }
                        else
                        {
                            if(FirmwareVersion != receiveBuffer[13])
                            {
                                Log.AddMessage("Firmware version changed from v1." + FirmwareVersion + " to v1." + receiveBuffer[13]);
                                FirmwareVersion = receiveBuffer[13];
                            }
                        }
                    }
                    else
                    {
                        Log.AddMessage("Received " + ret + " bytes: " + receiveBuffer[0] + receiveBuffer[1] + "");
                    }
                }
                return ret;
            }
            catch (SocketException se)
            {
                /* if the reception just timed out, continue */
                if (se.ErrorCode == 10060)
                {
                    return 0;
                }
                else
                {
                    throw se;
                }
            }
        }
    }
}