using LibRXFFT.Libraries.ShmemChain;

namespace LibRXFFT.Libraries.SampleSources
{
    public class ShmemSampleSource : SampleSource
    {
        public SharedMem ShmemChannel;
        private byte[] InBuffer;
        private byte[] NextInBuffer;
        private bool NextInBufferAvailable;

        public override int SamplesPerBlock
        {
            set
            {
                NextInBuffer = new byte[value * BytesPerSamplePair];
                NextInBufferAvailable = true;
                AllocateBuffers();
            }
            get
            {
                return NextInBuffer.Length / BytesPerSamplePair;
            }
        }

        

        public ShmemSampleSource(string name, int oversampling)
            : this(name, oversampling, 2184533)
        {
        }

        public ShmemSampleSource(string name, int oversampling, double samplingRate)
            : this(name, 0, oversampling, samplingRate)
        {
        }

        public ShmemSampleSource(string name, int srcChan, int oversampling, double samplingRate)
            : base(oversampling)
        {
            ShmemChannel = new SharedMem(srcChan, -1, name, 32 * 1024 * 1024);
            ShmemChannel.ReadTimeout = 100;
            ShmemChannel.ReadMode = eReadMode.TimeLimited;

            InvertedSpectrum = false;
            DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;

            SamplesPerBlock = 1024;

            if (samplingRate > 0)
            {
                InputSamplingRate = samplingRate;
            }
        }

        public override void Flush()
        {
            ShmemChannel.Flush();
        }

        public override void Close()
        {
            ShmemChannel.Unregister();
        }

        public override bool Read()
        {
            /* in case we should use some other input buffer */
            if (NextInBufferAvailable)
            {
                NextInBufferAvailable = false;
                InBuffer = NextInBuffer;
            }

            /* when the rate has changed */
            if (ShmemChannel.Rate != 0 && InputSamplingRate != ShmemChannel.Rate / 2)
            {
                ForceInputRate(ShmemChannel.Rate / 2);
            }

            int read = ShmemChannel.Read(InBuffer, 0, InBuffer.Length);

            /* in case buffer size has changed meanwhile, return */
            if (NextInBufferAvailable)
            {
                SamplesRead = 0;
                return true;
            }

            /* in case we could not read enough bytes, return */
            if (read != InBuffer.Length)
            {
                SamplesRead = 0;
                return false;
            }

            lock (SampleBufferLock)
            {
                if (InternalOversampling > 1)
                {
                    ByteUtil.SamplesFromBinary(InBuffer, OversampleI, OversampleQ, DataFormat, InvertedSpectrum);
                    IOversampler.Oversample(OversampleI, SourceSamplesI);
                    QOversampler.Oversample(OversampleQ, SourceSamplesQ);
                }
                else
                {
                    ByteUtil.SamplesFromBinary(InBuffer, SourceSamplesI, SourceSamplesQ, DataFormat, InvertedSpectrum);
                }

                SamplesRead = SourceSamplesI.Length;

                SaveData();
            }
            return true;
        }
    }
}
