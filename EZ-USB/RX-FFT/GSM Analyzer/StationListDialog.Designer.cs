namespace GSM_Analyzer
{
    partial class StationListDialog
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
            this.lstStations = new System.Windows.Forms.ListView();
            this.hdrArfcn = new System.Windows.Forms.ColumnHeader();
            this.hdrMccMnc = new System.Windows.Forms.ColumnHeader();
            this.hdrLac = new System.Windows.Forms.ColumnHeader();
            this.hdrCellIdent = new System.Windows.Forms.ColumnHeader();
            this.hdrBroadcast = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // lstStations
            // 
            this.lstStations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hdrArfcn,
            this.hdrMccMnc,
            this.hdrLac,
            this.hdrCellIdent,
            this.hdrBroadcast});
            this.lstStations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstStations.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstStations.Location = new System.Drawing.Point(0, 0);
            this.lstStations.Name = "lstStations";
            this.lstStations.Size = new System.Drawing.Size(391, 350);
            this.lstStations.TabIndex = 0;
            this.lstStations.UseCompatibleStateImageBehavior = false;
            this.lstStations.View = System.Windows.Forms.View.Details;
            this.lstStations.MouseDoubleClick +=new System.Windows.Forms.MouseEventHandler(lstStations_MouseDoubleClick);
            // 
            // hdrArfcn
            // 
            this.hdrArfcn.Text = "ARFCN";
            this.hdrArfcn.Width = 51;
            // 
            // hdrMccMnc
            // 
            this.hdrMccMnc.Text = "MCC/MNC";
            this.hdrMccMnc.Width = 63;
            // 
            // hdrLac
            // 
            this.hdrLac.Text = "LAC";
            // 
            // hdrCellIdent
            // 
            this.hdrCellIdent.Text = "CellIdent";
            this.hdrCellIdent.Width = 75;
            // 
            // hdrBroadcast
            // 
            this.hdrBroadcast.Text = "CBCH";
            this.hdrBroadcast.Width = 45;
            // 
            // StationListDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 350);
            this.Controls.Add(this.lstStations);
            this.Name = "StationListDialog";
            this.Text = "StationListDialog";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lstStations;
        private System.Windows.Forms.ColumnHeader hdrArfcn;
        private System.Windows.Forms.ColumnHeader hdrMccMnc;
        private System.Windows.Forms.ColumnHeader hdrLac;
        private System.Windows.Forms.ColumnHeader hdrCellIdent;
        private System.Windows.Forms.ColumnHeader hdrBroadcast;
    }
}