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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.MainFormMenus = new System.Windows.Forms.MenuStrip();
			this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.ExitMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OptionsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.JapaneseMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.ExportMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenRomDialog = new System.Windows.Forms.OpenFileDialog();
			this.RegionMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.EmulatorHost = new BeeDevelopment.Cogwheel.Sega8BitHost();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.MainFormMenus.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainFormMenus
			// 
			this.MainFormMenus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.OptionsMenu});
			this.MainFormMenus.Location = new System.Drawing.Point(0, 0);
			this.MainFormMenus.Name = "MainFormMenus";
			this.MainFormMenus.Size = new System.Drawing.Size(284, 24);
			this.MainFormMenus.TabIndex = 0;
			// 
			// FileMenu
			// 
			this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenMenu,
            this.toolStripMenuItem1,
            this.ExitMenu});
			this.FileMenu.Name = "FileMenu";
			this.FileMenu.Size = new System.Drawing.Size(37, 20);
			this.FileMenu.Text = "&File";
			// 
			// OpenMenu
			// 
			this.OpenMenu.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icons_SmsRom;
			this.OpenMenu.Name = "OpenMenu";
			this.OpenMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.OpenMenu.Size = new System.Drawing.Size(185, 22);
			this.OpenMenu.Text = "&Open ROM...";
			this.OpenMenu.Click += new System.EventHandler(this.OpenMenu_Click);
			// 
			// ExitMenu
			// 
			this.ExitMenu.Name = "ExitMenu";
			this.ExitMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.ExitMenu.Size = new System.Drawing.Size(185, 22);
			this.ExitMenu.Text = "E&xit";
			this.ExitMenu.Click += new System.EventHandler(this.ExitMenu_Click);
			// 
			// OptionsMenu
			// 
			this.OptionsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.JapaneseMenu,
            this.ExportMenu});
			this.OptionsMenu.Name = "OptionsMenu";
			this.OptionsMenu.Size = new System.Drawing.Size(61, 20);
			this.OptionsMenu.Text = "&Options";
			this.OptionsMenu.DropDownOpening += new System.EventHandler(this.OptionsMenu_DropDownOpening);
			// 
			// JapaneseMenu
			// 
			this.JapaneseMenu.Name = "JapaneseMenu";
			this.JapaneseMenu.Size = new System.Drawing.Size(121, 22);
			this.JapaneseMenu.Text = "&Japanese";
			this.JapaneseMenu.Click += new System.EventHandler(this.JapaneseMenu_Click);
			// 
			// ExportMenu
			// 
			this.ExportMenu.Name = "ExportMenu";
			this.ExportMenu.Size = new System.Drawing.Size(121, 22);
			this.ExportMenu.Text = "&Export";
			this.ExportMenu.Click += new System.EventHandler(this.ExportMenu_Click);
			// 
			// OpenRomDialog
			// 
			this.OpenRomDialog.Filter = resources.GetString("OpenRomDialog.Filter");
			// 
			// RegionMenu
			// 
			this.RegionMenu.Name = "RegionMenu";
			this.RegionMenu.Size = new System.Drawing.Size(32, 19);
			// 
			// EmulatorHost
			// 
			this.EmulatorHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EmulatorHost.Emulator = null;
			this.EmulatorHost.Location = new System.Drawing.Point(0, 24);
			this.EmulatorHost.Name = "EmulatorHost";
			this.EmulatorHost.PlayerA = null;
			this.EmulatorHost.PlayerB = null;
			this.EmulatorHost.Size = new System.Drawing.Size(284, 240);
			this.EmulatorHost.TabIndex = 1;
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(182, 6);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 264);
			this.Controls.Add(this.EmulatorHost);
			this.Controls.Add(this.MainFormMenus);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MainMenuStrip = this.MainFormMenus;
			this.Name = "MainForm";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.MainFormMenus.ResumeLayout(false);
			this.MainFormMenus.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip MainFormMenus;
		private System.Windows.Forms.ToolStripMenuItem FileMenu;
		private System.Windows.Forms.ToolStripMenuItem OpenMenu;
		private System.Windows.Forms.ToolStripMenuItem ExitMenu;
		private System.Windows.Forms.OpenFileDialog OpenRomDialog;
		private Sega8BitHost EmulatorHost;
		private System.Windows.Forms.ToolStripMenuItem OptionsMenu;
		private System.Windows.Forms.ToolStripMenuItem RegionMenu;
		private System.Windows.Forms.ToolStripMenuItem JapaneseMenu;
		private System.Windows.Forms.ToolStripMenuItem ExportMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
	}
}

