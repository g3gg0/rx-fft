using System;
using System.Windows.Forms;
using System.IO;

namespace RX_FFT.Components.GDI
{
    public partial class LogWindow : Form
    {
        private string LogText = "";
        private Timer UpdateTimer = new Timer();
        private TextWriter LogFile = null;
        public bool EnableLogFile = true;

        public LogWindow()
        {
            InitializeComponent();

            UpdateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            UpdateTimer.Interval = 50;
            UpdateTimer.Start();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            UpdateTimer.Stop();

            Visible = false;
            e.Cancel = true;
            base.OnClosing(e);
        }

        void UpdateTimer_Tick(object sender, EventArgs e)
        {
            lock (txtLog)
            {
                if (LogText.Length > 0)
                {
                    if (!Visible)
                    {
                        Show();
                    }
                    txtLog.Text += LogText;
                    txtLog.SelectionStart = txtLog.Text.Length;
                    txtLog.ScrollToCaret();
                    LogText = "";
                }
            }
        }

        public void AddMessage(string msg)
        {
            lock (txtLog)
            {
                if (LogFile == null && EnableLogFile)
                {
                    string app = Application.ProductName;
                    string name = app + "_" + DateTime.Now.ToShortDateString() + ".log";
                    FileStream file = File.Open(name, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    LogFile = new StreamWriter(file);
                    LogText += "Logging to '" + name + "'" + Environment.NewLine;
                }

                if (LogFile != null)
                {
                    LogFile.WriteLine(msg);
                    LogFile.Flush();
                }

                LogText += (msg + Environment.NewLine);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lock (txtLog)
            {
                LogText = "";
                txtLog.Clear();
            }
        }
    }
}
