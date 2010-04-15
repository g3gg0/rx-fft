using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries;

namespace LibRXFFT.Components.GDI
{
    public partial class FileFormatDialog : Form
    {
        public long SamplingRate = 0;

        public ByteUtil.eSampleFormat SampleFormat
        {
            get
            {
                try
                {
                    return (ByteUtil.eSampleFormat)Enum.Parse(typeof(ByteUtil.eSampleFormat), "Direct" + cmbType.Text);
                }
                catch (Exception)
                {
                }

                return ByteUtil.eSampleFormat.Unknown;
            }
            set
            {
                cmbType.Text = value.ToString().Replace("Direct", "");
            }
        }

        public FileFormatDialog()
        {
            InitializeComponent();

            foreach (string type in Enum.GetNames(typeof(ByteUtil.eSampleFormat)))
            {
                cmbType.Items.Add(type.Replace("Direct", ""));
            }
        }

        private void frequencySelector_EnterPresed(object sender, System.EventArgs e)
        {
            btnOk_Click(null, null);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                SamplingRate = frequencySelector.CurrentFrequency;
                SampleFormat = (ByteUtil.eSampleFormat)Enum.Parse(typeof(ByteUtil.eSampleFormat), "Direct" + cmbType.Text);
            }
            catch(Exception)
            {
            }

            Close();
        }

        internal void EstimateDetails(string fileName)
        {
            if (fileName.Contains("SR_") && fileName.Contains("HZ"))
            {
                string rate = fileName.Substring(fileName.IndexOf("SR_") + 3, fileName.IndexOf("HZ") - fileName.IndexOf("SR_") - 3);

                long.TryParse(rate, out SamplingRate);
            }
        }
    }
}
