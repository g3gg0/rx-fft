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

            EncryptionType = AssociatedSACCH.EncryptionType;
            ChannelEncrypted = AssociatedSACCH.ChannelEncrypted;

            StoreBurstContext(param, decodedBurst, TCHSeq);

            /* GSM 05.03 Ch 2.1 */
            /* when we got 8 TCH bursts */
            if (++TCHSeq == 8)
            {
                TCHSeq = 0;

                /* try to decrypt buffer if this is enabled */
                if (!HandleEncryption(param))
                {
                    /* encrypted but no decryptor available, silently return */
                    return eSuccessState.Unknown;
                }

                CopyEToI();

                /* deinterleave the 8 TCH bursts. the result is a 456 bit block. i[] to c[] */
                Deinterleave();

                /* was this burst stolen for a FACCH? hl(B) (in e[]) is set for the last 4 bursts */
                if (IsHL(decodedBurst))
                {
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
    }
}
