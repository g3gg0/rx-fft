using System;

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

            LogWindow.AddMessage(string.Format("{0:HH}:{0:mm}:{0:ss}:{0:ffff}", now) + " " + msg);
        }
    }
}
