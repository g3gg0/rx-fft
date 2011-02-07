namespace LibRXFFT.Components.GDI
{
    partial class DataFormatDialog
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
            this.frequencySelector = new LibRXFFT.Components.GDI.FrequencySelector();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // frequencySelector
            // 
            this.frequencySelector.FixedLengthDecades = 10;
            this.frequencySelector.FixedLengthString = false;
            this.frequencySelector.Frequency = ((long)(48000));
            this.frequencySelector.Location = new System.Drawing.Point(106, 12);
            this.frequencySelector.Name = "frequencySelector";
            this.frequencySelector.Size = new System.Drawing.Size(152, 20);
            this.frequencySelector.TabIndex = 0;
            this.frequencySelector.Text = "48 kHz";
            this.frequencySelector.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.frequencySelector.EnterPresed += new System.EventHandler(this.frequencySelector_EnterPresed);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Sampling Rate:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Sample format:";
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Location = new System.Drawing.Point(106, 42);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(152, 21);
            this.cmbType.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(208, 69);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(50, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // DataFormatDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 99);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.frequencySelector);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DataFormatDialog";
            this.Text = "Please specify the data format...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private FrequencySelector frequencySelector;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Button btnOk;
    }
}