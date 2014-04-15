// shmemchain_speedtestDlg.cpp : Implementierungsdatei
//

#include "stdafx.h"

extern "C" {
#include <shmemchain.h>
}

#include "shmemchain_speedtest.h"
#include "shmemchain_speedtestDlg.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#endif


#define MODE_NOTHING 0
#define MODE_READ    1
#define MODE_WRITE   2

#define MAX_PRINTF_SIZE       8192
#define DEFAULT_INTERVAL      1000
#define DEFAULT_TRANSFER_SIZE 1024
#define SHMEM_BUFFER_SIZE     128 * 1024 * 1024
#define PACKET_MAGIC    0xBABEFACE

UINT_PTR timer_handle = NULL;
LARGE_INTEGER highres_ticks_per_s;
LARGE_INTEGER highres_starttime;


unsigned int current_mode = MODE_NOTHING;
unsigned int timer_interval = DEFAULT_INTERVAL;
unsigned int timestamp_create = 0;
unsigned int timestamp_eval = 0;
unsigned int timestamp_transmit = 0;
unsigned int timestamp_print = 0;

unsigned int shmemchain_channel = 0;

unsigned int shmemchain_id = -1;

double meas_transferred = 0;
double meas_runtime = 0;
double meas_rate = 0;
double meas_delaysum = 0;
double meas_transfers = 0;


unsigned int transfer_size = DEFAULT_TRANSFER_SIZE;

typedef struct {
	unsigned int magic;
	LARGE_INTEGER timestamp;
	unsigned int buffer_size;
	unsigned int reset_data;
	unsigned int frame_number;
} t_header;


class CNumberFormatter
{
public:
	CNumberFormatter ( CString unit, int base = 1000 );
	CString convert ( double value );
	CString convert ( long long value );
private:
	int m_base;
	CString m_unit;
	CString m_number;
};

CNumberFormatter::CNumberFormatter ( CString unit, int base )
{
	m_unit = unit;
	m_base = base;
}

CString CNumberFormatter::convert ( double value )
{
	int exponent_default = 5; /* default: ' ' */
	int exponent = exponent_default; 
	unsigned char exponents[] = { 'f', 'p', 'n', 'µ', 'm', ' ', 'k', 'M', 'G', 'T', 'E' };

	if ( m_base < 2 )
		return _T("InvBase");

	while ( value >= m_base )
	{
		exponent++;
		value /= m_base;
	}

	while ( value < 1 && value > 0 )
	{
		exponent--;
		value *= m_base;
	}

	if ( exponent_default == exponent || exponent < 0 || exponent > (sizeof (exponents) / sizeof (exponents[0]) ) )
		m_number.Format ( _T("%.2f %s"), value, m_unit );
	else
	{
		if ( m_base == 1024 )
			m_number.Format ( _T("%.2f %ci%s"), value, exponents[exponent], m_unit );
		else
			m_number.Format ( _T("%.2f %c%s"), value, exponents[exponent], m_unit );
	}

	return m_number;
}


CString CNumberFormatter::convert ( long long value )
{
	return convert ( (double) value );
}

// CAboutDlg-Dialogfeld für Anwendungsbefehl "Info"

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialogfelddaten
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV-Unterstützung

// Implementierung
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


// Cshmemchain_speedtestDlg-Dialogfeld




Cshmemchain_speedtestDlg::Cshmemchain_speedtestDlg(CWnd* pParent /*=NULL*/)
	: CDialog(Cshmemchain_speedtestDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void Cshmemchain_speedtestDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_MESSAGES, mMessages);
}

BEGIN_MESSAGE_MAP(Cshmemchain_speedtestDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_TIMER()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_MODE_WRITE, &Cshmemchain_speedtestDlg::OnBnClickedModeWrite)
	ON_BN_CLICKED(IDC_MODE_READ, &Cshmemchain_speedtestDlg::OnBnClickedModeRead)
	ON_BN_CLICKED(IDC_MODE_NOTHING, &Cshmemchain_speedtestDlg::OnBnClickedModeNothing)
	ON_BN_CLICKED(IDC_TS_CREATE, &Cshmemchain_speedtestDlg::OnBnClickedTsCreate)
	ON_BN_CLICKED(IDC_TS_EVAL, &Cshmemchain_speedtestDlg::OnBnClickedTsEval)
	ON_BN_CLICKED(IDC_TS_TRANSMIT, &Cshmemchain_speedtestDlg::OnBnClickedTsTransmit)
	ON_BN_CLICKED(IDC_TS_LOG, &Cshmemchain_speedtestDlg::OnBnClickedTsLog)
	ON_BN_CLICKED(IDC_RESET, &Cshmemchain_speedtestDlg::OnBnClickedReset)
	ON_BN_CLICKED(IDC_CLEAR, &Cshmemchain_speedtestDlg::OnBnClickedClear)
END_MESSAGE_MAP()

void Cshmemchain_speedtestDlg::Log(CString fmt, ...) 
{
   va_list arg_ptr;
   CString buffer;

   va_start ( arg_ptr, fmt );
   buffer.FormatV ( fmt, arg_ptr );
   va_end ( arg_ptr );

   int nLength = mMessages.GetWindowTextLength();
   mMessages.SetSel ( nLength, nLength );
   mMessages.ReplaceSel ( buffer );

}


void Cshmemchain_speedtestDlg::OnTimer(UINT_PTR nIDEvent) 
{
	LARGE_INTEGER highres_timestamp;

	static t_header *current_header = NULL;
	static unsigned char *buffer = NULL;
	static int buffer_size = 0;
	static unsigned int next_frame_number = 0;

	highres_timestamp.QuadPart = 0;

	// handle buffer allocation and free'ing
	if ( buffer_size != transfer_size )
	{	
		if ( buffer )
			free ( buffer );
		buffer = NULL;

		buffer_size = transfer_size;
		if ( buffer_size < sizeof ( t_header ) )
			buffer_size = sizeof ( t_header );
	}

	if ( !buffer )
	{
		buffer = (unsigned char*)malloc ( buffer_size );
		if ( !buffer )
		{
			MessageBox ( _T("Failed allocating transfer buffer") );
			return;
		}
		memset ( buffer, 0x55, buffer_size );

		current_header = (t_header*)buffer;
		current_header->magic = PACKET_MAGIC;
		current_header->buffer_size = buffer_size;
		current_header->frame_number = 0;
	}


	if ( timestamp_create )
	{
		QueryPerformanceCounter ( &highres_timestamp );
		current_header->timestamp = highres_timestamp;
	}


	switch ( current_mode )
	{
		case MODE_NOTHING:
			break;

		case MODE_READ:
			{
				int bytes = shmemchain_read_data ( shmemchain_id, buffer, buffer_size, MODE_BLOCKING );
				if ( bytes != buffer_size )
				{
					Log ( _T("Read: Expected %i bytes, but read %i\r\n"), buffer_size, bytes );
					return;
				}

				if ( current_header->magic != PACKET_MAGIC )
					return;

				// buffer size changed!
				if ( current_header->buffer_size != buffer_size )
				{
					unsigned char *tmp_buffer = NULL;
					unsigned int buffer_size_old = buffer_size;

					// we read too few data - read the rest
					if ( current_header->buffer_size > buffer_size )
					{

						tmp_buffer = (unsigned char*)malloc ( current_header->buffer_size - buffer_size );
						if ( !tmp_buffer )
						{
							Log ( _T("Failed allocating temporary buffer\r\n") );
							return;
						}
						if ( shmemchain_read_data ( shmemchain_id, tmp_buffer, current_header->buffer_size - buffer_size, MODE_BLOCKING ) != (current_header->buffer_size - buffer_size) )
						{
							Log ( _T("Read rest of data resulted in short read\r\n") );
							return;
						}

						buffer = (unsigned char*)realloc ( buffer, current_header->buffer_size );
						if ( !buffer )
						{
							Log ( _T("Failed re-allocating temporary buffer\r\n") );
							return;
						}

						current_header = (t_header*)buffer;


						buffer_size = current_header->buffer_size;
						transfer_size = current_header->buffer_size;

						memcpy ( buffer + buffer_size_old, tmp_buffer, buffer_size - buffer_size_old );

						free ( tmp_buffer );

						CString str;
						str.Format ( _T("%i"), current_header->buffer_size );
						GetDlgItem ( IDC_TRANSFER_SIZE )->SetWindowTextW ( str );
					}
					else // we read too much data - read the rest of the remaining packet
					{
						// but read rest only if this was not a multiple of the old size
						if ( buffer_size % current_header->buffer_size )
						{
							tmp_buffer = (unsigned char*)malloc ( buffer_size % current_header->buffer_size );
							if ( !tmp_buffer )
							{
								Log ( _T("Failed allocating temporary buffer\r\n") );
								return;
							}
							if ( shmemchain_read_data ( shmemchain_id, tmp_buffer, buffer_size % current_header->buffer_size, MODE_BLOCKING ) != (buffer_size % current_header->buffer_size) )
							{
								Log ( _T("Read rest of data resulted in short read\r\n") );
								return;
							}

							free ( tmp_buffer );
						}

						buffer = (unsigned char*)realloc ( buffer, current_header->buffer_size );
						if ( !buffer )
						{
							Log ( _T("Failed re-allocating temporary buffer\r\n") );
							return;
						}

						current_header = (t_header*)buffer;

						buffer_size = current_header->buffer_size;
						transfer_size = current_header->buffer_size;

						// prevent warning about lost frames
						next_frame_number = current_header->frame_number;

						CString str;
						str.Format ( _T("%i"), current_header->buffer_size );
						GetDlgItem ( IDC_TRANSFER_SIZE )->SetWindowTextW ( str );
					}
				}

				if ( current_header->reset_data )
				{
					ResetData ();
					next_frame_number = 0;
				}

				if ( next_frame_number != current_header->frame_number )
				{
					int diff = 0;
					
					if ( next_frame_number > current_header->frame_number )
						diff = next_frame_number - current_header->frame_number;
					else
						diff = current_header->frame_number - next_frame_number;

					Log ( _T("%i Frames lost: %i -> %i\n"), diff, next_frame_number, current_header->frame_number );

					next_frame_number = current_header->frame_number + 1;
				}
				else
					next_frame_number++;

				meas_transferred += buffer_size;
				meas_transfers++;
			}
			break;

		case MODE_WRITE:

			if ( !shmemchain_is_write_blocking ( shmemchain_id, buffer_size ) )
			{
				/* on first frame also reset clients data */
				if ( meas_transfers == 0 )
				{
					current_header->frame_number = 0;
					current_header->reset_data = 1;
				}
				else
					current_header->reset_data = 0;

				shmemchain_write_data ( shmemchain_id, buffer, buffer_size );

				current_header->frame_number++;

				meas_transferred += buffer_size;
				meas_transfers++;
			}
			break;
	}

	LARGE_INTEGER highres_diff;
	double seconds = 0;

	if ( timestamp_eval && (timestamp_create || timestamp_transmit) )
	{
		QueryPerformanceCounter ( &highres_diff );

		if ( timestamp_transmit )
			highres_diff.QuadPart -= current_header->timestamp.QuadPart;
		else
			highres_diff.QuadPart -= highres_timestamp.QuadPart;

		seconds = ((double)highres_diff.QuadPart) / ((double)highres_ticks_per_s.QuadPart);

		meas_delaysum += seconds;

		if ( timestamp_print )
		{
			CNumberFormatter fmt_time ( _T("s"), 1000 );
			Log ( _T("Timestamp difference: %s\r\n"), fmt_time.convert(seconds) );
		}
	}

	QueryPerformanceCounter ( &highres_diff );
	highres_diff.QuadPart -= highres_starttime.QuadPart;

	meas_runtime = ((double)highres_diff.QuadPart) / ((double)highres_ticks_per_s.QuadPart);

	RefreshData ();
}

void Cshmemchain_speedtestDlg::SetupTimer ( unsigned int timeout )
{
	if ( timer_handle != NULL )
		KillTimer ( timer_handle );

	timer_handle = SetTimer ( 1, timeout, 0 );

	timer_interval = timeout;
}

void Cshmemchain_speedtestDlg::SetValue ( int itemID, unsigned int value )
{
	CString tmp_text;

	tmp_text.Format ( _T("%i"), value );
	((CEdit*)GetDlgItem ( itemID ))->SetWindowTextW ( tmp_text );
}

void Cshmemchain_speedtestDlg::SetDefaultValues ( )
{
	((CButton*)GetDlgItem(IDC_MODE_NOTHING))->SetCheck ( true );
	current_mode = MODE_NOTHING;

	SetupTimer ( DEFAULT_INTERVAL );

	SetValue ( IDC_SHMEMCHAN, 0 );
	SetValue ( IDC_INTERVAL, DEFAULT_INTERVAL );
	SetValue ( IDC_TRANSFER_SIZE, DEFAULT_TRANSFER_SIZE );

}

// Cshmemchain_speedtestDlg-Meldungshandler

BOOL Cshmemchain_speedtestDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Hinzufügen des Menübefehls "Info..." zum Systemmenü.

	// IDM_ABOUTBOX muss sich im Bereich der Systembefehle befinden.
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

	// Symbol für dieses Dialogfeld festlegen. Wird automatisch erledigt
	//  wenn das Hauptfenster der Anwendung kein Dialogfeld ist
	SetIcon(m_hIcon, TRUE);			// Großes Symbol verwenden
	SetIcon(m_hIcon, FALSE);		// Kleines Symbol verwenden

	Log ( _T("-----------------------------------\r\n") );
	Log ( _T("          SHMem Speed-Test\r\n") );
	Log ( _T("    (c) 2008 Georg Hofstetter\r\n") );
	Log ( _T("-----------------------------------\r\n") );
	Log ( _T("\r\n") );

	QueryPerformanceFrequency ( &highres_ticks_per_s );
	Log ( _T("Timestamp resolution: %lu Ticks per second\r\n"), highres_ticks_per_s.QuadPart );

	SetDefaultValues();

	ResetData ();

	shmemchain_id = shmemchain_register_node_special ( -1, -1, SHMEM_BUFFER_SIZE, (unsigned char*)"Speed-Test" );

	if ( shmemchain_id < 0 )
		Log ( _T("Failed registering SHMem Node!\r\n") );
	else
		Log ( _T("SHMem Node ID: %i\r\n"), shmemchain_id );



	return TRUE;  // Geben Sie TRUE zurück, außer ein Steuerelement soll den Fokus erhalten
}

void Cshmemchain_speedtestDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

// Wenn Sie dem Dialogfeld eine Schaltfläche "Minimieren" hinzufügen, benötigen Sie 
//  den nachstehenden Code, um das Symbol zu zeichnen. Für MFC-Anwendungen, die das 
//  Dokument/Ansicht-Modell verwenden, wird dies automatisch ausgeführt.

void Cshmemchain_speedtestDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // Gerätekontext zum Zeichnen

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Symbol in Clientrechteck zentrieren
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Symbol zeichnen
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// Die System ruft diese Funktion auf, um den Cursor abzufragen, der angezeigt wird, während der Benutzer
//  das minimierte Fenster mit der Maus zieht.
HCURSOR Cshmemchain_speedtestDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}


void Cshmemchain_speedtestDlg::OnBnClickedModeWrite()
{
	if ( ((CButton*)GetDlgItem(IDC_MODE_WRITE))->GetCheck ( ) && current_mode != MODE_WRITE )
	{
		current_mode = MODE_WRITE;
		shmemchain_update_node ( shmemchain_id, -2, shmemchain_channel );
	}
}

void Cshmemchain_speedtestDlg::OnBnClickedModeRead()
{
	if ( ((CButton*)GetDlgItem(IDC_MODE_READ))->GetCheck ( ) && current_mode != MODE_READ )
	{
		current_mode = MODE_READ;
		shmemchain_update_node ( shmemchain_id, shmemchain_channel, -2 );
	}
}

void Cshmemchain_speedtestDlg::OnBnClickedModeNothing()
{
	if ( ((CButton*)GetDlgItem(IDC_MODE_NOTHING))->GetCheck ( ) && current_mode != MODE_NOTHING )
	{
		current_mode = MODE_NOTHING;
		shmemchain_update_node ( shmemchain_id, -2, -2 );
	}
}

void Cshmemchain_speedtestDlg::OnBnClickedTsCreate()
{
	if ( ((CButton*)GetDlgItem ( IDC_TS_CREATE ))->GetCheck () )
		timestamp_create = 1;
	else
		timestamp_create = 0;

	if ( timestamp_create || timestamp_transmit )
	{
		GetDlgItem ( IDC_TS_EVAL )->EnableWindow ( true );
		GetDlgItem ( IDC_TS_LOG )->EnableWindow ( true );
	}
	else
	{
		GetDlgItem ( IDC_TS_EVAL )->EnableWindow ( false );
		GetDlgItem ( IDC_TS_LOG )->EnableWindow ( false );
	}
}

void Cshmemchain_speedtestDlg::OnBnClickedTsTransmit()
{
	if ( ((CButton*)GetDlgItem ( IDC_TS_TRANSMIT ))->GetCheck () )
		timestamp_transmit = 1;
	else
		timestamp_transmit = 0;

	if ( timestamp_create || timestamp_transmit )
	{
		GetDlgItem ( IDC_TS_EVAL )->EnableWindow ( true );
		GetDlgItem ( IDC_TS_LOG )->EnableWindow ( true );
	}
	else
	{
		GetDlgItem ( IDC_TS_EVAL )->EnableWindow ( false );
		GetDlgItem ( IDC_TS_LOG )->EnableWindow ( false );
	}
}

void Cshmemchain_speedtestDlg::OnBnClickedTsEval()
{
	if ( ((CButton*)GetDlgItem ( IDC_TS_EVAL ))->GetCheck () )
		timestamp_eval = 1;
	else
		timestamp_eval = 0;
}


void Cshmemchain_speedtestDlg::OnBnClickedTsLog()
{
	if ( ((CButton*)GetDlgItem ( IDC_TS_LOG ))->GetCheck () )
		timestamp_print = 1;
	else
		timestamp_print = 0;
}



void Cshmemchain_speedtestDlg::OnOK ()
{
	CString buffer;
	CComVariant variant;

	((CEdit*)GetDlgItem ( IDC_SHMEMCHAN ))->GetWindowTextW ( buffer );

	variant = buffer;
	if ( variant.ChangeType ( VT_INT ) != S_OK )
	{
		Log ( _T("Invalid Channel: <%s>\r\n"), buffer );
		return;
	}

	if ( shmemchain_channel != variant.intVal )
	{
		shmemchain_channel = variant.intVal;

		Log ( _T("Channel changed to %i\r\n"), shmemchain_channel );
	}


	//
	//
	//
	((CEdit*)GetDlgItem ( IDC_TRANSFER_SIZE ))->GetWindowTextW ( buffer );

	variant = buffer;
	if ( variant.ChangeType ( VT_INT ) != S_OK )
	{
		Log ( _T("Invalid Size: <%s>\r\n"), buffer );
		return;
	}

	if ( transfer_size != variant.intVal )
	{
		transfer_size = variant.intVal;
		ResetData ();

		Log ( _T("Size changed to %i\r\n"), transfer_size );
	}

	//
	//
	//
	((CEdit*)GetDlgItem ( IDC_INTERVAL ))->GetWindowTextW ( buffer );

	variant = buffer;
	if ( variant.ChangeType ( VT_INT ) != S_OK )
	{
		Log ( _T("Invalid Interval: <%s>\r\n"), buffer );
		return;
	}

	if ( timer_interval != variant.intVal )
	{
		SetupTimer ( variant.intVal );

		Log ( _T("Interval changed to %i\r\n"), variant.intVal );
	}

	shmemchain_read_data ( shmemchain_id, NULL, NULL, MODE_FLUSH );
	ResetData();

}

void Cshmemchain_speedtestDlg::OnBnClickedReset()
{
	shmemchain_read_data ( shmemchain_id, NULL, NULL, MODE_FLUSH );
	ResetData ();
}

void Cshmemchain_speedtestDlg::ResetData()
{
	meas_transferred = 0;
	meas_runtime = 0;
	meas_rate = 0;
	meas_delaysum = 0;
	meas_transfers = 0;

	QueryPerformanceCounter ( &highres_starttime );
	RefreshData();
}

void Cshmemchain_speedtestDlg::RefreshData()
{
	CNumberFormatter fmt_rate ( _T("Byte/s"), 1024 );
	CNumberFormatter fmt_time ( _T("s"), 1000 );
	CNumberFormatter fmt_data ( _T("Byte"), 1024 );

	GetDlgItem ( IDC_TRANSFERRED )->SetWindowText ( fmt_data.convert(meas_transferred) );

	GetDlgItem ( IDC_RUNTIME )->SetWindowText ( fmt_time.convert(meas_runtime) );

	if ( meas_runtime > 0 )
		GetDlgItem ( IDC_RATE )->SetWindowText ( fmt_rate.convert(meas_transferred/meas_runtime) );
	else
		GetDlgItem ( IDC_RATE )->SetWindowText ( fmt_rate.convert(0.0) );

	if ( meas_transfers > 0 )
		GetDlgItem ( IDC_LATENCY )->SetWindowText ( fmt_time.convert(meas_delaysum / meas_transfers) );
	else
		GetDlgItem ( IDC_LATENCY )->SetWindowText ( fmt_time.convert(0.0) );

}
void Cshmemchain_speedtestDlg::OnBnClickedClear()
{
	mMessages.SetWindowTextW ( _T("") );
}
