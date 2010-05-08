using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DemodulatorCollection.Interfaces;
using LuaInterface;
using System.Collections;
using System.Windows.Forms;
using RX_FFT.Components.GDI;
using System.Reflection;

namespace DemodulatorCollection.Demodulators
{
    public class ScriptableDemodulator : DigitalDemodulator, BitClockSink
    {
        private bool Running = false;
        private Lua LuaVm;
        private DigitalDemodulator Demodulator;

        public ScriptableDemodulator()
        {
            LuaVm = new Lua();
            LuaVm["SamplingRate"] = 0.0f;

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
                    Log.AddMessage("ScriptableDemodulator", "Failed to load the LUA file. " + e.Message);
                }
            }
        }

        /* for setting the bit clock sink on demodulator using "xxx.BitSink = this()" */
        [AttrLuaFunc("this", "")]
        public ScriptableDemodulator LuaThis()
        {
            return this;
        }

        [AttrLuaFunc("GetDemodulator", "")]
        public DigitalDemodulator LuaGetDemod()
        {
            return Demodulator;
        }

        [AttrLuaFunc("SetDemodulator", "", new[] { "" })]
        public void LuaSetDemod(DigitalDemodulator demod)
        {
            Demodulator = demod;
            Demodulator.BitSink = this;
        }

        [AttrLuaFunc("GetBitSink", "")]
        public BitClockSink LuaGetBitSink()
        {
            return BitSink;
        }

        [AttrLuaFunc("SetBitSink", "", new[] { "" })]
        public void LuaSetBitSink(BitClockSink sink)
        {
            BitSink = sink;
        }

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

        public object[] TryCallFunction(string name, params object[] parameters)
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
            }

            return null;
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
                try
                {
                    return (double)LuaVm["SamplingRate"];
                }
                catch (Exception e)
                {
                }

                return 0;
            }
            set
            {
                try
                {
                    LuaVm["SamplingRate"] = value;
                    TryCallFunction("SamplingRateChanged");
                    if (Demodulator != null)
                    {
                        Demodulator.SamplingRate = value;
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        public void Init()
        {
            CallFunction("Init");
        }

        public void Process(double iValue, double qValue)
        {
            if (Demodulator != null)
            {
                Demodulator.Process(iValue, qValue);
            }
            else if (LuaVm.GetFunction("ProcessSample") != null)
            {
                CallFunction("ProcessSample", iValue, qValue);
            }
        }

        #endregion


        #region BitClockSink Member

        private int CallDepth = 0;

        public void Resynchronized()
        {
            if (CallDepth == 0 && BitSink != null)
            {
                CallDepth++;
                BitSink.Resynchronized();
                CallDepth--;
            }
            else if (LuaVm.GetFunction("Resynchronized") != null)
            {
                CallFunction("Resynchronized");
            }
        }

        public void TransmissionStart()
        {
            if (CallDepth == 0 && BitSink != null)
            {
                CallDepth++;
                BitSink.TransmissionStart();
                CallDepth--;
            }
            else if (LuaVm.GetFunction("TransmissionStart") != null)
            {
                CallFunction("TransmissionStart");
            }
        }

        public void TransmissionEnd()
        {
            if (CallDepth == 0 && BitSink != null)
            {
                CallDepth++;
                BitSink.TransmissionEnd();
                CallDepth--;
            }
            else if (LuaVm.GetFunction("TransmissionEnd") != null)
            {
                CallFunction("TransmissionEnd");
            }
        }

        public void ClockBit(bool state)
        {
            if (CallDepth == 0 && BitSink != null)
            {
                CallDepth++;
                BitSink.ClockBit(state);
                CallDepth--;
            }
            else if (LuaVm.GetFunction("ClockBit") != null)
            {
                CallFunction("ClockBit", state);
            }
        }

        #endregion
    }
}
