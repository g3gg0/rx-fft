using System;
using System.Collections;
using System.Text;
using LibRXFFT.Libraries.SignalProcessing;

/* 
 * Pulse keying demodulator 
 *      ______      ___         ___
 *     |      |    |   |       |   |   
 * ____|      |____|   |_______|   |__ _ _
 * |<------------->|<------------->|      Constant symbol length
 * |               |               |
 * |   Inactive    |     Active    |
 * 
 */
namespace DemodulatorCollection
{
    public class ASKDemodulator : DigitalDemodulator
    {
        private enum eLearningState
        {
            Idle,
            BackgroundNoise,
            SignalStrength,
            BitTiming,
            Done
        }
        private static string Desc1 = "     Active        Inactive           ";
        private static string Desc2 = " ______________                       ";
        private static string Desc3 = "|              |              |       ";
        private static string Desc4 = "|              |______________|       ";
        private static string Desc5 = "|<------------>|<------------>|       ";

        public int MinDbDistance = 10;
        public int MinBitLength = 10;
        public double _SamplingRate = 0;

        private int SignalStrengthUpdateRate = 100000000;
        private int NoiseFloorUpdateRate = 100000;
        private double NoiseFloor = 0;
        private double SignalStrength = 0;

        private long ConsecutiveZeros = 0;
        private long LastBitSample = 0;

        private long NegativeEdge = 0;
        private long TransmittingSamples = 0;
        private long TransmittingSamplesMax = 0;

        private long NextSamplePoint = 0;
        private long SampleNum = 0;
        private long LastActiveStart = 0;
        private long LastActiveEnd = 0;
        private long SymbolDistance = 0;

        private bool Transmission = false;
        private bool TransmissionFirstSample = false;

        private bool Initialized = false;
        private eLearningState Learning = eLearningState.Idle;
        private bool BitTimeLocked = false;
        private bool EnableAGC = true;

        private int LearnBits = 0;
        private bool LearnTransmitState = false;

        private ArrayList Bits = new ArrayList();

        public ASKDemodulator()
        {
        }

        private long DelayEnd
        {
            get { return SymbolDistance * 4; }
        }
        private double DecisionValue
        {
            get { return (SignalStrength + NoiseFloor) / 2; }
        }


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
                    Log.AddMessage("Initializing Demodulator");
                    Init();
                }
                _SamplingRate = value;
            }
        }

        public static string Description
        {
            get
            {
                StringBuilder msg = new StringBuilder("");

                msg.Append(Desc1);
                msg.Append(Environment.NewLine);
                msg.Append(Desc2);
                msg.Append(Environment.NewLine);
                msg.Append(Desc3);
                msg.Append(Environment.NewLine);
                msg.Append(Desc4);
                msg.Append(Environment.NewLine);

                msg.Append(Desc5);
                msg.Append("Constant keying format.");
                msg.Append(Environment.NewLine);

                return msg.ToString();
            }
        }

        public void Init()
        {
            Learning = eLearningState.BackgroundNoise;
            BitTimeLocked = false;
            TransmittingSamplesMax = 0;
            NoiseFloor = 0;
            SignalStrength = 0;
            TransmittingSamples = 0;
            TransmittingSamplesMax = 0;
            SampleNum = 0;
            LastActiveStart = 0;
            SymbolDistance = 0;
            Transmission = false;
            TransmissionFirstSample = false;

            Initialized = true;

            Log.AddMessage("Enter learning mode...");
        }

        public void Process(double iValue, double qValue)
        {
            if (!Initialized)
                return;

            double sampleValue = Math.Sqrt(iValue * iValue + qValue * qValue);
            bool transmitting = false;

            SampleNum++;

            if (EnableAGC)
            {
                NoiseFloor = (NoiseFloor * NoiseFloorUpdateRate + sampleValue) / (NoiseFloorUpdateRate + 1);
                SignalStrength = (SignalStrength * SignalStrengthUpdateRate) / (SignalStrengthUpdateRate + 1);
            }

            switch (Learning)
            {
                case eLearningState.Idle:
                    break;

                case eLearningState.BackgroundNoise:
                    NoiseFloor = Math.Max(sampleValue, NoiseFloor);
                    if (SampleNum > SamplingRate)
                    {
                        Log.AddMessage("Learned Noise. Please start transmission now.");
                        Learning = eLearningState.SignalStrength;
                    }
                    break;

                case eLearningState.SignalStrength:
                    if (sampleValue > SignalStrength)
                    {
                        SignalStrength = (SignalStrength * 99 + sampleValue) / 100;
                    }

                    double signalDb = DBTools.SampleTodB(SignalStrength);
                    double noiseDb = DBTools.SampleTodB(NoiseFloor);

                    if (signalDb - noiseDb > MinDbDistance)
                    {
                        if (LearnBits > 3)
                        {
                            LearnedPower();
                            Learning = eLearningState.BitTiming;
                        }
                        else
                        {
                            bool state;

                            if (sampleValue < DecisionValue)
                            {
                                state = false;
                            }
                            else
                            {
                                state = true;
                            }

                            if (LearnTransmitState && !state)
                            {
                                LearnBits++;
                            }

                            LearnTransmitState = state;
                        }
                    }
                    break;

                case eLearningState.BitTiming:
                    Learning = eLearningState.Done;
                    break;

                case eLearningState.Done:

                    if (sampleValue > DecisionValue)
                    {
                        transmitting = true;

                        /* the first sample of the symbol */
                        if (TransmittingSamples == 0)
                        {
                            long diff = SampleNum - LastActiveEnd;

                            if (!Transmission && SymbolDistance > 0)
                            {
                                //Log.AddMessage("Transmission Start (sync to positive edge)" + " at " + SampleNum);
                                Transmission = true;
                                TransmissionFirstSample = true;
                                NegativeEdge = SampleNum;
                            }

                            if (!BitTimeLocked && LastActiveStart != 0 && (diff < SymbolDistance || SymbolDistance == 0))
                            {
                                double sampleTime = (diff / SamplingRate) * 1000;
                                SymbolDistance = diff;

                                //Log.AddMessage("SymbolDistance: " + SymbolDistance.ToString() + " (" + sampleTime.ToString() + "ms)" + " at " + SampleNum);
                            }

                            LastActiveStart = SampleNum;
                            NextSamplePoint = SampleNum + SymbolDistance / 2;
                        }
                        TransmittingSamples++;
                    }
                    else
                    {
                        transmitting = false;

                        /* was active? */
                        if (TransmittingSamples != 0)
                        {
                            TransmittingSamples = 0;
                            LastActiveEnd = SampleNum;
                        }
                    }

                    if (Transmission)
                    {
                        long diffToLastBit = SampleNum - LastBitSample;

                        if (TransmissionFirstSample)
                        {
                            DumpBits();
                            LastBitSample = SampleNum;
                            ConsecutiveZeros = 0;
                            TransmissionFirstSample = false;
                        }
                        else
                        {
                            /* sample in the middle of the bit */
                            if (SampleNum == NextSamplePoint)
                            {
                                NextSamplePoint = SampleNum + SymbolDistance;
                                //Log.AddMessage("Bit: " + (transmitting ? "1" : "0") + " at " + SampleNum);
                                Bits.Add(transmitting);

                                if (!transmitting)
                                {
                                    if (++ConsecutiveZeros > 7)
                                    {
                                        if (!BitTimeLocked)
                                        {
                                            BitTimeLocked = true;
                                            Transmission = false;
                                            LearnedTiming();
                                            Bits.Clear();
                                        }
                                        else
                                        {
                                            Transmission = false;
                                            DumpBits();
                                        }
                                        return;
                                    }
                                }
                                else
                                {
                                    ConsecutiveZeros = 0;
                                }
                            }
                        }
                    }
                    break;
            }

        }


        private void LearnedTiming()
        {
            StringBuilder msg = new StringBuilder("");

            Log.AddMessage("Learned bit timing:");
            Log.AddMessage(Desc1);
            Log.AddMessage(Desc2);
            Log.AddMessage(Desc3);
            Log.AddMessage(Desc4);

            msg.Length = 0;
            msg.Append(Desc5);
            msg.Append(string.Format("{0,3} samples, {1:0.###} µs", SymbolDistance, (SymbolDistance / SamplingRate) * 1000000));
            Log.AddMessage(msg.ToString());
            Log.AddMessage("");
        }

        private void LearnedPower()
        {
            double noiseDb = DBTools.SampleTodB(NoiseFloor);
            double signalDb = DBTools.SampleTodB(SignalStrength);

            Log.AddMessage("Learned signal power:");
            Log.AddMessage("   Noise:  " + string.Format("{0:0.00}", noiseDb));
            Log.AddMessage("   Signal: " + string.Format("{0:0.00}", signalDb));
        }

        private void DumpBits()
        {
            if (Bits.Count >= MinBitLength)
            {
                string msg = "";
                foreach (bool bit in Bits)
                {
                    msg += bit ? "1" : "0";
                }
                Log.AddMessage("Data: " + msg);
            }
            Bits.Clear();
        }
    }
}
