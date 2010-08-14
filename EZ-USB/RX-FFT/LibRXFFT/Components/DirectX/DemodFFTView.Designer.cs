namespace LibRXFFT.Components.DirectX
{
    partial class DemodFFTView
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.FFTInput = new LibRXFFT.Components.DirectX.DirectXFFTDisplay();
            this.FFTTranslated = new LibRXFFT.Components.DirectX.DirectXFFTDisplay();
            this.FFTFiltered = new LibRXFFT.Components.DirectX.DirectXFFTDisplay();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.FFTInput, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.FFTTranslated, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.FFTFiltered, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(635, 539);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // FFTInput
            // 
            this.FFTInput.CenterFrequency = 0;
            this.FFTInput.ColorBG = System.Drawing.Color.Black;
            this.FFTInput.ColorCursor = System.Drawing.Color.Red;
            this.FFTInput.ColorFG = System.Drawing.Color.Cyan;
            this.FFTInput.ColorFont = System.Drawing.Color.DarkCyan;
            this.FFTInput.ColorOverview = System.Drawing.Color.Red;
            this.FFTInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FFTInput.FFTSize = 256;
            this.FFTInput.Location = new System.Drawing.Point(0, 0);
            this.FFTInput.Margin = new System.Windows.Forms.Padding(0);
            this.FFTInput.Name = "FFTInput";
            this.FFTInput.SamplingRate = 100;
            this.FFTInput.Size = new System.Drawing.Size(635, 179);
            this.FFTInput.SpectParts = 1;
            this.FFTInput.TabIndex = 0;
            this.FFTInput.UpdateRate = 60.000003814697266;
            this.FFTInput.XZoomFactor = 1;
            this.FFTInput.XZoomFactorMax = 20;
            this.FFTInput.XZoomFactorMin = 1;
            this.FFTInput.YZoomFactor = 1;
            this.FFTInput.YZoomFactorMax = 50;
            this.FFTInput.YZoomFactorMin = 0.01;
            // 
            // FFTTranslated
            // 
            this.FFTTranslated.CenterFrequency = 0;
            this.FFTTranslated.ColorBG = System.Drawing.Color.Black;
            this.FFTTranslated.ColorCursor = System.Drawing.Color.Red;
            this.FFTTranslated.ColorFG = System.Drawing.Color.Cyan;
            this.FFTTranslated.ColorFont = System.Drawing.Color.DarkCyan;
            this.FFTTranslated.ColorOverview = System.Drawing.Color.Red;
            this.FFTTranslated.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FFTTranslated.FFTSize = 256;
            this.FFTTranslated.Location = new System.Drawing.Point(0, 179);
            this.FFTTranslated.Margin = new System.Windows.Forms.Padding(0);
            this.FFTTranslated.Name = "FFTTranslated";
            this.FFTTranslated.SamplingRate = 100;
            this.FFTTranslated.Size = new System.Drawing.Size(635, 179);
            this.FFTTranslated.SpectParts = 1;
            this.FFTTranslated.TabIndex = 1;
            this.FFTTranslated.UpdateRate = 60.000003814697266;
            this.FFTTranslated.XZoomFactor = 1;
            this.FFTTranslated.XZoomFactorMax = 20;
            this.FFTTranslated.XZoomFactorMin = 1;
            this.FFTTranslated.YZoomFactor = 1;
            this.FFTTranslated.YZoomFactorMax = 50;
            this.FFTTranslated.YZoomFactorMin = 0.01;
            // 
            // FFTFiltered
            // 
            this.FFTFiltered.CenterFrequency = 0;
            this.FFTFiltered.ColorBG = System.Drawing.Color.Black;
            this.FFTFiltered.ColorCursor = System.Drawing.Color.Red;
            this.FFTFiltered.ColorFG = System.Drawing.Color.Cyan;
            this.FFTFiltered.ColorFont = System.Drawing.Color.DarkCyan;
            this.FFTFiltered.ColorOverview = System.Drawing.Color.Red;
            this.FFTFiltered.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FFTFiltered.FFTSize = 256;
            this.FFTFiltered.Location = new System.Drawing.Point(0, 358);
            this.FFTFiltered.Margin = new System.Windows.Forms.Padding(0);
            this.FFTFiltered.Name = "FFTFiltered";
            this.FFTFiltered.SamplingRate = 100;
            this.FFTFiltered.Size = new System.Drawing.Size(635, 181);
            this.FFTFiltered.SpectParts = 1;
            this.FFTFiltered.TabIndex = 2;
            this.FFTFiltered.UpdateRate = 60.000003814697266;
            this.FFTFiltered.XZoomFactor = 1;
            this.FFTFiltered.XZoomFactorMax = 20;
            this.FFTFiltered.XZoomFactorMin = 1;
            this.FFTFiltered.YZoomFactor = 1;
            this.FFTFiltered.YZoomFactorMax = 50;
            this.FFTFiltered.YZoomFactorMin = 0.01;
            // 
            // DemodFFTView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 539);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DemodFFTView";
            this.Text = "DemodFFTView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public LibRXFFT.Components.DirectX.DirectXFFTDisplay FFTInput;
        public LibRXFFT.Components.DirectX.DirectXFFTDisplay FFTTranslated;
        public LibRXFFT.Components.DirectX.DirectXFFTDisplay FFTFiltered;


    }
}