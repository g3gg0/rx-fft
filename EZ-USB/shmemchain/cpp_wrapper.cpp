/*
extern "C" 
{
	__int32 shmemchain_register_node_ ( __int32 src_chan, __int32 dst_chan );
	unsigned __int32 shmemchain_read_data_ ( __int32 node_id, unsigned char *buffer, unsigned __int32 bytes, __int32 read_mode );
}

__declspec(dllexport) __int32 shmemchain_register_node ( __int32 src_chan, __int32 dst_chan )
{
	__int32 ret = shmemchain_register_node_ ( src_chan, dst_chan );

	return ret;
}

__declspec(dllexport) unsigned __int32 shmemchain_read_data ( __int32 node_id, unsigned char *buffer, unsigned __int32 bytes, __int32 read_mode )
{
	unsigned __int32 ret = shmemchain_read_data_ ( node_id, buffer, bytes, read_mode );

	return ret;
}
*/