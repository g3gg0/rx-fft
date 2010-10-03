using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.GSM.Layer1.PacketDump
{
    public interface PacketDumpWriter
    {
        void WriteRawBurst(GSMParameters Parameters, bool[] BurstBitsUndiffed);
        void Close();

        void WriteL2Data(GSMParameters param, byte[] l2Data);
    }
}
