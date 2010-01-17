using System;

namespace LibRXFFT.Libraries.USB_RX.Tuners
{
    public interface DigitalTuner : Tuner
    {
        long SamplingRate { get; }
        event EventHandler SamplingRateChanged;
    }
}
