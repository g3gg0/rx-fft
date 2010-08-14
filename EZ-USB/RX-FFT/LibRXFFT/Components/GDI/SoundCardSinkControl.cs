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

namespace LibRXFFT.Components.GDI
{
    public partial class SoundCardSinkControl : UserControl
    {
        private SoundCardSink Sink;

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
        }

        protected void lstDevice_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            Sink.InitDevice(((DeviceInfo)lstDevice.SelectedItem).Guid);
        }


    }
}
