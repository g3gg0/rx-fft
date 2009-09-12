using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Interfaces
{
    public interface SPIInterface
    {
        void SPIInit();
        bool SPIResetDevice();
        bool SPITransfer(byte[] dataWrite, byte[] dataRead);
        bool SPIReset(int state);
    }
}
