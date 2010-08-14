namespace LibRXFFT.Components.GDI
{
    partial class SoundCardSinkControl
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
            this.lstDevice = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lstDevice
            // 
            this.lstDevice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDevice.FormattingEnabled = true;
            this.lstDevice.Location = new System.Drawing.Point(0, 0);
            this.lstDevice.Name = "lstDevice";
            this.lstDevice.Size = new System.Drawing.Size(173, 21);
            this.lstDevice.TabIndex = 0;
            this.lstDevice.SelectedIndexChanged += new System.EventHandler(this.lstDevice_SelectedIndexChanged);
            // 
            // SoundCardSinkControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstDevice);
            this.Name = "SoundCardSinkControl";
            this.Size = new System.Drawing.Size(173, 76);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox lstDevice;
    }
}
