using System;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.USB_RX.Tuners;

namespace RX_FFT.DeviceControls
{
    public interface DeviceControl : DigitalTuner
    {
        int SamplesPerBlock { get; set; }
        event EventHandler TransferModeChanged;
        eTransferMode TransferMode { set; get; }
        SampleSource SampleSource { get; }
        bool Connected { get; }
        double BlocksPerSecond { get; set; }

        void Close();
        bool ReadBlock();
    }
}
