namespace RX_Oscilloscope.Components
{
    partial class IQPlot
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
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.waveForm = new LibRXFFT.Components.DirectX.DirectX2DPlot();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtSamplingRate = new LibRXFFT.Components.GDI.FrequencySelector();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblBufferTime = new System.Windows.Forms.Label();
            this.txtBufferTime = new LibRXFFT.Components.GDI.TextBoxMouseScroll();
            this.label3 = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.waveForm);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl);
            this.splitContainer1.Size = new System.Drawing.Size(997, 511);
            this.splitContainer1.SplitterDistance = 425;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 1;
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
            this.waveForm.MaxSamples = 10000;
            this.waveForm.Name = "waveForm";
            this.waveForm.SamplingRate = 0;
            this.waveForm.Size = new System.Drawing.Size(997, 425);
            this.waveForm.SpectParts = 1;
            this.waveForm.TabIndex = 0;
            this.waveForm.UpdateRate = 25;
            this.waveForm.XZoomFactor = 1;
            this.waveForm.XZoomFactorMax = 2000;
            this.waveForm.XZoomFactorMin = 1;
            this.waveForm.YZoomFactor = 1;
            this.waveForm.YZoomFactorMax = 30;
            this.waveForm.YZoomFactorMin = 0.25;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage4);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(997, 85);
            this.tabControl.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox5);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(989, 59);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Source Signal";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtSamplingRate);
            this.groupBox5.Location = new System.Drawing.Point(6, 6);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(109, 51);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Sampling Rate";
            // 
            // txtSamplingRate
            // 
            this.txtSamplingRate.FixedLengthDecades = 10;
            this.txtSamplingRate.FixedLengthString = false;
            this.txtSamplingRate.Frequency = ((long)(48000));
            this.txtSamplingRate.Location = new System.Drawing.Point(6, 16);
            this.txtSamplingRate.Name = "txtSamplingRate";
            this.txtSamplingRate.Size = new System.Drawing.Size(93, 20);
            this.txtSamplingRate.TabIndex = 0;
            this.txtSamplingRate.Text = "48 kHz";
            this.txtSamplingRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtSamplingRate.FrequencyChanged += new System.EventHandler(this.txtSamplingRate_FrequencyChanged);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.groupBox3);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(989, 54);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Buffering";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblBufferTime);
            this.groupBox3.Controls.Add(this.txtBufferTime);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(6, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(174, 46);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Buffer Size";
            // 
            // lblBufferTime
            // 
            this.lblBufferTime.AutoSize = true;
            this.lblBufferTime.Location = new System.Drawing.Point(117, 20);
            this.lblBufferTime.Name = "lblBufferTime";
            this.lblBufferTime.Size = new System.Drawing.Size(34, 13);
            this.lblBufferTime.TabIndex = 4;
            this.lblBufferTime.Text = "(unk.)";
            // 
            // txtBufferTime
            // 
            this.txtBufferTime.Location = new System.Drawing.Point(6, 16);
            this.txtBufferTime.LowerLimit = ((long)(2));
            this.txtBufferTime.Name = "txtBufferTime";
            this.txtBufferTime.Size = new System.Drawing.Size(47, 20);
            this.txtBufferTime.TabIndex = 3;
            this.txtBufferTime.Text = "1000";
            this.txtBufferTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtBufferTime.UpperLimit = ((long)(1000000));
            this.txtBufferTime.Value = ((long)(1000));
            this.txtBufferTime.ValueChanged += new System.EventHandler(this.txtBufferTime_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(59, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Samples, = ";
            // 
            // IQPlot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "IQPlot";
            this.Size = new System.Drawing.Size(997, 511);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private LibRXFFT.Components.DirectX.DirectX2DPlot waveForm;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox5;
        private LibRXFFT.Components.GDI.FrequencySelector txtSamplingRate;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblBufferTime;
        private LibRXFFT.Components.GDI.TextBoxMouseScroll txtBufferTime;
        private System.Windows.Forms.Label label3;
    }
}
