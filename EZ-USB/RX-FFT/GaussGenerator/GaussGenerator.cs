using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.SignalProcessing;

namespace GaussGenerator
{
    public partial class GaussGenerator : Form
    {
        private SharedMem ShmemChannel;

        public GaussGenerator()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (ShmemChannel != null)
            {
                ShmemChannel.Unregister();
                ShmemChannel = null;

                btnOpen.Text = "Open";
            }
            else
            {
                ShmemChannel = new SharedMem(0, -1, "Gauss Generator");
                btnOpen.Text = "Close";
            }

        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            string srcBits = txtSequence.Text;

            double BT = 0;
            double Oversampling = 0;

            try
            {
                BT = double.Parse(txtBT.Text);
                Oversampling = double.Parse(txtOversampling.Text);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Invalid parameter. Either BT or Oversampling value is wrong.");
                return;
            }


            double[] srcData = new double[srcBits.Length];

            for (int pos = 0; pos < srcBits.Length; pos++)
            {
                if (srcBits.Substring(pos, 1) == "1")
                    srcData[pos] = 1.0;
                else if (srcBits.Substring(pos, 1) == "0")
                    srcData[pos] = -1.0;
                else
                {
                    MessageBox.Show("Invalid binary sequence.");
                    return;
                }
            }

            //            byte[] srcData = new byte[] {0xB9, 0x62, 0x04, 0x0F, 0x2D, 0x45, 0x76, 0x1B};
            //            srcData = DifferenceCode.Encode(srcData);

            Oversampler sampler = new Oversampler();
            double[] samples = sampler.Oversample(srcData, Oversampling);

            GaussFilter filter = new GaussFilter(BT);
            double[] gaussSamples = filter.Process(samples, Oversampling);

            waveformDisplay.Clear();
            waveformDisplay.ProcessData(new double[128]);
            waveformDisplay.ProcessData(gaussSamples);
            if (ShmemChannel != null)
            {
                double[] diffSamples = Differenciator.Differenciate(gaussSamples);
                ShmemChannel.Write(ByteUtil.convertToBytesInterleaved(gaussSamples, diffSamples));
            }

        }

        private void txtSequence_TextChanged(object sender, EventArgs e)
        {
            btnCreate_Click(null, null);
        }

        private void txtBT_TextChanged(object sender, EventArgs e)
        {
            btnCreate_Click(null, null);
        }

        private void txtOversampling_TextChanged(object sender, EventArgs e)
        {
            btnCreate_Click(null, null);
        }
    }
}
