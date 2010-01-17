using System;
using System.Windows.Forms;
using LibRXFFT.Libraries;

namespace RX_FFT.Dialogs
{
    public partial class FrequencyBandDetailsDialog : Form
    {
        public FrequencyBand band;

        public FrequencyBandDetailsDialog(string title, FrequencyBand band)
        {
            this.Text = title;
            this.band = band;
            InitializeComponent();

            txtBaseFreq.Frequency = band.BaseFrequency;
            txtLabel.Text = band.Label;
            txtChannelDist.Frequency = band.ChannelDistance;
            txtChannelWidth.Frequency = band.ChannelWidth;
            txtChannelStart.Value = band.ChannelStart;
            txtChannelEnd.Value = band.ChannelEnd;
        }

        public FrequencyBandDetailsDialog(FrequencyBand band) : this("Band...", band) { }

        private void btnOk_Click(object sender, EventArgs e)
        {
            band.BaseFrequency = txtBaseFreq.Frequency;
            band.Label = txtLabel.Text;
            band.ChannelDistance = txtChannelDist.Frequency;
            band.ChannelWidth = txtChannelWidth.Frequency;
            band.ChannelStart = txtChannelStart.Value;
            band.ChannelEnd = txtChannelEnd.Value;
            DialogResult = DialogResult.OK;

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
