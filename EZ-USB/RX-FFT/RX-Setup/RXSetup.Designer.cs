namespace RX_Setup
{
    partial class RXSetup
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
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.btnDetails = new System.Windows.Forms.Button();
            this.btnFirmwareRead = new System.Windows.Forms.Button();
            this.btnFirmwareProgram = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnStress = new System.Windows.Forms.Button();
            this.lblStats = new System.Windows.Forms.Label();
            this.btnAtmelDelay = new System.Windows.Forms.Button();
            this.btnI2cTest = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(13, 13);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(81, 23);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtStatus
            // 
            this.txtStatus.Enabled = false;
            this.txtStatus.Location = new System.Drawing.Point(129, 15);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(110, 20);
            this.txtStatus.TabIndex = 1;
            // 
            // btnDetails
            // 
            this.btnDetails.Location = new System.Drawing.Point(13, 42);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(110, 23);
            this.btnDetails.TabIndex = 2;
            this.btnDetails.Text = "Board details";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // btnFirmwareRead
            // 
            this.btnFirmwareRead.Location = new System.Drawing.Point(6, 19);
            this.btnFirmwareRead.Name = "btnFirmwareRead";
            this.btnFirmwareRead.Size = new System.Drawing.Size(75, 23);
            this.btnFirmwareRead.TabIndex = 3;
            this.btnFirmwareRead.Text = "Read";
            this.btnFirmwareRead.UseVisualStyleBackColor = true;
            this.btnFirmwareRead.Click += new System.EventHandler(this.btnFirmwareRead_Click);
            // 
            // btnFirmwareProgram
            // 
            this.btnFirmwareProgram.Location = new System.Drawing.Point(87, 19);
            this.btnFirmwareProgram.Name = "btnFirmwareProgram";
            this.btnFirmwareProgram.Size = new System.Drawing.Size(75, 23);
            this.btnFirmwareProgram.TabIndex = 3;
            this.btnFirmwareProgram.Text = "Program";
            this.btnFirmwareProgram.UseVisualStyleBackColor = true;
            this.btnFirmwareProgram.Click += new System.EventHandler(this.btnFirmwareProgram_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnFirmwareRead);
            this.groupBox1.Controls.Add(this.btnFirmwareProgram);
            this.groupBox1.Location = new System.Drawing.Point(13, 141);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(226, 57);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Firmware";
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point(13, 205);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(226, 95);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "I²C";
            // 
            // btnStress
            // 
            this.btnStress.Location = new System.Drawing.Point(129, 71);
            this.btnStress.Name = "btnStress";
            this.btnStress.Size = new System.Drawing.Size(110, 23);
            this.btnStress.TabIndex = 0;
            this.btnStress.Text = "Stress Test";
            this.btnStress.UseVisualStyleBackColor = true;
            this.btnStress.Click += new System.EventHandler(this.btnStress_Click);
            // 
            // lblStats
            // 
            this.lblStats.AutoSize = true;
            this.lblStats.Location = new System.Drawing.Point(134, 47);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(0, 13);
            this.lblStats.TabIndex = 6;
            // 
            // btnAtmelDelay
            // 
            this.btnAtmelDelay.Location = new System.Drawing.Point(13, 100);
            this.btnAtmelDelay.Name = "btnAtmelDelay";
            this.btnAtmelDelay.Size = new System.Drawing.Size(110, 23);
            this.btnAtmelDelay.TabIndex = 7;
            this.btnAtmelDelay.Text = "Atmel delay test";
            this.btnAtmelDelay.UseVisualStyleBackColor = true;
            this.btnAtmelDelay.Click += new System.EventHandler(this.btnAtmelDelay_Click);
            // 
            // btnI2cTest
            // 
            this.btnI2cTest.Location = new System.Drawing.Point(13, 71);
            this.btnI2cTest.Name = "btnI2cTest";
            this.btnI2cTest.Size = new System.Drawing.Size(110, 23);
            this.btnI2cTest.TabIndex = 8;
            this.btnI2cTest.Text = "I²C/Atmel Test";
            this.btnI2cTest.UseVisualStyleBackColor = true;
            this.btnI2cTest.Click += new System.EventHandler(this.btnI2cTest_Click);
            // 
            // RXSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(251, 316);
            this.Controls.Add(this.btnI2cTest);
            this.Controls.Add(this.btnAtmelDelay);
            this.Controls.Add(this.lblStats);
            this.Controls.Add(this.btnStress);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.btnConnect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "RXSetup";
            this.Text = "RXSetup";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Button btnDetails;
        private System.Windows.Forms.Button btnFirmwareRead;
        private System.Windows.Forms.Button btnFirmwareProgram;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnStress;
        private System.Windows.Forms.Label lblStats;
        private System.Windows.Forms.Button btnAtmelDelay;
        private System.Windows.Forms.Button btnI2cTest;
    }
}

