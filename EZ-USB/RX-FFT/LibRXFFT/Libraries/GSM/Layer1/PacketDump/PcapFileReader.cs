//  Copyright: Erik Hjelmvik <hjelmvik@users.sourceforge.net>
//
//  NetworkMiner is free software; you can redistribute it and/or modify it
//  under the terms of the GNU General Public License
//
//  Contact Erik Hjelmvik if you wish to use NetworkMiner commersially
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace LibRXFFT.Libraries.GSM.Layer1.PacketDump
{

    //http://wiki.wireshark.org/Development/LibpcapFileFormat
    //http://www.winpcap.org/ntar/draft/PCAP-DumpFileFormat.html
    public class PcapFileReader : IDisposable
    {
        public delegate void EmptyDelegate();
        public delegate void ReadCompletedCallback(string filePathAndName, int framesCount, DateTime firstFrameTimestamp, DateTime lastFrameTimestamp);

        ~PcapFileReader()
        {
            //close the file stream here at least (instead of at the WorkerCompleted event)
            if (this.fileStream != null)
            {
                this.fileStream.Close();
                this.fileStream = null;
            }
            this.readCompletedCallback = null;
        }

        public int PercentRead
        {
            get
            {
                //the stream might be closed if we have read it through...
                return (int)(((this.fileStream.Position - this.PacketBytesInQueue) * 100) / this.fileStream.Length);
            }
        }

        public int PacketBytesInQueue
        {
            get { return this.enqueuedByteCount - this.dequeuedByteCount; }
        }

        public string Filename
        {
            get
            {
                return this.filename;
            }
        }

        public long PcapHeaderSize
        {
            get { return this.pcapHeaderSize; }
        }

        public long Position
        {
            get { return this.fileStream.Position; }
            set { this.fileStream.Position = value; }
        }



        private string filename;
        private System.IO.FileStream fileStream;
        private bool littleEndian;//is false if file format is Big endian
        private ushort majorVersionNumber;
        private ushort minorVersionNumber;
        private int timezoneOffsetSeconds;//GMT + 1:00 (Paris, Berlin, Stockholm) => -3600
        //ignore sigfigs (uint32)
        private uint maximumPacketSize;//snaplen
        private DataLinkType dataLinkType;

        private System.ComponentModel.BackgroundWorker backgroundFileReader;
        private System.Collections.Generic.Queue<PcapPacket> packetQueue;
        //private const int PACKET_QUEUE_SIZE=4000;
        private int packetQueueSize;
        private const int MAX_FRAME_SIZE = 131072;//Gigabit Ethernet Jumbo Frames are 9000 bytes (this is 15 times larger, so we should be safe)
        private int enqueuedByteCount;
        private int dequeuedByteCount;
        private long pcapHeaderSize;//number of bytes into the pcap where the packets start

        private ReadCompletedCallback readCompletedCallback;


        public DataLinkType FileDataLinkType { get { return this.dataLinkType; } }

        public PcapFileReader(string filename) : this(filename, 1000, null) { }
        public PcapFileReader(string filename, int packetQueueSize, ReadCompletedCallback captureCompleteCallback)
        {
            this.filename = filename;
            this.fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 262144, FileOptions.SequentialScan);

            this.packetQueueSize = packetQueueSize;
            this.readCompletedCallback = captureCompleteCallback;

            byte[] buffer4 = new byte[4];//32 bits is suitable
            byte[] buffer2 = new byte[2];//16 bits is sometimes needed
            uint wiresharkMagicNumber = 0xa1b2c3d4;

            //Section Header Block (mandatory)

            fileStream.Read(buffer4, 0, 4);

            if (wiresharkMagicNumber == this.ToUInt32(buffer4, false))
                this.littleEndian = false;
            else if (wiresharkMagicNumber == this.ToUInt32(buffer4, true))
                this.littleEndian = true;
            else
                throw new System.IO.InvalidDataException("The file " + filename + " is not a PCAP file. Magic number is " + this.ToUInt32(buffer4, false).ToString("X2") + " or " + this.ToUInt32(buffer4, true).ToString("X2") + " but should be " + wiresharkMagicNumber.ToString("X2") + ".");

            /* major version number */
            fileStream.Read(buffer2, 0, 2);
            this.majorVersionNumber = ToUInt16(buffer2, this.littleEndian);
            /* minor version number */
            fileStream.Read(buffer2, 0, 2);
            this.minorVersionNumber = ToUInt16(buffer2, this.littleEndian);
            /* GMT to local correction */
            fileStream.Read(buffer4, 0, 4);
            this.timezoneOffsetSeconds = (int)ToUInt32(buffer4, this.littleEndian);
            /* accuracy of timestamps */
            fileStream.Read(buffer4, 0, 4);
            /* max length of captured packets, in octets */
            fileStream.Read(buffer4, 0, 4);
            this.maximumPacketSize = ToUInt32(buffer4, this.littleEndian);
            /* data link type */
            fileStream.Read(buffer4, 0, 4);
            this.dataLinkType = (DataLinkType)ToUInt32(buffer4, this.littleEndian);

            this.pcapHeaderSize = fileStream.Position;

            this.backgroundFileReader = new System.ComponentModel.BackgroundWorker();
            this.packetQueue = new Queue<PcapPacket>(this.packetQueueSize);
            this.enqueuedByteCount = 0;
            this.dequeuedByteCount = 0;

            this.StartBackgroundWorkers();
        }

        public void StartBackgroundWorkers()
        {
            this.backgroundFileReader.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundFileReader_DoWork);
            this.backgroundFileReader.WorkerSupportsCancellation = true;
            this.backgroundFileReader.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundFileReader_RunWorkerCompleted);
            this.backgroundFileReader.RunWorkerAsync();
        }

        void backgroundFileReader_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //do some cleanup
            //this.fileStream.Close();//the file handle might be needed later on to see the position
            //this.packetQueue.Clear();
        }

        public void AbortFileRead()
        {
            this.backgroundFileReader.CancelAsync();
            this.packetQueue.Clear();
        }

        void backgroundFileReader_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            DateTime firstFrameTimestamp = DateTime.MinValue;
            DateTime lastFrameTimestamp = DateTime.MinValue;
            int framesCount = 0;
            try
            {
                while (!this.backgroundFileReader.CancellationPending && fileStream.Position + 1 < fileStream.Length)
                {
                    if (this.packetQueue.Count < this.packetQueueSize)
                    {
                        PcapPacket packet = ReadPcapPacket();
                        if (firstFrameTimestamp == DateTime.MinValue)
                            firstFrameTimestamp = packet.Timestamp;
                        lastFrameTimestamp = packet.Timestamp;
                        framesCount++;
                        lock (this.packetQueue)
                        {
                            this.packetQueue.Enqueue(packet);
                        }
                        this.enqueuedByteCount += packet.Data.Length;
                    }
                    else
                        System.Threading.Thread.Sleep(20);
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                e.Result = ex.Message;
                this.AbortFileRead();
            }
            //do a callback with this.filename as well as first and last timestamp
            if (this.readCompletedCallback != null && firstFrameTimestamp != DateTime.MinValue && lastFrameTimestamp != DateTime.MinValue)
                this.readCompletedCallback(this.filename, framesCount, firstFrameTimestamp, lastFrameTimestamp);
        }

        public IEnumerable<PcapPacket> PacketEnumerator()
        {
            return PacketEnumerator(null, null);
        }

        public IEnumerable<PcapPacket> PacketEnumerator(EmptyDelegate waitFunction, ReadCompletedCallback captureCompleteCallback)
        {

            while (!this.backgroundFileReader.CancellationPending && (this.backgroundFileReader.IsBusy || fileStream.Position + 1 < fileStream.Length || this.packetQueue.Count > 0))
            {
                //loops++;
                if (this.packetQueue.Count > 0)
                {
                    PcapPacket packet;
                    lock (this.packetQueue)
                    {
                        packet = this.packetQueue.Dequeue();
                    }
                    this.dequeuedByteCount += packet.Data.Length;
                    yield return packet;
                }
                else
                {
                    if (waitFunction == null)
                        System.Threading.Thread.Sleep(20);
                    else
                        waitFunction();
                }
            }

            //yield break;
        }

        public PcapPacket ReadPcapPacket()
        {
            byte[] buffer4 = new byte[4];//32 bits is suitable
            /* timestamp seconds */
            fileStream.Read(buffer4, 0, 4);
            long seconds = (long)ToUInt32(buffer4, this.littleEndian);/*seconds since January 1, 1970 00:00:00 GMT*/
            /* timestamp microseconds */
            fileStream.Read(buffer4, 0, 4);
            uint microseconds = ToUInt32(buffer4, this.littleEndian);
            /* number of octets of packet saved in file */
            fileStream.Read(buffer4, 0, 4);
            int bytesToRead = (int)ToUInt32(buffer4, this.littleEndian);
            if (bytesToRead > MAX_FRAME_SIZE)
                throw new Exception("Frame size is too large! Frame size = " + bytesToRead);
            else if (bytesToRead < 0)
                throw new Exception("Cannot read frames of negative sizes! Frame size = " + bytesToRead);
            /* actual length of packet */
            fileStream.Read(buffer4, 0, 4);

            byte[] data = new byte[bytesToRead];
            fileStream.Read(data, 0, bytesToRead);

            DateTime timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long tics = (seconds * 1000000 + microseconds) * 10;
            TimeSpan timespan = new TimeSpan(tics);

            return new PcapPacket(timestamp.Add(timespan), data);
        }



        private ushort ToUInt16(byte[] buffer, bool littleEndian)
        {
            if (littleEndian)
                return (ushort)(buffer[0] ^ buffer[1] << 8);
            else
                return (ushort)(buffer[0] << 8 ^ buffer[1]);
        }

        private uint ToUInt32(byte[] buffer, bool littleEndian)
        {
            if (littleEndian)
            {//swapped
                return (uint)(buffer[0] ^ buffer[1] << 8 ^ buffer[2] << 16 ^ buffer[3] << 24);
            }
            else//normal
                return (uint)(buffer[0] << 24 ^ buffer[1] << 16 ^ buffer[2] << 8 ^ buffer[3]);
        }



        #region IDisposable Members

        public void Dispose()
        {
            //throw new Exception("The method or operation is not implemented.");
            if (this.backgroundFileReader != null)
                this.backgroundFileReader.CancelAsync();
            if (this.fileStream != null)
            {
                this.fileStream.Close();
                this.fileStream = null;
            }
        }

        #endregion
    }
}


