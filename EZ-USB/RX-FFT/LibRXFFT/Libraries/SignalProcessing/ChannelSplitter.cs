using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.Filters;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class ChannelSplitter
    {
        /* configuration per channel */
        public class ChannelConfig
        {
            /* a reference provided by the class that uses the splitter */
            public object Reference;

            /* offset relative to BaseFrequency */
            public double FrequencyOffset;
            /* the width of the channel. the splitter will try to apply the next wider filter */
            public double ChannelWidth;

            /* TODO: after filter was applied, decimate input signal by this factor */
            public int Decimation;

            /* SetupConfig() and ProcessData() set the reference to the buffers */
            public double[] SampleBufferI = null;
            public double[] SampleBufferQ = null;
        }

        /* global configuration */
        public class SplitterConfig
        {
            public double BaseFrequency;
            public double SamplingRate;
            public ChannelConfig[] Channels = new ChannelConfig[0];
        }

        private SplitterConfig _Config = null;
        private Downmixer[] Downmixers = null;
        private Filter[] LowPassFiltersI = null;
        private Filter[] LowPassFiltersQ = null;
        private double[][] SampleBufferI = null;
        private double[][] SampleBufferQ = null;


        public ChannelSplitter() : this(new SplitterConfig())
        {
        }

        public ChannelSplitter(SplitterConfig config)
        {
            Config = config;
        }

        public SplitterConfig Config
        {
            get { return _Config; }
            set
            {
                lock (this)
                {
                    _Config = value;
                    UpdateConfig();
                }
            }
        }

        public double SamplingRate
        {
            get { return Config.SamplingRate; }
            set
            {
                lock (this)
                {
                    Config.SamplingRate = value;
                    UpdateFrequencies();
                }
            }
        }

        public double BaseFrequency
        {
            get { return Config.BaseFrequency; }
            set
            {
                lock (this)
                {
                    Config.BaseFrequency = value;
                    UpdateFrequencies();
                }
            }
        }

        private void UpdateFrequencies()
        {
            for (int num = 0; num < Config.Channels.Length; num++)
            {
                ChannelConfig chan = Config.Channels[num];
                double offset = Config.BaseFrequency - chan.FrequencyOffset;
                double relative = offset / Config.SamplingRate;

                if (Math.Abs(relative) <= 0.5f)
                {
                    Downmixers[num].SamplingRate = Config.SamplingRate;
                    Downmixers[num].TimeStep = relative * (2 * Math.PI);
                }
                else
                {
                    /* what do? */
                }
            }
        }

        public void UpdateConfig()
        {
            Downmixers = new Downmixer[Config.Channels.Length];
            LowPassFiltersI = new IIRFilter[Config.Channels.Length];
            LowPassFiltersQ = new IIRFilter[Config.Channels.Length];
            SampleBufferI = new double[Config.Channels.Length][];
            SampleBufferQ = new double[Config.Channels.Length][];

            for (int num = 0; num < Config.Channels.Length; num++)
            {
                ChannelConfig chan = Config.Channels[num];
                double filterDecim = chan.ChannelWidth / Config.SamplingRate;

                Downmixers[num] = new Downmixer();
                LowPassFiltersI[num] = IIRCoefficients.GetBestFitting(filterDecim);
                LowPassFiltersQ[num] = IIRCoefficients.GetBestFitting(filterDecim);
                SampleBufferI[num] = new double[0];
                SampleBufferQ[num] = new double[0];
                chan.SampleBufferI = SampleBufferI[num];
                chan.SampleBufferQ = SampleBufferQ[num];
            }

            UpdateFrequencies();
        }

        public virtual void ProcessData(double[] iDataIn, double[] qDataIn)
        {
            for (int num = 0; num < Config.Channels.Length; num++)
            {
                if (SampleBufferI[num].Length != iDataIn.Length || SampleBufferQ[num].Length != qDataIn.Length)
                {
                    Array.Resize<double>(ref SampleBufferI[num], iDataIn.Length);
                    Array.Resize<double>(ref SampleBufferQ[num], qDataIn.Length);

                    /* also update references in channel config */
                    ChannelConfig chan = Config.Channels[num];
                    chan.SampleBufferI = SampleBufferI[num];
                    chan.SampleBufferQ = SampleBufferQ[num];
                }

                /* translate input signal */
                Downmixers[num].ProcessData(iDataIn, qDataIn, SampleBufferI[num], SampleBufferQ[num]);

                /* now low pass filter translated data */
                LowPassFiltersI[num].Process(SampleBufferI[num], SampleBufferI[num]);
                LowPassFiltersQ[num].Process(SampleBufferQ[num], SampleBufferQ[num]);
            }
        }
    }
}
