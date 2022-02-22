using System;

namespace LibRXFFT.Components.DeviceControls
{
    partial class NetworkDeviceControl
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
            this.radioTCPServer = new System.Windows.Forms.RadioButton();
            this.radioTCPClient = new System.Windows.Forms.RadioButton();
            this.radioUDPListener = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.btnFormat = new System.Windows.Forms.Button();
            this.radioRtsaClient = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioRtsaClient);
            this.groupBox1.Controls.Add(this.radioTCPServer);
            this.groupBox1.Controls.Add(this.radioTCPClient);
            this.groupBox1.Controls.Add(this.radioUDPListener);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(113, 122);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection Type";
            // 
            // radioTCPServer
            // 
            this.radioTCPServer.AutoSize = true;
            this.radioTCPServer.Location = new System.Drawing.Point(7, 68);
            this.radioTCPServer.Name = "radioTCPServer";
            this.radioTCPServer.Size = new System.Drawing.Size(80, 17);
            this.radioTCPServer.TabIndex = 2;
            this.radioTCPServer.Text = "TCP Server";
            this.radioTCPServer.UseVisualStyleBackColor = true;
            this.radioTCPServer.CheckedChanged += new System.EventHandler(this.radioTCPServer_CheckedChanged);
            // 
            // radioTCPClient
            // 
            this.radioTCPClient.AutoSize = true;
            this.radioTCPClient.Location = new System.Drawing.Point(6, 44);
            this.radioTCPClient.Name = "radioTCPClient";
            this.radioTCPClient.Size = new System.Drawing.Size(75, 17);
            this.radioTCPClient.TabIndex = 1;
            this.radioTCPClient.Text = "TCP Client";
            this.radioTCPClient.UseVisualStyleBackColor = true;
            this.radioTCPClient.CheckedChanged += new System.EventHandler(this.radioTCPClient_CheckedChanged);
            // 
            // radioUDPListener
            // 
            this.radioUDPListener.AutoSize = true;
            this.radioUDPListener.Checked = true;
            this.radioUDPListener.Location = new System.Drawing.Point(7, 20);
            this.radioUDPListener.Name = "radioUDPListener";
            this.radioUDPListener.Size = new System.Drawing.Size(88, 17);
            this.radioUDPListener.TabIndex = 0;
            this.radioUDPListener.TabStop = true;
            this.radioUDPListener.Text = "UDP Listener";
            this.radioUDPListener.UseVisualStyleBackColor = true;
            this.radioUDPListener.CheckedChanged += new System.EventHandler(this.radioUDPListener_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtPort);
            this.groupBox2.Controls.Add(this.txtHost);
            this.groupBox2.Location = new System.Drawing.Point(133, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(173, 73);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "IP Address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Port:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Host:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(105, 43);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(62, 20);
            this.txtPort.TabIndex = 0;
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(54, 19);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(113, 20);
            this.txtHost.TabIndex = 0;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Location = new System.Drawing.Point(231, 94);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(75, 23);
            this.btnStartStop.TabIndex = 2;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // btnFormat
            // 
            this.btnFormat.Location = new System.Drawing.Point(133, 94);
            this.btnFormat.Name = "btnFormat";
            this.btnFormat.Size = new System.Drawing.Size(92, 23);
            this.btnFormat.TabIndex = 3;
            this.btnFormat.Text = "Data format";
            this.btnFormat.UseVisualStyleBackColor = true;
            this.btnFormat.Click += new System.EventHandler(this.btnFormat_Click);
            // 
            // radioRtsaClient
            // 
            this.radioRtsaClient.AutoSize = true;
            this.radioRtsaClient.Location = new System.Drawing.Point(7, 91);
            this.radioRtsaClient.Name = "radioRtsaClient";
            this.radioRtsaClient.Size = new System.Drawing.Size(83, 17);
            this.radioRtsaClient.TabIndex = 3;
            this.radioRtsaClient.Text = "RTSA Client";
            this.radioRtsaClient.UseVisualStyleBackColor = true;
            this.radioRtsaClient.CheckedChanged += new System.EventHandler(this.radioRtsaClient_CheckedChanged);
            // 
            // NetworkDeviceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(318, 161);
            this.Controls.Add(this.btnFormat);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "NetworkDeviceControl";
            this.Text = "Network Source";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioTCPServer;
        private System.Windows.Forms.RadioButton radioTCPClient;
        private System.Windows.Forms.RadioButton radioUDPListener;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Button btnFormat;
        private System.Windows.Forms.RadioButton radioRtsaClient;
    }
}