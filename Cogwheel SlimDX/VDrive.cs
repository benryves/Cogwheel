using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
			}
		}

		private void InsertDriveButton_Click(object sender, EventArgs e) {
			if (this.InsertDriveFolderBrowser.ShowDialog(this) == DialogResult.OK) {
				this.Drive.DiskPath = null;
				this.Drive.DiskPath = this.InsertDriveFolderBrowser.SelectedPath;
				this.Drive.CurrentDirectory = null;
			}
		}

		private void RemoveDriveButton_Click(object sender, EventArgs e) {
			this.Drive.DiskPath = null;
			this.Drive.CurrentDirectory = null;
		}
	}
}
