using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using BeeDevelopment.Zip;

namespace BeeDevelopment.Cogwheel {
	public partial class MainForm {

		/// <summary>Gets or sets the current quick-save slot.</summary>
		private int QuickSaveSlot { get; set; }

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
	}
}
