using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JabberNET;
using System.Threading;
using RX_FFT.Components.GDI;
using LibRXFFT.Libraries;

namespace GSM_Analyzer
{
    public class KrakenNetConnection
    {
        class CommandEntry
        {
            public string Request;
            public object Signal;
            public StringBuilder Result;
            public int Timeout;
        }

        public enum eConnState
        {
            Unknown,
            Ready,
            Closed
        }

        private JabberClient Client;
        private string Remote;

        private Queue<CommandEntry> CommandQueue = new Queue<CommandEntry>();
        
        private object ProcessAnswerSignal = new object();
        private string ProcessAnswerString = "";

        private object ProcessingThreadSignal = new object();
        private Thread ProcessingThread;
        private bool Processing = true;

        public KrakenJobStatus KrakenJobStatus = KrakenJobStatus.Unknown;
        public int RequestId = -1;
        public eConnState State = eConnState.Unknown;

        private DateTime LastResponse = DateTime.MinValue;
        private int IdleTimeout = 10000;
        private int CommandTimeout = 20000;
        private bool Logging = true;
        

        public KrakenNetConnection(JabberClient client, string remote)
        {
            Client = client;
            Remote = remote;

            ProcessingThread = new Thread(ProcessingFunc);
            ProcessingThread.Name = "XMPP: '" + remote + "' Connection";
            ProcessingThread.Start();
        }

        private void ProcessingFunc()
        {
            CommandEntry currentEntry = null;

            while (Processing)
            {
                try
                {
                    if (currentEntry != null)
                    {
                        lock (ProcessAnswerSignal)
                        {
                            /* wait until response arrives or we get a timeout */
                            if (!Monitor.Wait(ProcessAnswerSignal, Math.Min(currentEntry.Timeout, IdleTimeout)))
                            {
                                /* did we hit the timeout for the current command? */
                                if (currentEntry.Timeout > IdleTimeout)
                                {
                                    /* not yet, check when the last response was */
                                    currentEntry.Timeout -= IdleTimeout;

                                    if ((DateTime.Now - LastResponse).TotalMilliseconds > CommandTimeout)
                                    {
                                        /* the last answer was e.g. 10 sec ago. seems we are disconnected */
                                        Log.AddMessage(Remote, "Kraken did not response within the configured command timeout. Closing connection.");
                                        CloseConnection();
                                        return;
                                    }

                                    /* everything okay, send another idle command */
                                    Send("idle");
                                }
                                else
                                {
                                    currentEntry.Timeout = 0;

                                    /* in case of timeout */
                                    lock (currentEntry.Signal)
                                    {
                                        currentEntry.Result.Length = 0;
                                        Monitor.Pulse(currentEntry.Signal);
                                    }
                                    currentEntry = null;
                                }
                            }
                            else
                            {
                                /* answer received */
                                lock (currentEntry.Signal)
                                {
                                    currentEntry.Result.Append(ProcessAnswerString);
                                    Monitor.Pulse(currentEntry.Signal);
                                }
                                currentEntry = null;
                            }
                        }
                    }
                    else
                    {
                        if (CommandQueue.Count == 0)
                        {
                            lock (ProcessingThreadSignal)
                            {
                                Monitor.Wait(ProcessingThreadSignal, 500);
                            }
                        }
                        else
                        {
                            currentEntry = CommandQueue.Dequeue();
                            Send(currentEntry.Request);
                        }
                    }
                }
                catch (Exception e)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void Send(string msg)
        {
            if (Logging)
            {
                Log.AddMessage(Remote, "> " + msg);
            }
            Client.SendMessage(Remote, null, msg);
        }


        public void HandleMessage(string msg)
        {
            if (Logging)
            {
                Log.AddMessage(Remote, "< " + msg);
            }
            LastResponse = DateTime.Now;

            if (msg.StartsWith("100 "))
            {
                /* request is getting queued */
                KrakenJobStatus = KrakenJobStatus.Accepted;
            }
            else if (msg.StartsWith("101 "))
            {
                /* request was queued, get the ID */
                KrakenJobStatus = KrakenJobStatus.Queued;

                /* split into literals */
                string[] fields = msg.Split(' ');
                if (fields.Length >= 2)
                {
                    int.TryParse(fields[1], out RequestId);
                }
            }
            else if (msg.StartsWith("102 "))
            {
                /* request is being processed */
                KrakenJobStatus = KrakenJobStatus.Processing;
            }
            else if (msg.StartsWith("103 "))
            {
                /* match found, kraken is now calculating back */
            }
            else if (msg.StartsWith("211 "))
            {
                /* response to idle command */
            }
            else if (State == eConnState.Ready && msg.StartsWith("401 "))
            {
                /* not authorized. happens when kraken was restarted. how to retry? */
                Log.AddMessage(Remote, "Kraken access denied while being already authorized. Was kraken restarted?");
                CloseConnection();
            }
            else
            {
                lock (ProcessAnswerSignal)
                {
                    ProcessAnswerString = msg;
                    Monitor.Pulse(ProcessAnswerSignal);
                }
            }
        }

        public bool Available
        {
            get
            {
                for (int tries = 0; tries < 3; tries++)
                {
                    object signal = new object();
                    StringBuilder result = new StringBuilder();

                    QueueCommand("auth banane", signal, result, 1000);

                    lock (signal)
                    {
                        if (Monitor.Wait(signal))
                        {
                            if (result.ToString().StartsWith("201"))
                            {
                                State = eConnState.Ready;
                                return true;
                            }
                            else if (result.ToString().StartsWith("401"))
                            {
                                Log.AddMessage(Remote, "Kraken access denied");
                                CloseConnection();
                                return false;
                            }
                            else
                            {
                                Log.AddMessage(Remote, "Kraken is not responding");
                                CloseConnection();
                                return false;
                            }
                        }
                    }
                }

                CloseConnection();
                return false;
            }
        }

        public double GetJobProgress(int jobId)
        {
            for (int tries = 0; tries < 3; tries++)
            {
                object waitMutex = new object();
                StringBuilder result = new StringBuilder();

                QueueCommand("progress " + jobId, waitMutex, result, 1000);

                if (Monitor.Wait(waitMutex))
                {
                    /* parse result */
                }
            }

            CloseConnection();
            return -1;
        }


        private void QueueCommand(string request, object signal, StringBuilder result, int timeout)
        {
            CommandEntry entry = new CommandEntry();
             
            entry.Request = request;
            entry.Signal = signal;
            entry.Result = result;
            entry.Timeout = timeout;

            CommandQueue.Enqueue(entry);

            /* wake up processing thread */
            lock (ProcessingThreadSignal)
            {
                Monitor.Pulse(ProcessingThreadSignal);
            }
        }


        public byte[] RequestResult(bool[] key1, uint count1, bool[] key2, uint count2)
        {
            string request = "crack " + ByteUtil.BitsToString(key1) + " " + count1 + " " + ByteUtil.BitsToString(key2) + " " + count2;

            object signal = new object();
            StringBuilder result = new StringBuilder();

            KrakenJobStatus = KrakenJobStatus.Submitted;
            QueueCommand(request, signal, result, 5 * 60 * 1000);

            lock (signal)
            {
                if (Monitor.Wait(signal))
                {
                    string ret = result.ToString();

                    if (ret.StartsWith("200 "))
                    {
                        RequestId = -1;
                        KrakenJobStatus = KrakenJobStatus.Found;

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
                    else if (ret.StartsWith("404 "))
                    {
                        /* key not found */
                        RequestId = -1;
                        KrakenJobStatus = KrakenJobStatus.NotFound;
                        ParseSearchDuration(ret);
                        return null;
                    }
                    else if (ret.StartsWith("405 "))
                    {
                        /* job was cancelled */
                        RequestId = -1;
                        KrakenJobStatus = KrakenJobStatus.Cancelled;
                        return null;
                    }
                }
            }

            return null;
        }


        public int SearchDuration = 0;
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

        internal void CloseConnection()
        {
            Processing = false;

            CancelRequests();

            if (ProcessingThread != null)
            {
                ProcessingThread.Abort();
                ProcessingThread = null;
            }

            /* signal all waiting threads */
            foreach (CommandEntry entry in CommandQueue)
            {
                lock (entry.Signal)
                {
                    Monitor.PulseAll(entry.Signal);
                }
            }
        }

        internal void CancelRequests()
        {
            /* try to cancel active request */
            if (RequestId >= 0)
            {
                Log.AddMessage(Remote, "Cancel request " + RequestId);
                Client.SendMessage(Remote, null, "cancel " + RequestId);
            }
            else
            {
                Log.AddMessage(Remote, "No requests to cancel");
            }
        }
    }
}
