using System.Runtime.InteropServices;

namespace LibRXFFT.Libraries.ShmemChain
{
    public class SharedMemNative
    {
        public const int DEFAULT_BUFFER_SIZE = 8 * 1024 * 1024;

        public const int MODE_PARTIAL = 0;			// try to read up to n bytes (or less or zero)
        public const int MODE_NOPARTIAL = 1;			// try to read n bytes (or zero)
        public const int MODE_BLOCKING = 2;			// read n bytes (wait until we read them)
        public const int MODE_FLUSH = 3;			// flush buffers and return (no read made)
        public const int MODE_GET_AVAIL = 4;			// just get how much is available 
        public const int MODE_BLOCKING_TIME = 0x40000000;	// read n bytes (wait up to x*100ms until read completely)
        public const int MODE_BLOCKING_TIME_NOPARTIAL = 0x20000000;	// read n bytes (wait up to x*100ms or read completely)
        public const int MODE_BLOCKING_DYNAMIC = 0x10000000;	// used with MODE_BLOCKING_TIME (wait up to x*100ms in case of no data)

        public const int MODE_CRITICAL = 0x20000000;	// less sleep delay (obsolete!)
        public const int MODE_MASK = 0x000000FF;  // internal

        public const int MAX_NAME_LENGTH = 128;

        [DllImport("shmemchain.dll",CallingConvention=CallingConvention.StdCall)]
        public static extern int shmemchain_register_node(int src_chan, int dst_chan);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_register_node_special(int src_chan, int dst_chan, int buffer_size, byte[] name);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_unregister_node(int node_id);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_update_node(int node_id, int src_chan, int dst_chan);

        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_write_data(int node_id, byte[] buffer, uint bytes);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_write_data_ex(int node_id, byte[] buffer, uint offset, uint bytes);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_is_write_blocking(int node_id, uint bytes);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint shmemchain_read_data(int node_id, byte[] buffer, uint bytes, int read_mode);

        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_get_infos(int node_id, byte[] name, ulong[] data);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_get_all_nodes(int[] node_ids, int max_nodes);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_update_node_name(int node_id, byte[] name);

        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_get_last_error();
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_get_last_errorcode();
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern long shmemchain_get_rate(int node_id);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_set_rate(int node_id, long rate);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_get_blocksize(int node_id);
        [DllImport("shmemchain.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int shmemchain_set_blocksize(int node_id, int blocksize);




    }
}