using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SampleSources;
using LibRXFFT.Libraries.ShmemChain;
using RX_FFT.Components.GDI;
using DemodulatorCollection.BitClockSinks;
using DemodulatorCollection.Demodulators;
using DemodulatorCollection.Interfaces;

namespace DemodulatorCollection
{
    public partial class DemodulatorDialog : Form
    {
        private SampleSource SampleSource;
        private Thread ProcessThread;
        private bool Processing;
        private DigitalDemodulator Demodulator;

        public DemodulatorDialog()
        {
            InitializeComponent();
            Log.Init();
            txtPulseDelay.Text = PDDemodulator.Description;
            txtPulseKeying.Text = PKDemodulator.Description;
            txtAmplitudeShiftKeying.Text = ASKDemodulator.Description;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            CloseDemod();
            CloseSource();

            e.Cancel = false;
            base.OnClosing(e);
        }

        void btnOpenPulseKeying_Click(object sender, EventArgs e)
        {
            CloseDemod();

            if (SampleSource == null)
                return;

            DigitalDemodulator demod = new PKDemodulator();
            demod.SamplingRate = SampleSource.OutputSamplingRate;
            demod.Init();
            Demodulator = demod;
        }

        private void btnOpenPulseDelay_Click(object sender, EventArgs e)
        {
            CloseDemod();

            if(SampleSource==null)
                return;

            DigitalDemodulator demod = new PDDemodulator();
            demod.SamplingRate = SampleSource.OutputSamplingRate;
            demod.Init();
            Demodulator = demod;
        }

        private void btnOpenASK_Click(object sender, EventArgs e)
        {
            CloseDemod();

            if (SampleSource == null)
                return;

            DigitalDemodulator demod = new ASKDemodulator();
            demod.SamplingRate = SampleSource.OutputSamplingRate;
            demod.Init();
            Demodulator = demod;
        }

        private void btnPocsag_Click(object sender, EventArgs e)
        {
            CloseDemod();

            if (SampleSource == null)
                return;

            DigitalDemodulator demod = new PSKDemodulator();
            demod.BitSink = new POCSAGDecoder();

            demod.SamplingRate = SampleSource.OutputSamplingRate;
            demod.Init();
            Demodulator = demod;
        }


        private void btnLua_Click(object sender, EventArgs e)
        {
            CloseDemod();
            
            if (SampleSource == null)
                return;

            DigitalDemodulator demod = new ScriptableDemodulator();

            demod.SamplingRate = SampleSource.OutputSamplingRate;
            demod.Init();

            Demodulator = demod;
        }

        public void CloseDemod()
        {
            if (Demodulator != null)
            {
                Demodulator = null;
            }
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
            Log.AddMessage("Sampling rate changed: " + FrequencyFormatter.FreqToStringAccurate(SampleSource.OutputSamplingRate));
            if (Demodulator != null)
            {
                Demodulator.SamplingRate = SampleSource.OutputSamplingRate;
            }
        }

        private void ProcessMain()
        {
            while (Processing)
            {
                if (SampleSource.Read())
                {
                    for (int pos = 0; pos < SampleSource.SamplesRead; pos++)
                    {
                        lock (SampleSource.SampleBufferLock)
                        {
                            double I = SampleSource.SourceSamplesI[pos];
                            double Q = SampleSource.SourceSamplesQ[pos];

                            if (Demodulator != null)
                            {
                                try
                                {
                                    Demodulator.Process(I, Q);
                                }
                                catch (Exception e)
                                {
                                }
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
                CloseDemod();
                CloseSource();
                btnOpen.Text = "Open";
            }
            else
            {
                ContextMenu menu = new ContextMenu();

                menu.MenuItems.Add(new MenuItem("Shared Memory", new EventHandler(btnOpen_SharedMemory)));
//                menu.MenuItems.Add(new MenuItem("USRP CFile", new EventHandler(btnOpen_CFile)));
                btnOpen.ContextMenu = menu;
                btnOpen.ContextMenu.Show(btnOpen, new Point(10, 10));
            }
        }

        public void OpenSharedMem(int srcChan)
        {
            SampleSource = new ShmemSampleSource("GSM Analyzer", srcChan, 1, 0);

            SampleSource.DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;
            SampleSource.SamplingRateChanged += new EventHandler(SampleSource_SamplingRateChanged);

            Processing = true;
            ProcessThread = new Thread(ProcessMain);
            ProcessThread.Start();

            Log.AddMessage("Select your filter again to start decoding.");


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
            btnOpen.ContextMenu.Show(btnOpen, new Point(10, 10));
        }
    }
}
