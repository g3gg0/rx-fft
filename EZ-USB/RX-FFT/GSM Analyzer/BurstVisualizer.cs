using System.Windows.Forms;
using LibRXFFT.Libraries.GSM.Bursts;

namespace GSM_Analyzer
{
    public partial class BurstVisualizer : Form
    {
        private double _Oversampling;
        private int _XAxisOffset;

        public double Oversampling
        {
            get
            {
                return _Oversampling;
            }
            set
            {
                _Oversampling = value;
                waveformDisplay1.XAxisUnit = _Oversampling;
                waveformDisplay2.XAxisUnit = _Oversampling;
            }
        }
        public int XAxisOffset
        {
            get
            {
                return _XAxisOffset;
            }
            set
            {
                _XAxisOffset = value;
                waveformDisplay1.XAxisOffset = _XAxisOffset;
                waveformDisplay2.XAxisOffset = _XAxisOffset;
            }
        }

        public BurstVisualizer( double Oversampling )
        {
            InitializeComponent();
            this.Oversampling = Oversampling;
            waveformDisplay1.XAxisLines = (int)Burst.NetBitCount;
            waveformDisplay2.XAxisLines = (int)Burst.NetBitCount;
        }

        public void ProcessBurst ( double[] signal, double[] strength )
        {
            waveformDisplay1.ClearProcessData(signal);
            waveformDisplay2.ClearProcessData(strength);
        }
    }
}
