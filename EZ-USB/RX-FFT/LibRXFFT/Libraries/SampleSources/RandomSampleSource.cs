using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.SampleSources
{
    public class RandomSampleSource : SampleSource
    {
        private Random RandomData;
        private byte[] InBuffer;
        private double ModulationPos = 0.0f;

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
                double rel = pos/SourceSamplesI.Length;

                SourceSamplesI[pos] *= 0.001f;
                SourceSamplesQ[pos] *= 0.001f;

                SourceSamplesI[pos] += Math.Sin(0.1f * rel * Math.PI) * Math.Sin(ModulationPos * Math.PI);
                SourceSamplesQ[pos] += Math.Cos(0.5f * rel * Math.PI) * Math.Cos(ModulationPos * Math.PI);

                SourceSamplesI[pos] += Math.Sin(0.1f * rel * Math.PI) * Math.Sin(rel * ModulationPos * Math.PI);
                SourceSamplesQ[pos] += Math.Cos(0.5f * rel * Math.PI) * Math.Cos(rel * ModulationPos * Math.PI);

            }

            ModulationPos += 0.001f;
            SamplesRead = SourceSamplesI.Length;

            return true;
        }
    }
}
