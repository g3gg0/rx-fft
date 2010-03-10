using System;
using System.Threading;
using LibRXFFT.Libraries.Misc;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class BO35 : SerialPortTuner
    {
        public static bool DeviceTypeDisabled = true;

        public BO35(bool autoDetect)
            : base(autoDetect)
        {
        }

        public override bool ConnectionCheck()
        {
            if (DeviceTypeDisabled)
                return false;

            int tries = 50;
            double avgDelay = 0;

            /* Datasheet says, send empty command, read empty answer several times */
            for (int num = 0; num < tries; num++)
            {
                Send("");
                if (Receive() != "")
                {
                    return false;
                }
                avgDelay += TransmitDuration;
            }

            try
            {
                if (!Power)
                {
                    Power = true;
                }

            }
            catch (ArgumentException e)
            {
                Log.AddMessage("No BO-35 on port " + Port.PortName + ". Reason: Could not read power state. '" + e + "'");
                return false;
            }

            string ver = SystemVersion;
            if (ver.Substring(0, 2) != "VR")
            {
                Log.AddMessage("No BO-35 on port " + Port.PortName + ". Reason: Invalid System Version '" + ver + "'");
                return false;
            }

            /*
            Log.AddMessage("Connected to BO-35");
            Log.AddMessage("Average ping delay: " + FrequencyFormatter.TimeToString(avgDelay / tries));
            */

            return true;
        }

        public override bool OpenTuner()
        {
            if (IsOpened)
                return true;

            if (!base.OpenTuner())
                return false;

            /*
            Log.AddMessage("Power:     " + (Power ? "ON" : "OFF"));
            Log.AddMessage("Version:   " + SystemVersion);
            Log.AddMessage("Frequency: " + FrequencyFormatter.FreqToStringAccurate(Frequency));
            */

            /* set some defaults */
            try
            {
                SendCommand("RF0", false);
                SendCommand("AC3", true); /* BO-35 */
                SendCommand("ACF", true); /* AR-5000 */
                SendCommand("AT0", false);
                SendCommand("AI1", true);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public override void CloseTuner()
        {
            SendCommand("PW0", true);
            base.CloseTuner();
        }

        public override void CheckReturncode()
        {
            string ret = Receive();

            switch (ret)
            {
                case "?0":
                    throw new ArgumentException("Receiver says: Illegal command ('" + ret + "') in reply to :'" + LastCommand + "'");
                    return;

                case "?1":
                    throw new ArgumentException("Receiver says: Illegal parameter ('" + ret + "') in reply to :'" + LastCommand + "'");
                    return;

                case "?2":
                    throw new ArgumentException("Receiver says: Not allowed right now ('" + ret + "') in reply to :'" + LastCommand + "'");
                    return;

                case "":
                    return;
            }
        }


        #region Communication abstraction

        public bool Power
        {
            get
            {
                Send("PW");
                string resp = Receive();

                switch (resp)
                {
                    case "PW0":
                        return false;
                        break;

                    case "PW1":
                        return true;
                        break;

                    default:
                        throw new ArgumentException("Unexpected answer: " + resp);
                        break;
                }
            }

            set
            {
                if (value)
                {
                    Send("PW1");
                }
                else
                {
                    Send("PW0");
                }
                CheckReturncode();
            }
        }

        public string SystemVersion
        {
            get
            {
                Send("VR");
                return Receive();
            }
        }

        public long Frequency
        {
            get
            {
                Send("RF");

                string freq = Receive();
                if (freq.StartsWith("RF"))
                {
                    return long.Parse(freq.Replace("RF", "").Replace(".", ""));
                }

                return 0;
            }

            set
            {
                string rf = "RF" + (value / 1000000) + "." + (value % 1000000).ToString("000000");

                Send(rf);
                CheckReturncode();
            }
        }

        #endregion

        #region Tuner implementation

        public override bool SetFrequency(long frequency)
        {
            if (frequency < LowestFrequency || frequency > HighestFrequency)
            {
                return false;
            }

            try
            {
                Frequency = frequency;
            }
            catch (ArgumentException e)
            {
                Log.AddMessage("Command failed: " + e.ToString());
                return false;
            }

            return base.SetFrequency(frequency);
        }

        public override long IntermediateFrequency
        {
            get { return 10700000; }
        }

        public override long FilterWidth { get { return 10000000; } }
        public override string FilterWidthDescription
        {
            get
            {
                return "BO-35 " + FrequencyFormatter.FreqToString(FilterWidth) + " hard limit";
            }
        }
        public override bool InvertedSpectrum { get { return GetFrequency() > 3000000000; } }

        public override string[] Name
        {
            get { return new[] { "Boger BO-35" }; }
        }
        public override string[] Description
        {
            get { return new[] { "BO-35 10kHz-3.5GHz Receiver" }; }
        }
        public override string[] Details
        {
            get { return new[] { "Version: " + SystemVersion }; }
        }

        public override long LowestFrequency { get { return 10000; } }
        public override long HighestFrequency { get { return 3500000000; } }

        public override long UpperFilterMargin
        {
            get { return Math.Min(CurrentFrequency + FilterWidth / 2, HighestFrequency); }
        }

        public override long LowerFilterMargin
        {
            get { return Math.Max(CurrentFrequency - FilterWidth / 2, LowestFrequency); }
        }

        public override string UpperFilterMarginDescription
        {
            get
            {
                if (CurrentFrequency + FilterWidth / 2 > HighestFrequency)
                {
                    return "BO-35 max. freq";
                }
                else
                {
                    return "BO-35 filter width";
                }
            }
        }

        public override string LowerFilterMarginDescription
        {
            get
            {
                if (CurrentFrequency - FilterWidth / 2 < LowestFrequency)
                {
                    return "BO-35 min. freq.";
                }
                else
                {
                    return "BO-35 filter width";
                }
            }
        }
        public override double Amplification { get { return 0; } }
        public override double Attenuation { get { return 40; } }

        #endregion
    }
}