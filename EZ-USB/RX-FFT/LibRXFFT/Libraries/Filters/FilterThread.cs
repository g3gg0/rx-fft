using System;
using System.Threading;

namespace LibRXFFT.Libraries.Filters
{
    public class FilterThread
    {
        protected Object ProcessLock = new Object();
        protected Filter Filter;
        protected Thread Thread;
        protected bool Running;
        protected bool FilterRunning;
        protected ManualResetEvent DataArrived = new ManualResetEvent(false);
        public ManualResetEvent DataProcessed = new ManualResetEvent(false);

        protected double[] InData;
        protected double[] OutData;




        public FilterThread(Filter filter)
        {
            Filter = filter;

            FilterRunning = false;
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            if (Thread != null)
                Thread.Abort();

            Thread = null;
        }

        public void Process(double[] inData, double[] outData)
        {
            FilterRunning = true;

            InData = inData;
            OutData = outData;

            /* start thread the first time */
            if (Thread == null)
            {
                Thread = new Thread(new ThreadStart(FilterThreadFunc));
                Thread.Start();
            }

            DataProcessed.Reset();
            DataArrived.Set();
        }

        public void WaitProcessed()
        {
            if (FilterRunning)
            {
                DataProcessed.WaitOne();
                DataProcessed.Reset();
            }
        }

        public bool Processed
        {
            get
            {
                return FilterRunning;
            }
        }

        private void FilterThreadFunc()
        {
            Running = true;

            while (Running)
            {
                try
                {
                    DataArrived.WaitOne();

                    Filter.Process(InData, OutData);

                    FilterRunning = false;

                    DataArrived.Reset();
                    DataProcessed.Set();
                }
                catch (ThreadAbortException e)
                {
                    Running = false;
                }
            }

            DataArrived.Reset();
            FilterRunning = false;
            DataProcessed.Set();
        }
    }
}
