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
        public int CurrentRequestId = -1;
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
                                    if (CurrentRequestId >= 0)
                                    {
                                        Send("progess " + CurrentRequestId);
                                    }
                                    else
                                    {
                                        Send("idle");
                                    }
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

            bool handled = true;

            int code = -1;
            int id = -1;
            string message = "";

            ParseResponseString(msg, out code, out id, out message);

            /* asynchronously handle different responses here */
            switch (code)
            {
                case 100:
                    /* request is getting queued */
                    KrakenJobStatus = KrakenJobStatus.Accepted;
                    break;

                case 101:
                    /* request was queued, get the ID */
                    KrakenJobStatus = KrakenJobStatus.Queued;
                    CurrentRequestId = id;
                    break;

                case 102:
                    /* request is being processed */
                    if (CurrentRequestId == id)
                    {
                        KrakenJobStatus = KrakenJobStatus.Processing;
                    }
                    break;

                case 103:
                    /* match found, kraken is now calculating back */
                    break;

                case 211:
                    /* response to idle command */
                    break;

                case 401: 
                    if (State == eConnState.Ready)
                    {
                        /* not authorized. happens when kraken was restarted. how to retry? */
                        Log.AddMessage(Remote, "Kraken access denied while being already authorized. Was kraken restarted?");
                        CloseConnection();
                    }
                    break;

                case 200:
                case 404:
                case 405:
                    /* check if these are from an earlier request that is still being processed */
                    if (CurrentRequestId == id)
                    {
                        /* thats from our current request. do not ignore this message. */
                        handled = false;
                    }
                    else
                    {
                        Log.AddMessage(Remote, "Received answer for an old request. Ignoring.");
                    }
                    break;

                default:
                    handled = false;
                    break;
            }

            /* if not already handled, let forward to current processing thread */
            if (!handled)
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
            QueueCommand(request, signal, result, int.MaxValue);

            lock (signal)
            {
                if (Monitor.Wait(signal))
                {
                    string ret = result.ToString();

                    int code = -1;
                    int id = -1;
                    string msg = "";

                    ParseResponseString(ret, out code, out id, out msg);

                    switch (code)
                    {
                        case 200:
                                CurrentRequestId = -1;
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

                        case 404:
                                /* key not found */
                                CurrentRequestId = -1;
                                KrakenJobStatus = KrakenJobStatus.NotFound;
                                ParseSearchDuration(ret);
                                return null;

                        case 405:
                                /* job was cancelled */
                                CurrentRequestId = -1;
                                KrakenJobStatus = KrakenJobStatus.Cancelled;
                                return null;

                        default:
                                Log.AddMessage(Remote, "Last message unhandled");
                                break;
                    }
                }
            }

            return null;
        }

        private bool PopFrontValue(string msg, out int value, out string remaining)
        {
            value = -1;
            remaining = msg;

            string[] fields = remaining.Split(' ');

            if (fields.Length > 1 && int.TryParse(fields[0], out value))
            {
                remaining = remaining.Substring(remaining.IndexOf(' ')).Trim();
            }
            return true;
        }

        private bool ParseResponseString(string msg, out int code, out int id, out string remaining)
        {
            code = -1;
            id = -1;
            remaining = "";

            if (PopFrontValue(msg, out code, out remaining))
            {
                switch (code)
                {
                    /* these codes have a request id as next value */
                    case 101:
                    case 102:
                    case 103:
                    case 200:
                    case 404:
                    case 405:
                        if (PopFrontValue(remaining, out id, out remaining))
                        {
                            return true;
                        }
                        break;
                    
                    default:
                        return true;
                }
            }
            return false;
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

            CancelRequest();

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

        internal void CancelRequest()
        {
            /* try to cancel active request */
            if (CurrentRequestId >= 0)
            {
                Log.AddMessage(Remote, "Cancel request " + CurrentRequestId);
                Client.SendMessage(Remote, null, "cancel " + CurrentRequestId);
            }
            else
            {
                Log.AddMessage(Remote, "No requests to cancel");
            }
        }
    }
}
