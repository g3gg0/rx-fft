using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.USB_RX.Tuners;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using LibRXFFT.Libraries.USB_RX.Misc;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class AD6636 : Tuner
    {

        private AD6636Interface device;
        private long ncoFreq;
        private double ncoMul;
        private static int AD6636_REG_NCOFREQ = 0x70;
        private static int AD6636_REG_NCOFREQ_L = 4;
        private static int AD6636_REG_IOAC = 0x02;
        private static int AD6636_REG_IOAC_L = 1;
        private static int AD6636_REG_SOFTSYNC = 0x05;
        private static int AD6636_REG_SOFTSYNC_L = 1;
        private static int AD6636_REG_AGCHOR = 0xA0;
        private static int AD6636_REG_AGCHOR_L = 2;
        private static int AD6636_REG_AGCCR = 0xA2;
        private static int AD6636_REG_AGCCR_L = 2;
        private static int AD6636_REG_AGCSGR = 0xA4;
        private static int AD6636_REG_AGCSGR_L = 2;
        private static int AD6636_REG_AGCUD = 0xA6;
        private static int AD6636_REG_AGCUD_L = 2;
        private static int AD6636_REG_AGCPL = 0xA8;
        private static int AD6636_REG_AGCPL_L = 1;
        private static int AD6636_REG_AGCAS = 0xA9;
        private static int AD6636_REG_AGCAS_L = 1;
        private static int AD6636_REG_AGCLG = 0xAC;
        private static int AD6636_REG_AGCLG_L = 1;
        private static int AD6636_REG_AGCET = 0xAA;
        private static int AD6636_REG_AGCET_L = 2;
        private static int AD6636_REG_PPOC = 0xBC;
        private static int AD6636_REG_PPOC_L = 3;
        private static int AD6636_REG_OPC = 0xC0;
        private static int AD6636_REG_OPC_L = 2;

        private int[,] initTable = new[,]
	{
		{1, 1, 0},
		{2, 1, 0},
		{3, 1, 15},
		{4, 1, 0},
		{6, 2, 1536},
		{10, 2, 0},
		{12, 4, 33591296},
		{16, 2, 72},
		{20, 2, 0},
		{22, 2, 0},
		{24, 2, 0},
		{26, 2, 0},
		{28, 2, 0},
		{30, 2, 0},
		{32, 2, 0},
		{34, 2, 0},
		{36, 2, 0},
		{38, 2, 0},
		{40, 3, 0},
		{43, 1, 0},
		{44, 3, 4096},
		{51, 1, 25},
		{52, 2, 0},
		{54, 2, 0},
		{56, 3, 0},
		{59, 1, 0},
		{60, 3, 0},
		{67, 1, 0},
		{68, 2, 0},
		{70, 2, 0},
		{72, 3, 0},
		{75, 1, 0},
		{76, 3, 0},
		{83, 1, 0},
		{84, 2, 0},
		{86, 2, 0},
		{88, 3, 0},
		{91, 1, 0},
		{92, 3, 0},
		{99, 1, 0},
		{100, 2, 0},
		{102, 2, 0},
		{192, 2, 655},
        {-1, -1, -1}
	};

        private long cachedRegister = 0;

        public AD6636(AD6636Interface device, long ncoFreq)
        {
            this.device = device;
            setNcoFreq(ncoFreq);

            Init();
        }

        public AD6636(AD6636Interface device)
        {
            this.device = device;

            Init();
        }

        public void Init()
        {
            int pos = 0;
            while (initTable[pos, 0] != -1)
            {
                this.device.ad6636WriteReg(initTable[pos, 0], initTable[pos, 1], initTable[pos, 2]);
                pos++;
            }
        }

        public long getNcoFreq()
        {
            return ncoFreq;
        }

        public void setNcoFreq(long ncoFreq)
        {
            this.ncoFreq = ncoFreq;
            this.ncoMul = Math.Pow(2, 32) / ((double)ncoFreq);
        }

        public override bool setFrequency(double frequency)
        {
            double regValue = frequency * this.ncoMul;

            this.device.ad6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 0x0F);
            this.device.ad6636WriteReg(AD6636_REG_NCOFREQ, AD6636_REG_NCOFREQ_L, (long)Math.Round(regValue));
            this.device.ad6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0x00);
            this.device.ad6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0xCF);
            this.device.ad6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0xCF);
            return true;
        }

        public bool setMGCValue(int value)
        {
            this.device.ad6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 0x0B);

            if (value > 0)
            {
                int mgcFactor = (int)(4095 - (value - 1) * 41.36);
                this.device.ad6636WriteReg(AD6636_REG_AGCLG + 0, AD6636_REG_AGCLG_L, 0);
                this.device.ad6636WriteReg(AD6636_REG_AGCLG + 1, AD6636_REG_AGCLG_L, 0);
                this.device.ad6636WriteReg(AD6636_REG_AGCCR, AD6636_REG_AGCCR_L, 0x040C);
                this.device.ad6636WriteReg(AD6636_REG_AGCSGR, AD6636_REG_AGCSGR_L, mgcFactor);
            }
            else
                this.device.ad6636WriteReg(AD6636_REG_AGCCR, AD6636_REG_AGCCR_L, 0x040D);

            this.device.ad6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0x00);
            this.device.ad6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0xCF);
            this.device.ad6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0xCF);

            return true;
        }

        public bool setAGC()
        {
            this.device.ad6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 0x0B);

            this.device.ad6636WriteReg(AD6636_REG_AGCCR, AD6636_REG_AGCCR_L, 0x040C);
            this.device.ad6636WriteReg(AD6636_REG_AGCSGR, AD6636_REG_AGCSGR_L, 128);
            this.device.ad6636WriteReg(AD6636_REG_AGCUD, AD6636_REG_AGCUD_L, 16);
            this.device.ad6636WriteReg(AD6636_REG_AGCPL, AD6636_REG_AGCPL_L, 4);
            this.device.ad6636WriteReg(AD6636_REG_AGCAS, AD6636_REG_AGCAS_L, 2);
            this.device.ad6636WriteReg(AD6636_REG_AGCET, AD6636_REG_AGCET_L, 64);
            this.device.ad6636WriteReg(AD6636_REG_AGCLG + 0, AD6636_REG_AGCLG_L, 16);
            this.device.ad6636WriteReg(AD6636_REG_AGCLG + 1, AD6636_REG_AGCLG_L, 8);
            this.device.ad6636WriteReg(AD6636_REG_AGCLG + 2, AD6636_REG_AGCLG_L, 6);

            this.device.ad6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0x00);
            this.device.ad6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0xCF);
            this.device.ad6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0xCF);

            return true;
        }

        public bool setFilter(FilterFile filter)
        {

            this.device.ad6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 0x01);
            this.device.ad6636WriteReg(124, 2, 13);

            this.device.ad6636WriteReg(104, 2, 0);
            this.device.ad6636WriteReg(108, 2, 5);
            this.device.ad6636WriteReg(110, 2, 5);

            this.device.ad6636WriteReg(116, 2, 0);

            int filterFlags = 0;
            if (filter.HB2)
                filterFlags |= 0x01;
            if (filter.FIR2)
                filterFlags |= 0x02;
            if (filter.HB1)
                filterFlags |= 0x04;
            if (filter.FIR1)
                filterFlags |= 0x08;
            this.device.ad6636WriteReg(120, 1, filterFlags);

            this.device.ad6636WriteReg(121, 1, filter.CIC5Scale);
            this.device.ad6636WriteReg(122, 1, filter.CIC5Decimation - 1);

            if (filter.CIC5)
                this.device.ad6636WriteReg(123, 1, 0);
            else
                this.device.ad6636WriteReg(123, 1, 1);

            this.device.ad6636WriteReg(136, 1, filter.DRCFNTaps - 1);
            this.device.ad6636WriteReg(137, 1, (long)(64 - filter.DRCFNTaps / 2));
            this.device.ad6636WriteReg(138, 2, 0x700 | (filter.DRCFDecimation - 1) << 4);

            this.device.ad6636WriteReg(148, 1, filter.CRCFNTaps - 1);
            this.device.ad6636WriteReg(149, 1, (long)(64 - filter.CRCFNTaps / 2));
            this.device.ad6636WriteReg(150, 2, 0x700 | (filter.CRCFDecimation - 1) << 4);

            this.device.ad6636WriteReg(184, 2, 0);

            int DRCFEntries = (int)((filter.DRCFNTaps + 1) / 2);
            this.device.ad6636WriteReg(140, 1, 0);
            this.device.ad6636WriteReg(141, 1, DRCFEntries - 1);
            for (int pos = 0; pos < DRCFEntries; pos++)
                this.device.ad6636WriteReg(144, 2, filter.DRCFTaps[DRCFEntries - 1 + pos]);

            int CRCFEntries = (int)((filter.CRCFNTaps + 1) / 2);
            this.device.ad6636WriteReg(152, 1, 0);
            this.device.ad6636WriteReg(153, 1, CRCFEntries - 1);
            for (int pos = 0; pos < CRCFEntries; pos++)
                this.device.ad6636WriteReg(156, 3, filter.CRCFTaps[CRCFEntries - 1 + pos]);

            this.device.ad6636WriteReg(184, 2, 0);

            this.device.ad6636WriteReg(3, 1, 0);
            this.device.ad6636WriteReg(3, 1, 1);
            this.device.ad6636WriteReg(188, 3, 1);
            this.device.ad6636WriteReg(5, 1, 0);
            this.device.ad6636WriteReg(5, 1, 207);
            this.device.ad6636WriteReg(5, 1, 207);

            return true;
        }

        public bool muteOutput()
        {
            /*
            // first read the value to make sure its in the cache
            this.device.ad6636ReadReg(AD6636_REG_OPC, AD6636_REG_OPC_L);

            // then disable all outputs
            this.device.ad6636WriteReg(AD6636_REG_OPC, AD6636_REG_OPC_L, 0, false );
            */

            cachedRegister = this.device.ad6636ReadReg(AD6636_REG_PPOC, AD6636_REG_PPOC_L);

            // then disable all outputs
            this.device.ad6636WriteReg(AD6636_REG_PPOC, AD6636_REG_PPOC_L, 0, false);


            return true;
        }

        public bool unmuteOutput()
        {
            /*
            // read the cached value from cache
            long cached = this.device.ad6636ReadReg(AD6636_REG_OPC, AD6636_REG_OPC_L, true);
		
            // write back value from cache
            this.device.ad6636WriteReg(AD6636_REG_OPC, AD6636_REG_OPC_L, cached );
            */

            // read the cached value from cache
            //long cached = this.device.ad6636ReadReg(AD6636_REG_PPOC, AD6636_REG_PPOC_L, true);

            // write back value from cache
            this.device.ad6636WriteReg(AD6636_REG_PPOC, AD6636_REG_PPOC_L, cachedRegister);

            //this.device.ad6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 0x0B );


            return true;
        }

        public override double getFrequency()
        {
            return (double)this.device.ad6636ReadReg(AD6636_REG_NCOFREQ, AD6636_REG_NCOFREQ_L) / this.ncoMul;
        }

        public override bool isSpectrumInverted()
        {
            return false;
        }

    }
}
