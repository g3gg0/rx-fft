namespace LibRXFFT.Components.GDI
{
    partial class CFileDecimationDialog
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
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDecimation = new System.Windows.Forms.TextBox();
            this.clockRate = new LibRXFFT.Components.GDI.FrequencySelector();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(191, 62);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(33, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "USRP Decimation rate:";
            // 
            // txtDecimation
            // 
            this.txtDecimation.Location = new System.Drawing.Point(162, 10);
            this.txtDecimation.Name = "txtDecimation";
            this.txtDecimation.Size = new System.Drawing.Size(62, 20);
            this.txtDecimation.TabIndex = 2;
            this.txtDecimation.TextChanged += new System.EventHandler(this.txtDecimation_TextChanged);
            // 
            // clockRate
            // 
            this.clockRate.FixedLengthDecades = 10;
            this.clockRate.FixedLengthString = false;
            this.clockRate.Frequency = ((long)(64000000));
            this.clockRate.Location = new System.Drawing.Point(162, 36);
            this.clockRate.Name = "clockRate";
            this.clockRate.Size = new System.Drawing.Size(62, 20);
            this.clockRate.TabIndex = 3;
            this.clockRate.Text = "64 MHz";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "USRP Oscillator Clock rate:";
            // 
            // CFileDecimationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(236, 92);
            this.Controls.Add(this.clockRate);
            this.Controls.Add(this.txtDecimation);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CFileDecimationDialog";
            this.Text = "Decimation Rate";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDecimation;
        private FrequencySelector clockRate;
        private System.Windows.Forms.Label label2;
    }
}