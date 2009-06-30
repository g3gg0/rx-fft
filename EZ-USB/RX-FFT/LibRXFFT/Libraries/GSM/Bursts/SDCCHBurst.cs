using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    public class SDCCHBurst : NormalBurst
    {
        public static bool ShowEncryptedMessage = false;
        public static bool DumpEncryptedMessage = false;
        
        private long[] FN;
        private int SubChannel;

        public SDCCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "SDCCH";
            ShortName = "SD ";
            FN = new long[4];

            InitArrays();
        }

        public SDCCHBurst(L3Handler l3, int subChannel)
        {
            L3 = l3;
            Name = "SDCCH " + subChannel;
            ShortName = "SD" + subChannel;
            SubChannel = subChannel;
            FN = new long[4];

            InitArrays();
        }

        public SDCCHBurst(L3Handler l3, string name, int subChannel)
        {
            L3 = l3;
            Name = name;
            ShortName = "SD" + subChannel;
            SubChannel = subChannel;
            FN = new long[4];

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
                return true;
            }

            Array.Copy(decodedBurst, 3, BurstBuffer[sequence], 0, 57);
            Array.Copy(decodedBurst, 88, BurstBuffer[sequence], 57, 57);

            FN[sequence] = param.FN;

            if (FN[0] + 1 == FN[1] && FN[1] + 1 == FN[2] && FN[2] + 1 == FN[3])
            {
                InterleaveCoder.Deinterleave(BurstBuffer, DataDeinterleaved);

                if (ConvolutionalCoder.DecodeViterbi(DataDeinterleaved[0], DataDecoded) == null)
                {
                    if (ShowEncryptedMessage || DumpEncryptedMessage)
                    {
                        if (DumpEncryptedMessage)
                        {
                            StatusMessage = "(Encrypted) De-Interleaved bits: ";
                            DumpBits(DataDeinterleaved[0]);
                        }
                        else
                            StatusMessage = "(Error in ConvolutionalCoder, maybe encrypted)";
                    }
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
                L2.Handle(this, L3, Data);

                Array.Clear(FN, 0, 4);
            }
            else
                StatusMessage = null;

            return true;
        }
    }
}