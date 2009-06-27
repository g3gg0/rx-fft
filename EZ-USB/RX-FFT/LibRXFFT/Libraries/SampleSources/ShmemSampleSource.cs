using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SampleSources
{
    public class ShmemSampleSource : SampleSource
    {
        private SharedMem ShmemChannel;

        private int BlockSize = 1024;
        private byte[] InBuffer;

        public ShmemSampleSource(string name, int oversampling)
            : this(name, oversampling, 2184533)
        {
        }

        public ShmemSampleSource(string name, int oversampling, double samplingRate) : base(oversampling)
        {
            ShmemChannel = new SharedMem(0, -1, name, 256 * 1024 * 1024);
            ShmemChannel.ReadTimeout = 10;
            ShmemChannel.ReadMode = eReadMode.TimeLimited;

            InvertedSpectrum = false;
            DataFormat = eDataFormat.Direct16BitIQFixedPoint;

            InBuffer = new byte[BlockSize * BytesPerSamplePair];

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
                SamplingRateChanged = true;
                InputSamplingRate = (ShmemChannel.Rate / 2);
            }

            int read = ShmemChannel.Read(InBuffer, 0, InBuffer.Length);

            if (read != InBuffer.Length)
            {
                SamplesRead = 0;
                return false;
            }

            DecodeFromBinary(InBuffer, SourceSamplesI, SourceSamplesQ);

            if (InternalOversampling > 1)
            {
                IOversampler.Oversample(SourceSamplesI, OversampledI);
                QOversampler.Oversample(SourceSamplesQ, OversampledQ);
                Demodulator.ProcessData(OversampledI, OversampledQ, Signal, Strength);
            }
            else
                Demodulator.ProcessData(SourceSamplesI, SourceSamplesQ, Signal, Strength);

            SamplesRead = Signal.Length;

            return true;
        }
    }
}
