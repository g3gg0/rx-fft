using System;
using System.Runtime.InteropServices;
using RX_FFT.Components.GDI;
using System.Threading;

namespace LibRXFFT.Libraries.Timers
{
    public class AccurateTimer : IDisposable
    {
        //Lib API declarations
        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeSetEvent(uint uDelay, uint uResolution, TimerCallback lpTimeProc, UIntPtr dwUser, uint fuEvent);

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeKillEvent(uint uTimerID);

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeGetTime();

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeEndPeriod(uint uPeriod);

        //Timer type definitions
        [Flags]
        public enum fuEvent : uint
        {
            TIME_ONESHOT = 0,      //Event occurs once, after uDelay milliseconds.
            TIME_PERIODIC = 1,
            TIME_CALLBACK_FUNCTION = 0x0000,  /* callback is function */
            TIME_CALLBACK_EVENT_SET = 0x0010, /* callback is event - use SetEvent */
            TIME_CALLBACK_EVENT_PULSE = 0x0020  /* callback is event - use PulseEvent */
        }

        //Delegate definition for the API callback
        delegate void TimerCallback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2);

        protected bool Disposed = false;
        protected bool Running = false;
        protected bool HandlerActive = false;
        protected uint _Interval;

        public bool Periodic = true;
        public uint Interval
        {
            get { return _Interval; }
            set
            {
                /* a default of 5ms */
                if (value > 0)
                    _Interval = value;
                else
                    _Interval = 5;

                if (Running)
                    Start();
            }
        }


        public void Dispose()
        {
            if (!Disposed)
            {
                Stop();
            }
            Disposed = true;
        }

        ~AccurateTimer()
        {
            Dispose();
        }

        /// <summary>
        /// The current timer instance ID
        /// </summary>
        uint TimerID = 0;

        /// <summary>
        /// The callback used by the the API
        /// </summary>
        TimerCallback thisCB;
        EventArgs DefaultArgs;

        /// <summary>
        /// The timer elapsed event
        /// </summary>
        public event EventHandler Timer;

        public static string LastTimerTrace = "none yet";
        public string StackTrace = "";

        protected virtual void OnTimer(EventArgs e)
        {
            LastTimerTrace = StackTrace;

            HandlerActive = true;

            if (Timer != null)
                Timer(this, e);

            HandlerActive = false;
        }

        public AccurateTimer()
        {
            StackTrace = Environment.StackTrace;
            //Initialize the API callback
            thisCB = CBFunc;
        }

        /// <summary>
        /// Stop the current timer instance (if any)
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (TimerID != 0)
                {
                    timeKillEvent(TimerID);
                    //Debug.WriteLine("MMTimer " + id.ToString() + " stopped");
                    TimerID = 0;
                    Running = false;
                }

                Join();
            }
        }

        private void Join()
        {
            while (HandlerActive)
            {
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Start a timer instance
        /// </summary>
        /// <param name="ms">Timer interval in milliseconds</param>
        /// <param name="repeat">If true sets a repetitive event, otherwise sets a one-shot</param>
        public void Start()
        {
            //Kill any existing timer
            Stop();

            //Set the timer type flags
            fuEvent f = fuEvent.TIME_CALLBACK_FUNCTION | (Periodic ? fuEvent.TIME_PERIODIC : fuEvent.TIME_ONESHOT);

            lock (this)
            {
                TimerID = timeSetEvent(Interval, 0, thisCB, UIntPtr.Zero, (uint)f);
                if (TimerID == 0)
                {
                    Log.AddMessage("timeSetEvent error");
                    //throw new Exception("timeSetEvent error");
                }
                Running = true;
                //Debug.WriteLine("MMTimer " + id.ToString() + " started");
            }
        }

        void CBFunc(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            try
            {
                //Callback from the MMTimer API that fires the Timer event. Note we are in a different thread here
                OnTimer(DefaultArgs);
            }
            catch (Exception e)
            {
                Log.AddMessage("Exception: " + e.ToString());
            }
        }
    }
}
