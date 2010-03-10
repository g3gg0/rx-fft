namespace RX_Oscilloscope.Components
{
    partial class Oscilloscope
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
            this.waveForm = new LibRXFFT.Components.DirectX.DirectXWaveformDisplay();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtSamplingRate = new LibRXFFT.Components.GDI.FrequencySelector();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.radioPhase = new System.Windows.Forms.RadioButton();
            this.radioPower = new System.Windows.Forms.RadioButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cmbLowPass = new System.Windows.Forms.ComboBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtTriggerLevel = new LibRXFFT.Components.GDI.TextBoxMouseScroll();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.chkTriggerFalling = new System.Windows.Forms.CheckBox();
            this.chkTriggerRising = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblPreTrigTime = new System.Windows.Forms.Label();
            this.txtPreTrigSamples = new LibRXFFT.Components.GDI.TextBoxMouseScroll();
            this.label4 = new System.Windows.Forms.Label();
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
            this.groupBox6.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            this.splitContainer1.SplitterDistance = 430;
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
            this.waveForm.Size = new System.Drawing.Size(997, 430);
            this.waveForm.SpectParts = 1;
            this.waveForm.TabIndex = 0;
            this.waveForm.UpdateRate = 25;
            this.waveForm.XZoomFactor = 1;
            this.waveForm.XZoomFactorMax = 2000;
            this.waveForm.XZoomFactorMin = 1;
            this.waveForm.YZoomFactor = 0.0070000002160668373;
            this.waveForm.YZoomFactorMax = 0.5;
            this.waveForm.YZoomFactorMin = 0.005;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Controls.Add(this.tabPage4);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(997, 80);
            this.tabControl.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox5);
            this.tabPage1.Controls.Add(this.groupBox6);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(989, 54);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Source Signal";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtSamplingRate);
            this.groupBox5.Location = new System.Drawing.Point(6, 6);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(119, 46);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Sampling Rate";
            // 
            // txtSamplingRate
            // 
            this.txtSamplingRate.Frequency = ((long)(0));
            this.txtSamplingRate.Location = new System.Drawing.Point(6, 16);
            this.txtSamplingRate.Name = "txtSamplingRate";
            this.txtSamplingRate.Size = new System.Drawing.Size(100, 20);
            this.txtSamplingRate.TabIndex = 0;
            this.txtSamplingRate.Text = "0.000.000.000 Hz";
            this.txtSamplingRate.FrequencyChanged += new System.EventHandler(this.txtSamplingRate_FrequencyChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.radioPhase);
            this.groupBox6.Controls.Add(this.radioPower);
            this.groupBox6.Location = new System.Drawing.Point(131, 6);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(138, 46);
            this.groupBox6.TabIndex = 9;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Process";
            // 
            // radioPhase
            // 
            this.radioPhase.AutoSize = true;
            this.radioPhase.Location = new System.Drawing.Point(68, 16);
            this.radioPhase.Name = "radioPhase";
            this.radioPhase.Size = new System.Drawing.Size(55, 17);
            this.radioPhase.TabIndex = 0;
            this.radioPhase.Text = "Phase";
            this.radioPhase.UseVisualStyleBackColor = true;
            this.radioPhase.CheckedChanged += new System.EventHandler(this.radioPhase_CheckedChanged);
            // 
            // radioPower
            // 
            this.radioPower.AutoSize = true;
            this.radioPower.Checked = true;
            this.radioPower.Location = new System.Drawing.Point(7, 16);
            this.radioPower.Name = "radioPower";
            this.radioPower.Size = new System.Drawing.Size(55, 17);
            this.radioPower.TabIndex = 0;
            this.radioPower.TabStop = true;
            this.radioPower.Text = "Power";
            this.radioPower.UseVisualStyleBackColor = true;
            this.radioPower.CheckedChanged += new System.EventHandler(this.radioPower_CheckedChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox4);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(989, 58);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Pre-Processing";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cmbLowPass);
            this.groupBox4.Location = new System.Drawing.Point(6, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(95, 46);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Low-Pass";
            // 
            // cmbLowPass
            // 
            this.cmbLowPass.FormattingEnabled = true;
            this.cmbLowPass.Items.AddRange(new object[] {
            "none",
            "/2",
            "/4",
            "/8",
            "/16",
            "/32",
            "/64",
            "/128",
            "/256"});
            this.cmbLowPass.Location = new System.Drawing.Point(6, 16);
            this.cmbLowPass.Name = "cmbLowPass";
            this.cmbLowPass.Size = new System.Drawing.Size(74, 21);
            this.cmbLowPass.TabIndex = 6;
            this.cmbLowPass.SelectedIndexChanged += new System.EventHandler(this.cmbLowPass_SelectedIndexChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Controls.Add(this.groupBox2);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(989, 58);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Triggering";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtTriggerLevel);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.chkTriggerFalling);
            this.groupBox1.Controls.Add(this.chkTriggerRising);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(248, 46);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Trigger";
            // 
            // txtTriggerLevel
            // 
            this.txtTriggerLevel.Location = new System.Drawing.Point(174, 16);
            this.txtTriggerLevel.LowerLimit = ((long)(-160));
            this.txtTriggerLevel.Name = "txtTriggerLevel";
            this.txtTriggerLevel.Size = new System.Drawing.Size(41, 20);
            this.txtTriggerLevel.TabIndex = 2;
            this.txtTriggerLevel.Text = "0";
            this.txtTriggerLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtTriggerLevel.UpperLimit = ((long)(0));
            this.txtTriggerLevel.Value = ((long)(0));
            this.txtTriggerLevel.ValueChanged += new System.EventHandler(this.txtTriggerLevel_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(221, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "dB";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(131, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Level:";
            // 
            // chkTriggerFalling
            // 
            this.chkTriggerFalling.AutoSize = true;
            this.chkTriggerFalling.Location = new System.Drawing.Point(68, 18);
            this.chkTriggerFalling.Name = "chkTriggerFalling";
            this.chkTriggerFalling.Size = new System.Drawing.Size(56, 17);
            this.chkTriggerFalling.TabIndex = 0;
            this.chkTriggerFalling.Text = "Falling";
            this.chkTriggerFalling.UseVisualStyleBackColor = true;
            this.chkTriggerFalling.CheckedChanged += new System.EventHandler(this.chkTriggerFalling_CheckedChanged);
            // 
            // chkTriggerRising
            // 
            this.chkTriggerRising.AutoSize = true;
            this.chkTriggerRising.Location = new System.Drawing.Point(7, 18);
            this.chkTriggerRising.Name = "chkTriggerRising";
            this.chkTriggerRising.Size = new System.Drawing.Size(55, 17);
            this.chkTriggerRising.TabIndex = 0;
            this.chkTriggerRising.Text = "Rising";
            this.chkTriggerRising.UseVisualStyleBackColor = true;
            this.chkTriggerRising.CheckedChanged += new System.EventHandler(this.chkTriggerRising_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblPreTrigTime);
            this.groupBox2.Controls.Add(this.txtPreTrigSamples);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(260, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(172, 46);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Pre-Trigger";
            // 
            // lblPreTrigTime
            // 
            this.lblPreTrigTime.AutoSize = true;
            this.lblPreTrigTime.Location = new System.Drawing.Point(117, 20);
            this.lblPreTrigTime.Name = "lblPreTrigTime";
            this.lblPreTrigTime.Size = new System.Drawing.Size(34, 13);
            this.lblPreTrigTime.TabIndex = 4;
            this.lblPreTrigTime.Text = "(unk.)";
            // 
            // txtPreTrigSamples
            // 
            this.txtPreTrigSamples.Location = new System.Drawing.Point(6, 16);
            this.txtPreTrigSamples.LowerLimit = ((long)(-100000));
            this.txtPreTrigSamples.Name = "txtPreTrigSamples";
            this.txtPreTrigSamples.Size = new System.Drawing.Size(47, 20);
            this.txtPreTrigSamples.TabIndex = 3;
            this.txtPreTrigSamples.Text = "0";
            this.txtPreTrigSamples.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPreTrigSamples.UpperLimit = ((long)(100000));
            this.txtPreTrigSamples.Value = ((long)(0));
            this.txtPreTrigSamples.ValueChanged += new System.EventHandler(this.txtPreTrigSamples_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(59, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Samples, = ";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.groupBox3);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(989, 58);
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
            this.txtBufferTime.LowerLimit = ((long)(512));
            this.txtBufferTime.Name = "txtBufferTime";
            this.txtBufferTime.Size = new System.Drawing.Size(47, 20);
            this.txtBufferTime.TabIndex = 3;
            this.txtBufferTime.Text = "10000";
            this.txtBufferTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtBufferTime.UpperLimit = ((long)(1000000));
            this.txtBufferTime.Value = ((long)(10000));
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
            // Oscilloscope
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "Oscilloscope";
            this.Size = new System.Drawing.Size(997, 511);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private LibRXFFT.Components.DirectX.DirectXWaveformDisplay waveForm;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private LibRXFFT.Components.GDI.TextBoxMouseScroll txtTriggerLevel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkTriggerFalling;
        private System.Windows.Forms.CheckBox chkTriggerRising;
        private LibRXFFT.Components.GDI.TextBoxMouseScroll txtPreTrigSamples;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblPreTrigTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblBufferTime;
        private LibRXFFT.Components.GDI.TextBoxMouseScroll txtBufferTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ComboBox cmbLowPass;
        private System.Windows.Forms.GroupBox groupBox5;
        private LibRXFFT.Components.GDI.FrequencySelector txtSamplingRate;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RadioButton radioPhase;
        private System.Windows.Forms.RadioButton radioPower;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
    }
}
