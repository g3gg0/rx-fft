using LibRXFFT.Components.GDI;

namespace GMSK_Demodulator
{
    partial class GMSKDemod
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


            if (ReadThread != null && ReadThread.IsAlive)
            {
                ThreadActive = false;
                ReadThread.Abort();
                ReadThread.Join();
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
            this.btnOpen = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.btnIQ = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.hScrollBar = new System.Windows.Forms.HScrollBar();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.waveI = new WaveformDisplay();
            this.waveformDisplay1 = new WaveformDisplay();
            this.waveQ = new WaveformDisplay();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.phaseDisplay = new PhaseDisplay();
            this.differenciateDisplay = new DifferenciateDisplay();
            this.btnPhase = new System.Windows.Forms.Button();
            this.btnDiff = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(3, 3);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
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
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(752, 661);
            this.splitContainer1.SplitterDistance = 55;
            this.splitContainer1.TabIndex = 2;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.IsSplitterFixed = true;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.btnIQ);
            this.splitContainer5.Panel1.Controls.Add(this.btnOpen);
            this.splitContainer5.Panel1.Controls.Add(this.btnPause);
            this.splitContainer5.Panel1.Controls.Add(this.btnDiff);
            this.splitContainer5.Panel1.Controls.Add(this.btnPhase);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.hScrollBar);
            this.splitContainer5.Size = new System.Drawing.Size(752, 55);
            this.splitContainer5.SplitterDistance = 26;
            this.splitContainer5.SplitterWidth = 1;
            this.splitContainer5.TabIndex = 8;
            // 
            // btnIQ
            // 
            this.btnIQ.Location = new System.Drawing.Point(195, 3);
            this.btnIQ.Name = "btnIQ";
            this.btnIQ.Size = new System.Drawing.Size(75, 23);
            this.btnIQ.TabIndex = 7;
            this.btnIQ.Text = "I/Q";
            this.btnIQ.UseVisualStyleBackColor = true;
            this.btnIQ.Click += new System.EventHandler(this.btnIQ_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(84, 3);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 6;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // hScrollBar
            // 
            this.hScrollBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hScrollBar.Location = new System.Drawing.Point(0, 0);
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.Size = new System.Drawing.Size(752, 28);
            this.hScrollBar.TabIndex = 7;
            this.hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar_Scroll);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer3.Size = new System.Drawing.Size(752, 602);
            this.splitContainer3.SplitterDistance = 301;
            this.splitContainer3.TabIndex = 2;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.waveI);
            this.splitContainer4.Panel1.Controls.Add(this.waveformDisplay1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.waveQ);
            this.splitContainer4.Size = new System.Drawing.Size(752, 301);
            this.splitContainer4.SplitterDistance = 154;
            this.splitContainer4.TabIndex = 0;
            // 
            // waveI
            // 
            this.waveI.DisplayName = "I Samples";
            this.waveI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waveI.Location = new System.Drawing.Point(0, 0);
            this.waveI.MaxSamples = 10000;
            this.waveI.Name = "waveI";
            this.waveI.ShowFPS = false;
            this.waveI.Size = new System.Drawing.Size(752, 154);
            this.waveI.StartSample = 0;
            this.waveI.TabIndex = 1;
            this.waveI.UseLines = true;
            this.waveI.ZoomFactor = 1F;
            // 
            // waveformDisplay1
            // 
            this.waveformDisplay1.DisplayName = null;
            this.waveformDisplay1.Location = new System.Drawing.Point(4, 4);
            this.waveformDisplay1.MaxSamples = 10000;
            this.waveformDisplay1.Name = "waveformDisplay1";
            this.waveformDisplay1.ShowFPS = false;
            this.waveformDisplay1.Size = new System.Drawing.Size(428, 289);
            this.waveformDisplay1.StartSample = 0;
            this.waveformDisplay1.TabIndex = 0;
            this.waveformDisplay1.UseLines = true;
            this.waveformDisplay1.ZoomFactor = 1F;
            // 
            // waveQ
            // 
            this.waveQ.DisplayName = "Q Samples";
            this.waveQ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waveQ.Location = new System.Drawing.Point(0, 0);
            this.waveQ.MaxSamples = 10000;
            this.waveQ.Name = "waveQ";
            this.waveQ.ShowFPS = false;
            this.waveQ.Size = new System.Drawing.Size(752, 143);
            this.waveQ.StartSample = 0;
            this.waveQ.TabIndex = 0;
            this.waveQ.UseLines = true;
            this.waveQ.ZoomFactor = 1F;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.phaseDisplay);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.differenciateDisplay);
            this.splitContainer2.Size = new System.Drawing.Size(752, 297);
            this.splitContainer2.SplitterDistance = 148;
            this.splitContainer2.TabIndex = 1;
            // 
            // phaseDisplay
            // 
            this.phaseDisplay.DisplayName = "Phase Angle";
            this.phaseDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.phaseDisplay.Location = new System.Drawing.Point(0, 0);
            this.phaseDisplay.MaxSamples = 10000;
            this.phaseDisplay.Name = "phaseDisplay";
            this.phaseDisplay.ShowFPS = false;
            this.phaseDisplay.Size = new System.Drawing.Size(752, 148);
            this.phaseDisplay.StartSample = 0;
            this.phaseDisplay.TabIndex = 0;
            this.phaseDisplay.UseLines = true;
            this.phaseDisplay.ZoomFactor = 1F;
            // 
            // differenciateDisplay
            // 
            this.differenciateDisplay.DisplayName = "Phase Angle Difference";
            this.differenciateDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.differenciateDisplay.Location = new System.Drawing.Point(0, 0);
            this.differenciateDisplay.MaxSamples = 10000;
            this.differenciateDisplay.Name = "differenciateDisplay";
            this.differenciateDisplay.ShowFPS = false;
            this.differenciateDisplay.Size = new System.Drawing.Size(752, 145);
            this.differenciateDisplay.StartSample = 0;
            this.differenciateDisplay.TabIndex = 0;
            this.differenciateDisplay.UseLines = true;
            this.differenciateDisplay.ZoomFactor = 1F;
            // 
            // btnPhase
            // 
            this.btnPhase.Location = new System.Drawing.Point(276, 3);
            this.btnPhase.Name = "btnPhase";
            this.btnPhase.Size = new System.Drawing.Size(75, 23);
            this.btnPhase.TabIndex = 4;
            this.btnPhase.Text = "Phase";
            this.btnPhase.UseVisualStyleBackColor = true;
            this.btnPhase.Click += new System.EventHandler(this.btnPhase_Click);
            // 
            // btnDiff
            // 
            this.btnDiff.Location = new System.Drawing.Point(357, 3);
            this.btnDiff.Name = "btnDiff";
            this.btnDiff.Size = new System.Drawing.Size(75, 23);
            this.btnDiff.TabIndex = 5;
            this.btnDiff.Text = "Differenciate";
            this.btnDiff.UseVisualStyleBackColor = true;
            this.btnDiff.Click += new System.EventHandler(this.btnDiff_Click);
            // 
            // GMSKDemod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 661);
            this.Controls.Add(this.splitContainer1);
            this.Name = "GMSKDemod";
            this.Text = "GMSKDemod";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PhaseDisplay phaseDisplay;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private DifferenciateDisplay differenciateDisplay;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private WaveformDisplay waveI;
        private WaveformDisplay waveformDisplay1;
        private WaveformDisplay waveQ;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.Button btnIQ;
        private System.Windows.Forms.HScrollBar hScrollBar;
        private System.Windows.Forms.Button btnDiff;
        private System.Windows.Forms.Button btnPhase;
    }
}

