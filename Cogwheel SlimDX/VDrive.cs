using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BeeDevelopment.Cogwheel {
	public partial class VDrive : Form {
		public VDrive() {
			InitializeComponent();
		}

		Sega8Bit.Hardware.Controllers.VDrive.VDrive Drive;


		private Sega8Bit.Emulator GetEmulator() {
			var EmulatorForm = this.Owner as MainForm;
			if (EmulatorForm == null) return null;
			return EmulatorForm.Emulator;
		}

		private void VDrive_Load(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (emulator == null || !emulator.HasSerialPort) {
				this.Close();
			} else {
				if (emulator.HasCassetteRecorder) emulator.CassetteRecorder.ConnectedToEmulator = false;
				emulator.SerialPort.ConnectedToEmulator = true;
				this.Drive = new Sega8Bit.Hardware.Controllers.VDrive.VDrive(emulator);
				this.UpdateStatusLabels();
			}
		}

		private void UpdateStatusLabels() {
			this.InsertDriveButton.Enabled = !this.Drive.HasDisk;
			this.RemoveDriveButton.Enabled = this.Drive.HasDisk;
			this.DriveStatusLabel.Text = this.Drive.DiskPath;
			if (string.IsNullOrEmpty(this.DriveStatusLabel.Text)) {
				this.DriveStatusLabel.ToolTipText = null;
			} else {
				this.DriveStatusLabel.ToolTipText = this.DriveStatusLabel.Text;
			}
		}

		public void InsertDrive(string path) {
			this.Drive.DiskPath = null;
			this.Drive.DiskPath = path;
			this.Drive.CurrentDirectory = null;
			this.UpdateStatusLabels();
		}

		public void RemoveDrive() {
			this.Drive.DiskPath = null;
			this.Drive.CurrentDirectory = null;
			this.UpdateStatusLabels();
		}

		private void InsertDriveButton_Click(object sender, EventArgs e) {
			var oldPath = Properties.Settings.Default.OptionVDrivePath;
			if (!string.IsNullOrEmpty(oldPath) && Directory.Exists(oldPath)) {
				this.InsertDriveFolderBrowser.SelectedPath = oldPath;
			}
			if (this.InsertDriveFolderBrowser.ShowDialog(this) == DialogResult.OK) {
				this.InsertDrive(this.InsertDriveFolderBrowser.SelectedPath);
				Properties.Settings.Default.OptionVDrivePath = this.Drive.DiskPath;
			}
		}

		private void RemoveDriveButton_Click(object sender, EventArgs e) {
			this.RemoveDrive();
		}
	}
}
