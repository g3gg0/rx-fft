using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace LibRXFFT.Libraries.GSM_Layer3
{
    public class MessageInfo
    {
        public string name;
        public string reference;
        public string significance;
        public string direction;

        public ArrayList Slots = new ArrayList();
    }

    public class MessageSlotInfo
    {
        public int iei;
        public string reference;
        public string presence;
    }

    public class L3Messages
    {
        public Dictionary<string, MessageInfo> Map = new Dictionary<string, MessageInfo>();

        public L3Messages()
        {
            XmlTextReader reader = new XmlTextReader("D:\\cygwin\\home\\g3gg0\\dct3\\opengpa\\xml\\messagelist.xml");
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if ("messagedesc".Equals(reader.Name))
                        {
                            MessageInfo info = new MessageInfo();
                            bool nodeDone = false;

                            while (reader.MoveToNextAttribute())
                            {
                                if ("ref".Equals(reader.Name))
                                    info.reference = reader.Value;

                                if ("significance".Equals(reader.Name))
                                    info.significance = reader.Value;

                                if ("direction".Equals(reader.Name))
                                    info.direction = reader.Value;

                                if ("name".Equals(reader.Name))
                                    info.name = reader.Value;
                            }

                            while (!nodeDone && reader.Read())
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if ("pduslot".Equals(reader.Name))
                                        {
                                            MessageSlotInfo slot = new MessageSlotInfo();

                                            while (reader.MoveToNextAttribute())
                                            {
                                                if ("iei".Equals(reader.Name))
                                                    slot.iei = int.Parse(reader.Value, NumberStyles.HexNumber);

                                                if ("ref".Equals(reader.Name))
                                                    slot.reference = reader.Value;

                                                if ("presence".Equals(reader.Name))
                                                    slot.presence = reader.Value;

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

                            Map.Add(info.reference, info);
                        }
                        break;

                    case XmlNodeType.Text:
                        break;

                    case XmlNodeType.EndElement:
                        break;
                }
            }
        }


        public MessageInfo Get(string reference)
        {
            if (!Map.ContainsKey(reference))
                return null;

            return Map[reference];
        }

    }

}
