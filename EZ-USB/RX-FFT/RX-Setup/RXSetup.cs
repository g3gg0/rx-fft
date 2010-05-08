using System;
using System.ComponentModel;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Devices;
using LibRXFFT.Libraries.USB_RX.Misc;
using RX_FFT.Components.GDI;
using System.Threading;
using LibRXFFT.Libraries.Timers;
using System.IO;

namespace RX_Setup
{
    public partial class RXSetup : Form
    {
        private DateTime TestStartTime = DateTime.Now;
        private long TotalTransfers = 0;
        private Thread StatsThread;
        private USBRXDevice Device;

        private double TransfersPerSecond = 0.0f;

        public RXSetup()
        {
            InitializeComponent();
            Log.Init();

            StatsThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);

                    TransfersPerSecond = (TotalTransfers / (DateTime.Now - TestStartTime).TotalMilliseconds) * 1000;
                    BeginInvoke(new Action(() => lblStats.Text = "Transfers/s: " + TransfersPerSecond.ToString("0.00")));
                }
            });
            StatsThread.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (WorkerThread != null)
            {
                WorkerThread.Abort();
                WorkerThread = null;
            }

            if (Device != null)
            {
                if (Device.AtmelProgrammer != null)
                {
                    Device.AtmelProgrammer.SetProgrammingMode(false);
                }
                Device.Close();
                Device = null;
            }

            StatsThread.Abort();
            base.OnClosing(e);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (Device == null)
            {
                txtStatus.Text = "Connecting...";

                Device = new USBRXDevice();
                Device.ShowConsole(true);
                Device.TunerCombination = USBRXDevice.eCombinationType.None;

                if (!Device.Init())
                {
                    Log.AddMessage("Init failed");
                    Device = null;
                }
                else
                {
                    Device.I2CSleep = 0;
                    Device.I2CRetries = 0;
                    Device.I2CSetTimeout(1, 0);
                    btnConnect.Text = "Disconnect";
                }
            }
            else
            {
                AbortStressTest = true;
                if (WorkerThread != null)
                {
                    WorkerThread.Abort();
                    WorkerThread.Join(50);
                    WorkerThread = null;
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
                if (!Device.AtmelProgrammer.SetProgrammingMode(true))
                {
                    Log.AddMessage("Programming mode failed");
                }
                else
                {
                    Log.AddMessage("  Device Name:  " + Device.AtmelProgrammer.DeviceName);

                    Log.AddMessage("  Vendor:  " + string.Format("{0:x4}", Device.AtmelProgrammer.VendorCode));
                    Log.AddMessage("  Family:  " + string.Format("{0:x4}", Device.AtmelProgrammer.FamilyCode));
                    Log.AddMessage("  Part No: " + string.Format("{0:x4}", Device.AtmelProgrammer.PartNumberCode));

                    Log.AddMessage("  Fuses:   " + string.Format("{0:x4}", Device.AtmelProgrammer.FuseBits));
                    Log.AddMessage("  XFuses:  " + string.Format("{0:x4}", Device.AtmelProgrammer.XFuseBits));
                    Log.AddMessage("  Locks:   " + string.Format("{0:x4}", Device.AtmelProgrammer.LockBits));
                }
                if (!Device.AtmelProgrammer.SetProgrammingMode(false))
                {
                    Log.AddMessage("Normal mode failed");
                }

                Thread.Sleep(800);
                Log.AddMessage("");
                Log.AddMessage("Normal mode...");
                Log.AddMessage("  Atmel Serial: " + Device.Atmel.SerialNumber);
                Log.AddMessage("  AD6636 TCXO:  " + Device.Atmel.TCXOFreq);
                Log.AddMessage("");
            }
            catch (AtmelProgrammer.DeviceErrorException ex)
            {
                Log.AddMessage("Programming failed.");
            }
        }

        private void btnFirmwareRead_Click(object sender, EventArgs e)
        {
            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            if (Device == null || !Device.AtmelProgrammer.SetProgrammingMode(true) || Device.AtmelProgrammer.VendorCode != 0x1E)
            {
                MessageBox.Show("Failed to enter programming mode.");
                return;
            }

            string oldText = btnFirmwareRead.Text;
            TotalTransfers = 0;
            TestStartTime = DateTime.Now;
            WorkerThread = new Thread(() =>
            {
                MemoryDump16BitLE data = Device.AtmelProgrammer.ReadFlash(Device.AtmelProgrammer.FlashStart, Device.AtmelProgrammer.FlashSize, (AtmelProgrammer.BlockProcessInfo info) =>
                {
                    TotalTransfers++;
                    BeginInvoke(new Action(() =>
                    {
                        btnFirmwareRead.Text = (uint)((info.BlockNum * 100) / info.BlockCount) + "%";
                    }));
                    if (AbortStressTest)
                    {
                        info.Cancel = true;
                    }
                });

                BeginInvoke(new Action(() =>
                {
                    btnFirmwareRead.Text = oldText;
                }));

                Device.AtmelProgrammer.SetProgrammingMode(false);

                if (data == null)
                {
                    MessageBox.Show("Read was aborted");
                    return;
                }

                BeginInvoke(new Action(() =>
                {
                    FileDialog dlg = new SaveFileDialog();

                    dlg.DefaultExt = ".bin";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        FileStream writeFile = null;
                        BinaryWriter writer = null;
                        try
                        {
                            writeFile = File.OpenWrite(dlg.FileName);
                            writer = new BinaryWriter(writeFile);

                            writer.Write(((MemoryDump8Bit)data).Data);
                            writer.Close();
                        }
                        catch (Exception ex)
                        {
                            if (writer != null)
                            {
                                writer.Close();
                            }
                            else if (writeFile != null)
                            {
                                writeFile.Close();
                            }
                            MessageBox.Show("Could not save the file. Reason: " + ex.GetType().ToString(), "Saving failed");
                        }
                    }

                }));

                WorkerThread = null;
            });

            WorkerThread.Start();
        }

        private void btnFirmwareProgram_Click(object sender, EventArgs e)
        {
            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            if (Device == null || !Device.AtmelProgrammer.SetProgrammingMode(true) || Device.AtmelProgrammer.VendorCode != 0x1E)
            {
                MessageBox.Show("Failed to enter programming mode.");
                return;
            }

            FileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".hex";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                MemoryDump16BitLE dump;
                try
                {
                    IntelHexFile hf = new IntelHexFile(dlg.FileName);
                    dump = hf.Parse();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to read the hex-file");
                    return;
                }

                string oldText = btnFirmwareProgram.Text;
                TotalTransfers = 0;
                TestStartTime = DateTime.Now;
                WorkerThread = new Thread(() =>
                {
                    Device.AtmelProgrammer.ProgramFlash(dump, (AtmelProgrammer.BlockProcessInfo info) =>
                    {
                        TotalTransfers++;

                        if (AbortStressTest)
                        {
                            info.Cancel = true;
                        }

                        BeginInvoke(new Action(() =>
                        {
                            btnFirmwareProgram.Text = (uint)((info.BlockNum * 100) / info.BlockCount) + "%";
                        }));
                    });
                    BeginInvoke(new Action(() =>
                    {
                        btnFirmwareProgram.Text = oldText;
                    }));

                    Device.AtmelProgrammer.SetProgrammingMode(false);
                    WorkerThread = null;
                });
                WorkerThread.Start();
            }
        }

        private bool AbortStressTest = false;
        private Thread WorkerThread = null;
        private int I2CTestTransfers = 500;
        private int AtmelSPITestTransfers = 100;
        private int AtmelSerialTestTransfers = 500;
        private int AtmelMixedTestTransfers = 5000;
        private int AtmelAD6636MixedTestTransfers = 200;
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

        private bool ResetAtmelSleep()
        {
            /* this one may fail if there is no programming option */
            if (!Device.AtmelProgrammer.SetProgrammingMode(true))
            {
                Device.AtmelProgrammer.SetProgrammingMode(false);
                return true;
            }

            Device.AtmelProgrammer.SetProgrammingMode(false);

            string serial = Device.Atmel.SerialNumber;

            if (serial == null || serial == "")
            {
                return false;
            }

            return true;
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
                    if (!Device.AtmelProgrammer.SetProgrammingMode(true))
                    {
                        Log.AddMessage("   - [FAIL]");
                        Log.AddMessage("      Failed:  Programming mode failed");
                        Device.AtmelProgrammer.SetProgrammingMode(false);
                        return;
                    }

                    if (Device.AtmelProgrammer.VendorCode != 0x1E)
                    {
                        failed++;
                    }
                    else if (name == null)
                    {
                        name = Device.AtmelProgrammer.DeviceName;
                    }

                    if (!Device.AtmelProgrammer.SetProgrammingMode(false))
                    {
                        Log.AddMessage("   - [FAIL]");
                        Log.AddMessage("      Failed:  Normal mode failed");
                        return;
                    }
                }
                catch (AtmelProgrammer.DeviceErrorException ex)
                {
                    failed++;
                }
            }
            Device.AtmelProgrammer.SetProgrammingMode(false);

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
                TotalTransfers++;

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
                TotalTransfers++;

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
                lock (this)
                {
                    TotalTransfers++;
                }

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
                        lock (this)
                        {
                            TotalTransfers += 7;
                        }
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
            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            WorkerThread = new Thread(() =>
            {
                AbortStressTest = false;

                Log.AddMessage(" Stress test starting");
                Log.AddMessage("------------------------------------");
                try
                {
                    Log.AddMessage("");
                    Log.AddMessage(" Atmel+AD6636 extended mixed access");
                    Log.AddMessage(" (press stress test button to abort test)");

                    TotalTransfers = 0;
                    TestStartTime = DateTime.Now;

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

                WorkerThread = null;
            });

            WorkerThread.Start();
        }

        private void btnAtmelDelay_Click(object sender, EventArgs e)
        {
            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            WorkerThread = new Thread(() =>
            {
                int filterCount = Device.Atmel.GetFilterCount();
                int stepSize = 40;
                int smallSteps = 0;
                int delay = 50;
                int lastSuccessDelay = 0;
                bool lastSucceeded = true;

                Log.AddMessage("AtmelDelay", "Atmel SetFilter delay test");

                /* do 6 tries with 1ms increment */
                while (smallSteps < 6)
                {
                    bool success = true;
                    Device.Atmel.SetFilterDelay = delay;

                    for (int loop = 0; loop < 1; loop++)
                    {
                        for (int filter = 0; filter < filterCount; filter++)
                        {
                            success &= Device.Atmel.SetFilter(filter);
                            if (!success)
                            {
                                break;
                            }
                        }
                        if (!success)
                        {
                            break;
                        }
                    }

                    if (success)
                    {
                        lastSucceeded = true;
                        lastSuccessDelay = delay;
                        delay -= stepSize;
                        delay = Math.Max(0, delay);
                        Log.AddMessage("AtmelDelay", "Transmission succeeded. Trying with " + delay + " ms");
                    }
                    else
                    {
                        Thread.Sleep(150);

                        delay += stepSize;
                        Log.AddMessage("AtmelDelay", "Transmission failed. Trying with " + delay + " ms");

                        /* decrease step size if failed, but the delay before was fine */
                        if (lastSucceeded)
                        {
                            stepSize /= 2;
                            stepSize = Math.Max(1, stepSize);
                        }
                        lastSucceeded = false;
                    }

                    if (stepSize == 1 || delay == 0)
                    {
                        smallSteps++;
                    }
                }
                Device.Atmel.SetFilterDelay = lastSuccessDelay;


                stepSize = 40;
                smallSteps = 0;
                delay = 50;
                lastSuccessDelay = 0;
                lastSucceeded = true;
                Log.AddMessage("AtmelDelay", "Atmel SetAgc delay test");

                /* do 6 tries with 1ms increment */
                while (smallSteps < 6)
                {
                    bool success = true;
                    Device.Atmel.ReadFilterDelay = delay;

                    for (int loop = 0; loop < 1; loop++)
                    {
                        for (int filter = 0; filter < filterCount; filter++)
                        {
                            success &= Device.Atmel.ReadFilter(filter);
                            if (!success)
                            {
                                break;
                            }
                        }
                        if (!success)
                        {
                            break;
                        }
                    }

                    if (success)
                    {
                        lastSucceeded = true;
                        lastSuccessDelay = delay;
                        delay -= stepSize;
                        delay = Math.Max(0, delay);
                        Log.AddMessage("AtmelDelay", "Transmission succeeded. Trying with " + delay + " ms");
                    }
                    else
                    {
                        Thread.Sleep(150);

                        delay += stepSize;
                        Log.AddMessage("AtmelDelay", "Transmission failed. Trying with " + delay + " ms");

                        /* decrease step size if failed, but the delay before was fine */
                        if (lastSucceeded)
                        {
                            stepSize /= 2;
                            stepSize = Math.Max(1, stepSize);
                        }
                        lastSucceeded = false;
                    }

                    if (stepSize == 1 || delay == 0)
                    {
                        smallSteps++;
                    }
                }
                Device.Atmel.ReadFilterDelay = lastSuccessDelay;


                stepSize = 40;
                smallSteps = 0;
                delay = 50;
                lastSuccessDelay = 0;
                lastSucceeded = true;
                Log.AddMessage("AtmelDelay", "Atmel SetAgc delay test");

                /* do 6 tries with 1ms increment */
                while (smallSteps < 6)
                {
                    bool success = true;
                    Device.Atmel.SetAgcDelay = delay;

                    for (int loop = 0; loop < 2; loop++)
                    {
                        success &= Device.Atmel.SetAgc(USBRXDevice.eAgcType.Fast);
                        success &= Device.Atmel.SetAgc(USBRXDevice.eAgcType.Manual);
                        success &= Device.Atmel.SetAgc(USBRXDevice.eAgcType.Medium);
                        success &= Device.Atmel.SetAgc(USBRXDevice.eAgcType.Off);
                        success &= Device.Atmel.SetAgc(USBRXDevice.eAgcType.Slow);
                        if (!success)
                        {
                            break;
                        }
                    }

                    if (success)
                    {
                        lastSucceeded = true;
                        lastSuccessDelay = delay;
                        delay -= stepSize;
                        delay = Math.Max(0, delay);
                        Log.AddMessage("AtmelDelay", "Transmission succeeded. Trying with " + delay + " ms");
                    }
                    else
                    {                        
                        Thread.Sleep(250);

                        delay += stepSize;
                        Log.AddMessage("AtmelDelay", "Transmission failed. Trying with " + delay + " ms");

                        /* decrease step size if failed, but the delay before was fine */
                        if (lastSucceeded)
                        {
                            stepSize /= 2;
                            stepSize = Math.Max(1, stepSize);
                        }
                        lastSucceeded = false;
                    }

                    if (stepSize == 1 || delay == 0)
                    {
                        smallSteps++;
                    }
                }
                Device.Atmel.SetAgcDelay = lastSuccessDelay;


                stepSize = 40;
                smallSteps = 0;
                delay = 50;
                lastSuccessDelay = 0;
                lastSucceeded = true;
                Log.AddMessage("AtmelDelay", "Atmel SetAtt delay test");

                /* do 6 tries with 1ms increment */
                while (smallSteps < 6)
                {
                    bool success = true;
                    Device.Atmel.SetAttDelay = delay;

                    for (int loop = 0; loop < 10; loop++)
                    {
                        success &= Device.Atmel.SetAtt(true);
                        success &= Device.Atmel.SetAtt(false);
                        success &= Device.Atmel.SetAtt(0);
                        success &= Device.Atmel.SetAtt(1);
                        if (!success)
                        {
                            break;
                        }
                    }

                    if (success)
                    {
                        lastSucceeded = true;
                        lastSuccessDelay = delay;
                        delay -= stepSize;
                        delay = Math.Max(0, delay);
                        Log.AddMessage("AtmelDelay", "Transmission succeeded. Trying with " + delay + " ms");
                    }
                    else
                    {
                        Thread.Sleep(250);

                        delay += stepSize;
                        Log.AddMessage("AtmelDelay", "Transmission failed. Trying with " + delay + " ms");

                        /* decrease step size if failed, but the delay before was fine */
                        if (lastSucceeded)
                        {
                            stepSize /= 2;
                            stepSize = Math.Max(1, stepSize);
                        }
                        lastSucceeded = false;
                    }

                    if (stepSize == 1 || delay == 0)
                    {
                        smallSteps++;
                    }
                }
                Device.Atmel.SetAttDelay = lastSuccessDelay;



                stepSize = 100;
                smallSteps = 0;
                delay = 750;
                lastSuccessDelay = 0;
                lastSucceeded = true;
                Log.AddMessage("AtmelDelay", "Atmel reset delay test");

                if (!Device.AtmelProgrammer.SetProgrammingMode(true))
                {
                    Log.AddMessage("AtmelDelay", "  -> skipping due to missing hardware option");
                    Device.AtmelProgrammer.SetProgrammingMode(false);
                }
                else
                {
                    /* do 6 tries with 1ms increment */
                    while (smallSteps < 6)
                    {
                        bool success = true;
                        Device.AtmelProgrammer.RecoveryTime = delay;

                        for (int loop = 0; loop < 2; loop++)
                        {
                            success &= ResetAtmelSleep();
                            if (!success)
                            {
                                break;
                            }
                        }

                        if (success)
                        {
                            lastSucceeded = true;
                            lastSuccessDelay = delay;
                            delay -= stepSize;
                            delay = Math.Max(0, delay);
                            Log.AddMessage("AtmelDelay", "Transmission succeeded. Trying with " + delay + " ms");
                        }
                        else
                        {
                            Thread.Sleep(800);

                            delay += stepSize;
                            Log.AddMessage("AtmelDelay", "Transmission failed. Trying with " + delay + " ms");

                            /* decrease step size if failed, but the delay before was fine */
                            if (lastSucceeded)
                            {
                                stepSize /= 2;
                                stepSize = Math.Max(1, stepSize);
                            }
                            lastSucceeded = false;
                        }

                        if (stepSize == 1 || delay == 0)
                        {
                            smallSteps++;
                        }
                    }

                    Device.AtmelProgrammer.RecoveryTime = lastSuccessDelay;
                }


                Log.AddMessage("AtmelDelay", "");
                Log.AddMessage("AtmelDelay", "Test results:");
                Log.AddMessage("AtmelDelay", "---------------------------------");
                Log.AddMessage("AtmelDelay", "  Minimum SetFilterDelay: " + Device.Atmel.SetFilterDelay + " ms");
                Log.AddMessage("AtmelDelay", "  Minimum ReadFilterDelay: " + Device.Atmel.ReadFilterDelay + " ms");
                Log.AddMessage("AtmelDelay", "  Minimum SetAgcDelay: " + Device.Atmel.SetAgcDelay + " ms");
                Log.AddMessage("AtmelDelay", "  Minimum SetAttDelay: " + Device.Atmel.SetAttDelay + " ms");
                Log.AddMessage("AtmelDelay", "  Minimum reset delay: " + Device.AtmelProgrammer.RecoveryTime + " ms");
                Log.AddMessage("AtmelDelay", "---------------------------------");

                WorkerThread = null;
            });
            WorkerThread.Start();
        }



        private void btnI2cTest_Click(object sender, EventArgs e)
        {
            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            WorkerThread = new Thread(() =>
            {
                AbortStressTest = false;

                Log.AddMessage(" I²C test starting");
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
                    
                    TotalTransfers = 0;
                    TestStartTime = DateTime.Now;
                    Log.AddMessage("");
                    Log.AddMessage(" Atmel mixed access");
                    AtmelMixedTest();
                    Log.AddMessage("   - Transfers/s: " + TransfersPerSecond.ToString("0.00"));


                    TotalTransfers = 0;
                    TestStartTime = DateTime.Now;
                    Log.AddMessage("");
                    Log.AddMessage(" AD6636 mixed access");
                    AD6636MixedTest();
                    Log.AddMessage("   - Transfers/s: " + TransfersPerSecond.ToString("0.00"));

                }
                catch (Exception)
                {
                }
                Log.AddMessage("------------------------------------");
                Log.AddMessage(" I²C test finished");

                WorkerThread = null;
            });

            WorkerThread.Start();
        }
    }
}
