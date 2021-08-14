using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Hardware.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BeeDevelopment.Cogwheel {
	public partial class CassetteRecorder : Form {
		public CassetteRecorder() {
			InitializeComponent();
		}

		private Sega8Bit.Emulator GetEmulator() {
			var EmulatorForm = this.Owner as MainForm;
			if (EmulatorForm == null) return null;
			return EmulatorForm.Emulator;
		}

		private void PlayButton_Click(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if ((emulator != null) && emulator.HasCassetteRecorder) {
				emulator.CassetteRecorder.Play();
			}
		}

		public void Refresh(Emulator emulator) {
			if (!emulator.HasCassetteRecorder) {
				this.Enabled = false;
				return;
			} else {
				this.Enabled = true;
				
				var cassetteRecorder = emulator.CassetteRecorder;

				if (cassetteRecorder.Tape == null) {
					this.TapeProgressControls.Enabled = false;
					this.TapeProgress.Value = 0;
					this.TapeProgress.Maximum = 600000;
					this.TapeCounterLength.Text = this.TapeCounterPosition.Text = "--:--";

					this.PlayButton.Checked = this.StopButton.Checked = this.RewindButton.Checked = this.FFwdButton.Checked = false;

				} else {
					this.TapeProgressControls.Enabled = true;

					var tapePosition = cassetteRecorder.TapePosition;
					var tapeLength = cassetteRecorder.TapeLength;

					var value = (int)tapePosition.TotalMilliseconds;
					var maximum = (int)tapeLength.TotalMilliseconds;
					if (value > maximum) value = maximum; // <- just in case!

					if (value > this.TapeProgress.Maximum) {
						this.TapeProgress.Maximum = maximum;
						this.TapeProgress.Value = value;
					} else {
						this.TapeProgress.Value = value;
						this.TapeProgress.Maximum = maximum;
					}

					this.PlayButton.Checked = cassetteRecorder.PlayState == CassetteRecorderPlayState.Playing;
					this.StopButton.Checked = cassetteRecorder.PlayState == CassetteRecorderPlayState.Stopped;
					this.RewindButton.Checked = cassetteRecorder.PlayState == CassetteRecorderPlayState.Rewinding;
					this.FFwdButton.Checked = cassetteRecorder.PlayState == CassetteRecorderPlayState.FastForwarding;

					this.TapeCounterLength.Text = string.Format("{0}:{1:00}", tapeLength.Minutes, tapeLength.Seconds);
					this.TapeCounterPosition.Text = string.Format("{0}:{1:00}", tapePosition.Minutes, tapePosition.Seconds);
				}
			}
		}

		private void CassetteFileOpenMenu_Click(object sender, EventArgs e) {
			
			var emulator = this.GetEmulator();
			if (emulator == null) return;
			if (!emulator.HasCassetteRecorder) return;

			if (this.OpenCassetteDialog.ShowDialog(this) == DialogResult.OK) {
				try {
					emulator.CassetteRecorder.Stop();
					emulator.CassetteRecorder.Tape = UnifiedEmulatorFormat.FromFile(this.OpenCassetteDialog.FileName);
				} catch (Exception ex) {
					MessageBox.Show(this, "There was an error loading the UEF: " + ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
					
				}
			}
		}

		private void EjectButton_Click(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (emulator == null) return;
			if (!emulator.HasCassetteRecorder) return;
			emulator.CassetteRecorder.Stop();
			emulator.CassetteRecorder.Tape = null;
		}

		private void StopButton_Click(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (emulator == null) return;
			if (!emulator.HasCassetteRecorder) return;
			emulator.CassetteRecorder.Stop();
		}

		private void RewindButton_CheckedChanged(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (emulator == null) return;
			if (!emulator.HasCassetteRecorder) return;
			emulator.CassetteRecorder.Rewind();
		}

		private void FFwdButton_CheckedChanged(object sender, EventArgs e) {
			var emulator = this.GetEmulator();
			if (emulator == null) return;
			if (!emulator.HasCassetteRecorder) return;
			emulator.CassetteRecorder.FastForward();
		}
	}
}
