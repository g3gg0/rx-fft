using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Devices;
using LibRXFFT.Libraries.USB_RX.Misc;
using LibRXFFT.Libraries.Misc;

namespace LibRXFFT.Components.GDI
{
    public partial class FilterDetailDialog : Form
    {
        private Atmel Atmel = null;
        private FilterInformation FilterInfo;

        public FilterDetailDialog(FilterInformation filter)
        {
            if (filter.SourceDevice is Atmel)
            {
                Atmel = (Atmel)filter.SourceDevice;
            }
            FilterInfo = filter;

            InitializeComponent();

            txtFilterRate.Text = FrequencyFormatter.FreqToStringAccurate(filter.Rate);
            txtFilterWidth.Text = FrequencyFormatter.FreqToStringAccurate(filter.Width);
            txtFilterLocation.Text = filter.Location;

            txtFilterOffset.ValueChanged += new EventHandler(txtFilterOffset_ValueChanged);
            txtFilterGain.ValueChanged += new EventHandler(txtFilterGain_ValueChanged);

            ReadFilterDetails();
        }

        private void SetFilterDetails()
        {
            if (Atmel != null)
            {
                FilterCorrectionCoeff corr = Atmel.FilterCorrection;

                corr.AGCCorrectionGain = txtFilterGain.Value;
                corr.AGCCorrectionOffset = txtFilterOffset.Value;

                Atmel.FilterCorrection = corr;
            }
        }

        private void ReadFilterDetails()
        {
            if (Atmel != null)
            {
                FilterCorrectionCoeff corr = Atmel.FilterCorrection;

                txtFilterGain.Value = corr.AGCCorrectionGain;
                txtFilterOffset.Value = corr.AGCCorrectionOffset;
            }
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
