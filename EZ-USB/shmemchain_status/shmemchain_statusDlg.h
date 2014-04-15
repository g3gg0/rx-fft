// shmemchain_statusDlg.h : header file
//

#pragma once
#include "afxcmn.h"
#include "afxwin.h"


// Cshmemchain_statusDlg dialog
class Cshmemchain_statusDlg : public CDialog
{
// Construction
public:
	Cshmemchain_statusDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_SHMEMCHAIN_STATUS_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedRefresh();
	afx_msg void OnEnChangeStatus();
	CListCtrl m_Status;
};
