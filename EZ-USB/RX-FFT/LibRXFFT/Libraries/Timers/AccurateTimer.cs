using System;
using System.Runtime.InteropServices;
using RX_FFT.Components.GDI;
using System.Threading;
using System.Timers;

namespace LibRXFFT.Libraries.Timers
{
    public class AccurateTimer : IDisposable
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenEvent(uint dwDesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ResetEvent(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetEvent(IntPtr handle);

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        public static extern Int32 WaitForSingleObject(IntPtr handle, Int32 milliseconds);

        // Lib API declarations
        [DllImport("Winmm.dll", CharSet = CharSet.Auto, EntryPoint = "timeSetEvent")]
        static extern uint timeSetEvent(uint uDelay, uint uResolution, TimerCallback lpTimeProc, UIntPtr dwUser, uint fuEvent);

        [DllImport("Winmm.dll", CharSet = CharSet.Auto, EntryPoint = "timeSetEvent")]
        static extern uint timeSetEvent(uint uDelay, uint uResolution, IntPtr lpTimeProc, UIntPtr dwUser, uint fuEvent);

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
                {
                    Start();
                }
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
        private uint TimerID = 0;

        /// <summary>
        /// The callback used by the the API
        /// </summary>
        private TimerCallback SystemCallback;
        private System.Timers.Timer RecoveryTimer;
        private Thread TimerThread;
        private object TimerThreadEvent = new object();
        private bool Pulsed = false;

        /// <summary>
        /// The timer elapsed event
        /// </summary>
        public event EventHandler Timer;
        public string StackTrace = "";
        public string Name = "Unnamed Timer";
        private int Timeouts = 0;
        private int MaxTimeouts = 5;


        public AccurateTimer()
        {
            StackTrace = Environment.StackTrace;
        }

        public AccurateTimer(EventHandler callback)
        {
            Timer += callback;
            StackTrace = Environment.StackTrace;
        }

        /// <summary>
        /// Start a timer instance
        /// </summary>
        /// <param name="ms">Timer interval in milliseconds</param>
        /// <param name="repeat">If true sets a repetitive event, otherwise sets a one-shot</param>
        public void Start()
        {
            fuEvent f;

            //Kill any existing timer
            Stop();
            StackTrace = Environment.StackTrace;

            //Set the timer type flags
            f = fuEvent.TIME_CALLBACK_FUNCTION | (Periodic ? fuEvent.TIME_PERIODIC : fuEvent.TIME_ONESHOT);

            lock (this)
            {
                SystemCallback = CBFunc;
                TimerID = timeSetEvent(Interval, 0, SystemCallback, UIntPtr.Zero, (uint)f);
                if (TimerID == 0)
                {
                    Log.AddMessage("timeSetEvent error");
                    //throw new Exception("timeSetEvent error");
                }
                Running = true;
                //Debug.WriteLine("MMTimer " + id.ToString() + " started");

                TimerThread = new Thread(TimerThreadMain);
                TimerThread.Name = "AccurateTimer Thread - Interval: " + Interval;
                TimerThread.Start();
            }

            if (RecoveryTimer == null)
            {
                RecoveryTimer = new System.Timers.Timer(Interval * 2);
                RecoveryTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
                {
                    try
                    {
                        Timeouts++;

                        if (Timeouts > MaxTimeouts)
                        {
                            Timeouts = 0;
                            Log.AddMessage("Recovering Timer");

                            lock (this)
                            {
                                timeKillEvent(TimerID);

                                TimerID = timeSetEvent(Interval, 0, SystemCallback, UIntPtr.Zero, (uint)f);
                                if (TimerID == 0)
                                {
                                    Log.AddMessage("timeSetEvent error");
                                    //throw new Exception("timeSetEvent error");
                                }
                                Running = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                };
            }
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
                    if (RecoveryTimer != null)
                    {
                        RecoveryTimer.Stop();
                        RecoveryTimer = null;
                    }

                    StopInternal(true);
                }
                Join();
            }
        }

        /// <summary>
        /// Stop the current timer instance (if any)
        /// </summary>
        public void SoftStop()
        {
            lock (this)
            {
                if (TimerID != 0)
                {
                    if (RecoveryTimer != null)
                    {
                        RecoveryTimer.Stop();
                        RecoveryTimer = null;
                    }

                    StopInternal(false);
                }
                Join();
            }
        }

        private void StopInternal(bool killThread)
        {
            if (TimerID != 0)
            {
                timeKillEvent(TimerID);
                //Debug.WriteLine("MMTimer " + id.ToString() + " stopped");
                TimerID = 0;
                Running = false;
                //TriggerThread();
            }

            if (killThread && TimerThread != null)
            {
                if (!TimerThread.Join(100))
                {
                    TimerThread.Abort();
                }
                TimerThread = null;
            }
        }

        private void Join()
        {
            int calls = 10;

            while (HandlerActive && calls-- > 0)
            {
                Thread.Sleep(50);
            }
        }

        private void TriggerThread()
        {
            lock (TimerThreadEvent)
            {
                Pulsed = true;
                Monitor.Pulse(TimerThreadEvent);
            }
        }

        protected virtual void OnTimer(EventArgs e)
        {
            if (Timer != null)
                Timer(this, e);
        }

        private void TimerThreadMain()
        {
            try
            {
                while (Running)
                {
                    lock (TimerThreadEvent)
                    {
                        while (!Pulsed)
                        {
                            Monitor.Wait(TimerThreadEvent, 100);
                            if (!Running)
                            {
                                return;
                            }
                        }
                        Pulsed = false;
                    }

                    HandlerActive = true;
                    try
                    {
                        OnTimer(null);
                    }
                    catch (Exception e)
                    {
                        Log.AddMessage("Exception: " + e.ToString());
                    }
                    HandlerActive = false;
                }
            }
            catch (ThreadAbortException e)
            {
            }
            catch (Exception e)
            {
                Log.AddMessage("Exception: " + e.ToString());
            }

            HandlerActive = false;
        }

        void CBFunc(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            Timeouts = 0;
            TriggerThread();
        }
    }
}
