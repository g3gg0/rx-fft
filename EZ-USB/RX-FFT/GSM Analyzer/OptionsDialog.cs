using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries.GSM.Bursts;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer1;

namespace GSM_Analyzer
{
    public partial class OptionsDialog : Form
    {
        private GSMAnalyzer Analyzer;

        public OptionsDialog(GSMAnalyzer analyzer)
        {
            Analyzer = analyzer;
            InitializeComponent();

            Refresh();
        }

        private void Refresh()
        {
            chkL1DumpFrames.Checked = Analyzer.Parameters.DumpPackets;
            chkL1ShowEncrypted.Checked = SDCCHBurst.ShowEncryptedMessage;
            chkL1DumpEncrypted.Checked = SDCCHBurst.DumpEncryptedMessage;
            chkL1PreallocateTCH.Checked = TimeSlotHandler.PreallocateTCHs;

            chkL2ShowAllFrames.Checked = L2Handler.ShowAllMessages;
            chkL2DumpRaw.Checked = L2Handler.DumpRawData;

            chkL3CellBroadcast.Checked = CBCHandler.ShowCBMessages;
            chkL3DumpRaw.Checked = L3Handler.DumpRawData;
            chkL3ShowUnhandled.Checked = L3Handler.DumpUnhandled;
            chkL3SniffIMSI.Checked = L3Handler.SniffIMSI;

            chkSubSample.Checked = Analyzer.Subsampling;
            chkPhaseAutoOffset.Checked = Analyzer.PhaseAutoOffset;

            txtSubSampleOffset.Text = Analyzer.SubSampleOffset.ToString();
            txtPhaseOffset.Enabled = !Analyzer.PhaseAutoOffset;
            txtPhaseOffset.Text = Analyzer.PhaseOffset.ToString();
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

        private void chkL3DumpRaw_CheckedChanged(object sender, EventArgs e)
        {
            L3Handler.DumpRawData = chkL3DumpRaw.Checked;
        }

        private void chkL3ShowUnhandled_CheckedChanged(object sender, EventArgs e)
        {
            L3Handler.DumpUnhandled = chkL3ShowUnhandled.Checked;
        }

        private void chkL3SniffIMSI_CheckedChanged(object sender, EventArgs e)
        {
            L3Handler.SniffIMSI = chkL3SniffIMSI.Checked;
        }

        private void chkSubSample_CheckedChanged(object sender, EventArgs e)
        {
            Analyzer.Subsampling = chkSubSample.Checked;
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

        private void txtPhaseOffset_TextChanged(object sender, EventArgs e)
        {
            double offset;

            if (!double.TryParse(txtPhaseOffset.Text, out offset))
                return;

            Analyzer.PhaseOffset = offset;
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

        private void chkL1ShowEncrypted_CheckedChanged(object sender, EventArgs e)
        {
            SDCCHBurst.ShowEncryptedMessage = chkL1ShowEncrypted.Checked;
            SACCHBurst.ShowEncryptedMessage = chkL1ShowEncrypted.Checked;
        }

        private void chkL1DumpEncrypted_CheckedChanged(object sender, EventArgs e)
        {
            SDCCHBurst.DumpEncryptedMessage = chkL1DumpEncrypted.Checked;
            SACCHBurst.DumpEncryptedMessage = chkL1DumpEncrypted.Checked;
        }

        private void chkL3CellBroadcast_CheckedChanged(object sender, EventArgs e)
        {
            CBCHandler.ShowCBMessages = chkL3CellBroadcast.Checked;
        }

        private void chkL2DumpRaw_CheckedChanged(object sender, EventArgs e)
        {
            L2Handler.DumpRawData = chkL2DumpRaw.Checked;
        }

        private void chkL2ShowAllFrames_CheckedChanged(object sender, EventArgs e)
        {
            L2Handler.ShowAllMessages = chkL2ShowAllFrames.Checked;
        }

        private void chkL1DumpFrames_CheckedChanged(object sender, EventArgs e)
        {
            Analyzer.Parameters.DumpPackets = chkL1DumpFrames.Checked;
        }

        private void chkPhaseAutoOffset_CheckedChanged(object sender, EventArgs e)
        {
            Analyzer.PhaseAutoOffset = chkPhaseAutoOffset.Checked;
            txtPhaseOffset.Enabled = !Analyzer.PhaseAutoOffset;
        }

        private void btnBurstLengthA_Click(object sender, EventArgs e)
        {
            Analyzer.BurstLengthJitter = new[] { 0.0d, 0.0d, 0.0d, 0.0d };
            Refresh();
        }

        private void btnBurstLengthB_Click(object sender, EventArgs e)
        {
            Analyzer.BurstLengthJitter = new[] { 0.75, -0.25, -0.25, -0.25 };
            Refresh();
        }

        private void chkL1PreallocateTCH_CheckedChanged(object sender, EventArgs e)
        {
            TimeSlotHandler.PreallocateTCHs = chkL1PreallocateTCH.Checked;
        }


    }
}
