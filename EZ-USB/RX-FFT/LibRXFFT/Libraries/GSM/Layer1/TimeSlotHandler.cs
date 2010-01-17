using System;


using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;
using LibRXFFT.Libraries.GSM.Layer1.GMSK;
using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;

namespace LibRXFFT.Libraries.GSM.Layer1
{
    public delegate void AddMessageDelegate(string msg);

    public class TimeSlotHandler
    {
        public static bool PreallocateTCHs = false;

        private readonly double BT;
        public readonly double Oversampling;


        public AddMessageDelegate AddMessage { get; set; }

        public readonly GMSKDecoder Decoder;
        public readonly L3Handler L3;
        private readonly BCCHBurst BCCH;
        private readonly CCCHBurst CCCH;
        private readonly SCHBurst SCH;
        private readonly FCHBurst FCH;

        public readonly bool[] BurstBits = new bool[148];
        public readonly bool[] BurstBitsUndiffed = new bool[148];

        private GSMParameters Parameters;

        public int SpareBits = 3;


        public TimeSlotHandler(GSMParameters param, AddMessageDelegate addMessage)
        {
            Oversampling = param.Oversampling;
            BT = param.BT;
            AddMessage = addMessage;
            Parameters = param;

            Decoder = new GMSKDecoder(Oversampling, BT);

            L3 = new L3Handler();
            FCH = new FCHBurst(param);
            SCH = new SCHBurst(param);
            BCCH = new BCCHBurst(L3);
            CCCH = new CCCHBurst(L3);

            L3.PDUDataTriggers.Add("CCCH-CONF", TriggerCCCHCONF);
            L3.PDUDataTriggers.Add("ChannelAssignment", TriggerChannelAssignment);
            L3.PDUDataTriggers.Add("CBCHUpdate", TriggerCBCHUpdate);
            L3.PDUDataTriggers.Add("CBCHReset", TriggerCBCHReset);

            for (int pos = 0; pos < 8; pos++)
            {
                Parameters.TimeSlotInfo[pos].Type = eTimeSlotType.Unconfigured;
                Parameters.TimeSlotInfo[pos].SubChanAssignments = new int[8];
            }


            Parameters.TimeSlotHandlers[0] = new sTimeSlotParam[51];
            Parameters.TimeSlotHandlers[0][0] = new sTimeSlotParam(FCH, 0);
            Parameters.TimeSlotHandlers[0][1] = new sTimeSlotParam(SCH, 0);
            Parameters.TimeSlotHandlers[0][2] = new sTimeSlotParam(BCCH, 0);
            Parameters.TimeSlotHandlers[0][3] = new sTimeSlotParam(BCCH, 1);
            Parameters.TimeSlotHandlers[0][4] = new sTimeSlotParam(BCCH, 2);
            Parameters.TimeSlotHandlers[0][5] = new sTimeSlotParam(BCCH, 3);
            Parameters.TimeSlotHandlers[0][6] = new sTimeSlotParam(CCCH, 0);
            Parameters.TimeSlotHandlers[0][7] = new sTimeSlotParam(CCCH, 1);
            Parameters.TimeSlotHandlers[0][8] = new sTimeSlotParam(CCCH, 2);
            Parameters.TimeSlotHandlers[0][9] = new sTimeSlotParam(CCCH, 3);

            if (PreallocateTCHs)
            {
                for (int timeSlot = 1; timeSlot < 8; timeSlot++)
                {
                    Parameters.TimeSlotHandlers[timeSlot] = new sTimeSlotParam[26];

                    TCHBurst tch = new TCHBurst(L3, "TCH" + timeSlot + "/F", (int)timeSlot);
                    SACCHBurst sacch = new SACCHBurst(L3, "SACCH/TCH" + timeSlot, (int)timeSlot, true);

                    for (int frame = 0; frame < 25; frame++)
                    {
                        if (frame == 12)
                            Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(sacch, 0);
                        else
                            Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(tch, 0);
                    }
                }
            }

        }

        private void TriggerChannelAssignment(L3Handler L3Handler)
        {
            eTimeSlotType channelType;
            long subChannel;
            long timeSlot;

            lock (L3Handler.PDUDataFields)
            {
                if (!L3Handler.PDUDataRawFields.ContainsKey("ChannelType"))
                    return;
                if (!L3Handler.PDUDataRawFields.ContainsKey("SubChannel"))
                    return;
                if (!L3Handler.PDUDataRawFields.ContainsKey("TimeSlot"))
                    return;

                channelType = (eTimeSlotType)L3Handler.PDUDataRawFields["ChannelType"];
                subChannel = L3Handler.PDUDataRawFields["SubChannel"];
                timeSlot = L3Handler.PDUDataRawFields["TimeSlot"];
            }

            /* assigned time slot type does not match? */
            if (Parameters.TimeSlotHandlers[timeSlot] == null || Parameters.TimeSlotInfo[timeSlot].Type != channelType)
            {
                lock (Parameters.TimeSlotHandlers)
                {
                    switch (channelType)
                    {
                        case eTimeSlotType.TCHF:
                            AddMessage("   [L1] TimeSlot " + timeSlot + " now configured as TCH/F (was " + Parameters.TimeSlotInfo[timeSlot].Type + ")" + Environment.NewLine);
                            Parameters.TimeSlotHandlers[timeSlot] = new sTimeSlotParam[26];

                            TCHBurst tch = new TCHBurst(L3, "TCH" + timeSlot + "/F", (int)timeSlot);
                            SACCHBurst sacch = new SACCHBurst(L3, "SACCH/TCH" + timeSlot, (int)timeSlot, true);

                            for (int frame = 0; frame < 25; frame++)
                            {
                                if (frame == 12)
                                    Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(sacch, 0);
                                else
                                    Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(tch, 0);
                            }
                            break;

                        case eTimeSlotType.TCHH:
                            AddMessage("   [L1] TimeSlot " + timeSlot + " now configured as TCH/H (was " + Parameters.TimeSlotInfo[timeSlot].Type + ")" + Environment.NewLine);
                            Parameters.TimeSlotHandlers[timeSlot] = new sTimeSlotParam[26];

                            TCHBurst tch1 = new TCHBurst(L3, "TCH" + timeSlot + "/H 1", (int)timeSlot);
                            TCHBurst tch2 = new TCHBurst(L3, "TCH" + timeSlot + "/H 2", (int)timeSlot);
                            SACCHBurst sacch1 = new SACCHBurst(L3, "SACCH1/TCH" + timeSlot, (int)timeSlot, true);
                            SACCHBurst sacch2 = new SACCHBurst(L3, "SACCH2/TCH" + timeSlot, (int)timeSlot, true);

                            for (int frame = 0; frame < 26; frame++)
                            {
                                if (frame == 12)
                                    Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(sacch1, 0);
                                else if (frame == 25)
                                    Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(sacch2, 0);
                                else if ((frame & 1) == 0)
                                    Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(tch1, 0);
                                else
                                    Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(tch2, 0);
                            }
                            break;

                        case eTimeSlotType.SDCCH8:
                            if (timeSlot != 0)
                            {
                                AddMessage("   [L1] TimeSlot " + timeSlot + " now configured as SDCCH/8 (was " + Parameters.TimeSlotInfo[timeSlot].Type + ")" + Environment.NewLine);
                                Parameters.TimeSlotHandlers[timeSlot] = new sTimeSlotParam[51];

                                /* 8 SDCCHs */
                                for (int chan = 0; chan < 8; chan++)
                                {
                                    SDCCHBurst tmpSDCCH = new SDCCHBurst(L3, chan);
                                    Parameters.TimeSlotHandlers[timeSlot][chan * 4 + 0] = new sTimeSlotParam(tmpSDCCH, 0);
                                    Parameters.TimeSlotHandlers[timeSlot][chan * 4 + 1] = new sTimeSlotParam(tmpSDCCH, 1);
                                    Parameters.TimeSlotHandlers[timeSlot][chan * 4 + 2] = new sTimeSlotParam(tmpSDCCH, 2);
                                    Parameters.TimeSlotHandlers[timeSlot][chan * 4 + 3] = new sTimeSlotParam(tmpSDCCH, 3);
                                }

                                /* finally 4 SACCHs */
                                for (int chan = 0; chan < 4; chan++)
                                {
                                    SACCHBurst tmpSACCH = new SACCHBurst(L3, "SACCH " + chan + "/" + (chan + 4), chan);
                                    Parameters.TimeSlotHandlers[timeSlot][(chan + 8) * 4 + 0] = new sTimeSlotParam(tmpSACCH, 0);
                                    Parameters.TimeSlotHandlers[timeSlot][(chan + 8) * 4 + 1] = new sTimeSlotParam(tmpSACCH, 1);
                                    Parameters.TimeSlotHandlers[timeSlot][(chan + 8) * 4 + 2] = new sTimeSlotParam(tmpSACCH, 2);
                                    Parameters.TimeSlotHandlers[timeSlot][(chan + 8) * 4 + 3] = new sTimeSlotParam(tmpSACCH, 3);
                                }
                            }
                            else
                            {
                                AddMessage("   [L1] TimeSlot " + timeSlot + " NOT configured for SDCCH/8 as requested. Stays " + Parameters.TimeSlotInfo[timeSlot].Type + Environment.NewLine);
                            }
                            break;

                        default:
                            AddMessage("   [L1] TimeSlot " + timeSlot + " cannot get configured. Type: " + (int)channelType + Environment.NewLine);
                            break;
                    }

                    Parameters.TimeSlotInfo[timeSlot].Type = channelType;
                    Parameters.TimeSlotInfo[timeSlot].Configures++;
                }
            }

            Parameters.TimeSlotInfo[timeSlot].Assignments++;
            if (subChannel >= 0)
                Parameters.TimeSlotInfo[timeSlot].SubChanAssignments[subChannel]++;
        }

        private void TriggerCBCHReset(L3Handler L3Handler)
        {
            lock (L3Handler.PDUDataFields)
            {
                if (L3Handler.PDUDataFields.ContainsKey("ChannelType"))
                    L3Handler.PDUDataFields.Remove("ChannelType");
                if (L3Handler.PDUDataFields.ContainsKey("SubChannel"))
                    L3Handler.PDUDataFields.Remove("SubChannel");
                if (L3Handler.PDUDataFields.ContainsKey("TimeSlot"))
                    L3Handler.PDUDataFields.Remove("TimeSlot");

                if (L3Handler.PDUDataRawFields.ContainsKey("ChannelType"))
                    L3Handler.PDUDataRawFields.Remove("ChannelType");
                if (L3Handler.PDUDataRawFields.ContainsKey("SubChannel"))
                    L3Handler.PDUDataRawFields.Remove("SubChannel");
                if (L3Handler.PDUDataRawFields.ContainsKey("TimeSlot"))
                    L3Handler.PDUDataRawFields.Remove("TimeSlot");
            }

        }


        private void TriggerCBCHUpdate(L3Handler L3Handler)
        {
            long channelType;
            long subChannel;
            long timeSlot;

            if (Parameters.CBCH != eTriState.Unknown)
                return;

            lock (L3Handler.PDUDataFields)
            {
                if (!L3Handler.PDUDataRawFields.ContainsKey("ChannelType"))
                {
                    Parameters.CBCH = eTriState.No;
                    return;
                }
                if (!L3Handler.PDUDataRawFields.ContainsKey("SubChannel"))
                {
                    Parameters.CBCH = eTriState.No;
                    return;
                }
                if (!L3Handler.PDUDataRawFields.ContainsKey("TimeSlot"))
                {
                    Parameters.CBCH = eTriState.No;
                    return;
                }

                channelType = L3Handler.PDUDataRawFields["ChannelType"];
                subChannel = L3Handler.PDUDataRawFields["SubChannel"];
                timeSlot = L3Handler.PDUDataRawFields["TimeSlot"];
            }

            /* channel type 4 = SDCCH/4, channel type 8 = SDCCH/8 */
            if (subChannel < 0 || timeSlot < 0 || channelType < 4)
            {
                Parameters.CBCH = eTriState.No;
                return;
            }

            Parameters.CBCH = eTriState.Yes;

            CBCHBurst Burst = new CBCHBurst(L3, (int)subChannel);
            long frame = 0;

            /* type 4 is in timeslot 0 and shared with BCCH, CCCH and SDCCH */
            if (channelType == 4)
                frame = 20 + 2 + (subChannel / 2) * 10 + (subChannel & 1) * 4;
            else
                frame = 4 * subChannel;

            /* call generic trigger to set up arrays */
            TriggerChannelAssignment(L3Handler);
            AddMessage("   [L1] TimeSlot " + timeSlot + " SubChannel " + subChannel + " now configured as Cell Broadcast channel." + Environment.NewLine);

            lock (Parameters.TimeSlotHandlers)
            {
                Parameters.TimeSlotHandlers[timeSlot][frame + 0] = new sTimeSlotParam(Burst, 0);
                Parameters.TimeSlotHandlers[timeSlot][frame + 1] = new sTimeSlotParam(Burst, 1);
                Parameters.TimeSlotHandlers[timeSlot][frame + 2] = new sTimeSlotParam(Burst, 2);
                Parameters.TimeSlotHandlers[timeSlot][frame + 3] = new sTimeSlotParam(Burst, 3);
            }

        }

        private void TriggerCCCHCONF(L3Handler L3Handler)
        {
            long ccchConf;

            if (Parameters.TimeSlotInfo[0].Type != eTimeSlotType.Unconfigured)
                return;

            lock (L3Handler.PDUDataFields)
            {
                if (!L3Handler.PDUDataRawFields.ContainsKey("CCCH-CONF"))
                    return;
                ccchConf = L3Handler.PDUDataRawFields["CCCH-CONF"];
            }

            lock (Parameters.TimeSlotHandlers)
            {
                switch (ccchConf)
                {
                    /* 1 basic physical channel used for CCCH, not combined with SDCCHs */
                    case 0:

                        Parameters.TimeSlotInfo[0].Type = eTimeSlotType.BCCH_CCCH;
                        Parameters.TimeSlotInfo[0].Configures++;

                        for (int block = 1; block < 5; block++)
                        {
                            Parameters.TimeSlotHandlers[0][block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 2] = new sTimeSlotParam(CCCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 3] = new sTimeSlotParam(CCCH, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 4] = new sTimeSlotParam(CCCH, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 5] = new sTimeSlotParam(CCCH, 3);
                            Parameters.TimeSlotHandlers[0][block * 10 + 6] = new sTimeSlotParam(CCCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 7] = new sTimeSlotParam(CCCH, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 8] = new sTimeSlotParam(CCCH, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 9] = new sTimeSlotParam(CCCH, 3);
                        }
                        break;

                    /* 1 basic physical channel used for CCCH, combined with SDCCHs */
                    case 1:

                        Parameters.TimeSlotInfo[0].Type = eTimeSlotType.BCCH_CCCH_SDCCH4;
                        Parameters.TimeSlotInfo[0].Configures++;

                        /* setup the first block */
                        for (int block = 1; block < 2; block++)
                        {
                            Parameters.TimeSlotHandlers[0][block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 2] = new sTimeSlotParam(CCCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 3] = new sTimeSlotParam(CCCH, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 4] = new sTimeSlotParam(CCCH, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 5] = new sTimeSlotParam(CCCH, 3);
                            Parameters.TimeSlotHandlers[0][block * 10 + 6] = new sTimeSlotParam(CCCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 7] = new sTimeSlotParam(CCCH, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 8] = new sTimeSlotParam(CCCH, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 9] = new sTimeSlotParam(CCCH, 3);
                        }

                        /* then two SDCCH blocks */
                        for (int block = 2; block < 4; block++)
                        {
                            Parameters.TimeSlotHandlers[0][block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 1] = new sTimeSlotParam(SCH, 0);

                            SDCCHBurst tmpSDCCH1 = new SDCCHBurst(L3, (block - 2) * 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 2] = new sTimeSlotParam(tmpSDCCH1, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 3] = new sTimeSlotParam(tmpSDCCH1, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 4] = new sTimeSlotParam(tmpSDCCH1, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 5] = new sTimeSlotParam(tmpSDCCH1, 3);

                            SDCCHBurst tmpSDCCH2 = new SDCCHBurst(L3, (block - 2) * 2 + 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 6] = new sTimeSlotParam(tmpSDCCH2, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 7] = new sTimeSlotParam(tmpSDCCH2, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 8] = new sTimeSlotParam(tmpSDCCH2, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 9] = new sTimeSlotParam(tmpSDCCH2, 3);
                        }

                        /* finally one SACCH block */
                        for (int block = 4; block < 5; block++)
                        {
                            Parameters.TimeSlotHandlers[0][block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 1] = new sTimeSlotParam(SCH, 0);

                            SACCHBurst tmpSACCH1 = new SACCHBurst(L3, "SACCH 0/2", 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 2] = new sTimeSlotParam(tmpSACCH1, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 3] = new sTimeSlotParam(tmpSACCH1, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 4] = new sTimeSlotParam(tmpSACCH1, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 5] = new sTimeSlotParam(tmpSACCH1, 3);

                            SACCHBurst tmpSACCH2 = new SACCHBurst(L3, "SACCH 1/3", 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 6] = new sTimeSlotParam(tmpSACCH2, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 7] = new sTimeSlotParam(tmpSACCH2, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 8] = new sTimeSlotParam(tmpSACCH2, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 9] = new sTimeSlotParam(tmpSACCH2, 3);
                        }
                        break;

                    /* 2 basic physical channels used for CCCH, not combined with SDCCHs */
                    case 2:

                        Parameters.TimeSlotInfo[0].Type = eTimeSlotType.BCCH_CCCH;
                        Parameters.TimeSlotInfo[1].Type = eTimeSlotType.BCCH_CCCH;
                        Parameters.TimeSlotInfo[0].Configures++;
                        Parameters.TimeSlotInfo[1].Configures++;

                        /* timeslot 0 already has BCCH etc, so just fill that one */
                        for (int block = 1; block < 4; block++)
                        {
                            Parameters.TimeSlotHandlers[0][block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 2] = new sTimeSlotParam(CCCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 3] = new sTimeSlotParam(CCCH, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 4] = new sTimeSlotParam(CCCH, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 5] = new sTimeSlotParam(CCCH, 3);
                            Parameters.TimeSlotHandlers[0][block * 10 + 6] = new sTimeSlotParam(CCCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 7] = new sTimeSlotParam(CCCH, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 8] = new sTimeSlotParam(CCCH, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 9] = new sTimeSlotParam(CCCH, 3);
                        }

                        /* now fill with BCCH+CCCH */
                        for (int slot = 1; slot < 2; slot++)
                        {
                            Parameters.TimeSlotHandlers[slot][0] = new sTimeSlotParam(BCCH, 0);
                            Parameters.TimeSlotHandlers[slot][1] = new sTimeSlotParam(BCCH, 1);
                            Parameters.TimeSlotHandlers[slot][2] = new sTimeSlotParam(BCCH, 2);
                            Parameters.TimeSlotHandlers[slot][3] = new sTimeSlotParam(BCCH, 3);

                            for (int block = 1; block < 12; block++)
                            {
                                Parameters.TimeSlotHandlers[slot][block * 4 + 0] = new sTimeSlotParam(CCCH, 0);
                                Parameters.TimeSlotHandlers[slot][block * 4 + 1] = new sTimeSlotParam(CCCH, 1);
                                Parameters.TimeSlotHandlers[slot][block * 4 + 2] = new sTimeSlotParam(CCCH, 2);
                                Parameters.TimeSlotHandlers[slot][block * 4 + 3] = new sTimeSlotParam(CCCH, 3);
                            }
                        }

                        break;

                    /* 3 basic physical channels used for CCCH, not combined with SDCCHs */
                    case 3:

                        Parameters.TimeSlotInfo[0].Type = eTimeSlotType.BCCH_CCCH;
                        Parameters.TimeSlotInfo[1].Type = eTimeSlotType.BCCH_CCCH;
                        Parameters.TimeSlotInfo[2].Type = eTimeSlotType.BCCH_CCCH;
                        Parameters.TimeSlotInfo[0].Configures++;
                        Parameters.TimeSlotInfo[1].Configures++;
                        Parameters.TimeSlotInfo[2].Configures++;

                        /* timeslot 0 already has BCCH etc, so just fill that one */
                        for (int block = 1; block < 4; block++)
                        {
                            Parameters.TimeSlotHandlers[0][block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 2] = new sTimeSlotParam(CCCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 3] = new sTimeSlotParam(CCCH, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 4] = new sTimeSlotParam(CCCH, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 5] = new sTimeSlotParam(CCCH, 3);
                            Parameters.TimeSlotHandlers[0][block * 10 + 6] = new sTimeSlotParam(CCCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 7] = new sTimeSlotParam(CCCH, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 8] = new sTimeSlotParam(CCCH, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 9] = new sTimeSlotParam(CCCH, 3);
                        }

                        /* now fill with BCCH+CCCH */
                        for (int slot = 1; slot < 3; slot++)
                        {
                            Parameters.TimeSlotHandlers[slot][0] = new sTimeSlotParam(BCCH, 0);
                            Parameters.TimeSlotHandlers[slot][1] = new sTimeSlotParam(BCCH, 1);
                            Parameters.TimeSlotHandlers[slot][2] = new sTimeSlotParam(BCCH, 2);
                            Parameters.TimeSlotHandlers[slot][3] = new sTimeSlotParam(BCCH, 3);

                            for (int block = 1; block < 12; block++)
                            {
                                Parameters.TimeSlotHandlers[slot][block * 4 + 0] = new sTimeSlotParam(CCCH, 0);
                                Parameters.TimeSlotHandlers[slot][block * 4 + 1] = new sTimeSlotParam(CCCH, 1);
                                Parameters.TimeSlotHandlers[slot][block * 4 + 2] = new sTimeSlotParam(CCCH, 2);
                                Parameters.TimeSlotHandlers[slot][block * 4 + 3] = new sTimeSlotParam(CCCH, 3);
                            }
                        }

                        break;

                    /* 4 basic physical channels used for CCCH, not combined with SDCCHs */
                    case 4:

                        Parameters.TimeSlotInfo[0].Type = eTimeSlotType.BCCH_CCCH;
                        Parameters.TimeSlotInfo[1].Type = eTimeSlotType.BCCH_CCCH;
                        Parameters.TimeSlotInfo[2].Type = eTimeSlotType.BCCH_CCCH;
                        Parameters.TimeSlotInfo[3].Type = eTimeSlotType.BCCH_CCCH;
                        Parameters.TimeSlotInfo[0].Configures++;
                        Parameters.TimeSlotInfo[1].Configures++;
                        Parameters.TimeSlotInfo[2].Configures++;
                        Parameters.TimeSlotInfo[3].Configures++;


                        /* timeslot 0 already has BCCH etc, so just fill that one */
                        for (int block = 1; block < 4; block++)
                        {
                            Parameters.TimeSlotHandlers[0][block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 2] = new sTimeSlotParam(CCCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 3] = new sTimeSlotParam(CCCH, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 4] = new sTimeSlotParam(CCCH, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 5] = new sTimeSlotParam(CCCH, 3);
                            Parameters.TimeSlotHandlers[0][block * 10 + 6] = new sTimeSlotParam(CCCH, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 7] = new sTimeSlotParam(CCCH, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 8] = new sTimeSlotParam(CCCH, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 9] = new sTimeSlotParam(CCCH, 3);
                        }

                        /* now fill with BCCH+CCCH */
                        for (int slot = 1; slot < 4; slot++)
                        {
                            Parameters.TimeSlotHandlers[slot][0] = new sTimeSlotParam(BCCH, 0);
                            Parameters.TimeSlotHandlers[slot][1] = new sTimeSlotParam(BCCH, 1);
                            Parameters.TimeSlotHandlers[slot][2] = new sTimeSlotParam(BCCH, 2);
                            Parameters.TimeSlotHandlers[slot][3] = new sTimeSlotParam(BCCH, 3);

                            for (int block = 1; block < 12; block++)
                            {
                                Parameters.TimeSlotHandlers[slot][block * 4 + 0] = new sTimeSlotParam(CCCH, 0);
                                Parameters.TimeSlotHandlers[slot][block * 4 + 1] = new sTimeSlotParam(CCCH, 1);
                                Parameters.TimeSlotHandlers[slot][block * 4 + 2] = new sTimeSlotParam(CCCH, 2);
                                Parameters.TimeSlotHandlers[slot][block * 4 + 3] = new sTimeSlotParam(CCCH, 3);
                            }
                        }

                        break;

                    default:
                        AddMessage("   [L1] This particular BCCH/CCCH configuration is not handled yet!" + Environment.NewLine);
                        break;
                }
            }
        }


        public void Handle(double[] timeSlotSamples)
        {
            if (AddMessage == null)
                return;

            Parameters.TN++;
            Parameters.TN %= 8;
            if (Parameters.TN == 0)
                Parameters.FN++;

            /* get the burst handler */
            Burst handler = null;
            int sequence = 0;
            long frameNum = 0;
            bool handlerFailed = false;

            lock (Parameters.TimeSlotHandlers)
            {
                if (Parameters.TimeSlotHandlers[Parameters.TN] == null)
                {
                    handler = null;
                    sequence = 0;
                }
                else if (Parameters.TimeSlotHandlers[Parameters.TN].Length == 51)
                {
                    frameNum = Parameters.T3;
                    handler = Parameters.TimeSlotHandlers[Parameters.TN][Parameters.T3].Burst; // control frame number
                    sequence = Parameters.TimeSlotHandlers[Parameters.TN][Parameters.T3].Sequence;
                }
                else if (Parameters.TimeSlotHandlers[Parameters.TN].Length == 26)
                {
                    frameNum = Parameters.T2;
                    handler = Parameters.TimeSlotHandlers[Parameters.TN][Parameters.T2].Burst; // traffic frame number
                    sequence = Parameters.TimeSlotHandlers[Parameters.TN][Parameters.T2].Sequence;
                }
            }

            if (Burst.DumpRawData)
            {
                if (handler != null)
                    AddMessage("   [L1] Handler: " + handler.Name + "[" + sequence + "]  TN:" + Parameters.TN + "  Frame: " + frameNum + Environment.NewLine);
                else
                    AddMessage("   [L1] Handler: (none)  TN:" + Parameters.TN + "  Frame: " + frameNum + Environment.NewLine);
            }

            Burst.eSuccessState rawHandlerState = Burst.eSuccessState.Unknown;
            Burst.eSuccessState dataHandlerState = Burst.eSuccessState.Unknown;

            /* let the raw handler work with the data */
            if (handler != null)
                rawHandlerState = handler.ParseRawBurst(Parameters, timeSlotSamples);

            /* apply offset, if any */
            Decoder.SampleOffset = Parameters.SampleStartPosition + Parameters.SampleOffset + Parameters.SubSampleOffset;

            /* continue to decode the packet */
            Decoder.Decode(timeSlotSamples, BurstBits);
            DifferenceCode.Decode(BurstBits, BurstBitsUndiffed);

            if (handler != null)
            {

                /* only run data handler if raw handler didnt fail */
                if (rawHandlerState != Burst.eSuccessState.Failed)
                    dataHandlerState = handler.ParseData(Parameters, BurstBitsUndiffed, sequence);

                /* if everything went ok so far, pass the data to the associated handler */
                if (rawHandlerState == Burst.eSuccessState.Failed || dataHandlerState == Burst.eSuccessState.Failed)
                {
                    Parameters.Error();
                    AddMessage("   [L1] [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);
                    AddMessage("        ERROR: " + handler.ErrorMessage + Environment.NewLine);
                    AddMessage(Environment.NewLine);
                }
                else
                {
                    /* only update success counters when this burst was processed successfully */
                    if (dataHandlerState == Burst.eSuccessState.Succeeded)
                        Parameters.Success();

                    /* show L2 messages, if handler wishes */
                    bool showL2 = handler.L2.ShowMessage && !string.IsNullOrEmpty(handler.L2.StatusMessage);

                    /* show L3 messages, if there is any */
                    bool showL3 = !string.IsNullOrEmpty(handler.L3.StatusMessage);

                    /* only show L1 when one of L2/L3 has a message */
                    if (showL2 || showL3 || handler.StatusMessage != null)
                        AddMessage("   [L1] [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);

                    if (handler.StatusMessage != null)
                    {
                        AddMessage("        " + handler.StatusMessage + Environment.NewLine);
                        AddMessage(Environment.NewLine);
                    }

                    /* show L2 if L2 wants to, or if L3 has some message */
                    if (showL3 || showL2)
                        AddMessage("   [L2] " + handler.L2.StatusMessage);

                    /* L3 handler has a message to show */
                    if (showL3)
                        AddMessage("   [L3] " + handler.L3.StatusMessage);

                    /* if something shown, add newline */
                    if (showL2 || showL3)
                        AddMessage(Environment.NewLine);
                }

                /* and reset these "flags" */
                handler.StatusMessage = null;
                handler.ErrorMessage = null;
                handler.L2.ShowMessage = false;
                handler.L3.StatusMessage = null;

                /* does the Layer 3 provide any sniffed information? */
                if (handler.L3.SniffResult != null)
                {
                    AddMessage("   [L3] sniffed: " + handler.L3.SniffResult + Environment.NewLine);
                    handler.L3.SniffResult = null;
                }
            }
        }
    }
}