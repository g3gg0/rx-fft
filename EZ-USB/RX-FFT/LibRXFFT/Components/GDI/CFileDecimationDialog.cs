using System;
using System.Windows.Forms;

namespace LibRXFFT.Components.GDI
{
    public partial class CFileDecimationDialog : Form
    {
        public double Decimation;

        public CFileDecimationDialog()
        {
            InitializeComponent();

            txtDecimation.Select();
            txtDecimation.KeyPress += new KeyPressEventHandler(txtDecimation_KeyPress);                
        }

        public void EstimateDecimation(string fileName)
        {
            string[] partsA = fileName.Split('.');

            if (partsA.Length < 2)
                return;

            string[] partsB = partsA[partsA.Length - 2].Split('_');

            if (partsB.Length < 1)
                return;

            double result = 0;

            if (!double.TryParse(partsB[partsB.Length - 1], out result))
                return;

            txtDecimation.Text = result.ToString();
            txtDecimation.SelectAll();
        }

        void txtDecimation_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
            if (e.KeyChar == 0x0d)
            {
                Close();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtDecimation_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(txtDecimation.Text, out Decimation);
        }
    }
}
