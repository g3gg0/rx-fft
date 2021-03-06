﻿

using GSM_Analyzer;
using JabberNET;
using RX_FFT.Components.GDI;
using JabberNET.XMPP.Stanzas;
using System.Threading;
using System;
using JabberNET.XMPP;
using System.Collections.Generic;
using LibRXFFT.Libraries;
using System.Xml;


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
        XmlNodeList delayedNode = message.XmlDoc.GetElementsByTagName("delay");
        bool delayed = (delayedNode != null) && (delayedNode.Count != 0);

        if (message.Body != null && !delayed)
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
            int found = 0;

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
        int tries = 0;

        /* using a goto label to restart instead of recurse */
    restart:
        tries++;
        if (tries >= 10)
        {
            Log.AddMessage("KrakenNet", "RequestResult(): Tried to request crack " + tries + " times now... Abort!");
            return null;
        }

        /* do we have data for this request already in cache? */
        string request = "crack " + ByteUtil.BitsToString(key1) + " " + count1 + " " + ByteUtil.BitsToString(key2) + " " + count2;
        byte[] result = new byte[8];

        if (CheckScanResult(request, ref result))
        {
            return result;
        }

        /* wait until we have a connection */
        if (!InKrakenNet)
        {
            if (Jid != "")
            {
                Log.AddMessage("KrakenNet", "RequestResult(): Not connected. Waiting until reconnected.");

                while (!InKrakenNet)
                {
                    Thread.Sleep(100);
                }
            }
            else
            {
                Log.AddMessage("KrakenNet", "RequestResult(): Not Configured. Quitting.");
                return null;
            }
        }

        /* no key cached, try to find a node that cracks the key */
        NodeStatus node = GetBestNode();
        if (node == null)
        {
            Log.AddMessage("KrakenNet", "RequestResult(): Waiting for (usable) Kraken hosts to appear... " + KrakenNodes.Count + " found, but none in a usable state.");
            while (node == null)
            {
                node = GetBestNode();
                Thread.Sleep(100);

                /* restart if there are network problems */
                if (!InKrakenNet)
                {
                    goto restart;
                }
            }
        }

        /* that should not be entered */
        if (node == null)
        {
            Log.AddMessage("KrakenNet", "RequestResult(): No usable node found");
            return null;
        }

        Log.AddMessage("KrakenNet", "RequestResult(): Will choose " + node.Name + " for cracking.");

        /* make sure we enter this only once and lock from changes done in "Disconnect" */
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
                    goto restart;
                }
            }

            /* is this connection already closed */
            if (Connections[node.Name].State != KrakenNetConnection.eConnState.Ready)
            {
                node.Unusable = true;
                Connections.Remove(node.Name);

                /* repeat until working node found or no more nodes */
                goto restart;
            }

            /* increase internal sotred node load to decrease node priority */
            node.Load += node.Increment;
        }

        try
        {
            result = Connections[node.Name].RequestResult(key1, count1, key2, count2);
            AddScanResult(request, result);

            return result;
        }
        catch (ThreadAbortException e)
        {
            Connections[node.Name].CancelRequest();
            throw e;
        }
        catch (Exception e)
        {
            Log.AddMessage("KrakenNet", "RequestResult(): Caught '" + e.GetType().ToString() + "'. Retry.");
            goto restart;
        }
    }

    public override bool Connected
    {
        get
        {
            return (ConnectionThread != null);
        }
    }

    public override void Disconnect()
    {
        /* 'this' is locked here and in RequestResult */
        lock (this)
        {
            /* shut down discover thread */
            if (DiscoverThread != null)
            {
                DiscoverThread.Abort();
            }

            /* close connection thread */
            if (ConnectionThread != null)
            {
                ConnectionThread.Abort();
            }
            InKrakenNet = false;

            /* close all open connection */
            foreach (KrakenNetConnection conn in Connections.Values)
            {
                conn.CloseConnection();
            }
            Connections.Clear();

            /* clear all found kraken hosts */
            KrakenNodes.Clear();

            /* close XMPP connection */
            try
            {
                if (Client != null)
                {
                    Client.Logout();
                }
            }
            catch { }

            /* null these as last step */
            Client = null;
            DiscoverThread = null;
            ConnectionThread = null;
        }
    }

    public override bool Connect()
    {
        if (Connected)
        {
            return true;
        }

        ConnectionThread = new Thread(() =>
        {
            if (Connected)
            {
                try
                {
                    if (Client != null)
                    {
                        Log.AddMessage("Jabber", " [W] Closing unclosed XMPP connection. Should not happen");
                        Client.Logout();
                    }
                }
                catch (Exception ex)
                {
                }
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
                    Disconnect();
                    return;
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