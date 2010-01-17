using System;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class Burst
    {
        public static bool DumpRawData = false;

        public string ErrorMessage = "";
        public string StatusMessage = null;

        public L3Handler L3 = new L3Handler();
        public L2Handler L2 = new L2Handler();

        public const double PayloadBits = 142;
        public const double LeadingTailBits = 3;
        public const double TrailingTailBits = 3;
        public const double GuardBits = 8.25;

        public const double NetBitCount = LeadingTailBits + PayloadBits + TrailingTailBits;
        public const double TotalBitCount = LeadingTailBits + PayloadBits + TrailingTailBits + GuardBits;

        public string Name = "Unknown";
        public string ShortName = "Unk";

        public enum eSuccessState
        {
            Failed,
            Succeeded,
            Unknown
        }

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


        public virtual eSuccessState ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        /* parse the e[] bits */
        public virtual eSuccessState ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            return eSuccessState.Unknown;
        }

        /* parse the raw samples */
        public virtual eSuccessState ParseRawBurst(GSMParameters Parameters, double[] rawBurst)
        {
            return eSuccessState.Unknown;
        }
    }
}