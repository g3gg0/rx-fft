using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Devices;
using System.Threading;

namespace FirmwarePreloader
{
    public partial class FirmwarePreloader : Form
    {
        private static int MaxDevices = 16;

        public FirmwarePreloader()
        {
            InitializeComponent();
        }

        private void btnPreload_Click(object sender, EventArgs e)
        {
            int initialized = 0;

            btnPreload.Enabled = false;

            Thread preloadThread = new Thread(() =>
            {
                USBRXDeviceNative.UsbSetTimeout(0, USBRXDeviceNative.MODE_FORCEINIT);

                for (int devNum = 0; devNum < MaxDevices; devNum++)
                {
                    bool init = true;
                    int inits = 0;

                    BeginInvoke(new Action(() =>
                    {
                        btnPreload.Text = "Preload ID " + devNum;
                    }));

                    while (init)
                    {
                        init = USBRXDeviceNative.UsbInit(devNum);

                        if (init)
                        {
                            inits++;
                            initialized++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (inits == 0)
                    {
                        break;
                    }
                    else
                    {
                        USBRXDeviceNative.UsbClose(devNum);
                    }
                }

                BeginInvoke(new Action(() =>
                {
                    btnPreload.Enabled = true;
                    btnPreload.Text = "Preload Firmware";

                    if (initialized > 0)
                    {
                        MessageBox.Show("Preloaded " + initialized + " devices with firmware");
                    }
                    else
                    {
                        MessageBox.Show("No devices to preload found");
                    }
                }));
                
                for (int devNum = 0; devNum < MaxDevices; devNum++)
                {
                    USBRXDeviceNative.UsbClose(devNum);
                }                
            });

            preloadThread.Start();
        }

        internal static void PreloadAll()
        {
            int initialized = 0;

            USBRXDeviceNative.UsbSetTimeout(0, USBRXDeviceNative.MODE_FORCEINIT);

            for (int devNum = 0; devNum < MaxDevices; devNum++)
            {
                bool init = true;
                int inits = 0;

                while (init)
                {
                    init = USBRXDeviceNative.UsbInit(devNum);

                    if (init)
                    {
                        inits++;
                        initialized++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (inits == 0)
                {
                    break;
                }
                else
                {
                    USBRXDeviceNative.UsbClose(devNum);
                }
            }

            for (int devNum = 0; devNum < MaxDevices; devNum++)
            {
                USBRXDeviceNative.UsbClose(devNum);
            }    
        }
    }
}
