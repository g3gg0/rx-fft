using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SampleSources
{

    public class SampleSource
    {
        public bool InvertedSpectrum = false;
        protected Oversampler IOversampler;
        protected Oversampler QOversampler;

        public static eOversamplingType DefaultOversamplingType = eOversamplingType.SinX;
        public eOversamplingType OversamplingType
        {
            get { return IOversampler.Type; }
            set { IOversampler.Type = value; }
        }

        public static int DefaultSinXDepth = 8;
        public int SinXDepth
        {
            get { return IOversampler.SinXDepth; }
            set { IOversampler.SinXDepth = value; }
        }


        public int BytesPerSamplePair;
        public int BytesPerSample;
        private ByteUtil.eSampleFormat _DataFormat;

        public ByteUtil.eSampleFormat DataFormat
        {
            get { return _DataFormat; }
            set
            {
                _DataFormat = value;

                switch (value)
                {
                    case ByteUtil.eSampleFormat.Direct16BitIQFixedPoint:
                        BytesPerSamplePair = 4;
                        BytesPerSample = 2;
                        break;

                    case ByteUtil.eSampleFormat.Direct64BitIQFloat:
                    case ByteUtil.eSampleFormat.Direct64BitIQFloat64k:
                        BytesPerSamplePair = 8;
                        BytesPerSample = 4;
                        break;

                    default:
                        BytesPerSamplePair = 0;
                        BytesPerSample = 0;
                        break;
                }
            }
        }
        public readonly int InternalOversampling;
        public double[] SourceSamplesI;
        public double[] SourceSamplesQ;
        public double[] OversampleI;
        public double[] OversampleQ;
        public int SamplesRead;
        public virtual int SamplesPerBlock
        {
            set
            {
                AllocateBuffers();
            }
            get
            {
                return 1024;
            }
        }

        public int OutputBlockSize
        {
            get
            {
                return SamplesPerBlock * InternalOversampling;
            }
        }

        public double InputSamplingRate;
        public double OutputSamplingRate
        {
            get
            {
                return InputSamplingRate * InternalOversampling;
            }
        }

        public bool SamplingRateChanged;
        public event EventHandler SamplingRateChangedEvent;

        protected SampleSource(int oversampling)
        {
            DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;
            SamplesPerBlock = 1024;

            InternalOversampling = oversampling;

            IOversampler = new Oversampler(InternalOversampling);
            IOversampler.Type = DefaultOversamplingType;
            IOversampler.SinXDepth = DefaultSinXDepth;

            QOversampler = new Oversampler(InternalOversampling);
            QOversampler.Type = DefaultOversamplingType;
            QOversampler.SinXDepth = DefaultSinXDepth;

            SamplingRateChanged = true;
        }

        protected void AllocateBuffers()
        {
            SourceSamplesI = new double[SamplesPerBlock * InternalOversampling];
            SourceSamplesQ = new double[SamplesPerBlock * InternalOversampling];
            OversampleI = new double[SamplesPerBlock];
            OversampleQ = new double[SamplesPerBlock];
        }

        public virtual bool Read()
        {
            return false;
        }

        public virtual void Close()
        {
        }

        public virtual void ForceInputRate(double rate)
        {
            SamplingRateChanged = true;
            InputSamplingRate = rate;
            if (SamplingRateChangedEvent != null)
                SamplingRateChangedEvent(this, null);
        }
    }
}
