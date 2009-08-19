namespace RX_FFT
{
    partial class DemodulationDialog
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
            this.chkEnableDemod = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtDecim = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.radioFMAccurate = new System.Windows.Forms.RadioButton();
            this.radioFMFast = new System.Windows.Forms.RadioButton();
            this.radioAM = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioFilter256 = new System.Windows.Forms.RadioButton();
            this.radioFilter128 = new System.Windows.Forms.RadioButton();
            this.radioFilter64 = new System.Windows.Forms.RadioButton();
            this.radioFilter16 = new System.Windows.Forms.RadioButton();
            this.radioFilter32 = new System.Windows.Forms.RadioButton();
            this.radioFilter2 = new System.Windows.Forms.RadioButton();
            this.radioFilter4 = new System.Windows.Forms.RadioButton();
            this.radioFilter8 = new System.Windows.Forms.RadioButton();
            this.chkEnableCursorWin = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.txtSamplingRate = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.radioLowPass256 = new System.Windows.Forms.RadioButton();
            this.radioLowPass128 = new System.Windows.Forms.RadioButton();
            this.radioLowPass64 = new System.Windows.Forms.RadioButton();
            this.radioLowPass16 = new System.Windows.Forms.RadioButton();
            this.radioLowPass4 = new System.Windows.Forms.RadioButton();
            this.radioLowPass32 = new System.Windows.Forms.RadioButton();
            this.radioLowPass8 = new System.Windows.Forms.RadioButton();
            this.radioLowPass2 = new System.Windows.Forms.RadioButton();
            this.chkEnableLowpass = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.chkAmplify = new System.Windows.Forms.CheckBox();
            this.txtAmplify = new System.Windows.Forms.TextBox();
            this.chkShowDemod = new System.Windows.Forms.CheckBox();
            this.chkNative = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkEnableDemod
            // 
            this.chkEnableDemod.AutoSize = true;
            this.chkEnableDemod.Location = new System.Drawing.Point(6, 19);
            this.chkEnableDemod.Name = "chkEnableDemod";
            this.chkEnableDemod.Size = new System.Drawing.Size(59, 17);
            this.chkEnableDemod.TabIndex = 0;
            this.chkEnableDemod.Text = "Enable";
            this.chkEnableDemod.UseVisualStyleBackColor = true;
            this.chkEnableDemod.CheckedChanged += new System.EventHandler(this.chkEnableDemod_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtDecim);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.radioFMAccurate);
            this.groupBox1.Controls.Add(this.radioFMFast);
            this.groupBox1.Controls.Add(this.radioAM);
            this.groupBox1.Controls.Add(this.chkEnableDemod);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(104, 140);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Demodulation";
            // 
            // txtDecim
            // 
            this.txtDecim.Location = new System.Drawing.Point(74, 109);
            this.txtDecim.Name = "txtDecim";
            this.txtDecim.Size = new System.Drawing.Size(23, 20);
            this.txtDecim.TabIndex = 5;
            this.txtDecim.TextChanged += new System.EventHandler(this.txtDecim_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Decimation:";
            // 
            // radioFMAccurate
            // 
            this.radioFMAccurate.AutoSize = true;
            this.radioFMAccurate.Location = new System.Drawing.Point(6, 88);
            this.radioFMAccurate.Name = "radioFMAccurate";
            this.radioFMAccurate.Size = new System.Drawing.Size(91, 17);
            this.radioFMAccurate.TabIndex = 3;
            this.radioFMAccurate.TabStop = true;
            this.radioFMAccurate.Text = "FM (accurate)";
            this.radioFMAccurate.UseVisualStyleBackColor = true;
            this.radioFMAccurate.CheckedChanged += new System.EventHandler(this.radioFMAccurate_CheckedChanged);
            // 
            // radioFMFast
            // 
            this.radioFMFast.AutoSize = true;
            this.radioFMFast.Location = new System.Drawing.Point(6, 65);
            this.radioFMFast.Name = "radioFMFast";
            this.radioFMFast.Size = new System.Drawing.Size(66, 17);
            this.radioFMFast.TabIndex = 2;
            this.radioFMFast.TabStop = true;
            this.radioFMFast.Text = "FM (fast)";
            this.radioFMFast.UseVisualStyleBackColor = true;
            this.radioFMFast.CheckedChanged += new System.EventHandler(this.radioFMFast_CheckedChanged);
            // 
            // radioAM
            // 
            this.radioAM.AutoSize = true;
            this.radioAM.Location = new System.Drawing.Point(6, 42);
            this.radioAM.Name = "radioAM";
            this.radioAM.Size = new System.Drawing.Size(41, 17);
            this.radioAM.TabIndex = 1;
            this.radioAM.TabStop = true;
            this.radioAM.Text = "AM";
            this.radioAM.UseVisualStyleBackColor = true;
            this.radioAM.CheckedChanged += new System.EventHandler(this.radioAM_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioFilter256);
            this.groupBox2.Controls.Add(this.radioFilter128);
            this.groupBox2.Controls.Add(this.radioFilter64);
            this.groupBox2.Controls.Add(this.radioFilter16);
            this.groupBox2.Controls.Add(this.radioFilter32);
            this.groupBox2.Controls.Add(this.radioFilter2);
            this.groupBox2.Controls.Add(this.radioFilter4);
            this.groupBox2.Controls.Add(this.radioFilter8);
            this.groupBox2.Controls.Add(this.chkEnableCursorWin);
            this.groupBox2.Location = new System.Drawing.Point(123, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(161, 140);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Cursor Position Window";
            // 
            // radioFilter256
            // 
            this.radioFilter256.AutoSize = true;
            this.radioFilter256.Location = new System.Drawing.Point(82, 111);
            this.radioFilter256.Name = "radioFilter256";
            this.radioFilter256.Size = new System.Drawing.Size(82, 17);
            this.radioFilter256.TabIndex = 4;
            this.radioFilter256.TabStop = true;
            this.radioFilter256.Text = "Width / 256";
            this.radioFilter256.UseVisualStyleBackColor = true;
            this.radioFilter256.CheckedChanged += new System.EventHandler(this.radioFilter256_CheckedChanged);
            // 
            // radioFilter128
            // 
            this.radioFilter128.AutoSize = true;
            this.radioFilter128.Location = new System.Drawing.Point(6, 111);
            this.radioFilter128.Name = "radioFilter128";
            this.radioFilter128.Size = new System.Drawing.Size(82, 17);
            this.radioFilter128.TabIndex = 3;
            this.radioFilter128.TabStop = true;
            this.radioFilter128.Text = "Width / 128";
            this.radioFilter128.UseVisualStyleBackColor = true;
            this.radioFilter128.CheckedChanged += new System.EventHandler(this.radioFilter128_CheckedChanged);
            // 
            // radioFilter64
            // 
            this.radioFilter64.AutoSize = true;
            this.radioFilter64.Location = new System.Drawing.Point(82, 88);
            this.radioFilter64.Name = "radioFilter64";
            this.radioFilter64.Size = new System.Drawing.Size(76, 17);
            this.radioFilter64.TabIndex = 2;
            this.radioFilter64.TabStop = true;
            this.radioFilter64.Text = "Width / 64";
            this.radioFilter64.UseVisualStyleBackColor = true;
            this.radioFilter64.CheckedChanged += new System.EventHandler(this.radioFilter64_CheckedChanged);
            // 
            // radioFilter16
            // 
            this.radioFilter16.AutoSize = true;
            this.radioFilter16.Location = new System.Drawing.Point(82, 65);
            this.radioFilter16.Name = "radioFilter16";
            this.radioFilter16.Size = new System.Drawing.Size(76, 17);
            this.radioFilter16.TabIndex = 1;
            this.radioFilter16.TabStop = true;
            this.radioFilter16.Text = "Width / 16";
            this.radioFilter16.UseVisualStyleBackColor = true;
            this.radioFilter16.CheckedChanged += new System.EventHandler(this.radioFilter16_CheckedChanged);
            // 
            // radioFilter32
            // 
            this.radioFilter32.AutoSize = true;
            this.radioFilter32.Location = new System.Drawing.Point(6, 88);
            this.radioFilter32.Name = "radioFilter32";
            this.radioFilter32.Size = new System.Drawing.Size(76, 17);
            this.radioFilter32.TabIndex = 1;
            this.radioFilter32.TabStop = true;
            this.radioFilter32.Text = "Width / 32";
            this.radioFilter32.UseVisualStyleBackColor = true;
            this.radioFilter32.CheckedChanged += new System.EventHandler(this.radioFilter32_CheckedChanged);
            // 
            // radioFilter2
            // 
            this.radioFilter2.AutoSize = true;
            this.radioFilter2.Location = new System.Drawing.Point(6, 42);
            this.radioFilter2.Name = "radioFilter2";
            this.radioFilter2.Size = new System.Drawing.Size(70, 17);
            this.radioFilter2.TabIndex = 0;
            this.radioFilter2.Text = "Width / 2";
            this.radioFilter2.UseVisualStyleBackColor = true;
            this.radioFilter2.CheckedChanged += new System.EventHandler(this.radioFilter2_CheckedChanged);
            // 
            // radioFilter4
            // 
            this.radioFilter4.AutoSize = true;
            this.radioFilter4.Location = new System.Drawing.Point(82, 42);
            this.radioFilter4.Name = "radioFilter4";
            this.radioFilter4.Size = new System.Drawing.Size(70, 17);
            this.radioFilter4.TabIndex = 0;
            this.radioFilter4.TabStop = true;
            this.radioFilter4.Text = "Width / 4";
            this.radioFilter4.UseVisualStyleBackColor = true;
            this.radioFilter4.CheckedChanged += new System.EventHandler(this.radioFilter4_CheckedChanged);
            // 
            // radioFilter8
            // 
            this.radioFilter8.AutoSize = true;
            this.radioFilter8.Location = new System.Drawing.Point(6, 65);
            this.radioFilter8.Name = "radioFilter8";
            this.radioFilter8.Size = new System.Drawing.Size(70, 17);
            this.radioFilter8.TabIndex = 0;
            this.radioFilter8.TabStop = true;
            this.radioFilter8.Text = "Width / 8";
            this.radioFilter8.UseVisualStyleBackColor = true;
            this.radioFilter8.CheckedChanged += new System.EventHandler(this.radioFilter8_CheckedChanged);
            // 
            // chkEnableCursorWin
            // 
            this.chkEnableCursorWin.AutoSize = true;
            this.chkEnableCursorWin.Location = new System.Drawing.Point(6, 19);
            this.chkEnableCursorWin.Name = "chkEnableCursorWin";
            this.chkEnableCursorWin.Size = new System.Drawing.Size(59, 17);
            this.chkEnableCursorWin.TabIndex = 0;
            this.chkEnableCursorWin.Text = "Enable";
            this.chkEnableCursorWin.UseVisualStyleBackColor = true;
            this.chkEnableCursorWin.CheckedChanged += new System.EventHandler(this.chkEnableCursorWin_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.txtStatus);
            this.groupBox3.Controls.Add(this.txtSamplingRate);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(12, 158);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(272, 102);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Information";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Status:";
            // 
            // txtStatus
            // 
            this.txtStatus.Location = new System.Drawing.Point(145, 20);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(100, 20);
            this.txtStatus.TabIndex = 1;
            // 
            // txtSamplingRate
            // 
            this.txtSamplingRate.Location = new System.Drawing.Point(145, 46);
            this.txtSamplingRate.Name = "txtSamplingRate";
            this.txtSamplingRate.ReadOnly = true;
            this.txtSamplingRate.Size = new System.Drawing.Size(100, 20);
            this.txtSamplingRate.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Output Sampling Rate:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.radioLowPass256);
            this.groupBox4.Controls.Add(this.radioLowPass128);
            this.groupBox4.Controls.Add(this.radioLowPass64);
            this.groupBox4.Controls.Add(this.radioLowPass16);
            this.groupBox4.Controls.Add(this.radioLowPass4);
            this.groupBox4.Controls.Add(this.radioLowPass32);
            this.groupBox4.Controls.Add(this.radioLowPass8);
            this.groupBox4.Controls.Add(this.radioLowPass2);
            this.groupBox4.Controls.Add(this.chkEnableLowpass);
            this.groupBox4.Location = new System.Drawing.Point(290, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(162, 140);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Audio LowPass";
            // 
            // radioLowPass256
            // 
            this.radioLowPass256.AutoSize = true;
            this.radioLowPass256.Location = new System.Drawing.Point(83, 112);
            this.radioLowPass256.Name = "radioLowPass256";
            this.radioLowPass256.Size = new System.Drawing.Size(82, 17);
            this.radioLowPass256.TabIndex = 8;
            this.radioLowPass256.TabStop = true;
            this.radioLowPass256.Text = "Width / 256";
            this.radioLowPass256.UseVisualStyleBackColor = true;
            this.radioLowPass256.CheckedChanged += new System.EventHandler(this.radioLowPass256_CheckedChanged);
            // 
            // radioLowPass128
            // 
            this.radioLowPass128.AutoSize = true;
            this.radioLowPass128.Location = new System.Drawing.Point(7, 112);
            this.radioLowPass128.Name = "radioLowPass128";
            this.radioLowPass128.Size = new System.Drawing.Size(82, 17);
            this.radioLowPass128.TabIndex = 7;
            this.radioLowPass128.TabStop = true;
            this.radioLowPass128.Text = "Width / 128";
            this.radioLowPass128.UseVisualStyleBackColor = true;
            this.radioLowPass128.CheckedChanged += new System.EventHandler(this.radioLowPass128_CheckedChanged);
            // 
            // radioLowPass64
            // 
            this.radioLowPass64.AutoSize = true;
            this.radioLowPass64.Location = new System.Drawing.Point(83, 89);
            this.radioLowPass64.Name = "radioLowPass64";
            this.radioLowPass64.Size = new System.Drawing.Size(76, 17);
            this.radioLowPass64.TabIndex = 6;
            this.radioLowPass64.TabStop = true;
            this.radioLowPass64.Text = "Width / 64";
            this.radioLowPass64.UseVisualStyleBackColor = true;
            this.radioLowPass64.CheckedChanged += new System.EventHandler(this.radioLowPass64_CheckedChanged);
            // 
            // radioLowPass16
            // 
            this.radioLowPass16.AutoSize = true;
            this.radioLowPass16.Location = new System.Drawing.Point(83, 66);
            this.radioLowPass16.Name = "radioLowPass16";
            this.radioLowPass16.Size = new System.Drawing.Size(76, 17);
            this.radioLowPass16.TabIndex = 5;
            this.radioLowPass16.TabStop = true;
            this.radioLowPass16.Text = "Width / 16";
            this.radioLowPass16.UseVisualStyleBackColor = true;
            this.radioLowPass16.CheckedChanged += new System.EventHandler(this.radioLowPass16_CheckedChanged);
            // 
            // radioLowPass4
            // 
            this.radioLowPass4.AutoSize = true;
            this.radioLowPass4.Location = new System.Drawing.Point(83, 42);
            this.radioLowPass4.Name = "radioLowPass4";
            this.radioLowPass4.Size = new System.Drawing.Size(70, 17);
            this.radioLowPass4.TabIndex = 4;
            this.radioLowPass4.TabStop = true;
            this.radioLowPass4.Text = "Width / 4";
            this.radioLowPass4.UseVisualStyleBackColor = true;
            this.radioLowPass4.CheckedChanged += new System.EventHandler(this.radioLowPass4_CheckedChanged);
            // 
            // radioLowPass32
            // 
            this.radioLowPass32.AutoSize = true;
            this.radioLowPass32.Location = new System.Drawing.Point(7, 89);
            this.radioLowPass32.Name = "radioLowPass32";
            this.radioLowPass32.Size = new System.Drawing.Size(76, 17);
            this.radioLowPass32.TabIndex = 3;
            this.radioLowPass32.TabStop = true;
            this.radioLowPass32.Text = "Width / 32";
            this.radioLowPass32.UseVisualStyleBackColor = true;
            this.radioLowPass32.CheckedChanged += new System.EventHandler(this.radioLowPass32_CheckedChanged);
            // 
            // radioLowPass8
            // 
            this.radioLowPass8.AutoSize = true;
            this.radioLowPass8.Location = new System.Drawing.Point(7, 66);
            this.radioLowPass8.Name = "radioLowPass8";
            this.radioLowPass8.Size = new System.Drawing.Size(70, 17);
            this.radioLowPass8.TabIndex = 2;
            this.radioLowPass8.TabStop = true;
            this.radioLowPass8.Text = "Width / 8";
            this.radioLowPass8.UseVisualStyleBackColor = true;
            this.radioLowPass8.CheckedChanged += new System.EventHandler(this.radioLowPass8_CheckedChanged);
            // 
            // radioLowPass2
            // 
            this.radioLowPass2.AutoSize = true;
            this.radioLowPass2.Location = new System.Drawing.Point(7, 42);
            this.radioLowPass2.Name = "radioLowPass2";
            this.radioLowPass2.Size = new System.Drawing.Size(70, 17);
            this.radioLowPass2.TabIndex = 1;
            this.radioLowPass2.TabStop = true;
            this.radioLowPass2.Text = "Width / 2";
            this.radioLowPass2.UseVisualStyleBackColor = true;
            this.radioLowPass2.CheckedChanged += new System.EventHandler(this.radioLowPass2_CheckedChanged);
            // 
            // chkEnableLowpass
            // 
            this.chkEnableLowpass.AutoSize = true;
            this.chkEnableLowpass.Location = new System.Drawing.Point(7, 20);
            this.chkEnableLowpass.Name = "chkEnableLowpass";
            this.chkEnableLowpass.Size = new System.Drawing.Size(59, 17);
            this.chkEnableLowpass.TabIndex = 0;
            this.chkEnableLowpass.Text = "Enable";
            this.chkEnableLowpass.UseVisualStyleBackColor = true;
            this.chkEnableLowpass.CheckedChanged += new System.EventHandler(this.chkEnableLowpass_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.chkNative);
            this.groupBox5.Controls.Add(this.chkAmplify);
            this.groupBox5.Controls.Add(this.txtAmplify);
            this.groupBox5.Controls.Add(this.chkShowDemod);
            this.groupBox5.Location = new System.Drawing.Point(290, 158);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(161, 102);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Tweaks";
            // 
            // chkAmplify
            // 
            this.chkAmplify.AutoSize = true;
            this.chkAmplify.Location = new System.Drawing.Point(6, 43);
            this.chkAmplify.Name = "chkAmplify";
            this.chkAmplify.Size = new System.Drawing.Size(62, 17);
            this.chkAmplify.TabIndex = 3;
            this.chkAmplify.Text = "Amplify:";
            this.chkAmplify.UseVisualStyleBackColor = true;
            this.chkAmplify.CheckedChanged += new System.EventHandler(this.chkAmplify_CheckedChanged);
            // 
            // txtAmplify
            // 
            this.txtAmplify.Location = new System.Drawing.Point(82, 42);
            this.txtAmplify.Name = "txtAmplify";
            this.txtAmplify.Size = new System.Drawing.Size(46, 20);
            this.txtAmplify.TabIndex = 2;
            this.txtAmplify.Text = "1,0";
            this.txtAmplify.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtAmplify.TextChanged += new System.EventHandler(this.txtAmplify_TextChanged);
            // 
            // chkShowDemod
            // 
            this.chkShowDemod.AutoSize = true;
            this.chkShowDemod.Location = new System.Drawing.Point(6, 19);
            this.chkShowDemod.Name = "chkShowDemod";
            this.chkShowDemod.Size = new System.Drawing.Size(122, 17);
            this.chkShowDemod.TabIndex = 0;
            this.chkShowDemod.Text = "Show Demod Signal";
            this.chkShowDemod.UseVisualStyleBackColor = true;
            this.chkShowDemod.CheckedChanged += new System.EventHandler(this.chkShowDemod_CheckedChanged);
            // 
            // chkNative
            // 
            this.chkNative.AutoSize = true;
            this.chkNative.Location = new System.Drawing.Point(6, 66);
            this.chkNative.Name = "chkNative";
            this.chkNative.Size = new System.Drawing.Size(106, 17);
            this.chkNative.TabIndex = 4;
            this.chkNative.Text = "Native Functions";
            this.chkNative.UseVisualStyleBackColor = true;
            this.chkNative.CheckedChanged += new System.EventHandler(this.chkNative_CheckedChanged);
            // 
            // DemodulationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 272);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "DemodulationDialog";
            this.Text = "DemodulationDialog";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkEnableDemod;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioFMAccurate;
        private System.Windows.Forms.RadioButton radioFMFast;
        private System.Windows.Forms.RadioButton radioAM;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioFilter2;
        private System.Windows.Forms.RadioButton radioFilter4;
        private System.Windows.Forms.RadioButton radioFilter8;
        private System.Windows.Forms.CheckBox chkEnableCursorWin;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.TextBox txtSamplingRate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioFilter16;
        private System.Windows.Forms.RadioButton radioFilter32;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton radioLowPass16;
        private System.Windows.Forms.RadioButton radioLowPass4;
        private System.Windows.Forms.RadioButton radioLowPass32;
        private System.Windows.Forms.RadioButton radioLowPass8;
        private System.Windows.Forms.RadioButton radioLowPass2;
        private System.Windows.Forms.CheckBox chkEnableLowpass;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox chkShowDemod;
        private System.Windows.Forms.TextBox txtAmplify;
        private System.Windows.Forms.CheckBox chkAmplify;
        private System.Windows.Forms.RadioButton radioFilter256;
        private System.Windows.Forms.RadioButton radioFilter128;
        private System.Windows.Forms.RadioButton radioFilter64;
        private System.Windows.Forms.RadioButton radioLowPass256;
        private System.Windows.Forms.RadioButton radioLowPass128;
        private System.Windows.Forms.RadioButton radioLowPass64;
        private System.Windows.Forms.TextBox txtDecim;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkNative;
    }
}