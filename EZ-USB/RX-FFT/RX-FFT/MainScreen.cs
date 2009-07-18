using System;
using System.Windows.Forms;
using LibRXFFT.Libraries.ShmemChain;
using System.Threading;
using LibRXFFT.Libraries.FFTW;

namespace RX_FFT
{
    public partial class MainScreen : Form
    {
        bool ThreadActive;
        Thread ReadThread;
        //USBRXDevice dev;
        SharedMem ShmemChannel;
        bool processPaused;
        byte[] ReadBuffer = new byte[0];
        Object SpinLock = new Object();

        public MainScreen()
        {
            InitializeComponent();

            FFTSize = 2048;

            string[] windowingTypes = Enum.GetNames(typeof(FFTTransformer.eWindowingFunction));
            cmbWindowFunc.Items.AddRange(windowingTypes);
            cmbWindowFunc.Text = FFTDisplay.WindowingFunction.ToString();
            cmbAverage.Text = FFTDisplay.Averaging.ToString();
            cmbFFTSize.Text = FFTSize.ToString();
        }

        int FFTSize
        {
            get { return FFTDisplay.FFTSize; }
            set
            {
                lock (SpinLock)
                {
                    ReadBuffer = new byte[value * 4];
                    FFTDisplay.FFTSize = value;
                }
            }
        }

        void FFTReadFunc()
        {
            Random rnd = new Random();

            while (ThreadActive)
            {
                //dev.Read(inBuffer);
                lock (SpinLock)
                {
                    ShmemChannel.Read(ReadBuffer, 0, ReadBuffer.Length);

                    //Thread.Sleep(5);
                    //rnd.NextBytes(inBuffer);

                    if (!processPaused)
                    {
                        FFTDisplay.ProcessRawData(ReadBuffer);
                    }
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
                ShmemChannel = new SharedMem(0, -1, "FFT Display");
                ShmemChannel.ReadTimeout = 10;
                ShmemChannel.ReadMode = eReadMode.TimeLimited;

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

        private void cmbWindowFunc_TextChanged(object sender, EventArgs e)
        {
            string typeString = cmbWindowFunc.Text;

            try
            {
                FFTTransformer.eWindowingFunction type = (FFTTransformer.eWindowingFunction)Enum.Parse(typeof(FFTTransformer.eWindowingFunction), typeString);
                FFTDisplay.WindowingFunction = type;
            }
            catch (Exception ex)
            {
            }
        }

        private void cmbAverage_TextChanged(object sender, EventArgs e)
        {
            double avg;

            if (!double.TryParse(cmbAverage.Text, out avg))
                return;
            FFTDisplay.Averaging = avg;
        }

        private void cmbFFTSize_TextChanged(object sender, EventArgs e)
        {
            int size;

            if (!int.TryParse(cmbFFTSize.Text, out size))
                return;

            FFTSize = size;
            FFTDisplay.FFTSize = size;
        }

    }
}
