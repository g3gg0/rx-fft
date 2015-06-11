using RX_FFT.Components.GDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace LibRXFFT.Libraries.HiQ_SDR
{
    public class SNDP
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DiscoverMessage
        {
            public byte LengthLow;
            public byte LengthHigh;
            public byte KeyLow;
            public byte KeyHigh;
            public byte Operation;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Serial;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] IpAddressBytes;
            public ushort Port;

            public byte CustomField;

            public IPAddress IPAddress
            {
                get
                {
                    byte[] buf = new byte[4];

                    buf[0] = IpAddressBytes[3];
                    buf[1] = IpAddressBytes[2];
                    buf[2] = IpAddressBytes[1];
                    buf[3] = IpAddressBytes[0];

                    return new IPAddress(buf);
                }
            }
        }

        private const int RequestPort = 48321;
        private const int SearcherPort = 48322;

        private EndPoint LocalEndpoint = null;
        private EndPoint BroadcastEndPoint = null;
        private Socket SearchSocket = null;
        private SocketAsyncEventArgs AsyncReceive;
        private byte[] ReceiveBuffer = new byte[1024];

        private Dictionary<string, DiscoverMessage> DiscoveredDevices = new Dictionary<string, DiscoverMessage>();
        private Thread DiscoverThread;

        public static SNDP Instance = new SNDP();

        public SNDP()
        {
            LocalEndpoint = new IPEndPoint(IPAddress.Any, SearcherPort);
            BroadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, RequestPort);

            SearchSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            SearchSocket.Bind(LocalEndpoint);

            SearchSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            SearchSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            SearchSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5);

            AsyncReceive = new SocketAsyncEventArgs();
            AsyncReceive.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            AsyncReceive.SetBuffer(ReceiveBuffer, 0, ReceiveBuffer.Length);
            AsyncReceive.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
            {
                if (e.SocketError == SocketError.Success)
                {
                    HandlePacket(e.Buffer, e.Offset, e.BytesTransferred);
                }
            });

            SearchSocket.ReceiveFromAsync(AsyncReceive);
        }


        public DiscoverMessage[] Devices
        {
            get
            {
                lock(DiscoveredDevices)
                {
                    return DiscoveredDevices.Values.ToArray<DiscoverMessage>();
                }
            }
        }

        private void HandlePacket(byte[] buffer, int offset, int length)
        {
            DiscoverMessage msg = RawDeserialize<DiscoverMessage>(buffer, offset);

            if (DiscoveredDevices.ContainsKey(msg.Name))
            {
                DiscoveredDevices[msg.Name] = msg;
            }
            else
            {
                DiscoveredDevices.Add(msg.Name, msg);
            }
        }

        public void Discover()
        {
            DiscoverMessage msg = new DiscoverMessage();

            msg.LengthLow = 56;
            msg.LengthHigh = 0;
            msg.KeyLow = 0x5A;
            msg.KeyHigh = 0xA5;

            msg.Operation = 0;
            msg.Name = "";
            msg.Serial = "";

            byte[] buf = RawSerialize(msg);

            SearchSocket.SendTo(buf, BroadcastEndPoint);
        }    

        /// <summary>
        /// converts byte[] to struct
        /// </summary>
        public static T RawDeserialize<T>(byte[] rawData, int position)
        {
            int rawsize = Marshal.SizeOf(typeof(T));
            if (rawsize > rawData.Length - position)
                throw new ArgumentException("Not enough data to fill struct. Array length from position: " + (rawData.Length - position) + ", Struct length: " + rawsize);
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, position, buffer, rawsize);
            T retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
            Marshal.FreeHGlobal(buffer);
            return retobj;
        }

        /// <summary>
        /// converts a struct to byte[]
        /// </summary>
        public static byte[] RawSerialize(object anything)
        {
            int rawSize = Marshal.SizeOf(anything);
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(anything, buffer, false);
            byte[] rawDatas = new byte[rawSize];
            Marshal.Copy(buffer, rawDatas, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return rawDatas;
        }
    }
}
