using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading;
using LibRXFFT.Libraries.Timers;
using LibRXFFT.Libraries.USB_RX.Tuners;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class NetworkSerialTuner : Tuner
    {
        public SerialPortProperties Properties = new SerialPortProperties();
        public bool Debug = true;

        protected TcpClient Client = null;

        protected bool AutoDetectRunning = false;
        protected double TransmitDuration = 0;
        protected long CurrentFrequency = 0;

        private string NewLine = "\r";
        private Thread TransferThread;
        private HighPerformanceCounter TransmitDelay = new HighPerformanceCounter("Serial transmit delay");


        private enum eTransferDirection
        {
            Receive,
            Send,
            None
        }
        private eTransferDirection TransferDirection = eTransferDirection.None;
        private object TransferDone = new object();
        private object TransferLock = new object();
        private string TransferString;
        private bool TransferFailure;
        private bool AutoDetect;

        protected int[] BaudRates = new[] { 2400, 4800, 9600, 19200 };

        public NetworkSerialTuner(string host)
        {
            Hostname = host;
        }

        private bool Connect(string name)
        {
            try
            {
                Client = new TcpClient(name, 23);

                if(!Client.Connected)
                {
                    Client = null;
                    return false;
                }
                StreamReader = new StreamReader(Client.GetStream());
                StreamWriter = new StreamWriter(Client.GetStream());

                IsOpening = true;
                IsOpened = ConnectionCheck();
                IsOpening = false;

                if (!IsOpened)
                {
                    Client.Close();
                    Client = null;
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Client = null;
            }

            return false;
        }

        public virtual bool ConnectionCheck()
        {
            return true;
        }

        private void TransferThreadMain()
        {
            string lastSendString = "";
            try
            {
                lock (TransferLock)
                {
                    while (true)
                    {
                        Monitor.Wait(TransferLock);
                        bool retry = true;

                        for (int tries = 5; retry && (tries > 0); tries--)
                        {
                            /* default: do not retry transmission */
                            retry = false;
                            try
                            {
                                switch (TransferDirection)
                                {
                                    case eTransferDirection.Receive:
                                        TransferString = ReceiveInternal();
                                        TransmitDelay.Stop();
                                        TransmitDuration = TransmitDelay.Duration;
                                        lastSendString = "";
                                        break;

                                    case eTransferDirection.Send:
                                        TransmitDelay.Start();
                                        SendInternal(TransferString);
                                        lastSendString = TransferString;
                                        break;

                                    case eTransferDirection.None:
                                        break;
                                }
                            }
                            catch (TimeoutException e)
                            {
                                RX_FFT.Components.GDI.Log.AddMessage("[SerialPortTuner] Failure: TIMEOUT");
                                if (tries == 0 || lastSendString == "")
                                {
                                    TransferString = "TIMEOUT";
                                    TransferFailure = true;
                                }
                                else
                                {
                                    retry = true;
                                    SendInternal(lastSendString);
                                }
                            }
                            catch (Exception e)
                            {
                                RX_FFT.Components.GDI.Log.AddMessage("[SerialPortTuner] Failure: TRANSFER FAILURE (" + e.ToString() + ")");
                                if (tries == 0 || lastSendString == "")
                                {
                                    TransferString = "TRANSFER FAILURE";
                                    TransferFailure = true;
                                }
                                else
                                {
                                    retry = true;
                                    SendInternal(lastSendString);
                                }
                            }
                        }
                        TransferDirection = eTransferDirection.None;
                        Monitor.Pulse(TransferLock);
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Log.AddMessage("Exception: " + e.ToString());
            }
        }

        public string ReceiveInternal()
        {
            string ret = ReadLine().Trim(new []{'\r', '\n'});
            if (Debug)
            {
                Log.AddMessage("< '" + ret + "'");
            }

            return ret;
        }

        private string ReadLine()
        {
            return StreamReader.ReadLine();
        }

        public void SendInternal(string cmd)
        {
            if (Debug)
            {
                Log.AddMessage("> '" + cmd + "'");
            }
            WriteLine(cmd);
        }

        private void WriteLine(string cmd)
        {
            StreamWriter.WriteLine(cmd);
            StreamWriter.Flush();
        }

        public string Receive()
        {
            return Receive(false);
        }

        public string Receive(bool ignoreErrors)
        {
            if (!IsOpened && !IsOpening)
                return "NOT CONNECTED";

            /* reset failure flag if requested */
            if (ignoreErrors)
            {
                TransferFailure = false;
            }

            if (TransferFailure)
                return TransferString;

            try
            {
                lock (TransferLock)
                {
                    TransferDirection = eTransferDirection.Receive;
                    Monitor.Pulse(TransferLock);

                    while (TransferDirection == eTransferDirection.Receive)
                    {
                        Monitor.Wait(TransferLock, 5);
                    }

                    if (TransferFailure && !ignoreErrors)
                    {
                        RX_FFT.Components.GDI.Log.AddMessage("SerialPortTuner TransferFailure -> DeviceLost");
                        if (DeviceDisappeared != null)
                        {
                            DeviceDisappeared(this, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }

            /* reset failure flag if requested */
            if (ignoreErrors)
            {
                TransferFailure = false;
            }

            return TransferString;
        }


        public void Send(string cmd)
        {
            if (!IsOpened && !IsOpening)
                return;

            if (TransferFailure)
                return;

            try
            {
                lock (TransferLock)
                {
                    TransferDirection = eTransferDirection.Send;
                    TransferString = cmd;
                    Monitor.Pulse(TransferLock);

                    while (TransferDirection == eTransferDirection.Send)
                    {
                        Monitor.Wait(TransferLock, 5);
                    }
                }

                if (TransferFailure && DeviceDisappeared != null)
                {
                    DeviceDisappeared(this, null);
                }
            }
            catch (Exception e)
            {
            }

            return;
        }

        public virtual void CheckReturncode()
        {
        }

        public virtual void SendCommand(string cmd, bool ignoreErrors)
        {
            if (TransferFailure)
                return;

            LastCommand = cmd;
            try
            {
                Send(cmd);
                CheckReturncode();
            }
            catch (Exception e)
            {
                if (!ignoreErrors)
                {
                    TransferFailure = true;
                    if (DeviceDisappeared != null)
                    {
                        DeviceDisappeared(this, null);
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }
        public virtual void SendCommand(string cmd)
        {
            SendCommand(cmd, false);
        }

#region Tuner Members

        public event EventHandler FilterWidthChanged;
        public event EventHandler FrequencyChanged;
        public event EventHandler InvertedSpectrumChanged;
        public event EventHandler DeviceDisappeared;
        public event EventHandler DeviceClosed;
        public event EventHandler DeviceOpened;

        private long Frequency = 0;
        protected bool IsOpened;
        private bool IsOpening;
        protected string LastCommand;
        private string Hostname;
        private StreamReader StreamReader;
        private StreamWriter StreamWriter;

        public virtual bool SetFrequency(long frequency)
        {
            /* reduce load */
            if (frequency == CurrentFrequency)
                return true;

            bool inverted = InvertedSpectrum;

            CurrentFrequency = frequency;
            Frequency = frequency;

            /* check if spectrum changed and notify */
            if (inverted != InvertedSpectrum)
            {
                if (InvertedSpectrumChanged != null)
                {
                    InvertedSpectrumChanged(this, null);
                }
            }
            return true;
        }

        public virtual long GetFrequency()
        {
            return Frequency;
        }

        public virtual bool OpenTuner()
        {
            if (!IsOpened)
            {
                CurrentFrequency = -1;
                TransferFailure = false;

                TransferThread = new Thread(TransferThreadMain);
                TransferThread.Name = "Serial Tuner Data Transfer";
                TransferThread.Start();

                return Connect(Hostname);
            }

            return true;
        }

        public virtual void CloseTuner()
        {
            if (TransferThread != null)
            {
                TransferThread.Abort();
                lock (TransferLock)
                {
                    Monitor.Pulse(TransferLock);
                }
                TransferThread.Join(500);
                TransferThread.Abort();
            }

            if (Client != null)
            {
                try
                {
                    Client.Close();
                }
                catch(Exception e)
                {
                }
            }

            TransferThread = null;
            Client = null;

            IsOpened = false;
        }

        public virtual long FilterWidth { get; private set; }
        public virtual string FilterWidthDescription { get; private set; }
        public virtual bool InvertedSpectrum { get; private set; }
        public virtual string[] Name { get; private set; }
        public virtual string[] Description { get; private set; }
        public virtual string[] Details { get; private set; }
        public virtual long LowestFrequency { get; private set; }
        public virtual long HighestFrequency { get; private set; }
        public virtual long UpperFilterMargin { get; private set; }
        public virtual string UpperFilterMarginDescription { get; private set; }
        public virtual long LowerFilterMargin { get; private set; }
        public virtual string LowerFilterMarginDescription { get; private set; }
        public virtual double Amplification { get; set; }
        public virtual double Attenuation { get; private set; }
        public virtual long IntermediateFrequency { get; private set; }


#endregion

    }
}