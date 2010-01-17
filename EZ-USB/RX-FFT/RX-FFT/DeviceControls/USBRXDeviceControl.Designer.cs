namespace RX_FFT.DeviceControls
{
    partial class USBRXDeviceControl
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
            this.lblFrequency = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtFilterRate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFilterWidth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.frequencySelector1 = new LibRXFFT.Components.GDI.FrequencySelector();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnAtmel = new System.Windows.Forms.Button();
            this.btnFiles = new System.Windows.Forms.Button();
            this.FilterList = new LibRXFFT.Components.GDI.FilterList();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtMgcValue = new LibRXFFT.Components.GDI.TextBoxMouseScroll();
            this.radioAgcOff = new System.Windows.Forms.RadioButton();
            this.radioAgcManual = new System.Windows.Forms.RadioButton();
            this.radioAgcFast = new System.Windows.Forms.RadioButton();
            this.radioAgcMedium = new System.Windows.Forms.RadioButton();
            this.radioAgcSlow = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.radioTuner = new System.Windows.Forms.RadioButton();
            this.chkAtt = new System.Windows.Forms.CheckBox();
            this.radioRf1 = new System.Windows.Forms.RadioButton();
            this.radioRf4 = new System.Windows.Forms.RadioButton();
            this.radioRf2 = new System.Windows.Forms.RadioButton();
            this.chkPreAmp = new System.Windows.Forms.CheckBox();
            this.radioRf3 = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioAcqStream = new System.Windows.Forms.RadioButton();
            this.radioAcqBlock = new System.Windows.Forms.RadioButton();
            this.radioAcqOff = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblFrequency
            // 
            this.lblFrequency.AutoSize = true;
            this.lblFrequency.Location = new System.Drawing.Point(6, 25);
            this.lblFrequency.Name = "lblFrequency";
            this.lblFrequency.Size = new System.Drawing.Size(60, 13);
            this.lblFrequency.TabIndex = 1;
            this.lblFrequency.Text = "Frequency:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtFilterRate);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtFilterWidth);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblFrequency);
            this.groupBox1.Controls.Add(this.frequencySelector1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(163, 110);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tuning";
            // 
            // txtFilterRate
            // 
            this.txtFilterRate.Location = new System.Drawing.Point(72, 75);
            this.txtFilterRate.Name = "txtFilterRate";
            this.txtFilterRate.ReadOnly = true;
            this.txtFilterRate.Size = new System.Drawing.Size(85, 20);
            this.txtFilterRate.TabIndex = 5;
            this.txtFilterRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Rate:";
            // 
            // txtFilterWidth
            // 
            this.txtFilterWidth.Location = new System.Drawing.Point(72, 49);
            this.txtFilterWidth.Name = "txtFilterWidth";
            this.txtFilterWidth.ReadOnly = true;
            this.txtFilterWidth.Size = new System.Drawing.Size(85, 20);
            this.txtFilterWidth.TabIndex = 3;
            this.txtFilterWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Width:";
            // 
            // frequencySelector1
            // 
            this.frequencySelector1.Frequency = ((long)(0));
            this.frequencySelector1.Location = new System.Drawing.Point(72, 23);
            this.frequencySelector1.Name = "frequencySelector1";
            this.frequencySelector1.Size = new System.Drawing.Size(85, 20);
            this.frequencySelector1.TabIndex = 1;
            this.frequencySelector1.Text = "0 Hz";
            this.frequencySelector1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.frequencySelector1.FrequencyChanged += new System.EventHandler(this.frequencySelector1_FrequencyChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.splitContainer1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox2.Size = new System.Drawing.Size(577, 110);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filters";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(5, 18);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnAtmel);
            this.splitContainer1.Panel1.Controls.Add(this.btnFiles);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.FilterList);
            this.splitContainer1.Size = new System.Drawing.Size(567, 87);
            this.splitContainer1.SplitterDistance = 57;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // btnAtmel
            // 
            this.btnAtmel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAtmel.Location = new System.Drawing.Point(8, 21);
            this.btnAtmel.Name = "btnAtmel";
            this.btnAtmel.Size = new System.Drawing.Size(48, 21);
            this.btnAtmel.TabIndex = 6;
            this.btnAtmel.Text = "Atmel";
            this.btnAtmel.UseVisualStyleBackColor = true;
            this.btnAtmel.Click += new System.EventHandler(this.btnAtmel_Click);
            // 
            // btnFiles
            // 
            this.btnFiles.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnFiles.Location = new System.Drawing.Point(8, 1);
            this.btnFiles.Name = "btnFiles";
            this.btnFiles.Size = new System.Drawing.Size(48, 21);
            this.btnFiles.TabIndex = 5;
            this.btnFiles.Text = "Files";
            this.btnFiles.UseVisualStyleBackColor = true;
            this.btnFiles.Click += new System.EventHandler(this.btnFiles_Click);
            // 
            // FilterList
            // 
            this.FilterList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FilterList.Location = new System.Drawing.Point(0, 0);
            this.FilterList.Margin = new System.Windows.Forms.Padding(0);
            this.FilterList.Name = "FilterList";
            this.FilterList.Size = new System.Drawing.Size(509, 87);
            this.FilterList.TabIndex = 3;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox5);
            this.splitContainer2.Panel1.Controls.Add(this.groupBox4);
            this.splitContainer2.Panel1.Controls.Add(this.groupBox3);
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer2.Size = new System.Drawing.Size(577, 228);
            this.splitContainer2.SplitterDistance = 114;
            this.splitContainer2.TabIndex = 5;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtMgcValue);
            this.groupBox5.Controls.Add(this.radioAgcOff);
            this.groupBox5.Controls.Add(this.radioAgcManual);
            this.groupBox5.Controls.Add(this.radioAgcFast);
            this.groupBox5.Controls.Add(this.radioAgcMedium);
            this.groupBox5.Controls.Add(this.radioAgcSlow);
            this.groupBox5.Location = new System.Drawing.Point(427, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(144, 110);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "AGC";
            // 
            // txtMgcValue
            // 
            this.txtMgcValue.Location = new System.Drawing.Point(85, 65);
            this.txtMgcValue.LowerLimit = ((long)(0));
            this.txtMgcValue.Name = "txtMgcValue";
            this.txtMgcValue.ReadOnly = true;
            this.txtMgcValue.Size = new System.Drawing.Size(50, 20);
            this.txtMgcValue.TabIndex = 1;
            this.txtMgcValue.Text = "0";
            this.txtMgcValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtMgcValue.UpperLimit = ((long)(96));
            this.txtMgcValue.Value = ((long)(0));
            this.txtMgcValue.ValueChanged += new System.EventHandler(this.txtMgcValue_ValueChanged);
            // 
            // radioAgcOff
            // 
            this.radioAgcOff.AutoSize = true;
            this.radioAgcOff.Location = new System.Drawing.Point(75, 19);
            this.radioAgcOff.Name = "radioAgcOff";
            this.radioAgcOff.Size = new System.Drawing.Size(39, 17);
            this.radioAgcOff.TabIndex = 15;
            this.radioAgcOff.TabStop = true;
            this.radioAgcOff.Text = "Off";
            this.radioAgcOff.UseVisualStyleBackColor = true;
            this.radioAgcOff.CheckedChanged += new System.EventHandler(this.radioAgcOff_CheckedChanged);
            // 
            // radioAgcManual
            // 
            this.radioAgcManual.AutoSize = true;
            this.radioAgcManual.Location = new System.Drawing.Point(75, 42);
            this.radioAgcManual.Name = "radioAgcManual";
            this.radioAgcManual.Size = new System.Drawing.Size(60, 17);
            this.radioAgcManual.TabIndex = 16;
            this.radioAgcManual.TabStop = true;
            this.radioAgcManual.Text = "Manual";
            this.radioAgcManual.UseVisualStyleBackColor = true;
            this.radioAgcManual.CheckedChanged += new System.EventHandler(this.radioAgcManual_CheckedChanged);
            // 
            // radioAgcFast
            // 
            this.radioAgcFast.AutoSize = true;
            this.radioAgcFast.Enabled = false;
            this.radioAgcFast.Location = new System.Drawing.Point(6, 65);
            this.radioAgcFast.Name = "radioAgcFast";
            this.radioAgcFast.Size = new System.Drawing.Size(45, 17);
            this.radioAgcFast.TabIndex = 14;
            this.radioAgcFast.TabStop = true;
            this.radioAgcFast.Text = "Fast";
            this.radioAgcFast.UseVisualStyleBackColor = true;
            this.radioAgcFast.CheckedChanged += new System.EventHandler(this.radioAgcFast_CheckedChanged);
            // 
            // radioAgcMedium
            // 
            this.radioAgcMedium.AutoSize = true;
            this.radioAgcMedium.Enabled = false;
            this.radioAgcMedium.Location = new System.Drawing.Point(6, 42);
            this.radioAgcMedium.Name = "radioAgcMedium";
            this.radioAgcMedium.Size = new System.Drawing.Size(62, 17);
            this.radioAgcMedium.TabIndex = 13;
            this.radioAgcMedium.TabStop = true;
            this.radioAgcMedium.Text = "Medium";
            this.radioAgcMedium.UseVisualStyleBackColor = true;
            this.radioAgcMedium.CheckedChanged += new System.EventHandler(this.radioAgcMedium_CheckedChanged);
            // 
            // radioAgcSlow
            // 
            this.radioAgcSlow.AutoSize = true;
            this.radioAgcSlow.Enabled = false;
            this.radioAgcSlow.Location = new System.Drawing.Point(6, 19);
            this.radioAgcSlow.Name = "radioAgcSlow";
            this.radioAgcSlow.Size = new System.Drawing.Size(48, 17);
            this.radioAgcSlow.TabIndex = 12;
            this.radioAgcSlow.TabStop = true;
            this.radioAgcSlow.Text = "Slow";
            this.radioAgcSlow.UseVisualStyleBackColor = true;
            this.radioAgcSlow.CheckedChanged += new System.EventHandler(this.radioAgcSlow_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.radioTuner);
            this.groupBox4.Controls.Add(this.chkAtt);
            this.groupBox4.Controls.Add(this.radioRf1);
            this.groupBox4.Controls.Add(this.radioRf4);
            this.groupBox4.Controls.Add(this.radioRf2);
            this.groupBox4.Controls.Add(this.chkPreAmp);
            this.groupBox4.Controls.Add(this.radioRf3);
            this.groupBox4.Location = new System.Drawing.Point(258, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(162, 110);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Source";
            // 
            // radioTuner
            // 
            this.radioTuner.AutoSize = true;
            this.radioTuner.Location = new System.Drawing.Point(6, 88);
            this.radioTuner.Name = "radioTuner";
            this.radioTuner.Size = new System.Drawing.Size(53, 17);
            this.radioTuner.TabIndex = 11;
            this.radioTuner.TabStop = true;
            this.radioTuner.Text = "Tuner";
            this.radioTuner.UseVisualStyleBackColor = true;
            this.radioTuner.CheckedChanged += new System.EventHandler(this.radioTuner_CheckedChanged);
            // 
            // chkAtt
            // 
            this.chkAtt.AutoSize = true;
            this.chkAtt.Location = new System.Drawing.Point(4, 19);
            this.chkAtt.Name = "chkAtt";
            this.chkAtt.Size = new System.Drawing.Size(47, 17);
            this.chkAtt.TabIndex = 5;
            this.chkAtt.Text = "ATT";
            this.chkAtt.UseVisualStyleBackColor = true;
            this.chkAtt.CheckedChanged += new System.EventHandler(this.chkAtt_CheckedChanged);
            // 
            // radioRf1
            // 
            this.radioRf1.AutoSize = true;
            this.radioRf1.Location = new System.Drawing.Point(6, 42);
            this.radioRf1.Name = "radioRf1";
            this.radioRf1.Size = new System.Drawing.Size(45, 17);
            this.radioRf1.TabIndex = 7;
            this.radioRf1.TabStop = true;
            this.radioRf1.Text = "RF1";
            this.radioRf1.UseVisualStyleBackColor = true;
            this.radioRf1.CheckedChanged += new System.EventHandler(this.radioRf1_CheckedChanged);
            // 
            // radioRf4
            // 
            this.radioRf4.AutoSize = true;
            this.radioRf4.Location = new System.Drawing.Point(80, 65);
            this.radioRf4.Name = "radioRf4";
            this.radioRf4.Size = new System.Drawing.Size(45, 17);
            this.radioRf4.TabIndex = 10;
            this.radioRf4.TabStop = true;
            this.radioRf4.Text = "RF4";
            this.radioRf4.UseVisualStyleBackColor = true;
            this.radioRf4.CheckedChanged += new System.EventHandler(this.radioRf4_CheckedChanged);
            // 
            // radioRf2
            // 
            this.radioRf2.AutoSize = true;
            this.radioRf2.Location = new System.Drawing.Point(80, 42);
            this.radioRf2.Name = "radioRf2";
            this.radioRf2.Size = new System.Drawing.Size(45, 17);
            this.radioRf2.TabIndex = 8;
            this.radioRf2.TabStop = true;
            this.radioRf2.Text = "RF2";
            this.radioRf2.UseVisualStyleBackColor = true;
            this.radioRf2.CheckedChanged += new System.EventHandler(this.radioRf2_CheckedChanged);
            // 
            // chkPreAmp
            // 
            this.chkPreAmp.AutoSize = true;
            this.chkPreAmp.Location = new System.Drawing.Point(80, 19);
            this.chkPreAmp.Name = "chkPreAmp";
            this.chkPreAmp.Size = new System.Drawing.Size(57, 17);
            this.chkPreAmp.TabIndex = 6;
            this.chkPreAmp.Text = "+20dB";
            this.chkPreAmp.UseVisualStyleBackColor = true;
            this.chkPreAmp.CheckedChanged += new System.EventHandler(this.chkPreAmp_CheckedChanged);
            // 
            // radioRf3
            // 
            this.radioRf3.AutoSize = true;
            this.radioRf3.Location = new System.Drawing.Point(6, 65);
            this.radioRf3.Name = "radioRf3";
            this.radioRf3.Size = new System.Drawing.Size(45, 17);
            this.radioRf3.TabIndex = 9;
            this.radioRf3.TabStop = true;
            this.radioRf3.Text = "RF3";
            this.radioRf3.UseVisualStyleBackColor = true;
            this.radioRf3.CheckedChanged += new System.EventHandler(this.radioRf3_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioAcqStream);
            this.groupBox3.Controls.Add(this.radioAcqBlock);
            this.groupBox3.Controls.Add(this.radioAcqOff);
            this.groupBox3.Location = new System.Drawing.Point(172, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(80, 110);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Acquisition";
            // 
            // radioAcqStream
            // 
            this.radioAcqStream.AutoSize = true;
            this.radioAcqStream.Location = new System.Drawing.Point(6, 65);
            this.radioAcqStream.Name = "radioAcqStream";
            this.radioAcqStream.Size = new System.Drawing.Size(58, 17);
            this.radioAcqStream.TabIndex = 4;
            this.radioAcqStream.TabStop = true;
            this.radioAcqStream.Text = "Stream";
            this.radioAcqStream.UseVisualStyleBackColor = true;
            this.radioAcqStream.CheckedChanged += new System.EventHandler(this.radioAcqStream_CheckedChanged);
            // 
            // radioAcqBlock
            // 
            this.radioAcqBlock.AutoSize = true;
            this.radioAcqBlock.Location = new System.Drawing.Point(6, 42);
            this.radioAcqBlock.Name = "radioAcqBlock";
            this.radioAcqBlock.Size = new System.Drawing.Size(52, 17);
            this.radioAcqBlock.TabIndex = 3;
            this.radioAcqBlock.TabStop = true;
            this.radioAcqBlock.Text = "Block";
            this.radioAcqBlock.UseVisualStyleBackColor = true;
            this.radioAcqBlock.CheckedChanged += new System.EventHandler(this.radioAcqBlock_CheckedChanged);
            // 
            // radioAcqOff
            // 
            this.radioAcqOff.AutoSize = true;
            this.radioAcqOff.Location = new System.Drawing.Point(6, 19);
            this.radioAcqOff.Name = "radioAcqOff";
            this.radioAcqOff.Size = new System.Drawing.Size(39, 17);
            this.radioAcqOff.TabIndex = 2;
            this.radioAcqOff.TabStop = true;
            this.radioAcqOff.Text = "Off";
            this.radioAcqOff.UseVisualStyleBackColor = true;
            this.radioAcqOff.CheckedChanged += new System.EventHandler(this.radioAcqOff_CheckedChanged);
            // 
            // USBRXDeviceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 228);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximumSize = new System.Drawing.Size(1024, 1024);
            this.MinimumSize = new System.Drawing.Size(440, 227);
            this.Name = "USBRXDeviceControl";
            this.Text = "USBRXDeviceControl";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private LibRXFFT.Components.GDI.FrequencySelector frequencySelector1;
        private System.Windows.Forms.Label lblFrequency;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtFilterRate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtFilterWidth;
        private System.Windows.Forms.Label label2;
        private LibRXFFT.Components.GDI.FilterList FilterList;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioAcqStream;
        private System.Windows.Forms.RadioButton radioAcqBlock;
        private System.Windows.Forms.RadioButton radioAcqOff;
        private System.Windows.Forms.RadioButton radioRf4;
        private System.Windows.Forms.RadioButton radioRf3;
        private System.Windows.Forms.RadioButton radioRf2;
        private System.Windows.Forms.RadioButton radioRf1;
        private System.Windows.Forms.CheckBox chkPreAmp;
        private System.Windows.Forms.CheckBox chkAtt;
        private System.Windows.Forms.RadioButton radioTuner;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton radioAgcOff;
        private System.Windows.Forms.RadioButton radioAgcManual;
        private System.Windows.Forms.RadioButton radioAgcFast;
        private System.Windows.Forms.RadioButton radioAgcMedium;
        private System.Windows.Forms.RadioButton radioAgcSlow;
        private LibRXFFT.Components.GDI.TextBoxMouseScroll txtMgcValue;
        private System.Windows.Forms.Button btnAtmel;
        private System.Windows.Forms.Button btnFiles;
    }
}