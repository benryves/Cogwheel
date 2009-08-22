using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using BeeDevelopment.Zip;
using BeeDevelopment.Sega8Bit.Mappers;

namespace BeeDevelopment.Cogwheel {
	public partial class MainForm {

		#region General State Saving

		/// <summary>
		/// Save the current emulator state to a file.
		/// </summary>
		/// <param name="filename">The name of the file to save to.</param>
		private void SaveState(string filename) {

			var SavedState = new ZipFile();

			BeeDevelopment.Sega8Bit.Utility.SaveState.Save(this.Emulator, SavedState);

			var ScreenshotBitmap = this.GetScreenshotBitmap();
			if (ScreenshotBitmap != null) {
				try {
					using (var ScreenshotStream = new MemoryStream(2048)) {
						ScreenshotBitmap.Save(ScreenshotStream, ImageFormat.Png);
						SavedState.Add(new ZipFileEntry() {
							Name = "Screenshot.png",
							Comment = "A screenshot of the last video frame in PNG format.",
							Data = ScreenshotStream.ToArray(),
							LastWriteTime = DateTime.Now,
						});
					}
				} finally {
					ScreenshotBitmap.Dispose();
				}
			}

			SavedState.Save(filename);
		}

		/// <summary>
		/// Load the current emulator state from a file.
		/// </summary>
		/// <param name="filename">The name of the file to load the state from.</param>
		private void LoadState(string filename) {
			try {
				var StateFile = ZipFile.FromFile(filename);
				BeeDevelopment.Sega8Bit.Emulator NewEmulator;
				BeeDevelopment.Sega8Bit.Utility.SaveState.Load(out NewEmulator, StateFile);
				this.Emulator = NewEmulator;
			} finally {
				this.UpdateFormTitle(null);
			}
		}

		#endregion

		#region Quick-Saving

		/// <summary>Gets or sets the current quick-save slot.</summary>
		private int QuickSaveSlot { get; set; }

		/// <summary>
		/// Gets the filename for a quick-save for a particular slot based on the current emulator state.
		/// </summary>
		/// <param name="slot">The index of the slot to retrieve the filename for.</param>
		/// <returns>The filename for the quick-save.</returns>
		private string GetQuickSaveFilename(int slot) {
			var BaseSavePath = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName), "Cogwheel"), "QuickSaves");
			var RomDirectory = "Default";
			if (this.Emulator.CartridgeSlot.Memory != null) {
				RomDirectory = string.Format("Cartridge.{0:X8}", this.Emulator.CartridgeSlot.Memory.Crc32);
			} else if (this.Emulator.CardSlot.Memory != null) {
				RomDirectory = string.Format("Card.{0:X8}", this.Emulator.CardSlot.Memory.Crc32);
			} else if (this.Emulator.Bios.Memory != null) {
				RomDirectory = string.Format("Bios.{0:X8}", this.Emulator.Bios.Memory.Crc32);
			}
			return Path.Combine(Path.Combine(BaseSavePath, RomDirectory), string.Format("{0}.cogstate", slot));
		}

		/// <summary>
		/// Save the current emulator state to the current quick-save slot.
		/// </summary>
		private void QuickSaveState() {
			string Filename = this.GetQuickSaveFilename(this.QuickSaveSlot);
			try {
				if (!Directory.Exists(Path.GetDirectoryName(Filename))) Directory.CreateDirectory(Path.GetDirectoryName(Filename));
				this.SaveState(Filename);
				this.AddMessage(Properties.Resources.Icon_Disk, string.Format("Saved state to slot {0}", this.QuickSaveSlot));
			} catch (Exception ex) {
				MessageBox.Show(this, "Could not save state: " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Load the current emulator state from the current quick-save slot.
		/// </summary>
		private void QuickLoadState() {
			if (!File.Exists(this.GetQuickSaveFilename(this.QuickSaveSlot))) {
				System.Media.SystemSounds.Beep.Play();
				return;
			}
			try {
				this.LoadState(this.GetQuickSaveFilename(this.QuickSaveSlot));
				this.AddMessage(Properties.Resources.Icon_TimeGo, string.Format("Loaded state from slot {0}", this.QuickSaveSlot));
			} catch (Exception ex) {
				MessageBox.Show(this, "Could not load state: " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion

		#region SRAM Saving

		private string GetSaveRamFilename(bool checkEmpty) {
			
			// Try and convert the cartridge slot memory to the Standard mapper type.
			var StandardCartridge = this.Emulator.CartridgeSlot.Memory as Standard;
			if (StandardCartridge == null) return null;
			
			// Does it have RAM?
			if (StandardCartridge.Ram == null || StandardCartridge.Ram.Length == 0) return null;
			
			// Is anything written to RAM?
			if (checkEmpty) {
				bool AnythingWrittenToRam = false;
				foreach (var b in StandardCartridge.Ram) {
					if (b != 0) {
						AnythingWrittenToRam = true;
						break;
					}
				}
				if (!AnythingWrittenToRam) return null;
			}
			
			// Create the filename:
			var BaseSavePath = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName), "Cogwheel"), "SaveRAM");
			return Path.Combine(BaseSavePath, string.Format("{0:X8}.ram", StandardCartridge.Crc32));
		}

		private void SaveRam() {
			// Get the name of the save file.
			var SaveRamName = this.GetSaveRamFilename(true);
			if (SaveRamName == null) return;

			// Check we have RAM to save.
			var StandardCartridge = this.Emulator.CartridgeSlot.Memory as Standard;
			if (StandardCartridge == null) return;
			if (StandardCartridge.Ram == null || StandardCartridge.Ram.Length == 0) return;

			try {
				// If the save directory doesn't exist, create it.
				if (!Directory.Exists(Path.GetDirectoryName(SaveRamName))) Directory.CreateDirectory(Path.GetDirectoryName(SaveRamName));

				// Save the file.
				using (var F = File.Create(SaveRamName)) {
					F.Write(StandardCartridge.Ram, 0, StandardCartridge.Ram.Length);
				}
			} catch (Exception ex) {
				MessageBox.Show(this, "Could not store the save RAM: " + ex.Message, "Save RAM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void LoadRam() {
			// Get the name of the save file.
			var SaveRamName = this.GetSaveRamFilename(false);
			if (SaveRamName == null) return;
			
			// Are we using a standard cartridge?
			var StandardCartridge = this.Emulator.CartridgeSlot.Memory as Standard;
			if (StandardCartridge == null) return;
			
			// Can we load it?
			if (File.Exists(SaveRamName)) {
				try {
					StandardCartridge.Ram = File.ReadAllBytes(SaveRamName);
				} catch (Exception ex) {
					MessageBox.Show(this, "Could not load the save RAM: " + ex.Message, "Save RAM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

		#endregion
	}
}
