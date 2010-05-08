using System;
using System.Threading;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using LibRXFFT.Libraries.USB_RX.Tuners;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class VUHF_RX : Tuner
    {
        public static bool DeviceTypeDisabled = false;

        private long IF1Frequency = 1574500000;
        private long IFFrequency = 21804000;
        public long IFStepSize = 1000000;

        private I2CInterface I2cDevice;
        private int BusID = 33;
        private long CurrentFrequency;
        private long Modulus;
        private long PFD;

        public VUHF_RX(I2CInterface device)
        {
            I2cDevice = device;
        }

        #region Tuner Members

        public event EventHandler FrequencyChanged;
        public event EventHandler InvertedSpectrumChanged;
        public event EventHandler DeviceDisappeared;
        public event EventHandler FilterWidthChanged;
        public event EventHandler DeviceClosed;

        public bool OpenTuner()
        {
            if (DeviceTypeDisabled)
                return false;

            byte[] buffer = new byte[32];
            if (!I2cDevice.I2CWriteByte(BusID, 211))
            {
                return false;
            }
            if (!I2cDevice.I2CReadBytes(BusID, buffer))
            {
                return false;
            }

            byte[] wordBuffer = new byte[4];

            /* read PFD - 4 byte LSB first */
            if (!I2cDevice.I2CWriteByte(BusID, 215))
            {
                return false;
            }
            if (!I2cDevice.I2CReadBytes(BusID, wordBuffer))
            {
                return false;
            }
            PFD = (wordBuffer[3] << 24) | (wordBuffer[2] << 16) | (wordBuffer[1] << 8) | wordBuffer[0];

            /* read Modulus - 1 byte */
            if (!I2cDevice.I2CWriteByte(BusID, 217))
            {
                return false;
            }
            if (!I2cDevice.I2CReadByte(BusID, wordBuffer))
            {
                return false;
            }
            Modulus = wordBuffer[0];

            /* enable LNA */
            if (!I2cDevice.I2CWriteByte(BusID, 229))
            {
                return false;
            }

            /* disable ATT */
            if (!I2cDevice.I2CWriteBytes(BusID, new byte[] { 231, 0 }))
            {
                return false;
            }

            if (PFD == 0)
            {
                PFD = 20000000;
            }

            if (Modulus == 0)
            {
                Modulus = 40;
            }

            long oldFreq = GetStartupFrequency();

            Init();
            SetFrequency(oldFreq);

            return true;
        }
        
        public void CloseTuner()
        {
            SetStartupFrequency(CurrentFrequency);
        }

        private long GetStartupFrequency()
        {
            byte[] wordBuffer = new byte[4];

            /* read PFD - 4 byte LSB first */
            if (!I2cDevice.I2CWriteByte(BusID, 0xD5))
            {
                return 0;
            }

            Thread.Sleep(20);

            if (!I2cDevice.I2CReadBytes(BusID, wordBuffer))
            {
                return 0;
            }

            return (wordBuffer[3] << 24) | (wordBuffer[2] << 16) | (wordBuffer[1] << 8) | wordBuffer[0];
        }

        private bool SetStartupFrequency(long frequency)
        {
            byte[] wordBuffer = new byte[5];

            wordBuffer[0] = 0xD6;
            wordBuffer[1] = (byte)((frequency >> 0) & 0xFF);
            wordBuffer[2] = (byte)((frequency >> 8) & 0xFF);
            wordBuffer[3] = (byte)((frequency >> 16) & 0xFF);
            wordBuffer[4] = (byte)((frequency >> 24) & 0xFF);

            /* write last frequency */
            return I2cDevice.I2CWriteBytes(BusID, wordBuffer);
        }

        private double _Amplification = 0;
        public double Amplification
        {
            get { return _Amplification; }
            set { }
        }

        public double Attenuation
        {
            get { return 0; }
        }

        public long IntermediateFrequency
        {
            get { return IFFrequency; }
        }

        public long LowestFrequency
        {
            get { return 0; }
        }

        public long HighestFrequency
        {
            get { return 1000000000; }
        }

        public long UpperFilterMargin
        {
            get { return Math.Min(CurrentFrequency + FilterWidth / 2, HighestFrequency); }
        }

        public long LowerFilterMargin
        {
            get { return Math.Max(CurrentFrequency - FilterWidth / 2, LowestFrequency); }
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
                return 16000000;
            }
        }

        public string UpperFilterMarginDescription
        {
            get
            {
                if (CurrentFrequency + FilterWidth / 2 > HighestFrequency)
                {
                    return "VHFRX2 max. freq";
                }
                else
                {
                    return "VHFRX2 filter width";
                }
            }
        }

        public string LowerFilterMarginDescription
        {
            get
            {
                if (CurrentFrequency - FilterWidth / 2 < LowestFrequency)
                {
                    return "VHFRX2 min. freq.";
                }
                else
                {
                    return "VHFRX2 filter width";
                }
            }
        }

        public string FilterWidthDescription
        {
            get
            {
                return "VHFRX2 " + FrequencyFormatter.FreqToString(FilterWidth) + " hard limit";
            }
        }

        public string[] Name
        {
            get { return new[] { "Graß VHFRX2" }; }
        }

        public string[] Description
        {
            get { return new[] { "Advanced analysis tuner" }; }
        }

        public string[] Details
        {
            get { return new[] { "(none)" }; }
        }

        public long GetFrequency()
        {
            return CurrentFrequency;
        }

        public bool SetFrequency(long frequency)
        {
            bool success = false;

            if (LowestFrequency <= frequency && frequency <= HighestFrequency)
            {
                byte[] writeBuffer = new byte[6];
                double VCOFrq = frequency + IF1Frequency;
                int rfDivider = 1;

                if (VCOFrq > 2150000000)
                {
                    writeBuffer[5] = 0x00;
                    rfDivider = 1;
                }
                else
                {
                    writeBuffer[5] = 0x01;
                    rfDivider = 2;
                }

                double NGesamt = (rfDivider * VCOFrq) / PFD;
                long Nint = (long)Math.Truncate(NGesamt);
                long Nfrac = (long)Math.Round((NGesamt - Nint) * Modulus);
                long H1 = (Nint << 15) | (Nfrac << 3);

                //Log.AddMessage("INT: " + Nint + " FRAC: " + Nfrac);

                writeBuffer[0] = 0xE9;
                writeBuffer[1] = (byte)((H1 >> 24) & 0xFF);
                writeBuffer[2] = (byte)((H1 >> 16) & 0xFF);
                writeBuffer[3] = (byte)((H1 >> 8) & 0xFF);
                writeBuffer[4] = (byte)((H1 >> 0) & 0xFF);

                success = I2cDevice.I2CWriteBytes(BusID, writeBuffer);

                if (success)
                {
                    CurrentFrequency = (long)((Nint + ((double)Nfrac / Modulus)) * ((double)PFD / rfDivider) - IF1Frequency);
                }
            }
            return success;
        }

        #endregion

        public void Init()
        {
            return;
        }
    }
}
