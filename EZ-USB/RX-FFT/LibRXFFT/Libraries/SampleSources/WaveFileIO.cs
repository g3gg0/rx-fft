using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LibRXFFT.Libraries.SampleSources
{
    public enum eFileType
    {
        Unknown,
        WAV,
        RawIQ,
        CFile
    }

    public class WaveFileReader
    {
        private BinaryReader Reader;
        private int DataLength;
        private WaveHeader WavHeader;
        private eFileType Type;
        private long DataStartPos;

        public FileStream InputStream { get; set; }
        public int SamplingRate { get; set; }

        public WaveFileReader(string fileName) : this(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

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

        public long Length
        {
            get
            {
                return InputStream.Length - DataStartPos;
            }
        }

        public long Position
        {
            get
            {
                return InputStream.Position - DataStartPos;
            }
        }

        public long Seek(int pos, SeekOrigin seekOrigin)
        {
            return InputStream.Seek(DataStartPos + pos, seekOrigin) - DataStartPos;
        }

        public void Close()
        {
            InputStream.Close();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return InputStream.Read(buffer, offset, count);
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
    }

    public class WaveFileWriter
    {
        private BinaryWriter Writer;
        private int DataLength;
        private WaveHeader WavHeader;
        private eFileType Type;
        private bool HeaderWritten = false;
        private bool TrailerWritten = false;

        public int SamplingRate { get; set; }

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
                    break;

                case eFileType.RawIQ:
                case eFileType.CFile:
                    Writer = new BinaryWriter(stream);
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
