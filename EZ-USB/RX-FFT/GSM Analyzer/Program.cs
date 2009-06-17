using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Windows.Forms;

namespace GSM_Analyzer
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GSMAnalyzer());
            }
            catch(SecurityException securityException)
            {
                MessageBox.Show("There was an unhandled exception. Don't run this software from a network drive!" + Environment.NewLine + Environment.NewLine + securityException);
            }
            catch (Exception e)
            {
                MessageBox.Show("There was an unhandled exception." + Environment.NewLine + Environment.NewLine + "Exception:" + Environment.NewLine + e);
            }
        }
    }
}
