using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GSM_Analyzer
{
    public partial class CFileDecimationDialog : Form
    {
        public long Decimation;

        public CFileDecimationDialog()
        {
            InitializeComponent();

            txtDecimation.Select();
            txtDecimation.KeyPress += new KeyPressEventHandler(txtDecimation_KeyPress);
                
        }

        void txtDecimation_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0x0d)
                Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtDecimation_TextChanged(object sender, EventArgs e)
        {
            long.TryParse(txtDecimation.Text, out Decimation);
        }
    }
}
