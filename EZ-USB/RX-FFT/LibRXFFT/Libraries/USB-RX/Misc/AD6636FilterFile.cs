using System;
using System.Collections;
using System.IO;

namespace LibRXFFT.Libraries.USB_RX.Misc
{

    public class AD6636FilterFile : FilterInformation, IComparable
    {
        public string ProgramVersion;
        public string DeviceName;
        public string FileName;

        public long _Width;

        public long InputFrequency;
        public long _OutputFrequency;
        public long Decimation;
        public long PLLMult;
        public long PLLDiv;

        public bool FIR1;
        public bool FIR2;
        public bool HB1;
        public bool HB2;

        public bool CIC5;
        public long CIC5Scale;
        public long CIC5Decimation;
        public long CIC5Order;

        public bool CRCF;
        public bool CRCFSymmetric;
        public long CRCFDecimation;
        public long CRCFNTaps;
        public long[] CRCFTaps;

        public bool DRCF;
        public bool DRCFSymmetric;
        public long DRCFNTaps;
        public long DRCFDecimation;
        public long[] DRCFTaps;

        public bool MRCF;
        public bool MRCFSymmetric;
        public long MRCFNTaps;
        public long[] MRCFTaps;

        public bool LHB;

        public bool Valid = false;

        private ArrayList FileContent;

        internal class NoSuchFieldException : Exception
        {
            public NoSuchFieldException(string msg)
                : base(msg)
            {
            }
        }

        public AD6636FilterFile(String fileName)
        {
            FileName = fileName;
            try
            {
                TextReader reader = new StreamReader(fileName);
                FileContent = new ArrayList();

                try
                {
                    bool done = false;
                    while (!done)
                    {
                        String line = reader.ReadLine();
                        if (line != null)
                            FileContent.Add(line);
                        else
                            done = true;
                    }
                }
                catch (IOException e)
                {
                    FileContent = null;
                }
                finally
                {
                    reader.Close();
                }
            }
            catch (FileNotFoundException e)
            {
                FileContent = null;
            }

            try
            {
                ParseFile();
                Valid = true;
            }
            catch (NoSuchFieldException e)
            {
            }

        }

        public long Width
        {
            get
            {
                return _Width;
            }
        }

        public long Rate
        {
            get
            {
                return _OutputFrequency;
            }
        }

        public string Location
        {
            get
            {
                return FileName;
            }
        }

        public object SourceDevice
        {
            get { return null; }
        }

        private void ParseFile()
        {
            ProgramVersion = ReadSectionFieldString("Filter Design", "Version");
            DeviceName = ReadSectionFieldString("Device", "Name");

            _Width = 2 * ReadSectionFieldLong("Ideal Response", "Frequency");

            InputFrequency = (long)ReadSectionFieldDouble(DeviceName, "Input Frequency");
            _OutputFrequency = (long)ReadSectionFieldDouble(DeviceName, "Output Frequency");
            Decimation = (long)ReadSectionFieldDouble(DeviceName, "Decimation");
            PLLMult = ReadSectionFieldLong(DeviceName, "PLL Multiplier");
            PLLDiv = ReadSectionFieldLong(DeviceName, "PLL Divider");

            FIR1 = ReadSectionFieldBool("FIR 1 Filter", "Enabled");
            FIR2 = ReadSectionFieldBool("FIR 2 Filter", "Enabled");
            HB1 = ReadSectionFieldBool("HB 1 Filter", "Enabled");
            HB2 = ReadSectionFieldBool("HB 2 Filter", "Enabled");

            CIC5 = ReadSectionFieldBool("CIC5 Filter", "Enabled");
            CIC5Scale = ReadSectionFieldLong("CIC5 Filter", "Scale");
            CIC5Decimation = ReadSectionFieldLong("CIC5 Filter", "Decimation");
            CIC5Order = ReadSectionFieldLong("CIC5 Filter", "Order");

            CRCF = ReadSectionFieldBool("CRCF Filter", "Enabled");
            CRCFSymmetric = ReadSectionFieldBool("CRCF Filter", "Symmetric");
            CRCFDecimation = ReadSectionFieldLong("CRCF Filter", "Decimation");
            CRCFNTaps = ReadSectionFieldLong("CRCF Filter", "NTaps");
            CRCFTaps = ReadSectionFieldLongArray("CRCF Filter", CRCFNTaps);

            DRCF = ReadSectionFieldBool("DRCF Filter", "Enabled");
            DRCFSymmetric = ReadSectionFieldBool("DRCF Filter", "Symmetric");
            DRCFDecimation = ReadSectionFieldLong("DRCF Filter", "Decimation");
            DRCFNTaps = ReadSectionFieldLong("DRCF Filter", "NTaps");
            DRCFTaps = ReadSectionFieldLongArray("DRCF Filter", DRCFNTaps);

            MRCF = ReadSectionFieldBool("MRCF Filter", "Enabled");
            MRCFSymmetric = ReadSectionFieldBool("MRCF Filter", "Symmetric");
            MRCFNTaps = ReadSectionFieldLong("MRCF Filter", "NTaps");
            MRCFTaps = ReadSectionFieldLongArray("MRCF Filter", MRCFNTaps);

            LHB = ReadSectionFieldBool("LHB Filter", "Enabled");
        }

        private long[] ReadSectionFieldLongArray(String section, long taps)
        {
            ArrayList srcValues = new ArrayList();

            for (int pos = 0; pos < taps; pos++)
            {
                try
                {
                    long ret1 = ReadSectionFieldLong(section, "", pos);
                    if (ret1 != long.MinValue)
                    {
                        srcValues.Add(ret1);
                    }
                    else
                    {
                        long ret = ReadSectionFieldLong(section, (pos + 1).ToString());
                        srcValues.Add(ret);
                    }
                }
                catch (NoSuchFieldException ex)
                {
                    return null;
                }
            }

            long[] data = new long[srcValues.Count];

            int entry = 0;
            foreach (long value in srcValues)
                data[entry++] = value;


            return data;
        }

        private bool ReadSectionFieldBool(String section, String key)
        {
            return ReadSectionFieldBool(section, key, 0);
        }

        private bool ReadSectionFieldBool(String section, String key, int skips)
        {
            String ret = ReadSectionFieldString(section, key, skips);
            if (ret == null)
                throw new NoSuchFieldException("Could not find Field '" + key + "' in Section '" + section + "'");

            if ("True".Equals(ret))
                return true;
            if ("False".Equals(ret))
                return false;

            throw new NoSuchFieldException("Field '" + key + "' in Section '" + section + "' is '" + ret + "'. That is not True or False");
        }

        private long ReadSectionFieldLong(String section, String key)
        {
            return ReadSectionFieldLong(section, key, 0);
        }

        private long ReadSectionFieldLong(String section, String key, int skips)
        {
            String ret = ReadSectionFieldString(section, key, skips);
            if (ret == null)
            {
                return long.MinValue;
            }

            return long.Parse(ret);
        }

        private double ReadSectionFieldDouble(String section, String key)
        {
            return ReadSectionFieldDouble(section, key, 0);
        }

        private double ReadSectionFieldDouble(String section, String key, int skips)
        {
            String ret = ReadSectionFieldString(section, key, skips);
            if (ret == null)
                throw new NoSuchFieldException("Could not find Field '" + key + "' in Section '" + section + "'");

            return Double.Parse(ret.Replace('.', ','));

        }

        private String ReadSectionFieldString(String section, String key)
        {
            return ReadSectionFieldString(section, key, 0);
        }

        private String ReadSectionFieldString(String section, String key, int skips)
        {
            ArrayList sectionContent;

            sectionContent = ReadSection(section);
            if (section == null)
                throw new NoSuchFieldException("Could not find Section '" + section + "'");

            return ReadValue(sectionContent, key, skips);
        }

        private String ReadValue(ArrayList section, String key)
        {
            return ReadValue(section, key, 0);
        }

        private String ReadValue(ArrayList section, String key, int skips)
        {
            if (!key.Equals(""))
                key += "=";

            foreach (String line in section)
            {
                bool foundKey = line.Length >= key.Length && line.Substring(0, key.Length).Equals(key);

                /* if looking for keys without assignment (key == ""), skip all lines that contain a '=' */
                if (key.Equals("") && line.Contains("="))
                    foundKey = false;

                if (foundKey)
                {
                    if (skips > 0)
                        skips--;
                    else
                        return line.Substring(key.Length);
                }
            }

            return null;
        }

        private ArrayList ReadSection(String sectionName)
        {
            ArrayList section = new ArrayList();
            bool done = false;
            bool found = false;


            foreach (String line in FileContent)
            {
                if (line.Equals("[" + sectionName + "]"))
                    found = true;
                else if ("".Equals(line))
                    found = false;
                else if (found)
                    section.Add(line);
            }
            return section;
        }

        #region IComparable Member

        public int CompareTo(object obj)
        {
            if (obj.GetType() != typeof(AD6636FilterFile))
            {
                return -1;
            }
            AD6636FilterFile other = (AD6636FilterFile)obj;

            return (Width > other.Width) ? -1 : 1;
        }

        #endregion

    }
}
