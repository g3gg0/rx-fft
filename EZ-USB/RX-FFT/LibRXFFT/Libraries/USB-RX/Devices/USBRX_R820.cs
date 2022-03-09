

using System;
using System.Threading;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.USB_RX.Interfaces;
using LibRXFFT.Libraries.USB_RX.Tuners;
using SlimDX.Direct3D9;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class USBRX_R820 : Tuner
    {
        private USBRXDevice USBRX;

        private enum eTunerCommand
        {
            Init = 0x01,
            SetFrequency = 0x46,

            SetIFGain = 0x47,
            SetLNAGain = 0x48,
            SetMixGain = 0x4E,

            SetLNAGainAuto = 0x4C, /* 0x10: manual, 0x00: auto */
            SetMixGainAuto = 0x4D, /* 0x00: manual, 0x10: auto */

            ReadReg = 0x52
        };


        public long CurrentFrequency = 466230000;
        public long IFFrequency = 5000000;
        private double _Amplification = 0;
        internal long IFStepSize = 1000000;


        public USBRX_R820(USBRXDevice device)
        {
            USBRX = device;
        }

        #region Tuner Members

        public event EventHandler FrequencyChanged;
        public event EventHandler InvertedSpectrumChanged;
        public event EventHandler DeviceDisappeared;
        public event EventHandler FilterWidthChanged;
        public event EventHandler DeviceClosed;
        public event EventHandler DeviceOpened;

        public bool OpenTuner()
        {
            if (!USBRX.Atmel.SerialNumber.Contains("R820"))
            {
                return false;
            }

            Init();
            
            byte idCode = 0;
            ReadRegister(0, out idCode);
            if (idCode != 0x96)
            {
                return false;
            }

            SetFrequency(CurrentFrequency);

            return true;
        }

        public void CloseTuner()
        {
        }


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
            get { return 30000000; }
        }

        public long HighestFrequency
        {
            get { return 1650000000; }
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
                return true;
            }
        }

        public long FilterWidth
        {
            get
            {
                return 5000000;
            }
        }

        public string UpperFilterMarginDescription
        {
            get
            {
                if (CurrentFrequency + FilterWidth / 2 > HighestFrequency)
                {
                    return "R820 max. freq";
                }
                else
                {
                    return "R820 filter width";
                }
            }
        }

        public string LowerFilterMarginDescription
        {
            get
            {
                if (CurrentFrequency - FilterWidth / 2 < LowestFrequency)
                {
                    return "R820 min. freq.";
                }
                else
                {
                    return "R820 filter width";
                }
            }
        }

        public string FilterWidthDescription
        {
            get
            {
                return "MT2131 " + FrequencyFormatter.FreqToString(FilterWidth) + " hard limit";
            }
        }

        public string[] Name
        {
            get { return new[] { "R820" }; }
        }

        public string[] Description
        {
            get { return new[] { "Single-chip terrestrial tuner" }; }
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
            CurrentFrequency = frequency;
            USBRX.Atmel.TunerFrequency = frequency;
            /*
            RX_FFT.Components.GDI.Log.AddMessage("R820 Registers:");
            for (int reg = 0; reg < 16; reg++)
            {
                byte value = 0;
                ReadRegister(reg, out value);
                RX_FFT.Components.GDI.Log.AddMessage(" reg[" + reg + "]: 0x" + value.ToString("X2"));
            }
            */
            
            /*

            byte[] cmd = new byte[5];
            cmd[0] = (byte)eTunerCommand.SetFrequency;
            cmd[1] = (byte)((frequency) & 0xFF);
            cmd[2] = (byte)((frequency >> 8) & 0xFF);
            cmd[3] = (byte)((frequency >> 16) & 0xFF);
            cmd[4] = (byte)((frequency >> 24) & 0xFF);

            if (!USBRX.Atmel.TunerCommand(cmd, null))
            {
                return false;
            }
            */
            return true;
        }

        #endregion


        private bool Init()
        {
            if (!USBRX.Atmel.EnableTuner())
            {
                return false;
            }
            /*
            {
                byte[] cmd = new byte[1];
                cmd[0] = (byte)eTunerCommand.Init;

                if (!USBRX.Atmel.TunerCommand(cmd, null))
                {
                    return false;
                }
            }
            */
            SetGain(eGainType.IFGain, 15);
            SetGain(eGainType.LNAGain, true);
            SetGain(eGainType.MixGain, true);


            return true;
        }

        public enum eGainType
        {
            LNAGain,
            MixGain,
            IFGain
        }

        public bool SetGain(eGainType type, bool enabled)
        {
            return SetGain(type, -1, enabled);
        }

        public bool SetGain(eGainType type, int gain, bool auto = false)
        {
            byte[] cmdEnabled = new byte[2];
            byte[] cmdGain = new byte[2];

            cmdEnabled[0] = 0;
            cmdGain[1] = (byte)Math.Min(15, Math.Max(0, gain));

            switch (type)
            {
                case eGainType.IFGain:
                    cmdGain[0] = (byte)eTunerCommand.SetIFGain;
                    break;

                case eGainType.MixGain:
                    cmdGain[0] = (byte)eTunerCommand.SetMixGain;
                    cmdEnabled[0] = (byte)eTunerCommand.SetMixGainAuto;
                    cmdEnabled[1] = (byte)(auto ? 0x10 : 0x00);
                    break;

                case eGainType.LNAGain:
                    cmdGain[0] = (byte)eTunerCommand.SetLNAGain;
                    cmdEnabled[0] = (byte)eTunerCommand.SetLNAGainAuto;
                    cmdEnabled[1] = (byte)(auto ? 0x00 : 0x10);
                    break;
            }

            if (cmdEnabled[0] != 0)
            {
                if (!USBRX.Atmel.TunerCommand(cmdEnabled, null))
                {
                    return false;
                }
                Thread.Sleep(1);
            }

            if (gain >= 0)
            {
                if (!USBRX.Atmel.TunerCommand(cmdGain, null))
                {
                    return false;
                }
            }
            Thread.Sleep(1);

            return true;
        }

        public bool ReadRegister(int reg, out byte value)
        {
            byte[] cmd = new byte[2];
            byte[] ret = new byte[1];

            value = 0;

            cmd[0] = (byte)eTunerCommand.ReadReg;
            cmd[1] = (byte)reg;

            if (!USBRX.Atmel.TunerCommand(cmd, ret))
            {
                return false;
            }

            value = ret[0];
            return true;
        }

    }
}

