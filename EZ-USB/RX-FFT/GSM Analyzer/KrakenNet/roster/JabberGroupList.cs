using System;
using System.Xml;
using System.Collections;

namespace JabberNET.Roster
{

    public class JabberGroupList : IEnumerable
    {
        Hashtable groups;
        Hashtable contacts;

        public JabberGroupList(Hashtable contacts)
        {
            groups = new Hashtable();
            this.contacts = contacts;
        }

        public IEnumerator GetEnumerator()
        {
            return groups.Values.GetEnumerator();
        }

        public JabberGroup this[string Name]
        {
            get
            {
                return (JabberGroup)groups[Name];
            }
        }

        public void Create(string Name)
        {
            JabberGroup g = new JabberGroup(Name, contacts);
            groups.Add(Name, g);
        }

        public void Delete(string Name)
        {
            ((JabberGroup)groups[Name]).Clear();
        }

        public bool ContainsGroup(string Name)
        {
            return groups[Name] != null;
        }
    }
}
