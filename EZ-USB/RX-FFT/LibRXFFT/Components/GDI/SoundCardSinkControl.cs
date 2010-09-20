using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.SoundSinks;
using LibRXFFT.Libraries.SoundDevices;
using System.Threading;
using LibRXFFT.Libraries.Timers;

namespace LibRXFFT.Components.GDI
{
    public partial class SoundCardSinkControl : UserControl
    {
        private SoundCardSink Sink;
        private AccurateTimer BufferStatsTimer = null;

        public SoundCardSinkControl(SoundCardSink sink)
        {
            Sink = sink;
            InitializeComponent();

            DeviceInfo[] infos = Sink.GetDevices();

            foreach (DeviceInfo info in infos)
            {
                lstDevice.Items.Add(info);
            }

            lstDevice.Text = "Default";

            waveForm.ScaleUnit = "%";
            waveForm.ScalePosMax = 100;
            waveForm.ScalePosMin = 0;
            waveForm.YZoomFactorMin = 0.001;
            waveForm.MaxSamples = 400;

            BufferStatsTimer = new AccurateTimer((object sender, EventArgs e) => 
            {
                lock (waveForm)
                {
                    if (Sink.BufferSize > 0)
                    {
                        waveForm.ProcessData(((float)Sink.BufferUsage * 100.0f) / Sink.BufferSize, true);
                    }
                }
            });
            BufferStatsTimer.Interval = 100;
            BufferStatsTimer.Start();
        }

        protected void lstDevice_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            Sink.InitDevice(((DeviceInfo)lstDevice.SelectedItem).Guid);
        }

        internal void Shutdown()
        {
            BufferStatsTimer.Stop();
            Dispose();
        }

        private void btnClearPlot_Click(object sender, EventArgs e)
        {
            lock (waveForm)
            {
                waveForm.Clear();
            }
        }
    }
}
