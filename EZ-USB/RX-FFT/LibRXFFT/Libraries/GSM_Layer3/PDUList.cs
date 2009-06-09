using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace LibRXFFT.Libraries.GSM_Layer3
{
    public class PDUInfo
    {
        public int type;
        public int length;
        public string reference;
        public string name;
    }

    public class PDUList
    {


        public Dictionary<string, PDUInfo> Map = new Dictionary<string, PDUInfo>();

        public PDUList()
        {
            XmlTextReader reader = new XmlTextReader("D:\\cygwin\\home\\g3gg0\\dct3\\opengpa\\xml\\pdulist.xml");
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if ("pdudesc".Equals(reader.Name))
                        {
                            PDUInfo info = new PDUInfo();

                            while (reader.MoveToNextAttribute())
                            {
                                if ("type".Equals(reader.Name))
                                    info.type = int.Parse(reader.Value);

                                if ("ref".Equals(reader.Name))
                                    info.reference = reader.Value;

                                if ("length".Equals(reader.Name))
                                    info.length = int.Parse(reader.Value);

                                if ("name".Equals(reader.Name))
                                    info.name = reader.Value;

                            }
                            Map.Add(info.reference, info);
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

        public PDUInfo Get(string reference)
        {
            if (!Map.ContainsKey(reference))
                return null;

            return Map[reference];
        }

    }
}
