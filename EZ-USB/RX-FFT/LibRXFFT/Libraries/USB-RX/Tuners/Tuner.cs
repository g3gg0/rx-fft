using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Tuners
{
    public class Tuner
    {
        public virtual bool setFrequency(double frequency)
        {
            throw new NotImplementedException();
        }

        public virtual double getFrequency()
        {
            throw new NotImplementedException();
        }

        public virtual bool isSpectrumInverted()
        {
            throw new NotImplementedException();
        }
    }
}
