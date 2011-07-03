using System.Xml;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Threading;
using System.IO;

using System;

using JabberNET.XMPP;
using JabberNET.XMPP.Stanzas;
using JabberNET.Roster;
using JabberNET.Network;

namespace JabberNET
{
    public delegate void MessageHandlerDelegate(MessageStanza message);
    public delegate void PresenceHandlerDelegate(PresenceStanza presence);
    public delegate void ConnectionStatusHandlerDelegate(bool connected);

    public class JabberClient
    {
        // Connection to the server
        ServerConnection server;

        // Server listener
        MessageListener listener;

        // User Roster
        public JabberRoster Roster;

        public readonly string Server;
        public readonly int Port;


        public string MyJID
        {
            get
            {
                return username + "@" + Server + "/" + resource;
            }
        }

        bool online;
        public bool Online
        {
            get
            {
                return online;
            }
        }

        string username;
        string password;
        string resource;
        string serverID;

        string digest
        {
            get
            {
                // digest = lowercase (hex (sha1 (serverID + Password)))
                byte[] data = Encoding.ASCII.GetBytes(serverID + password);
                byte[] result;
                SHA1 sha = new SHA1CryptoServiceProvider();
                result = sha.ComputeHash(data);
                char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
                char[] chars = new char[result.Length * 2];
                for (int i = 0; i < result.Length; i++)
                {
                    int b = result[i];
                    chars[i * 2] = hexDigits[b >> 4];
                    chars[i * 2 + 1] = hexDigits[b & 0xF];
                }
                return (new string(chars)).ToLower();
            }
        }

        // Events handlers
        MessageHandlerDelegate messageHandler = null;
        public MessageHandlerDelegate MessageHandler
        {
            set
            {
                messageHandler = value;
                listener.MessageHandler = messageHandler;
            }
        }

        PresenceHandlerDelegate presenceHandler = null;
        public PresenceHandlerDelegate PresenceHandler
        {
            set
            {
                presenceHandler = value;
                listener.PresenceHandler = presenceHandler;
            }
        }

        ConnectionStatusHandlerDelegate connectionStatusHandler = null;
        public ConnectionStatusHandlerDelegate ConnectionStatusHandler
        {
            set
            {
                connectionStatusHandler = value;
                listener.ConnectionStatusHandler = connectionStatusHandler;
            }
        }

        public JabberClient(string ServerName, int ServerPort)
        {
            Server = ServerName;
            Port = ServerPort;
            server = new ServerConnection(Server, Port);
            listener = new MessageListener(server);

            // Handlers
            PresenceHandler = new PresenceHandlerDelegate(presenceHandlerMethod);
        }

        void presenceHandlerMethod(PresenceStanza presence)
        {
            //Console.WriteLine("Presence from " + presence.From);
            JabberContact contact = Roster[presence.From];
            if (contact != null)
            {
                contact.UpdatePresence(presence);
            }
            else
            {
                //Console.WriteLine("Unknown contact " + presence.From);
            }
        }

        public string LogFile
        {
            set
            {
                server.LogFile = value;
            }
        }

        public bool Login(string UserName, string Password, string Resource)
        {
            bool loggedIn = false;

            if (server.Connect())
            {
                string stanza, response;

                username = UserName;
                password = Password;
                resource = Resource;

                stanza = XMPP.Core.StartStream(Server, null, null, null, null);
                server.Send(stanza);
                response = server.Receive();

                if (XMPP.Core.StartStreamResponse(Server, response, out serverID))
                {
                    stanza = XMPP.JEP0078.Authentication1(Server, username);
                    server.Send(stanza);
                    response = server.Receive();

                    ArrayList loginParamaters;
                    Hashtable loginParamatersFilled;
                    if (XMPP.JEP0078.Authentication1Response(response, out loginParamaters))
                    {

                        bool digestAuth = loginParamaters.Contains("digest");
                        loginParamatersFilled = new Hashtable();
                        foreach (string field in loginParamaters)
                        {
                            switch (field)
                            {
                                case "username": loginParamatersFilled.Add(field, username); break;
                                case "resource": loginParamatersFilled.Add(field, resource); break;
                                case "digest": if (digestAuth) loginParamatersFilled.Add(field, digest); break;
                                case "password": if (!digestAuth) loginParamatersFilled.Add(field, password); break;
                            }
                        }
                        stanza = XMPP.JEP0078.Authentication2(loginParamatersFilled);
                        server.Send(stanza);
                        response = server.Receive();

                        if (XMPP.JEP0078.Authentication2Response(response))
                        {
                            loggedIn = true;
                            listener.MyJID = MyJID;
                            listener.Start();
                            //Console.WriteLine("Listening thread launched.");
                            // get roster
                            Roster = new JabberRoster();
                            if (Roster.Load(XmlRoster))
                            {
                                // print roster stats
                                Roster.Print();
                            }
                            // send initial presence
                            stanza = XMPP.IM.Presence();
                            server.Send(stanza);
                        }
                        else
                        {
                            // error on authentication
                            string msgError = XMPP.JEP0078.Error;
                            // Launch error event
                            Console.WriteLine(msgError);
                        }
                    }
                    else
                    {
                        // error starting authentication
                        string msgError = XMPP.JEP0078.Error;
                        Console.WriteLine(msgError);
                    }
                }
                else
                {
                    // error opening intial stream
                    string msgError = XMPP.JEP0078.Error;
                }
            }

            online = loggedIn;

            // inform
            if (connectionStatusHandler != null)
                connectionStatusHandler(online);

            return loggedIn;
        }

        public void Logout()
        {
            string stanza = XMPP.Core.CloseStream();
            server.Send(stanza);

            listener.Stop();

            server.Disconnect();
            online = false;

            if (connectionStatusHandler != null)
                connectionStatusHandler(online);
        }

        public string XmlRoster
        {
            get
            {
                string stanza = XMPP.IM.Roster(MyJID);
                listener.StartDialog();
                try
                {
                    server.Send(stanza);
                    stanza = server.Receive();
                }
                catch (IOException ioe)
                {
                    if (connectionStatusHandler != null)
                        connectionStatusHandler(false);
                }
                listener.StopDialog();
                return stanza;
            }
        }

        public void SendMessage(string JID, string Subject, string Body)
        {
            string stanza = XMPP.IM.Message(JID, MyJID, Subject, Body);
            server.Send(stanza);
        }

        public void SendGroupChatMessage(string JID, string Subject, string Body)
        {
            string stanza = XMPP.IM.GroupChatMessage(JID, MyJID, Subject, Body);
            server.Send(stanza);
        }

        public void SendPresence(string JID, string type)
        {
            string stanza = XMPP.IM.Presence(JID, type);
            server.Send(stanza);
        }

        public void SendPresence(string JID, string MyJID, string Show, string Status, int Priority)
        {
            string stanza = XMPP.IM.Presence(JID, MyJID, Show, Status, Priority);
            server.Send(stanza);
        }
    }
}
