using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.GSM.Bursts;

namespace GSM_Analyzer
{
    public partial class BurstVisualizer : Form
    {
        private double _Oversampling;
        private double _XAxisGridOffset;
        private double _XAxisSampleOffset;

        public double Oversampling
        {
            get
            {
                return _Oversampling;
            }
            set
            {
                _Oversampling = value;
                SampleDisplay.XAxisUnit = _Oversampling;
                StrengthDisplay.XAxisUnit = _Oversampling;
            }
        }
        public double XAxisGridOffset
        {
            get
            {
                return _XAxisGridOffset;
            }
            set
            {
                _XAxisGridOffset = value;
                SampleDisplay.XAxisGridOffset = _XAxisGridOffset;
                StrengthDisplay.XAxisGridOffset = _XAxisGridOffset;
            }
        }

        public double XAxisSampleOffset
        {
            get
            {
                return _XAxisSampleOffset;
            }
            set
            {
                _XAxisSampleOffset = value;
                SampleDisplay.XAxisSampleOffset = _XAxisSampleOffset;
                StrengthDisplay.XAxisSampleOffset = _XAxisSampleOffset;
            }
        }

        public BurstVisualizer( double Oversampling )
        {
            InitializeComponent();
            this.Oversampling = Oversampling;
            SampleDisplay.XAxisLines = (int)Burst.NetBitCount;
            SampleDisplay.Name = "Sample Value";
            StrengthDisplay.XAxisLines = (int)Burst.NetBitCount;
            StrengthDisplay.Name = "Strength";
        }

        public void ProcessBurst ( double[] signal, double[] strength )
        {
            SampleDisplay.ClearProcessData(signal);
            StrengthDisplay.ClearProcessData(strength);
        }
    }
}
