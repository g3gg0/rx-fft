using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.Filters
{
    public class IIRFilter : Filter
    {
        public static bool UseNative = true;

        protected IntPtr NativeContext;

        protected double[] m1;
        protected double[] m2;

        protected double Gain;
        protected int Section;
        protected double[] Num;
        protected double[] Den;
        
        [DllImport("libRXFFT_native.dll", EntryPoint = "IIRInit")]
        public static unsafe extern IntPtr IIRInit(double gain, int section, double[] num, double[] den);
        [DllImport("libRXFFT_native.dll", EntryPoint = "IIRProcess")]
        public static unsafe extern void IIRProcess(IntPtr ctx, double[] inData, double[] outData, int samples);
        [DllImport("libRXFFT_native.dll", EntryPoint = "IIRFree")]
        public static unsafe extern void IIRFree(IntPtr ctx);
        

        public IIRFilter(IIRCoefficients.IIRFilterCoeff coeffs)
        {
            Gain = coeffs.Gain;
            Section = coeffs.Section;
            Num = coeffs.Num;
            Den = coeffs.Den;
            NativeContext = IIRInit(Gain, Section, Num, Den);

            m1 = new double[Section];
            m2 = new double[Section];
        }

        public override void Dispose()
        {
            if (NativeContext != IntPtr.Zero)
            {
                IIRFree(NativeContext);
                NativeContext = IntPtr.Zero;
            }
        }

        public override double[] Process(double[] inData, double[] outData)
        {
            if (outData == null)
                outData = new double[inData.Length];

            if (UseNative && NativeContext != IntPtr.Zero)
            {
                IIRProcess(NativeContext, inData, outData, inData.Length);
            }
            else
            {
                for (int pos = 0; pos < inData.Length; pos++)
                {
                    outData[pos] = ProcessSample(inData[pos]);
                }
            }

            return outData;
        }

        public double ProcessSample(double inputSample)
        {
            int i = 0;
            int arrayPos = 0;
            double[] localDen = Den;
            double[] localNum = Num;
            double s0 = 0;
            double s1 = 0;

            s0 = Gain * inputSample;

            for (i = 0; i < Section; i++)
            {
                s1 = (s0 * localNum[arrayPos + 0] + m1[i]) / localDen[arrayPos + 0];
                m1[i] = m2[i] + s0 * localNum[arrayPos + 1] - s1 * localDen[arrayPos + 1];
                m2[i] = s0 * localNum[arrayPos + 2] - s1 * localDen[arrayPos + 2];
                s0 = s1;
                arrayPos += 3;
            }

            return s1;
        }

    }
}
