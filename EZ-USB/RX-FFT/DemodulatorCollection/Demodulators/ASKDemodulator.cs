using System;
using System.Collections;
using System.Text;
using LibRXFFT.Libraries.SignalProcessing;
using RX_FFT.Components.GDI;
using DemodulatorCollection.Interfaces;


namespace DemodulatorCollection.Demodulators
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

        public int BaudRate = 1200;
        public int MinDbDistance = 10;
        public int MinBitLength = 10;
        public bool EnableAGC = true;
        public BitClockSink BitSink { get; set; }
        public bool BitTimeLocked = false;
        public bool SignalStrengthLocked = false;

        private double _SamplingRate = 0;

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
        private eLearningState State = eLearningState.Idle;


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
                    Log.AddMessage("ASKDemodulator", "Initializing Demodulator");
                    _SamplingRate = value;
                    Init();
                }

                SymbolDistance = (long)(value / BaudRate);
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

            if (SamplingRate == 0)
            {
                Log.AddMessage("ASKDemodulator", "Idle");
                State = eLearningState.Idle;
                return;
            }

            State = eLearningState.BackgroundNoise;
            Log.AddMessage("ASKDemodulator", "Enter background noise learning mode...");
        }

        public void Process(double iValue, double qValue)
        {
            if (!Initialized)
                return;

            double sampleValue = Math.Sqrt(iValue * iValue + qValue * qValue);

            SampleNum++;

            if (EnableAGC)
            {
                NoiseFloor = (NoiseFloor * NoiseFloorUpdateRate + sampleValue) / (NoiseFloorUpdateRate + 1);
                SignalStrength = (SignalStrength * SignalStrengthUpdateRate) / (SignalStrengthUpdateRate + 1);
            }

            switch (State)
            {
                case eLearningState.Idle:
                    break;

                case eLearningState.BackgroundNoise:
                    NoiseFloor = Math.Max(sampleValue, NoiseFloor);
                    if (SampleNum > SamplingRate)
                    {
                        Log.AddMessage("Learned noise level. You may start transmission now.");
                        if (!SignalStrengthLocked)
                        {
                            State = eLearningState.SignalStrength;
                        }
                        else
                        {
                            SignalStrength = DBTools.SampleFromdB(DBTools.SampleTodB(NoiseFloor) + MinDbDistance);
                            State = eLearningState.BitTiming;
                        }
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
                            State = eLearningState.BitTiming;
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
                    State = eLearningState.Done;
                    break;

                case eLearningState.Done:
                    bool transmitting = false;
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
                        /* was active? */
                        if (TransmittingSamples != 0)
                        {
                            TransmittingSamples = 0;
                            LastActiveEnd = SampleNum;
                        }
                    }

                    if (Transmission)
                    {
                        if (TransmissionFirstSample)
                        {

                            if (BitSink != null)
                            {
                                BitSink.TransmissionStart();
                            }
                            //DumpBits();
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
                                if (BitSink != null)
                                {
                                    BitSink.ClockBit(transmitting);
                                }
                                //Bits.Add(transmitting);

                                if (!transmitting)
                                {
                                    if (++ConsecutiveZeros > 7)
                                    {
                                        Transmission = false;

                                        if (!BitTimeLocked)
                                        {
                                            BitTimeLocked = true;
                                            LearnedTiming();
                                            //Bits.Clear();
                                        }
                                        else
                                        {
                                            //DumpBits();
                                        }

                                        if (BitSink != null)
                                        {
                                            BitSink.TransmissionEnd();
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
            msg.Append(string.Format("{0,3} samples, {1:0.###} µs, Baud rate: {2:0}", SymbolDistance, (SymbolDistance / SamplingRate) * 1000000, 1.0 / ((SymbolDistance / SamplingRate))));
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
