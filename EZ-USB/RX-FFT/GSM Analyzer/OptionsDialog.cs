using System;
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

            chkDumpRaw.Checked = L3Handler.DumpRawData;
            chkShowUnhandled.Checked = L3Handler.DumpUnhandled;
            chkSniffIMSI.Checked = L3Handler.SniffIMSI;
            chkSubSample.Checked = GSMAnalyzer.Subsampling;

            if (Analyzer.Source != null)
            {
                chkFastAtan2.Checked = Analyzer.Source.Demodulator.UseFastAtan2;
                chkInvert.Checked = Analyzer.Source.Demodulator.InvertedSpectrum;
                txtRate.Text = Analyzer.Source.InputSamplingRate.ToString();
                txtInternalOvers.Text = Analyzer.Source.InternalOversampling.ToString();
            }
            else
            {
                chkFastAtan2.Checked = GMSKDemodulator.UseFastAtan2Default;
                chkInvert.Checked = GMSKDemodulator.InvertedSpectrumDefault;
                txtRate.Text = Analyzer.DefaultSamplingRate.ToString();
                txtInternalOvers.Text = Analyzer.InternalOversampling.ToString();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chkFastAtan2_CheckedChanged(object sender, EventArgs e)
        {
            if (Analyzer.Source != null)
                Analyzer.Source.Demodulator.UseFastAtan2 = chkFastAtan2.Checked;
            else
                GMSKDemodulator.UseFastAtan2Default = chkFastAtan2.Checked;
        }

        private void chkInvert_CheckedChanged(object sender, EventArgs e)
        {
            if (Analyzer.Source != null)
                Analyzer.Source.Demodulator.InvertedSpectrum = chkInvert.Checked;
            else
                GMSKDemodulator.InvertedSpectrumDefault = chkInvert.Checked;
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

            if (Analyzer.Source != null)
            {
                if (rate != Analyzer.Source.InputSamplingRate)
                {
                    Analyzer.Source.InputSamplingRate = rate;
                    Analyzer.Source.SamplingRateChanged = true;
                }
            }
            else
                if (rate != Analyzer.DefaultSamplingRate)
                    Analyzer.DefaultSamplingRate = rate;


            if (Analyzer.Oversampling > 1)
                lblOversampling.Text = string.Format("{0:0.000}", Analyzer.Oversampling);
            else
                lblOversampling.Text = string.Format("{0:0.000}", Analyzer.Oversampling) + " (invalid)";
        }

        private void txtInternalOvers_TextChanged(object sender, EventArgs e)
        {
            int rate;

            if (!int.TryParse(txtInternalOvers.Text, out rate))
            {
                lblOversampling.Text = "Invalid internal overs. rate!";
                return;
            }

            Analyzer.InternalOversampling = rate;
        }


    }
}
