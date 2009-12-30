using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.FFTW;

namespace GSM_Analyzer
{
    public partial class SpectrumVisualizer : Form
    {
        Object SpinLock = new Object();

        public double SamplingRate
        {
            set { FFTDisplay.SamplingRate = value; }
        }

        public SpectrumVisualizer()
        {
            InitializeComponent();

            FFTSize = 2048;

            string[] windowingTypes = Enum.GetNames(typeof(FFTTransformer.eWindowingFunction));
            cmbWindowFunc.Items.AddRange(windowingTypes);
            cmbWindowFunc.Text = FFTDisplay.WindowingFunction.ToString();
            cmbAverage.Text = FFTDisplay.VerticalSmooth.ToString();
            cmbFFTSize.Text = FFTSize.ToString();
        }

        public void ProcessIQSample(double I, double Q)
        {
            FFTDisplay.ProcessSample(I, Q);
        }

        public void ProcessIQData(double[] iSamples, double[] qSamples)
        {
            FFTDisplay.ProcessData(iSamples, qSamples);
        }

        int FFTSize
        {
            get { return FFTDisplay.FFTSize; }
            set
            {
                lock (SpinLock)
                {
                    FFTDisplay.FFTSize = value;
                }
            }
        }


        private void cmbWindowFunc_TextChanged(object sender, EventArgs e)
        {
            string typeString = cmbWindowFunc.Text;

            try
            {
                FFTTransformer.eWindowingFunction type = (FFTTransformer.eWindowingFunction)Enum.Parse(typeof(FFTTransformer.eWindowingFunction), typeString);
                FFTDisplay.WindowingFunction = type;
            }
            catch (Exception ex)
            {
            }
        }

        private void cmbAverage_TextChanged(object sender, EventArgs e)
        {
            double avg;

            if (!double.TryParse(cmbAverage.Text, out avg))
                return;
            FFTDisplay.VerticalSmooth = avg;
        }

        private void cmbFFTSize_TextChanged(object sender, EventArgs e)
        {
            int size;

            if (!int.TryParse(cmbFFTSize.Text, out size))
                return;

            FFTSize = size;
            FFTDisplay.FFTSize = size;
        }

    }
}
