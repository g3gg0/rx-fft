
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;

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
            Sampler = new Oversampler(Oversampling);
            Filter = new GaussFilter(BT);
        }

        public double[] GenerateDiffEncoded(byte[] srcData)
        {
            bool[] srcBits = DifferenceCode.Encode(ByteUtil.BytesToBits(srcData));

            return Generate(srcBits);
        }

        public double[] Generate(bool[] srcBits)
        {
            double[] samples = Sampler.Oversample(srcBits);
            double[] gaussSamples = Filter.Process(samples, Oversampling);

            return gaussSamples;
        }
    }
}