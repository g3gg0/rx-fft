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
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtOffset4 = new System.Windows.Forms.TextBox();
            this.txtOffset3 = new System.Windows.Forms.TextBox();
            this.txtOffset2 = new System.Windows.Forms.TextBox();
            this.txtOffset1 = new System.Windows.Forms.TextBox();
            this.txtSinxDepth = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.radioOvsLinear = new System.Windows.Forms.RadioButton();
            this.radioOvsSinx = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.txtDecisionLevel = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.chkL1DumpEncrypted = new System.Windows.Forms.CheckBox();
            this.chkL1DumpFrames = new System.Windows.Forms.CheckBox();
            this.chkL1ShowEncrypted = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.chkL2ShowAllFrames = new System.Windows.Forms.CheckBox();
            this.chkL2DumpRaw = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkFastAtan2
            // 
            this.chkFastAtan2.AutoSize = true;
            this.chkFastAtan2.Location = new System.Drawing.Point(11, 78);
            this.chkFastAtan2.Name = "chkFastAtan2";
            this.chkFastAtan2.Size = new System.Drawing.Size(133, 17);
            this.chkFastAtan2.TabIndex = 2;
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
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(206, 153);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "GMSK Processing";
            // 
            // chkSubSample
            // 
            this.chkSubSample.AutoSize = true;
            this.chkSubSample.Location = new System.Drawing.Point(11, 126);
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
            this.lblOversampling.Location = new System.Drawing.Point(91, 50);
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
            this.label3.Location = new System.Drawing.Point(9, 50);
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
            this.chkInvert.Location = new System.Drawing.Point(11, 102);
            this.chkInvert.Name = "chkInvert";
            this.chkInvert.Size = new System.Drawing.Size(113, 17);
            this.chkInvert.TabIndex = 3;
            this.chkInvert.Text = "Inverted Spectrum";
            this.chkInvert.UseVisualStyleBackColor = true;
            this.chkInvert.CheckedChanged += new System.EventHandler(this.chkInvert_CheckedChanged);
            // 
            // txtInternalOvers
            // 
            this.txtInternalOvers.Location = new System.Drawing.Point(157, 72);
            this.txtInternalOvers.Name = "txtInternalOvers";
            this.txtInternalOvers.Size = new System.Drawing.Size(38, 20);
            this.txtInternalOvers.TabIndex = 7;
            this.txtInternalOvers.TextChanged += new System.EventHandler(this.txtInternalOvers_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(112, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Internal Oversampling:";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(368, 384);
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
            this.groupBox2.Location = new System.Drawing.Point(224, 178);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(206, 118);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "L3 Options";
            // 
            // chkL3CellBroadcast
            // 
            this.chkL3CellBroadcast.AutoSize = true;
            this.chkL3CellBroadcast.Location = new System.Drawing.Point(10, 88);
            this.chkL3CellBroadcast.Name = "chkL3CellBroadcast";
            this.chkL3CellBroadcast.Size = new System.Drawing.Size(175, 17);
            this.chkL3CellBroadcast.TabIndex = 20;
            this.chkL3CellBroadcast.Text = "Show Cell Broadcast Messages";
            this.chkL3CellBroadcast.UseVisualStyleBackColor = true;
            this.chkL3CellBroadcast.CheckedChanged += new System.EventHandler(this.chkL3CellBroadcast_CheckedChanged);
            // 
            // chkL3SniffIMSI
            // 
            this.chkL3SniffIMSI.AutoSize = true;
            this.chkL3SniffIMSI.Location = new System.Drawing.Point(10, 19);
            this.chkL3SniffIMSI.Name = "chkL3SniffIMSI";
            this.chkL3SniffIMSI.Size = new System.Drawing.Size(77, 17);
            this.chkL3SniffIMSI.TabIndex = 17;
            this.chkL3SniffIMSI.Text = "Sniff IMSIs";
            this.chkL3SniffIMSI.UseVisualStyleBackColor = true;
            this.chkL3SniffIMSI.CheckedChanged += new System.EventHandler(this.chkL3SniffIMSI_CheckedChanged);
            // 
            // chkL3ShowUnhandled
            // 
            this.chkL3ShowUnhandled.AutoSize = true;
            this.chkL3ShowUnhandled.Location = new System.Drawing.Point(10, 65);
            this.chkL3ShowUnhandled.Name = "chkL3ShowUnhandled";
            this.chkL3ShowUnhandled.Size = new System.Drawing.Size(151, 17);
            this.chkL3ShowUnhandled.TabIndex = 19;
            this.chkL3ShowUnhandled.Text = "Show unknown Messages";
            this.chkL3ShowUnhandled.UseVisualStyleBackColor = true;
            this.chkL3ShowUnhandled.CheckedChanged += new System.EventHandler(this.chkL3ShowUnhandled_CheckedChanged);
            // 
            // chkL3DumpRaw
            // 
            this.chkL3DumpRaw.AutoSize = true;
            this.chkL3DumpRaw.Location = new System.Drawing.Point(10, 42);
            this.chkL3DumpRaw.Name = "chkL3DumpRaw";
            this.chkL3DumpRaw.Size = new System.Drawing.Size(121, 17);
            this.chkL3DumpRaw.TabIndex = 18;
            this.chkL3DumpRaw.Text = "Dump Raw Packets";
            this.chkL3DumpRaw.UseVisualStyleBackColor = true;
            this.chkL3DumpRaw.CheckedChanged += new System.EventHandler(this.chkL3DumpRaw_CheckedChanged);
            // 
            // txtSubSampleOffset
            // 
            this.txtSubSampleOffset.Location = new System.Drawing.Point(157, 23);
            this.txtSubSampleOffset.Name = "txtSubSampleOffset";
            this.txtSubSampleOffset.Size = new System.Drawing.Size(38, 20);
            this.txtSubSampleOffset.TabIndex = 5;
            this.txtSubSampleOffset.TextChanged += new System.EventHandler(this.txtSubSampleOffset_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.txtOffset4);
            this.groupBox3.Controls.Add(this.txtOffset3);
            this.groupBox3.Controls.Add(this.txtOffset2);
            this.groupBox3.Controls.Add(this.txtOffset1);
            this.groupBox3.Controls.Add(this.txtSinxDepth);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.radioOvsLinear);
            this.groupBox3.Controls.Add(this.radioOvsSinx);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.txtInternalOvers);
            this.groupBox3.Controls.Add(this.txtDecisionLevel);
            this.groupBox3.Controls.Add(this.txtSubSampleOffset);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(12, 171);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(206, 237);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Signal Processing";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(154, 186);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(24, 13);
            this.label12.TabIndex = 29;
            this.label12.Text = "4/8";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(105, 186);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(24, 13);
            this.label11.TabIndex = 29;
            this.label11.Text = "3/7";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(59, 186);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 13);
            this.label10.TabIndex = 29;
            this.label10.Text = "2/6";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 186);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(24, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "1/5";
            // 
            // txtOffset4
            // 
            this.txtOffset4.Location = new System.Drawing.Point(151, 202);
            this.txtOffset4.Name = "txtOffset4";
            this.txtOffset4.Size = new System.Drawing.Size(33, 20);
            this.txtOffset4.TabIndex = 14;
            this.txtOffset4.TextChanged += new System.EventHandler(this.txtOffset4_TextChanged);
            // 
            // txtOffset3
            // 
            this.txtOffset3.Location = new System.Drawing.Point(102, 202);
            this.txtOffset3.Name = "txtOffset3";
            this.txtOffset3.Size = new System.Drawing.Size(33, 20);
            this.txtOffset3.TabIndex = 13;
            this.txtOffset3.TextChanged += new System.EventHandler(this.txtOffset3_TextChanged);
            // 
            // txtOffset2
            // 
            this.txtOffset2.Location = new System.Drawing.Point(56, 202);
            this.txtOffset2.Name = "txtOffset2";
            this.txtOffset2.Size = new System.Drawing.Size(33, 20);
            this.txtOffset2.TabIndex = 12;
            this.txtOffset2.TextChanged += new System.EventHandler(this.txtOffset2_TextChanged);
            // 
            // txtOffset1
            // 
            this.txtOffset1.Location = new System.Drawing.Point(10, 202);
            this.txtOffset1.Name = "txtOffset1";
            this.txtOffset1.Size = new System.Drawing.Size(33, 20);
            this.txtOffset1.TabIndex = 11;
            this.txtOffset1.TextChanged += new System.EventHandler(this.txtOffset1_TextChanged);
            // 
            // txtSinxDepth
            // 
            this.txtSinxDepth.Location = new System.Drawing.Point(157, 124);
            this.txtSinxDepth.Name = "txtSinxDepth";
            this.txtSinxDepth.Size = new System.Drawing.Size(38, 20);
            this.txtSinxDepth.TabIndex = 10;
            this.txtSinxDepth.TextChanged += new System.EventHandler(this.textSinxDepth_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(7, 166);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(93, 13);
            this.label14.TabIndex = 26;
            this.label14.Text = "(must sum up to 0)";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 152);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(146, 13);
            this.label13.TabIndex = 26;
            this.label13.Text = "Burst length correction in bits:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 125);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(76, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "Sin(x)/x depth:";
            // 
            // radioOvsLinear
            // 
            this.radioOvsLinear.AutoSize = true;
            this.radioOvsLinear.Location = new System.Drawing.Point(59, 98);
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
            this.radioOvsSinx.Location = new System.Drawing.Point(119, 98);
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
            this.label7.Location = new System.Drawing.Point(7, 100);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "Method:";
            // 
            // txtDecisionLevel
            // 
            this.txtDecisionLevel.Location = new System.Drawing.Point(157, 47);
            this.txtDecisionLevel.Name = "txtDecisionLevel";
            this.txtDecisionLevel.Size = new System.Drawing.Size(38, 20);
            this.txtDecisionLevel.TabIndex = 6;
            this.txtDecisionLevel.TextChanged += new System.EventHandler(this.txtDecisionLevel_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Decision level:";
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
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.chkL1DumpEncrypted);
            this.groupBox4.Controls.Add(this.chkL1DumpFrames);
            this.groupBox4.Controls.Add(this.chkL1ShowEncrypted);
            this.groupBox4.Location = new System.Drawing.Point(224, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(206, 91);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "L1 Options";
            // 
            // chkL1DumpEncrypted
            // 
            this.chkL1DumpEncrypted.AutoSize = true;
            this.chkL1DumpEncrypted.Location = new System.Drawing.Point(10, 43);
            this.chkL1DumpEncrypted.Name = "chkL1DumpEncrypted";
            this.chkL1DumpEncrypted.Size = new System.Drawing.Size(136, 17);
            this.chkL1DumpEncrypted.TabIndex = 17;
            this.chkL1DumpEncrypted.Text = "Encrypted DCCH dump";
            this.chkL1DumpEncrypted.UseVisualStyleBackColor = true;
            this.chkL1DumpEncrypted.CheckedChanged += new System.EventHandler(this.chkL1DumpEncrypted_CheckedChanged);
            // 
            // chkL1DumpFrames
            // 
            this.chkL1DumpFrames.AutoSize = true;
            this.chkL1DumpFrames.Location = new System.Drawing.Point(10, 66);
            this.chkL1DumpFrames.Name = "chkL1DumpFrames";
            this.chkL1DumpFrames.Size = new System.Drawing.Size(121, 17);
            this.chkL1DumpFrames.TabIndex = 16;
            this.chkL1DumpFrames.Text = "Dump all frame infos";
            this.chkL1DumpFrames.UseVisualStyleBackColor = true;
            this.chkL1DumpFrames.CheckedChanged += new System.EventHandler(this.chkL1DumpFrames_CheckedChanged);
            // 
            // chkL1ShowEncrypted
            // 
            this.chkL1ShowEncrypted.AutoSize = true;
            this.chkL1ShowEncrypted.Location = new System.Drawing.Point(10, 19);
            this.chkL1ShowEncrypted.Name = "chkL1ShowEncrypted";
            this.chkL1ShowEncrypted.Size = new System.Drawing.Size(161, 17);
            this.chkL1ShowEncrypted.TabIndex = 15;
            this.chkL1ShowEncrypted.Text = "Encrypted DCCH notification";
            this.chkL1ShowEncrypted.UseVisualStyleBackColor = true;
            this.chkL1ShowEncrypted.CheckedChanged += new System.EventHandler(this.chkL1ShowEncrypted_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.chkL2ShowAllFrames);
            this.groupBox5.Controls.Add(this.chkL2DumpRaw);
            this.groupBox5.Location = new System.Drawing.Point(224, 109);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(206, 63);
            this.groupBox5.TabIndex = 19;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "L2 Options";
            // 
            // chkL2ShowAllFrames
            // 
            this.chkL2ShowAllFrames.AutoSize = true;
            this.chkL2ShowAllFrames.Location = new System.Drawing.Point(10, 17);
            this.chkL2ShowAllFrames.Name = "chkL2ShowAllFrames";
            this.chkL2ShowAllFrames.Size = new System.Drawing.Size(169, 17);
            this.chkL2ShowAllFrames.TabIndex = 16;
            this.chkL2ShowAllFrames.Text = "Show all Frames (Empty+Multi)";
            this.chkL2ShowAllFrames.UseVisualStyleBackColor = true;
            this.chkL2ShowAllFrames.CheckedChanged += new System.EventHandler(this.chkL2ShowAllFrames_CheckedChanged);
            // 
            // chkL2DumpRaw
            // 
            this.chkL2DumpRaw.AutoSize = true;
            this.chkL2DumpRaw.Location = new System.Drawing.Point(10, 40);
            this.chkL2DumpRaw.Name = "chkL2DumpRaw";
            this.chkL2DumpRaw.Size = new System.Drawing.Size(121, 17);
            this.chkL2DumpRaw.TabIndex = 18;
            this.chkL2DumpRaw.Text = "Dump Raw Packets";
            this.chkL2DumpRaw.UseVisualStyleBackColor = true;
            this.chkL2DumpRaw.CheckedChanged += new System.EventHandler(this.chkL2DumpRaw_CheckedChanged);
            // 
            // OptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 419);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
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
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
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
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox chkL1ShowEncrypted;
        private System.Windows.Forms.CheckBox chkL3CellBroadcast;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox chkL2ShowAllFrames;
        private System.Windows.Forms.CheckBox chkL1DumpFrames;
        private System.Windows.Forms.CheckBox chkL1DumpEncrypted;
        private System.Windows.Forms.CheckBox chkL2DumpRaw;
    }
}