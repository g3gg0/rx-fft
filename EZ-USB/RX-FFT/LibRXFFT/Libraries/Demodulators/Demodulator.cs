using System;

namespace LibRXFFT.Libraries.Demodulators
{
    public class Demodulator
    {
        public string Description = "TemplateDemod";
        public string ShortDescription = "TemplateDemod";
        public static bool UseNative = true;
        protected IntPtr NativeContext;

        public virtual double[] ProcessData(byte[] inBuffer, double[] outData)
        {
            const int bytePerSample = 2;
            const int channels = 2;

            int samplePairs = inBuffer.Length / (channels * bytePerSample);
            if (outData == null)
                outData = new double[samplePairs];

            for (int samplePair = 0; samplePair < samplePairs; samplePair++)
            {
                int samplePairPos = samplePair * bytePerSample * channels;
                double I = ByteUtil.getDoubleFromBytes(inBuffer, samplePairPos);
                double Q = ByteUtil.getDoubleFromBytes(inBuffer, samplePairPos + bytePerSample);

                outData[samplePair] = ProcessSample(I, Q);
            }

            return outData;
        }

        public virtual void Dispose()
        {
        }

        public virtual double[] ProcessDataNative(double[] iDataIn, double[] qDataIn, double[] outData)
        {
            return null;
        }

        public virtual double[] ProcessData(double[] iDataIn, double[] qDataIn, double[] outData)
        {
            if (outData == null)
                outData = new double[iDataIn.Length];

            if (UseNative && NativeContext != IntPtr.Zero)
            {
                return ProcessDataNative(iDataIn, qDataIn, outData);
            }
            else
            {
                for (int samplePair = 0; samplePair < iDataIn.Length; samplePair++)
                {
                    double I = iDataIn[samplePair];
                    double Q = qDataIn[samplePair];

                    outData[samplePair] = ProcessSample(I, Q);
                }
            }

            return outData;
        }

        public virtual double ProcessSample(double iData, double qData)
        {
            return 0;
        }

    }
}
