using System;
using System.Windows.Forms;

namespace RX_Oscilloscope
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt f�r die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RXOscilloscope());
        }
    }
}
