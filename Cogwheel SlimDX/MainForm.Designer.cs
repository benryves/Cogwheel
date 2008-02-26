﻿namespace CogwheelSlimDX {
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
			this.ExitMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.EditMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenRomDialog = new System.Windows.Forms.OpenFileDialog();
			this.Status = new System.Windows.Forms.StatusStrip();
			this.MessageStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.MessageTicker = new System.Windows.Forms.Timer(this.components);
			this.ToolsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.QuickLoadRomMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.CopyScreenshotMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.GameGenieMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.GameGenieEnabledMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.GameGenieEditMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.Menus.SuspendLayout();
			this.Status.SuspendLayout();
			this.SuspendLayout();
			// 
			// RenderPanel
			// 
			this.RenderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RenderPanel.Location = new System.Drawing.Point(0, 24);
			this.RenderPanel.Name = "RenderPanel";
			this.RenderPanel.Size = new System.Drawing.Size(325, 262);
			this.RenderPanel.TabIndex = 0;
			// 
			// Menus
			// 
			this.Menus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.EditMenu,
            this.ToolsMenu});
			this.Menus.Location = new System.Drawing.Point(0, 0);
			this.Menus.Name = "Menus";
			this.Menus.Size = new System.Drawing.Size(325, 24);
			this.Menus.TabIndex = 0;
			// 
			// FileMenu
			// 
			this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.QuickLoadRomMenu,
            this.ExitMenu});
			this.FileMenu.Name = "FileMenu";
			this.FileMenu.Size = new System.Drawing.Size(37, 20);
			this.FileMenu.Text = "&File";
			// 
			// ExitMenu
			// 
			this.ExitMenu.Name = "ExitMenu";
			this.ExitMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.ExitMenu.Size = new System.Drawing.Size(216, 22);
			this.ExitMenu.Text = "E&xit";
			this.ExitMenu.Click += new System.EventHandler(this.ExitMenu_Click);
			// 
			// EditMenu
			// 
			this.EditMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CopyScreenshotMenu});
			this.EditMenu.Name = "EditMenu";
			this.EditMenu.Size = new System.Drawing.Size(39, 20);
			this.EditMenu.Text = "&Edit";
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
			this.Status.Size = new System.Drawing.Size(325, 22);
			this.Status.TabIndex = 0;
			this.Status.Text = "statusStrip1";
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
			// ToolsMenu
			// 
			this.ToolsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GameGenieMenu});
			this.ToolsMenu.Name = "ToolsMenu";
			this.ToolsMenu.Size = new System.Drawing.Size(48, 20);
			this.ToolsMenu.Text = "&Tools";
			// 
			// QuickLoadRomMenu
			// 
			this.QuickLoadRomMenu.Image = global::CogwheelSlimDX.Properties.Resources.Icon_SmsRom;
			this.QuickLoadRomMenu.Name = "QuickLoadRomMenu";
			this.QuickLoadRomMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.QuickLoadRomMenu.Size = new System.Drawing.Size(216, 22);
			this.QuickLoadRomMenu.Text = "&Quick Load ROM...";
			this.QuickLoadRomMenu.Click += new System.EventHandler(this.QuickLoadRomMenu_Click);
			// 
			// CopyScreenshotMenu
			// 
			this.CopyScreenshotMenu.Image = global::CogwheelSlimDX.Properties.Resources.Icon_Camera;
			this.CopyScreenshotMenu.Name = "CopyScreenshotMenu";
			this.CopyScreenshotMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.CopyScreenshotMenu.Size = new System.Drawing.Size(205, 22);
			this.CopyScreenshotMenu.Text = "&Copy Screenshot";
			this.CopyScreenshotMenu.Click += new System.EventHandler(this.CopyScreenshotMenu_Click);
			// 
			// GameGenieMenu
			// 
			this.GameGenieMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GameGenieEnabledMenu,
            this.GameGenieEditMenu});
			this.GameGenieMenu.Image = global::CogwheelSlimDX.Properties.Resources.Icon_GameGenie;
			this.GameGenieMenu.Name = "GameGenieMenu";
			this.GameGenieMenu.Size = new System.Drawing.Size(152, 22);
			this.GameGenieMenu.Text = "&Game Genie";
			this.GameGenieMenu.DropDownOpening += new System.EventHandler(this.GameGenieMenu_DropDownOpening);
			// 
			// GameGenieEnabledMenu
			// 
			this.GameGenieEnabledMenu.CheckOnClick = true;
			this.GameGenieEnabledMenu.Name = "GameGenieEnabledMenu";
			this.GameGenieEnabledMenu.Size = new System.Drawing.Size(152, 22);
			this.GameGenieEnabledMenu.Text = "&Enabled";
			this.GameGenieEnabledMenu.Click += new System.EventHandler(this.GameGenieEnabledMenu_Click);
			// 
			// GameGenieEditMenu
			// 
			this.GameGenieEditMenu.Name = "GameGenieEditMenu";
			this.GameGenieEditMenu.Size = new System.Drawing.Size(152, 22);
			this.GameGenieEditMenu.Text = "Edit Codes...";
			this.GameGenieEditMenu.Click += new System.EventHandler(this.GameGenieEditMenu_Click);
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
			this.MainMenuStrip = this.Menus;
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
		private System.Windows.Forms.ToolStripMenuItem EditMenu;
		private System.Windows.Forms.ToolStripMenuItem CopyScreenshotMenu;
		private System.Windows.Forms.StatusStrip Status;
		private System.Windows.Forms.ToolStripStatusLabel MessageStatus;
		private System.Windows.Forms.Timer MessageTicker;
		private System.Windows.Forms.ToolStripMenuItem ToolsMenu;
		private System.Windows.Forms.ToolStripMenuItem GameGenieMenu;
		private System.Windows.Forms.ToolStripMenuItem GameGenieEnabledMenu;
		private System.Windows.Forms.ToolStripMenuItem GameGenieEditMenu;
	}
}

