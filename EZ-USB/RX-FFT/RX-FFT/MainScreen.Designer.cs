using LibRXFFT.Components;

namespace RX_FFT
{
    partial class MainScreen
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (ReadThread != null && ReadThread.IsAlive)
            {
                ThreadActive = false;
                ReadThread.Abort();
                ReadThread.Join();
            }

            if (ShmemChannel != null)
                ShmemChannel.Unregister();

            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtSamplingRate = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbFFTSize = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbAverage = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbWindowFunc = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.FFTDisplay = new LibRXFFT.Components.DirectX.DirectXWaterfallFFTDisplay();
            this.txtUpdatesPerSecond = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(3, 3);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(84, 3);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 3;
            this.btnPause.Text = "Pause/Cont";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.txtUpdatesPerSecond);
            this.splitContainer1.Panel1.Controls.Add(this.txtSamplingRate);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.cmbFFTSize);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.cmbAverage);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.cmbWindowFunc);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.btnOpen);
            this.splitContainer1.Panel1.Controls.Add(this.btnPause);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.FFTDisplay);
            this.splitContainer1.Size = new System.Drawing.Size(1002, 596);
            this.splitContainer1.SplitterDistance = 51;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 4;
            // 
            // txtSamplingRate
            // 
            this.txtSamplingRate.Location = new System.Drawing.Point(877, 4);
            this.txtSamplingRate.Name = "txtSamplingRate";
            this.txtSamplingRate.Size = new System.Drawing.Size(100, 20);
            this.txtSamplingRate.TabIndex = 11;
            this.txtSamplingRate.TextChanged += new System.EventHandler(this.txtSamplingRate_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(792, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Sampling Rate:";
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
            this.cmbFFTSize.Location = new System.Drawing.Point(243, 3);
            this.cmbFFTSize.Name = "cmbFFTSize";
            this.cmbFFTSize.Size = new System.Drawing.Size(78, 21);
            this.cmbFFTSize.TabIndex = 9;
            this.cmbFFTSize.TextChanged += new System.EventHandler(this.cmbFFTSize_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(166, 9);
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
            this.cmbAverage.Location = new System.Drawing.Point(660, 4);
            this.cmbAverage.Name = "cmbAverage";
            this.cmbAverage.Size = new System.Drawing.Size(121, 21);
            this.cmbAverage.TabIndex = 7;
            this.cmbAverage.TextChanged += new System.EventHandler(this.cmbAverage_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(570, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Average Factor:";
            // 
            // cmbWindowFunc
            // 
            this.cmbWindowFunc.FormattingEnabled = true;
            this.cmbWindowFunc.Location = new System.Drawing.Point(440, 4);
            this.cmbWindowFunc.Name = "cmbWindowFunc";
            this.cmbWindowFunc.Size = new System.Drawing.Size(121, 21);
            this.cmbWindowFunc.TabIndex = 5;
            this.cmbWindowFunc.TextChanged += new System.EventHandler(this.cmbWindowFunc_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(327, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Windowing Function:";
            // 
            // FFTDisplay
            // 
            this.FFTDisplay.Averaging = 1;
            this.FFTDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FFTDisplay.FFTSize = 256;
            this.FFTDisplay.Location = new System.Drawing.Point(0, 0);
            this.FFTDisplay.Name = "FFTDisplay";
            this.FFTDisplay.Size = new System.Drawing.Size(1002, 544);
            this.FFTDisplay.TabIndex = 0;
            this.FFTDisplay.WindowingFunction = LibRXFFT.Libraries.FFTW.FFTTransformer.eWindowingFunction.BlackmanHarris;
            // 
            // txtUpdatesPerSecond
            // 
            this.txtUpdatesPerSecond.Location = new System.Drawing.Point(243, 26);
            this.txtUpdatesPerSecond.Name = "txtUpdatesPerSecond";
            this.txtUpdatesPerSecond.Size = new System.Drawing.Size(78, 20);
            this.txtUpdatesPerSecond.TabIndex = 12;
            this.txtUpdatesPerSecond.TextChanged += new System.EventHandler(this.txtUpdatesPerSecond_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(166, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Update Rate:";
            // 
            // MainScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1002, 596);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainScreen";
            this.Text = "RX-FFT";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ComboBox cmbWindowFunc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbAverage;
        private System.Windows.Forms.Label label2;
        private LibRXFFT.Components.DirectX.DirectXWaterfallFFTDisplay FFTDisplay;
        private System.Windows.Forms.ComboBox cmbFFTSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSamplingRate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtUpdatesPerSecond;
    }
}

