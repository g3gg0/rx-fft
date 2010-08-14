using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries;

namespace RX_FFT.Dialogs
{
    public partial class ScanBandDialog : Form
    {
        private FrequencyBand Band;

        public long StartFrequency
        {
            get
            {
                return txtStartFreq.Frequency;
            }
            set
            {
                txtStartFreq.Frequency = value;
            }
        }

        public long EndFrequency
        {
            get
            {
                return txtEndFreq.Frequency;
            }
            set
            {
                txtEndFreq.Frequency = value;
            }
        }

        public ScanBandDialog(FrequencyBand band)
        {
            InitializeComponent();
            AcceptButton = btnOk;

            Band = band;
            StartFrequency = band.BaseFrequency;
            EndFrequency = band.BaseFrequency;

            UpdateDetails();
        }

        protected void txtStartFreq_FrequencyChanged(object sender, System.EventArgs e)
        {
            UpdateDetails();
        }

        protected void txtEndFreq_FrequencyChanged(object sender, System.EventArgs e)
        {
            UpdateDetails();
        }

        private void UpdateDetails()
        {
            if (StartFrequency > EndFrequency)
            {
                txtStartFreq.ForeColor = Color.Red;
                txtEndFreq.ForeColor = Color.Red;
            }
            else
            {
                txtStartFreq.ForeColor = Color.Cyan;
                txtEndFreq.ForeColor = Color.Cyan;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
        }

        private void Cancel()
        {
            DialogResult = DialogResult.Cancel;
        }

        private void Accept()
        {
            DialogResult = DialogResult.OK;

            long range = EndFrequency - StartFrequency;

            Band.BaseFrequency = StartFrequency + Band.ChannelDistance / 2;
            Band.ChannelStart = 0;
            Band.ChannelEnd = range / Band.ChannelDistance;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Accept();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }
    }
}
