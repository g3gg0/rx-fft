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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsDialog));
            this.chkFastAtan2 = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkSubSample = new System.Windows.Forms.CheckBox();
            this.lblOversampling = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRate = new System.Windows.Forms.TextBox();
            this.chkInvert = new System.Windows.Forms.CheckBox();
            this.txtInternalOvers = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkL3CellBroadcast = new System.Windows.Forms.CheckBox();
            this.chkL3SniffIMSI = new System.Windows.Forms.CheckBox();
            this.chkL3ShowUnhandled = new System.Windows.Forms.CheckBox();
            this.chkL3DumpRaw = new System.Windows.Forms.CheckBox();
            this.txtSubSampleOffset = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnBurstLengthB = new System.Windows.Forms.Button();
            this.btnBurstLengthA = new System.Windows.Forms.Button();
            this.chkPhaseAutoOffset = new System.Windows.Forms.CheckBox();
            this.txtPhaseOffset = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtOffset4 = new System.Windows.Forms.TextBox();
            this.txtOffset3 = new System.Windows.Forms.TextBox();
            this.txtOffset2 = new System.Windows.Forms.TextBox();
            this.txtOffset1 = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtDecisionLevel = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSinxDepth = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.radioOvsLinear = new System.Windows.Forms.RadioButton();
            this.radioOvsSinx = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.radioL1PreallocateSDCCHs = new System.Windows.Forms.RadioButton();
            this.radioL1PreallocateTCHs = new System.Windows.Forms.RadioButton();
            this.radioL1PreallocateNone = new System.Windows.Forms.RadioButton();
            this.chkL1ShowFaulty = new System.Windows.Forms.CheckBox();
            this.chkL1DumpEncrypted = new System.Windows.Forms.CheckBox();
            this.chkL1DumpFrames = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label19 = new System.Windows.Forms.Label();
            this.radioL2ShowAuto = new System.Windows.Forms.RadioButton();
            this.radioL2ShowCrypted = new System.Windows.Forms.RadioButton();
            this.radioL2ShowAll = new System.Windows.Forms.RadioButton();
            this.chkL2DumpFaulty = new System.Windows.Forms.CheckBox();
            this.chkL2DumpRaw = new System.Windows.Forms.CheckBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtA5Kc = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.txtKrakenHost = new System.Windows.Forms.TextBox();
            this.txtSimAuthHost = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkFastAtan2
            // 
            this.chkFastAtan2.AutoSize = true;
            this.chkFastAtan2.Location = new System.Drawing.Point(11, 74);
            this.chkFastAtan2.Name = "chkFastAtan2";
            this.chkFastAtan2.Size = new System.Drawing.Size(132, 17);
            this.chkFastAtan2.TabIndex = 2;
            this.chkFastAtan2.Text = "Fast/inaccurate Atan2";
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
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(206, 152);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Demodulation";
            // 
            // chkSubSample
            // 
            this.chkSubSample.AutoSize = true;
            this.chkSubSample.Location = new System.Drawing.Point(11, 97);
            this.chkSubSample.Name = "chkSubSample";
            this.chkSubSample.Size = new System.Drawing.Size(157, 17);
            this.chkSubSample.TabIndex = 4;
            this.chkSubSample.Text = "Subsample offset correction";
            this.chkSubSample.UseVisualStyleBackColor = true;
            this.chkSubSample.CheckedChanged += new System.EventHandler(this.chkSubSample_CheckedChanged);
            // 
            // lblOversampling
            // 
            this.lblOversampling.AutoSize = true;
            this.lblOversampling.Location = new System.Drawing.Point(91, 47);
            this.lblOversampling.Name = "lblOversampling";
            this.lblOversampling.Size = new System.Drawing.Size(13, 13);
            this.lblOversampling.TabIndex = 18;
            this.lblOversampling.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(175, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Hz";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Oversampling:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Sampling Rate:";
            // 
            // txtRate
            // 
            this.txtRate.Location = new System.Drawing.Point(94, 19);
            this.txtRate.Name = "txtRate";
            this.txtRate.Size = new System.Drawing.Size(75, 20);
            this.txtRate.TabIndex = 1;
            this.txtRate.TextChanged += new System.EventHandler(this.txtRate_TextChanged);
            // 
            // chkInvert
            // 
            this.chkInvert.AutoSize = true;
            this.chkInvert.Enabled = false;
            this.chkInvert.Location = new System.Drawing.Point(11, 120);
            this.chkInvert.Name = "chkInvert";
            this.chkInvert.Size = new System.Drawing.Size(113, 17);
            this.chkInvert.TabIndex = 3;
            this.chkInvert.Text = "Inverted Spectrum";
            this.chkInvert.UseVisualStyleBackColor = true;
            this.chkInvert.CheckedChanged += new System.EventHandler(this.chkInvert_CheckedChanged);
            // 
            // txtInternalOvers
            // 
            this.txtInternalOvers.Location = new System.Drawing.Point(137, 20);
            this.txtInternalOvers.Name = "txtInternalOvers";
            this.txtInternalOvers.Size = new System.Drawing.Size(61, 20);
            this.txtInternalOvers.TabIndex = 7;
            this.txtInternalOvers.TextChanged += new System.EventHandler(this.txtInternalOvers_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(112, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Internal Oversampling:";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(80, 459);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 99;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkL3CellBroadcast);
            this.groupBox2.Controls.Add(this.chkL3SniffIMSI);
            this.groupBox2.Controls.Add(this.chkL3ShowUnhandled);
            this.groupBox2.Controls.Add(this.chkL3DumpRaw);
            this.groupBox2.Location = new System.Drawing.Point(224, 249);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(206, 118);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "L3 Options";
            // 
            // chkL3CellBroadcast
            // 
            this.chkL3CellBroadcast.AutoSize = true;
            this.chkL3CellBroadcast.Location = new System.Drawing.Point(10, 64);
            this.chkL3CellBroadcast.Name = "chkL3CellBroadcast";
            this.chkL3CellBroadcast.Size = new System.Drawing.Size(174, 17);
            this.chkL3CellBroadcast.TabIndex = 20;
            this.chkL3CellBroadcast.Text = "Show Cell Broadcast messages";
            this.chkL3CellBroadcast.UseVisualStyleBackColor = true;
            this.chkL3CellBroadcast.CheckedChanged += new System.EventHandler(this.chkL3CellBroadcast_CheckedChanged);
            // 
            // chkL3SniffIMSI
            // 
            this.chkL3SniffIMSI.AutoSize = true;
            this.chkL3SniffIMSI.Location = new System.Drawing.Point(10, 87);
            this.chkL3SniffIMSI.Name = "chkL3SniffIMSI";
            this.chkL3SniffIMSI.Size = new System.Drawing.Size(108, 17);
            this.chkL3SniffIMSI.TabIndex = 17;
            this.chkL3SniffIMSI.Text = "Log sniffed IMSIs";
            this.chkL3SniffIMSI.UseVisualStyleBackColor = true;
            this.chkL3SniffIMSI.CheckedChanged += new System.EventHandler(this.chkL3SniffIMSI_CheckedChanged);
            // 
            // chkL3ShowUnhandled
            // 
            this.chkL3ShowUnhandled.AutoSize = true;
            this.chkL3ShowUnhandled.Location = new System.Drawing.Point(10, 41);
            this.chkL3ShowUnhandled.Name = "chkL3ShowUnhandled";
            this.chkL3ShowUnhandled.Size = new System.Drawing.Size(150, 17);
            this.chkL3ShowUnhandled.TabIndex = 19;
            this.chkL3ShowUnhandled.Text = "Show unknown messages";
            this.chkL3ShowUnhandled.UseVisualStyleBackColor = true;
            this.chkL3ShowUnhandled.CheckedChanged += new System.EventHandler(this.chkL3ShowUnhandled_CheckedChanged);
            // 
            // chkL3DumpRaw
            // 
            this.chkL3DumpRaw.AutoSize = true;
            this.chkL3DumpRaw.Location = new System.Drawing.Point(10, 18);
            this.chkL3DumpRaw.Name = "chkL3DumpRaw";
            this.chkL3DumpRaw.Size = new System.Drawing.Size(106, 17);
            this.chkL3DumpRaw.TabIndex = 18;
            this.chkL3DumpRaw.Text = "Dump data bytes";
            this.chkL3DumpRaw.UseVisualStyleBackColor = true;
            this.chkL3DumpRaw.CheckedChanged += new System.EventHandler(this.chkL3DumpRaw_CheckedChanged);
            // 
            // txtSubSampleOffset
            // 
            this.txtSubSampleOffset.Location = new System.Drawing.Point(134, 23);
            this.txtSubSampleOffset.Name = "txtSubSampleOffset";
            this.txtSubSampleOffset.Size = new System.Drawing.Size(61, 20);
            this.txtSubSampleOffset.TabIndex = 5;
            this.txtSubSampleOffset.TextChanged += new System.EventHandler(this.txtSubSampleOffset_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnBurstLengthB);
            this.groupBox3.Controls.Add(this.btnBurstLengthA);
            this.groupBox3.Controls.Add(this.chkPhaseAutoOffset);
            this.groupBox3.Controls.Add(this.txtPhaseOffset);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.txtOffset4);
            this.groupBox3.Controls.Add(this.txtOffset3);
            this.groupBox3.Controls.Add(this.txtOffset2);
            this.groupBox3.Controls.Add(this.txtOffset1);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.txtDecisionLevel);
            this.groupBox3.Controls.Add(this.txtSubSampleOffset);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(12, 281);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(206, 172);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Signal Processing (Advanced)";
            // 
            // btnBurstLengthB
            // 
            this.btnBurstLengthB.Location = new System.Drawing.Point(174, 95);
            this.btnBurstLengthB.Name = "btnBurstLengthB";
            this.btnBurstLengthB.Size = new System.Drawing.Size(21, 23);
            this.btnBurstLengthB.TabIndex = 32;
            this.btnBurstLengthB.Text = "B";
            this.btnBurstLengthB.UseVisualStyleBackColor = true;
            this.btnBurstLengthB.Click += new System.EventHandler(this.btnBurstLengthB_Click);
            // 
            // btnBurstLengthA
            // 
            this.btnBurstLengthA.Location = new System.Drawing.Point(152, 95);
            this.btnBurstLengthA.Name = "btnBurstLengthA";
            this.btnBurstLengthA.Size = new System.Drawing.Size(21, 23);
            this.btnBurstLengthA.TabIndex = 32;
            this.btnBurstLengthA.Text = "A";
            this.btnBurstLengthA.UseVisualStyleBackColor = true;
            this.btnBurstLengthA.Click += new System.EventHandler(this.btnBurstLengthA_Click);
            // 
            // chkPhaseAutoOffset
            // 
            this.chkPhaseAutoOffset.AutoSize = true;
            this.chkPhaseAutoOffset.Location = new System.Drawing.Point(82, 48);
            this.chkPhaseAutoOffset.Name = "chkPhaseAutoOffset";
            this.chkPhaseAutoOffset.Size = new System.Drawing.Size(48, 17);
            this.chkPhaseAutoOffset.TabIndex = 31;
            this.chkPhaseAutoOffset.Text = "Auto";
            this.chkPhaseAutoOffset.UseVisualStyleBackColor = true;
            this.chkPhaseAutoOffset.CheckedChanged += new System.EventHandler(this.chkPhaseAutoOffset_CheckedChanged);
            // 
            // txtPhaseOffset
            // 
            this.txtPhaseOffset.Location = new System.Drawing.Point(134, 46);
            this.txtPhaseOffset.Name = "txtPhaseOffset";
            this.txtPhaseOffset.Size = new System.Drawing.Size(61, 20);
            this.txtPhaseOffset.TabIndex = 30;
            this.txtPhaseOffset.TextChanged += new System.EventHandler(this.txtPhaseOffset_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(155, 127);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(24, 13);
            this.label12.TabIndex = 29;
            this.label12.Text = "4/8";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(106, 127);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(24, 13);
            this.label11.TabIndex = 29;
            this.label11.Text = "3/7";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(60, 127);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 13);
            this.label10.TabIndex = 29;
            this.label10.Text = "2/6";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(14, 127);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(24, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "1/5";
            // 
            // txtOffset4
            // 
            this.txtOffset4.Enabled = false;
            this.txtOffset4.Location = new System.Drawing.Point(150, 143);
            this.txtOffset4.Name = "txtOffset4";
            this.txtOffset4.Size = new System.Drawing.Size(33, 20);
            this.txtOffset4.TabIndex = 14;
            this.txtOffset4.TextChanged += new System.EventHandler(this.txtOffset4_TextChanged);
            // 
            // txtOffset3
            // 
            this.txtOffset3.Enabled = false;
            this.txtOffset3.Location = new System.Drawing.Point(103, 143);
            this.txtOffset3.Name = "txtOffset3";
            this.txtOffset3.Size = new System.Drawing.Size(33, 20);
            this.txtOffset3.TabIndex = 13;
            this.txtOffset3.TextChanged += new System.EventHandler(this.txtOffset3_TextChanged);
            // 
            // txtOffset2
            // 
            this.txtOffset2.Enabled = false;
            this.txtOffset2.Location = new System.Drawing.Point(57, 143);
            this.txtOffset2.Name = "txtOffset2";
            this.txtOffset2.Size = new System.Drawing.Size(33, 20);
            this.txtOffset2.TabIndex = 12;
            this.txtOffset2.TextChanged += new System.EventHandler(this.txtOffset2_TextChanged);
            // 
            // txtOffset1
            // 
            this.txtOffset1.Enabled = false;
            this.txtOffset1.Location = new System.Drawing.Point(11, 143);
            this.txtOffset1.Name = "txtOffset1";
            this.txtOffset1.Size = new System.Drawing.Size(33, 20);
            this.txtOffset1.TabIndex = 11;
            this.txtOffset1.TextChanged += new System.EventHandler(this.txtOffset1_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 100);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(139, 13);
            this.label13.TabIndex = 26;
            this.label13.Text = "Burst length correction type:";
            // 
            // txtDecisionLevel
            // 
            this.txtDecisionLevel.Location = new System.Drawing.Point(134, 69);
            this.txtDecisionLevel.Name = "txtDecisionLevel";
            this.txtDecisionLevel.Size = new System.Drawing.Size(61, 20);
            this.txtDecisionLevel.TabIndex = 6;
            this.txtDecisionLevel.TextChanged += new System.EventHandler(this.txtDecisionLevel_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 72);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Decision level:";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(7, 49);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(71, 13);
            this.label15.TabIndex = 0;
            this.label15.Text = "Phase Offset:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "SubSample Offset:";
            // 
            // txtSinxDepth
            // 
            this.txtSinxDepth.Location = new System.Drawing.Point(137, 72);
            this.txtSinxDepth.Name = "txtSinxDepth";
            this.txtSinxDepth.Size = new System.Drawing.Size(61, 20);
            this.txtSinxDepth.TabIndex = 10;
            this.txtSinxDepth.TextChanged += new System.EventHandler(this.textSinxDepth_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 73);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(76, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "Sin(x)/x depth:";
            // 
            // radioOvsLinear
            // 
            this.radioOvsLinear.AutoSize = true;
            this.radioOvsLinear.Location = new System.Drawing.Point(63, 46);
            this.radioOvsLinear.Name = "radioOvsLinear";
            this.radioOvsLinear.Size = new System.Drawing.Size(54, 17);
            this.radioOvsLinear.TabIndex = 8;
            this.radioOvsLinear.TabStop = true;
            this.radioOvsLinear.Text = "Linear";
            this.radioOvsLinear.UseVisualStyleBackColor = true;
            this.radioOvsLinear.CheckedChanged += new System.EventHandler(this.radioOvsLinear_CheckedChanged);
            // 
            // radioOvsSinx
            // 
            this.radioOvsSinx.AutoSize = true;
            this.radioOvsSinx.Location = new System.Drawing.Point(123, 46);
            this.radioOvsSinx.Name = "radioOvsSinx";
            this.radioOvsSinx.Size = new System.Drawing.Size(61, 17);
            this.radioOvsSinx.TabIndex = 9;
            this.radioOvsSinx.TabStop = true;
            this.radioOvsSinx.Text = "Sin(x)/x";
            this.radioOvsSinx.UseVisualStyleBackColor = true;
            this.radioOvsSinx.CheckedChanged += new System.EventHandler(this.radioOvsSinx_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 48);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "Method:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.radioL1PreallocateSDCCHs);
            this.groupBox4.Controls.Add(this.radioL1PreallocateTCHs);
            this.groupBox4.Controls.Add(this.radioL1PreallocateNone);
            this.groupBox4.Controls.Add(this.chkL1ShowFaulty);
            this.groupBox4.Controls.Add(this.chkL1DumpEncrypted);
            this.groupBox4.Controls.Add(this.chkL1DumpFrames);
            this.groupBox4.Location = new System.Drawing.Point(224, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(206, 137);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "L1 Options";
            // 
            // radioL1PreallocateSDCCHs
            // 
            this.radioL1PreallocateSDCCHs.AutoSize = true;
            this.radioL1PreallocateSDCCHs.Location = new System.Drawing.Point(125, 106);
            this.radioL1PreallocateSDCCHs.Name = "radioL1PreallocateSDCCHs";
            this.radioL1PreallocateSDCCHs.Size = new System.Drawing.Size(67, 17);
            this.radioL1PreallocateSDCCHs.TabIndex = 22;
            this.radioL1PreallocateSDCCHs.TabStop = true;
            this.radioL1PreallocateSDCCHs.Text = "SDCCHs";
            this.radioL1PreallocateSDCCHs.UseVisualStyleBackColor = true;
            this.radioL1PreallocateSDCCHs.CheckedChanged += new System.EventHandler(this.radioL1PreallocateSDCCHs_CheckedChanged);
            // 
            // radioL1PreallocateTCHs
            // 
            this.radioL1PreallocateTCHs.AutoSize = true;
            this.radioL1PreallocateTCHs.Location = new System.Drawing.Point(66, 106);
            this.radioL1PreallocateTCHs.Name = "radioL1PreallocateTCHs";
            this.radioL1PreallocateTCHs.Size = new System.Drawing.Size(52, 17);
            this.radioL1PreallocateTCHs.TabIndex = 21;
            this.radioL1PreallocateTCHs.TabStop = true;
            this.radioL1PreallocateTCHs.Text = "TCHs";
            this.radioL1PreallocateTCHs.UseVisualStyleBackColor = true;
            this.radioL1PreallocateTCHs.CheckedChanged += new System.EventHandler(this.radioL1PreallocateTCHs_CheckedChanged);
            // 
            // radioL1PreallocateNone
            // 
            this.radioL1PreallocateNone.AutoSize = true;
            this.radioL1PreallocateNone.Location = new System.Drawing.Point(10, 106);
            this.radioL1PreallocateNone.Name = "radioL1PreallocateNone";
            this.radioL1PreallocateNone.Size = new System.Drawing.Size(49, 17);
            this.radioL1PreallocateNone.TabIndex = 20;
            this.radioL1PreallocateNone.TabStop = true;
            this.radioL1PreallocateNone.Text = "none";
            this.radioL1PreallocateNone.UseVisualStyleBackColor = true;
            this.radioL1PreallocateNone.CheckedChanged += new System.EventHandler(this.radioL1PreallocateNone_CheckedChanged);
            // 
            // chkL1ShowFaulty
            // 
            this.chkL1ShowFaulty.AutoSize = true;
            this.chkL1ShowFaulty.Location = new System.Drawing.Point(10, 66);
            this.chkL1ShowFaulty.Name = "chkL1ShowFaulty";
            this.chkL1ShowFaulty.Size = new System.Drawing.Size(112, 17);
            this.chkL1ShowFaulty.TabIndex = 19;
            this.chkL1ShowFaulty.Text = "Show faulty bursts";
            this.chkL1ShowFaulty.UseVisualStyleBackColor = true;
            this.chkL1ShowFaulty.CheckedChanged += new System.EventHandler(this.chkL1ShowFaulty_CheckedChanged);
            // 
            // chkL1DumpEncrypted
            // 
            this.chkL1DumpEncrypted.AutoSize = true;
            this.chkL1DumpEncrypted.Location = new System.Drawing.Point(10, 19);
            this.chkL1DumpEncrypted.Name = "chkL1DumpEncrypted";
            this.chkL1DumpEncrypted.Size = new System.Drawing.Size(169, 17);
            this.chkL1DumpEncrypted.TabIndex = 17;
            this.chkL1DumpEncrypted.Text = "Dump bits for decrypted bursts";
            this.chkL1DumpEncrypted.UseVisualStyleBackColor = true;
            this.chkL1DumpEncrypted.CheckedChanged += new System.EventHandler(this.chkL1DumpEncrypted_CheckedChanged);
            // 
            // chkL1DumpFrames
            // 
            this.chkL1DumpFrames.AutoSize = true;
            this.chkL1DumpFrames.Location = new System.Drawing.Point(10, 43);
            this.chkL1DumpFrames.Name = "chkL1DumpFrames";
            this.chkL1DumpFrames.Size = new System.Drawing.Size(154, 17);
            this.chkL1DumpFrames.TabIndex = 16;
            this.chkL1DumpFrames.Text = "Dump all frame infos (slow!)";
            this.chkL1DumpFrames.UseVisualStyleBackColor = true;
            this.chkL1DumpFrames.CheckedChanged += new System.EventHandler(this.chkL1DumpFrames_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label19);
            this.groupBox5.Controls.Add(this.radioL2ShowAuto);
            this.groupBox5.Controls.Add(this.radioL2ShowCrypted);
            this.groupBox5.Controls.Add(this.radioL2ShowAll);
            this.groupBox5.Controls.Add(this.chkL2DumpFaulty);
            this.groupBox5.Controls.Add(this.chkL2DumpRaw);
            this.groupBox5.Location = new System.Drawing.Point(224, 155);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(206, 88);
            this.groupBox5.TabIndex = 19;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "L2 Options";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(6, 19);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(37, 13);
            this.label19.TabIndex = 23;
            this.label19.Text = "Show:";
            // 
            // radioL2ShowAuto
            // 
            this.radioL2ShowAuto.AutoSize = true;
            this.radioL2ShowAuto.Location = new System.Drawing.Point(45, 17);
            this.radioL2ShowAuto.Name = "radioL2ShowAuto";
            this.radioL2ShowAuto.Size = new System.Drawing.Size(46, 17);
            this.radioL2ShowAuto.TabIndex = 22;
            this.radioL2ShowAuto.TabStop = true;
            this.radioL2ShowAuto.Text = "auto";
            this.radioL2ShowAuto.UseVisualStyleBackColor = true;
            this.radioL2ShowAuto.CheckedChanged += new System.EventHandler(this.radioL2ShowNone_CheckedChanged);
            // 
            // radioL2ShowCrypted
            // 
            this.radioL2ShowCrypted.AutoSize = true;
            this.radioL2ShowCrypted.Location = new System.Drawing.Point(96, 17);
            this.radioL2ShowCrypted.Name = "radioL2ShowCrypted";
            this.radioL2ShowCrypted.Size = new System.Drawing.Size(60, 17);
            this.radioL2ShowCrypted.TabIndex = 21;
            this.radioL2ShowCrypted.TabStop = true;
            this.radioL2ShowCrypted.Text = "crypted";
            this.radioL2ShowCrypted.UseVisualStyleBackColor = true;
            this.radioL2ShowCrypted.CheckedChanged += new System.EventHandler(this.radioL2ShowCrypted_CheckedChanged);
            // 
            // radioL2ShowAll
            // 
            this.radioL2ShowAll.AutoSize = true;
            this.radioL2ShowAll.Location = new System.Drawing.Point(157, 17);
            this.radioL2ShowAll.Name = "radioL2ShowAll";
            this.radioL2ShowAll.Size = new System.Drawing.Size(35, 17);
            this.radioL2ShowAll.TabIndex = 20;
            this.radioL2ShowAll.TabStop = true;
            this.radioL2ShowAll.Text = "all";
            this.radioL2ShowAll.UseVisualStyleBackColor = true;
            this.radioL2ShowAll.CheckedChanged += new System.EventHandler(this.radioL2ShowAll_CheckedChanged);
            // 
            // chkL2DumpFaulty
            // 
            this.chkL2DumpFaulty.AutoSize = true;
            this.chkL2DumpFaulty.Location = new System.Drawing.Point(10, 61);
            this.chkL2DumpFaulty.Name = "chkL2DumpFaulty";
            this.chkL2DumpFaulty.Size = new System.Drawing.Size(123, 17);
            this.chkL2DumpFaulty.TabIndex = 19;
            this.chkL2DumpFaulty.Text = "Dump faulty packets";
            this.chkL2DumpFaulty.UseVisualStyleBackColor = true;
            this.chkL2DumpFaulty.CheckedChanged += new System.EventHandler(this.chkL2DumpFaulty_CheckedChanged);
            // 
            // chkL2DumpRaw
            // 
            this.chkL2DumpRaw.AutoSize = true;
            this.chkL2DumpRaw.Location = new System.Drawing.Point(10, 40);
            this.chkL2DumpRaw.Name = "chkL2DumpRaw";
            this.chkL2DumpRaw.Size = new System.Drawing.Size(106, 17);
            this.chkL2DumpRaw.TabIndex = 18;
            this.chkL2DumpRaw.Text = "Dump data bytes";
            this.chkL2DumpRaw.UseVisualStyleBackColor = true;
            this.chkL2DumpRaw.CheckedChanged += new System.EventHandler(this.chkL2DumpRaw_CheckedChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.radioOvsLinear);
            this.groupBox6.Controls.Add(this.label4);
            this.groupBox6.Controls.Add(this.txtInternalOvers);
            this.groupBox6.Controls.Add(this.label7);
            this.groupBox6.Controls.Add(this.radioOvsSinx);
            this.groupBox6.Controls.Add(this.label8);
            this.groupBox6.Controls.Add(this.txtSinxDepth);
            this.groupBox6.Location = new System.Drawing.Point(12, 172);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(206, 103);
            this.groupBox6.TabIndex = 100;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Signal Processing (slow!)";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(8, 22);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(23, 13);
            this.label16.TabIndex = 101;
            this.label16.Text = "Kc:";
            // 
            // txtA5Kc
            // 
            this.txtA5Kc.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtA5Kc.Location = new System.Drawing.Point(37, 19);
            this.txtA5Kc.Name = "txtA5Kc";
            this.txtA5Kc.Size = new System.Drawing.Size(163, 20);
            this.txtA5Kc.TabIndex = 102;
            this.txtA5Kc.WordWrap = false;
            this.txtA5Kc.TextChanged += new System.EventHandler(this.txtA5Kc_TextChanged);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label18);
            this.groupBox7.Controls.Add(this.label17);
            this.groupBox7.Controls.Add(this.txtKrakenHost);
            this.groupBox7.Controls.Add(this.txtSimAuthHost);
            this.groupBox7.Controls.Add(this.txtA5Kc);
            this.groupBox7.Controls.Add(this.label16);
            this.groupBox7.Location = new System.Drawing.Point(224, 373);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(206, 106);
            this.groupBox7.TabIndex = 103;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "A5 Decryption";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(10, 76);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(69, 13);
            this.label18.TabIndex = 104;
            this.label18.Text = "Kraken Host:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(10, 50);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(79, 13);
            this.label17.TabIndex = 104;
            this.label17.Text = "SIM Auth Host:";
            // 
            // txtKrakenHost
            // 
            this.txtKrakenHost.Location = new System.Drawing.Point(100, 72);
            this.txtKrakenHost.Name = "txtKrakenHost";
            this.txtKrakenHost.Size = new System.Drawing.Size(100, 20);
            this.txtKrakenHost.TabIndex = 103;
            this.txtKrakenHost.TextChanged += new System.EventHandler(this.txtKrakenHost_TextChanged);
            // 
            // txtSimAuthHost
            // 
            this.txtSimAuthHost.Location = new System.Drawing.Point(100, 46);
            this.txtSimAuthHost.Name = "txtSimAuthHost";
            this.txtSimAuthHost.Size = new System.Drawing.Size(100, 20);
            this.txtSimAuthHost.TabIndex = 103;
            this.txtSimAuthHost.TextChanged += new System.EventHandler(this.txtSimAuthHost_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 90);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 13);
            this.label14.TabIndex = 23;
            this.label14.Text = "Preallocate:";
            // 
            // OptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 497);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsDialog";
            this.Text = "GSM Decoding Options";
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
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
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
        private System.Windows.Forms.CheckBox chkL3DumpRaw;
        private System.Windows.Forms.CheckBox chkL3SniffIMSI;
        private System.Windows.Forms.CheckBox chkL3ShowUnhandled;
        private System.Windows.Forms.CheckBox chkSubSample;
        private System.Windows.Forms.TextBox txtInternalOvers;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSubSampleOffset;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtDecisionLevel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtSinxDepth;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.RadioButton radioOvsLinear;
        private System.Windows.Forms.RadioButton radioOvsSinx;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtOffset4;
        private System.Windows.Forms.TextBox txtOffset3;
        private System.Windows.Forms.TextBox txtOffset2;
        private System.Windows.Forms.TextBox txtOffset1;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox chkL3CellBroadcast;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox chkL1DumpFrames;
        private System.Windows.Forms.CheckBox chkL1DumpEncrypted;
        private System.Windows.Forms.CheckBox chkL2DumpRaw;
        private System.Windows.Forms.TextBox txtPhaseOffset;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.CheckBox chkPhaseAutoOffset;
        private System.Windows.Forms.Button btnBurstLengthB;
        private System.Windows.Forms.Button btnBurstLengthA;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckBox chkL2DumpFaulty;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtA5Kc;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtSimAuthHost;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtKrakenHost;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.RadioButton radioL2ShowAuto;
        private System.Windows.Forms.RadioButton radioL2ShowCrypted;
        private System.Windows.Forms.RadioButton radioL2ShowAll;
        private System.Windows.Forms.CheckBox chkL1ShowFaulty;
        private System.Windows.Forms.RadioButton radioL1PreallocateSDCCHs;
        private System.Windows.Forms.RadioButton radioL1PreallocateTCHs;
        private System.Windows.Forms.RadioButton radioL1PreallocateNone;
        private System.Windows.Forms.Label label14;
    }
}