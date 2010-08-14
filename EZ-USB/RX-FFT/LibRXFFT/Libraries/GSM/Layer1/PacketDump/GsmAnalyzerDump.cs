using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;

namespace LibRXFFT.Libraries.GSM.Layer1.PacketDump
{
    public class GsmAnalyzerDumpReader : PacketDumpReader
    {
        protected TextReader DumpFile = null;
        private bool RecordReady = false;

        private uint FN = 0;
        private uint TN = 0;
        private uint ARFCN = 0;        
        private byte[] Data = new byte[19];

        public GsmAnalyzerDumpReader(string fileName)
        {
            try
            {
                Stream readFile = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                DumpFile = new StreamReader(readFile);

                ReadNextRecord();
            }
            catch (Exception e)
            {
            }
        }

        void ReadNextRecord()
        {
            RecordReady = false;

            try
            {
                restart:

                string line = DumpFile.ReadLine();

                /* end of file */
                if (line == null)
                {
                    return;
                }

                if (!line.Trim().StartsWith("<b "))
                {
                    goto restart;
                }

                string[] fields = line.Replace("<b ", "").Replace("/>", "").Trim().Split(' ');

                foreach (string field in fields)
                {
                    bool fail = true;
                    string[] parts = field.Split('=');

                    if (parts.Length == 2)
                    {
                        string type = parts[0];
                        string data = parts[1].Trim('"');

                        switch (type)
                        {
                            case "a":
                                if (uint.TryParse(data, out ARFCN))
                                {
                                    fail = false;
                                }
                                break;
                            case "f":
                                if (uint.TryParse(data, out FN))
                                {
                                    fail = false;
                                }
                                break;

                            case "t":
                                if (uint.TryParse(data, out TN))
                                {
                                    fail = false;
                                }
                                break;

                            case "d":
                                if ("".Equals(data))
                                {
                                    Array.Copy(GsmAnalyzerDumpWriter.DummyData, Data, GsmAnalyzerDumpWriter.DummyData.Length);
                                    fail = false;
                                }
                                else if ((data.Length % 2) == 0)
                                {
                                    for (int pos = 0; pos < data.Length / 2; pos++)
                                    {
                                        string byteString = data.Substring(pos * 2, 2);
                                        if (byte.TryParse(byteString, System.Globalization.NumberStyles.HexNumber, null, out Data[pos]))
                                        {
                                            fail = false;
                                        }
                                    }
                                }
                                break;
                        }
                    }

                    if (fail)
                    {
                        goto restart;
                    }
                }

                RecordReady = true;
            }
            catch (Exception e)
            {
            }
        }

        #region PacketDumpReader Member

        public bool HasData
        {
            get { return RecordReady; }
        }

        public void Read(GSMParameters Parameters, bool[] BurstBitsUndiffed)
        {
            if (DumpFile != null)
            {
                Parameters.FN = FN;
                Parameters.TN = TN;
                Parameters.ARFCN = ARFCN;

                ByteUtil.BitsFromBytes(Data, BurstBitsUndiffed, 8, 0, 148);

                ReadNextRecord();
            }
        }

        public void Close()
        {
            if (DumpFile != null)
            {
                DumpFile.Close();
                DumpFile = null;
            }
        }

        #endregion
    }

    public class GsmAnalyzerDumpWriter : PacketDumpWriter
    {
        internal static byte[] DummyData = new byte[] { 0x1F, 0x6E, 0xC1, 0x49, 0xC1, 0x22, 0x03, 0xE3, 0x8B, 0x8B, 0x8A, 0xE9, 0x46, 0x67, 0x3D, 0x3E, 0x25, 0xF5, 0x00 };

        protected TextWriter DumpFile = null;
        protected byte[] BurstBufferD = new byte[19];
        protected StringBuilder OutputBuilder = new StringBuilder(80);


        public GsmAnalyzerDumpWriter(string fileName)
        {
            try
            {
                Stream writeFile = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                DumpFile = new StreamWriter(writeFile);
            }
            catch (Exception e)
            {
            }
        }

        #region GSMPacketDumper Member

        public void Write(GSMParameters param, bool[] burstBits)
        {
            if (DumpFile == null || param.State != eGSMState.Lock)
            {
                return;
            }

            lock (this)
            {
                OutputBuilder.Length = 0;
                OutputBuilder.AppendFormat("<b a=\"{0}\" f=\"{1}\" t=\"{2}\" d=\"", param.ARFCN, param.FN, param.TN);

                ByteUtil.BitsToBytes(burstBits, BurstBufferD, 8, 0, 148);

                if (!BurstBufferD.SequenceEqual(DummyData))
                {
                    for (int pos = 0; pos < BurstBufferD.Length; pos++)
                    {
                        OutputBuilder.AppendFormat("{0:X02}", BurstBufferD[pos]);
                    }
                }

                OutputBuilder.Append("\"/>");

                DumpFile.WriteLine(OutputBuilder);
            }
        }

        public void Close()
        {
            lock (this)
            {
                if (DumpFile != null)
                {
                    DumpFile.Close();
                    DumpFile = null;
                }
            }
        }

        #endregion
    }
}
