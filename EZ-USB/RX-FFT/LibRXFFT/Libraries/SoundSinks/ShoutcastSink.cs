using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using LibRXFFT.Libraries.SoundDevices;
using LibRXFFT.Components.GDI;
using System.Net.Sockets;
using System.Net;
using System.IO;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SoundSinks
{
    public class ShoutcastSink : SoundSink
    {
        private Mp3Writer Mp3Writer = null;
        private TcpListener Listener = null;
        private Thread AcceptThread = null;

        private class ClientInfo
        {
            public Stream Stream;
            public ulong DataSent;
            public bool UpdateMeta;
        }
        private LinkedList<ClientInfo> ClientStreams = new LinkedList<ClientInfo>();

        private byte[] EmptyBuffer = new byte[16];
        private byte[] MetaDataBuffer = new byte[0];

        private string LastMetaData = "";
        private ASCIIEncoding TextEncoding = new ASCIIEncoding();

        public bool SendMetadata = false;
        public int MetaInt = 32768;
        public string Description = "";

        public ShoutcastSink(Control displayControl)
        {
        }


        #region SoundSink Member

        public long SamplingRate
        {
            get;
            set;
        }

        public void Start()
        {
            if (Listener == null)
            {
                try
                {
                    BE_CONFIG mp3Config = new BE_CONFIG(new WaveFormat((int)SamplingRate, 16, 1));
                    
                    mp3Config.format.lhv1.bEnableVBR = 1;
                    mp3Config.format.lhv1.dwMaxBitrate = 128;
                    mp3Config.format.lhv1.nMode = MpegMode.MONO;
                    mp3Config.format.lhv1.nPreset = LAME_QUALITY_PRESET.LQP_AM;
                    
                    Mp3Writer = new Mp3Writer(mp3Config);
                    Listener = new TcpListener(IPAddress.Any, 12121);
                    Listener.Start();
                }
                catch (Exception e)
                {
                    Status = "Failed: " + e.GetType().ToString();
                    return;
                }

                AcceptThread = new Thread(() =>
                    {
                        while (true)
                        {
                            TcpClient client = Listener.AcceptTcpClient();
                            Stream stream = client.GetStream();
                            TextReader reader = new StreamReader(stream);

                            /* wait for request */
                            string line = "";
                            do
                            {
                                line = reader.ReadLine();

                                if (line.ToLower().StartsWith("icy-metadata:"))
                                {
                                    SendMetadata = (line.Substring(13).Trim() == "1");
                                }
                            } while (line != "");

                            string header = "";
                            header += "ICY 200 OK\r\n";
                            header += "icy-notice1:g3gg0.de\r\n";
                            header += "icy-notice2:MP3 Stream Node\r\n";
                            header += "icy-name:RX-FFT MP3 Stream\r\n";
                            header += "icy-genre:development\r\n";
                            header += "icy-url:http://g3gg0.de/\r\n";
                            header += "icy-pub:1\r\n";
                            header += "icy-br:128\r\n";

                            SendMetadata = false;
                            if (SendMetadata)
                            {
                                header += "icy-metaint:" + MetaInt + "\r\n";
                            }
                            else
                            {
                                header += "icy-metaint:0\r\n";
                            }
                            header += "\r\n";

                            byte[] arr = TextEncoding.GetBytes(header);
                            stream.Write(arr, 0, arr.Length);

                            lock (ClientStreams)
                            {
                                ClientInfo info = new ClientInfo();
                                info.Stream = stream;
                                info.DataSent = 0;
                                info.UpdateMeta = true;

                                ClientStreams.AddLast(info);
                                UpdateStatus();
                            }
                        }
                    }
                );

                AcceptThread.Start();
            }
        }

        private void UpdateStatus()
        {
            Status = ClientStreams.Count + " Listeners";
        }

        public void Stop()
        {
            if (Listener != null)
            {
                AcceptThread.Abort();

                Listener.Stop();
                Listener = null;

                lock (ClientStreams)
                {
                    foreach (ClientInfo i in ClientStreams)
                    {
                        i.Stream.Close();
                    }

                    ClientStreams.Clear();
                }
            }
            Status = "(idle)";
        }

        public void Process(double[] data)
        {
            short[] buff = new short[data.Length];

            for (int pos = 0; pos < data.Length; pos++)
            {
                buff[pos] = (short)(data[pos] * short.MaxValue);
            }

            Process(buff);
        }

        public void Process(short[] data)
        {
            byte[] buff = new byte[data.Length * 2];

            for (int pos = 0; pos < data.Length; pos++)
            {
                buff[2 * pos] = (byte)(data[pos] & 0xFF);
                buff[2 * pos + 1] = (byte)(data[pos] >> 8);
            }

            Process(buff);
        }

        public void Process(byte[] data)
        {
            if (Mp3Writer == null)
            {
                return;
            }

            Mp3Writer.Write(data);

            if (Mp3Writer.DataAvailable == 0)
            {
                return;
            }

            LinkedList<ClientInfo> remove = new LinkedList<ClientInfo>();

            /* only send metadata if it has changed */
            string metaData = "StreamTitle='" + Description + "';";
            bool updatedMetaData = (metaData != LastMetaData);
            LastMetaData = metaData;

            lock (ClientStreams)
            {
                if (ClientStreams.Count > 0)
                {
                    foreach (ClientInfo info in ClientStreams)
                    {
                        try
                        {
                            info.UpdateMeta |= updatedMetaData;

                            int nextMetaData = MetaInt - (int)(info.DataSent % (ulong)MetaInt);

                            /* split this buffer? */
                            if (SendMetadata && nextMetaData < Mp3Writer.DataAvailable)
                            {
                                int firstCount = nextMetaData;
                                int secondCount = (Mp3Writer.DataAvailable - firstCount);

                                /* first write the first part */
                                info.Stream.Write(Mp3Writer.m_OutBuffer, 0, firstCount);
                                info.DataSent += (ulong)firstCount;

                                /* only send metadata if it has changed */
                                SendMetaData(info, metaData);

                                /* finally the second part */
                                info.Stream.Write(Mp3Writer.m_OutBuffer, firstCount, secondCount);
                                info.DataSent += (ulong)secondCount;
                            }
                            else
                            {
                                info.Stream.Write(Mp3Writer.m_OutBuffer, 0, Mp3Writer.DataAvailable);
                                info.DataSent += (ulong)Mp3Writer.DataAvailable;
                            }
                        }
                        catch (Exception e)
                        {
                            remove.AddLast(info);
                        }
                    }
                }

                foreach (ClientInfo info in remove)
                {
                    ClientStreams.Remove(info);
                    info.Stream.Close();
                    UpdateStatus();
                }
            }
            Mp3Writer.DataAvailable = 0;
        }

        private void SendMetaData(ClientInfo info, string metaData)
        {
            /* only send metadata if is available */
            if (metaData != null && info.UpdateMeta)
            {
                info.UpdateMeta = false;

                byte[] data = TextEncoding.GetBytes(metaData);
                int lengthByte = ((data.Length + 15) / 16);

                if (MetaDataBuffer.Length != (lengthByte * 16 + 1))
                {
                    Array.Resize<byte>(ref MetaDataBuffer, lengthByte * 16 + 1);
                }

                Array.Clear(MetaDataBuffer, 0, MetaDataBuffer.Length);
                Array.Copy(data, 0, MetaDataBuffer, 1, data.Length);
                MetaDataBuffer[0] = (byte)lengthByte;

                info.Stream.Write(MetaDataBuffer, 0, MetaDataBuffer.Length);
            }
            else
            {
                info.Stream.Write(EmptyBuffer, 0, 1);
            }
        }

        public string Status
        {
            set;
            get;
        }

        string SoundSink.Description
        {
            set 
            { 
                Description = value;
            }
        }


        public DemodulationState.eSquelchState SquelchState
        {
            get;
            set;
        }

        #endregion
    }
}
