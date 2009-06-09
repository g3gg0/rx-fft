using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM_Layer3;
using LibRXFFT.Libraries.ShmemChain;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries.GSM
{
    public delegate void AddMessageDelegate(string msg);

    public class TimeSlotHandler
    {
        private double BT;
        private double Oversampling;
        public GMSKDecoder Decoder;

        public GMSKDecoder[] Decoders;
        public long[] DecoderErrors;
        public long[] DecoderSuccess;

        private byte[] TrainingCode = new byte[] { 0xB9, 0x62, 0x04, 0x0F, 0x2D, 0x45, 0x76, 0x1B };
        private double[] TrainingSequence;
        private bool Error = false;
        public AddMessageDelegate AddMessage { get; set; }

        public L3Handler L3;
        public BCCHBurst BCCH;
        public CCCHBurst CCCH;
        public SCHBurst SCH;
        public FCHBurst FCH;

        private int burstSamples;
        private double[] burstBuffer;
        public GSMParameters Parameters;
        public SharedMem SharedMemory;
        private bool DumpSCH = false;

        private Burst[][] TimeSlotHandlers;

        public TimeSlotHandler(double oversampling, double bt)
        {
            Oversampling = oversampling;
            BT = bt;

            L3 = new L3Handler();
            Decoder = new GMSKDecoder(Oversampling, BT);

            FCH = new FCHBurst();
            SCH = new SCHBurst();
            BCCH = new BCCHBurst(L3);
            CCCH = new CCCHBurst(L3);

            TimeSlotHandlers = new Burst[8][];
            for (int timeSlot = 0; timeSlot < 2; timeSlot++)
                TimeSlotHandlers[timeSlot] = new Burst[51];

            for (int timeSlot = 2; timeSlot < 8; timeSlot++)
                TimeSlotHandlers[timeSlot] = new Burst[26];


            TimeSlotHandlers[0][0] = FCH;
            TimeSlotHandlers[0][1] = SCH;
            TimeSlotHandlers[0][2] = BCCH;
            TimeSlotHandlers[0][3] = BCCH;
            TimeSlotHandlers[0][4] = BCCH;
            TimeSlotHandlers[0][5] = BCCH;
            TimeSlotHandlers[0][6] = CCCH;
            TimeSlotHandlers[0][7] = CCCH;
            TimeSlotHandlers[0][8] = CCCH;
            TimeSlotHandlers[0][9] = CCCH;
            TimeSlotHandlers[0][10] = FCH;
            TimeSlotHandlers[0][11] = SCH;
            TimeSlotHandlers[0][12] = CCCH;
            TimeSlotHandlers[0][13] = CCCH;
            TimeSlotHandlers[0][14] = CCCH;
            TimeSlotHandlers[0][15] = CCCH;
            TimeSlotHandlers[0][16] = CCCH;
            TimeSlotHandlers[0][17] = CCCH;
            TimeSlotHandlers[0][18] = CCCH;
            TimeSlotHandlers[0][19] = CCCH;
            TimeSlotHandlers[0][20] = FCH;
            TimeSlotHandlers[0][21] = SCH;
            TimeSlotHandlers[0][22] = CCCH;
            TimeSlotHandlers[0][23] = CCCH;
            TimeSlotHandlers[0][24] = CCCH;
            TimeSlotHandlers[0][25] = CCCH;
            TimeSlotHandlers[0][26] = CCCH;
            TimeSlotHandlers[0][27] = CCCH;
            TimeSlotHandlers[0][28] = CCCH;
            TimeSlotHandlers[0][29] = CCCH;
            TimeSlotHandlers[0][30] = FCH;
            TimeSlotHandlers[0][31] = SCH;
            TimeSlotHandlers[0][32] = CCCH;
            TimeSlotHandlers[0][33] = CCCH;
            TimeSlotHandlers[0][34] = CCCH;
            TimeSlotHandlers[0][35] = CCCH;
            TimeSlotHandlers[0][36] = CCCH;
            TimeSlotHandlers[0][37] = CCCH;
            TimeSlotHandlers[0][38] = CCCH;
            TimeSlotHandlers[0][39] = CCCH;
            TimeSlotHandlers[0][40] = FCH;
            TimeSlotHandlers[0][41] = SCH;
            TimeSlotHandlers[0][42] = CCCH;
            TimeSlotHandlers[0][43] = CCCH;
            TimeSlotHandlers[0][44] = CCCH;
            TimeSlotHandlers[0][45] = CCCH;
            TimeSlotHandlers[0][46] = CCCH;
            TimeSlotHandlers[0][47] = CCCH;
            TimeSlotHandlers[0][48] = CCCH;
            TimeSlotHandlers[0][49] = CCCH;
            TimeSlotHandlers[0][50] = null;

            /*
            DecoderErrors = new long[500];
            DecoderSuccess = new long[500];
            Decoders = new GMSKDecoder[500];
            for (int pos = 0; pos < Decoders.Length; pos++)
                Decoders[pos] = new GMSKDecoder(oversampling, BT);
            for (int pos = 0; pos < Decoders.Length; pos++)
                Decoders[pos].StartOffset = (int)(Oversampling * 2);

            for (int pos = 0; pos < Decoders.Length; pos++)
                Decoders[pos].MinPowerFact = 0.3f;
            for (int pos = 0; pos < Decoders.Length; pos++)
                Decoders[pos].DCOffsetFact = (double)pos / 1000;
            */

            /* create training sequence.. */

            double[] tmpTrainingSequence = new SequenceGenerator(Oversampling, BT).GenerateDiffEncoded(TrainingCode);

            /* ... and skip the first and last two samples since these are affected by the bits before */
            TrainingSequence = new double[(int)(tmpTrainingSequence.Length - 4 * Oversampling)];
            Array.Copy(tmpTrainingSequence, (int)(2 * Oversampling), TrainingSequence, 0, TrainingSequence.Length);

            burstSamples = (int)Math.Ceiling(Burst.TotalBitCount * Oversampling);
            burstBuffer = new double[burstSamples + 2];

        }


        private bool HandleSCHTrain(double[] timeSlotSamples)
        {
            string message = "   [SCH] ";

            /* skip some bits and the number of data bits defined in SCHBurst */
            int sequencePos = (int)(Oversampling * (SCHBurst.SyncOffset + 2 + 2));

            /* locate the training sequence over two bits */
            int position = SignalPower.Locate(timeSlotSamples, sequencePos, TrainingSequence, (int)(Oversampling * 2));
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

            Decoder.StartOffset = (int)(position - Oversampling * (SCHBurst.SyncOffset + 2 + 0.25f));

            if (Decoder.StartOffset < 0)
            {
                message += "(Error in StartOffset)" + Environment.NewLine;
                AddMessage(message);
                Parameters.Error = true;
                return false;
            }

            return true;
        }



        public void Handle(double[] timeSlotSamples)
        {
            if (AddMessage == null)
                return;

            Parameters.CurrentTimeSlot++;
            Parameters.CurrentTimeSlot %= 8;
            if (Parameters.CurrentTimeSlot == 0)
            {
                Parameters.CurrentControlFrame++;
                Parameters.CurrentControlFrame %= 51;

                Parameters.CurrentTrafficFrame++;
                Parameters.CurrentTrafficFrame %= 25;

                Parameters.AbsoluteFrameNumber++;
            }

            long frameNumber = Parameters.CurrentControlFrame;
            long timeSlot = Parameters.CurrentTimeSlot;


            /* 
             * tricky! the BTS sends the bursts with 156 bits instead of 156.25
             * but it delays one bit after 4 bursts. compensate this here.
             */
            Decoder.StartOffset = (int)(Oversampling * (2 - (timeSlot % 4) * 0.25f));

            /* get the burst handler */
            Burst handler = null;

            if (TimeSlotHandlers[timeSlot].Length == 51)
                handler = TimeSlotHandlers[timeSlot][Parameters.CurrentControlFrame];
            else if (TimeSlotHandlers[timeSlot].Length == 25)
                handler = TimeSlotHandlers[timeSlot][Parameters.CurrentTrafficFrame];
            else
                return;

            /* this is a SCH burst */
            if (Parameters.FirstSCH || handler == SCH)
            {
                Parameters.FirstSCH = false;

                /* try to detect sequence and update sampling offset */
                if (!HandleSCHTrain(timeSlotSamples))
                    return;
            }

            /* continue to decode the packet */
            bool[] data = Decoder.Decode(timeSlotSamples);

            if (handler != null)
            {
                bool success = handler.ParseData(Parameters, DifferenceCode.Decode(data));

                if (success)
                {
                    if (handler.StatusMessage != null)
                        AddMessage("   [" + handler.Name + "] " + handler.StatusMessage + Environment.NewLine);
                }
                else
                {
                    AddMessage("   [" + handler.Name + "]  ERROR: " + handler.ErrorMessage + Environment.NewLine);
                }
            }

            if (L3.StatusMessage != null)
            {
                AddMessage("   [L3] " + L3.StatusMessage + Environment.NewLine);
                L3.StatusMessage = null;
            }

        }

    }

}
