using System;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;
using System.Collections;
using System.Collections.Generic;
using LibRXFFT.Libraries.GSM.Layer1.PacketDump;
using LuaInterface;

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

    public enum eChannelType
    {
        FCH,
        SCH,
        BCCH,
        CCCH,
        SDCCH,
        SACCH,
        TCH
    }

    public enum eLinkDirection
    {
        Downlink = 0,
        Uplink = 1
    }

    /* 
     * This is a per-slot-persistent structure to track setup and changes.
     * Also contains the handler array
     */
    public struct sTimeSlotInfo
    {
        /* just to keep track where this TS setup belongs to */
        public long ARFCN;
        public eLinkDirection Direction;

        /* channel setup and statistics */
        public eTimeSlotType Type;
        public int Configures;
        public int Assignments;
        public int[] SubChanAssignments;

        /* and here all the handlers */
        public sTimeSlotParam[] Handlers;
    }

    public struct sTimeslotReference
    {
        public long T1;
        public long T2;
        public long T3;

        public static bool operator == (sTimeslotReference x, sTimeslotReference y)
        {
            return (x.T1 == y.T1) && (x.T2 == y.T2) && (x.T3 == y.T3);
        }

        public static bool operator != (sTimeslotReference x, sTimeslotReference y)
        {
            return (x.T1 != y.T1) || (x.T2 != y.T2) || (x.T3 != y.T3);
        }
    }

    public struct sTimeSlotParam
    {
        public readonly Burst Burst;
        public readonly int Sequence;
        public sTimeslotReference Reference;

        public sTimeSlotParam(Burst burst, int seq)
        {
            Burst = burst;
            Sequence = seq;
            Reference = new sTimeslotReference();
        }
    }

    public interface CipherCracker
    {
        byte[] Crack(bool[] key1, uint count1, bool[] key2, uint count2);
        bool Available { get; }
        int SearchDuration { get; }
        void SetJobInfo(int jobNumber, int jobCount);
        void Close();
    }

    public class GSMParameters
    {
        public LinkedList<NormalBurst> ActiveBursts = new LinkedList<NormalBurst>();
        public LinkedList<NormalBurst> UsedBursts = new LinkedList<NormalBurst>();

        /* 
         * This array will contain all handlers in the format:
         *
         *  TimeSlotHandlerConfig[#ARFCN,direction][timeslot][framenum]
         *  
         *  #ARFCN    = 0-n, for the configured number of ARFCNs
         *  direction = 0 or 1, with 0 = downlink, 1 = uplink
         *  timeslot  = 0-7, timeslot number
         *  framenum  = 0-51 or 0-26 
         *         
         */
        public sTimeSlotInfo[,][] TimeSlotConfig = null;

        /*
         * This dictionary contains all configured ARFCNs and their
         * position in TimeSlotHandlerConfig.
         * Keep a reverse lookup version for dumping ARFCN infos.
         */
        public Dictionary<long, long> ArfcnMap = new Dictionary<long, long>();
        public Dictionary<long, long> ArfcnMapRev = new Dictionary<long, long>();

        public DateTime TimeStamp = DateTime.Now;

        public eGSMState State = eGSMState.Idle;
        public eTriState CBCH = eTriState.Unknown;

        public bool SkipL2Parsing = false;
        public bool ReportL1Errors = true;
        public PacketDumpWriter PacketDumper = null;
        public Burst CurrentBurstHandler = null;

        public LinkedList<byte[]> A5KeyStore = new LinkedList<byte[]>();

        public CipherCracker CipherCracker = null;

        public Lua LuaVm = null; 

        public void AddA5Key(byte[] key)
        {
            /* dont add invalid keys */
            if (key == null || key.Length != 8)
            {
                return;
            }

            /* dont add already existing keys */
            lock (A5KeyStore)
            {
                bool exists = false;

                foreach (byte[] checkKey in A5KeyStore)
                {
                    bool match = true;

                    for (int pos = 0; pos < checkKey.Length; pos++)
                    {
                        if (checkKey[pos] != key[pos])
                        {
                            match = false;
                        }
                    }

                    if (match)
                    {
                        exists = true;
                    }
                }

                if (!exists)
                {
                    A5KeyStore.AddLast(key);
                }
            }
        }

        public void ClearA5Keys()
        {
            lock (A5KeyStore)
            {
                A5KeyStore.Clear();
            }
        }

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

        public double CurrentPower;
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

        public bool FirstSCH;

        public GSMParameters()
        {
            Reset();
        }


        public void Reset()
        {
            State = eGSMState.Reset;
            TotalErrors = 0;
            TotalSuccess = 0;
            PhaseOffsetValue = 0;
            FN = -1;
            MNC = -1;
            MCC = -1;
            LAC = -1;
            CellIdent = -1;

            ActiveBursts.Clear();
            UsedBursts.Clear();

            /* set up default arfcn map */
            ArfcnMap.Clear();
            ArfcnMapRev.Clear();
            ArfcnMap.Add(-1, 0);
            ArfcnMapRev.Add(0, -1);

            /* and set up empty timeslot configs */
            TimeSlotConfig = new sTimeSlotInfo[1, 2][];
            for (int pos = 0; pos < TimeSlotConfig.Length; pos++)
            {
                TimeSlotConfig[pos / 2, pos % 2] = new sTimeSlotInfo[8];
            }

            /* default is downlink of first ARFCN */
            ARFCN = -1;
            Dir = eLinkDirection.Downlink;

            //CurrentTimeSlotConfig = new sTimeSlotInfo[8];
            //CurrentTimeSlotHandlers = new sTimeSlotParam[8][];

            AverageIdlePower = 0;
            AveragePower = 0;
            CurrentPower = 0;
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

        public eLinkDirection Dir;
        public long _ARFCN;
        public int ARFCNidx
        {
            get
            {
                return GetARFCNIdx(ARFCN);
            }
        }
        public long ARFCN
        {
            get
            {
                return _ARFCN;
            }
            set
            {
                EnsureARFCN(value);
                _ARFCN = value;
            }
        }
        public long MNC;
        public long MCC;
        public long LAC;
        public long CellIdent;
        public byte BSIC;


        public long FN;
        public long TN;

        /* T1 [11 bits] as specified in GSM-05.02 Ch 3.3.2.2.1 b) */
        public long T1
        {
            get { return FN / (26 * 51); }
        }
        
        /* T1R [6 bits] as specified in GSM-05.02 Figure 6 */
        public long T1R
        {
            get { return T1 % 64; }
        }

        /* T2 [5 bits] as specified in GSM-05.02 Ch 3.3.2.2.1 b) */
        public long T2
        {
            get { return FN % 26; }
        }

        /* T3 [6 bits] as specified in GSM-05.02 Ch 3.3.2.2.1 b) */
        public long T3
        {
            get { return FN % 51; }
        }

        /* T3' [3 bits] as specified in GSM-05.02 Ch 3.3.2.2.1 b) */
        public long T3M
        {
            get { return (T3 - 1) / 10; }
        }

        /* COUNT [22 bits] as specified in GSM-03.20 Ch 3.1.2 */
        public uint Count
        {
            get { return (uint)((T1 << 11) | (T3 << 5) | T2); }
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
            return "T1: " + String.Format("{0,5}", T1) + " T2: " + String.Format("{0,2}", T2) + " T3: " + String.Format("{0,2}", T3) + " TN: " + String.Format("{0,1}", TN) + " FN: " + String.Format("{0,8}", FN);
        }


        public string GetTimeslotDetails()
        {
            string retVal = "";

            for (int pos = 0; pos < TimeSlotConfig.Length; pos++)
            {
                lock (TimeSlotConfig)
                {
                    Hashtable handlers = new Hashtable();

                    retVal += "  ------------------------------------------------------------------------------------ - -  -  -" + Environment.NewLine;
                    retVal += " | ARFCN: " + ArfcnMapRev[pos / 2];
                    switch ((eLinkDirection)(pos % 2))
                    {
                        case eLinkDirection.Downlink:
                            retVal += " Dir: Downlink" + Environment.NewLine;
                            break;
                        case eLinkDirection.Uplink:
                            retVal += " Dir: Uplink" + Environment.NewLine;
                            break;
                    }
                    retVal += " | TS || Handlers (FC=FCCH, SC=SCH, BC=BCCH, CC=CCCH, SD=SDCCH, SA=SACCH, TC=TCH" + Environment.NewLine;
                    retVal += " |----||------------------------------------------------------------------------------ - -  -  -" + Environment.NewLine;


                    sTimeSlotInfo[] info = TimeSlotConfig[pos / 2, pos % 2];

                    for (int slot = 0; slot < 8; slot++)
                    {
                        retVal += " |  " + slot + " || ";
                        if (info[slot].Handlers == null)
                        {
                            retVal += "(Unused)";
                        }
                        else
                        {
                            for (int frame = 0; frame < info[slot].Handlers.Length; frame++)
                            {
                                Burst handler = info[slot].Handlers[frame].Burst;
                                int seq = info[slot].Handlers[frame].Sequence;

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

                    retVal += "  ------------------------------------------------------------------------------------ - -  -  -" + Environment.NewLine;

                    ArrayList lines = new ArrayList();
                }
            } 
            
            retVal += Environment.NewLine;
            retVal += "Handler details:" + Environment.NewLine;
            foreach (NormalBurst burst in UsedBursts)
            {
                retVal += string.Format("  {0,12}:  [Data: {1,6}]  [Crypt: {2,6}]  [Dummy: {3,6}]   [{4}] - [{5}]" + Environment.NewLine, burst.Name, burst.DataBursts, burst.CryptedFrames, burst.DummyBursts, burst.AllocationTime, (burst.Released ? burst.ReleaseTime.ToString() : "now"));
            }

            retVal += Environment.NewLine;

            return retVal;
        }

        public string GetSlotUsage()
        {
            string retVal = "";

            for (int pos = 0; pos < TimeSlotConfig.Length; pos++)
            {
                retVal += "  ---------------------------------------------" + Environment.NewLine;
                retVal += " | ARFCN: " + ArfcnMapRev[pos / 2];
                switch ((eLinkDirection)(pos % 2))
                {
                    case eLinkDirection.Downlink:
                        retVal += " Dir: Downlink" + Environment.NewLine;
                        break;
                    case eLinkDirection.Uplink:
                        retVal += " Dir: Uplink" + Environment.NewLine;
                        break;
                }
                retVal += "  ---------------------------------------------    -------------------------------------------------------" + Environment.NewLine;
                retVal += " | TS || Type                |  Cfgs  |  Uses  |  |                  SubChannel Uses                      |" + Environment.NewLine;
                retVal += " |----||---------------------|--------|--------|  |-------------------------------------------------------|" + Environment.NewLine;


                sTimeSlotInfo[] info = TimeSlotConfig[pos / 2, pos % 2];
                for (int slot = 0; slot < 8; slot++)
                {
                    string type = "";
                    string configs = "";
                    string assignments = "";
                    string subchanAssignments = "";

                    switch (info[slot].Type)
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
                                subchanAssignments += " | " + String.Format("{0,4}", info[slot].SubChanAssignments[subChan]);
                            for (int subChan = 2; subChan < 8; subChan++)
                                subchanAssignments += " |     ";
                            subchanAssignments += " |";
                            break;
                        case eTimeSlotType.BCCH_CCCH_SDCCH4:
                            type = "BCCH+CCCH+SDCCH/4";
                            subchanAssignments = "";
                            for (int subChan = 0; subChan < 4; subChan++)
                                subchanAssignments += " | " + String.Format("{0,4}", info[slot].SubChanAssignments[subChan]);
                            for (int subChan = 4; subChan < 8; subChan++)
                                subchanAssignments += " |     ";
                            subchanAssignments += " |";
                            break;
                        case eTimeSlotType.SDCCH8:
                            type = "SDCCH/8";
                            for (int subChan = 0; subChan < 8; subChan++)
                                subchanAssignments += " | " + String.Format("{0,4}", info[slot].SubChanAssignments[subChan]);
                            subchanAssignments += " |";
                            break;
                        default:
                            type = "(Unknown)";
                            break;
                    }

                    configs = string.Format("{0}", info[slot].Configures);
                    assignments = string.Format("{0}", info[slot].Assignments);

                    retVal += " |  " + slot + " || " + String.Format("{0,-19}", type) + " | " + String.Format("{0,6}", configs) + " | " + String.Format("{0,6}", assignments) + " | " + subchanAssignments + Environment.NewLine;
                }

                retVal += "  ---------------------------------------------    -------------------------------------------------------" + Environment.NewLine;
            }
            return retVal;
        }
        
        internal void EnsureARFCN(long arfcn)
        {
            /* the first time an ARFCN was assigned? */
            if (ARFCN == -1)
            {
                /* this is our default ARFCN now */
                ArfcnMap.Clear();
                ArfcnMapRev.Clear();
                ArfcnMap.Add(arfcn, 0);
                ArfcnMapRev.Add(0, arfcn);
            }

            /* a new, unconfigured ARFCN? */
            if (!ArfcnMap.ContainsKey(arfcn))
            {
                /* resize array */
                int newIndex = (TimeSlotConfig.Length / 2) + 1;

                sTimeSlotInfo[,][] tmp = new sTimeSlotInfo[TimeSlotConfig.Length / 2 + 1, 2][];
                Array.Copy(TimeSlotConfig, tmp, TimeSlotConfig.Length);

                TimeSlotConfig = tmp;

                /* and put reference */
                ArfcnMap.Add(arfcn, newIndex);
                ArfcnMapRev.Add(newIndex, arfcn);
            }
        }

        internal int GetARFCNIdx(long arfcn)
        {
            return (int)ArfcnMap[arfcn];
        }
    }
}