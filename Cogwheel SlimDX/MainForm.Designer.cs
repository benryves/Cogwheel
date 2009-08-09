namespace BeeDevelopment.Cogwheel {
	partial class MainForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.RenderPanel = new System.Windows.Forms.Panel();
			this.Menus = new System.Windows.Forms.MenuStrip();
			this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.QuickLoadRomMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.AdvancedLoadMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.FileSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.LoadStateMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.SaveStateMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.FileSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.QuickLoadStateMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.QuickSaveStateMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.QuickStateSlotMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.dummyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.FileSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.RecentRomsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.dummyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.FileSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.ExitMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.ViewMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.CopyScreenshotMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.ToggleFullScreenMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.EmulationMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.EmulationVideoMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.EmulationVideoNtscMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.EmulationVideoPalMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.EmulationVideoSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.EmulationVideoBackgroundEnabledMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.EmulationVideoSpritesEnabledMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.GameGenieMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.GameGenieEnabledMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.GameGenieEditMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.SdscDebugConsoleMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OptionsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.ControllerProfileMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.ControllerProfileDefaultMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.ControllerProfileSC3000Menu = new System.Windows.Forms.ToolStripMenuItem();
			this.CustomiseControlsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OptionsSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.EnableSoundMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.EnableFMSoundMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OptionsSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.SimulateGameGearLcdMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.LinearInterpolationMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.MaintainAspectRatioMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OptionsSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.ThreeDeeGlassesMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.ThreeDeeGlassesDisabledMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.AboutMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.BugReportMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenRomDialog = new System.Windows.Forms.OpenFileDialog();
			this.Status = new System.Windows.Forms.StatusStrip();
			this.MessageStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.MessageTicker = new System.Windows.Forms.Timer(this.components);
			this.OpenStateDialog = new System.Windows.Forms.OpenFileDialog();
			this.SaveStateDialog = new System.Windows.Forms.SaveFileDialog();
			this.CursorHider = new System.Windows.Forms.Timer(this.components);
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.showLeftSideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showrightSideOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.redcyanAnaglyphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.interlacedLCDShutterGlassesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Menus.SuspendLayout();
			this.Status.SuspendLayout();
			this.SuspendLayout();
			// 
			// RenderPanel
			// 
			this.RenderPanel.BackColor = System.Drawing.Color.Black;
			this.RenderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RenderPanel.Location = new System.Drawing.Point(0, 24);
			this.RenderPanel.Name = "RenderPanel";
			this.RenderPanel.Size = new System.Drawing.Size(325, 262);
			this.RenderPanel.TabIndex = 0;
			this.RenderPanel.DoubleClick += new System.EventHandler(this.RenderPanel_DoubleClick);
			this.RenderPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RenderPanel_MouseMove);
			// 
			// Menus
			// 
			this.Menus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.ViewMenu,
            this.EmulationMenu,
            this.OptionsMenu,
            this.HelpMenu});
			this.Menus.Location = new System.Drawing.Point(0, 0);
			this.Menus.Name = "Menus";
			this.Menus.Size = new System.Drawing.Size(325, 24);
			this.Menus.TabIndex = 0;
			// 
			// FileMenu
			// 
			this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.QuickLoadRomMenu,
            this.AdvancedLoadMenu,
            this.FileSep0,
            this.LoadStateMenu,
            this.SaveStateMenu,
            this.FileSep3,
            this.QuickLoadStateMenu,
            this.QuickSaveStateMenu,
            this.QuickStateSlotMenu,
            this.FileSep1,
            this.RecentRomsMenu,
            this.FileSep2,
            this.ExitMenu});
			this.FileMenu.Name = "FileMenu";
			this.FileMenu.Size = new System.Drawing.Size(37, 20);
			this.FileMenu.Text = "&File";
			// 
			// QuickLoadRomMenu
			// 
			this.QuickLoadRomMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_SmsRom;
			this.QuickLoadRomMenu.Name = "QuickLoadRomMenu";
			this.QuickLoadRomMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.QuickLoadRomMenu.Size = new System.Drawing.Size(216, 22);
			this.QuickLoadRomMenu.Text = "&Quick Load ROM...";
			this.QuickLoadRomMenu.Click += new System.EventHandler(this.QuickLoadRomMenu_Click);
			// 
			// AdvancedLoadMenu
			// 
			this.AdvancedLoadMenu.Name = "AdvancedLoadMenu";
			this.AdvancedLoadMenu.Size = new System.Drawing.Size(216, 22);
			this.AdvancedLoadMenu.Text = "&Advanced Load...";
			this.AdvancedLoadMenu.Click += new System.EventHandler(this.AdvancedLoadMenu_Click);
			// 
			// FileSep0
			// 
			this.FileSep0.Name = "FileSep0";
			this.FileSep0.Size = new System.Drawing.Size(213, 6);
			// 
			// LoadStateMenu
			// 
			this.LoadStateMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_TimeGo;
			this.LoadStateMenu.Name = "LoadStateMenu";
			this.LoadStateMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F8)));
			this.LoadStateMenu.Size = new System.Drawing.Size(216, 22);
			this.LoadStateMenu.Text = "&Load State As...";
			this.LoadStateMenu.Click += new System.EventHandler(this.LoadStateMenu_Click);
			// 
			// SaveStateMenu
			// 
			this.SaveStateMenu.Name = "SaveStateMenu";
			this.SaveStateMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
			this.SaveStateMenu.Size = new System.Drawing.Size(216, 22);
			this.SaveStateMenu.Text = "&Save State As...";
			this.SaveStateMenu.Click += new System.EventHandler(this.SaveStateMenu_Click);
			// 
			// FileSep3
			// 
			this.FileSep3.Name = "FileSep3";
			this.FileSep3.Size = new System.Drawing.Size(213, 6);
			// 
			// QuickLoadStateMenu
			// 
			this.QuickLoadStateMenu.Name = "QuickLoadStateMenu";
			this.QuickLoadStateMenu.ShortcutKeys = System.Windows.Forms.Keys.F8;
			this.QuickLoadStateMenu.Size = new System.Drawing.Size(216, 22);
			this.QuickLoadStateMenu.Text = "Quick Load State";
			this.QuickLoadStateMenu.Click += new System.EventHandler(this.QuickLoadStateMenu_Click);
			// 
			// QuickSaveStateMenu
			// 
			this.QuickSaveStateMenu.Name = "QuickSaveStateMenu";
			this.QuickSaveStateMenu.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.QuickSaveStateMenu.Size = new System.Drawing.Size(216, 22);
			this.QuickSaveStateMenu.Text = "Quick Save State";
			this.QuickSaveStateMenu.Click += new System.EventHandler(this.QuickSaveStateMenu_Click);
			// 
			// QuickStateSlotMenu
			// 
			this.QuickStateSlotMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dummyToolStripMenuItem1});
			this.QuickStateSlotMenu.Name = "QuickStateSlotMenu";
			this.QuickStateSlotMenu.Size = new System.Drawing.Size(216, 22);
			this.QuickStateSlotMenu.Text = "Quick Save Slot";
			this.QuickStateSlotMenu.DropDownOpening += new System.EventHandler(this.QuickStateSlotMenu_DropDownOpening);
			// 
			// dummyToolStripMenuItem1
			// 
			this.dummyToolStripMenuItem1.Name = "dummyToolStripMenuItem1";
			this.dummyToolStripMenuItem1.Size = new System.Drawing.Size(117, 22);
			this.dummyToolStripMenuItem1.Text = "&Dummy";
			// 
			// FileSep1
			// 
			this.FileSep1.Name = "FileSep1";
			this.FileSep1.Size = new System.Drawing.Size(213, 6);
			// 
			// RecentRomsMenu
			// 
			this.RecentRomsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dummyToolStripMenuItem});
			this.RecentRomsMenu.Name = "RecentRomsMenu";
			this.RecentRomsMenu.Size = new System.Drawing.Size(216, 22);
			this.RecentRomsMenu.Text = "&Recent ROMs";
			this.RecentRomsMenu.DropDownOpening += new System.EventHandler(this.RecentRomsMenu_DropDownOpening);
			// 
			// dummyToolStripMenuItem
			// 
			this.dummyToolStripMenuItem.Name = "dummyToolStripMenuItem";
			this.dummyToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
			this.dummyToolStripMenuItem.Text = "&Dummy";
			// 
			// FileSep2
			// 
			this.FileSep2.Name = "FileSep2";
			this.FileSep2.Size = new System.Drawing.Size(213, 6);
			// 
			// ExitMenu
			// 
			this.ExitMenu.Name = "ExitMenu";
			this.ExitMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.ExitMenu.Size = new System.Drawing.Size(216, 22);
			this.ExitMenu.Text = "E&xit";
			this.ExitMenu.Click += new System.EventHandler(this.ExitMenu_Click);
			// 
			// ViewMenu
			// 
			this.ViewMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CopyScreenshotMenu,
            this.ToggleFullScreenMenu});
			this.ViewMenu.Name = "ViewMenu";
			this.ViewMenu.Size = new System.Drawing.Size(44, 20);
			this.ViewMenu.Text = "&View";
			// 
			// CopyScreenshotMenu
			// 
			this.CopyScreenshotMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_Camera;
			this.CopyScreenshotMenu.Name = "CopyScreenshotMenu";
			this.CopyScreenshotMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.CopyScreenshotMenu.Size = new System.Drawing.Size(205, 22);
			this.CopyScreenshotMenu.Text = "&Copy Screenshot";
			this.CopyScreenshotMenu.Click += new System.EventHandler(this.CopyScreenshotMenu_Click);
			// 
			// ToggleFullScreenMenu
			// 
			this.ToggleFullScreenMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_Monitor;
			this.ToggleFullScreenMenu.Name = "ToggleFullScreenMenu";
			this.ToggleFullScreenMenu.ShortcutKeys = System.Windows.Forms.Keys.F11;
			this.ToggleFullScreenMenu.Size = new System.Drawing.Size(205, 22);
			this.ToggleFullScreenMenu.Text = "Toggle &Full Screen";
			this.ToggleFullScreenMenu.Click += new System.EventHandler(this.ToggleFullScreenMenu_Click);
			// 
			// EmulationMenu
			// 
			this.EmulationMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EmulationVideoMenu,
            this.GameGenieMenu,
            this.SdscDebugConsoleMenu});
			this.EmulationMenu.Name = "EmulationMenu";
			this.EmulationMenu.Size = new System.Drawing.Size(73, 20);
			this.EmulationMenu.Text = "&Emulation";
			// 
			// EmulationVideoMenu
			// 
			this.EmulationVideoMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EmulationVideoNtscMenu,
            this.EmulationVideoPalMenu,
            this.EmulationVideoSep0,
            this.EmulationVideoBackgroundEnabledMenu,
            this.EmulationVideoSpritesEnabledMenu});
			this.EmulationVideoMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_Television;
			this.EmulationVideoMenu.Name = "EmulationVideoMenu";
			this.EmulationVideoMenu.Size = new System.Drawing.Size(152, 22);
			this.EmulationVideoMenu.Text = "&Video";
			this.EmulationVideoMenu.DropDownOpening += new System.EventHandler(this.DebugVideoMenu_DropDownOpening);
			// 
			// EmulationVideoNtscMenu
			// 
			this.EmulationVideoNtscMenu.Name = "EmulationVideoNtscMenu";
			this.EmulationVideoNtscMenu.Size = new System.Drawing.Size(214, 22);
			this.EmulationVideoNtscMenu.Text = "&NTSC (60Hz)";
			this.EmulationVideoNtscMenu.Click += new System.EventHandler(this.EmulationVideoNtscMenu_Click);
			// 
			// EmulationVideoPalMenu
			// 
			this.EmulationVideoPalMenu.Name = "EmulationVideoPalMenu";
			this.EmulationVideoPalMenu.Size = new System.Drawing.Size(214, 22);
			this.EmulationVideoPalMenu.Text = "&PAL (50Hz)";
			this.EmulationVideoPalMenu.Click += new System.EventHandler(this.EmulationVideoPalMenu_Click);
			// 
			// EmulationVideoSep0
			// 
			this.EmulationVideoSep0.Name = "EmulationVideoSep0";
			this.EmulationVideoSep0.Size = new System.Drawing.Size(211, 6);
			// 
			// EmulationVideoBackgroundEnabledMenu
			// 
			this.EmulationVideoBackgroundEnabledMenu.Name = "EmulationVideoBackgroundEnabledMenu";
			this.EmulationVideoBackgroundEnabledMenu.Size = new System.Drawing.Size(214, 22);
			this.EmulationVideoBackgroundEnabledMenu.Text = "&Background Layer Enabled";
			this.EmulationVideoBackgroundEnabledMenu.Click += new System.EventHandler(this.BackgroundEnabledMenu_Click);
			// 
			// EmulationVideoSpritesEnabledMenu
			// 
			this.EmulationVideoSpritesEnabledMenu.Name = "EmulationVideoSpritesEnabledMenu";
			this.EmulationVideoSpritesEnabledMenu.Size = new System.Drawing.Size(214, 22);
			this.EmulationVideoSpritesEnabledMenu.Text = "&Sprite Layer Enabled";
			this.EmulationVideoSpritesEnabledMenu.Click += new System.EventHandler(this.SpritesEnabledMenu_Click);
			// 
			// GameGenieMenu
			// 
			this.GameGenieMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GameGenieEnabledMenu,
            this.GameGenieEditMenu});
			this.GameGenieMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_GameGenie;
			this.GameGenieMenu.Name = "GameGenieMenu";
			this.GameGenieMenu.Size = new System.Drawing.Size(152, 22);
			this.GameGenieMenu.Text = "&Game Genie";
			this.GameGenieMenu.DropDownOpening += new System.EventHandler(this.GameGenieMenu_DropDownOpening);
			// 
			// GameGenieEnabledMenu
			// 
			this.GameGenieEnabledMenu.CheckOnClick = true;
			this.GameGenieEnabledMenu.Name = "GameGenieEnabledMenu";
			this.GameGenieEnabledMenu.Size = new System.Drawing.Size(139, 22);
			this.GameGenieEnabledMenu.Text = "&Enabled";
			this.GameGenieEnabledMenu.Click += new System.EventHandler(this.GameGenieEnabledMenu_Click);
			// 
			// GameGenieEditMenu
			// 
			this.GameGenieEditMenu.Name = "GameGenieEditMenu";
			this.GameGenieEditMenu.Size = new System.Drawing.Size(139, 22);
			this.GameGenieEditMenu.Text = "Edit Codes...";
			this.GameGenieEditMenu.Click += new System.EventHandler(this.GameGenieEditMenu_Click);
			// 
			// SdscDebugConsoleMenu
			// 
			this.SdscDebugConsoleMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_Terminal;
			this.SdscDebugConsoleMenu.Name = "SdscDebugConsoleMenu";
			this.SdscDebugConsoleMenu.Size = new System.Drawing.Size(152, 22);
			this.SdscDebugConsoleMenu.Text = "&SDSC Console";
			this.SdscDebugConsoleMenu.Click += new System.EventHandler(this.SdscDebugConsoleMenu_Click);
			// 
			// OptionsMenu
			// 
			this.OptionsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ControllerProfileMenu,
            this.CustomiseControlsMenu,
            this.OptionsSep0,
            this.EnableSoundMenu,
            this.EnableFMSoundMenu,
            this.OptionsSep1,
            this.SimulateGameGearLcdMenu,
            this.LinearInterpolationMenu,
            this.MaintainAspectRatioMenu,
            this.OptionsSep2,
            this.ThreeDeeGlassesMenu});
			this.OptionsMenu.Name = "OptionsMenu";
			this.OptionsMenu.Size = new System.Drawing.Size(61, 20);
			this.OptionsMenu.Text = "&Options";
			this.OptionsMenu.DropDownOpening += new System.EventHandler(this.OptionsMenu_DropDownOpening);
			// 
			// ControllerProfileMenu
			// 
			this.ControllerProfileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ControllerProfileDefaultMenu,
            this.ControllerProfileSC3000Menu});
			this.ControllerProfileMenu.Name = "ControllerProfileMenu";
			this.ControllerProfileMenu.Size = new System.Drawing.Size(247, 22);
			this.ControllerProfileMenu.Text = "Controller &Profile";
			this.ControllerProfileMenu.DropDownOpening += new System.EventHandler(this.ControllerProfileMenu_DropDownOpening);
			// 
			// ControllerProfileDefaultMenu
			// 
			this.ControllerProfileDefaultMenu.Name = "ControllerProfileDefaultMenu";
			this.ControllerProfileDefaultMenu.Size = new System.Drawing.Size(170, 22);
			this.ControllerProfileDefaultMenu.Text = "&Default";
			this.ControllerProfileDefaultMenu.Click += new System.EventHandler(this.SetControllerProfile_Click);
			// 
			// ControllerProfileSC3000Menu
			// 
			this.ControllerProfileSC3000Menu.Name = "ControllerProfileSC3000Menu";
			this.ControllerProfileSC3000Menu.Size = new System.Drawing.Size(170, 22);
			this.ControllerProfileSC3000Menu.Tag = "SC3000Keyboard";
			this.ControllerProfileSC3000Menu.Text = "&SC-3000 Keyboard";
			this.ControllerProfileSC3000Menu.Click += new System.EventHandler(this.SetControllerProfile_Click);
			// 
			// CustomiseControlsMenu
			// 
			this.CustomiseControlsMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_Controller;
			this.CustomiseControlsMenu.Name = "CustomiseControlsMenu";
			this.CustomiseControlsMenu.Size = new System.Drawing.Size(247, 22);
			this.CustomiseControlsMenu.Text = "&Customise Controls...";
			this.CustomiseControlsMenu.Click += new System.EventHandler(this.CustomiseControlsToolStripMenuItem_Click);
			// 
			// OptionsSep0
			// 
			this.OptionsSep0.Name = "OptionsSep0";
			this.OptionsSep0.Size = new System.Drawing.Size(244, 6);
			// 
			// EnableSoundMenu
			// 
			this.EnableSoundMenu.Name = "EnableSoundMenu";
			this.EnableSoundMenu.Size = new System.Drawing.Size(247, 22);
			this.EnableSoundMenu.Text = "&Enable Sound";
			this.EnableSoundMenu.Click += new System.EventHandler(this.EnableSoundMenu_Click);
			// 
			// EnableFMSoundMenu
			// 
			this.EnableFMSoundMenu.Name = "EnableFMSoundMenu";
			this.EnableFMSoundMenu.Size = new System.Drawing.Size(247, 22);
			this.EnableFMSoundMenu.Text = "Enable &FM Sound (Experimental)";
			this.EnableFMSoundMenu.Click += new System.EventHandler(this.EnableFMSoundMenu_Click);
			// 
			// OptionsSep1
			// 
			this.OptionsSep1.Name = "OptionsSep1";
			this.OptionsSep1.Size = new System.Drawing.Size(244, 6);
			// 
			// SimulateGameGearLcdMenu
			// 
			this.SimulateGameGearLcdMenu.Name = "SimulateGameGearLcdMenu";
			this.SimulateGameGearLcdMenu.Size = new System.Drawing.Size(247, 22);
			this.SimulateGameGearLcdMenu.Text = "Simulate Game Gear LCD &Scaling";
			this.SimulateGameGearLcdMenu.Click += new System.EventHandler(this.SimulateGameGearLcdMenu_Click);
			// 
			// LinearInterpolationMenu
			// 
			this.LinearInterpolationMenu.Name = "LinearInterpolationMenu";
			this.LinearInterpolationMenu.Size = new System.Drawing.Size(247, 22);
			this.LinearInterpolationMenu.Text = "&Linear Interpolation (Smooth)";
			this.LinearInterpolationMenu.Click += new System.EventHandler(this.LinearInterpolationMenu_Click);
			// 
			// MaintainAspectRatioMenu
			// 
			this.MaintainAspectRatioMenu.Name = "MaintainAspectRatioMenu";
			this.MaintainAspectRatioMenu.Size = new System.Drawing.Size(247, 22);
			this.MaintainAspectRatioMenu.Text = "Maintain &Aspect Ratio";
			this.MaintainAspectRatioMenu.Click += new System.EventHandler(this.MaintainAspectRatioMenu_Click);
			// 
			// OptionsSep2
			// 
			this.OptionsSep2.Name = "OptionsSep2";
			this.OptionsSep2.Size = new System.Drawing.Size(244, 6);
			// 
			// ThreeDeeGlassesMenu
			// 
			this.ThreeDeeGlassesMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ThreeDeeGlassesDisabledMenu,
            this.toolStripMenuItem1,
            this.showLeftSideToolStripMenuItem,
            this.showrightSideOnlyToolStripMenuItem,
            this.toolStripMenuItem2,
            this.redcyanAnaglyphToolStripMenuItem,
            this.interlacedLCDShutterGlassesToolStripMenuItem});
			this.ThreeDeeGlassesMenu.Name = "ThreeDeeGlassesMenu";
			this.ThreeDeeGlassesMenu.Size = new System.Drawing.Size(247, 22);
			this.ThreeDeeGlassesMenu.Text = "&3D glasses";
			// 
			// ThreeDeeGlassesDisabledMenu
			// 
			this.ThreeDeeGlassesDisabledMenu.Name = "ThreeDeeGlassesDisabledMenu";
			this.ThreeDeeGlassesDisabledMenu.Size = new System.Drawing.Size(216, 22);
			this.ThreeDeeGlassesDisabledMenu.Text = "&Disabled (Show both sides)";
			// 
			// HelpMenu
			// 
			this.HelpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutMenu,
            this.HelpSep0,
            this.BugReportMenu});
			this.HelpMenu.Name = "HelpMenu";
			this.HelpMenu.Size = new System.Drawing.Size(44, 20);
			this.HelpMenu.Text = "&Help";
			// 
			// AboutMenu
			// 
			this.AboutMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_Information;
			this.AboutMenu.Name = "AboutMenu";
			this.AboutMenu.Size = new System.Drawing.Size(142, 22);
			this.AboutMenu.Text = "&About";
			this.AboutMenu.Click += new System.EventHandler(this.AboutMenu_Click);
			// 
			// HelpSep0
			// 
			this.HelpSep0.Name = "HelpSep0";
			this.HelpSep0.Size = new System.Drawing.Size(139, 6);
			// 
			// BugReportMenu
			// 
			this.BugReportMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_Bug;
			this.BugReportMenu.Name = "BugReportMenu";
			this.BugReportMenu.Size = new System.Drawing.Size(142, 22);
			this.BugReportMenu.Text = "&Report a Bug";
			this.BugReportMenu.Click += new System.EventHandler(this.BugReportMenu_Click);
			// 
			// OpenRomDialog
			// 
			this.OpenRomDialog.Filter = resources.GetString("OpenRomDialog.Filter");
			// 
			// Status
			// 
			this.Status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MessageStatus});
			this.Status.Location = new System.Drawing.Point(0, 286);
			this.Status.Name = "Status";
			this.Status.Size = new System.Drawing.Size(325, 22);
			this.Status.SizingGrip = false;
			this.Status.TabIndex = 0;
			// 
			// MessageStatus
			// 
			this.MessageStatus.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.MessageStatus.Name = "MessageStatus";
			this.MessageStatus.Size = new System.Drawing.Size(0, 17);
			// 
			// MessageTicker
			// 
			this.MessageTicker.Enabled = true;
			this.MessageTicker.Interval = 2500;
			this.MessageTicker.Tick += new System.EventHandler(this.MessageTicker_Tick);
			// 
			// OpenStateDialog
			// 
			this.OpenStateDialog.Filter = "Cogwheel Saved States (*.cogstate)|*.cogstate|All Files (*.*)|*.*";
			// 
			// SaveStateDialog
			// 
			this.SaveStateDialog.Filter = "Cogwheel Saved States (*.cogstate)|*.cogstate|All Files (*.*)|*.*";
			// 
			// CursorHider
			// 
			this.CursorHider.Interval = 1000;
			this.CursorHider.Tick += new System.EventHandler(this.CursorHider_Tick);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(213, 6);
			// 
			// showLeftSideToolStripMenuItem
			// 
			this.showLeftSideToolStripMenuItem.Name = "showLeftSideToolStripMenuItem";
			this.showLeftSideToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.showLeftSideToolStripMenuItem.Text = "&Left side only";
			// 
			// showrightSideOnlyToolStripMenuItem
			// 
			this.showrightSideOnlyToolStripMenuItem.Name = "showrightSideOnlyToolStripMenuItem";
			this.showrightSideOnlyToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.showrightSideOnlyToolStripMenuItem.Text = "&Right side only";
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(228, 6);
			// 
			// redcyanAnaglyphToolStripMenuItem
			// 
			this.redcyanAnaglyphToolStripMenuItem.Name = "redcyanAnaglyphToolStripMenuItem";
			this.redcyanAnaglyphToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.redcyanAnaglyphToolStripMenuItem.Text = "Red-cyan &anaglyph";
			// 
			// interlacedLCDShutterGlassesToolStripMenuItem
			// 
			this.interlacedLCDShutterGlassesToolStripMenuItem.Name = "interlacedLCDShutterGlassesToolStripMenuItem";
			this.interlacedLCDShutterGlassesToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.interlacedLCDShutterGlassesToolStripMenuItem.Text = "&Interlaced LCD shutter glasses";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(325, 308);
			this.Controls.Add(this.RenderPanel);
			this.Controls.Add(this.Status);
			this.Controls.Add(this.Menus);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MainMenuStrip = this.Menus;
			this.MinimumSize = new System.Drawing.Size(256, 128);
			this.Name = "MainForm";
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
			this.Menus.ResumeLayout(false);
			this.Menus.PerformLayout();
			this.Status.ResumeLayout(false);
			this.Status.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel RenderPanel;
		private System.Windows.Forms.MenuStrip Menus;
		private System.Windows.Forms.ToolStripMenuItem FileMenu;
		private System.Windows.Forms.ToolStripMenuItem QuickLoadRomMenu;
		private System.Windows.Forms.ToolStripMenuItem ExitMenu;
		private System.Windows.Forms.OpenFileDialog OpenRomDialog;
		private System.Windows.Forms.ToolStripMenuItem ViewMenu;
		private System.Windows.Forms.ToolStripMenuItem CopyScreenshotMenu;
		private System.Windows.Forms.StatusStrip Status;
		private System.Windows.Forms.ToolStripStatusLabel MessageStatus;
		private System.Windows.Forms.Timer MessageTicker;
		private System.Windows.Forms.ToolStripMenuItem AdvancedLoadMenu;
		private System.Windows.Forms.ToolStripSeparator FileSep0;
		private System.Windows.Forms.ToolStripMenuItem HelpMenu;
		private System.Windows.Forms.ToolStripMenuItem AboutMenu;
		private System.Windows.Forms.ToolStripSeparator HelpSep0;
		private System.Windows.Forms.ToolStripMenuItem BugReportMenu;
		private System.Windows.Forms.ToolStripMenuItem OptionsMenu;
		private System.Windows.Forms.ToolStripMenuItem CustomiseControlsMenu;
		private System.Windows.Forms.ToolStripSeparator OptionsSep0;
		private System.Windows.Forms.ToolStripMenuItem SimulateGameGearLcdMenu;
		private System.Windows.Forms.ToolStripMenuItem LinearInterpolationMenu;
		private System.Windows.Forms.ToolStripMenuItem LoadStateMenu;
		private System.Windows.Forms.ToolStripMenuItem SaveStateMenu;
		private System.Windows.Forms.ToolStripSeparator FileSep1;
		private System.Windows.Forms.OpenFileDialog OpenStateDialog;
		private System.Windows.Forms.SaveFileDialog SaveStateDialog;
		private System.Windows.Forms.ToolStripMenuItem EnableSoundMenu;
		private System.Windows.Forms.ToolStripMenuItem RecentRomsMenu;
		private System.Windows.Forms.ToolStripSeparator FileSep2;
		private System.Windows.Forms.ToolStripMenuItem dummyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem MaintainAspectRatioMenu;
		private System.Windows.Forms.ToolStripMenuItem ToggleFullScreenMenu;
		private System.Windows.Forms.Timer CursorHider;
		private System.Windows.Forms.ToolStripMenuItem EmulationMenu;
		private System.Windows.Forms.ToolStripMenuItem EmulationVideoMenu;
		private System.Windows.Forms.ToolStripMenuItem EmulationVideoBackgroundEnabledMenu;
		private System.Windows.Forms.ToolStripMenuItem EmulationVideoSpritesEnabledMenu;
		private System.Windows.Forms.ToolStripMenuItem ControllerProfileMenu;
		private System.Windows.Forms.ToolStripMenuItem ControllerProfileDefaultMenu;
		private System.Windows.Forms.ToolStripMenuItem ControllerProfileSC3000Menu;
		private System.Windows.Forms.ToolStripSeparator FileSep3;
		private System.Windows.Forms.ToolStripMenuItem QuickLoadStateMenu;
		private System.Windows.Forms.ToolStripMenuItem QuickSaveStateMenu;
		private System.Windows.Forms.ToolStripMenuItem QuickStateSlotMenu;
		private System.Windows.Forms.ToolStripMenuItem dummyToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem SdscDebugConsoleMenu;
		private System.Windows.Forms.ToolStripMenuItem EmulationVideoNtscMenu;
		private System.Windows.Forms.ToolStripMenuItem EmulationVideoPalMenu;
		private System.Windows.Forms.ToolStripSeparator EmulationVideoSep0;
		private System.Windows.Forms.ToolStripMenuItem GameGenieMenu;
		private System.Windows.Forms.ToolStripMenuItem GameGenieEnabledMenu;
		private System.Windows.Forms.ToolStripMenuItem GameGenieEditMenu;
		private System.Windows.Forms.ToolStripSeparator OptionsSep1;
#if EMU2413
		private System.Windows.Forms.ToolStripMenuItem EnableFMSoundMenu;
		private System.Windows.Forms.ToolStripSeparator OptionsSep2;
		private System.Windows.Forms.ToolStripMenuItem ThreeDeeGlassesMenu;
		private System.Windows.Forms.ToolStripMenuItem ThreeDeeGlassesDisabledMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem showLeftSideToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showrightSideOnlyToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem redcyanAnaglyphToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem interlacedLCDShutterGlassesToolStripMenuItem;
#endif
	}
}

