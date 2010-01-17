using System;

namespace LibRXFFT.Libraries.SampleSources
{
    public class RandomSampleSource : SampleSource
    {
        private Random RandomData;
        private byte[] InBuffer;
        private double ModulationPos = 0.0f;
        private double CurrentFreq = 2;
        private double CurrentNoise = 0.01;
        
        private double EventCounter = 0;

        public override int SamplesPerBlock
        {
            set
            {
                InBuffer = new byte[value * BytesPerSamplePair];
                AllocateBuffers();
            }
            get
            {
                return InBuffer.Length / BytesPerSamplePair;
            }
        }


        public RandomSampleSource()
            : base(1)
        {
            RandomData = new Random((int)DateTime.Now.Ticks);
        }

        public override bool Read()
        {
            RandomData.NextBytes(InBuffer);
            ByteUtil.SamplesFromBinary(InBuffer, SourceSamplesI, SourceSamplesQ, DataFormat, InvertedSpectrum);

            for (int pos = 0; pos < SourceSamplesI.Length; pos++)
            {
                double rel = pos / SourceSamplesI.Length;

                SourceSamplesI[pos] *= CurrentNoise;
                SourceSamplesQ[pos] *= CurrentNoise;

                SourceSamplesI[pos] += Math.Sin((ModulationPos + pos) / CurrentFreq);
                SourceSamplesQ[pos] += Math.Cos((ModulationPos + pos) / CurrentFreq);

                if (++EventCounter == 5000000)
                {
                    EventCounter = 0;
                    CurrentFreq = (double)RandomData.Next(0, 1000) / 1000 + 0.001;
                    CurrentNoise = (double)RandomData.Next(0, 500) / 10000;
                }

                SourceSamplesI[pos] += Math.Sin(0.1f * rel * Math.PI) * Math.Sin(rel * Math.PI);
                SourceSamplesQ[pos] += Math.Cos(0.5f * rel * Math.PI) * Math.Cos(rel * Math.PI);

            }

            ModulationPos += SourceSamplesI.Length;
            SamplesRead = SourceSamplesI.Length;



            SaveData();

            return true;
        }
    }
}
