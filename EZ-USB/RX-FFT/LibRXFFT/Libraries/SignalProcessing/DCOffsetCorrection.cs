using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.SignalProcessing
{
    public class DCOffsetCorrection
    {
        public double OffsetI = 0.0f;
        public double OffsetQ = 0.0f;
        public double OffsetINext = 0.0f;
        public double OffsetQNext = 0.0f;
        public double RampSpeed = 0.05f;


        private void ValidateCoeff(ref double coeff, double defaultValue)
        {
            if (double.IsNaN(coeff) || double.IsInfinity(coeff))
            {
                coeff = defaultValue;
            }
        }

        public void PerformDCDetection(ref double[] input)
        {
            if (input.Length == 0)
            {
                return;
            }

            double avgI = 0.0f;
            double avgQ = 0.0f;
            for (int pos = 0; pos < input.Length; pos++)
            {
                avgI += (input[pos] - OffsetI) / input.Length;
            }

            OffsetINext = OffsetI + avgI * RampSpeed;
        }

        public void PerformDCCorrection(ref double[] input)
        {
            if (input.Length == 0)
            {
                return;
            }
            ValidateCoeff(ref OffsetI, 0);
            ValidateCoeff(ref OffsetINext, 0);

            double rampI = (OffsetINext - OffsetI) / input.Length;

            for (int pos = 0; pos < input.Length; pos++)
            {
                input[pos] -= OffsetI + rampI * pos;
            }

            OffsetI = OffsetINext;
        }


        public void PerformDCDetection(ref double[] inputI, ref double[] inputQ)
        {
            if (inputI.Length != inputQ.Length || inputI.Length == 0)
            {
                return;
            }

            double avgI = 0.0f;
            double avgQ = 0.0f;
            for (int pos = 0; pos < inputI.Length; pos++)
            {
                avgI += (inputI[pos] - OffsetI) / inputI.Length;
                avgQ += (inputQ[pos] - OffsetQ) / inputI.Length;
            }

            OffsetINext = OffsetI + avgI * RampSpeed;
            OffsetQNext = OffsetQ + avgQ * RampSpeed;
        }

        public void PerformDCCorrection(ref double[] inputI, ref double[] inputQ)
        {
            if (inputI.Length != inputQ.Length || inputI.Length == 0)
            {
                return;
            }
            ValidateCoeff(ref OffsetI, 0);
            ValidateCoeff(ref OffsetQ, 0);
            ValidateCoeff(ref OffsetINext, 0);
            ValidateCoeff(ref OffsetQNext, 0);

            double rampI = (OffsetINext - OffsetI) / inputI.Length;
            double rampQ = (OffsetQNext - OffsetQ) / inputI.Length;

            for (int pos = 0; pos < inputI.Length; pos++)
            {
                inputI[pos] -= OffsetI + rampI * pos;
                inputQ[pos] -= OffsetQ + rampQ * pos;
            }

            OffsetI = OffsetINext;
            OffsetQ = OffsetQNext;
        }
    }
}
