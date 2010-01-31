using System;

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

        public FrequencyBand(string label, long freq, long dist, long start, long end)
        {
            Label = label;
            BaseFrequency = freq;
            ChannelDistance = dist;
            ChannelStart = start;
            ChannelEnd = end;
        }

        public bool HasChannel(long channel)
        {
            return channel >= ChannelStart && channel <= ChannelEnd;
        }

        public long Frequency(long channel)
        {
            if (!HasChannel(channel))
                throw new ArgumentException("No such channel");

            return BaseFrequency + (channel - ChannelStart) * ChannelDistance;
        }
    }

}
