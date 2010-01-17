using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemodulatorCollection
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

            LogWindow.AddMessage(string.Format("{0:00}:{1:00}:{2:00}.{3:000}", now.Hour, now.Minute, now.Second, now.Millisecond) + " " + msg);
        }
    }
}
