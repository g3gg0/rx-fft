﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;


namespace LibRXFFT.Libraries.GSM.Layer1.PacketDump
{
    public class PcapFileWriter
    {
        private System.IO.FileStream fileStream;
        private const ushort MAJOR_VERSION_NUMBER = 0x02;
        private const ushort MINOR_VERSION_NUMBER = 0x04;
        private const uint MAGIC_NUMBER = 0xa1b2c3d4;
        private DateTime referenceTime;
        private bool isOpen;
        private string filename;

        public bool IsOpen { get { return this.isOpen; } }
        public string Filename { get { return this.filename; } }

        public PcapFileWriter(string filename, DataLinkType dataLinkType)
            : this(filename, dataLinkType, System.IO.FileMode.Create, 262144)
        {
            //nothing more needed
        }
        public PcapFileWriter(string filename, DataLinkType dataLinkType, System.IO.FileMode fileMode, int bufferSize)
        {
            this.filename = filename;
            this.referenceTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            this.fileStream = new FileStream(filename, fileMode, FileAccess.Write, FileShare.Write, bufferSize, FileOptions.SequentialScan);
            this.isOpen = true;

            fileStream.Write(ToByteArray(MAGIC_NUMBER), 0, 4);
            fileStream.Write(ToByteArray(MAJOR_VERSION_NUMBER), 0, 2);
            fileStream.Write(ToByteArray(MINOR_VERSION_NUMBER), 0, 2);
            fileStream.Write(ToByteArray((uint)0x00), 0, 4);//Time zone offset
            fileStream.Write(ToByteArray((uint)0x00), 0, 4);//accuracy of timestamps
            fileStream.Write(ToByteArray((uint)0xffff), 0, 4);//max length of captured packets, in octets
            fileStream.Write(ToByteArray((uint)dataLinkType), 0, 4);
        }



        public void WriteFrame(PcapPacket frame)
        {
            WriteFrame(frame, false);
        }

        public void WriteFrame(PcapPacket frame, bool flush)
        {
            lock (this)
            {
                TimeSpan delta = frame.Timestamp.Subtract(this.referenceTime);
                //The smallest unit of time is the tick, which is equal to 100 nanoseconds. A tick can be negative or positive.
                long totalMicroseconds = delta.Ticks / 10;
                uint seconds = (uint)(totalMicroseconds / 1000000);
                uint microseconds = (uint)(totalMicroseconds % 1000000);
                byte[] headerData = ToByteArray(frame.Header);

                fileStream.Write(ToByteArray(seconds), 0, 4);
                fileStream.Write(ToByteArray(microseconds), 0, 4);
                //number of octets of packet saved in file
                fileStream.Write(ToByteArray((uint)frame.Data.Length + headerData.Length), 0, 4);
                //actual length of packet
                fileStream.Write(ToByteArray((uint)frame.Data.Length + headerData.Length), 0, 4);
                //data
                fileStream.Write(headerData, 0, headerData.Length);

                fileStream.Write(frame.Data, 0, frame.Data.Length);
                if (flush)
                    fileStream.Flush();
            }
        }

        public void Close()
        {
            this.fileStream.Flush();
            this.fileStream.Close();
            this.isOpen = false;
        }

        public static byte[] ToByteArray(object obj)
        {
            int length = Marshal.SizeOf(obj);
            byte[] array = new byte[length];

            IntPtr ptr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, array, 0, length);
            Marshal.FreeHGlobal(ptr);

            return array;
        }

        public static byte[] ToByteArray(long value)
        {
            byte[] array = new byte[8];
            ToByteArray((uint)(value >> 32), array, 0);
            ToByteArray((uint)value, array, 4);
            return array;
        }
        public static byte[] ToByteArray(uint value)
        {
            byte[] array = new byte[4];
            ToByteArray(value, array, 0);
            return array;
        }
        public static byte[] ToByteArray(ushort value)
        {
            byte[] array = new byte[2];
            ToByteArray(value, array, 0);
            return array;
        }
        public static void ToByteArray(ushort value, byte[] array, int arrayOffset)
        {
            array[arrayOffset] = (byte)(value >> 8);
            array[arrayOffset + 1] = (byte)(value & 0x00ff);
        }
        public static void ToByteArray(uint value, byte[] array, int arrayOffset)
        {
            array[arrayOffset] = (byte)(value >> 24);
            array[arrayOffset + 1] = (byte)((value >> 16) & 0x000000ff);
            array[arrayOffset + 2] = (byte)((value >> 8) & 0x000000ff);
            array[arrayOffset + 3] = (byte)(value & 0x000000ff);
        }
    }
}

