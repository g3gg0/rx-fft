using System.Threading;
using LibRXFFT.Libraries.Demodulators;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.SignalProcessing;

namespace RX_FFT
{

    public class Demodulation
    {
        public enum eSquelchState
        {
            Open,
            Closed
        }
        public bool ReinitSound = false;
        public double InputRate = 0;
        public double AudioRate = 0;

        public bool AudioAmplificationEnabled = false;
        public double AudioAmplification = 1.0f;

        public int AudioDecimation = 1;

        public bool DisplayDemodulationSignal = false;

        public bool DemodulationEnabled = false;
        public DXSoundDevice SoundDevice = null;
        public Demodulator Demod = new AMDemodulator();
        public Downmixer DemodulationDownmixer = new Downmixer();

        public bool SquelchEnabled = false;
        public double SquelchLowerLimit = -25;
        public double SquelchAverage = -75;
        public double SquelchMax = -65;
        public eSquelchState SquelchState = eSquelchState.Open;
        public long SquelchSampleCounter = 0;
        public long SquelchSampleCount = 50;

        public bool AudioLowPassEnabled = false;
        public int AudioLowPassWidthFract = 2;
        public Filter _AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_8);
        public Filter AudioLowPass
        {
            get { return _AudioLowPass; }
            set
            {
                if (_AudioLowPass != null)
                {
                    _AudioLowPass.Dispose();
                }

                _AudioLowPass = value;
            }
        }

        public bool CursorPositionWindowEnabled = false;
        public int CursorWindowFilterWidthFract = 2;
        public int InputSignalDecimation
        {
            get
            {
                if (!CursorPositionWindowEnabled)
                    return 1;

                if (CursorWindowFilterWidthFract > 1)
                    return CursorWindowFilterWidthFract;

                return 1;
            }
        }

        protected Filter _CursorWindowFilterI;
        protected Filter _CursorWindowFilterQ;
        public ManualResetEvent[] CursorWindowFilterEvents = new ManualResetEvent[2];
        public FilterThread CursorWindowFilterThreadI;
        public FilterThread CursorWindowFilterThreadQ;

        public Filter CursorWindowFilterI
        {
            get { return _CursorWindowFilterI; }
            set
            {
                if (_CursorWindowFilterI != null)
                {
                    _CursorWindowFilterI.Dispose();
                }

                _CursorWindowFilterI = value;
                if (CursorWindowFilterThreadI != null)
                    CursorWindowFilterThreadI.Stop();
                CursorWindowFilterThreadI = new FilterThread(value);
                CursorWindowFilterEvents[0] = CursorWindowFilterThreadI.DataProcessed;
            }
        }
        public Filter CursorWindowFilterQ
        {
            get { return _CursorWindowFilterQ; }
            set
            {
                if (_CursorWindowFilterI != null)
                {
                    _CursorWindowFilterI.Dispose();
                }

                _CursorWindowFilterQ = value;
                if (CursorWindowFilterThreadQ != null)
                    CursorWindowFilterThreadQ.Stop();
                CursorWindowFilterThreadQ = new FilterThread(value);
                CursorWindowFilterEvents[1] = CursorWindowFilterThreadQ.DataProcessed;
            }
        }

        public Demodulation()
        {
            CursorWindowFilterI = new FIRFilter(FIRCoefficients.FIRLowPass_4);
            CursorWindowFilterQ = new FIRFilter(FIRCoefficients.FIRLowPass_4);
        }

        public void Dispose()
        {
            if (CursorWindowFilterThreadQ != null)
                CursorWindowFilterThreadQ.Dispose();
            if (CursorWindowFilterThreadI != null)
                CursorWindowFilterThreadI.Dispose();
            if (CursorWindowFilterI != null)
                CursorWindowFilterI.Dispose();
            if (CursorWindowFilterI != null)
                CursorWindowFilterI.Dispose();
        }

    }
}
