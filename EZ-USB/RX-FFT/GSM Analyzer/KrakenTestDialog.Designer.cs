namespace GSM_Analyzer
{
    partial class KrakenTestDialog
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
            this.label2 = new System.Windows.Forms.Label();
            this.textBursts = new System.Windows.Forms.TextBox();
            this.textTestCount = new System.Windows.Forms.TextBox();
            this.btnRunTest = new System.Windows.Forms.Button();
            this.krakenWorker = new System.ComponentModel.BackgroundWorker();
            this.krakenTestProgress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Test count";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Bursts per test";
            // 
            // textBursts
            // 
            this.textBursts.Location = new System.Drawing.Point(93, 33);
            this.textBursts.Name = "textBursts";
            this.textBursts.Size = new System.Drawing.Size(100, 20);
            this.textBursts.TabIndex = 1;
            // 
            // textTestCount
            // 
            this.textTestCount.Location = new System.Drawing.Point(93, 10);
            this.textTestCount.Name = "textTestCount";
            this.textTestCount.Size = new System.Drawing.Size(100, 20);
            this.textTestCount.TabIndex = 0;
            // 
            // btnRunTest
            // 
            this.btnRunTest.Location = new System.Drawing.Point(117, 60);
            this.btnRunTest.Name = "btnRunTest";
            this.btnRunTest.Size = new System.Drawing.Size(75, 23);
            this.btnRunTest.TabIndex = 2;
            this.btnRunTest.Text = "Run tests";
            this.btnRunTest.UseVisualStyleBackColor = true;
            this.btnRunTest.Click += new System.EventHandler(this.btnRunTest_Click);
            // 
            // krakenWorker
            // 
            this.krakenWorker.WorkerReportsProgress = true;
            this.krakenWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.krakenWorker_DoWork);
            this.krakenWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.krakenWorker_RunWorkerCompleted);
            this.krakenWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.krakenWorker_ProgressChanged);
            // 
            // krakenTestProgress
            // 
            this.krakenTestProgress.Location = new System.Drawing.Point(16, 67);
            this.krakenTestProgress.Name = "krakenTestProgress";
            this.krakenTestProgress.Size = new System.Drawing.Size(95, 10);
            this.krakenTestProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.krakenTestProgress.TabIndex = 5;
            // 
            // KrakenTestDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(205, 102);
            this.Controls.Add(this.krakenTestProgress);
            this.Controls.Add(this.btnRunTest);
            this.Controls.Add(this.textTestCount);
            this.Controls.Add(this.textBursts);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KrakenTestDialog";
            this.ShowIcon = false;
            this.Text = "Kraken coverage test";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBursts;
        private System.Windows.Forms.TextBox textTestCount;
        private System.Windows.Forms.Button btnRunTest;
        private System.ComponentModel.BackgroundWorker krakenWorker;
        private System.Windows.Forms.ProgressBar krakenTestProgress;
    }
}