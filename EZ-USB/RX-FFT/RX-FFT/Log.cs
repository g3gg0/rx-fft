using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RX_FFT.Dialogs;

namespace RX_FFT
{
    public class Log
    {
        private static bool Enabled = false;
        private static LogWindow LogWindow = null;

        public static void Init()
        {
            if (Enabled && LogWindow == null)
            {
                LogWindow = new LogWindow();
                LogWindow.Show();
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
