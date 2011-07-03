using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibRXFFT.Libraries.GSM.Misc;
using System.Text;
using System.Windows.Forms;
using RX_FFT.Components.GDI;
using LibRXFFT.Libraries.GSM.Layer1;

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
        public static bool AlreadyWarned = false;

        public static L3PacketTypes L3PacketTypesRR;
        public static L3PacketTypes L3PacketTypesCC;
        public static L3PacketTypes L3PacketTypesMM;
        public static L3PacketTypes L3PacketTypesSMS;

        public static L3Messages L3MessagesRadio;
        public static L3PDUList L3PduListRadio;
        
        public static L3Messages L3MessagesSMS;
        public static L3PDUList L3PduListSMS;

        public static L3Messages L3MessagesRP;
        public static L3PDUList L3PduListRP;

        public static MCCTable MCCTable;
        public static MNCTable MNCTable;

        public static string DataFilePath = null;

        public static bool SniffIMSI = false;
        public string SniffResult = null;

        public static bool EnableProviderLookup = false;
        public static bool ExceptFieldsEnabled = true;
        public static Dictionary<string, bool> SkipMessages = new Dictionary<string, bool>();
        public static Dictionary<string, bool> ExceptFields = new Dictionary<string, bool>();

        private bool ShowMessage = false;
        public string StatusMessage = null;
        private static bool DataLoaded = false;

        private readonly Dictionary<string, handlePDUDelegate> PDUParser = new Dictionary<string, handlePDUDelegate>();
        public static bool DumpRawData = false;
        public static bool DumpUnhandled = false;
        public static bool DumpUnhandledOctets = false;
        

        private delegate string[] handlePDUDelegate(byte[] pduData);


        public readonly Dictionary<string, handleTrigger> PDUDataTriggers = new Dictionary<string, handleTrigger>();

        /* these will keep the PDUs fields described in pdulist.xml */
        public readonly Dictionary<string, string> PDUDataFields = new Dictionary<string, string>();
        public readonly Dictionary<string, long> PDUDataRawFields = new Dictionary<string, long>();

        /* this will keep the whole PDU described in pdulist.xml */
        public readonly Dictionary<string, byte[]> PDUDataRaw = new Dictionary<string, byte[]>();


        public readonly Dictionary<string, fieldParser> PDUFieldParsers = new Dictionary<string, fieldParser>();

        public delegate string fieldParser(L3Handler handler, long value);
        public delegate void handleTrigger(L3Handler handler);

        /* skip decoding the pseudo-L2 headers */
        string[] DecodeSkipPDUs = new[] { "10.5.2.19", "10.2", "10.3.1", "10.3.2", "10.4", "10.5.1.8", "GSM 04.11 8.1.3"};
        string[] MobileIdentTypes = new[]
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

        public L3Handler()
        {
            if (!DataLoaded)
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
            }


            PDUParser.Add("GSM 04.11 8.1.4.1", HandleSMSCPData);
            PDUParser.Add("GSM 04.11 8.2.5.3", HandleSMSTLData);
            PDUParser.Add("10.5.1.4", HandleMobileIdentity);
            PDUParser.Add("10.5.2.1b", HandleCellChannelDescription);
            PDUParser.Add("10.5.2.22", HandleCellChannelDescription);
            PDUParser.Add("10.5.2.5", HandleChannelDescription);
            PDUParser.Add("10.5.2.5a", HandleChannelDescription);

            PDUFieldParsers.Add("ParseRA", ParseRA);
        }


        public static void ReloadFiles()
        {
            bool dataValid = true;

            dataValid &= CheckValid(L3PacketTypesMM);
            dataValid &= CheckValid(L3PacketTypesCC);
            dataValid &= CheckValid(L3PacketTypesRR);
            dataValid &= CheckValid(L3MessagesRadio);
            dataValid &= CheckValid(L3PduListRadio);
            dataValid &= CheckValid(L3PacketTypesSMS);
            dataValid &= CheckValid(L3MessagesSMS);
            dataValid &= CheckValid(L3PduListSMS);
            dataValid &= CheckValid(L3MessagesRP);
            dataValid &= CheckValid(L3PduListRP);
            dataValid &= CheckValid(MCCTable);
            dataValid &= CheckValid(MNCTable);
            
            if (DataFilePath == null || !Directory.Exists(DataFilePath))
            {
                DataFilePath = Environment.CurrentDirectory + "\\DataFiles\\";
            }
            if (!Directory.Exists(DataFilePath))
            {
                DataFilePath = Environment.CurrentDirectory + "\\..\\DataFiles\\";
            }
            if (!Directory.Exists(DataFilePath))
            {
                DataFilePath = Environment.CurrentDirectory + "\\..\\..\\DataFiles\\";
            }
            if (!Directory.Exists(DataFilePath))
            {
                DataFilePath = Environment.CurrentDirectory + "\\..\\..\\..\\..\\RX-FFT\\DataFiles\\";
            }
            if (!Directory.Exists(DataFilePath))
            {
                if (!AlreadyWarned)
                {
                    MessageBox.Show("Could not locate GSM Analyzer data files. Decoding is impossible.");
                    AlreadyWarned = true;
                }
                return;
            }

            try
            {
                L3PacketTypesMM = new L3PacketTypes(DataFilePath + "packeteering-mm.xml");
                L3PacketTypesCC = new L3PacketTypes(DataFilePath + "packeteering-cc.xml");
                L3PacketTypesRR = new L3PacketTypes(DataFilePath + "packeteering-rr.xml");
                L3MessagesRadio = new L3Messages(DataFilePath + "messagelist-radio.xml");
                L3PduListRadio = new L3PDUList(DataFilePath + "pdulist-radio.xml");

                L3PacketTypesSMS = new L3PacketTypes(DataFilePath + "packeteering-sms.xml");
                L3MessagesSMS = new L3Messages(DataFilePath + "messagelist-sms.xml");
                L3PduListSMS = new L3PDUList(DataFilePath + "pdulist-sms.xml");

                L3MessagesRP = new L3Messages(DataFilePath + "messagelist-rp.xml");
                L3PduListRP = new L3PDUList(DataFilePath + "pdulist-rp.xml");

                MCCTable = new MCCTable(DataFilePath + "mccentries.xml");
                MNCTable = new MNCTable(DataFilePath + "mncentries.xml");

                DataLoaded = true;
            }
            catch (Exception e)
            {
                if (!AlreadyWarned)
                {
                    MessageBox.Show("Failed to load GSM Analyzer data files. Decoding is impossible. (" + e.GetType().ToString() + ")");
                    AlreadyWarned = true;
                }
            }
        }

        private static bool CheckValid(object data)
        {
            return (data != null);
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


            if (!PDUDataFields.ContainsKey("EstablishmentCause"))
            {
                PDUDataFields.Add("EstablishmentCause", retVal);
            }
            else
            {
                PDUDataFields["EstablishmentCause"] = retVal;
            }



            if (!PDUDataRawFields.ContainsKey("EstablishmentCause"))
            {
                PDUDataRawFields.Add("EstablishmentCause", value);
            }
            else
            {
                PDUDataRawFields["EstablishmentCause"] = value;
            }

            return retVal;
        }

        private string[] HandleCellChannelDescription(byte[] pduData)
        {
            ArrayList lines = new ArrayList();
            StringBuilder builder = new StringBuilder();

            string typeString = "Unknown";
            int type0 = pduData[0] >> 6; /* these are called FORMAT-ID, here bits 6-7 */
            int type2 = (pduData[0] >> 1) & 7; /* bits 4-2 */

            bool extInd = ((pduData[0] >> 5) & 1) == 1;
            bool baInd = ((pduData[0] >> 4) & 1) == 1;

            builder.Length = 0;
            builder.Append("EXT-IND             ");
            if (!extInd)
                builder.Append("The information element carries the complete information of a BCCH channel sub list");
            else
                builder.Append("The information element carries only a part of the BCCH channel sub list");

            lines.Add(builder.ToString());
            builder.Length = 0;
            builder.Append("BA-IND              ");
            if (!baInd)
                builder.Append("The information element carries the complete BA");
            else
                builder.Append("The information element carries only a part of the BA");

            lines.Add(builder.ToString());
            builder.Length = 0;

            switch (type0)
            {
                case 0:
                    builder.Append("Description Type    ").Append("Bit map 0 format");
                    lines.Add(builder.ToString());
                    builder.Length = 0;

                    builder.Append("ARFCNs              ");

                    for (int arfcn = 124; arfcn > 0; arfcn--)
                    {
                        int byteNum = 16 - (arfcn + 7) / 8;
                        int bitNum = 7 - ((128 - arfcn) % 8);

                        if (((pduData[byteNum] >> bitNum) & 1) == 1)
                            builder.Append(arfcn).Append(" ");
                    }
                    lines.Add(builder.ToString());
                    builder.Length = 0;

                    return (string[])lines.ToArray(typeof(string));


                case 2:
                    switch (type2)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            builder.Append("Description Type    ").Append("Range 1024 format");
                            lines.Add(builder.ToString());
                            builder.Length = 0;

                            builder.Append("ARFCNs              ").Append("not supported yet");
                            lines.Add(builder.ToString());
                            builder.Length = 0;

                            return (string[])lines.ToArray(typeof(string));

                        case 4:
                            builder.Append("Description Type    ").Append("Range 512 format");
                            lines.Add(builder.ToString());
                            builder.Length = 0;

                            builder.Append("ARFCNs              ").Append("not supported yet");
                            lines.Add(builder.ToString());
                            builder.Length = 0;

                            return (string[])lines.ToArray(typeof(string));

                        case 5:
                            builder.Append("Description Type    ").Append("Range 256 format");
                            lines.Add(builder.ToString());
                            builder.Length = 0;

                            builder.Append("ARFCNs              ").Append("not supported yet");
                            lines.Add(builder.ToString());
                            builder.Length = 0;

                            return (string[])lines.ToArray(typeof(string));

                        case 6:
                            builder.Append("Description Type    ").Append("Range 128 format");
                            lines.Add(builder.ToString());
                            builder.Length = 0;

                            builder.Append("ARFCNs              ").Append("not supported yet");
                            lines.Add(builder.ToString());
                            builder.Length = 0;

                            return (string[])lines.ToArray(typeof(string));

                        case 7:
                            builder.Append("Description Type    ").Append("Variable bit map format");
                            lines.Add(builder.ToString());
                            builder.Length = 0;

                            builder.Append("ARFCNs              ");
                            int origArfcn = 0;

                            origArfcn |= (pduData[0] & 1);
                            origArfcn <<= 8;
                            origArfcn |= (pduData[1] & 0xFF);
                            origArfcn <<= 1;
                            origArfcn |= (pduData[2] >> 7);

                            builder.Append(origArfcn);
                            for (int rrfcn = 1; rrfcn < 112; rrfcn++)
                            {
                                int octet = 2 + rrfcn / 8;
                                int shift = 7 - (rrfcn % 8);
                                bool rrfcnUsed = ((pduData[octet] >> shift) & 0x01) != 0;
                                int arfcn = (origArfcn + rrfcn) % 1024;

                                if (rrfcnUsed)
                                {
                                    builder.Append(" ").Append(arfcn);
                                }
                            }

                            lines.Add(builder.ToString());
                            return (string[])lines.ToArray(typeof(string));
                    }
                    break;
            }

            return new[] { "This Cell Channel Description Type (" + typeString + ") is not implemented yet." };
        }

        private string[] HandleChannelDescription(byte[] pduData)
        {
            ArrayList lines = new ArrayList();
            StringBuilder builder = new StringBuilder();
            string chanType = "Reserved";
            int subChan = -1;
            int timeSlot = -1;
            bool hopping = (pduData[1] & 0x10) != 0x00;
            int type = pduData[0] >> 3;

            builder.Length = 0;
            builder.Append( "Channel             ");

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

            builder.Append(chanType);
            if (subChan >= 0)
            {
                builder.Append(", Subchan ").Append(subChan);
            }

            lines.Add(builder.ToString());
            builder.Length = 0;

            timeSlot = pduData[0] & 7;
            builder.Append("Timeslot Number     ").Append(timeSlot);
            lines.Add(builder.ToString());

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

                if (PDUDataRawFields.ContainsKey("Hopping"))
                    PDUDataRawFields["Hopping"] = hopping ? 1 : 0;
                else
                    PDUDataRawFields.Add("Hopping", hopping ? 1 : 0);

            }
            
            builder.Length = 0;
            builder.Append("TSC                 ").Append((pduData[1] >> 5) & 7);
            lines.Add(builder.ToString());
            builder.Length = 0;

            if (hopping)
            {
                long maio = ((pduData[1] & 0x0F) << 2) | ((pduData[2] & 0x03) >> 6);
                long hsn = pduData[2] & 0x3F;

                builder.Append("Hopping             ").Append("RF hopping channel");
                lines.Add(builder.ToString());
                builder.Length = 0;

                builder.Append("MAIO                ").Append(maio);
                lines.Add(builder.ToString());
                builder.Length = 0;

                builder.Append("HSN                 ").Append(hsn);
                lines.Add(builder.ToString());
                builder.Length = 0;

                lock (PDUDataFields)
                {
                    /* text */
                    if (PDUDataFields.ContainsKey("MAIO"))
                        PDUDataFields["MAIO"] = maio.ToString();
                    else
                        PDUDataFields.Add("MAIO", maio.ToString());

                    if (PDUDataFields.ContainsKey("HSN"))
                        PDUDataFields["HSN"] = hsn.ToString();
                    else
                        PDUDataFields.Add("HSN", hsn.ToString());

                    /* raw */
                    if (PDUDataRawFields.ContainsKey("MAIO"))
                        PDUDataRawFields["MAIO"] = maio;
                    else
                        PDUDataRawFields.Add("MAIO", maio);

                    if (PDUDataRawFields.ContainsKey("HSN"))
                        PDUDataRawFields["HSN"] = hsn;
                    else
                        PDUDataRawFields.Add("HSN", hsn);
                }
            }
            else
            {
                long arfcn = ((pduData[1] & 3) << 8) | pduData[2];

                builder.Append("Hopping             ").Append("Single RF channel");
                lines.Add(builder.ToString());
                builder.Length = 0;

                builder.Append("ARFCN               ").Append(arfcn);
                lines.Add(builder.ToString());
                builder.Length = 0;

                lock (PDUDataFields)
                {
                    if (PDUDataFields.ContainsKey("ARFCN"))
                        PDUDataFields["ARFCN"] = arfcn.ToString();
                    else
                        PDUDataFields.Add("ARFCN", arfcn.ToString());

                    if (PDUDataRawFields.ContainsKey("ARFCN"))
                        PDUDataRawFields["ARFCN"] = arfcn;
                    else
                        PDUDataRawFields.Add("ARFCN", arfcn);
                }

            }

            return (string[]) lines.ToArray(typeof(string));
        }


        StringBuilder IdentBuilder1 = new StringBuilder();
        StringBuilder IdentBuilder2 = new StringBuilder();

        private string[] HandleMobileIdentity(byte[] pduData)
        {
            int type = pduData[0] & 0x07;

            if (type == 0)
            {
                return new[] { "Type                None" };
            }

            IdentBuilder1.Length = 0;
            IdentBuilder2.Length = 0;
            string[] msg = new string[2];

            lock (ExceptFields)
            {
                if (ExceptFields.ContainsKey(MobileIdentTypes[type]))
                    ShowMessage = true;
            }

            msg[0] = "Type                ";
            msg[0] += MobileIdentTypes[type];

            IdentBuilder1.AppendFormat("{0,-20}", MobileIdentTypes[pduData[0] & 0x07]);

            for (int pos = 1; pos < pduData.Length * 2 - 1; pos++)
            {
                int byteNum = pos / 2;
                int digit;

                if ((pos & 1) == 0)
                {
                    digit = pduData[byteNum] & 0x0F;
                }
                else
                {
                    digit = pduData[byteNum] >> 4;
                }

                switch (type)
                {
                    case 1:
                        if (digit != 15)
                            IdentBuilder2.Append(digit);
                        break;

                    case 4:
                        if (pos > 1)
                            IdentBuilder2.AppendFormat("{0:X01}", digit);
                        break;

                    case 0:
                    case 2:
                    case 3:
                    case 5:
                    case 6:
                    case 7:
                        if (digit == 15)
                            IdentBuilder2.Append("_");
                        else
                            IdentBuilder2.Append(digit);
                        break;
                }
            }

            string identString = IdentBuilder2.ToString();

            if (!PDUDataFields.ContainsKey("Identity"))
            {
                PDUDataFields.Add("Identity", MobileIdentTypes[type] + " " + identString);
            }
            else
            {
                PDUDataFields["Identity"] = MobileIdentTypes[type] + " " + identString;
            }

            if (type == 1 && EnableProviderLookup)
            {
                string mcc = identString.Substring(0, 3);
                string mnc2 = identString.Substring(3, 2);
                string mnc3 = identString.Substring(3, 3);

                MCCEntry countryEntry = MCCTable.Get(mcc);
                MNCEntry netEntry2 = MNCTable.Get(mcc, mnc2);
                MNCEntry netEntry3 = MNCTable.Get(mcc, mnc3);

                if (countryEntry != null)
                    IdentBuilder2.Append("  ").Append(countryEntry.CC).Append(" (").Append(countryEntry.Country).Append(")");

                if (netEntry2 != null)
                    IdentBuilder2.Append(" (").Append(netEntry2.Network).Append(")");

                if (netEntry3 != null)
                    IdentBuilder2.Append(" (").Append(netEntry3.Network).Append(")");


                if (SniffIMSI)
                {
                    if (SniffResult == null)
                        SniffResult = IdentBuilder2.ToString();
                    else
                        SniffResult += ", " + IdentBuilder2.ToString();
                }
            }

            IdentBuilder1.Append(IdentBuilder2);
            msg[1] = IdentBuilder1.ToString();

            return msg;
        }

        private StringBuilder StrBuilder = new StringBuilder();
        private StringBuilder PduFieldData = new StringBuilder();
        private string[] HandleGenericPDU(ArrayList fields, byte[] pduData)
        {
            int pos = 0;
            string[] msg = new string[fields.Count];

            foreach (L3PDUField field in fields)
            {
                StrBuilder.Length = 0;

                /* trigger as requested */
                if (!string.IsNullOrEmpty(field.TriggerPre))
                {
                    if (PDUDataTriggers.ContainsKey(field.TriggerPre))
                        PDUDataTriggers[field.TriggerPre](this);
                }

                StrBuilder.AppendFormat("{0,-20}", field.Name);
                long value = 0; 
                PduFieldData.Length = 0;


                foreach (Bits bits in field.Bits)
                {
                    int mask = (1 << bits.Count) - 1;

                    value <<= bits.Count;
                    if (bits.Octet <= pduData.Length)
                    {
                        value |= (pduData[bits.Octet - 1] >> (bits.Start - 1)) & mask;
                    }
                    else
                    {
                        StrBuilder.Append("FAILED - PDU too short (" + (pduData.Length - bits.Octet) + " byte)");
                    }
                }
                
                switch (field.Type)
                {
                    case "hide":
                        break;

                    case "integer":
                        PduFieldData.Append(((long)field.Offset + field.Factor * value));
                        break;

                    case "varbcd":
                        if (field.Bits.Count != 1)
                        {
                            return new[] { "varbcd type requires one bit field to determine start octet" };
                        }
                        Bits startBit = (Bits)field.Bits[0];
                        for (int digit = 0; digit < (pduData.Length - (startBit.Octet - 1)) * 2; digit++)
                        {
                            long bcd = (pduData[(startBit.Octet - 1) + digit / 2] >> (4 * (digit % 2))) & 0x0F;

                            if (bcd < 10)
                            {
                                PduFieldData.Append(bcd);
                            }
                            else
                            {
                                PduFieldData.Append(" ");
                            }
                        }
                        break;

                    case "bcd":
                        for (int digit = 0; digit < field.Length; digit++)
                        {
                            long bcd = (value >> (4 * (field.Length - 1 - digit))) & 0x0F;
                            if (bcd < 10)
                            {
                                PduFieldData.Append(bcd);
                            }
                            else
                            {
                                PduFieldData.Append(" ");
                            }
                        }
                        break;

                    case "hexnumber":
                        PduFieldData.AppendFormat("{0:X}", value);
                        break;

                    case "stringIndex":
                        if (value < field.Strings.Length && !string.IsNullOrEmpty(field.Strings[value]))
                        {
                            PduFieldData.Append(field.Strings[value]);
                        }
                        else
                        {
                            PduFieldData.Append("Unknown entry: ").Append(value);
                        }
                        break;
                }

                StrBuilder.Append(PduFieldData);

                if (!string.IsNullOrEmpty(field.Parser) && PDUFieldParsers.ContainsKey(field.Parser))
                {
                    StrBuilder.Append(" (").Append(PDUFieldParsers[field.Parser](this, value)).Append(")");
                }

                /* update the fields in the field list */
                if (!string.IsNullOrEmpty(field.SetField))
                {
                    lock (PDUDataFields)
                    {
                        if (PDUDataFields.ContainsKey(field.SetField))
                            PDUDataFields[field.SetField] = PduFieldData.ToString();
                        else
                            PDUDataFields.Add(field.SetField, PduFieldData.ToString());

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

                msg[pos] = StrBuilder.ToString();
                pos++;
            }

            return msg;
        }

        private string DecodePacket(L3Messages l3Messages, L3PDUList l3Pdus, byte[] l3Data, int start, string reference)
        {
            /* half-octet based indexing */
            int currentPos = start * 2;
            /* must stay function-local since this function is reentrant */
            StringBuilder DecodePacketBuilder = new StringBuilder();

            L3MessageInfo l3Message = l3Messages.Get(reference);
            if (l3Message == null)
            {
                byte[] pduData = new byte[l3Data.Length - start];
                Array.Copy(l3Data, start, pduData, 0, pduData.Length);

                DecodePacketBuilder.Append("  Unspecified Message Type: ").Append(reference).Append(Environment.NewLine);
                DecodePacketBuilder.Append("    ").Append(DumpBytes(pduData)).Append(Environment.NewLine);
                return DecodePacketBuilder.ToString();
            }

            /* trigger as requested */
            if (!string.IsNullOrEmpty(l3Message.TriggerPre))
            {
                if (PDUDataTriggers.ContainsKey(l3Message.TriggerPre))
                    PDUDataTriggers[l3Message.TriggerPre](this);
            }

            foreach (L3MessageSlotInfo slotInfo in l3Message.Slots)
            {
                L3PDUInfo l3PduInfo = l3Pdus.Get(slotInfo.Reference);

                if (l3PduInfo == null)
                {
                    /* just inform about PDUs that are not skipped (pseudo-L3 headers) */
                    if (!DecodeSkipPDUs.Contains(slotInfo.Reference))
                    {
                        DecodePacketBuilder.Append("          Unknown PDU ").Append(slotInfo.Reference).Append(Environment.NewLine);
                    }
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
                            DecodePacketBuilder.Append("  PDU too short: ").Append(slotInfo.Reference).Append(Environment.NewLine);
                            return DecodePacketBuilder.ToString();
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

                        if (!string.IsNullOrEmpty(l3PduInfo.SetField))
                        {
                            lock (PDUDataFields)
                            {
                                if (PDUDataRaw.ContainsKey(l3PduInfo.SetField))
                                {
                                    PDUDataRaw.Remove(l3PduInfo.SetField);
                                }

                                PDUDataRaw.Add(l3PduInfo.SetField, pduData);
                            }
                        }

                        if (PDUParser.ContainsKey(l3PduInfo.Reference))
                        {
                            DecodePacketBuilder.Append("  ").Append(l3PduInfo.Name).Append(" (").Append(l3PduInfo.Reference + "):").Append(Environment.NewLine);
                            string[] info = PDUParser[l3PduInfo.Reference](pduData);
                            foreach (string s in info)
                            {
                                if (!string.IsNullOrEmpty(s))
                                {
                                    DecodePacketBuilder.Append("    ").Append(s).Append(Environment.NewLine);
                                }
                            }
                        }
                        else if (l3PduInfo.Fields.Count > 0)
                        {
                            DecodePacketBuilder.Append("  ").Append(l3PduInfo.Name).Append(" (").Append(l3PduInfo.Reference + "):").Append(Environment.NewLine);
                            string[] info = HandleGenericPDU(l3PduInfo.Fields, pduData);
                            foreach (string s in info)
                            {
                                if (!string.IsNullOrEmpty(s))
                                {
                                    DecodePacketBuilder.Append("    ").Append(s).Append(Environment.NewLine);
                                }
                            }
                        }
                        else
                        {
                            DecodePacketBuilder.Append("  ").Append(l3PduInfo.Name).Append(" (").Append(l3PduInfo.Reference + "):").Append(Environment.NewLine);
                            if (DumpUnhandledOctets)
                            {
                                if (pduData.Length > 0)
                                {
                                    DecodePacketBuilder.Append("    ").Append(DumpBytes(pduData)).Append(Environment.NewLine);
                                }
                            }
                            else
                            {
                                DecodePacketBuilder.Append("    (not dumped)").Append(Environment.NewLine);
                            }
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
                    DecodePacketBuilder.Append("  ").Append("Unhandled Rest Octets").Append(Environment.NewLine);
                    if (DumpUnhandledOctets)
                    {
                        DecodePacketBuilder.Append("    ").Append(DumpBytes(pduData)).Append(Environment.NewLine);
                    }
                    else
                    {
                        DecodePacketBuilder.Append("    (not dumped)").Append(Environment.NewLine);
                    }
                }
            }

            /* trigger as requested */
            if (!string.IsNullOrEmpty(l3Message.TriggerPost))
            {
                if (PDUDataTriggers.ContainsKey(l3Message.TriggerPost))
                    PDUDataTriggers[l3Message.TriggerPost](this);
            }
            return DecodePacketBuilder.ToString();
        }


#if false
        private string DecodeRRPacket(byte[] l3Data, int start, string reference)
        {
            /* half-octet based indexing */
            int currentPos = start * 2;
            StringBuilder builder = new StringBuilder();

            L3MessageInfo l3Message = L3MessagesRadio.Get(reference);
            if (l3Message == null)
            {
                byte[] pduData = new byte[l3Data.Length - start];
                Array.Copy(l3Data, start, pduData, 0, pduData.Length);

                builder.Append("          Unspecified Message Type: ").Append(reference).Append(Environment.NewLine);
                builder.Append("             ").Append(DumpBytes(pduData)).Append(Environment.NewLine);
                return builder.ToString();
            }

            /* trigger as requested */
            if (!string.IsNullOrEmpty(l3Message.TriggerPre))
            {
                if (PDUDataTriggers.ContainsKey(l3Message.TriggerPre))
                    PDUDataTriggers[l3Message.TriggerPre](this);
            }

            foreach (L3MessageSlotInfo slotInfo in l3Message.Slots)
            {
                L3PDUInfo l3PduInfo = L3PduListRadio.Get(slotInfo.Reference);

                if (l3PduInfo == null)
                {
                    /* just inform about PDUs that are not skipped (pseudo-L3 headers) */
                    if (!DecodeSkipPDUs.Contains(slotInfo.Reference))
                    {
                        builder.Append("          Unknown PDU ").Append(slotInfo.Reference).Append(Environment.NewLine);
                    }
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
                            builder.Append("          PDU too short: ").Append(slotInfo.Reference).Append(Environment.NewLine);
                            return builder.ToString();
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
                            builder.Append("          ").Append(l3PduInfo.Name).Append(" (").Append(l3PduInfo.Reference + "):").Append(Environment.NewLine);
                            string[] info = PDUParser[l3PduInfo.Reference](pduData);
                            foreach (string s in info)
                            {
                                if (!string.IsNullOrEmpty(s))
                                {
                                    builder.Append("             ").Append(s).Append(Environment.NewLine);
                                }
                            }
                        }
                        else if (l3PduInfo.Fields.Count > 0)
                        {
                            builder.Append("          ").Append(l3PduInfo.Name).Append(" (").Append(l3PduInfo.Reference + "):").Append(Environment.NewLine);
                            string[] info = HandleGenericPDU(l3PduInfo.Fields, pduData);
                            foreach (string s in info)
                            {
                                if (!string.IsNullOrEmpty(s))
                                {
                                    builder.Append("             ").Append(s).Append(Environment.NewLine);
                                }
                            }
                        }
                        else
                        {
                            builder.Append("          ").Append(l3PduInfo.Name).Append(" (").Append(l3PduInfo.Reference + "):").Append(Environment.NewLine);
                            if (pduData.Length > 0)
                            {
                                builder.Append("             ").Append(DumpBytes(pduData)).Append(Environment.NewLine);
                            }
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
                    builder.Append("          ").Append("Unhandled Rest Octets").Append(Environment.NewLine);
                    builder.Append("             ").Append(DumpBytes(pduData)).Append(Environment.NewLine);
                }
            }

            /* trigger as requested */
            if (!string.IsNullOrEmpty(l3Message.TriggerPost))
            {
                if (PDUDataTriggers.ContainsKey(l3Message.TriggerPost))
                    PDUDataTriggers[l3Message.TriggerPost](this);
            }
            return builder.ToString();
        }
#endif


        private string[] HandleSMSTLData(byte[] pduData)
        {
            return SMSDecoder.DecodeTLPacketDownlink(pduData);
        }

        private string[] HandleSMSCPData(byte[] pduData)
        {
            StringBuilder builder = new StringBuilder();
            string refDown = "GSM 04.11 7.3.1";

            builder.Append(" (embedded protocol)").Append(Environment.NewLine);
            builder.Append(DecodePacket(L3MessagesRP, L3PduListRP, pduData, 0, refDown));

            return new[] { builder.ToString() };
        }

        public void Handle(byte[] l3Data, GSMParameters param)
        {
            Handle(l3Data, 0, param);
        }

        public void Handle(byte[] l3Data, int start, GSMParameters param)
        {
            /* call LUA script */
            if (param.LuaVm != null)
            {
                LuaHelpers.CallFunction(param.LuaVm, "L3DataReceived", true, l3Data, param);
            }

            StringBuilder builder = new StringBuilder();

            if (l3Data.Length > 1)
            {
                eProtocol PD = (eProtocol)(l3Data[start + 0] & 0x0F);
                int packetType = l3Data[start + 1];

                builder.Append(PD);
                switch (PD)
                {
                    case eProtocol.SMSMessage:
                        {
                            ShowMessage = true;
                            L3PacketInfo type = L3PacketTypesSMS.Get(packetType);
                            if (type != null)
                            {
                                /* check if this packet should be skipped */
                                lock (SkipMessages)
                                {
                                    if (SkipMessages.ContainsKey(type.RefDown))
                                    {
                                        ShowMessage = false;
                                    }
                                }

                                builder.Append(": ").Append(type.Description).Append(" (").Append(type.RefDown).Append(")").Append(Environment.NewLine);
                                builder.Append(DecodePacket(L3MessagesSMS, L3PduListSMS, l3Data, start + 2, type.RefDown));
                            }
                            else
                            {
                                builder.Append(" ").Append(packetType).Append(" (Unhandled)").Append(Environment.NewLine);
                            }
                        }
                        break;

                    case eProtocol.RadioResource:
                        {
                            ShowMessage = true;
                            L3PacketInfo type = L3PacketTypesRR.Get(packetType);
                            if (type != null)
                            {
                                /* check if this packet should be skipped */
                                lock (SkipMessages)
                                {
                                    if (SkipMessages.ContainsKey(type.RefDown))
                                    {
                                        ShowMessage = false;
                                    }
                                }

                                builder.Append(": ").Append(type.Description).Append(" (").Append(type.RefDown).Append(")").Append(Environment.NewLine);
                                builder.Append(DecodePacket(L3MessagesRadio, L3PduListRadio, l3Data, start + 2, type.RefDown));
                            }
                            else
                            {
                                builder.Append(" ").Append(packetType).Append(" (Unhandled)").Append(Environment.NewLine);
                            }
                        }
                        break;

                    case eProtocol.CallControl:
                        {
                            ShowMessage = true;
                            L3PacketInfo type = L3PacketTypesCC.Get(packetType);
                            if (type != null)
                            {
                                /* check if this packet should be skipped */
                                lock (SkipMessages)
                                {
                                    if (SkipMessages.ContainsKey(type.RefDown))
                                    {
                                        ShowMessage = false;
                                    }
                                }

                                builder.Append(": ").Append(type.Description).Append(" (").Append(type.RefDown).Append(")").Append(Environment.NewLine);
                                builder.Append(DecodePacket(L3MessagesRadio, L3PduListRadio, l3Data, start + 2, type.RefDown));
                            }
                            else
                            {
                                builder.Append(" ").Append(packetType).Append(" (Unhandled)").Append(Environment.NewLine);
                            }
                        }
                        break;

                    case eProtocol.MobilityManagement:
                        {
                            ShowMessage = true;
                            L3PacketInfo type = L3PacketTypesMM.Get(packetType);
                            if (type != null)
                            {
                                /* check if this packet should be skipped */

                                lock (SkipMessages)
                                {
                                    if (SkipMessages.ContainsKey(type.RefDown))
                                    {
                                        ShowMessage = false;
                                    }
                                }

                                builder.Append(": ").Append(type.Description).Append(" (").Append(type.RefDown).Append(")").Append(Environment.NewLine);
                                builder.Append(DecodePacket(L3MessagesRadio, L3PduListRadio, l3Data, start + 2, type.RefDown));
                            }
                            else
                            {
                                if (DumpUnhandled || DumpRawData)
                                {
                                    builder.Append(" ").Append(packetType).Append(" (Unhandled)").Append(Environment.NewLine);
                                }
                            }
                        }
                        break;

                    default:
                        {
                            if (DumpUnhandled || DumpRawData)
                            {
                                ShowMessage = true;
                                builder.Append(" ").Append(packetType).Append(" (Unhandled)").Append(Environment.NewLine);
                            }
                        }
                        break;
                }

                if (DumpRawData)
                {
                    builder.Append("Raw L3 data").Append(Environment.NewLine);
                    builder.Append("    ").Append(DumpBytes(l3Data)).Append(Environment.NewLine);
                }
            }

            if (ShowMessage)
            {
                StatusMessage = builder.ToString();
            }
            else
            {
                StatusMessage = null;
            }
        }

        private static StringBuilder TempBuilder = new StringBuilder();
        private static string DumpBytes(byte[] data)
        {
                TempBuilder.Length = 0;

                foreach (byte value in data)
                    TempBuilder.AppendFormat("{0:X02} ", value);

                return TempBuilder.ToString();
        }
    }
}