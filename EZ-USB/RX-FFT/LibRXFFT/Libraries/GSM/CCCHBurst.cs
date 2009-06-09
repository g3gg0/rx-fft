using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GSM_Layer3;

namespace LibRXFFT.Libraries.GSM
{
    public class CCCHBurst : NormalBurst
    {
        private L3Handler L3;
        
        public CCCHBurst (L3Handler l3)
        {
            L3 = l3;
            Name = "CCCH";
        }


        public override bool ParseData(GSMParameters param, bool[] decodedBurst)
        {
            //DumpBits(decodedBurst);
            return true;
        }
    }
}
