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
            if (StatusUpdateTimer != null)
            {
                StatusUpdateTimer.Dispose();
                StatusUpdateTimer = null;
            }
            if (DemodState != null)
            {
                DemodState.Dispose();
                DemodState = null;
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
            LibRXFFT.Libraries.SignalProcessing.AttenuationCorrection attenuationCorrection1 = new LibRXFFT.Libraries.SignalProcessing.AttenuationCorrection();
            LibRXFFT.Libraries.SignalProcessing.AttenuationCorrection attenuationCorrection2 = new LibRXFFT.Libraries.SignalProcessing.AttenuationCorrection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainScreen));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.samplingRateLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.fpsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.maxDbLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.deviceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openBO35Menu = new System.Windows.Forms.ToolStripMenuItem();
            this.openBO35PlainMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openShMemMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openNetworkDeviceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openRandomDataMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openUSRPMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.hiQSDRToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.closeMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.scanBandMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.scanChannelsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.scanMarkersMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.demodulationMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.performanceStatisticsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.markersMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.loadScriptMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ReloadScriptsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.unloadScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unloadAllMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.ScriptShellMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.quitMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
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
            this.trackPeaksItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verticalSmoothMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.verticalSmoothMenuText = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.waterfallRecordingMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.waterfallRecordingEnabledMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.waterfallRecordingSaveAsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.dynamicWaterfallMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.fitSpectrumMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.displayFilterMarginsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.agcMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.dCOffsetCorrectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.gsmAnalyzerMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.digitalDemodulatorsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.oscilloscopeMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.deviceInformationMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.FFTDisplay = new LibRXFFT.Components.DirectX.DirectXWaterfallFFTDisplay();
            this.menuButtons = new System.Windows.Forms.ToolStrip();
            this.btnOpenDevice = new System.Windows.Forms.ToolStripButton();
            this.btnCloseDevice = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.btnStartScope = new System.Windows.Forms.ToolStripButton();
            this.btnStartGsmAnalyzer = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.menuFft512 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFft1024 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFft2048 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFft4096 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFft8192 = new System.Windows.Forms.ToolStripMenuItem();
            this.spectranV6ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.mainMenu.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.menuButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.statusLabel,
            this.toolStripStatusLabel2,
            this.samplingRateLabel,
            this.toolStripStatusLabel3,
            this.fpsLabel,
            this.toolStripStatusLabel4,
            this.maxDbLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 384);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(924, 28);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusBar";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Margin = new System.Windows.Forms.Padding(20, 3, 0, 2);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(42, 23);
            this.toolStripStatusLabel1.Text = "Status:";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = false;
            this.statusLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(120, 23);
            this.statusLabel.Text = "Idle";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Margin = new System.Windows.Forms.Padding(20, 3, 0, 2);
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
            this.samplingRateLabel.Size = new System.Drawing.Size(100, 23);
            this.samplingRateLabel.Text = "(none)";
            this.samplingRateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Margin = new System.Windows.Forms.Padding(20, 3, 0, 2);
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(71, 23);
            this.toolStripStatusLabel3.Text = "Frames/Sec:";
            // 
            // fpsLabel
            // 
            this.fpsLabel.AutoSize = false;
            this.fpsLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.fpsLabel.Name = "fpsLabel";
            this.fpsLabel.Size = new System.Drawing.Size(80, 23);
            this.fpsLabel.Text = "0";
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Margin = new System.Windows.Forms.Padding(20, 3, 0, 2);
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(68, 23);
            this.toolStripStatusLabel4.Text = "Input Level:";
            // 
            // maxDbLabel
            // 
            this.maxDbLabel.AutoSize = false;
            this.maxDbLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.maxDbLabel.Name = "maxDbLabel";
            this.maxDbLabel.Size = new System.Drawing.Size(60, 23);
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
            this.mainMenu.Size = new System.Drawing.Size(924, 24);
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
            this.scanBandMenu,
            this.scanChannelsMenu,
            this.scanMarkersMenu,
            this.toolStripSeparator8,
            this.demodulationMenu,
            this.performanceStatisticsMenu,
            this.markersMenu,
            this.toolStripSeparator2,
            this.loadScriptMenu,
            this.ReloadScriptsMenu,
            this.unloadScriptToolStripMenuItem,
            this.ScriptShellMenu,
            this.toolStripSeparator5,
            this.quitMenu,
            this.toolStripSeparator6});
            this.deviceMenu.Name = "deviceMenu";
            this.deviceMenu.Size = new System.Drawing.Size(54, 20);
            this.deviceMenu.Text = "Device";
            // 
            // openMenu
            // 
            this.openMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openBO35Menu,
            this.openBO35PlainMenu,
            this.openFileMenu,
            this.openShMemMenu,
            this.openNetworkDeviceMenu,
            this.openRandomDataMenu,
            this.openUSRPMenu,
            this.hiQSDRToolStripMenuItem,
            this.spectranV6ToolStripMenuItem});
            this.openMenu.Name = "openMenu";
            this.openMenu.Size = new System.Drawing.Size(191, 22);
            this.openMenu.Text = "Open";
            // 
            // openBO35Menu
            // 
            this.openBO35Menu.Name = "openBO35Menu";
            this.openBO35Menu.Size = new System.Drawing.Size(237, 22);
            this.openBO35Menu.Text = "USB-RX";
            this.openBO35Menu.Click += new System.EventHandler(this.openBO35Menu_Click);
            // 
            // openBO35PlainMenu
            // 
            this.openBO35PlainMenu.Name = "openBO35PlainMenu";
            this.openBO35PlainMenu.Size = new System.Drawing.Size(237, 22);
            this.openBO35PlainMenu.Text = "USB-RX (autodetect ext. Tuner)";
            this.openBO35PlainMenu.Click += new System.EventHandler(this.openBO35PlainMenu_Click);
            // 
            // openFileMenu
            // 
            this.openFileMenu.Name = "openFileMenu";
            this.openFileMenu.Size = new System.Drawing.Size(237, 22);
            this.openFileMenu.Text = "File on Disk";
            this.openFileMenu.Click += new System.EventHandler(this.openFileMenu_Click);
            // 
            // openShMemMenu
            // 
            this.openShMemMenu.Name = "openShMemMenu";
            this.openShMemMenu.Size = new System.Drawing.Size(237, 22);
            this.openShMemMenu.Text = "Shared Memory";
            this.openShMemMenu.Click += new System.EventHandler(this.openShMemMenu_Click);
            // 
            // openNetworkDeviceMenu
            // 
            this.openNetworkDeviceMenu.Name = "openNetworkDeviceMenu";
            this.openNetworkDeviceMenu.Size = new System.Drawing.Size(237, 22);
            this.openNetworkDeviceMenu.Text = "Network Device";
            this.openNetworkDeviceMenu.Click += new System.EventHandler(this.openNetworkDeviceMenu_Click);
            // 
            // openRandomDataMenu
            // 
            this.openRandomDataMenu.Name = "openRandomDataMenu";
            this.openRandomDataMenu.Size = new System.Drawing.Size(237, 22);
            this.openRandomDataMenu.Text = "Random Data";
            this.openRandomDataMenu.Click += new System.EventHandler(this.openRandomDataMenu_Click);
            // 
            // openUSRPMenu
            // 
            this.openUSRPMenu.Name = "openUSRPMenu";
            this.openUSRPMenu.Size = new System.Drawing.Size(237, 22);
            this.openUSRPMenu.Text = "USRP Device";
            this.openUSRPMenu.Click += new System.EventHandler(this.openUSRPMenu_Click);
            // 
            // hiQSDRToolStripMenuItem
            // 
            this.hiQSDRToolStripMenuItem.Name = "hiQSDRToolStripMenuItem";
            this.hiQSDRToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.hiQSDRToolStripMenuItem.Text = "HiQ-SDR";
            this.hiQSDRToolStripMenuItem.Click += new System.EventHandler(this.openHiQSDRMenuItem_Click);
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
            // scanBandMenu
            // 
            this.scanBandMenu.Name = "scanBandMenu";
            this.scanBandMenu.Size = new System.Drawing.Size(191, 22);
            this.scanBandMenu.Text = "Scan Band...";
            this.scanBandMenu.Click += new System.EventHandler(this.scanBandMenu_Click);
            // 
            // scanChannelsMenu
            // 
            this.scanChannelsMenu.Name = "scanChannelsMenu";
            this.scanChannelsMenu.Size = new System.Drawing.Size(191, 22);
            this.scanChannelsMenu.Text = "Scan Channels...";
            this.scanChannelsMenu.Click += new System.EventHandler(this.scanChannelsMenu_Click);
            // 
            // scanMarkersMenu
            // 
            this.scanMarkersMenu.Name = "scanMarkersMenu";
            this.scanMarkersMenu.Size = new System.Drawing.Size(191, 22);
            this.scanMarkersMenu.Text = "Scan Markers";
            this.scanMarkersMenu.Click += new System.EventHandler(this.scanMarkersMenu_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(188, 6);
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
            // loadScriptMenu
            // 
            this.loadScriptMenu.Name = "loadScriptMenu";
            this.loadScriptMenu.Size = new System.Drawing.Size(191, 22);
            this.loadScriptMenu.Text = "Load Script...";
            this.loadScriptMenu.Click += new System.EventHandler(this.loadScriptMenu_Click);
            // 
            // ReloadScriptsMenu
            // 
            this.ReloadScriptsMenu.Name = "ReloadScriptsMenu";
            this.ReloadScriptsMenu.Size = new System.Drawing.Size(191, 22);
            this.ReloadScriptsMenu.Text = "Reload Scripts";
            this.ReloadScriptsMenu.Click += new System.EventHandler(this.ReloadScriptsMenu_Click);
            // 
            // unloadScriptToolStripMenuItem
            // 
            this.unloadScriptToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.unloadAllMenu,
            this.toolStripSeparator7});
            this.unloadScriptToolStripMenuItem.Name = "unloadScriptToolStripMenuItem";
            this.unloadScriptToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.unloadScriptToolStripMenuItem.Text = "Unload Script";
            // 
            // unloadAllMenu
            // 
            this.unloadAllMenu.Name = "unloadAllMenu";
            this.unloadAllMenu.Size = new System.Drawing.Size(127, 22);
            this.unloadAllMenu.Text = "Unload all";
            this.unloadAllMenu.Click += new System.EventHandler(this.unloadAllMenu_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(124, 6);
            // 
            // ScriptShellMenu
            // 
            this.ScriptShellMenu.Name = "ScriptShellMenu";
            this.ScriptShellMenu.Size = new System.Drawing.Size(191, 22);
            this.ScriptShellMenu.Text = "Script Shell...";
            this.ScriptShellMenu.Click += new System.EventHandler(this.ScriptShellMenu_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(188, 6);
            // 
            // quitMenu
            // 
            this.quitMenu.Image = global::RX_FFT.Icons.imgExit;
            this.quitMenu.Name = "quitMenu";
            this.quitMenu.Size = new System.Drawing.Size(191, 22);
            this.quitMenu.Text = "Exit";
            this.quitMenu.Click += new System.EventHandler(this.quitMenu_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(188, 6);
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
            this.displayFilterMarginsMenu,
            this.agcMenu,
            this.dCOffsetCorrectionToolStripMenuItem});
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
            this.fftSizeOtherMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
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
            this.updateRateText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.updateRateText.Name = "updateRateText";
            this.updateRateText.Size = new System.Drawing.Size(100, 23);
            this.updateRateText.Text = "15";
            // 
            // averageSamplesToolStripMenuItem
            // 
            this.averageSamplesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.averageSamplesText,
            this.trackPeaksItem});
            this.averageSamplesToolStripMenuItem.Name = "averageSamplesToolStripMenuItem";
            this.averageSamplesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.averageSamplesToolStripMenuItem.Text = "Average Samples";
            // 
            // averageSamplesText
            // 
            this.averageSamplesText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.averageSamplesText.Name = "averageSamplesText";
            this.averageSamplesText.Size = new System.Drawing.Size(100, 23);
            this.averageSamplesText.Text = "4";
            // 
            // trackPeaksItem
            // 
            this.trackPeaksItem.Name = "trackPeaksItem";
            this.trackPeaksItem.Size = new System.Drawing.Size(160, 22);
            this.trackPeaksItem.Text = "Track Peaks";
            this.trackPeaksItem.Click += new System.EventHandler(this.trackPeaksItem_Click);
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
            this.verticalSmoothMenuText.Font = new System.Drawing.Font("Segoe UI", 9F);
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
            // agcMenu
            // 
            this.agcMenu.Name = "agcMenu";
            this.agcMenu.Size = new System.Drawing.Size(214, 22);
            this.agcMenu.Text = "Automatic Gain Control";
            this.agcMenu.Click += new System.EventHandler(this.agcMenu_Click);
            // 
            // dCOffsetCorrectionToolStripMenuItem
            // 
            this.dCOffsetCorrectionToolStripMenuItem.Name = "dCOffsetCorrectionToolStripMenuItem";
            this.dCOffsetCorrectionToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.dCOffsetCorrectionToolStripMenuItem.Text = "DC Offset Correction";
            this.dCOffsetCorrectionToolStripMenuItem.Click += new System.EventHandler(this.dCOffsetCorrectionToolStripMenuItem_Click);
            // 
            // advancedMenu
            // 
            this.advancedMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gsmAnalyzerMenu,
            this.digitalDemodulatorsMenu,
            this.oscilloscopeMenu});
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
            // oscilloscopeMenu
            // 
            this.oscilloscopeMenu.Name = "oscilloscopeMenu";
            this.oscilloscopeMenu.Size = new System.Drawing.Size(186, 22);
            this.oscilloscopeMenu.Text = "Oscilloscope";
            this.oscilloscopeMenu.Click += new System.EventHandler(this.oscilloscopeMenu_Click);
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
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(924, 329);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 24);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(924, 360);
            this.toolStripContainer1.TabIndex = 8;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuButtons);
            // 
            // FFTDisplay
            // 
            this.FFTDisplay.CenterFrequency = 0D;
            this.FFTDisplay.ChannelBandDetails = null;
            this.FFTDisplay.ChannelMode = false;
            this.FFTDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FFTDisplay.DynamicLimits = false;
            this.FFTDisplay.FFTSize = 2048;
            this.FFTDisplay.FilterCorrection = attenuationCorrection1;
            this.FFTDisplay.FitSpectrumEnabled = false;
            this.FFTDisplay.Interleaving = 1;
            this.FFTDisplay.LimiterColor = System.Drawing.Color.Green;
            this.FFTDisplay.LimiterDisplayEnabled = false;
            this.FFTDisplay.LimiterLowerDescription = "";
            this.FFTDisplay.LimiterLowerLimit = 0D;
            this.FFTDisplay.LimiterUpperDescription = "";
            this.FFTDisplay.LimiterUpperLimit = 0D;
            this.FFTDisplay.Location = new System.Drawing.Point(0, 0);
            this.FFTDisplay.Name = "FFTDisplay";
            this.FFTDisplay.ReceiverCorrection = attenuationCorrection2;
            this.FFTDisplay.SamplesToAverage = ((long)(1));
            this.FFTDisplay.SampleValuesTrackPeaks = true;
            this.FFTDisplay.SamplingRate = 0D;
            this.FFTDisplay.SavingEnabled = false;
            this.FFTDisplay.SavingName = "waterfall.png";
            this.FFTDisplay.Size = new System.Drawing.Size(924, 329);
            this.FFTDisplay.SpectParts = 1;
            this.FFTDisplay.TabIndex = 0;
            this.FFTDisplay.UpdateRate = 10D;
            this.FFTDisplay.VerticalSmooth = 1D;
            this.FFTDisplay.WindowingFunction = LibRXFFT.Libraries.FFTW.FFTTransformer.eWindowingFunction.BlackmanHarris;
            // 
            // menuButtons
            // 
            this.menuButtons.AutoSize = false;
            this.menuButtons.Dock = System.Windows.Forms.DockStyle.None;
            this.menuButtons.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuButtons.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpenDevice,
            this.btnCloseDevice,
            this.toolStripSeparator9,
            this.btnStartScope,
            this.btnStartGsmAnalyzer,
            this.toolStripSeparator10,
            this.toolStripSplitButton1});
            this.menuButtons.Location = new System.Drawing.Point(0, 0);
            this.menuButtons.Name = "menuButtons";
            this.menuButtons.Size = new System.Drawing.Size(924, 31);
            this.menuButtons.Stretch = true;
            this.menuButtons.TabIndex = 0;
            // 
            // btnOpenDevice
            // 
            this.btnOpenDevice.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenDevice.Image")));
            this.btnOpenDevice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpenDevice.Name = "btnOpenDevice";
            this.btnOpenDevice.Size = new System.Drawing.Size(102, 28);
            this.btnOpenDevice.Text = "Open Device";
            this.btnOpenDevice.Click += new System.EventHandler(this.btnOpenDevice_Click);
            // 
            // btnCloseDevice
            // 
            this.btnCloseDevice.Enabled = false;
            this.btnCloseDevice.Image = ((System.Drawing.Image)(resources.GetObject("btnCloseDevice.Image")));
            this.btnCloseDevice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCloseDevice.Name = "btnCloseDevice";
            this.btnCloseDevice.Size = new System.Drawing.Size(102, 28);
            this.btnCloseDevice.Text = "Close Device";
            this.btnCloseDevice.Click += new System.EventHandler(this.btnCloseDevice_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 31);
            // 
            // btnStartScope
            // 
            this.btnStartScope.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStartScope.Image = ((System.Drawing.Image)(resources.GetObject("btnStartScope.Image")));
            this.btnStartScope.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStartScope.Name = "btnStartScope";
            this.btnStartScope.Size = new System.Drawing.Size(28, 28);
            this.btnStartScope.Text = "Oscilloscope";
            this.btnStartScope.Click += new System.EventHandler(this.btnStartScope_Click);
            // 
            // btnStartGsmAnalyzer
            // 
            this.btnStartGsmAnalyzer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStartGsmAnalyzer.Image = ((System.Drawing.Image)(resources.GetObject("btnStartGsmAnalyzer.Image")));
            this.btnStartGsmAnalyzer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStartGsmAnalyzer.Name = "btnStartGsmAnalyzer";
            this.btnStartGsmAnalyzer.Size = new System.Drawing.Size(28, 28);
            this.btnStartGsmAnalyzer.Text = "GSM Analyzer";
            this.btnStartGsmAnalyzer.Click += new System.EventHandler(this.btnStartGsmAnalyzer_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 31);
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFft512,
            this.menuFft1024,
            this.menuFft2048,
            this.menuFft4096,
            this.menuFft8192});
            this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(40, 28);
            this.toolStripSplitButton1.Text = "toolStripSplitButton1";
            // 
            // menuFft512
            // 
            this.menuFft512.Name = "menuFft512";
            this.menuFft512.Size = new System.Drawing.Size(98, 22);
            this.menuFft512.Text = "512";
            this.menuFft512.Click += new System.EventHandler(this.menuFft512_Click);
            // 
            // menuFft1024
            // 
            this.menuFft1024.Name = "menuFft1024";
            this.menuFft1024.Size = new System.Drawing.Size(98, 22);
            this.menuFft1024.Text = "1024";
            this.menuFft1024.Click += new System.EventHandler(this.menuFft1024_Click);
            // 
            // menuFft2048
            // 
            this.menuFft2048.Name = "menuFft2048";
            this.menuFft2048.Size = new System.Drawing.Size(98, 22);
            this.menuFft2048.Text = "2048";
            this.menuFft2048.Click += new System.EventHandler(this.menuFft2048_Click);
            // 
            // menuFft4096
            // 
            this.menuFft4096.Name = "menuFft4096";
            this.menuFft4096.Size = new System.Drawing.Size(98, 22);
            this.menuFft4096.Text = "4096";
            this.menuFft4096.Click += new System.EventHandler(this.menuFft4096_Click);
            // 
            // menuFft8192
            // 
            this.menuFft8192.Name = "menuFft8192";
            this.menuFft8192.Size = new System.Drawing.Size(98, 22);
            this.menuFft8192.Text = "8192";
            this.menuFft8192.Click += new System.EventHandler(this.menuFft8192_Click);
            // 
            // spectranV6ToolStripMenuItem
            // 
            this.spectranV6ToolStripMenuItem.Name = "spectranV6ToolStripMenuItem";
            this.spectranV6ToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.spectranV6ToolStripMenuItem.Text = "Aaronia Spectran V6";
            this.spectranV6ToolStripMenuItem.Click += new System.EventHandler(this.spectranV6ToolStripMenuItem_Click);
            // 
            // MainScreen
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 412);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.mainMenu);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainScreen";
            this.Text = "RX-FFT";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.menuButtons.ResumeLayout(false);
            this.menuButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private LibRXFFT.Components.GDI.FrequencySelector freqSelector;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        public LibRXFFT.Components.DirectX.DirectXWaterfallFFTDisplay FFTDisplay;

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem deviceMenu;
        private System.Windows.Forms.ToolStripMenuItem openMenu;
        private System.Windows.Forms.ToolStripMenuItem openBO35Menu;
        private System.Windows.Forms.ToolStripMenuItem openShMemMenu;
        private System.Windows.Forms.ToolStripMenuItem openFileMenu;
        
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
        private System.Windows.Forms.ToolStripMenuItem openNetworkDeviceMenu;
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
        private System.Windows.Forms.ToolStripMenuItem scanChannelsMenu;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel fpsLabel;
        private System.Windows.Forms.ToolStripMenuItem oscilloscopeMenu;
        private System.Windows.Forms.ToolStripMenuItem scanMarkersMenu;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel maxDbLabel;
        private System.Windows.Forms.ToolStripMenuItem agcMenu;
        private System.Windows.Forms.ToolStripMenuItem loadScriptMenu;
        private System.Windows.Forms.ToolStripMenuItem unloadScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unloadAllMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem scanBandMenu;
        private System.Windows.Forms.ToolStripMenuItem openUSRPMenu;
        private System.Windows.Forms.ToolStripMenuItem hiQSDRToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trackPeaksItem;
        private System.Windows.Forms.ToolStripMenuItem ScriptShellMenu;
        private System.Windows.Forms.ToolStripMenuItem ReloadScriptsMenu;
        private System.Windows.Forms.ToolStrip menuButtons;
        private System.Windows.Forms.ToolStripButton btnOpenDevice;
        private System.Windows.Forms.ToolStripButton btnCloseDevice;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripButton btnStartScope;
        private System.Windows.Forms.ToolStripButton btnStartGsmAnalyzer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem menuFft512;
        private System.Windows.Forms.ToolStripMenuItem menuFft1024;
        private System.Windows.Forms.ToolStripMenuItem menuFft2048;
        private System.Windows.Forms.ToolStripMenuItem menuFft4096;
        private System.Windows.Forms.ToolStripMenuItem menuFft8192;
        private System.Windows.Forms.ToolStripMenuItem dCOffsetCorrectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spectranV6ToolStripMenuItem;
    }
}

