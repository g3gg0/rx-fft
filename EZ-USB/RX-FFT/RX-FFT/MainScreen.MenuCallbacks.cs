using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using DemodulatorCollection;
using GSM_Analyzer;
using LibRXFFT.Components.DirectX;
using LibRXFFT.Libraries;
using RX_FFT.DeviceControls;
using RX_FFT.Dialogs;
using RX_Oscilloscope;

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
            ProcessPaused = !ProcessPaused;
            pauseMenu.Checked = ProcessPaused;
            if (ProcessPaused)
            {
                statusLabel.Text += " (Paused)";
            }
            else
            {
                statusLabel.Text = statusLabel.Text.Replace(" (Paused)", "");
            }
        }

        private void closeMenu_Click(object sender, EventArgs e)
        {
            StopThreads();
            CloseDevice();
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

        private void scanBandMenu_Click(object sender, EventArgs e)
        {
            if(!DeviceOpened)
                return;

            FrequencyBand band = new FrequencyBand();
            band.BaseFrequency = 935014000;
            band.ChannelStart = 2;
            band.ChannelEnd = 124;
            band.ChannelDistance = 200000;

            FrequencyBandDetailsDialog dlg = new FrequencyBandDetailsDialog(band);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ScanFrequencies.Clear();
                for (long channel = band.ChannelStart; channel <= band.ChannelEnd; channel++)
                {
                    ScanFrequencies.AddLast(new FrequencyMarker("Ch. " + channel, "", band.BaseFrequency + (channel - band.ChannelStart) * band.ChannelDistance));
                }

                CurrentScanFreq = ScanFrequencies.First;
                ChannelBandDetails = band;

                ScanStrongestFrequency = false;
                ScanFrequenciesEnabled = true;
            }
            else
            {
                ScanFrequenciesEnabled = false;
            }
        }

        private void scanMarkersMenu_Click(object sender, EventArgs e)
        {
            
            ScanFrequencies.Clear();
            foreach (FrequencyMarker marker in MarkerList.Markers)
            {
                ScanFrequencies.AddLast(marker);
            }

            CurrentScanFreq = ScanFrequencies.First;
            ChannelBandDetails = new FrequencyBand();
            ScanStrongestFrequency = true;
            ScanFrequenciesEnabled = true;
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


        private void quitMenu_Click(object sender, EventArgs e)
        {
            StopThreads();
            CloseDevice();
            Close();
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
        
        private void fftSizeOtherMenu_KeyPress(object sender, KeyPressEventArgs e)
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
        private void verticalSmoothMenuText_KeyPress(object sender, KeyPressEventArgs e)
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

        private void averageSamplesText_KeyPress(object sender, KeyPressEventArgs e)
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

        private void updateRateText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e == null || e.KeyChar == 0x0D)
            {
                double rate = 0;
                if (TryParseMenuText(updateRateText, out rate))
                {
                    UpdateRate = rate;

                    if (e != null)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        #endregion


        private void agcMenu_Click(object sender, EventArgs e)
        {
            AGCEnabled = !AGCEnabled;
            agcMenu.Checked = AGCEnabled;
        }

        private void dynamicWaterfallMenu_Click(object sender, EventArgs e)
        {
            dynamicWaterfallMenu.Checked = !dynamicWaterfallMenu.Checked;
            FFTDisplay.DynamicLimits = dynamicWaterfallMenu.Checked;
        }

        private void fitSpectrumMenu_Click(object sender, EventArgs e)
        {
            FitSpectrum = !FitSpectrum;

            fitSpectrumMenu.Checked = FitSpectrum;
            displayFilterMarginsMenu.Checked = DisplayFilterMargins;
            displayFilterMarginsMenu.Enabled = !FitSpectrum;
        }

        private void displayFilterMarginsMenu_Click(object sender, EventArgs e)
        {
            DisplayFilterMargins = !DisplayFilterMargins;

            fitSpectrumMenu.Checked = FitSpectrum;
            displayFilterMarginsMenu.Checked = DisplayFilterMargins;
            displayFilterMarginsMenu.Enabled = !FitSpectrum;
        }

        private void saveMenu_Click(object sender, EventArgs e)
        {
            if (!DeviceOpened)
            {
                MessageBox.Show("Open a device or file first.");
                return;
            }
            if (Device.SampleSource.SavingEnabled)
            {
                Device.SampleSource.SavingEnabled = false;
                saveMenu.Text = "Save digital data...";
            }
            else
            {
                try
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "Raw complex data files (*.dat)|*.dat|Stereo WAV file (*.wav)|*.wav|USRP CFile (*.cfile)|*.cfile|All files (*.*)|*.*";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        EnableSaving(dlg.FileName);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void gsmAnalyzerMenu_Click(object sender, EventArgs e)
        {
            if (!DeviceOpened)
            {
                MessageBox.Show("Open a device or file first.");
                return;
            }
            if (!Device.AllowsMultipleReaders)
            {
                MessageBox.Show("Further analysis is not possible with the opened device.");
                return;
            }

            if (GsmAnalyzerWindow == null || GsmAnalyzerWindow.IsDisposed)
            {
                GsmAnalyzerWindow = new GSMAnalyzer();
            }

            GsmAnalyzerWindow.Show();
            if (Device != null)
            {
                GsmAnalyzerWindow.OpenSharedMem(Device.ShmemChannel);
                GsmAnalyzerWindow.Device = Device;
            }
        }

        private void digitalDemodulatorsMenu_Click(object sender, EventArgs e)
        {
            if (!DeviceOpened)
            {
                MessageBox.Show("Open a device or file first.");
                return;
            }
            if (!Device.AllowsMultipleReaders)
            {
                MessageBox.Show("Further analysis is not possible with the opened device.");
                return;
            }

            if (DemodulatorWindow == null || DemodulatorWindow.IsDisposed)
            {
                DemodulatorWindow = new DemodulatorDialog();
            }

            DemodulatorWindow.Show();
            if (Device != null)
            {
                DemodulatorWindow.OpenSharedMem(Device.ShmemChannel);
            }
        }


        private void oscilloscopeMenu_Click(object sender, EventArgs e)
        {
            if (!DeviceOpened)
            {
                MessageBox.Show("Open a device or file first.");
                return;
            }

            if (!Device.AllowsMultipleReaders)
            {
                MessageBox.Show("Further analysis is not possible with the opened device.");
                return;
            }

            if (OscilloscopeWindow == null || OscilloscopeWindow.IsDisposed)
            {
                OscilloscopeWindow = new RXOscilloscope();
            }

            OscilloscopeWindow.Show();
            if (Device != null)
            {
                OscilloscopeWindow.OpenSharedMem(Device.ShmemChannel);
            }
        }

        private DateTime RetrieveLinkerTimestamp()
        {
            string filePath = Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            Stream s = null;

            try
            {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }

        private void deviceInformationMenu_Click(object sender, EventArgs e)
        {
            string msg = "";

            if (DeviceOpened)
            {
                msg += " Device details:" + Environment.NewLine;
                msg += "-------------------" + Environment.NewLine;
                msg += "    Name:" + Environment.NewLine;
                foreach (string line in Device.Name)
                {
                    msg += "        " + line + Environment.NewLine;
                }
                msg += Environment.NewLine;

                msg += "    Description:  " + (Device.InvertedSpectrum ? "(inverted)" : "(non inverted)") + Environment.NewLine;
                foreach (string line in Device.Description)
                {
                    msg += "        " + line + Environment.NewLine;
                }
                msg += Environment.NewLine;

                msg += "    Details:" + Environment.NewLine;
                foreach (string line in Device.Details)
                {
                    msg += "        " + line + Environment.NewLine;
                }
                msg += Environment.NewLine;
            }

            msg += " Graphic details:" + Environment.NewLine;
            msg += "-------------------" + Environment.NewLine;
            foreach(string line in FFTDisplay.DisplayInformation)
            {
                msg += "    " + line + Environment.NewLine;
            }

            MessageBox.Show(msg);
        }

        private void aboutMenu_Click(object sender, EventArgs e)
        {
            MessageBox.Show("RX-FFT Prototype" + Environment.NewLine + "Built: " + RetrieveLinkerTimestamp());
        }
    }
}