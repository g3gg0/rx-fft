using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.GSM.Layer1.PacketDump
{
    /* pseudo header for WTAP_ENCAP_GSM_UM */
    [StructLayout(LayoutKind.Sequential)]
    public struct PcapGsmHeader
    {
        public bool Uplink;
        public eGsmChannel Channel;
        /* The following are only populated for downlink */
        public byte Bsic;
        public ushort Arfcn;
        public uint TdmaFrame;
        public byte Error;
        public ushort Timeshift;
    };

    public enum eGsmChannel
    {
        GSM_UM_CHANNEL_UNKNOWN,
        GSM_UM_CHANNEL_BCCH,
        GSM_UM_CHANNEL_SDCCH,
        GSM_UM_CHANNEL_SACCH,
        GSM_UM_CHANNEL_FACCH,
        GSM_UM_CHANNEL_CCCH,
        GSM_UM_CHANNEL_RACH,
        GSM_UM_CHANNEL_AGCH,
        GSM_UM_CHANNEL_PCH
    };

    public enum DataLinkType : uint
    {
        WTAP_ENCAP_NULL = 0,/* null encapsulation */
        WTAP_ENCAP_ETHERNET = 1,
        WTAP_ENCAP_TOKEN_RING = 6,/* IEEE 802 Networks - assume token ring */
        WTAP_ENCAP_ARCNET = 7,
        WTAP_ENCAP_SLIP = 8,
        WTAP_ENCAP_PPP = 9,
        WTAP_ENCAP_FDDI = 10,
        WTAP_ENCAP_REDBACK = 32,
        WTAP_ENCAP_PPP_2 = 50,
        WTAP_ENCAP_SYMANTEC = 99,/* Apparently used by the Axent Raptor firewall (now Symantec Enterprise Firewall). */
        WTAP_ENCAP_ATM_RFC1483 = 100,/*libpcap 0.5 and later*/
        WTAP_ENCAP_RAW_IP = 101,
        WTAP_ENCAP_SLIP_BSDOS = 102,
        WTAP_ENCAP_PPP_BSDOS = 103,
        WTAP_ENCAP_CHDLC = 104,/* Cisco HDLC */
        WTAP_ENCAP_IEEE_802_11 = 105, /* IEEE 802.11 */
        WTAP_ENCAP_LINUX_ATM_CLIP = 106,
        WTAP_ENCAP_FRELAY = 107,/* Frame Relay */
        WTAP_ENCAP_NULL_2 = 108,	/* OpenBSD loopback */
        WTAP_ENCAP_ENC = 109,	/* OpenBSD IPSEC enc */
        WTAP_ENCAP_LANE_802_3 = 110,/* ATM LANE 802.3 */
        WTAP_ENCAP_HIPPI = 111,	/* NetBSD HIPPI */
        WTAP_ENCAP_CHDLC_2 = 112,	/* NetBSD HDLC framing */

        WTAP_ENCAP_SLL = 113,/* Linux cooked capture */

        WTAP_ENCAP_LOCALTALK = 114,	/* Localtalk */
        WTAP_ENCAP_GSM_UM = 116,
        WTAP_ENCAP_PFLOG = 117,
        WTAP_ENCAP_CISCO_IOS = 118,
        WTAP_ENCAP_PRISM_HEADER = 119, /* Prism monitor mode hdr */
        WTAP_ENCAP_HHDLC = 121,	/* HiPath HDLC */
        WTAP_ENCAP_IP_OVER_FC = 122,   /* RFC 2625 IP-over-FC */
        WTAP_ENCAP_ATM_PDUS = 123,  /* SunATM */
        WTAP_ENCAP_IEEE_802_11_WLAN_RADIOTAP = 127,  /* 802.11 plus radiotap WLAN header */
        WTAP_ENCAP_TZSP = 128,	/* Tazmen Sniffer Protocol */
        WTAP_ENCAP_ARCNET_LINUX = 129,
        WTAP_ENCAP_JUNIPER_MLPPP = 130, /* Juniper MLPPP on ML-, LS-, AS- PICs */
        WTAP_ENCAP_JUNIPER_MLFR = 131, /* Juniper MLFR (FRF.15) on ML-, LS-, AS- PICs */
        WTAP_ENCAP_JUNIPER_GGSN = 133,
        /*
         * Values 132-134, 136 not listed here are reserved for use
         * in Juniper hardware.
         */
        WTAP_ENCAP_JUNIPER_ATM2 = 135, /* various encapsulations captured on the ATM2 PIC */
        WTAP_ENCAP_JUNIPER_ATM1 = 137, /* various encapsulations captured on the ATM1 PIC */

        WTAP_ENCAP_APPLE_IP_OVER_IEEE1394 = 138,
        /* Apple IP-over-IEEE 1394 */

        WTAP_ENCAP_MTP2_WITH_PHDR = 139,
        WTAP_ENCAP_MTP2 = 140,
        WTAP_ENCAP_MTP3 = 141,
        WTAP_ENCAP_DOCSIS = 143,
        WTAP_ENCAP_IRDA = 144,	/* IrDA capture */

        /* Reserved for private use. */
        WTAP_ENCAP_USER0 = 147,
        WTAP_ENCAP_USER1 = 148,
        WTAP_ENCAP_USER2 = 149,
        WTAP_ENCAP_USER3 = 150,
        WTAP_ENCAP_USER4 = 151,
        WTAP_ENCAP_USER5 = 152,
        WTAP_ENCAP_USER6 = 153,
        WTAP_ENCAP_USER7 = 154,
        WTAP_ENCAP_USER8 = 155,
        WTAP_ENCAP_USER9 = 156,
        WTAP_ENCAP_USER10 = 157,
        WTAP_ENCAP_USER11 = 158,
        WTAP_ENCAP_USER12 = 159,
        WTAP_ENCAP_USER13 = 160,
        WTAP_ENCAP_USER14 = 161,
        WTAP_ENCAP_USER15 = 162,

        WTAP_ENCAP_IEEE_802_11_WLAN_AVS = 163,  /* 802.11 plus AVS WLAN header */

        /*
         * 164 is reserved for Juniper-private chassis-internal
         * meta-information such as QoS profiles, etc..
         */

        WTAP_ENCAP_BACNET_MS_TP = 165,

        /*
         * 166 is reserved for a PPP variant in which the first byte
         * of the 0xff03 header, the 0xff, is replaced by a direction
         * byte.  I don't know whether any captures look like that,
         * but it is used for some Linux IP filtering (ipfilter?).
         */

        /* Ethernet PPPoE frames captured on a service PIC */
        WTAP_ENCAP_JUNIPER_PPPOE = 167,

        /*
     * 168 is reserved for more Juniper private-chassis-
     * internal meta-information.
     */

        WTAP_ENCAP_GPRS_LLC = 169,

        /*
         * 170 and 171 are reserved for ITU-T G.7041/Y.1303 Generic
         * Framing Procedure.
         */

        /* Registered by Gcom, Inc. */
        WTAP_GCOM_TIE1 = 172,
        WTAP_GCOM_SERIAL = 173,

        WTAP_ENCAP_LINUX_LAPD = 177,


        WTAP_ENCAP_JUNIPER_ETHER = 178, /* Ethernet frames prepended with meta-information */
        WTAP_ENCAP_JUNIPER_PPP = 179,/* PPP frames prepended with meta-information */
        WTAP_ENCAP_JUNIPER_FRELAY = 180,/* Frame-Relay frames prepended with meta-information */
        WTAP_ENCAP_JUNIPER_CHDLC = 181,/* C-HDLC frames prepended with meta-information */
        WTAP_ENCAP_JUNIPER_VP = 183,/* VOIP Frames prepended with meta-information */


        WTAP_ENCAP_USB = 186,	            /* raw USB packets */
        WTAP_ENCAP_BLUETOOTH_H4 = 187,      /* Bluetooth HCI UART transport (part H:4) frames, like hcidump */
        WTAP_ENCAP_IEEE802_16_MAC_CPS = 188,/* IEEE 802.16 MAC Common Part Sublayer */
        WTAP_ENCAP_USB_LINUX = 189,         /* USB packets with Linux-specified header */
        WTAP_ENCAP_ATM_RFC1483_2 = 11,
        WTAP_ENCAP_RAW_IP_2 = 12,
        WTAP_ENCAP_ATM_RFC1483_3 = 13,
        WTAP_ENCAP_RAW_IP_3 = 14,
        WTAP_ENCAP_LINUX_ATM_CLIP_2 = 16,
        WTAP_ENCAP_OLD_PFLOG = 17,
        WTAP_ENCAP_LINUX_ATM_CLIP_3 = 18,
        WTAP_ENCAP_LINUX_ATM_CLIP_4 = 19
    }

    public class PcapPacket
    {
        public DateTime Timestamp = DateTime.Now;
        public byte[] Data = new byte[1];
        public PcapGsmHeader Header = new PcapGsmHeader();

        public PcapPacket(DateTime timestamp, byte[] data)
        {
            Timestamp = timestamp;
            Data = data;
        }

        public PcapPacket()
        {
        }
    }
}
