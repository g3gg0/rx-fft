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

        public ShmemSampleSource(string name, int oversampling) : base(oversampling)
        {
            ShmemChannel = new SharedMem(0, -1, name);
            ShmemChannel.ReadTimeout = 10;
            ShmemChannel.ReadMode = eReadMode.TimeLimited;

            Demodulator.DataFormat = eDataFormat.Direct16BitIQFixedPoint;

            InBuffer = new byte[BlockSize * Demodulator.BytesPerSamplePair];

            InputSamplingRate = 2184533;
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

            if (InternalOversampling > 1)
            {
                Demodulator.ProcessData(InBuffer, read, TmpSignal, TmpStrength);
                Oversampler.Oversample(TmpSignal, Signal, InternalOversampling);
                Oversampler.Oversample(TmpStrength, Strength, InternalOversampling);
            }
            else
                Demodulator.ProcessData(InBuffer, read, Signal, Strength);

            //ShmemChannel.Write(ByteUtil.convertToBytesInterleaved(Signal, Strength));

            SamplesRead = Signal.Length;

            return true;
        }
    }
}
