using System.Threading;
using LibRXFFT.Libraries.Demodulators;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.SignalProcessing;
using System;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.SoundSinks;
using System.Windows.Forms;
using System.Collections.Generic;
using LibRXFFT.Components.GDI;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class DemodulationState
    {
        public enum eSquelchState
        {
            Unknown,
            Open,
            Closed
        }

        public enum eSourceFrequency
        {
            Center = 0,
            Fixed = 1,
            Cursor = 2,
            Marker = 3,
            Selection = 4
        }


        public DemodulationDialog Dialog = null;
        public event EventHandler DataUpdated;
        public bool ReinitSound = false;
        public double InputRate = 0;
        public double AudioRate
        {
            get
            {
                return InputRate / InputSignalDecimation / AudioDecimation;
            }
        }

        public eSourceFrequency SourceFrequency = eSourceFrequency.Center;
        public long DemodulationFrequencyCenter = 0;
        public long DemodulationFrequencyFixed = 0;
        public long DemodulationFrequencyCursor = 0;
        public long DemodulationFrequencyMarker = 0;
        public long DemodulationFrequencySelection = 0;

        public long DemodulationFrequency
        {
            get
            {
                switch (SourceFrequency)
                {
                    case eSourceFrequency.Center:
                        return DemodulationFrequencyCenter;
                    case eSourceFrequency.Fixed:
                        return DemodulationFrequencyFixed;
                    case eSourceFrequency.Cursor:
                        return DemodulationFrequencyCursor;
                    case eSourceFrequency.Marker:
                        return DemodulationFrequencyMarker;
                    case eSourceFrequency.Selection:
                        return DemodulationFrequencySelection;
                }

                return 0;
            }
        }
        public long BaseFrequency = 0;
        
        public string Description = null;

        public bool AudioAmplificationEnabled = false;
        public double AudioAmplification = 1.0f;

        public int AudioDecimation = 1;

        public bool DemodulationEnabled = false;
        public bool DemodulationPossible = false;

        public Demodulator SignalDemodulator = new AMDemodulator();
        public Downmixer DemodulationDownmixer = new Downmixer();
        public Downmixer SSBDownmixer = new Downmixer();
        public Filter SSBLowPassI = new FIRFilter(FIRCoefficients.FIRLowPass_2);
        public Filter SSBLowPassQ = new FIRFilter(FIRCoefficients.FIRLowPass_2);

        public DemodFFTView DemodView = null;

        public bool SquelchEnabled = false;
        public double SquelchLowerLimit = -25;
        public double SquelchAverage = -75;
        public double SquelchMax = -65;
        public eSquelchState SquelchState = eSquelchState.Open;
        public long SquelchSampleCounter = 0;
        public long SquelchSampleCount = 50;



        public bool AudioLowPassEnabled = false;
        public int AudioLowPassWidthFract = 2;
        public Filter _AudioLowPass = new FIRFilter(FIRCoefficients.FIRLowPass_8);
        public Filter AudioLowPass
        {
            get { return _AudioLowPass; }
            set
            {
                if (_AudioLowPass != null)
                {
                    _AudioLowPass.Dispose();
                }

                _AudioLowPass = value;
            }
        }

        public bool BandwidthLimiter = false;
        public int BandwidthLimiterFract = 1;

        public int DemodulatorFiltering
        {
            get
            {
                if (SignalDemodulator is SSBDemodulator)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
        }
        public int InputSignalDecimation
        {
            get
            {
                if (!BandwidthLimiter)
                    return 1;

                return BandwidthLimiterFract;
            }
        }

        protected Filter _CursorWindowFilterI;
        protected Filter _CursorWindowFilterQ;
        public ManualResetEvent[] CursorWindowFilterEvents = new ManualResetEvent[2];
        public event EventHandler CursorWindowFilterChanged;
        public FilterThread CursorWindowFilterThreadI;
        public FilterThread CursorWindowFilterThreadQ;

        public Filter CursorWindowFilterI
        {
            get { return _CursorWindowFilterI; }
            set
            {
                lock (this)
                {
                    if (_CursorWindowFilterI != null)
                    {
                        _CursorWindowFilterI.Dispose();
                    }

                    _CursorWindowFilterI = value;
                    if (CursorWindowFilterThreadI != null)
                        CursorWindowFilterThreadI.Stop();
                    CursorWindowFilterThreadI = new FilterThread(value);
                    CursorWindowFilterEvents[0] = CursorWindowFilterThreadI.DataProcessed;

                    if (CursorWindowFilterChanged != null)
                    {
                        CursorWindowFilterChanged(this, null);
                    }
                }
            }
        }

        public Filter CursorWindowFilterQ
        {
            get { return _CursorWindowFilterQ; }
            set
            {
                lock (this)
                {
                    if (_CursorWindowFilterI != null)
                    {
                        _CursorWindowFilterI.Dispose();
                    }

                    _CursorWindowFilterQ = value;
                    if (CursorWindowFilterThreadQ != null)
                        CursorWindowFilterThreadQ.Stop();
                    CursorWindowFilterThreadQ = new FilterThread(value);
                    CursorWindowFilterEvents[1] = CursorWindowFilterThreadQ.DataProcessed;
                }
            }
        }

        public DemodulationState()
        {
            CursorWindowFilterI = new FIRFilter(FIRCoefficients.FIRLowPass_4);
            CursorWindowFilterQ = new FIRFilter(FIRCoefficients.FIRLowPass_4);
        }

        public void Dispose()
        {
            if (CursorWindowFilterThreadQ != null)
                CursorWindowFilterThreadQ.Dispose();
            if (CursorWindowFilterThreadI != null)
                CursorWindowFilterThreadI.Dispose();
            if (CursorWindowFilterI != null)
                CursorWindowFilterI.Dispose();
            if (CursorWindowFilterI != null)
                CursorWindowFilterI.Dispose();
        }

        private DateTime LastListenerUpdate = DateTime.Now;

        public void UpdateListeners()
        {
            lock (this)
            {
                if ((DateTime.Now - LastListenerUpdate).TotalMilliseconds > 100)
                {
                    if (DataUpdated != null)
                    {
                        DataUpdated(this, null);
                    }
                    LastListenerUpdate = DateTime.Now;
                }
            }
        }
        
        public LinkedList<SoundSinkInfo> SoundSinkInfos = new LinkedList<SoundSinkInfo>();


        public void UpdateSinks()
        {
            lock (SoundSinkInfos)
            {
                foreach (SoundSinkInfo info in SoundSinkInfos)
                {
                    if (DemodulationEnabled)
                    {
                        info.Sink.Start();
                    }
                    else
                    {
                        info.Sink.Stop();
                    }
                }
            }
        }

        public void AddSink(SoundSinkInfo info)
        {
            lock (SoundSinkInfos)
            {
                SoundSinkInfos.AddLast(info);
                UpdateSinks();
            }
        }

        public void RemoveSink(SoundSinkInfo info)
        {
            lock (SoundSinkInfos)
            {
                SoundSinkInfos.Remove(info);
                info.Sink.Stop();
                info.Sink.Shutdown();
                UpdateSinks();
            }
        }

        public void RemoveSinks()
        {
            lock (SoundSinkInfos)
            {
                while (SoundSinkInfos.Count > 0)
                {
                    RemoveSink(SoundSinkInfos.First.Value);
                }
            }
        }
    }

    public struct SoundSinkInfo
    {
        public SinkTab Page;
        public SoundSink Sink;
        public DemodulationDialog DemodDialog;
    }
}
