using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.GSM.Layer1.PacketDump
{
    public interface PacketDumpWriter
    {
        void Write(GSMParameters Parameters, bool[] BurstBitsUndiffed);
        void Close();
    }
}
