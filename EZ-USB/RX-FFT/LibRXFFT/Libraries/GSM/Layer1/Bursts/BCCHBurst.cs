using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class BCCHBurst : NormalBurst
    {
        public BCCHBurst(L3Handler l3)
        {
            L3 = l3;
            Name = "BCCH";
            ShortName = "BC ";
            InitBuffers(4);
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            if (IsDummy(decodedBurst))
            {
                if (DumpRawData)
                    StatusMessage = "Dummy Burst";
                return eSuccessState.Succeeded;
            }

            UnmapToI(decodedBurst, sequence);

            if (sequence == 3)
            {
                /* deinterleave the 4 bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                if (!Deconvolution())
                {
                    ErrorMessage = "(Error in ConvolutionalCoder)";
                    return eSuccessState.Failed;
                }

                /* CRC check/fix */
                switch (CRCCheck())
                { 
                    case eCRCState.Fixed:
                        StatusMessage = "(CRC Error recovered)";
                        break;

                    case eCRCState.Failed:
                        ErrorMessage = "(CRC Error)";
                        return eSuccessState.Failed;
                }

                /* convert u[] to d[] bytes */
                PackBytes();

                /* BCCH and CCCH have L2 Pseudo Length */
                if ((BurstBufferD[0] & 3) != 1)
                {
                    ErrorMessage = "(Error in L2 Pseudo Length)";
                    return eSuccessState.Failed;
                }

                L2.Handle(this, L3, BurstBufferD);

                return eSuccessState.Succeeded;
            }
            else
                StatusMessage = null;

            return eSuccessState.Unknown;
        }
    }
}