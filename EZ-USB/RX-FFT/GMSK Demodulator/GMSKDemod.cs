using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Components;
using LibRXFFT.Libraries.ShmemChain;

namespace GMSK_Demodulator
{
    public partial class GMSKDemod : Form
    {
        private SharedMem ShmemChannel;
        private bool ThreadActive;
        private Thread ReadThread;
        private bool phasePaused;
        private int BlockSize = 512;
        private bool diffPaused;
        private bool allPaused;
        private bool iqPaused;

        private int MaxSamples = 10000;


        public GMSKDemod()
        {
            InitializeComponent();

            hScrollBar.Minimum = 0;
            hScrollBar.Maximum = MaxSamples;

            phaseDisplay.MaxSamples = MaxSamples;
            differenciateDisplay.MaxSamples = MaxSamples;
            waveI.MaxSamples = MaxSamples;
            waveQ.MaxSamples = MaxSamples;
        }


        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (ShmemChannel != null)
            {
                ThreadActive = false;
                Thread.Sleep(10);
                ReadThread.Abort();
                ShmemChannel.Unregister();
                ShmemChannel = null;

                btnOpen.Text = "Open";
            }
            else
            {
                ShmemChannel = new SharedMem(0, -1, "C# Test");
                ThreadActive = true;
                ReadThread = new Thread(new ThreadStart(FFTReadFunc));
                ReadThread.Start();

                btnOpen.Text = "Close";
            }
        }


        void FFTReadFunc()
        {
            byte[] inBuffer = new byte[BlockSize * 4];

            while (ThreadActive)
            {
                //dev.Read(inBuffer);
                int read = ShmemChannel.Read(inBuffer, 0, inBuffer.Length);

                if ( read != inBuffer.Length)
                {
                    return;
                }

                if (!allPaused)
                {
                    if (!iqPaused)
                    {
                        waveI.ProcessData(inBuffer, 2, 1);
                        waveQ.ProcessData(inBuffer, 2, 2);
                    }

                    if (!phasePaused)
                        phaseDisplay.ProcessData(inBuffer);
                    if (!diffPaused)
                        differenciateDisplay.ProcessData(inBuffer, ShmemChannel);
                }
            }
        }

        private void btnPhase_Click(object sender, EventArgs e)
        {
            phasePaused = !phasePaused;
        }

        private void btnDiff_Click(object sender, EventArgs e)
        {
            diffPaused = !diffPaused;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            allPaused = !allPaused;
        }

        private void btnIQ_Click(object sender, EventArgs e)
        {
            iqPaused = !iqPaused;
        }

        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            phaseDisplay.StartSample = e.NewValue;
            differenciateDisplay.StartSample = e.NewValue;
            waveI.StartSample = e.NewValue;
            waveQ.StartSample = e.NewValue;
        }

    }
}
