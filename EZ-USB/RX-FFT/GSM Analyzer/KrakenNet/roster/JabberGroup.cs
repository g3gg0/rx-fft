using System;
using System.Xml;
using System.Collections;

namespace JabberNET.Roster
{

    public class JabberGroup : IEnumerable
    {
        Hashtable members;
        Hashtable contacts;

        public string Name;

        public JabberGroup(string Name, Hashtable contacts)
        {
            this.Name = Name;
            this.contacts = contacts;
            members = new Hashtable();
        }

        public IEnumerator GetEnumerator()
        {
            return members.Values.GetEnumerator();
        }

        public JabberContact this[string JID]
        {
            get
            {
                return (JabberContact)members[JID];
            }
        }

        public void Add(string JID)
        {
            if (contacts.ContainsKey(JID))
            {
                ((JabberContact)contacts[JID]).AddToGroup(Name);
                members.Add(JID, contacts[JID]);
            }
        }

        public void Remove(string JID)
        {
            if (members.ContainsKey(JID))
            {
                ((JabberContact)members[JID]).RemoveFromGroup(Name);
                members.Remove(JID);
            }
        }

        public void Clear()
        {
            foreach (JabberContact c in members.Values)
                c.RemoveFromGroup(Name);
            members.Clear();
        }
    }
}
