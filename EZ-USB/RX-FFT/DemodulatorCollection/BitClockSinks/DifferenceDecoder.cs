using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DemodulatorCollection.Interfaces;
using RX_FFT.Components.GDI;

namespace DemodulatorCollection.BitClockSinks
{
    class DifferenceDecoder : BitClockSink
    {
        private bool LastBitState = false;

        public BitClockSink BitSink { get; set; }

        public void Synchronize(bool positivePhase)
        {
            BitSink.Resynchronized();
        }

        #region BitClockSink Member

        public void ClockBit(bool state)
        {
            bool newState = LastBitState ^ state;

            BitSink.ClockBit(newState);

            LastBitState = newState;
        }

        public void Resynchronized()
        {
            LastBitState = false;
            BitSink.Resynchronized();
        }

        public void TransmissionStart()
        {
            BitSink.TransmissionStart();
        }

        public void TransmissionEnd()
        {
            BitSink.TransmissionEnd();
        }

        #endregion
    }
}
