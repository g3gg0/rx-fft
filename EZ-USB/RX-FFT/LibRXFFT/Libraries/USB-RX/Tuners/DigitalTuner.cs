using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Tuners
{
    public interface DigitalTuner : Tuner
    {
        long SamplingRate { get; }
        event EventHandler SamplingRateChanged;
    }
}
