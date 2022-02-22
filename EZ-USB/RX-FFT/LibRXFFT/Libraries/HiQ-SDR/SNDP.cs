using RX_FFT.Components.GDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
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
        private Dictionary<string, DiscoverMessage> DiscoveredDevices = new Dictionary<string, DiscoverMessage>();

        public static SNDP Instance = new SNDP();

        public SNDP()
        {
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

        public bool Assign(IPAddress host)
        {
            return Assign(Devices[0].Name, Devices[0].Serial, host);
        }

        public bool Assign(string name, string serial, IPAddress host)
        {
            var msg = Devices.Where(d => d.Name == name && d.Serial == serial).FirstOrDefault();

            if (msg.Serial != serial)
            {
                return false;
            }

            msg.IpAddressBytes = host.GetAddressBytes().Reverse().ToArray();
            msg.Operation = 2;

            byte[] buf = RawSerialize(msg);

            /* https://stackoverflow.com/questions/22852781/how-to-do-network-discovery-using-udp-broadcast */
            NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces(); //get all network interfaces of the computer

            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, RequestPort); //braodcast IP address, and corresponding port

            foreach (NetworkInterface adapter in nics)
            {
                // Only select interfaces that are Ethernet type and support IPv4 (important to minimize waiting time)
                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet) { continue; }
                if (adapter.Supports(NetworkInterfaceComponent.IPv4) == false) { continue; }
                try
                {
                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                    foreach (var ua in adapterProperties.UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            //SEND BROADCAST IN THE ADAPTER
                            //1) Set the socket as UDP Client
                            Socket bcSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //broadcast socket
                                                                                                                          //2) Set socker options
                            bcSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                            bcSocket.ReceiveTimeout = 300; //receive timout 200ms
                                                           //3) Bind to the current selected adapter
                            IPEndPoint myLocalEndPoint = new IPEndPoint(ua.Address, SearcherPort);
                            bcSocket.Bind(myLocalEndPoint);
                            //4) Send the broadcast data
                            bcSocket.SendTo(buf, ip);

                            //RECEIVE BROADCAST IN THE ADAPTER
                            int BUFFER_SIZE_ANSWER = 1024;
                            byte[] bufferAnswer = new byte[BUFFER_SIZE_ANSWER];
                            do
                            {
                                try
                                {
                                    int recv = bcSocket.Receive(bufferAnswer);
                                    HandlePacket(bufferAnswer, 0, recv);
                                    bcSocket.Close();
                                    return true;
                                }
                                catch { break; }

                            } while (bcSocket.ReceiveTimeout != 0); //fixed receive timeout for each adapter that supports our broadcast
                            bcSocket.Close();
                        }
                    }
                }
                catch { }
            }

            return false;
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

            /* https://stackoverflow.com/questions/22852781/how-to-do-network-discovery-using-udp-broadcast */
            NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces(); //get all network interfaces of the computer

            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, RequestPort); //braodcast IP address, and corresponding port

            foreach (NetworkInterface adapter in nics)
            {
                // Only select interfaces that are Ethernet type and support IPv4 (important to minimize waiting time)
                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet) { continue; }
                if (adapter.Supports(NetworkInterfaceComponent.IPv4) == false) { continue; }
                try
                {
                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                    foreach (var ua in adapterProperties.UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            //SEND BROADCAST IN THE ADAPTER
                            //1) Set the socket as UDP Client
                            Socket bcSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //broadcast socket
                                                                                                                          //2) Set socker options
                            bcSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                            bcSocket.ReceiveTimeout = 200; //receive timout 200ms
                                                           //3) Bind to the current selected adapter
                            IPEndPoint myLocalEndPoint = new IPEndPoint(ua.Address, SearcherPort);
                            bcSocket.Bind(myLocalEndPoint);
                            //4) Send the broadcast data
                            bcSocket.SendTo(buf, ip);

                            //RECEIVE BROADCAST IN THE ADAPTER
                            int BUFFER_SIZE_ANSWER = 1024;
                            byte[] bufferAnswer = new byte[BUFFER_SIZE_ANSWER];
                            do
                            {
                                try
                                {
                                    int recv = bcSocket.Receive(bufferAnswer);
                                    HandlePacket(bufferAnswer, 0, recv);
                                    bcSocket.Close();
                                    return;
                                }
                                catch { break; }

                            } while (bcSocket.ReceiveTimeout != 0); //fixed receive timeout for each adapter that supports our broadcast
                            bcSocket.Close();
                        }
                    }
                }
                catch { }
            }
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
