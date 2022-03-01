// shmemchain.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "stdlib.h"
#include <windows.h>

#include "shmemchain.h"
#include <windows.h>
#include <strsafe.h>

#ifdef JNI_INTERFACE
#include "shmemchain_jni.h"

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_register_node
 * Signature: (II)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1register_1node ( JNIEnv *env, jclass java_class, jint src_chan, jint dst_chan )
{
	return (jint) shmemchain_register_node ( src_chan, dst_chan );
}

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_register_node_special
 * Signature: (IIILjava/lang/String;)I
 */

JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1register_1node_1special ( JNIEnv *env, jclass java_class, jint src_chan, jint dst_chan, jint buffer_size, jstring name )
{
	const jbyte* str = (*env)->GetStringUTFChars( env, name, NULL );
	__int32 ret = -1;

	if ( str == NULL )
		return -1;

	ret = shmemchain_register_node_special ( src_chan, dst_chan, buffer_size, str );

	(*env)->ReleaseStringUTFChars( env, name, str );

	return ret;
}

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_unregister_node
 * Signature: (I)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1unregister_1node ( JNIEnv *env, jclass java_class, jint node_id )
{
	return shmemchain_unregister_node ( node_id );
}

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_update_node
 * Signature: (III)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1update_1node ( JNIEnv *env, jclass java_class, jint node_id, jint src_chan, jint dst_chan )
{
	return shmemchain_update_node ( node_id, src_chan, dst_chan );
}

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_update_buffer_size
 * Signature: (III)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1update_1buffer_1size ( JNIEnv *env, jclass java_class, jint node_id, jint buffer_size )
{
	return shmemchain_update_buffer_size ( node_id, buffer_size );
}

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_write_data
 * Signature: (I[B)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1write_1data ( JNIEnv *env, jclass java_class, jint node_id, jbyteArray buffer )
{
	__int32 ret = 0;
	__int32 bytes = (*env)->GetArrayLength ( env, buffer );
	unsigned char *buf = (*env)->GetByteArrayElements ( env, buffer, NULL );
	unsigned char *real_buffer = NULL;
	if ( bytes < 1 )
		return 0;

	real_buffer = malloc ( bytes );
	memcpy ( real_buffer, buf, bytes );
	(*env)->ReleaseByteArrayElements ( env, buffer, buf, JNI_ABORT );

	ret = shmemchain_write_data ( node_id, real_buffer, bytes );
	free ( real_buffer );

	return ret;	
}

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_write_data_bytes
 * Signature: (I[B)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1write_1data_1bytes ( JNIEnv *env, jclass java_class, jint node_id, jbyteArray buffer, jint offset, jint bytes )
{
	/*
	__int32 ret = 0;
	unsigned char *buf = (*env)->GetByteArrayElements ( env, buffer, NULL );

	ret = shmemchain_write_data ( node_id, &buf[offset], bytes );

	(*env)->ReleaseByteArrayElements ( env, buffer, buf, JNI_ABORT );
	return ret;	
	*/
	__int32 ret = 0;
	unsigned char *buf = (*env)->GetByteArrayElements ( env, buffer, NULL );
	unsigned char *real_buffer = NULL;
	if ( bytes - offset < 1 )
		return 0;

	real_buffer = malloc ( bytes - offset );
	memcpy ( real_buffer, &buf[offset], bytes - offset );
	(*env)->ReleaseByteArrayElements ( env, buffer, buf, JNI_ABORT );

	ret = shmemchain_write_data ( node_id, real_buffer, bytes - offset );
	free ( real_buffer );

	return ret;	
}
/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_write_data_blocking
 * Signature: (II)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1is_1write_1blocking ( JNIEnv *env, jclass java_class, jint node_id, jint bytes )
{
	return shmemchain_is_write_blocking ( node_id, bytes );
}

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_read_data
 * Signature: (I[BI)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1read_1data ( JNIEnv *env, jclass java_class, jint node_id, jbyteArray buffer, jint read_mode )
{
	__int32 ret = 0;
	__int32 bytes = 0;
	unsigned char *buf = NULL;

	if ( !buffer || read_mode == MODE_FLUSH )
		return shmemchain_read_data ( node_id, NULL, 0, read_mode );

	bytes = (*env)->GetArrayLength ( env, buffer );
	buf = (*env)->GetByteArrayElements ( env, buffer, NULL );

	ret = shmemchain_read_data ( node_id, buf, bytes, read_mode );

	(*env)->ReleaseByteArrayElements ( env, buffer, buf, 0 );
	return ret;	
}

/*
 * Class:     shmemchainInterface_shmemchainInterface
 * Method:    shmemchain_get_info
 * Signature: (I[Ljava/lang/String;[I)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1get_1info 
  (JNIEnv* env, jclass java_class, jint node_id, jobjectArray name, jlongArray data_buf )
{
	__int32 ret = 0;
	__int64 *data = (*env)->GetLongArrayElements ( env, data_buf, NULL );
	static unsigned char str[MAX_NAME_LENGTH+1];

	ret = shmemchain_get_infos ( node_id, str, data );

	str[MAX_NAME_LENGTH] = '\000';

	(*env)->SetObjectArrayElement ( env, name, 0, (*env)->NewStringUTF ( env, str ) );
	(*env)->ReleaseLongArrayElements ( env, data_buf, data, 0 );

	return ret;	
}

/*
 * Class:     shmemchainInterface_shmemchainInterface
 * Method:    shmemchain_update_node_name
 * Signature: (ILjava/lang/String;)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1update_1node_1name (JNIEnv* env, jclass java_class, jint node_id, jstring name )
{
	const jbyte* str = (*env)->GetStringUTFChars( env, name, NULL );
	__int32 ret = -1;

	if ( str == NULL )
		return -1;

	ret = shmemchain_update_node_name ( node_id, str );

	(*env)->ReleaseStringUTFChars( env, name, str );

	return ret;
}

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_set_rate
 * Signature: (II)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1set_1rate ( JNIEnv *env, jclass java_class, jint node_id, jlong rate )
{
	return shmemchain_set_rate ( node_id, rate );
}
/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_get_rate
 * Signature: (I)I
 */
JNIEXPORT jlong JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1get_1rate ( JNIEnv *env, jclass java_class, jint node_id )
{
	return shmemchain_get_rate ( node_id );
}

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_set_blocksize
 * Signature: (II)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1set_1blocksize ( JNIEnv *env, jclass java_class, jint node_id, jint blocksize )
{
	return shmemchain_set_blocksize ( node_id, blocksize );
}

/*
 * Class:     shmemchainInterface
 * Method:    shmemchain_get_blocksize
 * Signature: (I)I
 */
JNIEXPORT jint JNICALL Java_shmemchainInterface_shmemchainInterface_shmemchain_1get_1blocksize ( JNIEnv *env, jclass java_class, jint node_id )
{
	return shmemchain_get_blocksize ( node_id );
}

#endif

__int32 shmemchain_lock_alloc ( __int32 node_id );
__int32 shmemchain_lock_node ( __int32 node_id );
__int32 shmemchain_unlock_node ( __int32 node_id );
__int32 shmemchain_unregister_node_locked ( __int32 node_id );

HANDLE local_mutex = NULL;
HANDLE shmemlib_mutex = NULL;
HANDLE shmlib_nodeinfo_h = NULL;
HANDLE shmlib_lockinfo_h = NULL;
struct s_nodeinfo *shmlib_nodeinfo = NULL;
struct s_lockinfo *shmlib_lockinfo = NULL;
struct s_nodeinfo_local local_nodes[MAX_NODES];
__int32 shmemchain_last_error = ERR_NONE;
__int32 shmemchain_last_errorcode = 0;

#define LOCK(h)       __lock_mutex(h,__FILE__,__FUNCTION__,__LINE__)
#define UNLOCK(h)     ReleaseMutex(h)
#define LOCK_FAILED() while(0){}





BOOL APIENTRY DllMain( HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved )
{
	__int32 already_initialized = 0;
	__int32 shmlib_nodeinfo_size = sizeof(struct s_nodeinfo)*MAX_NODES;
	__int32 shmlib_lockinfo_size = sizeof(struct s_lockinfo);

	switch (ul_reason_for_call)
	{
		case DLL_PROCESS_ATTACH:

			// set up local node info
			memset ( local_nodes, 0x00, sizeof(struct s_nodeinfo_local)*MAX_NODES );

			// set up global node info
			local_mutex = CreateMutex ( NULL, TRUE, NULL );
			if ( !local_mutex )
			{
				printf ( "local_mutex = CreateMutex ( NULL, TRUE, NULL );  failed\r\n" );
				return FALSE;
			}

			shmemlib_mutex = CreateMutex ( NULL, TRUE, L"shmlib_mutex" );
			if ( !shmemlib_mutex )
			{
				printf ( "shmemlib_mutex = CreateMutex ( NULL, TRUE, L\"shmlib_mutex\" );  failed\r\n" );
				return FALSE;
			}

			if ( GetLastError () == ERROR_ALREADY_EXISTS )
				already_initialized = 1;

			// create locking info struture
			shmlib_lockinfo_h = CreateFileMapping ( INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, shmlib_lockinfo_size, L"shmlib_lockinfo" );
			if ( !shmlib_lockinfo_h )
			{
				printf ( "shmlib_lockinfo_h = CreateFileMapping ( INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, shmlib_lockinfo_size, L\"shmlib_lockinfo\" ); failed\r\n" );
				return FALSE;
			}

			shmlib_lockinfo = (struct s_lockinfo *) MapViewOfFile ( shmlib_lockinfo_h, FILE_MAP_ALL_ACCESS, 0, 0, shmlib_lockinfo_size );
			if ( !shmlib_lockinfo )
			{
				printf ( "shmlib_lockinfo = (struct s_lockinfo *) MapViewOfFile ( shmlib_lockinfo_h, FILE_MAP_ALL_ACCESS, 0, 0, shmlib_lockinfo_size ); failed\r\n" );
				return FALSE;
			}
		
			// create node info structure
			shmlib_nodeinfo_h = CreateFileMapping ( INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, shmlib_nodeinfo_size, L"shmlib_nodeinfo" );
			if ( !shmlib_nodeinfo_h )
			{
				printf ( "shmlib_nodeinfo_h = CreateFileMapping ( INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, shmlib_nodeinfo_size, L\"shmlib_nodeinfo\" ); failed\r\n" );
				return FALSE;
			}

			shmlib_nodeinfo = (struct s_nodeinfo *) MapViewOfFile ( shmlib_nodeinfo_h, FILE_MAP_ALL_ACCESS, 0, 0, shmlib_nodeinfo_size );
			if ( !shmlib_nodeinfo )
			{
				printf ( "shmlib_nodeinfo = (struct s_nodeinfo *) MapViewOfFile ( shmlib_nodeinfo_h, FILE_MAP_ALL_ACCESS, 0, 0, shmlib_nodeinfo_size ); failed\r\n" );
				return FALSE;
			}

			{
				TCHAR* lpMsgBuf;
				DWORD dw = GetLastError(); 

				FormatMessage (
					FORMAT_MESSAGE_ALLOCATE_BUFFER | 
					FORMAT_MESSAGE_FROM_SYSTEM |
					FORMAT_MESSAGE_IGNORE_INSERTS,
					NULL,
					dw,
					MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
					(LPTSTR) &lpMsgBuf,
					0, NULL );

			}

			if ( !already_initialized )
			{
				memset ( shmlib_nodeinfo, 0x00, shmlib_nodeinfo_size );
				memset ( shmlib_lockinfo, 0x00, shmlib_lockinfo_size );

				UNLOCK ( shmemlib_mutex );
			}

			UNLOCK ( local_mutex );
			break;

		case DLL_THREAD_ATTACH:
		case DLL_THREAD_DETACH:
			break;

		case DLL_PROCESS_DETACH:
			{
				__int32 pos = 0;

				while ( pos < MAX_NODES )
				{
					if ( local_nodes[pos].local )
						shmemchain_unregister_node ( pos );
					pos++;
				}
			}
			CloseHandle ( local_mutex );

			break;

		default:
			printf ( "ul_reason_for_call %i\n", ul_reason_for_call );
	}
    return TRUE;
}


//
// internal helper functions
//
//


__int32 __lock_mutex ( HANDLE mutex, const char *file, const char *func, __int32 line )
{
	__int32 ret =  WaitForSingleObject ( mutex, MAX_LOCK_WAIT );
	if ( ret == WAIT_OBJECT_0 )
		return 1;

	printf ( "LOCK FAILED: 0x%08X at <%s> <%s>:%i\r\n", ret, file, func, line );

	if ( ret == WAIT_FAILED )
	{
		char* lpMsgBuf;
		DWORD dw = GetLastError(); 

		FormatMessage (
			FORMAT_MESSAGE_ALLOCATE_BUFFER | 
			FORMAT_MESSAGE_FROM_SYSTEM |
			FORMAT_MESSAGE_IGNORE_INSERTS,
			NULL,
			dw,
			MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
			&lpMsgBuf,
			0, NULL );

		printf ( "Reason: <%s>\r\n", lpMsgBuf );
	}

	return 0;
}

__int32 shmemchain_lock_read ( )
{
	__int32 locked = 0;
	__int32 tries = 0;

	while ( !locked )
	{
		if ( LOCK ( shmemlib_mutex ) ) 
		{
			if ( shmlib_lockinfo->lock_write == 0 && shmlib_lockinfo->lock_write_request == 0 )
			{
				shmlib_lockinfo->lock_read++;
				UNLOCK ( shmemlib_mutex );
				
				locked = 1;
//				printf ( "shmemchain_lock_read ( ): LOCKED after %i\n", tries );
			}
			else
			{
				UNLOCK ( shmemlib_mutex );
				tries++;

				/* force unlock now! */
				if(tries > 50)
				{
					shmlib_lockinfo->lock_write = 0;
					shmlib_lockinfo->lock_write_request = 0;
					shmlib_lockinfo->lock_read = 0;
				}
//				printf ( "shmemchain_lock_read ( ): %i\n", tries );

				// ToDo: this can be improved by using event objects!
				Sleep ( 20 );
			}
		}
		else
		{
			LOCK_FAILED();
			return E_FAIL;
		}
	}

	return E_OK;
}


__int32 shmemchain_unlock_read ( )
{
	__int32 unlocked = 0;

	while ( !unlocked )
	{
		if ( LOCK ( shmemlib_mutex ) ) 
		{
			shmlib_lockinfo->lock_read--;
			if ( shmlib_lockinfo->lock_read < 0 )
			{
				printf ( "FATAL! shmlib_lockinfo->lock_read < 0\n" );
			}
			UNLOCK ( shmemlib_mutex );
			
			unlocked = 1;
//			printf ( "shmemchain_unlock_read ( ): UNLOCKED\n" );
		}
		else
		{
			LOCK_FAILED();
			return E_FAIL;
		}
	}

	return E_OK;
}

__int32 shmemchain_lock_write_requested ( )
{
	return shmlib_lockinfo->lock_write_request;
}

__int32 shmemchain_lock_write ( )
{
	__int32 locked = 0;
	__int32 tries = 0;

	// first increase lock request count
	if ( LOCK ( shmemlib_mutex ) ) 
	{
		shmlib_lockinfo->lock_write_request++;
		UNLOCK ( shmemlib_mutex );
	}
	else
	{
		LOCK_FAILED();
		return E_FAIL;
	}

	// then try to get the write lock
	while ( !locked )
	{
		if ( LOCK ( shmemlib_mutex ) ) 
		{
			if ( shmlib_lockinfo->lock_read == 0 && shmlib_lockinfo->lock_write == 0 )
			{
				shmlib_lockinfo->lock_write++;
				shmlib_lockinfo->lock_write_request--;
				UNLOCK ( shmemlib_mutex );
//				printf ( "shmemchain_lock_write ( ): LOCKED after %i tries\n", tries );
				locked = 1;
			}
			else
			{
				UNLOCK ( shmemlib_mutex );
				tries++;
//				printf ( "shmemchain_lock_write ( ): %i\n", tries );

				// ToDo: this can be improved by using event objects!
				Sleep ( 10 );
			}
		}
		else
		{
			LOCK_FAILED();
			return E_FAIL;
		}
	}

	return E_OK;
}


__int32 shmemchain_unlock_write ( )
{
	__int32 unlocked = 0;

	while ( !unlocked )
	{
		if ( LOCK ( shmemlib_mutex ) ) 
		{
			shmlib_lockinfo->lock_write--;
			if ( shmlib_lockinfo->lock_write != 0 )
				printf ( "FATAL! shmlib_lockinfo->lock_write != 0\n" );

			UNLOCK ( shmemlib_mutex );
			
			unlocked = 1;
//			printf ( "shmemchain_unlock_write ( ): UNLOCKED\n" );
		}
		else
		{
			LOCK_FAILED();
			return E_FAIL;
		}
	}

	return E_OK;
}

__int32 shmemchain_insert_node ( struct s_nodeinfo node )
{
	__int32 id = NODE_INVALID;

	if ( shmemchain_lock_write ( ) == E_OK ) 
	{
		__int32 node_id = 0;

		while ( node_id < MAX_NODES && shmlib_nodeinfo[node_id].used )
			node_id++;

		if ( node_id < MAX_NODES )
		{
			// if the wanted channel is used, choose the next free
			__int32 out_free = 0;

			if ( node.dst_chan < 0 )
				out_free = 1;

			while ( !out_free )
			{
				__int32 out_check_pos = 0;

				while ( (out_check_pos < MAX_NODES) && (node.dst_chan >= 0) )
				{
					if ( shmlib_nodeinfo[out_check_pos].used && (shmlib_nodeinfo[out_check_pos].dst_chan == node.dst_chan) )
					{
						node.dst_chan++;
						out_check_pos = 0;
					}
					else
						out_check_pos++;
				}

				if ( out_check_pos >= MAX_NODES )
				{
					out_free = 1;
				}
				else
				{
					shmemchain_last_error = ERR_DST_CHANNEL_USED;
					shmemchain_unlock_write ( );
					return NODE_INVALID;
				}
			}

			node.used = 1;
			node.version = shmlib_nodeinfo[node_id].version + 1;
			wsprintf ( (LPWSTR) node.node_buffer, L"SHM_NODE_BUFFER_%02X_%02X", node_id, node.version );
			wsprintf ( (LPWSTR) node.node_mutex, L"SHM_NODE_MUTEX_%02X_%02X", node_id, node.version );
			wsprintf ( (LPWSTR) node.node_event, L"SHM_NODE_EVENT_%02X_%02X", node_id, node.version );
			wsprintf ( (LPWSTR) node.node_lock_event, L"SHM_NODE_LOCK_EVENT_%02X_%02X", node_id, node.version );

			local_nodes[node_id].node_mutex = NULL;
			local_nodes[node_id].node_event = NULL;
			local_nodes[node_id].buffer_h = NULL;
			local_nodes[node_id].buffer = NULL;
			local_nodes[node_id].used = 0;
			local_nodes[node_id].local = 1;

			local_nodes[node_id].node_mutex = CreateMutex ( 0, FALSE, (LPWSTR) node.node_mutex );
			local_nodes[node_id].node_event = CreateEvent ( 0, FALSE, FALSE, (LPWSTR) node.node_event );
			local_nodes[node_id].node_lock_event = CreateEvent ( 0, FALSE, FALSE, (LPWSTR) node.node_lock_event );

			if ( local_nodes[node_id].node_mutex )
			{
				local_nodes[node_id].buffer_h = CreateFileMapping ( INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, node.buffer_size, (LPWSTR) node.node_buffer );
				if ( local_nodes[node_id].buffer_h )
				{
					local_nodes[node_id].buffer = (unsigned char *) MapViewOfFile ( local_nodes[node_id].buffer_h, FILE_MAP_ALL_ACCESS, 0, 0, node.buffer_size );
					if ( local_nodes[node_id].buffer )
					{
						local_nodes[node_id].version = node.version;
						memcpy ( &(shmlib_nodeinfo[node_id]), &node, sizeof ( struct s_nodeinfo ) );
						local_nodes[node_id].used = 1;
						id = node_id;
					}
					else
					{
						CloseHandle ( local_nodes[node_id].buffer_h );
						CloseHandle ( local_nodes[node_id].node_mutex );
						CloseHandle ( local_nodes[node_id].node_event );
						CloseHandle ( local_nodes[node_id].node_lock_event );
						shmemchain_last_error = ERR_MAPVIEW_FAILED;
						shmemchain_last_errorcode = node.buffer_size;
						shmemchain_unlock_write ( );
						return NODE_INVALID;
					}
				}
				else
				{
					CloseHandle ( local_nodes[node_id].node_mutex );
					CloseHandle ( local_nodes[node_id].node_event );
					CloseHandle ( local_nodes[node_id].node_lock_event );
					shmemchain_last_error = ERR_MAPPING_FAILED;
					shmemchain_last_errorcode = GetLastError();
					shmemchain_unlock_write ( );
					return NODE_INVALID;
				}
			}
			else
			{
				CloseHandle ( local_nodes[node_id].node_event );
				CloseHandle ( local_nodes[node_id].node_lock_event );
				shmemchain_last_error = ERR_MUTEX_ALLOC_FAILED;
				shmemchain_last_errorcode = GetLastError();
				shmemchain_unlock_write ( );
				return NODE_INVALID;
			}
		}
		else
		{
			shmemchain_last_error = ERR_TOO_MANY_NODES;
			shmemchain_last_errorcode = 0;
			shmemchain_unlock_write ( );
			return NODE_INVALID;
		}

		shmemchain_unlock_write ( );
	}
	else
		shmemchain_last_error = ERR_LOCKING_FAILED;

	return id;
}

// required: locked shmemchain structure!
// call shmemchain_lock_read or shmemchain_lock_write before!
__int32 shmemchain_lock_node ( __int32 node_id )
{
	if ( node_id < 0 || node_id >= MAX_NODES || !local_nodes[node_id].node_mutex )
		return E_FAIL;
		
	if ( !LOCK ( local_nodes[node_id].node_mutex ) )
	{
		LOCK_FAILED();
		return E_FAIL;
	}

	return E_OK;
}

__int32 shmemchain_unlock_node ( __int32 node_id )
{
	if ( node_id < 0 || node_id >= MAX_NODES || !local_nodes[node_id].node_mutex )
		return E_FAIL;

	UNLOCK ( local_nodes[node_id].node_mutex );

	return E_OK;
}



__int32 shmemchain_lock_node_read ( __int32 node_id )
{
	__int32 locked = 0;
	__int32 tries = 0;
	struct s_lockinfo *info = &shmlib_nodeinfo[node_id].locks;

	while ( !locked )
	{
		if ( shmemchain_lock_node ( node_id ) == E_OK ) 
		{
			if ( info->lock_write == 0 && info->lock_write_request == 0 )
			{
				info->lock_read++;
				shmemchain_unlock_node ( node_id );
				
				locked = 1;
//				printf ( "shmemchain_lock_node_read ( ): LOCKED after %i\n", tries );
			}
			else
			{
				shmemchain_unlock_node ( node_id );
				tries++;
//				printf ( "shmemchain_lock_node_read ( ): %i\n", tries );
				WaitForSingleObject ( local_nodes[node_id].node_lock_event, 100 );
//				Sleep ( 20 );
			}
		}
		else
		{
			LOCK_FAILED();
			return E_FAIL;
		}
	}

	return E_OK;
}


__int32 shmemchain_unlock_node_read ( __int32 node_id )
{
	__int32 unlocked = 0;
	struct s_lockinfo *info = &shmlib_nodeinfo[node_id].locks;

	while ( !unlocked )
	{
		if ( shmemchain_lock_node ( node_id ) == E_OK ) 
		{
			info->lock_read--;
			if ( info->lock_read < 0 )
				printf ( "FATAL! info->lock_read < 0\n" );
			shmemchain_unlock_node ( node_id );
			SetEvent ( local_nodes[node_id].node_lock_event );
			
			unlocked = 1;
//			printf ( "shmemchain_unlock_node_read ( ): UNLOCKED\n" );
		}
		else
		{
			LOCK_FAILED();
			return E_FAIL;
		}
	}

	return E_OK;
}

__int32 shmemchain_lock_node_write ( __int32 node_id )
{
	__int32 locked = 0;
	__int32 tries = 0;
	struct s_lockinfo *info = &shmlib_nodeinfo[node_id].locks;

	// first increase lock request count
	if ( shmemchain_lock_node ( node_id ) == E_OK ) 
	{
		info->lock_write_request++;
		shmemchain_unlock_node ( node_id );
	}
	else
	{
		LOCK_FAILED();
		return E_FAIL;
	}

	// then try to get the write lock
	while ( !locked )
	{
		if ( shmemchain_lock_node ( node_id ) == E_OK ) 
		{
			if ( info->lock_read == 0 && info->lock_write == 0 )
			{
				info->lock_write++;
				info->lock_write_request--;
				shmemchain_unlock_node ( node_id );
//				printf ( "shmemchain_lock_node_write ( ): LOCKED after %i tries\n", tries );
				locked = 1;
			}
			else
			{
				shmemchain_unlock_node ( node_id );
				tries++;
//				printf ( "shmemchain_lock_node_write ( ): %i\n", tries );
//				Sleep ( 10 );
				WaitForSingleObject ( local_nodes[node_id].node_lock_event, 10 );
			}
		}
		else
		{
			LOCK_FAILED();
			return E_FAIL;
		}
	}

	return E_OK;
}


__int32 shmemchain_unlock_node_write ( __int32 node_id )
{
	__int32 unlocked = 0;
	struct s_lockinfo *info = &shmlib_nodeinfo[node_id].locks;

	while ( !unlocked )
	{
		if ( shmemchain_lock_node ( node_id ) == E_OK ) 
		{
			info->lock_write--;
			if ( info->lock_write != 0 )
				printf ( "FATAL! info->lock_write != 0\n" );

			shmemchain_unlock_node ( node_id );
			SetEvent ( local_nodes[node_id].node_lock_event );
			
			unlocked = 1;
//			printf ( "shmemchain_unlock_node_write ( ): UNLOCKED\n" );
		}
		else
		{
			LOCK_FAILED();
			return E_FAIL;
		}
	}

	return E_OK;
}


// this checks/changes the LOCAL representation of the nodes
// so its required to have a read-only lock on the shmeminfo
__int32 shmemchain_node_is_ok ( __int32 node_id )
{
	__int32 last_error = 0;

	if ( node_id < 0 || node_id >= MAX_NODES )
		return E_FAIL;

	// lock so two threads wont get in conflict
	if ( LOCK ( local_mutex ) ) 
	{
		// check if the local representation is the latest version
		// in case the owner thread un- or re-registered a node, the version will change
		if ( local_nodes[node_id].used && (local_nodes[node_id].version != shmlib_nodeinfo[node_id].version) )
			shmemchain_unregister_node_locked ( node_id );

		// if not used yet, initialize the local representation of the node
		if ( !local_nodes[node_id].used && shmlib_nodeinfo[node_id].used )
		{
			local_nodes[node_id].node_mutex = NULL;
			local_nodes[node_id].node_event = NULL;
			local_nodes[node_id].node_lock_event = NULL;
			local_nodes[node_id].buffer_h = NULL;
			local_nodes[node_id].buffer = NULL;
			local_nodes[node_id].used = 0;
			local_nodes[node_id].local = 0;

			local_nodes[node_id].node_mutex = CreateMutex ( NULL, FALSE, (LPWSTR) shmlib_nodeinfo[node_id].node_mutex );
			local_nodes[node_id].node_event = CreateEvent ( NULL, FALSE, FALSE, (LPWSTR) shmlib_nodeinfo[node_id].node_event );
			local_nodes[node_id].node_lock_event = CreateEvent ( NULL, FALSE, FALSE, (LPWSTR) shmlib_nodeinfo[node_id].node_lock_event );

			if ( local_nodes[node_id].node_mutex )
				local_nodes[node_id].buffer_h = CreateFileMapping ( INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, shmlib_nodeinfo[node_id].buffer_size, (LPWSTR) shmlib_nodeinfo[node_id].node_buffer );
			if ( local_nodes[node_id].buffer_h )
				local_nodes[node_id].buffer = (unsigned char *) MapViewOfFile ( local_nodes[node_id].buffer_h, FILE_MAP_ALL_ACCESS, 0, 0, shmlib_nodeinfo[node_id].buffer_size );
			if ( local_nodes[node_id].buffer )
				local_nodes[node_id].used = 1;
			local_nodes[node_id].version = shmlib_nodeinfo[node_id].version;

		}
		UNLOCK ( local_mutex );
	}
	else
	{
		LOCK_FAILED();
		return E_FAIL;
	}

	// final check if it is used now
	if ( !local_nodes[node_id].used )
	{
		last_error = GetLastError ();
		return E_FAIL;
	}

	return E_OK;
}

// unregister a node 
// required: locked shmemchain structure!
// call shmemchain_lock_read or shmemchain_lock_write before!
__int32 shmemchain_unregister_node_locked ( __int32 node_id )
{
	__int32 ret = E_FAIL;

	if ( node_id < 0 || node_id >= MAX_NODES || !local_nodes[node_id].used )
		return E_FAIL;
	
	local_nodes[node_id].used = 0;
	
	if ( local_nodes[node_id].local )
	{
		HANDLE tmp_mutex = local_nodes[node_id].node_mutex;
		LOCK ( tmp_mutex );

		local_nodes[node_id].local = 0;
		UnmapViewOfFile ( local_nodes[node_id].buffer );
		CloseHandle ( local_nodes[node_id].buffer_h );
		CloseHandle ( local_nodes[node_id].node_event );
		CloseHandle ( local_nodes[node_id].node_lock_event );

		if ( local_nodes[node_id].version == shmlib_nodeinfo[node_id].version )
		{
			shmlib_nodeinfo[node_id].version++;
			shmlib_nodeinfo[node_id].used = 0;
		}

		local_nodes[node_id].node_mutex = NULL;

		UNLOCK ( tmp_mutex );
		CloseHandle ( tmp_mutex );

		ret = E_OK;
	}
	else
	{
		// this case should not happen in context of shmemchain_node_is_ok!
		// (just happens when an other program deleted *our* node)
		// in context of shmemchain_unregister_node its safe
		HANDLE tmp_mutex = local_nodes[node_id].node_mutex;
		LOCK ( tmp_mutex );

		printf ( "NONLOCAL ACCESS - DO NOT DO THAT!\r\n" );
		CloseHandle ( local_nodes[node_id].node_event );
		CloseHandle ( local_nodes[node_id].node_lock_event );

		if ( local_nodes[node_id].buffer )
			UnmapViewOfFile ( local_nodes[node_id].buffer );
		if ( local_nodes[node_id].buffer_h )
			CloseHandle ( local_nodes[node_id].buffer_h );

		local_nodes[node_id].node_mutex = NULL;
		local_nodes[node_id].node_event = NULL;
		local_nodes[node_id].node_lock_event = NULL;
		local_nodes[node_id].buffer = NULL;
		local_nodes[node_id].buffer_h = NULL;

		if ( local_nodes[node_id].version == shmlib_nodeinfo[node_id].version )
		{
			shmlib_nodeinfo[node_id].version++;
			shmlib_nodeinfo[node_id].used = 0;
		}

		UNLOCK ( tmp_mutex );
		CloseHandle ( tmp_mutex );

		ret = E_OK;
	}
		

	return ret;
}


//
// library functions
//
//



SHMEMCHAIN_API struct s_nodeinfo * shmemchain_get_nodeinfo (  )
{
	return shmlib_nodeinfo;
}

SHMEMCHAIN_API HANDLE shmemchain_get_mutex (  )
{
	return shmemlib_mutex;
}

SHMEMCHAIN_API __int32 shmemchain_unregister_node ( __int32 node_id )
{
	__int32 ret = E_FAIL;

	if ( shmemchain_lock_write ( ) == E_OK )
	{
		if ( shmemchain_node_is_ok ( node_id ) == E_OK )
			ret = shmemchain_unregister_node_locked ( node_id );
		
		shmemchain_unlock_write ();
	}

	return ret;
}

__int32 shmemchain_register_node_ ( __int32 src_chan, __int32 dst_chan )
{
	return shmemchain_register_node ( src_chan, dst_chan );
}

SHMEMCHAIN_API __int32 shmemchain_register_node ( __int32 src_chan, __int32 dst_chan )
{
	__int32 id = NODE_INVALID;
	struct s_nodeinfo node;

	node.src_chan = src_chan;
	node.dst_chan = dst_chan;

	node.input_rate = 0;
	node.block_size = 0;

	node.bytes_written = 0;
	node.bytes_read = 0;

	node.buffer_start = 0;
	node.buffer_used = 0;
	node.buffer_size = DEFAULT_BUFFER_SIZE;

	node.locks.lock_count = 0;
	node.locks.lock_read = 0;
	node.locks.lock_write = 0;
	node.locks.lock_write_request = 0;
	

	strcpy ( node.name, "(undefined)" );

	id = shmemchain_insert_node ( node );

	return id;
}

SHMEMCHAIN_API __int32 shmemchain_register_node_special ( __int32 src_chan, __int32 dst_chan, __int32 buffer_size, unsigned char *name )
{
	__int32 id = NODE_INVALID;
	struct s_nodeinfo node;

	node.src_chan = src_chan;
	node.dst_chan = dst_chan;

	node.input_rate = 0;
	node.block_size = 0;

	node.bytes_written = 0;
	node.bytes_read = 0;

	node.buffer_start = 0;
	node.buffer_used = 0;
	node.buffer_size = buffer_size;

	node.locks.lock_count = 0;
	node.locks.lock_read = 0;
	node.locks.lock_write = 0;

	if ( name )
		strcpy ( node.name, name );
	else
		strcpy ( node.name, "(undefined)" );

	id = shmemchain_insert_node ( node );

	return id;
}


SHMEMCHAIN_API __int32 shmemchain_update_buffer_size ( __int32 node_id, __int32 buffer_size )
{
	__int32 ret = E_FAIL;

	// lock everything
	if ( shmemchain_lock_write () == E_OK ) 
	{
		if ( shmemchain_node_is_ok ( node_id ) == E_OK )
		{
			UnmapViewOfFile ( local_nodes[node_id].buffer );
			CloseHandle ( local_nodes[node_id].buffer_h );

			shmlib_nodeinfo[node_id].buffer_size = buffer_size;
			shmlib_nodeinfo[node_id].version++;
			local_nodes[node_id].version++;
			shmlib_nodeinfo[node_id].buffer_start = 0;
			shmlib_nodeinfo[node_id].buffer_used = 0;

			wsprintf ( (LPWSTR) shmlib_nodeinfo[node_id].node_buffer, L"SHM_NODE_BUFFER_%02X_%02X", node_id, shmlib_nodeinfo[node_id].version );

			local_nodes[node_id].buffer_h = CreateFileMapping ( INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, shmlib_nodeinfo[node_id].buffer_size, (LPWSTR) shmlib_nodeinfo[node_id].node_buffer );
			if ( local_nodes[node_id].buffer_h != NULL )
			{
				local_nodes[node_id].buffer = (unsigned char *) MapViewOfFile ( local_nodes[node_id].buffer_h, FILE_MAP_ALL_ACCESS, 0, 0, shmlib_nodeinfo[node_id].buffer_size );
				ret = E_OK;
			}
			else
				printf ( "failed re-allocating buffers\n" );
		}
		shmemchain_unlock_write ();
	}

	return ret;
}


SHMEMCHAIN_API __int32 shmemchain_update_node ( __int32 node_id, __int32 src_chan, __int32 dst_chan )
{
	__int32 source_node = 0;
	__int32 ret = E_FAIL;

	// lock everything
	if ( shmemchain_lock_write () == E_OK ) 
	{
		// if the wanted channel is used, choose the next free
		__int32 out_free = 0;
		
		if ( shmemchain_node_is_ok ( node_id ) != E_OK )
		{
			shmemchain_unlock_write ( );
			return E_FAIL;
		}

		// a disabled output is always "unused"		
		if ( dst_chan < 0 )
			out_free = 1;

		// get the next free output
		while ( !out_free )
		{
			__int32 out_check_pos = 0;

			while ( (out_check_pos < MAX_NODES) && (dst_chan < MAX_CHANS) )
			{
				if ( (out_check_pos != node_id) && shmlib_nodeinfo[out_check_pos].used && (shmlib_nodeinfo[out_check_pos].dst_chan == dst_chan) )
				{
					dst_chan++;
					out_check_pos = 0;
				}
				else
					out_check_pos++;
			}

			if ( out_check_pos >= MAX_NODES && dst_chan < MAX_CHANS )
				out_free = 1;
			else
			{
				shmemchain_unlock_write ();
				return NODE_INVALID;
			}
		}

		shmlib_nodeinfo[node_id].src_chan = src_chan;
		shmlib_nodeinfo[node_id].dst_chan = dst_chan;

		// set input data rate from source output rate
		while (source_node < MAX_NODES)
		{
			if ( shmlib_nodeinfo[source_node].used && shmlib_nodeinfo[node_id].src_chan >= 0 && (shmlib_nodeinfo[node_id].src_chan == shmlib_nodeinfo[source_node].dst_chan ) )
			{
				shmlib_nodeinfo[node_id].input_rate = shmlib_nodeinfo[source_node].output_rate;
				break;
			}
			source_node++;
		}

		ret = E_OK;

		shmemchain_unlock_write ();
	}

	return ret;
}


__int64 shmemchain_get_rate ( __int32 node_id )
{
	__int64 ret = E_FAIL;
	
	if ( shmemchain_lock_read () == E_OK ) 
	{
		if ( shmemchain_node_is_ok ( node_id ) == E_OK )
			ret = shmlib_nodeinfo[node_id].input_rate;
			
		shmemchain_unlock_read ();
	}
	
	return ret;
}


__int32 shmemchain_get_blocksize ( __int32 node_id )
{
	__int32 ret = E_FAIL;
	
	if ( shmemchain_lock_read () == E_OK ) 
	{
		if ( shmemchain_node_is_ok ( node_id ) == E_OK )
			ret = shmlib_nodeinfo[node_id].block_size;
		shmemchain_unlock_read ();
	}
	
	return ret;
}

__int32 shmemchain_set_rate ( __int32 node_id, __int64 rate )
{
	__int32 pos = 0;

	if ( shmemchain_lock_read () == E_OK ) 
	{
		if ( shmemchain_node_is_ok ( node_id ) != E_OK )
		{
			shmemchain_unlock_read ();
			return E_FAIL;
		}

		if ( shmlib_nodeinfo[node_id].dst_chan < 0 )
		{
			shmemchain_unlock_read ();
			return E_OK;
		}

		shmlib_nodeinfo[node_id].output_rate = rate;

		while ( pos < MAX_NODES )
		{
			if ( shmemchain_lock_node_write ( pos ) == E_OK ) 
			{
				if ( shmlib_nodeinfo[pos].used && shmlib_nodeinfo[pos].src_chan >= 0 && (shmlib_nodeinfo[pos].src_chan == shmlib_nodeinfo[node_id].dst_chan ) )
				{
					shmlib_nodeinfo[pos].input_rate = rate;
				}
				shmemchain_unlock_node_write ( pos );
			}
			pos++;
		}

		shmemchain_unlock_read ();
	}

	return E_OK;
}


__int32 shmemchain_set_blocksize ( __int32 node_id, __int32 blocksize )
{
	__int32 pos = 0;

	if ( shmemchain_lock_read () == E_OK ) 
	{
		if ( shmemchain_node_is_ok ( node_id ) != E_OK )
		{
			shmemchain_unlock_read ();
			return E_FAIL;
		}

		if ( shmlib_nodeinfo[node_id].dst_chan < 0 )
		{
			shmemchain_unlock_read ();
			return E_OK;
		}
		
		while ( pos < MAX_NODES )
		{
			if ( shmemchain_lock_node_write ( pos ) == E_OK ) 
			{
				if ( shmlib_nodeinfo[pos].used && shmlib_nodeinfo[pos].src_chan >= 0 && (shmlib_nodeinfo[pos].src_chan == shmlib_nodeinfo[node_id].dst_chan ) )
				{
					shmlib_nodeinfo[pos].block_size = blocksize;
				}
				shmemchain_unlock_node_write ( pos );
			}
			pos++;
		}
		shmemchain_unlock_read ();
	}

	return E_OK;
}

unsigned __int32 shmemchain_read_data_ ( __int32 node_id, unsigned char *buffer, unsigned __int32 bytes, __int32 read_mode )
{
	return shmemchain_read_data ( node_id, buffer, bytes, read_mode );
}

SHMEMCHAIN_API unsigned __int32 shmemchain_read_data ( __int32 node_id, unsigned char *buffer, unsigned __int32 bytes, __int32 read_mode )
{
	unsigned __int32 last_bytes_read = 0;
	unsigned __int32 bytes_read = 0;
	__int32 loop = 0;
	__int32 maxloops = 2;
	__int32 dynamic = 0;

#ifdef RESTRICT_FLOAT
	// dont accept data blocks that are not a multiple of 4 byte
	if ( bytes % 4 )
	{
		shmemchain_last_error = ERR_UNALIGNED_ACCESS;
		shmemchain_last_errorcode = __LINE__;
		printf ( "%s:%i: Error reading node %i: Size %i is not a multiple of 4 byte.\n", __FILE__, __LINE__, node_id, bytes );
		return 0;
	}
#endif

	// in case of a blocking read, we will restart here
	read_restart:

	if ( shmemchain_lock_read () != E_OK )
	{
		printf ( "%s:%i: Error locking\n", __FILE__, __LINE__ );
		return 0;
	}

	if ( shmemchain_node_is_ok ( node_id ) != E_OK )
	{
		shmemchain_unlock_read ();
		return 0;
	}
	
	// check if its our node
	if ( !local_nodes[node_id].local )
	{
		printf ( "Invalid node given\n" );
		shmemchain_unlock_read ( );
		return 0;
	}
	
	if ( read_mode & MODE_BLOCKING_TIME )
	{
		maxloops = (read_mode & MODE_MASK);
		read_mode &= ~MODE_MASK;
		read_mode |= MODE_PARTIAL;
		if ( read_mode & MODE_BLOCKING_DYNAMIC )
			dynamic = 1;
	}

	if ( read_mode & MODE_BLOCKING_TIME_NOPARTIAL )
	{
		maxloops = (read_mode & MODE_MASK);
		read_mode &= ~MODE_MASK;
		read_mode |= MODE_NOPARTIAL;
		if ( read_mode & MODE_BLOCKING_DYNAMIC )
			dynamic = 1;
	}

	if ( read_mode == MODE_GET_AVAIL )
	{
		__int32 used = 0;

		// get the number of used bytes
		// thats atomic and should not need any node locking
		used = shmlib_nodeinfo[node_id].buffer_used;

		// unlock again
		shmemchain_unlock_read ( );

		return used;
	}

	// clear buffer
	if ( !buffer || ((read_mode & MODE_MASK) == MODE_FLUSH) )
	{
		// that action needs locking of the node
		if ( shmemchain_lock_node_write ( node_id ) != E_OK )
		{
			printf ( "Error locking node %i\n", node_id );
			shmemchain_unlock_read ( );
			return 0;
		}

		shmlib_nodeinfo[node_id].buffer_used = 0;
		shmlib_nodeinfo[node_id].buffer_start = 0;

		// unlock again
		shmemchain_unlock_node_write ( node_id );
		shmemchain_unlock_read ( );

		return 0;
	}

	while ( (bytes_read < bytes) && (loop < maxloops) )
	{
		unsigned __int32 avail = 0;

		// that action needs locking of the node
		if ( shmemchain_lock_node_write ( node_id ) != E_OK )
		{
			printf ( "Error locking node %i\n", node_id );
			shmemchain_unlock_read ( );
			return 0;
		}

		avail = shmlib_nodeinfo[node_id].buffer_used;
		if ( ((read_mode & MODE_MASK) == MODE_NOPARTIAL) && (avail < bytes) )
		{
			avail = 0;
		}

		if ( avail )
		{
			// how much to get at once
			__int32 maxbytes = 0;

			// get the max block size
			if ( shmlib_nodeinfo[node_id].buffer_start + shmlib_nodeinfo[node_id].buffer_used <= shmlib_nodeinfo[node_id].buffer_size )
				maxbytes = shmlib_nodeinfo[node_id].buffer_used;
			else
				maxbytes = shmlib_nodeinfo[node_id].buffer_size - shmlib_nodeinfo[node_id].buffer_start;

			// not more than we should read
			if ( bytes_read + maxbytes > bytes )
				maxbytes = bytes - bytes_read;

#ifdef RESTRICT_FLOAT
			// dont accept data blocks that are not a multiple of 4 byte
			if ( maxbytes % 4 )
			{
				shmemchain_last_error = ERR_UNALIGNED_ACCESS;
				shmemchain_last_errorcode = __LINE__;
				shmemchain_unlock_node_write ( node_id );
				shmemchain_unlock_read ( );
				printf ( "%s:%i: Error reading node %i: Size %i is not a multiple of 4 byte.\n", __FILE__, __LINE__, node_id, maxbytes );
				return 0;
			}
#endif


#ifdef UNLOCK_MEMCPY
			// unlock for expensive memcpy
			shmemchain_unlock_node_write ( node_id );
#endif

			// get the data
			memcpy ( buffer + bytes_read, local_nodes[node_id].buffer + shmlib_nodeinfo[node_id].buffer_start, maxbytes );
			bytes_read += maxbytes;

#ifdef UNLOCK_MEMCPY
			// lock again for updating shared variables
			if ( shmemchain_lock_node_write ( node_id ) != E_OK )
			{
				printf ( "Error locking node %i\n", node_id );
				shmemchain_unlock_read ( );
				return 0;
			}
#endif

			// and advance pointers/statistics
			shmlib_nodeinfo[node_id].bytes_read += maxbytes;
			shmlib_nodeinfo[node_id].buffer_start += maxbytes;
			shmlib_nodeinfo[node_id].buffer_start %= shmlib_nodeinfo[node_id].buffer_size;
			shmlib_nodeinfo[node_id].buffer_used -= maxbytes;
		}

		// unlock again
		shmemchain_unlock_node_write ( node_id );

		// and wait for changes
		if ( dynamic && (last_bytes_read != bytes_read) )
		{
			loop = 0;
		}

		last_bytes_read = bytes_read;

		if ( bytes_read < bytes )
		{
			// when looping and a program wants to modify the structure, restart here - else we could end up in a deadlock
			if ( shmemchain_lock_write_requested () )
			{
				shmemchain_unlock_read ( );
				goto read_restart;
			}
			
			if ( (read_mode&MODE_MASK) != MODE_BLOCKING )
				loop++;
			WaitForSingleObject ( local_nodes[node_id].node_event, 100 );
			
		}
		else
		{
			shmemchain_unlock_read ( );
			return bytes_read;
		}

	}

	shmemchain_unlock_read ();

	return bytes_read;
}

/*
	return 
	  E_FAIL internal error
	  0/1 success
	
*/
__int32 shmemchain_write_node ( __int32 node_id, unsigned char *buffer, unsigned __int32 bytes, __int32 check_blocking )
{
	unsigned __int32 bytes_written = 0;
	unsigned __int32 free = 0;
	unsigned __int32 maxbytes = 0;
	unsigned __int32 writepos = 0;
	unsigned __int32 already_written = 0;

	if (shmemchain_node_is_ok(node_id) != E_OK)
	{
		printf("shmemchain_node_is_ok(%d) failed\n", node_id);
		return E_FAIL;
	}
		
	// dont accept data blocks that are not a multiple of 4 byte
#ifdef RESTRICT_FLOAT
	if ( bytes % 4 )
	{
		printf ( "%s:%i: Error writing to node %i: Size %i is not a multiple of 4 byte.\n", __FILE__, __LINE__, node_id, bytes );
		return E_FAIL;
	}
#endif
		
	// we need locking of the node below
	if ( shmemchain_lock_node_write ( node_id ) != E_OK )
	{
		printf ( "%s:%i: Error locking node %i\n", __FILE__, __LINE__, node_id );
		return E_FAIL;
	}

	free = shmlib_nodeinfo[node_id].buffer_size - shmlib_nodeinfo[node_id].buffer_used;

	if(free < 4)
	{
		shmemchain_last_error = ERR_BUFFER_TOO_FULL;
		shmemchain_last_errorcode = __LINE__;
		shmemchain_unlock_node_write ( node_id );
		printf ( "%s:%i: Error writing to node %i: (free < 4), free == %i\n", __FILE__, __LINE__, node_id, free );
		return E_FAIL;
	}

#ifdef RESTRICT_FLOAT
	if(free % 4)
	{
		shmemchain_last_error = ERR_UNALIGNED_ACCESS;
		shmemchain_last_errorcode = __LINE__;
		shmemchain_unlock_node_write ( node_id );
		printf ( "%s:%i: Error writing to node %i: (free % 4), free == %i\n", __FILE__, __LINE__, node_id, free );
		return E_FAIL;
	}
#endif

	free -= 4;


	// just check if data fits into buffers
	if ( !buffer || check_blocking )
	{
		shmemchain_unlock_node_write ( node_id );

		if ( free >= bytes )
			return E_FAIL;

		return 1;
	}

	/* dont write if there is no space left */
	if ( free < bytes )
	{
		shmemchain_unlock_node_write ( node_id );
		return 0;
	}

	writepos = (shmlib_nodeinfo[node_id].buffer_start + shmlib_nodeinfo[node_id].buffer_used) % shmlib_nodeinfo[node_id].buffer_size;


	/* write the first part that is required to fill until the end of the memory range */
	if ( shmlib_nodeinfo[node_id].buffer_start + shmlib_nodeinfo[node_id].buffer_used < shmlib_nodeinfo[node_id].buffer_size  )
	{
		maxbytes = shmlib_nodeinfo[node_id].buffer_size - (shmlib_nodeinfo[node_id].buffer_start + shmlib_nodeinfo[node_id].buffer_used);

		if ( maxbytes > bytes )
			maxbytes = bytes;

		memcpy ( local_nodes[node_id].buffer + writepos, buffer, maxbytes );
		bytes_written += maxbytes;
		bytes -= maxbytes;

		/* remember how many bytes are already written into the buffer */
		already_written = maxbytes;
		//shmlib_nodeinfo[node_id].buffer_used += maxbytes;
	}

	/* now write the rest */
	writepos = (shmlib_nodeinfo[node_id].buffer_start + (shmlib_nodeinfo[node_id].buffer_used + already_written)) % shmlib_nodeinfo[node_id].buffer_size;
	maxbytes = shmlib_nodeinfo[node_id].buffer_size - (shmlib_nodeinfo[node_id].buffer_used + already_written);

	if ( maxbytes > bytes )
		maxbytes = bytes;

#ifdef UNLOCK_MEMCPY
	// unlock for expensive memcpy. noone else is accessing this channel for writing (hopefully)
	shmemchain_unlock_node_write ( node_id );
#endif

	memcpy ( local_nodes[node_id].buffer + writepos, buffer + bytes_written, maxbytes );
	bytes_written += maxbytes;

#ifdef UNLOCK_MEMCPY
	// lock again for updating shared variable
	if ( shmemchain_lock_node_write ( node_id ) != E_OK )
	{
		printf ( "Error locking node %i\n", node_id );
		shmemchain_unlock_read ( );
		return E_FAIL;
	}
#endif

	/* now update the content size */
	shmlib_nodeinfo[node_id].buffer_used += maxbytes + already_written;

	shmemchain_unlock_node_write ( node_id );
	
	SetEvent ( local_nodes[node_id].node_event );

	return E_OK;
}

SHMEMCHAIN_API __int32 __stdcall shmemchain_write_data_ex(__int32 node_id, unsigned char* buffer, unsigned __int32 offset, unsigned __int32 bytes)
{
	__int32 pos = 0;
	__int32 ret = E_OK;

	if (shmemchain_lock_read() == E_OK)
	{
		if (shmemchain_node_is_ok(node_id) != E_OK)
		{
			printf("shmemchain_node_is_ok(%d) failed\n", node_id);
			shmemchain_unlock_read();
			return E_FAIL;
		}

		if (!buffer || !bytes)
		{
			shmemchain_unlock_read();
			return 0;
		}

		while (pos < MAX_NODES)
		{
			if (shmlib_nodeinfo[pos].used && (shmlib_nodeinfo[pos].src_chan >= 0) && (shmlib_nodeinfo[pos].src_chan == shmlib_nodeinfo[node_id].dst_chan))
			{
				if (shmemchain_write_node(pos, &buffer[offset], bytes, 0) < 0)
				{
					ret |= 1;
				}
			}

			pos++;
		}
		shmlib_nodeinfo[node_id].bytes_written += bytes;

		shmemchain_unlock_read();
	}
	else
	{
		printf("shmemchain_lock_read() failed\n");
	}

	return ret;
}

SHMEMCHAIN_API __int32 __stdcall shmemchain_write_data(__int32 node_id, unsigned char* buffer, unsigned __int32 bytes)
{
	return shmemchain_write_data_ex(node_id, buffer, 0, bytes);
}

SHMEMCHAIN_API __int32 __stdcall shmemchain_is_write_blocking ( __int32 node_id, unsigned __int32 bytes )
{
	__int32 pos = 0;

	if ( shmemchain_lock_read () == E_OK ) 
	{
		if ( shmemchain_node_is_ok ( node_id ) != E_OK )
		{
			shmemchain_unlock_read ();
			return 0;
		}
		
		if ( !local_nodes[node_id].local || !bytes )
		{
			shmemchain_unlock_read ();
			return 0;
		}

		while ( pos < MAX_NODES )
		{
			if ( shmlib_nodeinfo[pos].used && (shmlib_nodeinfo[pos].src_chan == shmlib_nodeinfo[node_id].dst_chan ) )
			{
				if ( shmemchain_write_node ( pos, NULL, bytes, 1 ) )
				{
					shmemchain_unlock_read ();
					return 1;
				}
			}
			pos++;
		}

		shmemchain_unlock_read ();
	}

	return 0;
}

SHMEMCHAIN_API __int32 __stdcall shmemchain_get_last_error ( )
{
	return shmemchain_last_error;
}

SHMEMCHAIN_API __int32 __stdcall shmemchain_get_last_errorcode ( )
{
	return shmemchain_last_errorcode;
}

/* this is a dirty hack!!! */
SHMEMCHAIN_API __int32 __stdcall shmemchain_get_infos ( __int32 node_id, unsigned char* name, __int64 data[] )
{
	__int32 used = 0;
	__int32 pct = 0;

	if ( shmemchain_lock_read () == E_OK ) 
	{
		// we need locking of the node
		if ( shmemchain_node_is_ok ( node_id ) != E_OK )
		{
			shmemchain_unlock_read ( );
			return E_FAIL;
		}
		
		// we need locking of the node
		if ( shmemchain_lock_node_write ( node_id ) != E_OK )
		{
			printf ( "Error locking node %i\n", node_id );
			shmemchain_unlock_read ( );
			return E_FAIL;
		}
		
		if ( !shmlib_nodeinfo[node_id].used )
		{
			shmemchain_unlock_read ();
			return E_FAIL;
		}

		if ( data )
		{
			if ( shmlib_nodeinfo[node_id].buffer_size > 0 )
				pct = (shmlib_nodeinfo[node_id].buffer_used * 100) / shmlib_nodeinfo[node_id].buffer_size;
			else
				pct = 0;

			data[0] = shmlib_nodeinfo[node_id].src_chan;
			data[1] = shmlib_nodeinfo[node_id].dst_chan;
			data[2] = shmlib_nodeinfo[node_id].bytes_read;
			data[3] = shmlib_nodeinfo[node_id].bytes_written;
			data[4] = shmlib_nodeinfo[node_id].buffer_size;
			data[5] = shmlib_nodeinfo[node_id].buffer_used;
			data[6] = pct;
		}

		if ( name != NULL )
			strncpy ( name, shmlib_nodeinfo[node_id].name, MAX_NAME_LENGTH );

		shmemchain_unlock_node_write ( node_id );
		shmemchain_unlock_read ();
	}

	return E_OK;
}


/* this is a dirty hack!!! */
SHMEMCHAIN_API __int32 __stdcall shmemchain_get_all_nodes ( __int32 node_ids[], __int32 max_nodes )
{
	__int32 node_id = 0;
	__int32 nodes_used = 0;

	if ( shmemchain_lock_read () == E_OK ) 
	{
		/* go through all nodes */
		while ( node_id < max_nodes && node_id < MAX_NODES )
		{
			/* is node used and available? */
			if ( shmemchain_node_is_ok ( node_id ) == E_OK )
			{
				node_ids[nodes_used++] = node_id;
			}			

			node_id++;
		}

		shmemchain_unlock_read ();
	}

	return nodes_used;
}

SHMEMCHAIN_API __int32 __stdcall shmemchain_update_node_name ( __int32 node_id, unsigned char* name )
{

	if ( !name )
		return E_FAIL;

	if ( shmemchain_lock_read () == E_OK ) 
	{
		// we need locking of the node
		if ( shmemchain_lock_node_write ( node_id ) != E_OK )
		{
			printf ( "Error locking node %i\n", node_id );
			shmemchain_unlock_read ( );
			return E_FAIL;
		}
		
		printf ( "Rename <%s> -> <%s>\n", shmlib_nodeinfo[node_id].name, name );
		strncpy ( shmlib_nodeinfo[node_id].name, name, MAX_NAME_LENGTH );
		
		shmemchain_unlock_node_write ( node_id );
		shmemchain_unlock_read ();
	}

	return E_OK;
}