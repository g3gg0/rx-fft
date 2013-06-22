namespace AORemote
{
    partial class AORemoteForm
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
            this.btnConnect = new System.Windows.Forms.Button();
            this.radioAnt1 = new System.Windows.Forms.RadioButton();
            this.radioAnt2 = new System.Windows.Forms.RadioButton();
            this.radioAnt3 = new System.Windows.Forms.RadioButton();
            this.radioAnt4 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.frequencySelector1 = new LibRXFFT.Components.GDI.FrequencySelector();
            this.chkRefresh = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(13, 13);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // radioAnt1
            // 
            this.radioAnt1.AutoSize = true;
            this.radioAnt1.Location = new System.Drawing.Point(6, 19);
            this.radioAnt1.Name = "radioAnt1";
            this.radioAnt1.Size = new System.Drawing.Size(53, 17);
            this.radioAnt1.TabIndex = 2;
            this.radioAnt1.TabStop = true;
            this.radioAnt1.Text = "ANT1";
            this.radioAnt1.UseVisualStyleBackColor = true;
            this.radioAnt1.CheckedChanged += new System.EventHandler(this.radioAnt1_CheckedChanged);
            // 
            // radioAnt2
            // 
            this.radioAnt2.AutoSize = true;
            this.radioAnt2.Location = new System.Drawing.Point(6, 39);
            this.radioAnt2.Name = "radioAnt2";
            this.radioAnt2.Size = new System.Drawing.Size(53, 17);
            this.radioAnt2.TabIndex = 2;
            this.radioAnt2.TabStop = true;
            this.radioAnt2.Text = "ANT2";
            this.radioAnt2.UseVisualStyleBackColor = true;
            this.radioAnt2.CheckedChanged += new System.EventHandler(this.radioAnt2_CheckedChanged);
            // 
            // radioAnt3
            // 
            this.radioAnt3.AutoSize = true;
            this.radioAnt3.Location = new System.Drawing.Point(6, 58);
            this.radioAnt3.Name = "radioAnt3";
            this.radioAnt3.Size = new System.Drawing.Size(53, 17);
            this.radioAnt3.TabIndex = 2;
            this.radioAnt3.TabStop = true;
            this.radioAnt3.Text = "ANT3";
            this.radioAnt3.UseVisualStyleBackColor = true;
            this.radioAnt3.CheckedChanged += new System.EventHandler(this.radioAnt3_CheckedChanged);
            // 
            // radioAnt4
            // 
            this.radioAnt4.AutoSize = true;
            this.radioAnt4.Location = new System.Drawing.Point(6, 77);
            this.radioAnt4.Name = "radioAnt4";
            this.radioAnt4.Size = new System.Drawing.Size(53, 17);
            this.radioAnt4.TabIndex = 2;
            this.radioAnt4.TabStop = true;
            this.radioAnt4.Text = "ANT4";
            this.radioAnt4.UseVisualStyleBackColor = true;
            this.radioAnt4.CheckedChanged += new System.EventHandler(this.radioAnt4_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioAnt1);
            this.groupBox1.Controls.Add(this.radioAnt4);
            this.groupBox1.Controls.Add(this.radioAnt2);
            this.groupBox1.Controls.Add(this.radioAnt3);
            this.groupBox1.Location = new System.Drawing.Point(258, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(68, 108);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ant In";
            // 
            // frequencySelector1
            // 
            this.frequencySelector1.BackColor = System.Drawing.Color.Black;
            this.frequencySelector1.FixedLengthDecades = 10;
            this.frequencySelector1.FixedLengthString = true;
            this.frequencySelector1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.frequencySelector1.ForeColor = System.Drawing.Color.Cyan;
            this.frequencySelector1.Frequency = ((long)(5000));
            this.frequencySelector1.Location = new System.Drawing.Point(44, 59);
            this.frequencySelector1.Name = "frequencySelector1";
            this.frequencySelector1.Size = new System.Drawing.Size(142, 20);
            this.frequencySelector1.TabIndex = 0;
            this.frequencySelector1.Text = "0.000.005.000 Hz";
            this.frequencySelector1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.frequencySelector1.TextChanged += new System.EventHandler(this.frequencySelector1_TextChanged);
            // 
            // chkRefresh
            // 
            this.chkRefresh.AutoSize = true;
            this.chkRefresh.Location = new System.Drawing.Point(105, 18);
            this.chkRefresh.Name = "chkRefresh";
            this.chkRefresh.Size = new System.Drawing.Size(63, 17);
            this.chkRefresh.TabIndex = 4;
            this.chkRefresh.Text = "Refresh";
            this.chkRefresh.UseVisualStyleBackColor = true;
            this.chkRefresh.CheckedChanged += new System.EventHandler(this.chkRefresh_CheckedChanged);
            // 
            // AORemoteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 268);
            this.Controls.Add(this.chkRefresh);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.frequencySelector1);
            this.Name = "AORemoteForm";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LibRXFFT.Components.GDI.FrequencySelector frequencySelector1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.RadioButton radioAnt1;
        private System.Windows.Forms.RadioButton radioAnt2;
        private System.Windows.Forms.RadioButton radioAnt3;
        private System.Windows.Forms.RadioButton radioAnt4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkRefresh;
    }
}

