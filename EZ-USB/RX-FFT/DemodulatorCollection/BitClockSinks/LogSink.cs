using DemodulatorCollection.Interfaces;
using RX_FFT.Components.GDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemodulatorCollection.BitClockSinks
{
    class LogSink : BitClockSink
    {
        private int BitPos = 0;
        private bool[] Bits = new bool[0];

        public void ClockBit(bool state)
        {
            Bits[BitPos++] = state;

            if(BitPos >= Bits.Length)
            {
                Array.Resize<bool>(ref Bits, BitPos + 1);
            }
        }

        public void TransmissionStart()
        {
            BitPos = 0;
            Bits = new bool[1];
        }

        public void TransmissionEnd()
        {
            if(BitPos < 8)
            {
                return;
            }

            string msg = "";
            int byteCount = (BitPos - 1) / 8 + 1;

            for (int bytePos = 0; bytePos < byteCount; bytePos++)
            {
                ulong value = 0;
                int startPos = (byteCount - bytePos - 1) * 8;
                int endPos = startPos + 8;

                for (int pos = Math.Max(startPos, 0); pos < Math.Min(endPos, Bits.Length); pos++)
                {
                    value <<= 1;
                    value |= (Bits[pos] ? 1U : 0);
                }

                msg = value.ToString("X2") + msg;
            }

            Log.AddMessage("Data: 0x" + msg);
        }

        public void Resynchronized()
        {
        }

        public void Desynchronized()
        {
        }
    }
}
