using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Devices;

namespace RX_FFT.Dialogs
{
    public partial class FilterDetailDialog : Form
    {
        private Atmel Atmel;

        public FilterDetailDialog(Atmel atmel)
        {
            Atmel = atmel;

            InitializeComponent();
        }

        private void SetFilterDetails()
        {
            FilterCorrectionCoeff corr = Atmel.FilterCorrection;

            corr.AGCCorrectionGain = txtFilterGain.Value;
            corr.AGCCorrectionOffset = txtFilterOffset.Value;

            Atmel.FilterCorrection = corr;
        }

        private void txtFilterGain_ValueChanged(object sender, System.EventArgs e)
        {
            SetFilterDetails();
        }


        private void txtFilterOffset_ValueChanged(object sender, System.EventArgs e)
        {
            SetFilterDetails();
        }
    }
}
