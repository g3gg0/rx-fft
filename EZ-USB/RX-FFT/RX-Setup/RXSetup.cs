using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Devices;
using RX_FFT.Components.GDI;
using LibRXFFT.Libraries.USB_RX.Misc;

namespace RX_Setup
{
    public partial class RXSetup : Form
    {
        USBRXDevice Device;
        AtmelProgrammer Atmel;

        public RXSetup()
        {
            InitializeComponent();
            Log.Init();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Atmel != null)
            {
                Atmel.leaveProgrammingMode();
                Atmel = null;
            }

            if (Device != null)
            {
                Device.Close();
                Device = null;
            }

            base.OnClosing(e);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            IntelHexFile hf = new IntelHexFile("rx11.hex");

            byte[] hex = hf.Parse();

            return;

            Device = new USBRXDevice();
            if (!Device.Init())
            {
                Log.AddMessage("Init failed");
                Device = null;
            }

            Log.AddMessage("Atmel Serial: " + Device.Atmel.SerialNumber);
            Log.AddMessage("AD6636 TCXO:  " + Device.Atmel.TCXOFreq);
            Log.AddMessage("");

            try
            {
                Atmel = new AtmelProgrammer(Device);
                Log.AddMessage("Programming Mode...");
                if (!Atmel.enterProgrammingMode())
                {
                    Log.AddMessage("Programming Mode failed");
                    return;
                }
                Log.AddMessage("Vendor:  " + string.Format("{0:x4}", Atmel.readDeviceVendorCode()));
                Log.AddMessage("Family:  " + string.Format("{0:x4}", Atmel.readDeviceFamilyCode()));
                Log.AddMessage("Part No: " + string.Format("{0:x4}", Atmel.readDevicePartNumberCode()));

                Log.AddMessage("Fuses:   " + string.Format("{0:x4}", Atmel.readFuseBits()));
                Log.AddMessage("XFuses:  " + string.Format("{0:x4}", Atmel.readLockBits()));
                Log.AddMessage("Locks:   " + string.Format("{0:x4}", Atmel.readLockBits()));

                MemoryDump data = Atmel.readFlash(0, Atmel.FlashSize);
            }
            catch (AtmelProgrammer.DeviceErrorException ex)
            {
                Log.AddMessage("Programming failed.");
            }
        }
    }
}
