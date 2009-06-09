namespace LibRXFFT.Libraries.GSM
{
    public class FCHBurst : Burst
    {
        public FCHBurst ()
        {
            Name = "FCH";
        }
        
        public override bool ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return true;
        }
    }
}