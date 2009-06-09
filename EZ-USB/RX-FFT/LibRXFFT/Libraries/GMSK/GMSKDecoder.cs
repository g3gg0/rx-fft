using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GSM;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.GMSK
{
    public enum eDecodeMode
    {
        Simple,
        Pattern,
        MLSE
    }

    public class GMSKDecoder
    {
        public double MinPowerFact = 0.20;

        public eDecodeMode Mode = eDecodeMode.MLSE;
        public int StartOffset = 0;
        private double Oversampling;
        private double[][] sampleSnaps;
        private SequenceGenerator generator;

        private static bool[][] sequenceTable;
        public double DCOffsetFact;

        public GMSKDecoder(double oversampling, double BT)
        {
            Oversampling = oversampling;
            int snapStart = (int)(Oversampling * 2);
            int snapLen = (int)(Oversampling * 1);

            generator = new SequenceGenerator(Oversampling, BT);
            SeqTableInit();

            sampleSnaps = new double[32][];
            byte[] token = new byte[1];

            for (int pos = 0; pos < 32; pos++)
            {
                token[0] = (byte)(pos << 3);
                double[] snap = generator.GenerateDiffEncoded(token);

                sampleSnaps[pos] = new double[snapLen];
                Array.Copy(snap, snapStart, sampleSnaps[pos], 0, snapLen);
            }

        }

        private void SeqTableInit()
        {
            sequenceTable = new bool[2][];
            sequenceTable[0] = new bool[2];
            sequenceTable[1] = new bool[2];

            /* this table defines, which bits are most likely when the bits before 
             * appeared and the current value is undeterminable
             */
            sequenceTable[0][0] = true;
            sequenceTable[0][1] = false;
            sequenceTable[1][0] = true;
            sequenceTable[1][1] = false;
        }

        private bool MostLikely(bool bit1, bool bit2)
        {
            int pos1 = bit1 ? 1 : 0;
            int pos2 = bit2 ? 1 : 0;

            return sequenceTable[pos1][pos2];
        }

        public bool[] Decode(double[] srcData)
        {
            return Decode(srcData, null);
        }

        public bool[] Decode(double[] srcData, bool[] dstData)
        {
            if (dstData == null)
                dstData = new bool[148];

            if (Mode == eDecodeMode.Pattern)
            {
                int currentBit = 3;

                dstData[0] = true;
                dstData[1] = true;
                dstData[2] = true;

                while (currentBit < 148)
                {
                    bool bit = FindBit(srcData, dstData, currentBit);

                    dstData[currentBit++] = bit;
                }

                return dstData;
            }

            if (Mode == eDecodeMode.Simple)
            {
                for (int currentbit = 0; currentbit < 148; currentbit++)
                {
                    double sampleValue = srcData[(int)(StartOffset + (currentbit + 0.5f) * Oversampling)];

                    if (sampleValue > 0)
                        dstData[currentbit] = true;
                    else
                        dstData[currentbit] = false;
                }

                return dstData;
            }

            if (Mode == eDecodeMode.MLSE)
            {
                for (int currentBit = 0; currentBit < Burst.LeadingTailBits; currentBit++)
                    dstData[currentBit] = true;

                double maxPower = SignalPower.Average(srcData, StartOffset, (int)((Burst.LeadingTailBits - 1) * Oversampling));
                double decisionPower = maxPower * MinPowerFact;

                for (int currentBit = (int)Burst.LeadingTailBits-1; currentBit < Burst.NetBitCount; currentBit++)
                {
                    int samplePos = (int)(StartOffset + (currentBit + 0.5f) * Oversampling);
                    double sampleValue = srcData[samplePos] - (maxPower*DCOffsetFact);

                    if (sampleValue > decisionPower)
                        dstData[currentBit] = true;
                    else if (sampleValue < -decisionPower)
                        dstData[currentBit] = false;
                    else
                    {
                        bool bit1 = dstData[currentBit - 2];
                        bool bit2 = dstData[currentBit - 1];

                        dstData[currentBit] = MostLikely(bit1, bit2);
                    }
                }

                return dstData;
            }


            return null;
        }

        private bool FindBit(double[] srcData, bool[] bits, int bitNum)
        {
            double maxStrength = double.MinValue;
            int maxPos = -1;
            int startPos = 0;

            if (bits[bitNum - 2])
                startPos |= 16;
            if (bits[bitNum - 1])
                startPos |= 8;

            for (int pos = 0; pos < 7; pos++)
            {
                double[] snap = sampleSnaps[startPos + pos];
                int samplePosition = (int)(StartOffset + bitNum * Oversampling);

                double strength = SignalPower.ProcessDiff(srcData, samplePosition, snap);
                //Console.Out.WriteLine("pos: " + pos + " strength: " + strength);

                if (strength > maxStrength)
                {
                    maxStrength = strength;
                    maxPos = pos;
                }
            }

            return maxPos > 3;
        }

    }
}
