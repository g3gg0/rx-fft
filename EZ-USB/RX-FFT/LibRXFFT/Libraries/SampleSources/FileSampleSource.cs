using System.IO;
using LibRXFFT.Components.GDI;


namespace LibRXFFT.Libraries.SampleSources
{
    public class FileSampleSource : SampleSource
    {
        public enum eFileType
        {
            Unknown,
            WAV,
            RawIQ,
            CFile
        }

        private byte[] InBuffer;
        private FileStream InputStream;

        public FileSampleSource(string fileName) : this(fileName, 1) { }

        public FileSampleSource(string fileName, int oversampling) : base(oversampling)
        {
            SourceName = fileName;

            switch (EstimateType(fileName))
            {
                case eFileType.CFile:

                    /* USRP has an inverted spectrum */
                    InvertedSpectrum = true;

                    DataFormat = ByteUtil.eSampleFormat.Direct64BitIQFloat64k;

                    /* calculate sampling rate from USRPs decimation rate */
                    CFileDecimationDialog dec = new CFileDecimationDialog();
                    dec.EstimateDecimation(fileName);
                    dec.ShowDialog();

                    if (dec.Decimation < 1)
                    {
                        dec.Decimation = 1;
                    }

                    InputSamplingRate = 64000000d / dec.Decimation;
                    break;

                case eFileType.RawIQ:

                    InvertedSpectrum = false;

                    FileFormatDialog dlg = new FileFormatDialog();
                    dlg.SampleFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;
                    dlg.EstimateDetails(fileName);
                    dlg.ShowDialog();

                    InputSamplingRate = dlg.SamplingRate;
                    DataFormat = dlg.SampleFormat;

                    break;

                case eFileType.WAV:
                    break;

                case eFileType.Unknown:
                    break;
            }

            InBuffer = new byte[SamplesPerBlock * BytesPerSamplePair];
            InputStream = new FileStream(fileName, FileMode.Open);            
        }

        private static eFileType EstimateType(string name)
        {
            if (name.EndsWith(".cfile"))
            {
                return eFileType.CFile;
            }
            if (name.EndsWith(".wav") || name.EndsWith(".riff"))
            {
                return eFileType.WAV;
            }

            return eFileType.RawIQ;
        }

        public override void Close()
        {
            InputStream.Close();
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
            long offset = (long)(pos * InputStream.Length);

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
