using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.Misc
{
    public class RateMeter
    {
        public ulong Events = 0;
        public DateTime StartTime = DateTime.Now;

        public void Add(ulong events)
        {
            lock (this)
            {
                Events += events;
            }
        }

        internal void Add(int samplesRead)
        {
            Add((ulong)samplesRead);
        }

        public void Reset()
        {
            lock(this)
            {
                Events = 0;
                StartTime = DateTime.Now;
            }
        }

        public double Rate
        {
            get
            {
                lock (this)
                {
                    double millis = (DateTime.Now - StartTime).TotalMilliseconds;

                    return Events / millis * 1000;
                }
            }
        }
    }
}
