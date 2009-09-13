using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.USB_RX.Tuners;
using LibRXFFT.Libraries.SampleSources;

namespace RX_FFT
{
    public interface DeviceControl : Tuner
    {
        int SamplesPerBlock { get; set ;}
        SampleSource SampleSource { get; }
        bool Connected { get; }

        event EventHandler FrequencyChanged;

        void Close();
        void StartRead();
        void StartStreamRead();
        void StopRead();
    }
}
