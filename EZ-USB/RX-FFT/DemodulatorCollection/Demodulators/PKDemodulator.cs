using System;
using System.Collections;
using System.Text;
using LibRXFFT.Libraries.SignalProcessing;
using RX_FFT.Components.GDI;
using DemodulatorCollection.Interfaces;

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
namespace DemodulatorCollection.Demodulators
{
    public class PKDemodulator : DigitalDemodulator
    {
        private static string Desc1 = "     Active    |   Inactive           ";
        private static string Desc2 = "     ______     ___        ___        ";
        private static string Desc3 = "|   |      |   |   |      |   |       ";
        private static string Desc4 = "|___|      |___|   |______|   |__ _ _ ";
        private static string Desc5 = "|   |<---->|   |   |<---->|   |       ";
        private static string Desc6 = "|<------------>|<------------>|       ";

        public int MinDbDistance = 8;
        public int MinBitLength = 10;
        public double _SamplingRate = 0;
        public BitClockSink BitSink { get; set; }

        private int SignalStrengthUpdateRate = 100000000;
        private int NoiseFloorUpdateRate = 100000;
        private double NoiseFloor = 0;
        private double SignalStrength = 0;

        private long NegativeEdge = 0;
        private long TransmittingSamples = 0;
        private long TransmittingSamplesMax = 0;

        private long SampleNum = 0;
        private long LastActiveStart = 0;
        private long LastActiveEnd = 0;
        private long SymbolDistance = 0;

        private bool Transmission = false;
        private bool TransmissionFirstSample = false;

        private bool Initialized = false;
        private bool Learning = true;
        private bool BitTimeLocked = false;
        private bool EnableAGC = true;

        private bool BitSampled = false;
        private bool LastNegState = false;
        private bool LastPosState = false;

        private int LearnBits = 0;
        private bool LearnTransmitState = false;

        private ArrayList Bits = new ArrayList();

        public PKDemodulator()
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

                msg.Append(Desc6);
                msg.Append("Constant symbol length.");
                msg.Append(Environment.NewLine);

                return msg.ToString();
            }
        }

        public void Init()
        {
            Learning = true;
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

            if (Learning)
            {
                if (sampleValue > SignalStrength)
                {
                    SignalStrength = (SignalStrength * 100 + sampleValue) / 101;
                }

                if (SampleNum < SamplingRate)
                {
                    NoiseFloor = Math.Max(sampleValue, NoiseFloor);
                }

                double signalDb = DBTools.SampleTodB(SignalStrength);
                double noiseDb = DBTools.SampleTodB(NoiseFloor);

                if (signalDb - noiseDb > MinDbDistance)
                {
                    if (LearnBits > 3)
                    {
                        LearnedPower();
                        Learning = false;
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

                return;
            }

            if (sampleValue > DecisionValue)
            {
                transmitting = true;

                /* the first sample of the symbol */
                if (TransmittingSamples == 0)
                {
                    long diff = SampleNum - LastActiveStart;

                    if (!BitTimeLocked && LastActiveStart != 0 && (diff > SymbolDistance || SymbolDistance == 0))
                    {
                        double sampleTime = (diff / SamplingRate) * 1000;
                        SymbolDistance = diff;

                        //Log.AddMessage("SymbolDistance: " + SymbolDistance.ToString() + " (" + sampleTime.ToString() + "ms)" + " at " + SampleNum);
                    }

                    LastActiveStart = SampleNum;
                }
                TransmittingSamples++;
            }
            else
            {
                transmitting = false;

                /* was active? */
                if (TransmittingSamples != 0)
                {
                    if (!Transmission && SymbolDistance > 0)
                    {
                        //Log.AddMessage("Transmission Start (sync to negative edge)" + " at " + SampleNum);
                        Transmission = true;
                        TransmissionFirstSample = true;
                        NegativeEdge = SampleNum;
                    }

                    if (!BitTimeLocked && TransmittingSamples > TransmittingSamplesMax)
                    {
                        //Log.AddMessage("TransmittingSamplesMax: " + TransmittingSamples.ToString() + " at " + SampleNum);
                        TransmittingSamplesMax = TransmittingSamples;
                    }

                    TransmittingSamples = 0;
                    LastActiveEnd = SampleNum;
                }
            }

            if (Transmission)
            {
                long diffToFirstSymbolStart = SampleNum - (NegativeEdge - TransmittingSamplesMax - (SymbolDistance - TransmittingSamplesMax) / 2);

                if (TransmissionFirstSample)
                {
                    DumpBits();
                    TransmissionFirstSample = false;
                }
                else
                {
                    long offset = diffToFirstSymbolStart % SymbolDistance;

                    /* sample in the middle of the first quarter */
                    if (offset == (SymbolDistance - TransmittingSamplesMax) / 4)
                    {
                        //Log.AddMessage("Neg now: " + (transmitting ? "1" : "0") + " at " + SampleNum);
                        LastNegState = transmitting;
                    }
                    /* sample in the middle of the symbol */
                    else if (offset == SymbolDistance/2)
                    {
                        //Log.AddMessage("Pos now: " + (transmitting ? "1" : "0") + " at " + SampleNum);
                        LastPosState = transmitting;

                        if (!LastPosState && !LastNegState)
                        {
                            //Log.AddMessage("Transmission end at " + SampleNum);
                            Transmission = false;

                            if (!BitTimeLocked)
                            {
                                BitTimeLocked = true;
                                Bits.Clear();
                                LearnedTiming();
                            }
                            else
                            {
                                DumpBits();
                            }
                        }
                        else if (LastPosState == LastNegState)
                        {
                            Log.AddMessage("ERROR: Both bits are " + (transmitting ? "1" : "0") + " at " + SampleNum);
                            Transmission = false;
                        }
                        else
                        {
                            BitSampled = true;

                            /* valid symbol, resync */
                            if (LastPosState)
                            {
                                //Log.AddMessage("Resync: " + LastActiveStart);
                                NegativeEdge = LastActiveStart + TransmittingSamplesMax;
                            }
                            else
                            {
                                //Log.AddMessage("Resync: " + LastActiveStart);
                                NegativeEdge = LastActiveEnd + TransmittingSamplesMax;
                            }
                        }
                    }
                    /* add data bit */
                    else if (offset == SymbolDistance - 1)
                    {
                        if (BitSampled)
                        {
                            BitSampled = false;
                            Bits.Add(LastPosState);
                        }
                    }
                }
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
            msg.Append(string.Format("{0,3} samples, {1:0.###} µs", TransmittingSamplesMax, (TransmittingSamplesMax / SamplingRate) * 1000000));
            Log.AddMessage(msg.ToString());

            msg.Length = 0;
            msg.Append(Desc6);
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
