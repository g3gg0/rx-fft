using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.USB_RX.Devices;
using System.Threading;
using RX_FFT.Components.GDI;

namespace AORemote
{
    public partial class AORemoteForm : Form
    {
        AR5000 Tuner = null;
        Thread UpdateThread = null;
        long TargetFrequency = 0;
        int CurrentAnt = 0;
        bool Refresh = false;

        public AORemoteForm()
        {
            Log.Init();
            InitializeComponent();
            SetConnected(false);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Tuner != null)
            {
                Tuner.CloseTuner();
                Tuner = null;
            }
            SetConnected(false);
            base.OnClosing(e);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (Tuner != null)
            {
                SetConnected(false);
                Tuner.CloseTuner();
                Tuner = null;
            }
            else
            {
                Tuner = new AR5000(true);

                if (!Tuner.OpenTuner())
                {
                    SetConnected(false);
                    Tuner = null;
                    return;
                }
                SetConnected(true);
            }
        }

        private void SetConnected(bool p)
        {
            frequencySelector1.Enabled = p;
            btnConnect.Text = p ? "Disconnect" : "Connect";

            if (UpdateThread != null)
            {
                UpdateThread.Abort();
                UpdateThread = null;
            }

            if (p)
            {
                UpdateThread = new Thread(UpdateThreadMain);
                UpdateThread.Start();
            }
        }

        private void UpdateThreadMain()
        {
            long currentFrequency = 0;
            int currentAnt = 0;
            int deadTime = 0;

            while (true)
            {
                if (TargetFrequency != currentFrequency)
                {
                    lock (Tuner)
                    {
                        currentFrequency = TargetFrequency;
                        Tuner.Frequency = TargetFrequency;
                    }
                    Thread.Sleep(50);

                    /* wait 500ms until polling status again */
                    deadTime = 50;
                }
                else if (deadTime == 0)
                {
                    if (Refresh)
                    {
                        lock (Tuner)
                        {
                            currentFrequency = Tuner.Frequency;
                            currentAnt = Tuner.SelectedAntenna;
                        }
                        if (frequencySelector1.Frequency != currentFrequency)
                        {
                            BeginInvoke(new Action(() =>
                            {
                                frequencySelector1.Frequency = currentFrequency;
                            }));
                        }

                        if (CurrentAnt != currentAnt)
                        {
                            CurrentAnt = currentAnt;
                            BeginInvoke(new Action(() =>
                            {
                                switch (currentAnt)
                                {
                                    case 1:
                                        radioAnt1.Select();
                                        break;
                                    case 2:
                                        radioAnt2.Select();
                                        break;
                                    case 3:
                                        radioAnt3.Select();
                                        break;
                                    case 4:
                                        radioAnt4.Select();
                                        break;
                                }
                            }));
                        }
                    }
                    Thread.Sleep(100);
                }
                else
                {
                    deadTime--;
                    Thread.Sleep(10);
                }

            }
        }

        private void frequencySelector1_TextChanged(object sender, EventArgs e)
        {
            lock (Tuner)
            {
                TargetFrequency = frequencySelector1.Frequency;
            }
        }

        private void SelectAntenna(int ant)
        {
            lock (Tuner)
            {
                Tuner.SelectedAntenna = ant;
            }
        }

        private void radioAnt1_CheckedChanged(object sender, EventArgs e)
        {
            SelectAntenna(1);
        }

        private void radioAnt2_CheckedChanged(object sender, EventArgs e)
        {
            SelectAntenna(2);
        }

        private void radioAnt3_CheckedChanged(object sender, EventArgs e)
        {
            SelectAntenna(3);
        }

        private void radioAnt4_CheckedChanged(object sender, EventArgs e)
        {
            SelectAntenna(4);
        }

        private void chkRefresh_CheckedChanged(object sender, EventArgs e)
        {
            Refresh = chkRefresh.Checked;
        }
    }
}
