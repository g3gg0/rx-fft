

using GSM_Analyzer;
using JabberNET;
using RX_FFT.Components.GDI;
using JabberNET.XMPP.Stanzas;
using System.Threading;
using System;
using JabberNET.XMPP;
using System.Collections.Generic;
using LibRXFFT.Libraries;


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

        public bool Broadcasting;
        public bool Unusable;
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

    private Dictionary<string, KrakenNetConnection> Connections = new Dictionary<string, KrakenNetConnection>();


    public KrakenNet(string jid, string pass, string server, string conference, string nick)
        : this()
    {
        LoadCache();

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

    public override int ParallelRequests
    {
        get { return NodesFound; }
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
            switch (message.Type)
            {
                case "groupchat":
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
                    break;

                case "normal":
                case "chat":
                    foreach (string s in message.Body.Values)
                    {
                        //Log.AddMessage("Jabber", "From: <" + message.FromResource + "> " + s);
                        lock (Connections)
                        {
                            if (Connections.ContainsKey(message.FromResource))
                            {
                                Connections[message.FromResource].HandleMessage(s);
                            }
                        }
                    }
                    break;
            }
        }
    }

    public int NodesFound
    {
        get
        {
            int found = 2;

            lock (KrakenNodes)
            {
                foreach (NodeStatus node in KrakenNodes.Values)
                {
                    if (!node.Unusable && node.Broadcasting)
                    {
                        found++;
                    }
                }
            }

            return found;
        }
    }

    private NodeStatus GetBestNode()
    {
        NodeStatus best = null;

        lock (KrakenNodes)
        {
            foreach (NodeStatus node in KrakenNodes.Values)
            {
                if (node.Broadcasting && !node.Unusable && (best == null || (node.Load + node.Increment) < (best.Load + best.Increment)))
                {
                    best = node;
                }
            }
        }

        return best;
    }

    private void ValidateNodes()
    {
        lock (KrakenNodes)
        {
            foreach (NodeStatus node in KrakenNodes.Values)
            {
                if ((DateTime.Now - node.LastSeen).TotalSeconds > 600)
                {
                    node.Broadcasting = false;
                }
                else
                {
                    node.Broadcasting = true;
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
        node.Broadcasting = true;
        node.Unusable = false;

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
            }
            else
            {
                Log.AddMessage("Jabber", " [i] JID " + Client.MyJID);
            }
        }
        catch
        { 
        }
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
        /* do we have data for this request already in cache? */
        string request = "crack " + ByteUtil.BitsToString(key1) + " " + count1 + " " + ByteUtil.BitsToString(key2) + " " + count2;
        byte[] result = new byte[8];

        if (false && CheckScanResult(request, ref result))
        {
            return result;
        }

        /* no key cached, try to find a node that cracks the key */
        NodeStatus node = GetBestNode();
        if (node == null)
        {
            Log.AddMessage("KrakenNet", "No usable node found");
            return null;
        }

        Log.AddMessage("KrakenNet", "Will choose " + node.Name + " for cracking");

        /* make sure we enter this only once */
        lock (this)
        {
            KrakenNetConnection conn = null;

            lock (Connections)
            {
                /* already connected? */
                if (!Connections.ContainsKey(node.Name))
                {
                    conn = new KrakenNetConnection(Client, node.Name);
                    Connections.Add(node.Name, conn);
                }
            }

            /* newly added? */
            if (conn != null)
            {
                /* failed to communicate with node? */
                if (!conn.Available)
                {
                    /* mark as unusable, remove and recurse */
                    node.Unusable = true;
                    conn.CloseConnection();
                    Connections.Remove(node.Name);

                    /* repeat until working node found or no more nodes */
                    return RequestResult(key1, count1, key2, count2);
                }
            }

            /* is this connection already closed */
            if (Connections[node.Name].State != KrakenNetConnection.eConnState.Ready)
            {
                node.Unusable = true;
                Connections.Remove(node.Name);

                /* repeat until working node found or no more nodes */
                return RequestResult(key1, count1, key2, count2);
            }
        }

        try
        {
            node.Load += node.Increment;
            return Connections[node.Name].RequestResult(key1, count1, key2, count2);
        }
        catch (ThreadAbortException e)
        {
            Connections[node.Name].CancelRequests();
            throw e;
        }
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