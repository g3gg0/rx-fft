using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GSM_Analyzer
{
    public partial class BurstVisualizer : Form
    {
        public BurstVisualizer()
        {
            InitializeComponent();
        }

        public void ProcessBurst ( double[] signal, double[] strength )
        {
            waveformDisplay1.ClearProcessData(signal);
            waveformDisplay2.ClearProcessData(strength);
        }
    }
}
