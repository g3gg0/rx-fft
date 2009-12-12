using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Tuners
{

    public interface Tuner
    {
        event EventHandler FilterWidthChanged;
        event EventHandler FrequencyChanged;
        event EventHandler InvertedSpectrumChanged;

        /* SetFrequency must be a function, because it may fail and returns a status flag */
        bool SetFrequency(long frequency);
        long GetFrequency();

        long FilterWidth { get; }
        bool InvertedSpectrum { get; }


        long LowestFrequency { get; }
        long HighestFrequency { get; }
        long UpperFilterMargin { get; }
        long LowerFilterMargin { get; }
    }
}
