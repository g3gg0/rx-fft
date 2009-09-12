using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Interfaces
{
    public interface I2CInterface
    {
        bool I2CWriteByte(int busID, byte data);
        bool I2CWriteBytes(int busID, byte[] data);
        bool I2CReadByte(int busID, byte[] data);
        bool I2CReadBytes(int busID, byte[] data);
        bool I2CDeviceAck(int busID);
    }
}
