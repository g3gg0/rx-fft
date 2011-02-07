using System;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer3;
using System.Text;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class Burst
    {
        public static bool DumpRawData = false;

        public readonly DateTime AllocationTime = DateTime.Now;
        public bool Released = false;
        public DateTime ReleaseTime;

        public string ErrorMessage = "";
        public string StatusMessage = null;

        public string Guid = System.Guid.NewGuid().ToString();

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

        private StringBuilder Builder = new StringBuilder();


        public enum eSuccessState
        {
            Failed,
            Succeeded,
            Unknown
        }

        protected string DumpBits(bool[] data)
        {
            return DumpBits(data, true);
        }

        protected string DumpBits(bool[] data, bool statusMessage)
        {
            Builder.Length = 0;

            foreach (bool value in data)
            {
                if (value)
                {
                    Builder.Append("1");
                }
                else
                {
                    Builder.Append("0");
                }
            }

            if (statusMessage)
            {
                if (StatusMessage == null)
                {
                    StatusMessage = Builder.ToString();
                }
                else
                {
                    StatusMessage += Builder.ToString();
                }
            }
            return Builder.ToString();
        }

        protected void DumpBytes(byte[] data)
        {
            Builder.Length = 0;

            foreach (byte value in data)
            {
                Builder.AppendFormat("{0:X02} ", value);
            }

            if (StatusMessage == null)
            {
                StatusMessage = Builder.ToString();
            }
            else
            {
                StatusMessage += Builder.ToString();
            }
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
        public virtual eSuccessState ParseRawBurst(GSMParameters Parameters, double[] rawBurst, double[] rawBurstStrength)
        {
            return eSuccessState.Unknown;
        }

        public virtual void Release()
        {
            ReleaseTime = DateTime.Now;
            Released = true;
        }
    }
}