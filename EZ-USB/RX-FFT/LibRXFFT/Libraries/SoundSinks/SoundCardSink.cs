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

        public SoundCardSink(Control displayControl)
        {
            SoundCardSinkControl ctrl = new SoundCardSinkControl(this);
            displayControl.Controls.Add(ctrl);
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

        public long SamplingRate
        {
            get
            {
                return 0;
            }
            set
            {
                if (SoundDevice != null)
                {
                    SoundDevice.SetInputRate((int)value);
                }
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

        public void Process(double[] samples)
        {
            if (SoundDevice != null)
            {
                SoundDevice.Write(samples);
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
