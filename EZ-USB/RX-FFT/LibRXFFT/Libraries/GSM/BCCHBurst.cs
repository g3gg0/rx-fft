using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibRXFFT.Libraries.GMSK;
using LibRXFFT.Libraries.GSM_Layer3;

namespace LibRXFFT.Libraries.GSM
{
    public class BCCHBurst : NormalBurst
    {
        private bool[][] BCCHBuffer;
        public L3PacketTypes pktTypes = new L3PacketTypes();
        public Dictionary<int, long> Occurences = new Dictionary<int, long>();
        private L3Handler L3;


        public BCCHBurst (L3Handler l3)
        {
            L3 = l3;
            Name = "BCCH";
            BCCHBuffer = new bool[4][];
            for (int pos = 0; pos < BCCHBuffer.Length; pos++)
                BCCHBuffer[pos] = new bool[114];
        }

        public override bool ParseData(GSMParameters param, bool[] decodedBurst)
        {
            int part = (int)((param.CurrentControlFrame % 51) - 2);

            Array.Copy(decodedBurst, 3, BCCHBuffer[part], 0, 57);
            Array.Copy(decodedBurst, 88, BCCHBuffer[part], 57, 57);

            if (part == 3)
            {
                bool[][] BCCHData = InterleaveCoder.Deinterleave(BCCHBuffer, null);
                bool[] BCCHDataDeinterleaved = ConvolutionalCoder.Decode(BCCHData[0], null);
                if (BCCHDataDeinterleaved == null)
                {
                    ErrorMessage = "(Error in ConvolutionalCoder)";
                    return false;
                }

                bool[] crc = CRC.Calc(BCCHDataDeinterleaved, 0, 224, CRC.PolynomialFIRE);
                if (!CRC.Matches(crc))
                {
                    ErrorMessage = "(Error in CRC)";
                    return false;
                }

                byte[] data = ByteUtil.BitsToBytesRev(BCCHDataDeinterleaved);

                if ( (data[0] & 3) != 1 )
                {
                    ErrorMessage = "(Error in L2 Pseudo Length)";
                    return false;
                }

                L3.Handle(data, 1);

                /*
                int l2PseudoLength = data[0] >> 2;
                
                byte[] l3Data = new byte[l2PseudoLength];

                Array.Copy(data, 1, l3Data, 0, l2PseudoLength);
                */

                /*
                if (data.Length - l2PseudoLength > 0)
                {
                    StatusMessage = "Rest Octets: ";
                    byte[] l2RestOctets = new byte[data.Length - l2PseudoLength];

                    Array.Copy(data, l2PseudoLength, l2RestOctets, 0, l2RestOctets.Length);
                    DumpBytes(l2RestOctets);
                }
                */
                //DumpBits(BCCHDataDeinterleaved);
                //DumpBytes(data);
            }
            else
                StatusMessage = null;

            return true;
        }

        private void UpdateStats(int packetType)
        {
            if (Occurences.ContainsKey(packetType))
                Occurences[packetType]++;
            else
                Occurences.Add(packetType,1);
        }
    }
}
