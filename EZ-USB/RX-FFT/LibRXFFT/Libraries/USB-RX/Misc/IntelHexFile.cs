using System;
using System.Globalization;
using System.IO;
using System.Collections;

namespace LibRXFFT.Libraries.USB_RX.Misc
{
    public class IntelHexFile
    {
        private int MAX_CODE_LENGTH = 0x10000;
        private int DATA_RECORD = 0;
        private string[] HexLines;

        public IntelHexFile(string fileName)
        {
            TextReader hexFile = null;

            try
            {
                hexFile = new StreamReader(fileName);

                ArrayList records = new ArrayList();
                string record;
                while ((record = hexFile.ReadLine()) != null)
                {
                    records.Add(record);
                }

                HexLines = (string[])records.ToArray(typeof(string));
            }
            catch (Exception e)
            {
            }

            if (hexFile != null)
            {
                hexFile.Close();
            }
        }

        public MemoryDump8Bit Parse()
        {
            int recordNum = 0;
            int dataLength = 0;
            int maxAddress = 0;
            int minAddress = MAX_CODE_LENGTH;
            byte[] hexVals = null;
            byte[] tempBuffer = new byte[MAX_CODE_LENGTH];

            for (int pos = 0; pos < tempBuffer.Length; pos++)
            {
                tempBuffer[pos] = 0xFF;
            }

            foreach (string record in HexLines)
            {
                recordNum++;
                hexVals = new byte[record.Length / 2];

                for (int pos = 0; pos < hexVals.Length; pos++)
                {
                    string byteHex = record.Substring(2 * pos + 1, 2);
                    hexVals[pos] = byte.Parse(byteHex, NumberStyles.HexNumber);
                }

                dataLength = hexVals[0];

                if (dataLength > 0 && hexVals[3] == DATA_RECORD)
                {
                    int address = hexVals[1] * 0x0100 + hexVals[2];

                    minAddress = Math.Min(minAddress, address);
                    maxAddress = Math.Max(maxAddress, address + dataLength);

                    for (int pos = 0; pos < dataLength; pos++)
                    {
                        if (pos + address > MAX_CODE_LENGTH)
                            throw new Exception("invalid address: " + pos + " in record " + recordNum);
                        tempBuffer[pos + address] = hexVals[4 + pos];
                    }

                    if (checksum(hexVals) != hexVals[4 + dataLength])
                        throw new Exception("invalid checksum in record " + recordNum);
                }
                else
                {
                    /* what do? */
                }
            }

            byte[] buffer = new byte[maxAddress + 1 - minAddress];
            for (int pos = 0; pos < maxAddress + 1; pos++)
            {
                buffer[pos] = tempBuffer[minAddress + pos];
            }

            MemoryDump8Bit memDump = new MemoryDump8Bit();
            memDump.Data = buffer;
            memDump.StartAddress = (uint)minAddress;

            return memDump;
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
