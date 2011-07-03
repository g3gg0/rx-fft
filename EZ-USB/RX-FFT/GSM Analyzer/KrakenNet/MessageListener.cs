using System;
using System.IO;
using System.Threading;
using System.Xml;


using JabberNET.Network;
using JabberNET.XMPP.Stanzas;

namespace JabberNET
{
    public class MessageListener
    {
        Thread listenerThread;
        public PresenceHandlerDelegate PresenceHandler;
        public MessageHandlerDelegate MessageHandler;
        public ConnectionStatusHandlerDelegate ConnectionStatusHandler;

        ServerConnection connection;
        bool dialogEnabled;
        bool threadWaiting;
        string jid;

        public MessageListener(ServerConnection Connection)
        {
            connection = Connection;
            dialogEnabled = false;
        }

        public string MyJID
        {
            get { return jid; }
            set { jid = value; }
        }


        public void StartDialog()
        {
            lock ((Object)dialogEnabled)
            {
                if (!dialogEnabled)
                {
                    dialogEnabled = true;
                }
            }
            // wait for listening thread to pause
            while (!threadWaiting)
            {
                Thread.Sleep(200);
            }
        }

        public void StopDialog()
        {
            lock ((Object)dialogEnabled)
            {
                dialogEnabled = false;
            }
            // wait for listening thread to continue
            while (threadWaiting)
            {
                Thread.Sleep(200);
            }
        }

        public bool DialogEnabled
        {
            get
            {
                bool result;
                lock ((Object)dialogEnabled)
                {
                    result = dialogEnabled;
                }
                return result;
            }
        }

        string Ping()
        {
            string stanza = XMPP.IM.Ping(MyJID);
            try
            {
                connection.Send(stanza);
                stanza = connection.Receive(2000);
            }
            catch (IOException ioe)
            {
                if (ConnectionStatusHandler != null)
                    ConnectionStatusHandler(false);
            }

            if (stanza == null)
            {
                if (ConnectionStatusHandler != null)
                    ConnectionStatusHandler(false);
            }
            return stanza;
        }

        void mainLoop()
        {
            DateTime time = DateTime.Now;

            while (true)
            {
                string msgReceived = null;
                
                if (DateTime.Now.Subtract(time).TotalSeconds > 60)
                {
                    Ping();
                    time = DateTime.Now;
                }                

                try
                {
                    msgReceived = connection.Receive(200);
                }
                catch (IOException ioe)
                {
                    connection.Disconnect();
                    if (ConnectionStatusHandler != null)
                        ConnectionStatusHandler(false);
                    return;
                }

                if (msgReceived == "</stream:stream>")
                {
                    //Console.WriteLine("TODO: Close connection and thread!");
                    connection.Disconnect();
                    if (ConnectionStatusHandler != null)
                        ConnectionStatusHandler(false);
                    return;
                }
                else if (msgReceived != null)
                {
                    // process message
                    XmlDocument xdoc = new XmlDocument();
                    try
                    {
                        xdoc.LoadXml(msgReceived);

                        if (xdoc != null)
                        {
                            switch (xdoc.FirstChild.Name)
                            {
                                case "message":
                                    {
                                        if (MessageHandler != null)
                                        {
                                            MessageStanza message = new MessageStanza(msgReceived);
                                            if (message != null)
                                                MessageHandler(message);
                                        }
                                        break;
                                    }
                                case "presence":
                                    {
                                        if (PresenceHandler != null)
                                        {
                                            PresenceStanza presence = new PresenceStanza(msgReceived);
                                            if (presence != null)
                                                PresenceHandler(presence);
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        MessageStanza message = new MessageStanza();
                                        message.From = "MessageListener";
                                        message.Body = new System.Collections.Hashtable();
                                        message.Body.Add("","Unknown message: \"" + msgReceived + "\"");
                                        if (message != null)
                                            MessageHandler(message);
                                        break;
                                    }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error loading message: \"" + msgReceived + "\"");
                    }
                    //Console.WriteLine (msgReceived);
                }

                if (DialogEnabled)
                {

                    lock ((Object)threadWaiting)
                    {
                        threadWaiting = true;
                    }
                    while (DialogEnabled)
                    {
                        Thread.Sleep(200);
                    }
                    lock ((Object)threadWaiting)
                    {
                        threadWaiting = false;
                    }
                }
            }
        }

        public void Start()
        {
            Stop();
            ThreadStart entryPoint = new ThreadStart(mainLoop);
            listenerThread = new Thread(entryPoint);
            listenerThread.SetApartmentState(ApartmentState.STA);
            listenerThread.Start();
        }

        public void Stop()
        {
            if (listenerThread != null)
            {
                listenerThread.Abort();
                listenerThread.Join();
            }
        }
    }
}


