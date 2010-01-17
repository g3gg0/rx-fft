using System;
using System.Windows.Forms;
using LibRXFFT.Components.DirectX;

namespace RX_FFT.Dialogs
{
    public partial class MarkerDetailsDialog : Form
    {
        public FrequencyMarker marker;

        public MarkerDetailsDialog(string title, FrequencyMarker marker)
        {
            this.Text = title;
            this.marker = marker;
            InitializeComponent();

            frequencySelector.Frequency = marker.Frequency;
            txtLabel.Text = marker.Label;
            txtDesc.Text = marker.Description.Replace("\r\n","\n").Replace("\n", "\r\n");
        }

        public MarkerDetailsDialog(FrequencyMarker marker) : this("Marker...", marker) {}

        private void btnOk_Click(object sender, EventArgs e)
        {
            marker.Frequency = frequencySelector.Frequency;
            marker.Label = txtLabel.Text;
            marker.Description = txtDesc.Text;
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
