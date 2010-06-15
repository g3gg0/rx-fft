using System;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.Demodulators
{
    public enum eSsbType
    {
        Usb,
        Lsb
    }

    public class SSBDemodulator : Demodulator
    {
        public eSsbType Type = eSsbType.Lsb;
        protected double LastQ = 0;


        public SSBDemodulator(eSsbType type)
        {
            Type = type;
        }

        public override void Dispose()
        {
        }

        public override double ProcessSample(double iData, double qData)
        {
            double ret = iData;

            switch (Type)
            {
                case eSsbType.Usb:
                    ret += LastQ;
                    break;
                case eSsbType.Lsb:
                    ret += LastQ;
                    break;
            }

            LastQ = qData;

            return ret;
        }
    }
}
