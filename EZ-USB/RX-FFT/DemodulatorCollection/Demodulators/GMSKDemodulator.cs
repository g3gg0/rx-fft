using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DemodulatorCollection.Interfaces;
using LibRXFFT.Libraries.SignalProcessing;
using RX_FFT.Components.GDI;
using LibRXFFT.Libraries.Misc;

namespace DemodulatorCollection.Demodulators
{
    public class GMSKDemodulator : DigitalDemodulator
    {
        private enum eLearningState
        {
            Idle,
            Learn,
            Process
        }

        private eLearningState State = eLearningState.Idle;
        private double LastPhase;
        private double[] AlignmentBuffer;
        private long AlignmentBufferPos;
        private double SampleOffset;
        private long SampleNum;
        private double NextSamplePoint;
        private bool AlignmentCheckDone;
        private double _SamplingRate;

        public bool UseFastAtan2 = true;
        public int SymbolsToCheck = 2;
        public long SymbolsPerSecond = 4800;

        private long SamplesPerSymbol
        {
            get
            {
                return (long)(SamplingRate / SymbolsPerSecond);
            }
        }

        #region DigitalDemodulator Member

        public BitClockSink BitSink
        {
            get;
            set;
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
                    Log.AddMessage("GMSKDemodulator", "Initializing Demodulator");
                    _SamplingRate = value;
                    Init();
                }
            }
        }

        public void Init()
        {
            AlignmentCheckDone = false;
            AlignmentBufferPos = 0;
            AlignmentBuffer = new double[SymbolsToCheck * SamplesPerSymbol];

            SampleNum = 0;

            if (SamplingRate == 0)
            {
                Log.AddMessage("GMSKDemodulator", "Idle");
                State = eLearningState.Idle;
                return;
            }

            State = eLearningState.Learn;

            Log.AddMessage("GMSKDemodulator", "SamplingRate:         " + FrequencyFormatter.FreqToStringAccurate(SamplingRate));
            Log.AddMessage("GMSKDemodulator", "SymbolsPerSecond:     " + SymbolsPerSecond);
            Log.AddMessage("GMSKDemodulator", "SamplesPerSymbol:     " + SamplesPerSymbol);
            Log.AddMessage("GMSKDemodulator", "Symbols for alignchk: " + SymbolsToCheck);
            Log.AddMessage("GMSKDemodulator", "");
            Log.AddMessage("GMSKDemodulator", "State: Learn");
        }

        public void Process(double iValue, double qValue)
        {
            double strength = (iValue * iValue + qValue * qValue);
            double phase;

            phase = UseFastAtan2 ? FastAtan2b(iValue, qValue) : Math.Atan2(iValue, qValue);

            while (phase - LastPhase < -(Math.PI / 2))
                phase += Math.PI;

            while (phase - LastPhase > Math.PI / 2)
                phase -= Math.PI;

            /* catch the case where I and Q are zero */
            if (double.IsNaN(phase))
                phase = LastPhase;

            double diff = phase - LastPhase;
            LastPhase = phase % (2 * Math.PI);


            switch (State)
            {
                case eLearningState.Idle:
                    break;

                case eLearningState.Learn:
                case eLearningState.Process:
                    /* build a buffer of samples to check the misalignment due to drifting clocks */
                    if (!AlignmentCheckDone)
                    {
                        AlignmentBuffer[AlignmentBufferPos++] = diff;

                        if (AlignmentBufferPos == AlignmentBuffer.Length)
                        {
                            AlignmentBufferPos = 0;
                            AlignmentCheckDone = true;
                            SampleOffset = OffsetEstimator.EstimateOffset(AlignmentBuffer, 0, AlignmentBuffer.Length, SamplesPerSymbol);

                            if (State == eLearningState.Learn)
                            {
                                NextSamplePoint = SampleNum + SampleOffset + SamplesPerSymbol / 2;
                                SampleOffset = 0;
                                State = eLearningState.Process;
                                Log.AddMessage("GMSKDemodulator", "State: Process");
                            }
                            else
                            {
                                SampleOffset -= SamplesPerSymbol / 2;
                                SampleOffset /= 2;
                            }
                            //Log.AddMessage("SampleOffset:     " + SampleOffset);
                        }
                    }

                    /* process data only when already learned */
                    if (State == eLearningState.Process)
                    {
                        if (SampleNum == (long)NextSamplePoint)
                        {
                            if (BitSink != null)
                            {
                                BitSink.ClockBit(diff > 0);
                            }

                            NextSamplePoint += SamplesPerSymbol;

                            if (AlignmentCheckDone)
                            {
                                /* start bit alignment check again */
                                AlignmentCheckDone = false;

                                /* apply offset */
                                NextSamplePoint += SampleOffset;
                            }
                        }
                    }
                    break;
            }

            SampleNum++;
        }

        #endregion

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
    }
}
