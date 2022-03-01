using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using RX_FFT.Components.GDI;

namespace LibRXFFT.Libraries.ShmemChain
{
    public enum eReadMode
    {
        Blocking = SharedMemNative.MODE_BLOCKING,
        Dynamic = SharedMemNative.MODE_BLOCKING_DYNAMIC,
        TimeLimited = SharedMemNative.MODE_BLOCKING_TIME,
        TimeLimitedNoPartial = SharedMemNative.MODE_BLOCKING_TIME_NOPARTIAL,        
        Partial = SharedMemNative.MODE_PARTIAL
    }

    public class NodeInfo
    {
        public int shmemID;
        public int srcChan;
        public int dstChan;
        public ulong bytesRead;
        public ulong bytesWritten;
        public uint bufferSize;
        public uint bufferUsed;
        public int pct;
        public string name;
    }

    public class SharedMem : Stream
    {
        protected const int defaultBufferSize = 8192 * 1024;
        protected int shmemID;
        protected int srcChan;
        protected int dstChan;
        protected eReadMode readMode = eReadMode.Blocking;
        protected int readTimeout = 1000;
        public long bufferSize;
        public string name;
        public bool Failed = false;

        public SharedMem() : this("") {}

        public SharedMem(string name) : this(-1, -1, name){}

        public SharedMem(int srcChan, int dstChan) : this(srcChan, dstChan, "") { }

        public SharedMem(int srcChan, int dstChan, string name) : this(srcChan, dstChan, name, defaultBufferSize) { }

        public SharedMem(int srcChan, int dstChan, string name, long bufferSize)
        {
            if (name == null || name == "")
            {
                name = "Unnamed Channel";
            }

            this.name = name;
            this.srcChan = srcChan;
            this.dstChan = dstChan;
            this.bufferSize = bufferSize;
            
            byte[] dBytes = new byte[name.Length];
            ASCIIEncoding enc = new ASCIIEncoding();

            shmemID = SharedMemNative.shmemchain_register_node_special(srcChan, dstChan, (int)bufferSize, enc.GetBytes(name));

            if (shmemID < 0)
            {
                throw new NotSupportedException("Failed to register shmem node. Error code #" + SharedMemNative.shmemchain_get_last_error() + " #" + SharedMemNative.shmemchain_get_last_errorcode());
            }

            /* try to get the real channel ids */
            foreach (NodeInfo info in GetNodeInfos())
            {
                if (info.shmemID== shmemID)
                {
                    this.srcChan = info.srcChan;
                    this.dstChan = info.dstChan;
                }
            }
        }

        public static NodeInfo[] GetNodeInfos()
        {
            ArrayList nodes = new ArrayList();
            int[] nodeIds = new int[512];
            int used = SharedMemNative.shmemchain_get_all_nodes(nodeIds, nodeIds.Length);

            for (int pos = 0; pos < used; pos++)
            {
                byte[] name = new byte[SharedMemNative.MAX_NAME_LENGTH];
                ulong[] data = new ulong[7];

                SharedMemNative.shmemchain_get_infos(nodeIds[pos], name, data);

                int stringLength = 0;
                while (name[stringLength] != (byte)0)
                {
                    stringLength++;
                }

                NodeInfo info = new NodeInfo();
                info.name = new ASCIIEncoding().GetString(name, 0, stringLength);
                info.shmemID = nodeIds[pos];
                info.srcChan = (int)data[0];
                info.dstChan = (int)data[1];
                info.bytesRead = data[2];
                info.bytesWritten = data[3];
                info.bufferSize = (uint)data[4];
                info.bufferUsed = (uint)data[5];
                info.pct = (int)data[6];

                nodes.Add(info);
            }

            return (NodeInfo[])nodes.ToArray(typeof(NodeInfo));
        }

        public int SrcChan
        {
            get { return srcChan; }
            set
            {
                this.srcChan = value;
                SharedMemNative.shmemchain_update_node(shmemID, srcChan, dstChan);
            }
        }

        public int DstChan
        {
            get { return dstChan; }
            set
            {
                this.dstChan = value;
                SharedMemNative.shmemchain_update_node(shmemID, srcChan, dstChan);
            }
        }

        public int BlockSize
        {
            get { return SharedMemNative.shmemchain_get_blocksize(shmemID); }
            set { SharedMemNative.shmemchain_set_blocksize(shmemID, value); }
        }

        public long Rate
        {
            get { return SharedMemNative.shmemchain_get_rate(shmemID); }
            set { SharedMemNative.shmemchain_set_rate(shmemID, value); }
        }

        public eReadMode ReadMode 
        {
            get { return readMode; }
            set { readMode = value; }
        }

        public override int ReadTimeout
        {
            get { return readTimeout; }
            set { readTimeout = value; }
        }


        public int Unregister()
        {
            return SharedMemNative.shmemchain_unregister_node(shmemID);
        }


        #region Stream_Interface

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            SharedMemNative.shmemchain_read_data(shmemID, null, 0, SharedMemNative.MODE_FLUSH);
        }

        public override long Length
        {
            get
            {
                return SharedMemNative.shmemchain_read_data(shmemID, null, 0, SharedMemNative.MODE_GET_AVAIL);
            }
        }

        public override void Close()
        {
            Unregister();
            base.Close();
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            int readParam = (int)readMode;

            if (SharedMemNative.shmemchain_get_last_error() != -100)
            {
                Log.AddMessage("Shmem Error: " + SharedMemNative.shmemchain_get_last_error() + " " + SharedMemNative.shmemchain_get_last_errorcode());
            }

            /* dynamic and time limited modes have the timeout in 100ms steps as parameter */
            if (readMode == eReadMode.Dynamic || readMode == eReadMode.TimeLimited || readMode == eReadMode.TimeLimitedNoPartial)
            {
                readParam |= ((readTimeout + 99) / 100);
            }

            /* directly read into buffer */
            if (offset == 0)
            {
                return (int)SharedMemNative.shmemchain_read_data(shmemID, buffer, (uint)count, readParam);
            }

            /* use a temporary buffer for writing at offset */
            byte[] tmpBuffer = new byte[count];
            int ret = (int)SharedMemNative.shmemchain_read_data(shmemID, tmpBuffer, (uint)count, readParam);

            Array.Copy(tmpBuffer, 0, buffer, offset, count);
            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Failed |= SharedMemNative.shmemchain_write_data_ex(shmemID, buffer, (uint)offset, (uint)count) < 0;
        }

        #endregion

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }
    }
}