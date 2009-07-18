

namespace LibRXFFT.Components.DirectX
{
    partial class DirectXWaterfallFFTDisplay
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.fftDisplay = new DirectXFFTDisplay();
            this.waterfallDisplay = new DirectXWaterfallDisplay();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.fftDisplay);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.waterfallDisplay);
            this.splitContainer.Size = new System.Drawing.Size(537, 377);
            this.splitContainer.SplitterDistance = 179;
            this.splitContainer.SplitterWidth = 2;
            this.splitContainer.TabIndex = 0;
            // 
            // fftDisplay
            // 
            this.fftDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fftDisplay.Location = new System.Drawing.Point(0, 0);
            this.fftDisplay.Name = "fftDisplay";
            this.fftDisplay.ShowFPS = true;
            this.fftDisplay.Size = new System.Drawing.Size(537, 179);
            this.fftDisplay.TabIndex = 0;
            this.fftDisplay.UseLines = true;
            // 
            // waterfallDisplay
            // 
            this.waterfallDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waterfallDisplay.Location = new System.Drawing.Point(0, 0);
            this.waterfallDisplay.Name = "waterfallDisplay";
            this.waterfallDisplay.ShowFPS = true;
            this.waterfallDisplay.Size = new System.Drawing.Size(537, 196);
            this.waterfallDisplay.TabIndex = 0;
            this.waterfallDisplay.UseLines = true;
            // 
            // WaterfallFFTDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "DirectXWaterfallFFTDisplay";
            this.Size = new System.Drawing.Size(537, 377);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private DirectXFFTDisplay fftDisplay;
        private DirectXWaterfallDisplay waterfallDisplay;
    }
}