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

                /* remove the pseudo length (1 byte) 
                byte[] buf = new byte[l2data.Length - startOffset - 1];
                Array.Copy(l2data, 1, buf, 0, buf.Length);
                */

                /* pass to L3 handler if not empty */
                if (!PacketIsEmpty(l2Data, 1))
                    l3.Handle(l2Data, 1);
            }
            else
            {

                /* always show empty/multiframe messages if requested */
                ShowMessage = ShowAllMessages;

                int dataStart = startOffset + 3;
                int sapi = (l2Data[startOffset] >> 2) & 7;
                int cr = (l2Data[startOffset] >> 1) & 1;
                int ea = (l2Data[startOffset] >> 0) & 1;
                int control = l2Data[startOffset + 1];
                int lengthIndicator = l2Data[startOffset + 2];

                /* get the flags from the length field */
                bool flagExtension = ((lengthIndicator >> 0) & 1) == 1;
                bool flagMore = ((lengthIndicator >> 1) & 1) == 1;
                int length = lengthIndicator >> 2;

                StatusMessage = "SAPI: " + sapi + "  C/R: " + cr + "  EA: " + ea + "  ";
                StatusMessage += "M: " + (flagMore ? "1" : "0") + "  ";
                StatusMessage += "EL: " + (flagExtension ? "1" : "0") + "  ";
                StatusMessage += "L: " + length + "  ";

                int NR = -1;
                int NS = -1;
                int U = -1;
                int S = -1;

                switch (control & 3)
                {
                    case 1:
                        NR = (control >> 5);
                        S = ((control >> 2) & 3);
                        StatusMessage += "S Format, N(R)=" + NR + " S=" + S + " ";
                        break;

                    case 3:
                        U = (((control >> 3) & 0x1C) | ((control >> 2) & 3));
                        StatusMessage += "U Format, U=" + U + " ";
                        break;

                    default:
                        NR = (control >> 5);
                        NS = ((control >> 1) & 0x7);
                        StatusMessage += "I Format, N(R)=" + NR + " N(S)=" + NS + " ";
                        break;
                }

                /* check if there is enough space in the buffer */
                if (length + packetBufferOffset <= packetBuffer.Length && length + dataStart <= l2Data.Length)
                {
                    /* dont buffer when its the same frame number (retransmission) */
                    if (!flagMore || LastNS < NS)
                    {
                        Array.Copy(l2Data, dataStart, packetBuffer, packetBufferOffset, length);
                        packetBufferOffset += length;
                        LastNS = NS;
                    }
                    else
                    {
                        StatusMessage += "!! Retransmission !! ";
                    }
                }
                else
                {
                    StatusMessage += "Faulty length?! Length = " + (packetBufferOffset + length) + Environment.NewLine;
                    StatusMessage += "          Raw Data" + Environment.NewLine;
                    StatusMessage += "             " + DumpBytes(l2Data) + Environment.NewLine;
                    ShowMessage = true;
                }

                /* that counter is just for convinience */
                NumPackets++;

                /* when reached the last packet, pass it to L3 handler */
                if (!flagMore)
                {
                    if (NumPackets > 1)
                        StatusMessage += "(packet " + NumPackets + ", total " + packetBufferOffset + " bytes)" + Environment.NewLine;
                    else
                        StatusMessage += Environment.NewLine;

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
                else
                    StatusMessage += "(packet " + NumPackets + ")" + Environment.NewLine;
            }

            if (DumpRawData)
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
    }
}
