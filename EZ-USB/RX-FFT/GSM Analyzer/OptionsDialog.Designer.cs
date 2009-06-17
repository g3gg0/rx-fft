namespace GSM_Analyzer
{
    partial class OptionsDialog
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
            this.chkFastAtan2 = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblOversampling = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRate = new System.Windows.Forms.TextBox();
            this.chkInvert = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkSniffIMSI = new System.Windows.Forms.CheckBox();
            this.chkShowUnhandled = new System.Windows.Forms.CheckBox();
            this.chkDumpRaw = new System.Windows.Forms.CheckBox();
            this.chkSubSample = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkFastAtan2
            // 
            this.chkFastAtan2.AutoSize = true;
            this.chkFastAtan2.Location = new System.Drawing.Point(10, 76);
            this.chkFastAtan2.Name = "chkFastAtan2";
            this.chkFastAtan2.Size = new System.Drawing.Size(133, 17);
            this.chkFastAtan2.TabIndex = 13;
            this.chkFastAtan2.Text = "Fast/Inaccurate Atan2";
            this.chkFastAtan2.UseVisualStyleBackColor = true;
            this.chkFastAtan2.CheckedChanged += new System.EventHandler(this.chkFastAtan2_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkSubSample);
            this.groupBox1.Controls.Add(this.lblOversampling);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtRate);
            this.groupBox1.Controls.Add(this.chkInvert);
            this.groupBox1.Controls.Add(this.chkFastAtan2);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(206, 152);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "GMSK Processing";
            // 
            // lblOversampling
            // 
            this.lblOversampling.AutoSize = true;
            this.lblOversampling.Location = new System.Drawing.Point(89, 44);
            this.lblOversampling.Name = "lblOversampling";
            this.lblOversampling.Size = new System.Drawing.Size(13, 13);
            this.lblOversampling.TabIndex = 18;
            this.lblOversampling.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(174, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Hz";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Oversampling:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Sampling Rate:";
            // 
            // txtRate
            // 
            this.txtRate.Location = new System.Drawing.Point(92, 19);
            this.txtRate.Name = "txtRate";
            this.txtRate.Size = new System.Drawing.Size(75, 20);
            this.txtRate.TabIndex = 15;
            this.txtRate.TextChanged += new System.EventHandler(this.txtRate_TextChanged);
            // 
            // chkInvert
            // 
            this.chkInvert.AutoSize = true;
            this.chkInvert.Location = new System.Drawing.Point(10, 99);
            this.chkInvert.Name = "chkInvert";
            this.chkInvert.Size = new System.Drawing.Size(113, 17);
            this.chkInvert.TabIndex = 14;
            this.chkInvert.Text = "Inverted Spectrum";
            this.chkInvert.UseVisualStyleBackColor = true;
            this.chkInvert.CheckedChanged += new System.EventHandler(this.chkInvert_CheckedChanged);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(144, 343);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 15;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkSniffIMSI);
            this.groupBox2.Controls.Add(this.chkShowUnhandled);
            this.groupBox2.Controls.Add(this.chkDumpRaw);
            this.groupBox2.Location = new System.Drawing.Point(13, 171);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(206, 95);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "L3 Handler";
            // 
            // chkSniffIMSI
            // 
            this.chkSniffIMSI.AutoSize = true;
            this.chkSniffIMSI.Location = new System.Drawing.Point(10, 66);
            this.chkSniffIMSI.Name = "chkSniffIMSI";
            this.chkSniffIMSI.Size = new System.Drawing.Size(77, 17);
            this.chkSniffIMSI.TabIndex = 1;
            this.chkSniffIMSI.Text = "Sniff IMSIs";
            this.chkSniffIMSI.UseVisualStyleBackColor = true;
            this.chkSniffIMSI.CheckedChanged += new System.EventHandler(this.chkSniffIMSI_CheckedChanged);
            // 
            // chkShowUnhandled
            // 
            this.chkShowUnhandled.AutoSize = true;
            this.chkShowUnhandled.Location = new System.Drawing.Point(10, 43);
            this.chkShowUnhandled.Name = "chkShowUnhandled";
            this.chkShowUnhandled.Size = new System.Drawing.Size(157, 17);
            this.chkShowUnhandled.TabIndex = 0;
            this.chkShowUnhandled.Text = "Show unhandled Messages";
            this.chkShowUnhandled.UseVisualStyleBackColor = true;
            this.chkShowUnhandled.CheckedChanged += new System.EventHandler(this.chkShowUnhandled_CheckedChanged);
            // 
            // chkDumpRaw
            // 
            this.chkDumpRaw.AutoSize = true;
            this.chkDumpRaw.Location = new System.Drawing.Point(10, 20);
            this.chkDumpRaw.Name = "chkDumpRaw";
            this.chkDumpRaw.Size = new System.Drawing.Size(121, 17);
            this.chkDumpRaw.TabIndex = 0;
            this.chkDumpRaw.Text = "Dump Raw Packets";
            this.chkDumpRaw.UseVisualStyleBackColor = true;
            this.chkDumpRaw.CheckedChanged += new System.EventHandler(this.chkDumpRaw_CheckedChanged);
            // 
            // chkSubSample
            // 
            this.chkSubSample.AutoSize = true;
            this.chkSubSample.Location = new System.Drawing.Point(10, 123);
            this.chkSubSample.Name = "chkSubSample";
            this.chkSubSample.Size = new System.Drawing.Size(157, 17);
            this.chkSubSample.TabIndex = 19;
            this.chkSubSample.Text = "Subsample offset correction";
            this.chkSubSample.UseVisualStyleBackColor = true;
            this.chkSubSample.CheckedChanged += new System.EventHandler(this.chkSubSample_CheckedChanged);
            // 
            // OptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(231, 378);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsDialog";
            this.Text = "OptionsDialog";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkFastAtan2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkInvert;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRate;
        private System.Windows.Forms.Label lblOversampling;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkDumpRaw;
        private System.Windows.Forms.CheckBox chkSniffIMSI;
        private System.Windows.Forms.CheckBox chkShowUnhandled;
        private System.Windows.Forms.CheckBox chkSubSample;
    }
}