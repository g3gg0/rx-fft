using System;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Components.DeviceControls;

namespace LibRXFFT.Components.DeviceControls
{
    public partial class SharedMemDeviceControl : Form, DeviceControl
    {
        private ShmemSampleSource _SampleSource;
        private double _BlocksPerSecond = 20;

        public int ShmemChannel
        {
            get { return _SampleSource.ShmemChannel.SrcChan; }
        }

        public SharedMemDeviceControl(int srcChan)
        {
            InitializeComponent();

            _SampleSource = new ShmemSampleSource("FFT Display", srcChan, 1, 0);
            _SampleSource.InvertedSpectrum = false;
            _SampleSource.SamplingRateChanged += new EventHandler(_SampleSource_SamplingRateChanged);
        }

        void _SampleSource_SamplingRateChanged(object sender, EventArgs e)
        {
            if (SamplingRateChanged != null)
            {
                SamplingRateChanged(this, null);
            }
        }

        #region DeviceControl Member


        public bool AllowsMultipleReaders
        {
            get
            {
                return true;
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

        public string ErrorMessage
        {
            get { return "None"; }
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

        public void CloseControl()
        {
            SampleSource.Close();
            CloseTuner();
            Close();
        }

        public bool ReadBlock()
        {
            bool ret;

            ret = SampleSource.Read();

            return ret;
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
                return (long)SampleSource.OutputSamplingRate;
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
            get { return new[] {"Shared Memory data source"}; }
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
        public bool ScanFrequenciesEnabled { get; set; }

        #endregion
    }
}
