using LibRXFFT.Components.GDI;
namespace RX_FFT.Dialogs
{
    partial class FrequencyBandDetailsDialog
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtChannelWidth = new LibRXFFT.Components.GDI.FrequencySelector();
            this.label5 = new System.Windows.Forms.Label();
            this.txtChannelDist = new LibRXFFT.Components.GDI.FrequencySelector();
            this.label4 = new System.Windows.Forms.Label();
            this.txtBaseFreq = new LibRXFFT.Components.GDI.FrequencySelector();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtChannelStart = new LibRXFFT.Components.GDI.TextBoxMouseScroll();
            this.txtChannelEnd = new LibRXFFT.Components.GDI.TextBoxMouseScroll();
            this.txtLabel = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtChannelEnd);
            this.groupBox1.Controls.Add(this.txtChannelWidth);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtChannelDist);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtBaseFreq);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtChannelStart);
            this.groupBox1.Controls.Add(this.txtLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(312, 184);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Band Settings";
            // 
            // txtChannelWidth
            // 
            this.txtChannelWidth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChannelWidth.Frequency = ((long)(0));
            this.txtChannelWidth.Location = new System.Drawing.Point(112, 97);
            this.txtChannelWidth.Name = "txtChannelWidth";
            this.txtChannelWidth.Size = new System.Drawing.Size(185, 20);
            this.txtChannelWidth.TabIndex = 3;
            this.txtChannelWidth.Text = "0 Hz";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 98);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Channel width:";
            // 
            // txtChannelDist
            // 
            this.txtChannelDist.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChannelDist.Frequency = ((long)(0));
            this.txtChannelDist.Location = new System.Drawing.Point(112, 71);
            this.txtChannelDist.Name = "txtChannelDist";
            this.txtChannelDist.Size = new System.Drawing.Size(185, 20);
            this.txtChannelDist.TabIndex = 3;
            this.txtChannelDist.Text = "0 Hz";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Channel distance:";
            // 
            // txtBaseFreq
            // 
            this.txtBaseFreq.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBaseFreq.Frequency = ((long)(0));
            this.txtBaseFreq.Location = new System.Drawing.Point(112, 45);
            this.txtBaseFreq.Name = "txtBaseFreq";
            this.txtBaseFreq.Size = new System.Drawing.Size(185, 20);
            this.txtBaseFreq.TabIndex = 3;
            this.txtBaseFreq.Text = "0 Hz";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Base frequency:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 126);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "First channel:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Label to display:";
            // 
            // txtChannelStart
            // 
            this.txtChannelStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChannelStart.Location = new System.Drawing.Point(112, 123);
            this.txtChannelStart.LowerLimit = ((long)(1));
            this.txtChannelStart.Name = "txtChannelStart";
            this.txtChannelStart.Size = new System.Drawing.Size(185, 20);
            this.txtChannelStart.TabIndex = 0;
            this.txtChannelStart.Text = "1";
            this.txtChannelStart.UpperLimit = ((long)(10000));
            this.txtChannelStart.Value = ((long)(1));
            // 
            // txtLabel
            // 
            this.txtLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLabel.Location = new System.Drawing.Point(112, 19);
            this.txtLabel.Name = "txtLabel";
            this.txtLabel.Size = new System.Drawing.Size(185, 20);
            this.txtLabel.TabIndex = 0;
            // 
            // txtChannelEnd
            // 
            this.txtChannelEnd.Location = new System.Drawing.Point(0, 0);
            this.txtChannelEnd.LowerLimit = ((long)(0));
            this.txtChannelEnd.Name = "txtChannelEnd";
            this.txtChannelEnd.Size = new System.Drawing.Size(100, 20);
            this.txtChannelEnd.TabIndex = 0;
            this.txtChannelEnd.UpperLimit = ((long)(0));
            this.txtChannelEnd.Value = ((long)(0));
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(196, 202);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(61, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(263, 202);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(62, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 152);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Last channel:";
            // 
            // txtChannelEnds
            // 
            this.txtChannelEnd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChannelEnd.Location = new System.Drawing.Point(112, 149);
            this.txtChannelEnd.LowerLimit = ((long)(1));
            this.txtChannelEnd.Name = "txtChannelEnds";
            this.txtChannelEnd.Size = new System.Drawing.Size(185, 20);
            this.txtChannelEnd.TabIndex = 4;
            this.txtChannelEnd.Text = "1";
            this.txtChannelEnd.UpperLimit = ((long)(10000));
            this.txtChannelEnd.Value = ((long)(1));
            // 
            // FrequencyBandDetailsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(336, 237);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Name = "FrequencyBandDetailsDialog";
            this.Text = "FrequencyBandDetailsDialog";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private LibRXFFT.Components.GDI.FrequencySelector txtBaseFreq;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLabel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private LibRXFFT.Components.GDI.FrequencySelector txtChannelWidth;
        private System.Windows.Forms.Label label5;
        private LibRXFFT.Components.GDI.FrequencySelector txtChannelDist;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private TextBoxMouseScroll txtChannelStart;
        private TextBoxMouseScroll txtChannelEnd;
        private System.Windows.Forms.Label label3;
    }
}