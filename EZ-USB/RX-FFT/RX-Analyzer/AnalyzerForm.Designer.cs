namespace RX_Analyzer
{
    partial class AnalyzerForm
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

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sharedMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newSignalPlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newPhasePlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newEyePlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newEyePlotToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.newFFTPlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rXAnalyzerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 453);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(675, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(45, 17);
            this.toolStripStatusLabel1.Text = "Status: ";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(12, 17);
            this.lblStatus.Text = "-";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.viewsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(675, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(47, 20);
            this.toolStripMenuItem1.Text = "Input";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sharedMemoryToolStripMenuItem,
            this.fileToolStripMenuItem});
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "Open";
            // 
            // sharedMemoryToolStripMenuItem
            // 
            this.sharedMemoryToolStripMenuItem.Name = "sharedMemoryToolStripMenuItem";
            this.sharedMemoryToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.sharedMemoryToolStripMenuItem.Text = "Shared memory";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // viewsToolStripMenuItem
            // 
            this.viewsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newSignalPlotToolStripMenuItem,
            this.newPhasePlotToolStripMenuItem,
            this.newEyePlotToolStripMenuItem,
            this.newEyePlotToolStripMenuItem1,
            this.newFFTPlotToolStripMenuItem});
            this.viewsToolStripMenuItem.Name = "viewsToolStripMenuItem";
            this.viewsToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
            this.viewsToolStripMenuItem.Text = "Views";
            // 
            // newSignalPlotToolStripMenuItem
            // 
            this.newSignalPlotToolStripMenuItem.Name = "newSignalPlotToolStripMenuItem";
            this.newSignalPlotToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.newSignalPlotToolStripMenuItem.Text = "New signal plot";
            this.newSignalPlotToolStripMenuItem.Click += new System.EventHandler(this.newSignalPlotToolStripMenuItem_Click);
            // 
            // newPhasePlotToolStripMenuItem
            // 
            this.newPhasePlotToolStripMenuItem.Name = "newPhasePlotToolStripMenuItem";
            this.newPhasePlotToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.newPhasePlotToolStripMenuItem.Text = "New phase plot";
            // 
            // newEyePlotToolStripMenuItem
            // 
            this.newEyePlotToolStripMenuItem.Name = "newEyePlotToolStripMenuItem";
            this.newEyePlotToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.newEyePlotToolStripMenuItem.Text = "New phase shift plot";
            // 
            // newEyePlotToolStripMenuItem1
            // 
            this.newEyePlotToolStripMenuItem1.Name = "newEyePlotToolStripMenuItem1";
            this.newEyePlotToolStripMenuItem1.Size = new System.Drawing.Size(182, 22);
            this.newEyePlotToolStripMenuItem1.Text = "New eye plot";
            // 
            // newFFTPlotToolStripMenuItem
            // 
            this.newFFTPlotToolStripMenuItem.Name = "newFFTPlotToolStripMenuItem";
            this.newFFTPlotToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.newFFTPlotToolStripMenuItem.Text = "New FFT plot";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rXAnalyzerToolStripMenuItem});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // rXAnalyzerToolStripMenuItem
            // 
            this.rXAnalyzerToolStripMenuItem.Name = "rXAnalyzerToolStripMenuItem";
            this.rXAnalyzerToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.rXAnalyzerToolStripMenuItem.Text = "RX-Analyzer";
            // 
            // AnalyzerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 475);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "AnalyzerForm";
            this.Text = "RX-Analyzer";
            this.TransparencyKey = System.Drawing.Color.White;
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem viewsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sharedMemoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newSignalPlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newPhasePlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newEyePlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newEyePlotToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem newFFTPlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rXAnalyzerToolStripMenuItem;
    }
}

