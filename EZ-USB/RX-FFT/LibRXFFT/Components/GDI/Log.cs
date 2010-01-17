using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RX_FFT.Components.GDI
{
    public class Log
    {
        private static bool Enabled = true;
        private static LogWindow LogWindow = null;

        public static void Init()
        {
            if (Enabled && LogWindow == null)
            {
                LogWindow = new LogWindow();
            }
        }

        public static void AddMessage(string msg)
        {
            if (!Enabled || LogWindow == null)
            {
                return;
            }

            DateTime now = DateTime.Now;

            LogWindow.AddMessage(now.Hour + ":" + now.Minute + ":" + now.Second + "." + now.Millisecond + " " + msg);
        }
    }
}
