using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemodulatorCollection
{
    public interface DigitalDemodulator
    {
        double SamplingRate { get; set; }

        void Init();        
        void Process(double iValue, double qValue);
    }
}
