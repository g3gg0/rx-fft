using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.ShmemChain;

namespace LibRXFFT.Libraries.GMSK
{
    public class GMSKDemodulator
    {
        private double lastPhase;
        private double totalPhase;
        private double averageStrength;

        public double[] ProcessData(byte[] dataBuffer, double[] sampleArray, double[] strengthArray)
        {
            return ProcessData(dataBuffer, sampleArray, strengthArray, null);
        }

        public double[] ProcessData(byte[] dataBuffer)
        {
            return ProcessData(dataBuffer, null, null, null);
        }

        public double[] ProcessData(byte[] dataBuffer, double[] sampleArray, double[] strengthArray, SharedMem outChan)
        {
            byte[] outBuffer = null;
            int outBufferPos = 0;
            int samplePos = 0;

            /* if caller provided no array, create one */
            if (sampleArray == null)
                sampleArray = new double[dataBuffer.Length / 4];

            if (outChan != null)
                outBuffer = new byte[dataBuffer.Length];

            const int bytePerSamplePair = 4;

            for (int pos = 0; pos < dataBuffer.Length / bytePerSamplePair; pos++)
            {
                double I = ByteUtil.getDoubleFromBytes(dataBuffer, bytePerSamplePair * pos);
                double Q = ByteUtil.getDoubleFromBytes(dataBuffer, bytePerSamplePair * pos + 2);

                double phase = Math.Atan2(I, Q);
                double strength =  10 * Math.Sqrt(I * I + Q * Q);

                /* dont use samples for averaging which strengths are far below the average. 
                 * lets use a 20% limit 
                 */
                if (strength > averageStrength / 5)
                {
                    /* the first time, just set the average to current strength */
                    if (averageStrength > 0)
                    {
                        averageStrength *= 99;
                        averageStrength += strength;
                        averageStrength /= 100;
                    }
                    else
                        averageStrength = strength;
                }

                while (phase - lastPhase < -(Math.PI / 2))
                    phase += Math.PI;

                while (phase - lastPhase > Math.PI / 2)
                    phase -= Math.PI;

                double diff = phase - lastPhase;

                strengthArray[samplePos] = strength;
                sampleArray[samplePos++] = diff;

                if (outBuffer != null)
                {
                    ByteUtil.putBytesFromDouble(outBuffer, 2 * (outBufferPos++), diff);
                    ByteUtil.putBytesFromDouble(outBuffer, 2 * (outBufferPos++), strength);
                }

                totalPhase += diff;
                lastPhase = phase % (2 * Math.PI);
            }

            if (outChan != null)
                outChan.Write(outBuffer, 0, outBufferPos * 2);

            return sampleArray;
        }

    }
}
