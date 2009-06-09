using System;
using System.Windows.Forms;

namespace LibRXFFT.Libraries.USB_RX
{
    class USBRXDevice
    {
        int devNum = 0;

        public USBRXDevice()
        {
        }

        public bool Open()
        {
            try
            {
                return USBRXDeviceNative.UsbInit(devNum);
            }
            catch (DllNotFoundException e)
            {
                MessageBox.Show("Was not able to load the driver: " + e.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show("Was not able to load the driver: " + e.Message);
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
    }
}
