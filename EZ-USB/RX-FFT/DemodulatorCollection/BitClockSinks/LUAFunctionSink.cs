using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;
using DemodulatorCollection.Interfaces;
using System.Windows.Forms;
using RX_FFT.Components.GDI;
using System.Reflection;
using System.Collections;
using LibRXFFT.Libraries;
using DemodulatorCollection.Demodulators;

namespace DemodulatorCollection.BitClockSinks
{
    public class LUAFunctionSink_obsolete : BitClockSink
    {
        private bool Running = true;
        public Lua LuaVm;
        public string Clock;
        public string Resync;
        public string Desync;
        public string End;
        public string Start;

        public DigitalDemodulator Demodulator;

        public LUAFunctionSink_obsolete()
        {
        }

        public LUAFunctionSink_obsolete(ScriptableDemodulator source, string clockBit, string resync, string desync, string end, string start)
        {
            LuaVm = source.LuaVm;

            Clock = clockBit;
            Resync = resync;
            Desync = desync;
            End = end;
            Start = start;
        }


        #region BitClockSink Member

        public void Resynchronized()
        {
            CallFunction(Resync);
        }

        public void Desynchronized()
        {
            CallFunction(Desync);
        }

        public void TransmissionStart()
        {
            CallFunction(Start);
        }

        public void ClockBit(bool state)
        {
            CallFunction(Clock, state);
        }

        public void TransmissionEnd()
        {
            CallFunction(End);
        }

        #endregion

        public object[] CallFunction(string name, params object[] parameters)
        {
            if (!Running || name == null || name == "")
            {
                return null;
            }

            try
            {
                return LuaHelpers.CallFunction(LuaVm, name, parameters);
            }
            catch (Exception ex)
            {
                Running = false;
                Log.AddMessage("Failed to call " + name + " (Reason: " + ex.Message + "). Stopping.");
            }

            return null;
        }        
    }
}
