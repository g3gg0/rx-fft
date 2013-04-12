using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LibRXFFT.Components.GDI
{
    public partial class LuaShell : Form
    {
        public delegate bool RunCommandFunc(string command);

        public RunCommandFunc RunCommand = null;


        public LuaShell()
        {
            InitializeComponent();
        }

        private void txtShell_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (RunCommand != null)
                {
                    RunCommand(txtShell.Text);
                }
                txtShell.Text = "";
            }
        }

        public void AddMessage(string msg)
        {
            DateTime now = DateTime.Now;

            AddString(string.Format("{0:HH}:{0:mm}:{0:ss}", now) + " " + msg);
        }

        public void AddString(string msg)
        {
            if (txtMessages.InvokeRequired)
            {
                BeginInvoke(new Action(() => { AddString(msg); }));
            }
            else
            {
                txtMessages.Text += msg + Environment.NewLine;
                txtMessages.ScrollToCaret();
            }
        }
    }
}
