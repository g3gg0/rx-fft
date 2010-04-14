using LibRXFFT.Components.DirectX;
using LibRXFFT.Components.GDI;

namespace GaussGenerator
{
    partial class GaussGenerator
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

            if (ShmemChannel != null)
                ShmemChannel.Unregister();

            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBT = new System.Windows.Forms.TextBox();
            this.txtSequence = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.waveformDisplay = new DirectXWaveformDisplay();
            this.label3 = new System.Windows.Forms.Label();
            this.txtOversampling = new System.Windows.Forms.TextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.txtOversampling);
            this.splitContainer1.Panel1.Controls.Add(this.txtBT);
            this.splitContainer1.Panel1.Controls.Add(this.txtSequence);
            this.splitContainer1.Panel1.Controls.Add(this.btnCreate);
            this.splitContainer1.Panel1.Controls.Add(this.btnOpen);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.waveformDisplay);
            this.splitContainer1.Size = new System.Drawing.Size(867, 487);
            this.splitContainer1.SplitterDistance = 40;
            this.splitContainer1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(361, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Bits:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(186, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "BT:";
            // 
            // txtBT
            // 
            this.txtBT.Location = new System.Drawing.Point(215, 6);
            this.txtBT.Name = "txtBT";
            this.txtBT.Size = new System.Drawing.Size(47, 20);
            this.txtBT.TabIndex = 3;
            this.txtBT.Text = "0,3";
            this.txtBT.TextChanged += new System.EventHandler(this.txtBT_TextChanged);
            // 
            // txtSequence
            // 
            this.txtSequence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtSequence.Location = new System.Drawing.Point(394, 6);
            this.txtSequence.Name = "txtSequence";
            this.txtSequence.Size = new System.Drawing.Size(445, 20);
            this.txtSequence.TabIndex = 2;
            this.txtSequence.Text = "1001101000101100111110011111011101000100000110000011001011101001";
            this.txtSequence.TextChanged += new System.EventHandler(this.txtSequence_TextChanged);
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(86, 4);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(94, 23);
            this.btnCreate.TabIndex = 1;
            this.btnCreate.Text = "Create Gauss";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(4, 4);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // waveformDisplay
            // 
            this.waveformDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waveformDisplay.Location = new System.Drawing.Point(0, 0);
            this.waveformDisplay.MaxSamples = 10000;
            this.waveformDisplay.Name = "waveformDisplay";
            this.waveformDisplay.Size = new System.Drawing.Size(867, 443);
            this.waveformDisplay.TabIndex = 0;
            this.waveformDisplay.YZoomFactor = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(268, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "OvS:";
            // 
            // txtOversampling
            // 
            this.txtOversampling.Location = new System.Drawing.Point(305, 6);
            this.txtOversampling.Name = "txtOversampling";
            this.txtOversampling.Size = new System.Drawing.Size(37, 20);
            this.txtOversampling.TabIndex = 3;
            this.txtOversampling.Text = "10";
            this.txtOversampling.TextChanged += new System.EventHandler(this.txtOversampling_TextChanged);
            // 
            // GaussGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(867, 487);
            this.Controls.Add(this.splitContainer1);
            this.Name = "GaussGenerator";
            this.Text = "GaussGenerator";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DirectXWaveformDisplay waveformDisplay;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.TextBox txtSequence;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBT;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtOversampling;
    }
}

