// shmemchain_speedtest.h : Hauptheaderdatei f�r die PROJECT_NAME-Anwendung
//

#pragma once

#ifndef __AFXWIN_H__
	#error "\"stdafx.h\" vor dieser Datei f�r PCH einschlie�en"
#endif

#include "resource.h"		// Hauptsymbole


// Cshmemchain_speedtestApp:
// Siehe shmemchain_speedtest.cpp f�r die Implementierung dieser Klasse
//

class Cshmemchain_speedtestApp : public CWinApp
{
public:
	Cshmemchain_speedtestApp();

// �berschreibungen
	public:
	virtual BOOL InitInstance();

// Implementierung

	DECLARE_MESSAGE_MAP()
};

extern Cshmemchain_speedtestApp theApp;