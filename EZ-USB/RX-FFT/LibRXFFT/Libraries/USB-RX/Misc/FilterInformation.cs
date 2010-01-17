using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Misc
{
    public interface FilterInformation
    {
        long Width { get; }
        long Rate { get; }
        string Location { get; }
    }
}
