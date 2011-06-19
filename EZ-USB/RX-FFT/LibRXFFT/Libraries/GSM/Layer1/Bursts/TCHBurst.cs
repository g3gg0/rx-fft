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
        private FileStream OutFileRaw;

        public FACCHBurst FACCH;
        public SACCHBurst AssociatedSACCH = null;
        public int ChannelMode = 0;

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


        /*
         * GSM-05.03 3.1.1 v9.0
         * 
         *     s(1..244)  EFR/AMR 244 bits
         *         |
         *         |   preliminary stage adding redundancy
         *        \|/
         *     w(1..260)
         *         |
         *         |   rearranged through table 6
         *        \|/
         *     d(1..260)
         *         |
         *         |   channel encoding TCH/FS
         *        \|/
         *       burst
         */

        internal bool[] BurstBufferW = new bool[260];
        internal bool[] BurstBufferS = new bool[244];
        internal bool[] BurstBufferSpeechBits = new bool[244];

        internal void UnmapDToW()
        {
            BitMapping.Unmap(GSMFrameBufferD, 0, BurstBufferW, 0, BitMapping.EFRBitOrder);
        }

        internal void UnmapWToS()
        {
            /*
             * GSM-05.03 3.1.1
             * 
             * w(k) = s(70)  for k = 72  and 73 
             * w(k) = s(120) for k = 124 and 125 
             * w(k) = s(173) for k = 179 and 180  
             * w(k) = s(223) for k = 231 and 232 
             * 
             * repetition bits
             *
             * break here if the repetition bits don't match 
             */
            if ((BurstBufferW[71] ^ BurstBufferW[72]) | (BurstBufferW[123] ^ BurstBufferW[124]) | (BurstBufferW[178] ^ BurstBufferW[179]) | (BurstBufferW[230] ^ BurstBufferW[231]))
            {
                return;
            }

            /*
             * GSM-05.03 3.1.1
             * 
             * w(k) = s(k)   for k = 1   to 71
             * w(k) = s(k-2) for k = 74  to 123 
             * w(k) = s(k-4) for k = 126 to 178 
             * w(k) = s(k-6) for k = 181 to s230 
             * w(k) = s(k-8) for k = 233 to s252
             * 
             */
            for (int k = 0; k < 71; k++)
            {
                BurstBufferS[k] = BurstBufferW[k];
            }
            for (int k = 73; k < 123; k++)
            {
                BurstBufferS[k - 2] = BurstBufferW[k];
            }
            for (int k = 125; k < 178; k++)
            {
                BurstBufferS[k - 4] = BurstBufferW[k];
            }
            for (int k = 180; k < 230; k++)
            {
                BurstBufferS[k - 6] = BurstBufferW[k];
            }
            for (int k = 232; k < 252; k++)
            {
                BurstBufferS[k - 8] = BurstBufferW[k];
            }

            BitMapping.Map(BurstBufferS, 0, BurstBufferSpeechBits, 0, BitMapping.AMR12BitOrder);
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
                State = eBurstState.Idle;
                DummyBursts++;

                //CloseFiles();

                /* don't treat TCHs as a reliable source for end-of-connection detection */
                //DummyBurstReceived(param);

                if (DumpRawData)
                    StatusMessage = "Dummy Burst";
                return eSuccessState.Succeeded;
            }

            EncryptionType = AssociatedSACCH.EncryptionType;
            ChannelEncrypted = AssociatedSACCH.ChannelEncrypted;

            StoreBurstContext(param, decodedBurst, TCHSeq);

            /* GSM 05.03 Ch 2.1 */
            /* when we got 8 TCH bursts */
            if (++TCHSeq == 8)
            {
                TCHSeq = 0;

                /* try to decrypt buffer if this is enabled, but do not try to crack the key */
                if (!HandleEncryption(param, false))
                {
                    State = eBurstState.CryptedTraffic;

                    /* encrypted but no decryptor available, silently return */
                    return eSuccessState.Unknown;
                }                

                /* deinterleave the 8 TCH bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                /* 
                 * GSM-05.03 4.2.5
                 * was this burst stolen for a FACCH? hl(B) (in e[]) is set for the last 4 bursts */
                if (IsHL(decodedBurst))
                {
                    /* pass encryption information to FACCH */
                    FACCH.A5Algorithm = A5Algorithm;
                    FACCH.A5CipherKey = A5CipherKey;
                    FACCH.ChannelEncrypted = ChannelEncrypted;

                    /* pass c[] to FACCH handler */
                    success = FACCH.ParseFACCHData(param, BurstBufferC);

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
                    if (ConvolutionalCoder.Decode(Class1DataConv, ref Class1Data) == 0)
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

                            if (ChannelEncrypted)
                            {
                                State = eBurstState.DecryptedTraffic;
                            }
                            else
                            {
                                State = eBurstState.PlainTraffic;
                            }
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

#endif
                            if (ChannelMode != 33)
                            {
                                #region write audio dump in RTP A/V Format
                                /* GSM frame magic */
                                RTPFrameBool[0] = true;
                                RTPFrameBool[1] = true;
                                RTPFrameBool[2] = false;
                                RTPFrameBool[3] = true;

                                /* directly unmap into boolean RTP frame buffer */
                                BitMapping.Unmap(GSMFrameBufferD, 0, RTPFrameBool, 4, BitMapping.g610BitOrder);

                                /* convert that RTP frame to byte[] */
                                ByteUtil.BitsToBytes(RTPFrameBool, RTPFrameByte);

                                StatusMessage = "";

                                if (ChannelEncrypted)
                                {
                                    StatusMessage += "======= encrypted =======" + Environment.NewLine;
                                }

                                try
                                {
                                    if (OutFile == null)
                                    {
                                        string name = ("GSM_" + Name + "_" + param.FN).Replace("/", "_");
                                        OutFile = new FileStream(name + ".gsm", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                                        //OutFileRaw = new FileStream(name + ".raw", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                                        StatusMessage += "Created file: '" + name + "'" + Environment.NewLine;
                                    }

                                    /* and write it */
                                    OutFile.Write(RTPFrameByte, 0, RTPFrameByte.Length);
                                    OutFile.Flush();

                                    /*
                                    Array.Copy(GSMFrameBufferD, 0, RTPFrameBool, 4, GSMFrameBufferD.Length);
                                    RTPFrameBool[0] = false;
                                    RTPFrameBool[1] = false;
                                    RTPFrameBool[2] = false;
                                    RTPFrameBool[3] = false;
                                    ByteUtil.BitsToBytes(RTPFrameBool, RTPFrameByte);
                                    OutFileRaw.Write(RTPFrameByte, 0, RTPFrameByte.Length);
                                    */

                                    StatusMessage += "GSM 06.10 Voice data (" + OutFile.Name + ")";
                                }
                                catch (Exception e)
                                {
                                    StatusMessage += "GSM 06.10 Voice data (Writing file failed, " + e.GetType() + ")";
                                }
                                #endregion
                            }
                            else
                            {
                                #region write audio dump in AMR Format (assume 12.2kbit/s)

                                UnmapDToW();
                                UnmapWToS();

                                /* convert that AMR frame to byte[] */
                                ByteUtil.BitsToBytes(BurstBufferSpeechBits, RTPFrameByte);

                                StatusMessage = "";

                                if (ChannelEncrypted)
                                {
                                    StatusMessage += "======= encrypted =======" + Environment.NewLine;
                                }

                                try
                                {
                                    if (OutFile == null)
                                    {
                                        byte[] fileHeader = new byte[] { 0x23, 0x21, 0x41, 0x4D, 0x52, 0x0A };
                                        string name = ("GSM_" + Name + "_" + param.FN).Replace("/", "_");
                                        OutFile = new FileStream(name + ".amr", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                                        OutFile.Write(fileHeader, 0, fileHeader.Length);
                                    }

                                    /* and write it */
                                    OutFile.WriteByte(0x3C);
                                    OutFile.Write(RTPFrameByte, 0, 31);
                                    OutFile.Flush();

                                    StatusMessage += "GSM 06.90 Voice data (" + OutFile.Name + ")";
                                }
                                catch (Exception e)
                                {
                                    StatusMessage += "GSM 06.90 Voice data (Writing file failed, " + e.GetType() + ")";
                                }

                                #endregion
                            }
                        }
                        else
                        {
                            State = eBurstState.Failed;
                            CryptedFrames++;
                            ErrorMessage = "(TCH/F Class Ia: CRC Error)";
                        }
                    }
                    else
                    {
                        State = eBurstState.Failed;
                        CryptedFrames++;
                        ErrorMessage = "(TCH/F Class I: Error in ConvolutionalCoder)";
                    }
                }


                /* trick: 
                 * first use the last 8 bursts until one block was successfully decoded.
                 * then use the last 4 bursts as we normally would do.
                 * this will help in finding the correct alignment within the 4 frames.
                 */

                if (success == eSuccessState.Succeeded)
                {
                    BurstShiftCount = 4;
                }
                else
                {
                    BurstShiftCount = 7;
                }
                
                /* save the last n bursts for the next block */
                for (int pos = 0; pos < BurstShiftCount; pos++)
                {
                    BurstData src = BurstBlock[(8 - BurstShiftCount) + pos];
                    BurstData dst = BurstBlock[pos];

                    dst.FN = src.FN;
                    dst.Count = src.Count;
                    Array.Copy(src.BurstBufferI, 0, dst.BurstBufferI, 0, dst.BurstBufferI.Length);
                    Array.Copy(src.BurstBufferE, 0, dst.BurstBufferE, 0, dst.BurstBufferE.Length);
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
            CloseFiles();
        }

        private void CloseFiles()
        {
            if (OutFile != null)
            {
                OutFile.Close();
            }

            if (OutFileRaw != null)
            {
                OutFileRaw.Close();
            }

            OutFile = null;
            OutFileRaw = null;
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
    }
}
