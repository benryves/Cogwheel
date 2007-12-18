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
			this.OpenRomDialog = new System.Windows.Forms.OpenFileDialog();
			this.MainFormMenus.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainFormMenus
			// 
			this.MainFormMenus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu});
			this.MainFormMenus.Location = new System.Drawing.Point(0, 0);
			this.MainFormMenus.Name = "MainFormMenus";
			this.MainFormMenus.Size = new System.Drawing.Size(284, 24);
			this.MainFormMenus.TabIndex = 0;
			// 
			// FileMenu
			// 
			this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenMenu,
            this.ExitMenu});
			this.FileMenu.Name = "FileMenu";
			this.FileMenu.Size = new System.Drawing.Size(37, 20);
			this.FileMenu.Text = "&File";
			// 
			// OpenMenu
			// 
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
			// OpenRomDialog
			// 
			this.OpenRomDialog.Filter = resources.GetString("OpenRomDialog.Filter");
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 264);
			this.Controls.Add(this.MainFormMenus);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
	}
}

