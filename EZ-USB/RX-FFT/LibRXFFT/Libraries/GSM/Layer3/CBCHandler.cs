using System;
using LibRXFFT.Libraries.GSM.CharacterCoding;

namespace LibRXFFT.Libraries.GSM.Layer3
{
    public class CBCHandler
    {
        public static bool ShowCBMessages = true;
        public string StatusMessage;
        private readonly byte[] CBCBuffer;
        private readonly byte[] CBMessage;
        private int Sequence;

        public CBCHandler()
        {
            CBCBuffer = new byte[88];
            CBMessage = new byte[82];
        }

        public bool Handle (byte[] data)
        {
            int lpd = (data[0] >> 5) & 3;
            int seq = data[0] & 0x0f;

            if (lpd != 1 || data.Length < 23)
                return false;

            if (Sequence != seq)
            {
                Sequence = 0;
                return false;
            }

            Array.Copy(data, 1, CBCBuffer, seq*22, 22);

            Sequence++;

            /* final packet */
            if (Sequence >= 4)
            {
                bool emergency = ((CBCBuffer[0] >> 5) & 0x01) == 1;
                bool popup = ((CBCBuffer[0] >> 4) & 0x01) == 1;
                int messageCode = (((CBCBuffer[0] << 8) | CBCBuffer[1]) >> 4) & 0xFF;
                int updateNr = CBCBuffer[1] & 0xF;
                int messageIdent = (CBCBuffer[2] << 8) | CBCBuffer[3];
                int dataCoding = CBCBuffer[4];
                int pageParam = CBCBuffer[5];

                Array.Copy(CBCBuffer, 6, CBMessage, 0, CBMessage.Length);

                if (ShowCBMessages)
                {
                    StatusMessage = "Cell Broadcast: ";

                    if (emergency)
                        StatusMessage += "[Emergency] ";
                    if (popup)
                        StatusMessage += "[Popup] ";

                    StatusMessage += "[Channel " + messageIdent + "] ";
                    StatusMessage += "[Update " + updateNr + "] ";
                    StatusMessage += "[Code " + messageCode + "] ";
                    StatusMessage += "[Coding 0x" + String.Format("{0:X2}", dataCoding) + "] ";
                    StatusMessage += "[Page 0x" + String.Format("{0:X2}", pageParam) + "] ";
                    StatusMessage += Environment.NewLine;
                    StatusMessage += "          '" + GSM7Bit.Decode(CBMessage) + "'" + Environment.NewLine;
                }
                else
                    StatusMessage = null;

                Sequence = 0;
            }

            return true;
        }
    }
}
