using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.Filters
{
    public abstract class Filter
    {
        public abstract double[] Process(double[] inData, double[] outData);
        public abstract void Dispose();
    }
}
