using System;
using System.Threading;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using LibRXFFT.Libraries.USB_RX.Misc;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    internal struct sAtmelTypes
    {
        internal uint Sig0;
        internal uint Sig1;
        internal uint Sig2;
        internal uint PageCountFlash;
        internal uint PageSizeFlash; /* words */
        internal uint PageCountEeprom;
        internal uint PageSizeEeprom; /* byte */
        internal string Name;

        public sAtmelTypes(uint Sig, uint FlashPages, uint FlashPageSize, uint EEPROMPages, uint EEPROMPageSize, string Name) : this((Sig >> 16) & 0xFF, (Sig >> 8) & 0xFF, Sig & 0xFF, FlashPages, FlashPageSize, EEPROMPages, EEPROMPageSize, Name) { }

        public sAtmelTypes(uint Sig0, uint Sig1, uint Sig2, uint FlashPages, uint FlashPageSize, uint EEPROMPages, uint EEPROMPageSize, string Name)
        {
            this.Sig0 = Sig0;
            this.Sig1 = Sig1;
            this.Sig2 = Sig2;
            this.PageCountFlash = FlashPages;
            this.PageSizeFlash = FlashPageSize;
            this.PageCountEeprom = EEPROMPages;
            this.PageSizeEeprom = EEPROMPageSize;
            this.Name = Name;
        }
    }

    public class AtmelProgrammer
    {
        /* from
         * http://www.wiki.elektronik-projekt.de/mikrocontroller/avr/signature_bytes
         * http://www.mikrocontroller.net/topic/85100
         */
        private sAtmelTypes[] AtmelTypes =
        {
            new sAtmelTypes ( 0x000102, 0, 0, 0, 0, "(Locked device)" ),
            new sAtmelTypes ( 0x1E5106, 0, 0, 0, 0, "AT89S51" ),
            new sAtmelTypes ( 0x1E5206, 0, 0, 0, 0, "AT89S52" ),
            new sAtmelTypes ( 0x1E9001, 0, 0, 0, 0, "AT90S1200" ),
            new sAtmelTypes ( 0x1E9004, 0, 0, 0, 0, "ATtiny11" ),
            new sAtmelTypes ( 0x1E9005, 0, 0, 0, 0, "ATtiny12" ),
            new sAtmelTypes ( 0x1E9006, 0, 0, 0, 0, "ATtiny15" ),
            new sAtmelTypes ( 0x1E9007, 0, 0, 0, 0, "ATtiny13" ),
            new sAtmelTypes ( 0x1E9101, 0, 0, 0, 0, "AT90S2313" ),
            new sAtmelTypes ( 0x1E9102, 0, 0, 0, 0, "AT90S2323" ),
            new sAtmelTypes ( 0x1E9103, 0, 0, 0, 0, "AT90S2343" ),
            new sAtmelTypes ( 0x1E9106, 0, 0, 0, 0, "ATtiny22" ),
            new sAtmelTypes ( 0x1E9107, 0, 0, 0, 0, "ATtiny28" ),
            new sAtmelTypes ( 0x1E9108, 0, 0, 0, 0, "ATtiny25" ),
            new sAtmelTypes ( 0x1E9109, 0, 0, 0, 0, "ATtiny26" ),
            new sAtmelTypes ( 0x1E910A, 0, 0, 0, 0, "ATtiny2313" ),
            new sAtmelTypes ( 0x1E910B, 0, 0, 0, 0, "ATtiny24" ),
            new sAtmelTypes ( 0x1E910C, 0, 0, 0, 0, "ATtiny261" ),
            new sAtmelTypes ( 0x1E9181, 0, 0, 0, 0, "AT86RF401" ),
            new sAtmelTypes ( 0x1E9201, 0, 0, 0, 0, "AT90S4414" ),
            new sAtmelTypes ( 0x1E9203, 0, 0, 0, 0, "AT90S4433" ),
            new sAtmelTypes ( 0x1E9205, 0, 0, 0, 0, "ATmega48" ),
            new sAtmelTypes ( 0x1E9206, 0, 0, 0, 0, "ATtiny45" ),
            new sAtmelTypes ( 0x1E9207, 0, 0, 0, 0, "ATtiny44" ),
            new sAtmelTypes ( 0x1E9208, 0, 0, 0, 0, "ATtiny461" ),
            new sAtmelTypes ( 0x1E920A, 0, 0, 0, 0, "ATmega48P" ),
            new sAtmelTypes ( 0x1E9301, 0, 0, 0, 0, "AT90S8515" ),
            new sAtmelTypes ( 0x1E9301, 0, 0, 0, 0, "AT90S8515comp" ),
            new sAtmelTypes ( 0x1E9303, 0, 0, 0, 0, "AT90S4434/AT90S8535" ),
            new sAtmelTypes ( 0x1E9306, 0, 0, 0, 0, "ATmega8515" ),
            new sAtmelTypes ( 0x1E9307, 0, 0, 0, 0, "ATmega8" ),
            new sAtmelTypes ( 0x1E9308, 0, 0, 0, 0, "ATmega8535" ),
            new sAtmelTypes ( 0x1E930A, 0, 0, 0, 0, "ATmega88" ),
            new sAtmelTypes ( 0x1E930B, 0, 0, 0, 0, "ATtiny85" ),
            new sAtmelTypes ( 0x1E930C, 0, 0, 0, 0, "ATtiny84" ),
            new sAtmelTypes ( 0x1E930D, 0, 0, 0, 0, "ATtiny861" ),
            new sAtmelTypes ( 0x1E930F, 0, 0, 0, 0, "ATmega88P" ),
            new sAtmelTypes ( 0x1E9381, 0, 0, 0, 0, "AT90PWM2/AT90PWM3" ),
            new sAtmelTypes ( 0x1E9383, 0, 0, 0, 0, "AT90PWM2B/AT90PWM3B" ),
            new sAtmelTypes ( 0x1E9401, 0, 0, 0, 0, "ATmega161" ),
            new sAtmelTypes ( 0x1E9402, 0, 0, 0, 0, "ATmega163" ),
            new sAtmelTypes ( 0x1E9403, 0, 0, 0, 0, "ATmega16" ),
            new sAtmelTypes ( 0x1E9404, 0, 0, 0, 0, "ATmega162" ),
            new sAtmelTypes ( 0x1E9405, 0, 0, 0, 0, "ATmega169" ),
            new sAtmelTypes ( 0x1E9405, 0, 0, 0, 0, "ATmega169P" ),
            new sAtmelTypes ( 0x1E9406, 0, 0, 0, 0, "ATmega168" ),
            new sAtmelTypes ( 0x1E9407, 0, 0, 0, 0, "ATmega165/P" ),
            new sAtmelTypes ( 0x1E940A, 0, 0, 0, 0, "ATmega164P" ),
            new sAtmelTypes ( 0x1E940B, 0, 0, 0, 0, "ATmega168P" ),
            new sAtmelTypes ( 0x1E9482, 0, 0, 0, 0, "AT90USB162" ),
            new sAtmelTypes ( 0x1E9482, 0, 0, 0, 0, "AT90USB162" ),
            new sAtmelTypes ( 0x1E9501, 0, 0, 0, 0, "ATmega323" ),
            new sAtmelTypes ( 0x1E9502, 256, 64, 256, 4, "ATMega32" ), /* used in USB-RX */
            new sAtmelTypes ( 0x1E9503, 0, 0, 0, 0, "ATmega329/P" ),
            new sAtmelTypes ( 0x1E9504, 0, 0, 0, 0, "ATmega3290/P" ),
            new sAtmelTypes ( 0x1E9505, 0, 0, 0, 0, "ATmega325/P" ),
            new sAtmelTypes ( 0x1E9506, 0, 0, 0, 0, "ATmega3250/P" ),
            new sAtmelTypes ( 0x1E9507, 0, 0, 0, 0, "ATmega406" ),
            new sAtmelTypes ( 0x1E9508, 0, 0, 0, 0, "ATmega324P" ),
            new sAtmelTypes ( 0x1E9581, 0, 0, 0, 0, "AT90CAN32" ),
            new sAtmelTypes ( 0x1E9588, 0, 0, 0, 0, "ATMega32U6" ),
            new sAtmelTypes ( 0x1E9602, 0, 0, 0, 0, "ATmega64" ),
            new sAtmelTypes ( 0x1E9603, 0, 0, 0, 0, "ATmega649" ),
            new sAtmelTypes ( 0x1E9604, 0, 0, 0, 0, "ATmega6490" ),
            new sAtmelTypes ( 0x1E9605, 0, 0, 0, 0, "ATmega645" ),
            new sAtmelTypes ( 0x1E9606, 0, 0, 0, 0, "ATmega6450" ),
            new sAtmelTypes ( 0x1E9608, 0, 0, 0, 0, "ATmega640" ),
            new sAtmelTypes ( 0x1E9609, 256, 128, 256, 8, "ATMega644" ), /* used in USB-RX */
            new sAtmelTypes ( 0x1E960A, 256, 128, 256, 8, "ATMega644P" ), /* used in USB-RX */
            new sAtmelTypes ( 0x1E964E, 0, 0, 0, 0, "ATxmega64A1" ),
            new sAtmelTypes ( 0x1E9681, 0, 0, 0, 0, "AT90CAN64" ),
            new sAtmelTypes ( 0x1E9682, 0, 0, 0, 0, "AT90USB64x" ),
            new sAtmelTypes ( 0x1E9701, 0, 0, 0, 0, "ATmega103" ),
            new sAtmelTypes ( 0x1E9702, 0, 0, 0, 0, "ATmega128" ),
            new sAtmelTypes ( 0x1E9703, 0, 0, 0, 0, "ATmega1280" ),
            new sAtmelTypes ( 0x1E9704, 0, 0, 0, 0, "ATmega1281" ),
            new sAtmelTypes ( 0x1E9706, 0, 0, 0, 0, "ATmega1284" ),
            new sAtmelTypes ( 0x1E974C, 0, 0, 0, 0, "ATxmega128A1" ),
            new sAtmelTypes ( 0x1E974E, 0, 0, 0, 0, "ATxmega192A1" ),
            new sAtmelTypes ( 0x1E9781, 0, 0, 0, 0, "AT90CAN128" ),
            new sAtmelTypes ( 0x1E9782, 0, 0, 0, 0, "AT90USB128x" ),
            new sAtmelTypes ( 0x1E9801, 0, 0, 0, 0, "ATmega2560" ),
            new sAtmelTypes ( 0x1E9802, 0, 0, 0, 0, "ATmega2561" ),
            new sAtmelTypes ( 0x1E9846, 0, 0, 0, 0, "ATxmega256A1" ),
        };

        public enum eProgramMode
        {
            None,
            Program,
            Read,
            Erase
        }
        public struct BlockProcessInfo
        {
            public eProgramMode Mode;
            public bool Cancel;
            public uint BlockNum;
            public uint BlockCount;
        }

        public class DeviceErrorException : Exception
        {
        }
        public delegate void BlockProcessedDelegate(BlockProcessInfo info);


        /*
         * transfer up to 7 words - 14 bytes
         * restriction: USB packet may not exceed 0x40 bytes
         *
         * thats 14*4 bytes = 56 bytes plus 2 bytes command header of the usb driver
         */
        private uint TransferBlockSize = 7;
        private bool SlowProgramming = false;
        private bool DeviceSupportsXFuses = true;
        private sAtmelTypes AtmelType;
        private SPIInterface Device;
        private int LastExtendedByte = -1;

        public int RecoveryTime = 500;


        public AtmelProgrammer(SPIInterface device)
        {
            this.Device = device;
        }

        public bool ResetAtmel()
        {
            bool ret = Device.SPIReset(true);
            Thread.Sleep(20);

            ret &= Device.SPIReset(false);
            Thread.Sleep(RecoveryTime);

            return ret;
        }

        public bool SetProgrammingMode(bool state)
        {
            try
            {
                if (state)
                {
                    Device.SPIInit();
                    Device.SPIReset(false);
                    Thread.Sleep(20);
                    Device.SPIReset(true);
                    Thread.Sleep(20);

                    byte[] dataRead = sendCommandPlain(0xAC, 0x53, 0x00, 0x00);
                    if (dataRead[2] != 0x53)
                        return false;

                    AtmelType = FindDevice();

                    byte familyCode = FamilyCode;

                    /* try to guess page size/count from device code if unknown */
                    if (FlashSize == 0 && (familyCode & 0xF0) == 0x90)
                    {
                        AtmelType.PageSizeFlash = 128;
                        AtmelType.PageCountFlash = ((uint)Math.Pow(2, (familyCode & 0x0F)) * 1024) / AtmelType.PageSizeFlash;
                    }
                }
                else
                {
                    bool ret = Device.SPIReset(false);
                    Thread.Sleep(RecoveryTime);

                    return ret;
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public byte LockBits
        {
            get
            {
                byte[] dataRead = SendCommand(0x58, 0x00, 0x00, 0x00);
                return dataRead[3];
            }
            set
            {
                SendCommand(0xAC, 0xE0, 0x00, value);
            }
        }

        public ushort FuseBits
        {
            get
            {
                byte[] dataRead = SendCommand(0x58, 0x08, 0x00, 0x00);
                byte hiByte = dataRead[3];

                dataRead = SendCommand(0x50, 0x00, 0x00, 0x00);
                byte loByte = dataRead[3];

                return (ushort)((hiByte << 8) | loByte);
            }
            set
            {
                SendCommand(0xAC, 0xA8, 0x00, (byte)(value >> 8));
                Thread.Sleep(10);
                SendCommand(0xAC, 0xA0, 0x00, (byte)(value & 0xFF));
            }
        }

        public byte XFuseBits
        {
            get
            {
                if (DeviceSupportsXFuses)
                {
                    byte[] dataRead = SendCommand(0x50, 0x08, 0x00, 0x00);
                    return dataRead[3];
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (DeviceSupportsXFuses)
                {
                    SendCommand(0xAC, 0xA4, 0x00, value);
                }
            }
        }

        public uint CalByte
        {
            get
            {
                uint retData = 0;

                byte[] dataRead;

                for (int addr = 0; addr < 4; addr++)
                {
                    dataRead = SendCommand(0x38, 0x00, (byte)(0x00 | addr), 0x00);
                    retData <<= 8;
                    retData |= dataRead[3];
                }

                return retData;
            }
        }

        public ushort ReadProgramWord(uint address)
        {
            byte[] dataRead;

            // read High byte
            dataRead = SendCommand(0x28, (byte)(address >> 8), (byte)(address & 0xFF), 0x00);
            byte hiByte = dataRead[3];

            // read Low byte
            dataRead = SendCommand(0x20, (byte)(address >> 8), (byte)(address & 0xFF), 0x00);
            byte loByte = dataRead[3];

            return (ushort)((hiByte << 8) | loByte);
        }

        public byte ReadEEPROMByte(int address)
        {
            byte[] dataRead;

            // read Low byte
            dataRead = SendCommand(0xA0, (byte)(address >> 8), (byte)(address & 0xFF), 0x00);

            return dataRead[3];
        }

        private sAtmelTypes FindDevice()
        {
            uint sig0 = VendorCode;
            uint sig1 = FamilyCode;
            uint sig2 = PartNumberCode;

            foreach (sAtmelTypes type in AtmelTypes)
            {
                if (sig0 == type.Sig0 && sig1 == type.Sig1 && sig2 == type.Sig2)
                {
                    return type;
                }
            }

            return new sAtmelTypes(sig0, sig1, sig2, 0, 0, 0, 0, "Unknown device");
        }


        public string DeviceName
        {
            get
            {
                return FindDevice().Name;
            }
        }

        public uint PageCountFlash
        {
            get
            {
                return AtmelType.PageCountFlash;
            }
            set
            {
                AtmelType.PageCountFlash = value;
            }
        }

        public uint PageCountEeprom
        {
            get
            {
                return AtmelType.PageCountEeprom;
            }
            set
            {
                AtmelType.PageCountEeprom = value;
            }
        }

        public uint PageSizeFlash
        {
            get
            {
                return AtmelType.PageSizeFlash;
            }
            set
            {
                AtmelType.PageSizeFlash = value;
            }
        }

        public uint PageSizeEeprom
        {
            get
            {
                return AtmelType.PageSizeEeprom;
            }
            set
            {
                AtmelType.PageSizeEeprom = value;
            }
        }

        public uint FlashSize
        {
            get
            {
                return 2 * PageCountFlash * PageSizeFlash;
            }
        }

        public uint FlashStart
        {
            get
            {
                return 0;
            }
        }

        public byte VendorCode
        {
            get
            {
                byte[] dataRead = SendCommand(0x30, 0x00, 0x00, 0x00);
                return dataRead[3];
            }
        }

        public byte FamilyCode
        {
            get
            {
                byte[] dataRead = SendCommand(0x30, 0x00, 0x01, 0x00);
                return dataRead[3];
            }
        }

        public byte PartNumberCode
        {
            get
            {
                byte[] dataRead = SendCommand(0x30, 0x00, 0x02, 0x00);
                return dataRead[3];
            }
        }

        private byte[] SendCommand(byte i, byte j, byte k, byte l)
        {
            byte[] dataWrite = new byte[4];
            byte[] dataRead = new byte[4];

            dataWrite[0] = i;
            dataWrite[1] = j;
            dataWrite[2] = k;
            dataWrite[3] = l;

            if (!Device.SPITransfer(dataWrite, dataRead) || dataRead[1] != dataWrite[0])
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

            if (!Device.SPITransfer(dataWrite, dataRead))
                throw new DeviceErrorException();

            return dataRead;
        }

        public void ChipErase()
        {
            SendCommand(0xAC, 0x80, 0x00, 0x00);
            try
            {
                Thread.Sleep(20);
            }
            catch (Exception e)
            {
            }

            SetProgrammingMode(true);
            return;
        }

        private void WaitReady(int delay)
        {
            int cycle = 0;
            if (delay < 5)
                delay = 5;

            while (cycle < delay / 5)
            {
                byte[] dataRead = SendCommand(0xF0, 0x00, 0x00, 0x00);
                if ((dataRead[3] & 0x01) == 0x00)
                    return;
                cycle++;
            }
            throw new DeviceErrorException();
        }

        public void LoadExtendedAddressByte(uint addr)
        {
            if (this.LastExtendedByte == addr)
                return;

            SendCommand(0x4D, 0x00, (byte)addr, 0x00);
            this.LastExtendedByte = (int)addr;
            return;
        }
        public MemoryDump16BitLE ReadFlash(uint startAddress, uint size)
        {
            return ReadFlash(startAddress, size, null);
        }

        public MemoryDump16BitLE ReadFlash(uint startAddress, uint size, BlockProcessedDelegate cb)
        {
            BlockProcessInfo info = new BlockProcessInfo();
            ushort[] buffer = new ushort[size];

            info.Mode = eProgramMode.Read;
            info.BlockCount = size;

            for (uint i = 0; i < size; i++)
            {
                buffer[i] = ReadProgramWord(startAddress + i);
                if (cb != null)
                {
                    info.BlockNum = i;
                    cb(info);

                    if (info.Cancel)
                    {
                        return null;
                    }
                }
            }

            MemoryDump16BitLE dump = new MemoryDump16BitLE();
            dump.StartAddress = startAddress;
            dump.Data = buffer;

            return dump;
        }

        public void ProgramFlash(MemoryDump16BitLE dump)
        {
            ProgramFlash(dump, null);
        }

        public void ProgramFlash(MemoryDump16BitLE dump, BlockProcessedDelegate cb)
        {
            BlockProcessInfo info = new BlockProcessInfo();
            uint posFlash = 0;
            uint startAddress = dump.StartAddress;

            // create data struct, aligned to flash size
            uint dataSize = (uint)(((dump.Length + (AtmelType.PageSizeFlash - 1)) / AtmelType.PageSizeFlash) * AtmelType.PageSizeFlash);
            ushort[] data = new ushort[dataSize];

            for (int pos = 0; pos < dataSize; pos++)
            {
                if (pos < dump.Length)
                    data[pos] = dump.Data[pos];
                else
                    data[pos] = 0xFFFF;
            }

            // first erase the chip
            ChipErase();

            info.Mode = eProgramMode.Program;
            info.BlockCount = (uint)data.Length;

            // then flash the given data
            while (posFlash < data.Length)
            {
                uint posTemp = 0;

                if (cb != null)
                {
                    info.BlockNum = posFlash;
                    cb(info);
                    if (info.Cancel)
                    {
                        return;
                    }
                }

                // skip empty pages
                if (!IsEmptyPage(data, posFlash, AtmelType.PageSizeFlash))
                {
                    // program byte-wise
                    if (SlowProgramming)
                    {
                        while (posTemp < AtmelType.PageSizeFlash && posFlash + posTemp < data.Length)
                        {
                            LoadProgramMemoryPage(posTemp, data[posFlash + (posTemp++)]);
                        }
                    }
                    else
                    {
                        uint blockSize = TransferBlockSize;
                        while (posTemp < AtmelType.PageSizeFlash && posFlash + posTemp < data.Length)
                        {
                            if (posTemp + blockSize > AtmelType.PageSizeFlash)
                            {
                                blockSize = AtmelType.PageSizeFlash - posTemp;
                            }
                            LoadProgramMemoryPageMulti(posTemp, data, posFlash + posTemp, blockSize);
                            posTemp += blockSize;
                        }

                    }
                    // program page to flash
                    WriteProgramMemoryPage(startAddress + posFlash);
                    WaitReady(100);
                }

                posFlash += AtmelType.PageSizeFlash;
            }
        }

        public void ProgramEEPROM(byte[] data)
        {
            int posEEPROM = 0;

            // program the given data
            while (posEEPROM < data.Length)
            {
                WriteEEPROMMemory(posEEPROM, data[posEEPROM]);
                posEEPROM++;
            }
        }

        private bool IsEmptyPage(ushort[] data, uint posFlash, uint pageSize)
        {
            for (int pos = 0; pos < pageSize; pos++)
            {
                if (data[posFlash + pos] != 0xFFFF)
                    return false;
            }

            return true;
        }

        private void LoadProgramMemoryPageMulti(uint address, ushort[] data, uint offset, uint blockSize)
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

            if (!Device.SPITransfer(dataWrite, dataRead))
                throw new DeviceErrorException();
        }

        public void LoadProgramMemoryPage(uint address, ushort data)
        {
            // send byte to page buffer
            // first LSB
            // then MSB
            SendCommand(0x40, (byte)(address >> 8), (byte)(address & 0xFF), (byte)(data & 0xFF));
            SendCommand(0x48, (byte)(address >> 8), (byte)(address & 0xFF), (byte)(data >> 8));
        }

        public void WriteProgramMemoryPage(uint address)
        {
            // make sure extended address byte is loaded
            // not needed for ATMega644?
            LoadExtendedAddressByte(address / 65536);

            // program page
            SendCommand(0x4C, (byte)(address >> 8), (byte)(address & 0xFF), 0x00);
        }

        public void WriteEEPROMMemory(int address, byte data)
        {
            SendCommand(0xC0, (byte)(address >> 8), (byte)(address & 0xFF), data);
            WaitReady(100);
        }



    }
}
