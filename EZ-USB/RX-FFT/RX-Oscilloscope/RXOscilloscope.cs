using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.ShmemChain;
using RX_Oscilloscope.Components;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Components.DeviceControls;

namespace RX_Oscilloscope
{
    public partial class RXOscilloscope : Form
    {
        private DeviceControl Source;
        private SampleSource SampleSource;
        private Thread ProcessThread;
        private bool Processing;
        public int SharedMemoryChannel = 0;
        private bool WindowActivated = false;

        public RXOscilloscope()
        {
            InitializeComponent();

            iqPlot.waveForm.UserEventCallback = UserEventCallbackFunc;
            scope.waveForm.UserEventCallback = UserEventCallbackFunc;

            scope.iqPlot = iqPlot;

            AddUserEventCallback(eUserEvent.MouseEnter);
            AddUserEventCallback(eUserEvent.MouseClickRight);
        }

        public void AddUserEventCallback(eUserEvent evt)
        {
            iqPlot.waveForm.EventActions[evt] = eUserAction.UserCallback;
            scope.waveForm.EventActions[evt] = eUserAction.UserCallback;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            CloseSource();

            e.Cancel = false;
            base.OnClosing(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            WindowActivated = true;
            FocusHovered();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            WindowActivated = false;
        }

        public void FocusHovered()
        {
            if (iqPlot.waveForm.MouseHovering)
            {
                iqPlot.Focus();
            }
            if (scope.waveForm.MouseHovering)
            {
                scope.Focus();
            }
        }

        public void UserEventCallbackFunc(eUserEvent evt, double param)
        {
            switch (evt)
            {
                /* when mouse is moved into a plot and we are in foreground, update focus to hovered plot */
                case eUserEvent.MouseEnter:
                    if (WindowActivated)
                    {
                        FocusHovered();
                    }
                    break;

                /* bring up popup menu. has to be improved */
                case eUserEvent.MouseClickRight:
                    break;
            }
        }

        public void CloseSource()
        {
            if (ProcessThread != null)
            {
                Processing = false;
                ProcessThread.Join(1000);
                ProcessThread = null;
            }
            if (SampleSource != null)
            {
                SampleSource.Close();
                SampleSource = null;
            }
        }

        void SampleSource_SamplingRateChanged(object sender, EventArgs e)
        {
            iqPlot.SamplingRate = SampleSource.OutputSamplingRate;
            scope.SamplingRate = SampleSource.OutputSamplingRate;

            SampleSource.SamplesPerBlock = (int)Math.Min(1024, SampleSource.OutputSamplingRate / 50);
        }

        private void ProcessMain()
        {
            while (Processing)
            {
                if (SampleSource.Read())
                {
                    if (SampleSource.SamplesRead > 0)
                    {
                        lock (SampleSource.SampleBufferLock)
                        {
                            for (int pos = 0; pos < SampleSource.SamplesRead; pos++)
                            {
                                double I = SampleSource.SourceSamplesI[pos];
                                double Q = SampleSource.SourceSamplesQ[pos];

                                //iqPlot.Process(I, Q);
                                scope.Process(I, Q);
                            }
                        }
                    }
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (SampleSource != null)
            {
                CloseSource();
                btnOpen.Text = "Open";
            }
            else
            {
                ContextMenu menu = new ContextMenu();

                menu.MenuItems.Add(new MenuItem("Shared Memory", new EventHandler(btnOpen_SharedMemory)));
                menu.MenuItems.Add(new MenuItem("IQ Wave File", new EventHandler(btnOpen_IQFile)));
                menu.MenuItems.Add(new MenuItem("Network Source", new EventHandler(btnOpen_NetworkSource)));
                btnOpen.ContextMenu = menu;
                btnOpen.ContextMenu.Show(btnOpen, new System.Drawing.Point(10, 10));
            }
        }

        public void OpenSharedMem(int srcChan)
        {
            try
            {
                SampleSource = new ShmemSampleSource("RX-Oscilloscope", srcChan, 1, 0);

                SampleSource.DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPointLE;
                SampleSource.SamplingRateChanged += new EventHandler(SampleSource_SamplingRateChanged);

                Processing = true;
                ProcessThread = new Thread(ProcessMain);
                ProcessThread.Start();

                btnOpen.Text = "Close";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open shared memory. Is the correct shmemchain.dll in the program directory?");
            }
        }

        private void btnOpen_IQFile(object sender, EventArgs e)
        {
            try
            {
                Source = new FileSourceDeviceControl(1);
                Source.OpenTuner();
                if (!Source.Connected)
                {
                    return;
                }

                SampleSource = Source.SampleSource;
                SampleSource.SamplingRateChanged += new EventHandler(SampleSource_SamplingRateChanged);

                Processing = true;
                ProcessThread = new Thread(ProcessMain);
                ProcessThread.Start();

                btnOpen.Text = "Close";
            }
            catch (DllNotFoundException ex)
            {
                MessageBox.Show("There is no shmemchain.dll in your working directory.", "Error while setting up shmem");
            }
            catch (BadImageFormatException ex)
            {
                MessageBox.Show("There is a wrong shmemchain.dll in your working directory.", "Error while setting up shmem");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception occured while trying to set up shmem: " + ex.GetType(), "Error while setting up shmem");
            }
        }

        public void btnOpen_NetworkSource(object sender, EventArgs e)
        {
            try
            {
                Source = new NetworkDeviceControl();
                Source.OpenTuner();
                if (!Source.Connected)
                {
                    return;
                }

                SampleSource = Source.SampleSource;
                SampleSource.SamplingRateChanged += new EventHandler(SampleSource_SamplingRateChanged);

                Processing = true;
                ProcessThread = new Thread(ProcessMain);
                ProcessThread.Start();

                btnOpen.Text = "Close";
            }
            catch (BadImageFormatException ex)
            {
                MessageBox.Show("There is a wrong shmemchain.dll in your working directory.", "Error while setting up shmem");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception occured while trying to set up shmem: " + ex.GetType(), "Error while setting up shmem");
            }
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

        private void btnOpen_SharedMemory(object sender, EventArgs e)
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

                btnOpen.ContextMenu = menu;
                btnOpen.ContextMenu.Show(btnOpen, new System.Drawing.Point(10, 10));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to init shared memory. Is the correct shmemchain.dll in the program directory?");
            }
        }
    }
}
