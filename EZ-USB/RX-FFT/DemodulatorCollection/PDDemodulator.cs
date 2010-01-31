using System;
using System.Collections;
using System.Text;
using LibRXFFT.Libraries.SignalProcessing;
using RX_FFT.Components.GDI;

/* 
 * Pulse distance demodulator 
 */
namespace DemodulatorCollection
{
    public class PDDemodulator : DigitalDemodulator
    {
        private static string Desc1 = "    _____          _____          ";
        private static string Desc2 = "   |     |        |     |         ";
        private static string Desc3 = "  _|     |________|     |___ _ _  ";
        private static string Desc4 = "   |<--->|        |<--->|         "; // Constant pulse length (TransmittingSamplesMax)
        private static string Desc5 = "   |<------------>|               "; // Variable distance (SymbolDistance is the shortest length and reference)
        private static string Desc6 = "                                  ";

        public int MinDbDistance = 8;
        public int MinBitLength = 10;
        public double _SamplingRate = 0;

        private int SignalStrengthUpdateRate = 100000000;
        private int NoiseFloorUpdateRate = 100000;
        private double NoiseFloor;
        private double SignalStrength;

        private long TransmittingSamples;
        private long TransmittingSamplesMax;

        private long SampleNum;
        private long LastActiveStart = 0;
        private long SymbolDistance = 0;

        private bool Transmission = false;
        private bool FirstTransmission = false;

        private bool Initialized = false;
        private bool Learning = true;
        private bool BitTimeLocked = false;
        private bool EnableAGC = true;

        private int LearnBits = 0;
        private bool LearnTransmitState = false;

        private ArrayList Bits = new ArrayList();

        public PDDemodulator()
        {
        }

        private long DelayEnd
        {
            get { return SymbolDistance * 4; }
        }
        private long DelayStartBit
        {
            get { return SymbolDistance * 2; }
        }
        private long DelayShortBit
        {
            get { return (long)(SymbolDistance * 0.75f); }
        }
        private long DelayLongBit
        {
            get { return (long)(SymbolDistance * 1.5f); }
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
                msg.Append("Constant pulse length");
                msg.Append(Environment.NewLine);

                msg.Append(Desc5);
                msg.Append("Variable pulse distance.");
                msg.Append(Environment.NewLine);

                msg.Append(Desc6);
                msg.Append("Packet starts with long delay.");
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
            FirstTransmission = false;

            Initialized = true;

            Log.AddMessage("Enter learning mode...");
        }

        public void Process(double iValue, double qValue)
        {
            if (!Initialized)
                return;

            double sampleValue = Math.Sqrt(iValue * iValue + qValue * qValue);
            bool bitStart = false;
            long diffToLastActive = SampleNum - LastActiveStart;

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

                /* reading noise level for 1 second */
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
                bitStart = false;

                /* the first sample of the symbol */
                if (TransmittingSamples == 0)
                {
                    long diff = SampleNum - LastActiveStart;

                    if (!BitTimeLocked && (diff < SymbolDistance || SymbolDistance == 0))
                    {
                        SymbolDistance = diff;
                    }

                    if (!Transmission)
                    {
                        //Log.AddMessage("Transmission Start");
                        Transmission = true;
                        FirstTransmission = true;
                    }

                    LastActiveStart = SampleNum;
                    bitStart = true;
                }
                TransmittingSamples++;
            }
            else
            {
                /* was active? */
                if (TransmittingSamples != 0)
                {
                    if (!BitTimeLocked)
                    {
                        TransmittingSamplesMax = Math.Max(TransmittingSamplesMax, TransmittingSamples);
                    }
                    TransmittingSamples = 0;
                }
            }

            if (Transmission)
            {
                if (FirstTransmission)
                {
                    FirstTransmission = false;
                }
                else
                {
                    if (diffToLastActive > DelayEnd)
                    {
                        //Log.AddMessage("Transmission STOP");
                        Transmission = false;
                        DumpBits();
                    }
                    else if (bitStart)
                    {
                        if (diffToLastActive > DelayStartBit)
                        {
                            if (!BitTimeLocked)
                            {
                                BitTimeLocked = true;
                                LearnedTiming();
                                Bits.Clear();
                            }
                            else
                            {
                                DumpBits();
                            } 
                            //Log.AddMessage("Transmission START " + diffToLastActive);
                        }
                        else if (diffToLastActive > DelayLongBit)
                        {
                            Bits.Add(true);
                        }
                        else if (diffToLastActive > DelayShortBit)
                        {
                            Bits.Add(false);
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

            msg.Length = 0;
            msg.Append(Desc4);
            msg.Append(string.Format("{0,3} samples, {1:0.###} µs", TransmittingSamplesMax, (TransmittingSamplesMax / SamplingRate) * 1000000));
            Log.AddMessage(msg.ToString());

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
