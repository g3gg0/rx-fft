using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace LibRXFFT.Libraries.GSM.Layer3
{
    public class L3PDUInfo
    {
        public int Type;
        public int Length;
        public string Reference;
        public string Name;
        public ArrayList Fields = new ArrayList();
        public string TriggerPre;
        public string TriggerPost;
    }

    public class L3PDUField
    {
        public string Name;
        public string Type;
        public double Factor = 1;
        public double Offset = 0;
        public ArrayList Bits = new ArrayList();
        public string[] Strings;
        public int Length;
        public string SetField;
        public string TriggerPre;
        public string TriggerPost;
        public string Parser;
    }

    public class Bits
    {
        public int Octet;
        public int Start;
        public int Count;
    }

    public class L3PDUList
    {
        public Dictionary<string, L3PDUInfo> Map = new Dictionary<string, L3PDUInfo>();

        private L3PDUInfo ParseL3PDUInfo(XmlTextReader reader)
        {
            bool fields = false;
            L3PDUInfo info = new L3PDUInfo();

            while (reader.MoveToNextAttribute())
            {
                if ("type".Equals(reader.Name))
                    info.Type = int.Parse(reader.Value);

                if ("trigger-pre".Equals(reader.Name))
                    info.TriggerPre = reader.Value;

                if ("trigger-post".Equals(reader.Name))
                    info.TriggerPost = reader.Value;

                if ("ref".Equals(reader.Name))
                    info.Reference = reader.Value;

                if ("length".Equals(reader.Name))
                    info.Length = int.Parse(reader.Value);

                if ("name".Equals(reader.Name))
                    info.Name = reader.Value;

                if ("fields".Equals(reader.Name))
                {
                    if ("true".Equals(reader.Value))
                        fields = true;
                }
            }

            while (fields && reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if ("field".Equals(reader.Name))
                        {
                            L3PDUField field = ParseL3PDUField(reader);
                            info.Fields.Add(field);
                        }
                        break;

                    case XmlNodeType.Text:
                        break;

                    case XmlNodeType.EndElement:
                        return info;
                }
            }

            return info;
        }

        private L3PDUField ParseL3PDUField(XmlTextReader reader)
        {
            L3PDUField field = new L3PDUField();

            while (reader.MoveToNextAttribute())
            {
                if ("type".Equals(reader.Name))
                    field.Type = reader.Value;

                if ("set".Equals(reader.Name))
                    field.SetField = reader.Value;

                if ("name".Equals(reader.Name))
                    field.Name = reader.Value;

                if ("parser".Equals(reader.Name))
                    field.Parser = reader.Value;

                if ("trigger-pre".Equals(reader.Name))
                    field.TriggerPre = reader.Value;

                if ("trigger-post".Equals(reader.Name))
                    field.TriggerPost = reader.Value;

                if ("length".Equals(reader.Name))
                    field.Length = int.Parse(reader.Value);

                if ("factor".Equals(reader.Name))
                    field.Factor = double.Parse(reader.Value);

                if ("offset".Equals(reader.Name))
                    field.Offset = double.Parse(reader.Value);

                if ("strings".Equals(reader.Name))
                    field.Strings = new string[int.Parse(reader.Value)];
            }

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if ("bits".Equals(reader.Name))
                        {
                            Bits bits = new Bits();

                            while (reader.MoveToNextAttribute())
                            {
                                if ("octet".Equals(reader.Name))
                                    bits.Octet = int.Parse(reader.Value);

                                if ("start".Equals(reader.Name))
                                    bits.Start = int.Parse(reader.Value);

                                if ("count".Equals(reader.Name))
                                    bits.Count = int.Parse(reader.Value);
                            }

                            field.Bits.Add(bits);
                        }
                        else if ("string".Equals(reader.Name))
                        {
                            int index = 0;
                            string str = "";

                            while (reader.MoveToNextAttribute())
                            {
                                if ("index".Equals(reader.Name))
                                    index = int.Parse(reader.Value);

                                if ("text".Equals(reader.Name))
                                    str = reader.Value;
                            }

                            field.Strings[index] = str;
                        }
                        break;

                    case XmlNodeType.Text:
                        break;

                    case XmlNodeType.EndElement:
                        return field;
                }
            }

            return field;
        }

        public L3PDUList(string file)
        {
            XmlTextReader reader = new XmlTextReader(file);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if ("pdudesc".Equals(reader.Name))
                        {
                            L3PDUInfo info = ParseL3PDUInfo(reader);
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

        public L3PDUInfo Get(string reference)
        {
            if (!Map.ContainsKey(reference))
                return null;

            return Map[reference];
        }
    }
}