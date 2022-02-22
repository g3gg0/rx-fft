using DemodulatorCollection.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemodulatorCollection.BitClockSinks
{
    class ManchesterBitSink : BitClockSink
    {
        private enum eLearnState
        {
            ProcessingBit1,
            ProcessingBit2
        }

        private eLearnState State = eLearnState.ProcessingBit1;
        private bool LastBitState = false;
        private bool PositivePhase = false;

        public bool Verbose = true;

        public BitClockSink BitSink { get; set; }

        #region BitClockSink Member

        public void ClockBit(bool state)
        {
            switch (State)
            {
                case eLearnState.ProcessingBit1:
                    LastBitState = state;
                    State = eLearnState.ProcessingBit2;
                    break;

                case eLearnState.ProcessingBit2:
                    if (LastBitState && !state)
                    {
                        if (BitSink != null)
                        {
                            BitSink.ClockBit(true);
                        }
                    }
                    else if (!LastBitState && state)
                    {
                        if (BitSink != null)
                        {
                            BitSink.ClockBit(false);
                        }
                    }
                    else
                    {
                        if (BitSink != null)
                        {
                            BitSink.TransmissionEnd();
                        }
                    }

                    State = eLearnState.ProcessingBit1;
                    break;
            }
        }

        public void Resynchronized()
        {
            State = eLearnState.ProcessingBit1;
        }

        public void Desynchronized()
        {
        }

        public void TransmissionStart()
        {
            State = eLearnState.ProcessingBit1;

            if (BitSink != null)
            {
                BitSink.TransmissionStart();
            }
        }

        public void TransmissionEnd()
        {
            if (BitSink != null)
            {
                BitSink.TransmissionEnd();
            }
        }

        #endregion
    }
}
