using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using LibRXFFT.Libraries;
using RX_FFT.Components.GDI;
using System.Threading;
using System.Globalization;

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
    public enum KrakenJobStatus
    {
        Unknown,
        Submitted,
        Accepted,
        Queued,
        Processing,
        NotFound,
        Found,
        Cancelled,
        Error
    };

    public class KrakenClient
    {
        private TcpClient DataClient = null;
        private Stream DataStream = null;
        private TcpClient ControlClient = null;
        private Stream ControlStream = null;
        private object ControlStreamLock = new object();

        private int Port = 8866;
        private const int ControlTimeout = 10000;
        private const int DataTimeout = 15 * 60 * 1000;


        /* public status values */
        public bool Logging = false;
        public int RequestId = -1;
        public int SearchDuration = 0;
        public string Hostname = "";
        public KrakenJobStatus KrakenJobStatus = KrakenJobStatus.Unknown;

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
                            Log.AddMessage("KrakenClient", "Cached as failed (#" + pos + ")");
                            storedResult = null;
                            return true;
                        }
                        else
                        {
                            Log.AddMessage("KrakenClient", "Cached as " + pair.Value + " (#" + pos + ")");
                            return ByteUtil.BytesFromString(pair.Value, ref storedResult);
                        }
                    }
                }
            }

            return false;
        }

        public KrakenClient(string host)
        {
            string[] splits = host.Split(':');

            if (splits.Length == 2)
            {
                Hostname = splits[0];
                int.TryParse(splits[1], out Port);
            }
            else
            {
                Hostname = host;
            }
        }

        public bool Connected
        {
            get
            {
                return DataStream != null;
            }
        }

        public string Status
        {
            get
            {
                lock (ControlStreamLock)
                {
                    Write("status");

                    string ret = Read(ControlStream, ControlTimeout);

                    if (ret == null)
                    {
                        ReconnectControl();
                        return "(connection error)";
                    }

                    return ret;
                }
            }
        }

        private string Read()
        {
            return Read(DataStream, -1);
        }

        /// <summary>
        /// Reads up to the \n of a line and returns the line without \r or \n.
        /// Returns null on timeout or disconnect
        /// </summary>
        private string Read(Stream stream, int timeout)
        {
            int pos = 0;
            byte[] buffer = new byte[512];

            if (stream == null)
            {
                return null;
            }

            while (pos < buffer.Length)
            {
                stream.ReadTimeout = timeout;

                try
                {
                    int read = stream.ReadByte();

                    if (read >= 0)
                    {
                        buffer[pos] = (byte)read;

                        /* contained a newline? return */
                        if (buffer[pos] == '\n')
                        {
                            /* remove \r also */
                            if (pos > 0 && buffer[pos - 1] == '\r')
                            {
                                pos--;
                            }

                            ASCIIEncoding enc = new ASCIIEncoding();
                            string ret = enc.GetString(buffer, 0, pos);

                            if (Logging)
                            {
                                Log.AddMessage("KrakenClient", "< " + ret);
                            }

                            return ret;
                        }
                        else
                        {
                            pos++;
                        }
                    }
                    else
                    {
                        /* socket closed */
                        return null;
                    }
                }
                catch (ThreadAbortException e)
                {
                    throw e;
                }
                catch (IOException e)
                {
                    /* timeout or connection error */
                    return null;
                }
                catch (Exception e)
                {
                    return null;
                }

                //Thread.Sleep(50);
            }

            /* line exceeded buffer length */

            return null;
        }

        private bool Write(string message)
        {
            return Write(message, DataStream);
        }

        private bool Write(string message, Stream stream)
        {
            /* do nothing if not connected */
            if (stream == null)
            {
                return false;
            }

            try
            {
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] data = enc.GetBytes(message + "\r\n");

                if (Logging)
                {
                    Log.AddMessage("KrakenClient", "> " + message);
                }

                stream.Write(data, 0, data.Length);
            }
            catch (ThreadAbortException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public void ReconnectControl()
        {
            try
            {
                if (ControlClient != null)
                {
                    ControlClient.Close();
                    ControlClient = null;
                }
            }
            catch (ThreadAbortException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
            }

            Connect();
        }

        public void Reconnect()
        {
            lock (ControlStreamLock)
            {
                Disconnect();
                Connect();
            }
        }

        public void Disconnect()
        {
            try
            {
                if (DataClient != null)
                {
                    DataClient.Close();
                }
                if (ControlClient != null)
                {
                    ControlClient.Close();
                }
            }
            catch (ThreadAbortException e)
            {
                throw e;
            }
            catch { }

            DataClient = null;
            DataStream = null;
            ControlClient = null;
            ControlStream = null;
        }

        public bool Connect()
        {
            try
            {
                if (DataClient == null)
                {
                    DataClient = new TcpClient();
                    DataClient.Connect(Hostname, Port);
                    DataStream = DataClient.GetStream();
                }

                if (ControlClient == null)
                {
                    ControlClient = new TcpClient();
                    ControlClient.Connect(Hostname, Port);
                    ControlStream = ControlClient.GetStream();
                }

                if (!Write("", DataStream) || !Write("", ControlStream))
                {
                    Disconnect();
                    return false;
                }
            }
            catch (ThreadAbortException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                /* make sure the connection is closed */
                Disconnect();
                return false;
            }

            return true;
        }

        public byte[] RequestResult(bool[] key1, uint count1, bool[] key2, uint count2)
        {
            string request = "crack " + ByteUtil.BitsToString(key1) + " " + count1 + " " + ByteUtil.BitsToString(key2) + " " + count2;
            byte[] result = new byte[8];

            RequestId = -1;

            //Log.AddMessage("Kraken: > '" + request.Substring(0, 35) + "...'");

            /* do we have data for this request already in cache? */
            if (CheckScanResult(request, ref result))
            {
                return result;
            }

            try
            {
                /* try a few times to get the key cracked */
                for (int tries = 0; tries < 3; tries++)
                {
                    DataStream.Flush();
                    Write(request);
                    KrakenJobStatus = KrakenJobStatus.Submitted;

                    result = GetResult();

                    if (result != null)
                    {
                        /* found valid key, add result and jump out of loop */
                        AddScanResult(request, result);
                        break;
                    }
                    else if (KrakenJobStatus == KrakenJobStatus.NotFound)
                    {
                        /* found no valid key, add result and jump out of loop */
                        AddScanResult(request, result);
                        break;
                    }
                    else
                    {
                        /* there went something wrong */

                        KrakenJobStatus = KrakenJobStatus.Unknown;

                        Log.AddMessage("KrakenClient", "Failed to crack. Reconnecting");
                        Reconnect();

                        /* cancel old job with the new connection */
                        if (RequestId != -1)
                        {
                            Log.AddMessage("KrakenClient", "   + cancel job " + RequestId);
                            Write("cancel " + RequestId);
                            RequestId = -1;
                        }

                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                /* try to cancel job with new connection */
                if (RequestId != -1)
                {
                    Log.AddMessage("KrakenClient", "Aborting. Reconnecting to cancel job" + RequestId);
                    Reconnect();
                    Write("cancel " + RequestId);
                    Disconnect();
                    Log.AddMessage("KrakenClient", "Aborting finished");
                    RequestId = -1;
                }
            }

            RequestId = -1;
            return result;
        }

        public double GetJobProgress()
        {
            return GetJobProgress(RequestId);
        }

        public double GetJobProgress(int jobId)
        {
            /* just return -1 if not connected to a kraken server */
            if (!Connected)
            {
                return -1.0f;
            }

            try
            {
                lock (ControlStreamLock)
                {
                    Write("progress " + jobId, ControlStream);

                    string ret = Read(ControlStream, ControlTimeout);

                    /* receive failed */
                    if (ret == null)
                    {
                        Log.AddMessage("KrakenClient", "Control stream not responding anymore or we got disconnected. Reconnecting.");
                        Reconnect();
                        return -1.0f;
                    }

                    if (ret.StartsWith("221 "))
                    {
                        /* split into literals */
                        string[] fields = ret.Split(' ');
                        if (fields.Length < 2)
                        {
                            return -1.0f;
                        }

                        if (fields[fields.Length - 1] != "%")
                        {
                            return -1.0f;
                        }

                        double progress = -1.0f;

                        double.TryParse(fields[fields.Length - 2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out progress);

                        return progress;
                    }
                    else if (ret.StartsWith("400 "))
                    {
                        /* job not existing anymore, update state if this is our current job */
                        if (RequestId == jobId)
                        {
                            KrakenJobStatus = KrakenJobStatus.Cancelled;
                            RequestId = -1;
                        }
                        return -1.0f;
                    }

                    Log.AddMessage("KrakenClient", "Unexpected 'progress' answer: '" + ret + "'");

                    /* unexpected response */
                    return -1.0f;
                }
            }
            catch (ThreadAbortException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                Log.AddMessage("KrakenClient", "Control stream error. Reconnecting.");
                Reconnect();
                return -1.0f;
            }
        }


        private byte[] GetResult()
        {
            RequestId = -1;

            if (!Connected)
            {
                KrakenJobStatus = KrakenJobStatus.Error;
                return null;
            }

            try
            {
                while (true)
                {
                    /* wait until data arrives */
                    string ret = Read(DataStream, DataTimeout);

                    /* receive failed */
                    if (ret == null)
                    {
                        KrakenJobStatus = KrakenJobStatus.Error;
                        Log.AddMessage("KrakenClient", "Kraken not responding anymore or got disconnected.");
                        return null;
                    }

                    /* valid result */
                    if (ret.StartsWith("200 "))
                    {
                        /* split into literals */
                        KrakenJobStatus = KrakenJobStatus.Found;

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
                        KrakenJobStatus = KrakenJobStatus.Accepted;
                    }
                    else if (ret.StartsWith("101 "))
                    {
                        /* request was queued, get the ID */
                        KrakenJobStatus = KrakenJobStatus.Queued;

                        /* split into literals */
                        string[] fields = ret.Split(' ');
                        if (fields.Length >= 2)
                        {
                            int.TryParse(fields[1], out RequestId);
                        }
                    }
                    else if (ret.StartsWith("102 "))
                    {
                        /* request is being processed */
                        KrakenJobStatus = KrakenJobStatus.Processing;
                    }
                    else if (ret.StartsWith("103 "))
                    {
                        /* match found, kraken is now calculating back */
                    }
                    else if (ret.StartsWith("404 "))
                    {
                        /* key not found */
                        KrakenJobStatus = KrakenJobStatus.NotFound;
                        ParseSearchDuration(ret);
                        return null;
                    }
                    else if (ret.StartsWith("405 "))
                    {
                        /* job was cancelled */
                        KrakenJobStatus = KrakenJobStatus.Cancelled;
                        return null;
                    }
                    else
                    {
                        /* unexpected response */
                        KrakenJobStatus = KrakenJobStatus.Error;
                        Log.AddMessage("KrakenClient", "Unexpected 'crack' answer: '" + ret + "'");

                        return null;
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                /* try to cancel the crack request */
                if (RequestId != -1)
                {
                    KrakenJobStatus = KrakenJobStatus.Cancelled;
                    Write("cancel " + RequestId);
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
