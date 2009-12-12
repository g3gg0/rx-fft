using System;
using System.IO;
using System.Collections;

namespace LibRXFFT.Libraries.ShmemChain
{
    public enum eReadMode
    {
        Blocking = SharedMemNative.MODE_BLOCKING,
        Dynamic = SharedMemNative.MODE_BLOCKING_DYNAMIC,
        TimeLimited = SharedMemNative.MODE_BLOCKING_TIME,
        Partial = SharedMemNative.MODE_PARTIAL
    };

    public class NodeInfo
    {
        public int shmemID;
        public int srcChan;
        public int dstChan;
        public long bytesRead;
        public long bytesWritten;
        public int bufferSize;
        public int bufferUsed;
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
        protected int readTimeout = 100;


        public SharedMem()
        {
            this.srcChan = -1;
            this.dstChan = -1;
            shmemID = SharedMemNative.shmemchain_register_node(srcChan, dstChan);

            if (shmemID < 0)
                throw new NotSupportedException("Failed to register shmem node. Error code #" + SharedMemNative.shmemchain_get_last_error());
        }

        public SharedMem(string name)
        {
            this.srcChan = -1;
            this.dstChan = -1;

            byte[] dBytes = new byte[name.Length];
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            shmemID = SharedMemNative.shmemchain_register_node_special(srcChan, dstChan, defaultBufferSize, enc.GetBytes(name));

            if (shmemID < 0)
                throw new NotSupportedException("Failed to register shmem node. Error code #" + SharedMemNative.shmemchain_get_last_error());
        }

        public SharedMem(int srcChan, int dstChan)
        {
            this.srcChan = srcChan;
            this.dstChan = dstChan;
            shmemID = SharedMemNative.shmemchain_register_node(srcChan, dstChan);

            if (shmemID < 0)
                throw new NotSupportedException("Failed to register shmem node. Error code #" + SharedMemNative.shmemchain_get_last_error());
        }

        public SharedMem(int srcChan, int dstChan, string name)
        {
            this.srcChan = srcChan;
            this.dstChan = dstChan;

            byte[] dBytes = new byte[name.Length];
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            shmemID = SharedMemNative.shmemchain_register_node_special(srcChan, dstChan, defaultBufferSize, enc.GetBytes(name));

            if (shmemID < 0)
                throw new NotSupportedException("Failed to register shmem node. Error code #" + SharedMemNative.shmemchain_get_last_error());
        }

        public SharedMem(int srcChan, int dstChan, string name, int bufferSize)
        {
            this.srcChan = srcChan;
            this.dstChan = dstChan;

            byte[] dBytes = new byte[name.Length];
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            shmemID = SharedMemNative.shmemchain_register_node_special(srcChan, dstChan, bufferSize, enc.GetBytes(name));

            if (shmemID < 0)
                throw new NotSupportedException("Failed to register shmem node. Error code #" + SharedMemNative.shmemchain_get_last_error());
        }

        public static NodeInfo[] GetNodeInfos()
        {
            ArrayList nodes = new ArrayList();
            int[] nodeIds = new int[64];
            int used = SharedMemNative.shmemchain_get_all_nodes(nodeIds, nodeIds.Length);

            for (int pos = 0; pos < used; pos++)
            {
                byte[] name = new byte[SharedMemNative.MAX_NAME_LENGTH];
                long[] data = new long[7];

                SharedMemNative.shmemchain_get_infos(nodeIds[pos], name, data);

                int stringLength = 0;
                while (name[stringLength] != (byte)0)
                {
                    stringLength++;
                }

                NodeInfo info = new NodeInfo();
                info.name = new System.Text.ASCIIEncoding().GetString(name, 0, stringLength);
                info.shmemID = nodeIds[pos];
                info.srcChan = (int)data[0];
                info.dstChan = (int)data[1];
                info.bytesRead = data[2];
                info.bytesWritten = data[3];
                info.bufferSize = (int)data[4];
                info.bufferUsed = (int)data[5];
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

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readParam = (int)readMode;

            if (readMode == eReadMode.Dynamic || readMode == eReadMode.TimeLimited)
                readParam |= readTimeout;

            if (offset == 0)
                return (int)SharedMemNative.shmemchain_read_data(shmemID, buffer, (uint)count, readParam);

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
            if (offset == 0)
                SharedMemNative.shmemchain_write_data(shmemID, buffer, (uint)count);
            else
            {
                byte[] tmpBuffer = new byte[count];
                Array.Copy(buffer, offset, tmpBuffer, 0, count);
                SharedMemNative.shmemchain_write_data(shmemID, tmpBuffer, (uint)count);
            }
        }

        #endregion

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }
    }
}