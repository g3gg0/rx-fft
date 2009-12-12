using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GSM.Bursts;
using LibRXFFT.Libraries.GSM.Layer3;

namespace LibRXFFT.Libraries.GSM.Layer2
{
    public class L2Handler
    {
        public static bool ShowAllMessages = false;
        public static bool DumpRawData = false;

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

        public void Handle(Burst source, L3Handler l3, byte[] l2Data)
        {
            Handle(source, l3, l2Data, 0);
        }

        public void Handle(Burst source, L3Handler l3, byte[] l2Data, int startOffset)
        {

            /* BCCH and CCCH packets have pseudo L2 headers (GSM 04.07 11.3.1) */
            if (source.GetType() == typeof(BCCHBurst) || source.GetType() == typeof(CCCHBurst))
            {
                StatusMessage = "Pseudo L2 Header" + Environment.NewLine;

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
                    StatusMessage = "SAPI: " + L2Data.SAPI + "  C/R: " + (L2Data.CR ? "1" : "0") + "  EA: " + (L2Data.EA ? "1" : "0") + "  ";
                    StatusMessage += "M: " + (L2Data.M ? "1" : "0") + "  ";
                    StatusMessage += "EL: " + (L2Data.EL ? "1" : "0") + "  ";
                    StatusMessage += "L: " + L2Data.Length + "  ";

                    switch (L2Data.FrameFormat)
                    {
                        case eFrameFormat.S_Format:
                            StatusMessage += "S Format, N(R)=" + L2Data.NR + " S=" + L2Data.S + " " + (eSupervisory)L2Data.S;
                            break;

                        case eFrameFormat.U_Format:
                            StatusMessage += "U Format, U=" + L2Data.U + " " + (eUnnumbered)L2Data.U;
                            break;

                        case eFrameFormat.I_Format:
                            StatusMessage += "I Format, N(R)=" + L2Data.NR + " N(S)=" + L2Data.NS + " ";
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
                            StatusMessage += "!! Retransmission !! ";
                    }
                }
                else
                {
                    StatusMessage += "Faulty length?! Length = " + (packetBufferOffset + L2Data.Length) + Environment.NewLine;
                    StatusMessage += "          Raw Data" + Environment.NewLine;
                    StatusMessage += "             " + DumpBytes(l2Data) + Environment.NewLine;
                    ShowMessage = true;
                }

                /* that counter is just for convinience */
                NumPackets++;

                /* when reached the last packet, pass it to L3 handler */
                if (!L2Data.M)
                {
                    if (L3Handler.ExceptFieldsEnabled || ShowMessage)
                    {
                        if (NumPackets > 1)
                            StatusMessage += "(packet " + NumPackets + ", total " + packetBufferOffset + " bytes)" + Environment.NewLine;
                        else
                            StatusMessage += Environment.NewLine;
                    }

                    /* but only pass it through when there is any data */
                    if (packetBufferOffset > 0)
                    {
                        /* allocate a new array with the correct size */
                        byte[] buf = new byte[packetBufferOffset];
                        Array.Copy(packetBuffer, buf, buf.Length);

                        if (!PacketIsEmpty(buf))
                            l3.Handle(buf);
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
                    StatusMessage += "(packet " + NumPackets + ")" + Environment.NewLine;
                }
            }

            if (DumpRawData && (L3Handler.ExceptFieldsEnabled || ShowMessage))
            {
                StatusMessage += "          Raw Data" + Environment.NewLine;
                StatusMessage += "             " + DumpBytes(l2Data) + Environment.NewLine;
            }
        }


        private static string DumpBytes(byte[] data)
        {
            string msg = "";

            foreach (byte value in data)
                msg += String.Format("{0:X02} ", value);

            return msg;
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
