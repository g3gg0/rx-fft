using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibRXFFT.Libraries.GSM.Layer1.Bursts;
using RX_FFT.Components.GDI;
using System.Threading;

namespace LibRXFFT.Libraries.GSM.Layer1.PacketDump
{
    public class GsmAnalyzerDumpReader : PacketDumpReader
    {
        /* allow a maximum of e.g. 100 frames distance between <b...> data, then resync */
        protected long DeltaFrameCount = 10000;

        protected Stream DumpFile = null;
        protected TextReader DumpStream = null;
        protected GSMParameters Parameters = null;
        private bool RecordReady = false;
        protected string DumpFileName = "";

        /* fill with invalid values */
        private long FN = -1;
        private long TN = -1;
        private long ARFCN = -1;

        private byte[] DataDown = new byte[19];
        private byte[] DataUp = new byte[19];

        public ulong BurstNumber = 0;

        private ulong StartAtBurstNumber = 0;
        private ulong EndAtBurstNumber = ulong.MaxValue;
        private bool StartFound = false;

        private bool FileIsBeingUpdated = false;
        private bool[] DummyBits = new bool[148];
        

        public GsmAnalyzerDumpReader(GSMParameters param, string fileName)
        {
            DumpFileName = fileName;
            Parameters = param;

            ByteUtil.BitsFromBytes(GsmAnalyzerDumpWriter.DummyData, DummyBits, 8, 0, 148);

            try
            {
                FileIsBeingUpdated = IsBeingUpdated();

                DumpFile = File.Open(DumpFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                DumpStream = new StreamReader(DumpFile);

                ReadNextRecord();
            }
            catch (Exception e)
            {
            }
        }

        private bool IsBeingUpdated()
        {            
            bool retVal = false;
            try
            {
                using (FileStream stream = new FileStream(DumpFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    try
                    {
                        stream.ReadByte();
                    }
                    catch (IOException)
                    {
                        retVal = true;
                    }
                    finally
                    {
                        stream.Close(); 
                        stream.Dispose();
                    }
                }
            }
            catch (IOException e)
            {
                //file is opened at another location 
                retVal = true;
            }
            catch (UnauthorizedAccessException e)
            {
                //Bypass this exception since this is due to the file is being set to read-only 
            }

            return retVal;
        }
        //Note that the FileAccess parameter is set to 'FileAccess.ReadWrite' and bypassing the 'UnauthorizedAccessException' exception.

        void ReadNextRecord()
        {
            RecordReady = false;
            long fn = -1;
            long tn = -1;

            try
            {
            /* yes, we are using gotos :) */
            restart:

                if (FileIsBeingUpdated)
                {
                    /* wait for a new record */
                    while (DumpFile.Length - DumpFile.Position < 1024)
                        Thread.Sleep(100);
                }

                string line = DumpStream.ReadLine();

                /* end of file */
                if (line == null)
                {
                    if (FileIsBeingUpdated)
                    {
                        /* wait for a new record */
                        while (DumpFile.Length - DumpFile.Position < 1024)
                            Thread.Sleep(100);
                        goto restart;
                    }
                    else
                    {
                        return;
                    }
                }

                /* check for metadata */
                if (line.Trim().StartsWith("<meta "))
                {
                    string[] fields = line.Replace("<meta ", "").Replace("/>", "").Trim().Split(' ');

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
                                case "kc":
                                    if (data.Length == 16)
                                    {
                                        byte[] key = new byte[8];

                                        for (int pos = 0; pos < data.Length / 2; pos++)
                                        {
                                            string byteString = data.Substring(pos * 2, 2);
                                            if (byte.TryParse(byteString, System.Globalization.NumberStyles.HexNumber, null, out key[pos]))
                                            {
                                                fail = false;
                                            }
                                        }

                                        if (!fail)
                                        {
                                            Parameters.AddA5Key(key);
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

                    /* read next line */
                    goto restart;
                }
                else if (line.Trim().StartsWith("<b "))
                {
                    string fieldString = line.Replace("<b ", "").Replace("/>", "").Trim();
                    int fieldPos = 0;
                    bool hasUp = false;
                    bool hasDown = false;

                    while (fieldPos < fieldString.Length)
                    {
                        bool fail = true;

                        /* split while allowing spaces in quotes */
                        int typeEnd = fieldString.IndexOf('=', fieldPos);

                        string type = fieldString.Substring(fieldPos, typeEnd - fieldPos).Trim();
                        int dataStart = fieldString.IndexOf('"', typeEnd + 1) + 1;
                        int dataEnd = fieldString.IndexOf('"', dataStart);
                        string data = "";

                        if (dataStart < 0 || dataEnd < 0)
                        {
                            goto restart;
                        }

                        fieldPos = dataEnd + 1;

                        if (dataStart >= 0 && dataEnd >= 0 && dataStart <= dataEnd)
                        {
                            data = fieldString.Substring(dataStart, dataEnd - dataStart);
                        }

                        switch (type)
                        {
                            case "time":
                                DateTime.TryParse(data, out Parameters.TimeStamp);
                                fail = false;
                                break;

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
                                    Array.Copy(GsmAnalyzerDumpWriter.DummyData, DataDown, GsmAnalyzerDumpWriter.DummyData.Length);
                                    fail = false;
                                }
                                else if ((data.Length % 2) == 0)
                                {
                                    for (int pos = 0; pos < data.Length / 2; pos++)
                                    {
                                        string byteString = data.Substring(pos * 2, 2);
                                        if (byte.TryParse(byteString, System.Globalization.NumberStyles.HexNumber, null, out DataDown[pos]))
                                        {
                                            fail = false;
                                            hasDown = true;
                                        }
                                    }
                                }
                                break;

                            case "u":
                                if ("".Equals(data))
                                {
                                    Array.Copy(GsmAnalyzerDumpWriter.DummyData, DataUp, GsmAnalyzerDumpWriter.DummyData.Length);
                                    fail = false;
                                }
                                else if ((data.Length % 2) == 0)
                                {
                                    for (int pos = 0; pos < data.Length / 2; pos++)
                                    {
                                        string byteString = data.Substring(pos * 2, 2);
                                        if (byte.TryParse(byteString, System.Globalization.NumberStyles.HexNumber, null, out DataUp[pos]))
                                        {
                                            fail = false;
                                            hasUp = true;
                                        }
                                    }
                                }
                                break;
                        }

                        /* for a cleaner error handling */
                        if (fail)
                        {
                            goto restart;
                        }
                    }

                    /* make sure there are always dummy bursts in nonexistent bursts */
                    if (!hasDown)
                    {
                        Array.Copy(GsmAnalyzerDumpWriter.DummyData, DataDown, DataDown.Length);
                    }
                    if (!hasUp)
                    {
                        Array.Copy(GsmAnalyzerDumpWriter.DummyData, DataUp, DataUp.Length);
                    }

                    BurstNumber++;

                    /* did the entry have a TN? */
                    if (tn != -1)
                    {
                        /* timeslot 7->0 wrap */
                        if (TN >= tn)
                        {
                            /* then update FN, but may get overwritten again if given by dump */
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
                            /* then update FN, but may get overwritten again if given by dump */
                            FN++;
                        }
                    }


                    /* FN given by dump? */
                    if (fn != -1)
                    {
                        /* this is the valid one */
                        FN = fn;
                    }

                    if (BurstNumber <= StartAtBurstNumber)
                    {
                        goto restart;
                    }

                    if (BurstNumber > EndAtBurstNumber)
                    {
                        return;
                    }

                    RecordReady = true;
                }
                else
                {
                    goto restart;
                }
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

        public void Read(bool[] bits)
        {
            Read(bits, null);
        }

        public void Read(bool[] bitsDownlink, bool[] bitsUplink)
        {
            lock (this)
            {
                if (DumpStream != null)
                {
                    long deltaSlots = (FN - Parameters.FN) * 8 + (TN - Parameters.TN);

                    /* does the analyzer want the next burst or are we too far away? (need synchronization) */
                    if (deltaSlots <= 1 || deltaSlots > (DeltaFrameCount * 8))
                    {
                        /* give him the actual data */
                        Parameters.FN = FN;
                        Parameters.TN = TN;

                        /* do not set ARFCN if we are not sure about its value */
                        if (ARFCN != -1)
                        {
                            Parameters.ARFCN = ARFCN;
                        }

                        ByteUtil.BitsFromBytes(DataDown, bitsDownlink, 8, 0, 148);
                        if (bitsUplink != null)
                        {
                            ByteUtil.BitsFromBytes(DataUp, bitsUplink, 8, 0, 148);
                        }

                        ReadNextRecord();
                    }
                    else
                    {
                        /* fake some dummy bursts here. those were omitted in the dump. */
                        Array.Copy(DummyBits, bitsDownlink, 148);
                        Array.Copy(DummyBits, bitsUplink, 148);

                        /* also update TN and such */
                        Parameters.TN++;
                        Parameters.TN %= 8;
                        if (Parameters.TN == 0)
                        {
                            Parameters.FN++;
                        }
                    }
                }
            }
        }

        public void Close()
        {
            lock (this)
            {
                if (DumpStream != null)
                {
                    DumpStream.Close();
                    DumpStream = null;
                }
            }
        }

        public double Progress
        {
            get 
            {
                double ret = ((double)DumpFile.Position) / DumpFile.Length;

                return ret;
            }
        }

        #endregion


    }

    public class GsmAnalyzerDumpWriter : PacketDumpWriter
    {
        internal static byte[] DummyData = new byte[] { 0x1F, 0x6E, 0xC1, 0x49, 0xC1, 0x22, 0x03, 0xE3, 0x8B, 0x8B, 0x8A, 0xE9, 0x46, 0x67, 0x3D, 0x3E, 0x25, 0xF5, 0x00 };

        /* omit redundant data in the dump */
        public bool StripOutput = true;
        public double TimeTagEvery = 2; /* seconds */

        protected string FileName = "";
        protected TextWriter DumpFile = null;
        protected Stream WriteFile = null;
        protected GSMParameters Parameters = null;
        protected byte[] BurstBufferD = new byte[19];
        protected StringBuilder OutputBuilder = new StringBuilder(80);
        protected DateTime StartTime;
        protected DateTime LastTaggedTime;
        protected long BurstCount = 0;
        protected long BurstEntries = 0;

        /* fill with invalid values by default */
        private long FN = -1;
        private long TN = -1;
        private long ARFCN = -1;
        private int SkippedBursts = 0;

        public GsmAnalyzerDumpWriter(GSMParameters param, string fileName)
        {
            FileName = fileName;
            Parameters = param;
            StartTime = DateTime.Now;
            LastTaggedTime = DateTime.Now;

            try
            {
                WriteFile = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                DumpFile = new StreamWriter(WriteFile);
            }
            catch (Exception e)
            {
            }
        }

        private void WriteHeader(StreamWriter writer)
        {
            DateTime now = DateTime.Now;
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("");
            builder.AppendLine("<!--     GSM Analyzer burst dump     -->");
            builder.AppendLine("<!-- ------------------------------- -->");
            builder.AppendLine("<!--  Created on     : " + StartTime.ToShortDateString() + " " + StartTime.ToShortTimeString() + "  -->");
            builder.AppendLine("<!--  Finished on    : " + now.ToShortDateString() + " " + now.ToShortTimeString() + "  -->");
            builder.AppendLine("<!--  Total Bursts   : " + BurstCount.ToString("00000000000") + "       -->");
            builder.AppendLine("<!--  Written Bursts : " + BurstEntries.ToString("00000000000") + "       -->");
            builder.AppendLine("<!--  ARFCN          : " + Parameters.MCC.ToString("0000") + "              -->");
            builder.AppendLine("<!--  MCC            : " + Parameters.MCC.ToString("000") + "               -->");
            builder.AppendLine("<!--  MNC            : " + Parameters.MNC.ToString("000") + "               -->");
            builder.AppendLine("<!--  LAC            : " + Parameters.LAC.ToString("000000") + "            -->");
            builder.AppendLine("<!--  CellIdent      : " + Parameters.CellIdent.ToString("000000") + "            -->");

            builder.AppendLine("");
            lock (Parameters.A5KeyStore)
            {
                foreach (byte[] key in Parameters.A5KeyStore)
                {
                    builder.Append("<meta kc=\"");
                    for (int pos = 0; pos < key.Length; pos++)
                    {
                        builder.Append(string.Format("{0:X02}", key[pos]));
                    }
                    builder.Append("\"/>" + Environment.NewLine);
                }
            }
            builder.Append("");

            writer.WriteLine(builder.ToString());
        }

        #region GSMPacketDumper Member

        public void WriteL2Data(byte[] l2Data)
        {
        }

        public void WriteRawBurst(bool[] burstBits)
        {
            bool dummyBurst = false;
            bool skippable = true;

            BurstCount++;

            lock (this)
            {
                /* if there is no file opened, return */
                if (DumpFile == null || Parameters.State != eGSMState.Lock)
                {
                    return;
                }

                long deltaSlots = (Parameters.FN - FN) * 8 + (Parameters.TN - TN);
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
                if (!StripOutput || (ARFCN != Parameters.ARFCN))
                {
                    skippable = false;
                    OutputBuilder.AppendFormat(" a=\"{0}\"", Parameters.ARFCN);
                }

                /* 
                 * output FN if a) it has changed and b) it is not an obvious FN change due to TN overflow.
                 * c) also output FN every 10 frames for better debug-ability.
                 */
                bool updatedFN = (FN != Parameters.FN);
                bool obviousFNchange = ((FN + 1) == Parameters.FN) && (TN > Parameters.TN);
                bool cyclicFNOutput = (((Parameters.FN % 10) == 0) && Parameters.TN == 0);

                if (!StripOutput || (updatedFN && (!obviousFNchange || cyclicFNOutput)))
                {
                    skippable = false;
                    OutputBuilder.AppendFormat(" f=\"{0}\"", Parameters.FN);
                }
                else if (SkippedBursts > 7)
                {
                    /* in case there were skipped more than 7 bursts, append FN on the next bursts. (will not happen due to TN=0 check below) */
                    OutputBuilder.AppendFormat(" f=\"{0}\"", Parameters.FN);
                }

                /* output TN if is not a simple increase. we will output TN=0 always */
                if (!StripOutput || ((TN + 1) != Parameters.TN))
                {
                    skippable = false;
                    OutputBuilder.AppendFormat(" t=\"{0}\"", Parameters.TN);
                }
                else if (SkippedBursts > 0)
                {
                    /* if there were bursts skipped previously, append TN */
                    OutputBuilder.AppendFormat(" t=\"{0}\"", Parameters.TN);
                }

                /* remember the last burst parameters */
                ARFCN = Parameters.ARFCN;
                FN = Parameters.FN;
                TN = Parameters.TN;

                if (!StripOutput || !skippable)
                {
                    /* only write time tag if the burst isnt skipped */
                    DateTime now = DateTime.Now;
                    bool updatedTime = ((now - LastTaggedTime).TotalSeconds > TimeTagEvery);

                    if (updatedTime)
                    {
                        LastTaggedTime = now;
                        OutputBuilder.AppendFormat(" time=\"{0}\"", LastTaggedTime.ToString());
                    }

                    OutputBuilder.Append("/>");

                    BurstEntries++;
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

                    try
                    {
                        /* read the file content and write with header into a temporary file */
                        FileStream readfile = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        FileStream writefile = File.Open(FileName + ".tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                        StreamReader reader = new StreamReader(readfile);
                        StreamWriter writer = new StreamWriter(writefile);

                        WriteHeader(writer);
                        while (!reader.EndOfStream)
                        {
                            writer.WriteLine(reader.ReadLine());
                        }
                        reader.Close();
                        writer.Close();

                        /* now write that back to the original file */
                        readfile = File.Open(FileName + ".tmp", FileMode.Open, FileAccess.Read, FileShare.None);
                        writefile = File.Open(FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        reader = new StreamReader(readfile);
                        writer = new StreamWriter(writefile);

                        while (!reader.EndOfStream)
                        {
                            writer.WriteLine(reader.ReadLine());
                        }
                        reader.Close();
                        writer.Close();

                        /* delete temporary file */
                        File.Delete(FileName + ".tmp");
                    }
                    catch (Exception e)
                    {
                        Log.AddMessage("Exception while rewriting log: " + e.GetType().ToString());
                    }
                }
            }
        }

        #endregion
    }
}
