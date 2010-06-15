namespace RX_Radio
{
    partial class RXRadioMain
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
            this.btnOpen = new System.Windows.Forms.Button();
            this.FilterList = new LibRXFFT.Components.GDI.FilterList();
            this.frequencySelector1 = new LibRXFFT.Components.GDI.FrequencySelector();
            this.powerBar = new LibRXFFT.Components.GDI.PowerBar();
            this.SuspendLayout();
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(13, 13);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // FilterList
            // 
            this.FilterList.Location = new System.Drawing.Point(13, 75);
            this.FilterList.Margin = new System.Windows.Forms.Padding(0);
            this.FilterList.Name = "FilterList";
            this.FilterList.Size = new System.Drawing.Size(437, 77);
            this.FilterList.TabIndex = 1;
            // 
            // frequencySelector1
            // 
            this.frequencySelector1.FixedLengthDecades = 10;
            this.frequencySelector1.FixedLengthString = false;
            this.frequencySelector1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.frequencySelector1.Frequency = ((long)(0));
            this.frequencySelector1.Location = new System.Drawing.Point(13, 43);
            this.frequencySelector1.Name = "frequencySelector1";
            this.frequencySelector1.Size = new System.Drawing.Size(100, 20);
            this.frequencySelector1.TabIndex = 2;
            this.frequencySelector1.Text = "0 Hz";
            this.frequencySelector1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // powerBar
            // 
            this.powerBar.Amplitude = 0;
            this.powerBar.LinePosition = 0;
            this.powerBar.Location = new System.Drawing.Point(114, 13);
            this.powerBar.Name = "powerBar";
            this.powerBar.Size = new System.Drawing.Size(111, 22);
            this.powerBar.TabIndex = 3;
            this.powerBar.Text = "powerBar";
            // 
            // RXRadioMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 375);
            this.Controls.Add(this.powerBar);
            this.Controls.Add(this.frequencySelector1);
            this.Controls.Add(this.FilterList);
            this.Controls.Add(this.btnOpen);
            this.Name = "RXRadioMain";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOpen;
        private LibRXFFT.Components.GDI.FilterList FilterList;
        private LibRXFFT.Components.GDI.FrequencySelector frequencySelector1;
        private LibRXFFT.Components.GDI.PowerBar powerBar;
    }
}

