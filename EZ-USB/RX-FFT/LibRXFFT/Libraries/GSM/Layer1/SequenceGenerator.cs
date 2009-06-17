using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.GSM.Layer1
{
    public class SequenceGenerator
    {
        private Oversampler Sampler;
        private GaussFilter Filter;
        private double Oversampling;

        public SequenceGenerator(double oversampling, double BT)
        {
            Oversampling = oversampling;
            Sampler = new Oversampler();
            Filter = new GaussFilter(BT);
        }

        public double[] GenerateDiffEncoded(byte[] srcData)
        {
            bool[] srcBits = DifferenceCode.Encode(ByteUtil.BytesToBits(srcData));

            return Generate(srcBits);
        }

        public double[] Generate(bool[] srcBits)
        {
            double[] samples = Sampler.Oversample(srcBits, Oversampling);
            double[] gaussSamples = Filter.Process(samples, Oversampling);

            return gaussSamples;
        }
    }
}