using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.Timers;

namespace RX_FFT.Dialogs
{
    public partial class PerformaceStatsDialog : Form
    {
        private MainScreen.PerformanceEnvelope Envelope;
        private Timer RefreshTimer;
        private ListViewItem[] ListItems;

        public PerformaceStatsDialog(MainScreen.PerformanceEnvelope envelope)
        {
            Envelope = envelope;
            InitializeComponent();

            RefreshTimer = new Timer();
            RefreshTimer.Tick += new EventHandler(RefreshTimer_Tick);
            RefreshTimer.Interval = 500;
            RefreshTimer.Start();
        }

        void UpdateCounter(ListViewItem item, HighPerformanceCounter counter, double refTime, double refCPUTime)
        {
            if(item.SubItems[0].Text != counter.Name)
                item.SubItems[0].Text = counter.Name;

            item.SubItems[1].Text = String.Format("{0:0.00} s", counter.TotalTime);

            if (refTime > 0)
                item.SubItems[2].Text = String.Format("{0:0.00} %", ((100 * counter.TotalTime) / refTime));
            else
                item.SubItems[2].Text = "-";

            if (refCPUTime > 0)
                item.SubItems[3].Text = String.Format("{0:0.00} %", ((100 * counter.TotalTime) / refCPUTime));
            else
                item.SubItems[3].Text = "-";
        }

        void RefreshTimer_Tick(object sender, EventArgs e)
        {
            /* just init the first time */
            if (ListItems == null || ListItems.Length == 0)
            {
                ListItems = new ListViewItem[10];
                for (int pos = 0; pos < ListItems.Length; pos++)
                    ListItems[pos] = new ListViewItem(new string[4]);

                listCounters.Items.Clear();
                listCounters.Items.AddRange(ListItems);
            }

            int entry = 0;
            double processTime = Envelope.CounterProcessing.TotalTime + Envelope.CounterVisualization.TotalTime;
            HighPerformanceCounter tmpCtr = new HighPerformanceCounter("Processing");
            tmpCtr.TotalTime = processTime;

            UpdateCounter(ListItems[entry++], Envelope.CounterRuntime, 0, 0);

            UpdateCounter(ListItems[entry++], Envelope.CounterReading, 0, Envelope.CounterRuntime.TotalTime);
            UpdateCounter(ListItems[entry++], tmpCtr, 0, Envelope.CounterRuntime.TotalTime);


            UpdateCounter(ListItems[entry++], Envelope.CounterXlat, processTime, Envelope.CounterRuntime.TotalTime);
            UpdateCounter(ListItems[entry++], Envelope.CounterXlatLowpass, processTime, Envelope.CounterRuntime.TotalTime);
            UpdateCounter(ListItems[entry++], Envelope.CounterXlatDecimate, processTime, Envelope.CounterRuntime.TotalTime);
            UpdateCounter(ListItems[entry++], Envelope.CounterDemod, processTime, Envelope.CounterRuntime.TotalTime);
            UpdateCounter(ListItems[entry++], Envelope.CounterDemodLowpass, processTime, Envelope.CounterRuntime.TotalTime);
            UpdateCounter(ListItems[entry++], Envelope.CounterDemodDecimate, processTime, Envelope.CounterRuntime.TotalTime);
            UpdateCounter(ListItems[entry++], Envelope.CounterVisualization, processTime, Envelope.CounterRuntime.TotalTime);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            lock (Envelope)
            {
                Envelope.Reset();
                Envelope.CounterRuntime.Start();
                Envelope.CounterProcessing.Start();
                Envelope.CounterReading.Start();
            }
        }
    }
}
