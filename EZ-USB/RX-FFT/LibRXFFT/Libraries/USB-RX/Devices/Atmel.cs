using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using System.Threading;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class Atmel : AD6636Interface
    {
        private I2CInterface I2CDevice;
        private int BusID;
        private static int DefaultBusID = 0x20;

        private long FilterClock;
        private long FilterWidth;
        private long AGCCorrectionOffset;
        private long AGCCorrectionGain;

        private object AtmelCommandLock = new object();
        private DateTime LastAtmelCommand = DateTime.Now;
        private double AtmelCommandDelay = 3; // wait between atmel commands

        internal class ad6636RegCacheEntry
        {
            internal int bytes;
            internal long value;
        }

        private ad6636RegCacheEntry[] AD6636RegCache = null;


        public Atmel(I2CInterface device)
            : this(device, DefaultBusID)
        {
        }

        public Atmel(I2CInterface device, int busID)
        {
            this.I2CDevice = device;
            this.BusID = busID;
        }

        // FIFO Functions
        public bool FIFOReset(bool state)
        {
            bool ret;

            AtmelDelay();
            if (state)
                ret = I2CDevice.I2CWriteByte(BusID, 0x28);
            else
                ret = I2CDevice.I2CWriteByte(BusID, 0x29);

            return ret;
        }

        public bool FIFOReset()
        {
            AtmelDelay();
            return I2CDevice.I2CWriteByte(BusID, 0x0d);
        }

        // Filter stuff
        public int GetFilterCount()
        {
            byte[] buf = new byte[1];

            AtmelDelay();
            if (!I2CDevice.I2CWriteByte(BusID, 0x64))
                return 0;

            AtmelDelay();
            if (!I2CDevice.I2CReadByte(BusID, buf))
                return 0;

            return (int)buf[0];
        }

        public int GetLastFilter()
        {
            byte[] buf = new byte[1];

            AtmelDelay();
            if (!I2CDevice.I2CWriteByte(BusID, 0x10))
                return 0;

            AtmelDelay();
            if (!I2CDevice.I2CReadByte(BusID, buf))
                return 0;

            return (int)buf[0];
        }

        public bool SetFilter(int index)
        {
            if (index < 0 || index > 98)
                return false;

            byte[] cmd = new byte[2];

            cmd[0] = (byte)(0x65 + index);
            cmd[1] = 1;

            AtmelDelay();
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return false;

            byte[] buf = new byte[9];
            AtmelDelay();
            if (!I2CDevice.I2CReadBytes(BusID, buf))
                return false;

            this.FilterClock = buf[5] + buf[6] * 0x100 + buf[7] * 0x10000 + buf[8] * 0x1000000;
            this.FilterWidth = buf[1] + buf[2] * 0x100 + buf[3] * 0x10000 + buf[4] * 0x1000000;

            return true;
        }

        private void WaitMs(int ms)
        {
            try
            {
                Thread.Sleep(ms);
            }
            catch (Exception e)
            {
            }
        }

        public bool ReadFilter(int index)
        {
            if (index < 0 || index > 98)
                return false;

            byte[] cmd = new byte[2];

            cmd[0] = (byte)(0x65 + index);
            cmd[1] = 0;

            AtmelDelay();
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return false;

            byte[] buf = new byte[9];
            AtmelDelay();
            if (!I2CDevice.I2CReadBytes(BusID, buf))
                return false;

            this.FilterClock = buf[5] + buf[6] * 0x100 + buf[7] * 0x10000 + buf[8] * 0x1000000;
            this.FilterWidth = buf[1] + buf[2] * 0x100 + buf[3] * 0x10000 + buf[4] * 0x1000000;

            return true;
        }

        public long GetFilterClock()
        {
            return FilterClock;
        }

        public long GetFilterWidth()
        {
            return FilterWidth;
        }

        public int TCXOFreq
        {
            get
            {
                byte[] buf = new byte[4];

                AtmelDelay();
                if (!I2CDevice.I2CWriteByte(BusID, 0xC8))
                    return 0;

                AtmelDelay();
                if (!I2CDevice.I2CReadBytes(BusID, buf))
                    return 0;

                int value = (buf[3]<<24) | (buf[2]<<16) | (buf[1]<<8) | buf[0];

                return value;
            }

            set
            {
                byte[] buf = new byte[5];

                buf[0] = 0xC9;
                buf[1] = (byte)(value & 0xFF);
                buf[2] = (byte)((value >> 8) & 0xFF);
                buf[3] = (byte)((value >> 16) & 0xFF);
                buf[4] = (byte)((value >> 24) & 0xFF);

                AtmelDelay();
                if (!I2CDevice.I2CWriteBytes(BusID, buf))
                    return;

                return;
            }

        }


        public int GetRBW()
        {
            byte[] buf = new byte[4];

            AtmelDelay();
            if (!I2CDevice.I2CWriteByte(BusID, 0xCC))
                return 0;
            AtmelDelay();
            if (!I2CDevice.I2CReadBytes(BusID, buf))
                return 0;

            return (buf[3] << 24) | (buf[2] << 16) | (buf[1] << 8) | buf[0];
        }

        public bool SetRBW(int value)
        {
            byte[] buf = new byte[5];

            buf[0] = 0xCD;
            buf[1] = (byte)((value >> 0) & 0xFF);
            buf[2] = (byte)((value >> 8) & 0xFF);
            buf[3] = (byte)((value >> 16) & 0xFF);
            buf[4] = (byte)((value >> 24) & 0xFF);

            AtmelDelay();
            if (!I2CDevice.I2CWriteBytes(BusID, buf))
                return false;

            return true;
        }

        public string SerialNumber
        {
            get
            {
                byte[] buf = new byte[34];

                AtmelDelay();
                if (I2CDevice.I2CWriteByte(BusID, 0x07) != true)
                    return null;
                AtmelDelay();
                if (I2CDevice.I2CReadBytes(BusID, buf) != true)
                    return null;

                int length = buf[0];
                if (length > 32)
                    length = 0;

                char[] array = new char[length];
                for (int i = 0; i < length; i++)
                    array[i] = (char)buf[1 + i];

                return new string(array);
            }

            set
            {
                char[] array = value.ToCharArray();
                byte[] buffer = new byte[2 + array.Length];

                buffer[0] = 0x08;
                buffer[1] = (byte)array.Length;

                for (int i = 0; i < array.Length; i++)
                    buffer[2 + i] = (byte)array[i];

                AtmelDelay();
                if (!I2CDevice.I2CWriteBytes(BusID, buffer))
                    return;
            }
        }


        public bool SetRfSource(USBRXDevice.eRfSource source)
        {
            AtmelDelay();
            switch (source)
            {
                case USBRXDevice.eRfSource.RF1:
                    if (!I2CDevice.I2CWriteByte(BusID, 25))
                        return false;
                    break;
                case USBRXDevice.eRfSource.RF2:
                    if (!I2CDevice.I2CWriteByte(BusID, 26))
                        return false;
                    break;
                case USBRXDevice.eRfSource.RF3:
                    if (!I2CDevice.I2CWriteByte(BusID, 27))
                        return false;
                    break;
                case USBRXDevice.eRfSource.RF4:
                    if (!I2CDevice.I2CWriteByte(BusID, 28))
                        return false;
                    break;
                case USBRXDevice.eRfSource.Tuner:
                    if (!I2CDevice.I2CWriteByte(BusID, 29))
                        return false;
                    break;
            }

            return true;
        }

        public bool SetAtt(bool state)
        {
            bool ret;

            AtmelDelay();
            if (state)
                ret = I2CDevice.I2CWriteByte(BusID, 0x17);
            else
                ret = I2CDevice.I2CWriteByte(BusID, 0x18);

            return ret;
        }

        public bool SetPreAmp(bool state)
        {
            bool ret;

            AtmelDelay();
            if (state)
                ret = I2CDevice.I2CWriteByte(BusID, 0x15);
            else
                ret = I2CDevice.I2CWriteByte(BusID, 0x16);

            return ret;
        }

        public bool SetMgc(int dB)
        {
            if (dB < 1 || dB > 96)
                return false;

            byte[] cmd = new byte[2];

            cmd[0] = 0x32;
            cmd[1] = (byte)dB;

            AtmelDelay();
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return false;

            return true;
        }

        public bool SetAgc(USBRXDevice.eAgcType type)
        {
            AtmelDelay();
            switch (type)
            {
                case USBRXDevice.eAgcType.Off:
                    if (!I2CDevice.I2CWriteByte(BusID, (byte)(0x32 + 4)))
                        return false;
                    break;
                case USBRXDevice.eAgcType.Fast:
                    if (!I2CDevice.I2CWriteByte(BusID, (byte)(0x32 + 3)))
                        return false;
                    break;
                case USBRXDevice.eAgcType.Medium:
                    if (!I2CDevice.I2CWriteByte(BusID, (byte)(0x32 + 2)))
                        return false;
                    break;
                case USBRXDevice.eAgcType.Slow:
                    if (!I2CDevice.I2CWriteByte(BusID, (byte)(0x32 + 1)))
                        return false;
                    break;
            }
            WaitMs(10);

            return true;
        }

        public bool SetAGCCorrection(bool state, int offset, int gain)
        {
            byte[] cmd = new byte[4];

            if (state)
                cmd[0] = 0x37;
            else
                cmd[0] = 0x38;
            cmd[1] = (byte)offset;
            cmd[2] = (byte)(gain & 0xFF);
            cmd[3] = (byte)((gain >> 8) & 0xFF);

            AtmelDelay();
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return false;

            return true;
        }

        public bool ReadAGCCorrection()
        {
            AtmelDelay();
            if (!I2CDevice.I2CWriteByte(BusID, 0x39))
                return false;

            byte[] buffer = new byte[3];
            AtmelDelay();
            if (!I2CDevice.I2CReadBytes(BusID, buffer))
                return false;

            this.AGCCorrectionOffset = buffer[0];
            this.AGCCorrectionGain = (buffer[2] << 8) | buffer[1];

            return true;
        }


        public long GetAGCCorrectionOffset()
        {
            return this.AGCCorrectionOffset;
        }

        public long GetAGCCorrectionGain()
        {
            return this.AGCCorrectionGain;
        }


        // AD6636 Functions
        public bool AD6636Reset()
        {
            AtmelDelay();
            return I2CDevice.I2CWriteByte(BusID, 0x05);
        }


        public long AD6636ReadReg(int address, int bytes)
        {
            return AD6636ReadReg(address, bytes, false);
        }

        public long AD6636ReadReg(int address, int bytes, bool cache)
        {

            if (bytes < 1 || bytes > 4)
                return 0;

            // read from cache only
            if (cache)
            {
                if (AD6636RegCache != null && address < AD6636RegCache.Length)
                {
                    if (AD6636RegCache[address].bytes == bytes)
                        return AD6636RegCache[address].value;
                }
            }

            byte[] cmd = new byte[3];

            cmd[0] = (byte)(0x1F + (bytes - 1));
            cmd[1] = (byte)address;
            cmd[2] = (byte)(bytes | 0x80);

            AtmelDelay();
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return 0;

            byte[] buffer = new byte[bytes];

            AtmelDelay();
            if (!I2CDevice.I2CReadBytes(BusID, buffer))
                return 0;

            long value = 0;
            for (int i = 0; i < bytes; i++)
            {
                value <<= 8;
                value |= buffer[bytes - 1 - i];
            }

            // cache the read value
            if (AD6636RegCache != null && address < AD6636RegCache.Length)
            {
                if (AD6636RegCache[address].bytes == bytes)
                    AD6636RegCache[address].value = value;
            }
            return value;
        }

        public bool AD6636WriteReg(int address, int bytes, long value)
        {
            return AD6636WriteReg(address, bytes, value, true);
        }

        public bool AD6636WriteReg(int address, int bytes, long value, bool cache)
        {
            if (bytes < 1 || bytes > 4)
                return false;

            if (cache)
            {
                if (AD6636RegCache == null)
                {
                    AD6636RegCache = new ad6636RegCacheEntry[0x100];
                    for (int pos = 0; pos < AD6636RegCache.Length; pos++)
                        AD6636RegCache[pos] = new ad6636RegCacheEntry();
                }

                if (address < AD6636RegCache.Length)
                {
                    /* data already in this register - return 
                    if (AD6636RegCache[address].bytes == bytes && AD6636RegCache[address].value == value)
                    {
                        return true;
                    }*/
                    AD6636RegCache[address].bytes = bytes;
                    AD6636RegCache[address].value = value;
                }
            }

            byte[] cmd = new byte[3 + bytes];

            cmd[0] = (byte)bytes; /* the commands 1-4 specify the number of bytes to write */
            cmd[1] = (byte)address;
            cmd[2] = (byte)bytes;

            for (int i = 0; i < bytes; i++)
            {
                cmd[3 + i] = (byte)(value & 0xFF);
                value >>= 8;
            }

            AtmelDelay();
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return false;

            return true;
        }

        private void AtmelDelay()
        {
            lock (AtmelCommandLock)
            {
                DateTime now = DateTime.Now;
                double commandDelay = now.Subtract(LastAtmelCommand).TotalMilliseconds;

                if (commandDelay < AtmelCommandDelay)
                {
                    WaitMs((int)(AtmelCommandDelay - commandDelay));
                }
                LastAtmelCommand = now;
            }
        }
    }
}
