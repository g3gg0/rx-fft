using System;
using System.ComponentModel;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Devices;
using LibRXFFT.Libraries.USB_RX.Misc;
using RX_FFT.Components.GDI;
using System.Threading;
using LibRXFFT.Libraries.Timers;
using System.IO;
using System.Collections;

namespace RX_Setup
{
    public partial class RXSetup : Form
    {
        private DateTime TestStartTime = DateTime.Now;
        private long TotalTransfers = 0;
        private Thread StatsThread;
        private USBRXDevice Device;
        private EEPROM EEPROMDevice = null;

        private double TransfersPerSecond = 0.0f;

        private Hashtable I2CDeviceNames = new Hashtable();

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

            I2CDeviceNames.Add(0x20, "(RX-USB Atmel)");
            I2CDeviceNames.Add(0x21, "(VUHF_RX Atmel)");
            I2CDeviceNames.Add(0x51, "(EEPROM)");
            I2CDeviceNames.Add(0x60, "(MT2131)");
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


        private void btnConnect_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ArrayList openedDevices = new ArrayList();

            if (e.Button == MouseButtons.Right)
            {
                for (int devNum = 0; devNum < 6; devNum++)
                {
                    Log.AddMessage("CUSB2DLL::LoadUsb2Dll() -> Usb2Dll loaded successfully!" );
                    Log.AddMessage("CUSB2DLL::Check(" + devNum + ") -> Usb2Dll loaded successfully!" );
                    
                    bool present = USBRXDeviceNative.UsbDevicePresent(devNum);
                    Log.AddMessage("CUSB2DLL::Check(" + devNum + ") -> UsbDevicePresent(" + devNum + ")=" + (present ? "1 -> Device present! " : "0 -> Device not present! ") );

                    if (present)
                    {
                        bool initialized = USBRXDeviceNative.UsbDeviceInitialized(devNum);
                        Log.AddMessage("CUSB2DLL::Check(" + devNum + ") -> UsbDeviceInitialized(" + devNum + ")=" + (initialized ? "1 -> Device initialized! " : "0 -> Device not initialized! ") );

                        Log.AddMessage("CUSB_RX1::CheckUSB_RX1DeviceList(): USB_RX1 at devNum=" + devNum + " available" + Environment.NewLine);

                        if (!initialized)
                        {
                            bool init = USBRXDeviceNative.UsbInit(devNum);
                            Log.AddMessage("CUSB2DLL::Init(" + devNum + ") -> UsbInit(" + devNum + ")=" + (init ? "1 -> Usb interface initialized sucessfully!" : "0 -> Usb interface not initialized!") );
                        }

                        bool open = USBRXDeviceNative.UsbOpen(devNum);
                        Log.AddMessage("CUSB2DLL::Init(" + devNum + ") -> UsbOpen(" + devNum + ")=" + (open ? "1 -> Usb device opened sucessfully!" : "0 -> Usb interface not opened!") );

                        byte[] param = new byte[1];
                        bool speed = USBRXDeviceNative.UsbCheckSpeed(devNum, param);
                        Log.AddMessage("CUSB2DLL::Init(" + devNum + ") -> UsbCheckSpeed(" + devNum + ",...)=" + ((param[0] != 0x00) ? "1 -> Usb interface is connected to a high-speed bus" : "0 -> Usb interface is connected to a low-speed bus ") );

                        openedDevices.Add(devNum);
                    }
                    else
                    {
                        Log.AddMessage("CUSB2DLL::~CUSB2DLL() -> Usb2Dll closed for [" + devNum + "]!" );
                    }
                }

                foreach (int devNum in openedDevices)
                {
                    byte data = 0x06;
                    bool sent = USBRXDeviceNative.UsbI2CWriteByte(devNum, 0x20, data);

                    Log.AddMessage("CUSB_RX1::I2CCommand(" + data + ") for m_DevNum=" + devNum + " returns " + (sent ? "1" : "0") );
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (Device == null)
            {
                txtStatus.Text = "Connecting...";

                Device = new USBRXDevice();
                Device.TunerCombination = USBRXDevice.eCombinationType.None;

                if (Form.ModifierKeys == Keys.Shift)
                {
                    Device.ShowConsole(true);
                    USBRXDeviceNative.UsbSetTimeout(0, USBRXDeviceNative.MODE_FORCEINIT);
                }

                if (!Device.Init())
                {
                    Log.AddMessage("Init failed");
                    Device = null;
                    return;
                }

                EEPROMDevice = new EEPROM(Device);

                Device.I2CSleep = 0;
                Device.I2CRetries = 0;
                Device.I2CSetTimeout(1, 0);
                btnConnect.Text = "Disconnect";
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

                EEPROMDevice = null;
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
                Log.AddMessage("Atmel programming...");
                if (!Device.AtmelProgrammer.SetProgrammingMode(true))
                {
                    Log.AddMessage("Programming mode failed");
                }
                else
                {
                    Log.AddMessage("  Device Name:  " + Device.AtmelProgrammer.DeviceName);

                    Log.AddMessage("  Vendor:       " + string.Format("{0:x4}", Device.AtmelProgrammer.VendorCode));
                    Log.AddMessage("  Family:       " + string.Format("{0:x4}", Device.AtmelProgrammer.FamilyCode));
                    Log.AddMessage("  Part No:      " + string.Format("{0:x4}", Device.AtmelProgrammer.PartNumberCode));

                    Log.AddMessage("  Fuses:        " + string.Format("{0:x4}", Device.AtmelProgrammer.FuseBits));
                    Log.AddMessage("  XFuses:       " + string.Format("{0:x4}", Device.AtmelProgrammer.XFuseBits));
                    Log.AddMessage("  Locks:        " + string.Format("{0:x4}", Device.AtmelProgrammer.LockBits));
                }
                if (!Device.AtmelProgrammer.SetProgrammingMode(false))
                {
                    Log.AddMessage("Normal mode failed");
                }

                Thread.Sleep(800);
                Log.AddMessage("");
                Log.AddMessage("Atmel commands...");
                Log.AddMessage("  Atmel serial: " + Device.Atmel.SerialNumber);
                Log.AddMessage("  AD6636 TCXO:  " + Device.Atmel.TCXOFreq);
                Log.AddMessage("  Filter count: " + Device.Atmel.GetFilterCount());
                Log.AddMessage("");

                Log.AddMessage("EEPROM...");
                DetectEEPROM();
                Log.AddMessage("");
            }
            catch (AtmelProgrammer.DeviceErrorException ex)
            {
                Log.AddMessage("Programming failed.");
            }
        }

        private void DetectEEPROM()
        {
            if (!EEPROMDevice.Exists)
            {
                Log.AddMessage("   No EEPROM found");
                return;
            }

            if (!EEPROMDevice.AutodetectAddressing())
            {
                Log.AddMessage("   Width:       FAILED!");
                return;
            }

            if (EEPROMDevice.AutodetectSize())
            {
                Log.AddMessage("   Width:       " + EEPROMDevice.AddressWidth + " bit");
                Log.AddMessage("   Size:        " + EEPROMDevice.Size + " byte");
            }
            else
            {
                Log.AddMessage("   Width:       " + EEPROMDevice.AddressWidth + " bit");
                Log.AddMessage("   Size:        FAILED!");
                return;
            }

            byte headerType = 0;

            if(!EEPROMDevice.ReadByte(0, ref headerType))
            {
                Log.AddMessage("   Header:      FAILED!");
            }

            switch (headerType)
            {
                case 0x00:
                case 0xFF:
                    Log.AddMessage("   Header:      Unprogrammed (" + headerType.ToString("X2") + ")");
                    break;

                case 0xC0:
                    Log.AddMessage("   Header:      without firmware (" + headerType.ToString("X2") + ")");
                    break;

                case 0xC2:
                    Log.AddMessage("   Header:      with firmware (" + headerType.ToString("X2") + ")");
                    break;

                default:
                    Log.AddMessage("   Header:      Illegal (" + headerType.ToString("X2") + ")");
                    break;
            }

        }

        private void btnFirmwareRead_Click(object sender, EventArgs e)
        {

            if (Device == null)
            {
                return;
            }

            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            if (!Device.AtmelProgrammer.SetProgrammingMode(true) || Device.AtmelProgrammer.VendorCode != 0x1E)
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

            if (Device == null)
            {
                return;
            }

            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            if (!Device.AtmelProgrammer.SetProgrammingMode(true) || Device.AtmelProgrammer.VendorCode != 0x1E)
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
        private int AtmelSPITestTransfers = 10;
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
            if (Device == null)
            {
                return;
            }

            
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

            if (Device == null)
            {
                return;
            }

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
                Log.AddMessage("AtmelDelay", "  Minimum SetFilterDelay:  " + Device.Atmel.SetFilterDelay + " ms");
                Log.AddMessage("AtmelDelay", "  Minimum ReadFilterDelay: " + Device.Atmel.ReadFilterDelay + " ms");
                Log.AddMessage("AtmelDelay", "  Minimum SetAgcDelay:     " + Device.Atmel.SetAgcDelay + " ms");
                Log.AddMessage("AtmelDelay", "  Minimum SetAttDelay:     " + Device.Atmel.SetAttDelay + " ms");
                Log.AddMessage("AtmelDelay", "  Minimum reset delay:     " + Device.AtmelProgrammer.RecoveryTime + " ms");
                Log.AddMessage("AtmelDelay", "---------------------------------");

                WorkerThread = null;
            });
            WorkerThread.Start();
        }



        private void btnI2cTest_Click(object sender, EventArgs e)
        {
            if (Device == null)
            {
                return;
            }

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

                    Log.AddMessage(" I²C ACK-Storm");
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    for (int tries = 0; tries < 1000; tries++)
                    {
                        Device.I2CDeviceAck(rnd.Next(0x7F));
                    }
                    I2CTestAck(0x51, "EEPROM");

                    for (int tries = 0; tries < 1000; tries++)
                    {
                        Device.I2CDeviceAck(rnd.Next(0x7F));
                    }
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

        private void btnI2cScan_Click(object sender, EventArgs e)
        {
            if (Device == null)
            {
                return;
            }

            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            WorkerThread = new Thread(() =>
            {
                AbortStressTest = false;

                Log.AddMessage(" I²C scan starting");
                Log.AddMessage("------------------------------------");
                try
                {
                    Log.AddMessage(" I²C ACK for ID:");
                    for (int dev = 0; dev < 0x80; dev++)
                    {
                        int tries = 0;
                        int success = 0;
                        string devName = "";

                        if (I2CDeviceNames.ContainsKey(dev))
                        {
                            devName = (string)I2CDeviceNames[dev];
                        }

                        for (tries = 0; tries < 50; tries++)
                        {
                            if (Device.I2CDeviceAck(dev))
                            {
                                success++;
                            }
                        }

                        if (success > 0)
                        {
                            if (success != tries)
                            {
                                Log.AddMessage("    0x" + dev.ToString("X2") + " " + devName + "      (" + (tries-success) + "/" + tries + " failed)" );
                            }
                            else
                            {
                                Log.AddMessage("    0x" + dev.ToString("X2") + " " + devName + " ");
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                Log.AddMessage("------------------------------------");
                Log.AddMessage(" I²C scan finished");

                WorkerThread = null;
            });

            WorkerThread.Start();
        }


        private void btnCypressEepromRead_Click(object sender, EventArgs e)
        {
            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            if (Device == null)
            {
                return;
            }

            string oldText = btnCypressEepromRead.Text;
            TotalTransfers = 0;
            TestStartTime = DateTime.Now;
            WorkerThread = new Thread(() =>
            {
                int blocksize = 16;
                byte[] buffer = new byte[blocksize];
                byte[] dumpData = new byte[EEPROMDevice.Size];
                int blocks = (int)(EEPROMDevice.Size / blocksize);
                int totalLen = blocks * blocksize;

                /* now copy hex dump data */
                for (int block = 0; block < blocks; block++)
                {
                    BeginInvoke(new Action(() =>
                    {
                        btnCypressEepromRead.Text = (uint)((block * 100) / blocks) + "%";
                    }));

                    EEPROMDevice.ReadBytes(block * blocksize, buffer);
                    Array.Copy(buffer, 0, dumpData, block * blocksize, blocksize);
                }


                BeginInvoke(new Action(() =>
                {
                    FileDialog dlg = new SaveFileDialog();

                    dlg.Filter = "Raw EEPROM Image (*.bin)|*.bin|All files (*.*)|*.*";
                    dlg.DefaultExt = ".bin";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        FileStream writeFile = null;
                        BinaryWriter writer = null;
                        try
                        {
                            writeFile = File.OpenWrite(dlg.FileName);
                            writer = new BinaryWriter(writeFile);

                            writer.Write(dumpData);
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

                BeginInvoke(new Action(() =>
                {
                    btnCypressEepromRead.Text = oldText;
                }));

                WorkerThread = null;
            });
            WorkerThread.Start();
        }

        private void btnCypressEepromProgram_Click(object sender, EventArgs e)
        {
            if (Device == null)
            {
                return;
            }

            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            FileDialog dlg = new OpenFileDialog();

            dlg.Filter = "HEX Firmware data (*.hex)|*.hex|Raw EEPROM Image (*.bin)|*.bin|All files (*.*)|*.*";
            dlg.DefaultExt = ".hex";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                MemoryDump8Bit dump;
                bool rawImage = false;

                try
                {
                    if (dlg.FileName.EndsWith(".hex"))
                    {
                        IntelHexFile hf = new IntelHexFile(dlg.FileName);
                        dump = hf.Parse();
                        rawImage = false;
                    }
                    else if (dlg.FileName.EndsWith(".bin"))
                    {
                        dump = new MemoryDump8Bit();
                        dump.StartAddress = 0;
                        dump.Data = File.ReadAllBytes(dlg.FileName);
                        rawImage = true;
                    }
                    else
                    {
                        MessageBox.Show("Unknown file type.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to read EEPROM file");
                    return;
                }

                string oldText = btnCypressEepromProgram.Text;
                TotalTransfers = 0;
                TestStartTime = DateTime.Now;
                WorkerThread = new Thread(() =>
                {
                    byte[] eepromBuffer;

                    if (rawImage)
                    {
                        eepromBuffer = dump.Data;
                    }
                    else
                    {
                        ArrayList buildBuffer = new ArrayList();

                        /* serialize the data to transfer */
                        {
                            BeginInvoke(new Action(() =>
                            {
                                btnCypressEepromProgram.Text = "Preparing...";
                            }));

                            /* add default usb ids, force disconnect after loading */
                            buildBuffer.AddRange(new byte[] { 0xC2, 0xB4, 0x04, 0x01, 0xEE, 0x00, 0x01, 0x41 });

                            /* split firmware into 1023 byte blocks */
                            int maxBlockSize = 1023;
                            int blocks = (int)((dump.Length + maxBlockSize - 1) / maxBlockSize);

                            for (int block = 0; block < blocks; block++)
                            {
                                /* either the rest of the dump or the maximum bloxk size */
                                int blockLength = (int)(Math.Min(dump.Length - (block * maxBlockSize), maxBlockSize));
                                int address = (int)(dump.StartAddress + block * maxBlockSize);
                                byte[] blockBuffer = new byte[blockLength];

                                Array.Copy(dump.Data, block * maxBlockSize, blockBuffer, 0, blockLength);

                                /* block header */
                                buildBuffer.AddRange(new byte[] { (byte)(blockLength >> 8), (byte)(blockLength & 0xFF), (byte)(address >> 8), (byte)(address & 0xFF) });
                                buildBuffer.AddRange(blockBuffer);
                            }

                            /* finally the last words to init CPUCS */
                            buildBuffer.AddRange(new byte[] { 0x80, 0x01, 0xE6, 0x00, 0x00 });
                        }

                        /* get the whole buffer into a plain array */
                        eepromBuffer = (byte[])buildBuffer.ToArray(typeof(byte));
                    }

                    /* first some safety check */
                    Log.AddMessage("Firmware requires " + eepromBuffer.Length + " bytes of EEPROM");
                    if (eepromBuffer.Length > EEPROMDevice.Size)
                    {
                        Log.AddMessage("Aborting. EEPROM is too small for this firmware.");
                    }
                    else
                    {
                        /* and transfer this in 16 byte blocks to EEPROM */
                        int blocksize = 16;
                        int blocks = (int)((eepromBuffer.Length + (blocksize - 1)) / blocksize);
                        int dataPos = 0;

                        for (int block = 0; block < blocks; block++)
                        {
                            int blockLength = Math.Min(eepromBuffer.Length - (block * blocksize), blocksize);
                            byte[] buffer = new byte[blockLength];

                            BeginInvoke(new Action(() =>
                            {
                                btnCypressEepromProgram.Text = (uint)((block * 100) / blocks) + "%";
                            }));

                            Array.Copy(eepromBuffer, dataPos, buffer, 0, blockLength);
                            EEPROMDevice.WriteBytes(dataPos, buffer);
                            dataPos += blockLength;
                        }
                    }

                    BeginInvoke(new Action(() =>
                    {
                        btnCypressEepromProgram.Text = oldText;
                    }));

                    WorkerThread = null;
                });
                WorkerThread.Start();
            }
            else
            {
                Log.AddMessage("Writing default EEPROM header");
                EEPROMDevice.WriteBytes(0, new byte[] { 0xC0, 0xB4, 0x04, 0x01, 0xEE, 0x00, 0x01, 0x01 });
            }
        }

        private void btnSetSerial_Click(object sender, EventArgs e)
        {
            if (Device == null)
            {
                return;
            }

            if (WorkerThread != null)
            {
                MessageBox.Show("Test already running. Stopping...");
                AbortStressTest = true;
                return;
            }

            SerialDialog dlg = new SerialDialog(Device.Atmel.SerialNumber, Device.Atmel.TCXOFreq);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Device.Atmel.SerialNumber = dlg.Serial;
                Device.Atmel.TCXOFreq = dlg.TCXOFreq;
            }
        }

        private void chkSlowI2C_CheckedChanged(object sender, EventArgs e)
        {
            if (Device != null)
            {
                Device.I2CSetSpeed(!chkSlowI2C.Checked);
            }
        }
    }
}
