using System.Text;
using System.Xml;
using System.Collections;

using JabberNET.XMPP.Stanzas;

namespace JabberNET.XMPP
{

    /********************************************************************************************************
     *
     *  Extensible Messaging and Presence Protocol (XMPP): Instant Messaging and Presence
     *
     *  Extensions to and applications of the core features of the Extensible Messaging 
     *  and Presence Protocol (XMPP) that provide the basic instant messaging (IM) and presence 
     *  functionality defined in RFC 2779.
     *  
     *  -----
     *
     *  Author: Daniel Pecos
     *  Version: 17.01.2004
     *
     ********************************************************************************************************/

    public class IM : JabberProtocol
    {

        public static string Message(string JID, string MyJID, string Subject, string Body)
        {
            return Message(null, JID, MyJID, Subject, Body);
        }

        public static string Message(string Lang, string JID, string MyJID, string Subject, string Body)
        {
            Hashtable subject = new Hashtable();
            string lang = Lang;
            if (Lang == null)
                lang = "";
            subject.Add(lang, Subject);
            Hashtable body = null;
            if (Body != null)
            {
                body = new Hashtable();
                body.Add(lang, Body);
            }
            return XMPP.Core.Message("normal", Lang, JID, MyJID, subject, body, null).InnerXml;
        }

        public static string GroupChatMessage(string JID, string MyJID, string Subject, string Body)
        {
            return GroupChatMessage(null, JID, MyJID, Subject, Body);
        }

        public static string GroupChatMessage(string Lang, string JID, string MyJID, string Subject, string Body)
        {
            string lang = Lang;
            if (Lang == null)
                lang = "";

            Hashtable subject = new Hashtable();
            if (Subject != null)
            {
                subject.Add(lang, Subject);
            }

            Hashtable body = null;
            if (Body != null)
            {
                body = new Hashtable();
                body.Add(lang, Body);
            }
            return XMPP.Core.Message("groupchat", Lang, JID, MyJID, subject, body, null).InnerXml;
        }

        public static string ChatMessage(string JID, string MyJID, string Subject, string Body, string Thread)
        {
            return ChatMessage(null, JID, MyJID, Subject, Body, Thread);
        }

        public static string ChatMessage(string Lang, string JID, string MyJID, string Subject, string Body, string Thread)
        {
            Hashtable subject = new Hashtable();
            string lang = Lang;
            if (Lang == null)
                lang = "";
            subject.Add(lang, Subject);
            Hashtable body = null;
            if (Body != null)
            {
                body = new Hashtable();
                body.Add(lang, Body);
            }
            //if (Thread != null or Thread != "")
            //generate a new thread ID

            return XMPP.Core.Message("chat", Lang, JID, MyJID, subject, body, Thread).InnerXml;
        }

        public static string Presence(string JID, string type)
        {
            // type = {unavailable, subscribe, subscribed, unsubscribe, unsubscribed, probe, error}
            return "<presence to=\"" + JID + "\" type=\"" + type + "\"/>";
        }

        public static string Presence()
        {
            return Presence(null, null, null, null, 0);
        }

        public static string Presence(string JID, string MyJID, string Show, string Status, int Priority)
        {
            Hashtable status = null;
            if (Status != null)
            {
                status = new Hashtable();
                status.Add("", Status);
            }

            return XMPP.Core.Presence(null, JID, MyJID, Show, status, Priority).InnerXml;
        }

        public static string Roster(string MyJID)
        {
            XmlDocument xml = XMPP.Core.IQ("get", "roster", null, MyJID);
            XmlNode iq = xml.FirstChild;

            XmlElement query = xml.CreateElement("query");
            query.SetAttribute("xmlns", "jabber:iq:roster");
            iq.AppendChild(query);

            return xml.InnerXml;
        }

        public static string Ping(string MyJID)
        {
            XmlDocument xml = XMPP.Core.IQ("get", "c2a1", null, MyJID);
            XmlNode iq = xml.FirstChild;

            XmlElement query = xml.CreateElement("ping");
            query.SetAttribute("xmlns", "jabber:iq:ping");
            iq.AppendChild(query);

            return xml.InnerXml;            
        }
    }
}
