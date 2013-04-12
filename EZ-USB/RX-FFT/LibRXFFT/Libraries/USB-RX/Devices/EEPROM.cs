using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using System.Threading;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class EEPROM
    {
        private I2CInterface I2CDevice;
        private int BusID;
        private static int DefaultBusID = 0x51;
        private bool Use16Bit = false;
        public int Size = 0;
        public int AddressWidth
        {
            get
            {
                return Use16Bit ? 16 : 8;
            }
        }

        public EEPROM(I2CInterface device)
            : this(device, DefaultBusID)
        {
        }

        public EEPROM(I2CInterface device, int busID)
        {
            I2CDevice = device;
            BusID = busID;

            if (Exists)
            {
                AutodetectAddressing();
                AutodetectSize();
            }
        }

        public bool Exists
        {
            get
            {
                return I2CDevice.I2CDeviceAck(BusID);
            }
        }

        public bool AutodetectSize()
        {
            byte baseData = 0;
            byte checkData = 0; 
            int size = 0;

            /* backup original byte at pos 0 */
            if (!ReadByte(0, ref baseData))
            {
                Log.AddMessage("EEPROM:AutodetectSize(): Failed to read byte at pos 0.");
                return false;
            }

            /* write our own */
            if (!WriteByte(0, 0xA5))
            {
                Log.AddMessage("EEPROM:AutodetectSize(): Failed to write byte at pos 0.");
                return false;
            }

            /* read back and verify */
            if (!ReadByte(0, ref checkData) || checkData != 0xA5)
            {
                Log.AddMessage("EEPROM:AutodetectSize(): Verification failed of byte at pos 0.");
                if (!WriteByte(0, baseData))
                {
                    Log.AddMessage("EEPROM:AutodetectSize(): Failed to restore byte at pos 0.");
                    return false;
                }

                return false;
            }

            /* detect size from 64 byte up to the max size */
            for (int sizeBit = 1; sizeBit < AddressWidth; sizeBit++)
            {
                size = (1 << sizeBit);
                byte backupData = 0;

                /* backup original data */
                if (!ReadByte(size, ref backupData))
                {
                    Log.AddMessage("EEPROM:AutodetectSize(): Failed to backup byte at pos " + size + ".");
                    return false;
                }

                /* write some test data */
                if (!WriteByte(size, 0x5A))
                {
                    Log.AddMessage("EEPROM:AutodetectSize(): Failed to write test byte at pos " + size + ".");
                    return false;
                }

                /* check if it was written to address 0 */
                if (!ReadByte(0, ref checkData))
                {
                    Log.AddMessage("EEPROM:AutodetectSize(): Failed to read check byte at pos " + size + ".");
                    return false;
                }

                /* wrapped over. EEPROM is of the current size. */
                if (checkData == 0x5A)
                {
                    break;
                }

                /* address counter did not wrap, restore old data */
                if (!WriteByte(size, backupData))
                {
                    Log.AddMessage("EEPROM:AutodetectSize(): Failed to restore byte at pos " + size + ".");
                    return false;
                }
            }

            /* restore old data at position 0 */
            if (!WriteByte(0, baseData))
            {
                Log.AddMessage("EEPROM:AutodetectSize(): Failed to restore byte at pos 0.");
                return false;
            }

            Size = size;
            return true;
        }

        public bool AutodetectAddressing()
        {
            byte[] buffer = new byte[3];
            byte[] backup8 = new byte[2];
            byte[] backup16 = new byte[1];

            /* first backup data at 0x00 and 0x01 */
            /* transmit read address */
            if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)0 }))
            {
                Log.AddMessage("EEPROM:AutodetectAddressing(): Failed to read header in 8 bit mode.");
                return false;
            }

            Thread.Sleep(10);

            /* read data */
            if (!I2CDevice.I2CReadBytes(BusID, backup8))
            {
                Log.AddMessage("EEPROM:AutodetectAddressing(): Failed to receive header in 8 bit mode.");
                return false;
            }


            /* first backup data at 0x00 for 16 bit eeproms */
            /* transmit read address */
            if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)0, (byte)0 }))
            {
                Log.AddMessage("EEPROM:AutodetectAddressing(): Failed to read header in 16 bit mode.");
                return false;
            }

            Thread.Sleep(10);

            /* read data */
            if (!I2CDevice.I2CReadBytes(BusID, backup16))
            {
                Log.AddMessage("EEPROM:AutodetectAddressing(): Failed to receive header in 16 bit mode.");
                return false;
            }


            /* write the test word */
            buffer[0] = (byte)0;
            buffer[1] = (byte)0;
            buffer[2] = (byte)0xAA;

            if (!I2CDevice.I2CWriteBytes(BusID, buffer))
            {
                Log.AddMessage("EEPROM:AutodetectAddressing(): Failed to write test header in 16 bit mode.");
                return false;
            }

            Thread.Sleep(10);

            /* read back what was placed in EEPROM */
            if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)0 }))
            {
                Log.AddMessage("EEPROM:AutodetectAddressing(): Failed to read test header in 8 bit mode.");
                return false;
            }

            Thread.Sleep(10);

            if (!I2CDevice.I2CReadBytes(BusID, buffer))
            {
                Log.AddMessage("EEPROM:AutodetectAddressing(): Failed to receive test header in 8 bit mode.");
                return false;
            }

            /* the second 0x00 upon address write got interpreted as data - its a 8 bit eeprom */
            if (buffer[0] == 0x00 && buffer[1] == 0xAA)
            {
                Use16Bit = false;

                /* addressig is determined now, write back backed up data. may be crap if the eeprom is 16 bit wide. */
                WriteBytes(0, backup8);

                return true;
            }


            /* read back what was placed in EEPROM, but use 16 bit addressing */
            if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)0, (byte)0 }))
            {
                Log.AddMessage("EEPROM:AutodetectAddressing(): Failed to read test header in 16 bit mode.");
                return false;
            }

            Thread.Sleep(10);

            if (!I2CDevice.I2CReadBytes(BusID, buffer))
            {
                Log.AddMessage("EEPROM:AutodetectAddressing(): Failed to receive test header in 16 bit mode.");
                return false;
            }


            if (buffer[0] == 0xAA)
            {
                Use16Bit = true;

                /* addressig is determined now, write back backed up data. */
                WriteBytes(0, backup16);
            }
            else
            {
                Log.AddMessage("EEPROM:AutodetectAddressing(): Inexpected result when reading in 16 bit mode: 0x" + buffer[0].ToString("X2"));
                return false;
            }

            return true;
        }

        public bool WriteBytes(int pos, byte[] data)
        {
            if (Use16Bit)
            {
                byte[] buffer = new byte[data.Length + 2];

                Array.Copy(data, 0, buffer, 2, data.Length);
                buffer[0] = (byte)(pos >> 8);
                buffer[1] = (byte)(pos & 0xFF);

                if (!I2CDevice.I2CWriteBytes(BusID, buffer))
                {
                    return false;
                }
            }
            else
            {
                byte[] buffer = new byte[data.Length + 1];

                Array.Copy(data, 0, buffer, 1, data.Length);
                buffer[0] = (byte)(pos & 0xFF);

                if (!I2CDevice.I2CWriteBytes(BusID, buffer))
                {
                    return false;
                }
            }

            return WaitAck();
        }

        public bool WriteByte(int pos, byte data)
        {
            if (Use16Bit)
            {
                if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)(pos >> 8), (byte)(pos & 0xFF), data }))
                {
                    return false;
                }
            }
            else
            {
                if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)(pos & 0xFF), data }))
                {
                    return false;
                }
            }

            return WaitAck();
        }

        private bool WaitAck()
        {
            for (int loop = 0; loop < 10; loop++)
            {
                Thread.Sleep(5);
                if (I2CDevice.I2CDeviceAck(BusID))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ReadByte(int pos, ref byte data)
        {
            byte[] buffer = new byte[1];

            if (Use16Bit)
            {
                /* transmit read address */
                if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)(pos >> 8), (byte)(pos & 0xFF) }))
                {
                    return false;
                }
            }
            else
            {
                if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)(pos & 0xFF) }))
                {
                    return false;
                }
            }

            Thread.Sleep(10);

            /* read data */
            if (!I2CDevice.I2CReadBytes(BusID, buffer))
            {
                return false;
            }

            data = buffer[0];
            return WaitAck();
        }

        public bool ReadBytes(int pos, byte[] data)
        {
            if (Use16Bit)
            {
                /* transmit read address */
                if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)(pos >> 8), (byte)(pos & 0xFF) }))
                {
                    return false;
                }
            }
            else
            {
                if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)(pos & 0xFF) }))
                {
                    return false;
                }
            }


            Thread.Sleep(10);

            /* read data */
            if (!I2CDevice.I2CReadBytes(BusID, data))
            {
                return false;
            }

            return WaitAck();
        }
    }
}
