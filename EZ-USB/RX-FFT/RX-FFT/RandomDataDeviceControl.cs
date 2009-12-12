using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.SampleSources;

namespace RX_FFT
{
    public partial class RandomDataDeviceControl : Form, DeviceControl
    {
        private RandomSampleSource _SampleSource;

        public RandomDataDeviceControl()
        {
            InitializeComponent();

            _SampleSource = new RandomSampleSource();
            _SampleSource.InvertedSpectrum = false;
        }

        #region DeviceControl Member

        public int SamplesPerBlock
        {
            get
            {
                return SampleSource.SamplesPerBlock;
            }
            set
            {
                SampleSource.SamplesPerBlock = value;
            }
        }

        public SampleSource SampleSource
        {
            get { return _SampleSource; }
        }

        public bool Connected
        {
            get { return true; }
        }

        public void StartRead()
        {

        }

        public void StartStreamRead()
        {

        }

        public void StopRead()
        {

        }

        #endregion

        #region DigitalTuner Member

        public long SamplingRate
        {
            get
            {
                return 0;
            }
        }

        public event EventHandler SamplingRateChanged;

        #endregion

        #region Tuner Member

        public event EventHandler FilterWidthChanged;
        public event EventHandler FrequencyChanged;
        public event EventHandler InvertedSpectrumChanged;

        public long LowestFrequency
        {
            get { return 0; }
        }

        public long HighestFrequency
        {
            get { return 1000000000; }
        }

        public long UpperFilterMargin
        {
            get { return HighestFrequency; }
        }

        public long LowerFilterMargin
        {
            get { return LowestFrequency; }
        }

        public bool SetFrequency(long frequency)
        {
            return true;
        }

        public long GetFrequency()
        {
            return 0;
        }

        public long FilterWidth
        {
            get
            {
                return 0;
            }
        }

        public bool InvertedSpectrum
        {
            get { return false; }
        }

        #endregion
    }
}
