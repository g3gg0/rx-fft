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
            this.lblInputData = new System.Windows.Forms.Label();
            this.btnL2ToL1 = new System.Windows.Forms.Button();
            this.textL1burst0 = new System.Windows.Forms.TextBox();
            this.textL1burst1 = new System.Windows.Forms.TextBox();
            this.textL1burst2 = new System.Windows.Forms.TextBox();
            this.textL1burst3 = new System.Windows.Forms.TextBox();
            this.lblOutput = new System.Windows.Forms.Label();
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioIncreaseFN = new System.Windows.Forms.RadioButton();
            this.radioSameFN = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioGadToBool = new System.Windows.Forms.RadioButton();
            this.radioBoolToGad = new System.Windows.Forms.RadioButton();
            this.textGad3 = new System.Windows.Forms.TextBox();
            this.textGad2 = new System.Windows.Forms.TextBox();
            this.textGad1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBoolGadConvert = new System.Windows.Forms.Button();
            this.textGad0 = new System.Windows.Forms.TextBox();
            this.textBool3 = new System.Windows.Forms.TextBox();
            this.textBool2 = new System.Windows.Forms.TextBox();
            this.textBool1 = new System.Windows.Forms.TextBox();
            this.textBool0 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // textL2Data
            // 
            this.textL2Data.Location = new System.Drawing.Point(62, 19);
            this.textL2Data.Name = "textL2Data";
            this.textL2Data.Size = new System.Drawing.Size(416, 20);
            this.textL2Data.TabIndex = 0;
            // 
            // lblInputData
            // 
            this.lblInputData.AutoSize = true;
            this.lblInputData.Location = new System.Drawing.Point(6, 22);
            this.lblInputData.Name = "lblInputData";
            this.lblInputData.Size = new System.Drawing.Size(43, 13);
            this.lblInputData.TabIndex = 1;
            this.lblInputData.Text = "L2 data";
            // 
            // btnL2ToL1
            // 
            this.btnL2ToL1.Location = new System.Drawing.Point(484, 17);
            this.btnL2ToL1.Name = "btnL2ToL1";
            this.btnL2ToL1.Size = new System.Drawing.Size(97, 23);
            this.btnL2ToL1.TabIndex = 2;
            this.btnL2ToL1.Text = "Convert to L1";
            this.btnL2ToL1.UseVisualStyleBackColor = true;
            this.btnL2ToL1.Click += new System.EventHandler(this.btnL2ToL1_Click);
            // 
            // textL1burst0
            // 
            this.textL1burst0.Location = new System.Drawing.Point(62, 46);
            this.textL1burst0.Name = "textL1burst0";
            this.textL1burst0.ReadOnly = true;
            this.textL1burst0.Size = new System.Drawing.Size(519, 20);
            this.textL1burst0.TabIndex = 3;
            // 
            // textL1burst1
            // 
            this.textL1burst1.Location = new System.Drawing.Point(62, 73);
            this.textL1burst1.Name = "textL1burst1";
            this.textL1burst1.ReadOnly = true;
            this.textL1burst1.Size = new System.Drawing.Size(519, 20);
            this.textL1burst1.TabIndex = 4;
            // 
            // textL1burst2
            // 
            this.textL1burst2.Location = new System.Drawing.Point(62, 100);
            this.textL1burst2.Name = "textL1burst2";
            this.textL1burst2.ReadOnly = true;
            this.textL1burst2.Size = new System.Drawing.Size(519, 20);
            this.textL1burst2.TabIndex = 5;
            // 
            // textL1burst3
            // 
            this.textL1burst3.Location = new System.Drawing.Point(62, 127);
            this.textL1burst3.Name = "textL1burst3";
            this.textL1burst3.ReadOnly = true;
            this.textL1burst3.Size = new System.Drawing.Size(519, 20);
            this.textL1burst3.TabIndex = 6;
            // 
            // lblOutput
            // 
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(6, 49);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(50, 13);
            this.lblOutput.TabIndex = 7;
            this.lblOutput.Text = "L1 bursts";
            // 
            // textKc
            // 
            this.textKc.Location = new System.Drawing.Point(85, 186);
            this.textKc.Name = "textKc";
            this.textKc.Size = new System.Drawing.Size(201, 20);
            this.textKc.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 159);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "A5/1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(59, 189);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Kc";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(293, 188);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "FN";
            // 
            // textFN
            // 
            this.textFN.Location = new System.Drawing.Point(321, 185);
            this.textFN.Name = "textFN";
            this.textFN.Size = new System.Drawing.Size(112, 20);
            this.textFN.TabIndex = 12;
            // 
            // btnEncryptToL1
            // 
            this.btnEncryptToL1.Location = new System.Drawing.Point(439, 182);
            this.btnEncryptToL1.Name = "btnEncryptToL1";
            this.btnEncryptToL1.Size = new System.Drawing.Size(139, 23);
            this.btnEncryptToL1.TabIndex = 13;
            this.btnEncryptToL1.Text = "Encrypt to L1";
            this.btnEncryptToL1.UseVisualStyleBackColor = true;
            this.btnEncryptToL1.Click += new System.EventHandler(this.btnEncryptToL1_Click);
            // 
            // textL1crypt0
            // 
            this.textL1crypt0.Location = new System.Drawing.Point(59, 213);
            this.textL1crypt0.Name = "textL1crypt0";
            this.textL1crypt0.ReadOnly = true;
            this.textL1crypt0.Size = new System.Drawing.Size(519, 20);
            this.textL1crypt0.TabIndex = 14;
            // 
            // textL1crypt1
            // 
            this.textL1crypt1.Location = new System.Drawing.Point(59, 240);
            this.textL1crypt1.Name = "textL1crypt1";
            this.textL1crypt1.ReadOnly = true;
            this.textL1crypt1.Size = new System.Drawing.Size(519, 20);
            this.textL1crypt1.TabIndex = 15;
            // 
            // textL1crypt2
            // 
            this.textL1crypt2.Location = new System.Drawing.Point(59, 267);
            this.textL1crypt2.Name = "textL1crypt2";
            this.textL1crypt2.ReadOnly = true;
            this.textL1crypt2.Size = new System.Drawing.Size(519, 20);
            this.textL1crypt2.TabIndex = 16;
            // 
            // textL1crypt3
            // 
            this.textL1crypt3.Location = new System.Drawing.Point(59, 294);
            this.textL1crypt3.Name = "textL1crypt3";
            this.textL1crypt3.ReadOnly = true;
            this.textL1crypt3.Size = new System.Drawing.Size(519, 20);
            this.textL1crypt3.TabIndex = 17;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 216);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(45, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "L1 crypt\r\n";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioIncreaseFN);
            this.groupBox1.Controls.Add(this.radioSameFN);
            this.groupBox1.Controls.Add(this.textL2Data);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.lblInputData);
            this.groupBox1.Controls.Add(this.textL1crypt3);
            this.groupBox1.Controls.Add(this.btnL2ToL1);
            this.groupBox1.Controls.Add(this.textL1crypt2);
            this.groupBox1.Controls.Add(this.textL1burst0);
            this.groupBox1.Controls.Add(this.textL1crypt1);
            this.groupBox1.Controls.Add(this.textL1burst1);
            this.groupBox1.Controls.Add(this.textL1crypt0);
            this.groupBox1.Controls.Add(this.textL1burst2);
            this.groupBox1.Controls.Add(this.btnEncryptToL1);
            this.groupBox1.Controls.Add(this.textL1burst3);
            this.groupBox1.Controls.Add(this.textFN);
            this.groupBox1.Controls.Add(this.lblOutput);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textKc);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(591, 328);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "L2 -> L1";
            // 
            // radioIncreaseFN
            // 
            this.radioIncreaseFN.AutoSize = true;
            this.radioIncreaseFN.Location = new System.Drawing.Point(136, 157);
            this.radioIncreaseFN.Name = "radioIncreaseFN";
            this.radioIncreaseFN.Size = new System.Drawing.Size(82, 17);
            this.radioIncreaseFN.TabIndex = 20;
            this.radioIncreaseFN.Text = "increase FN";
            this.radioIncreaseFN.UseVisualStyleBackColor = true;
            // 
            // radioSameFN
            // 
            this.radioSameFN.AutoSize = true;
            this.radioSameFN.Checked = true;
            this.radioSameFN.Location = new System.Drawing.Point(62, 157);
            this.radioSameFN.Name = "radioSameFN";
            this.radioSameFN.Size = new System.Drawing.Size(67, 17);
            this.radioSameFN.TabIndex = 19;
            this.radioSameFN.TabStop = true;
            this.radioSameFN.Text = "same FN";
            this.radioSameFN.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioGadToBool);
            this.groupBox2.Controls.Add(this.radioBoolToGad);
            this.groupBox2.Controls.Add(this.textGad3);
            this.groupBox2.Controls.Add(this.textGad2);
            this.groupBox2.Controls.Add(this.textGad1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btnBoolGadConvert);
            this.groupBox2.Controls.Add(this.textGad0);
            this.groupBox2.Controls.Add(this.textBool3);
            this.groupBox2.Controls.Add(this.textBool2);
            this.groupBox2.Controls.Add(this.textBool1);
            this.groupBox2.Controls.Add(this.textBool0);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(13, 348);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(591, 243);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "gad<->bool";
            // 
            // radioGadToBool
            // 
            this.radioGadToBool.AutoSize = true;
            this.radioGadToBool.Location = new System.Drawing.Point(506, 152);
            this.radioGadToBool.Name = "radioGadToBool";
            this.radioGadToBool.Size = new System.Drawing.Size(72, 17);
            this.radioGadToBool.TabIndex = 12;
            this.radioGadToBool.TabStop = true;
            this.radioGadToBool.Text = "gad->bool";
            this.radioGadToBool.UseVisualStyleBackColor = true;
            // 
            // radioBoolToGad
            // 
            this.radioBoolToGad.AutoSize = true;
            this.radioBoolToGad.Location = new System.Drawing.Point(506, 124);
            this.radioBoolToGad.Name = "radioBoolToGad";
            this.radioBoolToGad.Size = new System.Drawing.Size(72, 17);
            this.radioBoolToGad.TabIndex = 11;
            this.radioBoolToGad.TabStop = true;
            this.radioBoolToGad.Text = "bool->gad";
            this.radioBoolToGad.UseVisualStyleBackColor = true;
            // 
            // textGad3
            // 
            this.textGad3.Location = new System.Drawing.Point(62, 205);
            this.textGad3.Name = "textGad3";
            this.textGad3.Size = new System.Drawing.Size(438, 20);
            this.textGad3.TabIndex = 10;
            // 
            // textGad2
            // 
            this.textGad2.Location = new System.Drawing.Point(62, 179);
            this.textGad2.Name = "textGad2";
            this.textGad2.Size = new System.Drawing.Size(438, 20);
            this.textGad2.TabIndex = 9;
            // 
            // textGad1
            // 
            this.textGad1.Location = new System.Drawing.Point(62, 151);
            this.textGad1.Name = "textGad1";
            this.textGad1.Size = new System.Drawing.Size(438, 20);
            this.textGad1.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 129);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 26);
            this.label2.TabIndex = 7;
            this.label2.Text = ".gad\r\n(hex)";
            // 
            // btnBoolGadConvert
            // 
            this.btnBoolGadConvert.Location = new System.Drawing.Point(506, 179);
            this.btnBoolGadConvert.Name = "btnBoolGadConvert";
            this.btnBoolGadConvert.Size = new System.Drawing.Size(72, 23);
            this.btnBoolGadConvert.TabIndex = 6;
            this.btnBoolGadConvert.Text = "Convert";
            this.btnBoolGadConvert.UseVisualStyleBackColor = true;
            this.btnBoolGadConvert.Click += new System.EventHandler(this.btnBoolGadConvert_Click);
            // 
            // textGad0
            // 
            this.textGad0.Location = new System.Drawing.Point(62, 124);
            this.textGad0.Name = "textGad0";
            this.textGad0.Size = new System.Drawing.Size(438, 20);
            this.textGad0.TabIndex = 5;
            // 
            // textBool3
            // 
            this.textBool3.Location = new System.Drawing.Point(62, 97);
            this.textBool3.Name = "textBool3";
            this.textBool3.Size = new System.Drawing.Size(516, 20);
            this.textBool3.TabIndex = 4;
            // 
            // textBool2
            // 
            this.textBool2.Location = new System.Drawing.Point(62, 71);
            this.textBool2.Name = "textBool2";
            this.textBool2.Size = new System.Drawing.Size(516, 20);
            this.textBool2.TabIndex = 3;
            // 
            // textBool1
            // 
            this.textBool1.Location = new System.Drawing.Point(62, 44);
            this.textBool1.Name = "textBool1";
            this.textBool1.Size = new System.Drawing.Size(516, 20);
            this.textBool1.TabIndex = 2;
            // 
            // textBool0
            // 
            this.textBool0.Location = new System.Drawing.Point(62, 17);
            this.textBool0.Name = "textBool0";
            this.textBool0.Size = new System.Drawing.Size(516, 20);
            this.textBool0.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "bool\r\n(bin)";
            // 
            // FrameEncoderDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 626);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "FrameEncoderDialog";
            this.ShowIcon = false;
            this.Text = "Frame Encoder";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textL2Data;
        private System.Windows.Forms.Label lblInputData;
        private System.Windows.Forms.Button btnL2ToL1;
        private System.Windows.Forms.TextBox textL1burst0;
        private System.Windows.Forms.TextBox textL1burst1;
        private System.Windows.Forms.TextBox textL1burst2;
        private System.Windows.Forms.TextBox textL1burst3;
        private System.Windows.Forms.Label lblOutput;
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioIncreaseFN;
        private System.Windows.Forms.RadioButton radioSameFN;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textGad0;
        private System.Windows.Forms.TextBox textBool3;
        private System.Windows.Forms.TextBox textBool2;
        private System.Windows.Forms.TextBox textBool1;
        private System.Windows.Forms.TextBox textBool0;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBoolGadConvert;
        private System.Windows.Forms.TextBox textGad3;
        private System.Windows.Forms.TextBox textGad2;
        private System.Windows.Forms.TextBox textGad1;
        private System.Windows.Forms.RadioButton radioBoolToGad;
        private System.Windows.Forms.RadioButton radioGadToBool;
    }
}