using System;
using System.Collections;
using System.Text;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.GSM.Layer2;
using RX_FFT.Components.GDI;

namespace DemodulatorCollection
{
    public class POCSAGDecoder : BitClockSink
    {
        private const uint POCSAG_SYNC = 0x7CD215D8;
        private const uint POCSAG_SYNCINFO = 0x7CF21436;
        private const uint POCSAG_IDLE = 0x7a89c197;

        private ArrayList Bits = new ArrayList();
        private ArrayList MessageBits = new ArrayList();

        private bool[] FrameCrcPolynomial = new[] { true, true, true, false, true, true, false, true, false, false, true };
        private bool[] FrameCrcBuffer = new bool[10];
        private char[] NumericCoding = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '?', 'U', ' ', '_', '[', ']' };

        private bool Synchronized;
        private int ParityErrors = 0;

        /* current message */
        private uint Address = 0;
        private uint Mode = 0;
        private int BatchPosition;
        private const int ParityErrorsMax = 25;


        public void ClockBit(bool state)
        {
            Bits.Add(state);

            /* every codeword */
            if (Bits.Count >= 32)
            {
                /* only keep last 32 bits */
                Bits.RemoveRange(0, Bits.Count - 32);

                bool[] bits = (bool[])Bits.ToArray(typeof(bool));
                long codeWord = ByteUtil.BitsToLong(bits);

                if (Synchronized && !CorrectChecksum(bits))
                {
                    if (++ParityErrors > ParityErrorsMax)
                    {
                        //Log.AddMessage("Parity error, lost synchronization.");
                        Synchronized = false;
                    }
                }
                else
                {
                    ParityErrors = 0;

                    if (codeWord == POCSAG_SYNC || codeWord == POCSAG_SYNCINFO)
                    {
                        if (!Synchronized)
                        {
                            //Log.AddMessage("Synchronized...");
                        }

                        Synchronized = true;
                        BatchPosition = 0;
                        Bits.Clear();
                    }
                    else if (Synchronized)
                    {
                        if (codeWord == POCSAG_IDLE)
                        {
                            //Log.AddMessage("Idle");
                            DumpMessage();
                        }
                        else
                        {
                            if (!bits[0])
                            {
                                DumpMessage();

                                /* address */
                                //Log.AddMessage("Address");
                                Address = (uint)(codeWord >> 13) & 0x3FFF;
                                Address <<= 3;
                                Address |= (uint)BatchPosition >> 1;
                                Mode = (uint)(codeWord >> 11) & 0x03;
                            }
                            else
                            {
                                /* message bits */
                                //Log.AddMessage("Message");
                                MessageBits.AddRange(Bits.GetRange(1, 20));
                            }
                        }
                        Bits.Clear();
                        BatchPosition++;
                    }
                }
            }
        }

        private bool CorrectChecksum(bool[] bits)
        {
            int highCount = 0;

            foreach (bool bit in bits)
            {
                if (bit)
                {
                    highCount++;
                }
            }

            if ((highCount % 2) != 0)
                return false;

            bool[] data = new bool[bits.Length - 1];
            Array.Copy(bits, data, data.Length);

            CRC.Calc(data, 0, data.Length, FrameCrcPolynomial, FrameCrcBuffer);

            if (!CRC.Matches(FrameCrcBuffer, true))
                return false;

            return true;
        }

        private void DumpMessage()
        {
            if (Address != 0)
            {
                string message = "A: " + Address.ToString("0000000") + " M: " + Mode;

                if (MessageBits.Count > 0)
                {
                    bool[] reorderedNumBits = ReorderBits((bool[])MessageBits.ToArray(typeof(bool)), 4);
                    bool[] reorderedMsgBits = ReorderBits((bool[])MessageBits.ToArray(typeof(bool)), 7);
                    byte[] NumBytes = ByteUtil.BitsToBytes(reorderedNumBits, 4);
                    byte[] msgBytes = ByteUtil.BitsToBytes(reorderedMsgBits, 7);
                    byte[] rawBytes = ByteUtil.BitsToBytes((bool[])MessageBits.ToArray(typeof(bool)));
                    string textMessage = new ASCIIEncoding().GetString(msgBytes);
                    string numMessage = GetNumeric(NumBytes);

                    if (IsPrintable(textMessage))
                    {
                        message += " (TEXT) " + textMessage;
                    }
                    else
                    {
                        message += " (NUM) " + numMessage;
                        message += " (DATA) ";
                        foreach (byte msgByte in rawBytes)
                        {
                            message += msgByte.ToString("x2") + " ";
                        }
                    }
                }
                else
                {
                    message += " (BEEP)";
                }

                Log.AddMessage(message);

                Address = 0;
            }
            MessageBits.Clear();
        }

        private string GetNumeric(byte[] bytes)
        {
            string message = "";

            foreach (byte b in bytes)
            {
                message += NumericCoding[b & 0x0F];
            }

            return message;
        }


        private static bool IsPrintable(string message)
        {
            foreach (char character in message)
            {
                switch ((int)character)
                {
                    case 0:
                    case 4:
                    case 7:
                    case 10:
                    case 13:
                        break;

                    default:
                        if (character < 0x20 || character == 0x7F)
                            return false;
                        break;

                }
            }

            return true;
        }

        private static bool[] ReorderBits(bool[] bits)
        {
            return ReorderBits(bits, 7);
        }

        private static bool[] ReorderBits(bool[] bits, int bitsPerByte)
        {
            int bytes = bits.Length / bitsPerByte;
            bool[] reordered = new bool[bytes * bitsPerByte];

            for (int byteNum = 0; byteNum < bytes; byteNum++)
            {
                for (int bitNum = 0; bitNum < bitsPerByte; bitNum++)
                {
                    reordered[byteNum * bitsPerByte + bitNum] = bits[byteNum * bitsPerByte + ((bitsPerByte - 1) - bitNum)];
                }
            }

            return reordered;
        }
    }
}