using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.ShmemChain;
using System.Threading;

namespace RX_FFT
{
    public partial class MainScreen : Form
    {
        int fftSize = 512;

        bool ThreadActive;
        Thread ReadThread;
        //USBRXDevice dev;
        SharedMem ShmemChannel;
        private bool processPaused;

        public MainScreen()
        {
            InitializeComponent();

            FFTDisplay.FFTSize = 2048;
        }


        void FFTReadFunc()
        {
            byte[] inBuffer = new byte[fftSize * 4];

            while (ThreadActive)
            {
                //dev.Read(inBuffer);
                ShmemChannel.Read(inBuffer, 0, inBuffer.Length);

                if (!processPaused)
                {
                    
                    FFTDisplay.ProcessRawData(inBuffer);
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (ShmemChannel != null)
            {
                ThreadActive = false;
                Thread.Sleep(10);
                ReadThread.Abort();
                //dev.Close();
                //dev = null;
                ShmemChannel.Unregister();
                ShmemChannel = null;

                btnOpen.Text = "Open";
            }
            else
            {
                ShmemChannel = new SharedMem(0, -1, "C# Test");
                //dev = new USBRXDevice();
                //if (dev.Open())
                {
                    ThreadActive = true;
                    ReadThread = new Thread(new ThreadStart(FFTReadFunc));
                    ReadThread.Start();
                }

                btnOpen.Text = "Close";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            processPaused = !processPaused;
        }

    }
}
