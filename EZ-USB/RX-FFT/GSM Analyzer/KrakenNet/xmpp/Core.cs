using System;
using System.Text;
using System.Xml;
using System.Collections;


namespace JabberNET.XMPP
{

    /********************************************************************************************************
     *
     *  Extensible Messaging and Presence Protocol (XMPP): Core
     *
     *  Core features of the Extensible Messaging and Presence Protocol (XMPP), a protocol for streaming 
     *  Extensible Markup Language (XML) elements in order to exchange structured information in close 
     *  to real time between any two network endpoints.
     *
     *  -----
     *
     *  Author: Daniel Pecos
     *  Version: 17.01.2004
     *
     ********************************************************************************************************/

    public class Core : JabberProtocol
    {

        public static string StartStream(string To, string From, string Id, string Xmllang, string Version)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<stream:stream xmlns=\"jabber:client\" xmlns:stream=\"http://etherx.jabber.org/streams\"");
            if (To != null && To != "")
                sb.Append(" to=\"" + To + "\"");
            if (From != null && From != "")
                sb.Append(" from=\"" + From + "\"");
            if (Id != null && Id != "")
                sb.Append(" id=\"" + Id + "\"");
            if (Xmllang != null && Xmllang != "")
                sb.Append(" xmlns:lang=\"" + Xmllang + "\"");
            if (Version != null && Version != "")
                sb.Append(" version=\"" + Version + "\"");
            sb.Append(">");

            return sb.ToString();
        }

        public static string CloseStream()
        {
            return "</stream>";
        }

        private static XmlDocument CreateStanza(string Name, string From, string To, string Id, string Type, string Xmllang)
        {

            XmlDocument stanza = null;

            if (Name != null && Name != "")
            {
                stanza = new XmlDocument();
                XmlElement node = stanza.CreateElement(Name);
                stanza.AppendChild(node);

                if (From != null && From != "")
                    node.SetAttribute("from", From);

                if (To != null && To != "")
                    node.SetAttribute("to", To);

                if (Id != null && Id != "")
                    node.SetAttribute("id", Id);

                if (Type != null && Type != "")
                    node.SetAttribute("type", Type);

                if (Xmllang != null && Xmllang != "")
                    node.SetAttribute("xml:lang", Xmllang);
            }

            return stanza;
        }

        public static XmlDocument Message(string Type, string Lang, string JID, string MyJID, Hashtable Subject, Hashtable Body, string Thread)
        {

            if (Type == null)
                Type = "normal";

            XmlDocument xml = CreateStanza("message", MyJID, JID, null, Type, Lang);

            XmlNode message = xml.FirstChild;

            foreach (string lang in Subject.Keys)
            {
                XmlElement subject = xml.CreateElement("subject");
                message.AppendChild(subject);
                if (lang != "")
                    subject.SetAttribute("xml:lang", lang);
                subject.InnerText = (string)Subject[lang];
            }

            if (Body != null)
                foreach (string lang in Body.Keys)
                {
                    XmlElement body = xml.CreateElement("body");
                    message.AppendChild(body);
                    if (lang != "")
                        body.SetAttribute("xml:lang", lang);
                    body.InnerText = (string)Body[lang];
                }

            if (Thread != null)
            {
                XmlElement thread = xml.CreateElement("thread");
                message.AppendChild(thread);
                thread.InnerText = Thread;
            }
            Console.WriteLine("----------" + xml.InnerXml);

            return xml;
        }


        public static XmlDocument Presence(string Lang, string JID, string MyJID, string Show, Hashtable Status, int Priority)
        {
            // show = {away, chat, xa, dnd}
            XmlDocument xml = CreateStanza("presence", MyJID, JID, null, null, Lang);

            XmlNode presence = xml.FirstChild;

            if (Show != null)
            {
                XmlElement show = xml.CreateElement("show");
                presence.AppendChild(show);
                show.InnerText = Show;
            }
            if (Status != null)
            {
                XmlElement status = xml.CreateElement("status");
                xml.AppendChild(status);
                foreach (string lang in Status.Keys)
                {
                    status.SetAttribute("xml:lang", lang);
                    status.InnerText = (string)Status[lang];
                }
            }
            if (Priority != 0)
            {
                XmlElement priority = xml.CreateElement("priority");
                xml.AppendChild(priority);
                priority.InnerText = Priority.ToString();
            }
            return xml;
        }


        public static XmlDocument IQ(string Type, string Id, string JID, string MyJID)
        {

            XmlDocument xml = null;

            if (Type != null && Type != "")
            {
                if (Type == "get" || Type == "set" || Type == "error" || Type == "result")
                    if (Id != null && Id != "")
                    {
                        xml = CreateStanza("iq", MyJID, JID, Id, Type, null);
                    }
            }

            return xml;
        }


        /* ---------------------------------------------------------------------------------------------*/


        public static bool StartStreamResponse(string Server, string Response, out string ServerID)
        {
            cleanErrors();
            bool result = true;

            ServerID = null;

            XmlDocument xml = new XmlDocument();
            try
            {
                string response = Response.Substring(0, Response.Length - 1) + "/>";
                xml.LoadXml(response);

                XmlNode currentNode = xml.ChildNodes[0];

                if (currentNode.Name == "xml")
                    currentNode = xml.ChildNodes[1];

                if (currentNode.Name == "stream:stream")
                {
                    if (currentNode.Attributes["from"].InnerText == Server)
                    {
                        if (currentNode.Attributes["xmlns"].InnerText == "jabber:client")
                        {
                            if (currentNode.Attributes["xmlns:stream"].InnerText == "http://etherx.jabber.org/streams")
                            {
                                ServerID = currentNode.Attributes["id"].InnerText;
                            }
                            else
                            {
                                MsgError = "Wrong \"xmlns:stream\" attribute received from server.";
                                result = false;
                            }
                        }
                        else
                        {
                            MsgError = "Wrong \"xmlns\" attribute received from server.";
                            result = false;
                        }
                    }
                    else
                    {
                        MsgError = "Wrong \"from\" attribute received from server.";
                        result = false;
                    }
                }
                else
                {
                    MsgError = "Incorrect XML response received from server.";
                    result = false;
                }
            }
            catch (Exception e)
            {
                MsgError = "Error loading XML received from server (" + e.Message + ")";
                result = false;
            }

            return result;
        }

        public static Hashtable StanzaError(XmlDocument Stanza)
        {
            Hashtable errorHash = null;

            if (Stanza.HasChildNodes)
            {

                XmlNode first = Stanza.FirstChild;
                if (first.Attributes["type"].InnerXml == "error")
                {
                    errorHash = new Hashtable();
                    errorHash.Add("StanzaName", first.Name);
                    errorHash.Add("StanzaTo", first.Attributes["to"].InnerXml);
                    XmlNode error = first.FirstChild;
                    // continue, cancel, modify, auth, wait
                    errorHash.Add("ErrorType", error.Attributes["type"].InnerXml);
                    errorHash.Add("ErrorCondition", error.FirstChild.Name);
                    /*
                    if (error.ChildNodes ["text"]) {
                   XmlNode text = error.ChildNodes ["text"];
                   if (text.Attribute["xmlns"] == "urn:ietf:params:xml:ns:xmpp-stanzas")
                      errorHash.Add ("ErrorText", text.InnerXml);
                    }
                    */
                }
            }
            return errorHash;
        }

        /*
      
       Errores de JabberProtocol

        public static Hashtable CheckMessage (string XmlMsg) {
       Hashtable error = StanzaError (XmlMsg);
        }

        public static bool CheckPresence () {
        }

        public static bool CheckIQ () {
        }
  */
    }

}
