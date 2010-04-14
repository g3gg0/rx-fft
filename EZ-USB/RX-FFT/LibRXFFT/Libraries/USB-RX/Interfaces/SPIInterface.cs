namespace LibRXFFT.Libraries.USB_RX.Interfaces
{
    public interface SPIInterface
    {
        void SPIInit();
        bool SPIResetDevice();
        bool SPITransfer(byte[] dataWrite, byte[] dataRead);
        bool SPIReset(bool state);
    }
}
