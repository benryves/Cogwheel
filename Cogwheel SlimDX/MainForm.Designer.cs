namespace CogwheelSlimDX {
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
			this.OpenRomDialog = new System.Windows.Forms.OpenFileDialog();
			this.Status = new System.Windows.Forms.StatusStrip();
			this.MessageStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.MessageTicker = new System.Windows.Forms.Timer(this.components);
			this.Menus = new System.Windows.Forms.MainMenu(this.components);
			this.FileMenu = new System.Windows.Forms.MenuItem();
			this.QuickLoadRomMenu = new System.Windows.Forms.MenuItem();
			this.FileSep0 = new System.Windows.Forms.MenuItem();
			this.ExitMenu = new System.Windows.Forms.MenuItem();
			this.EditMenu = new System.Windows.Forms.MenuItem();
			this.CopyScreenshotMenu = new System.Windows.Forms.MenuItem();
			this.ToolsMenu = new System.Windows.Forms.MenuItem();
			this.GameGenieMenu = new System.Windows.Forms.MenuItem();
			this.GameGenieEnabledMenu = new System.Windows.Forms.MenuItem();
			this.GameGenieEditCodesMenu = new System.Windows.Forms.MenuItem();
			this.Status.SuspendLayout();
			this.SuspendLayout();
			// 
			// RenderPanel
			// 
			this.RenderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RenderPanel.Location = new System.Drawing.Point(0, 0);
			this.RenderPanel.Name = "RenderPanel";
			this.RenderPanel.Size = new System.Drawing.Size(325, 286);
			this.RenderPanel.TabIndex = 0;
			// 
			// OpenRomDialog
			// 
			this.OpenRomDialog.Filter = "ROM Files (*.zip;*.sms;*.gg;*.sg;*.sc;*.mv)|*.zip;*.sms;*.gg;*.sg;*.sc;*.mv|All F" +
				"iles (*.*)|*.*";
			// 
			// Status
			// 
			this.Status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MessageStatus});
			this.Status.Location = new System.Drawing.Point(0, 286);
			this.Status.Name = "Status";
			this.Status.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.Status.Size = new System.Drawing.Size(325, 22);
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
			// Menus
			// 
			this.Menus.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.FileMenu,
            this.EditMenu,
            this.ToolsMenu});
			// 
			// FileMenu
			// 
			this.FileMenu.Index = 0;
			this.FileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.QuickLoadRomMenu,
            this.FileSep0,
            this.ExitMenu});
			this.FileMenu.Text = "&File";
			// 
			// QuickLoadRomMenu
			// 
			this.QuickLoadRomMenu.Index = 0;
			this.QuickLoadRomMenu.Text = "&Quick Load ROM...";
			this.QuickLoadRomMenu.Click += new System.EventHandler(this.QuickLoadRomMenu_Click);
			// 
			// FileSep0
			// 
			this.FileSep0.Index = 1;
			this.FileSep0.Text = "-";
			// 
			// ExitMenu
			// 
			this.ExitMenu.Index = 2;
			this.ExitMenu.Text = "E&xit";
			this.ExitMenu.Click += new System.EventHandler(this.ExitMenu_Click);
			// 
			// EditMenu
			// 
			this.EditMenu.Index = 1;
			this.EditMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.CopyScreenshotMenu});
			this.EditMenu.Text = "&Edit";
			// 
			// CopyScreenshotMenu
			// 
			this.CopyScreenshotMenu.Index = 0;
			this.CopyScreenshotMenu.Text = "&Copy Screenshot";
			this.CopyScreenshotMenu.Click += new System.EventHandler(this.CopyScreenshotMenu_Click);
			// 
			// ToolsMenu
			// 
			this.ToolsMenu.Index = 2;
			this.ToolsMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.GameGenieMenu});
			this.ToolsMenu.Text = "&Tools";
			// 
			// GameGenieMenu
			// 
			this.GameGenieMenu.Index = 0;
			this.GameGenieMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.GameGenieEnabledMenu,
            this.GameGenieEditCodesMenu});
			this.GameGenieMenu.Text = "&Game Genie";
			this.GameGenieMenu.Popup += new System.EventHandler(this.GameGenieMenu_DropDownOpening);
			// 
			// GameGenieEnabledMenu
			// 
			this.GameGenieEnabledMenu.Index = 0;
			this.GameGenieEnabledMenu.Text = "&Enabled";
			this.GameGenieEnabledMenu.Click += new System.EventHandler(this.GameGenieEnabledMenu_Click);
			// 
			// GameGenieEditCodesMenu
			// 
			this.GameGenieEditCodesMenu.Index = 1;
			this.GameGenieEditCodesMenu.Text = "&Edit Codes...";
			this.GameGenieEditCodesMenu.Click += new System.EventHandler(this.GameGenieEditMenu_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(325, 308);
			this.Controls.Add(this.RenderPanel);
			this.Controls.Add(this.Status);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Menu = this.Menus;
			this.Name = "MainForm";
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
			this.Status.ResumeLayout(false);
			this.Status.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel RenderPanel;
		private System.Windows.Forms.OpenFileDialog OpenRomDialog;
		private System.Windows.Forms.StatusStrip Status;
		private System.Windows.Forms.ToolStripStatusLabel MessageStatus;
		private System.Windows.Forms.Timer MessageTicker;
		private System.Windows.Forms.MainMenu Menus;
		private System.Windows.Forms.MenuItem FileMenu;
		private System.Windows.Forms.MenuItem QuickLoadRomMenu;
		private System.Windows.Forms.MenuItem ExitMenu;
		private System.Windows.Forms.MenuItem EditMenu;
		private System.Windows.Forms.MenuItem CopyScreenshotMenu;
		private System.Windows.Forms.MenuItem ToolsMenu;
		private System.Windows.Forms.MenuItem GameGenieMenu;
		private System.Windows.Forms.MenuItem GameGenieEnabledMenu;
		private System.Windows.Forms.MenuItem GameGenieEditCodesMenu;
		private System.Windows.Forms.MenuItem FileSep0;
	}
}

