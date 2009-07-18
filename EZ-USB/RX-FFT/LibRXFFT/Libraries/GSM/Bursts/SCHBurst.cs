using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    public class SCHBurst : Burst
    {
        private readonly byte[] TrainingCode = new byte[] { 0xB9, 0x62, 0x04, 0x0F, 0x2D, 0x45, 0x76, 0x1B };
        private double[] TrainingSequence = null;

        private bool DumpData = false;
        public const double Data1Bits = 39;
        public const double SyncBits = 64;
        public const double Data2Bits = 39;
        public const double SyncOffset = LeadingTailBits + Data1Bits;
        private bool[] SCHData = new bool[78];
        private bool[] SCHDataDecoded = new bool[39];

        public SCHBurst(GSMParameters Parameters)
        {
            Name = "SCH";
            ShortName = "SC ";

            CreateSequence(Parameters);
        }

        private void CreateSequence(GSMParameters Parameters)
        {
            /* create training sequence ... */
            double[] tmpTrainingSequence = new SequenceGenerator(Parameters.Oversampling, Parameters.BT).GenerateDiffEncoded(TrainingCode);

            /* ... and skip the first and last two samples since these are affected by the bits before */
            TrainingSequence = new double[(int)(tmpTrainingSequence.Length - 4 * Parameters.Oversampling)];
            Array.Copy(tmpTrainingSequence, (int)(2 * Parameters.Oversampling), TrainingSequence, 0, TrainingSequence.Length);
        }

        public override eSuccessState ParseRawBurst(GSMParameters Parameters, double[] rawBurst)
        {
            double startOffset = Parameters.SampleStartPosition;


            int bitTolerance = 3;

            if (Parameters.FirstSCH)
                bitTolerance = 16;

            Parameters.FirstSCH = false;

            /* skip the number of data bits defined in SCHBurst plus SpareBits that are "pre"-feeded */
            int sequencePos = (int)(startOffset + Parameters.Oversampling * (SCHBurst.SyncOffset + 2));

            /* locate the training sequence over two bits */
            int position = SignalPower.Locate(rawBurst, sequencePos, TrainingSequence, (int)(Parameters.Oversampling * bitTolerance));
            if (position == int.MinValue)
            {
                ErrorMessage = "(Error in SignalPower.Locate)" + Environment.NewLine;
                return eSuccessState.Failed;
            }

            /* calculate the offset between guessed position and real */
            Parameters.SampleOffset += position - sequencePos;
            Parameters.SubSampleOffset = 0;

            return eSuccessState.Succeeded;
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            Array.Copy(decodedBurst, 3, SCHData, 0, 39);
            Array.Copy(decodedBurst, 106, SCHData, 39, 39);

            bool[] data = ConvolutionalCoder.Decode(SCHData, SCHDataDecoded);
            if (data == null)
            {
                ErrorMessage = "(Error in ConvolutionalCoder)";
                return eSuccessState.Failed;
            }

            bool[] crc = CRC.Calc(data, 0, 35, CRC.PolynomialSCH);
            if (!CRC.Matches(crc))
            {
                ErrorMessage = "(Error in CRC)";
                return eSuccessState.Failed;
            }

            long BSIC = ByteUtil.BitsToLongRev(data, 2, 6);
            long T1 = ByteUtil.BitsToLongRev(data, new[] { new[] { 0, 2 }, new[] { 8, 8 }, new[] { 23, 1 } });
            long T2 = ByteUtil.BitsToLongRev(data, 18, 5);
            long T3M = ByteUtil.BitsToLongRev(data, new[] { new[] { 16, 2 }, new[] { 24, 1 } });
            long T3 = (10 * T3M) + 1;

            long FN;
            if (T2 <= T3)
                FN = 51 * ((T3 - T2) % 26) + T3 + 51 * 26 * T1;
            else
                FN = 51 * (26 - ((T2 - T3) % 26)) + T3 + 51 * 26 * T1;

            if (param.FN != FN)
                param.FN = FN;

            param.TN = 0;

            if (DumpData)
            {
                StatusMessage =
                    "BSIC: " + String.Format("{0,3}", BSIC) +
                    "  T1: " + String.Format("{0,5}", T1) +
                    "  T2: " + String.Format("{0,3}", T2) +
                    "  T3: " + String.Format("{0,3}", T3) +
                    "  FN: " + String.Format("{0,8}", FN) +
                    "  TrainOffs: " + String.Format("{0,3}", param.SampleOffset);
            }

            return eSuccessState.Succeeded;
        }
    }
}