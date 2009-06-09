using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;

namespace LibRXFFT.Components
{
    public partial class WaterfallFFTDisplay : UserControl
    {
        Mutex FFTLock = new Mutex();
        private FFTTransformer FFT;
        private int _FFTSize = 512;


        public WaterfallFFTDisplay()
        {
            InitializeComponent();
        }

        public int FFTSize
        {
            get { return _FFTSize; }
            set
            {
                lock (FFTLock)
                {
                    _FFTSize = value;
                    FFT = new FFTTransformer(value);

                    fftDisplay.FFTSize = value;
                    waterfallDisplay.FFTSize = value;
                }
            }
        }

        public void ProcessData(byte[] dataBuffer)
        {
            const int bytePerSample = 2;
            const int channels = 2;

            lock (FFTLock)
            {
                int samplePairs = dataBuffer.Length / (channels * bytePerSample);

                for (int samplePair = 0; samplePair < samplePairs; samplePair++)
                {
                    int samplePairPos = samplePair * bytePerSample * channels;
                    double I = ByteUtil.getDoubleFromBytes(dataBuffer, samplePairPos);
                    double Q = ByteUtil.getDoubleFromBytes(dataBuffer, samplePairPos + bytePerSample);

                    FFT.AddSample(I, Q);

                    if (FFT.ResultAvailable)
                    {
                        double[] amplitudes = FFT.GetResult();

                        fftDisplay.ProcessData(amplitudes);
                        //waterfallDisplay.ProcessData(amplitudes);
                    }
                }
            }
        }

    }
}