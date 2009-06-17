using System.Collections.Generic;
using System.Xml;

namespace LibRXFFT.Libraries.GSM.Misc
{
    public class MCCTable
    {
        public Dictionary<string, MCCEntry> Map = new Dictionary<string, MCCEntry>();

        public MCCTable(string file)
        {
            XmlTextReader reader = new XmlTextReader(file);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if ("entry".Equals(reader.Name))
                        {
                            MCCEntry info = new MCCEntry();

                            while (reader.MoveToNextAttribute())
                            {
                                if ("mcc".Equals(reader.Name))
                                    info.MCC = reader.Value;

                                if ("cc".Equals(reader.Name))
                                    info.CC = reader.Value;

                                if ("country".Equals(reader.Name))
                                    info.Country = reader.Value;

                            }

                            /* check if the MCC is already added. if yes just add the country name */
                            if (Get(info.MCC) != null)
                            {
                                Get(info.MCC).CC += "/" + info.CC;
                                Get(info.MCC).Country += "/" + info.Country;
                            }
                            else
                                Map.Add(info.MCC, info);
                        }
                        break;

                    case XmlNodeType.Text:
                        break;

                    case XmlNodeType.EndElement:
                        break;
                }
            }
        }


        public MCCEntry Get(string cc)
        {
            if (!Map.ContainsKey(cc))
                return null;

            return Map[cc];
        }

    }

    public class MCCEntry
    {
        public string MCC;
        public string CC;
        public string Country;
    }
}