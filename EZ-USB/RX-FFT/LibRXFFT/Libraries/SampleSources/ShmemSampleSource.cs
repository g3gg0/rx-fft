using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.SignalProcessing;
using System;

namespace LibRXFFT.Libraries.SampleSources
{
    public class ShmemSampleSource : SampleSource
    {
        private SharedMem ShmemChannel;

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
            : base(oversampling)
        {
            ShmemChannel = new SharedMem(0, -1, name, 256 * 1024 * 1024);
            ShmemChannel.ReadTimeout = 100;
            ShmemChannel.ReadMode = eReadMode.TimeLimited;

            InvertedSpectrum = false;
            DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;

            SamplesPerBlock = 1024;

            if (samplingRate > 0)
                InputSamplingRate = samplingRate;
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

            return true;
        }
    }
}
