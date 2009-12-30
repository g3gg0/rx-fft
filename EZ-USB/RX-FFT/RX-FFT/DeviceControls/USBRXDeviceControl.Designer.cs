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
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtFilterRate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFilterWidth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.frequencySelector1 = new LibRXFFT.Components.GDI.FrequencySelector();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnFilterSelectDir = new System.Windows.Forms.Button();
            this.aD6636FilterList1 = new LibRXFFT.Components.GDI.AD6636FilterList();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
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
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Frequency:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtFilterRate);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtFilterWidth);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.frequencySelector1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(229, 106);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tuning";
            // 
            // txtFilterRate
            // 
            this.txtFilterRate.Location = new System.Drawing.Point(110, 71);
            this.txtFilterRate.Name = "txtFilterRate";
            this.txtFilterRate.ReadOnly = true;
            this.txtFilterRate.Size = new System.Drawing.Size(101, 20);
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
            this.txtFilterWidth.Location = new System.Drawing.Point(110, 49);
            this.txtFilterWidth.Name = "txtFilterWidth";
            this.txtFilterWidth.ReadOnly = true;
            this.txtFilterWidth.Size = new System.Drawing.Size(101, 20);
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
            this.frequencySelector1.Location = new System.Drawing.Point(110, 22);
            this.frequencySelector1.Name = "frequencySelector1";
            this.frequencySelector1.Size = new System.Drawing.Size(101, 20);
            this.frequencySelector1.TabIndex = 0;
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
            this.groupBox2.Size = new System.Drawing.Size(206, 171);
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
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnFilterSelectDir);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.aD6636FilterList1);
            this.splitContainer1.Size = new System.Drawing.Size(196, 148);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // btnFilterSelectDir
            // 
            this.btnFilterSelectDir.Location = new System.Drawing.Point(0, 0);
            this.btnFilterSelectDir.Name = "btnFilterSelectDir";
            this.btnFilterSelectDir.Size = new System.Drawing.Size(85, 23);
            this.btnFilterSelectDir.TabIndex = 4;
            this.btnFilterSelectDir.Text = "Select Filter...";
            this.btnFilterSelectDir.UseVisualStyleBackColor = true;
            this.btnFilterSelectDir.Click += new System.EventHandler(this.btnFilterSelectDir_Click);
            // 
            // aD6636FilterList1
            // 
            this.aD6636FilterList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aD6636FilterList1.Location = new System.Drawing.Point(0, 0);
            this.aD6636FilterList1.Name = "aD6636FilterList1";
            this.aD6636FilterList1.Padding = new System.Windows.Forms.Padding(5);
            this.aD6636FilterList1.Size = new System.Drawing.Size(196, 122);
            this.aD6636FilterList1.TabIndex = 3;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox3);
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer2.Size = new System.Drawing.Size(447, 171);
            this.splitContainer2.SplitterDistance = 237;
            this.splitContainer2.TabIndex = 5;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioAcqStream);
            this.groupBox3.Controls.Add(this.radioAcqBlock);
            this.groupBox3.Controls.Add(this.radioAcqOff);
            this.groupBox3.Location = new System.Drawing.Point(3, 116);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(229, 51);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Acquisition";
            // 
            // radioAcqStream
            // 
            this.radioAcqStream.AutoSize = true;
            this.radioAcqStream.Location = new System.Drawing.Point(110, 20);
            this.radioAcqStream.Name = "radioAcqStream";
            this.radioAcqStream.Size = new System.Drawing.Size(58, 17);
            this.radioAcqStream.TabIndex = 0;
            this.radioAcqStream.TabStop = true;
            this.radioAcqStream.Text = "Stream";
            this.radioAcqStream.UseVisualStyleBackColor = true;
            this.radioAcqStream.CheckedChanged += new System.EventHandler(this.radioAcqStream_CheckedChanged);
            // 
            // radioAcqBlock
            // 
            this.radioAcqBlock.AutoSize = true;
            this.radioAcqBlock.Location = new System.Drawing.Point(52, 20);
            this.radioAcqBlock.Name = "radioAcqBlock";
            this.radioAcqBlock.Size = new System.Drawing.Size(52, 17);
            this.radioAcqBlock.TabIndex = 0;
            this.radioAcqBlock.TabStop = true;
            this.radioAcqBlock.Text = "Block";
            this.radioAcqBlock.UseVisualStyleBackColor = true;
            this.radioAcqBlock.CheckedChanged += new System.EventHandler(this.radioAcqBlock_CheckedChanged);
            // 
            // radioAcqOff
            // 
            this.radioAcqOff.AutoSize = true;
            this.radioAcqOff.Location = new System.Drawing.Point(7, 20);
            this.radioAcqOff.Name = "radioAcqOff";
            this.radioAcqOff.Size = new System.Drawing.Size(39, 17);
            this.radioAcqOff.TabIndex = 0;
            this.radioAcqOff.TabStop = true;
            this.radioAcqOff.Text = "Off";
            this.radioAcqOff.UseVisualStyleBackColor = true;
            this.radioAcqOff.CheckedChanged += new System.EventHandler(this.radioAcqOff_CheckedChanged);
            // 
            // USBRXDeviceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 171);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(400, 150);
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
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private LibRXFFT.Components.GDI.FrequencySelector frequencySelector1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtFilterRate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtFilterWidth;
        private System.Windows.Forms.Label label2;
        private LibRXFFT.Components.GDI.AD6636FilterList aD6636FilterList1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnFilterSelectDir;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioAcqStream;
        private System.Windows.Forms.RadioButton radioAcqBlock;
        private System.Windows.Forms.RadioButton radioAcqOff;
    }
}