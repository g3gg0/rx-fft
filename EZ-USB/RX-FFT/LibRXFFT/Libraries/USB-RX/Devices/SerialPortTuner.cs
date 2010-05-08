using System;
using System.IO.Ports;
using System.Threading;
using LibRXFFT.Libraries.Timers;
using LibRXFFT.Libraries.USB_RX.Tuners;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class SerialPortProperties
    {
        public int BaudRate = 115200;

        public int DataBits = 8;
        public Parity Parity = Parity.None;
        public StopBits StopBits = StopBits.Two;
        public Handshake Handshake = Handshake.None;
    }

    public class SerialPortTuner : Tuner
    {
        public SerialPortProperties Properties = new SerialPortProperties();

        protected SerialPort Port;
        private string NewLine = "\r";
        private Thread TransferThread;
        private HighPerformanceCounter TransmitDelay = new HighPerformanceCounter("Serial transmit delay");
        protected double TransmitDuration = 0;

        protected long CurrentFrequency = 0;

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


        public SerialPortTuner(bool autoDetect)
        {
            AutoDetect = autoDetect;
        }

        protected bool AutoDetectPort()
        {
            string[] portNames = SerialPort.GetPortNames();

            foreach (string portName in portNames)
            {
                if (Connect(portName))
                {
                    return true;
                }
            }

            return false;
        }


        private bool Connect(string name)
        {
            try
            {

                Port = new SerialPort(name, Properties.BaudRate, Properties.Parity, Properties.DataBits, Properties.StopBits);

                Port.Handshake = Properties.Handshake;
                Port.NewLine = NewLine;
                Port.ReadTimeout = 500;

                Port.Open();
                GC.SuppressFinalize(Port.BaseStream);

                IsOpening = true;
                IsOpened = ConnectionCheck();
                IsOpening = false;

                if (!IsOpened)
                {
                    Port.Close();
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Port.Close();
            }

            return false;
        }

        public virtual bool ConnectionCheck()
        {
            return true;
        }

        private void TransferThreadMain()
        {
            try
            {
                lock (TransferLock)
                {
                    while (true)
                    {
                        Monitor.Wait(TransferLock);

                        try
                        {
                            switch (TransferDirection)
                            {
                                case eTransferDirection.Receive:
                                    TransferString = ReceiveInternal();
                                    TransmitDelay.Stop();
                                    TransmitDuration = TransmitDelay.Duration;

                                    break;

                                case eTransferDirection.Send:
                                    TransmitDelay.Start();
                                    SendInternal(TransferString);
                                    break;

                                case eTransferDirection.None:
                                    break;
                            }
                        }
                        catch (TimeoutException e)
                        {
                            TransferString = "TIMEOUT";
                            TransferFailure = true;
                        }
                        catch (Exception e)
                        {
                            TransferString = "TRANSFER FAILURE";
                            TransferFailure = true;
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
            string ret = Port.ReadLine();
            //Log.AddMessage("< " + ret);

            return ret;
        }

        public void SendInternal(string cmd)
        {
            //Log.AddMessage("> " + cmd);
            Port.WriteLine(cmd);
        }


        public string Receive()
        {
            if (!IsOpened && !IsOpening)
                return "NOT CONNECTED";

            if (TransferFailure)
                return TransferString;

            lock (TransferLock)
            {
                TransferDirection = eTransferDirection.Receive;
                Monitor.Pulse(TransferLock);

                while (TransferDirection == eTransferDirection.Receive)
                {
                    Monitor.Wait(TransferLock, 5);
                }

                if (TransferFailure)
                {
                    if (DeviceDisappeared != null)
                    {
                        DeviceDisappeared(this, null);
                    }
                }

                return TransferString;
            }
        }


        public void Send(string cmd)
        {
            if (!IsOpened && !IsOpening)
                return;

            if (TransferFailure)
                return;

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

        private long Frequency = 0;
        protected bool IsOpened;
        private bool IsOpening;
        protected string LastCommand;

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

                if (AutoDetect)
                {
                    if (!AutoDetectPort())
                    {
                        CloseTuner();
                        return false;
                    }
                }
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
                TransferThread.Join();
            }

            if (Port != null)
            {
                try
                {
                    Port.Close();
                    Port.Dispose();
                }
                catch(Exception e)
                {
                }
            }

            TransferThread = null;
            Port = null;

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