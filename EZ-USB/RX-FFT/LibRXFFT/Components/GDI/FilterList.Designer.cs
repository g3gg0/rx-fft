namespace LibRXFFT.Components.GDI
{
    partial class FilterList
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

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.ctrFilterFileButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.ctrAtmelFilterButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // ctrFilterFileButtons
            // 
            this.ctrFilterFileButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctrFilterFileButtons.Location = new System.Drawing.Point(0, 0);
            this.ctrFilterFileButtons.Margin = new System.Windows.Forms.Padding(0);
            this.ctrFilterFileButtons.Name = "ctrFilterFileButtons";
            this.ctrFilterFileButtons.Size = new System.Drawing.Size(466, 21);
            this.ctrFilterFileButtons.TabIndex = 0;
            // 
            // ctrAtmelFilterButtons
            // 
            this.ctrAtmelFilterButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctrAtmelFilterButtons.Location = new System.Drawing.Point(0, 0);
            this.ctrAtmelFilterButtons.Margin = new System.Windows.Forms.Padding(0);
            this.ctrAtmelFilterButtons.Name = "ctrAtmelFilterButtons";
            this.ctrAtmelFilterButtons.Size = new System.Drawing.Size(466, 21);
            this.ctrAtmelFilterButtons.TabIndex = 0;
            // 
            // FilterList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ctrFilterFileButtons);
            this.Controls.Add(this.ctrAtmelFilterButtons);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "FilterList";
            this.Size = new System.Drawing.Size(466, 21);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel ctrFilterFileButtons;
        private System.Windows.Forms.FlowLayoutPanel ctrAtmelFilterButtons;
    }
}
