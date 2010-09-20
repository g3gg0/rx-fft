using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;
using LibRXFFT.Libraries.GSM.Layer1.GMSK;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.SignalProcessing;

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
            chkL1DumpFrames.Checked = Burst.DumpRawData;
            chkL1ShowEncrypted.Checked = NormalBurst.ShowEncryptedMessage;
            chkL1DumpEncrypted.Checked = NormalBurst.DumpEncryptedMessage;
            chkL1PreallocateTCH.Checked = TimeSlotHandler.PreallocateTCHs;

            chkL2ShowAllFrames.Checked = L2Handler.ShowAllMessages;
            chkL2DumpRaw.Checked = L2Handler.DumpRawData;

            chkL3CellBroadcast.Checked = CBCHandler.ShowCBMessages;
            chkL3DumpRaw.Checked = L3Handler.DumpRawData;
            chkL3ShowUnhandled.Checked = L3Handler.DumpUnhandled;
            chkL3SniffIMSI.Checked = L3Handler.SniffIMSI;

            chkSubSample.Checked = Analyzer.Subsampling;
            chkPhaseAutoOffset.Checked = Analyzer.Parameters.PhaseAutoOffset;

            txtSubSampleOffset.Text = Analyzer.SubSampleOffset.ToString();
            txtPhaseOffset.Enabled = !Analyzer.Parameters.PhaseAutoOffset;
            txtPhaseOffset.Text = Analyzer.Parameters.PhaseOffsetValue.ToString();
            txtDecisionLevel.Text = GMSKDecoder.MinPowerFact.ToString();

            txtOffset1.Text = Analyzer.BurstLengthJitter[0].ToString();
            txtOffset2.Text = Analyzer.BurstLengthJitter[1].ToString();
            txtOffset3.Text = Analyzer.BurstLengthJitter[2].ToString();
            txtOffset4.Text = Analyzer.BurstLengthJitter[3].ToString();

            chkFastAtan2.Checked = Analyzer.Demodulator.UseFastAtan2;

            txtSimAuthHost.Text = Analyzer.AuthHostAddress;

            if(Analyzer.Parameters.A5Algorithm != null)
            {
                string hex = "";
                for (int pos = 0; pos < Analyzer.Parameters.A5Algorithm.Key.Length; pos++)
                {
                    hex += string.Format("{0:X2}", Analyzer.Parameters.A5Algorithm.Key[pos]);
                }
                txtA5Kc.Text = hex;
            }

            if (Analyzer.Source != null)
            {
                chkInvert.Checked = Analyzer.Demodulator.InvertedSpectrum;
                txtRate.Text = Analyzer.Source.InputSamplingRate.ToString();
                txtInternalOvers.Text = Analyzer.Source.InternalOversampling.ToString();
                radioOvsLinear.Checked = Analyzer.Source.OversamplingType == eOversamplingType.Linear;
                radioOvsSinx.Checked = Analyzer.Source.OversamplingType == eOversamplingType.SinC;
                txtSinxDepth.Text = Analyzer.Source.SinXDepth.ToString();
            }
            else
            {
                chkInvert.Checked = GMSKDemodulator.InvertedSpectrumDefault;
                txtRate.Text = Analyzer.DefaultSamplingRate.ToString();
                txtInternalOvers.Text = Analyzer.InternalOversampling.ToString();
                radioOvsLinear.Checked = SampleSource.DefaultOversamplingType == eOversamplingType.Linear;
                radioOvsSinx.Checked = SampleSource.DefaultOversamplingType == eOversamplingType.SinC;
                txtSinxDepth.Text = SampleSource.DefaultSinXDepth.ToString();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chkFastAtan2_CheckedChanged(object sender, EventArgs e)
        {
            Analyzer.Demodulator.UseFastAtan2 = chkFastAtan2.Checked;
        }

        private void chkInvert_CheckedChanged(object sender, EventArgs e)
        {
            if (Analyzer.Source != null)
                Analyzer.Demodulator.InvertedSpectrum = chkInvert.Checked;

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
                    Analyzer.Source.SamplingRateHasChanged = true;
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

            Analyzer.Parameters.PhaseOffsetValue = offset;
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
                    Analyzer.Source.OversamplingType = eOversamplingType.SinC;

                SampleSource.DefaultOversamplingType = eOversamplingType.SinC;
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
            NormalBurst.ShowEncryptedMessage = chkL1ShowEncrypted.Checked;
        }

        private void chkL1DumpEncrypted_CheckedChanged(object sender, EventArgs e)
        {
            NormalBurst.DumpEncryptedMessage = chkL1DumpEncrypted.Checked;
        }

        private void chkL3CellBroadcast_CheckedChanged(object sender, EventArgs e)
        {
            CBCHandler.ShowCBMessages = chkL3CellBroadcast.Checked;
        }

        private void chkL2DumpRaw_CheckedChanged(object sender, EventArgs e)
        {
            L2Handler.DumpRawData = chkL2DumpRaw.Checked;
        }

        private void chkL2DumpFaulty_CheckedChanged(object sender, EventArgs e)
        {
            L2Handler.DumpFaulty = chkL2DumpFaulty.Checked;
        }

        private void chkL2ShowAllFrames_CheckedChanged(object sender, EventArgs e)
        {
            L2Handler.ShowAllMessages = chkL2ShowAllFrames.Checked;
        }

        private void chkL1DumpFrames_CheckedChanged(object sender, EventArgs e)
        {
            Burst.DumpRawData = chkL1DumpFrames.Checked;
        }

        private void chkPhaseAutoOffset_CheckedChanged(object sender, EventArgs e)
        {
            Analyzer.Parameters.PhaseAutoOffset = chkPhaseAutoOffset.Checked;
            txtPhaseOffset.Enabled = !Analyzer.Parameters.PhaseAutoOffset;
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

        private void txtA5Kc_TextChanged(object sender, EventArgs e)
        {
            if (txtA5Kc.Text.Length == 16)
            {
                byte[] key = new byte[8];

                for (int pos = 0; pos < 8; pos++)
                {
                    string byteStr = txtA5Kc.Text.Substring(pos * 2, 2);

                    if (!byte.TryParse(byteStr, System.Globalization.NumberStyles.HexNumber, null, out key[pos]))
                    {
                        Analyzer.Parameters.A5AlgorithmAvailable = false;
                        return;
                    }
                }
                Analyzer.Parameters.A5Algorithm.Key = key;
                Analyzer.Parameters.A5AlgorithmAvailable = true;
            }
            else
            {
                Analyzer.Parameters.A5AlgorithmAvailable = false;
            }
        }

        private void txtSimAuthHost_TextChanged(object sender, EventArgs e)
        {
            Analyzer.AuthHostAddress = txtSimAuthHost.Text;
        }


    }
}
