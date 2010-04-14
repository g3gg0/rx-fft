using System;
using System.ComponentModel;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Devices;
using LibRXFFT.Libraries.USB_RX.Misc;
using RX_FFT.Components.GDI;
using System.Threading;
using LibRXFFT.Libraries.Timers;

namespace RX_Setup
{
    public partial class RXSetup : Form
    {
        USBRXDevice Device;

        public RXSetup()
        {
            InitializeComponent();
            Log.Init();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Device != null)
            {
                if (Device.AtmelProgrammer != null)
                {
                    Device.AtmelProgrammer.leaveProgrammingMode();
                }
                Device.Close();
                Device = null;
            }

            base.OnClosing(e);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (Device == null)
            {
                txtStatus.Text = "Connecting...";

                Device = new USBRXDevice();
                Device.ShowConsole(true);
                Device.I2CRetries = 0;

                if (!Device.Init())
                {
                    Log.AddMessage("Init failed");
                    Device = null;
                }
                else
                {
                    btnConnect.Text = "Disconnect";
                }
            }
            else
            {
                AbortStressTest = true;
                if (TestThread != null)
                {
                    TestThread.Abort();
                    TestThread.Join(50);
                    TestThread = null;
                }

                Device.Close();
                Device = null;

                btnConnect.Text = "Connect";
            }
            UpdateStatus();

        }

        private void UpdateStatus()
        {
            string status = "Not connected";

            if (Device != null)
            {
                status = "Connected";
            }

            txtStatus.Text = status;
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            if (Device == null)
            {
                return;
            }

            try
            {
                Log.AddMessage("Programming mode...");
                if (!Device.AtmelProgrammer.enterProgrammingMode())
                {
                    Log.AddMessage("Programming mode failed");
                    return;
                }
                Log.AddMessage("  Device Name:  " + Device.AtmelProgrammer.DeviceName);

                Log.AddMessage("  Vendor:  " + string.Format("{0:x4}", Device.AtmelProgrammer.readDeviceVendorCode()));
                Log.AddMessage("  Family:  " + string.Format("{0:x4}", Device.AtmelProgrammer.readDeviceFamilyCode()));
                Log.AddMessage("  Part No: " + string.Format("{0:x4}", Device.AtmelProgrammer.readDevicePartNumberCode()));

                Log.AddMessage("  Fuses:   " + string.Format("{0:x4}", Device.AtmelProgrammer.readFuseBits()));
                Log.AddMessage("  XFuses:  " + string.Format("{0:x4}", Device.AtmelProgrammer.readXFuseBits()));
                Log.AddMessage("  Locks:   " + string.Format("{0:x4}", Device.AtmelProgrammer.readLockBits()));

                if (!Device.AtmelProgrammer.leaveProgrammingMode())
                {
                    Log.AddMessage("Normal mode failed");
                    return;
                }

                Thread.Sleep(750);
                Log.AddMessage("");
                Log.AddMessage("Normal mode...");
                Log.AddMessage("  Atmel Serial: " + Device.Atmel.SerialNumber);
                Log.AddMessage("  AD6636 TCXO:  " + Device.Atmel.TCXOFreq);
                Log.AddMessage("");

                // 
            }
            catch (AtmelProgrammer.DeviceErrorException ex)
            {
                Log.AddMessage("Programming failed.");
            }
        }

        private void btnFirmwareRead_Click(object sender, EventArgs e)
        {
            MemoryDump data = Device.AtmelProgrammer.readFlash(Device.AtmelProgrammer.FlashStart, Device.AtmelProgrammer.FlashSize);


        }

        private void btnFirmwareProgram_Click(object sender, EventArgs e)
        {
            FileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".hex";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                IntelHexFile hf = new IntelHexFile(dlg.FileName);
                byte[] hex = hf.Parse();
            }
        }

        private bool AbortStressTest = false;
        private Thread TestThread = null;
        private int I2CTestTransfers = 500;
        private int AtmelSPITestTransfers = 100;
        private int AtmelSerialTestTransfers = 500;
        private int AtmelMixedTestTransfers = 5000;
        private int AtmelAD6636MixedTestTransfers = 500;
        private int AD6636MixedTestTransfers = 50;        
        

        private void I2CTestRead(int dev, string name, int length)
        {
            int transfer = 0;
            int failed = 0;
            byte[] buffer = new byte[length];

            while (transfer < I2CTestTransfers)
            {
                transfer++;
                if (!Device.I2CReadBytes(dev, buffer))
                {
                    failed++;
                }
            }

            if (failed > 0)
            {
                Log.AddMessage("   - " + name + ":    [FAIL]");
                Log.AddMessage("   Failed: " + failed + "/" + I2CTestTransfers + " failed");
                return;
            }
            Log.AddMessage("   - " + name + ":    [OK]");
        }

        private void I2CTestAck(int dev, string name)
        {
            int transfer = 0;
            int failed = 0;

            while (transfer < I2CTestTransfers)
            {
                transfer++;
                if (!Device.I2CDeviceAck(dev))
                {
                    failed++;
                }
            }

            if (failed > 0)
            {
                Log.AddMessage("   - " + name + ":    [FAIL]");
                Log.AddMessage("      Failed: " + failed + "/" + I2CTestTransfers + " failed");
                return;
            }
            Log.AddMessage("   - " + name + ":    [OK]");
        }

        private void AtmelSPITest()
        {
            int transfer = 0;
            int failed = 0;
            string name = null;

            while (transfer < AtmelSPITestTransfers)
            {
                transfer++;
                try
                {
                    if (!Device.AtmelProgrammer.enterProgrammingMode())
                    {
                        Log.AddMessage("Programming mode failed");
                        return;
                    }

                    if (Device.AtmelProgrammer.readDeviceVendorCode() != 0x1E)
                    {
                        failed++;
                    }
                    else if (name == null)
                    {
                        name = Device.AtmelProgrammer.DeviceName;
                    }

                    if (!Device.AtmelProgrammer.leaveProgrammingMode())
                    {
                        Log.AddMessage("Normal mode failed");
                        return;
                    }
                }
                catch (AtmelProgrammer.DeviceErrorException ex)
                {
                    failed++;
                }
            }
            Device.AtmelProgrammer.leaveProgrammingMode();

            /* let atmel recover... */
            Thread.Sleep(750);

            if (failed > 0)
            {
                Log.AddMessage("   - [FAIL]");
                Log.AddMessage("      Failed: " + failed + "/" + AtmelSPITestTransfers + " failed");
                return;
            }
            Log.AddMessage("   - [OK] (" + name + ")");
        }


        private void AtmelSerialTest()
        {
            int transfer = 0;
            int failed = 0;

            while (transfer < AtmelSerialTestTransfers)
            {
                transfer++;

                string serial = Device.Atmel.SerialNumber;

                if (serial == null || serial == "")
                {
                    failed++;
                }
            }

            if (failed > 0)
            {
                Log.AddMessage("   - [FAIL]");
                Log.AddMessage("      Failed: " + failed + "/" + AtmelSerialTestTransfers + " failed");
                return;
            }
            Log.AddMessage("   - [OK]");
        }

        private void AtmelMixedTest()
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            int transfer = 0;
            int failed = 0;

            while (transfer < AtmelMixedTestTransfers)
            {
                bool success = true;

                transfer++;

                switch (rnd.Next(6))
                {
                    case 0:
                        string serial = Device.Atmel.SerialNumber;
                        if (serial == null || serial == "")
                        {
                            success = false;
                        }
                        break;
                    case 1:
                        success &= Device.Atmel.SetRfSource(USBRXDevice.eRfSource.RF1);
                        break;
                    case 2:
                        success &= Device.Atmel.SetRfSource(USBRXDevice.eRfSource.RF2);
                        break;
                    case 3:
                        success &= Device.Atmel.SetRfSource(USBRXDevice.eRfSource.Tuner);
                        break;
                    case 4:
                        success &= Device.Atmel.FIFOReset(false);
                        break;
                    case 5:
                        success &= Device.Atmel.FIFOReset(true);
                        break;
                }

                if (!success)
                {
                    failed++;
                }
            }

            if (failed > 0)
            {
                Log.AddMessage("   - [FAIL]");
                Log.AddMessage("      Failed: " + failed + "/" + AtmelMixedTestTransfers + " failed");
                return;
            }
            Log.AddMessage("   - [OK]");
        }

        private void AD6636MixedTest()
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            int transfer = 0;
            int failed = 0;

            while (transfer < AD6636MixedTestTransfers)
            {
                bool success = true;

                transfer++;

                switch (rnd.Next(4))
                {
                    case 0:
                        success &= Device.AD6636.SetFrequency(rnd.Next(40000000));
                        break;
                    case 1:
                        success &= Device.AD6636.SoftSync();
                        break;
                    case 2:
                        success &= Device.AD6636.SetMgcValue(rnd.Next(96));
                        break;
                    case 3:
                        success &= Device.AD6636.SelectChannel(rnd.Next(4));
                        break;
                }

                if (!success)
                {
                    failed++;
                }
            }

            if (failed > 0)
            {
                Log.AddMessage("   - [FAIL]");
                Log.AddMessage("      Failed: " + failed + "/" + AD6636MixedTestTransfers + " failed");
                return;
            }
            Log.AddMessage("   - [OK]");
        }

        private RingBuffer<string> LastTransfers = new RingBuffer<string>(8);
        private HighPerformanceCounter Counter = null;
        private void AddLastTransfer(string action)
        {
            lock (this)
            {
                if (Counter == null)
                {
                    Counter = new HighPerformanceCounter("Transfer Debug");
                    Counter.Start();
                }

                Counter.Stop();
                double diff = Counter.Duration * 1000;
                Counter.Start();

                LastTransfers.Add("+" + diff.ToString("  0.000 ms") + " " + action);
            }
        }

        private void DumpLastTransfers()
        {
            lock (this)
            {
                Log.AddMessage(" Last transfers: ");
                foreach (string transfer in LastTransfers)
                {
                    Log.AddMessage("  " + transfer);
                }
                Log.AddMessage("        ^ this one failed. maybe previous one took too long");
            }
        }


        private void AtmelAD6636ExtendedMixedTest()
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            int transfer = 0;
            int failed = 0;
            int filterCount = Device.Atmel.GetFilterCount();

            while (transfer < AtmelAD6636MixedTestTransfers)
            {
                bool success = true;

                transfer++;

                switch (rnd.Next(20))
                {
                    case 0:
                        AddLastTransfer("Read Serial");
                        string serial = Device.Atmel.SerialNumber;
                        if (serial == null || serial == "")
                        {
                            success = false;
                        }
                        break;
                    case 1:
                        AddLastTransfer("SetRfSource(USBRXDevice.eRfSource.RF1");
                        success &= Device.Atmel.SetRfSource(USBRXDevice.eRfSource.RF1);
                        break;
                    case 2:
                        AddLastTransfer("SetRfSource(USBRXDevice.eRfSource.RF2");
                        success &= Device.Atmel.SetRfSource(USBRXDevice.eRfSource.RF2);
                        break;
                    case 3:
                        AddLastTransfer("SetRfSource(USBRXDevice.eRfSource.Tuner");
                        success &= Device.Atmel.SetRfSource(USBRXDevice.eRfSource.Tuner);
                        break;
                    case 4:
                        AddLastTransfer("FIFOReset(false)");
                        success &= Device.Atmel.FIFOReset(false);
                        break;
                    case 5:
                        AddLastTransfer("FIFOReset(true)");
                        success &= Device.Atmel.FIFOReset(true);
                        break;
                    case 6:
                        AddLastTransfer("AD6636ReadReg(0, 4)");
                        if (Device.Atmel.AD6636ReadReg(0, 4) == -1)
                        {
                            success = false;
                        }
                        break;
                    case 7:
                        AddLastTransfer("Atmel.ReadFilter(rnd.Next(filterCount))");
                        success &= Device.Atmel.ReadFilter(rnd.Next(filterCount));
                        break;
                    case 8:
                        AddLastTransfer("Atmel.SetFilter(rnd.Next(filterCount))");
                        success &= Device.Atmel.SetFilter(rnd.Next(filterCount));
                        break;
                    case 9:
                        AddLastTransfer("AD6636.SetFrequency(rnd.Next(40000000)");
                        success &= Device.AD6636.SetFrequency(rnd.Next(40000000));
                        break;
                    case 10:
                        AddLastTransfer("AD6636.SoftSync()");
                        success &= Device.AD6636.SoftSync();
                        break;
                    case 11:
                        AddLastTransfer("AD6636.SetMgcValue(rnd.Next(96))");
                        success &= Device.AD6636.SetMgcValue(rnd.Next(96));
                        break;
                    case 12:
                        AddLastTransfer("AD6636.SelectChannel(rnd.Next(4))");
                        success &= Device.AD6636.SelectChannel(rnd.Next(4));
                        break;
                    case 13:
                        AddLastTransfer("SetAgc(USBRXDevice.eAgcType.Off)");
                        success &= Device.SetAgc(USBRXDevice.eAgcType.Off);
                        break;
                    case 14:
                        AddLastTransfer("SetAgc(USBRXDevice.eAgcType.Manual)");
                        success &= Device.SetAgc(USBRXDevice.eAgcType.Manual);
                        break;
                    case 15:
                        AddLastTransfer("SetAgc(USBRXDevice.eAgcType.Fast)");
                        success &= Device.SetAgc(USBRXDevice.eAgcType.Fast);
                        break;
                    case 16:
                        AddLastTransfer("SetAgc(USBRXDevice.eAgcType.Medium)");
                        success &= Device.SetAgc(USBRXDevice.eAgcType.Medium);
                        break;
                    case 17:
                        AddLastTransfer("SetAgc(USBRXDevice.eAgcType.Slow)");
                        success &= Device.SetAgc(USBRXDevice.eAgcType.Slow);
                        break;
                    case 18:
                        AddLastTransfer("SetMgc(rnd.Next(96)");
                        success &= Device.SetMgc(rnd.Next(96));
                        break;
                    case 19:
                        AddLastTransfer("(multi) SetAgc/SetMgc");
                        success &= Device.SetAgc(USBRXDevice.eAgcType.Manual);
                        success &= Device.SetMgc(rnd.Next(96));
                        success &= Device.SetAgc(USBRXDevice.eAgcType.Manual);
                        success &= Device.SetMgc(rnd.Next(96));
                        success &= Device.SetAgc(USBRXDevice.eAgcType.Manual);
                        success &= Device.SetMgc(rnd.Next(96));
                        success &= Device.SetAgc(USBRXDevice.eAgcType.Manual);
                        success &= Device.SetMgc(rnd.Next(96));
                        break;
                }

                if (!success)
                {
                    DumpLastTransfers();
                    failed++;
                }
            }

            if (failed > 0)
            {
                Log.AddMessage(" - [FAIL]");
                Log.AddMessage("      Failed: " + failed + "/" + AtmelAD6636MixedTestTransfers + " failed");
                return;
            }
            Log.AddMessage(" - [OK]");
        }



        private void btnStress_Click(object sender, EventArgs e)
        {
            if (TestThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            TestThread = new Thread(() =>
            {
                AbortStressTest = false;

                Log.AddMessage(" Stress test starting");
                Log.AddMessage("------------------------------------");
                try
                {
                    Log.AddMessage("");
                    Log.AddMessage(" I²C ACK");
                    I2CTestAck(0x51, "EEPROM");
                    I2CTestAck(0x20, "Atmel ");
                    Log.AddMessage("");
                    Log.AddMessage(" I²C Read (1 byte)");
                    I2CTestRead(0x51, "EEPROM", 1);
                    I2CTestRead(0x20, "Atmel ", 1);
                    Log.AddMessage("");
                    Log.AddMessage(" I²C Read (8 byte)");
                    I2CTestRead(0x51, "EEPROM", 8);
                    I2CTestRead(0x20, "Atmel ", 8);
                    Log.AddMessage("");
                    Log.AddMessage(" I²C Read (32 byte)");
                    I2CTestRead(0x51, "EEPROM", 32);
                    I2CTestRead(0x20, "Atmel ", 32);

                    Log.AddMessage("");
                    Log.AddMessage(" Atmel SPI programming");
                    AtmelSPITest();

                    Log.AddMessage("");
                    Log.AddMessage(" Atmel serial number");
                    AtmelSerialTest();

                    Log.AddMessage("");
                    Log.AddMessage(" Atmel mixed access");
                    AtmelMixedTest();

                    Log.AddMessage("");
                    Log.AddMessage(" AD6636 mixed access");
                    AD6636MixedTest();

                    Log.AddMessage("");
                    Log.AddMessage(" Atmel+AD6636 extended mixed access");
                    Log.AddMessage(" (press stress test button to finish test)");
                     
                    Semaphore finished = new Semaphore(0, 2);
                    new Thread(() =>
                    {
                        while (!AbortStressTest)
                        {
                            AtmelAD6636ExtendedMixedTest();
                        }
                    
                        lock (finished)
                        {
                            finished.Release(1);
                        }
                    
                    }).Start();
                    new Thread(() =>
                    {
                        while (!AbortStressTest)
                        {
                            AtmelAD6636ExtendedMixedTest();
                        }

                        lock (finished)
                        {
                            finished.Release(1);
                        }
                    }).Start();

                    finished.WaitOne();
                    finished.WaitOne();

                }
                catch (Exception)
                {
                }
                Log.AddMessage("------------------------------------");
                Log.AddMessage(" Stress test finished");

                TestThread = null;
            });

            TestThread.Start();

        }


    }
}
