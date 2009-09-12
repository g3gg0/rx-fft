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
	    double tunerMainCenterFreq;
	    double tunerMainOffsetFreq;

        public TunerStack(Tuner tunerDevice, Tuner tunerMain,
                double tunerMainCenterFreq, double tunerMainOffsetFreq)
        {
            this.tunerDevice = tunerDevice;
            this.tunerMain = tunerMain;
            this.tunerMainCenterFreq = tunerMainCenterFreq;
            this.tunerMainOffsetFreq = tunerMainOffsetFreq;
        }

	    public override double getFrequency() {
		    double frequency;
		    double freqDevice = this.tunerDevice.getFrequency();
		    double freqMain = this.tunerMain.getFrequency();
    		
		    if (tunerMain.isSpectrumInverted() != tunerDevice.isSpectrumInverted())
			    frequency = tunerMainCenterFreq - freqDevice + freqMain;
		    else
			    frequency = tunerMainCenterFreq - freqDevice - freqMain;
    		
		    return frequency;
	    }

        public override bool setFrequency(double frequency)
        {
		    double freqMain = ((int)frequency / (int)tunerMainOffsetFreq) * (int)tunerMainOffsetFreq;
		    double freqDevice;
    		
		    if (tunerMain.isSpectrumInverted() != tunerDevice.isSpectrumInverted())
			    freqDevice = tunerMainCenterFreq - (frequency - freqMain) ;
		    else
			    freqDevice = tunerMainCenterFreq + (frequency - freqMain) ;
    		
		    tunerDevice.setFrequency(freqDevice);
		    tunerMain.setFrequency(freqMain);
    		
		    return false;
	    }

        public override bool isSpectrumInverted()
        {
            return tunerMain.isSpectrumInverted() != tunerDevice.isSpectrumInverted();
        }

	    public double getTunerMainCenterFreq() {
		    return tunerMainCenterFreq;
	    }

	    public void setTunerMainCenterFreq(double tunerMainCenterFreq) {
		    this.tunerMainCenterFreq = tunerMainCenterFreq;
	    }

	    public double getTunerMainOffsetFreq() {
		    return tunerMainOffsetFreq;
	    }

	    public void setTunerMainOffsetFreq(double tunerMainOffsetFreq) {
		    this.tunerMainOffsetFreq = tunerMainOffsetFreq;
	    }

    }
}
