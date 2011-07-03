using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading;
using RX_FFT.Components.GDI;

namespace JabberNET.Network
{

    public class ServerConnection
    {
        string server;
        int port;
        TcpClient tcp;
        NetworkStream ns;
        StreamWriter log;
        
        string previousMsg = null;

        public ServerConnection(string Server, int Port)
        {
            server = Server;
            port = Port;
        }

        public string LogFile
        {
            set
            {
                if (log != null)
                    log.Close();
                if (value != null)
                    log = new StreamWriter(value);
            }
        }

        public bool Connect()
        {
            bool connected = false;
            try
            {
                tcp = new TcpClient(server, port);
                ns = tcp.GetStream();
                connected = true;
            }
            catch (SocketException se)
            {
                Console.WriteLine("Could not connect to " + server + " (" + port + ")");
            }
            return connected;
        }

        public void Disconnect()
        {
            LogFile = null;
            if (ns != null)
            {
                ns.Close();
            }
            if (tcp != null)
            {
                tcp.Close();
            }
        }

        public void Send(string Msg)
        {
            if (ns != null && ns.CanWrite)
            {
                StreamWriter writer = new StreamWriter(ns);
                writer.WriteLine(Msg);
                writer.Flush();
                if (log != null)
                    log.WriteLine(Msg);
                //Log.AddMessage("SENT: " + Msg);
            }
        }

        static int readTag(string xml, int position, out string name, out int type)
        {
            name = null;
            type = 0;
            int nposition = position;
            int whitePos = 0;
            if (nposition < xml.Length && xml[nposition] == '<')
            {
                if (xml[nposition + 1] == '/')
                    type = -1;
                else
                    type = 1;
                while (nposition < xml.Length && xml[nposition] != '>')
                {
                    if (whitePos == 0 && xml[nposition] == ' ')
                        whitePos = nposition;
                    //Console.WriteLine (nposition + "\t" + xml [nposition]);
                    nposition += 1;
                }
                if (nposition != xml.Length)
                {
                    if (xml[nposition] != '>')
                        nposition = -1;
                    else if (xml[nposition - 1] == '/')
                    {
                        name = null;
                        type = 0;
                    }
                    else
                    {
                        int pos = position + 1;
                        if (type == -1)
                            pos += 1;
                        if (whitePos == 0)
                            whitePos = nposition;
                        name = xml.Substring(pos, whitePos - pos);
                    }
                }
            }
            else
            {
                while (nposition < xml.Length && xml[nposition] != '<')
                    nposition += 1;
                if (nposition != xml.Length)
                    nposition = readTag(xml, nposition, out name, out type);
                else
                    nposition = -1;
            }
            return nposition;
        }

        static int readNode(string xml, int position)
        {
            Stack stack = new Stack();
            //Console.WriteLine (xml);
            int newpos = position;
            //int length;
            do
            {
                int type;
                string name;
                newpos = readTag(xml, position, out name, out type);
                //Console.WriteLine (position + " " +newpos +" " + xml[newpos]+" "+ xml.Length + " " + type + "  " + name);
                if (newpos > position)
                {
                    if (name == "stream:stream")
                        return newpos;
                    else
                    {

                        //length = newpos - position + 1;
                        if (type != 0)
                        {
                            if (type == 1)
                                stack.Push(name);
                            else
                            {
                                //Console.WriteLine (stack.Count +" ---------> " + xml);
                                if (stack.Count > 0)
                                {
                                    if ((string)stack.Peek() == name)
                                        stack.Pop();
                                    else
                                        return -1;
                                }
                                else
                                    return -1;
                            }
                        }
                    }

                }
                else
                    return -1;

                if (stack.Count != 0)
                {
                    newpos += 1;
                    position = newpos;
                }
                //Console.Write("::" + newpos+":"+xml.Length+"::");
            } while (stack.Count != 0 && newpos < xml.Length);
            if (stack.Count != 0)
                return -1;
            //Console.WriteLine ("------------------------------------------------");
            return newpos;
        }


        public string Receive()
        {
            return Receive(0);
        }

        public string Receive(int Timeout)
        {
            string response = null;

            int position = 0;
            bool previousLoaded = false;

            if (Timeout == 0)
                Timeout = 10000;

            if (previousMsg != null)
            {
                response = previousMsg;
                position = readNode(previousMsg, 0);
                previousMsg = null;
                previousLoaded = true;
            }

            if (position <= 0)
            {
                if (ns.CanRead)
                {
                    tcp.ReceiveTimeout = Timeout;
                    int numBytes;
                    byte[] bytes = new byte[10];

                    StringBuilder sb = new StringBuilder();
                    if (previousLoaded)
                        sb.Append(response);

                    if (!tcp.Connected)
                        throw new IOException("Got Disconnected");

                    do
                    {
                        if (tcp.Client.Poll(Timeout * 1000, SelectMode.SelectRead))
                        {
                            try
                            {
                                numBytes = ns.Read(bytes, 0, bytes.Length);
                            }
                            catch (SocketException se)
                            {
                                // timeout
                                numBytes = -1;
                            }

                            if (numBytes > 0)
                            {
                                string tmp = Encoding.UTF8.GetString(bytes, 0, numBytes);
                                sb.Append(tmp);

                                bool specialChars = false;
                                if (tmp.Length < numBytes && numBytes == bytes.Length)
                                {
                                    // special chars
                                    specialChars = true;
                                }

                                if (bytes[numBytes - 1] == '>' || specialChars)
                                {
                                    response = sb.ToString();
                                    position = readNode(response, 0);
                                }
                                else
                                    position = -1;
                            }
                        }
                        else
                            break;

                    } while (position < 0);
                }

            }

            if (position > 0)
            {

                if (position != response.Length - 1)
                {
                    //Console.WriteLine ("--- More than one stanza in msg.");
                    //Console.WriteLine ("--> " + response);
                    previousMsg = response.Substring(position + 1);
                    response = response.Substring(0, position + 1);
                }
            }
            
            if (response != null)
            {
                //Log.AddMessage("RECEIVED: " + response + " " + response.Length + "-" + position);
            }
            
            return response;
        }
    }
}

