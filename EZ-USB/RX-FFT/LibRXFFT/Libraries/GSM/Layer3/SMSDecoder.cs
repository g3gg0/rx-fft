using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using LibRXFFT.Libraries.GSM.CharacterCoding;

namespace LibRXFFT.Libraries.GSM.Layer3
{
    /*
          RP-User Data (GSM 04.11 8.2.5.3):
             06 | FD | 0B 81 10 07 68 61 68 F3 | 01 80 12 32 25 44 80 | 01 80 12 32 25 64 80 00 
			 
          RP-User Data (GSM 04.11 8.2.5.3):
             04 | 05 89 66 93 F9 | 00 00 | 01 80 12 32 55 40 80 | 99 D7 32 9D 5E 96 EB 40 C1 36 68 1A 6E CF E9 E1 33 C8 9E 2E B3 CB F2 B7 9C 3E 07 CD E9 F2 30 9A 5D 76 93 CB 72 D0 F4 ED 76 97 DD F3 31 BA 9C 76 83 5A A0 63 F9 9E A6 D3 CB 72 50 DD 4D 06 49 CB E7 B2 DB BC 67 B3 CB A0 74 1B E4 7C CB C9 65 37 A8 05 6A EF 3D E9 73 59 0E 9A FA C9 F7 F2 9C 7E 4F BB C9 A0 D5 6A 05 A2 96 DB F0 B2 3C 4C AF CB CB 6E 90 FE 9E 9E 8F D1 65 37 48 36 03 D5 DD 64 D0 2C 06 3A CA C3 64 
			 
    */
    public class SMSDecoder
    {
        public static string[] DecodeTLPacketDownlink(byte[] pduData)
        {
            int pos = 0;
            int elementLength = 0;
            int tpMti = (pduData[0] & 0x03);
            StringBuilder builder = new StringBuilder();
            ArrayList messages = new ArrayList();

            builder.Append("Message type        ");
            switch (tpMti)
            {
                case 0:
                    {
                        int tpUdhi = (pduData[0] & 0x10);

                        builder.Append("SMS-DELIVER");
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        builder.Append("Originating address ").Append(DecodeAddress(pduData, pos, out elementLength));
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos += elementLength;

                        builder.Append("Protocol identifier ").Append(pduData[pos]);
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        builder.Append("Data coding scheme  ").Append(pduData[pos]);
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        builder.Append("SC Timestamp        ").Append(DecodeTimestamp(pduData, pos, out elementLength));
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos += elementLength;

                        builder.Append("TP-User Data length ").Append(pduData[pos]);
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        builder.Append("TP-User Data");
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        if (tpUdhi == 0)
                        {
                            builder.Append("GSM 7 bit message   ").Append(GSM7Bit.Decode(pduData, pos, pduData.Length - pos));
                            messages.Add(builder.ToString());
                            builder.Length = 0;
                        }
                        else
                        {
                            builder.Append("Message with User Data Header - will not decode");
                        }
                        break;
                    }
                case 1:
                    {
                        builder.Append("SMS-SUBMIT-REPORT");
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        break;
                    }
                case 2:
                    {
                        builder.Append("SMS-STATUS-REPORT");
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        builder.Append("Message reference   ").Append(pduData[pos]);
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        builder.Append("Recipient address   ").Append(DecodeAddress(pduData, pos, out elementLength));
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos += elementLength;

                        builder.Append("SC Timestamp        ").Append(DecodeTimestamp(pduData, pos, out elementLength));
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos += elementLength;

                        builder.Append("Discharge time      ").Append(DecodeTimestamp(pduData, pos, out elementLength));
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos += elementLength;

                        builder.Append("Status code         ").Append(pduData[pos]);
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        break;
                    }
                case 3:
                    {
                        builder.Append("Reserved");
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        break;
                    }
            }
            return (string[])messages.ToArray(typeof(string));
        }


        private static string DecodeTimestamp(byte[] pduData, int start, out int elementLength)
        {
            int length = 14;
            int digitStart = start;
            string[] separators = new[] { "20", "", "-", "", "-", "", " ", "", ":", "", ":", "", " TZ:", "" };
            StringBuilder builder = new StringBuilder();

            /* fixed length */
            elementLength = 7;

            for (int digit = 0; digit < length; digit++)
            {
                long bcd = (pduData[digitStart + digit / 2] >> (4 * (digit % 2))) & 0x0F;

                if (bcd < 10)
                {
                    builder.Append(separators[digit]).Append(bcd);
                }
                else
                {
                    builder.Append(separators[digit]).Append(" ");
                }
            }

            return builder.ToString();
        }

        private static string DecodeAddress(byte[] pduData, int start, out int elementLength)
        {
            int length = pduData[start];
            int digitStart = start + 2;
            StringBuilder builder = new StringBuilder();

            /* 1 byte digit count, 1 byte numbering scheme plus the number of bcd digits/2 */
            elementLength = 1 + 1 + (length + 1) / 2;

            for (int digit = 0; digit < length; digit++)
            {
                long bcd = (pduData[digitStart + digit / 2] >> (4 * (digit % 2))) & 0x0F;

                if (bcd < 10)
                {
                    builder.Append(bcd);
                }
                else
                {
                    builder.Append(" ");
                }
            }

            return builder.ToString();
        }
    }
}
