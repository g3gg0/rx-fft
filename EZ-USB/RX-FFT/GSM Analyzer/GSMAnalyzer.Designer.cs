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

            if (ShmemChannel != null)
                ShmemChannel.Unregister();

            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOpen = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.btnStep = new System.Windows.Forms.Button();
            this.chkSingleStep = new System.Windows.Forms.CheckBox();
            this.btnBurst = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtEstimationFact = new System.Windows.Forms.TextBox();
            this.lblSucess = new System.Windows.Forms.Label();
            this.lblErrors = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblRate = new System.Windows.Forms.Label();
            this.statusLock = new LibRXFFT.Components.GraphicalElements.StatusLamp();
            this.statusTrain = new LibRXFFT.Components.GraphicalElements.StatusLamp();
            this.statusSearch = new LibRXFFT.Components.GraphicalElements.StatusLamp();
            this.lblTS = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(3, 3);
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
            this.splitContainer1.Size = new System.Drawing.Size(728, 400);
            this.splitContainer1.SplitterDistance = 55;
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
            this.splitContainer5.Panel1.Controls.Add(this.btnStep);
            this.splitContainer5.Panel1.Controls.Add(this.chkSingleStep);
            this.splitContainer5.Panel1.Controls.Add(this.btnBurst);
            this.splitContainer5.Panel1.Controls.Add(this.btnOpen);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.lblTS);
            this.splitContainer5.Panel2.Controls.Add(this.lblRate);
            this.splitContainer5.Panel2.Controls.Add(this.label4);
            this.splitContainer5.Panel2.Controls.Add(this.txtEstimationFact);
            this.splitContainer5.Panel2.Controls.Add(this.lblSucess);
            this.splitContainer5.Panel2.Controls.Add(this.lblErrors);
            this.splitContainer5.Panel2.Controls.Add(this.label3);
            this.splitContainer5.Panel2.Controls.Add(this.label2);
            this.splitContainer5.Panel2.Controls.Add(this.label1);
            this.splitContainer5.Panel2.Controls.Add(this.statusLock);
            this.splitContainer5.Panel2.Controls.Add(this.statusTrain);
            this.splitContainer5.Panel2.Controls.Add(this.statusSearch);
            this.splitContainer5.Size = new System.Drawing.Size(728, 55);
            this.splitContainer5.SplitterDistance = 171;
            this.splitContainer5.SplitterWidth = 1;
            this.splitContainer5.TabIndex = 8;
            // 
            // btnStep
            // 
            this.btnStep.Location = new System.Drawing.Point(84, 29);
            this.btnStep.Name = "btnStep";
            this.btnStep.Size = new System.Drawing.Size(75, 23);
            this.btnStep.TabIndex = 4;
            this.btnStep.Text = "Step";
            this.btnStep.UseVisualStyleBackColor = true;
            this.btnStep.Click += new System.EventHandler(this.btnStep_Click);
            // 
            // chkSingleStep
            // 
            this.chkSingleStep.AutoSize = true;
            this.chkSingleStep.Location = new System.Drawing.Point(3, 32);
            this.chkSingleStep.Name = "chkSingleStep";
            this.chkSingleStep.Size = new System.Drawing.Size(77, 17);
            this.chkSingleStep.TabIndex = 3;
            this.chkSingleStep.Text = "SingleStep";
            this.chkSingleStep.UseVisualStyleBackColor = true;
            this.chkSingleStep.CheckedChanged += new System.EventHandler(this.chkSingleStep_CheckedChanged);
            // 
            // btnBurst
            // 
            this.btnBurst.Location = new System.Drawing.Point(84, 3);
            this.btnBurst.Name = "btnBurst";
            this.btnBurst.Size = new System.Drawing.Size(75, 23);
            this.btnBurst.TabIndex = 2;
            this.btnBurst.Text = "Bursts";
            this.btnBurst.UseVisualStyleBackColor = true;
            this.btnBurst.Click += new System.EventHandler(this.btnBurst_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(262, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Estimation Factor";
            // 
            // txtEstimationFact
            // 
            this.txtEstimationFact.Location = new System.Drawing.Point(265, 21);
            this.txtEstimationFact.Name = "txtEstimationFact";
            this.txtEstimationFact.Size = new System.Drawing.Size(38, 20);
            this.txtEstimationFact.TabIndex = 8;
            this.txtEstimationFact.Text = "0,25";
            this.txtEstimationFact.TextChanged += new System.EventHandler(this.txtEstimationFact_TextChanged);
            // 
            // lblSucess
            // 
            this.lblSucess.AutoSize = true;
            this.lblSucess.Location = new System.Drawing.Point(109, 21);
            this.lblSucess.Name = "lblSucess";
            this.lblSucess.Size = new System.Drawing.Size(60, 13);
            this.lblSucess.TabIndex = 7;
            this.lblSucess.Text = "Success: 0";
            // 
            // lblErrors
            // 
            this.lblErrors.AutoSize = true;
            this.lblErrors.Location = new System.Drawing.Point(109, 8);
            this.lblErrors.Name = "lblErrors";
            this.lblErrors.Size = new System.Drawing.Size(46, 13);
            this.lblErrors.TabIndex = 6;
            this.lblErrors.Text = "Errors: 0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Locked";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Train SCH";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Search FCCH";
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(728, 341);
            this.txtLog.TabIndex = 1;
            // 
            // lblRate
            // 
            this.lblRate.AutoSize = true;
            this.lblRate.Location = new System.Drawing.Point(109, 34);
            this.lblRate.Name = "lblRate";
            this.lblRate.Size = new System.Drawing.Size(56, 13);
            this.lblRate.TabIndex = 10;
            this.lblRate.Text = "Rate:  0 %";
            // 
            // statusLock
            // 
            this.statusLock.BackColor = System.Drawing.Color.DarkGray;
            this.statusLock.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.statusLock.Location = new System.Drawing.Point(12, 36);
            this.statusLock.Name = "statusLock";
            this.statusLock.Size = new System.Drawing.Size(10, 10);
            this.statusLock.State = LibRXFFT.Components.GraphicalElements.eLampState.Grayed;
            this.statusLock.TabIndex = 2;
            // 
            // statusTrain
            // 
            this.statusTrain.BackColor = System.Drawing.Color.DarkGray;
            this.statusTrain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.statusTrain.Location = new System.Drawing.Point(12, 23);
            this.statusTrain.Name = "statusTrain";
            this.statusTrain.Size = new System.Drawing.Size(10, 10);
            this.statusTrain.State = LibRXFFT.Components.GraphicalElements.eLampState.Grayed;
            this.statusTrain.TabIndex = 1;
            // 
            // statusSearch
            // 
            this.statusSearch.BackColor = System.Drawing.Color.DarkGray;
            this.statusSearch.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.statusSearch.Location = new System.Drawing.Point(12, 10);
            this.statusSearch.Name = "statusSearch";
            this.statusSearch.Size = new System.Drawing.Size(10, 10);
            this.statusSearch.State = LibRXFFT.Components.GraphicalElements.eLampState.Grayed;
            this.statusSearch.TabIndex = 0;
            // 
            // lblTS
            // 
            this.lblTS.AutoSize = true;
            this.lblTS.Location = new System.Drawing.Point(186, 8);
            this.lblTS.Name = "lblTS";
            this.lblTS.Size = new System.Drawing.Size(33, 13);
            this.lblTS.TabIndex = 11;
            this.lblTS.Text = "TS: 0";
            // 
            // GSMAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(728, 400);
            this.Controls.Add(this.splitContainer1);
            this.Name = "GSMAnalyzer";
            this.Text = "GSMAnalyzer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel1.PerformLayout();
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.Panel2.PerformLayout();
            this.splitContainer5.ResumeLayout(false);
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
        private LibRXFFT.Components.GraphicalElements.StatusLamp statusLock;
        private LibRXFFT.Components.GraphicalElements.StatusLamp statusTrain;
        private LibRXFFT.Components.GraphicalElements.StatusLamp statusSearch;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtEstimationFact;
        private System.Windows.Forms.Label lblSucess;
        private System.Windows.Forms.Label lblErrors;
        private System.Windows.Forms.Button btnStep;
        private System.Windows.Forms.CheckBox chkSingleStep;
        private System.Windows.Forms.Label lblRate;
        private System.Windows.Forms.Label lblTS;
    }
}

