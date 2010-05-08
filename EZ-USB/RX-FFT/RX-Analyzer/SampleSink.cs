using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RX_Analyzer
{
    public interface SampleSink
    {
        void Process(double iValue, double qValue);
    }
}
