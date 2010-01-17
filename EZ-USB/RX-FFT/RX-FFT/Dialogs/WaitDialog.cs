using System.Windows.Forms;

namespace RX_FFT.Dialogs
{
    public partial class WaitDialog : Form
    {
        public WaitDialog()
        {
            InitializeComponent();
        }

        public string Message
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }
    }
}
