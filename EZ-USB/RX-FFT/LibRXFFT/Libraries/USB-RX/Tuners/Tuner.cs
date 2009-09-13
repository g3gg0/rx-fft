using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Tuners
{
    public interface Tuner
    {
        bool SetFrequency(long frequency);
        long GetFrequency();
        bool InvertedSpectrum { get; }
    }
}
