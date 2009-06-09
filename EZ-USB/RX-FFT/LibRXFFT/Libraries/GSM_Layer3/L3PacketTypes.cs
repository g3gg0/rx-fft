using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace LibRXFFT.Libraries.GSM_Layer3
{
    public class PacketInfo
    {
        public int type;
        public string refDown;
        public string refUp;
        public string description;
    }

    public class L3PacketTypes
    {
        public Dictionary<int, PacketInfo> Map = new Dictionary<int, PacketInfo>();

        public L3PacketTypes()
        {
            XmlTextReader reader = new XmlTextReader("D:\\cygwin\\home\\g3gg0\\dct3\\opengpa\\xml\\packeteering-rr.xml");
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if ("packettype".Equals(reader.Name))
                        {
                            PacketInfo info = new PacketInfo();

                            while (reader.MoveToNextAttribute())
                            {
                                if ("type".Equals(reader.Name))
                                    info.type = int.Parse(reader.Value, NumberStyles.HexNumber);

                                if ("ref-down".Equals(reader.Name))
                                    info.refDown = reader.Value;

                                if ("ref-up".Equals(reader.Name))
                                    info.refUp = reader.Value;

                                if ("desc".Equals(reader.Name))
                                    info.description = reader.Value;

                            }
                            Map.Add(info.type, info);
                        }
                        break;

                    case XmlNodeType.Text:
                        Console.WriteLine(reader.Value);
                        break;

                    case XmlNodeType.EndElement:
                        Console.Write("</" + reader.Name);
                        Console.WriteLine(">");
                        break;
                }
            }
        }


        public PacketInfo Get(int type)
        {
            if (!Map.ContainsKey(type))
                return null;

            return Map[type];
        }

    }

}
