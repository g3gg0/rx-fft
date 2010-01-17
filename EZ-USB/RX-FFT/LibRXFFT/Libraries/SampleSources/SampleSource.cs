using System;
using System.IO;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SampleSources
{

    public class SampleSource
    {
        public bool InvertedSpectrum = false;
        protected Oversampler IOversampler;
        protected Oversampler QOversampler;

        private BinaryWriter SavingFile = null;
        private string _SavingFileName = "output.dat";
        private bool _SavingEnabled = false;
        private ByteUtil.eSampleFormat SavingDataType = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;
        private int SavingBytesPerSample = 2;


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
        public byte[] BinarySaveData;

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


        public string SavingFileName
        {
            get
            {
                return _SavingFileName;
            }
            set
            {
                _SavingFileName = value;
            }
        }

        public bool SavingEnabled
        {
            get
            {
                return _SavingEnabled;
            }
            set
            {
                _SavingEnabled = value;

                if (SavingEnabled)
                {
                    SavingFile = new BinaryWriter(File.Create(SavingFileName));
                }
                else
                {
                    if (SavingFile != null)
                    {
                        SavingFile.Close();
                        SavingFile = null;
                    }
                }
            }
        }

        public bool SamplingRateHasChanged;
        public event EventHandler SamplingRateChanged;

        protected SampleSource(int oversampling)
        {
            DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;

            InternalOversampling = oversampling;
            SamplesPerBlock = 1024;

            IOversampler = new Oversampler(InternalOversampling);
            IOversampler.Type = DefaultOversamplingType;
            IOversampler.SinXDepth = DefaultSinXDepth;

            QOversampler = new Oversampler(InternalOversampling);
            QOversampler.Type = DefaultOversamplingType;
            QOversampler.SinXDepth = DefaultSinXDepth;

            SamplingRateHasChanged = true;
        }

        protected void AllocateBuffers()
        {
            SourceSamplesI = new double[SamplesPerBlock * InternalOversampling];
            SourceSamplesQ = new double[SamplesPerBlock * InternalOversampling];
            OversampleI = new double[SamplesPerBlock];
            OversampleQ = new double[SamplesPerBlock];

            BinarySaveData = new byte[SamplesPerBlock * 2 * SavingBytesPerSample];
        }

        public virtual void Flush()
        {
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
            SamplingRateHasChanged = true;
            InputSamplingRate = rate;
            if (SamplingRateChanged != null)
                SamplingRateChanged(this, null);
        }

        internal void SaveData()
        {
            if (!SavingEnabled)
                return;
            ByteUtil.SamplesToBinary(BinarySaveData, SamplesRead, SourceSamplesI, SourceSamplesQ, SavingDataType, false);
            SavingFile.Write(BinarySaveData);
        }

    }
}
