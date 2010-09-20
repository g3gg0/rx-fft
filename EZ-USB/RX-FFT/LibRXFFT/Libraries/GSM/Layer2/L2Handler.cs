using System;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;
using LibRXFFT.Libraries.GSM.Layer3;
using System.Text;

namespace LibRXFFT.Libraries.GSM.Layer2
{
    public class L2Handler
    {
        public static bool ShowAllMessages = false;
        public static bool DumpRawData = false;
        public static bool DumpFaulty = false;

        byte[] packetBuffer = new byte[8192];
        private long packetBufferOffset = 0;
        public string StatusMessage = null;
        public bool ShowMessage = false;
        private int NumPackets;
        private int LastNS = -1;

        private CBCHandler CBCHandler;
        private L2HeaderWrapper L2Data = new L2HeaderWrapper();

        public L2Handler()
        {
            CBCHandler = new CBCHandler();

            for (int pos = 0; pos < packetBuffer.Length; pos++)
                packetBuffer[pos] = 0x2B;
        }

        private bool PacketIsEmpty(byte[] data)
        {
            return PacketIsEmpty(data, 0);
        }

        private bool PacketIsEmpty(byte[] data, int start)
        {
            for (int pos = start; pos < data.Length; pos++)
            {
                if (data[pos] != 0x2B)
                    return false;
            }

            return true;
        }

        public void Handle(NormalBurst source, L3Handler l3, byte[] l2Data)
        {
            Handle(source, l3, l2Data, 0);
        }

        private StringBuilder Builder = new StringBuilder();

        public void Handle(NormalBurst source, L3Handler l3, byte[] l2Data, int startOffset)
        {
            Builder.Length = 0;

            /* BCCH and CCCH packets have pseudo L2 headers (GSM 04.07 11.3.1) */
            if (source.GetType() == typeof(BCCHBurst) || source.GetType() == typeof(CCCHBurst))
            {
                Builder.Append( "Pseudo L2 Header").Append(Environment.NewLine);

                if (source.ChannelEncrypted)
                {
                    Builder.Append("        ======= encrypted =======").Append(Environment.NewLine);
                }

                /* pass to L3 handler if not empty and skip pseudo length */
                if (!PacketIsEmpty(l2Data, 1))
                    l3.Handle(l2Data, 1);
            }
            else
            {

                /* always show empty/multiframe messages if requested */
                ShowMessage = ShowAllMessages;

                L2Data.Payload = l2Data;
                L2Data.StartOffset = startOffset;

                if (L3Handler.ExceptFieldsEnabled || ShowMessage)
                {
                    Builder.Append("SAPI: ").Append(L2Data.SAPI).Append("  C/R: ").Append((L2Data.CR ? "1" : "0")).Append("  EA: ").Append((L2Data.EA ? "1" : "0")).Append("  ");
                    Builder.Append("M: ").Append((L2Data.M ? "1" : "0")).Append("  ");
                    Builder.Append("EL: ").Append((L2Data.EL ? "1" : "0")).Append("  ");
                    Builder.Append("L: ").Append(L2Data.Length).Append("  ");
                    /*
                    StatusMessage = "SAPI: " + L2Data.SAPI + "  C/R: " + (L2Data.CR ? "1" : "0") + "  EA: " + (L2Data.EA ? "1" : "0") + "  ";
                    StatusMessage += "M: " + (L2Data.M ? "1" : "0") + "  ";
                    StatusMessage += "EL: " + (L2Data.EL ? "1" : "0") + "  ";
                    StatusMessage += "L: " + L2Data.Length + "  ";
                    */
                    switch (L2Data.FrameFormat)
                    {
                        case eFrameFormat.S_Format:
                            Builder.Append("S Format, N(R)=").Append(L2Data.NR).Append(" S=").Append(L2Data.S).Append(" ").Append((eSupervisory)L2Data.S);
                            break;

                        case eFrameFormat.U_Format:
                            Builder.Append("U Format, U=").Append(L2Data.U).Append(" ").Append((eUnnumbered)L2Data.U);
                            break;

                        case eFrameFormat.I_Format:
                            Builder.Append("I Format, N(R)=").Append(L2Data.NR).Append(" N(S)=").Append(L2Data.NS).Append(" ");
                            break;
                    }
                }

                /* check if there is enough space in the buffer */
                if (L2Data.Length + packetBufferOffset <= packetBuffer.Length && L2Data.Length + L2Data.DataStart <= L2Data.Payload.Length)
                {
                    /* dont buffer when its the same frame number (retransmission) */
                    if (!L2Data.M || LastNS < L2Data.NS)
                    {
                        Array.Copy(L2Data.Payload, L2Data.DataStart, packetBuffer, packetBufferOffset, L2Data.Length);
                        packetBufferOffset += L2Data.Length;
                        LastNS = L2Data.NS;
                    }
                    else
                    {
                        if (L3Handler.ExceptFieldsEnabled || ShowMessage)
                            Builder.Append("!! Retransmission !! ");
                    }
                }
                else
                {
                    if (DumpFaulty)
                    {
                        Builder.Append("Faulty length?! Length = ").Append((packetBufferOffset + L2Data.Length)).Append(Environment.NewLine);
                        Builder.Append("          Raw Data").Append(Environment.NewLine);
                        Builder.Append("             ").Append(DumpBytes(l2Data)).Append(Environment.NewLine);
                        ShowMessage = true;
                    }
                }

                /* that counter is just for convinience */
                NumPackets++;

                /* when reached the last packet, pass it to L3 handler */
                if (!L2Data.M)
                {
                    if (L3Handler.ExceptFieldsEnabled || ShowMessage)
                    {
                        if (NumPackets > 1)
                            Builder.Append("(packet ").Append(NumPackets).Append(", total ").Append(packetBufferOffset).Append(" bytes)").Append(Environment.NewLine);
                        else
                            Builder.Append(Environment.NewLine);
                    }

                    if (source.ChannelEncrypted)
                    {
                        Builder.Append("        ======= encrypted =======").Append(Environment.NewLine);
                    }

                    /* but only pass it through when there is any data */
                    if (packetBufferOffset > 0)
                    {
                        /* allocate a new array with the correct size */
                        byte[] buf = new byte[packetBufferOffset];
                        Array.Copy(packetBuffer, buf, buf.Length);

                        if (!PacketIsEmpty(buf))
                        {
                            l3.Handle(buf);
                        }
                    }

                    /* reset the buffer etc */
                    packetBufferOffset = 0;
                    for (int pos = 0; pos < packetBuffer.Length; pos++)
                        packetBuffer[pos] = 0xCC;
                    NumPackets = 0;
                    LastNS = -1;
                }
                else if (L3Handler.ExceptFieldsEnabled || ShowMessage)
                {
                    Builder.Append("(packet ").Append(NumPackets).Append(")").Append(Environment.NewLine);
                }
            }

            if (DumpRawData && (L3Handler.ExceptFieldsEnabled || ShowMessage))
            {
                Builder.Append("        Raw Data").Append(Environment.NewLine);
                Builder.Append("             ").Append(DumpBytes(l2Data)).Append(Environment.NewLine);
            }

            StatusMessage = Builder.ToString();
        }


        private static string DumpBytes(byte[] data)
        {
            StringBuilder msg = new StringBuilder();

            foreach (byte value in data)
                msg.AppendFormat("{0:X02} ", value);

            return msg.ToString();
        }

        internal enum eUnnumbered
        {
            SABM = 7,
            DM = 3,
            UI = 0,
            DISC = 8,
            UA = 12
        }

        internal enum eSupervisory
        {
            RR = 0,
            RNR = 1,
            REJ = 4
        }


        internal enum eFrameFormat
        {
            I_Format,
            S_Format,
            U_Format
        }

        internal class L2HeaderWrapper
        {
            internal int StartOffset = 0;
            internal byte[] Payload = new byte[0];

            internal int DataStart
            {
                get
                {
                    return StartOffset + 3;
                }
            }

            internal int LPD
            {
                get
                {
                    return (Payload[StartOffset] >> 5) & 3;
                }
            }

            internal int SAPI
            {
                get
                {
                    return (Payload[StartOffset] >> 2) & 7;
                }
            }

            internal bool CR
            {
                get
                {
                    return ((Payload[StartOffset] >> 1) & 1) != 0;
                }
            }

            internal bool EA
            {
                get
                {
                    return (Payload[StartOffset] & 1) != 0;
                }
            }

            internal eFrameFormat FrameFormat
            {
                get
                {
                    if ((Payload[StartOffset + 1] & 1) == 0)
                        return eFrameFormat.I_Format;

                    if ((Payload[StartOffset + 1] & 2) == 0)
                        return eFrameFormat.S_Format;

                    return eFrameFormat.U_Format;
                }
            }

            internal int Length
            {
                get
                {
                    return Payload[StartOffset + 2] >> 2;
                }
            }

            internal bool M
            {
                get
                {
                    return ((Payload[StartOffset + 2] >> 1) & 1) != 0;
                }
            }

            internal bool EL
            {
                get
                {
                    return (Payload[StartOffset + 2] & 1) != 0;
                }
            }

            internal int NR
            {
                get
                {
                    return Payload[StartOffset + 1] >> 5;
                }
            }

            internal int NS
            {
                get
                {
                    return (Payload[StartOffset + 1] >> 1) & 7;
                }
            }

            internal int S
            {
                get
                {
                    return (Payload[StartOffset + 1] >> 2) & 7;
                }
            }

            internal int U
            {
                get
                {
                    return ((Payload[StartOffset + 1] >> 3) & 0x1C) | ((Payload[StartOffset + 1] >> 2) & 7);
                }
            }

            internal bool PF
            {
                get
                {
                    return ((Payload[StartOffset + 2] >> 4) & 1) != 0;
                }
            }

        }
    }
}
