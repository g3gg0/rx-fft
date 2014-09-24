namespace RX_Oscilloscope.Components
{
    partial class Oscilloscope
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.waveForm = new LibRXFFT.Components.DirectX.DirectXWaveformDisplay();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.txtSamplingDistance = new LibRXFFT.Components.GDI.TextBoxMouseScrollDecimal();
            this.txtSamplingTime = new LibRXFFT.Components.GDI.TextBoxMouseScrollDecimal();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioBufferSamples = new System.Windows.Forms.RadioButton();
            this.radioBufferTime = new System.Windows.Forms.RadioButton();
            this.lblBufferTime = new System.Windows.Forms.Label();
            this.txtBufferTime = new LibRXFFT.Components.GDI.TextBoxMouseScrollDecimal();
            this.txtBufferSamples = new LibRXFFT.Components.GDI.TextBoxMouseScrollDecimal();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtSamplingRate = new LibRXFFT.Components.GDI.FrequencySelector();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.txtEyePlotBlocks = new LibRXFFT.Components.GDI.TextBoxMouseScrollLong();
            this.chkEyePlot = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.radioPhase = new System.Windows.Forms.RadioButton();
            this.radioPower = new System.Windows.Forms.RadioButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cmbLowPass = new System.Windows.Forms.ComboBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtTriggerLevel = new LibRXFFT.Components.GDI.TextBoxMouseScrollLong();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.chkTriggerFalling = new System.Windows.Forms.CheckBox();
            this.chkTriggerRising = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblPreTrigTime = new System.Windows.Forms.Label();
            this.txtPreTrigSamples = new LibRXFFT.Components.GDI.TextBoxMouseScrollLong();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.waveForm);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl);
            this.splitContainer1.Size = new System.Drawing.Size(997, 511);
            this.splitContainer1.SplitterDistance = 426;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 1;
            // 
            // waveForm
            // 
            this.waveForm.CenterFrequency = 0D;
            this.waveForm.ColorBG = System.Drawing.Color.Black;
            this.waveForm.ColorCursor = System.Drawing.Color.Red;
            this.waveForm.ColorFG = System.Drawing.Color.Cyan;
            this.waveForm.ColorFont = System.Drawing.Color.DarkCyan;
            this.waveForm.ColorOverview = System.Drawing.Color.Red;
            this.waveForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waveForm.Location = new System.Drawing.Point(0, 0);
            this.waveForm.MaxSamples = 10000;
            this.waveForm.Name = "waveForm";
            this.waveForm.SamplingRate = 0D;
            this.waveForm.Size = new System.Drawing.Size(997, 426);
            this.waveForm.SpectParts = 1;
            this.waveForm.TabIndex = 0;
            this.waveForm.UpdateRate = 25D;
            this.waveForm.XZoomFactor = 1D;
            this.waveForm.XZoomFactorMax = 2000D;
            this.waveForm.XZoomFactorMin = 1D;
            this.waveForm.YZoomFactor = 0.0070000002160668373D;
            this.waveForm.YZoomFactorMax = 10D;
            this.waveForm.YZoomFactorMin = 0.005D;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(997, 84);
            this.tabControl.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox7);
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.groupBox5);
            this.tabPage1.Controls.Add(this.groupBox6);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(989, 58);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Source Signal";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label6);
            this.groupBox7.Controls.Add(this.label3);
            this.groupBox7.Controls.Add(this.txtSamplingDistance);
            this.groupBox7.Controls.Add(this.txtSamplingTime);
            this.groupBox7.Location = new System.Drawing.Point(602, 4);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(170, 52);
            this.groupBox7.TabIndex = 10;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Sample Points";
            // 
            // txtSamplingDistance
            // 
            this.txtSamplingDistance.Location = new System.Drawing.Point(82, 34);
            this.txtSamplingDistance.LowerLimit = ((long)(1));
            this.txtSamplingDistance.Name = "txtSamplingDistance";
            this.txtSamplingDistance.Size = new System.Drawing.Size(51, 20);
            this.txtSamplingDistance.TabIndex = 3;
            this.txtSamplingDistance.Text = "1";
            this.txtSamplingDistance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtSamplingDistance.UpperLimit = ((long)(1000000));
            this.txtSamplingDistance.Value = ((long)(1));
            this.txtSamplingDistance.ValueChanged += new System.EventHandler(this.txtSampling_ValueChanged);
            // 
            // txtSamplingOffset
            // 
            this.txtSamplingTime.Location = new System.Drawing.Point(82, 15);
            this.txtSamplingTime.LowerLimit = 1;
            this.txtSamplingTime.Name = "txtSamplingOffset";
            this.txtSamplingTime.Size = new System.Drawing.Size(51, 20);
            this.txtSamplingTime.TabIndex = 3;
            this.txtSamplingTime.Text = "0";
            this.txtSamplingTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtSamplingTime.UpperLimit = 10000000;
            this.txtSamplingTime.Value = 1;
            this.txtSamplingTime.ValueChanged += new System.EventHandler(this.txtSampling_ValueChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioBufferSamples);
            this.groupBox3.Controls.Add(this.radioBufferTime);
            this.groupBox3.Controls.Add(this.lblBufferTime);
            this.groupBox3.Controls.Add(this.txtBufferTime);
            this.groupBox3.Controls.Add(this.txtBufferSamples);
            this.groupBox3.Location = new System.Drawing.Point(410, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(186, 50);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Buffer/Block Size";
            // 
            // radioBufferSamples
            // 
            this.radioBufferSamples.AutoSize = true;
            this.radioBufferSamples.Checked = true;
            this.radioBufferSamples.Location = new System.Drawing.Point(7, 35);
            this.radioBufferSamples.Name = "radioBufferSamples";
            this.radioBufferSamples.Size = new System.Drawing.Size(69, 17);
            this.radioBufferSamples.TabIndex = 5;
            this.radioBufferSamples.TabStop = true;
            this.radioBufferSamples.Text = "[samples]";
            this.radioBufferSamples.UseVisualStyleBackColor = true;
            this.radioBufferSamples.CheckedChanged += new System.EventHandler(this.radioBufferSamples_CheckedChanged);
            // 
            // radioBufferTime
            // 
            this.radioBufferTime.AutoSize = true;
            this.radioBufferTime.Location = new System.Drawing.Point(7, 15);
            this.radioBufferTime.Name = "radioBufferTime";
            this.radioBufferTime.Size = new System.Drawing.Size(42, 17);
            this.radioBufferTime.TabIndex = 5;
            this.radioBufferTime.Text = "[µs]";
            this.radioBufferTime.UseVisualStyleBackColor = true;
            this.radioBufferTime.CheckedChanged += new System.EventHandler(this.radioBufferTime_CheckedChanged);
            // 
            // lblBufferTime
            // 
            this.lblBufferTime.AutoSize = true;
            this.lblBufferTime.Location = new System.Drawing.Point(135, 23);
            this.lblBufferTime.Name = "lblBufferTime";
            this.lblBufferTime.Size = new System.Drawing.Size(34, 13);
            this.lblBufferTime.TabIndex = 4;
            this.lblBufferTime.Text = "(unk.)";
            // 
            // txtBufferTime
            // 
            this.txtBufferTime.Location = new System.Drawing.Point(78, 15);
            this.txtBufferTime.LowerLimit = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.txtBufferTime.Name = "txtBufferTime";
            this.txtBufferTime.Size = new System.Drawing.Size(51, 20);
            this.txtBufferTime.TabIndex = 3;
            this.txtBufferTime.Text = "0";
            this.txtBufferTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtBufferTime.UpperLimit = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.txtBufferTime.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.txtBufferTime.ValueChanged += new System.EventHandler(this.txtBufferTime_Changed);
            // 
            // txtBufferSamples
            // 
            this.txtBufferSamples.Location = new System.Drawing.Point(78, 34);
            this.txtBufferSamples.LowerLimit = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.txtBufferSamples.Name = "txtBufferSamples";
            this.txtBufferSamples.Size = new System.Drawing.Size(51, 20);
            this.txtBufferSamples.TabIndex = 3;
            this.txtBufferSamples.Text = "1000";
            this.txtBufferSamples.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtBufferSamples.UpperLimit = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.txtBufferSamples.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.txtBufferSamples.ValueChanged += new System.EventHandler(this.txtBufferTime_Changed);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtSamplingRate);
            this.groupBox5.Location = new System.Drawing.Point(6, 6);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(107, 46);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Sampling Rate";
            // 
            // txtSamplingRate
            // 
            this.txtSamplingRate.FixedLengthDecades = 10;
            this.txtSamplingRate.FixedLengthString = false;
            this.txtSamplingRate.Frequency = ((long)(48000));
            this.txtSamplingRate.Location = new System.Drawing.Point(6, 16);
            this.txtSamplingRate.Name = "txtSamplingRate";
            this.txtSamplingRate.Size = new System.Drawing.Size(91, 20);
            this.txtSamplingRate.TabIndex = 0;
            this.txtSamplingRate.Text = "48 kHz";
            this.txtSamplingRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtSamplingRate.FrequencyChanged += new System.EventHandler(this.txtSamplingRate_FrequencyChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.txtEyePlotBlocks);
            this.groupBox6.Controls.Add(this.chkEyePlot);
            this.groupBox6.Controls.Add(this.label5);
            this.groupBox6.Controls.Add(this.radioPhase);
            this.groupBox6.Controls.Add(this.radioPower);
            this.groupBox6.Location = new System.Drawing.Point(119, 6);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(285, 46);
            this.groupBox6.TabIndex = 9;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Process";
            // 
            // txtEyePlotBlocks
            // 
            this.txtEyePlotBlocks.Location = new System.Drawing.Point(198, 15);
            this.txtEyePlotBlocks.LowerLimit = ((long)(0));
            this.txtEyePlotBlocks.Name = "txtEyePlotBlocks";
            this.txtEyePlotBlocks.Size = new System.Drawing.Size(33, 20);
            this.txtEyePlotBlocks.TabIndex = 1;
            this.txtEyePlotBlocks.Text = "1";
            this.txtEyePlotBlocks.UpperLimit = ((long)(10000));
            this.txtEyePlotBlocks.Value = ((long)(1));
            this.txtEyePlotBlocks.ValueChanged += new System.EventHandler(this.txtEyePlotBlocks_ValueChanged);
            // 
            // chkEyePlot
            // 
            this.chkEyePlot.AutoSize = true;
            this.chkEyePlot.Location = new System.Drawing.Point(136, 17);
            this.chkEyePlot.Name = "chkEyePlot";
            this.chkEyePlot.Size = new System.Drawing.Size(66, 17);
            this.chkEyePlot.TabIndex = 0;
            this.chkEyePlot.Text = "Eye with";
            this.chkEyePlot.UseVisualStyleBackColor = true;
            this.chkEyePlot.CheckedChanged += new System.EventHandler(this.chkEyePlot_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(237, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Blocks";
            // 
            // radioPhase
            // 
            this.radioPhase.AutoSize = true;
            this.radioPhase.Location = new System.Drawing.Point(68, 16);
            this.radioPhase.Name = "radioPhase";
            this.radioPhase.Size = new System.Drawing.Size(62, 17);
            this.radioPhase.TabIndex = 0;
            this.radioPhase.Text = "ΔPhase";
            this.radioPhase.UseVisualStyleBackColor = true;
            this.radioPhase.CheckedChanged += new System.EventHandler(this.radioPhase_CheckedChanged);
            // 
            // radioPower
            // 
            this.radioPower.AutoSize = true;
            this.radioPower.Checked = true;
            this.radioPower.Location = new System.Drawing.Point(7, 16);
            this.radioPower.Name = "radioPower";
            this.radioPower.Size = new System.Drawing.Size(55, 17);
            this.radioPower.TabIndex = 0;
            this.radioPower.TabStop = true;
            this.radioPower.Text = "Power";
            this.radioPower.UseVisualStyleBackColor = true;
            this.radioPower.CheckedChanged += new System.EventHandler(this.radioPower_CheckedChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox4);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(989, 61);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Pre-Processing";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cmbLowPass);
            this.groupBox4.Location = new System.Drawing.Point(6, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(95, 46);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Low-Pass";
            // 
            // cmbLowPass
            // 
            this.cmbLowPass.FormattingEnabled = true;
            this.cmbLowPass.Items.AddRange(new object[] {
            "none",
            "/2",
            "/4",
            "/8",
            "/16",
            "/32",
            "/64",
            "/128",
            "/256"});
            this.cmbLowPass.Location = new System.Drawing.Point(6, 16);
            this.cmbLowPass.Name = "cmbLowPass";
            this.cmbLowPass.Size = new System.Drawing.Size(74, 21);
            this.cmbLowPass.TabIndex = 6;
            this.cmbLowPass.SelectedIndexChanged += new System.EventHandler(this.cmbLowPass_SelectedIndexChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Controls.Add(this.groupBox2);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(989, 61);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Triggering";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtTriggerLevel);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.chkTriggerFalling);
            this.groupBox1.Controls.Add(this.chkTriggerRising);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(248, 46);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Trigger";
            // 
            // txtTriggerLevel
            // 
            this.txtTriggerLevel.Location = new System.Drawing.Point(174, 16);
            this.txtTriggerLevel.LowerLimit = ((long)(-160));
            this.txtTriggerLevel.Name = "txtTriggerLevel";
            this.txtTriggerLevel.Size = new System.Drawing.Size(41, 20);
            this.txtTriggerLevel.TabIndex = 2;
            this.txtTriggerLevel.Text = "0";
            this.txtTriggerLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtTriggerLevel.UpperLimit = ((long)(0));
            this.txtTriggerLevel.Value = ((long)(0));
            this.txtTriggerLevel.ValueChanged += new System.EventHandler(this.txtTriggerLevel_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(221, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "dB";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(131, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Level:";
            // 
            // chkTriggerFalling
            // 
            this.chkTriggerFalling.AutoSize = true;
            this.chkTriggerFalling.Location = new System.Drawing.Point(68, 18);
            this.chkTriggerFalling.Name = "chkTriggerFalling";
            this.chkTriggerFalling.Size = new System.Drawing.Size(56, 17);
            this.chkTriggerFalling.TabIndex = 0;
            this.chkTriggerFalling.Text = "Falling";
            this.chkTriggerFalling.UseVisualStyleBackColor = true;
            this.chkTriggerFalling.CheckedChanged += new System.EventHandler(this.chkTriggerFalling_CheckedChanged);
            // 
            // chkTriggerRising
            // 
            this.chkTriggerRising.AutoSize = true;
            this.chkTriggerRising.Location = new System.Drawing.Point(7, 18);
            this.chkTriggerRising.Name = "chkTriggerRising";
            this.chkTriggerRising.Size = new System.Drawing.Size(55, 17);
            this.chkTriggerRising.TabIndex = 0;
            this.chkTriggerRising.Text = "Rising";
            this.chkTriggerRising.UseVisualStyleBackColor = true;
            this.chkTriggerRising.CheckedChanged += new System.EventHandler(this.chkTriggerRising_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblPreTrigTime);
            this.groupBox2.Controls.Add(this.txtPreTrigSamples);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(260, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(172, 46);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Pre-Trigger";
            // 
            // lblPreTrigTime
            // 
            this.lblPreTrigTime.AutoSize = true;
            this.lblPreTrigTime.Location = new System.Drawing.Point(117, 20);
            this.lblPreTrigTime.Name = "lblPreTrigTime";
            this.lblPreTrigTime.Size = new System.Drawing.Size(34, 13);
            this.lblPreTrigTime.TabIndex = 4;
            this.lblPreTrigTime.Text = "(unk.)";
            // 
            // txtPreTrigSamples
            // 
            this.txtPreTrigSamples.Location = new System.Drawing.Point(6, 16);
            this.txtPreTrigSamples.LowerLimit = ((long)(-100000));
            this.txtPreTrigSamples.Name = "txtPreTrigSamples";
            this.txtPreTrigSamples.Size = new System.Drawing.Size(47, 20);
            this.txtPreTrigSamples.TabIndex = 3;
            this.txtPreTrigSamples.Text = "0";
            this.txtPreTrigSamples.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPreTrigSamples.UpperLimit = ((long)(100000));
            this.txtPreTrigSamples.Value = ((long)(0));
            this.txtPreTrigSamples.ValueChanged += new System.EventHandler(this.txtPreTrigSamples_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(59, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Samples, = ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Sample Dist";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Offset";
            // 
            // Oscilloscope
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "Oscilloscope";
            this.Size = new System.Drawing.Size(997, 511);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        internal LibRXFFT.Components.DirectX.DirectXWaveformDisplay waveForm;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private LibRXFFT.Components.GDI.TextBoxMouseScrollLong txtTriggerLevel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkTriggerFalling;
        private System.Windows.Forms.CheckBox chkTriggerRising;
        private LibRXFFT.Components.GDI.TextBoxMouseScrollLong txtPreTrigSamples;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblPreTrigTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblBufferTime;
        private LibRXFFT.Components.GDI.TextBoxMouseScrollDecimal txtBufferSamples;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ComboBox cmbLowPass;
        private System.Windows.Forms.GroupBox groupBox5;
        private LibRXFFT.Components.GDI.FrequencySelector txtSamplingRate;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RadioButton radioPhase;
        private System.Windows.Forms.RadioButton radioPower;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private LibRXFFT.Components.GDI.TextBoxMouseScrollLong txtEyePlotBlocks;
        private System.Windows.Forms.CheckBox chkEyePlot;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton radioBufferSamples;
        private System.Windows.Forms.RadioButton radioBufferTime;
        private LibRXFFT.Components.GDI.TextBoxMouseScrollDecimal txtBufferTime;
        private System.Windows.Forms.GroupBox groupBox7;
        private LibRXFFT.Components.GDI.TextBoxMouseScrollDecimal txtSamplingDistance;
        private LibRXFFT.Components.GDI.TextBoxMouseScrollDecimal txtSamplingTime;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
    }
}
