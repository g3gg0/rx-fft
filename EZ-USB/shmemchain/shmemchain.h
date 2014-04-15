// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the SHMEMCHAIN_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// SHMEMCHAIN_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.


#define SHMEMCHAIN_API __declspec(dllexport)

#define kB *1024
#define MB *1024 kB

#define DEFAULT_BUFFER_SIZE     8 MB
#define MAX_NODES				128
#define MAX_CHANS				128
#define MAX_NAME_LENGTH			128
#define NODE_INVALID			-1
#define MAX_LOCK_WAIT           5000

#define ERR_NONE                 -100
#define ERR_LOCKING_FAILED       -101
#define ERR_TOO_MANY_NODES       -102
#define ERR_DST_CHANNEL_USED     -103
#define ERR_BUFFER_ALLOC_FAILED  -104
#define ERR_MAPVIEW_FAILED       -105
#define ERR_MAPPING_FAILED       -106
#define ERR_MUTEX_ALLOC_FAILED   -107
#define ERR_UNALIGNED_ACCESS     -108
#define ERR_BUFFER_TOO_FULL      -109


#ifdef E_FAIL
#undef E_FAIL
#endif

#define E_OK					0
#define E_FAIL					-1

#define MODE_PARTIAL						0			// try to read up to n bytes (or less or zero)
#define MODE_NOPARTIAL						1			// try to read n bytes (or zero)
#define MODE_BLOCKING						2			// read n bytes (wait until we read them)
#define MODE_FLUSH							3			// flush buffers and return (no read made)
#define MODE_GET_AVAIL						4			// just get how much is available 
#define MODE_BLOCKING_TIME					0x40000000	// read n bytes (wait up to x*100ms, then return)
#define MODE_BLOCKING_TIME_NOPARTIAL		0x20000000	// read n bytes (wait up to x*100ms or read completely)
#define MODE_BLOCKING_DYNAMIC				0x10000000	// used with MODE_BLOCKING_TIME (wait up to x*100ms in case of no data)

#define MODE_CRITICAL			0x20000000	// less sleep delay (obsolete!)
#define MODE_MASK				0x000000FF  // internal


#ifdef SHMEMCHAIN_INTERNAL


struct s_lockinfo
{
	__int32 lock_count;
	__int32 lock_write;
	__int32 lock_read;
	__int32 lock_write_request;
};

struct s_nodeinfo
{
	__int32 used;
	__int32 version;
	__int32 src_chan;
	__int32 dst_chan;
	__int64 input_rate;
	__int64 output_rate;
	__int32 block_size;

	unsigned char name[MAX_NAME_LENGTH];

	__int64 bytes_written;
	__int64 bytes_read;


	unsigned __int32 buffer_start;
	unsigned __int32 buffer_used;
	unsigned __int32 buffer_size;

	unsigned _int16 node_buffer[32];
	unsigned _int16 node_mutex[32];
	unsigned _int16 node_event[32];
	unsigned _int16 node_lock_event[32];

	struct s_lockinfo locks;
//	__int32 node_locked;
};

struct s_nodeinfo_local
{
	__int32 used;
	__int32 version;
	__int32 local;

	unsigned char *buffer;
	HANDLE buffer_h;
	HANDLE node_mutex;
	HANDLE node_event;
	HANDLE node_lock_event;
};


SHMEMCHAIN_API struct s_nodeinfo * __stdcall shmemchain_get_nodeinfo (  );
SHMEMCHAIN_API HANDLE __stdcall shmemchain_get_mutex (  );
#endif


SHMEMCHAIN_API __int32 __stdcall shmemchain_register_node ( __int32 src_chan, __int32 dst_chan );
SHMEMCHAIN_API __int32 __stdcall shmemchain_register_node_special ( __int32 src_chan, __int32 dst_chan, __int32 buffer_size, unsigned char *name );
SHMEMCHAIN_API __int32 __stdcall shmemchain_unregister_node ( __int32 node_id );
SHMEMCHAIN_API __int32 __stdcall shmemchain_update_node ( __int32 node_id, __int32 src_chan, __int32 dst_chan );

SHMEMCHAIN_API __int32 __stdcall shmemchain_write_data ( __int32 node_id, unsigned char *buffer, unsigned __int32 bytes );
SHMEMCHAIN_API __int32 __stdcall shmemchain_is_write_blocking ( __int32 node_id, unsigned __int32 bytes );
SHMEMCHAIN_API unsigned __int32 __stdcall shmemchain_read_data ( __int32 node_id, unsigned char *buffer, unsigned __int32 bytes, __int32 read_mode );

SHMEMCHAIN_API __int32 __stdcall shmemchain_get_infos ( __int32 node_id, unsigned char* name, __int64 data[] );
SHMEMCHAIN_API __int32 __stdcall shmemchain_get_all_nodes ( __int32 node_ids[], __int32 max_nodes );
SHMEMCHAIN_API __int32 __stdcall shmemchain_update_node_name ( __int32 node_id, unsigned char* name );
SHMEMCHAIN_API __int32 __stdcall shmemchain_get_last_error ( );

SHMEMCHAIN_API __int64 __stdcall shmemchain_get_rate ( __int32 node_id );
SHMEMCHAIN_API __int32 __stdcall shmemchain_set_rate ( __int32 node_id, __int64 rate );
SHMEMCHAIN_API __int32 __stdcall shmemchain_get_blocksize ( __int32 node_id );
SHMEMCHAIN_API __int32 __stdcall shmemchain_set_blocksize ( __int32 node_id, __int32 blocksize );

SHMEMCHAIN_API __int32 __stdcall shmemchain_update_buffer_size ( __int32 node_id, __int32 buffer_size );




