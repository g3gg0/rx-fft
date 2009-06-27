using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries.GSM.Bursts;
using LibRXFFT.Libraries.GSM.Layer2;

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

            chkShowEncrypted.Checked = SDCCHBurst.ShowEncryptedMessage;
            chkCellBroadcast.Checked = CBCHandler.ShowCBMessages;
            chkShowAllFrames.Checked = L2Handler.ShowAllMessages;
            chkDumpFrames.Checked = Analyzer.Parameters.DumpPackets;

            txtSubSampleOffset.Text = Analyzer.SubSampleOffset.ToString();
            txtDecisionLevel.Text = GMSKDecoder.MinPowerFact.ToString();

            txtOffset1.Text = Analyzer.BurstLengthJitter[0].ToString();
            txtOffset2.Text = Analyzer.BurstLengthJitter[1].ToString();
            txtOffset3.Text = Analyzer.BurstLengthJitter[2].ToString();
            txtOffset4.Text = Analyzer.BurstLengthJitter[3].ToString();

            if (Analyzer.Source != null)
            {
                chkFastAtan2.Checked = Analyzer.Source.Demodulator.UseFastAtan2;
                chkInvert.Checked = Analyzer.Source.Demodulator.InvertedSpectrum;
                txtRate.Text = Analyzer.Source.InputSamplingRate.ToString();
                txtInternalOvers.Text = Analyzer.Source.InternalOversampling.ToString();
                radioOvsLinear.Checked = Analyzer.Source.OversamplingType == eOversamplingType.Linear;
                radioOvsSinx.Checked = Analyzer.Source.OversamplingType == eOversamplingType.SinX;
                txtSinxDepth.Text = Analyzer.Source.SinXDepth.ToString();
            }
            else
            {
                chkFastAtan2.Checked = GMSKDemodulator.UseFastAtan2Default;
                chkInvert.Checked = GMSKDemodulator.InvertedSpectrumDefault;
                txtRate.Text = Analyzer.DefaultSamplingRate.ToString();
                txtInternalOvers.Text = Analyzer.InternalOversampling.ToString();
                radioOvsLinear.Checked = SampleSource.DefaultOversamplingType == eOversamplingType.Linear;
                radioOvsSinx.Checked = SampleSource.DefaultOversamplingType == eOversamplingType.SinX;
                txtSinxDepth.Text = SampleSource.DefaultSinXDepth.ToString();
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

            GMSKDemodulator.UseFastAtan2Default = chkFastAtan2.Checked;
        }

        private void chkInvert_CheckedChanged(object sender, EventArgs e)
        {
            if (Analyzer.Source != null)
                Analyzer.Source.Demodulator.InvertedSpectrum = chkInvert.Checked;

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

        private void txtSubSampleOffset_TextChanged(object sender, EventArgs e)
        {
            int offset;

            if (!int.TryParse(txtSubSampleOffset.Text, out offset))
                return;

            Analyzer.SubSampleOffset = offset;
        }

        private void txtDecisionLevel_TextChanged(object sender, EventArgs e)
        {
            double level;

            if (!double.TryParse(txtDecisionLevel.Text, out level))
                return;

            GMSKDecoder.MinPowerFact = level;
        }

        private void radioOvsLinear_CheckedChanged(object sender, EventArgs e)
        {
            if (radioOvsLinear.Checked)
            {
                if (Analyzer.Source != null)
                    Analyzer.Source.OversamplingType = eOversamplingType.Linear;

                SampleSource.DefaultOversamplingType = eOversamplingType.Linear;
            }
        }

        private void radioOvsSinx_CheckedChanged(object sender, EventArgs e)
        {
            if (radioOvsSinx.Checked)
            {
                if (Analyzer.Source != null)
                    Analyzer.Source.OversamplingType = eOversamplingType.SinX;

                SampleSource.DefaultOversamplingType = eOversamplingType.SinX;
            }
        }

        private void textSinxDepth_TextChanged(object sender, EventArgs e)
        {
            int depth;

            if (!int.TryParse(txtSinxDepth.Text, out depth))
                return;

            if (Analyzer.Source != null)
                Analyzer.Source.SinXDepth = depth;

            SampleSource.DefaultSinXDepth = depth;
        }

        private void txtOffset1_TextChanged(object sender, EventArgs e)
        {
            parseOffsetCorrection();
        }

        private void txtOffset2_TextChanged(object sender, EventArgs e)
        {
            parseOffsetCorrection();
        }

        private void txtOffset3_TextChanged(object sender, EventArgs e)
        {
            parseOffsetCorrection();
        }

        private void txtOffset4_TextChanged(object sender, EventArgs e)
        {
            parseOffsetCorrection();
        }

        private void parseOffsetCorrection()
        {
            double offset1 = 0;
            double offset2 = 0;
            double offset3 = 0;
            double offset4 = 0;

            if (!double.TryParse(txtOffset1.Text, out offset1))
                return;
            if (!double.TryParse(txtOffset2.Text, out offset2))
                return;
            if (!double.TryParse(txtOffset3.Text, out offset3))
                return;
            if (!double.TryParse(txtOffset4.Text, out offset4))
                return;

            if (offset1 + offset2 + offset3 + offset4 != 0.0f)
                return;

            Analyzer.BurstLengthJitter[0] = offset1;
            Analyzer.BurstLengthJitter[1] = offset2;
            Analyzer.BurstLengthJitter[2] = offset3;
            Analyzer.BurstLengthJitter[3] = offset4;
        }

        private void chkShowEncrypted_CheckedChanged(object sender, EventArgs e)
        {
            SDCCHBurst.ShowEncryptedMessage = chkShowEncrypted.Checked;
            SACCHBurst.ShowEncryptedMessage = chkShowEncrypted.Checked;
        }

        private void chkCellBroadcast_CheckedChanged(object sender, EventArgs e)
        {
            CBCHandler.ShowCBMessages = chkCellBroadcast.Checked;
        }

        private void chkShowAllFrames_CheckedChanged(object sender, EventArgs e)
        {
            L2Handler.ShowAllMessages = chkShowAllFrames.Checked;
        }

        private void chkDumpFrames_CheckedChanged(object sender, EventArgs e)
        {
            Analyzer.Parameters.DumpPackets = chkDumpFrames.Checked;
        }
    }
}
