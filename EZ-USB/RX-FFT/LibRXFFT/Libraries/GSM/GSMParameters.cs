using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.GSM
{

    public enum eGSMState
    {
        Idle,
        FCCHSearch,
        SCHSearch,
        Lock,
        Reset
    }

    public class GSMParameters
    {
        private const long MaxErrors = 3;

        public long SampleOffset = 0;
        public long AbsoluteFrameNumber = 0;
        public long CurrentTimeSlot;
        public long CurrentControlFrame = 0;
        public long CurrentTrafficFrame = 0;

        public long TotalErrors;
        public long TotalSuccess;
        public long Errors;
        public bool Error
        {
            get
            {
                return Errors >= MaxErrors;
            }

            set
            {
                /* when resetting error bit, reset error counter. if setting, just increase counter */
                if (value)
                {
                    Errors++;
                    TotalErrors++;
                }
                else
                {
                    Errors = 0;
                    TotalSuccess++;
                }
            }
        }

        public eGSMState State = eGSMState.Idle;
        public bool FirstSCH;
    }

}
