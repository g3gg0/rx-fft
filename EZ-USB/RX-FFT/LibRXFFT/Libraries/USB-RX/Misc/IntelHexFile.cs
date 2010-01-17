using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LibRXFFT.Libraries.USB_RX.Misc
{
    public class IntelHexFile
    {
        private int MAX_CODE_LENGTH = 0x10000;
        private int DATA_RECORD = 0;
        private TextReader hexFile;

        public IntelHexFile(string fileName)
        {
            hexFile = new StreamReader(fileName);
        }

        public byte[] Parse()
        {
            String record = "";
            int recordNum = 0;
            int dataLength = 0;
            int maxAddress = 0;
            byte[] hexVals = null;
            byte[] tempBuffer = new byte[MAX_CODE_LENGTH];

            for (int pos = 0; pos < tempBuffer.Length; pos++)
            {
                tempBuffer[pos] = 0xFF;
            }

            while ((record = hexFile.ReadLine()) != null)
            {
                recordNum++;
                hexVals = new byte[record.Length / 2];

                for (int pos = 0; pos < hexVals.Length; pos++)
                {
                    string byteHex = record.Substring(2 * pos + 1, 2);
                    hexVals[pos] = byte.Parse(byteHex, System.Globalization.NumberStyles.HexNumber);
                }

                dataLength = hexVals[0];

                if (dataLength > 0 && hexVals[3] == DATA_RECORD)
                {
                    int address = hexVals[1] * 0x0100 + hexVals[2];

                    for (int pos = 0; pos < dataLength; pos++)
                    {
                        if (pos + address > MAX_CODE_LENGTH)
                            throw new Exception("invalid address: " + pos + " in record " + recordNum);
                        tempBuffer[pos + address] = hexVals[4 + pos];
                        maxAddress = Math.Max(maxAddress, pos + address);
                    }

                    if (checksum(hexVals) != hexVals[4 + dataLength])
                        throw new Exception("invalid checksum in record " + recordNum);
                }
            }

            byte[] buffer = new byte[maxAddress + 1];
            for (int pos = 0; pos < maxAddress + 1; pos++)
            {
                buffer[pos] = tempBuffer[pos];
            }

            return buffer;
        }

        public int checksum(byte[] data)
        {
            int check = 0;
            for (int pos = 0; pos < data.Length - 1; pos++)
            {
                check += data[pos];
            }

            return (check * 0xFF) & 0xFF;
        }
    }
}
