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

        /* fill with invalid values */
        private long FN = -1;
        private long TN = -1;
        private long ARFCN = -1;

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
            long fn = -1;
            long tn = -1;

            try
            {
            /* yes, we are using gotos :) */
            restart:

                string line = DumpFile.ReadLine();

                /* end of file */
                if (line == null)
                {
                    return;
                }

                /* skip empty lines */
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
                                if (long.TryParse(data, out ARFCN))
                                {
                                    fail = false;
                                }
                                break;

                            case "f":
                                if (long.TryParse(data, out fn))
                                {
                                    fail = false;
                                }
                                break;

                            case "t":
                                if (long.TryParse(data, out tn))
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

                    /* for a cleaner error handling */
                    if (fail)
                    {
                        goto restart;
                    }
                }

                /* did the entry have a TN? */
                if (tn != -1)
                {
                    /* timeslot 7->0 wrap */
                    if (TN >= tn)
                    {
                        /* then update FN */
                        FN++;
                    }
                    /* assign read value */
                    TN = tn;
                }
                else
                {
                    /* no TN given, just increase by one */
                    TN++;
                    TN %= 8;

                    /* timeslot 7->0 wrap */
                    if (TN == 0)
                    {
                        /* then update FN */
                        FN++;
                    }
                }

                
                /* FN given by dump? */
                if (fn != -1)
                {
                    /* this is the valid one */
                    FN = fn;
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

        public void Read(GSMParameters param, bool[] bits)
        {
            lock (this)
            {
                if (DumpFile != null)
                {
                    long deltaSlots = (FN - param.FN) * 8 + (TN - param.TN);

                    /* does the analyzer want the next burst or are we too far away? (need synchronization) */
                    if (deltaSlots <= 1 || deltaSlots > 10)
                    {
                        /* give him the actual data */
                        param.FN = FN;
                        param.TN = TN;
                        param.ARFCN = ARFCN;

                        ByteUtil.BitsFromBytes(Data, bits, 8, 0, 148);

                        ReadNextRecord();
                    }
                    else
                    {
                        /* fake some dummy bursts here. those were omitted in the dump. */
                        ByteUtil.BitsFromBytes(GsmAnalyzerDumpWriter.DummyData, bits, 8, 0, 148);

                        /* also update TN and such */
                        param.TN++;
                        param.TN %= 8;
                        if (param.TN == 0)
                        {
                            param.FN++;
                        }
                    }
                }
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

    public class GsmAnalyzerDumpWriter : PacketDumpWriter
    {
        internal static byte[] DummyData = new byte[] { 0x1F, 0x6E, 0xC1, 0x49, 0xC1, 0x22, 0x03, 0xE3, 0x8B, 0x8B, 0x8A, 0xE9, 0x46, 0x67, 0x3D, 0x3E, 0x25, 0xF5, 0x00 };

        /* omit redundant data in the dump */
        public bool StripOutput = true;

        protected TextWriter DumpFile = null;
        protected byte[] BurstBufferD = new byte[19];
        protected StringBuilder OutputBuilder = new StringBuilder(80);

        /* fill with invalid values by default */
        private long FN = -1;
        private long TN = -1;
        private long ARFCN = -1;
        private int SkippedBursts = 0;

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

        public void WriteL2Data(GSMParameters param, byte[] l2Data)
        {
        }

        public void WriteRawBurst(GSMParameters param, bool[] burstBits)
        {
            bool dummyBurst = false;
            bool skippable = true;

            lock (this)
            {
                /* if there is no file opened, return */
                if (DumpFile == null || param.State != eGSMState.Lock)
                {
                    return;
                }

                long deltaSlots = (param.FN - FN) * 8 + (param.TN - TN);
                if (deltaSlots != 1)
                {
                    /* hmm maybe demodulator resynchronized. force an entry. */
                    FN = -1;
                    TN = -1;
                    ARFCN = -1;
                    skippable = false;
                }

                OutputBuilder.Length = 0;
                OutputBuilder.Append("<b d=\"");

                /* convert bits to bytes for writing into output */
                ByteUtil.BitsToBytes(burstBits, BurstBufferD, 8, 0, 148);
                dummyBurst = BurstBufferD.SequenceEqual(DummyData);

                /* dummy bursts are skipped */
                if (!dummyBurst)
                {
                    skippable = false;
                    for (int pos = 0; pos < BurstBufferD.Length; pos++)
                    {
                        OutputBuilder.AppendFormat("{0:X02}", BurstBufferD[pos]);
                    }
                }

                OutputBuilder.Append("\"");

                /* output ARFCN if it has changed */
                if (!StripOutput || (ARFCN != param.ARFCN))
                {
                    skippable = false;
                    OutputBuilder.AppendFormat(" a=\"{0}\"", param.ARFCN);
                }

                /* 
                 * output FN if a) it has changed and b) it is not an obvious FN change due to TN overflow.
                 * c) also output FN every 10 frames for better debug-ability.
                 */
                bool updatedFN = (FN != param.FN);
                bool obviousFNchange = ((FN + 1) == param.FN) && (TN > param.TN);
                bool cyclicFNOutput = (((param.FN % 10) == 0) && param.TN == 0);

                if (!StripOutput || (updatedFN && (!obviousFNchange || cyclicFNOutput)))
                {
                    skippable = false;
                    OutputBuilder.AppendFormat(" f=\"{0}\"", param.FN);
                }
                else if (SkippedBursts > 7)
                {
                    /* in case there were skipped more than 7 bursts, append FN on the next bursts. (will not happen due to TN=0 check below) */
                    OutputBuilder.AppendFormat(" f=\"{0}\"", param.FN);
                }

                /* output TN if is not a simple increase. we will output TN=0 always */
                if (!StripOutput || ((TN + 1) != param.TN))
                {
                    skippable = false;
                    OutputBuilder.AppendFormat(" t=\"{0}\"", param.TN);
                }
                else if (SkippedBursts > 0)
                {
                    /* if there were bursts skipped previously, append TN */
                    OutputBuilder.AppendFormat(" t=\"{0}\"", param.TN);
                }

                /* remember the last burst parameters */
                ARFCN = param.ARFCN;
                FN = param.FN;
                TN = param.TN;

                OutputBuilder.Append("/>");

                if (!StripOutput || !skippable)
                {
                    SkippedBursts = 0;
                    DumpFile.WriteLine(OutputBuilder);
                }
                else
                {
                    SkippedBursts++;
                }
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
