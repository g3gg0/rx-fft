using System;
using System.Xml;
using System.Collections;

using JabberNET.XMPP;
using JabberNET.XMPP.Stanzas;

namespace JabberNET.Roster
{
    public class JabberRoster : IEnumerable
    {
        private Hashtable contacts;
        public JabberGroupList Groups;

        string xmlRoster;

        public JabberRoster()
        {
            contacts = new Hashtable();
            Groups = new JabberGroupList(contacts);
        }

        public JabberRoster(string XmlRoster)
            : this()
        {
            Load(XmlRoster);
        }

        public IEnumerator GetEnumerator()
        {
            return contacts.Values.GetEnumerator();
        }

        public JabberContact this[string JID]
        {
            get
            {
                return (JabberContact)contacts[JID];
            }
        }

        public bool Load(string XmlRoster)
        {
            bool loaded = true;
            IqStanza iq = null;
            try
            {
                iq = new IqStanza(XmlRoster);
            }
            catch
            {
                // Imprimir mensaje de error
                Console.WriteLine("Wrong XML Roster format.");
                iq = null;
                loaded = false;
            }
            if (iq != null)
            {
                xmlRoster = XmlRoster;
                if (iq.Name == "iq" && iq.Type == "result" && iq.Id == "roster")
                {
                    XmlNode query = iq.ChildNode;
                    if (query != null && query.Name == "query" && query.Attributes["xmlns"].InnerText == "jabber:iq:roster")
                    {
                        if (query.HasChildNodes)
                        {
                            foreach (XmlNode item in query.ChildNodes)
                            {
                                //if (item.Name == "item" && item.Attributes ["jid"] != null && item.Attributes ["name"] != null && item.Attributes ["subscription"] != null) {
                                if (item.Name == "item" && item.Attributes["jid"] != null && item.Attributes["subscription"] != null)
                                {
                                    JabberContact contact = NewContact(item.Attributes["jid"].InnerText);
                                    if (item.Attributes["name"] != null)
                                        contact.Name = item.Attributes["name"].InnerText;
                                    string subscription = item.Attributes["subscription"].InnerText;
                                    if (subscription == "none" || subscription == "to" || subscription == "from" || subscription == "both")
                                        contact.Subscription = (JabberContactSubscription)Enum.Parse(typeof(JabberContactSubscription), item.Attributes["subscription"].InnerText, true);
                                    else
                                        throw new IncorrectValue("Contact \"subscription\" = {none, to, from, both}");
                                    if (item.Attributes["ask"] != null)
                                        contact.Ask = item.Attributes["ask"].InnerText;
                                    if (item.HasChildNodes)
                                    {
                                        XmlNode group = item.FirstChild;
                                        while (group != null)
                                        {
                                            if (group.Name == "group")
                                            {
                                                if (!Groups.ContainsGroup(group.InnerText))
                                                    Groups.Create(group.InnerText);
                                                Groups[group.InnerText].Add(contact.JID);
                                            }
                                            group = group.NextSibling;
                                        }
                                    }
                                    //Console.WriteLine ("Roster: Loaded contact " + contact.JID);
                                }
                            }
                        }
                    }
                }
            }
            return loaded;
        }

        public JabberContact NewContact(string JID)
        {
            JabberContact contact = null;
            if (!contacts.ContainsKey(JID))
            {
                contact = new JabberContact(JID);
                contacts.Add(contact.JID, contact);
            }
            return contact;
        }

        public JabberContact NewContact(string JID, string Name)
        {
            JabberContact contact = NewContact(JID);
            if (contact != null)
                contact.Name = Name;
            return contact;
        }

        public void DeleteContact(string JID)
        {
            JabberContact contact = (JabberContact)contacts[JID];
            if (contact != null)
            {
                contacts.Remove(JID);
                foreach (string group in contact.Groups)
                {
                    Groups[group].Remove(JID);
                }
            }
        }


        public string Print()
        {
            // print fancy roster
            foreach (JabberGroup group in Groups)
            {
                Console.WriteLine("[" + group.Name + "]");
                foreach (JabberContact contact in group)
                {
                    Console.WriteLine("  - " + contact.Name + " <" + contact.JID + ">");
                }
            }
            Console.WriteLine("[NoGroup]");
            foreach (JabberContact contact in contacts.Values)
            {
                if (contact.Groups == null)
                    Console.WriteLine("  - " + contact.ToString());
                //Console.WriteLine ("  - " + contact.Name + " <" + contact.JID + "> (" + contact.Presence + ")");
            }
            return xmlRoster;
        }
    }
}
