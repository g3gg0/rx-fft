namespace RX_FFT
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.aD6636FilterList1 = new LibRXFFT.Components.GDI.AD6636FilterList();
            this.frequencySelector1 = new LibRXFFT.Components.GDI.FrequencySelector();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(229, 107);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tuning";
            // 
            // txtFilterRate
            // 
            this.txtFilterRate.Location = new System.Drawing.Point(72, 75);
            this.txtFilterRate.Name = "txtFilterRate";
            this.txtFilterRate.Size = new System.Drawing.Size(76, 20);
            this.txtFilterRate.TabIndex = 5;
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
            this.txtFilterWidth.Size = new System.Drawing.Size(76, 20);
            this.txtFilterWidth.TabIndex = 3;
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.aD6636FilterList1);
            this.groupBox2.Location = new System.Drawing.Point(256, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(459, 107);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filters";
            // 
            // aD6636FilterList1
            // 
            this.aD6636FilterList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aD6636FilterList1.Location = new System.Drawing.Point(3, 16);
            this.aD6636FilterList1.Name = "aD6636FilterList1";
            this.aD6636FilterList1.Size = new System.Drawing.Size(453, 88);
            this.aD6636FilterList1.TabIndex = 3;
            // 
            // frequencySelector1
            // 
            this.frequencySelector1.Frequency = ((long)(0));
            this.frequencySelector1.Location = new System.Drawing.Point(72, 22);
            this.frequencySelector1.Name = "frequencySelector1";
            this.frequencySelector1.Size = new System.Drawing.Size(139, 20);
            this.frequencySelector1.TabIndex = 0;
            this.frequencySelector1.Text = "0 Hz";
            this.frequencySelector1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.frequencySelector1.TextChanged += new System.EventHandler(this.frequencySelector1_TextChanged);
            // 
            // USBRXDeviceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(875, 207);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "USBRXDeviceControl";
            this.Text = "USBRXDeviceControl";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
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
    }
}