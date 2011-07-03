using System;
using System.Text;
using System.Xml;
using System.Collections;

using JabberNET.XMPP.Stanzas;

namespace JabberNET.XMPP
{

    /********************************************************************************************************
     * 
     *  JEP-0078: Non-SASL Authentication
     *
     *  A protocol for authentication with Jabber servers and services using the 'jabber:iq:auth' namespace
     *
     *  -----
     *
     *  Author: Daniel Pecos
     *  Version: 18.01.2004
     *
     ********************************************************************************************************/

    public class JEP0078 : JabberProtocol
    {
        public static string Authentication1(string Server, string UserName)
        {
            XmlDocument xml = XMPP.Core.IQ("get", "auth1", Server, null);
            XmlNode iq = xml.FirstChild;

            XmlElement query = xml.CreateElement("query");
            query.SetAttribute("xmlns", "jabber:iq:auth");
            iq.AppendChild(query);

            // NOTE: mono bug? -> <username xmlns="">
            XmlElement username = xml.CreateElement("username");
            username.InnerXml = UserName;
            query.AppendChild(username);
            //query.InnerXml = "<username>" + UserName + "</username>";

            return xml.InnerXml;
        }

        public static string Authentication2(Hashtable parameters)
        {
            // Plain or Digest authentication
            XmlDocument xml = XMPP.Core.IQ("set", "auth2", null, null);
            XmlNode iq = xml.FirstChild;

            XmlElement query = xml.CreateElement("query");
            query.SetAttribute("xmlns", "jabber:iq:auth");
            iq.AppendChild(query);

            bool digest = parameters.ContainsKey("digest");

            //StringBuilder sb = new StringBuilder ();
            foreach (string key in parameters.Keys)
            {
                if ((key == "digest" && !digest) || (key == "password" && digest))
                    continue;
                //sb.Append ("<" + key + ">" + parameters [key] + "</" + key + ">");
                XmlElement node = xml.CreateElement(key);
                node.InnerText = (string)parameters[key];
                query.AppendChild(node);
            }
            //query.InnerXml = sb.ToString ();
            return xml.InnerXml;
        }


        /* ---------------------------------------------------------------------------------------------*/


        static bool query(XmlNode node, out ArrayList fields)
        {

            cleanErrors();

            bool result = true;

            fields = null;

            if (node.Name == "query")
            {
                fields = new ArrayList();
                if (node.Attributes["xmlns"].InnerText == "jabber:iq:auth")
                {
                    foreach (XmlNode child in node.ChildNodes)
                        fields.Add(child.Name);
                }
                else
                {
                    MsgError = "Wrong \"xmlns\" attribute received from server.";
                    result = false;
                }
            }
            else
            {
                MsgError = "Incorrect XML response received from server.";
                result = false;
            }

            return result;
        }


        public static bool Authentication1Response(string Response, out ArrayList LoginParamaters)
        {
            cleanErrors();
            bool result = true;

            ArrayList loginParamaters = null;

            IqStanza iq = null;
            try
            {
                iq = new IqStanza(Response);
            }
            catch (JabberException je)
            {
                iq = null;
                MsgError = je.Message + "\nError loading XML received from server.";
                result = false;
            }
            if (result)
            {
                if (iq != null)
                {
                    if (iq.Type == "result")
                    {
                        if (iq.Id == "auth1")
                        {
                            if (!query(iq.ChildNode, out loginParamaters))
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            MsgError = "Wrong \"id\" attribute received from server.";
                            result = false;
                        }
                    }
                    else
                    {
                        // type of error
                        /*XmlNode xmlError = xml.FirstChild.LastChild;
                        if (xmlError != null && xmlError.Name == "error") {
                           errorType (xmlError);
                        } else {
                           MsgError = "Unknown error in authentication process.";
                        }*/
                        result = false;
                    }
                }
                else
                {
                    MsgError = "Incorrect XML response received from server.";
                    result = false;
                }
            }

            if (result)
                LoginParamaters = loginParamaters;
            else
                LoginParamaters = null;

            return result;
        }

        public static bool Authentication2Response(string Response)
        {
            cleanErrors();
            bool result = true;

            XmlDocument xml = new XmlDocument();
            IqStanza iq = null;
            try
            {
                iq = new IqStanza(Response);
            }
            catch (JabberException je)
            {
                iq = null;
                MsgError = je.Message + "\nError loading XML received from server.";
                result = false;
            }
            if (result)
            {
                if (iq != null)
                {
                    if (iq.Id == "auth2")
                    {
                        if (iq.Type != "result")
                        {
                            result = false;
                            // type of error
                            /*XmlNode xmlError = xml.FirstChild.LastChild;
                            Console.WriteLine(xmlError.Name);
                            if (xmlError != null && xmlError.Name == "error") {
                           errorType (xmlError);
                            } else {
                           MsgError = "Unknown error in authentication process.";
                            }*/
                        }
                    }
                    else
                    {
                        MsgError = "Wrong \"id\" attribute received from server.";
                        result = false;
                    }
                }
                else
                {
                    MsgError = "Incorrect XML response received from server.";
                    result = false;
                }

            }
            return result;
        }

    }

}
