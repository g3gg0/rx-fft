using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class USBRXDeviceNative
    {
        public static int MODE_IDLE = 0;
        public static int MODE_GPIF = 1;
/* 
 * configuration for dev board
        public static int PIN_SPI_RESET = 3;
        public static int PIN_SPI_CLK = 6;
        public static int PIN_SPI_SDI = 4;
        public static int PIN_SPI_SDO = 5;
        public static int PIN_SPI_LED_IN = 7;
*/
        public static int PIN_SPI_RESET = 2;
        public static int PIN_SPI_CLK = 6;
        public static int PIN_SPI_SDI = 4;
        public static int PIN_SPI_SDO = 5;
        public static int PIN_SPI_LED_IN = 7;

        public static int PIN_DIR_IN = 0;
        public static int PIN_DIR_OUT = 1;

        public static int PIN_STATE_LOW = 0;
        public static int PIN_STATE_HIGH = 1;

        /* those are the bits set via UsbSetTimeout when DevNum is 0x80 */
        public static ushort MODE_NORMAL = 0x0000;
        public static ushort MODE_NOATMEL = 0x0001;
        public static ushort MODE_FASTI2C = 0x0002;
        public static ushort MODE_POWERLINE = 0x0004;
        public static ushort MODE_CONSOLE = 0x0008;
        public static ushort MODE_PREBUFFER = 0x0010;
        public static ushort MODE_FORCEINIT = 0x1000;

        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int UsbGetShmemID(int DevNum);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int UsbGetShmemChannel(int DevNum);

        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSetGPIFMode (int DevNum );
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSetIdleMode (int DevNum );
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSetControlledTransfer (int DevNum, uint bytes, uint blocksize );
	
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetNumDevices ();
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetDeviceDesc(int DevNum, byte[] PTxtbuf, int TxtbufLen);

 
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSetTimeout(int DevNum, ushort timeout);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbOpen(int DevNum);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbClose(int DevNum);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UsbInit(int DevNum);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbGetErrorStringByNr(int ErrNr, byte[] PChar);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSetLicense(byte[] Filename);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbCheckSpeed(int DevNum, byte[] Speed);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void UsbGetVersion(byte[] VersionStr);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void UsbWaituSec(ushort usec);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbGetStatus(int DevNum);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbRetrieveAllErrors(int DevNum, bool RetrieveAll);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbDevicePresent(int DevNum);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbDeviceInitialized(int DevNum);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSetIOState(int DevNum, int LineNum, int State);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSetIODir(int DevNum, int LineNum, int State);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbGetIOState(int DevNum, int LineNum, byte[] State);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSetPortState(int DevNum, int PortNum, int State);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSetPortDir(int DevNum, int PortNum, int Dir);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbGetPortState(int DevNum, int PortNum, byte[] State);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbI2CSetTimeout(int DevNum, ushort Timeout);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbI2CSetSpeed(int DevNum, int Speed);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbI2CWriteBytes(int DevNum, int SlaveAddr, ushort Length, byte[] Data);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbI2CWriteByte(int DevNum, int SlaveAddr, byte Data);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbI2CReadBytes(int DevNum, int SlaveAddr, ushort Length, byte[] Data);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbI2CReadByte(int DevNum, int SlaveAddr, byte[] Data);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbI2CWriteAndReadBytes(int DevNum, int SlaveAddr, ushort WriteLen, ushort ReadLen, byte[] PWrite, byte[] PRead);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbEEpSetTimeout(int DevNum, ushort Timeout);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbEEpWriteBytes(int DevNum, ushort Addr, ushort Length, byte[] PData);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbEEpWriteByte(int DevNum, ushort Addr, int Data);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbEEpReadBytes(int DevNum, ushort Addr, ushort Length, byte[] PData);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbEEpReadByte(int DevNum, ushort Addr, byte[] Data);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbEEpSetAddr(int DevNum, int SlaveAddr);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbParSetTimeout(int DevNum, ushort Timeout);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbParSetWaveforms(int DevNum, int InWfIdx, int OutWfIdx);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbParInit(int DevNum, int mode);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbParInitUsingArray(int DevNum, byte[] PData);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbParOut(int DevNum, byte[] Data, uint Len);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbParIn(int DevNum, byte[] Data, uint Len);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbGetRdyState(int DevNum, byte[] Rdy);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSpiInit(int DevNum, bool LsbFirst, int SPIMode, int Speed);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSpiTransfer(int DevNum, byte[] PWrite, byte[] PRead, ushort Len);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbSpiTransferByte(int DevNum, int DatWrite, byte[] PDatRead);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UsbParSetTransferSize(int DevNum, uint Size);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetSlaveFifoParams(bool SyncFifo, int IfClkMode, int BitWidth);

        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool ResetEpFifo(int DevNum);
        [DllImport("usb2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetFifoFlushing(bool state);

    }
}
