using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Misc
{
    public class MemoryDump
    {
        public uint StartAddress;
    }

    public class MemoryDump8Bit : MemoryDump
    {
        public byte[] Data;
        public uint Length
        {
            get
            {
                return (uint)Data.Length;
            }
        }

        public static implicit operator MemoryDump16BitLE(MemoryDump8Bit dump)
        {
            MemoryDump16BitLE newDump = new MemoryDump16BitLE();

            newDump.StartAddress = dump.StartAddress;
            newDump.Data = new ushort[dump.Length / 2];

            for (uint pos = 0; pos < newDump.Length; pos++)
            {
                newDump.Data[pos] = (ushort)(dump.Data[2 * pos] | (dump.Data[2 * pos + 1] << 8));
            }

            return newDump;
        }

        public static implicit operator MemoryDump16BitBE(MemoryDump8Bit dump)
        {
            MemoryDump16BitBE newDump = new MemoryDump16BitBE();

            newDump.StartAddress = dump.StartAddress;
            newDump.Data = new ushort[dump.Length / 2];

            for (uint pos = 0; pos < newDump.Length; pos++)
            {
                newDump.Data[pos] = (ushort)(dump.Data[2 * pos + 1] | (dump.Data[2 * pos] << 8));
            }

            return newDump;
        }
    }

    public class MemoryDump16BitLE : MemoryDump
    {
        public ushort[] Data;
        public uint Length
        {
            get
            {
                return (uint)Data.Length;
            }
        }

        public static implicit operator MemoryDump8Bit(MemoryDump16BitLE dump)
        {
            MemoryDump8Bit newDump = new MemoryDump8Bit();

            newDump.StartAddress = dump.StartAddress;
            newDump.Data = new byte[dump.Length * 2];

            for (uint pos = 0; pos < dump.Length; pos++)
            {
                newDump.Data[2 * pos] = (byte)(dump.Data[pos] & 0xFF);
                newDump.Data[2 * pos + 1] = (byte)(dump.Data[pos] << 8);
            }

            return newDump;
        }
    }

    public class MemoryDump16BitBE : MemoryDump
    {
        public ushort[] Data;
        public uint Length
        {
            get
            {
                return (uint)Data.Length;
            }
        }

        public static implicit operator MemoryDump8Bit(MemoryDump16BitBE dump)
        {
            MemoryDump8Bit newDump = new MemoryDump8Bit();

            newDump.StartAddress = dump.StartAddress;
            newDump.Data = new byte[dump.Length * 2];

            for (uint pos = 0; pos < dump.Length; pos++)
            {
                newDump.Data[2 * pos] = (byte)(dump.Data[pos] << 8);
                newDump.Data[2 * pos + 1] = (byte)(dump.Data[pos] & 0xFF);
            }

            return newDump;
        }
    }
}
