using System.IO;
using GSM_Analyzer;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SampleSources
{
    public class USRPSampleSource : SampleSource
    {
        private SharedMem ShmemChannel;

        private byte[] InBuffer;
        private FileStream InputStream;

        public USRPSampleSource(string fileName, int oversampling) : base(oversampling)
        {
            /*
            ShmemChannel = new SharedMem(0, -1, "grrr");
            ShmemChannel.ReadTimeout = 10;
            ShmemChannel.ReadMode = eReadMode.TimeLimited;
             * */

            Oversampler = new Oversampler();
            Oversampler.Type = eOversamplingType.SinX;

            /* USRP has an inverted spectrum */
            Demodulator = new GMSKDemodulator();
            Demodulator.DataFormat = eDataFormat.Direct64BitIQFloat64k;
            Demodulator.InvertedSpectrum = true;

            InBuffer = new byte[BlockSize * Demodulator.BytesPerSamplePair];


            InputStream = new FileStream(fileName, FileMode.Open);

            CFileDecimationDialog dec = new CFileDecimationDialog();

            dec.ShowDialog();

            if (dec.Decimation < 1)
                return;

            /* calculate sampling rate from USRPs decimation rate */
            InputSamplingRate = 64000000f / dec.Decimation;
            SamplingRateChanged = true;
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
