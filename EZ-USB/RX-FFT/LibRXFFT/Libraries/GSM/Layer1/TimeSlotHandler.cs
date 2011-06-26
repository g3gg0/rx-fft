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
            
            Parameters.AddUsedBurst(BCCH);
            Parameters.AddUsedBurst(CCCH);
            
            L3.PDUDataTriggers.Add("ServiceRequest", TriggerServiceRequest);
            L3.PDUDataTriggers.Add("LocationUpdateTypeSet", TriggerLocationUpdateRequest);
            L3.PDUDataTriggers.Add("PagingResponseReceived", TriggerPagingResponse);
            L3.PDUDataTriggers.Add("CCCH-CONF", TriggerCCCHCONF);
            L3.PDUDataTriggers.Add("ChannelAssignment", TriggerChannelAssignment);
            L3.PDUDataTriggers.Add("CBCHUpdate", TriggerCBCHUpdate);
            L3.PDUDataTriggers.Add("CBCHReset", TriggerCBCHReset);
            L3.PDUDataTriggers.Add("CipherCommand", TriggerCipherCommand);

            for (int timeSlot = 0; timeSlot < 8; timeSlot++)
            {
                Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Type = eTimeSlotType.Unconfigured;
                Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].SubChanAssignments = new int[8];
                Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Type = eTimeSlotType.Unconfigured;
                Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Uplink][timeSlot].SubChanAssignments = new int[8];
            }

            sTimeSlotParam[] handlers = new sTimeSlotParam[51];
            handlers[0] = new sTimeSlotParam(FCH, 0);
            handlers[1] = new sTimeSlotParam(SCH, 0);
            handlers[2] = new sTimeSlotParam(BCCH, 0);
            handlers[3] = new sTimeSlotParam(BCCH, 1);
            handlers[4] = new sTimeSlotParam(BCCH, 2);
            handlers[5] = new sTimeSlotParam(BCCH, 3);
            handlers[6] = new sTimeSlotParam(CCCH, 0);
            handlers[7] = new sTimeSlotParam(CCCH, 1);
            handlers[8] = new sTimeSlotParam(CCCH, 2);
            handlers[9] = new sTimeSlotParam(CCCH, 3);

            Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][0].Handlers = handlers;
            Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Uplink][0].Handlers = new sTimeSlotParam[51];

            if (PreallocateTCHs)
            {
                for (int timeSlot = 1; timeSlot < 8; timeSlot++)
                {
                    Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers = new sTimeSlotParam[26];

                    TCHBurst tch = new TCHBurst(L3, "TCH" + timeSlot + "/F", (int)timeSlot);
                    SACCHBurst sacch = new SACCHBurst(L3, "SACCH/TCH" + timeSlot, (int)timeSlot, true);
                    
                    tch.AssociatedSACCH = sacch;
                    sacch.AssociatedTCH = tch;
                    Parameters.AddUsedBurst(tch);
                    Parameters.AddUsedBurst(sacch);
                    
                    for (int frame = 0; frame < 25; frame++)
                    {
                        if (frame == 12)
                            Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame] = new sTimeSlotParam(sacch, 0);
                        else
                            Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame] = new sTimeSlotParam(tch, 0);
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
            if (burst != null && !Parameters.ContainsActiveBursts(burst))
            {
                /* call LUA script */
                if (Parameters.LuaVm != null)
                {
                    LuaHelpers.CallFunction(Parameters.LuaVm, "RegisterActiveBurst", true, burst, Parameters);
                }

                Parameters.AddActiveBurst(burst);
                Parameters.AddUsedBurst(burst);
            }
        }
        

        private void UnregisterActiveBurst(NormalBurst burst)
        {
            if (burst != null && Parameters.ContainsActiveBursts(burst))
            {
                /* call LUA script */
                if (Parameters.LuaVm != null)
                {
                    LuaHelpers.CallFunction(Parameters.LuaVm, "UnregisterActiveBurst", true, burst, Parameters);
                }

                Parameters.RemoveActiveBurst(burst);
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

        private void TriggerPagingResponse(L3Handler L3Handler)
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
            }

            if (ident != null)
            {
                AddMessage("  [__] " + ident + " replied to paging" + Environment.NewLine);
            }

            /* when received a paging response, check if it was already set */
            if (Parameters.CurrentBurstHandler is NormalBurst)
            {
                NormalBurst burst = ((NormalBurst)Parameters.CurrentBurstHandler);

                burst.PhoneIdentity = ident;

                if (burst.EstablishmentCause == "(not set)")
                {
                    burst.EstablishmentCause = "Answer to paging";
                }
            }
        }

        private void TriggerChannelAssignment(L3Handler L3Handler)
        {
            eTimeSlotType channelType;
            long subChannel;
            long timeSlot;
            sTimeslotReference reference;
            bool hopping = false;
            long targetARFCN = Parameters.ARFCN;
            int targetARFCNidx = Parameters.ARFCNidx;

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

                if (L3Handler.PDUDataRawFields.ContainsKey("ARFCN"))
                {
                    targetARFCN = L3Handler.PDUDataRawFields["ARFCN"];
                    Parameters.EnsureARFCN(targetARFCN);
                    targetARFCNidx = Parameters.GetARFCNIdx(targetARFCN);
                }

                channelType = (eTimeSlotType)L3Handler.PDUDataRawFields["ChannelType"];
                subChannel = L3Handler.PDUDataRawFields["SubChannel"];
                timeSlot = L3Handler.PDUDataRawFields["TimeSlot"];

                reference.T1 = L3Handler.PDUDataRawFields["RefT1"];
                reference.T2 = L3Handler.PDUDataRawFields["RefT2"];
                reference.T3 = L3Handler.PDUDataRawFields["RefT3"];
            }

            bool reconfigure = Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers == null || Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Type != channelType;
            bool assignment = false;

            /* was: assigned time slot type does not match? */
            /* now: make sure thats no hopping channel. no chance yet to decode */

            if (!hopping)
            {
                lock (Parameters.TimeSlotConfig)
                {
                    switch (channelType)
                    {
                        case eTimeSlotType.TCHF:
                            if (Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers != null && Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[0].Reference == reference)
                            {
                                //AddMessage("   [L1] TimeSlot " + timeSlot + " already configured as TCH/F (was " + Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers.Type + ")" + Environment.NewLine);
                            }
                            else
                            {
                                /* this is a new assignment */
                                assignment = true;

                                if (Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers == null)
                                {
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers = new sTimeSlotParam[26];
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers = new sTimeSlotParam[26];
                                }

                                /* release the old bursts first */
                                for (int frame = 0; frame < Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers.Length; frame++)
                                {
                                    ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[frame].Burst);
                                    ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame].Burst);
                                }

                                AddMessage("  [__] TimeSlot " + timeSlot + " of ARFCN " + targetARFCN + " now configured as TCH/F (was " + Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Type + ")" + Environment.NewLine);

                                TCHBurst tchUp = new TCHBurst(L3, "TCH" + timeSlot + "/F", (int)timeSlot);
                                TCHBurst tchDown = new TCHBurst(L3, "TCH" + timeSlot + "/F", (int)timeSlot);
                                SACCHBurst sacchUp = new SACCHBurst(L3, "SACCH/TCH" + timeSlot, (int)timeSlot, true);
                                SACCHBurst sacchDown = new SACCHBurst(L3, "SACCH/TCH" + timeSlot, (int)timeSlot, true);

                                if (L3Handler.PDUDataRawFields.ContainsKey("ChannelMode"))
                                {
                                    int channelMode = (int)L3Handler.PDUDataRawFields["ChannelMode"];
                                    tchUp.ChannelMode = channelMode;
                                    tchDown.ChannelMode = channelMode;
                                }

                                tchUp.ARFCN = targetARFCN;
                                tchDown.ARFCN = targetARFCN;
                                sacchUp.ARFCN = targetARFCN;
                                sacchDown.ARFCN = targetARFCN;

                                tchUp.TimeSlot = timeSlot;
                                tchDown.TimeSlot = timeSlot;
                                sacchUp.TimeSlot = timeSlot;
                                sacchDown.TimeSlot = timeSlot;

                                tchUp.Direction = eLinkDirection.Uplink;
                                tchDown.Direction = eLinkDirection.Downlink;
                                sacchUp.Direction = eLinkDirection.Uplink;
                                sacchDown.Direction = eLinkDirection.Downlink;

                                RegisterActiveBurst(tchUp);
                                RegisterActiveBurst(tchDown);
                                RegisterActiveBurst(sacchUp);
                                RegisterActiveBurst(sacchDown);

                                tchUp.AssociatedSACCH = sacchUp;
                                tchDown.AssociatedSACCH = sacchDown;
                                sacchUp.AssociatedTCH = tchUp;
                                sacchDown.AssociatedTCH = tchDown;

                                /* this should be true in any case */
                                if (Parameters.CurrentBurstHandler is SDCCHBurst)
                                {
                                    SDCCHBurst sdcch = (SDCCHBurst)Parameters.CurrentBurstHandler;

                                    /* check if this connection had a key predefined and copy it */
                                    if (sdcch.ChannelEncrypted)
                                    {
                                        AddMessage("  [__] Using cipher key from associated SDCCH connection" + Environment.NewLine);
                                        tchUp.A5Algorithm = sdcch.A5Algorithm;
                                        tchUp.A5CipherKey = sdcch.A5CipherKey;
                                        tchUp.ChannelEncrypted = true;
                                        tchDown.A5Algorithm = sdcch.A5Algorithm;
                                        tchDown.A5CipherKey = sdcch.A5CipherKey;
                                        tchDown.ChannelEncrypted = true;

                                        sacchUp.A5Algorithm = sdcch.A5Algorithm;
                                        sacchUp.A5CipherKey = sdcch.A5CipherKey;
                                        sacchUp.ChannelEncrypted = true;
                                        sacchDown.A5Algorithm = sdcch.A5Algorithm;
                                        sacchDown.A5CipherKey = sdcch.A5CipherKey;
                                        sacchDown.ChannelEncrypted = true;
                                    }
                                }

                                if (Parameters.CurrentBurstHandler is NormalBurst && ((NormalBurst)Parameters.CurrentBurstHandler).ChannelEncrypted)
                                {
                                    tchUp.ChannelEncrypted = true;
                                    tchDown.ChannelEncrypted = true;
                                    sacchUp.ChannelEncrypted = true;
                                    sacchDown.ChannelEncrypted = true;
                                }

                                for (int frame = 0; frame < 25; frame++)
                                {
                                    if (frame == 12)
                                    {
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame] = new sTimeSlotParam(sacchDown, 0);
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[frame] = new sTimeSlotParam(sacchUp, 0);
                                    }
                                    else
                                    {
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame] = new sTimeSlotParam(tchDown, 0);
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[frame] = new sTimeSlotParam(tchUp, 0);
                                    }

                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame].Reference = reference;
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[frame].Reference = reference;
                                }
                            }
                            break;

                        case eTimeSlotType.TCHH:
                            /* TODO: Uplink! */
                            /* TODO: this is wrong! have to check subchannel reference! */
                            if (Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers != null && Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[0].Reference == reference)
                            {
                                //AddMessage("   [L1] TimeSlot " + timeSlot + " already configured as TCH/H (was " + Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers.Type + ")" + Environment.NewLine);
                            }
                            else
                            {
                                /* this is a new assignment */
                                assignment = true;

                                UnregisterActiveBursts(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers);

                                if (Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers == null)
                                {
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers = new sTimeSlotParam[26];
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers = new sTimeSlotParam[26];
                                }

                                /* make sure the old bursts get released */
                                ReleaseTimeSlotBursts(targetARFCNidx, timeSlot);

                                /* if length did not fit, reallocate */
                                if (Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers.Length != 26)
                                {
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers = new sTimeSlotParam[26];
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers = new sTimeSlotParam[26];
                                }

                                AddMessage("  [__] TimeSlot " + timeSlot + " of ARFCN " + targetARFCN + " now configured as TCH/H (was " + Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Type + ")" + Environment.NewLine);

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

                                tch1.ARFCN = targetARFCN;
                                tch2.ARFCN = targetARFCN;
                                sacch1.ARFCN = targetARFCN;
                                sacch2.ARFCN = targetARFCN;

                                tch1.TimeSlot = timeSlot;
                                tch2.TimeSlot = timeSlot;
                                sacch1.TimeSlot = timeSlot;
                                sacch2.TimeSlot = timeSlot;

                                tch1.Direction = eLinkDirection.Downlink;
                                tch1.Direction = eLinkDirection.Downlink;
                                sacch1.Direction = eLinkDirection.Downlink;
                                sacch1.Direction = eLinkDirection.Downlink;

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
                                    {
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame] = new sTimeSlotParam(sacch1, 0);
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[frame] = new sTimeSlotParam(sacch1, 0);
                                    }
                                    else if (frame == 25)
                                    {
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame] = new sTimeSlotParam(sacch2, 0);
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[frame] = new sTimeSlotParam(sacch2, 0);
                                    }
                                    else if ((frame & 1) == 0)
                                    {
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame] = new sTimeSlotParam(tch1, 0);
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[frame] = new sTimeSlotParam(tch1, 0);
                                    }
                                    else
                                    {
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame] = new sTimeSlotParam(tch2, 0);
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[frame] = new sTimeSlotParam(tch2, 0);
                                    }

                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame].Reference = reference;
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[frame].Reference = reference;
                                }
                            }
                            break;

                        case eTimeSlotType.SDCCH8:
                            {
#if false
                                AddMessage("   [L1] TimeSlot " + timeSlot + " of ARFCN " + targetARFCN + " now configured as SDCCH/8 (was " + Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers.Type + ")" + Environment.NewLine);
                                Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers = new sTimeSlotParam[51];

                                /* 8 SDCCHs */
                                for (int chan = 0; chan < 8; chan++)
                                {
                                    SDCCHBurst tmpSDCCH = new SDCCHBurst(L3, chan);
                                    Parameters.UsedBursts.AddLast(tmpSDCCH);
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[chan * 4 + 0] = new sTimeSlotParam(tmpSDCCH, 0);
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[chan * 4 + 1] = new sTimeSlotParam(tmpSDCCH, 1);
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[chan * 4 + 2] = new sTimeSlotParam(tmpSDCCH, 2);
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[chan * 4 + 3] = new sTimeSlotParam(tmpSDCCH, 3);
                                }

                                /* finally 4 SACCHs */
                                for (int chan = 0; chan < 4; chan++)
                                {
                                    SACCHBurst tmpSACCH = new SACCHBurst(L3, "SACCH " + chan + "/" + (chan + 4), chan);
                                    Parameters.UsedBursts.AddLast(tmpSACCH);
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[(chan + 8) * 4 + 0] = new sTimeSlotParam(tmpSACCH, 0);
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[(chan + 8) * 4 + 1] = new sTimeSlotParam(tmpSACCH, 1);
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[(chan + 8) * 4 + 2] = new sTimeSlotParam(tmpSACCH, 2);
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[(chan + 8) * 4 + 3] = new sTimeSlotParam(tmpSACCH, 3);
                                }
#else

                                int sdcchFrame = GetFrameNumForChannel(channelType, eChannelType.SDCCH, (int)subChannel);
                                int sacchFrame = GetFrameNumForChannel(channelType, eChannelType.SACCH, (int)subChannel);

                                if (Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers == null || Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers.Length != 51)
                                {
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers = new sTimeSlotParam[51];
                                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers = new sTimeSlotParam[51];
                                }

                                if (Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[sdcchFrame].Reference == reference)
                                {
                                    //AddMessage("   [L1] TimeSlot " + timeSlot + " already configured as SDCCH/8 (was " + Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers.Type + ")" + Environment.NewLine);
                                }
                                else
                                {
                                    /* this is a new assignment */
                                    assignment = true;

                                    AddMessage("  [__] TimeSlot " + timeSlot + " of ARFCN " + targetARFCN + " now configured as SDCCH/8 (was " + Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Type + ")" + Environment.NewLine);
                                    int sacchChannel1 = (int)subChannel / 2;
                                    int sacchChannel2 = sacchChannel1 + 4;

                                    /* release the old burst first */
                                    ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[sdcchFrame].Burst);
                                    ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[sdcchFrame].Burst);

                                    /* we are releasing a SACCH even if it may be still used by the other SDCCH using it */
                                    ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[sacchFrame].Burst);
                                    ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[sacchFrame].Burst);

                                    /* allocate a new SDCCH */
                                    SDCCHBurst tmpSDCCHUp = new SDCCHBurst(L3, (int)subChannel);
                                    SDCCHBurst tmpSDCCHDown = new SDCCHBurst(L3, (int)subChannel);

                                    tmpSDCCHUp.ARFCN = targetARFCN;
                                    tmpSDCCHDown.ARFCN = targetARFCN;
                                    tmpSDCCHUp.TimeSlot = timeSlot;
                                    tmpSDCCHDown.TimeSlot = timeSlot;
                                    tmpSDCCHUp.Direction = eLinkDirection.Uplink;
                                    tmpSDCCHDown.Direction = eLinkDirection.Downlink;

                                    RegisterActiveBurst(tmpSDCCHUp);
                                    RegisterActiveBurst(tmpSDCCHDown);

                                    if (L3Handler.PDUDataFields.ContainsKey("EstablishmentCause"))
                                    {
                                        tmpSDCCHUp.EstablishmentCause = L3Handler.PDUDataFields["EstablishmentCause"];
                                        tmpSDCCHUp.ServiceType = "(not set)";
                                        tmpSDCCHDown.EstablishmentCause = L3Handler.PDUDataFields["EstablishmentCause"];
                                        tmpSDCCHDown.ServiceType = "(not set)";

                                        /* remove this info to prevent false detection for later causes */
                                        L3Handler.PDUDataFields.Remove("EstablishmentCause");
                                    }

                                    int sequence = 0;
                                    for (int pos = sdcchFrame; pos < sdcchFrame + 4; pos++)
                                    {
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[pos] = new sTimeSlotParam(tmpSDCCHUp, sequence);
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[pos].Reference = reference;
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[pos] = new sTimeSlotParam(tmpSDCCHDown, sequence++);
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[pos].Reference = reference;
                                    }

                                    /* we are allocating a new SACCH even if it may be used by the other SDCCH using it */
                                    SACCHBurst tmpSACCHUp = new SACCHBurst(L3, "SACCH " + sacchChannel1 + "/" + sacchChannel2, (int)sacchChannel1);
                                    SACCHBurst tmpSACCHDown = new SACCHBurst(L3, "SACCH " + sacchChannel1 + "/" + sacchChannel2, (int)sacchChannel1);

                                    tmpSACCHUp.ARFCN = targetARFCN;
                                    tmpSACCHDown.ARFCN = targetARFCN;
                                    tmpSACCHUp.TimeSlot = timeSlot;
                                    tmpSACCHDown.TimeSlot = timeSlot;
                                    tmpSACCHUp.Direction = eLinkDirection.Uplink;
                                    tmpSACCHDown.Direction = eLinkDirection.Downlink;

                                    RegisterActiveBurst(tmpSACCHUp);
                                    RegisterActiveBurst(tmpSACCHDown);

                                    sequence = 0;
                                    for (int pos = sacchFrame; pos < sacchFrame + 4; pos++)
                                    {
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[pos] = new sTimeSlotParam(tmpSACCHUp, sequence);
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[pos].Reference = reference;
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[pos] = new sTimeSlotParam(tmpSACCHDown, sequence++);
                                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[pos].Reference = reference;
                                    }

                                    tmpSDCCHUp.AssociatedSACCH = tmpSACCHUp;
                                    tmpSDCCHDown.AssociatedSACCH = tmpSACCHDown;
                                }
#endif
                            }
                            break;

                        case eTimeSlotType.BCCH_CCCH_SDCCH4:
                            {
                                int sdcchFrame = GetFrameNumForChannel(channelType, eChannelType.SDCCH, (int)subChannel);
                                int sacchFrame = GetFrameNumForChannel(channelType, eChannelType.SACCH, (int)subChannel);

                                if (Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers != null && (sdcchFrame < Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers.Length))
                                {
                                    if (Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[sdcchFrame].Reference == reference)
                                    {
                                        //AddMessage("   [L1] TimeSlot " + timeSlot + " already configured as BCCH_CCCH_SDCCH4 (was " + Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers.Type + ")" + Environment.NewLine);
                                    }
                                    else
                                    {
#if false
                                    AddMessage("  [__] TimeSlot " + timeSlot + " of ARFCN " + targetARFCN + " SDCCH subchan " + subChannel + " assigned" + Environment.NewLine);

                                    Burst burst = Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[framePos].Burst;

                                    /* safety measure */
                                    if (!(burst is NormalBurst))
                                    {
                                        /* wait... what?! */
                                        AddMessage("  [EE] TimeSlot " + timeSlot + " of ARFCN " + targetARFCN + " SDCCH subchan " + subChannel + " expected" + Environment.NewLine);
                                        break;
                                    }
                                    NormalBurst assigned = (NormalBurst)burst;

                                    if (L3Handler.PDUDataFields.ContainsKey("EstablishmentCause"))
                                    {
                                        assigned.EstablishmentCause = L3Handler.PDUDataFields["EstablishmentCause"];
                                        assigned.ServiceType = "(not set)";

                                        /* remove this info to prevent false detection for later causes */
                                        L3Handler.PDUDataFields.Remove("EstablishmentCause");
                                    }
#endif

                                        /* this is a new assignment */
                                        assignment = true;

                                        AddMessage("  [__] TimeSlot " + timeSlot + " of ARFCN " + targetARFCN + " now configured as SDCCH/8 (was " + Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Type + ")" + Environment.NewLine);
                                        int sacchChannel1 = (int)subChannel / 2;
                                        int sacchChannel2 = sacchChannel1 + 4;

                                        /* release the old burst first */
                                        ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[sdcchFrame].Burst);
                                        ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[sdcchFrame].Burst);

                                        /* we are releasing a SACCH even if it may be still used by the other SDCCH using it */
                                        ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[sacchFrame].Burst);
                                        ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[sacchFrame].Burst);

                                        /* allocate a new SDCCH */
                                        SDCCHBurst tmpSDCCHUp = new SDCCHBurst(L3, (int)subChannel);
                                        SDCCHBurst tmpSDCCHDown = new SDCCHBurst(L3, (int)subChannel);

                                        tmpSDCCHUp.ARFCN = targetARFCN;
                                        tmpSDCCHDown.ARFCN = targetARFCN;
                                        tmpSDCCHUp.TimeSlot = timeSlot;
                                        tmpSDCCHDown.TimeSlot = timeSlot;
                                        tmpSDCCHUp.Direction = eLinkDirection.Uplink;
                                        tmpSDCCHDown.Direction = eLinkDirection.Downlink;

                                        RegisterActiveBurst(tmpSDCCHUp);
                                        RegisterActiveBurst(tmpSDCCHDown);

                                        if (L3Handler.PDUDataFields.ContainsKey("EstablishmentCause"))
                                        {
                                            tmpSDCCHUp.EstablishmentCause = L3Handler.PDUDataFields["EstablishmentCause"];
                                            tmpSDCCHUp.ServiceType = "(not set)";
                                            tmpSDCCHDown.EstablishmentCause = L3Handler.PDUDataFields["EstablishmentCause"];
                                            tmpSDCCHDown.ServiceType = "(not set)";

                                            /* remove this info to prevent false detection for later causes */
                                            L3Handler.PDUDataFields.Remove("EstablishmentCause");
                                        }

                                        int sequence = 0;
                                        for (int pos = sdcchFrame; pos < sdcchFrame + 4; pos++)
                                        {
                                            Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[pos] = new sTimeSlotParam(tmpSDCCHUp, sequence);
                                            Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[pos].Reference = reference;
                                            Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[pos] = new sTimeSlotParam(tmpSDCCHDown, sequence++);
                                            Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[pos].Reference = reference;
                                        }

                                        /* we are allocating a new SACCH even if it may be used by the other SDCCH using it */
                                        SACCHBurst tmpSACCHUp = new SACCHBurst(L3, "SACCH " + sacchChannel1 + "/" + sacchChannel2, (int)sacchChannel1);
                                        SACCHBurst tmpSACCHDown = new SACCHBurst(L3, "SACCH " + sacchChannel1 + "/" + sacchChannel2, (int)sacchChannel1);

                                        tmpSACCHUp.ARFCN = targetARFCN;
                                        tmpSACCHDown.ARFCN = targetARFCN;
                                        tmpSACCHUp.TimeSlot = timeSlot;
                                        tmpSACCHDown.TimeSlot = timeSlot;
                                        tmpSACCHUp.Direction = eLinkDirection.Uplink;
                                        tmpSACCHDown.Direction = eLinkDirection.Downlink;

                                        RegisterActiveBurst(tmpSACCHUp);
                                        RegisterActiveBurst(tmpSACCHDown);

                                        sequence = 0;
                                        for (int pos = sacchFrame; pos < sacchFrame + 4; pos++)
                                        {
                                            Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[pos] = new sTimeSlotParam(tmpSACCHUp, sequence);
                                            Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[pos].Reference = reference;
                                            Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[pos] = new sTimeSlotParam(tmpSACCHDown, sequence++);
                                            Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[pos].Reference = reference;
                                        }

                                        tmpSDCCHUp.AssociatedSACCH = tmpSACCHUp;
                                        tmpSDCCHDown.AssociatedSACCH = tmpSACCHDown;
                                    }
                                }
                                else
                                {
                                    AddMessage("  [__] TimeSlot " + timeSlot + " of ARFCN " + targetARFCN + " cannot get configured. Type: " + channelType + Environment.NewLine);
                                }
                            }
                            break;

                        default:
                            AddMessage("  [__] TimeSlot " + timeSlot + " of ARFCN " + targetARFCN + " cannot get configured. Type: " + channelType + Environment.NewLine);
                            break;
                    }

                    if (reconfigure)
                    {
                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Type = channelType;
                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Configures++;
                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Type = channelType;
                        Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Configures++;
                    }
                }
            }

            /* only update if this was a "new" assignment with other reference code */
            if (assignment)
            {
                Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Assignments++;
                Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Assignments++;
                if (subChannel >= 0)
                {
                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].SubChanAssignments[subChannel]++;
                    Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].SubChanAssignments[subChannel]++;
                }
            }
        }

        private void ReleaseTimeSlotBursts(int targetARFCNidx, long timeSlot)
        {
            for (int frame = 0; frame < Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers.Length; frame++)
            {
                ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[frame].Burst);
                ReleaseBurst(Parameters.TimeSlotConfig[targetARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame].Burst);
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
                    NormalBurst burst = ((NormalBurst)Parameters.CurrentBurstHandler);

                    burst.PhoneIdentity = ident;
                    burst.ServiceType = type;
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
            
            Parameters.AddUsedBurst(Burst);

            /* type 4 is in timeslot 0 and shared with BCCH, CCCH and SDCCH */
            if (channelType == 4)
                frame = 20 + 2 + (subChannel / 2) * 10 + (subChannel & 1) * 4;
            else
                frame = 4 * subChannel;

            /* call generic trigger to set up arrays */
            TriggerChannelAssignment(L3Handler);
            AddMessage("  [__] TimeSlot " + timeSlot + " SubChannel " + subChannel + " now configured as Cell Broadcast channel." + Environment.NewLine);

            lock (Parameters.TimeSlotConfig)
            {
                if (Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers == null)
                {
                    Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers = new sTimeSlotParam[51];
                    Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers = new sTimeSlotParam[51];
                }

                for (long pos = frame; pos < frame + 4; pos++)
                {
                    ReleaseBurst(Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Uplink][timeSlot].Handlers[pos].Burst);
                    ReleaseBurst(Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[pos].Burst);
                }
                Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame + 0] = new sTimeSlotParam(Burst, 0);
                Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame + 1] = new sTimeSlotParam(Burst, 1);
                Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame + 2] = new sTimeSlotParam(Burst, 2);
                Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink][timeSlot].Handlers[frame + 3] = new sTimeSlotParam(Burst, 3);
           }
        }

        private void TriggerCCCHCONF(L3Handler L3Handler)
        {
            long ccchConf;

            if (Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)Parameters.Dir][0].Type != eTimeSlotType.Unconfigured)
                return;

            lock (L3Handler.PDUDataFields)
            {
                if (!L3Handler.PDUDataRawFields.ContainsKey("CCCH-CONF"))
                    return;
                ccchConf = L3Handler.PDUDataRawFields["CCCH-CONF"];
            }

            lock (Parameters.TimeSlotConfig)
            {
                sTimeSlotInfo[] infoUp = Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Uplink];
                sTimeSlotInfo[] infoDown = Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)eLinkDirection.Downlink];

                switch (ccchConf)
                {
                    /* 1 basic physical channel used for CCCH, not combined with SDCCHs */
                    case 0:
                        infoDown[0].Type = eTimeSlotType.BCCH_CCCH;
                        infoDown[0].Configures++;

                        for (int block = 1; block < 5; block++)
                        {
                            infoDown[0].Handlers[block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            infoDown[0].Handlers[block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            infoDown[0].Handlers[block * 10 + 2] = new sTimeSlotParam(CCCH, 0);
                            infoDown[0].Handlers[block * 10 + 3] = new sTimeSlotParam(CCCH, 1);
                            infoDown[0].Handlers[block * 10 + 4] = new sTimeSlotParam(CCCH, 2);
                            infoDown[0].Handlers[block * 10 + 5] = new sTimeSlotParam(CCCH, 3);
                            infoDown[0].Handlers[block * 10 + 6] = new sTimeSlotParam(CCCH, 0);
                            infoDown[0].Handlers[block * 10 + 7] = new sTimeSlotParam(CCCH, 1);
                            infoDown[0].Handlers[block * 10 + 8] = new sTimeSlotParam(CCCH, 2);
                            infoDown[0].Handlers[block * 10 + 9] = new sTimeSlotParam(CCCH, 3);
                        }
                        break;

                    /* 1 basic physical channel used for CCCH, combined with SDCCHs */
                    case 1:

                        infoDown[0].Type = eTimeSlotType.BCCH_CCCH_SDCCH4;
                        infoDown[0].Configures++;
                        infoUp[0].Type = eTimeSlotType.BCCH_CCCH_SDCCH4;
                        infoUp[0].Configures++;

                        /* setup the first block */
                        for (int block = 1; block < 2; block++)
                        {
                            infoDown[0].Handlers[block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            infoDown[0].Handlers[block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            infoDown[0].Handlers[block * 10 + 2] = new sTimeSlotParam(CCCH, 0);
                            infoDown[0].Handlers[block * 10 + 3] = new sTimeSlotParam(CCCH, 1);
                            infoDown[0].Handlers[block * 10 + 4] = new sTimeSlotParam(CCCH, 2);
                            infoDown[0].Handlers[block * 10 + 5] = new sTimeSlotParam(CCCH, 3);
                            infoDown[0].Handlers[block * 10 + 6] = new sTimeSlotParam(CCCH, 0);
                            infoDown[0].Handlers[block * 10 + 7] = new sTimeSlotParam(CCCH, 1);
                            infoDown[0].Handlers[block * 10 + 8] = new sTimeSlotParam(CCCH, 2);
                            infoDown[0].Handlers[block * 10 + 9] = new sTimeSlotParam(CCCH, 3);
                        }

                        /* then two SDCCH blocks. they will get configured on assignment */
                        for (int block = 2; block < 4; block++)
                        {
                            /* FCH/SCH too */
                            infoDown[0].Handlers[block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            infoDown[0].Handlers[block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            
#if false
                            SDCCHBurst tmpSDCCH1Down = new SDCCHBurst(L3, (block - 2) * 2);
                            SDCCHBurst tmpSDCCH2Down = new SDCCHBurst(L3, (block - 2) * 2 + 1);
                            SDCCHBurst tmpSDCCH1Up = new SDCCHBurst(L3, (block - 2) * 2);
                            SDCCHBurst tmpSDCCH2Up = new SDCCHBurst(L3, (block - 2) * 2 + 1);

                            tmpSDCCH1Down.TimeSlot = 0;
                            tmpSDCCH2Down.TimeSlot = 0;
                            tmpSDCCH1Up.TimeSlot = 0;
                            tmpSDCCH2Up.TimeSlot = 0;

                            tmpSDCCH1Down.Direction = eLinkDirection.Downlink;
                            tmpSDCCH2Down.Direction = eLinkDirection.Downlink;
                            tmpSDCCH1Up.Direction = eLinkDirection.Uplink;
                            tmpSDCCH2Up.Direction = eLinkDirection.Uplink;

                            RegisterActiveBurst(tmpSDCCH1Down);
                            RegisterActiveBurst(tmpSDCCH2Down);
                            RegisterActiveBurst(tmpSDCCH1Up);
                            RegisterActiveBurst(tmpSDCCH2Up);

                            infoDown[0].Handlers[block * 10 + 2] = new sTimeSlotParam(tmpSDCCH1Down, 0);
                            infoDown[0].Handlers[block * 10 + 3] = new sTimeSlotParam(tmpSDCCH1Down, 1);
                            infoDown[0].Handlers[block * 10 + 4] = new sTimeSlotParam(tmpSDCCH1Down, 2);
                            infoDown[0].Handlers[block * 10 + 5] = new sTimeSlotParam(tmpSDCCH1Down, 3);

                            infoDown[0].Handlers[block * 10 + 6] = new sTimeSlotParam(tmpSDCCH2Down, 0);
                            infoDown[0].Handlers[block * 10 + 7] = new sTimeSlotParam(tmpSDCCH2Down, 1);
                            infoDown[0].Handlers[block * 10 + 8] = new sTimeSlotParam(tmpSDCCH2Down, 2);
                            infoDown[0].Handlers[block * 10 + 9] = new sTimeSlotParam(tmpSDCCH2Down, 3);

                            infoUp[0].Handlers[block * 10 + 2] = new sTimeSlotParam(tmpSDCCH1Up, 0);
                            infoUp[0].Handlers[block * 10 + 3] = new sTimeSlotParam(tmpSDCCH1Up, 1);
                            infoUp[0].Handlers[block * 10 + 4] = new sTimeSlotParam(tmpSDCCH1Up, 2);
                            infoUp[0].Handlers[block * 10 + 5] = new sTimeSlotParam(tmpSDCCH1Up, 3);

                            infoUp[0].Handlers[block * 10 + 6] = new sTimeSlotParam(tmpSDCCH2Up, 0);
                            infoUp[0].Handlers[block * 10 + 7] = new sTimeSlotParam(tmpSDCCH2Up, 1);
                            infoUp[0].Handlers[block * 10 + 8] = new sTimeSlotParam(tmpSDCCH2Up, 2);
                            infoUp[0].Handlers[block * 10 + 9] = new sTimeSlotParam(tmpSDCCH2Up, 3);
#endif
                        }

                        /* finally one SACCH block */
                        for (int block = 4; block < 5; block++)
                        {
                            /* FCH/SCH too */
                            infoDown[0].Handlers[block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            infoDown[0].Handlers[block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            
#if false
                            SACCHBurst tmpSACCH1Down = new SACCHBurst(L3, "SACCH 0/2", 0);
                            SACCHBurst tmpSACCH2Down = new SACCHBurst(L3, "SACCH 1/3", 1);
                            SACCHBurst tmpSACCH1Up = new SACCHBurst(L3, "SACCH 0/2", 0);
                            SACCHBurst tmpSACCH2Up = new SACCHBurst(L3, "SACCH 1/3", 1);

                            tmpSACCH1Down.TimeSlot = 0;
                            tmpSACCH2Down.TimeSlot = 0;
                            tmpSACCH1Up.TimeSlot = 0;
                            tmpSACCH2Up.TimeSlot = 0;

                            tmpSACCH1Down.Direction = eLinkDirection.Downlink;
                            tmpSACCH2Down.Direction = eLinkDirection.Downlink;
                            tmpSACCH1Up.Direction = eLinkDirection.Uplink;
                            tmpSACCH2Up.Direction = eLinkDirection.Uplink;

                            RegisterActiveBurst(tmpSACCH1Down);
                            RegisterActiveBurst(tmpSACCH2Down);
                            RegisterActiveBurst(tmpSACCH1Up);
                            RegisterActiveBurst(tmpSACCH2Up);

                            infoDown[0].Handlers[block * 10 + 2] = new sTimeSlotParam(tmpSACCH1Down, 0);
                            infoDown[0].Handlers[block * 10 + 3] = new sTimeSlotParam(tmpSACCH1Down, 1);
                            infoDown[0].Handlers[block * 10 + 4] = new sTimeSlotParam(tmpSACCH1Down, 2);
                            infoDown[0].Handlers[block * 10 + 5] = new sTimeSlotParam(tmpSACCH1Down, 3);

                            infoDown[0].Handlers[block * 10 + 6] = new sTimeSlotParam(tmpSACCH2Down, 0);
                            infoDown[0].Handlers[block * 10 + 7] = new sTimeSlotParam(tmpSACCH2Down, 1);
                            infoDown[0].Handlers[block * 10 + 8] = new sTimeSlotParam(tmpSACCH2Down, 2);
                            infoDown[0].Handlers[block * 10 + 9] = new sTimeSlotParam(tmpSACCH2Down, 3);

                            infoUp[0].Handlers[block * 10 + 2] = new sTimeSlotParam(tmpSACCH1Up, 0);
                            infoUp[0].Handlers[block * 10 + 3] = new sTimeSlotParam(tmpSACCH1Up, 1);
                            infoUp[0].Handlers[block * 10 + 4] = new sTimeSlotParam(tmpSACCH1Up, 2);
                            infoUp[0].Handlers[block * 10 + 5] = new sTimeSlotParam(tmpSACCH1Up, 3);

                            infoUp[0].Handlers[block * 10 + 6] = new sTimeSlotParam(tmpSACCH2Up, 0);
                            infoUp[0].Handlers[block * 10 + 7] = new sTimeSlotParam(tmpSACCH2Up, 1);
                            infoUp[0].Handlers[block * 10 + 8] = new sTimeSlotParam(tmpSACCH2Up, 2);
                            infoUp[0].Handlers[block * 10 + 9] = new sTimeSlotParam(tmpSACCH2Up, 3);
#endif
                        }
                        break;

                    /* 2 basic physical channels used for CCCH, not combined with SDCCHs */
                    case 2:

                        infoDown[0].Type = eTimeSlotType.BCCH_CCCH;
                        infoDown[1].Type = eTimeSlotType.BCCH_CCCH;
                        infoDown[0].Configures++;
                        infoDown[1].Configures++;

                        /* timeslot 0 already has BCCH etc, so just fill that one */
                        for (int block = 1; block < 4; block++)
                        {
                            infoDown[0].Handlers[block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            infoDown[0].Handlers[block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            infoDown[0].Handlers[block * 10 + 2] = new sTimeSlotParam(CCCH, 0);
                            infoDown[0].Handlers[block * 10 + 3] = new sTimeSlotParam(CCCH, 1);
                            infoDown[0].Handlers[block * 10 + 4] = new sTimeSlotParam(CCCH, 2);
                            infoDown[0].Handlers[block * 10 + 5] = new sTimeSlotParam(CCCH, 3);
                            infoDown[0].Handlers[block * 10 + 6] = new sTimeSlotParam(CCCH, 0);
                            infoDown[0].Handlers[block * 10 + 7] = new sTimeSlotParam(CCCH, 1);
                            infoDown[0].Handlers[block * 10 + 8] = new sTimeSlotParam(CCCH, 2);
                            infoDown[0].Handlers[block * 10 + 9] = new sTimeSlotParam(CCCH, 3);
                        }

                        /* now fill with BCCH+CCCH */
                        for (int slot = 1; slot < 2; slot++)
                        {
                            infoDown[slot].Handlers[0] = new sTimeSlotParam(BCCH, 0);
                            infoDown[slot].Handlers[1] = new sTimeSlotParam(BCCH, 1);
                            infoDown[slot].Handlers[2] = new sTimeSlotParam(BCCH, 2);
                            infoDown[slot].Handlers[3] = new sTimeSlotParam(BCCH, 3);

                            for (int block = 1; block < 12; block++)
                            {
                                infoDown[slot].Handlers[block * 4 + 0] = new sTimeSlotParam(CCCH, 0);
                                infoDown[slot].Handlers[block * 4 + 1] = new sTimeSlotParam(CCCH, 1);
                                infoDown[slot].Handlers[block * 4 + 2] = new sTimeSlotParam(CCCH, 2);
                                infoDown[slot].Handlers[block * 4 + 3] = new sTimeSlotParam(CCCH, 3);
                            }
                        }

                        break;

                    /* 3 basic physical channels used for CCCH, not combined with SDCCHs */
                    case 3:

                        infoDown[0].Type = eTimeSlotType.BCCH_CCCH;
                        infoDown[1].Type = eTimeSlotType.BCCH_CCCH;
                        infoDown[2].Type = eTimeSlotType.BCCH_CCCH;
                        infoDown[0].Configures++;
                        infoDown[1].Configures++;
                        infoDown[2].Configures++;

                        /* timeslot 0 already has BCCH etc, so just fill that one */
                        for (int block = 1; block < 4; block++)
                        {
                            infoDown[0].Handlers[block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            infoDown[0].Handlers[block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            infoDown[0].Handlers[block * 10 + 2] = new sTimeSlotParam(CCCH, 0);
                            infoDown[0].Handlers[block * 10 + 3] = new sTimeSlotParam(CCCH, 1);
                            infoDown[0].Handlers[block * 10 + 4] = new sTimeSlotParam(CCCH, 2);
                            infoDown[0].Handlers[block * 10 + 5] = new sTimeSlotParam(CCCH, 3);
                            infoDown[0].Handlers[block * 10 + 6] = new sTimeSlotParam(CCCH, 0);
                            infoDown[0].Handlers[block * 10 + 7] = new sTimeSlotParam(CCCH, 1);
                            infoDown[0].Handlers[block * 10 + 8] = new sTimeSlotParam(CCCH, 2);
                            infoDown[0].Handlers[block * 10 + 9] = new sTimeSlotParam(CCCH, 3);
                        }

                        /* now fill with BCCH+CCCH */
                        for (int slot = 1; slot < 3; slot++)
                        {
                            infoDown[slot].Handlers[0] = new sTimeSlotParam(BCCH, 0);
                            infoDown[slot].Handlers[1] = new sTimeSlotParam(BCCH, 1);
                            infoDown[slot].Handlers[2] = new sTimeSlotParam(BCCH, 2);
                            infoDown[slot].Handlers[3] = new sTimeSlotParam(BCCH, 3);

                            for (int block = 1; block < 12; block++)
                            {
                                infoDown[slot].Handlers[block * 4 + 0] = new sTimeSlotParam(CCCH, 0);
                                infoDown[slot].Handlers[block * 4 + 1] = new sTimeSlotParam(CCCH, 1);
                                infoDown[slot].Handlers[block * 4 + 2] = new sTimeSlotParam(CCCH, 2);
                                infoDown[slot].Handlers[block * 4 + 3] = new sTimeSlotParam(CCCH, 3);
                            }
                        }

                        break;

                    /* 4 basic physical channels used for CCCH, not combined with SDCCHs */
                    case 4:
                        infoDown[0].Type = eTimeSlotType.BCCH_CCCH;
                        infoDown[1].Type = eTimeSlotType.BCCH_CCCH;
                        infoDown[2].Type = eTimeSlotType.BCCH_CCCH;
                        infoDown[3].Type = eTimeSlotType.BCCH_CCCH;
                        infoDown[0].Configures++;
                        infoDown[1].Configures++;
                        infoDown[2].Configures++;
                        infoDown[3].Configures++;


                        /* timeslot 0 already has BCCH etc, so just fill that one */
                        for (int block = 1; block < 4; block++)
                        {
                            infoDown[0].Handlers[block * 10 + 0] = new sTimeSlotParam(FCH, 0);
                            infoDown[0].Handlers[block * 10 + 1] = new sTimeSlotParam(SCH, 0);
                            infoDown[0].Handlers[block * 10 + 2] = new sTimeSlotParam(CCCH, 0);
                            infoDown[0].Handlers[block * 10 + 3] = new sTimeSlotParam(CCCH, 1);
                            infoDown[0].Handlers[block * 10 + 4] = new sTimeSlotParam(CCCH, 2);
                            infoDown[0].Handlers[block * 10 + 5] = new sTimeSlotParam(CCCH, 3);
                            infoDown[0].Handlers[block * 10 + 6] = new sTimeSlotParam(CCCH, 0);
                            infoDown[0].Handlers[block * 10 + 7] = new sTimeSlotParam(CCCH, 1);
                            infoDown[0].Handlers[block * 10 + 8] = new sTimeSlotParam(CCCH, 2);
                            infoDown[0].Handlers[block * 10 + 9] = new sTimeSlotParam(CCCH, 3);
                        }

                        /* now fill with BCCH+CCCH */
                        for (int slot = 1; slot < 4; slot++)
                        {
                            infoDown[slot].Handlers[0] = new sTimeSlotParam(BCCH, 0);
                            infoDown[slot].Handlers[1] = new sTimeSlotParam(BCCH, 1);
                            infoDown[slot].Handlers[2] = new sTimeSlotParam(BCCH, 2);
                            infoDown[slot].Handlers[3] = new sTimeSlotParam(BCCH, 3);

                            for (int block = 1; block < 12; block++)
                            {
                                infoDown[slot].Handlers[block * 4 + 0] = new sTimeSlotParam(CCCH, 0);
                                infoDown[slot].Handlers[block * 4 + 1] = new sTimeSlotParam(CCCH, 1);
                                infoDown[slot].Handlers[block * 4 + 2] = new sTimeSlotParam(CCCH, 2);
                                infoDown[slot].Handlers[block * 4 + 3] = new sTimeSlotParam(CCCH, 3);
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
            if (Parameters.CurrentBurstHandler != null && (Parameters.CurrentBurstHandler.GetType() == typeof(SACCHBurst) || Parameters.CurrentBurstHandler.GetType() == typeof(SDCCHBurst)))
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
            sTimeSlotParam[] param = Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)Parameters.Dir][Parameters.TN].Handlers;


            lock (Parameters.TimeSlotConfig)
            {
                if (param == null)
                {
                    handler = null;
                    sequence = 0;
                }
                else
                {
                    if (param.Length == 51)
                    {
                        frameNum = Parameters.T3; // control frame number
                    }
                    else if (param.Length == 26)
                    {
                        frameNum = Parameters.T2; // traffic frame number
                    }

                    handler = param[frameNum].Burst;
                    sequence = param[frameNum].Sequence;
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
                bool important = (handler is SCHBurst) || ((handler is BCCHBurst) && Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)Parameters.Dir][0].Configures == 0);

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
            string layerPrefix = "";

            switch (Parameters.Dir)
            {
                case eLinkDirection.Uplink:
                    layerPrefix = "  [L1,U]";
                    break;

                case eLinkDirection.Downlink:
                    layerPrefix = "  [L1,D]";
                    break;
            }

            sTimeSlotParam[] param = Parameters.TimeSlotConfig[Parameters.ARFCNidx, (int)(int)Parameters.Dir][Parameters.TN].Handlers;


            lock (Parameters.TimeSlotConfig)
            {
                if (param == null)
                {
                    handler = null;
                    sequence = 0;
                }
                else
                {
                    if (param.Length == 51)
                    {
                        frameNum = Parameters.T3; // control frame number
                    }
                    else if (param.Length == 26)
                    {
                        frameNum = Parameters.T2; // traffic frame number
                    }
                    handler = param[frameNum].Burst;
                    sequence = param[frameNum].Sequence;
                }

                Parameters.CurrentBurstHandler = handler;
            }


            if (Burst.DumpRawData)
            {
                if (handler != null)
                    AddMessage(layerPrefix + " Handler: " + handler.Name + "[" + sequence + "]  TN:" + Parameters.TN + "  Frame: " + frameNum + "  FN: " + sequence + Environment.NewLine);
                else
                    AddMessage(layerPrefix + " Handler: (none)  TN:" + Parameters.TN + "  Frame: " + frameNum + "  FN: " + sequence + Environment.NewLine);
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
                        AddMessage(layerPrefix + " [" + handler.Name + "] - [Burst: " + burstId + "] [" + Parameters + "]" + Environment.NewLine);
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
                        AddMessage(layerPrefix + " [ARFCN: " + Parameters.ARFCN + "] ");
                        AddMessage("[Burst: " + burstId + "] ");
                        AddMessage("[MCC: " + Parameters.MCC + "] ");
                        AddMessage("[MNC: " + Parameters.MNC + "] ");
                        AddMessage("[LAC: " + Parameters.LAC + "] ");
                        AddMessage("[CellID: " + Parameters.CellIdent + "] ");
                        AddMessage(Environment.NewLine);
                        AddMessage(layerPrefix + " [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);
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