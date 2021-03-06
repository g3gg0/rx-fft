﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LibRXFFT.Libraries.Misc
{
    public class TraceMutex
    {
        private Mutex Mutex = new Mutex();
        private bool Locked = false;
        private string Trace = "";

        public void WaitOne()
        {
            Mutex.WaitOne();
            Trace = Environment.StackTrace;
            Locked = true;
        }

        public void ReleaseMutex()
        {
            Locked = false;
            Mutex.ReleaseMutex();
        }
    }
}
