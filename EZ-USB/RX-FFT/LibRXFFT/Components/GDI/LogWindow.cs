using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RX_FFT.Components.GDI
{
    public partial class LogWindow : Form
    {
        private string LogText = "";
        private Timer UpdateTimer = new Timer();

        public LogWindow()
        {
            InitializeComponent();

            UpdateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            UpdateTimer.Interval = 50;
            UpdateTimer.Start();
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
                    LogText = "";
                }
            }
        }

        public void AddMessage(string msg)
        {
            lock (txtLog)
            {
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
