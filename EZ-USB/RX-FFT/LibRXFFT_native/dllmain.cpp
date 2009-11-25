// dllmain.cpp : Definiert den Einstiegspunkt für die DLL-Anwendung.
#include <windows.h>
#include <omp.h>

#define NUM_THREADS 4


BOOL APIENTRY DllMain( HMODULE hModule,
					  DWORD  ul_reason_for_call,
					  LPVOID lpReserved
					  )
{
	switch (ul_reason_for_call)
	{
		case DLL_PROCESS_ATTACH:
		case DLL_THREAD_ATTACH:
		case DLL_THREAD_DETACH:
		case DLL_PROCESS_DETACH:
			omp_set_num_threads(NUM_THREADS);
			break;
	}
	return TRUE;
}

