using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using System.Threading;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class EEPROM
    {
        private I2CInterface I2CDevice;
        private int BusID;
        private static int DefaultBusID = 0x51;
        public int Size = 128;
        public EEPROM(I2CInterface device)
            : this(device, DefaultBusID)
        {
        }

        public EEPROM(I2CInterface device, int busID)
        {
            I2CDevice = device;
            BusID = busID;
        }

        public bool AutodetectSize()
        {
            byte baseData = 0;
            byte checkData = 0; 
            int size = 0;

            byte[] buffer = new byte[8];
            buffer[0] = 0xC0; // C0 HEADER
            buffer[1] = 0xB4; // Cypress 0x04B4
            buffer[2] = 0x04; // 
            buffer[3] = 0x00; // Product 0xEE00
            buffer[4] = 0xEE; // 
            buffer[5] = 0x00; // Release 0x0001
            buffer[6] = 0x01; // 
            buffer[7] = 0x01; // Config Byte 0x01

            if (!WriteBytes(0, buffer))
            {
                return false;
            }
            if (!ReadBytes(0, buffer))
            {
                return false;
            }

            /* backup original byte at pos 0 */
            if (!ReadByte(0, ref baseData))
            {
                return false;
            }

            /* write our own */
            if (!WriteByte(0, 0xA5))
            {
                return false;
            }

            /* read back and verify */
            if (!ReadByte(0, ref checkData) || checkData != 0xA5)
            {
                if (!WriteByte(0, baseData))
                {
                    return false;
                }

                return false;
            }

            /* detect size from 64 byte up to  */
            for (int sizeBit = 1; sizeBit < 16; sizeBit++)
            {
                size = (1 << sizeBit);
                byte backupData = 0;


                /* backup original data */
                if (!ReadByte(size, ref backupData))
                {
                    return false;
                }

                /* write some test data */
                if (!WriteByte(size, 0x5A))
                {
                    return false;
                }

                /* check if it was written to address 0 */
                if (!ReadByte(0, ref checkData))
                {
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
                    return false;
                }
            }

            /* restore old data at position 0 */
            if (!WriteByte(0, baseData))
            {
                return false;
            }

            Size = size;
            return true;
        }

        private bool WriteBytes(int pos, byte[] data)
        {
            byte[] buffer = new byte[data.Length + 2];

            Array.Copy(data, 0, buffer, 2, data.Length);
            buffer[0] = (byte)(pos >> 8);
            buffer[1] = (byte)(pos & 0xFF);

            if (!I2CDevice.I2CWriteBytes(BusID, buffer))
            {
                return false;
            }

            return WaitAck();
        }

        private bool WriteByte(int pos, byte data)
        {
            if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)(pos >> 8), (byte)(pos & 0xFF), data }))
            {
                return false;
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

        private bool ReadByte(int pos, ref byte data)
        {
            byte[] buffer = new byte[1];

            /* transmit read address */
            if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)(pos >> 8), (byte)(pos & 0xFF) }))
            {
                return false;
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

        private bool ReadBytes(int pos, byte[] data)
        {
            /* transmit read address */
            if (!I2CDevice.I2CWriteBytes(BusID, new byte[] { (byte)(pos >> 8), (byte)(pos & 0xFF) }))
            {
                return false;
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
