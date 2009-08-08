using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GSM.Misc;

namespace LibRXFFT.Libraries.GSM.Layer3
{
    public enum eProtocol
    {
        GroupCallControl = 0,
        BroadcastCallControl = 1,
        PDSS1 = 2,
        CallControl = 3,
        PDSS2 = 4,
        MobilityManagement = 5,
        RadioResource = 6,
        Undefined_7 = 7,
        GRPSMobility = 8,
        SMSMessage = 9,
        GPRSSessionManagement = 10,
        SSMessage = 11,
        LocationServices = 12,
        Undefined_13 = 13,
        Reserved1 = 14,
        ReservedTest = 15
    }

    public class L3Handler
    {
        public static L3PacketTypes L3PacketTypesRR;
        public static L3PacketTypes L3PacketTypesCC;
        public static L3PacketTypes L3PacketTypesMM;

        public static L3Messages L3Messages;
        public static L3PDUList L3PduList;
        public static MCCTable MCCTable;
        public static MNCTable MNCTable;
        


        public static bool SniffIMSI = false;
        public string SniffResult = null;

        public static bool ExceptFieldsEnabled = true;
        public static Dictionary<string, bool> SkipMessages = new Dictionary<string, bool>();
        public static Dictionary<string, bool> ExceptFields = new Dictionary<string, bool>();

        private bool ShowMessage = false;
        public string StatusMessage = null;

        private readonly Dictionary<string, handlePDUDelegate> PDUParser = new Dictionary<string, handlePDUDelegate>();
        public static bool DumpRawData = false;
        public static bool DumpUnhandled = false;

        private delegate string[] handlePDUDelegate(byte[] pduData);


        public readonly Dictionary<string, handleTrigger> PDUDataTriggers = new Dictionary<string, handleTrigger>();
        public readonly Dictionary<string, string> PDUDataFields = new Dictionary<string, string>();
        public readonly Dictionary<string, long> PDUDataRawFields = new Dictionary<string, long>();

        public readonly Dictionary<string, fieldParser> PDUFieldParsers = new Dictionary<string, fieldParser>();

        public delegate string fieldParser(L3Handler handler, long value);
        public delegate void handleTrigger(L3Handler handler);

        public L3Handler()
        {
            try
            {
                ReloadFiles();
            }
            catch (FileNotFoundException e)
            {
                StatusMessage = "Error! Could not load needed XML files." + Environment.NewLine + Environment.NewLine + e;
                return;
            }

            PDUParser.Add("10.5.1.4", HandleMobileIdentity);
            PDUParser.Add("10.5.2.1b", HandleCellChannelDescription);
            PDUParser.Add("10.5.2.22", HandleCellChannelDescription);
            PDUParser.Add("10.5.2.5", HandleChannelDescription);
            PDUParser.Add("10.5.2.5a", HandleChannelDescription);

            PDUFieldParsers.Add("ParseRA", ParseRA);
        }

        public static void ReloadFiles()
        {
            L3PacketTypesMM = new L3PacketTypes("packeteering-mm.xml");
            L3PacketTypesCC = new L3PacketTypes("packeteering-cc.xml");
            L3PacketTypesRR = new L3PacketTypes("packeteering-rr.xml");
            L3Messages = new L3Messages("messagelist.xml");
            L3PduList = new L3PDUList("pdulist.xml");
            MCCTable = new MCCTable("mccentries.xml");
            MNCTable = new MNCTable("mncentries.xml");
        }

        private string ParseRA(L3Handler handler, long value)
        {
            string retVal = "unknown";

            if (!PDUDataRawFields.ContainsKey("NECI") || PDUDataRawFields["NECI"] == 0)
            {
                switch (value >> 5)
                {
                    case 0:
                        retVal = "Other procedures (SDCCH)";
                        break;
                    case 1:
                        retVal = "Answer to paging, Dual rate MS and TCH/F is requested (TCH/F or SDCCH)";
                        break;
                    case 2:
                        retVal = "unknown type 010xxxxx";
                        break;
                    case 3:
                        retVal = "Answer to paging, Dual rate MS and TCH/H or TCH/F is requested (TCH/H, TCH/F or SDCCH)";
                        break;
                    case 4:
                        retVal = "Answer to paging (SDCCH)";
                        break;
                    case 5:
                        retVal = "Emergency call (SDCCH)";
                        lock (ExceptFields)
                        {
                            if (ExceptFields.ContainsKey("Emergency call"))
                                ShowMessage = true;
                        }
                        break;
                    case 6:
                        retVal = "Call re-establishment (SDCCH)";
                        break;
                    case 7:
                        retVal = "Originating call (SDCCH)";
                        break;
                }
            }
            else
            {
                switch (value >> 5)
                {
                    case 0:
                        switch ((value >> 4) & 1)
                        {
                            case 0:
                                retVal = "Location updating (SDCCH)";
                                break;
                            case 1:
                                retVal = "Other procedures (SDCCH)";
                                break;
                        }
                        break;
                    case 1:
                        switch ((value >> 4) & 1)
                        {
                            case 0:
                                retVal = "Answer to paging, Dual rate MS and TCH/F is requested (TCH/F or SDCCH)";
                                break;
                            case 1:
                                retVal = "Answer to paging, Dual rate MS and TCH/H or TCH/F is requested (TCH/H, TCH/F or SDCCH)";
                                break;
                        }
                        break;
                    case 2:
                        switch ((value >> 4) & 1)
                        {
                            case 0:
                                retVal = "Originating speech call from dual rate MS when TCH/H is sufficient (TCH/H, TCH/F or SDCCH)";
                                break;
                            case 1:
                                retVal = "Originating data call from dual rate MS when TCH/H is sufficient (TCH/H, TCH/F or SDCCH)";
                                break;
                        }
                        break;
                    case 3:
                        if (((value >> 2) & 7) == 2)
                            retVal = "Call re-establishment; TCH/H was in use. (TCH/H, TCH/F or SDCCH)";
                        else
                            retVal = "unknown type";
                        break;
                    case 4:
                        retVal = "Answer to paging (SDCCH)";
                        break;
                    case 5:
                        retVal = "Emergency call (SDCCH)";
                        lock (ExceptFields)
                        {
                            if (ExceptFields.ContainsKey("Emergency call"))
                                ShowMessage = true;
                        }
                        break;
                    case 6:
                        retVal = "Call re-establishment (SDCCH)";
                        break;
                    case 7:
                        retVal = "Originating call (SDCCH)";
                        break;
                }
            }

            return retVal;
        }

        private string[] HandleCellChannelDescription(byte[] pduData)
        {
            string[] msg = new string[3];
            string typeString = "Unknown";
            int type0 = pduData[0] >> 6;
            int type1 = (pduData[0] >> 3) & 1;
            int type2 = (pduData[0] >> 1) & 3;

            bool extInd = ((pduData[0] >> 5) & 1) == 1;
            bool baInd = ((pduData[0] >> 4) & 1) == 1;

            msg[0] = "EXT-IND             ";
            if (!extInd)
                msg[0] += "The information element carries the complete information of a BCCH channel sub list";
            else
                msg[0] += "The information element carries only a part of the BCCH channel sub list";

            msg[1] = "BA-IND              ";
            if (!baInd)
                msg[1] += "The information element carries the complete BA";
            else
                msg[1] += "The information element carries only a part of the BA";


            switch (type0)
            {
                case 0:
                    msg[2] = "ARFCNs              ";

                    for (int arfcn = 124; arfcn > 0; arfcn--)
                    {
                        int byteNum = 16 - (arfcn + 7) / 8;
                        int bitNum = 7 - ((128 - arfcn) % 8);

                        if (((pduData[byteNum] >> bitNum) & 1) == 1)
                            msg[2] += arfcn + " ";
                    }

                    return msg;


                case 2:
                    switch (type1)
                    {
                        case 0:
                            typeString = "1024 range";
                            break;

                        case 1:
                            switch (type2)
                            {
                                case 0:
                                    typeString = "512 range";
                                    break;
                                case 1:
                                    typeString = "256 range";
                                    break;
                                case 2:
                                    typeString = "128 range";
                                    break;
                                case 3:
                                    typeString = "variable bit map";
                                    break;
                            }
                            break;

                    }
                    break;
            }

            return new[] { "This Cell Channel Description Type (" + typeString + ") is not implemented yet." };
        }

        private string[] HandleChannelDescription(byte[] pduData)
        {
            string[] msg = new string[5];
            string chanType = "Reserved";
            int subChan = -1;
            int timeSlot = -1;

            int type = pduData[0] >> 3;

            msg[0] = "Channel             ";

            if ((type & 0x1F) == 1)
            {
                chanType = "TCH/F + ACCHs";
                subChan = -1;
            }
            else if ((type & 0x1E) == 2)
            {
                chanType = "TCH/H + ACCHs";
                subChan = (type & 1);
                type &= 0x1E;
            }
            else if ((type & 0x1C) == 4)
            {
                chanType = "SDCCH/4 + SACCH/C4 or CBCH (SDCCH/4)";
                subChan = (type & 3);
                type &= 0x1C;
            }
            else if ((type & 0x18) == 8)
            {
                chanType = "SDCCH/8 + SACCH/C8 or CBCH (SDCCH/8)";
                subChan = (type & 7);
                type &= 0x18;
            }

            msg[0] += chanType;
            if (subChan >= 0)
                msg[0] += ", Subchan " + subChan;

            msg[1] = "Timeslot Number     ";
            timeSlot = pduData[0] & 7;
            msg[1] += timeSlot;


            lock (PDUDataFields)
            {
                /* text values */
                if (PDUDataFields.ContainsKey("ChannelType"))
                    PDUDataFields["ChannelType"] = chanType;
                else
                    PDUDataFields.Add("ChannelType", chanType);

                if (PDUDataFields.ContainsKey("SubChannel"))
                    PDUDataFields["SubChannel"] = subChan.ToString();
                else
                    PDUDataFields.Add("SubChannel", subChan.ToString());

                if (PDUDataFields.ContainsKey("TimeSlot"))
                    PDUDataFields["TimeSlot"] = timeSlot.ToString();
                else
                    PDUDataFields.Add("TimeSlot", timeSlot.ToString());

                /* raw values */
                if (PDUDataRawFields.ContainsKey("ChannelType"))
                    PDUDataRawFields["ChannelType"] = type;
                else
                    PDUDataRawFields.Add("ChannelType", type);

                if (PDUDataRawFields.ContainsKey("SubChannel"))
                    PDUDataRawFields["SubChannel"] = subChan;
                else
                    PDUDataRawFields.Add("SubChannel", subChan);

                if (PDUDataRawFields.ContainsKey("TimeSlot"))
                    PDUDataRawFields["TimeSlot"] = timeSlot;
                else
                    PDUDataRawFields.Add("TimeSlot", timeSlot);

            }


            msg[2] = "TSC                 ";
            msg[2] += (pduData[1] >> 5) & 7;

            bool hopping = (pduData[1] & 0x10) == 0x10;
            msg[2] = "Hopping             ";

            if (hopping)
                msg[2] += "RF hopping channel";
            else
                msg[2] += "Single RF channel";

            if (hopping)
            {
                long maio = ((pduData[1] & 0x0F) << 2) | ((pduData[2] & 0x03) >> 6);
                long hsn = pduData[2] & 0x3F;
                msg[3] = "MAIO                ";
                msg[3] += maio;
                msg[4] = "HSN                 ";
                msg[4] += hsn;
            }
            else
            {
                long arfcn = ((pduData[1] & 3) << 8) | pduData[2];
                msg[3] = "ARFCN               ";
                msg[3] += arfcn;
            }

            return msg;
        }


        private string[] HandleMobileIdentity(byte[] pduData)
        {
            int type = pduData[0] & 0x07;

            if (type == 0)
                return new[] { "Type                None" };

            string imsi = "";
            string[] msg = new string[2];
            string[] typeDesc = new[]
                                    {
                                        "None",
                                        "IMSI",
                                        "IMEI",
                                        "IMEISV",
                                        "TMSI/P-TMSI",
                                        "Unknown",
                                        "Unknown",
                                        "Unknown"
                                    };

            lock (ExceptFields)
            {
                if (ExceptFields.ContainsKey(typeDesc[type]))
                    ShowMessage = true;
            }


            msg[0] = "Type                ";
            msg[0] += typeDesc[type];

            msg[1] = String.Format("{0,-20}", typeDesc[pduData[0] & 0x07]);

            for (int pos = 1; pos < pduData.Length * 2 - 1; pos++)
            {
                int byteNum = pos / 2;
                int digit;

                if ((pos & 1) == 0)
                    digit = pduData[byteNum] & 0x0F;
                else
                    digit = pduData[byteNum] >> 4;

                switch (type)
                {
                    case 1:
                        if (digit != 15)
                            imsi += digit;
                        break;

                    case 4:
                        if (pos > 1)
                            msg[1] += String.Format("{0:X01}", digit);
                        break;

                    case 0:
                    case 2:
                    case 3:
                    case 5:
                    case 6:
                    case 7:
                        if (digit == 15)
                            msg[1] += "_";
                        else
                            msg[1] += digit;
                        break;
                }
            }

            if (type == 1)
            {
                string imsiString = imsi;

                string mcc = imsi.Substring(0, 3);
                string mnc2 = imsi.Substring(3, 2);
                string mnc3 = imsi.Substring(3, 3);

                MCCEntry countryEntry = MCCTable.Get(mcc);
                MNCEntry netEntry2 = MNCTable.Get(mcc, mnc2);
                MNCEntry netEntry3 = MNCTable.Get(mcc, mnc3);

                if (countryEntry != null)
                    imsiString += "  " + countryEntry.CC + " (" + countryEntry.Country + ")";

                if (netEntry2 != null)
                    imsiString += " (" + netEntry2.Network + ")";

                if (netEntry3 != null)
                    imsiString += " (" + netEntry3.Network + ")";


                msg[1] += imsiString;

                if (SniffIMSI)
                {
                    if (SniffResult == null)
                        SniffResult = imsiString;
                    else
                        SniffResult += ", " + imsiString;
                }
            }


            return msg;
        }

        private string[] HandleGenericPDU(ArrayList fields, byte[] pduData)
        {
            int pos = 0;
            string[] msg = new string[fields.Count];

            foreach (L3PDUField field in fields)
            {
                /* trigger as requested */
                if (!string.IsNullOrEmpty(field.TriggerPre))
                {
                    if (PDUDataTriggers.ContainsKey(field.TriggerPre))
                        PDUDataTriggers[field.TriggerPre](this);
                }

                msg[pos] = String.Format("{0,-20}", field.Name);
                long value = 0;

                foreach (Bits bits in field.Bits)
                {
                    int mask = (1 << bits.Count) - 1;

                    value <<= bits.Count;
                    value |= (pduData[bits.Octet - 1] >> (bits.Start - 1)) & mask;
                }

                string pduFieldData = "";
                switch (field.Type)
                {
                    case "hide":
                        break;

                    case "integer":
                        pduFieldData += field.Offset + field.Factor * value;
                        break;

                    case "bcd":
                        for (int digit = 0; digit < field.Length; digit++)
                        {
                            long bcd = (value >> (4 * (field.Length - 1 - digit))) & 0x0F;
                            if (bcd < 10)
                                pduFieldData += bcd;
                            else
                                pduFieldData += " ";
                        }
                        break;

                    case "hexnumber":
                        pduFieldData += String.Format("{0:X}", value);
                        break;

                    case "stringIndex":
                        if (value < field.Strings.Length && !string.IsNullOrEmpty(field.Strings[value]))
                            pduFieldData += field.Strings[value];
                        else
                            pduFieldData += "Unknown entry: " + value;
                        break;
                }

                msg[pos] += pduFieldData;

                if (!string.IsNullOrEmpty(field.Parser) && PDUFieldParsers.ContainsKey(field.Parser))
                    msg[pos] += " (" + PDUFieldParsers[field.Parser](this, value) + ")";

                /* update the fields in the field list */
                if (!string.IsNullOrEmpty(field.SetField))
                {
                    lock (PDUDataFields)
                    {
                        if (PDUDataFields.ContainsKey(field.SetField))
                            PDUDataFields[field.SetField] = pduFieldData;
                        else
                            PDUDataFields.Add(field.SetField, pduFieldData);

                        if (PDUDataRawFields.ContainsKey(field.SetField))
                            PDUDataRawFields[field.SetField] = value;
                        else
                            PDUDataRawFields.Add(field.SetField, value);
                    }
                }

                /* trigger as requested */
                if (!string.IsNullOrEmpty(field.TriggerPost))
                {
                    if (PDUDataTriggers.ContainsKey(field.TriggerPost))
                        PDUDataTriggers[field.TriggerPost](this);
                }


                pos++;
            }

            return msg;
        }


        private string DecodePacket(byte[] l3Data, int start, string reference)
        {
            /* half-octet based indexing */
            int currentPos = start * 2;
            string text = "";

            /* skip the pseudo-L2 headers */
            string[] skipPDUs = new[] { "10.5.2.19", "10.2", "10.3.1", "10.3.2", "10.4", "10.5.1.8" };

            L3MessageInfo l3Message = L3Messages.Get(reference);
            if (l3Message == null)
            {
                byte[] pduData = new byte[l3Data.Length - start];
                Array.Copy(l3Data, start, pduData, 0, pduData.Length);

                text += "          Unspecified Message Type: " + reference + Environment.NewLine;
                text += "             " + DumpBytes(pduData) + Environment.NewLine;
                return text;
            }

            /* trigger as requested */
            if (!string.IsNullOrEmpty(l3Message.TriggerPre))
            {
                if (PDUDataTriggers.ContainsKey(l3Message.TriggerPre))
                    PDUDataTriggers[l3Message.TriggerPre](this);
            }

            foreach (L3MessageSlotInfo slotInfo in l3Message.Slots)
            {
                L3PDUInfo l3PduInfo = L3PduList.Get(slotInfo.Reference);

                if (l3PduInfo == null)
                {
                    /* just inform about PDUs that are not skipped (pseudo-L3 headers) */
                    if (!skipPDUs.Contains(slotInfo.Reference))
                        text += "          Unknown PDU " + slotInfo.Reference + Environment.NewLine;
                }
                else
                {
                    int pduLength = l3PduInfo.Length * 2;
                    bool pduExists = true;

                    /* round up to the next octet if this is not a mandatory half-octet field */
                    if (l3PduInfo.Type != 1 || slotInfo.Presence != "M")
                        currentPos = (currentPos + 1) & ~1;

                    /* check if the optionsl PDU is present */
                    if (slotInfo.Presence != "M")
                    {
                        switch (l3PduInfo.Type)
                        {
                            case 1:
                                if (currentPos / 2 >= l3Data.Length || (l3Data[currentPos / 2] >> 4) != slotInfo.IEI)
                                    pduExists = false;
                                break;

                            default:
                                if (currentPos / 2 >= l3Data.Length || l3Data[currentPos / 2] != slotInfo.IEI)
                                    pduExists = false;
                                else
                                {
                                    /* skip two half-octets IEI */
                                    currentPos += 2;
                                }
                                break;
                        }
                    }

                    if (pduExists)
                    {
                        /* trigger as requested */
                        if (!string.IsNullOrEmpty(slotInfo.TriggerPre))
                        {
                            if (PDUDataTriggers.ContainsKey(slotInfo.TriggerPre))
                                PDUDataTriggers[slotInfo.TriggerPre](this);
                        }

                        /* trigger as requested */
                        if (!string.IsNullOrEmpty(l3PduInfo.TriggerPre))
                        {
                            if (PDUDataTriggers.ContainsKey(l3PduInfo.TriggerPre))
                                PDUDataTriggers[l3PduInfo.TriggerPre](this);
                        }

                        /* this pdu is either present or mandatory */
                        switch (l3PduInfo.Type)
                        {
                            case 1:
                                /* half octet type TV or V */
                                pduLength = 1;
                                break;

                            case 2:
                                /* Type only (Format T) */
                                pduLength = 0;
                                break;

                            case 3:
                                /* Fixed length (Format V or TV) */
                                pduLength = l3PduInfo.Length * 2;
                                break;

                            case 4:
                                /* Dynamic length (TLV or LV) */
                                if (currentPos / 2 >= l3Data.Length)
                                    pduLength = 0;
                                else
                                    pduLength = l3Data[currentPos / 2] * 2;

                                currentPos += 2;
                                break;

                            case 5:
                                /* rest octets */
                                pduLength = 0;
                                for (int pos = currentPos / 2; pos < l3Data.Length; pos++)
                                {
                                    if (l3Data[pos] != 0x2B)
                                        pduLength = l3Data.Length * 2 - currentPos;
                                }
                                break;
                        }

                        if ((currentPos + pduLength) / 2 > l3Data.Length)
                        {
                            text += "          PDU too long: " + slotInfo.Reference + Environment.NewLine;
                            return text;
                        }

                        /* thats the PDU that should be there, dump it */
                        byte[] pduData = new byte[(pduLength + 1) / 2];
                        Array.Copy(l3Data, currentPos / 2, pduData, 0, pduData.Length);

                        /* align the data for the weird half-octet type */
                        if (l3PduInfo.Type == 1)
                        {
                            if ((currentPos & 1) == 1)
                                pduData[0] >>= 4;
                            else
                                pduData[0] &= 0x0F;
                        }

                        currentPos += pduLength;

                        if (PDUParser.ContainsKey(l3PduInfo.Reference))
                        {
                            text += "          " + l3PduInfo.Name + " (" + l3PduInfo.Reference + "):" +
                                    Environment.NewLine;
                            string[] info = PDUParser[l3PduInfo.Reference](pduData);
                            foreach (string s in info)
                            {
                                if (!string.IsNullOrEmpty(s))
                                    text += "             " + s + Environment.NewLine;
                            }
                        }
                        else if (l3PduInfo.Fields.Count > 0)
                        {
                            text += "          " + l3PduInfo.Name + " (" + l3PduInfo.Reference + "):" +
                                    Environment.NewLine;
                            string[] info = HandleGenericPDU(l3PduInfo.Fields, pduData);
                            foreach (string s in info)
                            {
                                if (!string.IsNullOrEmpty(s))
                                    text += "             " + s + Environment.NewLine;
                            }
                        }
                        else
                        {
                            text += "          " + l3PduInfo.Name + " (" + l3PduInfo.Reference + "):" +
                                    Environment.NewLine;
                            if (pduData.Length > 0)
                                text += "             " + DumpBytes(pduData) + Environment.NewLine;
                        }

                        /* trigger as requested */
                        if (!string.IsNullOrEmpty(l3PduInfo.TriggerPost))
                        {
                            if (PDUDataTriggers.ContainsKey(l3PduInfo.TriggerPost))
                                PDUDataTriggers[l3PduInfo.TriggerPost](this);
                        }

                        /* trigger as requested */
                        if (!string.IsNullOrEmpty(slotInfo.TriggerPost))
                        {
                            if (PDUDataTriggers.ContainsKey(slotInfo.TriggerPost))
                                PDUDataTriggers[slotInfo.TriggerPost](this);
                        }
                    }
                }
            }

            if (currentPos / 2 < l3Data.Length)
            {
                /* thats the PDU that should be there, dump it */
                byte[] pduData = new byte[l3Data.Length - (currentPos + 1) / 2];
                Array.Copy(l3Data, currentPos / 2, pduData, 0, pduData.Length);

                bool hasData = false;
                for (int pos = 0; pos < pduData.Length; pos++)
                {
                    if (pduData[pos] != 0x2B)
                        hasData = true;
                }

                if (hasData)
                {
                    text += "          " + "Unhandled Rest Octets" + Environment.NewLine;
                    text += "             " + DumpBytes(pduData) + Environment.NewLine;
                }
            }

            /* trigger as requested */
            if (!string.IsNullOrEmpty(l3Message.TriggerPost))
            {
                if (PDUDataTriggers.ContainsKey(l3Message.TriggerPost))
                    PDUDataTriggers[l3Message.TriggerPost](this);
            }
            return text;
        }


        public void Handle(byte[] l3Data)
        {
            Handle(l3Data, 0);
        }

        public void Handle(byte[] l3Data, int start)
        {
            if (l3Data.Length > 1)
            {
                eProtocol PD = (eProtocol)(l3Data[start + 0] & 0x0F);
                int packetType = l3Data[start + 1];

                ShowMessage = true;

                switch (PD)
                {
                    case eProtocol.RadioResource:
                        {
                            L3PacketInfo type = L3PacketTypesRR.Get(packetType);
                            if (type != null)
                            {

                                /* check if this packet should be skipped */
                                lock (SkipMessages)
                                {
                                    if (SkipMessages.ContainsKey(type.RefDown))
                                    {
                                        if (ExceptFieldsEnabled)
                                            ShowMessage = false;
                                        else
                                            return;
                                    }
                                }

                                StatusMessage = PD + ": " + type.Description + " (" + type.RefDown + ")" +
                                                Environment.NewLine;
                                StatusMessage += DecodePacket(l3Data, start + 2, type.RefDown);
                            }
                            else
                            {
                                StatusMessage = PD + " " + packetType + " (Unhandled)" + Environment.NewLine;
                            }
                        }
                        break;

                    case eProtocol.CallControl:
                        {
                            L3PacketInfo type = L3PacketTypesCC.Get(packetType);
                            if (type != null)
                            {
                                /* check if this packet should be skipped */

                                lock (SkipMessages)
                                {
                                    if (SkipMessages.ContainsKey(type.RefDown))
                                    {
                                        if (ExceptFieldsEnabled)
                                            ShowMessage = false;
                                        else
                                            return;
                                    }
                                }

                                StatusMessage = PD + ": " + type.Description + " (" + type.RefDown + ")" +
                                                Environment.NewLine;
                                StatusMessage += DecodePacket(l3Data, start + 2, type.RefDown);

                            }
                            else
                            {
                                StatusMessage = PD + " " + packetType + " (Unhandled)" + Environment.NewLine;
                            }
                        }
                        break;

                    case eProtocol.MobilityManagement:
                        {
                            L3PacketInfo type = L3PacketTypesMM.Get(packetType);
                            if (type != null)
                            {
                                /* check if this packet should be skipped */

                                lock (SkipMessages)
                                {
                                    if (SkipMessages.ContainsKey(type.RefDown))
                                    {
                                        if (ExceptFieldsEnabled)
                                            ShowMessage = false;
                                        else
                                            return;
                                    }
                                }

                                StatusMessage = PD + ": " + type.Description + " (" + type.RefDown + ")" +
                                                Environment.NewLine;
                                StatusMessage += DecodePacket(l3Data, start + 2, type.RefDown);

                            }
                            else
                            {
                                if (DumpUnhandled || DumpRawData)
                                    StatusMessage = PD + " " + packetType + " (Unhandled)" + Environment.NewLine;
                            }
                        }
                        break;

                    default:
                        {
                            if (DumpUnhandled || DumpRawData)
                                StatusMessage = PD + " " + packetType + " (Unhandled)" + Environment.NewLine;
                        }
                        break;
                }

                if (DumpRawData)
                {
                    StatusMessage += "          Raw Data" + Environment.NewLine;
                    StatusMessage += "             " + DumpBytes(l3Data) + Environment.NewLine;
                }
            }

            if (!ShowMessage)
                StatusMessage = null;
        }

        private static string DumpBytes(byte[] data)
        {
            string msg = "";

            foreach (byte value in data)
                msg += String.Format("{0:X02} ", value);

            return msg;
        }
    }
}