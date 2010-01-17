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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BurstVisualizer));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.SampleDisplay = new LibRXFFT.Components.DirectX.DirectXBurstDisplay();
            this.StrengthDisplay = new LibRXFFT.Components.DirectX.DirectXWaveformDisplayOld();
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
            this.splitContainer1.Panel1.Controls.Add(this.SampleDisplay);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.StrengthDisplay);
            this.splitContainer1.Size = new System.Drawing.Size(873, 489);
            this.splitContainer1.SplitterDistance = 232;
            this.splitContainer1.TabIndex = 0;
            // 
            // SampleDisplay
            // 
            this.SampleDisplay.ColorBG = System.Drawing.Color.Black;
            this.SampleDisplay.ColorFG = System.Drawing.Color.Cyan;
            this.SampleDisplay.ColorFont = System.Drawing.Color.DarkCyan;
            this.SampleDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SampleDisplay.Location = new System.Drawing.Point(0, 0);
            this.SampleDisplay.MaxSamples = 10000;
            this.SampleDisplay.Name = "SampleDisplay";
            this.SampleDisplay.MainText = "Sample Value";
            this.SampleDisplay.Size = new System.Drawing.Size(873, 232);
            this.SampleDisplay.TabIndex = 0;
            this.SampleDisplay.XZoomFactor = 2;
            this.SampleDisplay.YZoomFactor = 1;
            // 
            // StrengthDisplay
            // 
            this.StrengthDisplay.ColorBG = System.Drawing.Color.Black;
            this.StrengthDisplay.ColorFG = System.Drawing.Color.Cyan;
            this.StrengthDisplay.ColorFont = System.Drawing.Color.DarkCyan;
            this.StrengthDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StrengthDisplay.Location = new System.Drawing.Point(0, 0);
            this.StrengthDisplay.MaxSamples = 10000;
            this.StrengthDisplay.Name = "StrengthDisplay";
            this.StrengthDisplay.MainText = "Strength";
            this.StrengthDisplay.Size = new System.Drawing.Size(873, 253);
            this.StrengthDisplay.TabIndex = 0;
            this.StrengthDisplay.XZoomFactor = 2;
            this.StrengthDisplay.YZoomFactor = 1;
            // 
            // BurstVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(873, 489);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BurstVisualizer";
            this.Text = "BurstVisualizer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        public DirectXBurstDisplay SampleDisplay;
        private DirectXWaveformDisplayOld StrengthDisplay;
    }
}