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
            DataFormat = ByteUtil.eSampleFormat.Direct64BitIQFloat64k;

            InBuffer = new byte[SamplesPerBlock * BytesPerSamplePair];

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


            if (InternalOversampling > 1)
            {
                ByteUtil.SamplesFromBinary(InBuffer, OversampleI, OversampleQ, DataFormat, InvertedSpectrum);
                IOversampler.Oversample(OversampleI, SourceSamplesI);
                QOversampler.Oversample(OversampleQ, SourceSamplesQ);
            }
            else
            {
                ByteUtil.SamplesFromBinary(InBuffer, SourceSamplesI, SourceSamplesQ, DataFormat, InvertedSpectrum);
                //Demodulator.ProcessData(SourceSamplesI, SourceSamplesQ, Signal, Strength);
            }

            SamplesRead = SourceSamplesI.Length;

            return true;
        }
    }
}
