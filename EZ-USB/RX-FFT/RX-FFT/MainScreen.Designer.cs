using LibRXFFT.Components;

namespace RX_FFT
{
    partial class MainScreen
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
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.samplingRateLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.deviceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openBO35Menu = new System.Windows.Forms.ToolStripMenuItem();
            this.openBO35PlainMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openShMemMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openRandomDataMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.closeMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.demodulationMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.performanceStatisticsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.markersMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.quitMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.fftSizeMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.fftSize512Menu = new System.Windows.Forms.ToolStripMenuItem();
            this.fftSize1024Menu = new System.Windows.Forms.ToolStripMenuItem();
            this.fftSize2048Menu = new System.Windows.Forms.ToolStripMenuItem();
            this.fftSize4096Menu = new System.Windows.Forms.ToolStripMenuItem();
            this.fftSize8192Menu = new System.Windows.Forms.ToolStripMenuItem();
            this.fftSize16386Menu = new System.Windows.Forms.ToolStripMenuItem();
            this.fftSizeOtherMenu = new System.Windows.Forms.ToolStripTextBox();
            this.windowingFunctionMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.updateRateMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.updateRateText = new System.Windows.Forms.ToolStripTextBox();
            this.averageSamplesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.averageSamplesText = new System.Windows.Forms.ToolStripTextBox();
            this.verticalSmoothMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.verticalSmoothMenuText = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.waterfallRecordingMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.waterfallRecordingEnabledMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.waterfallRecordingSaveAsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.dynamicWaterfallMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.fitSpectrumMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.displayFilterMarginsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.gsmAnalyzerMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.digitalDemodulatorsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.deviceInformationMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.FFTDisplay = new LibRXFFT.Components.DirectX.DirectXWaterfallFFTDisplay();
            this.scanBandMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.mainMenu.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.statusLabel,
            this.toolStripStatusLabel2,
            this.samplingRateLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 382);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(802, 28);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusBar";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Padding = new System.Windows.Forms.Padding(40, 0, 0, 0);
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(82, 23);
            this.toolStripStatusLabel1.Text = "Status:";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = false;
            this.statusLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(150, 23);
            this.statusLabel.Text = "Idle";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Margin = new System.Windows.Forms.Padding(30, 3, 0, 2);
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(86, 23);
            this.toolStripStatusLabel2.Text = "Sampling Rate:";
            // 
            // samplingRateLabel
            // 
            this.samplingRateLabel.AutoSize = false;
            this.samplingRateLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.samplingRateLabel.Name = "samplingRateLabel";
            this.samplingRateLabel.Size = new System.Drawing.Size(120, 23);
            this.samplingRateLabel.Text = "(none)";
            this.samplingRateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deviceMenu,
            this.optionsMenu,
            this.advancedMenu,
            this.helpMenu});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(802, 24);
            this.mainMenu.TabIndex = 7;
            this.mainMenu.Text = "menuStrip1";
            // 
            // deviceMenu
            // 
            this.deviceMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMenu,
            this.pauseMenu,
            this.saveMenu,
            this.closeMenu,
            this.toolStripSeparator1,
            this.demodulationMenu,
            this.performanceStatisticsMenu,
            this.markersMenu,
            this.toolStripSeparator2,
            this.quitMenu,
            this.scanBandMenu});
            this.deviceMenu.Name = "deviceMenu";
            this.deviceMenu.Size = new System.Drawing.Size(54, 20);
            this.deviceMenu.Text = "Device";
            // 
            // openMenu
            // 
            this.openMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openBO35Menu,
            this.openBO35PlainMenu,
            this.openShMemMenu,
            this.openRandomDataMenu});
            this.openMenu.Name = "openMenu";
            this.openMenu.Size = new System.Drawing.Size(191, 22);
            this.openMenu.Text = "Open";
            // 
            // openBO35Menu
            // 
            this.openBO35Menu.Name = "openBO35Menu";
            this.openBO35Menu.Size = new System.Drawing.Size(214, 22);
            this.openBO35Menu.Text = "BO-35digi";
            this.openBO35Menu.Click += new System.EventHandler(this.openBO35Menu_Click);
            // 
            // openBO35PlainMenu
            // 
            this.openBO35PlainMenu.Name = "openBO35PlainMenu";
            this.openBO35PlainMenu.Size = new System.Drawing.Size(214, 22);
            this.openBO35PlainMenu.Text = "BO-35digi (w/o ext. Tuner)";
            this.openBO35PlainMenu.Click += new System.EventHandler(this.openBO35PlainMenu_Click);
            // 
            // openShMemMenu
            // 
            this.openShMemMenu.Name = "openShMemMenu";
            this.openShMemMenu.Size = new System.Drawing.Size(214, 22);
            this.openShMemMenu.Text = "Shared Memory";
            this.openShMemMenu.Click += new System.EventHandler(this.openShMemMenu_Click);
            // 
            // openRandomDataMenu
            // 
            this.openRandomDataMenu.Name = "openRandomDataMenu";
            this.openRandomDataMenu.Size = new System.Drawing.Size(214, 22);
            this.openRandomDataMenu.Text = "Random Data";
            this.openRandomDataMenu.Click += new System.EventHandler(this.openRandomDataMenu_Click);
            // 
            // pauseMenu
            // 
            this.pauseMenu.Name = "pauseMenu";
            this.pauseMenu.Size = new System.Drawing.Size(191, 22);
            this.pauseMenu.Text = "Pause";
            this.pauseMenu.Click += new System.EventHandler(this.pauseMenu_Click);
            // 
            // saveMenu
            // 
            this.saveMenu.Name = "saveMenu";
            this.saveMenu.Size = new System.Drawing.Size(191, 22);
            this.saveMenu.Text = "Save digital data...";
            this.saveMenu.Click += new System.EventHandler(this.saveMenu_Click);
            // 
            // closeMenu
            // 
            this.closeMenu.Name = "closeMenu";
            this.closeMenu.Size = new System.Drawing.Size(191, 22);
            this.closeMenu.Text = "Close";
            this.closeMenu.Click += new System.EventHandler(this.closeMenu_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(188, 6);
            // 
            // demodulationMenu
            // 
            this.demodulationMenu.Name = "demodulationMenu";
            this.demodulationMenu.Size = new System.Drawing.Size(191, 22);
            this.demodulationMenu.Text = "Demodulation";
            this.demodulationMenu.Click += new System.EventHandler(this.demodulationMenu_Click);
            // 
            // performanceStatisticsMenu
            // 
            this.performanceStatisticsMenu.Name = "performanceStatisticsMenu";
            this.performanceStatisticsMenu.Size = new System.Drawing.Size(191, 22);
            this.performanceStatisticsMenu.Text = "Performance Statistics";
            this.performanceStatisticsMenu.Click += new System.EventHandler(this.performanceStatisticsMenu_Click);
            // 
            // markersMenu
            // 
            this.markersMenu.Name = "markersMenu";
            this.markersMenu.Size = new System.Drawing.Size(191, 22);
            this.markersMenu.Text = "Markers";
            this.markersMenu.Click += new System.EventHandler(this.markersToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(188, 6);
            // 
            // quitMenu
            // 
            this.quitMenu.Image = global::RX_FFT.Icons.imgExit;
            this.quitMenu.Name = "quitMenu";
            this.quitMenu.Size = new System.Drawing.Size(191, 22);
            this.quitMenu.Text = "Exit";
            this.quitMenu.Click += new System.EventHandler(this.quitMenu_Click);
            // 
            // optionsMenu
            // 
            this.optionsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fftSizeMenu,
            this.windowingFunctionMenu,
            this.toolStripSeparator3,
            this.updateRateMenu,
            this.averageSamplesToolStripMenuItem,
            this.verticalSmoothMenu,
            this.toolStripSeparator4,
            this.waterfallRecordingMenu,
            this.dynamicWaterfallMenu,
            this.fitSpectrumMenu,
            this.displayFilterMarginsMenu});
            this.optionsMenu.Name = "optionsMenu";
            this.optionsMenu.Size = new System.Drawing.Size(61, 20);
            this.optionsMenu.Text = "Options";
            // 
            // fftSizeMenu
            // 
            this.fftSizeMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fftSize512Menu,
            this.fftSize1024Menu,
            this.fftSize2048Menu,
            this.fftSize4096Menu,
            this.fftSize8192Menu,
            this.fftSize16386Menu,
            this.fftSizeOtherMenu});
            this.fftSizeMenu.Name = "fftSizeMenu";
            this.fftSizeMenu.Size = new System.Drawing.Size(214, 22);
            this.fftSizeMenu.Text = "FFT Size";
            // 
            // fftSize512Menu
            // 
            this.fftSize512Menu.Name = "fftSize512Menu";
            this.fftSize512Menu.Size = new System.Drawing.Size(160, 22);
            this.fftSize512Menu.Text = "512";
            this.fftSize512Menu.Click += new System.EventHandler(this.fftSize512Menu_Click);
            // 
            // fftSize1024Menu
            // 
            this.fftSize1024Menu.Name = "fftSize1024Menu";
            this.fftSize1024Menu.Size = new System.Drawing.Size(160, 22);
            this.fftSize1024Menu.Text = "1024";
            this.fftSize1024Menu.Click += new System.EventHandler(this.fftSize1024Menu_Click);
            // 
            // fftSize2048Menu
            // 
            this.fftSize2048Menu.Name = "fftSize2048Menu";
            this.fftSize2048Menu.Size = new System.Drawing.Size(160, 22);
            this.fftSize2048Menu.Text = "2048";
            this.fftSize2048Menu.Click += new System.EventHandler(this.fftSize2048Menu_Click);
            // 
            // fftSize4096Menu
            // 
            this.fftSize4096Menu.Name = "fftSize4096Menu";
            this.fftSize4096Menu.Size = new System.Drawing.Size(160, 22);
            this.fftSize4096Menu.Text = "4096";
            this.fftSize4096Menu.Click += new System.EventHandler(this.fftSize4096Menu_Click);
            // 
            // fftSize8192Menu
            // 
            this.fftSize8192Menu.Name = "fftSize8192Menu";
            this.fftSize8192Menu.Size = new System.Drawing.Size(160, 22);
            this.fftSize8192Menu.Text = "8192";
            this.fftSize8192Menu.Click += new System.EventHandler(this.fftSize8192Menu_Click);
            // 
            // fftSize16386Menu
            // 
            this.fftSize16386Menu.Name = "fftSize16386Menu";
            this.fftSize16386Menu.Size = new System.Drawing.Size(160, 22);
            this.fftSize16386Menu.Text = "16384";
            this.fftSize16386Menu.Click += new System.EventHandler(this.fftSize16384Menu_Click);
            // 
            // fftSizeOtherMenu
            // 
            this.fftSizeOtherMenu.Name = "fftSizeOtherMenu";
            this.fftSizeOtherMenu.Size = new System.Drawing.Size(100, 23);
            this.fftSizeOtherMenu.Text = "other...";
            this.fftSizeOtherMenu.Click += new System.EventHandler(this.fftSizeOtherMenu_Click);
            // 
            // windowingFunctionMenu
            // 
            this.windowingFunctionMenu.Name = "windowingFunctionMenu";
            this.windowingFunctionMenu.Size = new System.Drawing.Size(214, 22);
            this.windowingFunctionMenu.Text = "Windowing Function";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(211, 6);
            // 
            // updateRateMenu
            // 
            this.updateRateMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateRateText});
            this.updateRateMenu.Name = "updateRateMenu";
            this.updateRateMenu.Size = new System.Drawing.Size(214, 22);
            this.updateRateMenu.Text = "Update Rate";
            // 
            // updateRateText
            // 
            this.updateRateText.Name = "updateRateText";
            this.updateRateText.Size = new System.Drawing.Size(100, 23);
            this.updateRateText.Text = "15";
            // 
            // averageSamplesToolStripMenuItem
            // 
            this.averageSamplesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.averageSamplesText});
            this.averageSamplesToolStripMenuItem.Name = "averageSamplesToolStripMenuItem";
            this.averageSamplesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.averageSamplesToolStripMenuItem.Text = "Average Samples";
            // 
            // averageSamplesText
            // 
            this.averageSamplesText.Name = "averageSamplesText";
            this.averageSamplesText.Size = new System.Drawing.Size(100, 23);
            this.averageSamplesText.Text = "4";
            // 
            // verticalSmoothMenu
            // 
            this.verticalSmoothMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verticalSmoothMenuText});
            this.verticalSmoothMenu.Name = "verticalSmoothMenu";
            this.verticalSmoothMenu.Size = new System.Drawing.Size(214, 22);
            this.verticalSmoothMenu.Text = "Vertical Smooth";
            // 
            // verticalSmoothMenuText
            // 
            this.verticalSmoothMenuText.Name = "verticalSmoothMenuText";
            this.verticalSmoothMenuText.Size = new System.Drawing.Size(100, 23);
            this.verticalSmoothMenuText.Text = "1";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(211, 6);
            // 
            // waterfallRecordingMenu
            // 
            this.waterfallRecordingMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.waterfallRecordingEnabledMenu,
            this.waterfallRecordingSaveAsMenu});
            this.waterfallRecordingMenu.Name = "waterfallRecordingMenu";
            this.waterfallRecordingMenu.Size = new System.Drawing.Size(214, 22);
            this.waterfallRecordingMenu.Text = "Waterfall Recording";
            // 
            // waterfallRecordingEnabledMenu
            // 
            this.waterfallRecordingEnabledMenu.Name = "waterfallRecordingEnabledMenu";
            this.waterfallRecordingEnabledMenu.Size = new System.Drawing.Size(166, 22);
            this.waterfallRecordingEnabledMenu.Text = "Enable Recording";
            this.waterfallRecordingEnabledMenu.Click += new System.EventHandler(this.waterfallRecordingEnabledMenu_Click);
            // 
            // waterfallRecordingSaveAsMenu
            // 
            this.waterfallRecordingSaveAsMenu.Name = "waterfallRecordingSaveAsMenu";
            this.waterfallRecordingSaveAsMenu.Size = new System.Drawing.Size(166, 22);
            this.waterfallRecordingSaveAsMenu.Text = "Save as...";
            this.waterfallRecordingSaveAsMenu.Click += new System.EventHandler(this.waterfallRecordingSaveAsMenu_Click);
            // 
            // dynamicWaterfallMenu
            // 
            this.dynamicWaterfallMenu.Name = "dynamicWaterfallMenu";
            this.dynamicWaterfallMenu.Size = new System.Drawing.Size(214, 22);
            this.dynamicWaterfallMenu.Text = "Dynamic Waterfall";
            this.dynamicWaterfallMenu.Click += new System.EventHandler(this.dynamicWaterfallMenu_Click);
            // 
            // fitSpectrumMenu
            // 
            this.fitSpectrumMenu.Name = "fitSpectrumMenu";
            this.fitSpectrumMenu.Size = new System.Drawing.Size(214, 22);
            this.fitSpectrumMenu.Text = "Fit spectrum to filter width";
            this.fitSpectrumMenu.Click += new System.EventHandler(this.fitSpectrumMenu_Click);
            // 
            // displayFilterMarginsMenu
            // 
            this.displayFilterMarginsMenu.Name = "displayFilterMarginsMenu";
            this.displayFilterMarginsMenu.Size = new System.Drawing.Size(214, 22);
            this.displayFilterMarginsMenu.Text = "Display filter margins";
            this.displayFilterMarginsMenu.Click += new System.EventHandler(this.displayFilterMarginsMenu_Click);
            // 
            // advancedMenu
            // 
            this.advancedMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gsmAnalyzerMenu,
            this.digitalDemodulatorsMenu});
            this.advancedMenu.Name = "advancedMenu";
            this.advancedMenu.Size = new System.Drawing.Size(72, 20);
            this.advancedMenu.Text = "Advanced";
            // 
            // gsmAnalyzerMenu
            // 
            this.gsmAnalyzerMenu.Name = "gsmAnalyzerMenu";
            this.gsmAnalyzerMenu.Size = new System.Drawing.Size(186, 22);
            this.gsmAnalyzerMenu.Text = "GSM Analyzer";
            this.gsmAnalyzerMenu.Click += new System.EventHandler(this.gsmAnalyzerMenu_Click);
            // 
            // digitalDemodulatorsMenu
            // 
            this.digitalDemodulatorsMenu.Name = "digitalDemodulatorsMenu";
            this.digitalDemodulatorsMenu.Size = new System.Drawing.Size(186, 22);
            this.digitalDemodulatorsMenu.Text = "Digital Demodulators";
            this.digitalDemodulatorsMenu.Click += new System.EventHandler(this.digitalDemodulatorsMenu_Click);
            // 
            // helpMenu
            // 
            this.helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutMenu,
            this.deviceInformationMenu});
            this.helpMenu.Name = "helpMenu";
            this.helpMenu.Size = new System.Drawing.Size(44, 20);
            this.helpMenu.Text = "Help";
            // 
            // aboutMenu
            // 
            this.aboutMenu.Name = "aboutMenu";
            this.aboutMenu.Size = new System.Drawing.Size(175, 22);
            this.aboutMenu.Text = "About";
            this.aboutMenu.Click += new System.EventHandler(this.aboutMenu_Click);
            // 
            // deviceInformationMenu
            // 
            this.deviceInformationMenu.Name = "deviceInformationMenu";
            this.deviceInformationMenu.Size = new System.Drawing.Size(175, 22);
            this.deviceInformationMenu.Text = "Device Information";
            this.deviceInformationMenu.Click += new System.EventHandler(this.deviceInformationMenu_Click);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.FFTDisplay);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(802, 333);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 24);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(802, 358);
            this.toolStripContainer1.TabIndex = 8;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // FFTDisplay
            // 
            this.FFTDisplay.CenterFrequency = 0;
            this.FFTDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FFTDisplay.DynamicLimits = false;
            this.FFTDisplay.FFTSize = 2048;
            this.FFTDisplay.FitSpectrumEnabled = false;
            this.FFTDisplay.LimiterColor = System.Drawing.Color.Green;
            this.FFTDisplay.LimiterDisplayEnabled = false;
            this.FFTDisplay.LimiterLowerDescription = "";
            this.FFTDisplay.LimiterLowerLimit = 0;
            this.FFTDisplay.LimiterUpperDescription = "";
            this.FFTDisplay.LimiterUpperLimit = 0;
            this.FFTDisplay.Location = new System.Drawing.Point(0, 0);
            this.FFTDisplay.Name = "FFTDisplay";
            this.FFTDisplay.SamplesToAverage = ((long)(1));
            this.FFTDisplay.SamplingRate = 0;
            this.FFTDisplay.SavingEnabled = false;
            this.FFTDisplay.SavingName = "waterfall.png";
            this.FFTDisplay.Size = new System.Drawing.Size(802, 333);
            this.FFTDisplay.TabIndex = 0;
            this.FFTDisplay.UpdateRate = 10;
            this.FFTDisplay.VerticalSmooth = 1;
            this.FFTDisplay.WindowingFunction = LibRXFFT.Libraries.FFTW.FFTTransformer.eWindowingFunction.BlackmanHarris;
            // 
            // scanBandMenu
            // 
            this.scanBandMenu.Name = "scanBandMenu";
            this.scanBandMenu.Size = new System.Drawing.Size(191, 22);
            this.scanBandMenu.Text = "Scan band";
            this.scanBandMenu.Click += new System.EventHandler(this.scanBandMenu_Click);
            // 
            // MainScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(802, 410);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.mainMenu);
            this.Controls.Add(this.statusStrip1);
            this.Name = "MainScreen";
            this.Text = "RX-FFT";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private LibRXFFT.Components.GDI.FrequencySelector freqSelector;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private LibRXFFT.Components.DirectX.DirectXWaterfallFFTDisplay FFTDisplay;

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem deviceMenu;
        private System.Windows.Forms.ToolStripMenuItem openMenu;
        private System.Windows.Forms.ToolStripMenuItem openBO35Menu;
        private System.Windows.Forms.ToolStripMenuItem openShMemMenu;
        private System.Windows.Forms.ToolStripMenuItem closeMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem quitMenu;
        private System.Windows.Forms.ToolStripMenuItem optionsMenu;
        private System.Windows.Forms.ToolStripMenuItem helpMenu;
        private System.Windows.Forms.ToolStripMenuItem aboutMenu;
        private System.Windows.Forms.ToolStripMenuItem deviceInformationMenu;
        private System.Windows.Forms.ToolStripMenuItem fftSizeMenu;
        private System.Windows.Forms.ToolStripMenuItem fftSize512Menu;
        private System.Windows.Forms.ToolStripMenuItem fftSize1024Menu;
        private System.Windows.Forms.ToolStripMenuItem fftSize2048Menu;
        private System.Windows.Forms.ToolStripMenuItem fftSize4096Menu;
        private System.Windows.Forms.ToolStripMenuItem fftSize8192Menu;
        private System.Windows.Forms.ToolStripMenuItem fftSize16386Menu;
        private System.Windows.Forms.ToolStripTextBox fftSizeOtherMenu;
        private System.Windows.Forms.ToolStripMenuItem windowingFunctionMenu;
        private System.Windows.Forms.ToolStripMenuItem updateRateMenu;
        private System.Windows.Forms.ToolStripTextBox updateRateText;
        private System.Windows.Forms.ToolStripMenuItem averageSamplesToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox averageSamplesText;
        private System.Windows.Forms.ToolStripMenuItem pauseMenu;
        private System.Windows.Forms.ToolStripMenuItem demodulationMenu;
        private System.Windows.Forms.ToolStripMenuItem performanceStatisticsMenu;
        private System.Windows.Forms.ToolStripMenuItem verticalSmoothMenu;
        private System.Windows.Forms.ToolStripTextBox verticalSmoothMenuText;
        private System.Windows.Forms.ToolStripMenuItem waterfallRecordingMenu;
        private System.Windows.Forms.ToolStripMenuItem waterfallRecordingEnabledMenu;
        private System.Windows.Forms.ToolStripMenuItem waterfallRecordingSaveAsMenu;
        private System.Windows.Forms.ToolStripMenuItem advancedMenu;
        private System.Windows.Forms.ToolStripMenuItem gsmAnalyzerMenu;
        private System.Windows.Forms.ToolStripMenuItem openRandomDataMenu;
        private System.Windows.Forms.ToolStripMenuItem markersMenu;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStripMenuItem dynamicWaterfallMenu;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel samplingRateLabel;
        private System.Windows.Forms.ToolStripMenuItem saveMenu;
        private System.Windows.Forms.ToolStripMenuItem digitalDemodulatorsMenu;
        private System.Windows.Forms.ToolStripMenuItem openBO35PlainMenu;
        private System.Windows.Forms.ToolStripMenuItem fitSpectrumMenu;
        private System.Windows.Forms.ToolStripMenuItem displayFilterMarginsMenu;
        private System.Windows.Forms.ToolStripMenuItem scanBandMenu;
    }
}

