using System;
using System.Drawing;
using System.Windows.Forms;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries.Filters;
using LibRXFFT.Libraries.Misc;
using LibRXFFT.Libraries.SignalProcessing;
using LibRXFFT.Components.DirectX.Drawables;
using LibRXFFT.Components.DirectX.Drawables.Docks;

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

        private long SampleDistance = 1;
        public IQPlot iqPlot = null;

        private decimal SamplesExact = 0;
        /* number of samples to throw away "per sample" */
        private decimal SamplesThrowAwayDistance = 0;
        /* used for throwing away samples to round sampling rate */
        private decimal SamplesThrowAwayCounter = 0;

        enum eDisplayMode
        {
            Power,
            Phase,
            DeltaPhase
        }
        private eDisplayMode DisplayPhase = eDisplayMode.Power;

        /* when this is set, force waveform display to draw all samples */
        private bool ForceShowSamples = false;

        private LabelledLine TriggerLevelBar = new LabelledLine("Trig lvl", -1000, Color.BlueViolet);
        private LabelledLine TriggerSampleBar = new LabelledLine("Trig", -1000, Color.Yellow);
        private PlotVertsHistory History;

        private IIRFilter LowPass = null;
        private IIRFilter LowPassI = null;
        private IIRFilter LowPassQ = null;
        private double LastPhase;

        public Oscilloscope()
        {
            InitializeComponent();

            cmbLowPass.SelectedIndex = 0;

            waveForm.KeepText = true;
            waveForm.MaxSamples = SamplesTotal;
            waveForm.LabelledHorLines.AddLast(TriggerLevelBar);
            waveForm.LabelledVertLines.AddLast(TriggerSampleBar);

            History = new PlotVertsHistory(waveForm);
            History.HistLength = 1;

            DockPanel panel = new DockPanel(waveForm, eOrientation.RightBorder);
            new DensityMap(panel).Granularity = 32;
            new WaveformAreaSelectionDetails(new WaveformAreaSelection(waveForm), panel);

            UpdateScale();
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

                if (InvokeRequired)
                {
                    try
                    {
                        BeginInvoke(new MethodInvoker(delegate()
                        {
                            SamplingRate = value;
                        }));
                    }
                    catch (Exception)
                    { }
                }
                else
                {
                    txtSamplingRate.Frequency = (long)value;
                    UpdateSampleTimes();
                }
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

        private double FastAtan2b(double y, double x)
        {
            const double ONEQTR_PI = Math.PI / 4.0f;
            const double THRQTR_PI = 3.0f * Math.PI / 4.0f;
            double r;
            double angle;
            double abs_y = Math.Abs(y);

            if (x < 0.0f)
            {
                r = (x + abs_y) / (abs_y - x);
                angle = THRQTR_PI;
            }
            else
            {
                r = (x - abs_y) / (x + abs_y);
                angle = ONEQTR_PI;
            }

            angle += (0.1963f * r * r - 0.9817f) * r;

            return y < 0.0f ? -angle : angle;
        }

        internal void Process(double I, double Q)
        {
            double level = 0;
            bool process = true;

            /* fine tune sampling rate */
            SamplesThrowAwayCounter += SamplesThrowAwayDistance;
            if (SamplesThrowAwayCounter >= 1)
            {
                SamplesThrowAwayCounter -= 1;
                process = false;
            }

            lock (this)
            {
                if (LowPassI != null && LowPassQ != null)
                {
                    I = LowPassI.ProcessSample(I);
                    Q = LowPassQ.ProcessSample(Q);
                }
            }

            if (DisplayPhase == eDisplayMode.DeltaPhase)
            {
                double phase;

                phase = UseFastAtan2 ? FastAtan2b(I, Q) : Math.Atan2(I, Q);

                while (phase - LastPhase < -(Math.PI / 2))
                {
                    phase += Math.PI;
                }

                while (phase - LastPhase > Math.PI / 2)
                {
                    phase -= Math.PI;
                }

                /* catch the case where I and Q are zero */
                if (double.IsNaN(phase))
                {
                    phase = LastPhase;
                }

                double diff = phase - LastPhase;

                level = diff;

                LastPhase = phase % (2 * Math.PI);
            }
            else if (DisplayPhase == eDisplayMode.Phase)
            {
                double phase;

                phase = UseFastAtan2 ? FastAtan2b(I, Q) : Math.Atan2(I, Q);

                while (phase - LastPhase < -(Math.PI / 2))
                {
                    phase += Math.PI;
                }

                while (phase - LastPhase > Math.PI / 2)
                {
                    phase -= Math.PI;
                }

                /* catch the case where I and Q are zero */
                if (double.IsNaN(phase))
                {
                    phase = LastPhase;
                }

                double diff = phase - LastPhase;

                level = phase;

                LastPhase = phase % (2 * Math.PI);
            }
            else
            {
                if (LowPass != null)
                {
                    level = Math.Sqrt(I * I + Q * Q);
                    //level = LowPass.ProcessSample(level);
                    level = DBTools.SampleTodB(level);
                }
                else
                {
                    level = I * I + Q * Q;
                    level = DBTools.SquaredSampleTodB(level);
                }
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
                if (process)
                {
                    waveForm.ProcessData(level, ForceShowSamples);
                    iqPlot.Process(I, Q);
                }
            }
            else
            {
                if (TriggeredSamples < SamplesTotal / 2 - SamplesPreTrigger)
                {
                    if (process)
                    {
                        waveForm.ProcessData(level, Triggered);
                        iqPlot.Process(I, Q);
                    }
                    TriggeredSamples++;
                }
            }
        }

        protected bool UseFastAtan2 { get; set; }

        private void txtSamplingRate_FrequencyChanged(object sender, EventArgs e)
        {
            if (SamplingRate != txtSamplingRate.Frequency)
            {
                SamplingRate = txtSamplingRate.Frequency;
            }
        }


        private void txtBufferTime_Changed(object sender, EventArgs e)
        {
            UpdateBufferSizes();
        }

        private void txtPreTrigSamples_ValueChanged(object sender, EventArgs e)
        {
            SamplesPreTrigger = (int)txtPreTrigSamples.Value;

            UpdateSampleTimes();
            UpdateTriggerBars();
        }

        private void txtTriggerLevel_ValueChanged(object sender, EventArgs e)
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
            lock (this)
            {
                switch (cmbLowPass.SelectedIndex)
                {
                    case 0:
                        LowPass = null;
                        LowPassI = null;
                        LowPassQ = null;
                        break;
                    case 1:
                        LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_2);
                        LowPassI = new IIRFilter(IIRCoefficients.IIRLowPass_2);
                        LowPassQ = new IIRFilter(IIRCoefficients.IIRLowPass_2);
                        break;
                    case 2:
                        LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_4);
                        LowPassI = new IIRFilter(IIRCoefficients.IIRLowPass_4);
                        LowPassQ = new IIRFilter(IIRCoefficients.IIRLowPass_4);
                        break;
                    case 3:
                        LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_8);
                        LowPassI = new IIRFilter(IIRCoefficients.IIRLowPass_8);
                        LowPassQ = new IIRFilter(IIRCoefficients.IIRLowPass_8);
                        break;
                    case 4:
                        LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_16);
                        LowPassI = new IIRFilter(IIRCoefficients.IIRLowPass_16);
                        LowPassQ = new IIRFilter(IIRCoefficients.IIRLowPass_16);
                        break;
                    case 5:
                        LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_32);
                        LowPassI = new IIRFilter(IIRCoefficients.IIRLowPass_32);
                        LowPassQ = new IIRFilter(IIRCoefficients.IIRLowPass_32);
                        break;
                    case 6:
                        LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_64);
                        LowPassI = new IIRFilter(IIRCoefficients.IIRLowPass_64);
                        LowPassQ = new IIRFilter(IIRCoefficients.IIRLowPass_64);
                        break;
                    case 7:
                        LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_128);
                        LowPassI = new IIRFilter(IIRCoefficients.IIRLowPass_128);
                        LowPassQ = new IIRFilter(IIRCoefficients.IIRLowPass_128);
                        break;
                    case 8:
                        LowPass = new IIRFilter(IIRCoefficients.IIRLowPass_256);
                        LowPassI = new IIRFilter(IIRCoefficients.IIRLowPass_256);
                        LowPassQ = new IIRFilter(IIRCoefficients.IIRLowPass_256);
                        break;
                }
            }
        }


        private void UpdateScale()
        {
            if (DisplayPhase == eDisplayMode.DeltaPhase)
            {
                waveForm.ScalePosMax = 10;
                waveForm.ScalePosMin = -10;
                waveForm.ScaleBarDistance = 1;
                waveForm.ScaleTextDistance = 2;
                waveForm.ScaleUnit = "rad/s";
            }
            else if (DisplayPhase == eDisplayMode.Phase)
            {
                waveForm.ScalePosMax = 10;
                waveForm.ScalePosMin = -10;
                waveForm.ScaleBarDistance = 1;
                waveForm.ScaleTextDistance = 2;
                waveForm.ScaleUnit = "rad";
            }
            else
            {
                waveForm.ScalePosMax = 0;
                waveForm.ScalePosMin = -120;
                waveForm.ScaleBarDistance = 10;
                waveForm.ScaleTextDistance = 20;
                waveForm.ScaleUnit = "dB";
            }

            waveForm.UpdateAxis = true;
            waveForm.UpdateCursor = true;
            waveForm.UpdateOverlays = true;
        }

        private void radioPower_CheckedChanged(object sender, EventArgs e)
        {
            if (radioPower.Checked)
            {
                DisplayPhase = eDisplayMode.Power;
            }
            UpdateScale();
        }
        private void radioPhase_CheckedChanged(object sender, EventArgs e)
        {
            if (radioPhase.Checked)
            {
                DisplayPhase = eDisplayMode.Phase;
            }
            UpdateScale();
        }

        private void radioDeltaPhase_CheckedChanged(object sender, EventArgs e)
        {
            if (radioDeltaPhase.Checked)
            {
                DisplayPhase = eDisplayMode.DeltaPhase;
            }
            UpdateScale();
        }

        private void txtEyePlotBlocks_ValueChanged(object sender, System.EventArgs e)
        {
            History.HistLength = txtEyePlotBlocks.Value;
            iqPlot.History.HistLength = txtEyePlotBlocks.Value;

            //waveForm.HistLength = txtEyePlotBlocks.Value;
        }

        private void chkEyePlot_CheckedChanged(object sender, EventArgs e)
        {
            txtEyePlotBlocks.Enabled = chkEyePlot.Checked;
            waveForm.RealTimeMode = chkEyePlot.Checked;
            iqPlot.waveForm.RealTimeMode = chkEyePlot.Checked;
            History.HistLength = txtEyePlotBlocks.Value;
            History.Enabled = chkEyePlot.Checked;
            iqPlot.History.HistLength = txtEyePlotBlocks.Value;
            iqPlot.History.Enabled = chkEyePlot.Checked;
        }

        private void radioBufferTime_CheckedChanged(object sender, EventArgs e)
        {
            UpdateBufferSizes();
        }

        private void radioBufferSamples_CheckedChanged(object sender, EventArgs e)
        {
            UpdateBufferSizes();
        }

        private void UpdateBufferSizes()
        {
            decimal rate = (decimal)waveForm.SamplingRate;

            if(rate == 0)
            {
                return;
            }

            try
            {
                if (radioBufferSamples.Checked)
                {
                    SamplesExact = txtBufferSamples.Value;

                    /* calculate the number of samples being shown and the number of samples to throw away */
                    SamplesTotal = (int)SamplesExact;
                    SamplesThrowAwayDistance = (SamplesExact - SamplesTotal) / SamplesTotal;

                    txtBufferTime.Value = (SamplesExact / rate) * 1000000.0m;
                }
                else
                {
                    decimal time = 0;
                    decimal.TryParse(txtBufferTime.Text, out time);
                    decimal samples = rate * time / 1000000.0m;

                    if(samples == 0)
                    {
                        return;
                    }

                    SamplesExact = samples;

                    /* calculate the number of samples being shown and the number of samples to throw away */
                    SamplesTotal = (int)SamplesExact;
                    SamplesThrowAwayDistance = (SamplesExact - SamplesTotal) / SamplesTotal;

                    txtBufferSamples.Value = SamplesExact;
                }
                waveForm.MaxSamples = SamplesTotal;
                iqPlot.waveForm.MaxSamples = SamplesTotal;
            }
            catch (Exception ex)
            {
            }
            UpdateSampleTimes();
        }

        private void txtSampling_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                History.SampleDist = txtSamplingDistance.Value;
                History.SamplePos = txtSamplingTime.Value;
                iqPlot.History.SampleDist = txtSamplingDistance.Value;
                iqPlot.History.SamplePos = txtSamplingTime.Value;
            }
            catch (Exception ex)
            {
            }

        }
    }
}
