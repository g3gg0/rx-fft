using System;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.SampleSources;

namespace RX_FFT.DeviceControls
{
    public partial class RandomDataDeviceControl : Form, DeviceControl
    {
        private long CurrentFrequency = 0;
        private long CurrentWidth = 1000000;
        private long CurrentRate = 1024000;
        private RandomSampleSource _SampleSource;
        private double _BlocksPerSecond = 20;

        public RandomDataDeviceControl()
        {
            InitializeComponent();

            _SampleSource = new RandomSampleSource();
            _SampleSource.InvertedSpectrum = false;
        }

        #region DeviceControl Member

        public bool AllowsMultipleReaders
        {
            get
            {
                return false;
            }
        }

        public event EventHandler TransferModeChanged;

        public eTransferMode TransferMode
        {
            get
            {
                return eTransferMode.Stream;
            }
            set
            {
            }
        }

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

        public double BlocksPerSecond
        {
            get
            {
                return _BlocksPerSecond;
            }
            set 
            {
                _BlocksPerSecond = value;
            }
        }

        public bool ReadBlock()
        {
            bool ret;

            ret = SampleSource.Read();

            return ret;
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
                return CurrentRate;
            }
        }

        public event EventHandler SamplingRateChanged;
        

        #endregion

        #region Tuner Member

        public event EventHandler FilterWidthChanged;
        public event EventHandler FrequencyChanged;
        public event EventHandler InvertedSpectrumChanged;
        public event EventHandler DeviceDisappeared;
        public event EventHandler DeviceClosed;

        public bool OpenTuner()
        {
            return true;
        }

        public void CloseTuner()
        {
        }

        public double Amplification
        {
            get { return 0; }
            set { }
        }

        public double Attenuation
        {
            get { return 0; }
        }

        public long IntermediateFrequency
        {
            get { return 0; }
        }

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

        public string UpperFilterMarginDescription
        {
            get
            {
                return "artificial limit";
            }
        }

        public string LowerFilterMarginDescription
        {
            get
            {
                return "artificial limit";
            }
        }

        public string FilterWidthDescription
        {
            get
            {
                return "artificial limit";
            }
        }

        public string[] Name
        {
            get { return new[] {"Random noise source"}; }
        }

        public string[] Description
        {
            get { return new[] {"(none)"}; }
        }

        public string[] Details
        {
            get { return new[] {"(none)"}; }
        }

        public bool SetFrequency(long frequency)
        {
            if (SamplingRateChanged != null)
            {
                SamplingRateChanged(this, null);
            }
            return true;
        }

        public long GetFrequency()
        {
            return CurrentFrequency;
        }

        public long FilterWidth
        {
            get
            {
                return CurrentWidth;
            }
        }

        public bool InvertedSpectrum
        {
            get { return false; }
        }

        public bool ScanFrequenciesEnabled { get; set; }
        public int ShmemChannel
        {
            get { return 0; }
        }


        public string ErrorMessage
        {
            get { return "None"; }
        }

        #endregion
    }
}
