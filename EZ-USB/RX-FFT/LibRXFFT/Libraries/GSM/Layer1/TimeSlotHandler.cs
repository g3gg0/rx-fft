using System;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;
using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;
using LibRXFFT.Libraries.GSM.Layer1.GMSK;
using LibRXFFT.Libraries.GSM.Layer3;
using System.Collections.Generic;
using RX_FFT.Components.GDI;
using System.Threading;

namespace LibRXFFT.Libraries.GSM.Layer1
{
    public delegate void AddMessageDelegate(string msg);

    public class TimeSlotHandler
    {
        public static bool PreallocateTCHs = false;
        public static bool PreallocateSDCCHs = false;

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

        public static GSMParameters _HACK_Parameters;

        public int SpareBits = 3;


        public TimeSlotHandler(GSMParameters param, AddMessageDelegate addMessage)
        {
            Oversampling = param.Oversampling;
            BT = param.BT;
            AddMessage = addMessage;
            Parameters = param;

            _HACK_Parameters = param;

            Decoder = new GMSKDecoder(Oversampling, BT);

            L3 = new L3Handler();
            FCH = new FCHBurst(param);
            SCH = new SCHBurst(param);
            BCCH = new BCCHBurst(L3);
            CCCH = new CCCHBurst(L3);

            Parameters.UsedBursts.AddLast(BCCH);
            Parameters.UsedBursts.AddLast(CCCH);

            L3.PDUDataTriggers.Add("ServiceRequest", TriggerServiceRequest);
            L3.PDUDataTriggers.Add("LocationUpdateTypeSet", TriggerLocationUpdateRequest);            
            L3.PDUDataTriggers.Add("CCCH-CONF", TriggerCCCHCONF);
            L3.PDUDataTriggers.Add("ChannelAssignment", TriggerChannelAssignment);
            L3.PDUDataTriggers.Add("CBCHUpdate", TriggerCBCHUpdate);
            L3.PDUDataTriggers.Add("CBCHReset", TriggerCBCHReset);
            L3.PDUDataTriggers.Add("CipherCommand", TriggerCipherCommand);

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

                    tch.AssociatedSACCH = sacch;
                    sacch.AssociatedTCH = tch;
                    Parameters.UsedBursts.AddLast(tch);
                    Parameters.UsedBursts.AddLast(sacch);

                    for (int frame = 0; frame < 25; frame++)
                    {
                        if (frame == 12)
                            Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(sacch, 0);
                        else
                            Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(tch, 0);
                    }
                }
            }

            if (PreallocateSDCCHs)
            {
                for (int timeSlot = 1; timeSlot < 8; timeSlot++)
                {

                    /* try all subchannels for SDCCH, since it's not saved yet in .gad file */
                    for (int subChannel = 0; subChannel < 8; subChannel++)
                    {

                        /* do all things with existing code */

                        /* remove all old values, we do not want duplicates */
                        L3.PDUDataRawFields.Remove("ChannelType");
                        L3.PDUDataRawFields.Remove("SubChannel");
                        L3.PDUDataRawFields.Remove("TimeSlot");
                        L3.PDUDataRawFields.Remove("RefT1");
                        L3.PDUDataRawFields.Remove("RefT2");
                        L3.PDUDataRawFields.Remove("RefT3");

                        L3.PDUDataRawFields.Add("ChannelType", (long)eTimeSlotType.SDCCH8);

                        L3.PDUDataRawFields.Add("SubChannel", subChannel);

                        /* this should be probably equal to t-attribut of .gad file */
                        L3.PDUDataRawFields.Add("TimeSlot", timeSlot);

                        /* this should be different to 0, so it will setup */
                        L3.PDUDataRawFields.Add("RefT1", 1);
                        L3.PDUDataRawFields.Add("RefT2", subChannel);
                        L3.PDUDataRawFields.Add("RefT3", timeSlot);

                        /* setup handler now */
                        TriggerChannelAssignment(L3);
                    }
                }
            }

        }

        private void UnregisterActiveBursts(sTimeSlotParam[] sTimeSlotParam)
        {
            if (sTimeSlotParam == null)
            {
                return;
            }

            foreach (sTimeSlotParam parm in sTimeSlotParam)
            {
                if (parm.Burst is NormalBurst)
                {
                    UnregisterActiveBurst((NormalBurst)parm.Burst);
                }
            }
        }

        private void RegisterActiveBurst(NormalBurst burst)
        {
            if (burst != null && !Parameters.ActiveBursts.Contains(burst))
            {
                /* call LUA script */
                if (Parameters.LuaVm != null)
                {
                    LuaHelpers.CallFunction(Parameters.LuaVm, "RegisterActiveBurst", true, burst, Parameters);
                }

                Parameters.ActiveBursts.AddLast(burst);
                Parameters.UsedBursts.AddLast(burst);
            }
        }

        private void UnregisterActiveBurst(NormalBurst burst)
        {
            if (burst != null && Parameters.ActiveBursts.Contains(burst))
            {
                /* call LUA script */
                if (Parameters.LuaVm != null)
                {
                    LuaHelpers.CallFunction(Parameters.LuaVm, "UnregisterActiveBurst", true, burst, Parameters);
                }

                Parameters.ActiveBursts.Remove(burst);
            }
        }

        private void ReleaseBurst(Burst burst)
        {
            if (burst != null && !burst.Released && burst is NormalBurst)
            {
                UnregisterActiveBurst((NormalBurst)burst);
                burst.Release();
            }
        }

        private int GetFrameNumForChannel(eTimeSlotType slotType, eChannelType type, int channelNum)
        {
            int[] framePosSDCCHinSDCCH8 = new[] { 0, 4, 8, 12, 16, 20, 24, 32 };

            switch (slotType)
            {
                case eTimeSlotType.BCCH_CCCH:
                    break;
                case eTimeSlotType.BCCH_CCCH_SDCCH4:
                    switch (type)
                    {
                        case eChannelType.SDCCH:
                            if (channelNum >= 0 && channelNum <= 3)
                            {
                                int[] framePosSDCCHinBCCH = new[] { 22, 26, 32, 36 };
                                return framePosSDCCHinBCCH[channelNum];
                            }
                            break;
                        case eChannelType.SACCH:
                            if (channelNum >= 0 && channelNum <= 3)
                            {
                                int[] framePosSACCHinBCCH = new[] { 42, 46, 42, 46 };
                                return framePosSACCHinBCCH[channelNum];
                            }
                            break;
                    }
                    break;

                case eTimeSlotType.SDCCH8:
                    switch (type)
                    {
                        case eChannelType.SDCCH:
                            if (channelNum >= 0 && channelNum <= 7)
                            {
                                return channelNum * 4;
                            }
                            break;
                        case eChannelType.SACCH:
                            if (channelNum >= 0 && channelNum <= 7)
                            {
                                return 32 + ((channelNum % 4) * 4);
                            }
                            break;
                    }
                    break;
            }

            throw new NotSupportedException("requested frame position, but not implemented yet");
        }


        private void TriggerChannelAssignment(L3Handler L3Handler)
        {

            eTimeSlotType channelType;
            long subChannel;
            long timeSlot;
            sTimeslotReference reference;
            bool hopping = false;

            lock (L3Handler.PDUDataFields)
            {
                if (!L3Handler.PDUDataRawFields.ContainsKey("ChannelType"))
                    return;
                if (!L3Handler.PDUDataRawFields.ContainsKey("SubChannel"))
                    return;
                if (!L3Handler.PDUDataRawFields.ContainsKey("TimeSlot"))
                    return;
                if (!L3Handler.PDUDataRawFields.ContainsKey("RefT1"))
                    return;

                if (!L3Handler.PDUDataRawFields.ContainsKey("RefT2"))
                    return;

                if (!L3Handler.PDUDataRawFields.ContainsKey("RefT3"))
                    return;

                if (L3Handler.PDUDataRawFields.ContainsKey("Hopping"))
                {
                    hopping = (L3Handler.PDUDataRawFields["Hopping"] == 1);
                }

                channelType = (eTimeSlotType)L3Handler.PDUDataRawFields["ChannelType"];
                subChannel = L3Handler.PDUDataRawFields["SubChannel"];
                timeSlot = L3Handler.PDUDataRawFields["TimeSlot"];

                reference.T1 = L3Handler.PDUDataRawFields["RefT1"];
                reference.T2 = L3Handler.PDUDataRawFields["RefT2"];
                reference.T3 = L3Handler.PDUDataRawFields["RefT3"];
            }

            /* was: assigned time slot type does not match? */
            /* now: make sure thats no hopping channel. no chance yet to decode */

            if (!hopping /*|| Parameters.TimeSlotHandlers[timeSlot] == null || Parameters.TimeSlotInfo[timeSlot].Type != channelType*/)
            {
                lock (Parameters.TimeSlotHandlers)
                {
                    switch (channelType)
                    {
                        case eTimeSlotType.TCHF:
                            if (Parameters.TimeSlotHandlers[timeSlot] != null && Parameters.TimeSlotHandlers[timeSlot][0].Reference == reference)
                            {
                                //AddMessage("   [L1] TimeSlot " + timeSlot + " already configured as TCH/F (was " + Parameters.TimeSlotInfo[timeSlot].Type + ")" + Environment.NewLine);
                            }
                            else
                            {
                                if (Parameters.TimeSlotHandlers[timeSlot] == null)
                                {
                                    Parameters.TimeSlotHandlers[timeSlot] = new sTimeSlotParam[26];
                                }

                                /* release the old bursts first */
                                for (int frame = 0; frame < Parameters.TimeSlotHandlers[timeSlot].Length; frame++)
                                {
                                    Burst burst = Parameters.TimeSlotHandlers[timeSlot][frame].Burst;
                                    ReleaseBurst(burst);
                                }

                                AddMessage("  [__] TimeSlot " + timeSlot + " now configured as TCH/F (was " + Parameters.TimeSlotInfo[timeSlot].Type + ")" + Environment.NewLine);

                                TCHBurst tch = new TCHBurst(L3, "TCH" + timeSlot + "/F", (int)timeSlot);
                                SACCHBurst sacch = new SACCHBurst(L3, "SACCH/TCH" + timeSlot, (int)timeSlot, true);

                                if (L3Handler.PDUDataRawFields.ContainsKey("ChannelMode"))
                                {
                                    int channelMode = (int)L3Handler.PDUDataRawFields["ChannelMode"];
                                    tch.ChannelMode = channelMode;
                                }

                                tch.TimeSlot = timeSlot;
                                sacch.TimeSlot = timeSlot;

                                RegisterActiveBurst(tch);
                                RegisterActiveBurst(sacch);

                                tch.AssociatedSACCH = sacch;
                                sacch.AssociatedTCH = tch;

                                /* this should be true in any case */
                                if (Parameters.CurrentBurstHandler is SDCCHBurst)
                                {
                                    SDCCHBurst sdcch = (SDCCHBurst)Parameters.CurrentBurstHandler;

                                    /* check if this connection had a key predefined and copy it */
                                    if (sdcch.ChannelEncrypted)
                                    {
                                        AddMessage("  [__] Using cipher key from associated SDCCH connection" + Environment.NewLine);
                                        tch.A5Algorithm = sdcch.A5Algorithm;
                                        tch.A5CipherKey = sdcch.A5CipherKey;
                                        tch.ChannelEncrypted = true;

                                        sacch.A5Algorithm = sdcch.A5Algorithm;
                                        sacch.A5CipherKey = sdcch.A5CipherKey;
                                        sacch.ChannelEncrypted = true;
                                    }
                                }

                                if (Parameters.CurrentBurstHandler is NormalBurst && ((NormalBurst)Parameters.CurrentBurstHandler).ChannelEncrypted)
                                {
                                    tch.ChannelEncrypted = true;
                                    sacch.ChannelEncrypted = true;
                                }

                                for (int frame = 0; frame < 25; frame++)
                                {
                                    if (frame == 12)
                                        Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(sacch, 0);
                                    else
                                        Parameters.TimeSlotHandlers[timeSlot][frame] = new sTimeSlotParam(tch, 0);

                                    Parameters.TimeSlotHandlers[timeSlot][frame].Reference = reference;
                                }
                            }
                            break;

                        case eTimeSlotType.TCHH:
                            /* TODO: this is wrong! have to check subchannel reference! */
                            if (Parameters.TimeSlotHandlers[timeSlot] != null && Parameters.TimeSlotHandlers[timeSlot][0].Reference == reference)
                            {
                                //AddMessage("   [L1] TimeSlot " + timeSlot + " already configured as TCH/H (was " + Parameters.TimeSlotInfo[timeSlot].Type + ")" + Environment.NewLine);
                            }
                            else
                            {
                                UnregisterActiveBursts(Parameters.TimeSlotHandlers[timeSlot]);

                                if (Parameters.TimeSlotHandlers[timeSlot] == null)
                                {
                                    Parameters.TimeSlotHandlers[timeSlot] = new sTimeSlotParam[26];
                                }

                                /* make sure the old bursts get released */
                                ReleaseTimeSlotBursts(timeSlot);

                                /* if length did not fit, reallocate */
                                if (Parameters.TimeSlotHandlers.Length != 26)
                                {
                                    Parameters.TimeSlotHandlers[timeSlot] = new sTimeSlotParam[26];
                                }
                                
                                AddMessage("  [__] TimeSlot " + timeSlot + " now configured as TCH/H (was " + Parameters.TimeSlotInfo[timeSlot].Type + ")" + Environment.NewLine);

                                TCHBurst tch1 = new TCHBurst(L3, "TCH" + timeSlot + "/H 1", (int)timeSlot);
                                TCHBurst tch2 = new TCHBurst(L3, "TCH" + timeSlot + "/H 2", (int)timeSlot);
                                SACCHBurst sacch1 = new SACCHBurst(L3, "SACCH1/TCH" + timeSlot, (int)timeSlot, true);
                                SACCHBurst sacch2 = new SACCHBurst(L3, "SACCH2/TCH" + timeSlot, (int)timeSlot, true);

                                if (L3Handler.PDUDataRawFields.ContainsKey("ChannelMode"))
                                {
                                    int channelMode = (int)L3Handler.PDUDataRawFields["ChannelMode"];
                                    tch1.ChannelMode = channelMode;
                                    tch2.ChannelMode = channelMode;
                                }

                                tch1.TimeSlot = timeSlot;
                                tch2.TimeSlot = timeSlot;
                                sacch1.TimeSlot = timeSlot;
                                sacch2.TimeSlot = timeSlot;

                                RegisterActiveBurst(tch1);
                                RegisterActiveBurst(tch2);
                                RegisterActiveBurst(sacch1);
                                RegisterActiveBurst(sacch2);

                                tch1.AssociatedSACCH = sacch1;
                                sacch1.AssociatedTCH = tch1;
                                tch2.AssociatedSACCH = sacch2;
                                sacch2.AssociatedTCH = tch2;

                                /* this should be true in any case */
                                if (Parameters.CurrentBurstHandler is SDCCHBurst)
                                {
                                    SDCCHBurst sdcch = (SDCCHBurst)Parameters.CurrentBurstHandler;

                                    /* check if this connection had a key predefined and copy it */
                                    if (sdcch.ChannelEncrypted)
                                    {
                                        AddMessage("  [__] Using cipher key from associated SDCCH connection" + Environment.NewLine);
                                        tch1.A5Algorithm = sdcch.A5Algorithm;
                                        tch1.A5CipherKey = sdcch.A5CipherKey;
                                        tch1.ChannelEncrypted = true;
                                        sacch1.A5Algorithm = sdcch.A5Algorithm;
                                        sacch1.A5CipherKey = sdcch.A5CipherKey;
                                        sacch1.ChannelEncrypted = true;
                                        tch2.A5Algorithm = sdcch.A5Algorithm;
                                        tch2.A5CipherKey = sdcch.A5CipherKey;
                                        tch2.ChannelEncrypted = true;
                                        sacch2.A5Algorithm = sdcch.A5Algorithm;
                                        sacch2.A5CipherKey = sdcch.A5CipherKey;
                                        sacch2.ChannelEncrypted = true;
                                    }
                                }


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

                                    Parameters.TimeSlotHandlers[timeSlot][frame].Reference = reference;
                                }
                            }
                            break;

                        case eTimeSlotType.SDCCH8:
                            if (timeSlot != 0)
                            {
#if false
                                AddMessage("   [L1] TimeSlot " + timeSlot + " now configured as SDCCH/8 (was " + Parameters.TimeSlotInfo[timeSlot].Type + ")" + Environment.NewLine);
                                Parameters.TimeSlotHandlers[timeSlot] = new sTimeSlotParam[51];

                                /* 8 SDCCHs */
                                for (int chan = 0; chan < 8; chan++)
                                {
                                    SDCCHBurst tmpSDCCH = new SDCCHBurst(L3, chan);
                                    Parameters.UsedBursts.AddLast(tmpSDCCH);
                                    Parameters.TimeSlotHandlers[timeSlot][chan * 4 + 0] = new sTimeSlotParam(tmpSDCCH, 0);
                                    Parameters.TimeSlotHandlers[timeSlot][chan * 4 + 1] = new sTimeSlotParam(tmpSDCCH, 1);
                                    Parameters.TimeSlotHandlers[timeSlot][chan * 4 + 2] = new sTimeSlotParam(tmpSDCCH, 2);
                                    Parameters.TimeSlotHandlers[timeSlot][chan * 4 + 3] = new sTimeSlotParam(tmpSDCCH, 3);
                                }

                                /* finally 4 SACCHs */
                                for (int chan = 0; chan < 4; chan++)
                                {
                                    SACCHBurst tmpSACCH = new SACCHBurst(L3, "SACCH " + chan + "/" + (chan + 4), chan);
                                    Parameters.UsedBursts.AddLast(tmpSACCH);
                                    Parameters.TimeSlotHandlers[timeSlot][(chan + 8) * 4 + 0] = new sTimeSlotParam(tmpSACCH, 0);
                                    Parameters.TimeSlotHandlers[timeSlot][(chan + 8) * 4 + 1] = new sTimeSlotParam(tmpSACCH, 1);
                                    Parameters.TimeSlotHandlers[timeSlot][(chan + 8) * 4 + 2] = new sTimeSlotParam(tmpSACCH, 2);
                                    Parameters.TimeSlotHandlers[timeSlot][(chan + 8) * 4 + 3] = new sTimeSlotParam(tmpSACCH, 3);
                                }
#else

                                int sdcchFrame = GetFrameNumForChannel(channelType, eChannelType.SDCCH, (int)subChannel);
                                int sacchFrame = GetFrameNumForChannel(channelType, eChannelType.SACCH, (int)subChannel);

                                if (Parameters.TimeSlotHandlers[timeSlot] == null || Parameters.TimeSlotHandlers[timeSlot].Length != 51)
                                {
                                    Parameters.TimeSlotHandlers[timeSlot] = new sTimeSlotParam[51];
                                }

                                if (Parameters.TimeSlotHandlers[timeSlot][sdcchFrame].Reference == reference)
                                {
                                    //AddMessage("   [L1] TimeSlot " + timeSlot + " already configured as SDCCH/8 (was " + Parameters.TimeSlotInfo[timeSlot].Type + ")" + Environment.NewLine);
                                }
                                else
                                {
                                    AddMessage("  [__] TimeSlot " + timeSlot + " now configured as SDCCH/8 (was " + Parameters.TimeSlotInfo[timeSlot].Type + ")" + Environment.NewLine);
                                    int sacchChannel1 = (int)subChannel / 2;
                                    int sacchChannel2 = sacchChannel1 + 4;

                                    /* release the old burst first */
                                    Burst burst = Parameters.TimeSlotHandlers[timeSlot][sdcchFrame].Burst;
                                    ReleaseBurst(burst);

                                    /* we are releasing a SACCH even if it may be still used by the other SDCCH using it */
                                    burst = Parameters.TimeSlotHandlers[timeSlot][sacchFrame].Burst;
                                    ReleaseBurst(burst);

                                    /* allocate a new SDCCH */
                                    SDCCHBurst tmpSDCCH = new SDCCHBurst(L3, (int)subChannel);

                                    tmpSDCCH.TimeSlot = timeSlot;
                                    RegisterActiveBurst(tmpSDCCH);

                                    if (L3Handler.PDUDataFields.ContainsKey("EstablishmentCause"))
                                    {
                                        tmpSDCCH.EstablishmentCause = L3Handler.PDUDataFields["EstablishmentCause"];
                                        tmpSDCCH.ServiceType = "(none)";

                                        /* remove this info to prevent false detection for later causes */
                                        L3Handler.PDUDataFields.Remove("EstablishmentCause");
                                    }

                                    int sequence = 0;
                                    for (int pos = sdcchFrame; pos < sdcchFrame + 4; pos++)
                                    {
                                        Parameters.TimeSlotHandlers[timeSlot][pos] = new sTimeSlotParam(tmpSDCCH, sequence++);
                                        Parameters.TimeSlotHandlers[timeSlot][pos].Reference = reference;
                                    }

                                    /* we are allocating a new SACCH even if it may be used by the other SDCCH using it */
                                    SACCHBurst tmpSACCH = new SACCHBurst(L3, "SACCH " + sacchChannel1 + "/" + sacchChannel2, (int)sacchChannel1);

                                    tmpSACCH.TimeSlot = timeSlot;
                                    RegisterActiveBurst(tmpSACCH);

                                    sequence = 0;
                                    for (int pos = sacchFrame; pos < sacchFrame + 4; pos++)
                                    {
                                        Parameters.TimeSlotHandlers[timeSlot][pos] = new sTimeSlotParam(tmpSACCH, sequence++);
                                        Parameters.TimeSlotHandlers[timeSlot][pos].Reference = reference;
                                    }

                                    tmpSDCCH.AssociatedSACCH = tmpSACCH;
                                }
#endif
                            }
                            else
                            {
                                AddMessage("  [__] TimeSlot " + timeSlot + " NOT configured for SDCCH/8 as requested. Stays " + Parameters.TimeSlotInfo[timeSlot].Type + Environment.NewLine);
                            }
                            break;

                        case eTimeSlotType.BCCH_CCCH_SDCCH4:
                            int framePos = GetFrameNumForChannel(channelType, eChannelType.SDCCH, (int)subChannel);

                            if (Parameters.TimeSlotHandlers[timeSlot] != null && (framePos < Parameters.TimeSlotHandlers[timeSlot].Length))
                            {
                                AddMessage("  [__] TimeSlot " + timeSlot + " SDCCH subchan " + subChannel + " assigned" + Environment.NewLine);

                                Burst burst = Parameters.TimeSlotHandlers[timeSlot][framePos].Burst;

                                /* safety measure */
                                if (!(burst is NormalBurst))
                                {
                                    /* wait... what?! */
                                    AddMessage("  [EE] TimeSlot " + timeSlot + " SDCCH subchan " + subChannel + " expected" + Environment.NewLine);
                                    break;
                                }
                                NormalBurst assigned = (NormalBurst)burst;

                                if (L3Handler.PDUDataFields.ContainsKey("EstablishmentCause"))
                                {
                                    assigned.EstablishmentCause = L3Handler.PDUDataFields["EstablishmentCause"];
                                    assigned.ServiceType = "(none)";

                                    /* remove this info to prevent false detection for later causes */
                                    L3Handler.PDUDataFields.Remove("EstablishmentCause");
                                }
                            }
                            else
                            {
                                AddMessage("  [__] TimeSlot " + timeSlot + " cannot get configured. Type: " + channelType + Environment.NewLine);
                            }
                            break;

                        default:
                            AddMessage("  [__] TimeSlot " + timeSlot + " cannot get configured. Type: " + channelType + Environment.NewLine);
                            break;
                    }

                    Parameters.TimeSlotInfo[timeSlot].Type = channelType;
                    Parameters.TimeSlotInfo[timeSlot].Configures++;
                }
            }

            Parameters.TimeSlotInfo[timeSlot].Assignments++;
            if (subChannel >= 0)
            {
                Parameters.TimeSlotInfo[timeSlot].SubChanAssignments[subChannel]++;
            }
        }

        private void ReleaseTimeSlotBursts(long timeSlot)
        {
            for (int frame = 0; frame < Parameters.TimeSlotHandlers[timeSlot].Length; frame++)
            {
                Burst burst = Parameters.TimeSlotHandlers[timeSlot][frame].Burst;
                ReleaseBurst(burst);
            }
        }

        private void TriggerServiceRequest(L3Handler L3Handler)
        {
            string ident = null;
            string type = null;

            lock (L3Handler.PDUDataFields)
            {
                if (L3Handler.PDUDataFields.ContainsKey("Identity"))
                {
                    ident = L3Handler.PDUDataFields["Identity"];

                    /* remove this info to prevent false detection for later frames */
                    L3Handler.PDUDataFields.Remove("Identity");
                }
                if (L3Handler.PDUDataFields.ContainsKey("ServiceType"))
                {
                    type = L3Handler.PDUDataFields["ServiceType"];

                    /* remove this info to prevent false detection for later frames */
                    L3Handler.PDUDataFields.Remove("ServiceType");
                }
            }

            if (ident != null && type != null)
            {
                AddMessage("  [__] " + ident + " Requests '" + type + "'" + Environment.NewLine);

                /* let the current burst "know" which service was requested */
                if (Parameters.CurrentBurstHandler is NormalBurst)
                {
                    ((NormalBurst)Parameters.CurrentBurstHandler).ServiceType = type;
                }
            }
        }

        private void TriggerLocationUpdateRequest(L3Handler L3Handler)
        {
            string ident = null;
            string type = null;

            lock (L3Handler.PDUDataFields)
            {
                if (L3Handler.PDUDataFields.ContainsKey("Identity"))
                {
                    ident = L3Handler.PDUDataFields["Identity"];

                    /* remove this info to prevent false detection for later frames */
                    L3Handler.PDUDataFields.Remove("Identity");
                }
                if (L3Handler.PDUDataFields.ContainsKey("LocationUpdateType"))
                {
                    type = L3Handler.PDUDataFields["LocationUpdateType"];

                    /* remove this info to prevent false detection for later frames */
                    L3Handler.PDUDataFields.Remove("LocationUpdateType");
                }
            }

            if (ident != null && type != null)
            {
                AddMessage("  [Log] " + ident + " Requests '" + type + "'" + Environment.NewLine);

                /* let the current burst "know" which service was requested */
                if (Parameters.CurrentBurstHandler is NormalBurst)
                {
                    ((NormalBurst)Parameters.CurrentBurstHandler).ServiceType = type;
                }
            }
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

            long frame = 0;
            CBCHBurst Burst = new CBCHBurst(L3, (int)subChannel);
            Parameters.UsedBursts.AddLast(Burst);

            /* type 4 is in timeslot 0 and shared with BCCH, CCCH and SDCCH */
            if (channelType == 4)
                frame = 20 + 2 + (subChannel / 2) * 10 + (subChannel & 1) * 4;
            else
                frame = 4 * subChannel;

            /* call generic trigger to set up arrays */
            TriggerChannelAssignment(L3Handler);
            AddMessage("  [__] TimeSlot " + timeSlot + " SubChannel " + subChannel + " now configured as Cell Broadcast channel." + Environment.NewLine);

            lock (Parameters.TimeSlotHandlers)
            {
                if (Parameters.TimeSlotHandlers[timeSlot] == null)
                {
                    Parameters.TimeSlotHandlers[timeSlot] = new sTimeSlotParam[51];
                }

                for (long pos = frame; pos < frame + 4; pos++)
                {
                    Burst burst = Parameters.TimeSlotHandlers[timeSlot][pos].Burst;
                    ReleaseBurst(burst);
                }
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
                            RegisterActiveBurst(tmpSDCCH1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 2] = new sTimeSlotParam(tmpSDCCH1, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 3] = new sTimeSlotParam(tmpSDCCH1, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 4] = new sTimeSlotParam(tmpSDCCH1, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 5] = new sTimeSlotParam(tmpSDCCH1, 3);

                            SDCCHBurst tmpSDCCH2 = new SDCCHBurst(L3, (block - 2) * 2 + 1);
                            RegisterActiveBurst(tmpSDCCH2);
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
                            RegisterActiveBurst(tmpSACCH1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 2] = new sTimeSlotParam(tmpSACCH1, 0);
                            Parameters.TimeSlotHandlers[0][block * 10 + 3] = new sTimeSlotParam(tmpSACCH1, 1);
                            Parameters.TimeSlotHandlers[0][block * 10 + 4] = new sTimeSlotParam(tmpSACCH1, 2);
                            Parameters.TimeSlotHandlers[0][block * 10 + 5] = new sTimeSlotParam(tmpSACCH1, 3);

                            SACCHBurst tmpSACCH2 = new SACCHBurst(L3, "SACCH 1/3", 1);
                            RegisterActiveBurst(tmpSACCH2);
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
                        AddMessage("  [L1] This particular BCCH/CCCH configuration is not handled yet!" + Environment.NewLine);
                        break;
                }
            }
        }


        private void TriggerCipherCommand(L3Handler L3Handler)
        {
            int state = -1;
            int type = -1;

            lock (L3Handler.PDUDataFields)
            {
                if (L3Handler.PDUDataFields.ContainsKey("CipherState"))
                    state = (int)L3Handler.PDUDataRawFields["CipherState"];

                if (L3Handler.PDUDataFields.ContainsKey("CipherType"))
                    type = (int)L3Handler.PDUDataRawFields["CipherType"];
            }

            AddMessage("  [L1] [" + Parameters.CurrentBurstHandler.Name + "] Will switch encrypted state" + Environment.NewLine);

            /* find the associated TCH and update its encryption state */
            if (Parameters.CurrentBurstHandler != null && (Parameters.CurrentBurstHandler.GetType() == typeof(SACCHBurst) ||Parameters.CurrentBurstHandler.GetType() == typeof(SDCCHBurst)))
            {
                NormalBurst channel = (NormalBurst)Parameters.CurrentBurstHandler;

                channel.ChannelEncrypted = (state != 0);
                channel.EncryptionType = type;

                if (channel.AssociatedSACCH != null)
                {
                    channel.AssociatedSACCH.ChannelEncrypted = true;
                }
            }
            else
            {
                AddMessage("ERROR: Got DCCH message but it was not in a DCCH. (" + Parameters.CurrentBurstHandler + ")");
            }
        }

        public void Handle(double[] timeSlotSamples, double[] timeSlotStrengths)
        {
            if (AddMessage == null)
                return;

            Parameters.TN++;
            Parameters.TN %= 8;
            if (Parameters.TN == 0)
                Parameters.FN++;

            Parameters.TimeStamp = DateTime.Now;

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
                else
                {
                    if (Parameters.TimeSlotHandlers[Parameters.TN].Length == 51)
                    {
                        frameNum = Parameters.T3; // control frame number
                    }
                    else if (Parameters.TimeSlotHandlers[Parameters.TN].Length == 26)
                    {
                        frameNum = Parameters.T2; // traffic frame number
                    }

                    handler = Parameters.TimeSlotHandlers[Parameters.TN][frameNum].Burst;
                    sequence = Parameters.TimeSlotHandlers[Parameters.TN][frameNum].Sequence;
                }

                Parameters.CurrentBurstHandler = handler;
            }

            if (Burst.DumpRawData)
            {
                if (handler != null)
                    AddMessage("  [L1] Handler: " + handler.Name + "[" + sequence + "]  TN:" + Parameters.TN + "  Frame: " + frameNum + Environment.NewLine);
                else
                    AddMessage("  [L1] Handler: (none)  TN:" + Parameters.TN + "  Frame: " + frameNum + Environment.NewLine);
            }

            Burst.eSuccessState rawHandlerState = Burst.eSuccessState.Unknown;
            Burst.eSuccessState dataHandlerState = Burst.eSuccessState.Unknown;

            /* let the raw handler work with the data */
            if (handler != null)
            {
                try
                {
                    rawHandlerState = handler.ParseRawBurst(Parameters, timeSlotSamples, timeSlotStrengths);
                }
                catch (ThreadAbortException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    AddMessage("Handler " + handler + " caused an exception: " + e.ToString());
                }
            }

            /* apply offset, if any */
            Decoder.SampleOffset = Parameters.SampleStartPosition + Parameters.SampleOffset + Parameters.SubSampleOffset;

            /* continue to decode the packet */
            Decoder.Decode(timeSlotSamples, BurstBits);
            DifferenceCode.Decode(BurstBits, BurstBitsUndiffed);

            if (Parameters.PacketDumper != null)
            {
                Parameters.PacketDumper.WriteRawBurst(BurstBitsUndiffed);
            }

            /* 
             * process if there is a handler, but not if there is a dumper with skipping activated
             * dont skip SCH bursts and BCCH
             */
            if (handler != null)
            {
                bool dump = Parameters.PacketDumper != null;
                bool skip = Parameters.SkipL2Parsing;

                /* 
                 * SCH will correct sample offsets and BCCH is needed until we received the
                 * first network configuration information in SYSTEM INFORMATION TYPE 3. 
                 * with wrong network config we will miss a lot of SCHs which will cause drifting etc.
                 */
                bool important = (handler is SCHBurst) || ((handler is BCCHBurst) && Parameters.TimeSlotInfo[0].Configures == 0);

                if (dump && skip && !important)
                {
                    if (rawHandlerState == Burst.eSuccessState.Failed)
                    {
                        Parameters.Error();
                        if (Parameters.ReportL1Errors)
                        {
                            AddMessage("  [L1] [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);
                            AddMessage("       ERROR: " + handler.ErrorMessage + Environment.NewLine);
                            AddMessage(Environment.NewLine);
                        }
                        else
                        {
                            handler.StatusMessage = null;
                        }
                    }
                    else
                    {
                        Parameters.Success();
                    }
                }
                else
                {
                    /* only run data handler if raw handler didn't fail */
                    if (rawHandlerState != Burst.eSuccessState.Failed)
                    {
                        try
                        {
                            dataHandlerState = handler.ParseData(Parameters, BurstBitsUndiffed, sequence);
                        }
                        catch (ThreadAbortException e)
                        {
                            throw e;
                        }
                        catch (Exception e)
                        {
                            AddMessage("Handler " + handler + " caused an exception: " + e.ToString());
                        }
                    }

                    /* if everything went ok so far, pass the data to the associated handler */
                    if (rawHandlerState == Burst.eSuccessState.Failed || dataHandlerState == Burst.eSuccessState.Failed)
                    {
                        Parameters.Error();
                        if (Parameters.ReportL1Errors)
                        {
                            AddMessage("  [L1] [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);
                            AddMessage("       ERROR: " + handler.ErrorMessage + Environment.NewLine);
                            AddMessage(Environment.NewLine);
                        }
                        else
                        {
                            handler.StatusMessage = null;
                        }
                    }
                    else
                    {
                        /* only update success counters when this burst was processed successfully */
                        if (dataHandlerState == Burst.eSuccessState.Succeeded)
                        {
                            Parameters.Success();
                        }

                        /* show L2 messages, if handler wishes */
                        bool showL2 = handler.L2.ShowMessage && !string.IsNullOrEmpty(handler.L2.StatusMessage);

                        /* show L3 messages, if there is any */
                        bool showL3 = !string.IsNullOrEmpty(handler.L3.StatusMessage);

                        /* only show L1 when one of L2/L3 has a message */
                        if (showL2 || showL3 || handler.StatusMessage != null)
                        {
                            AddMessage("  [L1] [ARFCN: " + Parameters.ARFCN + "] ");
                            AddMessage("[MCC: " + Parameters.MCC + "] ");
                            AddMessage("[MNC: " + Parameters.MNC + "] ");
                            AddMessage("[LAC: " + Parameters.LAC + "] ");
                            AddMessage("[CellID: " + Parameters.CellIdent + "] ");
                            AddMessage(Environment.NewLine);
                            AddMessage("  [L1] [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);
                        }

                        if (handler.StatusMessage != null && handler.StatusMessage.Trim() != "")
                        {
                            bool first = true;
                            foreach (string line in handler.StatusMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (line.Trim() != "")
                                {
                                    AddMessage("       " + line + Environment.NewLine);
                                }
                            }
                        }

                        /* show L2 if L2 wants to, or if L3 has some message */
                        if (showL3 || showL2)
                        {
                            bool first = true;
                            foreach (string line in handler.L2.StatusMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (first)
                                {
                                    first = false;
                                    AddMessage("  [L2] " + line + Environment.NewLine);
                                }
                                else if (line.Trim() != "")
                                {
                                    AddMessage("       " + line + Environment.NewLine);
                                }
                            }
                        }

                        /* L3 handler has a message to show */
                        if (showL3)
                        {
                            bool first = true;
                            foreach (string line in handler.L3.StatusMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (first)
                                {
                                    first = false;
                                    AddMessage("  [L3] " + line + Environment.NewLine);
                                }
                                else if (line.Trim() != "")
                                {
                                    AddMessage("       " + line + Environment.NewLine);
                                }
                            }
                        }

                        /* if something shown, add newline */
                        if (showL2 || showL3)
                        {
                            AddMessage(Environment.NewLine);
                        }
                    }

                    /* and reset these "flags" */
                    handler.StatusMessage = null;
                    handler.ErrorMessage = null;
                    handler.L2.ShowMessage = false;
                    handler.L3.StatusMessage = null;

                    /* does the Layer 3 provide any sniffed information? */
                    if (handler.L3.SniffResult != null)
                    {
                        AddMessage("  [L3] sniffed: " + handler.L3.SniffResult + Environment.NewLine);
                        handler.L3.SniffResult = null;
                    }
                }
            }
        }

        public void Handle(bool[] burstBits, ulong burstId)
        {
            if (AddMessage == null)
                return;

            /* TC and FN will get filled by dump reader */

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
                else
                {
                    if (Parameters.TimeSlotHandlers[Parameters.TN].Length == 51)
                    {
                        frameNum = Parameters.T3; // control frame number
                    }
                    else if (Parameters.TimeSlotHandlers[Parameters.TN].Length == 26)
                    {
                        frameNum = Parameters.T2; // traffic frame number
                    }

                    handler = Parameters.TimeSlotHandlers[Parameters.TN][frameNum].Burst;
                    sequence = Parameters.TimeSlotHandlers[Parameters.TN][frameNum].Sequence;
                }

                Parameters.CurrentBurstHandler = handler;
            }

            if (Burst.DumpRawData)
            {
                if (handler != null)
                    AddMessage("  [L1] Handler: " + handler.Name + "[" + sequence + "]  TN:" + Parameters.TN + "  Frame: " + frameNum + "  FN: " + sequence + Environment.NewLine);
                else
                    AddMessage("  [L1] Handler: (none)  TN:" + Parameters.TN + "  Frame: " + frameNum + "  FN: " + sequence + Environment.NewLine);
            }

            Burst.eSuccessState dataHandlerState = Burst.eSuccessState.Unknown;

            if (Parameters.PacketDumper != null)
            {
                Parameters.PacketDumper.WriteRawBurst(burstBits);
            }

            if (handler != null)
            {
                try
                {
                    dataHandlerState = handler.ParseData(Parameters, burstBits, sequence);
                }
                catch (ThreadAbortException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    AddMessage("Handler " + handler + " caused an exception: " + e.ToString());
                }

                /* if everything went ok so far, pass the data to the associated handler */
                if (dataHandlerState == Burst.eSuccessState.Failed)
                {
                    //Log.AddMessage("ERROR in " + handler.GetType());

                    Parameters.Error();
                    if (Parameters.ReportL1Errors)
                    {
                        AddMessage("  [L1] [" + handler.Name + "] - [Burst: " + burstId + "] [" + Parameters + "]" + Environment.NewLine);
                        AddMessage("       ERROR: " + handler.ErrorMessage + Environment.NewLine);
                        AddMessage(Environment.NewLine);
                    }
                    else
                    {
                        handler.StatusMessage = null;
                    }
                }
                else
                {
                    /* only update success counters when this burst was processed successfully */
                    if (dataHandlerState == Burst.eSuccessState.Succeeded)
                    {
                        Parameters.Success();
                    }

                    /* show L2 messages, if handler wishes */
                    bool showL2 = handler.L2.ShowMessage && !string.IsNullOrEmpty(handler.L2.StatusMessage);

                    /* show L3 messages, if there is any */
                    bool showL3 = !string.IsNullOrEmpty(handler.L3.StatusMessage);

                    /* only show L1 when one of L2/L3 has a message */
                    if (showL2 || showL3 || handler.StatusMessage != null)
                    {
                        AddMessage("  [L1] [ARFCN: " + Parameters.ARFCN + "] ");
                        AddMessage("[Burst: " + burstId + "] ");
                        AddMessage("[MCC: " + Parameters.MCC + "] ");
                        AddMessage("[MNC: " + Parameters.MNC + "] ");
                        AddMessage("[LAC: " + Parameters.LAC + "] ");
                        AddMessage("[CellID: " + Parameters.CellIdent + "] ");
                        AddMessage(Environment.NewLine);
                        AddMessage("  [L1] [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);
                    }

                    if (handler.StatusMessage != null && handler.StatusMessage.Trim() != "")
                    {
                        bool first = true;
                        foreach (string line in handler.StatusMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (line.Trim() != "")
                            {
                                AddMessage("       " + line + Environment.NewLine);
                            }
                        }
                    }

                    /* show L2 if L2 wants to, or if L3 has some message */
                    if (showL3 || showL2)
                    {
                        bool first = true;
                        foreach (string line in handler.L2.StatusMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (first)
                            {
                                first = false;
                                AddMessage("  [L2] " + line + Environment.NewLine);
                            }
                            else if (line.Trim() != "")
                            {
                                AddMessage("       " + line + Environment.NewLine);
                            }
                        }
                    }

                    /* L3 handler has a message to show */
                    if (showL3)
                    {
                        bool first = true;
                        foreach (string line in handler.L3.StatusMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (first)
                            {
                                first = false;
                                AddMessage("  [L3] " + line + Environment.NewLine);
                            }
                            else if (line.Trim() != "")
                            {
                                AddMessage("       " + line + Environment.NewLine);
                            }
                        }
                    }

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
                    AddMessage("  [L3] sniffed: " + handler.L3.SniffResult + Environment.NewLine);
                    handler.L3.SniffResult = null;
                }
            }
        }
    }
}