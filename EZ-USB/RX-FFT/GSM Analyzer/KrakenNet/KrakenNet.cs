

using GSM_Analyzer;
using JabberNET;
using RX_FFT.Components.GDI;
using JabberNET.XMPP.Stanzas;
using System.Threading;
using System;
using JabberNET.XMPP;
using System.Collections.Generic;


public class KrakenNet : KrakenClient
{
    public class NodeStatus
    {
        public string Name;
        public string Status;
        public string Owner;
        public double Load;
        public double Increment;
        public DateTime LastSeen;
        public DateTime FirstSeen;

        public bool Valid;
    }

    private Dictionary<string, NodeStatus> KrakenNodes = new Dictionary<string, NodeStatus>();

    private string Jid = "";
    private string Password = "";
    private string Conference = "";
    private string Nickname = "";
    private string ConferenceJid = "";

    private string Resource = "GSMAnalyzer";
    private string Server = "jabber.org";
    private int ServerPort = 5222;

    private JabberClient Client = null;

    private Thread ConnectionThread = null;
    private Thread DiscoverThread = null;
    private DateTime LastDiscovery = DateTime.MinValue;
    private int DiscoverSleepTime = 1;
    private int DiscoveryRequestTime = 30;

    private bool InKrakenNet = false;


    public KrakenNet(string jid, string pass, string server, string conference, string nick)
        : this()
    {
        Jid = jid;
        Password = pass;
        Conference = conference;
        Nickname = nick;
        ConferenceJid = Conference + "/" + Nickname;

        string[] hostInfo = server.Split(':');
        int port = 5222;

        if (hostInfo.Length == 2)
        {
            int.TryParse(hostInfo[1], out port);
        }

        ServerPort = port;
        Server = hostInfo[0];
    }

    public KrakenNet(string configString)
        : this()
    {
        Hostname = configString;

        string[] config = configString.Split('|');


        if (config.Length > 5 && config[0] == "XMPP")
        {
            Jid = config[1];
            Password = config[2];
            Conference = config[4];
            Nickname = config[5];

            string[] hostInfo = config[3].Split(':');
            int port = 5222;

            if (hostInfo.Length == 2)
            {
                int.TryParse(hostInfo[1], out port);
            }

            ServerPort = port;
            Server = hostInfo[0];

            ConferenceJid = Conference + "/" + Nickname;
        }
    }

    public KrakenNet()
    {
        DiscoverThread = new Thread(Discover);
        DiscoverThread.Start();
    }

    private void Discover()
    {
        while (true)
        {
            if (InKrakenNet && ((DateTime.Now - LastDiscovery).TotalSeconds > DiscoveryRequestTime))
            {
                LastDiscovery = DateTime.Now;
                Client.SendGroupChatMessage(Conference, null, "#discover");
            }

            Thread.Sleep(DiscoverSleepTime);
        }
    }

    #region Handlers

    public void JabberMessageHandler(MessageStanza message)
    {
        if (message.Body != null)
        {
            if (message.Type == "groupchat")
            {
                foreach (String s in message.Body.Values)
                {
                    if (s.StartsWith("##"))
                    {
                        ParseKrakenDiscovery(message.FromResource, s);
                    }
                    if ("#discover".Equals(s))
                    {
                        /* there was a discovery, cache its date */
                        LastDiscovery = DateTime.Now;
                    }
                }
            }
            else if (message.Type == "chat")
            {
                foreach (string s in message.Body.Values)
                {
                    Log.AddMessage("Jabber", "From: <" + message.From + "> " + s);

                    Client.SendMessage(message.From, "Question", "Who are you?");
                }
            }
        }
    }

    private void ValidateNodes()
    {
        lock (KrakenNodes)
        {
            foreach (NodeStatus node in KrakenNodes.Values)
            {
                if ((DateTime.Now - node.LastSeen).TotalSeconds > 600)
                {
                    node.Valid = false;
                }
                else
                {
                    node.Valid = true;
                }
            }
        }
    }

    private void ParseKrakenDiscovery(string from, string msg)
    {
        string[] fields = msg.Replace("##", "").Split('|');
        NodeStatus node = new NodeStatus();

        node.FirstSeen = DateTime.Now;
        node.LastSeen = DateTime.Now;
        node.Name = from;
        node.Valid = false;

        foreach (string rawField in fields)
        {
            if (rawField.Contains(":"))
            {
                string field = rawField.Trim();
                string key = field.Substring(0, field.IndexOf(':')).Trim();
                string value = field.Substring(field.IndexOf(':') + 1).Trim();

                switch (key)
                {
                    case "S":
                        node.Status = value;
                        break;
                    case "O":
                        node.Owner = value;
                        break;
                    case "L":
                        double.TryParse(value, out node.Load);
                        break;
                    case "I":
                        double.TryParse(value, out node.Increment);
                        break;
                }
            }
        }

        lock (KrakenNodes)
        {
            NodeStatus listNode;
            if (KrakenNodes.ContainsKey(node.Name))
            {
                listNode = KrakenNodes[node.Name];
            }
            else
            {
                KrakenNodes.Add(node.Name, node);
                listNode = node;
            }

            listNode.Status = node.Status;
            listNode.Owner = node.Owner;
            listNode.Load = node.Load;
            listNode.Increment = node.Increment;
            listNode.LastSeen = node.LastSeen;
        }
    }

    private void JoinConference(int timeout)
    {
        Thread t = new Thread(() =>
        {
            Thread.Sleep(timeout);
            JoinConference();
        });
        t.Start();
    }

    private void JoinConference()
    {
        Client.SendPresence(ConferenceJid, Jid + "/" + Resource, null, null, 0);
    }

    public void JabberPresenceHandler(PresenceStanza message)
    {
        if (message.From == (Jid + "@" + Server))
        {
            switch (message.Type)
            {
                case "available":
                    /* join conference */
                    Log.AddMessage("Jabber", " [i] Join KrakenNet... ");
                    JoinConference();
                    break;
                case "unavailable":
                case "error":
                    InKrakenNet = false;
                    Log.AddMessage("Jabber", " [E] Not in KrakenNet, retrying...");
                    Client.SendPresence(Jid, "available");
                    break;
                default:
                    Log.AddMessage("Jabber", " [E] Unhandled XMPP presence: ");
                    Log.AddMessage("Jabber", "     Presence: " + message.From + " is " + message.Type);
                    break;
            }
        }

        if (message.From == Conference)
        {
            if (message.FromResource == ConferenceJid)
            {
                switch (message.Type)
                {
                    case "available":
                        if (!InKrakenNet)
                        {
                            Log.AddMessage("Jabber", " [i] We are in KrakenNet now");
                        } 
                        InKrakenNet = true;
                        break;
                    case "unavailable":
                    case "error":
                        InKrakenNet = false;
                        Log.AddMessage("Jabber", " [E] Not in KrakenNet channel anymore, retrying...");
                        JoinConference(5000);
                        break;
                    default:
                        Log.AddMessage("Jabber", " [E] Unhandled XMPP presence: ");
                        Log.AddMessage("Jabber", "     Presence: " + message.From + " is " + message.Type);
                        break;
                }
            }
            else
            {
                string name = message.FromResource.Split('/')[1];

                switch (message.Type)
                {
                    case "available":
                        Log.AddMessage("Jabber", " [i] KrakenNet: " + name + " joined");
                        break;
                    case "unavailable":
                    case "error":
                        Log.AddMessage("Jabber", " [i] KrakenNet: " + name + " left");
                        break;
                    default:
                        Log.AddMessage("Jabber", " [E] Unhandled XMPP presence: ");
                        Log.AddMessage("Jabber", "     Presence: " + message.From + " is " + message.Type);
                        break;
                }
            }
        }

        if (message.Type == "subscribe")
        {
            Log.AddMessage("Jabber", "Presence: Accepted " + message.From);
            Client.SendPresence(message.From, "subscribed");
        }
    }

    public void JabberConnectionStatusHandler(bool status)
    {
        try
        {
            if (!status || Client == null)
            {
                Log.AddMessage("Jabber", " [i] Disconnected");
                InKrakenNet = false;

                //Thread.Sleep(5000);
                //Log.AddMessage("Jabber", "Reconnecting...");
                //Connect();
            }
            else
            {
                Log.AddMessage("Jabber", " [i] JID " + Client.MyJID);
            }
        }
        catch
        { }

    }

    public override double GetJobProgress()
    {
        return GetJobProgress(RequestId);
    }

    public override double GetJobProgress(int jobId)
    {
        return 0;
    }

    public override byte[] RequestResult(bool[] key1, uint count1, bool[] key2, uint count2)
    {
        return null;
    }

    public override bool Connected
    {
        get
        {
            return true;
        }
    }

    public override void Disconnect()
    {
        if (DiscoverThread != null)
        {
            DiscoverThread.Abort();
            DiscoverThread = null;
        }

        try
        {
            if (Client != null)
                Client.Logout();
        }
        catch { }

        Client = null;

        if (ConnectionThread != null)
        {
            ConnectionThread.Abort();
            ConnectionThread = null;
        }
        InKrakenNet = false;
    }

    public override bool Connect()
    {
        ConnectionThread = new Thread(() =>
        {
            try
            {
                if (Client != null)
                    Client.Logout();
            }
            catch (Exception ex)
            {
            }

            try
            {
                Log.AddMessage("Jabber", " [i] Connecting...");
                Client = new JabberClient(Server, ServerPort);
                Client.MessageHandler = new MessageHandlerDelegate(JabberMessageHandler);
                Client.PresenceHandler = new PresenceHandlerDelegate(JabberPresenceHandler);
                Client.ConnectionStatusHandler = new ConnectionStatusHandlerDelegate(JabberConnectionStatusHandler);

                if (!Client.Login(Jid, Password, Resource))
                {
                    Log.AddMessage("Jabber", " [E] Failed to log in. Wrong username/password?");
                }
            }
            catch (Exception ex)
            {
                Log.AddMessage("Jabber", "Exception: " + ex.ToString());
            }
        });

        ConnectionThread.Start();
        return true;
    }
    #endregion


}