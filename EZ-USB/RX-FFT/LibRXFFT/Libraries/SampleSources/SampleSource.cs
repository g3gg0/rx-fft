using System;
using System.IO;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries.ShmemChain;

namespace LibRXFFT.Libraries.SampleSources
{
    public class SampleSource
    {
        public string SourceName = "<unnamed>";
        public bool InvertedSpectrum = false;
        protected Oversampler IOversampler;
        protected Oversampler QOversampler;

        public SharedMem OutputShmemChannel;

        private WaveFileWriter SavingFile = null;
        private string _SavingFileName = "output.dat";
        private bool _SavingEnabled = false;
        private ByteUtil.eSampleFormat SavingDataType = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;
        private int SavingBytesPerSample = 2;

        public bool BufferOverrun = false;

        public static eOversamplingType DefaultOversamplingType = eOversamplingType.SinC;
        public eOversamplingType OversamplingType
        {
            get { return IOversampler.Type; }
            set { IOversampler.Type = value; }
        }

        public static int DefaultSinXDepth = 8;
        public int SinXDepth
        {
            get { return IOversampler.SinCDepth; }
            set { IOversampler.SinCDepth = value; }
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
        public object SampleBufferLock = new object();

        public int SamplesRead;
        protected int _SamplesPerBlock;
        public virtual int SamplesPerBlock
        {
            set
            {
                _SamplesPerBlock = value;
                AllocateBuffers();
            }
            get
            {
                return _SamplesPerBlock;
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
                if (value)
                {
                    lock (this)
                    {
                        SavingFile = new WaveFileWriter(SavingFileName);
                        SavingFile.SamplingRate = (int)OutputSamplingRate;
                        _SavingEnabled = true;
                    }
                }
                else
                {
                    lock (this)
                    {
                        _SavingEnabled = false;
                        if (SavingFile != null)
                        {
                            SavingFile.Close();
                            SavingFile = null;
                        }
                    }
                }
            }
        }

        public bool ForwardEnabled 
        {
            get
            {
                return OutputShmemChannel != null;
            }

            set
            {
                if (value == ForwardEnabled)
                {
                    return;
                }

                if (value)
                {
                    OutputShmemChannel = new SharedMem(-2, 0, SourceName);
                }
                else
                {
                    OutputShmemChannel.Unregister();
                    OutputShmemChannel.Close();
                    OutputShmemChannel = null;
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
            IOversampler.SinCDepth = DefaultSinXDepth;

            QOversampler = new Oversampler(InternalOversampling);
            QOversampler.Type = DefaultOversamplingType;
            QOversampler.SinCDepth = DefaultSinXDepth;

            SamplingRateHasChanged = true;
        }

        protected virtual void AllocateBuffers()
        {
            int samplesPerBlock = SamplesPerBlock;
            int oversampling = InternalOversampling;
            int savingBytes = SavingBytesPerSample;

            lock (SampleBufferLock)
            {
                SourceSamplesI = new double[samplesPerBlock * oversampling];
                SourceSamplesQ = new double[samplesPerBlock * oversampling];
                OversampleI = new double[samplesPerBlock];
                OversampleQ = new double[samplesPerBlock];

                BinarySaveData = new byte[samplesPerBlock * 2 * savingBytes];
            }
        }

        protected virtual void ForwardData(byte[] buffer)
        {
            if (ForwardEnabled)
            {
                OutputShmemChannel.Rate = (long)OutputSamplingRate * 2;

                /* convert to our standard format */
                if (DataFormat != SavingDataType)
                {
                    ByteUtil.SamplesToBinary(BinarySaveData, SamplesRead, SourceSamplesI, SourceSamplesQ, SavingDataType, false);
                    OutputShmemChannel.Write(BinarySaveData);
                }
                else
                {
                    OutputShmemChannel.Write(buffer);
                }
            }
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
            /* this will unregister etc */
            ForwardEnabled = false;
        }
        
        public virtual bool Restart()
        {
            return true;
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
            lock (this)
            {
                if (!SavingEnabled)
                    return;
                ByteUtil.SamplesToBinary(BinarySaveData, SamplesRead, SourceSamplesI, SourceSamplesQ, SavingDataType, false);
                SavingFile.Write(BinarySaveData);
            }
        }
    }
}
