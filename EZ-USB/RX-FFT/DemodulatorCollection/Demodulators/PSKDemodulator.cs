using System;
using System.Collections;
using LibRXFFT.Libraries.GSM.Layer3;
using RX_FFT.Components.GDI;
using DemodulatorCollection.BitClockSinks;
using DemodulatorCollection.Interfaces;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SignalProcessing;

namespace DemodulatorCollection.Demodulators
{
    public class PSKDemodulator : DigitalDemodulator
    {
        private enum eLearningState
        {
            Idle,
            Start,
            BackgroundNoise,
            PhaseDiff,
            TransmissionStart,
            TransmissionActive,
            TransmissionStop,
            TransmissionIdle,
            Prepare
        }

        public int BaudRate = 1200;
        public int NoiseFloorLearnSamples = 1000;
        public double PhaseShift = 0.25f;
        public double MinDbDistance = 10.0f;
        public BitClockSink BitSink { get; set; }

        public bool UseFastAtan2 = false;

        private double LastPhase;

        private double _SamplingRate;
        private eLearningState State;
        private long SampleNum;
        private double NoiseFloor = 0;
        
        private double PhaseDiffHigh;
        private double PhaseDiffLow;

        private long SymbolDistance;
        private long SamplePointStart;
        private long SamplePointEnd;
        private bool PhasePositive = false;
        private double PhaseDifferenceSmooth = 0.0f;
        private double PhaseSum = 0.0f;

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
                    Log.AddMessage("PSKDemodulator", "Initializing Demodulator");
                    Init();
                }
                _SamplingRate = value;

                SymbolDistance = (long)(value / BaudRate);
            }
        }

        public void Init()
        {
            State = eLearningState.Start;

            Log.AddMessage("PSKDemodulator", "Enter learning mode...");
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

        public void Process(double iValue, double qValue)
        {
            SampleNum++;

            double sampleValue = Math.Sqrt(iValue * iValue + qValue * qValue);
            double phase = UseFastAtan2 ? FastAtan2b(iValue, qValue) : Math.Atan2(iValue, qValue);
            double signalDb = DBTools.SampleTodB(sampleValue);
            double noiseDb = DBTools.SampleTodB(NoiseFloor);

            if (Math.Abs(sampleValue) >= 1.0)
            {
                return;
            }

            while (phase - LastPhase < -(Math.PI / 2))
                phase += Math.PI;

            while (phase - LastPhase > Math.PI / 2)
                phase -= Math.PI;

            /* catch the case where I and Q are zero */
            if (double.IsNaN(phase))
                phase = LastPhase;

            double phaseDifference = phase - LastPhase;

            LastPhase = phase % (2 * Math.PI);


            // work with phase difference now
            switch (State)
            {
                case eLearningState.Idle:
                    break;

                case eLearningState.Prepare:
                    if(SamplingRate != 0)
                    {
                        Log.AddMessage("PSKDemodulator", "Waiting for Sampling rate being published.");
                        State = eLearningState.Start;
                    }
                    break;

                case eLearningState.Start:
                    if (SamplingRate != 0)
                    {
                        Log.AddMessage("PSKDemodulator", "Learn background noise for " + FrequencyFormatter.TimeToString(NoiseFloorLearnSamples / SamplingRate) + ".");
                        State = eLearningState.BackgroundNoise;
                    }
                    break;

                case eLearningState.BackgroundNoise:
                    NoiseFloor += sampleValue;
                    if (SampleNum > NoiseFloorLearnSamples)
                    {
                        NoiseFloor /= NoiseFloorLearnSamples;
                        Log.AddMessage("PSKDemodulator", "Learned Noise. Transmission may start now.");
                        State = eLearningState.PhaseDiff;
                    }
                    break;

                case eLearningState.PhaseDiff:
                    PhaseDiffHigh = PhaseShift;
                    PhaseDiffLow = -PhaseShift;
                    State = eLearningState.TransmissionIdle;
                    break;

                case eLearningState.TransmissionIdle:
                    if (signalDb > noiseDb + MinDbDistance)
                    {
                        State = eLearningState.TransmissionStart;
                    }
                    else
                    {
                        State = eLearningState.TransmissionIdle;
                    }
                    break;

                case eLearningState.TransmissionStart:
                    /* wait until quarter of a symbol was sent before using phase information */
                    SamplePointStart = SampleNum + SymbolDistance / 4;
                    SamplePointEnd = SamplePointStart + SymbolDistance / 2;
                    State = eLearningState.TransmissionActive;

                    if (BitSink != null)
                    {
                        BitSink.TransmissionStart();
                    }
                    break;


                case eLearningState.TransmissionActive:
                    if (SampleNum < SamplePointStart || SampleNum > SamplePointEnd)
                    {
                        PhaseDifferenceSmooth /= 2;
                        PhaseDifferenceSmooth += phaseDifference;

                        if (PhaseDifferenceSmooth / 1.75f > PhaseDiffHigh)
                        {
                            /* handle a low->high transition */
                            if (!PhasePositive)
                            {
                                PhasePositive = true;
                                SamplePointStart = (long) (SampleNum + SymbolDistance * 0.15f);
                                SamplePointEnd = (long)(SamplePointStart + SymbolDistance * 0.7f);
                            }
                        }
                        else if (PhaseDifferenceSmooth / 1.75f < PhaseDiffLow)
                        {
                            /* handle a high->low transition */
                            if (PhasePositive)
                            {
                                PhasePositive = false;
                                SamplePointStart = (long)(SampleNum + SymbolDistance * 0.15f);
                                SamplePointEnd = (long)(SamplePointStart + SymbolDistance * 0.7f);
                            }
                        }
                    }
                    else if (SampleNum == SamplePointStart)
                    {
                        PhaseSum = 0;
                    }
                    else if (SampleNum == SamplePointEnd)
                    {
                        PhasePositive = PhaseSum > 0;

                        if (BitSink != null)
                        {
                            BitSink.ClockBit(!PhasePositive);
                        }

                        /* set the next sampling points. will get overriden when phase changes */
                        SamplePointStart += SymbolDistance;
                        SamplePointEnd += SymbolDistance;
                    }
                    else
                    {
                        /* check whether signal strength has decreased */
                        if (signalDb < noiseDb + MinDbDistance)
                        {
                            State = eLearningState.TransmissionStop;
                        }
                        else
                        {
                            PhaseSum += phaseDifference;
                        }
                    }

                    break;

                case eLearningState.TransmissionStop:
                    if (BitSink != null)
                    {
                        BitSink.TransmissionEnd();
                    }
                    State = eLearningState.TransmissionIdle;
                    break;
            }
        }

    }
}