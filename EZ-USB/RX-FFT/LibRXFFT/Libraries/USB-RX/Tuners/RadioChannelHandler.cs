using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Tuners
{
    public class RadioChannelHandler
    {
        public long FrequencyOffset;
        public Tuner Device;

        private ArrayList Bands = new ArrayList();

        public RadioChannelHandler(Tuner device)
        {
            Device = device;
        }

        public RadioChannelHandler(Tuner device, FrequencyBand[] bands)
        {
            Device = device;
            foreach (FrequencyBand band in bands)
            {
                AddBand(band);
            }
        }

        public void AddBand(FrequencyBand band)
        {
            Bands.Add(band);
        }

        public long _Channel;
        public long Channel
        {
            get
            {
                return _Channel;
            }
            set
            {
                _Channel = value;
                if (Device != null)
                {
                    Device.SetFrequency(ChannelFrequency() + FrequencyOffset);
                }
            }
        }

        public long ChannelFrequency()
        {
            return ChannelFrequency(Channel);
        }

        public long ChannelFrequency(long channel)
        {
            if (Bands.Count == 0)
                throw new ArgumentException("No channels defined");

            foreach (FrequencyBand band in Bands)
            {
                if (band.HasChannel(channel))
                {
                    return band.Frequency(channel);
                }
            }

            throw new ArgumentException("No such channel");
        }

        public bool HasChannel(long channel)
        {
            foreach (FrequencyBand band in Bands)
            {
                if (band.HasChannel(channel))
                {
                    return true;
                }
            }

            return false;
        }

        public long LowestChannel
        {
            get
            {
                if (Bands.Count == 0)
                    throw new ArgumentException("No channels defined");

                long lowestChannel = int.MaxValue;

                foreach (FrequencyBand band in Bands)
                {
                    lowestChannel = Math.Min(lowestChannel, band.ChannelStart);
                }

                return lowestChannel;
            }
        }

        public long HighestChannel
        {
            get
            {
                if(Bands.Count==0)
                    throw new ArgumentException("No channels defined");

                long highestChannel = int.MinValue;

                foreach (FrequencyBand band in Bands)
                {
                    highestChannel = Math.Max(highestChannel, band.ChannelEnd);
                }

                return highestChannel;
            }
        }

        public long NextChannel
        {
            get
            {
                if (Bands.Count == 0)
                    throw new ArgumentException("No channels defined");

                long nextChannel = Channel + 1;

                if (!HasChannel(nextChannel))
                {
                    long lowestChannel = HighestChannel;

                    foreach (FrequencyBand band in Bands)
                    {
                        if (band.ChannelStart > nextChannel && band.ChannelStart < lowestChannel)
                        {
                            lowestChannel = band.ChannelStart;
                        }
                    }

                    if (lowestChannel < nextChannel)
                    {
                        nextChannel = LowestChannel;
                    }
                    else
                    {
                        nextChannel = lowestChannel;
                    }
                }

                return nextChannel;
            }
        }
    }
}
