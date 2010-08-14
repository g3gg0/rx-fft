using System;
using System.IO;
using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;
using LibRXFFT.Libraries.GSM.Layer2;
using LibRXFFT.Libraries.GSM.Layer3;
using LibRXFFT.Libraries.GSM.Misc;
using System.Text;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class TCHBurst : NormalBurst
    {
        public SACCHBurst AssociatedSACCH = null;

        private int TCHSeq = 0;
        private int SubChannel;

        private int BurstShiftCount = 7;
        private bool[] Class1DataConv = new bool[378];
        private bool[] Class1Data = new bool[189];

        private bool[] GSMFrameBufferD = new bool[260];

        /* for GSM audio extraction */
        private bool[] RTPFrameBool = new bool[264];
        private byte[] RTPFrameByte = new byte[33];

        private bool[] WAV49FrameBool = new bool[520];
        private byte[] WAV49FrameByte = new byte[65];
        private bool WAV49First = true;

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
                DummyBursts++;
                if (DumpRawData)
                    StatusMessage = "Dummy Burst";
                return eSuccessState.Succeeded;
            }

            /* get 114 e[] bits from burst into our buffer. two bits are just stealing flags */
            UnmapToE(decodedBurst);

            if (AssociatedSACCH.EncryptionState && SACCHBurst.DumpEncryptedMessage)
            {
                StatusMessage = "(Encrypted) e[]: ";
                DumpBits(BurstBufferE);
                return eSuccessState.Unknown;
            }

            /* here we would have to decrypt 114 e[]-bits of the 116 bits in burst. */
            DecryptA5(param, BurstBufferE);

            /* decode e[] to i[] and save into our burstbuffer */
            CopyEToI(TCHSeq++);
            //UnmapToI(decodedBurst, TCHSeq++);


            /* GSM 05.03 Ch 2.1 */
            /* when we got 8 TCH bursts */
            if (TCHSeq == 8)
            {
                /* deinterleave the 8 TCH bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                /* was this burst stolen for a FACCH? hl(B) (in e[]) is set for the last 4 bursts */
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
                            DataBursts++;
                            success = eSuccessState.Succeeded;
#if false
                            #region Microsoft WAV49 GSM Format
                            if (WAV49First)
                            {
                                BitMapping.Unmap(GSMFrameBufferD, 0, WAV49FrameBool, 0, BitMapping.g610BitOrder);
                            }
                            else
                            {
                                /* directly unmap into boolean WAV49 frame buffer */
                                BitMapping.Unmap(GSMFrameBufferD, 0, WAV49FrameBool, 260, BitMapping.g610BitOrder);

                                /* convert that WAV49 frame to byte[] */
                                ByteUtil.BitsToBytes(WAV49FrameBool, WAV49FrameByte);

                                try
                                {
                                    if (OutFile == null)
                                    {
                                        string name = ("GSM_" + Name + "_" + param.FN + ".wav").Replace("/", "_");
                                        OutFile = new FileStream(name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                                        WriteHeader(OutFile);
                                    }

                                    /* and write it */
                                    WriteBuffer(OutFile, WAV49FrameByte);
                                    WriteHeader(OutFile);
                                    StatusMessage = "GSM 06.10 Voice data (" + OutFile.Name + ")";
                                }
                                catch (Exception e)
                                {
                                    StatusMessage = "GSM 06.10 Voice data (Writing file failed, " + e.GetType() + ")";
                                }
                            }
                            WAV49First = !WAV49First;
                            #endregion

#else

                            #region RTP A/V Format
                            /* GSM frame magic */
                            RTPFrameBool[0] = true;
                            RTPFrameBool[1] = true;
                            RTPFrameBool[2] = false;
                            RTPFrameBool[3] = true;

                            /* directly unmap into boolean RTP frame buffer */
                            BitMapping.Unmap(GSMFrameBufferD, 0, RTPFrameBool, 4, BitMapping.g610BitOrder);

                            /* convert that RTP frame to byte[] */
                            ByteUtil.BitsToBytes(RTPFrameBool, RTPFrameByte);

                            try
                            {
                                if (OutFile == null)
                                {
                                    string name = ("GSM_" + Name + "_" + param.FN + ".gsm").Replace("/", "_");
                                    OutFile = new FileStream(name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                                }

                                /* and write it */
                                OutFile.Write(RTPFrameByte, 0, RTPFrameByte.Length);
                                StatusMessage = "GSM 06.10 Voice data (" + OutFile.Name + ")";
                            }
                            catch (Exception e)
                            {
                                StatusMessage = "GSM 06.10 Voice data (Writing file failed, " + e.GetType() + ")";
                            }
                            #endregion
#endif
                        }
                        else
                        {
                            CryptedBursts++;
                            ErrorMessage = "(TCH/F Class Ia: CRC Error)";
                        }
                    }
                    else
                    {
                        CryptedBursts++;
                        ErrorMessage = "(TCH/F Class I: Error in ConvolutionalCoder)";
                    }
                }


                /* 
                 * trick: 
                 * first use the last 8 bursts until one block was successfully decoded.
                 * then use the last 4 bursts as we normally would do.
                 * this will help in finding the correct alignment within the 4 frames.
                 */

                if (success == eSuccessState.Succeeded)
                {
                    BurstShiftCount = 4;
                }
                
                /* save the last n bursts for the next block */
                for (int pos = 0; pos < BurstShiftCount; pos++)
                {
                    Array.Copy(BurstBufferI[(8 - BurstShiftCount) + pos], 0, BurstBufferI[pos], 0, BurstBufferI[pos].Length);
                }

                /* and continue at position n (so we will read another 8-n bursts) */
                TCHSeq = BurstShiftCount;

                /* only when in sync, return error flag */
                if (BurstShiftCount == 4)
                {
                    return success;
                }
            }

            return eSuccessState.Unknown;
        }

        private void DecryptA5(GSMParameters param, bool[] BurstBufferE)
        {
        }

        public override void Release()
        {
            base.Release();

            if (OutFile != null)
            {
                OutFile.Close();
            }
        }

        private void WriteHeader(FileStream OutFile)
        {
            /* Samples per second (always 8000 for this format). */
            uint sample_rate = 8000;
            /* Bytes per second (always 1625 for this format). */
            uint byte_sample_rate = 1625;
            /* This is the size of the "fmt " subchunk */
            uint fmtsize = 20;
            /* WAV #49 */
            ushort fmt = 49;
            /* Mono = 1 channel */
            ushort chans = 1;
            /* Each block of data is exactly 65 bytes in size. */
            uint block_align = 65;
            /* Not actually 2, but rounded up to the nearest bit */
            ushort bits_per_sample = 2;
            /* Needed for compressed formats */
            ushort extra_format = 320;
            /* This is the size of the "fact" subchunk */
            uint factsize = 4;
            /* Number of samples in the data chunk */
            uint num_samples = 0;
            /* Number of bytes in the data chunk */
            uint size = 0;
            /* Write a GSM header, ignoring sizes which will be filled in later */

            ASCIIEncoding enc = new ASCIIEncoding();
                        
            OutFile.Seek(0, SeekOrigin.Begin);

            /*  0: Chunk ID */
            WriteBuffer(OutFile, enc.GetBytes("RIFF"));
            /*  4: Chunk Size */
            WriteBuffer(OutFile, BitConverter.GetBytes((uint)(OutFile.Length - 8)));
            /*  8: Chunk Format */
            WriteBuffer(OutFile, enc.GetBytes("WAVE"));
            /* 12: Subchunk 1: ID */
            WriteBuffer(OutFile, enc.GetBytes("fmt "));
            /* 16: Subchunk 1: Size (minus 8) */
            WriteBuffer(OutFile, BitConverter.GetBytes(fmtsize));
            /* 20: Subchunk 1: Audio format (49) */
            WriteBuffer(OutFile, BitConverter.GetBytes(fmt));
            /* 22: Subchunk 1: Number of channels */
            WriteBuffer(OutFile, BitConverter.GetBytes(chans));
            /* 24: Subchunk 1: Sample rate */
            WriteBuffer(OutFile, BitConverter.GetBytes(sample_rate));
            /* 28: Subchunk 1: Byte rate */
            WriteBuffer(OutFile, BitConverter.GetBytes(byte_sample_rate));
            /* 32: Subchunk 1: Block align */
            WriteBuffer(OutFile, BitConverter.GetBytes(block_align));
            /* 36: Subchunk 1: Bits per sample */
            WriteBuffer(OutFile, BitConverter.GetBytes(bits_per_sample));
            /* 38: Subchunk 1: Extra format bytes */
            WriteBuffer(OutFile, BitConverter.GetBytes(extra_format));
#if false            
            /* 40: Subchunk 2: ID */
            WriteBuffer(OutFile, enc.GetBytes("fact"));
            /* 44: Subchunk 2: Size (minus 8) */
            WriteBuffer(OutFile, BitConverter.GetBytes(factsize));
            /* 48: Subchunk 2: Number of samples */
            WriteBuffer(OutFile, BitConverter.GetBytes(num_samples));
#endif
            /* 52: Subchunk 3: ID */
            WriteBuffer(OutFile, enc.GetBytes("data"));
            /* 56: Subchunk 3: Size */
            WriteBuffer(OutFile, BitConverter.GetBytes((uint)(OutFile.Length - (OutFile.Position + 4))));

            OutFile.Seek(0, SeekOrigin.End);
        }

        private void WriteBuffer(FileStream OutFile, byte[] p)
        {
            OutFile.Write(p, 0, p.Length);
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
