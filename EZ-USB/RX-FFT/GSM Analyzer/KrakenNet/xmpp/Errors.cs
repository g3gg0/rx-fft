using System;

namespace JabberNET.XMPP.Stanzas
{

    public class IncorrectNumberOfNodes : JabberException
    {
        public IncorrectNumberOfNodes(string Msg)
            : base("Incorrect number of nodes: " + Msg)
        {
        }
    }

    public class IncorrectNameOfNode : JabberException
    {
        public IncorrectNameOfNode(string Msg)
            : base("Incorrect name of node: " + Msg)
        {
        }
    }

    public class IncorrectTypeOfNode : JabberException
    {
        public IncorrectTypeOfNode(string Msg)
            : base("Incorrect type of node: " + Msg)
        {
        }
    }

    public class IncorrectNumberOfAttributes : JabberException
    {
        public IncorrectNumberOfAttributes(string Msg)
            : base("Incorrect number of attribute: " + Msg)
        {
        }
    }

    public class IncorrectTypeOfAttribute : JabberException
    {
        public IncorrectTypeOfAttribute(string Msg)
            : base("Incorrect type of attribute: " + Msg)
        {
        }
    }

}
