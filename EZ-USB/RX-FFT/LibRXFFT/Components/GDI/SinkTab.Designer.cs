using System.Windows.Forms;
using System.Drawing;
using System;
namespace LibRXFFT.Components.GDI
{
    partial class SinkTab
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
            this.closeLabel = new System.Windows.Forms.Label();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.IsSplitterFixed = true;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer2";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.closeLabel);
            this.splitContainer.Panel1MinSize = 10;
            this.splitContainer.Size = new System.Drawing.Size(150, 150);
            this.splitContainer.SplitterDistance = 13;
            this.splitContainer.TabIndex = 0;
            // 
            // label1
            // 
            this.closeLabel.AutoSize = true;
            this.closeLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.closeLabel.Location = new System.Drawing.Point(136, 0);
            this.closeLabel.Name = "label1";
            this.closeLabel.Size = new System.Drawing.Size(14, 13);
            this.closeLabel.TabIndex = 0;
            this.closeLabel.Text = "X";
            // 
            // SinkTab
            // 
            this.Controls.Add(this.splitContainer);
            this.Name = "SinkTab";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public Label closeLabel;
        public SplitContainer splitContainer;
    }
}
