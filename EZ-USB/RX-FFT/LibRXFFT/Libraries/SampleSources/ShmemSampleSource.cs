using LibRXFFT.Libraries.ShmemChain;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.SampleSources
{
    public class ShmemSampleSource : SampleSource
    {
        public SharedMem ShmemChannel;
        private byte[] InBuffer;
        private byte[] NextInBuffer;
        private bool NextInBufferAvailable;
        private static double DefaultRate = 2184533;
        private static long DefaultBufferSize = 32 * 1024 * 1024;

        public bool TraceReads = false;

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
            : this(name, oversampling, DefaultRate) { }

        public ShmemSampleSource(string name, int oversampling, double samplingRate)
            : this(name, 0, oversampling, samplingRate) { }

        public ShmemSampleSource(string name, int srcChan, int oversampling, double samplingRate)
            : this(name, srcChan, oversampling, samplingRate, DefaultBufferSize) { }

        public ShmemSampleSource(string name, int srcChan, int oversampling, double samplingRate, long bufferSize)
            : base(oversampling)
        {
            ShmemChannel = new SharedMem(srcChan, -1, name, bufferSize);
            ShmemChannel.ReadTimeout = 100;
            ShmemChannel.ReadMode = eReadMode.TimeLimitedNoPartial;

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
            BufferOverrun = false;
            ShmemChannel.Flush();
        }

        public override void Close()
        {
            BufferOverrun = false;
            ShmemChannel.Unregister();
        }

        public override bool Read()
        {
            /* buffer overrun? */
            if (ShmemChannel.Length > ShmemChannel.bufferSize * 0.9f)
            {
                BufferOverrun = true;
            }

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

            if (TraceReads)
            {
                Log.AddMessage("ShmemSamplesource", "Read Block: " + read + "/" + InBuffer.Length + " (" + ShmemChannel.Length + " available)");
            }

            /* in case buffer size has changed meanwhile, return */
            if (NextInBufferAvailable)
            {
                SamplesRead = 0;
                return true;
            }

            /* seems we have timed out */
            if (read == 0)
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
