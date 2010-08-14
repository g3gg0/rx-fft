namespace RX_FFT.Dialogs
{
    partial class MarkerListDialog
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
            this.lstMarkers = new System.Windows.Forms.ListView();
            this.colFrequency = new System.Windows.Forms.ColumnHeader();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.MarkersMenu = new System.Windows.Forms.MenuStrip();
            this.selectedEntryMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.newEntryMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.editEntryMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteEntryMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.wholeListMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.clearListMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.loadListMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.saveListMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.MarkersMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstMarkers
            // 
            this.lstMarkers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFrequency,
            this.colName});
            this.lstMarkers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMarkers.Location = new System.Drawing.Point(0, 0);
            this.lstMarkers.Name = "lstMarkers";
            this.lstMarkers.Size = new System.Drawing.Size(520, 370);
            this.lstMarkers.TabIndex = 0;
            this.lstMarkers.UseCompatibleStateImageBehavior = false;
            this.lstMarkers.View = System.Windows.Forms.View.Details;
            this.lstMarkers.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstMarkers_MouseDoubleClick);
            this.lstMarkers.MouseClick += new System.Windows.Forms.MouseEventHandler(lstMarkers_MouseClick);
            // 
            // colFrequency
            // 
            this.colFrequency.Text = "Frequency";
            this.colFrequency.Width = 100;
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 200;
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
            this.splitContainer1.Panel1.Controls.Add(this.lstMarkers);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.MarkersMenu);
            this.splitContainer1.Size = new System.Drawing.Size(520, 396);
            this.splitContainer1.SplitterDistance = 370;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 5;
            // 
            // MarkersMenu
            // 
            this.MarkersMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectedEntryMenu,
            this.wholeListMenu});
            this.MarkersMenu.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.MarkersMenu.Location = new System.Drawing.Point(0, 0);
            this.MarkersMenu.Name = "MarkersMenu";
            this.MarkersMenu.Size = new System.Drawing.Size(520, 24);
            this.MarkersMenu.TabIndex = 6;
            this.MarkersMenu.Text = "menuStrip1";
            // 
            // selectedEntryMenu
            // 
            this.selectedEntryMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newEntryMenu,
            this.editEntryMenu,
            this.deleteEntryMenu});
            this.selectedEntryMenu.Name = "selectedEntryMenu";
            this.selectedEntryMenu.Size = new System.Drawing.Size(46, 20);
            this.selectedEntryMenu.Text = "Entry";
            // 
            // newEntryMenu
            // 
            this.newEntryMenu.Name = "newEntryMenu";
            this.newEntryMenu.Size = new System.Drawing.Size(152, 22);
            this.newEntryMenu.Text = "New Marker...";
            this.newEntryMenu.Click += new System.EventHandler(this.newEntryMenu_Click);
            // 
            // editEntryMenu
            // 
            this.editEntryMenu.Name = "editEntryMenu";
            this.editEntryMenu.Size = new System.Drawing.Size(152, 22);
            this.editEntryMenu.Text = "Edit...";
            this.editEntryMenu.Click += new System.EventHandler(this.editEntryMenu_Click);
            // 
            // deleteEntryMenu
            // 
            this.deleteEntryMenu.Name = "deleteEntryMenu";
            this.deleteEntryMenu.Size = new System.Drawing.Size(152, 22);
            this.deleteEntryMenu.Text = "Delete";
            this.deleteEntryMenu.Click += new System.EventHandler(this.deleteEntryMenu_Click);
            // 
            // wholeListMenu
            // 
            this.wholeListMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearListMenu,
            this.loadListMenu,
            this.saveListMenu});
            this.wholeListMenu.Name = "wholeListMenu";
            this.wholeListMenu.Size = new System.Drawing.Size(74, 20);
            this.wholeListMenu.Text = "Whole List";
            // 
            // clearListMenu
            // 
            this.clearListMenu.Name = "clearListMenu";
            this.clearListMenu.Size = new System.Drawing.Size(109, 22);
            this.clearListMenu.Text = "Clear";
            this.clearListMenu.Click += new System.EventHandler(this.clearListMenu_Click);
            // 
            // loadListMenu
            // 
            this.loadListMenu.Name = "loadListMenu";
            this.loadListMenu.Size = new System.Drawing.Size(109, 22);
            this.loadListMenu.Text = "Load...";
            this.loadListMenu.Click += new System.EventHandler(this.loadListMenu_Click);
            // 
            // saveListMenu
            // 
            this.saveListMenu.Name = "saveListMenu";
            this.saveListMenu.Size = new System.Drawing.Size(109, 22);
            this.saveListMenu.Text = "Save...";
            this.saveListMenu.Click += new System.EventHandler(this.saveListMenu_Click);
            // 
            // MarkerListDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 396);
            this.Controls.Add(this.splitContainer1);
            this.MainMenuStrip = this.MarkersMenu;
            this.Name = "MarkerListDialog";
            this.Text = "MarkerListDialog";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.MarkersMenu.ResumeLayout(false);
            this.MarkersMenu.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.ListView lstMarkers;
        private System.Windows.Forms.ColumnHeader colFrequency;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.MenuStrip MarkersMenu;
        private System.Windows.Forms.ToolStripMenuItem selectedEntryMenu;
        private System.Windows.Forms.ToolStripMenuItem newEntryMenu;
        private System.Windows.Forms.ToolStripMenuItem editEntryMenu;
        private System.Windows.Forms.ToolStripMenuItem deleteEntryMenu;
        private System.Windows.Forms.ToolStripMenuItem wholeListMenu;
        private System.Windows.Forms.ToolStripMenuItem clearListMenu;
        private System.Windows.Forms.ToolStripMenuItem loadListMenu;
        private System.Windows.Forms.ToolStripMenuItem saveListMenu;
    }
}