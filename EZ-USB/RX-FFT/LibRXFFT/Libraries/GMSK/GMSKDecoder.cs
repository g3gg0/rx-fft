using System;
using LibRXFFT.Libraries.GSM.Bursts;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.GMSK
{
    public enum eInterpolation
    {
        Automatic,
        None,
        Linear,
        SinX
    }

    public class GMSKDecoder
    {
        public int StartOffset = 0;
        public double SubSampleOffset;

        public eInterpolation Interpolation = eInterpolation.Automatic;

        public static double MinPowerFact = 0.1;
        private readonly double Oversampling;
        public double DecisionPower = 0;
        public double MaxPower = 0;
        private long BurstsProcessed;


        public GMSKDecoder(double oversampling, double BT)
        {
            Oversampling = oversampling;
        }

        public void Reset()
        {
            BurstsProcessed = 0;
            DecisionPower = 0.0f;
        }

        public bool[] Decode(double[] srcData)
        {
            return Decode(srcData, null);
        }

        public bool[] Decode(double[] srcData, bool[] dstData)
        {
            bool interpolate;

            switch (Interpolation)
            {
                case eInterpolation.Automatic:
                    if (Oversampling < 4)
                        interpolate = true;
                    else
                        interpolate = false;
                    break;

                case eInterpolation.None:
                    interpolate = false;
                    break;

                default:
                    interpolate = true;
                    break;
            }

            if (dstData == null)
                dstData = new bool[148];

            /* the first bit gets set to true since we start at bit 1 */
            dstData[0] = true;

            /* find the highest amplitude over some bits (start at bit 1 and use 5 bits) */
            if (DecisionPower == 0 || BurstsProcessed >= 8)
            {
                int firstBits = (int)(5 * Oversampling);

                BurstsProcessed = 0;
                MaxPower = SignalPower.Max(srcData, (int)(StartOffset + SubSampleOffset), firstBits);

                /* build an average over the last max-levels */
                if (DecisionPower == 0)
                    DecisionPower = MaxPower * MinPowerFact;
                else
                    DecisionPower = (5*DecisionPower + (MaxPower * MinPowerFact)) / 6;
            }

            for (int currentBit = 1; currentBit < Burst.NetBitCount; currentBit++)
            {
                double samplePos = StartOffset + SubSampleOffset + (currentBit + 0.5f) * Oversampling;
                double sampleValue;

                if (interpolate)
                {
                    switch (Interpolation)
                    {
                        case eInterpolation.Automatic:
                        case eInterpolation.Linear:
                            {
                                double delta = samplePos - Math.Floor(samplePos);
                                int samplePos1 = (int)samplePos;
                                int samplePos2 = (int)samplePos + 1;

                                double sampleValue1;
                                double sampleValue2;

                                if (samplePos1 >= 0 && samplePos1 < srcData.Length)
                                    sampleValue1 = srcData[samplePos1];
                                else
                                    sampleValue1 = 0;

                                if (samplePos2 >= 0 && samplePos2 < srcData.Length)
                                    sampleValue2 = srcData[samplePos2];
                                else
                                    sampleValue2 = sampleValue1;


                                sampleValue = sampleValue1 * (1 - delta) + sampleValue2 * delta;
                            }
                            break;

                        case eInterpolation.SinX:
                            {
                                double delta = samplePos - Math.Floor(samplePos);
                                int samplePos1 = (int)samplePos;
                                int samplePos2 = (int)samplePos + 1;

                                double sampleValue1;
                                double sampleValue2;

                                if (samplePos1 >= 0 && samplePos1 < srcData.Length)
                                    sampleValue1 = srcData[samplePos1];
                                else
                                    sampleValue1 = 0;

                                if (samplePos2 >= 0 && samplePos2 < srcData.Length)
                                    sampleValue2 = srcData[samplePos2];
                                else
                                    sampleValue2 = sampleValue1;

                                /* is that correct? */
                                sampleValue = sampleValue1 * Math.Sin(delta) / delta + sampleValue2 * Math.Sin(1 - delta) / (1 - delta);
                            }
                            break;

                        default:
                            sampleValue = 0;
                            break;
                    }
                }
                else
                {
                    if (samplePos >= 0 && samplePos < srcData.Length)
                        sampleValue = srcData[(int)samplePos];
                    else
                        sampleValue = 0;
                }


                /* if the bit is undeterminable, then it is the inverse of the last bit */
                if (sampleValue > DecisionPower)
                    dstData[currentBit] = true;
                else if (sampleValue < -DecisionPower)
                    dstData[currentBit] = false;
                else
                    dstData[currentBit] = !dstData[currentBit - 1];
            }

            BurstsProcessed++;
            return dstData;

        }



    }
}
