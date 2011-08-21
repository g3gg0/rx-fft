namespace LibRXFFT.Components.DeviceControls
{
    partial class USRPDeviceControl
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
            if(Connected)
            {
                CloseTuner();
            }

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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtFilterRate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFilterWidth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.frequencySelector1 = new LibRXFFT.Components.GDI.FrequencySelector();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtChannel = new LibRXFFT.Components.GDI.TextBoxMouseScroll();
            this.txtMgcValue = new LibRXFFT.Components.GDI.TextBoxMouseScroll();
            this.radioAgcOff = new System.Windows.Forms.RadioButton();
            this.radioAgcManual = new System.Windows.Forms.RadioButton();
            this.radioAgcFast = new System.Windows.Forms.RadioButton();
            this.radioAgcMedium = new System.Windows.Forms.RadioButton();
            this.radioAgcSlow = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAtt = new LibRXFFT.Components.GDI.TextBoxMouseScroll();
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
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtFilterRate);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtFilterWidth);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.frequencySelector1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(148, 110);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tuning";
            // 
            // txtFilterRate
            // 
            this.txtFilterRate.BackColor = System.Drawing.Color.Black;
            this.txtFilterRate.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFilterRate.ForeColor = System.Drawing.Color.Cyan;
            this.txtFilterRate.Location = new System.Drawing.Point(50, 71);
            this.txtFilterRate.Name = "txtFilterRate";
            this.txtFilterRate.ReadOnly = true;
            this.txtFilterRate.Size = new System.Drawing.Size(85, 20);
            this.txtFilterRate.TabIndex = 5;
            this.txtFilterRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Rate:";
            // 
            // txtFilterWidth
            // 
            this.txtFilterWidth.BackColor = System.Drawing.Color.Black;
            this.txtFilterWidth.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFilterWidth.ForeColor = System.Drawing.Color.Cyan;
            this.txtFilterWidth.Location = new System.Drawing.Point(50, 45);
            this.txtFilterWidth.Name = "txtFilterWidth";
            this.txtFilterWidth.ReadOnly = true;
            this.txtFilterWidth.Size = new System.Drawing.Size(85, 20);
            this.txtFilterWidth.TabIndex = 3;
            this.txtFilterWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Width:";
            // 
            // frequencySelector1
            // 
            this.frequencySelector1.BackColor = System.Drawing.Color.Black;
            this.frequencySelector1.FixedLengthDecades = 10;
            this.frequencySelector1.FixedLengthString = true;
            this.frequencySelector1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.frequencySelector1.ForeColor = System.Drawing.Color.Cyan;
            this.frequencySelector1.Frequency = ((long)(0));
            this.frequencySelector1.Location = new System.Drawing.Point(10, 19);
            this.frequencySelector1.Name = "frequencySelector1";
            this.frequencySelector1.Size = new System.Drawing.Size(125, 20);
            this.frequencySelector1.TabIndex = 1;
            this.frequencySelector1.Text = "0.000.000.000 Hz";
            this.frequencySelector1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.frequencySelector1.FrequencyChanged += new System.EventHandler(this.frequencySelector1_FrequencyChanged);
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
            this.splitContainer2.Panel2.Controls.Add(this.txtInfo);
            this.splitContainer2.Size = new System.Drawing.Size(565, 414);
            this.splitContainer2.SplitterDistance = 114;
            this.splitContainer2.TabIndex = 5;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Controls.Add(this.txtChannel);
            this.groupBox5.Controls.Add(this.txtMgcValue);
            this.groupBox5.Controls.Add(this.radioAgcOff);
            this.groupBox5.Controls.Add(this.radioAgcManual);
            this.groupBox5.Controls.Add(this.radioAgcFast);
            this.groupBox5.Controls.Add(this.radioAgcMedium);
            this.groupBox5.Controls.Add(this.radioAgcSlow);
            this.groupBox5.Location = new System.Drawing.Point(412, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(144, 110);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "AGC";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "AD6636 Channel";
            // 
            // txtChannel
            // 
            this.txtChannel.Location = new System.Drawing.Point(100, 84);
            this.txtChannel.LowerLimit = ((long)(0));
            this.txtChannel.Name = "txtChannel";
            this.txtChannel.Size = new System.Drawing.Size(26, 20);
            this.txtChannel.TabIndex = 17;
            this.txtChannel.Text = "0";
            this.txtChannel.UpperLimit = ((long)(5));
            this.txtChannel.Value = ((long)(0));
            // 
            // txtMgcValue
            // 
            this.txtMgcValue.Location = new System.Drawing.Point(95, 39);
            this.txtMgcValue.LowerLimit = ((long)(0));
            this.txtMgcValue.Name = "txtMgcValue";
            this.txtMgcValue.ReadOnly = true;
            this.txtMgcValue.Size = new System.Drawing.Size(26, 20);
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
            this.radioAgcManual.Size = new System.Drawing.Size(14, 13);
            this.radioAgcManual.TabIndex = 16;
            this.radioAgcManual.TabStop = true;
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
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.txtAtt);
            this.groupBox4.Controls.Add(this.radioTuner);
            this.groupBox4.Controls.Add(this.chkAtt);
            this.groupBox4.Controls.Add(this.radioRf1);
            this.groupBox4.Controls.Add(this.radioRf4);
            this.groupBox4.Controls.Add(this.radioRf2);
            this.groupBox4.Controls.Add(this.chkPreAmp);
            this.groupBox4.Controls.Add(this.radioRf3);
            this.groupBox4.Location = new System.Drawing.Point(243, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(162, 110);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Source";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "ATT";
            // 
            // txtAtt
            // 
            this.txtAtt.Location = new System.Drawing.Point(37, 17);
            this.txtAtt.LowerLimit = ((long)(0));
            this.txtAtt.Name = "txtAtt";
            this.txtAtt.Size = new System.Drawing.Size(27, 20);
            this.txtAtt.TabIndex = 12;
            this.txtAtt.Text = "0";
            this.txtAtt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtAtt.UpperLimit = ((long)(31));
            this.txtAtt.Value = ((long)(0));
            this.txtAtt.ValueChanged += new System.EventHandler(this.txtAtt_ValueChanged);
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
            this.chkAtt.Location = new System.Drawing.Point(80, 89);
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
            this.groupBox3.Location = new System.Drawing.Point(157, 3);
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
            // txtInfo
            // 
            this.txtInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInfo.Location = new System.Drawing.Point(0, 0);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(565, 296);
            this.txtInfo.TabIndex = 0;
            // 
            // USRPDeviceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 414);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximumSize = new System.Drawing.Size(1024, 1024);
            this.MinimumSize = new System.Drawing.Size(440, 227);
            this.Name = "USRPDeviceControl";
            this.Text = "USRP Device Control";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtFilterRate;
        private System.Windows.Forms.TextBox txtFilterWidth;
        private System.Windows.Forms.Label label2;
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
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private LibRXFFT.Components.GDI.TextBoxMouseScroll txtAtt;
        private System.Windows.Forms.Label label4;
        private LibRXFFT.Components.GDI.TextBoxMouseScroll txtChannel;
        private System.Windows.Forms.TextBox txtInfo;
    }
}