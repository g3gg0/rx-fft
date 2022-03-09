using System;
using System.Collections;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.USB_RX.Tuners
{
    public class TunerStack : DigitalTuner
    {
        private Tuner SlaveTuner;
        private Tuner MasterTuner;
        private long MasterTunerFreqSteps;
        private long CurrentFrequency;


        public TunerStack(Tuner masterTuner, DigitalTuner slaveTuner, long masterTunerFreqSteps)
            : this(masterTuner, (Tuner)slaveTuner, masterTunerFreqSteps)
        {
            /* this is a digital tuner, register for sampling rate change */
            slaveTuner.SamplingRateChanged += new EventHandler(Tuner_SamplingRateChanged);
        }


        public TunerStack(Tuner masterTuner, Tuner slaveTuner, long masterTunerFreqSteps)
        {
            this.SlaveTuner = slaveTuner;
            this.MasterTuner = masterTuner;
            this.MasterTunerFreqSteps = masterTunerFreqSteps;

            if (this.SlaveTuner.GetFrequency() == 0)
            {
                this.SlaveTuner.SetFrequency(this.MasterTuner.IntermediateFrequency);
            }

            /* register for any filter width etc change */
            SlaveTuner.InvertedSpectrumChanged += new EventHandler(Tuner_InvertedSpectrumChanged);
            SlaveTuner.FrequencyChanged += new EventHandler(Tuner_FrequencyChanged);
            SlaveTuner.FilterWidthChanged += new EventHandler(Tuner_FilterWidthChanged);
            SlaveTuner.DeviceDisappeared += new EventHandler(SlaveTuner_DeviceDisappeared);

            MasterTuner.FilterWidthChanged += new EventHandler(Tuner_FilterWidthChanged);
            MasterTuner.InvertedSpectrumChanged += new EventHandler(Tuner_InvertedSpectrumChanged);
            MasterTuner.FrequencyChanged += new EventHandler(Tuner_FrequencyChanged);
            MasterTuner.DeviceDisappeared += new EventHandler(MasterTuner_DeviceDisappeared);
        }

        void SlaveTuner_DeviceDisappeared(object sender, EventArgs e)
        {
            if (DeviceDisappeared != null)
            {
                DeviceDisappeared(SlaveTuner, null);
            }
        }

        void MasterTuner_DeviceDisappeared(object sender, EventArgs e)
        {
            if (DeviceDisappeared != null)
            {
                DeviceDisappeared(MasterTuner, null);
            }
        }

        void Tuner_SamplingRateChanged(object sender, EventArgs e)
        {
            if (SamplingRateChanged != null)
                SamplingRateChanged(this, null);
        }

        void Tuner_FrequencyChanged(object sender, EventArgs e)
        {
            if (FrequencyChanged != null)
                FrequencyChanged(this, null);
        }

        void Tuner_InvertedSpectrumChanged(object sender, EventArgs e)
        {
            if (InvertedSpectrumChanged != null)
                InvertedSpectrumChanged(this, null);
        }

        void Tuner_FilterWidthChanged(object sender, EventArgs e)
        {
            if (FilterWidthChanged != null)
                FilterWidthChanged(this, null);
        }


        #region DigitalTuner Member

        public event EventHandler SamplingRateChanged;
        

        public long SamplingRate
        {
            get
            {
                if (SlaveTuner.GetType().IsAssignableFrom(typeof(DigitalTuner)))
                    throw new NotSupportedException();

                return ((DigitalTuner)SlaveTuner).SamplingRate;
            }
        }

        #endregion

        #region Tuner Members

        public event EventHandler FrequencyChanged;
        public event EventHandler InvertedSpectrumChanged;
        public event EventHandler FilterWidthChanged;
        public event EventHandler DeviceDisappeared;
        public event EventHandler DeviceClosed;
        public event EventHandler DeviceOpened;

        public bool OpenTuner()
        {
            MasterTuner.OpenTuner();
            SlaveTuner.OpenTuner();
            return true;
        }
        public void CloseTuner()
        {
            MasterTuner.CloseTuner();
            SlaveTuner.CloseTuner();
        }

        public long IntermediateFrequency
        {
            get { return MasterTuner.IntermediateFrequency; }
        }

        public double Amplification
        {
            get { return MasterTuner.Amplification + SlaveTuner.Amplification; }
            set
            {
                double val = value;

                /* try to set master tuners amplification */
                MasterTuner.Amplification = val;

                /* leave the rest up to the slave tuner */
                val -= MasterTuner.Amplification;
                SlaveTuner.Amplification = val;
            }
        }

        public double Attenuation
        {
            get { return MasterTuner.Attenuation + SlaveTuner.Attenuation; }
        }

        public long LowestFrequency
        {
            get { return MasterTuner.LowestFrequency; }
        }

        public long HighestFrequency
        {
            get { return MasterTuner.HighestFrequency; }
        }

        public long UpperFilterMargin
        {
            get { return Math.Min(CurrentFrequency + FilterWidth / 2, HighestFrequency); }
        }

        public long LowerFilterMargin
        {
            get { return Math.Max(CurrentFrequency - FilterWidth / 2, LowestFrequency); }
        }

        public long FilterWidth
        {
            get
            {
                /* return the most narrow filter */
                return Math.Min(SlaveTuner.FilterWidth, MasterTuner.FilterWidth);
            }
        }

        public string UpperFilterMarginDescription
        {
            get
            {
                if ((SlaveTuner.UpperFilterMargin - SlaveTuner.GetFrequency()) < (MasterTuner.UpperFilterMargin - MasterTuner.GetFrequency()))
                {
                    return SlaveTuner.UpperFilterMarginDescription;
                }
                else
                {
                    return MasterTuner.UpperFilterMarginDescription;
                }
            }
        }

        public string LowerFilterMarginDescription
        {
            get
            {
                if ((SlaveTuner.GetFrequency() - SlaveTuner.LowerFilterMargin) < (MasterTuner.GetFrequency() - MasterTuner.LowerFilterMargin))
                {
                    return SlaveTuner.LowerFilterMarginDescription;
                }
                else
                {
                    return MasterTuner.LowerFilterMarginDescription;
                }
            }
        }

        public string FilterWidthDescription
        {
            get
            {
                if (SlaveTuner.FilterWidth < MasterTuner.FilterWidth)
                {
                    return SlaveTuner.FilterWidthDescription;
                }

                return MasterTuner.FilterWidthDescription;
            }
        }

        public string[] Name
        {
            get
            {
                ArrayList lines = new ArrayList();

                lines.Add("Combination of two tuners.");
                lines.Add("Master:");
                foreach (string line in MasterTuner.Name)
                {
                    lines.Add("    " + line);
                }
                lines.Add("Slave:");
                foreach (string line in SlaveTuner.Name)
                {
                    lines.Add("    " + line);
                }

                return (string[])lines.ToArray(typeof(string));
            }
        }

        public string[] Description
        {
            get
            {
                ArrayList lines = new ArrayList();

                lines.Add("Master: " + (MasterTuner.InvertedSpectrum ? "(inverted)" : "(non inverted)"));
                foreach (string line in MasterTuner.Description)
                {
                    lines.Add("    " + line);
                }
                lines.Add("Slave:" + (SlaveTuner.InvertedSpectrum ? "(inverted)" : "(non inverted)"));
                foreach (string line in SlaveTuner.Description)
                {
                    lines.Add("    " + line);
                }

                return (string[])lines.ToArray(typeof(string));
            }
        }

        public string[] Details
        {
            get
            {
                ArrayList lines = new ArrayList();

                lines.Add("Master:");
                foreach (string line in MasterTuner.Details)
                {
                    lines.Add("    " + line);
                }
                lines.Add("Slave:");
                foreach (string line in SlaveTuner.Details)
                {
                    lines.Add("    " + line);
                }

                return (string[])lines.ToArray(typeof(string));
            }
        }

        public long GetFrequency()
        {
            long freqSlave = this.SlaveTuner.GetFrequency();
            long freqMaster = this.MasterTuner.GetFrequency();

            long frequency = freqMaster;

            long delta = freqSlave - MasterTuner.IntermediateFrequency;

            if (MasterTuner.InvertedSpectrum)
                frequency += delta;
            else
                frequency -= delta;

            CurrentFrequency = frequency;

            return frequency;
        }

        public bool SetFrequency(long frequency)
        {
            long freqMaster = frequency;
            long freqSlave = MasterTuner.IntermediateFrequency;

            if (MasterTunerFreqSteps > 0)
            {
                freqMaster = (frequency / MasterTunerFreqSteps) * MasterTunerFreqSteps;
            }

            long delta = frequency - freqMaster;

            if (MasterTuner.InvertedSpectrum)
                freqSlave -= delta;
            else
                freqSlave += delta;

            Log.AddMessage("-> " + frequency + "  = M: " + freqMaster + " S: " + (freqSlave + MasterTuner.IntermediateFrequency));

            if (!SlaveTuner.SetFrequency(freqSlave))
                return false;
            if (!MasterTuner.SetFrequency(freqMaster))
                return false;

            CurrentFrequency = frequency;

            return true;
        }

        public bool InvertedSpectrum
        {
            get
            {
                return MasterTuner.InvertedSpectrum != SlaveTuner.InvertedSpectrum;
            }
        }

        #endregion

    }
}
