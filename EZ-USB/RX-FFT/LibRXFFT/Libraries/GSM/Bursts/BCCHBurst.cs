﻿using System;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM.Layer1;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Bursts
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

        public override bool ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            if (IsDummy(decodedBurst))
            {
                if (param.DumpPackets)
                    StatusMessage = "Dummy Burst";
                return true;
            }

            UnmapToI(decodedBurst, sequence);

            if (sequence == 3)
            {
                /* deinterleave the 4 bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                if (!Deconvolution())
                {
                    ErrorMessage = "(Error in ConvolutionalCoder)";
                    return false;
                }

                /* CRC check/fix */
                switch (CRCCheck())
                { 
                    case eCRCState.Fixed:
                        StatusMessage = "(CRC Error recovered)";
                        break;

                    case eCRCState.Failed:
                        ErrorMessage = "(CRC Error)";
                        return false;
                }

                /* convert u[] to d[] bytes */
                PackBytes();

                /* BCCH and CCCH have L2 Pseudo Length */
                if ((BurstBufferD[0] & 3) != 1)
                {
                    ErrorMessage = "(Error in L2 Pseudo Length)";
                    return false;
                }

                L2.Handle(this, L3, BurstBufferD);
            }
            else
                StatusMessage = null;

            return true;
        }
    }
}