using System;
using System.Collections;
using System.Xml;

using JabberNET.Roster;

namespace JabberNET.XMPP.Stanzas
{

    public abstract class Stanza
    {
        public string Name;
        public string Xml;
        public XmlDocument XmlDoc;

        // Common attributes
        public string To;
        public string From;
        public string ToRessource;
        public string FromResource;
        public string Id;
        public string Type;
        public string XmlLang;

        public Stanza()
        {
        }

        public Stanza(XmlDocument xml)
        {
            Xml = xml.InnerXml;
            XmlDoc = xml;
            load();
        }

        public Stanza(string xml)
        {
            Xml = xml;
            XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);
            load();
        }

        void load()
        {
            if (XmlDoc != null)
            {
                XmlNode node = XmlDoc.FirstChild;
                Name = node.Name;
                if (node.Attributes["to"] != null)
                {
                    To = node.Attributes["to"].InnerText;
                    ToRessource = To;
                    int index = To.IndexOf("/");
                    if (index > 0)
                        To = To.Substring(0, index);
                }
                if (node.Attributes["from"] != null)
                {
                    From = node.Attributes["from"].InnerText;
                    FromResource = From;
                    int index = From.IndexOf("/");
                    if (index > 0)
                        From = From.Substring(0, index);
                }
                if (node.Attributes["id"] != null) Id = node.Attributes["id"].InnerText;
                if (node.Attributes["type"] != null) Type = node.Attributes["type"].InnerText;
                if (node.Attributes["xml:lang"] != null) XmlLang = node.Attributes["xml:lang"].InnerText;
            }
        }
    }



    public class MessageStanza : Stanza
    {

        // Type = {chat, error, groupchat, headline, normal}

        public XmlNode ChildErrorNode;
        public string ChildError;
        public Hashtable Subject;
        public Hashtable Body;
        public string Thread;

        public MessageStanza(XmlDocument XmlMsg)
            : base(XmlMsg)
        {
            loadData();
        }

        public MessageStanza(string XmlMsg)
            : base(XmlMsg)
        {
            loadData();
        }

        public MessageStanza()
        {
        }

        void loadData()
        {
            if (Type == null)
                Type = "normal";

            XmlNode message = XmlDoc.FirstChild;
            if (message.Name == "message")
            {

                if (Type != "chat" && Type != "error" && Type != "groupchat" && Type != "headline" && Type != "normal")
                    // launch error: incorrect type of type attribute
                    throw new IncorrectTypeOfAttribute("For Messages \"type\" must be {chat, error, groupchat, headline, normal}.");

                if (Type == "error")
                {

                    // TODO: Error

                }
                else
                {
                    if (message.HasChildNodes)
                    {
                        bool thread = false;
                        foreach (XmlNode node in message.ChildNodes)
                        {
                            switch (node.Name)
                            {
                                case "subject":
                                    {
                                        if (node.Attributes != null && node.Attributes.Count <= 1)
                                        {
                                            if (Subject == null)
                                                Subject = new Hashtable();
                                            string lang = null;
                                            if (node.Attributes["xml:lang"] != null)
                                                lang = node.Attributes["xml:lang"].InnerText;
                                            if (lang == null)
                                                lang = "";
                                            Subject.Add(lang, node.InnerText);
                                        }
                                        else
                                            throw new IncorrectNumberOfAttributes("Message \"subject\" can only have one \"xml:lang\" attribute.");
                                        break;
                                    }
                                case "body":
                                    {
                                        if (node.Attributes != null && node.Attributes.Count <= 1)
                                        {
                                            if (Body == null)
                                                Body = new Hashtable();
                                            string lang = null;
                                            if (node.Attributes["xml:lang"] != null)
                                                lang = node.Attributes["xml:lang"].InnerText;
                                            if (lang == null)
                                                lang = "";
                                            Body.Add(lang, node.InnerText);
                                        }
                                        else
                                            throw new IncorrectNumberOfAttributes("Message \"body\" can only have one \"xml:lang\" attribute.");
                                        break;
                                    }

                                case "thread":
                                    {
                                        if (!thread)
                                        {
                                            thread = true;
                                            if (node.Attributes != null && node.Attributes.Count == 0)
                                            {
                                                Thread = node.InnerText;
                                            }
                                            else
                                                throw new IncorrectNumberOfAttributes("Message \"thread\" must not have attributes.");
                                        }
                                        else
                                            throw new IncorrectNumberOfNodes("Message must have only one \"thread\" child.");
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            else
            {
                // launch error: incorrect node
                throw new IncorrectNameOfNode("Expecting \"message\" node.");
            }
        }
    }



    public class PresenceStanza : Stanza
    {

        // Must = {?}
        //   Type = {subscribe, subscribed, unsubscribe, unsusbscribed, probe, error} && available, unavailable
        //   Show = {away, chat, xa, dnd} && online, offline
        //   Subscription = {none, to, from, both}

        public JabberContactShow Show;
        public Hashtable Status;
        public int Priority = 0;

        public PresenceStanza(XmlDocument XmlPresence)
            : base(XmlPresence)
        {
            loadData();
        }

        public PresenceStanza(string XmlPresence)
            : base(XmlPresence)
        {
            loadData();
        }

        void loadData()
        {
            if (Type != null)
            {
                if (Type != "available" && Type != "unavailable" && Type != "subscribe" && Type != "subscribed" && Type != "unsubscribe" && Type != "unsubscribed" && Type != "probe" && Type != "error")
                    throw new IncorrectTypeOfAttribute("Presence \"type\" = {available, unavailable, subscribe, subscribed, unsubscribe, unsusbscribed, probe, error}.");
            }
            else
            {
                Type = "available";
            }
            XmlNode presence = XmlDoc.FirstChild;
            if (presence.Name == "presence")
            {

                if (Type == "error")
                {

                    // TODO: Error

                }
                else
                {
                    if (presence.HasChildNodes)
                    {

                        bool show = false, priority = false;
                        if (Type == "unavailable")
                        {
                            show = true;
                            Show = JabberContactShow.Offline;
                        }
                        foreach (XmlNode node in presence.ChildNodes)
                        {
                            switch (node.Name)
                            {

                                case "show":
                                    {
                                        if (!show)
                                        {
                                            show = true;
                                            if (node.Attributes != null && node.Attributes.Count == 0)
                                            {
                                                string showText = node.InnerText;
                                                if (showText != "away" && showText != "chat" && showText != "xa" && showText != "dnd")
                                                    throw new IncorrectTypeOfNode("Presence \"show\" = {away, chat, xa, dnd}.");
                                                else
                                                    Show = (JabberContactShow)Enum.Parse(typeof(JabberContactShow), showText, true);
                                            }
                                            else
                                                throw new IncorrectNumberOfAttributes("Presence \"show\" must not have attributes.");
                                        }
                                        else
                                            throw new IncorrectNumberOfNodes("Presence must have only one \"show\" child.");
                                        break;
                                    }
                                case "status":
                                    {
                                        if (node.Attributes != null && node.Attributes.Count <= 1)
                                        {
                                            if (Status == null)
                                                Status = new Hashtable();
                                            string lang = null;
                                            if (node.Attributes["xml:lang"] != null)
                                                lang = node.Attributes["xml:lang"].InnerText;
                                            if (lang == null)
                                                lang = "";
                                            Status.Add(lang, node.InnerText);
                                        }
                                        else
                                            throw new IncorrectNumberOfAttributes("Presence \"status\" can only have one \"xml:lang\" attribute.");
                                        break;
                                    }
                                case "priority":
                                    {
                                        if (!priority)
                                        {
                                            priority = true;
                                            if (node.Attributes != null && node.Attributes.Count == 0)
                                            {
                                                Priority = Convert.ToInt32(node.InnerText);
                                                if (Priority < -128 || Priority > 127)
                                                    throw new IncorrectTypeOfNode("Presence \"priority\" must be in range [-128:+127].");
                                            }
                                            else
                                                throw new IncorrectNumberOfAttributes("Presence \"priority\" must not have attributes.");
                                        }
                                        else
                                            throw new IncorrectNumberOfNodes("Presence must have only one \"priority\" child.");

                                        break;
                                    }
                            }
                        }
                        if (!show)
                            Show = JabberContactShow.Online;
                    }
                }
            }
            else
            {
                // launch error: incorrect node
                throw new IncorrectNameOfNode("Expecting \"presence\" node.");
            }
        }
    }



    public class IqStanza : Stanza
    {

        // Must = {?}
        //   Type = {get, set, result, error}

        public string Child;
        public XmlNode ChildNode;
        public string ChildError;
        public XmlNode ChildErrorNode;

        public IqStanza(XmlDocument XmlIq)
            : base(XmlIq)
        {
            loadData();
        }

        public IqStanza(string XmlIq)
            : base(XmlIq)
        {
            loadData();
        }

        void loadData()
        {
            XmlNode iq = XmlDoc.FirstChild;
            if (iq.Name == "iq")
            {

                if (Type == "get" || Type == "set")
                {

                    if (iq.ChildNodes.Count == 1)
                        Child = iq.OuterXml;
                    else
                    {
                        // launch error: incorrect number of childs	
                        throw new IncorrectNumberOfNodes("IQ (\"type\"={get,set}) must have one and only one child.");
                    }

                }
                else if (Type == "result")
                {

                    if (iq.ChildNodes.Count == 1)
                    {
                        Child = iq.InnerXml;
                        ChildNode = iq.FirstChild;
                    }
                    // no error if zero childs

                }
                else if (Type == "error")
                {

                    if (iq.ChildNodes.Count <= 2 || iq.ChildNodes.Count > 0)
                    {
                        foreach (XmlNode node in iq.ChildNodes)
                        {
                            if (node.Name == "error")
                            {
                                ChildError = node.OuterXml;
                                ChildNode = node;
                            }
                            else
                            {
                                Child = node.OuterXml;
                                ChildErrorNode = node;
                            }
                        }
                        if (ChildError == null)
                        {
                            // launch error: type=error must include error node
                            throw new IncorrectNumberOfNodes("IQ (\"type\"=error) must have one error child.");
                        }
                    }
                    else
                    {
                        // launch error: incorrect number of childs	
                        throw new IncorrectNumberOfNodes("IQ (\"type\"=error) must have one or two childs.");
                    }
                }
                else
                {
                    throw new IncorrectTypeOfAttribute("IQ \"type\" = {get, set, result, error}.");
                }

            }
            else
            {
                // launch error: incorrect node
                throw new IncorrectNameOfNode("Expecting \"iq\" node.");
            }
        }
    }
}
