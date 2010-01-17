using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.SampleSources;
using System.Threading;
using LibRXFFT.Libraries.ShmemChain;

namespace RX_Oscilloscope
{
    public partial class RXOscilloscope : Form
    {
        private SampleSource SampleSource;
        private Thread ProcessThread;
        private bool Processing;
        public int SharedMemoryChannel = 0;

        public RXOscilloscope()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            CloseSource();

            e.Cancel = false;
            base.OnClosing(e);
        }

        public void CloseSource()
        {
            if (ProcessThread != null)
            {
                Processing = false;
                ProcessThread.Join(500);
                ProcessThread.Abort();
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
            scope.SamplingRate = SampleSource.OutputSamplingRate;

            SampleSource.SamplesPerBlock = (int)Math.Min(1024, SampleSource.OutputSamplingRate / 50);
        }

        private void ProcessMain()
        {
            while (Processing)
            {
                if (SampleSource.Read())
                {
                    for (int pos = 0; pos < SampleSource.SamplesRead; pos++)
                    {
                        double I = SampleSource.SourceSamplesI[pos];
                        double Q = SampleSource.SourceSamplesQ[pos];

                        scope.Process(I, Q);
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
                btnOpen.ContextMenu = menu;
                btnOpen.ContextMenu.Show(btnOpen, new System.Drawing.Point(10, 10));
            }
        }

        public void OpenSharedMem(int srcChan)
        {
            SampleSource = new ShmemSampleSource("RX-Oscilloscope", srcChan, 1, 0);

            SampleSource.DataFormat = LibRXFFT.Libraries.ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;
            SampleSource.SamplingRateChanged += new EventHandler(SampleSource_SamplingRateChanged);

            Processing = true;
            ProcessThread = new Thread(ProcessMain);
            ProcessThread.Start();

            btnOpen.Text = "Close";
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
    }
}
