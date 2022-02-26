using Newtonsoft.Json;
using RX_FFT.Components.GDI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LibRXFFT.Libraries.SampleSources
{
    internal class RtsaHttpIO
    {
        private IPEndPoint Endpoint;
        private Socket ReceiveSocket;
        private DateTime LastStatistics = DateTime.MinValue;
        private long BytesTransferred = 0;


        public double FilterWidth = 0;
        public double SamplingRate;
        public double Frequency;
        public ByteUtil.eSampleFormat SampleFormat = ByteUtil.eSampleFormat.Direct32BitIQFloat;

        public event EventHandler SamplingRateChanged;
        public event EventHandler FilterWidthChanged;
        public event EventHandler FrequencyChanged;


        public RtsaHttpIO()
        {

        }

        public bool ConnectInput(IPAddress addr, int port)
        {
            try
            {
                Endpoint = new IPEndPoint(addr, port);
                ReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ReceiveSocket.ReceiveBufferSize = 128 * 1024 * 1024;

                try
                {
                    /* SIO_LOOPBACK_FAST_PATH */
                    ReceiveSocket.IOControl(-1744830448, new byte[4] { 1, 0, 0, 0 }, null);
                }
                catch (Exception ex)
                {
                }
                ReceiveSocket.Connect(Endpoint);

                StreamWriter sw = new StreamWriter(new NetworkStream(ReceiveSocket));

                sw.Write("GET /stream?format=raw32 HTTP/1.1\n\n");
                sw.Flush();

                while (ReadLine(ReceiveSocket) != "")
                {
                }

                LastStatistics = DateTime.Now;
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        /* https://i.kym-cdn.com/entries/icons/mobile/000/032/744/maneuver.jpg */
        private bool ParseChunk(Socket receiveSocket, out RtsaHeader infos, ref byte[] block, out int payloadStart, out int payloadLength)
        {
            string lengthString = ReadLine(ReceiveSocket);

            int length = int.Parse(lengthString, System.Globalization.NumberStyles.HexNumber);

            if (block.Length < length)
            {
                Array.Resize(ref block, length);
            }
            payloadStart = 0;
            payloadLength = 0;

            receiveSocket.Receive(block, 0, length, SocketFlags.None);
            string dummy = ReadLine(ReceiveSocket);
            string json = "";

            for (int pos = 0; pos < block.Length; pos++)
            {
                if (block[pos] == '\n')
                {
                    json = Encoding.ASCII.GetString(block, 0, pos);
                    payloadStart = pos + 2;
                    payloadLength = block.Length - pos - 2;
                    break;
                }
            }

            RtsaHeader obj = JsonConvert.DeserializeObject<RtsaHeader>(json);
            infos = obj;

            return true;
        }

        internal bool ReceiveInput(ref byte[] receiveBuffer, out int payloadStart, out int payloadLength)
        {
            bool ret = false;

            payloadLength = 0;
            payloadStart = 0;

            if (ReceiveSocket != null)
            {
                DateTime now = DateTime.Now;

                if (LastStatistics == DateTime.MinValue)
                {
                    LastStatistics = now;
                }
                else if ((now - LastStatistics).TotalMilliseconds > 1000)
                {
                    double rate = BytesTransferred / (now - LastStatistics).TotalSeconds;
                    Log.AddMessage("Statistics: " + (long)rate + " bytes/s, " + ((long)(rate / 8)) + " IQ-samples/s");

                    BytesTransferred = 0;
                    LastStatistics = now;
                }

                if (ParseChunk(ReceiveSocket, out RtsaHeader obj, ref receiveBuffer, out payloadStart, out payloadLength))
                {
                    double start = obj.startFrequency;
                    double end = obj.endFrequency;
                    double width = end - start;
                    double center = start + width / 2;

                    ulong rate = 0;
                    
                    if (width > 200000000)
                    {
                        rate = 245759924;
                    }
                    else if (width > 150000000)
                    {
                        rate = 184319943;
                    }
                    else if (width > 100000000)
                    {
                        rate = 122879962;
                    }
                    else if (width > 50000000)
                    {
                        rate = 92159971;
                    }

                    if(FilterWidth != width)
                    {
                        Log.AddMessage(String.Format("[RTSA] Width changed from {0} to {1}.", FilterWidth, width));
                        FilterWidth = width;
                        FilterWidthChanged?.Invoke(this, null);
                    }

                    if (SamplingRate != rate)
                    {
                        Log.AddMessage(String.Format("[RTSA] Rate changed from {0} to {1}.", SamplingRate, rate));
                        SamplingRate = rate;
                        SamplingRateChanged?.Invoke(this, null);
                    }

                    if (Frequency != (long)center)
                    {
                        Log.AddMessage(String.Format("[RTSA] Center frequency changed from {0} to {1}.", Frequency, center));
                        Frequency = (long)center;
                        FrequencyChanged?.Invoke(this, null);
                    }
                    
                    if (receiveBuffer.Length < payloadLength)
                    {
                        Log.AddMessage(String.Format("[RTSA] Block size changed from {0} to {1}.", receiveBuffer.Length, payloadLength));
                        Array.Resize(ref receiveBuffer, payloadLength);
                    }

                    BytesTransferred += payloadLength;

                    ret = true;
                }
            }

            return ret;
        }

        private string ReadLine(Socket sock)
        {
            string line = "";
            while (true)
            {
                byte[] buf = new byte[1];
                if (sock.Receive(buf) != 1)
                {
                    sock.Close();
                    return null;
                }
                if (buf[0] == '\r')
                {
                }
                else if (buf[0] == '\n')
                {
                    return line;
                }
                else
                {
                    line += (char)buf[0];
                }
            }
        }

        public class RtsaAntennaInfo
        {
            public string name;
        }

        public class RtsaHeader
        {
            public double startTime;
            public double endTime;
            public double startFrequency;
            public double endFrequency;
            public int minPower;
            public int maxPower;
            public int sampleSize;
            public int sampleDepth;
            public string payload;
            public string unit;
            public RtsaAntennaInfo antenna;
            public int scale;
            public int samples;
        }

    }
}
