using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SampleSources
{
    public class SampleSource
    {
        protected Oversampler Oversampler;
        public GMSKDemodulator Demodulator;

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
            TmpSignal = new double[BlockSize];
            TmpStrength = new double[BlockSize];
            Signal = new double[BlockSize * InternalOversampling];
            Strength = new double[BlockSize * InternalOversampling];

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
