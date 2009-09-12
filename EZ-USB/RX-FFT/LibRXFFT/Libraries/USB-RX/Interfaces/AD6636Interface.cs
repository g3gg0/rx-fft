

namespace LibRXFFT.Libraries.USB_RX.Interfaces
{
    public interface AD6636Interface
    {
        bool ad6636WriteReg(int address, int bytes, long value, bool cache);
        bool ad6636WriteReg(int address, int bytes, long value);
        long ad6636ReadReg(int address, int bytes, bool cache);
        long ad6636ReadReg(int address, int bytes);
        bool ad6636Reset();
    }
}
