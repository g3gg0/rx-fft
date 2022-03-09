using System;

namespace LibRXFFT.Libraries.USB_RX.Tuners
{

    public interface Tuner
    {
        event EventHandler FilterWidthChanged;
        event EventHandler FrequencyChanged;
        event EventHandler InvertedSpectrumChanged;
        event EventHandler DeviceDisappeared;
        event EventHandler DeviceOpened;
        event EventHandler DeviceClosed;

        bool OpenTuner();
        void CloseTuner();

        /* SetFrequency must be a function, because it may fail and returns a status flag */
        bool SetFrequency(long frequency);
        long GetFrequency();

        long FilterWidth { get; }
        string FilterWidthDescription { get; }

        bool InvertedSpectrum { get; }

        string[] Name { get; }
        string[] Description { get; }
        string[] Details { get; }

        long IntermediateFrequency { get; }

        long LowestFrequency { get; }
        long HighestFrequency { get; }

        long UpperFilterMargin { get; }
        string UpperFilterMarginDescription { get; }

        long LowerFilterMargin { get; }
        string LowerFilterMarginDescription { get; }

        /* returns the amplification in dB */
        double Amplification { get; set;  }

        /* returns the attenuation to compensate in dB */
        double Attenuation { get; }
    }
}
