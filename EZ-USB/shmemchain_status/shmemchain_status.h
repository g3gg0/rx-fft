// shmemchain_status.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// Cshmemchain_statusApp:
// See shmemchain_status.cpp for the implementation of this class
//

class Cshmemchain_statusApp : public CWinApp
{
public:
	Cshmemchain_statusApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern Cshmemchain_statusApp theApp;