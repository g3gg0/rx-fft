using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using LibRXFFT.Libraries;
using RX_FFT.Components.GDI;
using System.Threading;

/* 
 * Kraken Status codes:
 * 
 *   100 - your request will get queued
 *   101 [id] - your request has been queued with ID [id]
 *   102 [id] - your request [id] is getting processed now
 *   103 [id] [result] [bitpos] - intermediate result found
 *
 *   200 [id] [key] - Kc for [id] was found
 *
 *   400 - invalid request
 *   404 [id] - no key found for this request
 *   
 *   210 - status response
 *   211 - idle response
 *   212 - cancelling positive response
 *   213 - faking response
 *   
 */

namespace GSM_Analyzer
{
    public class KrakenClient
    {
        private TcpClient Client = null;
        private Stream Stream = null;

        public string Hostname = "";
        public int SearchDuration = 0;

        private byte[] ReceiveBuffer = new byte[512];
        private int ReceivePos = 0;

        private static Dictionary<string, string> ScanResults = new Dictionary<string, string>();
        private static string CacheFileName = "KrakenScanCache.txt";

        static KrakenClient()
        {
            LoadCache();
        }

        private static void SaveCache()
        {
            try
            {
                StreamWriter writer = new StreamWriter(CacheFileName);

                lock (ScanResults)
                {
                    foreach (KeyValuePair<string, string> pair in ScanResults)
                    {
                        writer.WriteLine(pair.Value + " : " + pair.Key);
                    }
                }

                writer.Close();
            }
            catch (Exception e)
            {
            }
        }

        private static void LoadCache()
        {
            try
            {
                StreamReader reader = new StreamReader(CacheFileName);

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] split = line.Split(':');

                    if (split.Length == 2)
                    {
                        lock (ScanResults)
                        {
                            ScanResults.Add(split[1].Trim(), split[0].Trim());
                        }
                    }
                }

                reader.Close();
            }
            catch (Exception e)
            {
            }
        }

        private static void AddScanResult(string request, byte[] result)
        {
            lock (ScanResults)
            {
                if (result != null)
                {
                    ScanResults.Add(request, ByteUtil.BytesToString(result));
                }
                else
                {
                    ScanResults.Add(request, "----------------");
                }
                SaveCache();
            }
        }

        private static bool CheckScanResult(string request, ref byte[] storedResult)
        {
            lock (ScanResults)
            {
                int pos = 0;

                foreach (KeyValuePair<string, string> pair in ScanResults)
                {
                    pos++;

                    if (request.Equals(pair.Key))
                    {
                        if ("----------------".Equals(pair.Value))
                        {
                            Log.AddMessage("Kraken: Cached as failed (#" + pos + ")");
                            storedResult = null;
                            return true;
                        }
                        else
                        {
                            Log.AddMessage("Kraken: Cached as " + pair.Value + " (#" + pos + ")");
                            return ByteUtil.BytesFromString(pair.Value, ref storedResult);
                        }
                    }
                }
            }

            return false;
        }

        public KrakenClient(string host)
        {
            Hostname = host;
        }

        public bool Connected
        {
            get
            {
                return Stream != null;
            }
        }

        public string Status
        {
            get
            {
                Write("status");
                return Read();
            }
        }

        private string Read()
        {
            IAsyncResult ar;

            while (ReceivePos < ReceiveBuffer.Length)
            {
                try
                {
                    /* todo: null reference exception */
                    ar = Stream.BeginRead(ReceiveBuffer, ReceivePos, 1, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne())
                    {
                        /* beware - the answer to the next message will be corrupted. */
                        return null;
                    }

                    int length = Stream.EndRead(ar);

                    if (length == 1)
                    {
                        /* contained a newline? return */
                        if (ReceiveBuffer[ReceivePos] == '\n')
                        {
                            /* remove \r also */
                            if (ReceivePos > 0 && ReceiveBuffer[ReceivePos - 1] == '\r')
                            {
                                ReceivePos--;
                            }

                            ASCIIEncoding enc = new ASCIIEncoding();
                            string ret = enc.GetString(ReceiveBuffer, 0, ReceivePos);

                            ReceivePos = 0;

                            return ret;
                        }
                        else
                        {
                            ReceivePos++;
                        }
                    }
                }
                catch (IOException e)
                {
                    Disconnect();
                    return null;
                }
            }

            /* line exceeded buffer length */
            ReceivePos = 0;

            return null;
        }

        private void Write(string message)
        {
            try
            {
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] data = enc.GetBytes(message + "\r\n");

                //Log.AddMessage("Kraken command: " + message);
                Stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Disconnect();
            }
        }


        public void Disconnect()
        {
            try
            {
                Client.Close();
            }
            catch { }

            Client = null;
            Stream = null;
        }

        public bool Connect()
        {
            try
            {
                Client = new TcpClient();
                Client.Connect(Hostname, 8866);
                Stream = Client.GetStream();
                Write("");
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public byte[] RequestResult(bool[] key1, uint count1, bool[] key2, uint count2)
        {
            string request = "crack " + ByteUtil.BitsToString(key1) + " " + count1 + " " + ByteUtil.BitsToString(key2) + " " + count2;
            byte[] result = new byte[8];

            Log.AddMessage("Kraken: > '" + request.Substring(0, 35) + "...'");

            if (CheckScanResult(request, ref result))
            {
                return result;
            }

            Write(request);
            result = GetResult();

            AddScanResult(request, result);

            return result;
        }


        private byte[] GetResult()
        {
            int requestId = -1;

            try
            {
                while (true)
                {
                    string ret = Read();

                    if (!Connected)
                    {
                        return null;
                    }

                    /* receive failed */
                    if (ret == null)
                    {
                        Log.AddMessage("Kraken not responding anymore. Disconnecting.");
                        Disconnect();
                        return null;
                    }
                    else
                    {
                        Log.AddMessage("Kraken: < '" + ret.Replace('\r', '|').Replace('\n', '|') + "'");

                        if (ret != "")
                        {
                            /* valid result */
                            if (ret.StartsWith("200 "))
                            {
                                /* split into literals */
                                string[] fields = ret.Split(' ');
                                if (fields.Length < 3)
                                {
                                    return null;
                                }

                                /* the 3rd literal is the found key */
                                string keystring = fields[2];
                                byte[] key = new byte[8];

                                for (int pos = 0; pos < 8; pos++)
                                {
                                    string byteStr = keystring.Substring(pos * 2, 2);

                                    if (!byte.TryParse(byteStr, System.Globalization.NumberStyles.HexNumber, null, out key[pos]))
                                    {
                                        key = null;
                                        break;
                                    }
                                }

                                ParseSearchDuration(ret);

                                return key;
                            }
                            else if (ret.StartsWith("100 "))
                            {
                                /* request is getting queued */
                            }
                            else if (ret.StartsWith("101 "))
                            {
                                /* request was queued, get the ID */

                                /* split into literals */
                                string[] fields = ret.Split(' ');
                                if (fields.Length >= 2)
                                {
                                    int.TryParse(fields[1], out requestId);
                                }
                            }
                            else if (ret.StartsWith("102 "))
                            {
                                /* request is being processed */
                            }
                            else if (ret.StartsWith("103 "))
                            {
                                /* match found, kraken is now calculating back */
                            }
                            else if (ret.StartsWith("404 "))
                            {
                                ParseSearchDuration(ret);
                                /* key not found */
                                return null;
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                /* try to cancel the crack request */
                if (requestId != -1)
                {
                    Write("cancel " + requestId);
                }
                else
                {
                    Write("cancel");
                }
                throw ex;
            }
        }

        private void ParseSearchDuration(string ret)
        {
            SearchDuration = 0;

            if (ret.Contains("search took"))
            {
                string str = ret.Substring(ret.IndexOf("search took"));
                if (ret != null)
                {
                    string[] parts = str.Split(' ');
                    if (parts.Length >= 3)
                    {
                        int.TryParse(parts[2], out SearchDuration);
                    }
                }
            }
        }
    }
}
