using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace LibRXFFT.Libraries.SampleSources
{
    public enum eFileType
    {
        Unknown,
        WAV,
        RawIQ,
        CFile,
        Rtsa,
        CSV
    }
    public enum eSampleType
    {
        Complex,
        Simple
    }

    public class CsvFileReader : WaveFileReader
    {
        private TextReader Reader;
        private long _Length;
        private long _Position;
        public eSampleType SampleType = eSampleType.Complex;
        public decimal TimeBase = 0.001m;

        public CsvFileReader(string fileName) : base(fileName)
        {
            Reader = File.OpenText(fileName);

            _Length = 0;
            string line = "";
            DataStartPos = 0;

            decimal firstTime = 0;
            decimal lastTime = 0;
            do
            {
                line = Reader.ReadLine();

                if (line != null)
                {
                    var entries = line.Split(',');
                    decimal val1 = 0;
                    decimal val2 = 0;

                    if(entries.Length == 2)
                    {
                        switch(entries[0].Trim(new[] { ' ', '(', ')' }))
                        {
                            case "Zeit":
                            case "Time":
                                SampleType = eSampleType.Simple;
                                break;
                            case "ms":
                            case "msec":
                                TimeBase = 1000;
                                SampleType = eSampleType.Simple;
                                break;
                            case "s":
                            case "sec":
                                TimeBase = 1;
                                SampleType = eSampleType.Simple;
                                break;
                        }
                    }

                    if (decimal.TryParse(entries[0], NumberStyles.Float, CultureInfo.InvariantCulture, out val1) && decimal.TryParse(entries[1], NumberStyles.Float, CultureInfo.InvariantCulture, out val2))
                    {
                        if(_Length == 0)
                        {
                            firstTime = val1;
                        }
                        lastTime = val1;
                        _Length++;
                    }
                    else if (_Length == 0)
                    {
                        DataStartPos++;
                    }
                }
            } while (line != null);

            Rewind();

            decimal div = (lastTime - firstTime);

            if (div > 0)
            {
                SamplingRate = (int)(1.0m / div * TimeBase * _Length);
            }
        }

        private void Rewind()
        {
            (Reader as StreamReader).BaseStream.Position = 0;
            (Reader as StreamReader).DiscardBufferedData();

            int pos = 0;
            do
            {
                string line = Reader.ReadLine();

            } while (++pos < DataStartPos);

            _Position = 0;
        }

        public override long Seek(int pos, SeekOrigin seekOrigin)
        {
            Rewind();
            while(pos-- > 0)
            {
                string line = Reader.ReadLine();
                _Position++;
            }
            return 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            while (count >= 8)
            {
                string line = Reader.ReadLine();

                if (line != null)
                {
                    var entries = line.Split(',');
                    float val1 = 0;
                    float val2 = 0;

                    if (float.TryParse(entries[0], NumberStyles.Float, CultureInfo.InvariantCulture, out val1) && float.TryParse(entries[1], NumberStyles.Float, CultureInfo.InvariantCulture, out val2))
                    {
                        switch(SampleType)
                        {
                            case eSampleType.Complex:
                                Array.Copy(BitConverter.GetBytes(val1), 0, buffer, offset, 4);
                                Array.Copy(BitConverter.GetBytes(val2), 0, buffer, offset + 4, 4);
                                break;
                            case eSampleType.Simple:
                                Array.Copy(BitConverter.GetBytes(val2), 0, buffer, offset, 4);
                                break;
                        }

                        offset += 8;
                        read += 8;
                        count -= 8;
                    }
                }
                else
                {
                    break;
                }
            }

            return read;
        }

        public override long Position
        {
            get
            {
                return _Position * 8;
            }
        }
        public override long Length
        {
            get
            {
                return _Length * 8;
            }
        }
    }

    public class WaveFileReader
    {
        private BinaryReader Reader;
        private WaveHeader WavHeader;
        private eFileType Type;
        protected long DataStartPos;

        public FileStream InputStream { get; set; }
        public int SamplingRate { get; set; }

        public WaveFileReader(string fileName) : this(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }
        public WaveFileReader()
        { 
        }

        public WaveFileReader(string fileName, eFileType type) : this(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

        public WaveFileReader(FileStream stream) : this(stream, EstimateType(stream.Name)) { }

        public WaveFileReader(FileStream stream, eFileType type)
        {
            InputStream = stream;
            Type = type;

            switch (Type)
            {
                case eFileType.WAV:
                    // Open the destination .wav file
                    Reader = new BinaryReader(InputStream);

                    WavHeader = new WaveHeader();
                    WavHeader.Read(Reader);

                    DataStartPos = InputStream.Seek(0, SeekOrigin.Current);
                    SamplingRate = WavHeader.fmt.sampleRate;
                    break;

                case eFileType.RawIQ:
                case eFileType.CFile:
                    Reader = new BinaryReader(InputStream);
                    break;
            }
        }

        public static eFileType EstimateType(string name)
        {
            if (name.EndsWith(".cfile"))
            {
                return eFileType.CFile;
            }
            if (name.EndsWith(".wav") || name.EndsWith(".riff"))
            {
                return eFileType.WAV;
            }
            if (name.EndsWith(".csv"))
            {
                return eFileType.CSV;
            }
            if (name.EndsWith(".rtsa"))
            {
                return eFileType.Rtsa;
            }

            return eFileType.RawIQ;
        }

        public virtual long Length
        {
            get
            {
                return InputStream.Length - DataStartPos;
            }
        }

        public virtual long Position
        {
            get
            {
                return InputStream.Position - DataStartPos;
            }
        }

        public virtual long Seek(int pos, SeekOrigin seekOrigin)
        {
            return InputStream.Seek(DataStartPos + pos, seekOrigin) - DataStartPos;
        }

        public virtual void Close()
        {
            InputStream.Close();
        }

        public virtual int Read(byte[] buffer, int offset, int count)
        {
            return InputStream.Read(buffer, offset, count);
        }
    }

    public class WaveFileWriter
    {
        private BinaryWriter Writer;
        private int DataLength;
        private WaveHeader WavHeader;
        private eFileType Type;
        private bool HeaderWritten = false;
        private bool TrailerWritten = false;
        private byte[] BinarySaveData = new byte[1];
        private int BytesPerSamplePair;
        private int BytesPerSample;
        private ByteUtil.eSampleFormat _DataFormat;

        public int SamplingRate { get; set; }

        public ByteUtil.eSampleFormat DataFormat
        {
            get { return _DataFormat; }
            set
            {
                _DataFormat = value;

                BytesPerSamplePair = ByteUtil.GetBytePerSamplePair(value);
                BytesPerSample = ByteUtil.GetBytePerSample(value);
            }
        }
        public WaveFileWriter(string fileName) : this(File.Open(fileName, FileMode.Create, FileAccess.Write)) { }

        public WaveFileWriter(string fileName, eFileType type) : this(File.Open(fileName, FileMode.Create, FileAccess.Write), type) { }

        public WaveFileWriter(FileStream stream) : this(stream, EstimateType(stream.Name)) { }

        public WaveFileWriter(FileStream stream, eFileType type)
        {
            Type = type;
            switch (Type)
            {
                case eFileType.WAV:
                    // Fill in the appropriate members of the header
                    WavHeader = new WaveHeader();
                    WavHeader.sound.dataSize = int.MaxValue;
                    WavHeader.chunkSize = WavHeader.formatTag.Length + WavHeader.fmt.formatTag.Length +
                    WavHeader.fmt.formatSize + sizeof(int) + WavHeader.sound.dataTag.Length +
                    WavHeader.sound.dataSize + sizeof(int);

                    // Open the destination .wav file
                    Writer = new BinaryWriter(stream);

                    // Write out the header
                    WavHeader.Write(Writer);
                    DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPointLE;
                    break;

                case eFileType.RawIQ:
                    Writer = new BinaryWriter(stream);
                    DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPointLE;
                    break;

                case eFileType.CFile:
                    Writer = new BinaryWriter(stream);
                    DataFormat = ByteUtil.eSampleFormat.Direct32BitIQFloat64k;
                    break;
            }
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

        public void Write(byte[] data)
        {
            if (!HeaderWritten)
            {
                switch (Type)
                {
                    case eFileType.WAV:
                        // Fill in the appropriate members of the header
                        WavHeader = new WaveHeader();
                        WavHeader.sound.dataSize = int.MaxValue;
                        WavHeader.chunkSize = WavHeader.formatTag.Length + WavHeader.fmt.formatTag.Length +
                        WavHeader.fmt.formatSize + sizeof(int) + WavHeader.sound.dataTag.Length +
                        WavHeader.sound.dataSize + sizeof(int);

                        // Write out the header
                        WavHeader.Write(Writer);
                        break;

                    case eFileType.RawIQ:
                    case eFileType.CFile:
                        break;
                }

                HeaderWritten = true;
            }

            DataLength += data.Length;
            Writer.Write(data);
        }

        public void Close()
        {
            if (!TrailerWritten)
            {
                switch (Type)
                {
                    case eFileType.WAV:
                        /* update fields */
                        WavHeader.sound.dataSize = DataLength;
                        WavHeader.fmt.sampleRate = SamplingRate;
                        WavHeader.fmt.byteRate = WavHeader.fmt.bytesPerSample * WavHeader.fmt.sampleRate;

                        WavHeader.chunkSize = WavHeader.formatTag.Length + WavHeader.fmt.formatTag.Length +
                        WavHeader.fmt.formatSize + sizeof(int) + WavHeader.sound.dataTag.Length +
                        WavHeader.sound.dataSize + sizeof(int);

                        Writer.Seek(0, SeekOrigin.Begin);

                        WavHeader.Write(Writer);
                        break;

                    case eFileType.RawIQ:
                    case eFileType.CFile:
                        break;
                }

                TrailerWritten = true;
            }

            Writer.Close();
        }

        internal void Write(int samples, double[] samplesI, double[] samplesQ)
        {
            Write(samples, samplesI, samplesQ, false);
        }

        internal void Write(int samples, double[] samplesI, double[] samplesQ, bool inverted)
        {
            if (BinarySaveData.Length != samples * BytesPerSamplePair)
            {
                Array.Resize<byte>(ref BinarySaveData, samples * BytesPerSamplePair);
            }

            ByteUtil.SamplesToBinary(BinarySaveData, samples, samplesI, samplesQ, DataFormat, inverted);
            Write(BinarySaveData);
        }
    }

    public class FormatChunk
    {
        public string formatTag;    //"fmt "
        public int formatSize;      //size of chunk following this value.
        public short audioFormat;   //two bytes; value 1 for PCM.
        public short nChannels;     //two bytes.
        public int sampleRate;      //44100
        public int byteRate;        //sampleRate*channels*bitsPerSample/8
        public short bytesPerSample;//channels*bitsPerSample/8 - two bytes
        public short bitsPerSample; //two bytes.

        public FormatChunk()
        {
            formatTag = "fmt ";
            formatSize = 16;        // PCM default
            audioFormat = 1;        // PCM default
            nChannels = 2;            // Stereo default; 1 = mono
            sampleRate = 0xDEAD;        // Default
            bitsPerSample = 16;        // Default
            bytesPerSample = (short)(nChannels * bitsPerSample / 8);
            byteRate = bytesPerSample * sampleRate;
        }

        internal void Write(BinaryWriter bw)
        {
            bw.Write(formatTag.ToCharArray());
            bw.Write(formatSize);
            bw.Write(audioFormat);
            bw.Write(nChannels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write(bytesPerSample);
            bw.Write(bitsPerSample);
        }

        internal void Read(BinaryReader br)
        {
            formatTag = br.ReadChars(4).ToString();
            formatSize = br.ReadInt32();
            audioFormat = br.ReadInt16();
            nChannels = br.ReadInt16();
            sampleRate = br.ReadInt32();
            byteRate = br.ReadInt32();
            bytesPerSample = br.ReadInt16();
            bitsPerSample = br.ReadInt16();
        }
    }

    public class DataChunk
    {
        public string dataTag;
        public int dataSize;

        public DataChunk()
        {
            dataTag = "data";
        }

        internal void Write(BinaryWriter bw)
        {
            bw.Write(dataTag.ToCharArray());
            bw.Write(dataSize);
        }

        internal void Read(BinaryReader br)
        {
            dataTag = br.ReadChars(4).ToString();
            dataSize = br.ReadInt32();
        }
    }

    public class WaveHeader
    {
        public string chunkID;
        public int chunkSize;    // Size of file following this value
        public string formatTag;// Specific format
        public FormatChunk fmt;
        public DataChunk sound;
        public WaveHeader()
        {
            chunkID = "RIFF";
            formatTag = "WAVE";
            fmt = new FormatChunk();
            sound = new DataChunk();
        }

        internal void Write(BinaryWriter bw)
        {
            bw.Write(chunkID.ToCharArray());
            bw.Write(chunkSize);
            bw.Write(formatTag.ToCharArray());
            fmt.Write(bw);
            sound.Write(bw);
        }

        internal void Read(BinaryReader br)
        {
            try
            {
                chunkID = br.ReadChars(4).ToString();
                chunkSize = br.ReadInt32();
                formatTag = br.ReadChars(4).ToString();
                fmt.Read(br);
                sound.Read(br);
            }
            catch (Exception)
            {
                throw new InvalidDataException("The file specified is not a WAV file");
            }
        }
    }
}
