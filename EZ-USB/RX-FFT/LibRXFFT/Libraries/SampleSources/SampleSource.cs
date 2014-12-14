using System;
using System.IO;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries.ShmemChain;

namespace LibRXFFT.Libraries.SampleSources
{
    public class SampleSource
    {
        public ByteUtil.eSampleFormat ForwardFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPointLE;

        public string SourceName = "<unnamed>";
        public bool InvertedSpectrum = false;
        protected Oversampler IOversampler;
        protected Oversampler QOversampler;

        public SharedMem OutputShmemChannel;

        private WaveFileWriter SavingFile = null;
        private string _SavingFileName = "output.dat";
        private bool _SavingEnabled = false;
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
                BytesPerSamplePair = ByteUtil.GetBytePerSamplePair(value);
                BytesPerSample = ByteUtil.GetBytePerSample(value);
            }
        }
        public readonly int InternalOversampling;
        public double[] SourceSamplesI;
        public double[] SourceSamplesQ;
        public double[] OversampleI;
        public double[] OversampleQ;
        public byte[] ForwardDataBuffer;
        public object SampleBufferLock = new object();
        public object SavingLock = new object();


        public virtual long SamplesAvailable
        {
            get { return 0; }
        }

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
        public bool SavingInvertedSpectrum = false;
        public bool SavingEnabled
        {
            get
            {
                return _SavingEnabled;
            }
            set
            {
                lock (SavingLock)
                {
                    if (value)
                    {
                        SavingFile = new WaveFileWriter(SavingFileName);
                        SavingFile.SamplingRate = (int)OutputSamplingRate;
                        _SavingEnabled = true;

                    }
                    else
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
            DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPointLE;

            InternalOversampling = oversampling;
            SamplesPerBlock = 32768;

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

            lock (SampleBufferLock)
            {
                SourceSamplesI = new double[samplesPerBlock * oversampling];
                SourceSamplesQ = new double[samplesPerBlock * oversampling];
                OversampleI = new double[samplesPerBlock];
                OversampleQ = new double[samplesPerBlock];

                ForwardDataBuffer = new byte[samplesPerBlock * oversampling * ByteUtil.GetBytePerSamplePair(ForwardFormat)];
            }
        }

        protected virtual void ForwardData(byte[] buffer)
        {
            if (ForwardEnabled)
            {
                OutputShmemChannel.Rate = (long)OutputSamplingRate * 2;

                /* convert to our standard format */
                if (DataFormat != ForwardFormat)
                {
                    ByteUtil.SamplesToBinary(ForwardDataBuffer, SamplesRead, SourceSamplesI, SourceSamplesQ, ForwardFormat, false);
                    OutputShmemChannel.Write(ForwardDataBuffer);
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
                lock (SavingLock)
                {
                    if (!SavingEnabled)
                        return;

                    SavingFile.Write(SamplesRead, SourceSamplesI, SourceSamplesQ, SavingInvertedSpectrum);
                }
            }
        }
    }
}
