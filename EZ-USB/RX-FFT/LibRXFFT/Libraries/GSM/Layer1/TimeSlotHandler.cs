using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Bursts;
using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.GSM.Layer1
{
    public delegate void AddMessageDelegate(string msg);

    public class TimeSlotHandler
    {
        private readonly double BT;
        public readonly double Oversampling;

        private readonly byte[] TrainingCode = new byte[] {0xB9, 0x62, 0x04, 0x0F, 0x2D, 0x45, 0x76, 0x1B};
        private readonly double[] TrainingSequence;
        public AddMessageDelegate AddMessage { get; set; }

        public readonly GMSKDecoder Decoder;
        public readonly L3Handler L3;
        private readonly BCCHBurst BCCH;
        private readonly CCCHBurst CCCH;
        private readonly SCHBurst SCH;
        private readonly FCHBurst FCH;

        private readonly bool[] burstBits = new bool[148];
        private readonly bool[] burstBitsUndiffed = new bool[148];

        public GSMParameters Parameters;

        private readonly sTimeSlotParam[][] TimeSlotHandlers;
        public int SpareBits = 3;

        private struct sTimeSlotParam
        {
            public readonly Burst Burst;
            public readonly int Sequence;

            public sTimeSlotParam(Burst burst, int seq)
            {
                Burst = burst;
                Sequence = seq;
            }
        }

        public TimeSlotHandler(double oversampling, double bt, AddMessageDelegate addMessage)
        {
            Oversampling = oversampling;
            BT = bt;
            AddMessage = addMessage;

            Decoder = new GMSKDecoder(Oversampling, BT);

            L3 = new L3Handler();

            FCH = new FCHBurst();
            SCH = new SCHBurst();
            BCCH = new BCCHBurst(L3);
            CCCH = new CCCHBurst(L3);

            TimeSlotHandlers = new sTimeSlotParam[8][];
            for (int timeSlot = 0; timeSlot < 2; timeSlot++)
                TimeSlotHandlers[timeSlot] = new sTimeSlotParam[51];

            for (int timeSlot = 2; timeSlot < 8; timeSlot++)
                TimeSlotHandlers[timeSlot] = new sTimeSlotParam[26];


            TimeSlotHandlers[0][0] = new sTimeSlotParam(FCH, 0);
            TimeSlotHandlers[0][1] = new sTimeSlotParam(SCH, 0);
            TimeSlotHandlers[0][2] = new sTimeSlotParam(BCCH, 0);
            TimeSlotHandlers[0][3] = new sTimeSlotParam(BCCH, 1);
            TimeSlotHandlers[0][4] = new sTimeSlotParam(BCCH, 2);
            TimeSlotHandlers[0][5] = new sTimeSlotParam(BCCH, 3);
            TimeSlotHandlers[0][6] = new sTimeSlotParam(CCCH, 0);
            TimeSlotHandlers[0][7] = new sTimeSlotParam(CCCH, 1);
            TimeSlotHandlers[0][8] = new sTimeSlotParam(CCCH, 2);
            TimeSlotHandlers[0][9] = new sTimeSlotParam(CCCH, 3);
            TimeSlotHandlers[0][10] = new sTimeSlotParam(FCH, 0);
            TimeSlotHandlers[0][11] = new sTimeSlotParam(SCH, 0);
            TimeSlotHandlers[0][12] = new sTimeSlotParam(CCCH, 0);
            TimeSlotHandlers[0][13] = new sTimeSlotParam(CCCH, 1);
            TimeSlotHandlers[0][14] = new sTimeSlotParam(CCCH, 2);
            TimeSlotHandlers[0][15] = new sTimeSlotParam(CCCH, 3);
            TimeSlotHandlers[0][16] = new sTimeSlotParam(CCCH, 0);
            TimeSlotHandlers[0][17] = new sTimeSlotParam(CCCH, 1);
            TimeSlotHandlers[0][18] = new sTimeSlotParam(CCCH, 2);
            TimeSlotHandlers[0][19] = new sTimeSlotParam(CCCH, 3);
            TimeSlotHandlers[0][20] = new sTimeSlotParam(FCH, 0);
            TimeSlotHandlers[0][21] = new sTimeSlotParam(SCH, 0);
            TimeSlotHandlers[0][22] = new sTimeSlotParam(CCCH, 0);
            TimeSlotHandlers[0][23] = new sTimeSlotParam(CCCH, 1);
            TimeSlotHandlers[0][24] = new sTimeSlotParam(CCCH, 2);
            TimeSlotHandlers[0][25] = new sTimeSlotParam(CCCH, 3);
            TimeSlotHandlers[0][26] = new sTimeSlotParam(CCCH, 0);
            TimeSlotHandlers[0][27] = new sTimeSlotParam(CCCH, 1);
            TimeSlotHandlers[0][28] = new sTimeSlotParam(CCCH, 2);
            TimeSlotHandlers[0][29] = new sTimeSlotParam(CCCH, 3);
            TimeSlotHandlers[0][30] = new sTimeSlotParam(FCH, 0);
            TimeSlotHandlers[0][31] = new sTimeSlotParam(SCH, 0);
            TimeSlotHandlers[0][32] = new sTimeSlotParam(CCCH, 0);
            TimeSlotHandlers[0][33] = new sTimeSlotParam(CCCH, 1);
            TimeSlotHandlers[0][34] = new sTimeSlotParam(CCCH, 2);
            TimeSlotHandlers[0][35] = new sTimeSlotParam(CCCH, 3);
            TimeSlotHandlers[0][36] = new sTimeSlotParam(CCCH, 0);
            TimeSlotHandlers[0][37] = new sTimeSlotParam(CCCH, 1);
            TimeSlotHandlers[0][38] = new sTimeSlotParam(CCCH, 2);
            TimeSlotHandlers[0][39] = new sTimeSlotParam(CCCH, 3);
            TimeSlotHandlers[0][40] = new sTimeSlotParam(FCH, 0);
            TimeSlotHandlers[0][41] = new sTimeSlotParam(SCH, 0);
            TimeSlotHandlers[0][42] = new sTimeSlotParam(CCCH, 0);
            TimeSlotHandlers[0][43] = new sTimeSlotParam(CCCH, 1);
            TimeSlotHandlers[0][44] = new sTimeSlotParam(CCCH, 2);
            TimeSlotHandlers[0][45] = new sTimeSlotParam(CCCH, 3);
            TimeSlotHandlers[0][46] = new sTimeSlotParam(CCCH, 0);
            TimeSlotHandlers[0][47] = new sTimeSlotParam(CCCH, 1);
            TimeSlotHandlers[0][48] = new sTimeSlotParam(CCCH, 2);
            TimeSlotHandlers[0][49] = new sTimeSlotParam(CCCH, 3);
            TimeSlotHandlers[0][50] = new sTimeSlotParam(null, 0);

            for (int fn = 0; fn < 32; fn+=4)
            {
                SDCCHBurst tmpSDCCH = new SDCCHBurst(L3, fn/4);
                TimeSlotHandlers[1][fn + 0] = new sTimeSlotParam(tmpSDCCH, 0);
                TimeSlotHandlers[1][fn + 1] = new sTimeSlotParam(tmpSDCCH, 1);
                TimeSlotHandlers[1][fn + 2] = new sTimeSlotParam(tmpSDCCH, 2);
                TimeSlotHandlers[1][fn + 3] = new sTimeSlotParam(tmpSDCCH, 3);
            }
            for (int fn = 32; fn < 48; fn += 4)
            {
                SACCHBurst tmpSACCH = new SACCHBurst(L3, (fn - 32) / 4);
                TimeSlotHandlers[1][fn + 0] = new sTimeSlotParam(tmpSACCH, 0);
                TimeSlotHandlers[1][fn + 1] = new sTimeSlotParam(tmpSACCH, 1);
                TimeSlotHandlers[1][fn + 2] = new sTimeSlotParam(tmpSACCH, 2);
                TimeSlotHandlers[1][fn + 3] = new sTimeSlotParam(tmpSACCH, 3);
            }

            for (int tn = 2; tn < 8; tn += 1)
            {
                SACCHBurst tmpSACCH = new SACCHBurst(L3, "SACCH/TCH TN" + tn, tn);
                TimeSlotHandlers[tn][12] = new sTimeSlotParam(tmpSACCH, 0);
            }


            /* create training sequence ... */
            double[] tmpTrainingSequence = new SequenceGenerator(Oversampling, BT).GenerateDiffEncoded(TrainingCode);

            /* ... and skip the first and last two samples since these are affected by the bits before */
            TrainingSequence = new double[(int) (tmpTrainingSequence.Length - 4*Oversampling)];
            Array.Copy(tmpTrainingSequence, (int) (2*Oversampling), TrainingSequence, 0, TrainingSequence.Length);
        }


        private bool HandleSCHTrain(double[] timeSlotSamples)
        {
            string message = Parameters + "   [SCH] ";

            /* skip the number of data bits defined in SCHBurst plus SpareBits that are "pre"-feeded */
            int sequencePos = (int) (Oversampling*(SCHBurst.SyncOffset + 2 + SpareBits));

            /* locate the training sequence over two bits */
            int position = SignalPower.Locate(timeSlotSamples, sequencePos, TrainingSequence, (int) (Oversampling*3));
            if (position == int.MinValue)
            {
                message += "(Error in SignalPower.Locate)" + Environment.NewLine;
                AddMessage(message);
                Parameters.SampleOffset = int.MinValue;
                Parameters.Errors++;
                return false;
            }

            /* calculate the offset between guessed position and real */
            Parameters.SampleOffset = position - sequencePos;

            Decoder.StartOffset += (int)Parameters.SampleOffset;
            Decoder.SubSampleOffset = 0;/* OffsetEstimator.EstimateOffset(timeSlotSamples,
                                (int)(Decoder.StartOffset + Oversampling / 2 + 5 * Oversampling),
                                (int)((Burst.NetBitCount - 5) * Oversampling),
                                Oversampling);*/


            return true;
        }


        public void Handle(double[] timeSlotSamples)
        {
            if (AddMessage == null)
                return;

            Parameters.TN++;
            Parameters.TN %= 8;
            if (Parameters.TN == 0)
                Parameters.FN++;


            /* get the burst handler */
            Burst handler = null;
            int Sequence = 0;

            if (TimeSlotHandlers[Parameters.TN].Length == 51)
            {
                handler = TimeSlotHandlers[Parameters.TN][Parameters.T3].Burst; // control frame number
                Sequence = TimeSlotHandlers[Parameters.TN][Parameters.T3].Sequence;
            }
            else if (TimeSlotHandlers[Parameters.TN].Length == 26)
            {
                handler = TimeSlotHandlers[Parameters.TN][Parameters.T2].Burst; // traffic frame number
                Sequence = TimeSlotHandlers[Parameters.TN][Parameters.T2].Sequence;
            }

            /* this is a SCH burst */
            if (Parameters.FirstSCH || handler == SCH)
            {
                Parameters.FirstSCH = false;

                /* try to detect sequence and update sampling offset */
                if (!HandleSCHTrain(timeSlotSamples))
                    return;
            }

            /* continue to decode the packet */
            Decoder.Decode(timeSlotSamples, burstBits);

            if (handler != null)
            {
                DifferenceCode.Decode(burstBits, burstBitsUndiffed);

                /* check the first and last three bits to be low. thats required for all bursts from the BTS. */
                /*
                if (burstBitsUndiffed[0] || burstBitsUndiffed[1] || burstBitsUndiffed[2] || burstBitsUndiffed[145] || burstBitsUndiffed[146] || burstBitsUndiffed[147])
                {
                    AddMessage("   [GMSK] Delimiter bits are not low (" + Parameters + ")" + Environment.NewLine);
                    Parameters.Error = true;
                    return;
                }
                */

                /* if everything went ok so far, give the data to the associated handler */
                if (handler.ParseData(Parameters, burstBitsUndiffed, Sequence))
                {
                    if (handler.StatusMessage != null)
                    {
                        AddMessage("   [L1] [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);
                        AddMessage("        " + handler.StatusMessage + Environment.NewLine);
                        AddMessage(Environment.NewLine);
                    }
                    handler.StatusMessage = null;
                }
                else
                {
                    AddMessage("   [L1] [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);
                    AddMessage("        ERROR: " + handler.ErrorMessage + Environment.NewLine);
                    AddMessage(Environment.NewLine);
                }

                /* show L2 messages, if handler wishes */
                bool showL2 = handler.L2.ShowMessage && !string.IsNullOrEmpty(handler.L2.StatusMessage);
                /* show L3 messages, if there is any */
                bool showL3 = !string.IsNullOrEmpty(handler.L3.StatusMessage);

                /* only show L1 when one of both apply */
                if (showL2 || showL3)
                    AddMessage("   [L1] [" + handler.Name + "] - [" + Parameters + "]" + Environment.NewLine);

                /* show L2 if L2 wants to, or if L3 has some message */
                if (showL3 || showL2)
                    AddMessage("   [L2] " + handler.L2.StatusMessage);

                /* L3 handler has a message to show */
                if (showL3)
                    AddMessage("   [L3] " + handler.L3.StatusMessage);

                /* if something shown, add newline */
                if (showL2 || showL3)
                    AddMessage(Environment.NewLine);

                /* and reset these "flags" */
                handler.L2.ShowMessage = false;
                handler.L3.StatusMessage = null;

                /* does the Layer 3 provide any sniffed information? */
                if (handler.L3.SniffResult != null)
                {
                    AddMessage("   [L3] sniffed: " + handler.L3.SniffResult + Environment.NewLine);
                    handler.L3.SniffResult = null;
                }
            }
        }
    }
}