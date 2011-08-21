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
            this.cmbRate = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioAcqStream = new System.Windows.Forms.RadioButton();
            this.radioAcqOff = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.frequencySelector1 = new LibRXFFT.Components.GDI.FrequencySelector();
            this.label2 = new System.Windows.Forms.Label();
            this.lblRate = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cmbRate);
            this.groupBox1.Controls.Add(this.frequencySelector1);
            this.groupBox1.Location = new System.Drawing.Point(7, 61);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(148, 78);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tuning";
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
            // cmbRate
            // 
            this.cmbRate.BackColor = System.Drawing.Color.Black;
            this.cmbRate.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbRate.ForeColor = System.Drawing.Color.Cyan;
            this.cmbRate.FormattingEnabled = true;
            this.cmbRate.Items.AddRange(new object[] {
            "48000",
            "96000",
            "192000",
            "240000",
            "384000",
            "480000",
            "960000"});
            this.cmbRate.Location = new System.Drawing.Point(47, 43);
            this.cmbRate.Name = "cmbRate";
            this.cmbRate.Size = new System.Drawing.Size(88, 22);
            this.cmbRate.TabIndex = 2;
            this.cmbRate.Text = "960000";
            this.cmbRate.SelectedIndexChanged += new System.EventHandler(this.cmbRate_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioAcqStream);
            this.groupBox3.Controls.Add(this.radioAcqOff);
            this.groupBox3.Location = new System.Drawing.Point(161, 61);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(80, 78);
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
            this.groupBox2.Location = new System.Drawing.Point(7, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(234, 51);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Device";
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
            this.txtHost.Location = new System.Drawing.Point(10, 18);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(125, 20);
            this.txtHost.TabIndex = 0;
            this.txtHost.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtHost_KeyPress);
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(248, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Rate:";
            // 
            // lblRate
            // 
            this.lblRate.AutoSize = true;
            this.lblRate.Location = new System.Drawing.Point(288, 21);
            this.lblRate.Name = "lblRate";
            this.lblRate.Size = new System.Drawing.Size(0, 13);
            this.lblRate.TabIndex = 6;
            // 
            // HiQSDRDeviceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 148);
            this.ControlBox = false;
            this.Controls.Add(this.lblRate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LibRXFFT.Components.GDI.FrequencySelector frequencySelector1;
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
    }
}