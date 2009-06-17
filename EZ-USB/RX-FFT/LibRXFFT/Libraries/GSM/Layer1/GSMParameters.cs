using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.GSM.Layer1
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

        public long FN;
        public long TN;

        public long T1
        {
            get { return FN/(26*51); }
        }

        public long T2
        {
            get { return FN%26; }
        }

        public long T3
        {
            get { return FN%51; }
        }

        public long T3M
        {
            get { return (T3 - 1)/10; }
        }

        public long TotalErrors;
        public long TotalSuccess;
        public long Errors;

        public bool Error
        {
            get { return Errors >= MaxErrors; }

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

        public override string ToString()
        {
            return "T1: " + String.Format("{0,5}", T1) + " T2: " + String.Format("{0,2}", T2) + " T3: " + String.Format("{0,2}", T3) + " TN: " + String.Format("{0,1}", TN) + " FN: " + String.Format("{0,8}", FN);
        }
    }
}