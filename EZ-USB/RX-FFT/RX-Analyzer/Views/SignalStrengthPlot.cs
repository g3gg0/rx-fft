using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.SignalProcessing;

namespace RX_Analyzer.Views
{
    public partial class SignalStrengthPlot : Form, SampleSink
    {
        public SignalStrengthPlot()
        {
            InitializeComponent();
        }

        #region SampleSink Member

        public void Process(double iValue, double qValue)
        {
            waveForm.ProcessData(DBTools.SampleTodB( Math.Sqrt(iValue * iValue + qValue * qValue)));
        }

        #endregion
       
    }
}
