using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.ShmemChain;

namespace LibRXFFT.Libraries.SampleSources
{
    public class ShmemSampleSource
    {
        private SharedMem ShmemChannel;
        private GMSKDemodulator Demodulator;

        private int BlockSize = 512;
        private byte[] InBuffer;

        public double[] Signal;
        public double[] Strength;
        public int SamplesRead;

        public ShmemSampleSource(string name)
        {
            ShmemChannel = new SharedMem(0, -1, name);
            Demodulator = new GMSKDemodulator();
            Demodulator.DataFormat = eDataFormat.Direct16BitIQFixedPoint;

            InBuffer = new byte[BlockSize*Demodulator.BytesPerSamplePair];
            Signal = new double[BlockSize];
            Strength = new double[BlockSize];
        }

        public bool Read()
        {
            int read = ShmemChannel.Read(InBuffer, 0, InBuffer.Length);

            if (read != 0 && read != InBuffer.Length)
            {
                SamplesRead = 0;
                return false;
            }

            Demodulator.ProcessData(InBuffer, read, Signal, Strength);
            SamplesRead = BlockSize;

            return true;
        }
    }
}
