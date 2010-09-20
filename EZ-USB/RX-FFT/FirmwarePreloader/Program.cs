using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FirmwarePreloader
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "/auto":
                        FirmwarePreloader.PreloadAll();
                        return;
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FirmwarePreloader());
        }
    }
}
