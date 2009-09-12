using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using LibRXFFT.Libraries.USB_RX.Tuners;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class USBRXDevice : I2CInterface, SPIInterface
    {


        int devNum = 0;
        public Tuner Tuner;
        public Atmel Atmel;
        public AD6636 AD6636;

        public USBRXDevice()
        {
        }

        public bool Init()
        {
            try
            {
                if (USBRXDeviceNative.UsbInit(devNum))
                {
                    Atmel = new Atmel(this);
                    AD6636 = new AD6636(Atmel, Atmel.getTCXOFreq());

                    TunerMT2131 mt2131 = new TunerMT2131(this);

                    if (!mt2131.exists())
                    {
                        Tuner = AD6636;
                    }
                    else
                    {
                        Tuner = new TunerStack(AD6636, mt2131, mt2131.IFFreq, mt2131.IFStepSize);
                    }

                }
            }
            catch (DllNotFoundException e)
            {
                MessageBox.Show("Was not able to load the driver. The DLL was not found:" + Environment.NewLine + e.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show("Was not able to load the driver: " + Environment.NewLine + e.Message);
            }

            return false;
        }

        public void Close()
        {
            USBRXDeviceNative.UsbClose(devNum);
        }

        public bool Read(byte[] data)
        {
            return USBRXDeviceNative.UsbParIn(devNum, data, (uint)data.Length);
        }

        #region Transfer

        #endregion

        #region I2CInterface Member

        public bool I2CWriteByte(int busID, byte data)
        {
            return USBRXDeviceNative.UsbI2CWriteByte(devNum, busID, data);
        }

        public bool I2CWriteBytes(int busID, byte[] data)
        {
            return USBRXDeviceNative.UsbI2CWriteBytes(devNum, busID, (ushort)data.Length, data);
        }

        public bool I2CReadByte(int busID, byte[] data)
        {
            return USBRXDeviceNative.UsbI2CReadByte(devNum, busID, data);
        }

        public bool I2CReadBytes(int busID, byte[] data)
        {
            return USBRXDeviceNative.UsbI2CReadBytes(devNum, busID, (ushort)data.Length, data);
        }

        public bool I2CDeviceAck(int busID)
        {
            return USBRXDeviceNative.UsbI2CReadBytes(devNum, busID, 0, null);
        }

        #endregion

        #region SPIInterface Member

        public void SPIInit()
        {
            USBRXDeviceNative.UsbSetIODir(devNum, USBRXDeviceNative.PIN_SPI_RESET, USBRXDeviceNative.PIN_DIR_OUT); // RESET
            USBRXDeviceNative.UsbSetIODir(devNum, USBRXDeviceNative.PIN_SPI_SDO, USBRXDeviceNative.PIN_DIR_OUT); // SDO
            USBRXDeviceNative.UsbSetIODir(devNum, USBRXDeviceNative.PIN_SPI_CLK, USBRXDeviceNative.PIN_DIR_OUT); // CLK
            USBRXDeviceNative.UsbSetIODir(devNum, USBRXDeviceNative.PIN_SPI_SDI, USBRXDeviceNative.PIN_DIR_IN); // SDI
            USBRXDeviceNative.UsbSetIODir(devNum, USBRXDeviceNative.PIN_SPI_LED_IN, USBRXDeviceNative.PIN_DIR_IN); // LED pin

            USBRXDeviceNative.UsbSetIOState(devNum, USBRXDeviceNative.PIN_SPI_RESET, USBRXDeviceNative.PIN_STATE_HIGH); // RESET inactive
        }

        public bool SPIResetDevice()
        {
            return true;
        }

        public bool SPITransfer(byte[] dataWrite, byte[] dataRead)
        {
            return USBRXDeviceNative.UsbSpiTransfer(devNum, dataWrite, dataRead, (ushort)dataWrite.Length);
        }

        public bool SPIReset(int state)
        {
            if (state > 0)
                return USBRXDeviceNative.UsbSetIOState(devNum, USBRXDeviceNative.PIN_SPI_RESET, USBRXDeviceNative.PIN_STATE_LOW);
            else
                return USBRXDeviceNative.UsbSetIOState(devNum, USBRXDeviceNative.PIN_SPI_RESET, USBRXDeviceNative.PIN_STATE_HIGH);
        }

        #endregion

        public void StartStream()
        {
            USBRXDeviceNative.UsbSetGPIFMode(devNum);
            USBRXDeviceNative.UsbSetControlledTransfer(devNum, 8192*8192, 0);
            USBRXDeviceNative.UsbSetControlledTransfer(devNum, 0, 0);
        }

        public void ShowConsole(bool show)
        {
            ushort mode = (ushort)(USBRXDeviceNative.MODE_NORMAL | USBRXDeviceNative.MODE_FASTI2C);

            if (show)
                mode |= USBRXDeviceNative.MODE_CONSOLE;

            USBRXDeviceNative.UsbSetTimeout(0x80, mode);
        }

    }
}
