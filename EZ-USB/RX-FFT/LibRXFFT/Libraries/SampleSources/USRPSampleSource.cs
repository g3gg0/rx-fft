using System.IO;
using GSM_Analyzer;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SampleSources
{
    public class USRPSampleSource : SampleSource
    {
        private byte[] InBuffer;
        private FileStream InputStream;

        public USRPSampleSource(string fileName, int oversampling) : base(oversampling)
        {
            /* USRP has an inverted spectrum */
            InvertedSpectrum = true;
            DataFormat = eDataFormat.Direct64BitIQFloat64k;

            InBuffer = new byte[BlockSize * BytesPerSamplePair];

            InputStream = new FileStream(fileName, FileMode.Open);

            /* calculate sampling rate from USRPs decimation rate */
            CFileDecimationDialog dec = new CFileDecimationDialog();
            dec.EstimateDecimation(fileName);
            dec.ShowDialog();

            if (dec.Decimation < 1)
                return;

            InputSamplingRate = 64000000d / dec.Decimation;
        }

        public override void Close()
        {
            InputStream.Close();
        }

        public override bool Read()
        {
            int read = InputStream.Read(InBuffer, 0, InBuffer.Length);

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
