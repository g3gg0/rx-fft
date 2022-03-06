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
        private readonly Dictionary<ulong, (ulong, uint)> WidthRateMap = new Dictionary<ulong, (ulong, uint)>();

        public double FilterWidth = 0;
        public double SamplingRate;
        public double Frequency;
        public ByteUtil.eSampleFormat SampleFormat = ByteUtil.eSampleFormat.Direct32BitIQFloat;

        public event EventHandler SamplingRateChanged;
        public event EventHandler FilterWidthChanged;
        public event EventHandler FrequencyChanged;



        public RtsaHttpIO()
        {
            /* only build table for two rates as these are just doubled frequncy. */
            var widthRatePairs = new (ulong, ulong)[] {
                    //( 76500000,  92159971),
                    //(102000000, 122879962),
                    (153000000, 184319943),
                    (204000000, 245759924)
            };

            foreach (var pair in widthRatePairs)
            {
                /* build the map for all decimations from 1 to 1024 */
                for(int dec = 0; dec < 11; dec++ )
                {
                    ulong width = (pair.Item1 >> dec) / 1000;
                    ulong rate = pair.Item2;
                    WidthRateMap.Add(width, (rate, 1U << dec));
                }
            }
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
                    payloadLength = length - pos - 2;
                    break;
                }
            }

            if((payloadLength & 7) != 0)
            {
                payloadLength &= ~7;
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

            if (ReceiveSocket == null)
            {
                return false;
            }
            DateTime now = DateTime.Now;

            if (LastStatistics == DateTime.MinValue)
            {
                LastStatistics = now;
            }
            else if ((now - LastStatistics).TotalMilliseconds > 1000)
            {
                double xferRate = BytesTransferred / (now - LastStatistics).TotalSeconds;
                Log.AddMessage("RtsaHttpIO: " + (long)xferRate + " bytes/s, " + ((long)(xferRate / 8)) + " IQ-samples/s");

                BytesTransferred = 0;
                LastStatistics = now;
            }

            if (!ParseChunk(ReceiveSocket, out RtsaHeader obj, ref receiveBuffer, out payloadStart, out payloadLength))
            {
                return false;
            }

            /* process metadata */
            double start = obj.startFrequency;
            double end = obj.endFrequency;
            double width = end - start;
            double center = start + width / 2;

            double rate = SamplingRate;

            if (FilterWidth != width)
            {
                Log.AddMessage(String.Format("[RTSA] Width changed from {0} to {1}.", FilterWidth, width));

                ulong widthRounded = (ulong)(width / 1000);
                if (!WidthRateMap.ContainsKey(widthRounded))
                {
                    rate = 0;
                }
                else
                {
                    rate = 2 * WidthRateMap[widthRounded].Item1 / WidthRateMap[widthRounded].Item2;
                }

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
