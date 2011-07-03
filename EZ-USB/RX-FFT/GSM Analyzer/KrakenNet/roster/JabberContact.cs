using System;
using System.Text;
using System.Xml;
using System.Collections;

using JabberNET.XMPP.Stanzas;

namespace JabberNET.Roster
{
    public enum JabberContactShow
    {
        Online, Offline, Away, XA, DND, Chat
    }

    public enum JabberContactSubscription
    {
        None, To, From, Both
    }

    public class JabberContact
    {
        public readonly string JID = null;
        public readonly string Resource = null;
        public string Name = null;

        JabberContactSubscription subscription;
        public JabberContactSubscription Subscription
        {
            set
            {
                if (ReferenceEquals(value.GetType(), typeof(JabberContactSubscription)))
                    subscription = value;
                else
                    throw new IncorrectValue("Contact \"subscription\" = {none, to, from, both}");
            }
            get
            {
                return subscription;
            }
        }

        public string Presence
        {
            get
            {
                string presence = null;
                switch (Show)
                {
                    case JabberContactShow.Online:
                        {
                            presence = "Online";
                            break;
                        }
                    case JabberContactShow.Offline:
                        {
                            presence = "Offline";
                            break;
                        }
                    case JabberContactShow.Away:
                        {
                            presence = "Away";
                            break;
                        }
                    case JabberContactShow.XA:
                        {
                            presence = "Extended Away";
                            break;
                        }
                    case JabberContactShow.DND:
                        {
                            presence = "Do Not Disturb";
                            break;
                        }
                    case JabberContactShow.Chat:
                        {
                            presence = "Available for Chat";
                            break;
                        }
                }
                return presence;
            }
        }

        public string Type;

        JabberContactShow show;
        public JabberContactShow Show
        {
            set
            {
                if (ReferenceEquals(value.GetType(), typeof(JabberContactShow)))
                    show = value;
                else
                    throw new IncorrectValue("Contact \"show\" = {online, offline, away, xa, dnd, chat}");
            }
            get
            {
                return show;
            }
        }
        public Hashtable Status;
        public int Priority;
        public string Ask;
        public string[] Groups
        {
            get
            {
                if (groups.Count == 0)
                    return null;
                else
                {
                    string[] groupNames = new string[groups.Count];
                    groups.CopyTo(groupNames, 0);
                    return groupNames;
                }
            }
        }
        ArrayList groups;

        public JabberContact(string JID)
        {
            int index = JID.IndexOf("/");
            if (index > 0)
            {
                this.JID = JID.Substring(0, index);
                this.Resource = JID.Substring(index + 1, JID.Length - JID.IndexOf('/') - 1);
            }
            else
            {
                this.JID = JID;
                this.Resource = null;
            }
            Name = null;
            groups = new ArrayList();
            Show = JabberContactShow.Offline;
            Status = null;
            Priority = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Name != null && Name.Length > 0)
            {
                sb.Append(Name + " ");
            }

            sb.Append("<" + JID + ">");
            sb.Append(" [" + Subscription + "]");
            sb.Append(" (" + Presence + " - " + Priority);

            if (Status != null)
            {
                sb.Append(": ");
                foreach (string lang in Status.Keys)
                    if (lang != "")
                        sb.Append("{ " + lang + " -> " + Status[lang] + "} ");
                    else
                        sb.Append("{ * -> " + Status[lang] + "} ");
            }
            sb.Append(")");

            return sb.ToString();
        }

        public void UpdatePresence(PresenceStanza p)
        {
            Show = p.Show; // online, offline, away, xa, chat, dnd
            Status = p.Status; // user defined
            Priority = p.Priority;
        }

        public void SetPresence(JabberContactShow show, Hashtable status, int priority)
        {
            if ((Object)show != null)
            {
                Show = show;
                Status = status;
                Priority = priority;
                //string stanza = XMPP.IM.Presence ();
            }
        }

        internal void AddToGroup(string JabberGroup)
        {
            groups.Add(JabberGroup);
        }

        internal void RemoveFromGroup(string JabberGroup)
        {
            groups.Remove(JabberGroup);
        }

    }
}
