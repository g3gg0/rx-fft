using System.Windows.Forms;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;

namespace GSM_Analyzer
{
    public partial class BurstVisualizer : Form
    {

        private double _Oversampling;
        private double _XAxisGridOffset;
        private double _XAxisSampleOffset;

        public bool[] BurstBits
        {
            set { SampleDisplay.BurstBits = value; }
        }

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

        public BurstVisualizer(double Oversampling)
        {
            InitializeComponent();
            this.Oversampling = Oversampling;
            SampleDisplay.XAxisLines = (int)Burst.NetBitCount;
            SampleDisplay.Name = "Sample Value";
            StrengthDisplay.XAxisLines = (int)Burst.NetBitCount;
            StrengthDisplay.Name = "Strength";

            /* handle X Zoom and X Offset ourselves */
            SampleDisplay.UserEventCallback = UserEventCallback;
            StrengthDisplay.UserEventCallback = UserEventCallback;

            SampleDisplay.EventActions[eUserEvent.MouseDragX] = eUserAction.UserCallback;
            SampleDisplay.EventActions[eUserEvent.MouseWheelUpShift] = eUserAction.UserCallback;
            SampleDisplay.EventActions[eUserEvent.MouseWheelDownShift] = eUserAction.UserCallback;
            StrengthDisplay.EventActions[eUserEvent.MouseDragX] = eUserAction.UserCallback;
            StrengthDisplay.EventActions[eUserEvent.MouseWheelUpShift] = eUserAction.UserCallback;
            StrengthDisplay.EventActions[eUserEvent.MouseWheelDownShift] = eUserAction.UserCallback;
        }

        public void ProcessBurst(double[] signal, double[] strength)
        {
            SampleDisplay.ClearProcessData(signal);
            StrengthDisplay.ClearProcessData(strength);
        }

        public void UserEventCallback(eUserEvent evt, double delta)
        {
            switch (evt)
            {
                case eUserEvent.MouseDragX:
                    SampleDisplay.ProcessUserAction(eUserAction.XOffset, delta);
                    StrengthDisplay.ProcessUserAction(eUserAction.XOffset, delta);
                    break;

                case eUserEvent.MouseWheelUpShift:
                    SampleDisplay.ProcessUserAction(eUserAction.XZoomIn, delta);
                    StrengthDisplay.ProcessUserAction(eUserAction.XZoomIn, delta);
                    break;

                case eUserEvent.MouseWheelDownShift:
                    SampleDisplay.ProcessUserAction(eUserAction.XZoomOut, delta);
                    StrengthDisplay.ProcessUserAction(eUserAction.XZoomOut, delta);
                    break;
            }
        }


    }
}
