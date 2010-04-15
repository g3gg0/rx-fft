using System;
using System.Collections;
using LibRXFFT.Libraries.GSM.Layer3;
using RX_FFT.Components.GDI;

namespace DemodulatorCollection
{
    public class PSKDemodulator : DigitalDemodulator
    {
        private enum eLearningState
        {
            Idle,
            PhaseDiff,
            Done
        }

        public BitClockSink BitSink;
        public int BaudRate = 1200;
        public double PhaseShift = 0.25f;

        private double LastPhase;

        private double _SamplingRate;
        private eLearningState Learning;
        private long SampleNum;

        private double PhaseDiffHigh;
        private double PhaseDiffLow;

        private long TransmittingSamples;
        private long SymbolDistance;
        private long NextSamplePoint;
        private bool PhasePositive = false;

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

                SymbolDistance = (long)(value / BaudRate);
            }
        }

        public void Init()
        {
            Learning = eLearningState.PhaseDiff;

            Log.AddMessage("Enter learning mode...");
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

            double phase = UseFastAtan2 ? FastAtan2b(iValue, qValue) : Math.Atan2(iValue, qValue);

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
            switch (Learning)
            {
                case eLearningState.Idle:
                    break;

                case eLearningState.PhaseDiff:
                    PhaseDiffHigh = PhaseShift;
                    PhaseDiffLow = -PhaseShift;
                    Learning = eLearningState.Done;
                    break;


                case eLearningState.Done:
                    if(phaseDifference > PhaseDiffHigh)
                    {
                        /* handle a low->high transition */
                        if (!PhasePositive)
                        {
                            PhasePositive = true;
                            NextSamplePoint = SampleNum + SymbolDistance / 2;
                        }
                    }
                    else if (phaseDifference < PhaseDiffLow)
                    {
                        /* handle a high->low transition */
                        if (PhasePositive)
                        {
                            PhasePositive = false;
                            NextSamplePoint = SampleNum + SymbolDistance / 2;
                        }
                    }

                    if(SampleNum == NextSamplePoint)
                    {
                        NextSamplePoint = SampleNum + SymbolDistance;

                        if(BitSink!=null)
                        {
                            BitSink.ClockBit(!PhasePositive);
                        }
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected bool UseFastAtan2
        {
            get; set;
        }
    }
}