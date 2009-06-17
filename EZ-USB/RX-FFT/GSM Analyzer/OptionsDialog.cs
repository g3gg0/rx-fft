using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer3;

namespace GSM_Analyzer
{
    public partial class OptionsDialog : Form
    {
        private GSMAnalyzer Analyzer;

        public OptionsDialog(GSMAnalyzer analyzer)
        {
            Analyzer = analyzer;
            InitializeComponent();

            chkFastAtan2.Checked = GMSKDemodulator.UseFastAtan2;
            chkInvert.Checked = GMSKDemodulator.InvertedSpectrum;
            chkDumpRaw.Checked = L3Handler.DumpRawData;
            chkShowUnhandled.Checked = L3Handler.DumpUnhandled;
            chkSniffIMSI.Checked = L3Handler.SniffIMSI;
            chkSubSample.Checked = GSMAnalyzer.Subsampling;

            txtRate.Text = Analyzer.SamplingRate.ToString();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chkFastAtan2_CheckedChanged(object sender, EventArgs e)
        {
            GMSKDemodulator.UseFastAtan2 = chkFastAtan2.Checked;
        }

        private void chkInvert_CheckedChanged(object sender, EventArgs e)
        {
            GMSKDemodulator.InvertedSpectrum = chkInvert.Checked;
        }

        private void chkDumpRaw_CheckedChanged(object sender, EventArgs e)
        {
            L3Handler.DumpRawData = chkDumpRaw.Checked;
        }

        private void chkShowUnhandled_CheckedChanged(object sender, EventArgs e)
        {
            L3Handler.DumpUnhandled = chkShowUnhandled.Checked;
        }

        private void chkSniffIMSI_CheckedChanged(object sender, EventArgs e)
        {
            L3Handler.SniffIMSI = chkSniffIMSI.Checked;
        }


        private void chkSubSample_CheckedChanged(object sender, EventArgs e)
        {
            GSMAnalyzer.Subsampling = chkSubSample.Checked;
        }

        private void txtRate_TextChanged(object sender, EventArgs e)
        {
            double rate;

            if (!double.TryParse(txtRate.Text, out rate))
            {
                lblOversampling.Text = "Invalid rate!";
                return;
            }

            if (rate != Analyzer.SamplingRate)
            {
                Analyzer.SamplingRate = rate;
                Analyzer.SamplingRateChanged = true;
            }

            if (Analyzer.Oversampling > 1)
                lblOversampling.Text = string.Format("{0:0.000}", Analyzer.Oversampling);
            else
                lblOversampling.Text = string.Format("{0:0.000}", Analyzer.Oversampling) + " (invalid)";
        }


    }
}
