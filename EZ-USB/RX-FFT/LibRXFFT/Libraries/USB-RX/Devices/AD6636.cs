﻿using System;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using LibRXFFT.Libraries.USB_RX.Misc;
using LibRXFFT.Libraries.USB_RX.Tuners;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class AD6636 : DigitalTuner
    {
        private long FilterRate = 0;
        private long _FilterWidth = 0;
        private long CurrentFrequency;
        private int CurrentChannel = 0;
        public bool AgcState = false;

        private AD6636Interface Device;
        private double NCOMul;
        private long _NCOFreq;
        private long CachedRegister = 0;

        private FilterInformation CurrentFilter;


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

        private int[,] RegisterInitTable = new[,]
	    {
		    {1, 1, 0},
		    {2, 1, 0},
		    {3, 1, 0x0F},
		    {4, 1, 0},
		    {6, 2, 0x600},
		    {10, 2, 0},
		    {12, 4, 0x2009000},
		    {16, 2, 0x48},
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
		    {44, 3, 0x1000},
		    {51, 1, 0x19},
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
		    {192, 2, 0x28F},
            {-1, -1, -1}
	    };


        public AD6636(AD6636Interface device) : this(device, 98304000) { }

        public AD6636(AD6636Interface device, long ncoFreq)
        {
            this.Device = device;
            NCOFreq = ncoFreq;

            InitRegisters();

            Device.Register(this);
        }

        public void ReInit()
        {
            long freq = CurrentFrequency;
            CurrentFrequency = 0;

            InitRegisters();
            SetMgcValue((int)Amplification);

            if (CurrentFilter is AtmelFilter)
            {
                SetFilter((AtmelFilter)CurrentFilter);
            }
            if (CurrentFilter is AD6636FilterFile)
            {
                SetFilter(CurrentChannel, (AD6636FilterFile)CurrentFilter);
            }

            SetFrequency(freq);
            SoftSync();
        }

        public void InitRegisters()
        {
            int pos = 0;

            lock (this)
            {
                while (RegisterInitTable[pos, 0] != -1)
                {
                    Device.AD6636WriteReg(RegisterInitTable[pos, 0], RegisterInitTable[pos, 1], RegisterInitTable[pos, 2]);
                    pos++;
                }
            }
        }

        public long NCOFreq
        {
            get
            {
                return _NCOFreq;
            }

            set
            {
                _NCOFreq = value;
                NCOMul = Math.Pow(2, 32) / ((double)value);
            }
        }


        public bool SetMgcValue(int value)
        {
            bool success = true;
            value = Math.Min(96, value);
            lock (this)
            {
                success &= this.Device.AD6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 0x0B);

                if (value > 0)
                {
                    int mgcFactor = (int)(4095 - (value - 1) * 41.36);
                    success &= this.Device.AD6636WriteReg(AD6636_REG_AGCCR, AD6636_REG_AGCCR_L, 0x040C);
                    success &= this.Device.AD6636WriteReg(AD6636_REG_AGCLG + 0, AD6636_REG_AGCLG_L, 0);
                    success &= this.Device.AD6636WriteReg(AD6636_REG_AGCLG + 1, AD6636_REG_AGCLG_L, 0);
                    success &= this.Device.AD6636WriteReg(AD6636_REG_AGCSGR, AD6636_REG_AGCSGR_L, mgcFactor);
                }
                else
                {
                    success &= this.Device.AD6636WriteReg(AD6636_REG_AGCCR, AD6636_REG_AGCCR_L, 0x040D);
                }

                AgcState = false;

                _Amplification = value;
                success &= SoftSync();
            }
            return success;
        }

        public bool SetAgc()
        {
            bool success = true;
            lock (this)
            {
                success &= this.Device.AD6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 0x0B);
                success &= this.Device.AD6636WriteReg(AD6636_REG_AGCCR, AD6636_REG_AGCCR_L, 0x040C);
                success &= this.Device.AD6636WriteReg(AD6636_REG_AGCSGR, AD6636_REG_AGCSGR_L, 128);
                success &= this.Device.AD6636WriteReg(AD6636_REG_AGCUD, AD6636_REG_AGCUD_L, 16);
                success &= this.Device.AD6636WriteReg(AD6636_REG_AGCPL, AD6636_REG_AGCPL_L, 4);
                success &= this.Device.AD6636WriteReg(AD6636_REG_AGCAS, AD6636_REG_AGCAS_L, 2);
                success &= this.Device.AD6636WriteReg(AD6636_REG_AGCET, AD6636_REG_AGCET_L, 64);
                success &= this.Device.AD6636WriteReg(AD6636_REG_AGCLG + 0, AD6636_REG_AGCLG_L, 16);
                success &= this.Device.AD6636WriteReg(AD6636_REG_AGCLG + 1, AD6636_REG_AGCLG_L, 8);
                success &= this.Device.AD6636WriteReg(AD6636_REG_AGCLG + 2, AD6636_REG_AGCLG_L, 6);
                
                AgcState = true;

                success &= SoftSync();
                _Amplification = 0;
            }
            return success;
        }

        public bool SelectChannel(int index)
        {
            bool success = true;
            int bitMask = (1 << index);

            lock (this)
            {
                success &= this.Device.AD6636WriteReg(3, 1, 0);
                success &= this.Device.AD6636WriteReg(3, 1, bitMask);
                success &= this.Device.AD6636WriteReg(188, 3, 1);

                success &= SoftSync();

                CurrentChannel = index;
            }
            return success;
        }


        /* used if atmel uploads filter */
        public bool SetFilter(AtmelFilter filter)
        {
            bool success = true;
            CurrentFilter = filter;
            FilterRate = filter.Rate;
            _FilterWidth = filter.Width;

            /* inform listeners */
            if (SamplingRateChanged != null)
                SamplingRateChanged(this, null);
            if (FilterWidthChanged != null)
                FilterWidthChanged(this, null);

            return success;
        }

        public bool SetFilter(AD6636FilterFile filter)
        {
            return SetFilter(0, filter);
        }

        public bool SetFilter(int channel, AD6636FilterFile filter)
        {
            bool success = true;
            CurrentFilter = filter;
            FilterRate = filter.Rate;
            _FilterWidth = filter.Width;

            lock (this)
            {
                switch (channel)
                {
                    case 0:
                        success &= this.Device.AD6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 1);
                        success &= this.Device.AD6636WriteReg(124, 2, 13);
                        break;
                    case 1:
                        success &= this.Device.AD6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 2);
                        success &= this.Device.AD6636WriteReg(124, 2, 1037);
                        break;
                    case 2:
                        success &= this.Device.AD6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 4);
                        success &= this.Device.AD6636WriteReg(124, 2, 2049);
                        break;
                    case 3:
                        success &= this.Device.AD6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 8);
                        success &= this.Device.AD6636WriteReg(124, 2, 3073);
                        break;
                    default:
                        return false;
                }

                success &= this.Device.AD6636WriteReg(104, 2, 0);
                success &= this.Device.AD6636WriteReg(108, 2, 5);
                success &= this.Device.AD6636WriteReg(110, 2, 5);

                success &= this.Device.AD6636WriteReg(116, 2, 0);

                int filterFlags = 0;
                if (filter.HB2)
                    filterFlags |= 0x01;
                if (filter.FIR2)
                    filterFlags |= 0x02;
                if (filter.HB1)
                    filterFlags |= 0x04;
                if (filter.FIR1)
                    filterFlags |= 0x08;

                success &= this.Device.AD6636WriteReg(120, 1, filterFlags);
                success &= this.Device.AD6636WriteReg(121, 1, filter.CIC5Scale);
                success &= this.Device.AD6636WriteReg(122, 1, filter.CIC5Decimation - 1);

                if (filter.CIC5)
                    success &= this.Device.AD6636WriteReg(123, 1, 0);
                else
                    success &= this.Device.AD6636WriteReg(123, 1, 1);

                success &= this.Device.AD6636WriteReg(136, 1, filter.DRCFNTaps - 1);
                success &= this.Device.AD6636WriteReg(137, 1, (long)(64 - filter.DRCFNTaps / 2));
                success &= this.Device.AD6636WriteReg(138, 2, 0x700 | ((filter.DRCFDecimation - 1) << 4));

                success &= this.Device.AD6636WriteReg(148, 1, filter.CRCFNTaps - 1);
                success &= this.Device.AD6636WriteReg(149, 1, (long)(64 - filter.CRCFNTaps / 2));
                success &= this.Device.AD6636WriteReg(150, 2, 0x700 | ((filter.CRCFDecimation - 1) << 4));

                success &= this.Device.AD6636WriteReg(184, 2, 0);

                int DRCFEntries = (int)((filter.DRCFNTaps + 1) / 2);
                success &= this.Device.AD6636WriteReg(140, 1, 0);
                success &= this.Device.AD6636WriteReg(141, 1, DRCFEntries - 1);
                for (int pos = 0; pos < DRCFEntries; pos++)
                    success &= this.Device.AD6636WriteReg(144, 2, filter.DRCFTaps[DRCFEntries - 1 + pos]);

                int CRCFEntries = (int)((filter.CRCFNTaps + 1) / 2);
                success &= this.Device.AD6636WriteReg(152, 1, 0);
                success &= this.Device.AD6636WriteReg(153, 1, CRCFEntries - 1);

                for (int pos = 0; pos < CRCFEntries; pos++)
                    success &= this.Device.AD6636WriteReg(156, 3, filter.CRCFTaps[CRCFEntries - 1 + pos]);

                success &= this.Device.AD6636WriteReg(184, 2, 0);

                if (FilterRate > NCOFreq / 4)
                {
                    success &= SetDivisor(1);
                }
                else if (FilterRate > NCOFreq / 8)
                {
                    success &= SetDivisor(2);
                }
                else if (FilterRate > NCOFreq / 16)
                {
                    success &= SetDivisor(4);
                }
                else
                {
                    success &= SetDivisor(8);
                }

                success &= SelectChannel(channel);
                success &= SoftSync();

                /* inform listeners */
                if (SamplingRateChanged != null)
                    SamplingRateChanged(this, null);
                if (FilterWidthChanged != null)
                    FilterWidthChanged(this, null);
            }
            return success;
        }

        private bool SetDivisor(int div)
        {
            bool success = true;
            lock (this)
            {

                /* read from cache */
                long regValue = this.Device.AD6636ReadReg(192, 2, true);
                long bitVal = 0;

                switch (div)
                {
                    case 1:
                        bitVal = 0;
                        break;
                    case 2:
                        bitVal = 1;
                        break;
                    case 4:
                        bitVal = 2;
                        break;
                    case 8:
                        bitVal = 3;
                        break;
                }

                regValue &= 0x00FF;
                regValue |= bitVal << 8;

                success &= this.Device.AD6636WriteReg(192, 2, regValue);
            }
            return success;
        }

        public bool SoftSync()
        {
            return SoftSync(0x0F);
        }

        public bool SoftSync(int channelMask)
        {
            bool success = true;

            lock (this)
            {
                success &= this.Device.AD6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0x00);
                success &= this.Device.AD6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0xC0 | channelMask);
                success &= this.Device.AD6636WriteReg(AD6636_REG_SOFTSYNC, AD6636_REG_SOFTSYNC_L, 0xC0 | channelMask);
            }

            return success;
        }

        public long Offset
        {
            set
            {
                bool success = true;
                long val = Math.Min(255, Math.Max(0, value));

                lock (this)
                {
                    success &= this.Device.AD6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 0x0F);

                    success &= this.Device.AD6636WriteReg(0x1C, 2, val);
                    success &= this.Device.AD6636WriteReg(0x1E, 2, val);

                    success &= SoftSync(0x0F);
                }

                return;
            }
        }

        public long Gain
        {
            set
            {
                bool success = true;
                long val = Math.Min(32768, Math.Max(0, value));

                lock (this)
                {
                    success &= this.Device.AD6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 0x04);

                    success &= this.Device.AD6636WriteReg(0xA4, 2, val);

                    success &= SoftSync(0x04);
                }

                return;
            }
        }

        public void StopTransfer()
        {
            //this.Device.AD6636WriteReg(3, 1, 0);
        }

        public void StartTransfer()
        {
            //this.Device.AD6636WriteReg(3, 1, (1 << CurrentChannel));
        }


        #region Tuner Members

        public event EventHandler FrequencyChanged;
        public event EventHandler FilterWidthChanged;
        public event EventHandler InvertedSpectrumChanged;
        public event EventHandler DeviceDisappeared;
        public event EventHandler DeviceClosed;

        public bool OpenTuner()
        {
            return true;
        }

        public void CloseTuner()
        {
        }

        public long IntermediateFrequency
        {
            get { return 0; }
        }


        private double _Amplification = 0;
        public double Amplification
        {
            get { return _Amplification; }
            set
            {
                if (Math.Abs(value - Amplification) > 5)
                {
                    SetMgcValue((int)value);
                }
            }
        }

        public double Attenuation
        {
            get { return 0; }
        }

        public long LowestFrequency
        {
            get { return 0; }
        }

        public long HighestFrequency
        {
            get { return NCOFreq / 2; }
        }

        public long UpperFilterMargin
        {
            get { return Math.Min(CurrentFrequency + FilterWidth / 2, HighestFrequency); }
        }

        public long LowerFilterMargin
        {
            get { return Math.Max(CurrentFrequency - FilterWidth / 2, LowestFrequency); }
        }

        public string UpperFilterMarginDescription
        {
            get
            {
                if (CurrentFrequency + FilterWidth / 2 > HighestFrequency)
                {
                    return "AD6636 max. freq";
                }
                else
                {
                    return "AD6636 filter width";
                }
            }
        }

        public string LowerFilterMarginDescription
        {
            get
            {
                if (CurrentFrequency - FilterWidth / 2 < LowestFrequency)
                {
                    return "AD6636 min. freq.";
                }
                else
                {
                    return "AD6636 filter width";
                }
            }
        }

        public string FilterWidthDescription
        {
            get
            {
                return "AD6636 filter width: " + FrequencyFormatter.FreqToString(FilterWidth);
            }
        }

        public string[] Name
        {
            get { return new[] { "Analog Devices AD6636" }; }
        }

        public string[] Description
        {
            get { return new[] { "150 MSPS Wideband (Digital) Receive Signal Processor" }; }
        }

        public string[] Details
        {
            get
            {
                return new[] 
                { 
                    "Clocked at: " + FrequencyFormatter.FreqToStringAccurate(NCOFreq),
                    "Selected Channel: " + CurrentChannel
                };
            }
        }

        public long GetFrequency()
        {
            return CurrentFrequency;
            /*
            long frequency = (long)((double)this.Device.AD6636ReadReg(AD6636_REG_NCOFREQ, AD6636_REG_NCOFREQ_L) / this.NCOMul);

            CurrentFrequency = frequency;

            return frequency;
             * */
        }

        public bool SetFrequency(long frequency)
        {
            bool success = true;

            if (CurrentFrequency == frequency)
            {
                return true;
            }

            if (LowestFrequency <= frequency && frequency <= HighestFrequency)
            {
                double regValue = frequency * this.NCOMul;
                lock (this)
                {
                    this.Device.AD6636WriteReg(AD6636_REG_IOAC, AD6636_REG_IOAC_L, 0x0F);
                    this.Device.AD6636WriteReg(AD6636_REG_NCOFREQ, AD6636_REG_NCOFREQ_L, (long)Math.Round(regValue));
                    SoftSync();
                    
                    CurrentFrequency = frequency;
                }
                success = true;
            }

            if (FrequencyChanged != null)
                FrequencyChanged(this, null);

            return success;
        }

        public bool InvertedSpectrum
        {
            get
            {
                return false;
            }
        }

        public long FilterWidth
        {
            get
            {
                return _FilterWidth;
            }
        }

        #endregion


        #region DigitalTuner Members

        public event EventHandler SamplingRateChanged;


        public long SamplingRate
        {
            get
            {
                return FilterRate;
            }
        }

        #endregion

    }
}
