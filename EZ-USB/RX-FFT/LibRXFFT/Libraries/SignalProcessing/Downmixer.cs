using System;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class Downmixer
    {
        public static bool UseNative = true;
        protected IntPtr NativeContext;

        protected int TimePos = 0;
        protected int TableLength = 8192;
        protected double[] CosTable = new double[8192];
        protected double[] SinTable = new double[8192];
        protected double _TimeStep = 0;
        protected double _SamplingRate = 0;

        protected bool TableCalculated = false;
        protected double TableLeastDiff = 0.0f;
        protected int TableLeastDiffPos = 0;


        protected Object TableLock = new Object();

        public static double MaxDifference = 0.001f;
        public static bool Precalculate = false;

        public double SamplingRate
        {
            get { return _SamplingRate; }
            set
            {
                _SamplingRate = value;
                Init();
            }
        }

        public double TimeStep
        {
            get { return _TimeStep; }
            set
            {
                _TimeStep = value;
                Init();
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
            SamplingRate = 44100;
        }

        public void Init()
        {
            int length = (int)(SamplingRate / 2);

            if (SamplingRate == 0)
            {
                length = 8192;
            }

            Array.Resize<double>(ref CosTable, length);
            Array.Resize<double>(ref SinTable, length);

            /* old way of calculation */
            if (Precalculate)
            {
                int pos = 0;
                double timePos = 0;

                /* search for the best precalc length */
                do
                {
                    pos++;
                    timePos += TimeStep;
                    timePos %= 2 * Math.PI;
                } while (Math.Abs(timePos) > (MaxDifference * Math.Sqrt(pos)));

                lock (TableLock)
                {
                    TimePos = 0;
                    TableLength = pos;

                    if (TableLength > CosTable.Length)
                    {
                        Array.Resize<double>(ref CosTable, TableLength);
                        Array.Resize<double>(ref SinTable, TableLength);
                    }

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
                    try
                    {
                        NativeContext = DownmixInit(CosTable, SinTable, CosTable.Length);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            else
            {
                lock (TableLock)
                {
                    TimePos = 0;
                    TableLength = 0;
                    TableCalculated = false;
                    TableLeastDiff = double.MaxValue;
                }
            }
        }

        public void Dispose()
        {
            if (NativeContext != IntPtr.Zero)
            {
                DownmixFree(NativeContext);
                NativeContext = IntPtr.Zero;
            }
        }

        public virtual void ProcessData(double[] iDataIn, double[] qDataIn)
        {
            ProcessData(iDataIn, qDataIn, iDataIn, qDataIn);
        }

        public virtual void ProcessData(double[] iDataIn, double[] qDataIn, double[] iDataOut, double[] qDataOut)
        {
            lock (TableLock)
            {
                if (UseNative && NativeContext != IntPtr.Zero && Precalculate)
                {
                    DownmixProcess(NativeContext, iDataIn, qDataIn, iDataOut, qDataOut, iDataIn.Length);
                }
                else
                {
                    if (Precalculate)
                    {
                        for (int sample = 0; sample < iDataIn.Length; sample++)
                        {
                            double I = iDataIn[sample];
                            double Q = qDataIn[sample];

                            iDataOut[sample] = CosTable[TimePos] * I - SinTable[TimePos] * Q;
                            qDataOut[sample] = CosTable[TimePos] * Q + SinTable[TimePos] * I;

                            TimePos++;
                            TimePos %= TableLength;
                        }
                    }
                    else
                    {
                        double sin;
                        double cos;

                        for (int sample = 0; sample < iDataIn.Length; sample++)
                        {
                            double I = iDataIn[sample];
                            double Q = qDataIn[sample];

                            if (!TableCalculated)
                            {
                                double timePos = (TimePos * TimeStep) % (2 * Math.PI);

                                sin = Math.Sin(timePos);
                                cos = Math.Cos(timePos);

                                SinTable[TimePos] = sin;
                                CosTable[TimePos] = cos;

                                /* are we near zero? */
                                if (TimePos > 0 && Math.Abs(timePos) < TableLeastDiff)
                                {
                                    TableLeastDiff = Math.Abs(timePos);
                                    TableLeastDiffPos = TimePos;
                                }

                                TimePos++;
                                /* buffer is full, take the ideal cut point */
                                if (TimePos >= CosTable.Length)
                                {
                                    TableLength = TableLeastDiffPos;
                                    TimePos %= TableLength;
                                    TableCalculated = true;
                                }
                            }
                            else
                            {
                                sin = SinTable[TimePos];
                                cos = CosTable[TimePos];

                                TimePos++;
                                TimePos %= TableLength;
                            }

                            iDataOut[sample] = cos * I - sin * Q;
                            qDataOut[sample] = cos * Q + sin * I;
                        }
                    }
                }
            }
        }
    }
}
