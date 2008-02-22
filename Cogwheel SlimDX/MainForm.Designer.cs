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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.RenderPanel = new System.Windows.Forms.Panel();
			this.Menus = new System.Windows.Forms.MenuStrip();
			this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.QuickLoadRomMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.ExitMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenRomDialog = new System.Windows.Forms.OpenFileDialog();
			this.PropertiesMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.Menus.SuspendLayout();
			this.SuspendLayout();
			// 
			// RenderPanel
			// 
			this.RenderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RenderPanel.Location = new System.Drawing.Point(0, 24);
			this.RenderPanel.Name = "RenderPanel";
			this.RenderPanel.Size = new System.Drawing.Size(325, 284);
			this.RenderPanel.TabIndex = 0;
			// 
			// Menus
			// 
			this.Menus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu});
			this.Menus.Location = new System.Drawing.Point(0, 0);
			this.Menus.Name = "Menus";
			this.Menus.Size = new System.Drawing.Size(325, 24);
			this.Menus.TabIndex = 0;
			this.Menus.Text = "menuStrip1";
			// 
			// FileMenu
			// 
			this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.QuickLoadRomMenu,
            this.PropertiesMenu,
            this.ExitMenu});
			this.FileMenu.Name = "FileMenu";
			this.FileMenu.Size = new System.Drawing.Size(37, 20);
			this.FileMenu.Text = "&File";
			// 
			// QuickLoadRomMenu
			// 
			this.QuickLoadRomMenu.Image = global::CogwheelSlimDX.Properties.Resources.sms_rom;
			this.QuickLoadRomMenu.Name = "QuickLoadRomMenu";
			this.QuickLoadRomMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.QuickLoadRomMenu.Size = new System.Drawing.Size(216, 22);
			this.QuickLoadRomMenu.Text = "&Quick Load ROM...";
			this.QuickLoadRomMenu.Click += new System.EventHandler(this.QuickLoadRomMenu_Click);
			// 
			// ExitMenu
			// 
			this.ExitMenu.Name = "ExitMenu";
			this.ExitMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.ExitMenu.Size = new System.Drawing.Size(216, 22);
			this.ExitMenu.Text = "E&xit";
			this.ExitMenu.Click += new System.EventHandler(this.ExitMenu_Click);
			// 
			// OpenRomDialog
			// 
			this.OpenRomDialog.Filter = "ROM Files (*.zip;*.sms;*.gg;*.sg;*.sc;*.mv)|*.zip;*.sms;*.gg;*.sg;*.sc;*.mv|All F" +
				"iles (*.*)|*.*";
			// 
			// PropertiesMenu
			// 
			this.PropertiesMenu.Name = "PropertiesMenu";
			this.PropertiesMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Return)));
			this.PropertiesMenu.Size = new System.Drawing.Size(216, 22);
			this.PropertiesMenu.Text = "&Properties...";
			this.PropertiesMenu.Click += new System.EventHandler(this.PropertiesMenu_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(325, 308);
			this.Controls.Add(this.RenderPanel);
			this.Controls.Add(this.Menus);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.Menus;
			this.Name = "MainForm";
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
			this.Menus.ResumeLayout(false);
			this.Menus.PerformLayout();
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
		private System.Windows.Forms.ToolStripMenuItem PropertiesMenu;
	}
}

