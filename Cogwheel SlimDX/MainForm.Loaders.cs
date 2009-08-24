using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;
using BeeDevelopment.Sega8Bit;
using BeeDevelopment.Sega8Bit.Utility;

namespace BeeDevelopment.Cogwheel {
	public partial class MainForm {

		/// <summary>
		///  Display a warning dialog if the ColecoVision BIOS ROM is not present.
		/// </summary>
		private void WarnAboutColecoRom() {
			if (this.Emulator.Family == HardwareFamily.ColecoVision && !File.Exists(Path.Combine(Application.StartupPath, "COLECO.ROM"))) {
				MessageBox.Show(this, "ColecoVision emulation requires a copy of the ColecoVision BIOS ROM." + Environment.NewLine +  "Please copy COLECO.ROM to the application's installation directory.", "ColecoVision", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		/// <summary>
		/// Override any automatic settings (eg region) with user-defined ones.
		/// </summary>
		private void OverrideAutomaticSettings(RomInfo romInfo) {
			if (!Properties.Settings.Default.OptionRegionAutomatic) {
				this.Emulator.Region = Properties.Settings.Default.OptionRegionJapanese ? BeeDevelopment.Sega8Bit.Region.Japanese : BeeDevelopment.Sega8Bit.Region.Export;
			}
			if (!Properties.Settings.Default.OptionVideoStandardAutomatic) {
				this.Emulator.Video.System = Properties.Settings.Default.OptionVideoStandardNtsc ? BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Ntsc : BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Pal;
			}
			if (romInfo != null) {
				if (romInfo.Model == HardwareModel.GameGearMasterSystem && !Properties.Settings.Default.OptionSimulateGameGearLcdScaling) {
					this.Emulator.Video.SetCapabilitiesByModelAndVideoSystem(HardwareModel.MasterSystem2, this.Emulator.Video.System);
				}
			}
		}

		/// <summary>
		/// Quick-load "something" (ROM, VGM, save state etc).
		/// </summary>
		/// <param name="filename"></param>
		private void QuickLoad(string filename) {
			switch (Path.GetExtension(filename).ToLowerInvariant()) {
				case ".cogstate":
					this.LoadState(filename);
					break;
				case ".vgm":
				case ".vgz":
					this.LoadVgm(filename);
					break;
				default:
					this.QuickLoadRom(filename);
					break;
			}
		}

		/// <summary>
		/// Quick-load a ROM.
		/// </summary>
		/// <param name="filename">The name of the ROM file to quick-load.</param>
		private void QuickLoadRom(string filename) {
			this.QuickLoadRom(filename, true);
		}

		/// <summary>
		/// Quick-load a ROM.
		/// </summary>
		/// <param name="filename">The name of the ROM file to quick-load.</param>
		/// <param name="addToRecentFileList">True (default) to add to the MRU, false to skip it.</param>
		private void QuickLoadRom(string filename, bool addToRecentFileList) {

			string Filename = filename;

			RomInfo LoadingRomInfo = null;
			try {
				LoadingRomInfo = this.Identifier.QuickLoadEmulator(ref Filename, this.Emulator);
			} catch (Exception ex) {
				MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.Dumper.RecreateDevice();

			this.UpdateFormTitle(Filename);

			if (addToRecentFileList) {
				this.AddRecentFile(filename);
			}

			if (LoadingRomInfo != null) {

				this.AddMessage(Properties.Resources.Icon_Information, LoadingRomInfo.Title);
				if (!string.IsNullOrEmpty(LoadingRomInfo.Author)) this.AddMessage(Properties.Resources.Icon_User, LoadingRomInfo.Author);

				switch (LoadingRomInfo.Type) {
					case RomInfo.RomType.HeaderedFootered:
						if (LoadingRomInfo.FooterSize > 0) this.AddMessage(Properties.Resources.Icon_Exclamation, "Footered");
						if (LoadingRomInfo.HeaderSize > 0) this.AddMessage(Properties.Resources.Icon_Exclamation, "Headered");
						break;
					case RomInfo.RomType.Overdumped:
						this.AddMessage(Properties.Resources.Icon_Exclamation, "Overdumped");
						break;
					case RomInfo.RomType.Translation:
						this.AddMessage(Properties.Resources.Icon_CommentEdit, "Translation");
						break;
					case RomInfo.RomType.Bios:
						this.AddMessage(Properties.Resources.Icon_Lightning, "BIOS");
						break;
					case RomInfo.RomType.Bad:
						this.AddMessage(Properties.Resources.Icon_Exclamation, "Fixable errors in dump");
						break;
					case RomInfo.RomType.VeryBad:
						this.AddMessage(Properties.Resources.Icon_Error, "Unusable dump");
						break;
					case RomInfo.RomType.Demo:
						this.AddMessage(Properties.Resources.Icon_House, "Homebrew");
						break;
					case RomInfo.RomType.Hack:
						this.AddMessage(Properties.Resources.Icon_Wrench, "Hack");
						break;
				}

				switch (LoadingRomInfo.Country) {
					case Country.Japan: this.AddMessage(Properties.Resources.Flag_JP, "Japan"); break;
					case Country.Brazil: this.AddMessage(Properties.Resources.Flag_BR, "Brazil"); break;
					case Country.UnitedStates: this.AddMessage(Properties.Resources.Flag_US, "United States"); break;
					case Country.Korea: this.AddMessage(Properties.Resources.Flag_KR, "Korea"); break;
					case Country.France: this.AddMessage(Properties.Resources.Flag_FR, "France"); break;
					case Country.Spain: this.AddMessage(Properties.Resources.Flag_ES, "Spain"); break;
					case Country.Germany: this.AddMessage(Properties.Resources.Flag_DE, "Germany"); break;
					case Country.Italy: this.AddMessage(Properties.Resources.Flag_IT, "Italy"); break;
					case Country.England: this.AddMessage(Properties.Resources.Flag_EN, "England"); break;
					case Country.NewZealand: this.AddMessage(Properties.Resources.Flag_NZ, "New Zealand"); break;
				}
			}

			// Load the SRAM.
			this.LoadRam();

			this.OverrideAutomaticSettings(LoadingRomInfo);

			this.WarnAboutColecoRom();

		}

		private bool LoadVgm(string filename) {
			// Grab the VGM player stub:
			var StubName = Path.Combine(Application.StartupPath, "vgmplayer.stub");
			if (!File.Exists(StubName)) {
				if (MessageBox.Show(this, "To play VGM files you will need to extract vgmplayer.stub from Maxim's VGM Player into Cogwheel's installation directory." + Environment.NewLine + "Would you like to visit Maxim's VGM Player page to download the software?", "Play VGM", MessageBoxButtons.YesNo) == DialogResult.Yes) {
					this.GoToUrl(Properties.Settings.Default.UrlVgmPlayerStub);
				}
				return false;
			}
			byte[] VgmPlayerStub = File.ReadAllBytes(StubName);

			// Load (and decompress) the VGM.
			byte[] SourceVgm = new byte[0];
			try {
				using (var SourceVgmStream = new GZipStream(File.OpenRead(filename), CompressionMode.Decompress)) {
					using (var Reader = new BinaryReader(SourceVgmStream)) {
						var DecompressedData = new List<byte>(1024);
						byte[] Chunk = null;
						do {
							Chunk = Reader.ReadBytes(1024);
							DecompressedData.AddRange(Chunk);
						} while (Chunk.Length > 0);
						SourceVgm = DecompressedData.ToArray();
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(this, "Could not open VGM: " + ex.Message, "Play VGM");
				return false;
			}

			// Check it's a valid VGM file.
			if (SourceVgm.Length < 64 || Encoding.ASCII.GetString(SourceVgm, 0, 4) != "Vgm ") {
				MessageBox.Show(this, Path.GetFileName(this.OpenVgmDialog.FileName) + " is not a valid VGM file.", "Play VGM", MessageBoxButtons.OK);
				return false;
			}

			// Create a temporary file made from the VGM file appended to the VGM player stub:
			string TempFileName = null;
			while (TempFileName == null || File.Exists(TempFileName)) {
				TempFileName = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), "sms"));
			}
			try {
				using (var TempVgmPlayer = new BinaryWriter(File.Create(TempFileName))) {
					TempVgmPlayer.Write(VgmPlayerStub);
					TempVgmPlayer.Write(SourceVgm);
				}
				// Load the ROM:
				this.QuickLoadRom(TempFileName, false);
				int VgmVersion = BitConverter.ToInt32(SourceVgm, 0x08);
				// Do we have a rate setting?
				if (VgmVersion >= 0x101) {
					this.Emulator.Video.System = BitConverter.ToInt32(SourceVgm, 0x24) == 50 ? BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Pal : BeeDevelopment.Sega8Bit.Hardware.VideoDisplayProcessor.VideoSystem.Ntsc;
				}
				// Do we have periodic noise settings?
				if (VgmVersion >= 0x110) {
					short TappedBits = BitConverter.ToInt16(SourceVgm, 0x28);
					if (TappedBits != 0) this.Emulator.Sound.TappedBits = TappedBits;
					byte ShiftRegisterWidth = SourceVgm[0x2A];
					if (ShiftRegisterWidth != 0) this.Emulator.Sound.ShiftRegisterWidth = ShiftRegisterWidth;
				}
				this.OverrideAutomaticSettings(null);
				// Ensure we have an SMS2 VDP, as we need the extended resolution support.
				this.Emulator.Video.SetCapabilitiesByModelAndVideoSystem(HardwareModel.MasterSystem2, this.Emulator.Video.System);
				return true;
			} finally {
				File.Delete(TempFileName);
			}
		}

	}
}
