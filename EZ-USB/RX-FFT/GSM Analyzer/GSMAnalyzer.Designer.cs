using LibRXFFT.Components.GDI;
using System;
using System.ComponentModel;

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

            if (ChannelScanThread != null)
            {
                ChannelScanThread.Join(50);
                ChannelScanThread.Abort();
                ChannelScanThread = null;
            }

            if (ReadThread != null && ReadThread.IsAlive)
            {
                ThreadActive = false;
                ReadThread.Join(50);
                ReadThread.Abort();
            }

            if (Source != null)
                Source.CloseControl();

            base.Dispose(disposing);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                CloseSource();
                TextBoxCommitTimer.Stop();
            }
            catch (Exception ex)
            {
            }

            base.OnClosing(e);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GSMAnalyzer));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.label16 = new System.Windows.Forms.Label();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnStep = new System.Windows.Forms.Button();
            this.chkSingleStep = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.chkSlotUsage = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
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
            this.lblDate = new System.Windows.Forms.Label();
            this.lblSnr = new System.Windows.Forms.Label();
            this.lblIdlePower = new System.Windows.Forms.Label();
            this.lblPower = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
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
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnL3Filter = new System.Windows.Forms.ToolStripButton();
            this.btnOptions = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAdvanced = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnDumpStatistics = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnToggleBurst = new System.Windows.Forms.ToolStripMenuItem();
            this.btnToggleSpectrum = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnDump = new System.Windows.Forms.ToolStripMenuItem();
            this.btnClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnQuit = new System.Windows.Forms.ToolStripButton();
            this.asdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.txtArfcn = new LibRXFFT.Components.GDI.TextBoxMouseScroll();
            this.slotUsageControl = new GSM_Analyzer.SlotUsageControl();
            this.statusSearch = new LibRXFFT.Components.GDI.StatusLamp();
            this.statusLock = new LibRXFFT.Components.GDI.StatusLamp();
            this.statusTrain = new LibRXFFT.Components.GDI.StatusLamp();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
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
            this.splitContainer1.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtLog);
            this.splitContainer1.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer1.Size = new System.Drawing.Size(762, 415);
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
            this.splitContainer5.Panel1.Controls.Add(this.txtArfcn);
            this.splitContainer5.Panel1.Controls.Add(this.label16);
            this.splitContainer5.Panel1.Controls.Add(this.btnScan);
            this.splitContainer5.Panel1.Controls.Add(this.btnStep);
            this.splitContainer5.Panel1.Controls.Add(this.chkSingleStep);
            this.splitContainer5.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer5.Panel1MinSize = 0;
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.groupBox5);
            this.splitContainer5.Panel2.Controls.Add(this.groupBox4);
            this.splitContainer5.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer5.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer5.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer5.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer5.Size = new System.Drawing.Size(762, 81);
            this.splitContainer5.SplitterDistance = 0;
            this.splitContainer5.SplitterWidth = 1;
            this.splitContainer5.TabIndex = 8;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(9, 33);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(46, 13);
            this.label16.TabIndex = 19;
            this.label16.Text = "ARFCN:";
            // 
            // btnScan
            // 
            this.btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScan.Location = new System.Drawing.Point(61, 53);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(47, 23);
            this.btnScan.TabIndex = 18;
            this.btnScan.Text = "Scan";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnStep
            // 
            this.btnStep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStep.Location = new System.Drawing.Point(33, 2);
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
            this.chkSingleStep.Location = new System.Drawing.Point(12, 6);
            this.chkSingleStep.Name = "chkSingleStep";
            this.chkSingleStep.Size = new System.Drawing.Size(15, 14);
            this.chkSingleStep.TabIndex = 3;
            this.chkSingleStep.UseVisualStyleBackColor = true;
            this.chkSingleStep.CheckedChanged += new System.EventHandler(this.chkSingleStep_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.chkSlotUsage);
            this.groupBox5.Controls.Add(this.slotUsageControl);
            this.groupBox5.Location = new System.Drawing.Point(561, 2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(615, 78);
            this.groupBox5.TabIndex = 21;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Slot Usage";
            // 
            // chkSlotUsage
            // 
            this.chkSlotUsage.AutoSize = true;
            this.chkSlotUsage.Location = new System.Drawing.Point(75, 0);
            this.chkSlotUsage.Name = "chkSlotUsage";
            this.chkSlotUsage.Size = new System.Drawing.Size(15, 14);
            this.chkSlotUsage.TabIndex = 21;
            this.chkSlotUsage.UseVisualStyleBackColor = true;
            this.chkSlotUsage.CheckedChanged += new System.EventHandler(this.chkSlotUsage_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.statusSearch);
            this.groupBox4.Controls.Add(this.statusLock);
            this.groupBox4.Controls.Add(this.statusTrain);
            this.groupBox4.Location = new System.Drawing.Point(6, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(96, 78);
            this.groupBox4.TabIndex = 19;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "State";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(19, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Train SCH";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(19, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Search FCCH";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(19, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Locked";
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
            this.groupBox3.Location = new System.Drawing.Point(108, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(89, 78);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Param";
            // 
            // lblT3
            // 
            this.lblT3.AutoSize = true;
            this.lblT3.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblT3.Location = new System.Drawing.Point(32, 43);
            this.lblT3.Name = "lblT3";
            this.lblT3.Size = new System.Drawing.Size(23, 12);
            this.lblT3.TabIndex = 11;
            this.lblT3.Text = "---";
            // 
            // lblTN
            // 
            this.lblTN.AutoSize = true;
            this.lblTN.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTN.Location = new System.Drawing.Point(32, 56);
            this.lblTN.Name = "lblTN";
            this.lblTN.Size = new System.Drawing.Size(23, 12);
            this.lblTN.TabIndex = 11;
            this.lblTN.Text = "---";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(6, 55);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(22, 13);
            this.label13.TabIndex = 11;
            this.label13.Text = "TN:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(7, 42);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(21, 13);
            this.label12.TabIndex = 11;
            this.label12.Text = "T3:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(7, 29);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(21, 13);
            this.label11.TabIndex = 11;
            this.label11.Text = "T2:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(7, 16);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(21, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "T1:";
            // 
            // lblT1
            // 
            this.lblT1.AutoSize = true;
            this.lblT1.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblT1.Location = new System.Drawing.Point(32, 17);
            this.lblT1.Name = "lblT1";
            this.lblT1.Size = new System.Drawing.Size(23, 12);
            this.lblT1.TabIndex = 11;
            this.lblT1.Text = "---";
            // 
            // lblT2
            // 
            this.lblT2.AutoSize = true;
            this.lblT2.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblT2.Location = new System.Drawing.Point(32, 30);
            this.lblT2.Name = "lblT2";
            this.lblT2.Size = new System.Drawing.Size(23, 12);
            this.lblT2.TabIndex = 11;
            this.lblT2.Text = "---";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblDate);
            this.groupBox2.Controls.Add(this.lblSnr);
            this.groupBox2.Controls.Add(this.lblIdlePower);
            this.groupBox2.Controls.Add(this.lblPower);
            this.groupBox2.Controls.Add(this.label19);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.lblSucess);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.lblErrors);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.lblFreqOffset);
            this.groupBox2.Controls.Add(this.lblRate);
            this.groupBox2.Location = new System.Drawing.Point(341, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(214, 78);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Statistics";
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDate.Location = new System.Drawing.Point(107, 58);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(23, 12);
            this.lblDate.TabIndex = 15;
            this.lblDate.Text = "---";
            // 
            // lblSnr
            // 
            this.lblSnr.AutoSize = true;
            this.lblSnr.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSnr.Location = new System.Drawing.Point(136, 45);
            this.lblSnr.Name = "lblSnr";
            this.lblSnr.Size = new System.Drawing.Size(23, 12);
            this.lblSnr.TabIndex = 15;
            this.lblSnr.Text = "---";
            // 
            // lblIdlePower
            // 
            this.lblIdlePower.AutoSize = true;
            this.lblIdlePower.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIdlePower.Location = new System.Drawing.Point(136, 32);
            this.lblIdlePower.Name = "lblIdlePower";
            this.lblIdlePower.Size = new System.Drawing.Size(23, 12);
            this.lblIdlePower.TabIndex = 15;
            this.lblIdlePower.Text = "---";
            // 
            // lblPower
            // 
            this.lblPower.AutoSize = true;
            this.lblPower.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPower.Location = new System.Drawing.Point(136, 19);
            this.lblPower.Name = "lblPower";
            this.lblPower.Size = new System.Drawing.Size(23, 12);
            this.lblPower.TabIndex = 15;
            this.lblPower.Text = "---";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(106, 44);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(31, 13);
            this.label19.TabIndex = 15;
            this.label19.Text = "SNR:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(107, 31);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(27, 13);
            this.label18.TabIndex = 15;
            this.label18.Text = "Idle:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(107, 18);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(28, 13);
            this.label17.TabIndex = 15;
            this.label17.Text = "Pwr:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(6, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Errors:";
            // 
            // lblSucess
            // 
            this.lblSucess.AutoSize = true;
            this.lblSucess.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSucess.Location = new System.Drawing.Point(44, 32);
            this.lblSucess.Name = "lblSucess";
            this.lblSucess.Size = new System.Drawing.Size(23, 12);
            this.lblSucess.TabIndex = 7;
            this.lblSucess.Text = "---";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(6, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(36, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Succ.:";
            // 
            // lblErrors
            // 
            this.lblErrors.AutoSize = true;
            this.lblErrors.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblErrors.Location = new System.Drawing.Point(44, 19);
            this.lblErrors.Name = "lblErrors";
            this.lblErrors.Size = new System.Drawing.Size(23, 12);
            this.lblErrors.TabIndex = 6;
            this.lblErrors.Text = "---";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(6, 57);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(38, 13);
            this.label15.TabIndex = 14;
            this.label15.Text = "Offset:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(6, 44);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(32, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Rate:";
            // 
            // lblFreqOffset
            // 
            this.lblFreqOffset.AutoSize = true;
            this.lblFreqOffset.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFreqOffset.Location = new System.Drawing.Point(44, 58);
            this.lblFreqOffset.Name = "lblFreqOffset";
            this.lblFreqOffset.Size = new System.Drawing.Size(23, 12);
            this.lblFreqOffset.TabIndex = 10;
            this.lblFreqOffset.Text = "---";
            // 
            // lblRate
            // 
            this.lblRate.AutoSize = true;
            this.lblRate.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRate.Location = new System.Drawing.Point(44, 45);
            this.lblRate.Name = "lblRate";
            this.lblRate.Size = new System.Drawing.Size(23, 12);
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
            this.groupBox1.Location = new System.Drawing.Point(203, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(132, 78);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cell Info";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(8, 18);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(59, 13);
            this.label9.TabIndex = 14;
            this.label9.Text = "MCC/MNC:";
            // 
            // lblCellBroadcast
            // 
            this.lblCellBroadcast.AutoSize = true;
            this.lblCellBroadcast.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCellBroadcast.Location = new System.Drawing.Point(77, 58);
            this.lblCellBroadcast.Name = "lblCellBroadcast";
            this.lblCellBroadcast.Size = new System.Drawing.Size(23, 12);
            this.lblCellBroadcast.TabIndex = 15;
            this.lblCellBroadcast.Text = "---";
            // 
            // lblCellIdent
            // 
            this.lblCellIdent.AutoSize = true;
            this.lblCellIdent.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCellIdent.Location = new System.Drawing.Point(77, 45);
            this.lblCellIdent.Name = "lblCellIdent";
            this.lblCellIdent.Size = new System.Drawing.Size(23, 12);
            this.lblCellIdent.TabIndex = 15;
            this.lblCellIdent.Text = "---";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(8, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "LAC: ";
            // 
            // lblLAC
            // 
            this.lblLAC.AutoSize = true;
            this.lblLAC.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLAC.Location = new System.Drawing.Point(77, 32);
            this.lblLAC.Name = "lblLAC";
            this.lblLAC.Size = new System.Drawing.Size(23, 12);
            this.lblLAC.TabIndex = 15;
            this.lblLAC.Text = "---";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(8, 57);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 13);
            this.label14.TabIndex = 14;
            this.label14.Text = "CellBroadc.:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(8, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "CellIdent:";
            // 
            // lblMCCMNC
            // 
            this.lblMCCMNC.AutoSize = true;
            this.lblMCCMNC.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMCCMNC.Location = new System.Drawing.Point(77, 19);
            this.lblMCCMNC.Name = "lblMCCMNC";
            this.lblMCCMNC.Size = new System.Drawing.Size(23, 12);
            this.lblMCCMNC.TabIndex = 15;
            this.lblMCCMNC.Text = "---";
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(762, 333);
            this.txtLog.TabIndex = 0;
            this.txtLog.WordWrap = false;
            // 
            // toolStripContainer1
            // 
            this.toolStripContainer1.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(762, 415);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.RightToolStripPanelVisible = false;
            this.toolStripContainer1.Size = new System.Drawing.Size(762, 454);
            this.toolStripContainer1.TabIndex = 1;
            this.toolStripContainer1.Text = "Main menu";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripContainer1.TopToolStripPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpen,
            this.btnL3Filter,
            this.btnOptions,
            this.toolStripSeparator2,
            this.btnAdvanced,
            this.btnClear,
            this.toolStripSeparator1,
            this.btnQuit,
            this.toolStripSeparator3});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(508, 39);
            this.toolStrip1.TabIndex = 0;
            // 
            // btnOpen
            // 
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(72, 36);
            this.btnOpen.Text = "Open";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnL3Filter
            // 
            this.btnL3Filter.Image = ((System.Drawing.Image)(resources.GetObject("btnL3Filter.Image")));
            this.btnL3Filter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnL3Filter.Name = "btnL3Filter";
            this.btnL3Filter.Size = new System.Drawing.Size(84, 36);
            this.btnL3Filter.Text = "L3 Filter";
            this.btnL3Filter.Click += new System.EventHandler(this.btnL3Filter_Click);
            // 
            // btnOptions
            // 
            this.btnOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnOptions.Image")));
            this.btnOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(85, 36);
            this.btnOptions.Text = "Options";
            this.btnOptions.Click += new System.EventHandler(this.btnOptions_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // btnAdvanced
            // 
            this.btnAdvanced.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnDumpStatistics,
            this.toolStripMenuItem1,
            this.btnToggleBurst,
            this.btnToggleSpectrum,
            this.toolStripMenuItem2,
            this.btnDump});
            this.btnAdvanced.Image = ((System.Drawing.Image)(resources.GetObject("btnAdvanced.Image")));
            this.btnAdvanced.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Size = new System.Drawing.Size(105, 36);
            this.btnAdvanced.Text = "Advanced";
            // 
            // btnDumpStatistics
            // 
            this.btnDumpStatistics.Name = "btnDumpStatistics";
            this.btnDumpStatistics.Size = new System.Drawing.Size(209, 22);
            this.btnDumpStatistics.Text = "Dump statistics";
            this.btnDumpStatistics.Click += new System.EventHandler(this.btnDumpStatistics_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(206, 6);
            // 
            // btnToggleBurst
            // 
            this.btnToggleBurst.Name = "btnToggleBurst";
            this.btnToggleBurst.Size = new System.Drawing.Size(209, 22);
            this.btnToggleBurst.Text = "Toggle burst window";
            this.btnToggleBurst.Click += new System.EventHandler(this.btnToggleBurst_Click);
            // 
            // btnToggleSpectrum
            // 
            this.btnToggleSpectrum.Name = "btnToggleSpectrum";
            this.btnToggleSpectrum.Size = new System.Drawing.Size(209, 22);
            this.btnToggleSpectrum.Text = "Toggle spectrum window";
            this.btnToggleSpectrum.Click += new System.EventHandler(this.btnToggleSpectrum_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(206, 6);
            // 
            // btnDump
            // 
            this.btnDump.Name = "btnDump";
            this.btnDump.Size = new System.Drawing.Size(209, 22);
            this.btnDump.Text = "File dumping";
            this.btnDump.Click += new System.EventHandler(this.btnDump_Click);
            // 
            // btnClear
            // 
            this.btnClear.Image = ((System.Drawing.Image)(resources.GetObject("btnClear.Image")));
            this.btnClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(70, 36);
            this.btnClear.Text = "Clear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // btnQuit
            // 
            this.btnQuit.Image = ((System.Drawing.Image)(resources.GetObject("btnQuit.Image")));
            this.btnQuit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(66, 36);
            this.btnQuit.Text = "Quit";
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // asdToolStripMenuItem
            // 
            this.asdToolStripMenuItem.Name = "asdToolStripMenuItem";
            this.asdToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.asdToolStripMenuItem.Text = "asd";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 39);
            // 
            // txtArfcn
            // 
            this.txtArfcn.Location = new System.Drawing.Point(61, 30);
            this.txtArfcn.LowerLimit = ((long)(0));
            this.txtArfcn.Name = "txtArfcn";
            this.txtArfcn.Size = new System.Drawing.Size(47, 20);
            this.txtArfcn.TabIndex = 20;
            this.txtArfcn.Text = "0";
            this.txtArfcn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtArfcn.UpperLimit = ((long)(2048));
            this.txtArfcn.Value = ((long)(0));
            this.txtArfcn.ValueChanged += new System.EventHandler(this.txtArfcn_ValueChanged);
            // 
            // slotUsageControl
            // 
            this.slotUsageControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.slotUsageControl.Location = new System.Drawing.Point(3, 16);
            this.slotUsageControl.Name = "slotUsageControl";
            this.slotUsageControl.Size = new System.Drawing.Size(609, 59);
            this.slotUsageControl.TabIndex = 20;
            // 
            // statusSearch
            // 
            this.statusSearch.BackColor = System.Drawing.Color.DarkGray;
            this.statusSearch.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.statusSearch.Location = new System.Drawing.Point(10, 21);
            this.statusSearch.Name = "statusSearch";
            this.statusSearch.Size = new System.Drawing.Size(10, 10);
            this.statusSearch.State = LibRXFFT.Components.GDI.eLampState.Grayed;
            this.statusSearch.TabIndex = 13;
            // 
            // statusLock
            // 
            this.statusLock.BackColor = System.Drawing.Color.DarkGray;
            this.statusLock.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.statusLock.Location = new System.Drawing.Point(10, 55);
            this.statusLock.Name = "statusLock";
            this.statusLock.Size = new System.Drawing.Size(10, 10);
            this.statusLock.State = LibRXFFT.Components.GDI.eLampState.Grayed;
            this.statusLock.TabIndex = 13;
            // 
            // statusTrain
            // 
            this.statusTrain.BackColor = System.Drawing.Color.DarkGray;
            this.statusTrain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.statusTrain.Location = new System.Drawing.Point(10, 38);
            this.statusTrain.Name = "statusTrain";
            this.statusTrain.Size = new System.Drawing.Size(10, 10);
            this.statusTrain.State = LibRXFFT.Components.GDI.eLampState.Grayed;
            this.statusTrain.TabIndex = 13;
            // 
            // GSMAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(762, 454);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GSMAnalyzer";
            this.Text = "GSM Analyzer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel1.PerformLayout();
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer5;
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
        private System.Windows.Forms.Button btnScan;
        private TextBoxMouseScroll txtArfcn;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label lblPower;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label lblIdlePower;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label lblSnr;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripMenuItem btnDumpStatistics;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem btnToggleBurst;
        private System.Windows.Forms.ToolStripMenuItem btnToggleSpectrum;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem btnDump;
        private System.Windows.Forms.ToolStripDropDownButton btnAdvanced;
        private System.Windows.Forms.ToolStripMenuItem asdToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton btnOptions;
        private System.Windows.Forms.ToolStripButton btnClear;
        private System.Windows.Forms.ToolStripButton btnL3Filter;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnQuit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private SlotUsageControl slotUsageControl;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.CheckBox chkSlotUsage;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}

