
using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace LibRXFFT.Libraries.Timers
{
    public class HighPerformanceCounter
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long StartTime;
        private long StopTime;
        private long Freq;

        public double TotalTime;
        public string Name;

        // Constructor
        public HighPerformanceCounter(string name)
        {
            Name = name;
            StartTime = 0;
            StopTime = 0;
            TotalTime = 0;

            if (QueryPerformanceFrequency(out Freq) == false)
            {
                // high-performance counter not supported
                throw new Win32Exception();
            }
        }

        // Start the timer
        public void Start()
        {
            QueryPerformanceCounter(out StartTime);
        }

        // Stop the timer
        public void Stop()
        {
            QueryPerformanceCounter(out StopTime);

            TotalTime += Duration;
        }

        // Update TotalTime
        public void Update()
        {
            QueryPerformanceCounter(out StopTime);

            TotalTime = Duration;
        }

        // Reset counters to zero
        public void Reset()
        {
            StartTime = 0;
            StopTime = 0;
            TotalTime = 0;
        }

        // Returns the duration of the timer (in seconds)
        public double Duration
        {
            get
            {
                return (double)(StopTime - StartTime) / (double)Freq;
            }
        }
    }
}
