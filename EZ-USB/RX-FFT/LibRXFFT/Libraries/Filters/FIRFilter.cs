using System;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.Filters
{
    public class FIRFilter : Filter
    {
        public static bool UseNative = true;

        protected double[] Coefficients = null;
        protected double[] DelayLine;
        protected int DelayLinePosition = 0;
        protected IntPtr NativeContext;

        [DllImport("libRXFFT_native.dll", EntryPoint = "FIRInit", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern IntPtr FIRInit(double[] coeff, int entries);
        [DllImport("libRXFFT_native.dll", EntryPoint = "FIRProcess", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void FIRProcess(IntPtr ctx, double[] inData, double[] outData, int samples);
        [DllImport("libRXFFT_native.dll", EntryPoint = "FIRFree", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void FIRFree(IntPtr ctx);



        public FIRFilter(double[] coeffs)
        {
            try
            {
                NativeContext = FIRInit(coeffs, coeffs.Length);
            }
            catch (Exception e)
            {
            }

            Coefficients = coeffs;
            DelayLine = new double[Coefficients.Length];
        }

        public override void Dispose ()
        {
            if (NativeContext != IntPtr.Zero)
            {
                FIRFree(NativeContext);
                NativeContext = IntPtr.Zero;
            }
        }

        public override double[] Process(double[] inData, double[] outData)
        {
            if (outData == null)
                outData = new double[inData.Length];

            if (UseNative && NativeContext != IntPtr.Zero)
            {
                FIRProcess(NativeContext, inData, outData, inData.Length);
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

	    public double ProcessSample (double inputSample) 
        {
		    DelayLine[DelayLinePosition] = inputSample;
		    double result = 0.0;
		    int index = DelayLinePosition;
            for (int i = 0; i < Coefficients.Length; i++) 
		    {
			    result += Coefficients[i] * DelayLine[index--];
			    if (index < 0)
                    index = Coefficients.Length - 1;
		    }
            if (++DelayLinePosition >= Coefficients.Length)
			    DelayLinePosition = 0;
		    return result;
	    }
    }
}
