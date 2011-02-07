using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Net;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;

namespace LibRXFFT.Libraries.GSM.Layer1.PacketDump
{
    public class GsmTapWriter : PacketDumpWriter
    {
        private const byte GSMTAP_VERSION = 0x02;
        private const byte GSMTAP_TYPE_UM = 0x01;
        private const byte GSMTAP_TYPE_ABIS = 0x02;
        private const byte GSMTAP_TYPE_UM_BURST = 0x03;   /* raw burst bits */

        private const byte GSMTAP_BURST_UNKNOWN = 0x00;
        private const byte GSMTAP_BURST_FCCH = 0x01;
        private const byte GSMTAP_BURST_PARTIAL_SCH = 0x02;
        private const byte GSMTAP_BURST_SCH = 0x03;
        private const byte GSMTAP_BURST_CTS_SCH = 0x04;
        private const byte GSMTAP_BURST_COMPACT_SCH = 0x05;
        private const byte GSMTAP_BURST_NORMAL = 0x06;
        private const byte GSMTAP_BURST_DUMMY = 0x07;
        private const byte GSMTAP_BURST_ACCESS = 0x08;
        private const byte GSMTAP_BURST_NONE = 0x09;

        private const byte GSMTAP_CHANNEL_UNKNOWN = 0x00;
        private const byte GSMTAP_CHANNEL_BCCH = 0x01;
        private const byte GSMTAP_CHANNEL_CCCH = 0x02;
        private const byte GSMTAP_CHANNEL_RACH = 0x03;
        private const byte GSMTAP_CHANNEL_AGCH = 0x04;
        private const byte GSMTAP_CHANNEL_PCH = 0x05;
        private const byte GSMTAP_CHANNEL_SDCCH = 0x06;
        private const byte GSMTAP_CHANNEL_SDCCH4 = 0x07;
        private const byte GSMTAP_CHANNEL_SDCCH8 = 0x08;
        private const byte GSMTAP_CHANNEL_TCH_F = 0x09;
        private const byte GSMTAP_CHANNEL_TCH_H = 0x0a;
        private const byte GSMTAP_CHANNEL_ACCH = 0x80;

        private const uint GSMTAP_ARFCN_F_PCS = 0x8000;
        private const uint GSMTAP_ARFCN_F_UPLINK = 0x4000;
        private const uint GSMTAP_ARFCN_MASK = 0x3fff;

        private const uint GSMTAP_UDP_PORT = 4729;

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        protected struct GsmTapHeader 
        {
            public byte version;        /* version, set to 0x01 currently */
            public byte hdr_len;        /* length in number of 32bit words */
            public byte type;           /* see GSMTAP_TYPE_* */
            public byte timeslot;       /* timeslot (0..7 on Um) */

            public UInt16 arfcn;         /* ARFCN (frequency) */
            public byte signal_dbm;      /* signal level in dBm */
            public byte snr_db;          /* signal/noise ratio in dB */

            public UInt32 frame_number;  /* GSM Frame Number (FN) */

            public byte sub_type;       /* Type of burst/channel, see above */
            public byte antenna_nr;     /* Antenna Number */
            public byte sub_slot;       /* sub-slot within timeslot */
            public byte res;            /* reserved for future use (RFU) */
        };

        protected GsmTapHeader Header;
        protected byte[] GsmTapBuffer = null;
        protected UdpClient Udp = new UdpClient();
        protected IPEndPoint Endpoint;
        protected GSMParameters Parameters = null;

        public GsmTapWriter(GSMParameters param)
        {
            Parameters = param;
            Endpoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), (int)GSMTAP_UDP_PORT);
            Header.version = GSMTAP_VERSION;
            Header.hdr_len = 4;
            Header.type = GSMTAP_TYPE_UM;
        }

        #region PacketDumpWriter Member

        public void WriteRawBurst(bool[] BurstBitsUndiffed)
        {
        }

        public void WriteL2Data(byte[] l2Data)
        {        
            Header.arfcn = (ushort)IPAddress.HostToNetworkOrder((short)Parameters.ARFCN);
            Header.frame_number = (uint)IPAddress.HostToNetworkOrder((int)Parameters.FN);
            Header.timeslot = (byte)Parameters.TN;
            
            if (Parameters.CurrentBurstHandler is BCCHBurst)
            {
                Header.sub_type = GSMTAP_CHANNEL_BCCH;
            }
            else if (Parameters.CurrentBurstHandler is CBCHBurst)
            {
                Header.sub_type = GSMTAP_CHANNEL_CCCH;
            }
            else if (Parameters.CurrentBurstHandler is CCCHBurst)
            {
                Header.sub_type = GSMTAP_CHANNEL_CCCH;
            }
            else if (Parameters.CurrentBurstHandler is SDCCHBurst)
            {
                Header.sub_type = GSMTAP_CHANNEL_SDCCH;
            }
            else if (Parameters.CurrentBurstHandler is SACCHBurst)
            {
                Header.sub_type = GSMTAP_CHANNEL_ACCH;
            }
            else if (Parameters.CurrentBurstHandler is TCHBurst)
            {
                Header.sub_type = GSMTAP_CHANNEL_TCH_F;
            }
            else
            {
                Header.sub_type = GSMTAP_CHANNEL_UNKNOWN;
            }

            if (GsmTapBuffer == null || Marshal.SizeOf(Header) + l2Data.Length > GsmTapBuffer.Length)
            {
                GsmTapBuffer = new byte[Marshal.SizeOf(Header) + l2Data.Length];
            }

            GCHandle handle = GCHandle.Alloc(GsmTapBuffer, GCHandleType.Pinned);
            IntPtr buffer = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(Header, buffer, false);
            handle.Free();

            Array.Copy(l2Data, 0, GsmTapBuffer, 16, l2Data.Length);

            Udp.Send(GsmTapBuffer, GsmTapBuffer.Length, Endpoint);
        }

        public void Close()
        {
        }

        #endregion
    }
}
