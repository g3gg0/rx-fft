namespace LibRXFFT.Components.DeviceControls
{
    partial class HiQSDRDeviceControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblRate = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbRate = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioAcqStream = new System.Windows.Forms.RadioButton();
            this.radioAcqOff = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkTxCw = new System.Windows.Forms.CheckBox();
            this.chkTxOther = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.chkTxPTT = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lblFirmware = new System.Windows.Forms.Label();
            this.txtTxLevel = new LibRXFFT.Components.GDI.TextBoxMouseScrollLong();
            this.txtFreqTx = new LibRXFFT.Components.GDI.FrequencySelector();
            this.txtAttenuation = new LibRXFFT.Components.GDI.TextBoxMouseScrollLong();
            this.txtFreqRx = new LibRXFFT.Components.GDI.FrequencySelector();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.lblRate);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtAttenuation);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cmbRate);
            this.groupBox1.Controls.Add(this.txtFreqRx);
            this.groupBox1.Location = new System.Drawing.Point(12, 69);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(317, 78);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Rx Settings";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Rate";
            // 
            // lblRate
            // 
            this.lblRate.AutoSize = true;
            this.lblRate.Location = new System.Drawing.Point(246, 47);
            this.lblRate.Name = "lblRate";
            this.lblRate.Size = new System.Drawing.Size(0, 13);
            this.lblRate.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(182, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Attenuation";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(182, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Measured";
            // 
            // cmbRate
            // 
            this.cmbRate.BackColor = System.Drawing.Color.Black;
            this.cmbRate.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbRate.ForeColor = System.Drawing.Color.Cyan;
            this.cmbRate.FormattingEnabled = true;
            this.cmbRate.Items.AddRange(new object[] {
            "8000",
            "9600",
            "12000",
            "16000",
            "19200",
            "24000",
            "38400",
            "48000",
            "60000",
            "96000",
            "120000",
            "192000",
            "240000",
            "320000",
            "384000",
            "480000",
            "640000",
            "960000",
            "1280000",
            "1536000",
            "1920000",
            "2560000",
            "3840000"});
            this.cmbRate.Location = new System.Drawing.Point(66, 42);
            this.cmbRate.Name = "cmbRate";
            this.cmbRate.Size = new System.Drawing.Size(101, 22);
            this.cmbRate.TabIndex = 2;
            this.cmbRate.Text = "960000";
            this.cmbRate.SelectedIndexChanged += new System.EventHandler(this.cmbRate_SelectedIndexChanged);
            this.cmbRate.TextChanged += new System.EventHandler(this.cmbRate_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioAcqStream);
            this.groupBox3.Controls.Add(this.radioAcqOff);
            this.groupBox3.Location = new System.Drawing.Point(335, 68);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(79, 79);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Acquisition";
            // 
            // radioAcqStream
            // 
            this.radioAcqStream.AutoSize = true;
            this.radioAcqStream.Location = new System.Drawing.Point(6, 42);
            this.radioAcqStream.Name = "radioAcqStream";
            this.radioAcqStream.Size = new System.Drawing.Size(39, 17);
            this.radioAcqStream.TabIndex = 4;
            this.radioAcqStream.TabStop = true;
            this.radioAcqStream.Text = "On";
            this.radioAcqStream.UseVisualStyleBackColor = true;
            this.radioAcqStream.CheckedChanged += new System.EventHandler(this.radioAcqStream_CheckedChanged);
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnConnect);
            this.groupBox2.Controls.Add(this.txtHost);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(234, 51);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "IP Address";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(147, 18);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 20);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtHost
            // 
            this.txtHost.BackColor = System.Drawing.Color.Black;
            this.txtHost.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHost.ForeColor = System.Drawing.Color.Cyan;
            this.txtHost.Location = new System.Drawing.Point(10, 18);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(131, 20);
            this.txtHost.TabIndex = 0;
            this.txtHost.Text = "192.168.0.100";
            this.txtHost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtHost.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtHost_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(182, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "TX Level";
            // 
            // chkTxCw
            // 
            this.chkTxCw.AutoSize = true;
            this.chkTxCw.Location = new System.Drawing.Point(185, 46);
            this.chkTxCw.Name = "chkTxCw";
            this.chkTxCw.Size = new System.Drawing.Size(59, 17);
            this.chkTxCw.TabIndex = 9;
            this.chkTxCw.Text = "Tx CW";
            this.chkTxCw.UseVisualStyleBackColor = true;
            this.chkTxCw.CheckedChanged += new System.EventHandler(this.chkTxCw_CheckedChanged);
            // 
            // chkTxOther
            // 
            this.chkTxOther.AutoSize = true;
            this.chkTxOther.Location = new System.Drawing.Point(250, 46);
            this.chkTxOther.Name = "chkTxOther";
            this.chkTxOther.Size = new System.Drawing.Size(67, 17);
            this.chkTxOther.TabIndex = 9;
            this.chkTxOther.Text = "Tx Other";
            this.chkTxOther.UseVisualStyleBackColor = true;
            this.chkTxOther.CheckedChanged += new System.EventHandler(this.chkTxOther_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.chkTxOther);
            this.groupBox4.Controls.Add(this.comboBox1);
            this.groupBox4.Controls.Add(this.txtTxLevel);
            this.groupBox4.Controls.Add(this.chkTxPTT);
            this.groupBox4.Controls.Add(this.txtFreqTx);
            this.groupBox4.Controls.Add(this.chkTxCw);
            this.groupBox4.Location = new System.Drawing.Point(12, 153);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(419, 92);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Tx Settings";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Rate";
            // 
            // comboBox1
            // 
            this.comboBox1.BackColor = System.Drawing.Color.Black;
            this.comboBox1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.ForeColor = System.Drawing.Color.Cyan;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "8000",
            "192000",
            "480000"});
            this.comboBox1.Location = new System.Drawing.Point(66, 54);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(101, 22);
            this.comboBox1.TabIndex = 2;
            this.comboBox1.Text = "8000";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.cmbRate_SelectedIndexChanged);
            this.comboBox1.TextChanged += new System.EventHandler(this.cmbRate_TextChanged);
            // 
            // chkTxPTT
            // 
            this.chkTxPTT.AutoSize = true;
            this.chkTxPTT.Location = new System.Drawing.Point(185, 69);
            this.chkTxPTT.Name = "chkTxPTT";
            this.chkTxPTT.Size = new System.Drawing.Size(47, 17);
            this.chkTxPTT.TabIndex = 9;
            this.chkTxPTT.Text = "PTT";
            this.chkTxPTT.UseVisualStyleBackColor = true;
            this.chkTxPTT.CheckedChanged += new System.EventHandler(this.chkTxPtt_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(252, 34);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Firmware:";
            // 
            // lblFirmware
            // 
            this.lblFirmware.AutoSize = true;
            this.lblFirmware.Location = new System.Drawing.Point(310, 34);
            this.lblFirmware.Name = "lblFirmware";
            this.lblFirmware.Size = new System.Drawing.Size(27, 13);
            this.lblFirmware.TabIndex = 5;
            this.lblFirmware.Text = "N/A";
            // 
            // txtTxLevel
            // 
            this.txtTxLevel.BackColor = System.Drawing.Color.Black;
            this.txtTxLevel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTxLevel.ForeColor = System.Drawing.Color.Cyan;
            this.txtTxLevel.Location = new System.Drawing.Point(249, 20);
            this.txtTxLevel.LowerLimit = ((long)(0));
            this.txtTxLevel.Name = "txtTxLevel";
            this.txtTxLevel.Size = new System.Drawing.Size(48, 20);
            this.txtTxLevel.TabIndex = 7;
            this.txtTxLevel.Text = "0";
            this.txtTxLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtTxLevel.UpperLimit = ((long)(255));
            this.txtTxLevel.Value = ((long)(0));
            this.txtTxLevel.ValueChanged += new System.EventHandler(this.txtTxLevel_ValueChanged);
            // 
            // txtFreqTx
            // 
            this.txtFreqTx.BackColor = System.Drawing.Color.Black;
            this.txtFreqTx.FixedLengthDecades = 10;
            this.txtFreqTx.FixedLengthString = true;
            this.txtFreqTx.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFreqTx.ForeColor = System.Drawing.Color.Cyan;
            this.txtFreqTx.Frequency = ((long)(0));
            this.txtFreqTx.Location = new System.Drawing.Point(13, 23);
            this.txtFreqTx.Name = "txtFreqTx";
            this.txtFreqTx.Size = new System.Drawing.Size(154, 20);
            this.txtFreqTx.TabIndex = 1;
            this.txtFreqTx.Text = "0.000.000.000 Hz";
            this.txtFreqTx.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtFreqTx.FrequencyChanged += new System.EventHandler(this.txtFreqTx_FrequencyChanged);
            // 
            // txtAttenuation
            // 
            this.txtAttenuation.BackColor = System.Drawing.Color.Black;
            this.txtAttenuation.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.txtAttenuation.ForeColor = System.Drawing.Color.Cyan;
            this.txtAttenuation.Location = new System.Drawing.Point(249, 19);
            this.txtAttenuation.LowerLimit = ((long)(-1));
            this.txtAttenuation.Name = "txtAttenuation";
            this.txtAttenuation.Size = new System.Drawing.Size(48, 20);
            this.txtAttenuation.TabIndex = 7;
            this.txtAttenuation.Text = "0";
            this.txtAttenuation.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtAttenuation.UpperLimit = ((long)(44));
            this.txtAttenuation.Value = ((long)(0));
            this.txtAttenuation.ValueChanged += new System.EventHandler(this.txtAttenuation_ValueChanged);
            // 
            // txtFreqRx
            // 
            this.txtFreqRx.BackColor = System.Drawing.Color.Black;
            this.txtFreqRx.FixedLengthDecades = 10;
            this.txtFreqRx.FixedLengthString = true;
            this.txtFreqRx.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFreqRx.ForeColor = System.Drawing.Color.Cyan;
            this.txtFreqRx.Frequency = ((long)(0));
            this.txtFreqRx.Location = new System.Drawing.Point(13, 19);
            this.txtFreqRx.Name = "txtFreqRx";
            this.txtFreqRx.Size = new System.Drawing.Size(154, 20);
            this.txtFreqRx.TabIndex = 1;
            this.txtFreqRx.Text = "0.000.000.000 Hz";
            this.txtFreqRx.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtFreqRx.FrequencyChanged += new System.EventHandler(this.txtFreqRx_FrequencyChanged);
            // 
            // HiQSDRDeviceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 251);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblFirmware);
            this.Controls.Add(this.label6);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximumSize = new System.Drawing.Size(1024, 1024);
            this.Name = "HiQSDRDeviceControl";
            this.Text = "HiQ-SDR Device Control";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LibRXFFT.Components.GDI.FrequencySelector txtFreqRx;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioAcqStream;
        private System.Windows.Forms.RadioButton radioAcqOff;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbRate;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblRate;
        private System.Windows.Forms.Label label3;
        private GDI.TextBoxMouseScrollLong txtAttenuation;
        private GDI.TextBoxMouseScrollLong txtTxLevel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkTxCw;
        private System.Windows.Forms.CheckBox chkTxOther;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox chkTxPTT;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox1;
        private GDI.FrequencySelector txtFreqTx;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblFirmware;
    }
}