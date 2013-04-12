namespace LibRXFFT.Components.GDI
{
    partial class LuaShell
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
            this.txtMessages = new System.Windows.Forms.TextBox();
            this.txtShell = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtMessages
            // 
            this.txtMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessages.Font = new System.Drawing.Font("Courier New", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessages.Location = new System.Drawing.Point(0, 0);
            this.txtMessages.Multiline = true;
            this.txtMessages.Name = "txtMessages";
            this.txtMessages.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMessages.Size = new System.Drawing.Size(183, 68);
            this.txtMessages.TabIndex = 0;
            // 
            // txtShell
            // 
            this.txtShell.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtShell.Font = new System.Drawing.Font("Courier New", 7F);
            this.txtShell.Location = new System.Drawing.Point(0, 0);
            this.txtShell.Margin = new System.Windows.Forms.Padding(0);
            this.txtShell.Name = "txtShell";
            this.txtShell.Size = new System.Drawing.Size(183, 18);
            this.txtShell.TabIndex = 1;
            this.txtShell.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtShell_KeyPress);
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
            this.splitContainer1.Panel1.Controls.Add(this.txtMessages);
            this.splitContainer1.Panel1MinSize = 0;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtShell);
            this.splitContainer1.Size = new System.Drawing.Size(183, 95);
            this.splitContainer1.SplitterDistance = 68;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 2;
            // 
            // LuaShell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(183, 95);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "LuaShell";
            this.Text = "LuaShell";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtMessages;
        private System.Windows.Forms.TextBox txtShell;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}