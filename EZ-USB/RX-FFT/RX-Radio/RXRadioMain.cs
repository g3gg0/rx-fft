using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Devices;
using LibRXFFT.Libraries.USB_RX.Misc;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.SampleSources;
using System.Threading;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries;

namespace RX_Radio
{
    public partial class RXRadioMain : Form
    {
        private USBRXDevice USBRX;
        private ShmemSampleSource InSampleSource;
        private Thread PowerUpdateThread;

        private AudioDemodulator Demod;
        private DemodulationDialog DemodDlg;

        private string ErrorMessage = "";

        public RXRadioMain()
        {
            InitializeComponent();

            frequencySelector1.FrequencyChanged += new EventHandler(frequencySelector1_FrequencyChanged);
        }

        void frequencySelector1_FrequencyChanged(object sender, EventArgs e)
        {
            if (USBRX != null)
            {
                USBRX.Tuner.SetFrequency(frequencySelector1.Frequency);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            StopThreads();
            base.OnClosing(e);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (USBRX == null)
            {
                USBRXDevice device = new USBRXDevice();

                device.ShowConsole(false);
                device.TunerCombination = USBRXDevice.eCombinationType.Automatic;

                try
                {
                    if (!device.Init())
                    {
                        ErrorMessage = "There was no BO-35digi found on USB bus.";
                        return;
                    }
                }
                catch (BadImageFormatException ex)
                {
                    ErrorMessage = "Unsupported architecture.";
                    return;
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Unhandled exception." + Environment.NewLine + e;
                    return;
                }

                USBRX = device;

                FilterList.NCOFreq = USBRX.Atmel.TCXOFreq;
                FilterList.UpdateFilters("Filter");
                FilterList.AddFilters("..\\..\\..\\Filter");
                FilterList.FilterSelected += new EventHandler(FilterList_FilterSelected);

                frequencySelector1.UpperLimit = USBRX.Tuner.HighestFrequency;
                frequencySelector1.LowerLimit = USBRX.Tuner.LowestFrequency;

                /* bar update thread */
                InSampleSource = new ShmemSampleSource("RX-Radio Reader", USBRX.ShmemChannel, 1, 0);
                InSampleSource.DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;
                PowerUpdateThread = new Thread(() =>
                    {
                        InSampleSource.SamplesPerBlock = 1024;
                        while (true)
                        {
                            InSampleSource.Flush();
                            InSampleSource.Read();

                            double maxValue = DBTools.MaximumDb(InSampleSource.SourceSamplesI, InSampleSource.SourceSamplesQ);
                            BeginInvoke(new Action(() =>
                                {
                                    powerBar.Amplitude = (maxValue + 100) / 100;
                                    powerBar.LinePosition = (maxValue + 100) / 100;
                                    powerBar.Enabled = true;
                                    powerBar.Invalidate();
                                }));

                            Thread.Sleep(250);
                        }
                    });
                PowerUpdateThread.Start();

                /* demodulator */
                Demod = new AudioDemodulator();
                Demod.AudioInSampleSource = new ShmemSampleSource("RX-FFT Audio Decoder", USBRX.ShmemChannel, 1, 0);
                Demod.AudioInSampleSource.DataFormat = ByteUtil.eSampleFormat.Direct16BitIQFixedPoint;
                Demod.AudioOutShmem = new SharedMem(-2, 1, "RX-Radio Demodulated Audio");

                lock (Demod.DemodOptions)
                {
                    Demod.DemodOptions.SoundDevice = new DXSoundDevice(Handle);
                }
                Demod.Start();

                /* demodulator dialog */
                DemodDlg = new DemodulationDialog(Demod.DemodOptions);
                DemodDlg.Show();


                /* finished */
                USBRX.CurrentMode = LibRXFFT.Libraries.eTransferMode.Stopped;

                btnOpen.Text = "Close";
            }
            else
            {
                StopThreads();
                btnOpen.Text = "Open";
            }
        }

        private void StopThreads()
        {
            if (USBRX != null)
            {
                USBRX.CurrentMode = LibRXFFT.Libraries.eTransferMode.Stopped;
            }

            if (PowerUpdateThread != null)
            {
                PowerUpdateThread.Abort();
                PowerUpdateThread = null;
            }

            if (DemodDlg != null)
            {
                DemodDlg.Close();
                DemodDlg = null;
            }

            if (Demod != null)
            {
                Demod.AudioInSampleSource.Close();
                Demod.AudioOutShmem.Close();
                Demod.Stop();
                Demod = null;
            }

            if (USBRX != null)
            {
                USBRX.Close();
                USBRX = null;
            }
        }

        void FilterList_FilterSelected(object sender, EventArgs e)
        {
            if (USBRX != null)
            {
                FilterInformation CurrentFilter = (FilterInformation)sender;
                if (sender is AD6636FilterFile)
                {
                    USBRX.SetFilter((AD6636FilterFile)sender);
                    SharedMemNative.shmemchain_set_rate(USBRX.ShmemNode, ((AD6636FilterFile)sender).Rate * 2);
                }
                else if (sender is AtmelFilter)
                {
                    USBRX.SetFilter((AtmelFilter)sender);
                    SharedMemNative.shmemchain_set_rate(USBRX.ShmemNode, ((AtmelFilter)sender).Rate * 2);
                }

                USBRX.CurrentMode = LibRXFFT.Libraries.eTransferMode.Stream;
            }
        }
    }
}
