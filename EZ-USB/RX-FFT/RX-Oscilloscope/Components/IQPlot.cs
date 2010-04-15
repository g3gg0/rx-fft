using System;
using System.Drawing;
using System.Windows.Forms;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SignalProcessing;

namespace RX_Oscilloscope.Components
{
    public partial class IQPlot : UserControl
    {
        private int SamplesTotal = 10000;

        private IIRFilter LowPass = null;
        private double LastPhase;

        public IQPlot()
        {
            InitializeComponent();
            waveForm.KeepText = true;
            UpdateScale();
        }

        public double SamplingRate
        {
            get
            {
                return waveForm.SamplingRate;
            }

            set
            {
                waveForm.SamplingRate = value;

                try
                {
                    BeginInvoke(new MethodInvoker(delegate()
                    {
                        txtSamplingRate.Frequency = (long)value;
                        UpdateSampleTimes();
                    }));
                }
                catch (Exception)
                {}
            }
        }

        internal void Process(double I, double Q)
        {
            waveForm.ProcessData(I, Q);
        }
        
        private void txtSamplingRate_FrequencyChanged(object sender, EventArgs e)
        {
            if (SamplingRate != txtSamplingRate.Frequency)
            {
                SamplingRate = txtSamplingRate.Frequency;
            }
        }


        private void txtBufferTime_ValueChanged(object sender, EventArgs e)
        {
            SamplesTotal = (int)txtBufferTime.Value;
            waveForm.MaxSamples = SamplesTotal;

            UpdateSampleTimes();
        }


        private void UpdateSampleTimes()
        {
            double rate = waveForm.SamplingRate;
            if (rate != 0)
            {
                lblBufferTime.Text = FrequencyFormatter.TimeToString(SamplesTotal / rate);
            }
        }


        private void UpdateScale()
        {

        }

    }
}
