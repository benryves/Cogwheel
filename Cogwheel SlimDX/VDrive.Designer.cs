
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
			this.SuspendLayout();
			// 
			// InsertDriveButton
			// 
			this.InsertDriveButton.Location = new System.Drawing.Point(12, 12);
			this.InsertDriveButton.Name = "InsertDriveButton";
			this.InsertDriveButton.Size = new System.Drawing.Size(161, 58);
			this.InsertDriveButton.TabIndex = 0;
			this.InsertDriveButton.Text = "&Insert Drive...";
			this.InsertDriveButton.UseVisualStyleBackColor = true;
			this.InsertDriveButton.Click += new System.EventHandler(this.InsertDriveButton_Click);
			// 
			// InsertDriveFolderBrowser
			// 
			this.InsertDriveFolderBrowser.Description = "Please select a folder to use as the emulated USB drive\'s file storage area.";
			// 
			// VDrive
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(368, 172);
			this.Controls.Add(this.InsertDriveButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "VDrive";
			this.ShowInTaskbar = false;
			this.Text = "VDrive";
			this.Load += new System.EventHandler(this.VDrive_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button InsertDriveButton;
		private System.Windows.Forms.FolderBrowserDialog InsertDriveFolderBrowser;
	}
}