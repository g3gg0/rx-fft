namespace GSM_Analyzer
{
    partial class FrameEncoderDialog
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
            this.textL2Data = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnL2ToL1 = new System.Windows.Forms.Button();
            this.textL1burst0 = new System.Windows.Forms.TextBox();
            this.textL1burst1 = new System.Windows.Forms.TextBox();
            this.textL1burst2 = new System.Windows.Forms.TextBox();
            this.textL1burst3 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textKc = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textFN = new System.Windows.Forms.TextBox();
            this.btnEncryptToL1 = new System.Windows.Forms.Button();
            this.textL1crypt0 = new System.Windows.Forms.TextBox();
            this.textL1crypt1 = new System.Windows.Forms.TextBox();
            this.textL1crypt2 = new System.Windows.Forms.TextBox();
            this.textL1crypt3 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textL2Data
            // 
            this.textL2Data.Location = new System.Drawing.Point(69, 10);
            this.textL2Data.Name = "textL2Data";
            this.textL2Data.Size = new System.Drawing.Size(416, 20);
            this.textL2Data.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "L2 data";
            // 
            // btnL2ToL1
            // 
            this.btnL2ToL1.Location = new System.Drawing.Point(491, 8);
            this.btnL2ToL1.Name = "btnL2ToL1";
            this.btnL2ToL1.Size = new System.Drawing.Size(97, 23);
            this.btnL2ToL1.TabIndex = 2;
            this.btnL2ToL1.Text = "Convert to L1";
            this.btnL2ToL1.UseVisualStyleBackColor = true;
            this.btnL2ToL1.Click += new System.EventHandler(this.btnL2ToL1_Click);
            // 
            // textL1burst0
            // 
            this.textL1burst0.Location = new System.Drawing.Point(69, 37);
            this.textL1burst0.Name = "textL1burst0";
            this.textL1burst0.ReadOnly = true;
            this.textL1burst0.Size = new System.Drawing.Size(519, 20);
            this.textL1burst0.TabIndex = 3;
            // 
            // textL1burst1
            // 
            this.textL1burst1.Location = new System.Drawing.Point(69, 64);
            this.textL1burst1.Name = "textL1burst1";
            this.textL1burst1.ReadOnly = true;
            this.textL1burst1.Size = new System.Drawing.Size(519, 20);
            this.textL1burst1.TabIndex = 4;
            // 
            // textL1burst2
            // 
            this.textL1burst2.Location = new System.Drawing.Point(69, 91);
            this.textL1burst2.Name = "textL1burst2";
            this.textL1burst2.ReadOnly = true;
            this.textL1burst2.Size = new System.Drawing.Size(519, 20);
            this.textL1burst2.TabIndex = 5;
            // 
            // textL1burst3
            // 
            this.textL1burst3.Location = new System.Drawing.Point(69, 118);
            this.textL1burst3.Name = "textL1burst3";
            this.textL1burst3.ReadOnly = true;
            this.textL1burst3.Size = new System.Drawing.Size(519, 20);
            this.textL1burst3.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "L1 bursts";
            // 
            // textKc
            // 
            this.textKc.Location = new System.Drawing.Point(95, 147);
            this.textKc.Name = "textKc";
            this.textKc.Size = new System.Drawing.Size(201, 20);
            this.textKc.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 150);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "A5/1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(69, 150);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Kc";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(303, 149);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "FN";
            // 
            // textFN
            // 
            this.textFN.Location = new System.Drawing.Point(331, 146);
            this.textFN.Name = "textFN";
            this.textFN.Size = new System.Drawing.Size(112, 20);
            this.textFN.TabIndex = 12;
            // 
            // btnEncryptToL1
            // 
            this.btnEncryptToL1.Location = new System.Drawing.Point(449, 143);
            this.btnEncryptToL1.Name = "btnEncryptToL1";
            this.btnEncryptToL1.Size = new System.Drawing.Size(139, 23);
            this.btnEncryptToL1.TabIndex = 13;
            this.btnEncryptToL1.Text = "Encrypt to L1";
            this.btnEncryptToL1.UseVisualStyleBackColor = true;
            this.btnEncryptToL1.Click += new System.EventHandler(this.btnEncryptToL1_Click);
            // 
            // textL1crypt0
            // 
            this.textL1crypt0.Location = new System.Drawing.Point(69, 174);
            this.textL1crypt0.Name = "textL1crypt0";
            this.textL1crypt0.ReadOnly = true;
            this.textL1crypt0.Size = new System.Drawing.Size(519, 20);
            this.textL1crypt0.TabIndex = 14;
            // 
            // textL1crypt1
            // 
            this.textL1crypt1.Location = new System.Drawing.Point(69, 201);
            this.textL1crypt1.Name = "textL1crypt1";
            this.textL1crypt1.ReadOnly = true;
            this.textL1crypt1.Size = new System.Drawing.Size(519, 20);
            this.textL1crypt1.TabIndex = 15;
            // 
            // textL1crypt2
            // 
            this.textL1crypt2.Location = new System.Drawing.Point(69, 228);
            this.textL1crypt2.Name = "textL1crypt2";
            this.textL1crypt2.ReadOnly = true;
            this.textL1crypt2.Size = new System.Drawing.Size(519, 20);
            this.textL1crypt2.TabIndex = 16;
            // 
            // textL1crypt3
            // 
            this.textL1crypt3.Location = new System.Drawing.Point(69, 255);
            this.textL1crypt3.Name = "textL1crypt3";
            this.textL1crypt3.ReadOnly = true;
            this.textL1crypt3.Size = new System.Drawing.Size(519, 20);
            this.textL1crypt3.TabIndex = 17;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 177);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 39);
            this.label6.TabIndex = 18;
            this.label6.Text = "L1 crypt\r\n(all in one\r\nframe)";
            // 
            // FrameEncoderDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(610, 301);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textL1crypt3);
            this.Controls.Add(this.textL1crypt2);
            this.Controls.Add(this.textL1crypt1);
            this.Controls.Add(this.textL1crypt0);
            this.Controls.Add(this.btnEncryptToL1);
            this.Controls.Add(this.textFN);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textKc);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textL1burst3);
            this.Controls.Add(this.textL1burst2);
            this.Controls.Add(this.textL1burst1);
            this.Controls.Add(this.textL1burst0);
            this.Controls.Add(this.btnL2ToL1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textL2Data);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "FrameEncoderDialog";
            this.ShowIcon = false;
            this.Text = "Frame Encoder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textL2Data;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnL2ToL1;
        private System.Windows.Forms.TextBox textL1burst0;
        private System.Windows.Forms.TextBox textL1burst1;
        private System.Windows.Forms.TextBox textL1burst2;
        private System.Windows.Forms.TextBox textL1burst3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textKc;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textFN;
        private System.Windows.Forms.Button btnEncryptToL1;
        private System.Windows.Forms.TextBox textL1crypt0;
        private System.Windows.Forms.TextBox textL1crypt1;
        private System.Windows.Forms.TextBox textL1crypt2;
        private System.Windows.Forms.TextBox textL1crypt3;
        private System.Windows.Forms.Label label6;
    }
}