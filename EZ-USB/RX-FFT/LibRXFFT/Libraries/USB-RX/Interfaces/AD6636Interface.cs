

using LibRXFFT.Libraries.USB_RX.Devices;
namespace LibRXFFT.Libraries.USB_RX.Interfaces
{
    public interface AD6636Interface
    {
        bool AD6636WriteReg(int address, int bytes, long value, bool cache);
        bool AD6636WriteReg(int address, int bytes, long value);
        long AD6636ReadReg(int address, int bytes, bool cache);
        long AD6636ReadReg(int address, int bytes);
        bool AD6636Reset();

        void Register(AD6636 ad6636);
    }
}
