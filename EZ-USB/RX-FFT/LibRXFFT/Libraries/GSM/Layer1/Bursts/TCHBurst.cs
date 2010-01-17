using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GSM.Layer1;

using LibRXFFT.Libraries.GSM.Layer3;
using System.IO;
using LibRXFFT.Libraries.GSM.Misc;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class TCHBurst : NormalBurst
    {
        private long[] FN;
        private int TCHSeq = 0;
        private int SubChannel;

        private int BurstShiftCount = 7;
        private bool[] Class1DataConv = new bool[378];
        private bool[] Class1Data = new bool[189];

        bool[] GSMFrameBufferD = new bool[260];

        bool[] RTPFrameBool = new bool[264];
        byte[] RTPFrameByte = new byte[33];

        private FileStream OutFile;

        private FACCHBurst FACCH;

        public TCHBurst(L3Handler l3)
        {
            Name = "TCH";
            ShortName = "TC ";

            FACCH = new FACCHBurst(l3);
            L2 = FACCH.L2;
            L3 = FACCH.L3;

            InitBuffers(8);
        }

        public TCHBurst(L3Handler l3, int subChan)
        {
            Name = "TCH " + subChan;
            ShortName = "TC" + subChan;
            SubChannel = subChan;

            FACCH = new FACCHBurst(l3, subChan);
            L2 = FACCH.L2;
            L3 = FACCH.L3;

            InitBuffers(8);
        }

        public TCHBurst(L3Handler l3, string name, int subChan)
        {
            Name = name;
            ShortName = "TC" + subChan;
            SubChannel = subChan;

            FACCH = new FACCHBurst(l3, subChan);
            L2 = FACCH.L2;
            L3 = FACCH.L3;

            InitBuffers(8);
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst)
        {
            return ParseData(param, decodedBurst, 0);
        }

        public override eSuccessState ParseData(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            eSuccessState success = eSuccessState.Unknown;

            if (IsDummy(decodedBurst))
            {
                if (DumpRawData)
                    StatusMessage = "Dummy Burst";
                return eSuccessState.Succeeded;
            }

            /* decode e[] to i[] and save into our burstbuffer */
            UnmapToI(decodedBurst, TCHSeq++);

            /* when we got 8 TCH bursts */
            if (TCHSeq == 8)
            {
                /* deinterleave the 8 TCH bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                /* was this burst stolen for a FACCH? hl(B) is set for the last 4 bursts */
                if (IsHL(decodedBurst))
                {
                    /* pass c[] to FACCH handler */
                    success = FACCH.ParseData(param, BurstBufferC, sequence);

                    StatusMessage = FACCH.StatusMessage;
                    ErrorMessage = FACCH.ErrorMessage;

                    FACCH.StatusMessage = null;
                    FACCH.ErrorMessage = null;
                }
                else
                {
                    /* TCH speech/data (data not supported yet) */

                    /* split up the class 1... */
                    Array.Copy(BurstBufferC, Class1DataConv, Class1DataConv.Length);
                    /* ... and class 2 bits */
                    Array.Copy(BurstBufferC, 378, GSMFrameBufferD, 182, 78);

                    /* use an own convolutional coder buffer for these 188 bits */
                    if (ConvolutionalCoder.Decode(Class1DataConv, Class1Data) != null)
                    {
                        bool[] parityBits = new bool[53];

                        for (int pos = 0; pos < 91; pos++)
                        {
                            GSMFrameBufferD[2 * pos] = Class1Data[pos];
                            GSMFrameBufferD[2 * pos + 1] = Class1Data[184 - pos];
                        }

                        /* calculate parity */
                        Array.Copy(GSMFrameBufferD, 0, parityBits, 0, 50);
                        Array.Copy(Class1Data, 91, parityBits, 50, 3);

                        bool[] crc = CRC.Calc(parityBits, 0, 53, CRC.PolynomialTCHFR);
                        if (CRC.Matches(crc))
                        {
                            success = eSuccessState.Succeeded;

                            /* GSM frame magic */
                            RTPFrameBool[0] = true;
                            RTPFrameBool[1] = true;
                            RTPFrameBool[2] = false;
                            RTPFrameBool[3] = true;

                            /* directly unmap into boolean RTP frame buffer */
                            BitMapping.Unmap(GSMFrameBufferD, 0, RTPFrameBool, 4, BitMapping.g610BitOrder);

                            /* convert that RTP frame to byte[] */
                            ByteUtil.BitsToBytes(RTPFrameBool, RTPFrameByte);

                            if (OutFile == null)
                            {
                                string name = ("GSM_" + Name + "_" + param.FN + ".gsm").Replace("/", "_");
                                OutFile = new FileStream(name, FileMode.Create);
                            }

                            /* and write it */
                            OutFile.Write(RTPFrameByte, 0, RTPFrameByte.Length);

                            StatusMessage = "GSM 06.10 Voice data (" + OutFile.Name + ")";
                        }
                        else
                            ErrorMessage = "(TCH/F Class Ia: CRC Error)";
                    }
                    else
                        ErrorMessage = "(TCH/F Class I: Error in ConvolutionalCoder)";
                }


                /* 
                 * trick: 
                 * first use the last 8 bursts until one block was successfully decoded.
                 * then use the last 4 bursts as we normally would do.
                 * this will help in finding the correct alignment within the 4 frames.
                 */

                if (success == eSuccessState.Succeeded)
                    BurstShiftCount = 4;
                
                /* save the last n bursts for the next block */
                for (int pos = 0; pos < BurstShiftCount; pos++)
                    Array.Copy(BurstBufferI[(8 - BurstShiftCount) + pos], 0, BurstBufferI[pos], 0, BurstBufferI[pos].Length);

                /* and continue at position n (so we will read another 8-n bursts) */
                TCHSeq = BurstShiftCount;

                /* only when in sync, return error flag */
                if (BurstShiftCount == 4)
                    return success;
            }

            return eSuccessState.Unknown;
        }


        /*
        private void UnMapCodecBitsFromTable(bool[] buffer, bool[] bufferUnmapped, int[] table, int start)
        {
            for (int pos = 0; pos < table.Length; pos++)
                bufferUnmapped[table[pos] - 1] = buffer[start + pos];
        }

        private void UnMapCodecBits(bool[] buffer, bool[] bufferUnmapped)
        {
            int[] mapClass1a = new int[] { 6, 53, 109, 165, 221, 5, 12, 17, 4, 11, 16, 22, 43, 99, 155, 211, 52, 108, 164, 220, 10, 26, 30, 42, 98, 154, 210, 41, 97, 153, 209, 40, 96, 152, 208, 39, 95, 151, 207, 51, 107, 163, 219, 3, 21, 33, 38, 94, 150, 206 };
            int[] mapClass1b = new int[] { 25, 29, 45, 101, 157, 213, 37, 93, 149, 205, 47, 103, 159, 215, 2, 9, 15, 36, 20, 24, 32, 44, 100, 156, 212, 50, 106, 162, 218, 56, 59, 62, 65, 68, 71, 74, 77, 80, 83, 86, 89, 92, 112, 115, 118, 121, 124, 127, 130, 133, 136, 139, 142, 145, 148, 168, 171, 174, 177, 180, 183, 186, 189, 192, 195, 198, 201, 204, 224, 227, 230, 233, 236, 239, 242, 245, 248, 251, 254, 257, 260, 46, 102, 158, 214, 49, 105, 161, 217, 55, 58, 61, 64, 67, 70, 73, 76, 79, 82, 85, 88, 91, 111, 114, 117, 120, 123, 126, 129, 132, 135, 138, 141, 144, 147, 167, 170, 173, 176, 179, 182, 185, 188, 191, 194, 197, 200, 203, 223, 226, 229, 232 };
            int[] mapClass2 = new int[] { 235, 238, 241, 244, 247, 250, 253, 256, 259, 1, 8, 14, 28, 31, 35, 34, 13, 19, 18, 23, 48, 104, 160, 216, 54, 57, 60, 63, 66, 69, 72, 75, 78, 81, 84, 87, 90, 110, 113, 116, 119, 122, 125, 128, 131, 134, 137, 140, 143, 146, 166, 169, 172, 175, 178, 181, 184, 187, 190, 193, 196, 199, 202, 222, 225, 228, 231, 234, 237, 240, 243, 246, 249, 252, 255, 258, 7, 27 };

            UnMapCodecBitsFromTable(buffer, bufferUnmapped, mapClass1a, 0);
            UnMapCodecBitsFromTable(buffer, bufferUnmapped, mapClass1b, 50);
            UnMapCodecBitsFromTable(buffer, bufferUnmapped, mapClass2, 182);
        }

        private void ReverseBits(bool[] GSMFrameBoolBuffer, int start, int bits)
        {
            bool[] tmpBits = new bool[bits];
            Array.Copy(GSMFrameBoolBuffer, (start - 1), tmpBits, 0, bits);

            for (int pos = 0; pos < bits; pos++)
                GSMFrameBoolBuffer[(start-1) + pos] = tmpBits[(bits - 1) - pos];
        }

        private void ReverseGSMBits(bool[] GSMFrameBoolBuffer)
        {
            ReverseBits(GSMFrameBoolBuffer, 1, 6);
            ReverseBits(GSMFrameBoolBuffer, 7, 6);
            ReverseBits(GSMFrameBoolBuffer, 13, 5);
            ReverseBits(GSMFrameBoolBuffer, 18, 5);
            ReverseBits(GSMFrameBoolBuffer, 23, 4);
            ReverseBits(GSMFrameBoolBuffer, 27, 4);
            ReverseBits(GSMFrameBoolBuffer, 31, 3);
            ReverseBits(GSMFrameBoolBuffer, 34, 3);
            ReverseBits(GSMFrameBoolBuffer, 37, 7);
            ReverseBits(GSMFrameBoolBuffer, 44, 2);
            ReverseBits(GSMFrameBoolBuffer, 46, 2);
            ReverseBits(GSMFrameBoolBuffer, 48, 6);

            for (int pos = 0; pos < 13; pos++)
                ReverseBits(GSMFrameBoolBuffer, 54 + pos * 3, 3);

            ReverseBits(GSMFrameBoolBuffer, 93, 7);
            ReverseBits(GSMFrameBoolBuffer, 100, 2);
            ReverseBits(GSMFrameBoolBuffer, 102, 2);
            ReverseBits(GSMFrameBoolBuffer, 104, 6);

            for (int pos = 0; pos < 13; pos++)
                ReverseBits(GSMFrameBoolBuffer, 110 + pos * 3, 3);

            ReverseBits(GSMFrameBoolBuffer, 149, 7);
            ReverseBits(GSMFrameBoolBuffer, 156, 2);
            ReverseBits(GSMFrameBoolBuffer, 158, 2);
            ReverseBits(GSMFrameBoolBuffer, 160, 6);

            for (int pos = 0; pos < 13; pos++)
                ReverseBits(GSMFrameBoolBuffer, 166 + pos * 3, 3);

            ReverseBits(GSMFrameBoolBuffer, 205, 7);
            ReverseBits(GSMFrameBoolBuffer, 212, 2);
            ReverseBits(GSMFrameBoolBuffer, 214, 2);
            ReverseBits(GSMFrameBoolBuffer, 216, 6);

            for (int pos = 0; pos < 13; pos++)
                ReverseBits(GSMFrameBoolBuffer, 222 + pos * 3, 3);
        }
         * */
    }
}
