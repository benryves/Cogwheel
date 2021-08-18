
namespace BeeDevelopment.Cogwheel {
	partial class CassetteRecorder {
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
			this.PlayButton = new System.Windows.Forms.CheckBox();
			this.TransportControls = new System.Windows.Forms.TableLayoutPanel();
			this.EjectButton = new System.Windows.Forms.CheckBox();
			this.StopButton = new System.Windows.Forms.CheckBox();
			this.FFwdButton = new System.Windows.Forms.CheckBox();
			this.RewindButton = new System.Windows.Forms.CheckBox();
			this.TapeProgressControls = new System.Windows.Forms.TableLayoutPanel();
			this.ProgressData = new System.Windows.Forms.TableLayoutPanel();
			this.TapeCounterLength = new System.Windows.Forms.Label();
			this.TapeProgress = new System.Windows.Forms.TrackBar();
			this.TapeCounterPosition = new System.Windows.Forms.Label();
			this.CassetteRecorderMenu = new System.Windows.Forms.MenuStrip();
			this.CassetteFileMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.CassetteFileOpenMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.CassetteOptionsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.CassetteInvertPhaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CassetteRecorderToolStrip = new System.Windows.Forms.ToolStripContainer();
			this.BlockList = new System.Windows.Forms.ListView();
			this.BlockListTimeHeading = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.BlockListNameHeading = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.BlockListNumberHeading = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.BlockListLengthHeading = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.OpenCassetteDialog = new System.Windows.Forms.OpenFileDialog();
			this.CassetteFileSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.CassetteFileExport = new System.Windows.Forms.ToolStripMenuItem();
			this.ExportFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.ExportFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.TransportControls.SuspendLayout();
			this.TapeProgressControls.SuspendLayout();
			this.ProgressData.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TapeProgress)).BeginInit();
			this.CassetteRecorderMenu.SuspendLayout();
			this.CassetteRecorderToolStrip.ContentPanel.SuspendLayout();
			this.CassetteRecorderToolStrip.TopToolStripPanel.SuspendLayout();
			this.CassetteRecorderToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// PlayButton
			// 
			this.PlayButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.PlayButton.AutoCheck = false;
			this.PlayButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PlayButton.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_ControlPlay;
			this.PlayButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.PlayButton.Location = new System.Drawing.Point(3, 3);
			this.PlayButton.Name = "PlayButton";
			this.PlayButton.Size = new System.Drawing.Size(96, 38);
			this.PlayButton.TabIndex = 0;
			this.PlayButton.Text = "&Play";
			this.PlayButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.PlayButton.UseVisualStyleBackColor = true;
			this.PlayButton.Click += new System.EventHandler(this.PlayButton_Click);
			// 
			// TransportControls
			// 
			this.TransportControls.ColumnCount = 5;
			this.TransportControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.TransportControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.TransportControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.TransportControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.TransportControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.TransportControls.Controls.Add(this.EjectButton, 4, 0);
			this.TransportControls.Controls.Add(this.StopButton, 3, 0);
			this.TransportControls.Controls.Add(this.FFwdButton, 2, 0);
			this.TransportControls.Controls.Add(this.RewindButton, 1, 0);
			this.TransportControls.Controls.Add(this.PlayButton, 0, 0);
			this.TransportControls.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportControls.Location = new System.Drawing.Point(3, 33);
			this.TransportControls.Name = "TransportControls";
			this.TransportControls.RowCount = 1;
			this.TransportControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TransportControls.Size = new System.Drawing.Size(512, 44);
			this.TransportControls.TabIndex = 2;
			// 
			// EjectButton
			// 
			this.EjectButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.EjectButton.AutoCheck = false;
			this.EjectButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EjectButton.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_ControlEject;
			this.EjectButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.EjectButton.Location = new System.Drawing.Point(411, 3);
			this.EjectButton.Name = "EjectButton";
			this.EjectButton.Size = new System.Drawing.Size(98, 38);
			this.EjectButton.TabIndex = 4;
			this.EjectButton.Text = "&Eject";
			this.EjectButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.EjectButton.UseVisualStyleBackColor = true;
			this.EjectButton.Click += new System.EventHandler(this.EjectButton_Click);
			// 
			// StopButton
			// 
			this.StopButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.StopButton.AutoCheck = false;
			this.StopButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StopButton.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_ControlStop;
			this.StopButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.StopButton.Location = new System.Drawing.Point(309, 3);
			this.StopButton.Name = "StopButton";
			this.StopButton.Size = new System.Drawing.Size(96, 38);
			this.StopButton.TabIndex = 3;
			this.StopButton.Text = "&Stop";
			this.StopButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.StopButton.UseVisualStyleBackColor = true;
			this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
			// 
			// FFwdButton
			// 
			this.FFwdButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.FFwdButton.AutoCheck = false;
			this.FFwdButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FFwdButton.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_ControlFastForward;
			this.FFwdButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.FFwdButton.Location = new System.Drawing.Point(207, 3);
			this.FFwdButton.Name = "FFwdButton";
			this.FFwdButton.Size = new System.Drawing.Size(96, 38);
			this.FFwdButton.TabIndex = 2;
			this.FFwdButton.Text = "&F.Fwd";
			this.FFwdButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.FFwdButton.UseVisualStyleBackColor = true;
			this.FFwdButton.Click += new System.EventHandler(this.FFwdButton_CheckedChanged);
			// 
			// RewindButton
			// 
			this.RewindButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.RewindButton.AutoCheck = false;
			this.RewindButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RewindButton.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_ControlRewind;
			this.RewindButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.RewindButton.Location = new System.Drawing.Point(105, 3);
			this.RewindButton.Name = "RewindButton";
			this.RewindButton.Size = new System.Drawing.Size(96, 38);
			this.RewindButton.TabIndex = 1;
			this.RewindButton.Text = "&Rewind";
			this.RewindButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.RewindButton.UseVisualStyleBackColor = true;
			this.RewindButton.Click += new System.EventHandler(this.RewindButton_CheckedChanged);
			// 
			// TapeProgressControls
			// 
			this.TapeProgressControls.ColumnCount = 1;
			this.TapeProgressControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TapeProgressControls.Controls.Add(this.ProgressData, 0, 0);
			this.TapeProgressControls.Controls.Add(this.TransportControls, 0, 1);
			this.TapeProgressControls.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TapeProgressControls.Location = new System.Drawing.Point(0, 324);
			this.TapeProgressControls.Name = "TapeProgressControls";
			this.TapeProgressControls.RowCount = 2;
			this.TapeProgressControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TapeProgressControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.TapeProgressControls.Size = new System.Drawing.Size(518, 80);
			this.TapeProgressControls.TabIndex = 3;
			// 
			// ProgressData
			// 
			this.ProgressData.ColumnCount = 3;
			this.ProgressData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.ProgressData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ProgressData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.ProgressData.Controls.Add(this.TapeCounterLength, 2, 0);
			this.ProgressData.Controls.Add(this.TapeProgress, 1, 0);
			this.ProgressData.Controls.Add(this.TapeCounterPosition, 0, 0);
			this.ProgressData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgressData.Location = new System.Drawing.Point(3, 3);
			this.ProgressData.Name = "ProgressData";
			this.ProgressData.RowCount = 1;
			this.ProgressData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ProgressData.Size = new System.Drawing.Size(512, 24);
			this.ProgressData.TabIndex = 4;
			// 
			// TapeCounterLength
			// 
			this.TapeCounterLength.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TapeCounterLength.Location = new System.Drawing.Point(455, 0);
			this.TapeCounterLength.Name = "TapeCounterLength";
			this.TapeCounterLength.Size = new System.Drawing.Size(54, 24);
			this.TapeCounterLength.TabIndex = 3;
			this.TapeCounterLength.Text = "--:--";
			this.TapeCounterLength.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TapeProgress
			// 
			this.TapeProgress.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TapeProgress.LargeChange = 2000;
			this.TapeProgress.Location = new System.Drawing.Point(63, 3);
			this.TapeProgress.Maximum = 600000;
			this.TapeProgress.Name = "TapeProgress";
			this.TapeProgress.Size = new System.Drawing.Size(386, 18);
			this.TapeProgress.SmallChange = 250;
			this.TapeProgress.TabIndex = 2;
			this.TapeProgress.TickFrequency = 15000;
			// 
			// TapeCounterPosition
			// 
			this.TapeCounterPosition.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TapeCounterPosition.Location = new System.Drawing.Point(3, 0);
			this.TapeCounterPosition.Name = "TapeCounterPosition";
			this.TapeCounterPosition.Size = new System.Drawing.Size(54, 24);
			this.TapeCounterPosition.TabIndex = 4;
			this.TapeCounterPosition.Text = "--:--";
			this.TapeCounterPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CassetteRecorderMenu
			// 
			this.CassetteRecorderMenu.Dock = System.Windows.Forms.DockStyle.None;
			this.CassetteRecorderMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CassetteFileMenu,
            this.CassetteOptionsMenu});
			this.CassetteRecorderMenu.Location = new System.Drawing.Point(0, 0);
			this.CassetteRecorderMenu.Name = "CassetteRecorderMenu";
			this.CassetteRecorderMenu.Size = new System.Drawing.Size(518, 24);
			this.CassetteRecorderMenu.TabIndex = 4;
			this.CassetteRecorderMenu.Text = "menuStrip1";
			// 
			// CassetteFileMenu
			// 
			this.CassetteFileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CassetteFileOpenMenu,
            this.CassetteFileSep0,
            this.CassetteFileExport});
			this.CassetteFileMenu.Name = "CassetteFileMenu";
			this.CassetteFileMenu.Size = new System.Drawing.Size(37, 20);
			this.CassetteFileMenu.Text = "&File";
			this.CassetteFileMenu.DropDownOpening += new System.EventHandler(this.CassetteFileMenu_DropDownOpening);
			// 
			// CassetteFileOpenMenu
			// 
			this.CassetteFileOpenMenu.Name = "CassetteFileOpenMenu";
			this.CassetteFileOpenMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.CassetteFileOpenMenu.Size = new System.Drawing.Size(187, 22);
			this.CassetteFileOpenMenu.Text = "&Open tape...";
			this.CassetteFileOpenMenu.Click += new System.EventHandler(this.CassetteFileOpenMenu_Click);
			// 
			// CassetteOptionsMenu
			// 
			this.CassetteOptionsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CassetteInvertPhaseMenuItem});
			this.CassetteOptionsMenu.Name = "CassetteOptionsMenu";
			this.CassetteOptionsMenu.Size = new System.Drawing.Size(61, 20);
			this.CassetteOptionsMenu.Text = "&Options";
			this.CassetteOptionsMenu.DropDownOpening += new System.EventHandler(this.CassetteOptionsMenu_DropDownOpening);
			// 
			// CassetteInvertPhaseMenuItem
			// 
			this.CassetteInvertPhaseMenuItem.Name = "CassetteInvertPhaseMenuItem";
			this.CassetteInvertPhaseMenuItem.Size = new System.Drawing.Size(180, 22);
			this.CassetteInvertPhaseMenuItem.Text = "&Invert phase 180°";
			this.CassetteInvertPhaseMenuItem.Click += new System.EventHandler(this.CassetteInvertPhaseMenuItem_Click);
			// 
			// CassetteRecorderToolStrip
			// 
			// 
			// CassetteRecorderToolStrip.ContentPanel
			// 
			this.CassetteRecorderToolStrip.ContentPanel.Controls.Add(this.BlockList);
			this.CassetteRecorderToolStrip.ContentPanel.Controls.Add(this.TapeProgressControls);
			this.CassetteRecorderToolStrip.ContentPanel.Size = new System.Drawing.Size(518, 404);
			this.CassetteRecorderToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CassetteRecorderToolStrip.Location = new System.Drawing.Point(0, 0);
			this.CassetteRecorderToolStrip.Name = "CassetteRecorderToolStrip";
			this.CassetteRecorderToolStrip.Size = new System.Drawing.Size(518, 428);
			this.CassetteRecorderToolStrip.TabIndex = 5;
			this.CassetteRecorderToolStrip.Text = "toolStripContainer1";
			// 
			// CassetteRecorderToolStrip.TopToolStripPanel
			// 
			this.CassetteRecorderToolStrip.TopToolStripPanel.Controls.Add(this.CassetteRecorderMenu);
			// 
			// BlockList
			// 
			this.BlockList.AutoArrange = false;
			this.BlockList.CheckBoxes = true;
			this.BlockList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.BlockListTimeHeading,
            this.BlockListNameHeading,
            this.BlockListNumberHeading,
            this.BlockListLengthHeading});
			this.BlockList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BlockList.FullRowSelect = true;
			this.BlockList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.BlockList.HideSelection = false;
			this.BlockList.Location = new System.Drawing.Point(0, 0);
			this.BlockList.Name = "BlockList";
			this.BlockList.Size = new System.Drawing.Size(518, 324);
			this.BlockList.TabIndex = 4;
			this.BlockList.UseCompatibleStateImageBehavior = false;
			this.BlockList.View = System.Windows.Forms.View.Details;
			this.BlockList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.BlockList_ItemChecked);
			// 
			// BlockListTimeHeading
			// 
			this.BlockListTimeHeading.Text = "Time";
			this.BlockListTimeHeading.Width = 89;
			// 
			// BlockListNameHeading
			// 
			this.BlockListNameHeading.Text = "Name";
			this.BlockListNameHeading.Width = 230;
			// 
			// BlockListNumberHeading
			// 
			this.BlockListNumberHeading.Text = "Number";
			this.BlockListNumberHeading.Width = 76;
			// 
			// BlockListLengthHeading
			// 
			this.BlockListLengthHeading.Text = "Size";
			this.BlockListLengthHeading.Width = 69;
			// 
			// OpenCassetteDialog
			// 
			this.OpenCassetteDialog.Filter = "Unified Emulator Format (*.uef)|*.uef";
			// 
			// CassetteFileSep0
			// 
			this.CassetteFileSep0.Name = "CassetteFileSep0";
			this.CassetteFileSep0.Size = new System.Drawing.Size(184, 6);
			// 
			// CassetteFileExport
			// 
			this.CassetteFileExport.Name = "CassetteFileExport";
			this.CassetteFileExport.Size = new System.Drawing.Size(187, 22);
			this.CassetteFileExport.Text = "&Export selected files...";
			this.CassetteFileExport.Click += new System.EventHandler(this.CassetteFileExport_Click);
			// 
			// ExportFileDialog
			// 
			this.ExportFileDialog.Filter = "All files (*.*)|*.*";
			// 
			// CassetteRecorder
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(518, 428);
			this.Controls.Add(this.CassetteRecorderToolStrip);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MainMenuStrip = this.CassetteRecorderMenu;
			this.Name = "CassetteRecorder";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Cassette Recorder";
			this.TransportControls.ResumeLayout(false);
			this.TapeProgressControls.ResumeLayout(false);
			this.ProgressData.ResumeLayout(false);
			this.ProgressData.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TapeProgress)).EndInit();
			this.CassetteRecorderMenu.ResumeLayout(false);
			this.CassetteRecorderMenu.PerformLayout();
			this.CassetteRecorderToolStrip.ContentPanel.ResumeLayout(false);
			this.CassetteRecorderToolStrip.TopToolStripPanel.ResumeLayout(false);
			this.CassetteRecorderToolStrip.TopToolStripPanel.PerformLayout();
			this.CassetteRecorderToolStrip.ResumeLayout(false);
			this.CassetteRecorderToolStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.CheckBox PlayButton;
		private System.Windows.Forms.TableLayoutPanel TransportControls;
		private System.Windows.Forms.CheckBox EjectButton;
		private System.Windows.Forms.CheckBox StopButton;
		private System.Windows.Forms.CheckBox FFwdButton;
		private System.Windows.Forms.CheckBox RewindButton;
		private System.Windows.Forms.TableLayoutPanel TapeProgressControls;
		private System.Windows.Forms.MenuStrip CassetteRecorderMenu;
		private System.Windows.Forms.ToolStripMenuItem CassetteFileMenu;
		private System.Windows.Forms.ToolStripMenuItem CassetteFileOpenMenu;
		private System.Windows.Forms.ToolStripContainer CassetteRecorderToolStrip;
		private System.Windows.Forms.OpenFileDialog OpenCassetteDialog;
		private System.Windows.Forms.TableLayoutPanel ProgressData;
		private System.Windows.Forms.Label TapeCounterLength;
		private System.Windows.Forms.TrackBar TapeProgress;
		private System.Windows.Forms.Label TapeCounterPosition;
		private System.Windows.Forms.ListView BlockList;
		private System.Windows.Forms.ColumnHeader BlockListTimeHeading;
		private System.Windows.Forms.ColumnHeader BlockListNameHeading;
		private System.Windows.Forms.ColumnHeader BlockListNumberHeading;
		private System.Windows.Forms.ColumnHeader BlockListLengthHeading;
		private System.Windows.Forms.ToolStripMenuItem CassetteOptionsMenu;
		private System.Windows.Forms.ToolStripMenuItem CassetteInvertPhaseMenuItem;
		private System.Windows.Forms.ToolStripSeparator CassetteFileSep0;
		private System.Windows.Forms.ToolStripMenuItem CassetteFileExport;
		private System.Windows.Forms.FolderBrowserDialog ExportFolderDialog;
		private System.Windows.Forms.SaveFileDialog ExportFileDialog;
	}
}