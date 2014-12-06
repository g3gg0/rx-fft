using DemodulatorCollection.Interfaces;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.SignalProcessing;
using RX_FFT.Components.GDI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemodulatorCollection.Demodulators
{
    class PPMDemodulator : DigitalDemodulator
    {
        enum eLearningState
        {
            BackgroundNoise,
            Synchronizing,
            DataStream,
            Init
        }
        private bool[] Preamble = new bool[] { false, false, false, false, true, false, true, false, false, false, false, true, false, true, false, false, false, false, false, false };
        private double[] PreambleSignalBuffer = new double[0];
        private double[] BitGroupSignalBuffer = new double[0];
        private double[] WholeSignalBuffer = new double[0];
        private double[] BestMatchBuffer = new double[0];
        private double DetectionLevel = 0.9;
        private int MinDbDistance = 5;
        private double DecisionLevel = 0;
        private double DecisionDistance = 0;
        private double NoiseFloor;
        private double SampleNum;
        private double NoiseFloorInteg;
        private int SameBits;
        private bool LastBit;
        private int IntegratedSignalCount;
        private int WholeSignalBufferPos = 0;

        private eLearningState State = eLearningState.BackgroundNoise;

        private double IntegratedSignal = 0;
        private int DataBitSample = 0;

        private int DataBitNumber
        {
            get
            {
                return (int)(Math.Round((decimal)DataBitSample / SamplesPerBit));
            }
        }


        private decimal SamplesPerBit
        {
            get
            {
                return (decimal)SamplingRate * PulseLength;
            }
        }

        public decimal _PulseLength = 0.0000005m;
        public decimal PulseLength
        {
            get
            {
                return _PulseLength;
            }
            set
            {
                _PulseLength = value;

                UpdateBuffers();
            }
        }

        private void UpdateBuffers()
        {
            PreambleSignalBuffer = new double[(int)(SamplesPerBit * (decimal)Preamble.Length)];
            BitGroupSignalBuffer = new double[(int)(SamplesPerBit * 2m)];
            WholeSignalBuffer = new double[(int)(SamplesPerBit * 250m)];
        }

        public BitClockSink BitSink
        {
            get;
            set;
        }

        public double _SamplingRate = 0;
        private int RisingEdgeOffset = 0;
        public double SamplingRate
        {
            get
            {
                return _SamplingRate;
            }
            set
            {
                if (SamplingRate != value)
                {
                    Log.AddMessage("PPMDemodulator", "Initializing Demodulator");
                    _SamplingRate = value;
                    Init();
                }
            }
        }

        public void Init()
        {
            State = eLearningState.Init;
        }

        public void Process(double iValue, double qValue)
        {
            double power = /*Math.Sqrt*/(iValue * iValue + qValue * qValue);

            Process(power);
        }

        double prev = 0;

        private void Process(double power)
        {
            switch(State)
            {
                case eLearningState.Init:
                    if (SamplingRate == 0)
                    {
                        break;
                    }
                    UpdateBuffers();

                    Plot.RealTimeMode = true;
                    Plot.MaxSamples = PreambleSignalBuffer.Length;
                    Plot.LabelledHorLines.Clear();
                    Plot.LabelledHorLines.AddLast(new LabelledLine("Decision", 0, Color.Red));
                    Plot.LabelledVertLines.Clear();

                    NoiseFloorInteg = 0;
                    SampleNum = 0;

                    Log.AddMessage("PPMDemodulator", "Enter background noise learning mode...");
                    State = eLearningState.BackgroundNoise;
                    break;

                case eLearningState.BackgroundNoise:
                    NoiseFloorInteg += power;
                    SampleNum++;

                    if (SampleNum > 5 * SamplingRate)
                    {
                        NoiseFloor = DBTools.SquaredSampleTodB(NoiseFloorInteg / SampleNum);
                        Log.AddMessage("Learned noise level (" + NoiseFloor.ToString("0.00") + " dB)");
                        State = eLearningState.Synchronizing;
                    }
                    break;

                case eLearningState.Synchronizing:
                    Array.Copy(PreambleSignalBuffer, 1, PreambleSignalBuffer, 0, PreambleSignalBuffer.Length - 1);

                    PreambleSignalBuffer[PreambleSignalBuffer.Length - 1] = power * prev * 1000;
                    prev = power;

                    double match = CalcMatch();

                    if (match > DetectionLevel)
                    {
                        DetermineLevels();

                        IntegratedSignal = 0;
                        IntegratedSignalCount = 0;
                        DataBitSample = 0;
                        WholeSignalBufferPos = 0;

                        SameBits = 0;
                        LastBit = false;

                        Array.Copy(PreambleSignalBuffer, WholeSignalBuffer, PreambleSignalBuffer.Length);
                        WholeSignalBufferPos = PreambleSignalBuffer.Length;

                        Plot.LabelledVertLines.Clear();
                        Plot.LabelledVertLines.AddLast(new LabelledLine("" + DataBitNumber, WholeSignalBufferPos, Color.Yellow));

                        IntegratedSignal += power * prev * 1000;
                        IntegratedSignalCount++;
                        IntegratedSignal += power * prev * 1000;
                        IntegratedSignalCount++;

                        BitSink.TransmissionStart();
                        State = eLearningState.DataStream;
                    }
                    break;

                case eLearningState.DataStream:

#if false
                    BitGroupSignalBuffer[BitPos] = power;
                    BitPos++;
                    if(BitPos >= BitGroupSignalBuffer.Length)
                    {

                    }
#else
                    if (WholeSignalBufferPos < WholeSignalBuffer.Length)
                    {
                        WholeSignalBuffer[WholeSignalBufferPos++] = power * prev * 1000;
                    }
               
                    int prevBit = DataBitNumber;
                    IntegratedSignal += power * prev * 1000;
                    IntegratedSignalCount++;
                    DataBitSample++;

                    prev = power;

                    /* the current sample is within the next bit already */
                    if(DataBitNumber != prevBit)
                    {
                        Plot.LabelledVertLines.AddLast(new LabelledLine("" + DataBitNumber, WholeSignalBufferPos, Color.Green));

                        bool bit = false;
                        double level = IntegratedSignal / IntegratedSignalCount;

                        if (level > DecisionLevel)
                        {
                            bit = true;
                        }

                        if(LastBit == bit)
                        {
                            SameBits++;
                            if (SameBits >= 2)
                            {
                                DumpWholeSignalBuffer();
                                BitSink.TransmissionEnd();
                                SameBits = 0;

                                State = eLearningState.Synchronizing;
                            }
                        }
                        else
                        {
                            SameBits = 0;
                        }

                        BitSink.ClockBit(bit);
                        LastBit = bit;

                        IntegratedSignal = 0;
                        IntegratedSignalCount = 0;

                    }

                    if(DataBitNumber >= 2 * 112)
                    {
                        DumpWholeSignalBuffer();

                        BitSink.TransmissionEnd();
                        State = eLearningState.Synchronizing;
                    }
#endif
                    break;
            }
        }

        private void DumpWholeSignalBuffer()
        {
            while (WholeSignalBufferPos < WholeSignalBuffer.Length)
            {
                WholeSignalBuffer[WholeSignalBufferPos++] = 0;
            }
            /*
            double prev = WholeSignalBuffer[0];
            for (int pos = 0; pos < WholeSignalBuffer.Length; pos++ )
            {
                double next = WholeSignalBuffer[pos];
                WholeSignalBuffer[pos] = WholeSignalBuffer[pos] * prev * 1000;
                prev = next;
            }
             * */
            Plot.MaxSamples = WholeSignalBuffer.Length;
            Plot.LabelledHorLines.First().Position = DecisionLevel;
            Plot.ProcessData(WholeSignalBuffer);
            Plot.NeedsRender = true;
            Plot.NeedsUpdate = true;
        }

        private void DetermineLevels()
        {
            int highBits = 0;
            int lowBits = 0;
            double highBitLevel = 0;
            double lowBitLevel = 0;

            for (int pos = 0; pos < PreambleSignalBuffer.Length; pos++)
            {
                double value = PreambleSignalBuffer[pos];
                int bit = (int)((decimal)pos / SamplesPerBit);

                if (Preamble[bit])
                {
                    highBits++;
                    highBitLevel += value;
                }
                else
                {
                    lowBits++;
                    lowBitLevel += value;
                }
            }

            highBitLevel /= highBits;
            lowBitLevel /= lowBits;

            DecisionDistance = highBitLevel - lowBitLevel;
            DecisionLevel = lowBitLevel + DecisionDistance / 2;
        }

        private void DumpSignalBuffer()
        {
            Plot.ProcessData(PreambleSignalBuffer);
            Plot.LabelledHorLines.First().Position = DecisionLevel;
            Plot.NeedsRender = true;
            Plot.NeedsUpdate = true;
        }

        private double CalcMatch()
        {
            return CalcMatch(0);
            /*
            double[] matchLevel = new double[3];
            int center = (matchLevel.Length - 1) / 2;

            for(int offset = 0; offset < matchLevel.Length; offset++)
            {
                matchLevel[offset] = CalcMatch(offset - center);
            }

            double max = double.MinValue;
            Array.ForEach<double>(matchLevel, (v) => max = Math.Max(v, max));

            if (max == matchLevel[center])
            {
                return max;
            }
            
            return 0;
            */
        }

        private double CalcMatch(int offset)
        {
            if(!HasRisingEdge())
            {
                //return 0;
            }

            double match = 0;
            double possibleMatch = 0;
            double max = 0;
            double[] integs = new double[Preamble.Length];

            decimal spb = SamplesPerBit;

            /* build integrals */
            for (int bit = 1; bit < Preamble.Length - 1; bit++)
            {
                int bitPos = (int)(bit * spb);
                int bitPosEnd = (int)((bit + 1) * spb);
                int bits = (bitPosEnd - bitPos);

                integs[bit] = 0.0f;
                for (int pos = bitPos; pos < bitPosEnd; pos++)
                {
                    integs[bit] += PreambleSignalBuffer[pos + offset];
                }
                integs[bit] /= bits;
                max = Math.Max(max, integs[bit]);
            }

            double strength = DBTools.SquaredSampleTodB(max);
            double snr = strength - NoiseFloor;
            if (snr < MinDbDistance)
            {
                return 0;
            }

            /* calc match */
            for (int bit = 0; bit < Preamble.Length; bit++)
            {
                double bitInteg = integs[bit] / max;
                double dest = Preamble[bit] ? 1.0 : 0.0;
                double delta = Math.Abs(bitInteg - dest);

                match += (1 - delta);
                possibleMatch += 1;
            }

            return match / possibleMatch;
        }

        private bool HasRisingEdge()
        {
            int firstBit = 0;

            for (int pos = 0; pos < Preamble.Length; pos++)
            {
                if (Preamble[pos])
                {
                    firstBit = pos;
                    break;
                }
            }

            int samplePos = (int)((decimal)firstBit * SamplesPerBit);

            double avg = CalcAverage(PreambleSignalBuffer, 0, samplePos);
            double delta = CalcDelta(PreambleSignalBuffer, 0, samplePos);

            double edgeDelta = PreambleSignalBuffer[samplePos + RisingEdgeOffset] - avg;

            if(edgeDelta > 4 * delta)
            {
                return true;
            }

            return false;
        }

        private double CalcDelta(double[] buffer, int startPos, int entries)
        {
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            for (int pos = startPos; pos < startPos + entries; pos++)
            {
                minValue = Math.Min(minValue, buffer[pos]);
                maxValue = Math.Max(maxValue, buffer[pos]);
            }

            double delta = maxValue - minValue;

            return delta;
        }

        private double CalcAverage(double[] buffer, int startPos, int entries)
        {
            double value = 0;

            for(int pos = startPos; pos < startPos+entries; pos++)
            {
                value += buffer[pos];
            }

            return value / entries;
        }

        public DirectXWaveformDisplay Plot { get; set; }
    }
}
