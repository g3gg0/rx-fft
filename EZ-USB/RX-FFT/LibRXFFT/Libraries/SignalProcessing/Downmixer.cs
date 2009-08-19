using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class Downmixer
    {
        public static bool UseNative = false;
        protected IntPtr NativeContext;

        protected int TimePos = 0;
        protected double[] CosTable;
        protected double[] SinTable;
        protected double _TimeStep = 0;
        protected Object TableLock = new Object();

        public double TimeStep
        {
            get { return _TimeStep; }
            set
            {
                _TimeStep = value;
                int pos = 0;
                double timePos = 0;

                do
                {
                    pos++;
                    timePos += TimeStep;
                    timePos %= 2 * Math.PI;
                } while (Math.Abs(timePos) > (0.005f * Math.Sqrt(pos)));

                lock (TableLock)
                {
                    TimePos = 0;
                    CosTable = new double[pos];
                    SinTable = new double[pos];

                    timePos = 0;
                    for (pos = 0; pos < CosTable.Length; pos++)
                    {
                        timePos += TimeStep;
                        timePos %= 2 * Math.PI;
                        CosTable[pos] = Math.Cos(timePos);
                        SinTable[pos] = Math.Sin(timePos);
                    }

                    if (NativeContext != IntPtr.Zero)
                    {
                        DownmixFree(NativeContext);
                    }
                    NativeContext = DownmixInit(CosTable, SinTable, CosTable.Length);
                }
            }
        }

        [DllImport("libRXFFT_native.dll", EntryPoint = "DownmixInit")]
        public static unsafe extern IntPtr DownmixInit(double[] cosTable, double[] sinTable, int entries);
        [DllImport("libRXFFT_native.dll", EntryPoint = "DownmixProcess")]
        public static unsafe extern void DownmixProcess(IntPtr ctx, double[] iDataIn, double[] qDataIn, double[] iDataOut, double[] qDataOut, int samples);
        [DllImport("libRXFFT_native.dll", EntryPoint = "DownmixFree")]
        public static unsafe extern void DownmixFree(IntPtr ctx);




        public Downmixer()
        {
            /* will allocate native context */
            TimeStep = 0.5f;
        }

        public void Dispose()
        {
            if (NativeContext != IntPtr.Zero)
            {
                DownmixFree(NativeContext);
                NativeContext = IntPtr.Zero;
            }
        }

        public virtual void ProcessData(double[] iDataIn, double[] qDataIn, double[] iDataOut, double[] qDataOut)
        {
            lock (TableLock)
            {
                if (UseNative && NativeContext != IntPtr.Zero)
                {
                    DownmixProcess(NativeContext, iDataIn, qDataIn, iDataOut, qDataOut, iDataIn.Length);
                }
                else
                {
                    for (int sample = 0; sample < iDataIn.Length; sample++)
                    {
                        double I = iDataIn[sample];
                        double Q = qDataIn[sample];

                        iDataOut[sample] = CosTable[TimePos] * I - SinTable[TimePos] * Q;
                        qDataOut[sample] = CosTable[TimePos] * Q + SinTable[TimePos] * I;

                        TimePos++;
                        TimePos %= CosTable.Length;
                    }
                }
            }
        }
    }
}
