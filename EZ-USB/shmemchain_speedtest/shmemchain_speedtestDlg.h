// shmemchain_speedtestDlg.h : Headerdatei
//

#pragma once
#include "afxwin.h"


// Cshmemchain_speedtestDlg-Dialogfeld
class Cshmemchain_speedtestDlg : public CDialog
{
// Konstruktion
public:
	Cshmemchain_speedtestDlg(CWnd* pParent = NULL);	// Standardkonstruktor

// Dialogfelddaten
	enum { IDD = IDD_SHMEMCHAIN_SPEEDTEST_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV-Unterstützung


// Implementierung
protected:
	HICON m_hIcon;

	void SetupTimer ( unsigned int timeout );
	void SetDefaultValues ( );
	void OnTimer(UINT_PTR nIDEvent);
	void Log(CString fmt, ...);
	void SetValue ( int itemID, unsigned int value );
	void ResetData ( );
	void RefreshData ( );

	// Generierte Funktionen für die Meldungstabellen
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnOK ();
	afx_msg void OnBnClickedModeWrite();
	afx_msg void OnBnClickedModeRead();
	afx_msg void OnBnClickedModeNothing();
	afx_msg void OnBnClickedTsCreate();
	afx_msg void OnBnClickedTsEval();
	afx_msg void OnBnClickedTsTransmit();
	CEdit mMessages;
	afx_msg void OnEnChangeShmemchan();
	afx_msg void OnEnChangeInterval();
	afx_msg void OnEnChangeTransferSize();
	afx_msg void OnBnClickedReset();
	afx_msg void OnBnClickedClear();
	afx_msg void OnBnClickedTsLog();
};
