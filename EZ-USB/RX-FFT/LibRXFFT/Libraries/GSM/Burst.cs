using System;

namespace LibRXFFT.Libraries.GSM
{
    public class Burst
    {
        public string ErrorMessage = "";
        public string StatusMessage = null;

        public const double PayloadBits = 142;
        public const double LeadingTailBits = 3;
        public const double TrailingTailBits = 3;
        public const double GuardBits = 8.25;

        public const double NetBitCount = LeadingTailBits + PayloadBits + TrailingTailBits;
        public const double TotalBitCount = LeadingTailBits + PayloadBits + TrailingTailBits + GuardBits;

        public string Name = "Unknown";


        protected void DumpBits(bool[] data)
        {
            string msg = "";

            foreach (bool value in data)
            {
                if (value)
                    msg += "1";
                else
                    msg += "0";
            }

            if (StatusMessage == null)
                StatusMessage = msg;
            else
                StatusMessage += msg;
        }

        protected void DumpBytes(byte[] data)
        {
            string msg = "";

            foreach (byte value in data)
                msg += String.Format("{0:X02} ", value);

            if (StatusMessage == null)
                StatusMessage = msg;
            else
                StatusMessage += msg;
        }

        public virtual bool ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return false;
        }
    }
}