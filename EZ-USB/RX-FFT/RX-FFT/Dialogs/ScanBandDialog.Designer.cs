namespace RX_FFT.Dialogs
{
    partial class ScanBandDialog
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
            this.txtStartFreq = new LibRXFFT.Components.GDI.FrequencySelector();
            this.label2 = new System.Windows.Forms.Label();
            this.txtEndFreq = new LibRXFFT.Components.GDI.FrequencySelector();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtStartFreq
            // 
            this.txtStartFreq.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStartFreq.BackColor = System.Drawing.Color.Black;
            this.txtStartFreq.FixedLengthDecades = 10;
            this.txtStartFreq.FixedLengthString = false;
            this.txtStartFreq.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStartFreq.ForeColor = System.Drawing.Color.Cyan;
            this.txtStartFreq.Frequency = ((long)(0));
            this.txtStartFreq.Location = new System.Drawing.Point(110, 19);
            this.txtStartFreq.Name = "txtStartFreq";
            this.txtStartFreq.Size = new System.Drawing.Size(141, 20);
            this.txtStartFreq.TabIndex = 5;
            this.txtStartFreq.Text = "0 Hz";
            this.txtStartFreq.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtStartFreq.FrequencyChanged += new System.EventHandler(this.txtStartFreq_FrequencyChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Start frequency:";
            // 
            // txtEndFreq
            // 
            this.txtEndFreq.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEndFreq.BackColor = System.Drawing.Color.Black;
            this.txtEndFreq.FixedLengthDecades = 10;
            this.txtEndFreq.FixedLengthString = false;
            this.txtEndFreq.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEndFreq.ForeColor = System.Drawing.Color.Cyan;
            this.txtEndFreq.Frequency = ((long)(0));
            this.txtEndFreq.Location = new System.Drawing.Point(110, 45);
            this.txtEndFreq.Name = "txtEndFreq";
            this.txtEndFreq.Size = new System.Drawing.Size(141, 20);
            this.txtEndFreq.TabIndex = 7;
            this.txtEndFreq.Text = "0 Hz";
            this.txtEndFreq.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtEndFreq.FrequencyChanged += new System.EventHandler(this.txtEndFreq_FrequencyChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "End frequency:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtStartFreq);
            this.groupBox1.Controls.Add(this.txtEndFreq);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.MinimumSize = new System.Drawing.Size(193, 77);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(266, 77);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Area to scan";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(5, 5);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnCancel);
            this.splitContainer1.Panel2.Controls.Add(this.btnOk);
            this.splitContainer1.Size = new System.Drawing.Size(266, 115);
            this.splitContainer1.SplitterDistance = 77;
            this.splitContainer1.TabIndex = 9;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(185, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(78, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(104, 6);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(78, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Start";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // ScanBandDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(276, 125);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ScanBandDialog";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Text = "Scan frequency band";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
        }


        #endregion

        private LibRXFFT.Components.GDI.FrequencySelector txtStartFreq;
        private System.Windows.Forms.Label label2;
        private LibRXFFT.Components.GDI.FrequencySelector txtEndFreq;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
    }
}