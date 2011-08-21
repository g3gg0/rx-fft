using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Threading;

namespace LibRXFFT.Libraries.HiQ_SDR
{
    public class HiQSDRControl
    {
        public const int HIQSDR_RX_PORT = 0xbc77;
        public const int HIQSDR_CTL_PORT = 0xbc78;
        public const int HIQSDR_TX_PORT = 0xbc79;

        public const int RX_CLOCK = 122880000;

        private bool Running = true;

        private byte[] TransmitBuffer = new byte[14];

        private EndPoint LocalEndpoint = null;
        private EndPoint ReceiveEndpoint = null;
        private EndPoint ControlEndpoint = null;
        private Socket RemoteSocket = null;
        private IPAddress RemoteAddress = null;

        private Thread ControlThread = null;


        public HiQSDRControl(IPAddress host)
        {
            RemoteAddress = host;

            TransmitBuffer[0] = (byte)'S';
            TransmitBuffer[1] = (byte)'t';

            try
            {
                LocalEndpoint = new IPEndPoint(IPAddress.Any, HIQSDR_RX_PORT);
                ReceiveEndpoint = new IPEndPoint(host, HIQSDR_RX_PORT);
                ControlEndpoint = new IPEndPoint(host, HIQSDR_CTL_PORT);

                RemoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                RemoteSocket.Bind(LocalEndpoint);
                RemoteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5);
                //RemoteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, 0);

                /* set some default values */
                SetRXFreq(11000000);
                SetSampleRate(960000);
                SetPtt(false);

                ControlThread = new Thread(ControlThreadMain);
                ControlThread.Start();

            }
            catch (Exception e)
            {
            }
        }

        Semaphore TransferControlData = new Semaphore(0, 100);
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
                }
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
            TransferControlData.Release();
        }

        ~HiQSDRControl()
        {
            try
            {
                StopTransfer();
            }
            catch (Exception e)
            {
            }
        }

        public void SetRXFreq(int f)
        {
            uint ph = ((uint)((float)(f) / RX_CLOCK * 0x100000000LU + 0.5)) & 0xffffffff;

            TransmitBuffer[2] = (byte)((ph >> 0) & 0xff);
            TransmitBuffer[3] = (byte)((ph >> 8) & 0xff);
            TransmitBuffer[4] = (byte)((ph >> 16) & 0xff);
            TransmitBuffer[5] = (byte)((ph >> 24) & 0xff);

            TransmitControl();
        }

        public void SetTXFreq(int f)
        {
            uint ph = ((uint)((float)(f) / RX_CLOCK * 0x100000000LU + 0.5)) & 0xffffffff;

            TransmitBuffer[6] = (byte)((ph >> 0) & 0xff);
            TransmitBuffer[7] = (byte)((ph >> 8) & 0xff);
            TransmitBuffer[8] = (byte)((ph >> 16) & 0xff);
            TransmitBuffer[9] = (byte)((ph >> 24) & 0xff);

            TransmitControl();
        }

        public void SetSampleRate(int rate)
        {
            switch (rate)
            {
                case 48000:
                case 96000:
                case 192000:
                case 240000:
                case 384000:
                case 480000:
                case 960000:
                    TransmitBuffer[12] = (byte)((RX_CLOCK / (64 * rate)) - 1);
                    break;
                default:
                    return;
            }
            TransmitControl();
        }

        public void SetPtt(bool enabled)
        {
            TransmitBuffer[11] = (byte)(enabled ? 2 : 0);
            TransmitControl();
        }

        public void SetTXLevel(int level)
        {
            TransmitBuffer[10] = (byte)level;
            TransmitControl();
        }
        
        internal void StopTransfer()
        {
            RemoteSocket.SendTo(new byte[] { (byte)'s', (byte)'s' }, ReceiveEndpoint);
        }

        internal void StartTransfer()
        {
            RemoteSocket.SendTo(new byte[] { (byte)'r', (byte)'r' }, ReceiveEndpoint);
        }

        internal int Receive(byte[] receiveBuffer, ref EndPoint Endpoint)
        {
            try
            {
                return RemoteSocket.ReceiveFrom(receiveBuffer, receiveBuffer.Length, SocketFlags.Partial, ref Endpoint);
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

        internal void Close()
        {
            Running = false;
            TransferControlData.Release();
        }
    }
}