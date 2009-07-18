using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SampleSources
{
    public enum eDataFormat
    {
        Direct16BitIQFixedPoint,
        Direct64BitIQFloat,
        Direct64BitIQFloat64k
    }

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
        private eDataFormat _DataFormat;

        public eDataFormat DataFormat
        {
            get { return _DataFormat; }
            set
            {
                _DataFormat = value;

                switch (value)
                {
                    case eDataFormat.Direct16BitIQFixedPoint:
                        BytesPerSamplePair = 4;
                        BytesPerSample = 2;
                        break;

                    case eDataFormat.Direct64BitIQFloat:
                    case eDataFormat.Direct64BitIQFloat64k:
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

        protected void DecodeFromBinary(byte[] dataBuffer, double[] samplesI, double[] samplesQ)
        {
            DecodeFromBinary(dataBuffer, dataBuffer.Length, samplesI, samplesQ);
        }

        protected void DecodeFromBinary(byte[] dataBuffer, int bytesRead, double[] samplesI, double[] samplesQ)
        {
            byte[] outBuffer = null;
            int outBufferPos = 0;
            int samplePos = 0;
            int samplePairs = bytesRead / BytesPerSamplePair;

            for (int pos = 0; pos < samplePairs; pos++)
            {
                double I;
                double Q;
                switch (DataFormat)
                {
                    case eDataFormat.Direct16BitIQFixedPoint:
                        I = ByteUtil.getDoubleFromBytes(dataBuffer, BytesPerSamplePair * pos);
                        Q = ByteUtil.getDoubleFromBytes(dataBuffer, BytesPerSamplePair * pos + BytesPerSample);
                        break;

                    case eDataFormat.Direct64BitIQFloat64k:
                        I = BitConverter.ToSingle(dataBuffer, BytesPerSamplePair * pos) / 65536;
                        Q = BitConverter.ToSingle(dataBuffer, BytesPerSamplePair * pos + BytesPerSample) / 65536;
                        break;

                    case eDataFormat.Direct64BitIQFloat:
                        I = BitConverter.ToSingle(dataBuffer, BytesPerSamplePair * pos);
                        Q = BitConverter.ToSingle(dataBuffer, BytesPerSamplePair * pos + BytesPerSample);
                        break;

                    default:
                        return;
                }

                if (InvertedSpectrum)
                    I = -I;

                samplesI[pos] = I;
                samplesQ[pos] = Q;
            }

            return;
        }
    }
}
