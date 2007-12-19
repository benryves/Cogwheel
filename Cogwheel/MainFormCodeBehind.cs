using BeeDevelopment.Zip;
using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Emulation;
using System.Collections.Generic;
using System.IO;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using BeeDevelopment.Sega8Bit.Devices.Input;
using BeeDevelopment.Sega8Bit.Utility.RomData;

namespace BeeDevelopment.Cogwheel {

	class KeyTarget {
		public int PlayerIndex { get; set; }
		public Joypad.Pins Pin { get; set; }
	}

	partial class MainForm {


	

		/// <summary>
		/// Prompt to open a ROM file, and load it if applicable.
		/// </summary>
		private void PromptOpenRom() {
			try {
				if (this.OpenRomDialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK) {
					byte[] Data;
					MachineType Machine;
					try {
						Machine = this.LoadRom(this.OpenRomDialog.FileName, out Data);
					} catch (Exception ex) {
						MessageBox.Show(this, "Error loading ROM file: " + Environment.NewLine + ex.Message, "Open ROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					RomData RomInfo;
					if (this.Identifier.TryGetRomData(Data, out RomInfo)) {
						RomInfo.Fix(ref Data);
						this.Text = RomInfo.Name + " - " + Application.ProductName;
					} else {
						this.Text = Application.ProductName;
					}

							
					this.EmulatorHost.Emulator = new Emulator();
					this.EmulatorHost.Emulator.Mapper = this.Identifier.GuessMapper(Data);
					GC.Collect();
					this.EmulatorHost.Emulator.Machine = Machine;
					this.EmulatorHost.Emulator.LoadCartridge(new MemoryStream(Data));
					

					this.EmulatorHost.Emulator.ControllerPortA = (this.EmulatorHost.PlayerA = new Joypad(this.EmulatorHost.Emulator));
					this.EmulatorHost.Emulator.ControllerPortB = (this.EmulatorHost.PlayerB = new Joypad(this.EmulatorHost.Emulator));

					this.EmulatorHost.Emulator.IsJapanese = Properties.Settings.Default.EmulatorIsJapanese;
				}
			} finally {
				this.LastRun = DateTime.Now;
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


		private DateTime LastRun = DateTime.Now;

		private void RunEmulatorTicks() {
			var Now = DateTime.Now;

			if (this.EmulatorHost.Emulator != null) {
				
				var TimeToRun = Now - LastRun;
				var TimePerFrame = TimeSpan.FromSeconds(1d / 60d);

				int FramesToRun = (int)Math.Floor(TimeToRun.TotalSeconds / TimePerFrame.TotalSeconds);

				if (TimeToRun < TimePerFrame || FramesToRun == 0) {
					this.Invalidate();
					Thread.Sleep(1);
				} else {
					for (int i = 0; i < FramesToRun; ++i) {
						this.EmulatorHost.Emulator.RunFrame();
						LastRun += TimePerFrame;
					}
				}				
			} else {
				LastRun = Now;
			}
		}
	}
}
