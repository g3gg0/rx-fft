using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibRXFFT.Libraries.SampleSources
{
    public class RtsaFileReader : WaveFileReader
    {
        private BinaryReader Reader;
        private long _Length = 0;
        private long _Position = 0;
        private byte[] RemainingBuffer = new byte[0];
        private int RemainingAvailable = 0;
        private Dictionary<long, long> LogicalPositionMap = new Dictionary<long, long>();

        public RtsaFileReader(string fileName) : this(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

        public RtsaFileReader(string fileName, eFileType type) : this(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

        public RtsaFileReader(FileStream stream)
        {
            InputStream = stream;

            Reader = new BinaryReader(InputStream);

            while(true)
            {
                var block = ReadBlock(out byte[] payload);

                if(block == null)
                {
                    Reader.BaseStream.Position = 0;
                    break;
                }
                if (block is SstrBlock subStream)
                {
                    SamplingRate = (int)subStream.StepFreq;
                }
                else if(block is SampBlock samples)
                {
                    if (samples.Compression != 0)
                    {
                        throw new Exception("Compression not supported");
                    }
                    if (samples.PayloadType != eRtsaPayloadType.Iq)
                    {
                        throw new Exception(samples.PayloadType + " not supported");
                    }
                    LogicalPositionMap.Add(_Position, Reader.BaseStream.Position);

                    _Position += payload.Length;
                }
            }

            _Length = _Position;
            _Position = 0;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RtsaBlock 
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public char[] Type;
            public uint Length;
            public uint Flags;
            public ushort Version;
            public ushort HeaderSize; 
            
            public string TypeString { get => new string(Type); set => Type = value.ToCharArray(); }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DsfhBlock
        {
            public RtsaBlock Header;
            public double CreationTime;

            public override string ToString()
            {
                return Header.TypeString;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AntaBlock
        {
            public RtsaBlock Header;
            public ulong AntennaId;

            public override string ToString()
            {
                return Header.TypeString + " - ID: " + AntennaId;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StrmBlock 
        {
            public RtsaBlock Header;
            public ulong StreamId;
            public double StartTime;
            public double Unk1;
            public double StartTimeOffset;

            public override string ToString()
            {
                return Header.TypeString + " - ID: " + StreamId + 
                    " Start: " + UnixTimeStampToDateTime(StartTime) + 
                    " Unk: " + Unk1 + 
                    " Start: " + StartTimeOffset;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SstrBlock
        {
            public RtsaBlock Header;
            public ulong StreamId;
            public uint SubStreamId;
            public uint SubStreamOffset;
            public double RbwFreq;
            public double StartFreq;
            public double StepFreq;
            public double SpanFreq;
            public double MinValue;
            public double MaxValue;
            public double Unk4;
            public double Unk5;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x80)]
            public byte[] Unk6;
            public ulong AntennaId;
            public ulong Unk7;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public byte[] Unk8;
            public double Unk9;
            public double ReferenceLevel;

            public override string ToString()
            {
                return Header.TypeString + " - ID: " + StreamId +
                    " RBW: " + RbwFreq +
                    " Start: " + StartFreq +
                    " Step: " + StepFreq +
                    " Span: " + SpanFreq +
                    " Min: " + MinValue +
                    " Max: " + MaxValue +
                    " Antenna: " + AntennaId +
                    " Reference: " + ReferenceLevel;
            }
        }

        public enum eRtsaPayloadType : byte
        {
            Generic = 0x00,
            Audio = 0x01,
            Iq = 0x02,
            Spectra = 0x03,
            Detection = 0x04,
            Histogram = 0x05,
            Energy = 0x06,
            Vector3 = 0x07,
            Structured = 0x08,
            IqSlice = 0x09,
            Image = 0x0A,
            Decoded = 0x10,
        }

        public enum eRtsaDataType : byte
        {
            U8 = 0x00,
            U16 = 0x01,
            S16 = 0x02,
            U32 = 0x03,
            S32 = 0x04,
            F32 = 0x05,
            U8N = 0x06,
            U16N = 0x07,
            S16N = 0x08,
            U32N = 0x09,
            S32N = 0x0A,
            F32N = 0x0B
        }

        public enum eRtsaSampUnit : byte
        {
            Generic = 0x00,
            dBm = 0x01,
            Percentage,
            dBmHz,
            dBmM2,
            Index,
            Phase,
            Signed1,
            Unsigned1,
            Volt = 0x13,
        }

        [Flags]
        public enum eRtsaFlags : uint
        {
            StreamStart = 0x01,
            StreamEnd = 0x02,
            GS = 0x04,
            GE = 0x08,
            Overflow = 0x0200,
            C0 = 0x10000000,
            C1 = 0x20000000,
            C2 = 0x40000000,
            C3 = 0x80000000,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SampBlock 
        {
            public RtsaBlock Header;
            public ulong StreamId;
            public uint SubstreamId;
            public eRtsaDataType DataType; 
            public eRtsaSampUnit SampUnit;
            public eRtsaPayloadType PayloadType; 
            public char Compression;
            public double StartTime;
            public double EndTime;
            public eRtsaFlags Flags;
            public uint SampSize;
            public uint SampCount;
            public uint Samples;
            public uint Unk3;
            public uint Unk4;

            public override string ToString()
            {
                return Header.TypeString + 
                    " - ID: " + StreamId +
                    " Start: " + StartTime.ToString("0.000000") +
                    " End: " + EndTime.ToString("0.000000") +
                    " SampUnit: " + SampUnit +
                    " DataType: " + DataType +
                    " PayloadType: " + PayloadType +
                    " Compression: " + (int)Compression +
                    " Flags: " + Flags +
                    " SampSize: " + SampSize +
                    " SampCount: " + SampCount +
                    " Samples: " + Samples +
                    " Unk3: " + Unk3 +
                    " Unk4: " + Unk4;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DsftBlock
        {
            public RtsaBlock Header;
            public double CompletionTime;
            public ulong StreamOffset;
            public uint NumStreams;

            public override string ToString()
            {
                return Header.TypeString;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StrtBlock
        {
            public RtsaBlock Header;
            public ulong StreamOffset;
            public ulong SubStreamOffset;
            public ulong PreviewOffset;
            public ulong NumSamples;
            public ulong PayloadSize;
            public uint PreviewLevels;
            public uint NumPreviews;
            public uint NumPreviewSegments;
            public uint Unk10;
            public double StopTime;
            public ulong AntennaOffset;
            public ulong MetaDataOffset;

            public override string ToString()
            {
                return Header.TypeString;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SprvBlock
        {
            public RtsaBlock Header;
            public ulong Unk1;
            public ulong Unk2;
            public ulong Unk3;
            public ulong Unk4;
            public ulong Unk5;
            public ulong Unk6;
            public ulong Unk7;
            public ulong Unk8;
            public ulong Unk9;
            public ulong Unk10;
            public ulong Unk11;
            public ulong Unk12;
            public ulong Unk13;
            public ulong Unk14;
            public ulong Unk15;
            public ulong Unk16;
            public ulong Unk17;

            public override string ToString()
            {
                return Header.TypeString;
            }
        }

        byte[] ToBytes<T>(T block)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(block, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        T FromBytes<T>(byte[] arr)
        {
            T block;

            int size = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            block = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return block;
        }

        private object ReadBlock(out byte[] payload)
        {
            payload = new byte[0];
            byte[] hdrBytes = Reader.ReadBytes(16);
            object block = null;

            if(hdrBytes == null || hdrBytes.Length == 0)
            {
                return null;
            }

            Reader.BaseStream.Position -= 16;

            RtsaBlock hdr = FromBytes<RtsaBlock>(hdrBytes);

            switch(hdr.TypeString)
            {
                case "DSFH":
                    hdrBytes = Reader.ReadBytes(hdr.HeaderSize);
                    block = FromBytes<DsfhBlock>(hdrBytes);
                    break;

                case "STRM":
                    hdrBytes = Reader.ReadBytes(hdr.HeaderSize);
                    block = FromBytes<StrmBlock>(hdrBytes);
                    break;

                case "SSTR":
                    hdrBytes = Reader.ReadBytes(hdr.HeaderSize);
                    block = FromBytes<SstrBlock>(hdrBytes);
                    break;

                case "STRT":
                    hdrBytes = Reader.ReadBytes(hdr.HeaderSize);
                    block = FromBytes<StrtBlock>(hdrBytes);
                    break;

                case "DSFT":
                    hdrBytes = Reader.ReadBytes(hdr.HeaderSize);
                    block = FromBytes<DsftBlock>(hdrBytes);
                    break;

                case "SAMP":
                    hdrBytes = Reader.ReadBytes(hdr.HeaderSize);
                    block = FromBytes<SampBlock>(hdrBytes);
                    payload = Reader.ReadBytes((int)(hdr.Length - hdr.HeaderSize));
                    break;

                case "SPRV":
                    hdrBytes = Reader.ReadBytes(hdr.HeaderSize);
                    block = FromBytes<SprvBlock>(hdrBytes);
                    payload = Reader.ReadBytes((int)(hdr.Length - hdr.HeaderSize));
                    break;

                case "ANTA":
                    hdrBytes = Reader.ReadBytes(hdr.HeaderSize);
                    block = FromBytes<AntaBlock>(hdrBytes);
                    payload = Reader.ReadBytes((int)(hdr.Length - hdr.HeaderSize));
                    break;

                default:
                    Console.WriteLine("Skip block '" + hdr.TypeString + "'");
                    break;
            }

            return block;
        }

        private void Rewind()
        {
            _Position = 0;
            Reader.BaseStream.Position = 0;
        }

        public override long Seek(long pos, SeekOrigin seekOrigin)
        {
            long newPos = _Position;

            switch(seekOrigin)
            {
                case SeekOrigin.Begin:
                    newPos = pos;
                    break;
                case SeekOrigin.Current:
                    newPos += pos;
                    break;
                case SeekOrigin.End:
                    newPos = _Length - 1 - pos;
                    break;
            }

            if(newPos >= _Length)
            {
                newPos = _Length - 1;
            }
            if (newPos < 0)
            {
                newPos = 0;
            }

            var blocks = LogicalPositionMap.Where(p => p.Key <= newPos);

            if(blocks.Count() > 0)
            {
                var block = blocks.Last();

                _Position = block.Key;
                Reader.BaseStream.Position = block.Value;
            }

            return _Position;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;

            while (true)
            {
                if (RemainingAvailable > 0)
                {
                    int copyLen = Math.Min(RemainingAvailable, count);
                    Array.Copy(RemainingBuffer, 0, buffer, offset, copyLen);

                    /* move remaining data left */
                    Array.Copy(RemainingBuffer, copyLen, RemainingBuffer, 0, RemainingAvailable - copyLen);
                    RemainingAvailable -= copyLen;
                    read += copyLen;
                    offset += copyLen;
                    count -= copyLen;
                }

                if (count == 0)
                {
                    return read;
                }

                var block = ReadBlock(out byte[] payload);

                if (block == null)
                {
                    return read;
                }

                if (block is DsfhBlock blk1)
                {
                    Console.WriteLine(blk1.ToString());
                }
                else if (block is StrmBlock blk2)
                {
                    Console.WriteLine(blk2.ToString());
                }
                else if (block is SstrBlock blk3)
                {
                    Console.WriteLine(blk3.ToString());
                }
                else if (block is SampBlock blk4)
                {
                    Console.WriteLine(blk4.ToString());
                    if(blk4.Compression != 0)
                    {
                        throw new Exception("Compression not supported");
                    }
                    if(RemainingBuffer.Length < RemainingAvailable + payload.Length)
                    {
                        Array.Resize(ref RemainingBuffer, RemainingAvailable + payload.Length);
                    }
                    Array.Copy(payload, 0, RemainingBuffer, RemainingAvailable, payload.Length);

                    RemainingAvailable += payload.Length;
                    _Position += payload.Length;
                }
                else if (block is SprvBlock blk5)
                {
                    Console.WriteLine(blk5.ToString());
                }
                else if (block is StrtBlock blk6)
                {
                    Console.WriteLine(blk6.ToString());
                }
                else if (block is DsftBlock blk7)
                {
                    Console.WriteLine(blk7.ToString());
                }
                else if (block is AntaBlock blk8)
                {
                    Console.WriteLine(blk8.ToString());
                }
                else
                {
                    Console.WriteLine("Unknown block");
                }
            }
        }

        public override long Position
        {
            get
            {
                return _Position;
            }
        }
        public override long Length
        {
            get
            {
                return _Length;
            }
        }
    }

}