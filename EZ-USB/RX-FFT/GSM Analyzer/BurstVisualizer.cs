using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.GSM.Bursts;
using LibRXFFT.Components.DirectX;

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

            SampleDisplay.ActionMouseDragX = eUserAction.UserCallback;
            SampleDisplay.ActionMouseWheelShift = eUserAction.UserCallback;
            StrengthDisplay.ActionMouseDragX = eUserAction.UserCallback;
            StrengthDisplay.ActionMouseWheelShift = eUserAction.UserCallback;
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

                case eUserEvent.MouseWheelShift:
                    SampleDisplay.ProcessUserAction(eUserAction.XZoom, delta);
                    StrengthDisplay.ProcessUserAction(eUserAction.XZoom, delta);
                    break;
            }
        }


    }
}
