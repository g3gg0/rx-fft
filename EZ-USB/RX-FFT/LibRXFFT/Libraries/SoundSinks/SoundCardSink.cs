using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using LibRXFFT.Libraries.SoundDevices;
using LibRXFFT.Components.GDI;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.SoundSinks
{
    public class SoundCardSink : SoundSink
    {
        private DXSoundDevice SoundDevice = null;
        private Guid SelectedDevice = Guid.Empty;
        private SoundCardSinkControl Control = null;
        private Control DisplayControl = null;
        private Resampler Oversampler = new Resampler(1);
        private int OutputRate = 96000;

        public decimal OversamplingFactor { get { return  OutputRate / (decimal)_SamplingRate; } }

        public SoundCardSink(Control displayControl)
        {
            Control = new SoundCardSinkControl(this);
            Control.Dock = DockStyle.Fill;
            DisplayControl = displayControl;
            DisplayControl.Controls.Add(Control);
        }

        public DeviceInfo[] GetDevices()
        {
            return DXSoundDevice.GetDevices();
        }

        public void InitDevice(Guid device)
        {
            SelectedDevice = device;

            if (SoundDevice != null)
            {
                Stop();
                Start();
            }
        }

        #region SoundSink Member

        public double _SamplingRate = 0;
        public double SamplingRate
        {
            get
            {
                if (SoundDevice != null)
                {
                    return _SamplingRate;
                }

                return 0;
            }
            set
            {
                if (SoundDevice != null)
                {
                    _SamplingRate = value;

                    OutputRate = (int) _SamplingRate;
                    Oversampler.Oversampling = OversamplingFactor;
                    Oversampler.Type = eResamplingType.SinC;
                    Oversampler.SinCDepth = 4;
                    SoundDevice.SetInputRate(OutputRate);
                }
            }
        }

        public int BufferSize
        {
            get
            {
                if (SoundDevice != null)
                {
                    return SoundDevice.BufferSize;
                }
                return 0;
            }
        }

        public int BufferUsage
        {
            get
            {
                if (SoundDevice != null)
                {
                    return SoundDevice.BufferUsage;
                }
                return 0;
            }
        }

        public void Start()
        {
            if (SoundDevice == null)
            {
                SquelchState = DemodulationState.eSquelchState.Open;

                Form newForm = new Form();
                newForm.Show();
                newForm.SetDesktopLocation(8192, 8192);
                newForm.Visible = false;

                SoundDevice = new DXSoundDevice(newForm.Handle, SelectedDevice);
                SoundDevice.Start();

                newForm.Close();
            }
        }

        public void Stop()
        {
            if (SoundDevice != null)
            {
                SoundDevice.Stop();
                SoundDevice = null;
            }
        }

        public void Shutdown()
        {
            Stop();
            Control.Shutdown();
            Control = null;
        }

        bool DynamicResample = false;
        public void Process(double[] samples)
        {
            if (SoundDevice != null)
            {
                decimal bufferLoad = (decimal)BufferUsage / (decimal)BufferSize;
                decimal midDelta = 0.5m - bufferLoad;

                Oversampler.Oversampling = OversamplingFactor;

                /* start resampling when out of range */
                if(bufferLoad > 0.8m || bufferLoad< 0.2m)
                {
                    DynamicResample = true;
                }
                if (Math.Abs(midDelta) < 0.1m)
                {
                    DynamicResample = false;
                }

                if (DynamicResample)
                {
                    Oversampler.Oversampling = OversamplingFactor + 1.0m * midDelta;
                }

                double[] ret = Oversampler.Resample(samples);
                SoundDevice.Write(ret);
            }
        }

        public string Status
        {
            get
            {
                if (SoundDevice != null)
                {
                    return SoundDevice.Status;
                }

                return "(Idle)";
            }
        }

        public string Description
        {
            set 
            { 
                /* not used */
            }
        }

        private DemodulationState.eSquelchState _SquelchState = DemodulationState.eSquelchState.Open;
        public DemodulationState.eSquelchState SquelchState
        {
            get
            {
                return _SquelchState;
            }

            set
            {
                if (SoundDevice != null)
                {
                    if (SquelchState == DemodulationState.eSquelchState.Closed && value == DemodulationState.eSquelchState.Open)
                    {
                        SoundDevice.Start();
                    }

                    if (SquelchState == DemodulationState.eSquelchState.Open && value == DemodulationState.eSquelchState.Closed)
                    {
                        SoundDevice.Stop();
                    }
                }

                _SquelchState = value;
            }
        }

        #endregion
    }
}
