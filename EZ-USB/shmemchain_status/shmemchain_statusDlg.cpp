// shmemchain_statusDlg.cpp : implementation file
//

#include "stdafx.h"
#include "shmemchain_status.h"
#include "shmemchain_statusDlg.h"

extern "C"
{
#include "shmemchain.h"
}

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

HANDLE shmemlib_mutex = NULL;
HANDLE shmlib_nodeinfo_h = NULL;
struct s_nodeinfo *shmlib_nodeinfo = NULL;


// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()


// Cshmemchain_statusDlg dialog




Cshmemchain_statusDlg::Cshmemchain_statusDlg(CWnd* pParent /*=NULL*/)
	: CDialog(Cshmemchain_statusDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void Cshmemchain_statusDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST, m_Status);
}

BEGIN_MESSAGE_MAP(Cshmemchain_statusDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()




void CALLBACK EXPORT TimerProc( HWND hWnd, UINT nMsg, UINT_PTR nIDEvent, DWORD dwTime )
{
	int pos = 0;
	int item_pos = 0;
	static int item_count = 0;

	Cshmemchain_statusDlg *dlg = (Cshmemchain_statusDlg*)AfxGetMainWnd();
	CListCtrl *status = (CListCtrl*) dlg->GetDlgItem ( IDC_LIST );


	if ( WaitForSingleObject ( shmemlib_mutex, 1000 ) == WAIT_OBJECT_0 ) 
	{
		while ( pos < MAX_NODES )
		{
			if ( shmlib_nodeinfo[pos].used )
			{
				CString temp;
				HANDLE mutex = CreateMutex ( NULL, TRUE, (LPWSTR) shmlib_nodeinfo[pos].node_mutex );

				if ( pos >= item_count )
				{
					status->InsertItem ( item_pos, L"" );
					item_count++;
				}

				if ( WaitForSingleObject ( mutex, 1000 ) == WAIT_OBJECT_0 )
				{
					temp.Format ( L"%d", pos );
					status->SetItemText ( item_pos, 0, temp );

					CString name ( shmlib_nodeinfo[pos].name );

					temp.Format ( L"%s", name );
					status->SetItemText ( item_pos, 1, temp );

					if ( shmlib_nodeinfo[pos].src_chan < 0 )
						temp.Format ( L"-" );
					else
						temp.Format ( L"%d", shmlib_nodeinfo[pos].src_chan );
					status->SetItemText ( item_pos, 2, temp );

					if ( shmlib_nodeinfo[pos].dst_chan < 0 )
						temp.Format ( L"-" );
					else
						temp.Format ( L"%d", shmlib_nodeinfo[pos].dst_chan );
					status->SetItemText ( item_pos, 3, temp );
					
					if ( shmlib_nodeinfo[pos].src_chan < 0 )
						temp.Format ( L"-" );
					else
						temp.Format ( L"%f", shmlib_nodeinfo[pos].bytes_read );
					status->SetItemText ( item_pos, 4, temp );
					
					if ( shmlib_nodeinfo[pos].dst_chan < 0 )
						temp.Format ( L"-" );
					else
						temp.Format ( L"%f", shmlib_nodeinfo[pos].bytes_written );
					status->SetItemText ( item_pos, 5, temp );

					if ( shmlib_nodeinfo[pos].src_chan < 0 )
						temp.Format ( L"-" );
					else
					{
						int used = 0;
						int pct = 0;

						used = shmlib_nodeinfo[pos].buffer_used;

						if ( shmlib_nodeinfo[pos].buffer_size > 0 )
							pct = (used * 100) / shmlib_nodeinfo[pos].buffer_size;
						else
							pct = 0;

						temp.Format ( L"%d%%", pct );
					}
					status->SetItemText ( item_pos, 6, temp );
				}	

				item_pos++;
				ReleaseMutex ( mutex );
				CloseHandle ( mutex );
			}
			pos++;
		}
		ReleaseMutex ( shmemlib_mutex );
	}
	while ( item_count >= item_pos )
		status->DeleteItem ( item_count-- );
	
}

// Cshmemchain_statusDlg message handlers

BOOL Cshmemchain_statusDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString strAboutMenu;
		strAboutMenu.LoadString(IDS_ABOUTBOX);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// set up global node info
	m_Status.InsertColumn( 0, L"ID", LVCFMT_RIGHT, 30, 1 );
	m_Status.InsertColumn( 1, L"Name", LVCFMT_RIGHT, 120, 1 );
	m_Status.InsertColumn( 2, L"Src", LVCFMT_RIGHT, 40, 1 );
	m_Status.InsertColumn( 3, L"Dst", LVCFMT_RIGHT, 40, 1 );
	m_Status.InsertColumn( 4, L"Read", LVCFMT_RIGHT, 75, 1 );
	m_Status.InsertColumn( 5, L"Written", LVCFMT_RIGHT, 75, 1 );
	m_Status.InsertColumn( 6, L"%", LVCFMT_RIGHT, 40, 1 );

	// get internal structs/mutex
	shmemlib_mutex = shmemchain_get_mutex ();
	shmlib_nodeinfo = shmemchain_get_nodeinfo ();

	SetTimer ( 1, 200, TimerProc );

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void Cshmemchain_statusDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void Cshmemchain_statusDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR Cshmemchain_statusDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

