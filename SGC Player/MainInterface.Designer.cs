namespace BeeDevelopment.SgcPlayer {
	partial class MainInterface {
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
			this.MetaDataTable = new System.Windows.Forms.TableLayoutPanel();
			this.SongNameLabel = new System.Windows.Forms.Label();
			this.AuthorLabel = new System.Windows.Forms.Label();
			this.CopyrightLabel = new System.Windows.Forms.Label();
			this.CopyrightHeaderLabel = new System.Windows.Forms.Label();
			this.SongNameHeaderLabel = new System.Windows.Forms.Label();
			this.AuthorHeaderLabel = new System.Windows.Forms.Label();
			this.SongSelection = new System.Windows.Forms.TrackBar();
			this.ButtonPrevious = new System.Windows.Forms.Button();
			this.ControlButtonContainer = new System.Windows.Forms.TableLayoutPanel();
			this.ButtonNext = new System.Windows.Forms.Button();
			this.ButtonPlayPause = new System.Windows.Forms.Button();
			this.Menus = new System.Windows.Forms.MenuStrip();
			this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.FileOpenMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.FileSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.FileExitMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenSgcFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.MetaDataTable.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SongSelection)).BeginInit();
			this.ControlButtonContainer.SuspendLayout();
			this.Menus.SuspendLayout();
			this.SuspendLayout();
			// 
			// MetaDataTable
			// 
			this.MetaDataTable.ColumnCount = 2;
			this.MetaDataTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.MetaDataTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MetaDataTable.Controls.Add(this.SongNameLabel, 1, 0);
			this.MetaDataTable.Controls.Add(this.AuthorLabel, 1, 1);
			this.MetaDataTable.Controls.Add(this.CopyrightLabel, 1, 2);
			this.MetaDataTable.Controls.Add(this.CopyrightHeaderLabel, 0, 2);
			this.MetaDataTable.Controls.Add(this.SongNameHeaderLabel, 0, 0);
			this.MetaDataTable.Controls.Add(this.AuthorHeaderLabel, 0, 1);
			this.MetaDataTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MetaDataTable.Location = new System.Drawing.Point(0, 24);
			this.MetaDataTable.Name = "MetaDataTable";
			this.MetaDataTable.RowCount = 4;
			this.MetaDataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.MetaDataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.MetaDataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.MetaDataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.MetaDataTable.Size = new System.Drawing.Size(442, 102);
			this.MetaDataTable.TabIndex = 0;
			// 
			// SongNameLabel
			// 
			this.SongNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.SongNameLabel.AutoSize = true;
			this.SongNameLabel.Location = new System.Drawing.Point(83, 8);
			this.SongNameLabel.Name = "SongNameLabel";
			this.SongNameLabel.Size = new System.Drawing.Size(82, 13);
			this.SongNameLabel.TabIndex = 5;
			this.SongNameLabel.Text = "No song loaded";
			// 
			// AuthorLabel
			// 
			this.AuthorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.AuthorLabel.AutoSize = true;
			this.AuthorLabel.Location = new System.Drawing.Point(83, 38);
			this.AuthorLabel.Name = "AuthorLabel";
			this.AuthorLabel.Size = new System.Drawing.Size(0, 13);
			this.AuthorLabel.TabIndex = 4;
			// 
			// CopyrightLabel
			// 
			this.CopyrightLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.CopyrightLabel.AutoSize = true;
			this.CopyrightLabel.Location = new System.Drawing.Point(83, 68);
			this.CopyrightLabel.Name = "CopyrightLabel";
			this.CopyrightLabel.Size = new System.Drawing.Size(0, 13);
			this.CopyrightLabel.TabIndex = 3;
			// 
			// CopyrightHeaderLabel
			// 
			this.CopyrightHeaderLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.CopyrightHeaderLabel.AutoSize = true;
			this.CopyrightHeaderLabel.Location = new System.Drawing.Point(3, 68);
			this.CopyrightHeaderLabel.Name = "CopyrightHeaderLabel";
			this.CopyrightHeaderLabel.Size = new System.Drawing.Size(51, 13);
			this.CopyrightHeaderLabel.TabIndex = 2;
			this.CopyrightHeaderLabel.Text = "Copyright";
			// 
			// SongNameHeaderLabel
			// 
			this.SongNameHeaderLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.SongNameHeaderLabel.AutoSize = true;
			this.SongNameHeaderLabel.Location = new System.Drawing.Point(3, 8);
			this.SongNameHeaderLabel.Name = "SongNameHeaderLabel";
			this.SongNameHeaderLabel.Size = new System.Drawing.Size(32, 13);
			this.SongNameHeaderLabel.TabIndex = 1;
			this.SongNameHeaderLabel.Text = "Song";
			// 
			// AuthorHeaderLabel
			// 
			this.AuthorHeaderLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.AuthorHeaderLabel.AutoSize = true;
			this.AuthorHeaderLabel.Location = new System.Drawing.Point(3, 38);
			this.AuthorHeaderLabel.Name = "AuthorHeaderLabel";
			this.AuthorHeaderLabel.Size = new System.Drawing.Size(38, 13);
			this.AuthorHeaderLabel.TabIndex = 1;
			this.AuthorHeaderLabel.Text = "Author";
			// 
			// SongSelection
			// 
			this.SongSelection.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SongSelection.Enabled = false;
			this.SongSelection.Location = new System.Drawing.Point(0, 126);
			this.SongSelection.Name = "SongSelection";
			this.SongSelection.Size = new System.Drawing.Size(442, 45);
			this.SongSelection.TabIndex = 1;
			this.SongSelection.ValueChanged += new System.EventHandler(this.SongSelection_ValueChanged);
			this.SongSelection.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SongSelection_MouseDown);
			this.SongSelection.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SongSelection_MouseUp);
			// 
			// ButtonPrevious
			// 
			this.ButtonPrevious.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ButtonPrevious.Image = global::BeeDevelopment.SgcPlayer.Properties.Resources.control_start;
			this.ButtonPrevious.Location = new System.Drawing.Point(3, 3);
			this.ButtonPrevious.Name = "ButtonPrevious";
			this.ButtonPrevious.Size = new System.Drawing.Size(141, 28);
			this.ButtonPrevious.TabIndex = 3;
			this.ButtonPrevious.UseVisualStyleBackColor = true;
			this.ButtonPrevious.Click += new System.EventHandler(this.ButtonPrevious_Click);
			// 
			// ControlButtonContainer
			// 
			this.ControlButtonContainer.ColumnCount = 3;
			this.ControlButtonContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.ControlButtonContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.ControlButtonContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.ControlButtonContainer.Controls.Add(this.ButtonNext, 2, 0);
			this.ControlButtonContainer.Controls.Add(this.ButtonPlayPause, 1, 0);
			this.ControlButtonContainer.Controls.Add(this.ButtonPrevious, 0, 0);
			this.ControlButtonContainer.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ControlButtonContainer.Enabled = false;
			this.ControlButtonContainer.Location = new System.Drawing.Point(0, 171);
			this.ControlButtonContainer.Name = "ControlButtonContainer";
			this.ControlButtonContainer.RowCount = 1;
			this.ControlButtonContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ControlButtonContainer.Size = new System.Drawing.Size(442, 34);
			this.ControlButtonContainer.TabIndex = 4;
			// 
			// ButtonNext
			// 
			this.ButtonNext.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ButtonNext.Image = global::BeeDevelopment.SgcPlayer.Properties.Resources.control_end;
			this.ButtonNext.Location = new System.Drawing.Point(297, 3);
			this.ButtonNext.Name = "ButtonNext";
			this.ButtonNext.Size = new System.Drawing.Size(142, 28);
			this.ButtonNext.TabIndex = 5;
			this.ButtonNext.UseVisualStyleBackColor = true;
			this.ButtonNext.Click += new System.EventHandler(this.ButtonNext_Click);
			// 
			// ButtonPlayPause
			// 
			this.ButtonPlayPause.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ButtonPlayPause.Image = global::BeeDevelopment.SgcPlayer.Properties.Resources.control_play;
			this.ButtonPlayPause.Location = new System.Drawing.Point(150, 3);
			this.ButtonPlayPause.Name = "ButtonPlayPause";
			this.ButtonPlayPause.Size = new System.Drawing.Size(141, 28);
			this.ButtonPlayPause.TabIndex = 4;
			this.ButtonPlayPause.UseVisualStyleBackColor = true;
			this.ButtonPlayPause.Click += new System.EventHandler(this.ButtonPlayPause_Click);
			// 
			// Menus
			// 
			this.Menus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu});
			this.Menus.Location = new System.Drawing.Point(0, 0);
			this.Menus.Name = "Menus";
			this.Menus.Size = new System.Drawing.Size(442, 24);
			this.Menus.TabIndex = 5;
			this.Menus.Text = "menuStrip1";
			// 
			// FileMenu
			// 
			this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileOpenMenu,
            this.FileSep0,
            this.FileExitMenu});
			this.FileMenu.Name = "FileMenu";
			this.FileMenu.Size = new System.Drawing.Size(37, 20);
			this.FileMenu.Text = "&File";
			// 
			// FileOpenMenu
			// 
			this.FileOpenMenu.Name = "FileOpenMenu";
			this.FileOpenMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.FileOpenMenu.Size = new System.Drawing.Size(155, 22);
			this.FileOpenMenu.Text = "&Open...";
			this.FileOpenMenu.Click += new System.EventHandler(this.FileOpenMenu_Click);
			// 
			// FileSep0
			// 
			this.FileSep0.Name = "FileSep0";
			this.FileSep0.Size = new System.Drawing.Size(152, 6);
			// 
			// FileExitMenu
			// 
			this.FileExitMenu.Name = "FileExitMenu";
			this.FileExitMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.FileExitMenu.Size = new System.Drawing.Size(155, 22);
			this.FileExitMenu.Text = "E&xit";
			this.FileExitMenu.Click += new System.EventHandler(this.FileExitMenu_Click);
			// 
			// OpenSgcFileDialog
			// 
			this.OpenSgcFileDialog.Filter = "SGC Files (*.sgc)|*.sgc|All Files (*.*)|*.*";
			// 
			// MainInterface
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(442, 205);
			this.Controls.Add(this.MetaDataTable);
			this.Controls.Add(this.SongSelection);
			this.Controls.Add(this.ControlButtonContainer);
			this.Controls.Add(this.Menus);
			this.MainMenuStrip = this.Menus;
			this.Name = "MainInterface";
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainInterface_DragDrop);
			this.DragOver += new System.Windows.Forms.DragEventHandler(this.MainInterface_DragOver);
			this.MetaDataTable.ResumeLayout(false);
			this.MetaDataTable.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SongSelection)).EndInit();
			this.ControlButtonContainer.ResumeLayout(false);
			this.Menus.ResumeLayout(false);
			this.Menus.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel MetaDataTable;
		private System.Windows.Forms.Label CopyrightHeaderLabel;
		private System.Windows.Forms.Label SongNameHeaderLabel;
		private System.Windows.Forms.Label AuthorHeaderLabel;
		private System.Windows.Forms.Label SongNameLabel;
		private System.Windows.Forms.Label AuthorLabel;
		private System.Windows.Forms.Label CopyrightLabel;
		private System.Windows.Forms.TrackBar SongSelection;
		private System.Windows.Forms.Button ButtonPrevious;
		private System.Windows.Forms.TableLayoutPanel ControlButtonContainer;
		private System.Windows.Forms.Button ButtonNext;
		private System.Windows.Forms.Button ButtonPlayPause;
		private System.Windows.Forms.MenuStrip Menus;
		private System.Windows.Forms.ToolStripMenuItem FileMenu;
		private System.Windows.Forms.ToolStripMenuItem FileOpenMenu;
		private System.Windows.Forms.ToolStripSeparator FileSep0;
		private System.Windows.Forms.ToolStripMenuItem FileExitMenu;
		private System.Windows.Forms.OpenFileDialog OpenSgcFileDialog;
	}
}

