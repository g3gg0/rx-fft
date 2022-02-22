using System;
using System.Collections.Generic;
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
        private WaveHeader WavHeader;
        private long _Length;
        private long _Position;

        public RtsaFileReader(string fileName) : this(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

        public RtsaFileReader(string fileName, eFileType type) : this(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

        public RtsaFileReader(FileStream stream)
        {
            InputStream = stream;

            // Open the destination .wav file
            Reader = new BinaryReader(InputStream);
            /*
            while(true)
            {
                var block = ReadBlock(out byte[] payload);

                if(block == null)
                {
                    return;
                }

                if (block is DsfhBlock blk1)
                {
                    Console.WriteLine("Block: " + blk1.Header.TypeString);
                }
                else if (block is StrmBlock blk2)
                {
                    Console.WriteLine("Block: " + blk2.Header.TypeString);
                }
                else if (block is SstrBlock blk3)
                {
                    Console.WriteLine("Block: " + blk3.Header.TypeString);
                }
                else if (block is SampBlock blk4)
                {
                    Console.WriteLine("Block: " + blk4.Header.TypeString);
                }
                else if (block is SprvBlock blk5)
                {
                    Console.WriteLine("Block: " + blk5.Header.TypeString);
                }
                else if (block is StrtBlock blk6)
                {
                    Console.WriteLine("Block: " + blk6.Header.TypeString);
                }
                else if (block is DsftBlock blk7)
                {
                    Console.WriteLine("Block: " + blk7.Header.TypeString);
                }
            }*/

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RtsaBlock 
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public char[] Type;
            public uint Length;
            public uint Unk1;
            public ushort Unk2;
            public ushort HeaderSize; 
            
            public string TypeString { get => new string(Type); set => Type = value.ToCharArray(); }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DsfhBlock 
        {
            public RtsaBlock Header;
            public ulong Unk1;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StrmBlock 
        {
            public RtsaBlock Header;
            public ulong Unk1;
            public uint Unk2;
            public uint Unk3;
            public uint Unk4;
            public uint Unk5;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SstrBlock 
        {
            public RtsaBlock Header;
            public ulong Unk1;
            public uint Unk2;
            public uint Unk3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xD0)]
            public byte[] Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SampBlock 
        {
            public RtsaBlock Header;
            public ulong Unk1;
            public uint Unk2;
            public uint Unk3;
            public double StartTime;
            public double EndTime;
            public uint Unk4;
            public uint Bins;
            public uint Unk5;
            public uint BinCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DsftBlock
        {
            public RtsaBlock Header;
            public ulong Unk1;
            public uint Unk2;
            public uint Unk3;
            public ulong Unk4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StrtBlock
        {
            public RtsaBlock Header;
            public ulong Unk1;
            public uint Unk2;
            public uint Unk3;
            public ulong Unk4;
            public ulong Unk5;
            public ulong Unk6;
            public uint Unk7;
            public uint Unk8;
            public uint Unk9;
            public uint Unk10;
            public double StopTime;
            public ulong Unk11;
            public ulong Unk12;
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
            }

            return block;
        }

        private void Rewind()
        {
            _Position = 0;
        }

        public override long Seek(int pos, SeekOrigin seekOrigin)
        {
            Rewind();

            return 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;

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

}
