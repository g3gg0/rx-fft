using System.Collections.Generic;
using System.Xml;

namespace LibRXFFT.Libraries.GSM.Misc
{
    public class MNCTable
    {
        public Dictionary<string, Dictionary<string, MNCEntry>> Map = new Dictionary<string, Dictionary<string, MNCEntry>>();

        public MNCTable(string file)
        {
            XmlTextReader reader = new XmlTextReader(file);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if ("entry".Equals(reader.Name))
                        {
                            MNCEntry info = new MNCEntry();

                            while (reader.MoveToNextAttribute())
                            {
                                if ("mcc".Equals(reader.Name))
                                    info.MCC = reader.Value;

                                if ("mnc".Equals(reader.Name))
                                    info.MNC = reader.Value;

                                if ("network".Equals(reader.Name))
                                    info.Network = reader.Value;

                            }

                            /* check if the MCC is already added. if yes just add the country name */
                            if (Map.ContainsKey(info.MCC))
                            {
                                Dictionary<string, MNCEntry> list = Map[info.MCC];

                                if (list.ContainsKey(info.MNC))
                                {
                                    MNCEntry entry = list[info.MNC];

                                    entry.MNC += "/" + info.MNC;
                                    entry.Network += "/" + info.Network;
                                }
                                else
                                    list.Add(info.MNC, info);
                            }
                            else
                            {
                                Dictionary<string, MNCEntry> list = new Dictionary<string, MNCEntry>();
                                list.Add(info.MNC, info);
                                Map.Add(info.MCC, list);
                            }
                        }
                        break;

                    case XmlNodeType.Text:
                        break;

                    case XmlNodeType.EndElement:
                        break;
                }
            }
        }


        public MNCEntry Get(string mcc, string mnc)
        {
            if (!Map.ContainsKey(mcc))
                return null;

            if(!Map[mcc].ContainsKey(mnc))
                return null;

            return Map[mcc][mnc];
        }

    }

    public class MNCEntry
    {
        public string MCC;
        public string MNC;
        public string Network;
    }
}