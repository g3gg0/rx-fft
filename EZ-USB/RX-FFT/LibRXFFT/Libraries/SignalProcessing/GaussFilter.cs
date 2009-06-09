using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class GaussFilter
    {
        private double alpha;
        private double partialBase;
        private double _BT;

        public double BT
        {
            get { return _BT; }
            set
            {
                _BT = value;
                UpdateAlpha();
            }
        }

        public GaussFilter(double bt)
        {
            BT = bt;
        }

        public double Gauss(double t)
        {
            double exponent = alpha * t * BT;
            return partialBase*Math.Exp(-exponent*exponent);
        }

        private void UpdateAlpha()
        {
            alpha = Math.PI / Math.Sqrt(Math.Log(2));
            partialBase = alpha / Math.Sqrt(Math.PI);
        }

        public double[] Process(double[] srcData, double oversampling)
        {
            return Process(srcData, null, oversampling);
        }

        public double[] Process(double[] srcData, double[] dstData, double oversampling)
        {
            if (dstData == null)
                dstData = new double[srcData.Length];

            for (int dstPos = 0; dstPos < dstData.Length; dstPos++)
            {
                double dstVal = 0;

                for (int srcPos = 0; srcPos < srcData.Length; srcPos++)
                {
                    double gaussValue = Gauss((srcPos - dstPos) / oversampling);

                    gaussValue /= oversampling;
                    gaussValue *= BT;

                    dstVal += srcData[srcPos] * gaussValue;
                }
                dstData[dstPos] = dstVal;
            }

            return dstData;
        }
    }
}
