using LibRXFFT.Components.GDI;

namespace GSM_Analyzer
{
    partial class GSMAnalyzer
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

            if (ReadThread != null && ReadThread.IsAlive)
            {
                ThreadActive = false;
                ReadThread.Abort();
                ReadThread.Join();
            }

            if (Source != null)
                Source.Close();

            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GSMAnalyzer));
            this.btnOpen = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.btnSpectrum = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnOptions = new System.Windows.Forms.Button();
            this.btnStats = new System.Windows.Forms.Button();
            this.btnL3Filter = new System.Windows.Forms.Button();
            this.btnStep = new System.Windows.Forms.Button();
            this.chkSingleStep = new System.Windows.Forms.CheckBox();
            this.btnBurst = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.statusSearch = new LibRXFFT.Components.GDI.StatusLamp();
            this.statusLock = new LibRXFFT.Components.GDI.StatusLamp();
            this.statusTrain = new LibRXFFT.Components.GDI.StatusLamp();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblT3 = new System.Windows.Forms.Label();
            this.lblTN = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblT1 = new System.Windows.Forms.Label();
            this.lblT2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lblSucess = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblErrors = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblFreqOffset = new System.Windows.Forms.Label();
            this.lblRate = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.lblCellBroadcast = new System.Windows.Forms.Label();
            this.lblCellIdent = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblLAC = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblMCCMNC = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(3, 2);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtLog);
            this.splitContainer1.Size = new System.Drawing.Size(910, 476);
            this.splitContainer1.SplitterDistance = 81;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 3;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.btnSpectrum);
            this.splitContainer5.Panel1.Controls.Add(this.btnClear);
            this.splitContainer5.Panel1.Controls.Add(this.btnOptions);
            this.splitContainer5.Panel1.Controls.Add(this.btnStats);
            this.splitContainer5.Panel1.Controls.Add(this.btnL3Filter);
            this.splitContainer5.Panel1.Controls.Add(this.btnStep);
            this.splitContainer5.Panel1.Controls.Add(this.chkSingleStep);
            this.splitContainer5.Panel1.Controls.Add(this.btnBurst);
            this.splitContainer5.Panel1.Controls.Add(this.btnOpen);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.groupBox4);
            this.splitContainer5.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer5.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer5.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer5.Size = new System.Drawing.Size(910, 81);
            this.splitContainer5.SplitterDistance = 348;
            this.splitContainer5.SplitterWidth = 1;
            this.splitContainer5.TabIndex = 8;
            // 
            // btnSpectrum
            // 
            this.btnSpectrum.Location = new System.Drawing.Point(186, 29);
            this.btnSpectrum.Name = "btnSpectrum";
            this.btnSpectrum.Size = new System.Drawing.Size(75, 23);
            this.btnSpectrum.TabIndex = 17;
            this.btnSpectrum.Text = "Show Spect";
            this.btnSpectrum.UseVisualStyleBackColor = true;
            this.btnSpectrum.Click += new System.EventHandler(this.btnSpectrum_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(105, 29);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 16;
            this.btnClear.Text = "Clear Log";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnOptions
            // 
            this.btnOptions.Location = new System.Drawing.Point(3, 29);
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(75, 23);
            this.btnOptions.TabIndex = 15;
            this.btnOptions.Text = "Options";
            this.btnOptions.UseVisualStyleBackColor = true;
            this.btnOptions.Click += new System.EventHandler(this.btnOptions_Click);
            // 
            // btnStats
            // 
            this.btnStats.Location = new System.Drawing.Point(267, 2);
            this.btnStats.Name = "btnStats";
            this.btnStats.Size = new System.Drawing.Size(75, 23);
            this.btnStats.TabIndex = 14;
            this.btnStats.Text = "Statistics";
            this.btnStats.UseVisualStyleBackColor = true;
            this.btnStats.Click += new System.EventHandler(this.btnStats_Click);
            // 
            // btnL3Filter
            // 
            this.btnL3Filter.Location = new System.Drawing.Point(267, 29);
            this.btnL3Filter.Name = "btnL3Filter";
            this.btnL3Filter.Size = new System.Drawing.Size(75, 23);
            this.btnL3Filter.TabIndex = 13;
            this.btnL3Filter.Text = "L3 Filtering";
            this.btnL3Filter.UseVisualStyleBackColor = true;
            this.btnL3Filter.Click += new System.EventHandler(this.btnL3Filter_Click);
            // 
            // btnStep
            // 
            this.btnStep.Location = new System.Drawing.Point(105, 2);
            this.btnStep.Name = "btnStep";
            this.btnStep.Size = new System.Drawing.Size(75, 23);
            this.btnStep.TabIndex = 4;
            this.btnStep.Text = "SingleStep";
            this.btnStep.UseVisualStyleBackColor = true;
            this.btnStep.Click += new System.EventHandler(this.btnStep_Click);
            // 
            // chkSingleStep
            // 
            this.chkSingleStep.AutoSize = true;
            this.chkSingleStep.Location = new System.Drawing.Point(84, 7);
            this.chkSingleStep.Name = "chkSingleStep";
            this.chkSingleStep.Size = new System.Drawing.Size(15, 14);
            this.chkSingleStep.TabIndex = 3;
            this.chkSingleStep.UseVisualStyleBackColor = true;
            this.chkSingleStep.CheckedChanged += new System.EventHandler(this.chkSingleStep_CheckedChanged);
            // 
            // btnBurst
            // 
            this.btnBurst.Location = new System.Drawing.Point(186, 2);
            this.btnBurst.Name = "btnBurst";
            this.btnBurst.Size = new System.Drawing.Size(75, 23);
            this.btnBurst.TabIndex = 2;
            this.btnBurst.Text = "Show Bursts";
            this.btnBurst.UseVisualStyleBackColor = true;
            this.btnBurst.Click += new System.EventHandler(this.btnBurst_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.statusSearch);
            this.groupBox4.Controls.Add(this.statusLock);
            this.groupBox4.Controls.Add(this.statusTrain);
            this.groupBox4.Location = new System.Drawing.Point(13, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(96, 78);
            this.groupBox4.TabIndex = 19;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "State";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Train SCH";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Search FCCH";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Locked";
            // 
            // statusSearch
            // 
            this.statusSearch.BackColor = System.Drawing.Color.DarkGray;
            this.statusSearch.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.statusSearch.Location = new System.Drawing.Point(10, 19);
            this.statusSearch.Name = "statusSearch";
            this.statusSearch.Size = new System.Drawing.Size(10, 10);
            this.statusSearch.State = LibRXFFT.Components.GDI.eLampState.Grayed;
            this.statusSearch.TabIndex = 13;
            // 
            // statusLock
            // 
            this.statusLock.BackColor = System.Drawing.Color.DarkGray;
            this.statusLock.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.statusLock.Location = new System.Drawing.Point(10, 53);
            this.statusLock.Name = "statusLock";
            this.statusLock.Size = new System.Drawing.Size(10, 10);
            this.statusLock.State = LibRXFFT.Components.GDI.eLampState.Grayed;
            this.statusLock.TabIndex = 13;
            // 
            // statusTrain
            // 
            this.statusTrain.BackColor = System.Drawing.Color.DarkGray;
            this.statusTrain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.statusTrain.Location = new System.Drawing.Point(10, 36);
            this.statusTrain.Name = "statusTrain";
            this.statusTrain.Size = new System.Drawing.Size(10, 10);
            this.statusTrain.State = LibRXFFT.Components.GDI.eLampState.Grayed;
            this.statusTrain.TabIndex = 13;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblT3);
            this.groupBox3.Controls.Add(this.lblTN);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.lblT1);
            this.groupBox3.Controls.Add(this.lblT2);
            this.groupBox3.Location = new System.Drawing.Point(115, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(82, 78);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Param";
            // 
            // lblT3
            // 
            this.lblT3.AutoSize = true;
            this.lblT3.Location = new System.Drawing.Point(32, 42);
            this.lblT3.Name = "lblT3";
            this.lblT3.Size = new System.Drawing.Size(16, 13);
            this.lblT3.TabIndex = 11;
            this.lblT3.Text = "---";
            // 
            // lblTN
            // 
            this.lblTN.AutoSize = true;
            this.lblTN.Location = new System.Drawing.Point(32, 55);
            this.lblTN.Name = "lblTN";
            this.lblTN.Size = new System.Drawing.Size(16, 13);
            this.lblTN.TabIndex = 11;
            this.lblTN.Text = "---";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 55);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(25, 13);
            this.label13.TabIndex = 11;
            this.label13.Text = "TN:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 42);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(23, 13);
            this.label12.TabIndex = 11;
            this.label12.Text = "T3:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 29);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(23, 13);
            this.label11.TabIndex = 11;
            this.label11.Text = "T2:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 16);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(23, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "T1:";
            // 
            // lblT1
            // 
            this.lblT1.AutoSize = true;
            this.lblT1.Location = new System.Drawing.Point(32, 16);
            this.lblT1.Name = "lblT1";
            this.lblT1.Size = new System.Drawing.Size(16, 13);
            this.lblT1.TabIndex = 11;
            this.lblT1.Text = "---";
            // 
            // lblT2
            // 
            this.lblT2.AutoSize = true;
            this.lblT2.Location = new System.Drawing.Point(32, 29);
            this.lblT2.Name = "lblT2";
            this.lblT2.Size = new System.Drawing.Size(16, 13);
            this.lblT2.TabIndex = 11;
            this.lblT2.Text = "---";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.lblSucess);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.lblErrors);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.lblFreqOffset);
            this.groupBox2.Controls.Add(this.lblRate);
            this.groupBox2.Location = new System.Drawing.Point(203, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(114, 78);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Statistics";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Errors:";
            // 
            // lblSucess
            // 
            this.lblSucess.AutoSize = true;
            this.lblSucess.Location = new System.Drawing.Point(47, 31);
            this.lblSucess.Name = "lblSucess";
            this.lblSucess.Size = new System.Drawing.Size(16, 13);
            this.lblSucess.TabIndex = 7;
            this.lblSucess.Text = "---";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Succ.:";
            // 
            // lblErrors
            // 
            this.lblErrors.AutoSize = true;
            this.lblErrors.Location = new System.Drawing.Point(47, 18);
            this.lblErrors.Name = "lblErrors";
            this.lblErrors.Size = new System.Drawing.Size(16, 13);
            this.lblErrors.TabIndex = 6;
            this.lblErrors.Text = "---";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 57);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(38, 13);
            this.label15.TabIndex = 14;
            this.label15.Text = "Offset:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 44);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(33, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Rate:";
            // 
            // lblFreqOffset
            // 
            this.lblFreqOffset.AutoSize = true;
            this.lblFreqOffset.Location = new System.Drawing.Point(47, 57);
            this.lblFreqOffset.Name = "lblFreqOffset";
            this.lblFreqOffset.Size = new System.Drawing.Size(16, 13);
            this.lblFreqOffset.TabIndex = 10;
            this.lblFreqOffset.Text = "---";
            // 
            // lblRate
            // 
            this.lblRate.AutoSize = true;
            this.lblRate.Location = new System.Drawing.Point(47, 44);
            this.lblRate.Name = "lblRate";
            this.lblRate.Size = new System.Drawing.Size(16, 13);
            this.lblRate.TabIndex = 10;
            this.lblRate.Text = "---";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.lblCellBroadcast);
            this.groupBox1.Controls.Add(this.lblCellIdent);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.lblLAC);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.lblMCCMNC);
            this.groupBox1.Location = new System.Drawing.Point(323, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(132, 78);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cell Info";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 18);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(62, 13);
            this.label9.TabIndex = 14;
            this.label9.Text = "MCC/MNC:";
            // 
            // lblCellBroadcast
            // 
            this.lblCellBroadcast.AutoSize = true;
            this.lblCellBroadcast.Location = new System.Drawing.Point(77, 57);
            this.lblCellBroadcast.Name = "lblCellBroadcast";
            this.lblCellBroadcast.Size = new System.Drawing.Size(16, 13);
            this.lblCellBroadcast.TabIndex = 15;
            this.lblCellBroadcast.Text = "---";
            // 
            // lblCellIdent
            // 
            this.lblCellIdent.AutoSize = true;
            this.lblCellIdent.Location = new System.Drawing.Point(77, 44);
            this.lblCellIdent.Name = "lblCellIdent";
            this.lblCellIdent.Size = new System.Drawing.Size(16, 13);
            this.lblCellIdent.TabIndex = 15;
            this.lblCellIdent.Text = "---";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "LAC: ";
            // 
            // lblLAC
            // 
            this.lblLAC.AutoSize = true;
            this.lblLAC.Location = new System.Drawing.Point(77, 31);
            this.lblLAC.Name = "lblLAC";
            this.lblLAC.Size = new System.Drawing.Size(16, 13);
            this.lblLAC.TabIndex = 15;
            this.lblLAC.Text = "---";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 57);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(64, 13);
            this.label14.TabIndex = 14;
            this.label14.Text = "CellBroadc.:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "CellIdent:";
            // 
            // lblMCCMNC
            // 
            this.lblMCCMNC.AutoSize = true;
            this.lblMCCMNC.Location = new System.Drawing.Point(77, 18);
            this.lblMCCMNC.Name = "lblMCCMNC";
            this.lblMCCMNC.Size = new System.Drawing.Size(16, 13);
            this.lblMCCMNC.TabIndex = 15;
            this.lblMCCMNC.Text = "---";
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(910, 394);
            this.txtLog.TabIndex = 1;
            // 
            // GSMAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(910, 476);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GSMAnalyzer";
            this.Text = "GSMAnalyzer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel1.PerformLayout();
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnBurst;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSucess;
        private System.Windows.Forms.Label lblErrors;
        private System.Windows.Forms.Button btnStep;
        private System.Windows.Forms.CheckBox chkSingleStep;
        private System.Windows.Forms.Label lblRate;
        private System.Windows.Forms.Label lblTN;
        private System.Windows.Forms.Label lblT3;
        private System.Windows.Forms.Label lblT2;
        private System.Windows.Forms.Label lblT1;
        private StatusLamp statusLock;
        private StatusLamp statusTrain;
        private StatusLamp statusSearch;
        private System.Windows.Forms.Button btnL3Filter;
        private System.Windows.Forms.Button btnStats;
        private System.Windows.Forms.Button btnOptions;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblLAC;
        private System.Windows.Forms.Label lblMCCMNC;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblCellIdent;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblCellBroadcast;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label lblFreqOffset;
        private System.Windows.Forms.Button btnSpectrum;
    }
}

