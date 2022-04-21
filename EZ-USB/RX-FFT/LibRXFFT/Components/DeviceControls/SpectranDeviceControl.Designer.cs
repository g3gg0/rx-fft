namespace LibRXFFT.Components.DeviceControls
{
    partial class SpectranDeviceControl
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
            this.txtAttenuation = new LibRXFFT.Components.GDI.TextBoxMouseScrollLong();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbRate = new System.Windows.Forms.ComboBox();
            this.txtFreqRx = new LibRXFFT.Components.GDI.FrequencySelector();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioAcqStream = new System.Windows.Forms.RadioButton();
            this.radioAcqOff = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.txtTxLevel = new LibRXFFT.Components.GDI.TextBoxMouseScrollLong();
            this.txtFreqTx = new LibRXFFT.Components.GDI.FrequencySelector();
            this.label6 = new System.Windows.Forms.Label();
            this.lblFirmware = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblSerial = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
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
            "50000000",
            "64000000"});
            this.cmbRate.Location = new System.Drawing.Point(66, 42);
            this.cmbRate.Name = "cmbRate";
            this.cmbRate.Size = new System.Drawing.Size(101, 22);
            this.cmbRate.TabIndex = 2;
            this.cmbRate.Text = "64000000";
            this.cmbRate.SelectedIndexChanged += new System.EventHandler(this.cmbRate_SelectedIndexChanged);
            this.cmbRate.TextChanged += new System.EventHandler(this.cmbRate_TextChanged);
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
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(182, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "TX Level";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.comboBox1);
            this.groupBox4.Controls.Add(this.txtTxLevel);
            this.groupBox4.Controls.Add(this.txtFreqTx);
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
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(254, 47);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Firmware:";
            // 
            // lblFirmware
            // 
            this.lblFirmware.AutoSize = true;
            this.lblFirmware.Location = new System.Drawing.Point(312, 47);
            this.lblFirmware.Name = "lblFirmware";
            this.lblFirmware.Size = new System.Drawing.Size(27, 13);
            this.lblFirmware.TabIndex = 5;
            this.lblFirmware.Text = "N/A";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(254, 34);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(36, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Serial:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(254, 21);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "Name:";
            // 
            // lblSerial
            // 
            this.lblSerial.AutoSize = true;
            this.lblSerial.Location = new System.Drawing.Point(312, 34);
            this.lblSerial.Name = "lblSerial";
            this.lblSerial.Size = new System.Drawing.Size(27, 13);
            this.lblSerial.TabIndex = 11;
            this.lblSerial.Text = "N/A";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(312, 21);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(27, 13);
            this.lblName.TabIndex = 11;
            this.lblName.Text = "N/A";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(25, 24);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 12;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // SpectranDeviceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 251);
            this.ControlBox = false;
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblSerial);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblFirmware);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximumSize = new System.Drawing.Size(1024, 1024);
            this.Name = "SpectranDeviceControl";
            this.Text = "Spectran V6 Device Control";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblRate;
        private System.Windows.Forms.Label label3;
        private GDI.TextBoxMouseScrollLong txtAttenuation;
        private GDI.TextBoxMouseScrollLong txtTxLevel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox1;
        private GDI.FrequencySelector txtFreqTx;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblFirmware;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblSerial;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Button btnConnect;
    }
}