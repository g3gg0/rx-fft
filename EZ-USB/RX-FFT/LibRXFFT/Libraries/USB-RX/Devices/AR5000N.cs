using System;
using System.Threading;
using LibRXFFT.Libraries.Misc;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public class AR5000N : NetworkSerialTuner
    {
        public static bool DeviceTypeDisabled = false;

        public AR5000N(string host)
            : base(host)
        {
        }

        public override bool ConnectionCheck()
        {
            if (DeviceTypeDisabled)
                return false;

            int tries = 5;
            double avgDelay = 0;


            Client.GetStream().ReadTimeout = 200;
            /* send some data to make it power on */
            Send("");
            /* receive or wait for timeout */
            Receive(true);

            Client.GetStream().ReadTimeout = 800;

            for (int num = 0; num < tries; num++)
            {
                Send("");
                if (Receive() != "?")
                {
                    return false;
                }
                avgDelay += TransmitDuration;
            }

            string ver = SystemVersion;
            if (ver.Substring(0, 4) != "VER-")
            {
                if (!AutoDetectRunning)
                {
                    Log.AddMessage("No AR5000N. Reason: Invalid System Version '" + ver + "'");
                }
                return false;
            }

            Log.AddMessage("Connected to AR5000");
            Log.AddMessage("Average ping delay: " + FrequencyFormatter.TimeToString(avgDelay / tries));

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
                SendCommand("ACF", true); /* AR-5000: AGC control fast */
                SendCommand("AI1", true); /* AR-5000: EXIT-IF 1 ON */
                SendCommand("AN1", true); /* AR-5000: ANTENNA 2 */
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public int SelectedAntenna
        {
            get
            {
                Send("AN");
                string ant = Receive();

                if (!ant.StartsWith("AN"))
                {
                    return 0;
                }
                return int.Parse(ant.Substring(2).Trim());
            }
            set
            {
                if (value >= 1 && value <= 4)
                {
                    SendCommand("AN" + value);
                }
            }
        }

        public override void CloseTuner()
        {
            Power = false;

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

        public double PingDelay
        {
            get
            {
                int tries = 100;
                double avgDelay = 0;

                /* Datasheet says, send empty command, read empty answer several times */
                for (int num = 0; num < tries; num++)
                {
                    Send("VR");
                    if (!Receive().StartsWith("VER"))
                    {
                        return 0;
                    }
                    avgDelay += TransmitDuration;
                }

                return avgDelay / tries;
            }
        }

#region Communication abstraction

        public bool Power
        {
            get
            {
                return true;
            }

            set
            {
                if (!value)
                {
                    Send("QP");
                }
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
                Send("RX");

                string freq = Receive();
                string[] tokens = freq.Split(' ');

                foreach (string token in tokens)
                {
                    if (token.StartsWith("RF"))
                    {
                        return long.Parse(token.Replace("RF", ""));
                    }
                }

                return 0;
            }

            set
            {
                string rf = "RF" + value.ToString("0000000000");

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
                return "AR5000 " + FrequencyFormatter.FreqToString(FilterWidth) + " hard limit";
            }
        }
        public override bool InvertedSpectrum { get { return GetFrequency() < 2010000000; } }

        public override string[] Name
        {
            get { return new[] { "Boger AR5000" }; }
        }
        public override string[] Description
        {
            get { return new[] { "AR5000 10kHz-3.5GHz Receiver" }; }
        }
        public override string[] Details
        {
            get { return new[] { "Version: " + SystemVersion, "Ping delay: " + PingDelay }; }
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
                    return "AR5000 max. freq";
                }
                else
                {
                    return "AR5000 filter width";
                }
            }
        }

        public override string LowerFilterMarginDescription
        {
            get
            {
                if (CurrentFrequency - FilterWidth / 2 < LowestFrequency)
                {
                    return "AR5000 min. freq.";
                }
                else
                {
                    return "AR5000 filter width";
                }
            }
        }
        public override double Amplification { get { return 0; } }
        public override double Attenuation { get { return 40; } }

#endregion
    }
}
