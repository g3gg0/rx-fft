using LibRXFFT.Libraries.GSM.Layer1;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    public class FCHBurst : Burst
    {
        public FCHBurst()
        {
            Name = "FCH";
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return true;
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            return true;
        }
    }
}