﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DemodulatorCollection.Interfaces;
using RX_FFT.Components.GDI;

namespace DemodulatorCollection.BitClockSinks
{
    class BiphaseDecoder : BitClockSink
    {
        private enum eLearnState
        {
            Idle,
            Synchronizing,
            ProcessingBit1,
            ProcessingBit2
        }

        private eLearnState State = eLearnState.Idle;
        private bool LastBitState = false;
        private bool PositivePhase = false;

        public bool Verbose = true;

        public BitClockSink BitSink { get; set; }

        public void Synchronize(bool positivePhase)
        {
            PositivePhase = positivePhase;
            if (BitSink != null)
            {
                BitSink.Resynchronized();
            }
            State = eLearnState.ProcessingBit1;
        }

        #region BitClockSink Member

        public void ClockBit(bool state)
        {
            switch (State)
            {
                case eLearnState.Idle:
                    LastBitState = state;
                    State = eLearnState.Synchronizing;
                    break;

                case eLearnState.Synchronizing:
                    if (state == LastBitState)
                    {
                        /* the phase depends on the current state */
                        PositivePhase = state;

                        if (Verbose)
                        {
                            Log.AddMessage("BiphaseDecoder", "Synchronized.");
                        }
                        if (BitSink != null)
                        {
                            BitSink.Resynchronized();
                            BitSink.ClockBit(false);
                        }
                        State = eLearnState.ProcessingBit1;
                    }
                    else
                    {
                        LastBitState = state;
                    }
                    break;

                case eLearnState.ProcessingBit1:
                    LastBitState = state;
                    State = eLearnState.ProcessingBit2;
                    break;

                case eLearnState.ProcessingBit2:
                    if (LastBitState == state)
                    {
                        if (PositivePhase == state)
                        {
                            if (Verbose)
                            {
                                Log.AddMessage("BiphaseDecoder", "Decoding failed. Resynchronizing.");
                            }
                            if (BitSink != null)
                            {
                                BitSink.Desynchronized();
                            }
                            State = eLearnState.Idle;
                            return;
                        }
                        else
                        {
                            PositivePhase ^= true;
                            if (BitSink != null)
                            {
                                BitSink.ClockBit(false);
                            }
                        }
                    }
                    else
                    {
                        if (PositivePhase != state)
                        {
                            if (Verbose)
                            {
                                Log.AddMessage("BiphaseDecoder", "Decoding failed. Resynchronizing.");
                            }
                            if (BitSink != null)
                            {
                                BitSink.Desynchronized();
                            }
                            State = eLearnState.Idle;
                            return;
                        }
                        else
                        {
                            if (BitSink != null)
                            {
                                BitSink.ClockBit(true);
                            }
                        }
                    }
                    State = eLearnState.ProcessingBit1;
                    break;
            }
        }

        public void Resynchronized()
        {
            State = eLearnState.Idle;
        }

        public void Desynchronized()
        {
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
