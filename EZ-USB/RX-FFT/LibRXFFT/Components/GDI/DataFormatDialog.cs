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
    public partial class DataFormatDialog : Form
    {
        public long _SamplingRate = 0;
        public long SamplingRate
        {
            get
            {
                return _SamplingRate;
            }
            set
            {
                _SamplingRate = value;
                frequencySelector.Frequency = SamplingRate;
            }
        }

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

        public DataFormatDialog()
        {
            InitializeComponent();

            foreach (string type in Enum.GetNames(typeof(ByteUtil.eSampleFormat)))
            {
                cmbType.Items.Add(type.Replace("Direct", ""));
            }

            frequencySelector.Frequency = SamplingRate;
        }

        private void frequencySelector_EnterPresed(object sender, System.EventArgs e)
        {
            btnOk_Click(null, null);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                frequencySelector.ParseFrequency();
                SamplingRate = frequencySelector.CurrentFrequency;
                SampleFormat = (ByteUtil.eSampleFormat)Enum.Parse(typeof(ByteUtil.eSampleFormat), "Direct" + cmbType.Text);
            }
            catch(Exception)
            {
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        internal void EstimateDetails(string fileName)
        {
            if (fileName.Contains("SR_") && fileName.Contains("HZ"))
            {
                string rate = fileName.Substring(fileName.IndexOf("SR_") + 3, fileName.IndexOf("HZ") - fileName.IndexOf("SR_") - 3);

                long.TryParse(rate, out _SamplingRate);
            }
        }
    }
}
