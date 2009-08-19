using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.Demodulators
{
    public class FMDemodulator : Demodulator
    {
        private double LastI = 0;
        private double LastQ = 0;
        public bool Accurate = false;

        [DllImport("libRXFFT_native.dll", EntryPoint = "FMDemodInit")]
        public static unsafe extern IntPtr FMDemodInit();
        [DllImport("libRXFFT_native.dll", EntryPoint = "FMDemodProcess")]
        public static unsafe extern void FMDemodProcess(IntPtr ctx, double[] inDataI, double[] inDataQ, double[] outData, int entries);
        [DllImport("libRXFFT_native.dll", EntryPoint = "FMDemodFree")]
        public static unsafe extern void FMDemodFree(IntPtr ctx);



        public FMDemodulator()
        {
            NativeContext = FMDemodInit();
        }

        public override void Dispose()
        {
            if (NativeContext != IntPtr.Zero)
            {
                FMDemodFree(NativeContext);
                NativeContext = IntPtr.Zero;
            }
        }

        public override double[] ProcessDataNative(double[] iDataIn, double[] qDataIn, double[] outData)
        {
            if (NativeContext != IntPtr.Zero)
            {
                FMDemodProcess(NativeContext, iDataIn, qDataIn, outData, iDataIn.Length);
            }
            return outData;
        }

        public override double ProcessSample(double iData, double qData)
        {
            double norm = (iData * iData + qData * qData) * 4;
            double deltaI = iData - LastI;
            double deltaQ = qData - LastQ;

            double sampleValue = (iData * deltaQ - qData * deltaI) / norm;

            LastI = iData;
            LastQ = qData;

            return sampleValue;
        }
    }
}
