using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GSM.Layer1;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    public class TCHBurst : NormalBurst
    {
        public TCHBurst()
        {
            Name = "TCH";
            ShortName = "TC ";
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
