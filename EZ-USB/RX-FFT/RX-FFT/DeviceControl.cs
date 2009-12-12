using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.USB_RX.Tuners;
using LibRXFFT.Libraries.SampleSources;

namespace RX_FFT
{
    public interface DeviceControl : DigitalTuner
    {
        int SamplesPerBlock { get; set ;}
        SampleSource SampleSource { get; }
        bool Connected { get; }



        void Close();
        void StartRead();
        void StartStreamRead();
        void StopRead();
    }
}
