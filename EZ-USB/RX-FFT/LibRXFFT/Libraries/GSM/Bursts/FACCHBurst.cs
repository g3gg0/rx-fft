using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer1;

namespace LibRXFFT.Libraries.GSM.Bursts
{
    class FACCHBurst : NormalBurst
    {
        public static bool ShowEncryptedMessage = false;
        public static bool DumpEncryptedMessage = false;

        private long[] FN;
        private int FACCHSeq = 0;
        private int SubChannel;

        public FACCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "FACCH";
            ShortName = "FA ";
            FN = new long[4];

            InitBuffers(4);
        }

        public FACCHBurst(L3Handler l3, int subChan)
        {
            L3 = l3;
            Name = "FACCH " + subChan;
            ShortName = "FA" + subChan;
            SubChannel = subChan;
            FN = new long[4];

            InitBuffers(4);
        }

        public FACCHBurst(L3Handler l3, string name, int subChan)
        {
            L3 = l3;
            Name = name;
            ShortName = "FA" + subChan;
            SubChannel = subChan;
            FN = new long[4];

            InitBuffers(4);
        }

        public FACCHBurst(L3Handler l3, string name, int subChan, bool tchType)
        {
            L3 = l3;
            Name = name;
            ShortName = "FA" + subChan;
            SubChannel = subChan;
            FN = new long[4];

            InitBuffers(4);
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        public override bool ParseData(GSMParameters param, bool[] burstBufferC, int sequence)
        {
            /* decode */
            if (ConvolutionalCoder.DecodeViterbi(burstBufferC, BurstBufferU) == null)
            {
                ErrorMessage = "(FACCH: Error in ConvolutionalCoder, maybe encrypted)";
                return false;
            }

            /* CRC check/fix */
            switch (CRCCheck())
            {
                case eCRCState.Fixed:
                    StatusMessage = "(FACCH: CRC Error recovered)";
                    break;

                case eCRCState.Failed:
                    ErrorMessage = "(FACCH: CRC Error)";
                    return false;
            }

            /* convert u[] to d[] bytes */
            PackBytes();

            L2.Handle(this, L3, BurstBufferD);

            return true;
        }
    }
}
