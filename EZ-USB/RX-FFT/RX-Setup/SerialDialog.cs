using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RX_Setup
{
    public partial class SerialDialog : Form
    {
        public string Serial = "(not set)";
        public int TCXOFreq = 0;

        public SerialDialog(string serial, int tcxoFreq)
        {
            InitializeComponent();

            txtSerial.Text = serial;
            txtTCXO.Text = tcxoFreq.ToString();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            Serial = txtSerial.Text;
            int.TryParse(txtTCXO.Text, out TCXOFreq);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
