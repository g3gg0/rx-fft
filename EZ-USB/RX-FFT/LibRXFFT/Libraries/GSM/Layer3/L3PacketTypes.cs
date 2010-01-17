using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace LibRXFFT.Libraries.GSM.Layer3
{
    public class L3PacketInfo
    {
        public int Type;
        public string RefDown;
        public string RefUp;
        public string Description;
    }

    public class L3PacketTypes
    {
        public Dictionary<int, L3PacketInfo> Map = new Dictionary<int, L3PacketInfo>();

        public L3PacketTypes(string file)
        {
            XmlTextReader reader = new XmlTextReader(file);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if ("packettype".Equals(reader.Name))
                        {
                            L3PacketInfo info = new L3PacketInfo();

                            while (reader.MoveToNextAttribute())
                            {
                                if ("type".Equals(reader.Name))
                                    info.Type = int.Parse(reader.Value, NumberStyles.HexNumber);

                                if ("ref-down".Equals(reader.Name))
                                    info.RefDown = reader.Value;

                                if ("ref-up".Equals(reader.Name))
                                    info.RefUp = reader.Value;

                                if ("desc".Equals(reader.Name))
                                    info.Description = reader.Value;
                            }

                            Map.Add(info.Type, info);
                        }
                        break;

                    case XmlNodeType.Text:
                        break;

                    case XmlNodeType.EndElement:
                        break;
                }
            }
        }


        public L3PacketInfo Get(int type)
        {
            if (!Map.ContainsKey(type))
                return null;

            return Map[type];
        }
    }
}