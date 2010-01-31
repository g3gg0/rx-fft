using System;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;

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

    /* see GSM 04.08 10.5.2.5, table 10.2.25 */
    public enum eTimeSlotType
    {
        Unconfigured = 0,
        TCHF = 1,
        TCHH = 2,
        BCCH_CCCH_SDCCH4 = 4,
        SDCCH8 = 8,
        BCCH_CCCH = 100 /* for this one just using any other value */
    }

    public struct sTimeSlotInfo
    {
        public eTimeSlotType Type;
        public int Configures;
        public int Assignments;
        public int[] SubChanAssignments;
    }


    public struct sTimeSlotParam
    {
        public readonly Burst Burst;
        public readonly int Sequence;

        public sTimeSlotParam(Burst burst, int seq)
        {
            Burst = burst;
            Sequence = seq;
        }
    }


    public class GSMParameters
    {
        public sTimeSlotParam[][] TimeSlotHandlers;
        public sTimeSlotInfo[] TimeSlotInfo;

        public eGSMState State = eGSMState.Idle;
        public eTriState CBCH = eTriState.Unknown;

        public double PhaseOffsetFrequency
        {
            set 
            {
                PhaseOffsetValue = value / (Oversampling * (1625000.0f / 24.0f) / (Math.PI / 2));
            }
            get
            {
                return PhaseOffsetValue * Oversampling * (1625000.0f / 24.0f) / (Math.PI / 2);
            }
        }

        public double AveragePower;
        public double AverageIdlePower;

        public double PhaseOffsetValue = 0;
        public bool PhaseAutoOffset = true;
        public double Oversampling = 1;
        public double BT = 0.3d;

        private const long MaxErrors = 8*3;

        /* where in the double[] burst buffer the first bit starts */
        public double SampleStartPosition = 0;

        /* 
         * a temporary offset to apply. this is determined by the ParseSampleData method 
         * and reset after the burst was parsed
         */
        public double SampleOffset = 0;

        /* 
         * the fractional part of sample offset determined by zero crossing analysis.
         * has to get merged with SampleOffset
         * */
        public double SubSampleOffset = 0;

        public long FN;
        public long TN;

        public bool FirstSCH;

        public GSMParameters()
        {
            Reset();
        }


        public void Reset()
        {
            TotalErrors = 0;
            TotalSuccess = 0;
            State = eGSMState.Reset;
            TimeSlotInfo = new sTimeSlotInfo[8];
            TimeSlotHandlers = new sTimeSlotParam[8][];
        }

        /* 
         * SYSTEM INFORMATION TYPE mapping
         * 
         *  Message    TC       Allocation
            Type_1      0       BCCH Norm
            Type_2      1       BCCH Norm
            Type_2bis   5       BCCH Norm
            Type_2ter   5 or 4  BCCH Norm
            Type_3      2 and 6 BCCH Norm
            Type_4      3 and 7 BCCH Norm
            Type_7      7       BCCH Ext
            Type_8      3       BCCH Ext
            Type_9      4       BCCH Norm
            Type_13     4       BCCH norm
            Type_13     0       BCCH Ext
            Type 16     6       BCCH Ext
            Type 17     2       BCCH Ext
            Type 18  Not fixed  Not fixed
            Type 19  Not Fixed  Not Fixed
            Type 20  Not fixed  Not fixed
         */

        public long TC
        {
            get { return (FN / 51) % 8; }
        }

        public long T1
        {
            get { return FN / (26 * 51); }
        }

        public long T2
        {
            get { return FN % 26; }
        }

        public long T3
        {
            get { return FN % 51; }
        }

        public long T3M
        {
            get { return (T3 - 1) / 10; }
        }

        public long TotalErrors;
        public long TotalSuccess;
        public long Errors;

        public bool ErrorLimit
        {
            get { return Errors >= MaxErrors; }
        }

        public void ResetError()
        {
            Errors = 0;
        }

        public void Error()
        {
            Errors++;
            TotalErrors++;
        }

        internal void Success()
        {
            Errors = 0;
            TotalSuccess++;
        }

        public override string ToString()
        {
            return "T1: " + String.Format("{0,5}", T1) + " T2: " + String.Format("{0,2}", T2) + " T3: " + String.Format("{0,2}", T3) + " TN: " + String.Format("{0,1}", TN) + " TC: " + String.Format("{0,1}", TC) + " FN: " + String.Format("{0,8}", FN);
        }


        public string GetTimeslotDetails()
        {
            string retVal = "";

            retVal += "  ------------------------------------------------------------------------------------ - -  -  -" + Environment.NewLine;
            retVal += " | TS || Handlers (FC=FCCH, SC=SCH, BC=BCCH, CC=CCCH, SD=SDCCH, SA=SACCH, TC=TCH" + Environment.NewLine;
            retVal += " |----||------------------------------------------------------------------------------ - -  -  -" + Environment.NewLine;


            lock (TimeSlotHandlers)
            {
                for (int slot = 0; slot < 8; slot++)
                {
                    retVal += " |  " + slot + " || ";
                    if (TimeSlotHandlers[slot] == null)
                    {
                        retVal += "(Unused)";
                    }
                    else
                    {
                        for (int frame = 0; frame < TimeSlotHandlers[slot].Length; frame++)
                        {
                            Burst handler = TimeSlotHandlers[slot][frame].Burst;
                            int seq = TimeSlotHandlers[slot][frame].Sequence;

                            if (seq == 0)
                            {
                                if (frame != 0)
                                    retVal += "|";
                            }
                            else
                                retVal += " ";

                            if (handler != null)
                                retVal += handler.ShortName;
                            else
                                retVal += "-- ";
                        }
                    }
                    retVal += " |" + Environment.NewLine;
                }
            }

            retVal += "  ------------------------------------------------------------------------------------ - -  -  -" + Environment.NewLine;

            return retVal;
        }
        public string GetSlotUsage()
        {
            string retVal = "";

            retVal += "  ---------------------------------------------    -------------------------------------------------------" + Environment.NewLine;
            retVal += " | TS || Type                |  Cfgs  |  Uses  |  |                  SubChannel Uses                      |" + Environment.NewLine;
            retVal += " |----||---------------------|--------|--------|  |-------------------------------------------------------|" + Environment.NewLine;


            for (int pos = 0; pos < 8; pos++)
            {
                string type = "";
                string configs = "";
                string assignments = "";
                string subchanAssignments = "";

                switch (TimeSlotInfo[pos].Type)
                {
                    case eTimeSlotType.Unconfigured:
                        type = "(Unused)";
                        subchanAssignments = "";
                        for (int subChan = 0; subChan < 8; subChan++)
                            subchanAssignments += " |     ";
                        subchanAssignments += " |";
                        break;
                    case eTimeSlotType.BCCH_CCCH:
                        type = "BCCH+CCCH";
                        subchanAssignments = "";
                        for (int subChan = 0; subChan < 8; subChan++)
                            subchanAssignments += " |     ";
                        subchanAssignments += " |";
                        break;
                    case eTimeSlotType.TCHF:
                        type = "TCH/F";
                        subchanAssignments = "";
                        for (int subChan = 0; subChan < 8; subChan++)
                            subchanAssignments += " |     ";
                        subchanAssignments += " |";
                        break;
                    case eTimeSlotType.TCHH:
                        type = "TCH/H";
                        subchanAssignments = "";
                        for (int subChan = 0; subChan < 2; subChan++)
                            subchanAssignments += " | " + String.Format("{0,4}", TimeSlotInfo[pos].SubChanAssignments[subChan]);
                        for (int subChan = 2; subChan < 8; subChan++)
                            subchanAssignments += " |     ";
                        subchanAssignments += " |";
                        break;
                    case eTimeSlotType.BCCH_CCCH_SDCCH4:
                        type = "BCCH+CCCH+SDCCH/4";
                        subchanAssignments = "";
                        for (int subChan = 0; subChan < 4; subChan++)
                            subchanAssignments += " | " + String.Format("{0,4}", TimeSlotInfo[pos].SubChanAssignments[subChan]);
                        for (int subChan = 4; subChan < 8; subChan++)
                            subchanAssignments += " |     ";
                        subchanAssignments += " |";
                        break;
                    case eTimeSlotType.SDCCH8:
                        type = "SDCCH/8";
                        for (int subChan = 0; subChan < 8; subChan++)
                            subchanAssignments += " | " + String.Format("{0,4}", TimeSlotInfo[pos].SubChanAssignments[subChan]);
                        subchanAssignments += " |";
                        break;
                    default:
                        type = "(Unknown)";
                        break;
                }

                configs = string.Format("{0}", TimeSlotInfo[pos].Configures);
                assignments = string.Format("{0}", TimeSlotInfo[pos].Assignments);

                retVal += " |  " + pos + " || " + String.Format("{0,-19}", type) + " | " + String.Format("{0,6}", configs) + " | " + String.Format("{0,6}", assignments) + " | " + subchanAssignments + Environment.NewLine;
            }

            retVal += "  ---------------------------------------------    -------------------------------------------------------" + Environment.NewLine;

            return retVal;
        }



    }
}