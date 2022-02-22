using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.GSM.Layer1
{
    public class SequenceGenerator
    {
        private Resampler Sampler;
        private GaussFilter Filter;
        private double Oversampling;

        public SequenceGenerator(double oversampling, double BT)
        {
            Oversampling = oversampling;
            Sampler = new Resampler((decimal)Oversampling);
            Filter = new GaussFilter(BT);
        }

        public double[] GenerateDiffEncoded(byte[] srcData)
        {
            bool[] srcBits = DifferenceCode.Encode(ByteUtil.BytesToBits(srcData));

            return Generate(srcBits);
        }

        public double[] Generate(bool[] srcBits)
        {
            double[] samples = Sampler.Resample(srcBits);
            double[] gaussSamples = Filter.Process(samples, Oversampling);

            return gaussSamples;
        }
    }
}