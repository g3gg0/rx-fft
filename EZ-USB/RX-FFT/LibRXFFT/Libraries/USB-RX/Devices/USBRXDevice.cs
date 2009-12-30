using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using LibRXFFT.Libraries.USB_RX.Tuners;
using LibRXFFT.Libraries.Timers;
using LibRXFFT.Libraries.USB_RX.Misc;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class USBRXDevice : I2CInterface, SPIInterface
    {
        public enum TransferMode
        {
            Stopped,
            Stream,
            Block
        }

        private int DevNum = 0;
        private AD6636 AD6636;
        private TransferMode CurrentMode;
        public DigitalTuner Tuner;
        public Atmel Atmel;

        public uint _ReadBlockSize = 4096;
        public uint ReadBlockSize 
        {
            get { return _ReadBlockSize; }
            set 
            {
                _ReadBlockSize = value;
                if (CurrentMode == TransferMode.Block)
                {
                    USBRXDeviceNative.UsbSetControlledTransfer(DevNum, 0, ReadBlockSize);
                }
            }
        }

        public int ShmemChannel
        {
            get
            {
                return USBRXDeviceNative.UsbGetShmemChannel(DevNum);
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
                if (USBRXDeviceNative.UsbInit(DevNum))
                {
                    CurrentMode = TransferMode.Stopped;
                    Atmel = new Atmel(this);
                    AD6636 = new AD6636(Atmel, Atmel.TCXOFreq);

                    MT2131 mt2131 = new MT2131(this);

                    if (!mt2131.exists())
                    {
                        Tuner = AD6636;
                    }
                    else
                    {
                        Tuner = new TunerStack(mt2131, AD6636, mt2131.IFFrequency, mt2131.IFStepSize);
                    }

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

        #region Abstraction

        public bool SetFilter(AD6636FilterFile filter)
        {
            if (AD6636 == null)
                return false;

            return AD6636.SetFilter(filter);
        }
        #endregion

        #region Transfer

        private AccurateTimer ReadTimer;

        void ReadTimer_Timer(object sender, EventArgs e)
        {
            //USBRXDeviceNative.SetSlaveFifoParams(true, 0, 0);
            USBRXDeviceNative.UsbSetControlledTransfer(DevNum, ReadBlockSize, ReadBlockSize);
        }

        public void StartStreamRead()
        {
            CurrentMode = TransferMode.Stream;
            USBRXDeviceNative.UsbSetGPIFMode(DevNum);
            USBRXDeviceNative.UsbSetControlledTransfer(DevNum, 0, ReadBlockSize);
        }

        public void StopStreamRead()
        {
            CurrentMode = TransferMode.Stopped;
            USBRXDeviceNative.UsbSetGPIFMode(DevNum);
        }

        public void StartRead()
        {
            CurrentMode = TransferMode.Block;
            USBRXDeviceNative.UsbSetGPIFMode(DevNum);
            ReadTimer.Start();
        }

        public void StopRead()
        {
            CurrentMode = TransferMode.Stopped;
            ReadTimer.Stop();
        }

        public void Close()
        {
            USBRXDeviceNative.UsbClose(DevNum);
        }

        public bool Read(byte[] data)
        {
            return USBRXDeviceNative.UsbParIn(DevNum, data, (uint)data.Length);
        }

        #endregion

        #region I2CInterface Member

        public bool I2CWriteByte(int busID, byte data)
        {
            return USBRXDeviceNative.UsbI2CWriteByte(DevNum, busID, data);
        }

        public bool I2CWriteBytes(int busID, byte[] data)
        {
            return USBRXDeviceNative.UsbI2CWriteBytes(DevNum, busID, (ushort)data.Length, data);
        }

        public bool I2CReadByte(int busID, byte[] data)
        {
            return USBRXDeviceNative.UsbI2CReadByte(DevNum, busID, data);
        }

        public bool I2CReadBytes(int busID, byte[] data)
        {
            return USBRXDeviceNative.UsbI2CReadBytes(DevNum, busID, (ushort)data.Length, data);
        }

        public bool I2CDeviceAck(int busID)
        {
            return USBRXDeviceNative.UsbI2CReadBytes(DevNum, busID, 0, null);
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
