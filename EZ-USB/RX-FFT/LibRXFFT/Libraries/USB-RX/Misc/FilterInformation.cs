namespace LibRXFFT.Libraries.USB_RX.Misc
{
    public interface FilterInformation
    {
        object SourceDevice { get; }
        long Width { get; }
        long Rate { get; }
        string Location { get; }
    }
}
