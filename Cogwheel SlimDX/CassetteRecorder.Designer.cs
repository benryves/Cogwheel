
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
			this.CassetteRecorderMenu = new System.Windows.Forms.MenuStrip();
			this.CassetteFileMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.CassetteFileOpenMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.CassetteRecorderToolStrip = new System.Windows.Forms.ToolStripContainer();
			this.OpenCassetteDialog = new System.Windows.Forms.OpenFileDialog();
			this.ProgressData = new System.Windows.Forms.TableLayoutPanel();
			this.TapeProgress = new System.Windows.Forms.TrackBar();
			this.TapeCounterLength = new System.Windows.Forms.Label();
			this.TapeCounterPosition = new System.Windows.Forms.Label();
			this.TransportControls.SuspendLayout();
			this.TapeProgressControls.SuspendLayout();
			this.CassetteRecorderMenu.SuspendLayout();
			this.CassetteRecorderToolStrip.ContentPanel.SuspendLayout();
			this.CassetteRecorderToolStrip.TopToolStripPanel.SuspendLayout();
			this.CassetteRecorderToolStrip.SuspendLayout();
			this.ProgressData.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TapeProgress)).BeginInit();
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
			this.PlayButton.Size = new System.Drawing.Size(93, 38);
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
			this.TransportControls.Size = new System.Drawing.Size(495, 44);
			this.TransportControls.TabIndex = 2;
			// 
			// EjectButton
			// 
			this.EjectButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.EjectButton.AutoCheck = false;
			this.EjectButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EjectButton.Image = global::BeeDevelopment.Cogwheel.Properties.Resources.Icon_ControlEject;
			this.EjectButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.EjectButton.Location = new System.Drawing.Point(399, 3);
			this.EjectButton.Name = "EjectButton";
			this.EjectButton.Size = new System.Drawing.Size(93, 38);
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
			this.StopButton.Location = new System.Drawing.Point(300, 3);
			this.StopButton.Name = "StopButton";
			this.StopButton.Size = new System.Drawing.Size(93, 38);
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
			this.FFwdButton.Location = new System.Drawing.Point(201, 3);
			this.FFwdButton.Name = "FFwdButton";
			this.FFwdButton.Size = new System.Drawing.Size(93, 38);
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
			this.RewindButton.Location = new System.Drawing.Point(102, 3);
			this.RewindButton.Name = "RewindButton";
			this.RewindButton.Size = new System.Drawing.Size(93, 38);
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
			this.TapeProgressControls.Location = new System.Drawing.Point(0, 210);
			this.TapeProgressControls.Name = "TapeProgressControls";
			this.TapeProgressControls.RowCount = 2;
			this.TapeProgressControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TapeProgressControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.TapeProgressControls.Size = new System.Drawing.Size(501, 80);
			this.TapeProgressControls.TabIndex = 3;
			// 
			// CassetteRecorderMenu
			// 
			this.CassetteRecorderMenu.Dock = System.Windows.Forms.DockStyle.None;
			this.CassetteRecorderMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CassetteFileMenu});
			this.CassetteRecorderMenu.Location = new System.Drawing.Point(0, 0);
			this.CassetteRecorderMenu.Name = "CassetteRecorderMenu";
			this.CassetteRecorderMenu.Size = new System.Drawing.Size(501, 24);
			this.CassetteRecorderMenu.TabIndex = 4;
			this.CassetteRecorderMenu.Text = "menuStrip1";
			// 
			// CassetteFileMenu
			// 
			this.CassetteFileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CassetteFileOpenMenu});
			this.CassetteFileMenu.Name = "CassetteFileMenu";
			this.CassetteFileMenu.Size = new System.Drawing.Size(37, 20);
			this.CassetteFileMenu.Text = "&File";
			// 
			// CassetteFileOpenMenu
			// 
			this.CassetteFileOpenMenu.Name = "CassetteFileOpenMenu";
			this.CassetteFileOpenMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.CassetteFileOpenMenu.Size = new System.Drawing.Size(181, 22);
			this.CassetteFileOpenMenu.Text = "&Open tape...";
			this.CassetteFileOpenMenu.Click += new System.EventHandler(this.CassetteFileOpenMenu_Click);
			// 
			// CassetteRecorderToolStrip
			// 
			// 
			// CassetteRecorderToolStrip.ContentPanel
			// 
			this.CassetteRecorderToolStrip.ContentPanel.Controls.Add(this.TapeProgressControls);
			this.CassetteRecorderToolStrip.ContentPanel.Size = new System.Drawing.Size(501, 290);
			this.CassetteRecorderToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CassetteRecorderToolStrip.Location = new System.Drawing.Point(0, 0);
			this.CassetteRecorderToolStrip.Name = "CassetteRecorderToolStrip";
			this.CassetteRecorderToolStrip.Size = new System.Drawing.Size(501, 314);
			this.CassetteRecorderToolStrip.TabIndex = 5;
			this.CassetteRecorderToolStrip.Text = "toolStripContainer1";
			// 
			// CassetteRecorderToolStrip.TopToolStripPanel
			// 
			this.CassetteRecorderToolStrip.TopToolStripPanel.Controls.Add(this.CassetteRecorderMenu);
			// 
			// OpenCassetteDialog
			// 
			this.OpenCassetteDialog.Filter = "Unified Emulator Format (*.uef)|*.uef";
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
			this.ProgressData.Size = new System.Drawing.Size(495, 24);
			this.ProgressData.TabIndex = 4;
			// 
			// TapeProgress
			// 
			this.TapeProgress.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TapeProgress.LargeChange = 2000;
			this.TapeProgress.Location = new System.Drawing.Point(63, 3);
			this.TapeProgress.Maximum = 600000;
			this.TapeProgress.Name = "TapeProgress";
			this.TapeProgress.Size = new System.Drawing.Size(369, 18);
			this.TapeProgress.SmallChange = 250;
			this.TapeProgress.TabIndex = 2;
			this.TapeProgress.TickFrequency = 15000;
			// 
			// TapeCounterLength
			// 
			this.TapeCounterLength.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TapeCounterLength.Location = new System.Drawing.Point(438, 0);
			this.TapeCounterLength.Name = "TapeCounterLength";
			this.TapeCounterLength.Size = new System.Drawing.Size(54, 24);
			this.TapeCounterLength.TabIndex = 3;
			this.TapeCounterLength.Text = "--:--";
			this.TapeCounterLength.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
			// CassetteRecorder
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(501, 314);
			this.Controls.Add(this.CassetteRecorderToolStrip);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MainMenuStrip = this.CassetteRecorderMenu;
			this.Name = "CassetteRecorder";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Cassette Recorder";
			this.TransportControls.ResumeLayout(false);
			this.TapeProgressControls.ResumeLayout(false);
			this.CassetteRecorderMenu.ResumeLayout(false);
			this.CassetteRecorderMenu.PerformLayout();
			this.CassetteRecorderToolStrip.ContentPanel.ResumeLayout(false);
			this.CassetteRecorderToolStrip.TopToolStripPanel.ResumeLayout(false);
			this.CassetteRecorderToolStrip.TopToolStripPanel.PerformLayout();
			this.CassetteRecorderToolStrip.ResumeLayout(false);
			this.CassetteRecorderToolStrip.PerformLayout();
			this.ProgressData.ResumeLayout(false);
			this.ProgressData.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TapeProgress)).EndInit();
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
	}
}