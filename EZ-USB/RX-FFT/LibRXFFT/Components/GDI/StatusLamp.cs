using System.Drawing;
using System.Windows.Forms;

namespace LibRXFFT.Components.GDI
{
    public partial class StatusLamp : UserControl
    {
        private eLampState _State;
        private readonly Color[] Colors;

        public StatusLamp()
        {
            InitializeComponent();
            Colors = new Color[3];
            Colors[(int) eLampState.Grayed] = Color.DarkGray;
            Colors[(int) eLampState.Red] = Color.Red;
            Colors[(int) eLampState.Green] = Color.LimeGreen;
        }

        public eLampState State
        {
            get { return _State; }
            set
            {
                _State = value;
                BackColor = Colors[(int) _State];
                Invalidate();
            }
        }
    }

    public enum eLampState
    {
        Grayed = 0,
        Red = 1,
        Green = 2
    }
}