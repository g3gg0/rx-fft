using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RX_Analyzer.Views;
using System.Threading;
using LibRXFFT.Components.DeviceControls;
using LibRXFFT.Libraries.ShmemChain;

namespace RX_Analyzer
{
    public partial class AnalyzerForm : Form
    {
        private DeviceControl Device;
        private Thread ProcessThread;
        private bool ProcessThreadRun;
        
        public AnalyzerForm()
        {
            InitializeComponent();
        }

        private void InitDevice(DeviceControl dev)
        {
            if (!dev.Connected)
            {
                MessageBox.Show("Failed to open the device. Reason: " + dev.ErrorMessage);
                return;
            }

            Device = dev;

            dev.FrequencyChanged += new EventHandler(Device_FrequencyChanged);
            dev.SamplingRateChanged += new EventHandler(Device_RateChanged);
            dev.FilterWidthChanged += new EventHandler(Device_FilterWidthChanged);
            dev.DeviceDisappeared += new EventHandler(Device_DeviceDisappeared);
            dev.DeviceClosed += new EventHandler(Device_DeviceClosed);
            dev.TransferMode = LibRXFFT.Libraries.eTransferMode.Stream;

            ProcessThreadRun = true;
            ProcessThread = new Thread(ProcessThreadFunction);
            ProcessThread.Start();
        }

        void ProcessThreadFunction()
        {
            while (ProcessThreadRun)
            {
                Device.ReadBlock();

                lock (Device.SampleSource.SampleBufferLock)
                {
                    double[] inputI = Device.SampleSource.SourceSamplesI;
                    double[] inputQ = Device.SampleSource.SourceSamplesQ;

                    for (int pos = 0; pos < inputI.Length; pos++)
                    {
                        foreach (Form f in MdiChildren)
                        {
                            if (f is SampleSink)
                            {
                                ((SampleSink)f).Process(inputI[pos], inputQ[pos]);
                            }
                        }
                    }
                }
            }
        }


        void Device_DeviceClosed(object sender, EventArgs e)
        {
            CloseDevice();
        }

        void Device_DeviceDisappeared(object sender, EventArgs e)
        {
            CloseDevice();
        }

        private void CloseDevice()
        {
            if (ProcessThread != null)
            {
                ProcessThread.Abort();
                ProcessThread = null;
            }

            if (Device != null)
            {
                Device.CloseControl();
                Device = null;
            }
        }

        void Device_InvertedSpectrumChanged(object sender, EventArgs e)
        {
        }

        void Device_TransferModeChanged(object sender, EventArgs e)
        {
        }

        void Device_FilterWidthChanged(object sender, EventArgs e)
        {
        }

        void Device_RateChanged(object sender, EventArgs e)
        {
        }

        void Device_FrequencyChanged(object sender, EventArgs e)
        {
        }

        private void newSignalPlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SignalStrengthPlot plot = new SignalStrengthPlot();
            plot.MdiParent = this;
            plot.Show();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitDevice(new FileSourceDeviceControl());
        }

        public void OpenSharedMem(int srcChan)
        {
            InitDevice(new SharedMemDeviceControl(srcChan));
        }

        private MenuItem btnOpen_SharedMemoryCreateMenuItem(string name, int srcChan)
        {
            MenuItem item;

            if (srcChan < 0)
            {
                item = new MenuItem("No data from <" + name + ">");
                item.Enabled = false;
            }
            else
            {
                item = new MenuItem("Channel " + srcChan + " from <" + name + ">",
                new EventHandler(delegate(object sender, EventArgs e)
                {
                    OpenSharedMem(srcChan);
                }));
            }

            return item;
        }

        private void sharedMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ContextMenu menu = new ContextMenu();
                NodeInfo[] infos = SharedMem.GetNodeInfos();

                foreach (NodeInfo info in infos)
                {
                    MenuItem item = btnOpen_SharedMemoryCreateMenuItem(info.name, info.dstChan);
                    menu.MenuItems.Add(item);
                }

                if (infos.Length == 0)
                {
                    MenuItem item = new MenuItem("(No nodes found)");
                    item.Enabled = false;
                    menu.MenuItems.Add(item);
                }

                menu.Show(this, new System.Drawing.Point(10, 10));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to init shared memory. Is the correct shmemchain.dll in the program directory? (" + ex.ToString() + ")");
            }
        }

        private void newEyePlotToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
