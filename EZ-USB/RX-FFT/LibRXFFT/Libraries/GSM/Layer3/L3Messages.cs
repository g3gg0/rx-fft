using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace LibRXFFT.Libraries.GSM.Layer3
{
    public class L3MessageInfo
    {
        public string Name;
        public string Reference;
        public string Significance;
        public string Direction;
        public string TriggerPre;
        public string TriggerPost;

        public ArrayList Slots = new ArrayList();

        public override string ToString()
        {
            return String.Format("{0,-11}", "(" + Reference + ")") + Name;
        }
    }

    public class L3MessageSlotInfo
    {
        public int IEI;
        public string Reference;
        public string Presence;
        public string TriggerPre;
        public string TriggerPost;
    }

    public class L3Messages
    {
        public Dictionary<string, L3MessageInfo> Map = new Dictionary<string, L3MessageInfo>();

        public L3Messages(string file)
        {
            XmlTextReader reader = new XmlTextReader(file);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if ("messagedesc".Equals(reader.Name))
                        {
                            L3MessageInfo info = new L3MessageInfo();
                            bool nodeDone = false;

                            while (reader.MoveToNextAttribute())
                            {
                                if ("ref".Equals(reader.Name))
                                    info.Reference = reader.Value;

                                if ("significance".Equals(reader.Name))
                                    info.Significance = reader.Value;

                                if ("direction".Equals(reader.Name))
                                    info.Direction = reader.Value;

                                if ("name".Equals(reader.Name))
                                    info.Name = reader.Value;

                                if ("trigger-pre".Equals(reader.Name))
                                    info.TriggerPre = reader.Value;

                                if ("trigger-post".Equals(reader.Name))
                                    info.TriggerPost = reader.Value;
                            }

                            while (!nodeDone && reader.Read())
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if ("pduslot".Equals(reader.Name))
                                        {
                                            L3MessageSlotInfo slot = new L3MessageSlotInfo();

                                            while (reader.MoveToNextAttribute())
                                            {
                                                if ("iei".Equals(reader.Name))
                                                    slot.IEI = int.Parse(reader.Value, NumberStyles.HexNumber);

                                                if ("ref".Equals(reader.Name))
                                                    slot.Reference = reader.Value;

                                                if ("trigger-pre".Equals(reader.Name))
                                                    slot.TriggerPre = reader.Value;

                                                if ("trigger-post".Equals(reader.Name))
                                                    slot.TriggerPost = reader.Value;

                                                if ("presence".Equals(reader.Name))
                                                    slot.Presence = reader.Value;
                                            }
                                            info.Slots.Add(slot);
                                        }
                                        break;

                                    case XmlNodeType.Text:
                                        break;

                                    case XmlNodeType.EndElement:
                                        nodeDone = true;
                                        break;
                                }
                            }

                            Map.Add(info.Reference, info);
                        }
                        break;

                    case XmlNodeType.Text:
                        break;

                    case XmlNodeType.EndElement:
                        break;
                }
            }
        }


        public L3MessageInfo Get(string reference)
        {
            if (!Map.ContainsKey(reference))
                return null;

            return Map[reference];
        }
    }
}