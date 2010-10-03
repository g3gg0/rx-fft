using System;
using System.Drawing;
using System.Windows.Forms;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Components.DirectX.Drawables;
using LibRXFFT.Components.DirectX.Drawables.Docks;

namespace RX_Oscilloscope.Components
{
    public partial class IQPlot : UserControl
    {
        private int SamplesTotal = 10000;
        private PlotVertsHistory History;


        public IQPlot()
        {
            InitializeComponent();
            waveForm.MaxSamples = SamplesTotal;
            waveForm.KeepText = true;

            History = new PlotVertsHistory(waveForm);
            History.HistLength = 1;

            DockPanel panel = new DockPanel(waveForm, eOrientation.RightBorder);
            new DensityMap(panel).Granularity = 32;

            panel = new DockPanel(waveForm, eOrientation.BottomBorder);
            new DensityMap(panel).Granularity = 32;

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

                if (InvokeRequired)
                {
                    try
                    {
                        BeginInvoke(new MethodInvoker(delegate()
                        {
                            SamplingRate = value;
                        }));
                    }
                    catch (Exception)
                    { }
                }
                else
                {
                    txtSamplingRate.Frequency = (long)value;
                    UpdateSampleTimes();
                }
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

        private void txtEyePlotBlocks_ValueChanged(object sender, System.EventArgs e)
        {
            History.HistLength = txtEyePlotBlocks.Value;
        }

        private void chkEyePlot_CheckedChanged(object sender, EventArgs e)
        {
            txtEyePlotBlocks.Enabled = chkEyePlot.Checked;
            waveForm.RealTimeMode = chkEyePlot.Checked;
            History.HistLength = txtEyePlotBlocks.Value;
            History.Enabled = chkEyePlot.Checked;
        }
    }
}
