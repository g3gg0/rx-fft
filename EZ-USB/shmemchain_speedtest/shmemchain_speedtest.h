// shmemchain_speedtest.h : Hauptheaderdatei für die PROJECT_NAME-Anwendung
//

#pragma once

#ifndef __AFXWIN_H__
	#error "\"stdafx.h\" vor dieser Datei für PCH einschließen"
#endif

#include "resource.h"		// Hauptsymbole


// Cshmemchain_speedtestApp:
// Siehe shmemchain_speedtest.cpp für die Implementierung dieser Klasse
//

class Cshmemchain_speedtestApp : public CWinApp
{
public:
	Cshmemchain_speedtestApp();

// Überschreibungen
	public:
	virtual BOOL InitInstance();

// Implementierung

	DECLARE_MESSAGE_MAP()
};

extern Cshmemchain_speedtestApp theApp;