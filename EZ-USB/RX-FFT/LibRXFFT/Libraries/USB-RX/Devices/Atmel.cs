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
        private I2CInterface i2cDevice;
        private int busID;
        private static int defaultBusID = 0x20;

        private long filterClock;
        private long filterWidth;
        private long agcCorrectionOffset;
        private long agcCorrectionGain;

        public static int AGC_OFF = 0; // 50
        public static int AGC_SLOW = 1; // 51
        public static int AGC_MIDDLE = 2; // 52
        public static int AGC_FAST = 3; // 53
        public static int AGC_MANUAL = 4; // 54

        internal class ad6636RegCacheEntry
        {
            internal int bytes;
            internal long value;
        }

        private ad6636RegCacheEntry[] ad6636RegCache = null;


        public Atmel(I2CInterface device)
            : this(device, defaultBusID)
        {
        }

        public Atmel(I2CInterface device, int busID)
        {
            this.i2cDevice = device;
            this.busID = busID;

        }

        // Select bandpass
        public bool setBP(int index)
        {
            if (index < 0 || index > 3)
                return false;
            waitMs(10);
            bool ret = i2cDevice.I2CWriteByte(busID, (byte)(0x19 + index));
            waitMs(10);

            return ret;
        }


        // FIFO Functions
        public bool FIFOReset(bool state)
        {
            waitMs(10);
            bool ret;
            if (state)
                ret = i2cDevice.I2CWriteByte(busID, 0x28);
            else
                ret = i2cDevice.I2CWriteByte(busID, 0x29);
            waitMs(10);

            return ret;
        }

        public bool FIFOReset()
        {
            return i2cDevice.I2CWriteByte(busID, 0x0d);
        }

        // Filter stuff
        public int getFilterCount()
        {
            byte[] buf = new byte[1];

            waitMs(10);
            if (!i2cDevice.I2CWriteByte(busID, 0x64))
                return 0;

            waitMs(10);
            if (!i2cDevice.I2CReadByte(busID, buf))
                return 0;

            return (int)buf[0];
        }

        public int getFilterLast()
        {
            byte[] buf = new byte[1];

            waitMs(10);
            if (!i2cDevice.I2CWriteByte(busID, 0x10))
                return 0;

            waitMs(10);
            if (!i2cDevice.I2CReadByte(busID, buf))
                return 0;

            return (int)buf[0];
        }

        public bool setFilter(int index)
        {
            if (index < 0 || index > 98)
                return false;

            byte[] cmd = new byte[2];

            cmd[0] = (byte)(0x65 + index);
            cmd[1] = 1;

            waitMs(10);
            if (!i2cDevice.I2CWriteBytes(busID, cmd))
                return false;

            byte[] buf = new byte[9];
            waitMs(150);
            if (!i2cDevice.I2CReadBytes(busID, buf))
                return false;

            this.filterClock = buf[5] + buf[6] * 0x100 + buf[7] * 0x10000 + buf[8] * 0x1000000;
            this.filterWidth = buf[1] + buf[2] * 0x100 + buf[3] * 0x10000 + buf[4] * 0x1000000;

            return true;
        }

        private void waitMs(int ms)
        {
            try
            {
                Thread.Sleep(ms);
            }
            catch (Exception e)
            {
            }
        }

        public bool readFilter(int index)
        {
            if (index < 0 || index > 98)
                return false;

            byte[] cmd = new byte[2];

            cmd[0] = (byte)(0x65 + index);
            cmd[1] = 0;

            waitMs(10);
            if (!i2cDevice.I2CWriteBytes(busID, cmd))
                return false;

            byte[] buf = new byte[9];
            waitMs(150);
            if (!i2cDevice.I2CReadBytes(busID, buf))
                return false;

            this.filterClock = buf[5] + buf[6] * 0x100 + buf[7] * 0x10000 + buf[8] * 0x1000000;
            this.filterWidth = buf[1] + buf[2] * 0x100 + buf[3] * 0x10000 + buf[4] * 0x1000000;

            return true;
        }

        public long getFilterClock()
        {
            return filterClock;
        }

        public long getFilterWidth()
        {
            return filterWidth;
        }

        public int getTCXOFreq()
        {
            byte[] buf = new byte[4];

            waitMs(10);
            if (!i2cDevice.I2CWriteByte(busID, 0xC8))
                return 0;

            waitMs(20);
            if (!i2cDevice.I2CReadBytes(busID, buf))
                return 0;

            int value = buf[3];
            value *= 256;
            value += buf[2];
            value *= 256;
            value += buf[1];
            value *= 256;
            value += buf[0];

            return value;
        }

        public bool setTCXOFreq(int value)
        {
            byte[] buf = new byte[5];

            buf[0] = 0xC9;
            buf[1] = (byte)(value % 256);
            value /= 256;
            buf[2] = (byte)(value % 256);
            value /= 256;
            buf[3] = (byte)(value % 256);
            value /= 256;
            buf[4] = (byte)(value % 256);

            waitMs(10);
            if (!i2cDevice.I2CWriteBytes(busID, buf))
                return false;
            waitMs(20);

            return true;
        }

        public int getRBW()
        {
            byte[] buf = new byte[4];

            waitMs(10);
            if (!i2cDevice.I2CWriteByte(busID, 0xCC))
                return 0;
            waitMs(20);
            if (!i2cDevice.I2CReadBytes(busID, buf))
                return 0;

            return (buf[3] << 24) | (buf[2] << 16) | (buf[1] << 8) | buf[0];
        }

        public bool setRBW(int value)
        {
            byte[] buf = new byte[5];

            buf[0] = 0xCD;
            buf[1] = (byte)((value >> 0) & 0xFF);
            buf[2] = (byte)((value >> 8) & 0xFF);
            buf[3] = (byte)((value >> 16) & 0xFF);
            buf[4] = (byte)((value >> 24) & 0xFF);

            waitMs(10);
            if (!i2cDevice.I2CWriteBytes(busID, buf))
                return false;
            waitMs(10);

            return true;
        }

        public bool setSerial(String serial)
        {
            char[] array = serial.ToCharArray();
            byte[] buffer = new byte[2 + array.Length];

            buffer[0] = 0x08;
            buffer[1] = (byte)array.Length;

            for (int i = 0; i < array.Length; i++)
                buffer[2 + i] = (byte)array[i];

            waitMs(10);
            if (!i2cDevice.I2CWriteBytes(busID, buffer))
                return false;
            waitMs(30);

            return true;
        }

        public String getSerial()
        {
            byte[] buf = new byte[34];

            waitMs(10);
            if (i2cDevice.I2CWriteByte(busID, 0x07) != true)
                return null;
            waitMs(30);
            if (i2cDevice.I2CReadBytes(busID, buf) != true)
                return null;

            int length = buf[0];
            if (length > 32)
                length = 0;

            char[] array = new char[length];
            for (int i = 0; i < length; i++)
                array[i] = (char)buf[1 + i];

            return new String(array);
        }

        public bool setATT(bool state)
        {
            waitMs(10);
            bool ret;
            if (state)
                ret = i2cDevice.I2CWriteByte(busID, 0x15);
            else
                ret = i2cDevice.I2CWriteByte(busID, 0x16);
            waitMs(10);

            return ret;
        }

        public bool setPreAmp(bool state)
        {
            waitMs(10);
            bool ret;
            if (state)
                ret = i2cDevice.I2CWriteByte(busID, 0x17);
            else
                ret = i2cDevice.I2CWriteByte(busID, 0x18);
            waitMs(10);

            return ret;
        }

        public bool setMGC(int dB)
        {
            if (dB < 0 || dB > 96)
                return false;

            byte[] cmd = new byte[2];

            cmd[0] = 0x32;
            cmd[1] = (byte)dB;

            waitMs(10);
            if (!i2cDevice.I2CWriteBytes(busID, cmd))
                return false;
            waitMs(10);

            return true;
        }

        public bool setAGC(int state)
        {
            if (state < AGC_OFF || state > AGC_MANUAL)
                return false;

            waitMs(10);
            if (!i2cDevice.I2CWriteByte(busID, (byte)(0x32 + state)))
                return false;
            waitMs(10);

            return true;
        }

        public bool setAGCCorrection(bool state, int offset, int gain)
        {
            byte[] cmd = new byte[4];

            if (state)
                cmd[0] = 0x37;
            else
                cmd[0] = 0x38;
            cmd[1] = (byte)offset;
            cmd[2] = (byte)(gain & 0xFF);
            cmd[3] = (byte)((gain >> 8) & 0xFF);

            waitMs(10);
            if (!i2cDevice.I2CWriteBytes(busID, cmd))
                return false;
            waitMs(10);

            return true;
        }

        public bool readAGCCorrection()
        {
            waitMs(10);
            if (!i2cDevice.I2CWriteByte(busID, 0x39))
                return false;

            byte[] buffer = new byte[3];
            waitMs(20);
            if (!i2cDevice.I2CReadBytes(busID, buffer))
                return false;

            this.agcCorrectionOffset = buffer[0];
            this.agcCorrectionGain = (buffer[2] << 8) | buffer[1];

            return true;
        }


        public long getAGCCorrectionOffset()
        {
            return this.agcCorrectionOffset;
        }

        public long getAGCCorrectionGain()
        {
            return this.agcCorrectionGain;
        }


        // AD6636 Functions
        public bool ad6636Reset()
        {
            waitMs(10);
            return i2cDevice.I2CWriteByte(busID, 0x05);
        }


        public long ad6636ReadReg(int address, int bytes)
        {
            return ad6636ReadReg(address, bytes, false);
        }

        public long ad6636ReadReg(int address, int bytes, bool cache)
        {

            if (bytes < 1 || bytes > 4)
                return 0;

            // read from cache only
            if (cache)
            {
                if (ad6636RegCache != null && address < ad6636RegCache.Length)
                {
                    if (ad6636RegCache[address].bytes == bytes)
                        return ad6636RegCache[address].value;
                }
            }

            byte[] cmd = new byte[3];

            cmd[0] = (byte)(0x1F + (bytes - 1));
            cmd[1] = (byte)address;
            cmd[2] = (byte)(bytes | 0x80);

            waitMs(10);
            if (!i2cDevice.I2CWriteBytes(busID, cmd))
                return 0;

            byte[] buffer = new byte[bytes];
            waitMs(10);
            if (!i2cDevice.I2CReadBytes(busID, buffer))
                return 0;

            long value = 0;
            for (int i = 0; i < bytes; i++)
            {
                value <<= 8;
                value |= buffer[bytes - 1 - i];
            }

            // cache the read value
            if (ad6636RegCache != null && address < ad6636RegCache.Length)
            {
                if (ad6636RegCache[address].bytes == bytes)
                    ad6636RegCache[address].value = value;
            }
            return value;
        }

        public bool ad6636WriteReg(int address, int bytes, long value)
        {
            return ad6636WriteReg(address, bytes, value, true);
        }

        public bool ad6636WriteReg(int address, int bytes, long value, bool cache)
        {

            if (bytes < 1 || bytes > 4)
                return false;

            if (cache)
            {
                if (ad6636RegCache == null)
                {
                    ad6636RegCache = new ad6636RegCacheEntry[0x100];
                    for (int pos = 0; pos < ad6636RegCache.Length; pos++)
                        ad6636RegCache[pos] = new ad6636RegCacheEntry();
                }

                if (address < ad6636RegCache.Length)
                {
                    ad6636RegCache[address].bytes = bytes;
                    ad6636RegCache[address].value = value;
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

            //waitMs(10);
            if (!i2cDevice.I2CWriteBytes(busID, cmd))
                return false;
            //waitMs(10);

            return true;
        }

    }
}
