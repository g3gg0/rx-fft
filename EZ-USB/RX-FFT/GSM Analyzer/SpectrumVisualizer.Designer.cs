namespace GSM_Analyzer
{
    partial class SpectrumVisualizer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpectrumVisualizer));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cmbFFTSize = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbAverage = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbWindowFunc = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.FFTDisplay = new LibRXFFT.Components.DirectX.DirectXWaterfallFFTDisplay();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.cmbFFTSize);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.cmbAverage);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.cmbWindowFunc);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.FFTDisplay);
            this.splitContainer1.Size = new System.Drawing.Size(851, 596);
            this.splitContainer1.SplitterDistance = 31;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 4;
            // 
            // cmbFFTSize
            // 
            this.cmbFFTSize.FormattingEnabled = true;
            this.cmbFFTSize.Items.AddRange(new object[] {
            "256",
            "512",
            "1024",
            "2048",
            "4096",
            "8192",
            "16384"});
            this.cmbFFTSize.Location = new System.Drawing.Point(65, 5);
            this.cmbFFTSize.Name = "cmbFFTSize";
            this.cmbFFTSize.Size = new System.Drawing.Size(96, 21);
            this.cmbFFTSize.TabIndex = 9;
            this.cmbFFTSize.TextChanged += new System.EventHandler(this.cmbFFTSize_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "FFT Size:";
            // 
            // cmbAverage
            // 
            this.cmbAverage.FormattingEnabled = true;
            this.cmbAverage.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
            this.cmbAverage.Location = new System.Drawing.Point(500, 5);
            this.cmbAverage.Name = "cmbAverage";
            this.cmbAverage.Size = new System.Drawing.Size(121, 21);
            this.cmbAverage.TabIndex = 7;
            this.cmbAverage.TextChanged += new System.EventHandler(this.cmbAverage_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(410, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Average Factor:";
            // 
            // cmbWindowFunc
            // 
            this.cmbWindowFunc.FormattingEnabled = true;
            this.cmbWindowFunc.Location = new System.Drawing.Point(280, 5);
            this.cmbWindowFunc.Name = "cmbWindowFunc";
            this.cmbWindowFunc.Size = new System.Drawing.Size(121, 21);
            this.cmbWindowFunc.TabIndex = 5;
            this.cmbWindowFunc.TextChanged += new System.EventHandler(this.cmbWindowFunc_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(167, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Windowing Function:";
            // 
            // FFTDisplay
            // 
            this.FFTDisplay.VerticalSmooth = 1;
            this.FFTDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FFTDisplay.FFTSize = 256;
            this.FFTDisplay.Location = new System.Drawing.Point(0, 0);
            this.FFTDisplay.Name = "FFTDisplay";
            this.FFTDisplay.Size = new System.Drawing.Size(851, 564);
            this.FFTDisplay.TabIndex = 0;
            this.FFTDisplay.WindowingFunction = LibRXFFT.Libraries.FFTW.FFTTransformer.eWindowingFunction.BlackmanHarris;
            // 
            // SpectrumVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 596);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SpectrumVisualizer";
            this.Text = "Spectrum Visualizer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion




        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ComboBox cmbWindowFunc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbAverage;
        private System.Windows.Forms.Label label2;
        private LibRXFFT.Components.DirectX.DirectXWaterfallFFTDisplay FFTDisplay;
        private System.Windows.Forms.ComboBox cmbFFTSize;
        private System.Windows.Forms.Label label3;
    }
}