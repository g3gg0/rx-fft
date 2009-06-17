using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    public class NormalBurst : Burst
    {
        public const double Data1Bits = 58;
        public const double SyncBits = 26;
        public const double Data2Bits = 58;

        public bool[] DummyBurstBits = new[] { true, true, true, true, true, false, true, true, false, true, true, true, false, true, true, false, false, false, false, false, true, false, true, false, false, true, false, false, true, true, true, false, false, false, false, false, true, false, false, true, false, false, false, true, false, false, false, false, false, false, false, true, true, true, true, true, false, false, false, true, true, true, false, false, false, true, false, true, true, true, false, false, false, true, false, true, true, true, false, false, false, true, false, true, false, true, true, true, false, true, false, false, true, false, true, false, false, false, true, true, false, false, true, true, false, false, true, true, true, false, false, true, true, true, true, false, true, false, false, true, true, true, true, true, false, false, false, true, false, false, true, false, true, true, true, true, true, false, true, false, true, false };

        internal bool[][] BurstBuffer;
        internal bool[][] DataDeinterleaved;
        internal bool[] DataDecoded;
        internal bool[] CRCBuffer;
        internal byte[] Data;

        internal void InitArrays()
        {
            DataDeinterleaved = new bool[1][];
            DataDecoded = new bool[228];

            DataDeinterleaved[0] = new bool[456];
            CRCBuffer = new bool[CRC.PolynomialFIRE.Length-1];
            Data = new byte[23];

            BurstBuffer = new bool[4][];
            for (int pos = 0; pos < BurstBuffer.Length; pos++)
                BurstBuffer[pos] = new bool[114];


        }

        internal bool IsDummy (bool[] burstBits, int start)
        {
            for (int pos = 0; pos < DummyBurstBits.Length; pos++ )
                if ( DummyBurstBits[pos] != burstBits[start+pos])
                    return false;

            return true;
        }
    }
}