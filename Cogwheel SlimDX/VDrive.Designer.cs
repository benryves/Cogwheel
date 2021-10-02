
namespace BeeDevelopment.Cogwheel {
	partial class VDrive {
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
			this.InsertDriveButton = new System.Windows.Forms.Button();
			this.InsertDriveFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
			this.RemoveDriveButton = new System.Windows.Forms.Button();
			this.VDriveStatus = new System.Windows.Forms.StatusStrip();
			this.VDriveToolStrip = new System.Windows.Forms.ToolStripContainer();
			this.DriveButtons = new System.Windows.Forms.TableLayoutPanel();
			this.DriveStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.VDriveStatus.SuspendLayout();
			this.VDriveToolStrip.BottomToolStripPanel.SuspendLayout();
			this.VDriveToolStrip.ContentPanel.SuspendLayout();
			this.VDriveToolStrip.SuspendLayout();
			this.DriveButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// InsertDriveButton
			// 
			this.InsertDriveButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InsertDriveButton.Location = new System.Drawing.Point(3, 3);
			this.InsertDriveButton.Name = "InsertDriveButton";
			this.InsertDriveButton.Size = new System.Drawing.Size(399, 77);
			this.InsertDriveButton.TabIndex = 0;
			this.InsertDriveButton.Text = "&Insert Drive...";
			this.InsertDriveButton.UseVisualStyleBackColor = true;
			this.InsertDriveButton.Click += new System.EventHandler(this.InsertDriveButton_Click);
			// 
			// InsertDriveFolderBrowser
			// 
			this.InsertDriveFolderBrowser.Description = "Please select a folder to use as the emulated USB drive\'s file storage area.";
			// 
			// RemoveDriveButton
			// 
			this.RemoveDriveButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RemoveDriveButton.Location = new System.Drawing.Point(3, 86);
			this.RemoveDriveButton.Name = "RemoveDriveButton";
			this.RemoveDriveButton.Size = new System.Drawing.Size(399, 24);
			this.RemoveDriveButton.TabIndex = 1;
			this.RemoveDriveButton.Text = "&Remove Drive";
			this.RemoveDriveButton.UseVisualStyleBackColor = true;
			this.RemoveDriveButton.Click += new System.EventHandler(this.RemoveDriveButton_Click);
			// 
			// VDriveStatus
			// 
			this.VDriveStatus.Dock = System.Windows.Forms.DockStyle.None;
			this.VDriveStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DriveStatusLabel});
			this.VDriveStatus.Location = new System.Drawing.Point(0, 0);
			this.VDriveStatus.Name = "VDriveStatus";
			this.VDriveStatus.Size = new System.Drawing.Size(405, 22);
			this.VDriveStatus.TabIndex = 2;
			this.VDriveStatus.Text = "statusStrip1";
			// 
			// VDriveToolStrip
			// 
			// 
			// VDriveToolStrip.BottomToolStripPanel
			// 
			this.VDriveToolStrip.BottomToolStripPanel.Controls.Add(this.VDriveStatus);
			// 
			// VDriveToolStrip.ContentPanel
			// 
			this.VDriveToolStrip.ContentPanel.Controls.Add(this.DriveButtons);
			this.VDriveToolStrip.ContentPanel.Size = new System.Drawing.Size(405, 113);
			this.VDriveToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VDriveToolStrip.Location = new System.Drawing.Point(0, 0);
			this.VDriveToolStrip.Name = "VDriveToolStrip";
			this.VDriveToolStrip.Size = new System.Drawing.Size(405, 135);
			this.VDriveToolStrip.TabIndex = 3;
			this.VDriveToolStrip.Text = "toolStripContainer1";
			// 
			// DriveButtons
			// 
			this.DriveButtons.ColumnCount = 1;
			this.DriveButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.DriveButtons.Controls.Add(this.InsertDriveButton, 0, 0);
			this.DriveButtons.Controls.Add(this.RemoveDriveButton, 0, 1);
			this.DriveButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DriveButtons.Location = new System.Drawing.Point(0, 0);
			this.DriveButtons.Name = "DriveButtons";
			this.DriveButtons.RowCount = 2;
			this.DriveButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.DriveButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.DriveButtons.Size = new System.Drawing.Size(405, 113);
			this.DriveButtons.TabIndex = 0;
			// 
			// DriveStatusLabel
			// 
			this.DriveStatusLabel.AutoToolTip = true;
			this.DriveStatusLabel.Name = "DriveStatusLabel";
			this.DriveStatusLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.DriveStatusLabel.Size = new System.Drawing.Size(359, 17);
			this.DriveStatusLabel.Spring = true;
			this.DriveStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// VDrive
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(405, 135);
			this.Controls.Add(this.VDriveToolStrip);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "VDrive";
			this.ShowInTaskbar = false;
			this.Text = "VDrive";
			this.Load += new System.EventHandler(this.VDrive_Load);
			this.VDriveStatus.ResumeLayout(false);
			this.VDriveStatus.PerformLayout();
			this.VDriveToolStrip.BottomToolStripPanel.ResumeLayout(false);
			this.VDriveToolStrip.BottomToolStripPanel.PerformLayout();
			this.VDriveToolStrip.ContentPanel.ResumeLayout(false);
			this.VDriveToolStrip.ResumeLayout(false);
			this.VDriveToolStrip.PerformLayout();
			this.DriveButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button InsertDriveButton;
		private System.Windows.Forms.FolderBrowserDialog InsertDriveFolderBrowser;
		private System.Windows.Forms.Button RemoveDriveButton;
		private System.Windows.Forms.StatusStrip VDriveStatus;
		private System.Windows.Forms.ToolStripStatusLabel DriveStatusLabel;
		private System.Windows.Forms.ToolStripContainer VDriveToolStrip;
		private System.Windows.Forms.TableLayoutPanel DriveButtons;
	}
}