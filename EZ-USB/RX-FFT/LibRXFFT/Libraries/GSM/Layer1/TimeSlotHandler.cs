using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Bursts;
using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.GSM.Layer1
{
    public delegate void AddMessageDelegate(string msg);

    public class TimeSlotHandler
    {
        public static bool PreallocateTCHs = false;

        private readonly double BT;
        public readonly double Oversampling;


        private readonly byte[] TrainingCode = new byte[] { 0xB9, 0x62, 0x04, 0x0F, 0x2D, 0x45, 0x76, 0x1B };
        private readonly double[] TrainingSequence;
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


        public TimeSlotHandler(double oversampling, double bt, AddMessageDelegate addMessage, GSMParameters param)
        {
            Oversampling = oversampling;
            BT = bt;
            AddMessage = addMessage;
            Parameters = param;

            Decoder = new GMSKDecoder(Oversampling, BT);

            L3 = new L3Handler();
            FCH = new FCHBurst();
            SCH = new SCHBurst();
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

            /* create training sequence ... */
            double[] tmpTrainingSequence = new SequenceGenerator(Oversampling, BT).GenerateDiffEncoded(TrainingCode);

            /* ... and skip the first and last two samples since these are affected by the bits before */
            TrainingSequence = new double[(int)(tmpTrainingSequence.Length - 4 * Oversampling)];
            Array.Copy(tmpTrainingSequence, (int)(2 * Oversampling), TrainingSequence, 0, TrainingSequence.Length);
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

        private void HandleFCH(double[] timeSlotSamples)
        {
            /* dont use all bits. skip 4 bits at the start and 4 at the end */
            int bits = (int)FCHBurst.PayloadBits - 8;
            int startPos = (int)(Oversampling * 4);
            int samples = (int)(Oversampling * bits);

            double avg = 0;
            for (int pos = startPos; pos < samples + startPos; pos++)
                avg += timeSlotSamples[Decoder.StartOffset + pos];
            avg /= bits;

            /* should have +PI/2 per high bit. calculate phase correction value per sample */
            double phaseOffset = (Math.PI / 2 - avg) / Oversampling;

            /* set offset */
            if (Parameters.PhaseAutoOffset)
                Parameters.PhaseOffsetValue += phaseOffset;


#if false
            /* prevent division by zero (1Hz error is really not worth mentioning) */
            if (avg == 0)
                avg = 1;

            /* update average if the difference is below 200Hz and it didn't vary by more than 100% */
            if (Math.Abs(Parameters.FCCHOffset - avg) < 200 || (Math.Abs(Parameters.FCCHOffset / avg) <= 2 && Math.Abs(Parameters.FCCHOffset / avg) >= 0.5))
            {
                avg += 49 * Parameters.FCCHOffset;
                avg /= 50;
            }
#endif
        }

        private bool HandleSCHTrain(double[] timeSlotSamples)
        {
            string message = Parameters + "   [SCH] ";
            int bitTolerance = 3;

            if (Parameters.FirstSCH)
                bitTolerance = 8;

            /* skip the number of data bits defined in SCHBurst plus SpareBits that are "pre"-feeded */
            int sequencePos = (int)(Oversampling * (SCHBurst.SyncOffset + 2 + SpareBits));

            /* locate the training sequence over two bits */
            int position = SignalPower.Locate(timeSlotSamples, sequencePos, TrainingSequence, (int)(Oversampling * bitTolerance));
            if (position == int.MinValue)
            {
                message += "(Error in SignalPower.Locate)" + Environment.NewLine;
                AddMessage(message);
                Parameters.SampleOffset = int.MinValue;
                Parameters.Errors++;
                return false;
            }

            /* calculate the offset between guessed position and real */
            Parameters.SampleOffset = position - sequencePos;

            Decoder.StartOffset += (int)Parameters.SampleOffset;
            Decoder.SubSampleOffset = 0;/* OffsetEstimator.EstimateOffset(timeSlotSamples,
                                (int)(Decoder.StartOffset + Oversampling / 2 + 5 * Oversampling),
                                (int)((Burst.NetBitCount - 5) * Oversampling),
                                Oversampling);*/


            return true;
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

            if (Parameters.DumpPackets)
            {
                if (handler != null)
                    AddMessage("   [L1] Handler: " + handler.Name + "[" + sequence + "]  TN:" + Parameters.TN + "  Frame: " + frameNum + Environment.NewLine);
                else
                    AddMessage("   [L1] Handler: (none)  TN:" + Parameters.TN + "  Frame: " + frameNum + Environment.NewLine);
            }

            /* this is a SCH burst */
            if (Parameters.FirstSCH || handler == SCH)
            {
                Parameters.FirstSCH = false;

                /* try to detect sequence and update sampling offset */
                if (!HandleSCHTrain(timeSlotSamples))
                    return;
            }

            if (handler == FCH)
            {
                HandleFCH(timeSlotSamples);
            }

            /* continue to decode the packet */
            Decoder.Decode(timeSlotSamples, BurstBits);

            if (handler != null)
            {
                DifferenceCode.Decode(BurstBits, BurstBitsUndiffed);

                /* check the first and last three bits to be low. thats required for all bursts from the BTS. */
                /*
                if (burstBitsUndiffed[0] || burstBitsUndiffed[1] || burstBitsUndiffed[2] || burstBitsUndiffed[145] || burstBitsUndiffed[146] || burstBitsUndiffed[147])
                {
                    AddMessage("   [GMSK] Delimiter bits are not low (" + Parameters + ")" + Environment.NewLine);
                    Parameters.Error = true;
                    return;
                }
                */

                /* if everything went ok so far, pass the data to the associated handler */
                if (!handler.ParseData(Parameters, BurstBitsUndiffed, sequence))
                {
                    AddMessage("   [L1] [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);
                    AddMessage("        ERROR: " + handler.ErrorMessage + Environment.NewLine);
                    AddMessage(Environment.NewLine);
                }
                else
                {
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