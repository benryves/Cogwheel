using BeeDevelopment.Zip;
using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Emulation;
using System.IO;
using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace BeeDevelopment.Cogwheel {
	partial class MainForm {

		/// <summary>
		/// Prompt to open a ROM file, and load it if applicable.
		/// </summary>
		private void PromptOpenRom() {
			if (this.OpenRomDialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK) {
				byte[] Data;
				MachineType Machine;
				try {
					Machine = this.LoadRom(this.OpenRomDialog.FileName, out Data);
				} catch (Exception ex) {
					MessageBox.Show(this, "Error loading ROM file: " + Environment.NewLine + ex.Message, "Open ROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}
		}

		/// <summary>
		/// Load a ROM from a file into an array of bytes.
		/// </summary>
		/// <param name="filename">The name of the file. Can be a zip archive.</param>
		/// <param name="data">The data contained in the ROM file.</param>
		/// <returns>The <see cref="MachineType"/> (derived from the file extension).</returns>
		private MachineType LoadRom(string filename, out byte[] data) {

			// Try and load from a zip file (if applicable).
			try {
				if (Path.GetExtension(filename).ToLowerInvariant() == ".zip") {
					var CompressedRom = ZipFile.FromFile(filename);
					foreach (var ZippedFile in CompressedRom) {
						switch (Path.GetExtension(ZippedFile.Name).ToLowerInvariant()) {
							case ".sms":
								data = ZippedFile.Data;
								return MachineType.MasterSystem2;
							case ".gg":
								data = ZippedFile.Data;
								return MachineType.GameGear;
							case ".sg":
								data = ZippedFile.Data;
								return MachineType.Sg1000;
							case ".sc":
								data = ZippedFile.Data;
								return MachineType.Sc3000;
						}
					}
				}
			} catch { }

			// Just read the data and old how.
			data = File.ReadAllBytes(filename);
			switch (Path.GetExtension(filename).ToLowerInvariant()) {
				case ".sms": return MachineType.MasterSystem2;
				case ".gg": return MachineType.GameGear;
				case ".sg": return MachineType.Sg1000;
				case ".sc": return MachineType.Sc3000;
				default: return MachineType.MasterSystem2;
			}

		}
	}
}
