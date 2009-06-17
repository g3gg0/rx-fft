using System;
using LibRXFFT.Libraries.ShmemChain;

namespace LibRXFFT.Libraries.GMSK
{
    public enum eDataFormat
    {
        Direct16BitIQFixedPoint,
        Direct64BitIQFloat,
        Direct64BitIQFloat64k
    }

    public class GMSKDemodulator
    {
        private double LastPhase;
        public static bool UseFastAtan2 = true;
        public static bool InvertedSpectrum = false;
        public int BytesPerSamplePair;
        public int BytesPerSample;
        private eDataFormat _DataFormat;

        private SharedMem ShmemOutChan;

        public eDataFormat DataFormat
        {
            get { return _DataFormat; }
            set
            {
                _DataFormat = value;

                switch (value)
                {
                    case eDataFormat.Direct16BitIQFixedPoint:
                        BytesPerSamplePair = 4;
                        BytesPerSample = 2;
                        break;

                    case eDataFormat.Direct64BitIQFloat:
                    case eDataFormat.Direct64BitIQFloat64k:
                        BytesPerSamplePair = 8;
                        BytesPerSample = 4;
                        break;

                    default:
                        BytesPerSamplePair = 0;
                        BytesPerSample = 0;
                        break;
                }
            }
        }

        public GMSKDemodulator()
        {
            DataFormat = eDataFormat.Direct16BitIQFixedPoint;
            //ShmemOutChan = new SharedMem(-2, -1, "GMSKDemod");

        }

        private double FastAtan2b(double y, double x)
        {
            const double ONEQTR_PI = Math.PI / 4.0f;
            const double THRQTR_PI = 3.0f * Math.PI / 4.0f;
            double r;
            double angle;
            double abs_y = Math.Abs(y);

            if (x < 0.0f)
            {
                r = (x + abs_y) / (abs_y - x);
                angle = THRQTR_PI;
            }
            else
            {
                r = (x - abs_y) / (x + abs_y);
                angle = ONEQTR_PI;
            }

            angle += (0.1963f * r * r - 0.9817f) * r;

            return y < 0.0f ? -angle : angle;
        }


        public double[] ProcessData(byte[] dataBuffer, double[] sampleArray, double[] strengthArray)
        {
            return ProcessData(dataBuffer, dataBuffer.Length, sampleArray, strengthArray);
        }

        public double[] ProcessData(byte[] dataBuffer)
        {
            return ProcessData(dataBuffer, dataBuffer.Length, null, null);
        }

        public double[] ProcessData(byte[] dataBuffer, int bytesRead, double[] sampleArray, double[] strengthArray)
        {
            byte[] outBuffer = null;
            int outBufferPos = 0;
            int samplePos = 0;
            int samplePairs = bytesRead/BytesPerSamplePair;

            /* if caller provided no array, create one */
            if (sampleArray == null)
                sampleArray = new double[samplePairs];

            /* shmem output buffer */
            if (ShmemOutChan != null)
                outBuffer = new byte[samplePairs * 4];

            for (int pos = 0; pos < samplePairs; pos++)
            {
                double I;
                double Q;
                switch (DataFormat)
                {
                    case eDataFormat.Direct16BitIQFixedPoint:
                        I = ByteUtil.getDoubleFromBytes(dataBuffer, BytesPerSamplePair * pos);
                        Q = ByteUtil.getDoubleFromBytes(dataBuffer, BytesPerSamplePair * pos + BytesPerSample);
                        break;

                    case eDataFormat.Direct64BitIQFloat64k:
                        I = BitConverter.ToSingle(dataBuffer, BytesPerSamplePair * pos) / (65536*2);
                        Q = BitConverter.ToSingle(dataBuffer, BytesPerSamplePair * pos + BytesPerSample) / (65536 * 2);
                        break;

                    case eDataFormat.Direct64BitIQFloat:
                        I = BitConverter.ToSingle(dataBuffer, BytesPerSamplePair * pos);
                        Q = BitConverter.ToSingle(dataBuffer, BytesPerSamplePair * pos + BytesPerSample);
                        break;

                    default:
                        return null;
                }

                if (InvertedSpectrum)
                    I = -I;

                /* 
                 * this strength calculation is incorrect!
                 * the Math.Sqrt() is missing, but that would consume
                 * too much CPU power. 
                 * Thats okay since the strength is just used qualitative
                 */
                double strength = 100 * (I * I + Q * Q);
                double phase;

                phase = UseFastAtan2 ? FastAtan2b(I, Q) : Math.Atan2(I, Q);

                while (phase - LastPhase < -(Math.PI / 2))
                    phase += Math.PI;

                while (phase - LastPhase > Math.PI / 2)
                    phase -= Math.PI;

                /* catch the case where I and Q are zero */
                if (double.IsNaN(phase))
                    phase = LastPhase;

                double diff = phase - LastPhase;

                strengthArray[samplePos] = strength;
                sampleArray[samplePos++] = diff;

                if (outBuffer != null)
                {
                    /*
                    ByteUtil.putBytesFromDouble(outBuffer, 2 * (outBufferPos++), I);
                    ByteUtil.putBytesFromDouble(outBuffer, 2 * (outBufferPos++), Q);
                    */
                    ByteUtil.putBytesFromDouble(outBuffer, 2 * (outBufferPos++), diff * strength);
                    ByteUtil.putBytesFromDouble(outBuffer, 2 * (outBufferPos++), strength);
                }

                LastPhase = phase % (2 * Math.PI);
            }

            if (ShmemOutChan != null)
                ShmemOutChan.Write(outBuffer, 0, outBuffer.Length);

            return sampleArray;
        }

    }
}
