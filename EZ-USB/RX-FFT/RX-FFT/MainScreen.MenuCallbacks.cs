using LibRXFFT.Components;
using System.Windows.Forms;
using System;
using System.Threading;
using RX_FFT.Dialogs;
using GSM_Analyzer;
using RX_FFT.DeviceControls;

namespace RX_FFT
{
    partial class MainScreen
    {
        ToolTip faultTooltip = new ToolTip();

        private bool TryParseMenuText(ToolStripTextBox textBox, out int value)
        {
            int samples = 0;

            if (!int.TryParse(textBox.Text, out value))
            {
                faultTooltip.Active = false;
                faultTooltip.IsBalloon = true;
                faultTooltip.UseAnimation = false;
                faultTooltip.ShowAlways = true;
                faultTooltip.Active = true;

                faultTooltip.SetToolTip(fftSizeOtherMenu.Control, "Invalid value entered");
                textBox.Control.Hide();
                textBox.Control.Show();

                return false;
            }
            else
            {
                optionsMenu.HideDropDown();
                faultTooltip.Active = false;

                return true;
            }
        }

        private bool TryParseMenuText(ToolStripTextBox textBox, out double value)
        {
            double samples = 0;

            if (!double.TryParse(textBox.Text, out value))
            {
                faultTooltip.Active = false;
                faultTooltip.IsBalloon = true;
                faultTooltip.UseAnimation = false;
                faultTooltip.ShowAlways = true;
                faultTooltip.Active = true;

                faultTooltip.SetToolTip(fftSizeOtherMenu.Control, "Invalid value entered");
                textBox.Control.Hide();
                textBox.Control.Show();

                return false;
            }
            else
            {
                optionsMenu.HideDropDown();
                faultTooltip.Active = false;

                return true;
            }
        }
        
        private void pauseMenu_Click(object sender, EventArgs e)
        {
            processPaused = !processPaused;
            pauseMenu.Checked = processPaused;
        }

        private void closeMenu_Click(object sender, EventArgs e)
        {
            /* pause transfers and finish threads */
            processPaused = true;
            ReadThreadRun = false;
            AudioThreadRun = false;

            lock (DemodOptions)
            {
                DemodOptions.SoundDevice.Close();
                DemodOptions.SoundDevice = null;
            }

            if (DemodDialog != null)
            {
                //DemodDialog.Close();
                DemodDialog = null;
            }

            if (ReadThread != null)
                ReadThread.Abort();
            if (AudioThread != null)
                AudioThread.Abort();

            if (AudioShmem != null)
            {
                AudioShmem.Close();
                AudioShmem = null;
            }

            Device.Close();
            Device = null;

            DeviceOpened = false;

            /* un-pause again */
            processPaused = false;
            pauseMenu.Checked = processPaused;

        }

        private void performanceStatisticsMenu_Click(object sender, EventArgs e)
        {
            if (StatsDialog != null)
            {
                StatsDialog.Close();
                StatsDialog = null;
            }
            else
            {
                StatsDialog = new PerformaceStatsDialog(PerformanceCounters);
                StatsDialog.Show();
            }
        }

        private void demodulationMenu_Click(object sender, EventArgs e)
        {
            if (DemodDialog != null)
            {
                //DemodDialog.Close();
                DemodDialog = null;
            }
            else
            {
                DemodDialog = new DemodulationDialog(DemodOptions);
                DemodDialog.Show();
            }
        }

        #region Waterfall Recording Options

        private void waterfallRecordingEnabledMenu_Click(object sender, EventArgs e)
        {
            if (FFTDisplay.SavingEnabled)
            {
                FFTDisplay.SavingEnabled = false;
                waterfallRecordingEnabledMenu.Checked = false;
            }
            else
            {
                FFTDisplay.SavingEnabled = true;
                waterfallRecordingEnabledMenu.Checked = true;
            }
        }

        private void waterfallRecordingSaveAsMenu_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "PNG Files (*.png)|*.png|All files (*.*)|*.*";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FFTDisplay.SavingName = dlg.FileName;
            }
        }

        #endregion

        #region FFT Size Menu

        private void fftSize512Menu_Click(object sender, EventArgs e)
        {
            FFTSize = 512;
        }

        private void fftSize1024Menu_Click(object sender, EventArgs e)
        {
            FFTSize = 1024;
        }

        private void fftSize2048Menu_Click(object sender, EventArgs e)
        {
            FFTSize = 2048;
        }

        private void fftSize4096Menu_Click(object sender, EventArgs e)
        {
            FFTSize = 4096;
        }

        private void fftSize8192Menu_Click(object sender, EventArgs e)
        {
            FFTSize = 8192;
        }

        private void fftSize16384Menu_Click(object sender, EventArgs e)
        {
            FFTSize = 16384;
        }

        private void fftSizeOtherMenu_Click(object sender, EventArgs e)
        {
            long samples = 0;

            if (!long.TryParse(fftSizeOtherMenu.Text, out samples))
            {
                fftSizeOtherMenu.Text = "";
            }
        }
        
        private void fftSizeOtherMenu_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e == null || e.KeyChar == 0x0D)
            {
                int samples = 0;
                if (TryParseMenuText(fftSizeOtherMenu, out samples))
                {
                    FFTSize = samples;
                    if (e != null)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        #endregion

        #region Additional Numerical Options
        private void verticalSmoothMenuText_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e == null || e.KeyChar == 0x0D)
            {
                double rate = 0;
                if (TryParseMenuText(verticalSmoothMenuText, out rate))
                {
                    FFTDisplay.VerticalSmooth = rate;
                    if (e != null)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void averageSamplesText_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e == null || e.KeyChar == 0x0D)
            {
                int rate = 0;
                if (TryParseMenuText(averageSamplesText, out rate))
                {
                    SamplesToAverage = rate;
                    if (e != null)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void updateRateText_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e == null || e.KeyChar == 0x0D)
            {
                double rate = 0;
                if (TryParseMenuText(updateRateText, out rate))
                {
                    FFTDisplay.UpdateRate = rate;
                    if (e != null)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        #endregion

        private void gmAnalyzerMenu_Click(object sender, EventArgs e)
        {
            GSMAnalyzer analyzer = new GSMAnalyzer();

            analyzer.Show();
            if (Device != null)
            {
                analyzer.OpenSharedMem(((USBRXDeviceControl)Device).ShmemChannel);
            }
        }

    }
}