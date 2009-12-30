namespace RX_FFT.Dialogs
{
    partial class PerformaceStatsDialog
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "",
            "",
            "",
            ""}, -1);
            this.listCounters = new System.Windows.Forms.ListView();
            this.hdrName = new System.Windows.Forms.ColumnHeader();
            this.hdrTotal = new System.Windows.Forms.ColumnHeader();
            this.hdrPct = new System.Windows.Forms.ColumnHeader();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnReset = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listCounters
            // 
            this.listCounters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hdrName,
            this.hdrTotal,
            this.hdrPct,
            this.columnHeader1});
            this.listCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listCounters.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listCounters.Location = new System.Drawing.Point(0, 0);
            this.listCounters.Name = "listCounters";
            this.listCounters.Size = new System.Drawing.Size(382, 301);
            this.listCounters.TabIndex = 0;
            this.listCounters.UseCompatibleStateImageBehavior = false;
            this.listCounters.View = System.Windows.Forms.View.Details;
            // 
            // hdrName
            // 
            this.hdrName.Text = "Name";
            this.hdrName.Width = 150;
            // 
            // hdrTotal
            // 
            this.hdrTotal.Text = "Time [s]";
            this.hdrTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.hdrTotal.Width = 100;
            // 
            // hdrPct
            // 
            this.hdrPct.Text = "Time [%]";
            this.hdrPct.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "CPU [%]";
            this.columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listCounters);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnReset);
            this.splitContainer1.Size = new System.Drawing.Size(382, 333);
            this.splitContainer1.SplitterDistance = 301;
            this.splitContainer1.TabIndex = 1;
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.Location = new System.Drawing.Point(304, 3);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 0;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // PerformaceStatsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 333);
            this.Controls.Add(this.splitContainer1);
            this.Name = "PerformaceStatsDialog";
            this.Text = "Performace Details";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listCounters;
        private System.Windows.Forms.ColumnHeader hdrName;
        private System.Windows.Forms.ColumnHeader hdrTotal;
        private System.Windows.Forms.ColumnHeader hdrPct;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnReset;
    }
}