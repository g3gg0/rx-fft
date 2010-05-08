namespace USB_RX_Console
{
    partial class USBRXConsole
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
            this.frequencySelector1 = new LibRXFFT.Components.GDI.FrequencySelector();
            this.SuspendLayout();
            // 
            // frequencySelector1
            // 
            this.frequencySelector1.Location = new System.Drawing.Point(108, 194);
            this.frequencySelector1.Name = "frequencySelector1";
            this.frequencySelector1.Size = new System.Drawing.Size(156, 20);
            this.frequencySelector1.TabIndex = 0;
            // 
            // USBRXConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 447);
            this.Controls.Add(this.frequencySelector1);
            this.Name = "USBRXConsole";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LibRXFFT.Components.GDI.FrequencySelector frequencySelector1;

    }
}

