using LibRXFFT.Libraries.ShmemChain;

namespace LibRXFFT.Libraries.SampleSources
{
    public class ShmemSampleSource : SampleSource
    {
        public SharedMem ShmemChannel;

        public override int SamplesPerBlock
        {
            set
            {
                InBuffer = new byte[value * BytesPerSamplePair];
                AllocateBuffers();
            }
            get
            {
                return InBuffer.Length / BytesPerSamplePair;
            }
        }

        private byte[] InBuffer;

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
            ShmemChannel = new SharedMem(srcChan, -1, name, 256 * 1024 * 1024);
            ShmemChannel.ReadTimeout = 100;
            ShmemChannel.ReadMode = eReadMode.TimeLimited;

            InvertedSpectrum = false;
            DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;

            SamplesPerBlock = 1024;

            if (samplingRate > 0)
                InputSamplingRate = samplingRate;
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
            /* when the rate has changed */
            if (ShmemChannel.Rate != 0 && InputSamplingRate != ShmemChannel.Rate / 2)
            {
                ForceInputRate(ShmemChannel.Rate / 2);
            }

            int read = ShmemChannel.Read(InBuffer, 0, InBuffer.Length);

            if (read != InBuffer.Length)
            {
                SamplesRead = 0;
                return false;
            }


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

            return true;
        }
    }
}
