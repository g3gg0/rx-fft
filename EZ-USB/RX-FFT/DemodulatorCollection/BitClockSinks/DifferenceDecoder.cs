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
            if (BitSink != null)
            {
                BitSink.Resynchronized();
            }
        }

        #region BitClockSink Member

        public void ClockBit(bool state)
        {
            bool newState = LastBitState ^ state;

            if (BitSink != null)
            {
                BitSink.ClockBit(newState);
            }

            LastBitState = newState;
        }

        public void Resynchronized()
        {
            LastBitState = false;
            if (BitSink != null)
            {
                BitSink.Resynchronized();
            }
        }

        public void Desynchronized()
        {
            if (BitSink != null)
            {
                BitSink.Desynchronized();
            }
        }

        public void TransmissionStart()
        {
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
