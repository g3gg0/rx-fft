using System.IO;
using LibRXFFT.Components.GDI;
using System;

namespace LibRXFFT.Libraries.SampleSources
{
    public class FileSampleSource : SampleSource
    {
        private byte[] InBuffer;
        private WaveFileReader InputStream;

        public FileSampleSource(string fileName) : this(fileName, 1) { }

        public FileSampleSource(string fileName, int oversampling) : base(oversampling)
        {
            SourceName = fileName;

            switch (WaveFileReader.EstimateType(fileName))
            {
                case eFileType.CFile:
                    /* USRP has an inverted spectrum */
                    InvertedSpectrum = true;

                    DataFormat = ByteUtil.eSampleFormat.Direct32BitIQFloat64k;

                    /* calculate sampling rate from USRPs decimation rate */
                    CFileDecimationDialog dec = new CFileDecimationDialog();
                    dec.EstimateDecimation(fileName);
                    dec.ShowDialog();

                    if (dec.Decimation < 1)
                    {
                        dec.Decimation = 1;
                    }

                    InputSamplingRate = (double)dec.ClockRate / dec.Decimation;
                    InputStream = new WaveFileReader(fileName, eFileType.CFile);

                    break;

                case eFileType.RawIQ:
                    InvertedSpectrum = false;

                    DataFormatDialog dlg = new DataFormatDialog();
                    dlg.SampleFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPointLE;
                    dlg.EstimateDetails(fileName);
                    dlg.ShowDialog();

                    InputSamplingRate = dlg.SamplingRate;
                    DataFormat = dlg.SampleFormat;
                    InputStream = new WaveFileReader(fileName, eFileType.RawIQ);

                    break;

                case eFileType.Rtsa:
                    InvertedSpectrum = false;

                    InputStream = new RtsaFileReader(fileName);
                    InputSamplingRate = 0;// dlg.SamplingRate;
                    DataFormat = ByteUtil.eSampleFormat.Direct32BitIQFloat;

                    break;

                case eFileType.WAV:
                    InputStream = new WaveFileReader(fileName, eFileType.WAV);
                    InputSamplingRate = InputStream.SamplingRate;
                    DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPointLE;
                    break;

                case eFileType.CSV:
                    InputStream = new CsvFileReader(fileName);
                    InputSamplingRate = InputStream.SamplingRate;
                    DataFormat = ByteUtil.eSampleFormat.Direct32BitIQFloat;
                    break;

                case eFileType.Unknown:
                    return;
            }
            AllocateBuffers();
        }

        protected override void AllocateBuffers()
        {
            int size = SamplesPerBlock * BytesPerSamplePair;

            if (InputStream != null)
            {
                size = (int)Math.Min(size, InputStream.Length);
            }

            InBuffer = new byte[size];
            base.AllocateBuffers();
        }

        public override void Close()
        {
            InputStream.Close();
            base.Close();
        }

        public override bool Restart()
        {
            InputStream.Seek(0, SeekOrigin.Begin);

            return true;
        }

        public double GetTotalTime()
        {
            return (double)InputStream.Length / (BytesPerSamplePair * InputSamplingRate);
        }

        public double GetPosition()
        {
            return (double)InputStream.Position / (double)InputStream.Length;
        }

        public void Seek(double pos)
        {
            int offset = (int)(pos * InputStream.Length);

            InputStream.Seek(offset - (offset % BytesPerSamplePair), SeekOrigin.Begin);
        }

        public override bool Read()
        {
            lock (SampleBufferLock)
            {
                int read = InputStream.Read(InBuffer, 0, InBuffer.Length);

                if (read != InBuffer.Length)
                {
                    SamplesRead = 0;
                    return false;
                }

                ForwardData(InBuffer);

                if (InternalOversampling > 1)
                {
                    ByteUtil.SamplesFromBinary(InBuffer, OversampleI, OversampleQ, DataFormat, InvertedSpectrum);
                    IOversampler.Resample(OversampleI, ref SourceSamplesI);
                    QOversampler.Resample(OversampleQ, ref SourceSamplesQ);
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
