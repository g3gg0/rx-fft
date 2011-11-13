using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Libraries.GSM.Layer1;
using System.Threading;

namespace GSM_Analyzer
{
    public partial class KrakenStatusBox : UserControl
    {
        private GSMAnalyzer.KrakenCracker Kraken = null;
        private Thread UpdateThread;

        public KrakenStatusBox()
        {
            InitializeComponent();

            UpdateThread = new Thread(()=>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    lock (this)
                    {
                        UpdateControls();
                    }
                }
            });
            UpdateThread.Start();
        }


        internal void SetCracker(GSMAnalyzer.KrakenCracker cipherCracker)
        {
            lock (this)
            {
                Kraken = cipherCracker;
            }
        }

        public void SafeInvoke(Action updater)
        {
            bool forceSynchronous = false;

            if (!IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                if (forceSynchronous)
                {
                    Invoke((Action)delegate { SafeInvoke(updater); });
                }
                else
                {
                    BeginInvoke((Action)delegate { SafeInvoke(updater); });
                }
            }
            else
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException("Control is already disposed.");
                }

                updater();
            }
        }

        private void UpdateControls()
        {
            double progress = -1.0f;
            int jobId = -1;
            int jobNumber = -1; /* number ob job being processed for current burst */
            int jobcount = -1; /* total number of jobs to process for current burst */
            bool connected = false;
            KrakenJobStatus status = KrakenJobStatus.Unknown;

            try
            {
                if (Kraken != null && Kraken.Kraken != null)
                {
                    connected = Kraken.Available;

                    if (connected)
                    {
                        int jobs = 0;

                        foreach (KrakenClient k in Kraken.Kraken)
                        {
                            status = k.KrakenJobStatus;
                            jobId = k.RequestId;

                            if (jobId != -1)
                            {
                                jobs++;
                                progress += k.GetJobProgress();
                                //Kraken.GetJobInfo(out jobNumber, out jobcount);
                            }
                        }
                        progress /= jobs;
                        jobcount = jobs;
                        jobNumber = -1;
                    }
                }
            }
            catch (Exception e)
            {
                progress = -1.0f;
            }

            SafeInvoke(new Action(() =>
            {
                if (connected)
                {
                    if (progress >= 0)
                    {
                        lblStatus.Text = jobNumber + "/" + jobcount + " " + "(Job ID " + jobId + ")";
                        progressBar1.Value = (int)progress;
                        lblProgress.Text = (int)progress + " % (" + status + ")";
                    }
                    else
                    {
                        lblStatus.Text = "No Jobs";
                        progressBar1.Value = 0;
                        lblProgress.Text = "0 %";
                    }
                }
                else
                {
                    lblStatus.Text = "Not connected";
                    progressBar1.Value = 0;
                    lblProgress.Text = "0 %";
                }
            }));
        }
    }
}
