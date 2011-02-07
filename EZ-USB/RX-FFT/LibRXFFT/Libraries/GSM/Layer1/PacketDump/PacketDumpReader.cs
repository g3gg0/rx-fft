using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.GSM.Layer1.PacketDump
{
    public interface PacketDumpReader
    {
        bool HasData { get; }
        void Read(bool[] BurstBitsUndiffed);
        void Close();
    }
}
