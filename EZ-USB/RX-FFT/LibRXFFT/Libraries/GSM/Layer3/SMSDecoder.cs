using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using LibRXFFT.Libraries.GSM.CharacterCoding;
using System.IO;

namespace LibRXFFT.Libraries.GSM.Layer3
{
    /*
          RP-User Data (GSM 04.11 8.2.5.3):
             06 | FD | 0B 81 10 07 68 61 68 F3 | 01 80 12 32 25 44 80 | 01 80 12 32 25 64 80 00 
			 
          RP-User Data (GSM 04.11 8.2.5.3):
             04 | 05 89 66 93 F9 | 00 00 | 01 80 12 32 55 40 80 | 99 D7 32 9D 5E 96 EB 40 C1 36 68 1A 6E CF E9 E1 33 C8 9E 2E B3 CB F2 B7 9C 3E 07 CD E9 F2 30 9A 5D 76 93 CB 72 D0 F4 ED 76 97 DD F3 31 BA 9C 76 83 5A A0 63 F9 9E A6 D3 CB 72 50 DD 4D 06 49 CB E7 B2 DB BC 67 B3 CB A0 74 1B E4 7C CB C9 65 37 A8 05 6A EF 3D E9 73 59 0E 9A FA C9 F7 F2 9C 7E 4F BB C9 A0 D5 6A 05 A2 96 DB F0 B2 3C 4C AF CB CB 6E 90 FE 9E 9E 8F D1 65 37 48 36 03 D5 DD 64 D0 2C 06 3A CA C3 64 
			 
     * GSM 03.40, (GSM 07.05)
     
     * 
     * 
     * http://www.activexperts.com/xmstoolkit/sms/technical/
     * 
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

            messages.Add("SMS-TL (GSM 03.40 9.2.2)");
            builder.Append("Message type        ");

            string message = null;
            string originating = null;
            string timestamp = null;

            switch (tpMti)
            {
                case 0:
                    {
                        /* GSM 03.40 9.2.2.1, http://www.slideshare.net/seanraz/10-slides-to-sms */
                        bool tpRp = (pduData[0] & 0x80) != 0;
                        bool tpUdhi = (pduData[0] & 0x40) != 0;
                        bool tpSri = (pduData[0] & 0x20) != 0;
                        bool tpMms = (pduData[0] & 0x04) != 0;
                        int tpDcs = 0;
                        int tpUdl = 0;

                        builder.Append("SMS-DELIVER");
                        messages.Add(builder.ToString());
                        builder.Length = 0;

                        builder.Append("More Messages       " + (tpMms?"Yes":"No"));
                        messages.Add(builder.ToString());
                        builder.Length = 0;

                        builder.Append("Reply Path          " + (tpRp ? "Yes" : "No"));
                        messages.Add(builder.ToString());
                        builder.Length = 0;

                        builder.Append("User Data Header    " + (tpUdhi ? "Exists" : "None"));
                        messages.Add(builder.ToString());
                        builder.Length = 0;

                        builder.Append("Status Report       " + (tpSri ? "Requested" : "Not requested"));
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        /* second octet, TP-OA */
                        originating = DecodeAddress(pduData, pos, out elementLength);
                        builder.Append("Originating address ").Append(originating);
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos += elementLength;

                        /* TP-PID */
                        builder.Append("Protocol identifier ").Append(pduData[pos]);
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        /* TP-DCS */
                        tpDcs = pduData[pos];
                        builder.Append("Data coding scheme  ").Append(tpDcs);
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        /* TP-SCTS */
                        timestamp = DecodeTimestamp(pduData, pos, out elementLength);
                        builder.Append("SC Timestamp        ").Append(timestamp);
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos += elementLength;

                        /* TP-UDL */
                        tpUdl = pduData[pos];
                        builder.Append("TP-User Data length ").Append(tpUdl).Append(" characters");
                        messages.Add(builder.ToString());
                        builder.Length = 0;
                        pos++;

                        int udhLength = 0;
                        bool containsSMSOTA = false;

                        /* GSM 03.40 9.2.3.24 */
                        if (!tpUdhi)
                        {
                            builder.Append("TP-User Data Header ").Append("None");
                            messages.Add(builder.ToString());
                            builder.Length = 0;
                            udhLength = 0;
                        }
                        else
                        {
                            int udhPos = pos;

                            /* TP-UDH */
                            builder.Append("TP-User Data Header ").Append("Exists");
                            messages.Add(builder.ToString());
                            builder.Length = 0;

                            udhLength = pduData[pos] + 1;
                            builder.Append("   Header Length    ");
                            builder.Append(udhLength).Append(" byte");
                            messages.Add(builder.ToString());
                            builder.Length = 0;

                            udhPos++;
                            while (udhPos < (pos + udhLength))
                            {
                                int type = pduData[udhPos++];
                                int length = pduData[udhPos++];

                                builder.Append("      Type          ");
                                builder.Append(type);
                                messages.Add(builder.ToString());
                                builder.Length = 0;

                                builder.Append("      Length        ");
                                builder.Append(length).Append(" byte");
                                messages.Add(builder.ToString());
                                builder.Length = 0;

                                builder.Append("      Content       ");
                                for (int len = 0; len < length; len++)
                                {
                                    builder.AppendFormat("{0:X02} ", pduData[udhPos + len]);
                                }
                                messages.Add(builder.ToString());
                                builder.Length = 0;


                                switch (type)
                                {
                                    case 0:
                                        int reference = pduData[udhPos];
                                        int max = pduData[udhPos + 1];
                                        int num = pduData[udhPos + 2];

                                        builder.Append("      Decoded       ");
                                        builder.AppendFormat("Concatenated SMS. This is message {1} of {2} (ID: {0})", reference, num, max);
                                        messages.Add(builder.ToString());
                                        builder.Length = 0;
                                        break;

                                    case 1:
                                        int indication = pduData[udhPos] & 0x7F;
                                        bool store = (pduData[udhPos] & 0x80) != 0;
                                        int msgs = pduData[udhPos + 1];
                                        string msgType = "Unknown: " + indication;

                                        switch (indication)
                                        {
                                            case 0:
                                                msgType = "Voice Message";
                                                break;
                                            case 1:
                                                msgType = "Fax Message";
                                                break;
                                            case 2:
                                                msgType = "E-Mail Message";
                                                break;
                                            case 3:
                                                msgType = "Other Message";
                                                break;
                                        }

                                        builder.Append("      Decoded       ");
                                        builder.AppendFormat("Message notification, type: '{1}', messages: {2}, {0}", store ? "store this sms" : "don't store this sms", msgType, msgs);
                                        messages.Add(builder.ToString());
                                        builder.Length = 0;
                                        break;

                                    case 0x70:
                                        int ccSize = pduData[udhPos];
                                        containsSMSOTA = true;

                                        builder.Append("      Decoded       ");
                                        builder.AppendFormat("SMS-PP Command Packet with size {0}", ccSize);
                                        messages.Add(builder.ToString());
                                        builder.Length = 0;
                                        break;

                                    default:
                                        builder.Append("      Decoded       ");
                                        builder.AppendFormat("Unknown/Undefined ID 0x{0:X02}", type);
                                        messages.Add(builder.ToString());
                                        builder.Length = 0;
                                        break;

                                }
                                udhPos += length;
                            }
                        }

                        /* TP-UD */
                        builder.Append("TP-User Data        ");

                        int userdataPayload = pduData.Length - pos;
                        bool decoded = false;
                        int coding = -1;

                        /* no message class, plain text. still missing the special cases defined in GSM 03.38 */
                        if ((tpDcs & 0xF0) == 0)
                        {
                            coding = (tpDcs >> 2) & 3;
                        }
                        else if ((tpDcs & 0xF0) == 0x10)
                        {
                            /* message with message class */
                            coding = (tpDcs >> 2) & 3;
                        }
                        else if ((tpDcs & 0xF0) == 0xF0)
                        {
                            if ((tpDcs & 3) != 0)
                            {
                                /* message class other than 0 */
                                coding = tpDcs;
                            }
                            else
                            {
                                /* class 0 message */
                                if ((tpDcs & 4) != 0)
                                {
                                    /* 8 bit data */
                                    coding = 1;
                                }
                                else
                                {
                                    /* 7 bit data */
                                    coding = 0;
                                }
                            }
                        }

                        switch (coding)
                        {
                            case 0:
                                builder.Append("7 bit GSM alphabet");
                                messages.Add(builder.ToString());
                                builder.Length = 0;

                                message = DecodeGSM7Bit(pduData, pos, udhLength, tpUdl);
                                builder.Append("   Content          ");
                                builder.Append(message);
                                messages.Add(builder.ToString());
                                builder.Length = 0;
                                decoded = true;
                                break;

                            case 1:
                                builder.Append("8 bit message");
                                messages.Add(builder.ToString());
                                builder.Length = 0;

                                message = DecodeASCII(pduData, pos, udhLength, tpUdl);
                                builder.Append("   Content          ");
                                builder.Append(message);
                                messages.Add(builder.ToString());
                                builder.Length = 0;
                                decoded = true;
                                break;

                            case 2:
                                builder.Append("Unicode message");
                                messages.Add(builder.ToString());
                                builder.Length = 0;

                                message = DecodeUnicode(pduData, pos, udhLength, tpUdl);
                                builder.Append("   Content          ");
                                builder.Append(message);
                                messages.Add(builder.ToString());
                                builder.Length = 0;
                                decoded = true;
                                break;

                            case 0xF5:
                                break;

                            /*
                             * need to check this:
                             * http://adywicaksono.wordpress.com/2008/05/21/understanding-gsm-0348/ 
                             * seems to be a good documentation to check against
                             */
                            case 0xF6:
                                builder.Append("SMS-OTA (GSM-03.48)");
                                messages.Add(builder.ToString());
                                builder.Length = 0;

                                if (containsSMSOTA && pduData.Length > pos + 14)
                                {
                                    int pLength = (pduData[pos] << 8) | pduData[pos + 1];
                                    int hLength = pduData[pos + 2];
                                    int spi1 = pduData[pos + 3];
                                    int spi2 = pduData[pos + 4];
                                    int kic = pduData[pos + 5];
                                    int kid = pduData[pos + 6];
                                    int tar = (pduData[pos + 7] << 16) | (pduData[pos + 8] << 8) | pduData[pos + 9];
                                    uint cntr = ((uint)pduData[pos + 10] << 24) | (uint)(pduData[pos + 11] << 16) | (uint)(pduData[pos + 12] << 8) | (uint)pduData[pos + 13];

                                    builder.Append("   Packet Length:   ").Append(pLength);
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;
                                    builder.Append("   Header Length:   ").Append(hLength);
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;


                                    /* SPI */
                                    builder.Append("   SPI:             ");
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    switch (spi1 & 3)
                                    {
                                        case 0:
                                            builder.Append("      No RC, CC or DS");
                                            break;
                                        case 1:
                                            builder.Append("      Redundancy Check");
                                            break;
                                        case 2:
                                            builder.Append("      Cryptographic Checksum");
                                            break;
                                        case 3:
                                            builder.Append("      Digital Signature");
                                            break;
                                    }
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;


                                    switch ((spi1 >> 2) & 1)
                                    {
                                        case 0:
                                            builder.Append("      No Ciphering");
                                            break;
                                        case 1:
                                            builder.Append("      Ciphering");
                                            break;
                                    }
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    switch ((spi1 >> 3) & 3)
                                    {
                                        case 0:
                                            builder.Append("      No counter available");
                                            break;
                                        case 1:
                                            builder.Append("      Counter available; no replay or sequence checking");
                                            break;
                                        case 2:
                                            builder.Append("      Process if and only if counter value is higher than the value in the RE");
                                            break;
                                        case 3:
                                            builder.Append("      Process if and only if counter value is one higher than");
                                            break;
                                    }
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    switch (spi2 & 3)
                                    {
                                        case 0:
                                            builder.Append("      No PoR reply to the Sending Entity (SE)");
                                            break;
                                        case 1:
                                            builder.Append("      PoR required to be sent to the SE");
                                            break;
                                        case 2:
                                            builder.Append("      PoR required only when an error has occured");
                                            break;
                                        case 3:
                                            builder.Append("      Reserved");
                                            break;
                                    }
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    switch ((spi2 >> 2) & 3)
                                    {
                                        case 0:
                                            builder.Append("      No security applied to PoR response to SE");
                                            break;
                                        case 1:
                                            builder.Append("      PoR response with simple RC applied to it");
                                            break;
                                        case 2:
                                            builder.Append("      PoR response with CC applied to it");
                                            break;
                                        case 3:
                                            builder.Append("      PoR response with DS applied to it");
                                            break;
                                    }
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    switch ((spi2 >> 4) & 1)
                                    {
                                        case 0:
                                            builder.Append("      PoR response shall not be ciphered");
                                            break;
                                        case 1:
                                            builder.Append("      PoR response shall be ciphered");
                                            break;
                                    }
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    switch ((spi2 >> 5) & 1)
                                    {
                                        case 0:
                                            builder.Append("      PoR response shall be sent using SMS-DELIVER-REPORT");
                                            break;
                                        case 1:
                                            builder.Append("      PoR response shall be sent using SMS-SUBMIT ");
                                            break;
                                    }
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;


                                    /* KIc */
                                    builder.Append("   KIc:             ");
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    switch (kic & 3)
                                    {
                                        case 0:
                                            builder.Append("      Algorithm known implicitly by both entities");
                                            break;
                                        case 1:
                                            builder.Append("      DES");
                                            break;
                                        case 2:
                                            builder.Append("      Reserved");
                                            break;
                                        case 3:
                                            builder.Append("      proprietary Implementations");
                                            break;
                                    }

                                    switch ((kic >> 2) & 3)
                                    {
                                        case 0:
                                            builder.Append("      DES in CBC mode");
                                            break;
                                        case 1:
                                            builder.Append("      Triple DES in outer-CBC mode using two different keys");
                                            break;
                                        case 2:
                                            builder.Append("      Triple DES in outer-CBC mode using three different keys");
                                            break;
                                        case 3:
                                            builder.Append("      DES in ECB mode");
                                            break;
                                    }
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    builder.Append("      Using key #").Append((kic >> 4));
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;


                                    /* KID */
                                    switch (kid & 3)
                                    {
                                        case 0:
                                            builder.Append("      Algorithm known implicitly by both entities");
                                            break;
                                        case 1:
                                            builder.Append("      DES");
                                            break;
                                        case 2:
                                            builder.Append("      Reserved");
                                            break;
                                        case 3:
                                            builder.Append("      proprietary Implementations");
                                            break;
                                    }

                                    switch ((kid >> 2) & 3)
                                    {
                                        case 0:
                                            builder.Append("      DES in CBC mode");
                                            break;
                                        case 1:
                                            builder.Append("      Triple DES in outer-CBC mode using two different keys");
                                            break;
                                        case 2:
                                            builder.Append("      Triple DES in outer-CBC mode using three different keys");
                                            break;
                                        case 3:
                                            builder.Append("      DES in ECB mode");
                                            break;
                                    }
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    builder.Append("      Using key #").Append((kid >> 4));
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    /* counter */

                                    builder.Append("   TAR:             ").AppendFormat("{0:X06}", tar);
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;
                                    builder.Append("   CNTR:            ").AppendFormat("{0:X08}", cntr);
                                    messages.Add(builder.ToString());
                                    builder.Length = 0;

                                    decoded = true;
                                }
                                break;
                        }
                    
                        
                        if(!decoded)
                        {
                            builder.Append("Message with unknown data coding scheme - will not decode");
                            messages.Add(builder.ToString());
                            builder.Length = 0;

                            builder.Append("   Content          ");
                            for (int len = pos; len < userdataPayload; len++)
                            {
                                builder.AppendFormat("{0:X02} ", pduData[len]);
                            }
                            messages.Add(builder.ToString());
                            builder.Length = 0;
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

            if (timestamp != null && message != null && originating != null)
            {
                try
                {
                    FileStream stream = File.Open("sms.txt", FileMode.Append, FileAccess.Write);
                    TextWriter writer = new StreamWriter(stream);
                    string key = "(failed)";
                    string ident = "(failed)";
                    string prevIdent = "";

                    try
                    {
                        key = ByteUtil.BytesToString(((LibRXFFT.Libraries.GSM.Layer1.Bursts.NormalBurst)LibRXFFT.Libraries.GSM.Layer1.TimeSlotHandler._HACK_Parameters.CurrentBurstHandler).A5CipherKey);
                        ident = ((LibRXFFT.Libraries.GSM.Layer1.Bursts.NormalBurst)LibRXFFT.Libraries.GSM.Layer1.TimeSlotHandler._HACK_Parameters.CurrentBurstHandler).PhoneIdentity.Trim().Replace("TMSI/P-TMSI ", "").Replace("IMSI ", "").Replace("  ", " ").PadRight(14);
                        prevIdent = ((LibRXFFT.Libraries.GSM.Layer1.Bursts.NormalBurst)LibRXFFT.Libraries.GSM.Layer1.TimeSlotHandler._HACK_Parameters.CurrentBurstHandler).PhoneIdentityPrev.Trim().Replace("TMSI/P-TMSI ", "").Replace("IMSI ", "").Replace("  ", " ").PadRight(14);
                    }
                    catch (Exception)
                    {
                    }

                    string identStr = "";

                    if (prevIdent != "")
                    {
                        identStr = prevIdent + " -> " + ident;
                    }
                    else
                    {
                        identStr = ident + "    " + "".PadRight(14);
                    }

                    writer.WriteLine(originating.PadRight(16) + " | " + identStr + " | " + key + " | " + timestamp + " | " + message);
                    writer.Close();
                }
                catch (Exception)
                {
                }
            }

            return (string[])messages.ToArray(typeof(string));
        }

        private static string DecodeGSM7Bit(byte[] pduData, int start, int skipBytes, int charCount)
        {
            try
            {
                return GSM7Bit.Decode(pduData, start, charCount, (skipBytes * 8 + 6) / 7);
            }
            catch (Exception e)
            {
                return "(DecodeGSM7Bit failed)";
            }
        }

        private static string DecodeASCII(byte[] pduData, int pos, int skipBytes, int charCount)
        {
            try
            {
                ASCIIEncoding enc = new ASCIIEncoding();

                return enc.GetString(pduData, pos + skipBytes, charCount - skipBytes);
            }
            catch (Exception e)
            {
                return "(DecodeASCII failed)";
            }
        }

        private static string DecodeUnicode(byte[] pduData, int pos, int skipBytes, int charCount)
        {
            try
            {
                UnicodeEncoding enc = new UnicodeEncoding(true, false);

                return enc.GetString(pduData, pos + skipBytes, charCount - skipBytes);
            }
            catch (Exception e)
            {
                return "(DecodeUnicode failed)";
            }
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
            int coding = pduData[start + 1];
            int digitStart = start + 2;
            StringBuilder builder = new StringBuilder();

            /* 1 byte digit count, 1 byte numbering scheme plus the number of nibbles/2 */
            elementLength = 1 + 1 + (length + 1) / 2;

            switch (coding & 0x7F)            
            {
                case 0x50:
                    /* alphanumeric */
                    string message = GSM7Bit.Decode(pduData, digitStart, ((length * 4) / 7), 0);
                    builder.Append(message);
                    break;

                default:
                    char[] characters = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '*', '#', 'a', 'b', 'c', ' ', };

                    for (int digit = 0; digit < length; digit++)
                    {
                        long bcd = (pduData[digitStart + digit / 2] >> (4 * (digit % 2))) & 0x0F;

                        builder.Append(characters[bcd]);
                    }
                    break;
            }

            return builder.ToString();
        }
    }
}
