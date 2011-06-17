using System;
using LibRXFFT.Libraries.GSM.Layer1.ChannelCoding;
using LibRXFFT.Libraries.GSM.Layer2;
using RX_FFT.Components.GDI;
using System.Collections.Generic;
using System.Collections;

namespace LibRXFFT.Libraries.GSM.Layer1.Bursts
{
    public class NormalBurst : Burst
    {
        internal enum eCRCState
        {
            Correct,
            Fixed,
            Failed
        }

        public enum eBurstState
        {
            Idle,
            PlainTraffic,
            CryptedTraffic,
            DecryptedTraffic,
            Failed
        }

        internal class BurstData
        {
            /* encrypted e[] bits */
            internal bool[] BurstBufferE = new bool[114];

            /* interleaved i[] bits */
            internal bool[] BurstBufferI = new bool[114];

            /* frame number */
            internal long FN;

            /* A5 related frame number */
            internal uint Count;

            internal BurstData Clone()
            {
                BurstData ret = new BurstData();

                ret.BurstBufferE = (bool[])BurstBufferE.Clone();
                ret.BurstBufferI = (bool[])BurstBufferI.Clone();
                ret.FN = FN;
                ret.Count = Count;

                return ret;
            }
        }

        public const double Data1Bits = 57;
        public const double HLBits = 1;
        public const double SyncBits = 26;
        public const double HUBits = 1;
        public const double Data2Bits = 57;

        public const double Data1BitsPos = LeadingTailBits;
        public const double HLBitsPos = Data1BitsPos + Data1Bits;
        public const double SyncBitsPos = HLBitsPos + HLBits;
        public const double HUBitsPos = SyncBitsPos + SyncBits;
        public const double Data2BitsPos = HUBitsPos + HUBits;

        public long DummyBursts = 0;
        public long DataBursts = 0;
        public long CryptedFrames = 0;

        public eBurstState State = eBurstState.Idle;
        public long TimeSlot = -1;

        internal bool[] FireCRCBuffer;


        /* Burstblock contains 4 or 8 bursts and their context */
        internal BurstData[] BurstBlock;


        /* interleaved i[] bits, references to BurstBlock */
        internal bool[][] BurstBufferI;

        /* convolutional coded c[] bits */
        internal bool[] BurstBufferC;

        /* data bits u[] */
        internal bool[] BurstBufferU;

        /* resulting data byte array d[] */
        internal byte[] BurstBufferD;


        /* data supplied by timeslothandler and its callbacks */
        public string EstablishmentCause = "";
        /* 
         * Reason why the mobile got an assignment
         *
         * IMMEDIATE ASSIGNMENT (9.1.18)
         *     Answer to paging (SDCCH)   the mobile was called by the BTS for some SDCCH stuff e.g. SMS
         *     Location updating (SDCCH)  the mobile wants to update its location info
         *     Other procedures (SDCCH)   the mobile wants to e.g. send a SMS
         *     unknown type               maybe handover
         * 
         */
        public string ServiceType = "";
        /* 
         * Service requested by the mobile
         *
         * CM SERVICE REQUEST (9.2.9)
         *     Mobile originating call establishment or packet mode connection establishment
         *     Emergency call establishment
         *     Short message service
         *     Supplementary service activation
         *     Voice group call establishment
         *     Voice broadcast call establishment
         *     Location Services
         * 
         * LOCATION UPDATING REQUEST (9.2.15)
         *     Normal location updating
         *     Periodic updating
         *     IMSI attach
         * 
         * 
         */


        internal void InitBuffers(int burstCount)
        {
            BurstBufferI = new bool[burstCount][];
            BurstBufferC = new bool[456];
            BurstBufferU = new bool[228];
            BurstBufferD = new byte[23];

            BurstBlock = new BurstData[burstCount];

            for (int pos = 0; pos < BurstBlock.Length; pos++)
            {
                BurstBlock[pos] = new BurstData();
                BurstBufferI[pos] = BurstBlock[pos].BurstBufferI;
            }

            FireCRCBuffer = new bool[CRC.PolynomialFIRE.Length - 1];
        }

        internal void StoreBurstContext(GSMParameters param, bool[] decodedBurst, int sequence)
        {
            /* get 114 e[] bits from burst into our buffer. two bits are just stealing flags - skip them */
            UnmapToE(decodedBurst, sequence);

            /* store context */
            BurstBlock[sequence].FN = param.FN;
            BurstBlock[sequence].Count = param.Count;
        }

        internal bool AllBurstsReceived()
        {
            for (int pos = 1; pos < BurstBlock.Length; pos++)
            {
                if ((BurstBlock[pos - 1].FN + 1) != BurstBlock[pos].FN)
                {
                    return false;
                }
            }

            return true;
        }

        internal void ClearBurstContext()
        {
            for (int pos = 0; pos < BurstBlock.Length; pos++)
            {
                BurstBlock[pos].FN = 0;
            }
        }

        internal void UnmapToI(bool[] bits, int dstBurst)
        {
            Array.Copy(bits, (int)Data1BitsPos, BurstBlock[dstBurst].BurstBufferI, 0, (int)Data1Bits);
            Array.Copy(bits, (int)Data2BitsPos, BurstBlock[dstBurst].BurstBufferI, (int)Data1Bits, (int)Data2Bits);
        }

        internal void UnmapToE(bool[] bits, int dstBurst)
        {
            Array.Copy(bits, (int)Data1BitsPos, BurstBlock[dstBurst].BurstBufferE, 0, (int)Data1Bits);
            Array.Copy(bits, (int)Data2BitsPos, BurstBlock[dstBurst].BurstBufferE, (int)Data1Bits, (int)Data2Bits);
        }

        internal void CopyEToI()
        {
            for (int dstBurst = 0; dstBurst < BurstBlock.Length; dstBurst++)
            {
                Array.Copy(BurstBlock[dstBurst].BurstBufferE, BurstBlock[dstBurst].BurstBufferI, (int)(Data1Bits + Data2Bits));
            }
        }

        internal void Deinterleave(bool[][] sourceBufferI)
        {
            InterleaveCoder.Deinterleave(sourceBufferI, new[] { BurstBufferC });
        }

        internal void Deinterleave()
        {
            Deinterleave(BurstBufferI);
        }

        internal bool Deconvolution()
        {
            return ConvolutionalCoder.Decode(BurstBufferC, BurstBufferU) != null;
        }

        internal eCRCState CRCCheck()
        {
            CRC.Calc(BurstBufferU, 0, 224, CRC.PolynomialFIRE, FireCRCBuffer);
            if (!CRC.Matches(FireCRCBuffer))
            {
                bool[] DataRepaired = new bool[224];

                FireCode fc = new FireCode(40, 184);
                if (!fc.FC_check_crc(BurstBufferU, DataRepaired))
                    return eCRCState.Failed;

                Array.Copy(DataRepaired, BurstBufferU, DataRepaired.Length);

                return eCRCState.Fixed;
            }
            return eCRCState.Correct;
        }

        internal void PackBytes()
        {
            ByteUtil.BitsToBytesRev(BurstBufferU, BurstBufferD, 0, 184);
        }

        internal bool IsHL(bool[] decodedBurst)
        {
            return decodedBurst[(int)HLBitsPos];
        }

        internal bool IsHU(bool[] decodedBurst)
        {
            return decodedBurst[(int)HUBitsPos];
        }


        /* for burst types which allow encryption */
        public bool ChannelEncrypted = false;
        public int EncryptionType = 0;
        public static bool ShowEncryptedMessage = false;
        public static bool DumpEncryptedMessageBits = false;
        public static bool DumpRawBits = false;
        public string EncryptionBitString = "";


        public byte[] A5CipherKey = null;
        internal CryptA5 A5Algorithm = null;
        public SACCHBurst AssociatedSACCH = null;

        internal bool AutoCrackBursts = true;
        internal int BurstsToLog = 4 * 300;
        internal int BurstsToCrack = 4 * 3;
        internal LinkedList<BurstData> LastBursts = new LinkedList<BurstData>();

        /*
         * empty L2 frames have encrypted burst data like this. helps finding the encryption key bits.
         * these are for O² and are idle L2 frames. they get sent in the first frame and after
         * the channel was released.
         * 
           [L1] [ARFCN: 1001] [MCC: 262] [MNC: 7] [LAC: 50834] [CellID: 206] 
           [L1] [SDCCH 7] - [T1:  1205 T2: 23 T3: 31 TN: 1 FN:  1598269]
                COUNT: 2468756 Burst #0 (Decrypted) i[]: 100000010001110101010000000010100000000111111101010000001010000100010111010100000000101000010000010101010100000010 (   Key   ) c[]: 010111011011100101000011110100100000000011010000111100101111101001100100111001001010000000001110011110001000101010 (Encrypted) e[]: 110111001010010000010011110110000000000100101101101100100101101101110011101101001010101000011110001011011100101000
                COUNT: 2468789 Burst #1 (Decrypted) i[]: 101010111111111101000000101010101111111111110100000000100010111111111111010101000000001010101011011101010000001000 (   Key   ) c[]: 110000100110000111010100010100101100110010110110101000011110010000000110011001111100001111100111001011011010001000 (Encrypted) e[]: 011010011001111010010100111110000011001101000010101000111100101111111001001100111100000101001100010110001010000000
                COUNT: 2468822 Burst #2 (Decrypted) i[]: 000000011111010101010000100000010001010111010101000010100001010001111101010001000010000000000101110101010100000010 (   Key   ) c[]: 000111010100100110101101010011001000001100100111010100111011010001111000001000000001001001110101111011000000111011 (Encrypted) e[]: 000111001011110011111101110011011001011011110010010110011010000000000101011001000011001001110000001110010100111001
                COUNT: 2468855 Burst #3 (Decrypted) i[]: 000100001010101010111101110101010000000010101110111111010100010000001010101011011101010001000010001011101111010101 (   Key   ) c[]: 010101010001100011100000100011111111101000000100011011010101100000110000010001011011101111111101010100101000010111 (Encrypted) e[]: 010001011011001001011101010110101111101010101010100100000001110000111010111010000110111110111111011111000111000010

           [L2] SAPI: 0  C/R: 1  EA: 1  M: 0  EL: 1  L: 0  U Format, U=0 UI
                ======= encrypted =======
                Raw Data
                     03 03 01 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 
         * 
         */
        private bool[][] GuessedBitsUI0 = new[] 
        {
            new [] { true, false, false, false, false, false, false, true, false, false, false, true, true, true, false, true, false, true, false, true, false, false, false, false, false, false, false, false, true, false, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, false, true, false, true, false, false, false, false, false, false, true, false, true, false, false, false, false, true, false, false, false, true, false, true, true, true, false, true, false, true, false, false, false, false, false, false, false, false, true, false, true, false, false, false, false, true, false, false, false, false, false, true, false, true, false, true, false, true, false, true, false, false, false, false, false, false, true, false },
            new [] { true, false, true, false, true, false, true, true, true, true, true, true, true, true, true, true, false, true, false, false, false, false, false, false, true, false, true, false, true, false, true, false, true, true, true, true, true, true, true, true, true, true, true, true, false, true, false, false, false, false, false, false, false, false, true, false, false, false, true, false, true, true, true, true, true, true, true, true, true, true, true, true, false, true, false, true, false, true, false, false, false, false, false, false, false, false, true, false, true, false, true, false, true, false, true, true, false, true, true, true, false, true, false, true, false, false, false, false, false, false, true, false, false, false },
            new [] { false, false, false, false, false, false, false, true, true, true, true, true, false, true, false, true, false, true, false, true, false, false, false, false, true, false, false, false, false, false, false, true, false, false, false, true, false, true, false, true, true, true, false, true, false, true, false, true, false, false, false, false, true, false, true, false, false, false, false, true, false, true, false, false, false, true, true, true, true, true, false, true, false, true, false, false, false, true, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, true, false, true, true, true, false, true, false, true, false, true, false, true, false, false, false, false, false, false, true, false },
            new [] { false, false, false, true, false, false, false, false, true, false, true, false, true, false, true, false, true, false, true, true, true, true, false, true, true, true, false, true, false, true, false, true, false, false, false, false, false, false, false, false, true, false, true, false, true, true, true, false, true, true, true, true, true, true, false, true, false, true, false, false, false, true, false, false, false, false, false, false, true, false, true, false, true, false, true, false, true, true, false, true, true, true, false, true, false, true, false, false, false, true, false, false, false, false, true, false, false, false, true, false, true, true, true, false, true, true, true, true, false, true, false, true, false, true }
        };

        /*
         * The first crypted might also be a RR with N(R)=2
         * 
         * 
           [L1] [ARFCN: 1001] [MCC: 262] [MNC: 7] [LAC: 51802] [CellID: 206] 
           [L1] [SDCCH 6] - [T1:   978 T2: 10 T3: 27 TN: 1 FN:  1297722]
                ======= Found encryption key: D9AEE14845BB05DD =======
                COUNT: 2003719 Burst #0 (Decrypted) i[]: 100000000101110101010100000000100001000101011101000000100010000000011111110100010000001000010100011111010100010010 (   Key   ) c[]: 010111111010100011010010011101011001110010000100111000100111001010110110001001101001110011000001000100000111101111 (Encrypted) e[]: 110111111111010110000110011101111000110111011001111000000101001010101001111101111001111011010101011011010011111101
                COUNT: 2003752 Burst #1 (Decrypted) i[]: 101011101101011101010000100010101010110101110101000010001010111011110111010000000010101010101111011101010001000010 (   Key   ) c[]: 010010000111110011111111100101011011111010101000010110011110000011101001001111011111101001100110110001011000101000 (Encrypted) e[]: 111001101010101110101111000111110001001111011101010100010100111000011110011111011101000011001001101100001001101010
                COUNT: 2003785 Burst #2 (Decrypted) i[]: 000100010101010101010000101000000101010101010100000010000000000001010101000001000000000101000101010101000100000010 (   Key   ) c[]: 100110111101001110110001011101110111011011111101111000111001111111101101000110000010111100100011111100101100001101 (Encrypted) e[]: 100010101000011011100001110101110010001110101001111010111001111110111000000111000010111001100110101001101000001111
                COUNT: 2003818 Burst #3 (Decrypted) i[]: 010100001000101111110101010101010000001010101010111101010001010000101011101111010101010001000010001010101111010101 (   Key   ) c[]: 101111110100010111111101100011011111110100100000000100001100111001110011001010010101100111111101110011111111101010 (Encrypted) e[]: 111011111100111000001000110110001111111110001010111001011101101001011000100101000000110110111111111001010000111111

           [L2] SAPI: 0  C/R: 0  EA: 1  M: 0  EL: 1  L: 0  S Format, N(R)=2 S=0 RR
                ======= encrypted =======
                Raw Data
                     01 41 01 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B 2B AA 
         *          
         */
        private bool[][] GuessedBitsRR2 = new[] 
        {
            new [] { true, false, false, false, false, false, false, false, false, true, false, true, true, true, false, true, false, true, false, true, false, true, false, false, false, false, false, false, false, false, true, false, false, false, false, true, false, false, false, true, false, true, false, true, true, true, false, true, false, false, false, false, false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, false, true, false, false, false, true, false, false, false, false, false, false, true, false, false, false, false, true, false, true, false, false, false, true, true, true, true, true, false, true, false, true, false, false, false, true, false, false, true, false },
            new [] { true, false, true, false, true, true, true, false, true, true, false, true, false, true, true, true, false, true, false, true, false, false, false, false, true, false, false, false, true, false, true, false, true, false, true, false, true, true, false, true, false, true, true, true, false, true, false, true, false, false, false, false, true, false, false, false, true, false, true, false, true, true, true, false, true, true, true, true, false, true, true, true, false, true, false, false, false, false, false, false, false, false, true, false, true, false, true, false, true, false, true, false, true, true, true, true, false, true, true, true, false, true, false, true, false, false, false, true, false, false, false, false, true, false },
            new [] { false, false, false, true, false, false, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, false, false, false, true, false, true, false, false, false, false, false, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, true, false, true, false, true, false, true, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, true, false, true, false, false, false, true, false, true, false, true, false, true, false, true, false, false, false, true, false, false, false, false, false, false, true, false },
            new [] { false, true, false, true, false, false, false, false, true, false, false, false, true, false, true, true, true, true, true, true, false, true, false, true, false, true, false, true, false, true, false, true, false, false, false, false, false, false, true, false, true, false, true, false, true, false, true, false, true, true, true, true, false, true, false, true, false, false, false, true, false, true, false, false, false, false, true, false, true, false, true, true, true, false, true, true, true, true, false, true, false, true, false, true, false, true, false, false, false, true, false, false, false, false, true, false, false, false, true, false, true, false, true, false, true, true, true, true, false, true, false, true, false, true }
        };

        internal void ReplayBursts(GSMParameters param, LinkedList<BurstData> lastBursts)
        {
            bool[] burstBits = new bool[(int)(Data2BitsPos + Data2Bits)];
            bool[][] burstBufferI = new bool[4][];

            for (int burstNum = 0; burstNum < 4; burstNum++)
            {
                burstBufferI[burstNum] = new bool[114];
            }

            StatusMessage += "    __________________________________________________________________________________" + Environment.NewLine;
            StatusMessage += "  //                                                                                  \\\\" + Environment.NewLine;
            StatusMessage += " ||  Replaying L2 now. Predefine the Kc in the options dialog for a cleaner L2 dump.   ||" + Environment.NewLine;
            StatusMessage += "  \\\\__________________________________________________________________________________//" +Environment.NewLine;

            int burst = 0;
            foreach (BurstData data in lastBursts)
            {
                /* update COUNT and let it decrypt our burst */
                A5Algorithm.CryptDownlink(data.BurstBufferI, data.Count);

                Array.Copy(data.BurstBufferI, burstBufferI[burst], 114);

                burst++;
                burst %= 4;

                /* processed a full frame */
                if (burst == 0)
                {
                    Deinterleave(burstBufferI);

                    /* undo convolutional coding c[] to u[] */
                    if (Deconvolution())
                    {
                        /* CRC check/fix */
                        if (CRCCheck() != eCRCState.Failed)
                        {
                            PackBytes();
                            L2.Handle(param, this, L3, BurstBufferD);
                            
                            /* show L2 messages, if handler wishes */
                            bool showL2 = L2.ShowMessage && !string.IsNullOrEmpty(L2.StatusMessage);

                            /* show L3 messages, if there is any */
                            bool showL3 = !string.IsNullOrEmpty(L3.StatusMessage);

                            /* show L2 if L2 wants to, or if L3 has some message */
                            if (showL3 || showL2)
                            {
                                bool first = true;
                                foreach (string line in L2.StatusMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (first)
                                    {
                                        first = false;
                                        StatusMessage += ("  [L2] " + line + Environment.NewLine);
                                    }
                                    else if (line.Trim() != "")
                                    {
                                        StatusMessage += ("       " + line + Environment.NewLine);
                                    }
                                }
                            }

                            /* L3 handler has a message to show */
                            if (showL3)
                            {
                                bool first = true;
                                foreach (string line in L3.StatusMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (first)
                                    {
                                        first = false;
                                        StatusMessage += ("  [L3] " + line + Environment.NewLine);
                                    }
                                    else if (line.Trim() != "")
                                    {
                                        StatusMessage += ("       " + line + Environment.NewLine);
                                    }
                                }
                            }

                            L2.ShowMessage = false;
                            L2.StatusMessage = "";
                            L3.StatusMessage = "";
                        }
                    }
                }
            }
        }

        internal void DummyBurstReceived(GSMParameters param)
        {
            if (ChannelEncrypted)
            {
                bool crackThis = false;
                string crackReason = "";

                StatusMessage = "[Encrypted frames: " + CryptedFrames + "] ";

                /* check if we should crack this connection */
                if (param.LuaVm != null)
                {
                    object[] ret = LuaHelpers.CallFunction(param.LuaVm, "ShouldCrack", true, this, param);

                    if (ret != null && (ret.Length >= 1) && (ret[0] is bool))
                    {
                        crackThis = (bool)ret[0];
                        if ((ret.Length >= 2) && (ret[1] is string))
                        {
                            crackReason += (string)ret[1];
                        }
                    }
                }

                /* should we? */
                if (!crackThis)
                {
                    if (crackReason == "")
                    {
                        StatusMessage += "Skipping auto-cracking. No clear reason. Cause: " + EstablishmentCause + " Service: " + ServiceType;
                        StatusMessage += Environment.NewLine;
                    }
                    else
                    {
                        StatusMessage += "Skipping auto-cracking.";
                        StatusMessage += Environment.NewLine;
                        StatusMessage += "  Reason: " + crackReason;
                        StatusMessage += Environment.NewLine;
                    }
                }
                else if(A5CipherKey != null)
                {
                    StatusMessage += "Would auto-crack if there were no valid key already in keystore.";
                    StatusMessage += Environment.NewLine;
                    StatusMessage += "  Reason: " + crackReason;
                    StatusMessage += Environment.NewLine;
                }
                else
                {
                    DateTime startTime = DateTime.Now;
                    bool found = false;

                    StatusMessage += "Auto-cracking. ";
                    StatusMessage += Environment.NewLine;
                    StatusMessage += "  Reason: " + crackReason;
                    StatusMessage += Environment.NewLine;

                    LinkedList<bool[][]> guessedData = new LinkedList<bool[][]>();
                    bool[][] guessedKeyBits = new bool[4][];
                    uint[] counts = new uint[4];

                    int frames = LastBursts.Count / 4;

                    for (int burstNum = 0; burstNum < 4; burstNum++)
                    {
                        guessedKeyBits[burstNum] = new bool[114];
                    }

                    /* for every frame received */
                    for (int frame = 0; frame < frames; frame++)
                    {
                        int frameNum = frames - frame;

                        /* add default guessed data bits */
                        guessedData.Clear();
                        guessedData.AddLast(GuessedBitsUI0);

                        /* does the LUA script provide some better ideas? */
                        if (param.LuaVm != null)
                        {
                            object[] ret = LuaHelpers.CallFunction(param.LuaVm, "GetCrackBits", true, frameNum, frames, this, param);

                            if (ret != null)
                            {
                                /* yes, it has a better idea */
                                guessedData.Clear();

                                /* get all possibilities it thinks there are */
                                for (int argNum = 0; argNum < ret.Length; argNum++)
                                {
                                    if(ret[argNum] is string)
                                    {
                                        string arg = ((string)ret[argNum]).Trim();

                                        /* collapse double-spaces */
                                        while (arg.Contains("  "))
                                        {
                                            arg = arg.Replace("  ", " ");
                                        }

                                        string[] burstDataBits = arg.Split(' ');

                                        /* must contain 4 bursts separated with one space */
                                        if (arg.Length == (114 * 4 + 3) && burstDataBits.Length == 4)
                                        {
                                            bool[][] guessed = new bool[4][];

                                            guessed[0] = ByteUtil.BitsFromString(burstDataBits[0]);
                                            guessed[1] = ByteUtil.BitsFromString(burstDataBits[1]);
                                            guessed[2] = ByteUtil.BitsFromString(burstDataBits[2]);
                                            guessed[3] = ByteUtil.BitsFromString(burstDataBits[3]);

                                            guessedData.AddLast(guessed);
                                        }
                                        else
                                        {
                                            Log.AddMessage("GetCrackBits returned invalid bits as retvalue #" + argNum + ": " + (string)ret[argNum]);
                                        }
                                    }
                                }
                            }
                        }

                        BurstData[] burstData = new BurstData[4];

                        /* get the bursts from the end */
                        for (int burstNum = 0; burstNum < 4; burstNum++)
                        {
                            burstData[3 - burstNum] = LastBursts.Last.Value;
                            LastBursts.RemoveLast();
                        }

                        int burst = 0;
                        foreach(bool[][] guessedDataBits in guessedData)
                        {
                            /* calculate guessed key bits */
                            for (int burstNum = 0; burstNum < 4; burstNum++)
                            {
                                for (int pos = 0; pos < 114; pos++)
                                {
                                    guessedKeyBits[burstNum][pos] = burstData[burstNum].BurstBufferE[pos] ^ guessedDataBits[burstNum][pos];
                                }
                                counts[burstNum] = burstData[burstNum].Count;
                            }

                            /* now try to crack */
                            if (TryToCrack(guessedKeyBits, counts, param, burst * 4, guessedDataBits.Length * 4))
                            {
                                string msg = "";

                                msg += "======= Cracked encryption key: ";
                                for (int pos = 0; pos < A5CipherKey.Length; pos++)
                                {
                                    msg += string.Format("{0:X02}", A5CipherKey[pos]);
                                }
                                msg += " (" + param.CipherCracker.SearchDuration + " sec, burst " + (burst + 1) + ", total " + (DateTime.Now - startTime).TotalSeconds + " sec) =======";

                                StatusMessage += msg + Environment.NewLine;
                                Log.AddMessage(msg);

                                /* key found! */
                                found = true;
                                break;
                            }

                            burst++;
                        }

                        if (found)
                        {
                            break;
                        }
                    }

                    if (!found)
                    {
                        string msg = "Crack attempt took " + (DateTime.Now - startTime).TotalSeconds + " seconds.";
                        StatusMessage += msg + Environment.NewLine;
                        Log.AddMessage(msg);
                    }
                    else
                    {
                        A5Algorithm.Key = A5CipherKey;
                        ReplayBursts(param, LastBursts);
                    }
                }

                LastBursts.Clear();

                /* reset key and algorithm */
                ChannelEncrypted = false;
                A5CipherKey = null;
                A5Algorithm = null;
            }
        }

        internal bool TryToCrack(bool[][] guessedKeyBits, uint[] counts, GSMParameters param, int burstNum, int burstCount)
        {
            /* sanity checks */
            if (guessedKeyBits.Length < 4 || (guessedKeyBits.Length % 4) != 0)
            {
                /* only alert if there are any bursts */
                if (guessedKeyBits.Length != 0)
                {
                    Log.AddMessage("Invalid burst count for cracking");
                }
                return false;
            }

            int burst = 0;
            for (int dstBurst = guessedKeyBits.Length - 1; dstBurst >= 0; dstBurst--)
            {
                int block = dstBurst / 4;
                int offset = dstBurst % 4;
                int nextBurst = (block * 4) + ((offset + 1) % 4);

                if (param.CipherCracker == null || !param.CipherCracker.Available)
                {
                    //StatusMessage += "crack " + DumpBits(guessedKeyBits[dstBurst], false) + " " + counts[dstBurst] + " " + DumpBits(guessedKeyBits[nextBurst], false) + " " + counts[nextBurst] + Environment.NewLine;
                }
                else
                {
                    Log.AddMessage("Cracking burst: " + dstBurst + " (Block: " + block + " Offset: " + offset + ") Next: " + nextBurst);

                    /* make sure the UI does get all information */
                    param.CipherCracker.SetJobInfo(burstNum + burst, burstCount);
                    
                    byte[] key = param.CipherCracker.Crack(guessedKeyBits[dstBurst], counts[dstBurst], guessedKeyBits[nextBurst], counts[nextBurst]);
                    if (key != null)
                    {
                        param.AddA5Key(key);
                        A5CipherKey = key;

                        /* also set encryption info in associated SACCH */
                        if (AssociatedSACCH != null)
                        {
                            AssociatedSACCH.A5CipherKey = A5CipherKey;
                            AssociatedSACCH.A5Algorithm = A5Algorithm;
                        }
                        return true;
                    }
                }
                burst++;
            }

            return false;
        }

        /* try to find the correct key for this connection from keystore */
        internal bool DetectKey(GSMParameters param)
        {
            bool[][] burstBufferI = new bool[BurstBlock.Length][];
            for (int pos = 0; pos < BurstBlock.Length; pos++)
            {
                burstBufferI[pos] = new bool[114];
            }

            /* create algorithm. this will stay in any case as this is the flag that we searched already. */
            A5Algorithm = new CryptA5();

            lock (param.A5KeyStore)
            {
                foreach (byte[] key in param.A5KeyStore)
                {
                    /* try to decrypt all 4 bursts */
                    for (int dstBurst = 0; dstBurst < BurstBlock.Length; dstBurst++)
                    {
                        /* get the original bits */
                        Array.Copy(BurstBlock[dstBurst].BurstBufferI, burstBufferI[dstBurst], 114);

                        /* update COUNT and let it decrypt our burst */
                        A5Algorithm.Key = key;
                        A5Algorithm.CryptDownlink(burstBufferI[dstBurst], BurstBlock[dstBurst].Count);
                    }

                    /* check if this was the correct key by trying to deconvolve */
                    Deinterleave(burstBufferI);
                    if (Deconvolution())
                    {
                        A5CipherKey = key;

                        /* also set encryption info in associated SACCH */
                        if (AssociatedSACCH != null)
                        {
                            AssociatedSACCH.A5CipherKey = A5CipherKey;
                            AssociatedSACCH.A5Algorithm = A5Algorithm;
                        }

                        StatusMessage += "======= Found encryption key: ";
                        for (int pos = 0; pos < key.Length; pos++)
                        {
                            StatusMessage += string.Format("{0:X02}", key[pos]);
                        }
                        StatusMessage += " =======" + Environment.NewLine;

                        return true;
                    }
                }
            }

            return false;
        }

        internal bool HandleEncryption(GSMParameters param)
        {
            return HandleEncryption(param, true);
        }

        internal bool HandleEncryption(GSMParameters param, bool detectKey)
        {
            CopyEToI();

            /* this channel was flagged as encrypted */
            if (ChannelEncrypted)
            {
                CryptedFrames++;

                if (A5Algorithm == null && detectKey)
                {
                    DetectKey(param);
                }

                /* do we have an A5 decryptor? */
                if (A5CipherKey != null)
                {
                    if (DumpEncryptedMessageBits)
                    {
                        EncryptionBitString = "Raw L1 data" + Environment.NewLine;
                    }

                    /* now decrypt all 4 bursts */
                    for (int dstBurst = 0; dstBurst < BurstBlock.Length; dstBurst++)
                    {
                        /* update COUNT and let it decrypt our burst */
                        A5Algorithm.CryptDownlink(BurstBlock[dstBurst].BurstBufferI, BurstBlock[dstBurst].Count);

                        if (DumpEncryptedMessageBits)
                        {
                            //EncryptionBitString += "   COUNT: " + BurstBlock[dstBurst].Count;
                            EncryptionBitString += " i[]: " + DumpBits(BurstBlock[dstBurst].BurstBufferI, false);
                            EncryptionBitString += " k[]: " + DumpBits(A5Algorithm.DownlinkKey, false);
                            EncryptionBitString += " e[]: " + DumpBits(BurstBlock[dstBurst].BurstBufferE, false);
                            EncryptionBitString += Environment.NewLine;
                        }
                    }

                    return true;
                }
                else if (detectKey)
                {
                    bool[][] guessedKey = new bool[BurstBlock.Length][];

                    /* this is for using the last few bursts for Kc cracking */
                    if (AutoCrackBursts)
                    {
                        for (int dstBurst = 0; dstBurst < BurstBlock.Length; dstBurst++)
                        {
                            LastBursts.AddLast(BurstBlock[dstBurst].Clone());
                        }

                        while (LastBursts.Count > BurstsToLog)
                        {
                            LastBursts.RemoveFirst();
                        }
                    }
                    else
                    {
                        StatusMessage = "";
                        StatusMessage += Environment.NewLine;

                        /* no decryption available - just dump */
                        for (int dstBurst = 0; dstBurst < BurstBlock.Length; dstBurst++)
                        {
                            guessedKey[dstBurst] = new bool[114];

                            for (int pos = 0; pos < 114; pos++)
                            {
                                guessedKey[dstBurst][pos] = BurstBlock[dstBurst].BurstBufferE[pos] ^ GuessedBitsUI0[dstBurst][pos];
                            }
                        }

                        for (int dstBurst = 0; dstBurst < BurstBlock.Length; dstBurst++)
                        {
                            int nextBurst = (dstBurst + 1) % BurstBlock.Length;
                            StatusMessage += "crack " + DumpBits(guessedKey[dstBurst], false) + " " + BurstBlock[dstBurst].Count + " " + DumpBits(guessedKey[nextBurst], false) + " " + BurstBlock[nextBurst].Count + Environment.NewLine;
                        }
                    }
                }
                return false;
            }
            else
            {
                CryptedFrames = 0;
            }

            return true;
        }
    }
}