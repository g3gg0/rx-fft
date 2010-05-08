namespace RX_Analyzer.Views
{
    partial class SignalStrengthPlot
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.waveForm = new LibRXFFT.Components.DirectX.DirectXWaveformDisplay();
            this.SuspendLayout();
            // 
            // waveForm
            // 
            this.waveForm.CenterFrequency = 0;
            this.waveForm.ColorBG = System.Drawing.Color.Black;
            this.waveForm.ColorCursor = System.Drawing.Color.Red;
            this.waveForm.ColorFG = System.Drawing.Color.Cyan;
            this.waveForm.ColorFont = System.Drawing.Color.DarkCyan;
            this.waveForm.ColorOverview = System.Drawing.Color.Red;
            this.waveForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waveForm.Location = new System.Drawing.Point(0, 0);
            this.waveForm.MaxSamples = 8192;
            this.waveForm.Name = "waveForm";
            this.waveForm.SamplingRate = 0;
            this.waveForm.Size = new System.Drawing.Size(622, 387);
            this.waveForm.SpectParts = 1;
            this.waveForm.TabIndex = 0;
            this.waveForm.UpdateRate = 62.5;
            this.waveForm.XZoomFactor = 1;
            this.waveForm.XZoomFactorMax = 20;
            this.waveForm.XZoomFactorMin = 1;
            this.waveForm.YZoomFactor = 1;
            this.waveForm.YZoomFactorMax = 50;
            this.waveForm.YZoomFactorMin = 0.001;
            this.waveForm.RealTimeMode = true;
            // 
            // SignalStrengthPlot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 387);
            this.Controls.Add(this.waveForm);
            this.Name = "SignalStrengthPlot";
            this.Text = "SignalPlot";
            this.ResumeLayout(false);

        }

        #endregion

        private LibRXFFT.Components.DirectX.DirectXWaveformDisplay waveForm;
    }
}