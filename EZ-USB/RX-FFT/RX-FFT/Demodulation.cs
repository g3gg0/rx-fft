using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries.Demodulators;

namespace RX_FFT
{

    public class Demodulation
    {
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

        public bool AudioLowPassEnabled = false;
        public int AudioLowPassWidthFract = 2;
        public FIRFilter _AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_8);
        public FIRFilter AudioLowPass
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

                if (CursorWindowFilterWidthFract > 2)
                    return CursorWindowFilterWidthFract / 2;

                return 1;
            }
        }
        public FIRFilter _CursorWindowFilterI = new FIRFilter(FIRCoefficients.FIRLowPass_4);
        public FIRFilter _CursorWindowFilterQ = new FIRFilter(FIRCoefficients.FIRLowPass_4);
        public FIRFilter CursorWindowFilterI
        {
            get { return _CursorWindowFilterI; }
            set
            {
                if (_CursorWindowFilterI != null)
                {
                    _CursorWindowFilterI.Dispose();
                }

                _CursorWindowFilterI = value;
            }
        }
        public FIRFilter CursorWindowFilterQ
        {
            get { return _CursorWindowFilterQ; }
            set
            {
                if (_CursorWindowFilterI != null)
                {
                    _CursorWindowFilterI.Dispose();
                }

                _CursorWindowFilterQ = value;
            }
        }

    }
}
