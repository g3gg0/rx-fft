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

namespace DemodulatorCollection.BitClockSinks
{
    public class ScriptableSink : BitClockSink
    {
        private bool Running = false;
        private Lua LuaVm;

        public DigitalDemodulator Demodulator;

        public ScriptableSink()
        {
            LuaVm = new Lua();
            
            LuaHelpers.RegisterAssembly("DemodulatorCollection");

            LuaHelpers.RegisterNamespace("DemodulatorCollection.Demodulators");
            LuaHelpers.RegisterNamespace("DemodulatorCollection.BitClockSinks");

            LuaHelpers.RegisterLuaFunctions(LuaVm, new LuaHelpers());
            LuaHelpers.RegisterLuaFunctions(LuaVm, this);

            FileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LuaVm.DoFile(dlg.FileName);
                    Running = true;
                }
                catch (Exception e)
                {
                    Log.AddMessage("ScriptableSink", "Failed to load the LUA file. " + e.Message);
                }
            }
        }

        public ScriptableSink(Lua vm)
        {
            LuaVm = vm;
            LuaHelpers.RegisterLuaFunctions(LuaVm, this);
        }

        [AttrLuaFunc("GetDemod", "")]
        public DigitalDemodulator LuaGetDemod()
        {
            return Demodulator;
        }

        [AttrLuaFunc("SgpTimeConversion", "Convert SGP milisecond count to a date string", new[] { "Miliseconds from Monday, 00:00" })]
        public string LuaSgpTimeConversion(UInt32 miliseconds)
        {
            DateTime d = DateTime.Now;

            while (d.DayOfWeek != DayOfWeek.Monday)
            {
                d = d.Subtract(TimeSpan.FromDays(1));
            }
            d = d.AddMilliseconds(miliseconds);

            return d.DayOfWeek + ", " + d;
        }

        public void Init()
        {
            CallFunction("Init");
        }

        #region BitClockSink Member

        public void Resynchronized()
        {
            CallFunction("Resynchronized");
        }

        public void TransmissionStart()
        {
            CallFunction("TransmissionStart");
        }

        public void ClockBit(bool state)
        {
            CallFunction("ClockBit", state);
        }

        public void TransmissionEnd()
        {
            CallFunction("TransmissionEnd");
        }

        #endregion

        public object[] CallFunction(string name, params object[] parameters)
        {
            if (!Running)
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
