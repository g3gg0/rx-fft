using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    public class SACCHBurst : NormalBurst
    {
        public static bool ShowEncryptedMessage = false;
        public bool TchType = false;

        private long[] FN;
        private int tchSeq = 0;
        private int SubChannel;

        public SACCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "SACCH";
            ShortName = "SA ";
            FN = new long[4];

            InitArrays();
        }

        public SACCHBurst(L3Handler l3, int subChan)
        {
            L3 = l3;
            Name = "SACCH " + subChan;
            ShortName = "SA" + subChan;
            SubChannel = subChan;
            FN = new long[4];

            InitArrays();
        }

        public SACCHBurst(L3Handler l3, string name, int subChan)
        {
            L3 = l3;
            Name = name;
            ShortName = "SA" + subChan;
            SubChannel = subChan;
            FN = new long[4];

            InitArrays();
        }

        public SACCHBurst(L3Handler l3, string name, int subChan, bool tchType)
        {
            L3 = l3;
            Name = name;
            ShortName = "SA" + subChan;
            SubChannel = subChan;
            FN = new long[4];
            TchType = tchType;

            InitArrays();
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            if (IsDummy(decodedBurst, 3))
            {
                if (param.DumpPackets)
                    StatusMessage = "Dummy Burst";

                tchSeq = 0;
                return true;
            }

            bool isComplete;

            /* decide between normal SACCH and SACCH/TCH */
            if (!TchType)
            {
                /* thats a normal SACCH */
                Array.Copy(decodedBurst, 3, BurstBuffer[sequence], 0, 57);
                Array.Copy(decodedBurst, 88, BurstBuffer[sequence], 57, 57);

                FN[sequence] = param.FN;

                /* the frame is complete when 4 sequences of four consecutive FN were buffered */
                isComplete = (FN[0] + 1 == FN[1]) && (FN[1] + 1 == FN[2]) && (FN[2] + 1 == FN[3]);
            }
            else
            {
                /* thats a SACCH/TCH */
                Array.Copy(decodedBurst, 3, BurstBuffer[tchSeq], 0, 57);
                Array.Copy(decodedBurst, 88, BurstBuffer[tchSeq], 57, 57);

                /* when we caught four bursts, the frame is complete */
                tchSeq++;
                isComplete = tchSeq>3;
            }

            if (isComplete)
            {
                /* clean up */
                tchSeq = 0;
                Array.Clear(FN, 0, 4);

                InterleaveCoder.Deinterleave(BurstBuffer, DataDeinterleaved);

                if (ConvolutionalCoder.DecodeViterbi(DataDeinterleaved[0], DataDecoded) == null)
                {
                    if (ShowEncryptedMessage)
                        StatusMessage = "(Error in ConvolutionalCoder, maybe encrypted)";
                    return true;
                }

                CRC.Calc(DataDecoded, 0, 224, CRC.PolynomialFIRE, CRCBuffer);
                if (!CRC.Matches(CRCBuffer))
                {
                    bool[] DataRepaired = new bool[224];

                    FireCode fc = new FireCode(40, 184);
                    if (!fc.FC_check_crc(DataDecoded, DataRepaired))
                    {
                        ErrorMessage = "(Error in CRC)";
                        return false;
                    }

                    StatusMessage = "(CRC Error recovered)";
                    Array.Copy(DataRepaired, DataDecoded, DataRepaired.Length);
                }

                ByteUtil.BitsToBytesRev(DataDecoded, Data, 0, 184);

                //DumpBytes(Data);
                L2.Handle(this, L3, Data, 2);
            }
            else
                StatusMessage = null;

            return true;
        }
    }
}