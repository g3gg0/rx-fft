using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SampleSources
{

    public class SampleSource
    {
        protected bool InvertedSpectrum = false;
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
        public int BlockSize = 1024;
        public int OutputBlockSize
        {
            get
            {
                return BlockSize * InternalOversampling;
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

        protected SampleSource(int oversampling)
        {
            InternalOversampling = oversampling;

            IOversampler = new Oversampler(InternalOversampling);
            IOversampler.Type = DefaultOversamplingType;
            IOversampler.SinXDepth = DefaultSinXDepth;

            QOversampler = new Oversampler(InternalOversampling);
            QOversampler.Type = DefaultOversamplingType;
            QOversampler.SinXDepth = DefaultSinXDepth;

            SourceSamplesI = new double[BlockSize * InternalOversampling];
            SourceSamplesQ = new double[BlockSize * InternalOversampling];
            OversampleI = new double[BlockSize];
            OversampleQ = new double[BlockSize];

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
