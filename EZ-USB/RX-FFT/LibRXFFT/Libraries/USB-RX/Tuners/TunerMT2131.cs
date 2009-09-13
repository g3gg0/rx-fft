using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using System.Threading;

namespace LibRXFFT.Libraries.USB_RX.Tuners
{
    class TunerMT2131 : Tuner
    {
        public long IFFreq = 43998658;
        public long IFStepSize = 50000;

        private I2CInterface i2cDevice;
        private int busID;
        private long currentFrequency;

        private readonly byte defaultBusID = 0x60;
        public readonly byte[] mt2131_config1 = { 0x50, 0x00, 0x50, 0x80, 0x00, 0x49,
			0xfa, 0x88, 0x08, 0x77, 0x41, 0x04, 0x00, 0x00, 0x00, 0x32, 0x7f,
			0xda, 0x4c, 0x00, 0x10, 0xaa, 0x78, 0x80, 0xff, 0x68, 0xa0, 0xff,
			0xdd, 0x00, 0x00 };

        public readonly byte[] mt2131_config2 = new byte[] { 0x7f, 0xc8, 0x0a, 0x5f, 0x00, 0x04 };

        public readonly byte MT2131_REG_PWR = 0x07;
        public readonly byte MT2131_REG_LOCK = 0x08;
        public readonly byte MT2131_REG_UPC_1 = 0x0b;
        public readonly byte MT2131_REG_AGC_RL = 0x10;
        public readonly byte MT2131_REG_MISC_2 = 0x15;

        /* frequency values in KHz */
        public readonly int MT2131_IF1 = 1220;
        public readonly int MT2131_IF2 = 44000;
        public readonly int MT2131_FREF = 16000;


        public TunerMT2131(I2CInterface device)
        {
            i2cDevice = device;
            busID = defaultBusID;

            if (!exists())
            {
                return;
            }
            long oldFreq = GetFrequency();
            init();
            SetFrequency(oldFreq);

        }

        public long GetFrequency()
        {
            double f_lo1, f_lo2;
            double div1, num1, div2, num2;
            byte[] b = new byte[6];

            readReg(1, b);

            num1 = b[0] * 0x20 + b[1];
            div1 = b[2];
            num2 = b[3] * 0x20 + b[4];
            div2 = b[5];

            num1 = Math.Round(div1) * 0x2000 + num1;
            f_lo1 = num1 * (MT2131_FREF / 128.0) / 64.0;
            num2 = Math.Round(div2) * 0x2000 + num2;
            f_lo2 = num2 * (MT2131_FREF / 128.0) / 64.0;

            currentFrequency = (long)((f_lo1 - f_lo2 - MT2131_IF2) * 1000);
            /*
                    System.out.printf("READ PLL [0..5]: %2x %2x %2x %2x %2x %2x\n", (int) b[0],
                            (int) b[1], (int) b[2], (int) b[3], (int) b[4], (int) b[5]);
    		
                    System.out.println ( "=> "+(int) currentFrequency );
            */
            return currentFrequency;
        }

        public bool SetFrequency(long frequency)
        {
            long freq;
            byte if_band_center;
            double f_lo1, f_lo2;
            double div1, num1, div2, num2;
            byte[] b = new byte[6];

            /* more than 1 GHz not possible */
            if (frequency > 1000000000)
                return false;

            freq = frequency / 1000; // Hz -> kHz

            f_lo1 = freq + MT2131_IF1 * 1000;
            f_lo1 = (f_lo1 / 250) * 250;
            f_lo2 = f_lo1 - freq - MT2131_IF2;

            currentFrequency = (long)( (f_lo1 - f_lo2 - MT2131_IF2) * 1000);

            /* Frequency LO1 = 16MHz * (DIV1 + NUM1/8192 ) */
            num1 = f_lo1 * 64 / (MT2131_FREF / 128);
            div1 = num1 / 0x2000;
            num1 %= 0x2000;

            /* Frequency LO2 = 16MHz * (DIV2 + NUM2/8192 ) */
            num2 = f_lo2 * 64 / (MT2131_FREF / 128);
            div2 = num2 / 0x2000;
            num2 %= 0x2000;

            if (freq <= 82500)
                if_band_center = 0x00;
            else if (freq <= 137500)
                if_band_center = 0x01;
            else if (freq <= 192500)
                if_band_center = 0x02;
            else if (freq <= 247500)
                if_band_center = 0x03;
            else if (freq <= 302500)
                if_band_center = 0x04;
            else if (freq <= 357500)
                if_band_center = 0x05;
            else if (freq <= 412500)
                if_band_center = 0x06;
            else if (freq <= 467500)
                if_band_center = 0x07;
            else if (freq <= 522500)
                if_band_center = 0x08;
            else if (freq <= 577500)
                if_band_center = 0x09;
            else if (freq <= 632500)
                if_band_center = 0x0A;
            else if (freq <= 687500)
                if_band_center = 0x0B;
            else if (freq <= 742500)
                if_band_center = 0x0C;
            else if (freq <= 797500)
                if_band_center = 0x0D;
            else if (freq <= 852500)
                if_band_center = 0x0E;
            else if (freq <= 907500)
                if_band_center = 0x0F;
            else if (freq <= 962500)
                if_band_center = 0x10;
            else if (freq <= 1017500)
                if_band_center = 0x11;
            else if (freq <= 1072500)
                if_band_center = 0x12;
            else
                if_band_center = 0x13;

            b[0] = (byte)(num1 / 0x20);
            b[1] = (byte)(num1 % 0x20);
            b[2] = (byte)div1;
            b[3] = (byte)(num2 / 0x20);
            b[4] = (byte)(num2 % 0x20);
            b[5] = (byte)div2;

            /*
                    System.out.printf("IF1: %dMHz IF2: %dMHz\n", MT2131_IF1, MT2131_IF2);
                    System.out.printf("PLL freq=%dkHz  band=%d\n", (int) freq,
                            (int) if_band_center);
                    System.out.printf("PLL f_lo1=%dkHz  f_lo2=%dkHz\n", (int) f_lo1,
                            (int) f_lo2);
                    System.out.printf("PLL div1=%d  num1=%d  div2=%d  num2=%d\n",
                            (int) div1, (int) num1, (int) div2, (int) num2);
                    System.out.printf("WRITE PLL [0..5]: %2x %2x %2x %2x %2x %2x\n", (int) b[0],
                            (int) b[1], (int) b[2], (int) b[3], (int) b[4], (int) b[5]);
            */

            writeReg(0x01, b);
            writeReg(0x0b, if_band_center);

            /*
             * Wait for lock i = 0; do { mt2131_readreg(priv, 0x08, &lockval); if
             * ((lockval & 0x88) == 0x88) break; msleep(4); i++; } while (i < 10);
             */

            /* Wait for lock */
            int i = 20;
            do
            {
                readReg(MT2131_REG_LOCK, b);
                if ((b[0] & 0x88) == 0x88)
                    break;
                try
                {
                    Thread.Sleep(5);
                }
                catch (ThreadAbortException e) 
                { 
                }
                i--;
            } while (i > 0);

            if (i == 0)
                return false;

            return true;
        }

        public void readReg(byte register, byte[] buf)
        {
            this.i2cDevice.I2CWriteByte(busID, register);
            this.i2cDevice.I2CReadBytes(busID, buf);
        }

        public bool writeReg(byte register, byte[] data)
        {
            byte[] buf = new byte[data.Length + 1];

            buf[0] = register;
            for (int pos = 0; pos < data.Length; pos++)
                buf[1 + pos] = data[pos];

            return this.i2cDevice.I2CWriteBytes(busID, buf);
        }

        public bool writeReg(byte register, byte data)
        {
            byte[] buf = new byte[2];

            buf[0] = register;
            buf[1] = data;

            return this.i2cDevice.I2CWriteByte(busID, register);
        }

        public void init()
        {
            this.writeReg(0x01, mt2131_config1);
            this.writeReg(0x0b, 0x09);
            this.writeReg(0x15, 0x47);
            this.writeReg(0x07, 0xf2);
            this.writeReg(0x0b, 0x01);
            this.writeReg(0x10, mt2131_config2);

            return;
        }


        public bool exists()
        {
            byte[] buf = new byte[1];
            readReg(0x00, buf);

            if (buf[0] != 0x3E && buf[0] != 0x3F)
                return false;

            return true;
        }

        public bool InvertedSpectrum
        {
            get
            {
                return true;
            }
        }
    }
}
