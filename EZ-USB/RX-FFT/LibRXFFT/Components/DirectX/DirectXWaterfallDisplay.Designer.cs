namespace LibRXFFT.Components.DirectX
{
    partial class DirectXWaterfallDisplay
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
            SaveThreadRunning = false;
            if (SaveThread != null)
            {
                SaveBufferTrigger.Release(1);
                if (!SaveThread.Join(250))
                {
                    SaveThread.Abort();
                }
                SaveThread = null;
            }

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
            this.SuspendLayout();
            // 
            // WaterfallDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "DirectXWaterfallDisplay";
            this.Size = new System.Drawing.Size(538, 333);

            this.ResumeLayout(false);
        }

        #endregion
    }
}