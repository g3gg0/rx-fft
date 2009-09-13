using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Tuners
{
    public class TunerStack : Tuner 
    {
	    Tuner tunerDevice;
	    Tuner tunerMain;
        long tunerMainCenterFreq;
        long tunerMainOffsetFreq;

        public TunerStack(Tuner tunerDevice, Tuner tunerMain,
                long tunerMainCenterFreq, long tunerMainOffsetFreq)
        {
            this.tunerDevice = tunerDevice;
            this.tunerMain = tunerMain;
            this.tunerMainCenterFreq = tunerMainCenterFreq;
            this.tunerMainOffsetFreq = tunerMainOffsetFreq;
        }

        public long GetFrequency()
        {
            long frequency;
            long freqDevice = this.tunerDevice.GetFrequency();
            long freqMain = this.tunerMain.GetFrequency();
    		
		    if (tunerMain.InvertedSpectrum != tunerDevice.InvertedSpectrum)
			    frequency = tunerMainCenterFreq - freqDevice + freqMain;
		    else
			    frequency = tunerMainCenterFreq - freqDevice - freqMain;
    		
		    return frequency;
	    }

        public bool SetFrequency(long frequency)
        {
            long freqMain = (frequency / tunerMainOffsetFreq) * tunerMainOffsetFreq;
            long freqDevice;
    		
		    if (tunerMain.InvertedSpectrum != tunerDevice.InvertedSpectrum)
			    freqDevice = tunerMainCenterFreq - (frequency - freqMain) ;
		    else
			    freqDevice = tunerMainCenterFreq + (frequency - freqMain) ;
    		
		    if (!tunerDevice.SetFrequency(freqDevice))
                return false;
            if (!tunerMain.SetFrequency(freqMain))
                return false;
    		
		    return true;
	    }

        public bool InvertedSpectrum
        {
            get
            {
                return tunerMain.InvertedSpectrum != tunerDevice.InvertedSpectrum;
            }
        }

        public long getTunerMainCenterFreq()
        {
		    return tunerMainCenterFreq;
	    }

        public void setTunerMainCenterFreq(long tunerMainCenterFreq)
        {
		    this.tunerMainCenterFreq = tunerMainCenterFreq;
	    }

        public long getTunerMainOffsetFreq()
        {
		    return tunerMainOffsetFreq;
	    }

        public void setTunerMainOffsetFreq(long tunerMainOffsetFreq)
        {
		    this.tunerMainOffsetFreq = tunerMainOffsetFreq;
	    }

    }
}
