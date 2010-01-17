using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.SignalProcessing;

namespace RX_Oscilloscope.Components
{
    public partial class Oscilloscope : UserControl
    {
        private bool TriggerRising = false;
        private bool TriggerFalling = false;
        private double TriggerLevel = 0.0d;
        private double LastLevel = 0.0d;

        private int TriggeredSamples = 0;
        private int SamplesTotal = 10000;
        private int SamplesPreTrigger = 0;
        private bool Triggered = false;

        /* when this is set, force waveform display to draw all samples */
        private bool ForceShowSamples = false;

        private LabelledLine TriggerLevelBar = new LabelledLine("Trig lvl", -1000, Color.BlueViolet);
        private LabelledLine TriggerSampleBar = new LabelledLine("Trig", -1000, Color.Yellow);

        private IIRFilter LowPass = null;

        public Oscilloscope()
        {
            InitializeComponent();

            cmbLowPass.SelectedIndex = 0;

            waveForm.KeepText = true;
            waveForm.MaxSamples = SamplesTotal;
            waveForm.LabelledHorLines.AddLast(TriggerLevelBar);
            waveForm.LabelledVertLines.AddLast(TriggerSampleBar);
        }

        public double SamplingRate
        {
            get
            {
                return waveForm.SamplingRate;
            }

            set
            {
                waveForm.SamplingRate = value;

                try
                {
                    BeginInvoke(new MethodInvoker(delegate()
                    {
                        txtSamplingRate.Frequency = (long)value;
                        UpdateSampleTimes();
                    }));
                }
                catch (Exception)
                {}
            }
        }

        private double TriggerSamplePos
        {
            get
            {
                return TriggerSampleBar.Position;
            }
            set
            {
            }
        }

        internal void Process(double I, double Q)
        {
            double level;

            if (LowPass != null)
            {
                level = Math.Sqrt(I * I + Q * Q);
                level = LowPass.ProcessSample(level);
                level = DBTools.SampleTodB(level);
            }
            else
            {
                level = I * I + Q * Q;
                level = DBTools.SquaredSampleTodB(level);
            }


            if (!Triggered)
            {
                if (TriggerRising)
                {
                    bool lastOver = LastLevel > TriggerLevel;
                    bool nowOver = level > TriggerLevel;

                    if (!lastOver && nowOver)
                    {
                        TriggeredSamples = 0;
                        Triggered = true;
                        waveForm.MainText = "Triggered!";
                    }
                }

                if (TriggerFalling)
                {
                    bool lastOver = LastLevel > TriggerLevel;
                    bool nowOver = level > TriggerLevel;

                    if (lastOver && !nowOver)
                    {
                        TriggeredSamples = 0;
                        Triggered = true;
                        waveForm.MainText = "Triggered!";
                    }
                }

                LastLevel = level;
                waveForm.ProcessData(level, ForceShowSamples);
            }
            else
            {
                if (TriggeredSamples < SamplesTotal / 2 - SamplesPreTrigger)
                {
                    waveForm.ProcessData(level, Triggered);
                    TriggeredSamples++;
                }
            }
        }

        private void txtSamplingRate_FrequencyChanged(object sender, System.EventArgs e)
        {
            if (SamplingRate != txtSamplingRate.Frequency)
            {
                SamplingRate = txtSamplingRate.Frequency;
            }
        }


        private void txtBufferTime_ValueChanged(object sender, System.EventArgs e)
        {
            SamplesTotal = (int)txtBufferTime.Value;
            waveForm.MaxSamples = SamplesTotal;

            UpdateSampleTimes();
        }

        private void txtPreTrigSamples_ValueChanged(object sender, System.EventArgs e)
        {
            SamplesPreTrigger = (int)txtPreTrigSamples.Value;

            UpdateSampleTimes();
            UpdateTriggerBars();
        }

        private void txtTriggerLevel_ValueChanged(object sender, System.EventArgs e)
        {
            TriggerLevel = (double)txtTriggerLevel.Value;

            UpdateTriggerBars();
        }

        private void chkTriggerRising_CheckedChanged(object sender, EventArgs e)
        {
            TriggerRising = chkTriggerRising.Checked;

            UpdateTriggerBars();
        }

        private void chkTriggerFalling_CheckedChanged(object sender, EventArgs e)
        {
            Triggered = false;
            TriggerFalling = chkTriggerFalling.Checked;

            UpdateTriggerBars();
        }

        private void UpdateSampleTimes()
        {
            double rate = waveForm.SamplingRate;
            if (rate != 0)
            {
                lblBufferTime.Text = FrequencyFormatter.TimeToString(SamplesTotal / rate);
                lblPreTrigTime.Text = FrequencyFormatter.TimeToString(SamplesPreTrigger / rate);
            }

            ForceShowSamples = SamplesPreTrigger > (SamplesTotal / 2);
        }

        private void UpdateTriggerBars()
        {
            if (TriggerRising || TriggerFalling)
            {
                waveForm.MainText = "Armed...";
                TriggerLevelBar.Position = TriggerLevel;
                TriggerSampleBar.Position = SamplesTotal / 2 + SamplesPreTrigger;
                waveForm.UpdateOverlays = true;
            }
            else
            {
                waveForm.MainText = "";
                TriggerLevelBar.Position = -1000;
                TriggerSampleBar.Position = -1000;
                waveForm.UpdateOverlays = true;
            }

            Triggered = false;
        }

        private void cmbLowPass_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbLowPass.SelectedIndex)
            {
                case 0:
                    LowPass = null;
                    break;
                case 1:
                    LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_2);
                    break;
                case 2:
                    LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_4);
                    break;
                case 3:
                    LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_8);
                    break;
                case 4:
                    LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_16);
                    break;
                case 5:
                    LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_32);
                    break;
                case 6:
                    LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_64);
                    break;
                case 7:
                    LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_128);
                    break;
                case 8:
                    LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_256);
                    break;
            }
        }
    }
}
