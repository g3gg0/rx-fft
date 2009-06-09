using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.GSM_Layer3
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
        GRPSMobility = 8,
        SMSMessage = 9,
        GPRSSessionManagement = 10,
        SSMessage = 11,
        LocationServices = 12,
        Reserved1 = 14,
        ReservedTest = 15
    }

    public class L3Handler
    {
        public L3PacketTypes pktTypes;
        public PDUList pduList;
        public string StatusMessage = null;
        private L3Messages msgList;

        private Dictionary<string, handlePDUDelegate> PDUParser = new Dictionary<string, handlePDUDelegate>();

        delegate string[] handlePDUDelegate(byte[] pduData);

        public L3Handler()
        {
            pktTypes = new L3PacketTypes();
            pduList = new PDUList();
            msgList = new L3Messages();

            PDUParser.Add("10.5.1.1", HandleCellIdentity);
            PDUParser.Add("10.5.2.3", HandleCellOptionsBCCH);
            PDUParser.Add("10.5.2.3a", HandleCellOptionsSACCH);
            PDUParser.Add("10.5.2.11", HandleCCCHDesc);
            PDUParser.Add("10.5.1.3", HandleLAI);
        }

        private string[] HandleCellOptionsSACCH(byte[] pduData)
        {
            string[] msg = new string[3];
            string[] dtxDesc = new[]
            {
                "The MS may use uplink discontinuous transmission on a TCH-F. The MS shall not use uplink discontinuous transmission on TCH-H.",
                "The MS shall use uplink discontinuous transmission on a TCH-F. The MS shall not use uplink discontinuous transmission on TCH-H.",
                "The MS shall not use uplink discontinuous transmission on a TCH-F. The MS shall not use uplink discontinuous transmission on TCH-H.",
                "The MS shall use uplink discontinuous transmission on a TCH-F. The MS may use uplink discontinuous transmission on TCH-H.",
                "The MS may use uplink discontinuous transmission on a TCH-F. The MS may use uplink discontinuous transmission on TCH-H.",
                "The MS shall use uplink discontinuous transmission on a TCH-F. The MS shall use uplink discontinuous transmission on TCH-H.",
                "The MS shall not use uplink discontinuous transmission on a TCH-F. The MS shall use uplink discontinuous transmission on TCH-H.",
                "The MS may use uplink discontinuous transmission on a TCH-F. The MS shall use uplink discontinuous transmission on TCH-H."
            };

            msg[0] = "Radio-Link-Timeout:  ";
            msg[0] += 4 * ((pduData[0] & 0x0F)+1);

            msg[1] = "DTX:                 ";
            msg[1] += dtxDesc[((pduData[0]&0x80) >> 5) | ((pduData[0] >> 4) & 0x0F)];

            msg[2] = "PWRC:                ";
            if ((pduData[0] & 0x40) != 0)
                msg[2] += "true";
            else
                msg[2] += "false";


            return msg;

        }
        private string[] HandleCellOptionsBCCH(byte[] pduData)
        {
            string[] msg = new string[3];
            string[] dtxDesc = new[]
            {
                "The MSs may use uplink discontinuous transmission.",
                "The MSs shall use uplink discontinuous transmission.",
                "The MS shall not use uplink discontinuous transmission.",
                "Undefined."
            };

            msg[0] = "Radio-Link-Timeout:  ";
            msg[0] += 4 * ((pduData[0] & 0x0F) + 1);

            msg[1] = "DTX:                 ";
            msg[1] += dtxDesc[((pduData[0] >> 4) & 0x0F)];

            msg[2] = "PWRC:                ";
            if ((pduData[0] & 0x40) != 0)
                msg[2] += "true";
            else
                msg[2] += "false";

            return msg;
        }

        private string[] HandleCCCHDesc(byte[] pduData)
        {
            string[] msg = new string[5];

            msg[0] = "CCCHConf:  ";
            BCDAdd(ref msg[0], pduData[0] & 0x07);

            msg[1] = "BsAgBlk:   ";
            BCDAdd(ref msg[1], (pduData[0]>>3) & 0x07);

            msg[2] = "ATT:       ";
            if ((pduData[0] & 0x40) != 0)
                msg[2] += "true";
            else
                msg[2] += "false";

            msg[3] = "BsPaMFrms: ";
            BCDAdd(ref msg[3], pduData[1] & 0x07);

            msg[4] = "T3212:     ";
            BCDAdd(ref msg[4], pduData[2]);

            return msg;
        }

        private string[] HandleCellIdentity(byte[] pduData)
        {
            string[] msg = new string[1];
            msg[0] = "CellID:    ";

            msg[0] += (pduData[0] << 8) | pduData[1];

            return msg;
        }

        private string[] HandleLAI(byte[] pduData)
        {
            string[] msg = new string[2];
            msg[0] = "MCC/MNC:   ";

            BCDAdd(ref msg[0], pduData[0] & 0x0F);
            BCDAdd(ref msg[0], (pduData[0] >> 4) & 0x0F);
            BCDAdd(ref msg[0], pduData[1] & 0x0F);

            msg[0] += "-";
            BCDAdd(ref msg[0], pduData[2] & 0x0F);
            BCDAdd(ref msg[0], (pduData[2] >> 4) & 0x0F);
            BCDAdd(ref msg[0], (pduData[1] >> 4) & 0x0F);

            msg[1] = "LAC:       ";
            msg[1] += (pduData[3] << 8) | pduData[4];

            return msg;
        }

        private void BCDAdd(ref string txt, int bcd)
        {
            if (bcd != 0x0f)
                txt += bcd;
        }


        public void Handle(byte[] l3Data, int start)
        {
            if (l3Data.Length > 0)
            {
                eProtocol PD = (eProtocol)(l3Data[start+0] & 0x0F);
                int packetType = l3Data[start + 1];

                if (PD == eProtocol.RadioResource)
                {
                    if (pktTypes.Get(packetType) != null)
                    {
                        StatusMessage = "RadioResource: " + pktTypes.Get(packetType).description + Environment.NewLine;
                        StatusMessage += Decode(l3Data, start+2, pktTypes.Get(packetType).refDown);
                    }
                    else
                    {
                        StatusMessage = "Unknown RadioResource: " + packetType + " ";
                    }
                }
                else
                {
                    StatusMessage = PD + " " + packetType;
                }
            }
        }

        public string Decode ( byte[] l3Data, int start, string reference )
        {
            string text = "";
            int currentPos = start;
            string skipPDUs = ";10.5.2.19;10.2;10.3.1;10.4;";

            MessageInfo message = msgList.Get(reference);
            if (message == null)
                return "Unknown: " + reference;

            foreach (MessageSlotInfo slotInfo in message.Slots)
            {
                PDUInfo pduInfo = pduList.Get(slotInfo.reference);

                if (pduInfo == null)
                {
                    /* just inform about PDUs that are not skipped (pseudo-L3 headers) */
                    if (!skipPDUs.Contains(";" + slotInfo.reference + ";"))
                        text += "   Unknown PDU " + slotInfo.reference + Environment.NewLine;
                }
                else
                {
                    int pduLength = pduInfo.length;

                    if (currentPos + pduInfo.length > l3Data.Length)
                    {
                        text += "   PDU too long " + slotInfo.reference + Environment.NewLine;
                        return text;
                    }

                    /* rest octet type */
                    if (pduInfo.type == 5)
                        pduLength = l3Data.Length - currentPos;

                    if (slotInfo.presence != "M" && l3Data[currentPos] != slotInfo.iei)
                    {
                        /* this pdu is optional and doesnt appear to be present */
                    }
                    else
                    {
                        /* thats the PDU that should be there, dump it */
                        byte[] pduData = new byte[pduLength];

                        Array.Copy(l3Data, currentPos, pduData, 0, pduData.Length);
                        currentPos += pduData.Length;

                        if (PDUParser.ContainsKey(pduInfo.reference))
                        {
                            text += "          " + pduInfo.name + " ("+pduInfo.reference+"):" + Environment.NewLine;
                            string[] info = PDUParser[pduInfo.reference](pduData);
                            foreach (string s in info)
                                text += "             " + s + Environment.NewLine;
                        }
                        else
                        {
                            text += "          " + pduInfo.name + " (" + pduInfo.reference + "):" + Environment.NewLine;
                            text += "             " + DumpBytes(pduData) + Environment.NewLine;
                        }
                    }
                }

            }

            return text;
        }


        protected string DumpBytes(byte[] data)
        {
            string msg = "";

            foreach (byte value in data)
                msg += String.Format("{0:X02} ", value);

            return msg;
        }
    }
}
