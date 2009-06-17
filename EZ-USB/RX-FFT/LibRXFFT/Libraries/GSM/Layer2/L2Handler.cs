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
        byte[] packetBuffer = new byte[8192];
        private long packetBufferOffset = 0;
        public string StatusMessage = null;
        public bool ShowMessage = false;
        private byte MessageType;
        private int NumPackets;

        private CBCHandler CBCHandler;

        public L2Handler()
        {
            CBCHandler = new CBCHandler();

            for (int pos = 0; pos < packetBuffer.Length; pos++)
                packetBuffer[pos] = 0x2B;
        }

        private bool PacketIsEmpty(byte[] data)
        {
            for (int pos = 0; pos < data.Length; pos++)
            {
                if (data[pos] != 0x2B)
                    return false;
            }

            return true;
        }

        public void Handle(Burst source, L3Handler l3, byte[] data)
        {
            /* BCCH and CCCH packets have pseudo L2 headers */
            if (source.GetType() == typeof(BCCHBurst) || source.GetType() == typeof(CCCHBurst))
            {
                StatusMessage = "Pseudo L2 Header" + Environment.NewLine;

                /* remove the pseudo length (1 byte) */
                byte[] buf = new byte[data.Length - 1];
                Array.Copy(data, 1, buf, 0, buf.Length);

                if (!PacketIsEmpty(buf))
                    l3.Handle(buf);
                return;
            }

            if (source.GetType() == typeof(SACCHBurst) && source.Name.Contains("TCH"))
            {
                StatusMessage = "Dumping - Length = " + (packetBufferOffset + data.Length) + Environment.NewLine;
                StatusMessage += "                 " + DumpBytes(data);
                ShowMessage = true;
                return;
            }

            byte lengthIndicator;
            int dataStart;

            MessageType = data[0];
            StatusMessage = "Header Type " + MessageType + " ";

            if (MessageType == 0x05)
            {
                lengthIndicator = data[4];
                dataStart = 5;
            }
            else
            {
                lengthIndicator = data[2];
                dataStart = 3;
            }

            /* get the flags from the length field */
            bool flagExtension = ((lengthIndicator >> 0) & 1) == 1;
            bool flagMore = ((lengthIndicator >> 1) & 1) == 1;
            int length = lengthIndicator >> 2;

            if (length + packetBufferOffset <= packetBuffer.Length && length + dataStart <= data.Length)
            {
                Array.Copy(data, dataStart, packetBuffer, packetBufferOffset, length);
                packetBufferOffset += length;
            }
            else
            {
                StatusMessage += "Faulty length?! Length = " + (packetBufferOffset + length) + Environment.NewLine;
                StatusMessage += DumpBytes(data);
                ShowMessage = true;
            }

            /* that counter is just for convinience */
            NumPackets++;

            /* when reached the last packet, pass it to L3 handler */
            if (!flagMore)
            {
                if (NumPackets > 1)
                    StatusMessage += "(final, packet " + NumPackets + ")" + Environment.NewLine;
                else
                    StatusMessage += Environment.NewLine;

                /* but only pass it through when there is any data */
                if (packetBufferOffset > 0)
                {
                    byte[] buf = new byte[packetBufferOffset];
                    Array.Copy(packetBuffer, buf, buf.Length);

                    if (!PacketIsEmpty(buf))
                        l3.Handle(buf);
                }

                /* reset the buffer etc */
                packetBufferOffset = 0;
                for (int pos = 0; pos < packetBuffer.Length; pos++)
                    packetBuffer[pos] = 0x2B;
                NumPackets = 0;
                return;
            }

            StatusMessage += "(packet " + NumPackets + ", " + length + " bytes)" + Environment.NewLine;
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
