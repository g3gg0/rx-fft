using LibRXFFT.Components.DirectX;
using LibRXFFT.Components.GDI;

namespace GSM_Analyzer
{
    partial class BurstVisualizer
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.waveformDisplay1 = new DirectXWaveformDisplay();
            this.waveformDisplay2 = new DirectXWaveformDisplay();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.waveformDisplay1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.waveformDisplay2);
            this.splitContainer1.Size = new System.Drawing.Size(873, 489);
            this.splitContainer1.SplitterDistance = 232;
            this.splitContainer1.TabIndex = 0;
            // 
            // waveformDisplay1
            // 
            this.waveformDisplay1.DisplayName = null;
            this.waveformDisplay1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waveformDisplay1.Location = new System.Drawing.Point(0, 0);
            this.waveformDisplay1.MaxSamples = 10000;
            this.waveformDisplay1.Name = "waveformDisplay1";
            this.waveformDisplay1.ShowFPS = false;
            this.waveformDisplay1.Size = new System.Drawing.Size(873, 232);
            this.waveformDisplay1.StartSample = 0;
            this.waveformDisplay1.TabIndex = 0;
            this.waveformDisplay1.UseLines = true;
            this.waveformDisplay1.YZoomFactor = 1;
            // 
            // waveformDisplay2
            // 
            this.waveformDisplay2.DisplayName = null;
            this.waveformDisplay2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waveformDisplay2.Location = new System.Drawing.Point(0, 0);
            this.waveformDisplay2.MaxSamples = 10000;
            this.waveformDisplay2.Name = "waveformDisplay2";
            this.waveformDisplay2.ShowFPS = false;
            this.waveformDisplay2.Size = new System.Drawing.Size(873, 253);
            this.waveformDisplay2.StartSample = 0;
            this.waveformDisplay2.TabIndex = 0;
            this.waveformDisplay2.UseLines = true;
            this.waveformDisplay2.YZoomFactor = 1;
            // 
            // BurstVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(873, 489);
            this.Controls.Add(this.splitContainer1);
            this.Name = "BurstVisualizer";
            this.Text = "BurstVisualizer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private DirectXWaveformDisplay waveformDisplay1;
        private DirectXWaveformDisplay waveformDisplay2;
    }
}