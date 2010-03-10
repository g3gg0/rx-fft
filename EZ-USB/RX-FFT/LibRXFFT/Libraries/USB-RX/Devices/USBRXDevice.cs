using System;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.Timers;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using LibRXFFT.Libraries.USB_RX.Misc;
using LibRXFFT.Libraries.USB_RX.Tuners;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class USBRXDevice : I2CInterface, SPIInterface
    {
        private int DevNum;
        private AD6636 AD6636;
        private eTransferMode _CurrentMode = eTransferMode.Stopped;
        public DigitalTuner Tuner;
        public Atmel Atmel;

        private bool PreQueueTransfer = false;

        public uint SamplesPerBlock { get; set; }

        private uint _ReadBlockSize = 4096;
        public uint ReadBlockSize
        {
            get { return _ReadBlockSize; }
            set
            {
                _ReadBlockSize = value;
                if (CurrentMode == eTransferMode.Block)
                {
                    USBRXDeviceNative.UsbSetControlledTransfer(DevNum, 0, ReadBlockSize);
                }
            }
        }

        public double BlocksPerSecond
        {
            get
            {
                return 1000 / (double)ReadTimer.Interval;
            }

            set
            {
                ReadTimer.Interval = (uint)(1000 / value);
            }
        }

        public int ShmemChannel
        {
            get
            {
                return USBRXDeviceNative.UsbGetShmemChannel(DevNum);
            }
        }

        public int ShmemNode
        {
            get
            {
                return USBRXDeviceNative.UsbGetShmemID(DevNum);
            }
        }

        public USBRXDevice()
        {
            ReadTimer = new AccurateTimer();
            ReadTimer.Interval = 50;
            ReadTimer.Timer += new EventHandler(ReadTimer_Timer);
        }

        public bool Init()
        {
            try
            {
                ShowConsole(false);

                if (USBRXDeviceNative.UsbInit(DevNum))
                {
                    Atmel = new Atmel(this);
                    AD6636 = new AD6636(Atmel, Atmel.TCXOFreq);
                    Tuner = AD6636;

                    /* try to open BO-35 */
                    BO35 bo35 = new BO35(true);
                    bool success = false;

                    try
                    {
                        success = bo35.OpenTuner();
                    }
                    catch(Exception e)
                    {
                        
                    }

                    if (success)
                    {
                        Tuner = new TunerStack(bo35, Tuner, 0);
                    }
                    else
                    {
                        MT2131 mt2131 = new MT2131(this);

                        if (mt2131.OpenTuner())
                        {
                            Tuner = new TunerStack(mt2131, Tuner, mt2131.IFStepSize);
                        }
                    }

                    Tuner.OpenTuner();

                    CurrentMode = eTransferMode.Stopped;

                    ReadThread = new Thread(ReadThreadMain);
                    ReadThread.Name = "USB-RX Block read thread";
                    ReadThread.Start();

                    ReadTriggerThread = new Thread(ReadTriggerThreadMain);
                    ReadTriggerThread.Name = "USB-RX Block read trigger thread";
                    ReadTriggerThread.Start();

                    return true;
                }
            }
            catch (DllNotFoundException e)
            {
                MessageBox.Show("Was not able to load the driver. The driver DLL was not found." + Environment.NewLine + Environment.NewLine + e.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show("Was not able to load the driver:" + Environment.NewLine + Environment.NewLine + e.Message);
            }

            return false;
        }

        public void ShowConsole(bool show)
        {
            ushort mode = (ushort)(USBRXDeviceNative.MODE_NORMAL | USBRXDeviceNative.MODE_FASTI2C);

            if (show)
                mode |= USBRXDeviceNative.MODE_CONSOLE;

            USBRXDeviceNative.UsbSetTimeout(0x80, mode);
        }

        public eTransferMode CurrentMode
        {
            get { return _CurrentMode; }
            set
            {
                PreQueueTransfer = false;

                switch (value)
                {
                    case eTransferMode.Block:
                        if (CurrentMode == eTransferMode.Stream)
                        {
                            StopStreamRead();
                        }
                        else
                        {
                            StopRead();
                        }
                        StartRead();
                        break;

                    case eTransferMode.Stream:
                        if (CurrentMode == eTransferMode.Stream)
                        {
                            StopStreamRead();
                        }
                        else
                        {
                            StopRead();
                        }
                        StartStreamRead();
                        break;

                    case eTransferMode.Stopped:
                        if (CurrentMode == eTransferMode.Stream)
                        {
                            StopStreamRead();
                        }
                        else
                        {
                            StopRead();
                        }
                        break;
                }
                _CurrentMode = value;
            }
        }


        #region Abstraction

        public enum eAgcType
        {
            Off,
            Slow,
            Medium,
            Fast,
            Manual
        }

        public enum eRfSource
        {
            Tuner,
            RF1,
            RF2,
            RF3,
            RF4
        }

        public bool SetFilter(AtmelFilter filter)
        {
            if (AD6636 == null)
                return false;

            if (!Atmel.SetFilter(filter.Id))
                return false;

            AD6636.SetFilter(filter);

            return true;
        }

        public bool SetFilter(AD6636FilterFile filter)
        {
            if (AD6636 == null)
                return false;

            return AD6636.SetFilter(filter);
        }

        public bool SetAgc(eAgcType type)
        {
            SetMgc(0);
            //AD6636.SetAgc();
            return Atmel.SetAgc(type);
        }

        public bool SetMgc(int dB)
        {
            return AD6636.SetMgcValue(dB);
        }

        public bool SetRfSource(eRfSource source)
        {
            return Atmel.SetRfSource(source);
        }

        public bool SetAtt(bool state)
        {
            return Atmel.SetAtt(state);
        }

        public bool SetPreAmp(bool state)
        {
            return Atmel.SetPreAmp(state);
        }

        #endregion

        #region Transfer

        private AccurateTimer ReadTimer;
        private Thread ReadThread;
        private Thread ReadTriggerThread;
        private object ReadTriggerTrigger = new object();
        private object ReadTrigger = new object();
        private object ReadTimerLock = new object();

        private bool ReadTriggered = false;
        private bool ReadTimerLocked = false;

        private double ExpectedReadDuration = 0;
        private int SleepDuration = 10;
        private int MaxOvertimeFactor = 100;
        private int TimeoutsHappened = 0;
        public bool DeviceLost = false;

        private void ReadTriggerThreadMain()
        {
            try
            {
                while (true)
                {
                    lock (ReadTriggerTrigger)
                    {
                        Monitor.Wait(ReadTriggerTrigger);
                    }
                    //Log.AddMessage("ReadTriggerTrigger [fired]");

                    lock (ReadTimerLock)
                    {
                        lock (ReadTrigger)
                        {
                            int loops = 0;
                            int maxLoops = (int)(MaxOvertimeFactor * 1000 * ExpectedReadDuration / SleepDuration);
                            bool timeout = false;

                            /* dont fire next read until last data was processed */
                            while (ReadTimerLocked && !timeout)
                            {
                                /* minimum once again */
                                timeout = loops++ > maxLoops;
                                Monitor.Wait(ReadTimerLock, SleepDuration);
                            }

                            if (timeout)
                            {
                                TimeoutsHappened++;

                                switch(TimeoutsHappened)
                                {
                                    case 1:
                                    case 5:
                                        AD6636.SoftSync();
                                        Log.AddMessage("USBRXDevice: Timeout " + TimeoutsHappened + ". Re-Sync AD6636");
                                        break;

                                    case 2:
                                    case 6:
                                        AD6636.ReInit();
                                        Log.AddMessage("USBRXDevice: Timeout " + TimeoutsHappened + ". Re-Init AD6636");
                                        break;

                                    case 3:
                                    case 7:
                                        Atmel.AD6636Reset();
                                        AD6636.ReInit();
                                        Log.AddMessage("USBRXDevice: Timeout " + TimeoutsHappened + ". Reset AD6636");
                                        break;

                                    case 4:
                                    case 8:
                                        USBRXDeviceNative.UsbSetIdleMode(DevNum);
                                        USBRXDeviceNative.UsbSetGPIFMode(DevNum);
                                        USBRXDeviceNative.SetSlaveFifoParams(true, 0, 0);
                                        Atmel.FIFOReset(false);
                                        Log.AddMessage("USBRXDevice: Timeout " + TimeoutsHappened + ". Reset Cypress Transfers");
                                        break;


                                    default:
                                        DeviceLost = true;
                                        break;
                                }
                                //Log.AddMessage("TIMEOUT! Expected " + FrequencyFormatter.TimeToString(ExpectedReadDuration) + ", stopped after " + FrequencyFormatter.TimeToString(((double)(loops*SleepDuration)/1000)));
                            }
                            //Log.AddMessage("ReadTimerLocked [false]");

                            ReadTriggered = true;
                            Monitor.Pulse(ReadTrigger);
                        }
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                return;
            }
        }

        private void ReadThreadMain()
        {
            try
            {
                lock (ReadTrigger)
                {
                    while (true)
                    {
                        /* when read timer fires */
                        while (!ReadTriggered)
                        {
                            Monitor.Wait(ReadTrigger, 50);
                        }

                        ReadTriggered = false;

                        /* start a new transfer */
                        //Log.AddMessage("Transfer");
                        USBRXDeviceNative.SetSlaveFifoParams(true, 0, 0);

                        if (!PreQueueTransfer)
                        {
                            PreQueueTransfer = true;
                            USBRXDeviceNative.UsbSetControlledTransfer(DevNum, ReadBlockSize, ReadBlockSize);
                        }

                        Atmel.FIFOReset(false);

                        ExpectedReadDuration = SamplesPerBlock / (double)Tuner.SamplingRate;

                        /* dont fire next read until data was processed */
                        ReadTimerLocked = true;
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                return;
            }
        }

        private void ReadTimer_Timer(object sender, EventArgs e)
        {
            //Log.AddMessage("ReadTimer_Timer [fired]");
            lock (ReadTriggerTrigger)
            {
                Monitor.Pulse(ReadTriggerTrigger);
            }
        }

        public void ReadBlockReceived()
        {
            lock (ReadTimerLock)
            {
                TimeoutsHappened = 0;
                DeviceLost = false;

                Atmel.FIFOReset(true);
                // reset EP FIFOs
                USBRXDeviceNative.SetSlaveFifoParams(true, DevNum, 0);

                // and already pre-start transfer
                if (PreQueueTransfer)
                {
                    USBRXDeviceNative.UsbSetControlledTransfer(DevNum, ReadBlockSize, ReadBlockSize);
                }

                ReadTimerLocked = false;
                Monitor.Pulse(ReadTimerLock);
            }
        }

        private void StartStreamRead()
        {
            USBRXDeviceNative.UsbSetGPIFMode(DevNum);
            USBRXDeviceNative.UsbSetControlledTransfer(DevNum, 0, ReadBlockSize);
        }

        private void StopStreamRead()
        {
            USBRXDeviceNative.UsbSetControlledTransfer(DevNum, ReadBlockSize, ReadBlockSize);
            USBRXDeviceNative.UsbSetGPIFMode(DevNum);
        }

        private void StartRead()
        {
            USBRXDeviceNative.UsbSetGPIFMode(DevNum);
            ReadTimerLocked = false;
            ReadTimer.Start();
            lock (ReadTimerLock)
            {
                Monitor.Pulse(ReadTimerLock);
            }
        }

        private void StopRead()
        {
            ReadTimer.Stop();
            ReadTimerLocked = false;
            lock (ReadTimerLock)
            {
                Monitor.Pulse(ReadTimerLock);
            }
        }


        public void Close()
        {
            Tuner.CloseTuner();

            /* stop read timer */
            ReadTimer.Stop();

            /* stop read trigger thread */
            ReadTriggerThread.Abort();
            ReadTimerLocked = false;
            lock (ReadTimerLock)
            {
                Monitor.Pulse(ReadTimerLock);
            }
            lock (ReadTriggerTrigger)
            {
                Monitor.Pulse(ReadTriggerTrigger);
            }


            /* stop read thread */
            ReadThread.Abort();
            lock (ReadTrigger)
            {
                Monitor.Pulse(ReadTrigger);
            }
            ReadThread.Join();

            /* close driver handle */
            USBRXDeviceNative.UsbClose(DevNum);
        }

        public bool Read(byte[] data)
        {
            return USBRXDeviceNative.UsbParIn(DevNum, data, (uint)data.Length);
        }

        #endregion

        #region I2CInterface Member

        private object I2CAccessLock = new object();

        public bool I2CWriteByte(int busID, byte data)
        {
            lock (I2CAccessLock)
            {
                return USBRXDeviceNative.UsbI2CWriteByte(DevNum, busID, data);
            }
        }

        public bool I2CWriteBytes(int busID, byte[] data)
        {
            lock (I2CAccessLock)
            {
                return USBRXDeviceNative.UsbI2CWriteBytes(DevNum, busID, (ushort)data.Length, data);
            }
        }

        public bool I2CReadByte(int busID, byte[] data)
        {
            lock (I2CAccessLock)
            {
                return USBRXDeviceNative.UsbI2CReadByte(DevNum, busID, data);
            }
        }

        public bool I2CReadBytes(int busID, byte[] data)
        {
            lock (I2CAccessLock)
            {
                return USBRXDeviceNative.UsbI2CReadBytes(DevNum, busID, (ushort)data.Length, data);
            }
        }

        public bool I2CDeviceAck(int busID)
        {
            lock (I2CAccessLock)
            {
                return USBRXDeviceNative.UsbI2CReadBytes(DevNum, busID, 0, null);
            }
        }

        #endregion

        #region SPIInterface Member

        public void SPIInit()
        {
            USBRXDeviceNative.UsbSetIODir(DevNum, USBRXDeviceNative.PIN_SPI_RESET, USBRXDeviceNative.PIN_DIR_OUT); // RESET
            USBRXDeviceNative.UsbSetIODir(DevNum, USBRXDeviceNative.PIN_SPI_SDO, USBRXDeviceNative.PIN_DIR_OUT); // SDO
            USBRXDeviceNative.UsbSetIODir(DevNum, USBRXDeviceNative.PIN_SPI_CLK, USBRXDeviceNative.PIN_DIR_OUT); // CLK
            USBRXDeviceNative.UsbSetIODir(DevNum, USBRXDeviceNative.PIN_SPI_SDI, USBRXDeviceNative.PIN_DIR_IN); // SDI
            USBRXDeviceNative.UsbSetIODir(DevNum, USBRXDeviceNative.PIN_SPI_LED_IN, USBRXDeviceNative.PIN_DIR_IN); // LED pin

            USBRXDeviceNative.UsbSetIOState(DevNum, USBRXDeviceNative.PIN_SPI_RESET, USBRXDeviceNative.PIN_STATE_HIGH); // RESET inactive
        }

        public bool SPIResetDevice()
        {
            return true;
        }

        public bool SPITransfer(byte[] dataWrite, byte[] dataRead)
        {
            return USBRXDeviceNative.UsbSpiTransfer(DevNum, dataWrite, dataRead, (ushort)dataWrite.Length);
        }

        public bool SPIReset(int state)
        {
            if (state > 0)
                return USBRXDeviceNative.UsbSetIOState(DevNum, USBRXDeviceNative.PIN_SPI_RESET, USBRXDeviceNative.PIN_STATE_LOW);
            else
                return USBRXDeviceNative.UsbSetIOState(DevNum, USBRXDeviceNative.PIN_SPI_RESET, USBRXDeviceNative.PIN_STATE_HIGH);
        }

        #endregion
    }
}
