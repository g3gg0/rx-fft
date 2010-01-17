using System;
using System.Threading;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using LibRXFFT.Libraries.USB_RX.Misc;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class AtmelProgrammer
    {
        public class DeviceErrorException : Exception
        {
        }

        SPIInterface device;
        private int lastExtendedByte = -1;

        // flash/eeprom block sizes
        private int pageSizeFlash = 128;
        private int pageSizeEEPROM = 8;

        // transfer up to 7 words - 14 bytes
        // restriction: USB packet may not exceed 0x40 bytes
        //
        // thats 14*4 bytes = 56 bytes plus 3 bytes command header
        //
        private int transferBlockSize = 7;
        private bool slowProgramming = false;

        private bool deviceSupportsXFuses = false;



        public AtmelProgrammer(SPIInterface device)
        {
            this.device = device;
        }

        public bool enterProgrammingMode()
        {
            try
            {
                device.SPIInit();
                device.SPIReset(0);
                Thread.Sleep(20);
                device.SPIReset(1);
                Thread.Sleep(20);

                byte[] dataRead = sendCommandPlain(0xAC, 0x53, 0x00, 0x00);
                if (dataRead[2] != 0x53)
                    return false;

            }
            catch (Exception e)
            {
            }

            return true;
        }

        public bool leaveProgrammingMode()
        {
            device.SPIReset(0);
            return true;
        }

        public byte readLockBits()
        {
            byte[] dataRead = sendCommand(0x58, 0x00, 0x00, 0x00);
            return dataRead[3];
        }

        public ushort readFuseBits()
        {
            byte loByte;
            byte hiByte;

            int fuse;
            byte[] dataRead = sendCommand(0x58, 0x08, 0x00, 0x00);
            hiByte = dataRead[3];

            dataRead = sendCommand(0x50, 0x00, 0x00, 0x00);
            loByte = dataRead[3];

            return (ushort)((hiByte << 8) | loByte);
        }

        public byte readXFuseBits()
        {
            if (deviceSupportsXFuses)
            {
                byte[] dataRead = sendCommand(0x50, 0x08, 0x00, 0x00);
                return dataRead[3];
            }
            else
            {
                return 0;
            }
        }

        public uint readCalByte()
        {
            uint retData = 0;

            byte[] dataRead;

            for (int addr = 0; addr < 4; addr++)
            {
                dataRead = sendCommand(0x38, 0x00, (byte)(0x00 | addr), 0x00);
                retData <<= 8;
                retData |= dataRead[3];
            }

            return retData;
        }

        public void writeFuseBits(int data)
        {
            sendCommand(0xAC, 0xA8, 0x00, (byte)(data >> 8));
            try
            {
                Thread.Sleep(10);
            }
            catch (Exception e)
            {
            }
            sendCommand(0xAC, 0xA0, 0x00, (byte)(data & 0xFF));
        }

        public void writeXFuseBits(byte data)
        {
            if (deviceSupportsXFuses)
                sendCommand(0xAC, 0xA4, 0x00, data);
        }

        public void writeLockBits(byte data)
        {
            sendCommand(0xAC, 0xE0, 0x00, data);
        }

        public ushort readProgramWord(uint address)
        {
            byte loByte;
            byte hiByte;
            byte[] dataRead;

            // read High byte
            dataRead = sendCommand(0x28, (byte)(address >> 8), (byte)(address & 0xFF), 0x00);
            hiByte = dataRead[3];

            // read Low byte
            dataRead = sendCommand(0x20, (byte)(address >> 8), (byte)(address & 0xFF), 0x00);
            loByte = dataRead[3];

            return (ushort)((hiByte << 8) | loByte);
        }


        public byte readEEPROMByte(int address)
        {
            byte[] dataRead;

            // read Low byte
            dataRead = sendCommand(0xA0, (byte)(address >> 8), (byte)(address & 0xFF), 0x00);

            return dataRead[3];
        }

        public uint FlashSize
        {
            get
            {
                return (uint)Math.Pow(2, (readDeviceFamilyCode() & 0x0F)) * 1024;
            }
        }

        public MemoryDump16BitLE readFlash(uint startAddress, uint size)
        {
            ushort[] buffer = new ushort[size];

            for (uint i = 0; i < size; i++)
            {
                buffer[i] = readProgramWord(startAddress + i);
            }

            MemoryDump16BitLE dump = new MemoryDump16BitLE();
            dump.StartAddress = startAddress;
            dump.Data = buffer;

            return dump;
        }

        public byte readDeviceVendorCode()
        {
            byte[] dataRead = sendCommand(0x30, 0x00, 0x00, 0x00);
            return dataRead[3];
        }

        public byte readDeviceFamilyCode()
        {
            byte[] dataRead = sendCommand(0x30, 0x00, 0x01, 0x00);
            return dataRead[3];
        }

        public byte readDevicePartNumberCode()
        {
            byte[] dataRead = sendCommand(0x30, 0x00, 0x02, 0x00);
            return dataRead[3];
        }

        private byte[] sendCommand(byte i, byte j, byte k, byte l)
        {
            byte[] dataWrite = new byte[4];
            byte[] dataRead = new byte[4];

            dataWrite[0] = i;
            dataWrite[1] = j;
            dataWrite[2] = k;
            dataWrite[3] = l;

            if (!device.SPITransfer(dataWrite, dataRead) || dataRead[1] != dataWrite[0])
                throw new DeviceErrorException();

            return dataRead;
        }

        private byte[] sendCommandPlain(byte i, byte j, byte k, byte l)
        {
            byte[] dataWrite = new byte[4];
            byte[] dataRead = new byte[4];

            dataWrite[0] = i;
            dataWrite[1] = j;
            dataWrite[2] = k;
            dataWrite[3] = l;

            if (!device.SPITransfer(dataWrite, dataRead))
                throw new DeviceErrorException();

            return dataRead;
        }

        public void chipErase()
        {
            sendCommand(0xAC, 0x80, 0x00, 0x00);
            try
            {
                Thread.Sleep(20);
            }
            catch (Exception e)
            {
            }
            enterProgrammingMode();
            return;
        }

        private void waitReady(int delay)
        {
            int cycle = 0;
            if (delay < 5)
                delay = 5;

            while (cycle < delay / 5)
            {
                byte[] dataRead = sendCommand(0xF0, 0x00, 0x00, 0x00);
                if ((dataRead[3] & 0x01) == 0x00)
                    return;
                cycle++;
            }
            throw new DeviceErrorException();
        }

        public void loadExtendedAddressByte(int addr)
        {
            if (this.lastExtendedByte == addr)
                return;

            sendCommand(0x4D, 0x00, (byte)addr, 0x00);
            this.lastExtendedByte = addr;
            return;
        }

        public void programFlash(MemoryDump16BitLE dump)
        {
            int posFlash = 0;

            // create data struct, aligned to flash size
            uint dataSize = (uint)(((dump.Length + (pageSizeFlash - 1)) / pageSizeFlash) * pageSizeFlash);
            ushort[] data = new ushort[dataSize];

            for (int pos = 0; pos < dataSize; pos++)
            {
                if (pos < dump.Length)
                    data[pos] = dump.Data[pos];
                else
                    data[pos] = 0xFFFF;
            }

            // first erase the chip
            chipErase();

            // then flash the given data
            while (posFlash < data.Length)
            {
                int posTemp = 0;

                // skip empty pages
                if (!isEmptyPage(data, posFlash, this.pageSizeFlash))
                {
                    // program byte-wise
                    if (slowProgramming)
                    {
                        while (posTemp < this.pageSizeFlash && posFlash + posTemp < data.Length)
                        {
                            loadProgramMemoryPage(posTemp, data[posFlash + (posTemp++)]);
                        }
                    }
                    else
                    {
                        int blockSize = transferBlockSize;
                        while (posTemp < this.pageSizeFlash && posFlash + posTemp < data.Length)
                        {
                            if (posTemp + blockSize > this.pageSizeFlash)
                            {
                                blockSize = this.pageSizeFlash - posTemp;
                            }
                            loadProgramMemoryPageMulti(posTemp, data, posFlash + posTemp, blockSize);
                            posTemp += blockSize;
                        }

                    }
                    // program page to flash
                    writeProgramMemoryPage(posFlash);
                    waitReady(100);
                }

                posFlash += this.pageSizeFlash;
            }
        }

        public void programEEPROM(byte[] data)
        {
            int posEEPROM = 0;

            // program the given data
            while (posEEPROM < data.Length)
            {
                writeEEPROMMemory(posEEPROM, data[posEEPROM]);
                posEEPROM++;
            }
        }

        private bool isEmptyPage(ushort[] data, int posFlash, int pageSize)
        {
            for (int pos = 0; pos < pageSize; pos++)
            {
                if (data[posFlash + pos] != 0xFFFF)
                    return false;
            }

            return true;
        }

        private void loadProgramMemoryPageMulti(int address, ushort[] data, int offset, int blockSize)
        {
            byte[] dataWrite = new byte[blockSize * 8];
            byte[] dataRead = new byte[blockSize * 8];

            for (int i = 0; i < blockSize; i++)
            {
                dataWrite[8 * i + 0] = 0x40;
                dataWrite[8 * i + 1] = (byte)(address >> 8);
                dataWrite[8 * i + 2] = (byte)(address & 0xFF);
                dataWrite[8 * i + 3] = (byte)(data[offset + i] & 0xFF);

                dataWrite[8 * i + 4] = 0x48;
                dataWrite[8 * i + 5] = (byte)(address >> 8);
                dataWrite[8 * i + 6] = (byte)(address & 0xFF);
                dataWrite[8 * i + 7] = (byte)(data[offset + i] >> 8);

                address++;
            }

            if (!device.SPITransfer(dataWrite, dataRead))
                throw new DeviceErrorException();
        }

        public void loadProgramMemoryPage(int address, ushort data)
        {
            // send byte to page buffer
            // first LSB
            // then MSB
            sendCommand(0x40, (byte)(address >> 8), (byte)(address & 0xFF), (byte)(data & 0xFF));
            sendCommand(0x48, (byte)(address >> 8), (byte)(address & 0xFF), (byte)(data >> 8));
        }

        public void writeProgramMemoryPage(int address)
        {
            // make sure extended address byte is loaded
            // not needed for ATMega644?
            loadExtendedAddressByte(address / 65536);

            // program page
            sendCommand(0x4C, (byte)(address >> 8), (byte)(address & 0xFF), 0x00);
        }

        public void writeEEPROMMemory(int address, byte data)
        {
            sendCommand(0xC0, (byte)(address >> 8), (byte)(address & 0xFF), data);
            waitReady(100);
        }

        public int getPageSizeFlash()
        {
            return pageSizeFlash;
        }

        public void setPageSizeFlash(int pageSizeFlash)
        {
            this.pageSizeFlash = pageSizeFlash;
        }

        public int getPageSizeEEPROM()
        {
            return pageSizeEEPROM;
        }

        public void setPageSizeEEPROM(int pageSizeEEPROM)
        {
            this.pageSizeEEPROM = pageSizeEEPROM;
        }

        public bool isSlowProgramming()
        {
            return slowProgramming;
        }

        public void setSlowProgramming(bool slowProgramming)
        {
            this.slowProgramming = slowProgramming;
        }
    }
}
