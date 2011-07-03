using System;
using System.Collections;
using System.Xml;

namespace JabberNET.XMPP
{
    public abstract class JabberProtocol
    {

        public static int CodeError;
        public static string MsgError;
        public static string ServerError;

        public static string Error
        {
            get
            {
                string error = null;
                if (CodeError != 0)
                    error = CodeError + ": " + MsgError + "\n(Server said: " + ServerError + ")";
                else
                    error = MsgError;
                return error;
            }
        }

        protected static void cleanErrors()
        {
            CodeError = 0;
            MsgError = null;
            ServerError = null;
        }

        protected static void errorType(XmlNode XmlError)
        {
            Hashtable errors = new Hashtable();
            errors.Add(401, "Authentication failed.");
            errors.Add(406, "Required information not provided.");
            errors.Add(409, "Resource conflict.");

            int numError = Convert.ToInt32(XmlError.Attributes["code"].InnerText);
            CodeError = numError;
            MsgError = (string)errors[numError];
            ServerError = XmlError.InnerText;
        }
    }
}
