﻿using System;
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
        private long _FilterWidth;
        private long AGCCorrectionOffset;
        private long AGCCorrectionGain;

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

        // Select bandpass
        public bool SetBP(int index)
        {
            if (index < 0 || index > 3)
                return false;
            WaitMs(10);
            bool ret = I2CDevice.I2CWriteByte(BusID, (byte)(0x19 + index));
            WaitMs(10);

            return ret;
        }


        // FIFO Functions
        public bool FIFOReset(bool state)
        {
            WaitMs(10);
            bool ret;
            if (state)
                ret = I2CDevice.I2CWriteByte(BusID, 0x28);
            else
                ret = I2CDevice.I2CWriteByte(BusID, 0x29);
            WaitMs(10);

            return ret;
        }

        public bool FIFOReset()
        {
            return I2CDevice.I2CWriteByte(BusID, 0x0d);
        }

        // Filter stuff
        public int GetFilterCount()
        {
            byte[] buf = new byte[1];

            WaitMs(10);
            if (!I2CDevice.I2CWriteByte(BusID, 0x64))
                return 0;

            WaitMs(10);
            if (!I2CDevice.I2CReadByte(BusID, buf))
                return 0;

            return (int)buf[0];
        }

        public int GetLastFilter()
        {
            byte[] buf = new byte[1];

            WaitMs(10);
            if (!I2CDevice.I2CWriteByte(BusID, 0x10))
                return 0;

            WaitMs(10);
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

            WaitMs(10);
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return false;

            byte[] buf = new byte[9];
            WaitMs(150);
            if (!I2CDevice.I2CReadBytes(BusID, buf))
                return false;

            this.FilterClock = buf[5] + buf[6] * 0x100 + buf[7] * 0x10000 + buf[8] * 0x1000000;
            this._FilterWidth = buf[1] + buf[2] * 0x100 + buf[3] * 0x10000 + buf[4] * 0x1000000;

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

            WaitMs(10);
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return false;

            byte[] buf = new byte[9];
            WaitMs(150);
            if (!I2CDevice.I2CReadBytes(BusID, buf))
                return false;

            this.FilterClock = buf[5] + buf[6] * 0x100 + buf[7] * 0x10000 + buf[8] * 0x1000000;
            this._FilterWidth = buf[1] + buf[2] * 0x100 + buf[3] * 0x10000 + buf[4] * 0x1000000;

            return true;
        }

        public long GetFilterClock()
        {
            return FilterClock;
        }

        public long GetFilterWidth()
        {
            return _FilterWidth;
        }

        public int TCXOFreq
        {
            get
            {
                byte[] buf = new byte[4];

                WaitMs(10);
                if (!I2CDevice.I2CWriteByte(BusID, 0xC8))
                    return 0;

                WaitMs(20);
                if (!I2CDevice.I2CReadBytes(BusID, buf))
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

            set
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

                WaitMs(10);
                if (!I2CDevice.I2CWriteBytes(BusID, buf))
                    return;
                WaitMs(20);

                return;
            }

        }


        public int GetRBW()
        {
            byte[] buf = new byte[4];

            WaitMs(10);
            if (!I2CDevice.I2CWriteByte(BusID, 0xCC))
                return 0;
            WaitMs(20);
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

            WaitMs(10);
            if (!I2CDevice.I2CWriteBytes(BusID, buf))
                return false;
            WaitMs(10);

            return true;
        }

        public string SerialNumber
        {
            get
            {
                byte[] buf = new byte[34];

                WaitMs(10);
                if (I2CDevice.I2CWriteByte(BusID, 0x07) != true)
                    return null;
                WaitMs(30);
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

                WaitMs(10);
                if (!I2CDevice.I2CWriteBytes(BusID, buffer))
                    return;
                WaitMs(30);
            }
        }


        public bool SetATT(bool state)
        {
            WaitMs(10);
            bool ret;
            if (state)
                ret = I2CDevice.I2CWriteByte(BusID, 0x15);
            else
                ret = I2CDevice.I2CWriteByte(BusID, 0x16);
            WaitMs(10);

            return ret;
        }

        public bool SetPreAmp(bool state)
        {
            WaitMs(10);
            bool ret;
            if (state)
                ret = I2CDevice.I2CWriteByte(BusID, 0x17);
            else
                ret = I2CDevice.I2CWriteByte(BusID, 0x18);
            WaitMs(10);

            return ret;
        }

        public bool SetMGC(int dB)
        {
            if (dB < 0 || dB > 96)
                return false;

            byte[] cmd = new byte[2];

            cmd[0] = 0x32;
            cmd[1] = (byte)dB;

            WaitMs(10);
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return false;
            WaitMs(10);

            return true;
        }

        public bool SetAGC(int state)
        {
            if (state < AGC_OFF || state > AGC_MANUAL)
                return false;

            WaitMs(10);
            if (!I2CDevice.I2CWriteByte(BusID, (byte)(0x32 + state)))
                return false;
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

            WaitMs(10);
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return false;
            WaitMs(10);

            return true;
        }

        public bool ReadAGCCorrection()
        {
            WaitMs(10);
            if (!I2CDevice.I2CWriteByte(BusID, 0x39))
                return false;

            byte[] buffer = new byte[3];
            WaitMs(20);
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
            WaitMs(10);
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

            WaitMs(10);
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return 0;

            byte[] buffer = new byte[bytes];
            WaitMs(10);
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

            //waitMs(10);
            if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                return false;
            //waitMs(10);

            return true;
        }

    }
}