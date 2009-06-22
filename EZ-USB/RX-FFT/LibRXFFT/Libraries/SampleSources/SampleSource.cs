using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SampleSources
{
    public class SampleSource
    {
        protected Oversampler Oversampler;
        public GMSKDemodulator Demodulator;

        public static eOversamplingType DefaultOversamplingType = eOversamplingType.SinX;
        public eOversamplingType OversamplingType
        {
            get { return Oversampler.Type; }
            set { Oversampler.Type = value; }
        }

        public static int DefaultSinXDepth = 4;
        public int SinXDepth
        {
            get { return Oversampler.SinXDepth; }
            set { Oversampler.SinXDepth = value; }
        }

        public readonly int InternalOversampling;
        public double[] TmpSignal;
        public double[] TmpStrength;
        public double[] Signal;
        public double[] Strength;
        public int SamplesRead;
        protected int BlockSize = 1024;

        public double InputSamplingRate;
        public double OutputSamplingRate
        {
            get
            {
                return InputSamplingRate * InternalOversampling;
            }
        }

        public bool SamplingRateChanged;

        protected SampleSource(int oversampling)
        {
            InternalOversampling = oversampling;

            Oversampler = new Oversampler();
            Oversampler.Type = DefaultOversamplingType;

            Demodulator = new GMSKDemodulator();

            TmpSignal = new double[BlockSize];
            TmpStrength = new double[BlockSize];

            Signal = new double[BlockSize * InternalOversampling];
            Strength = new double[BlockSize * InternalOversampling];

            SamplingRateChanged = true;
        }

        public virtual bool Read()
        {
            return false;
        }

        public virtual void Close()
        {
        }
    }
}
