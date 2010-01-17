namespace LibRXFFT.Libraries
{
    public enum eTriState
    {
        Yes,
        No,
        Unknown
    }

    public enum eTransferMode
    {
        Stopped,
        Stream,
        Block
    }


    public class FrequencyBand
    {
        public string Label;
        public long BaseFrequency;
        public long ChannelDistance;
        public long ChannelWidth;
        public long ChannelStart;
        public long ChannelEnd;

        public FrequencyBand()
        {
        }
    }

}
