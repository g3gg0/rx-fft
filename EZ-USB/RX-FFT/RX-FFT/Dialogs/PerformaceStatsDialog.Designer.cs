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
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "",
            "",
            "",
            ""}, -1);
            this.listCounters = new System.Windows.Forms.ListView();
            this.hdrName = new System.Windows.Forms.ColumnHeader();
            this.hdrTotal = new System.Windows.Forms.ColumnHeader();
            this.hdrPct = new System.Windows.Forms.ColumnHeader();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
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
            listViewItem2});
            this.listCounters.Location = new System.Drawing.Point(0, 0);
            this.listCounters.Name = "listCounters";
            this.listCounters.Size = new System.Drawing.Size(381, 186);
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
            // PerformaceStatsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 186);
            this.Controls.Add(this.listCounters);
            this.Name = "PerformaceStatsWindow";
            this.Text = "Performace Details";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listCounters;
        private System.Windows.Forms.ColumnHeader hdrName;
        private System.Windows.Forms.ColumnHeader hdrTotal;
        private System.Windows.Forms.ColumnHeader hdrPct;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}