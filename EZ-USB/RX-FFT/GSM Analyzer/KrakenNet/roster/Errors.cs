using JabberNET;

namespace JabberNET.Roster
{
    public class IncorrectValue : JabberException
    {
        public IncorrectValue(string Msg)
            : base("Incorrect value: " + Msg)
        {
        }
    }
}
