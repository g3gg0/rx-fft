﻿using System;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.Demodulators
{
    public class AMDemodulator : Demodulator
    {
        [DllImport("LibRXFFT_native.dll", EntryPoint = "AMDemodInit", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern IntPtr AMDemodInit();
        [DllImport("LibRXFFT_native.dll", EntryPoint = "AMDemodProcess", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void AMDemodProcess(IntPtr ctx, double[] inDataI, double[] inDataQ, double[] outData, int entries);
        [DllImport("LibRXFFT_native.dll", EntryPoint = "AMDemodFree", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void AMDemodFree(IntPtr ctx);

        public AMDemodulator()
        {
            Description = "Simple AM Demodulator";
            ShortDescription = "AM";
            try
            {
                NativeContext = AMDemodInit();
            }
            catch (Exception e)
            {
            }
        }

        public override void Dispose()
        {
            if (NativeContext != IntPtr.Zero)
            {
                AMDemodFree(NativeContext);
                NativeContext = IntPtr.Zero;
            }
        }

        public override double[] ProcessDataNative(double[] iDataIn, double[] qDataIn, double[] outData)
        {
            if (NativeContext != IntPtr.Zero)
            {
                AMDemodProcess(NativeContext, iDataIn, qDataIn, outData, iDataIn.Length);
            }
            return outData;
        }

        public override double ProcessSample(double iData, double qData)
        {
            return Math.Sqrt((iData * iData) + (qData * qData));
        }
    }
}
