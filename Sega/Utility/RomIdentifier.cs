using System;
using System.Collections.Generic;
using System.IO;
using BeeDevelopment.Sega8Bit.Mappers;
#if SILVERLIGHT
using FileEx = System.IO.FileEx;
#else
using FileEx = System.IO.File;
#endif

namespace BeeDevelopment.Sega8Bit.Utility {
	public class RomIdentifier {

		/// <summary>
		/// Gets a list of <see cref="RomData"/> files that can be searched to identify a ROM.
		/// </summary>
		public List<RomData> RomData { get; private set; }

		/// <summary>
		/// Creates an instance of the <see cref="RomIdentifier"/> class.
		/// </summary>
		public RomIdentifier() {
			this.RomData = new List<RomData>();
		}

		/// <summary>
		/// Creates an instance of the <see cref="RomIdentifier"/> class and load <c>.romdata</c> files automatically.
		/// </summary>
		/// <param name="romDataDirectory">The directory to search for <c>.romdata</c> files.</param>
		public RomIdentifier(string romDataDirectory) : this() {
			foreach (var RomDataFile in Directory.GetFiles(romDataDirectory, "*.romdata")) {
				this.RomData.Add(new RomData(RomDataFile));
			}
		}

		/// <summary>
		/// Returns a <see cref="RomInfo"/> instance describing the ROM identified by its CRC-32 checksum, or <c>null</c> if no ROM was found.
		/// </summary>
		/// <param name="crc">The CRC-32 checksum of the ROM to find.</param>
		/// <param name="filename">The name of the file you are trying to identify.</param>
		/// <returns>A <see cref="RomInfo"/> instance describing the ROM, or <c>null</c> if no ROM was found.</returns>
		/// <remarks>The filename does not have to be a full, valid path - only the extension is used.</remarks>
		public RomInfo GetRomInfo(int crc, string filename) {

			// Check against valid extension first.

			if (!string.IsNullOrEmpty(filename)) {

				string Extension = Path.GetExtension(filename).ToLowerInvariant();

				foreach (var RomDataFile in this.RomData) {
					if (RomDataFile.Extensions != null) {
						foreach (var CheckExtension in RomDataFile.Extensions) {
							if (CheckExtension == Extension) {
								RomInfo Result = RomDataFile.GetRomInfo(crc);
								if (Result != null) return Result;
							}
						}
					}
				}
			}

			// Check any old file next.

			foreach (var RomDataFile in this.RomData) {
				RomInfo Result = RomDataFile.GetRomInfo(crc);
				if (Result != null) return Result;
			}

			return null;

		}

		/// <summary>
		/// Returns a <see cref="RomInfo"/> instance describing the ROM identified by its CRC-32 checksum, or <c>null</c> if no ROM was found.
		/// </summary>
		/// <param name="crc">The CRC-32 checksum of the ROM to find.</param>
		/// <returns>A <see cref="RomInfo"/> instance describing the ROM, or <c>null</c> if no ROM was found.</returns>
		public RomInfo GetRomInfo(int crc) {
			return this.GetRomInfo(crc, null);
		}

		/// <summary>
		/// Returns a <see cref="RomInfo"/> instance describing the current ROM identified by its data, or <c>null</c> if no ROM was found.
		/// </summary>
		/// <param name="data">The data to search for.</param>
		/// <param name="filename">The name of the file you are trying to identify.</param>
		/// <returns>A <see cref="RomInfo"/> instance describing the ROM, or <c>null</c> if no ROM was found.</returns>
		/// <remarks>The filename does not have to be a full, valid path - only the extension is used.</remarks>
		public RomInfo GetRomInfo(byte[] data, string filename) {
			if (data == null) return null;
			return this.GetRomInfo(Zip.Crc32.Calculate(data), filename);
		}

		/// <summary>
		/// Returns a <see cref="RomInfo"/> instance describing the current ROM identified by its data, or <c>null</c> if no ROM was found.
		/// </summary>
		/// <param name="data">The data to search for.</param>
		/// <returns>A <see cref="RomInfo"/> instance describing the ROM, or <c>null</c> if no ROM was found.</returns>
		public RomInfo GetRomInfo(byte[] data) {
			return this.GetRomInfo(data, null);
		}

		/// <summary>
		/// Creates an <see cref="IMemoryMapper"/> instance from a ROM dump.
		/// </summary>
		/// <param name="data">The data to create the <see cref="IMemoryMapper"/> instance from.</param>
		/// <returns>An <see cref="IMemoryMapper"/> with ROM dump data loaded.</returns>
		/// <remarks>This method will attempt to guess the correct mapper to use.</remarks>
		public IMemoryMapper CreateMapper(byte[] data) {

			IMemoryMapper Result = null;

			if (data.Length >= 0x8000) {
				ushort CodemastersChecksumA = BitConverter.ToUInt16(data, 0x7FE6);
				ushort CodemastersChecksumB = BitConverter.ToUInt16(data, 0x7FE8);
				if (CodemastersChecksumB == 0x10000 - CodemastersChecksumA) {
					Result = new Codemasters();
				}
			}


			RomInfo TryIdentifyHardware = this.GetRomInfo(data);
			if (TryIdentifyHardware != null) {
				switch (TryIdentifyHardware.Model) {
					case HardwareModel.SG1000:
					case HardwareModel.SC3000:
						Result = new Ram64();
						break;
					case HardwareModel.GameGear:
					case HardwareModel.GameGearMasterSystem:
						if (TryIdentifyHardware.Type == RomInfo.RomType.Bios) Result = new Shared1KBios();
						break;
					case HardwareModel.ColecoVision:
						if (data.Length <= 0x2000) {
							Result = new Rom8();
						} else if (data.Length <= 0x4000) {
							Result = new Rom16();
						} else {
							Result = new Rom32();
						}						
						break;
				}
			}

			Result = Result ?? new Standard();

			Result.Load(data);
			Result.Reset();

			return Result;

		}

		/// <summary>
		/// Quick-loads a ROM file into an emulator, setting up the state automatically.
		/// </summary>
		/// <param name="romFileName">The filename of the ROM file.</param>
		/// <param name="emulator">The emulator instance to load the ROM file for.</param>
		/// <returns>A <see cref="RomInfo"/> instance that describes the ROM, or null if none was found.</returns>
		public RomInfo QuickLoadEmulator(ref string romFileName, Emulator emulator) {

			HardwareModel Model = HardwareModel.MasterSystem2;
			RomInfo Info = null;

			var Data = LoadAndFixRomData(ref romFileName, out Info);

			Model = RomIdentifier.GetModelFromExtension(Path.GetExtension(romFileName));

			if (Info != null) Model = Info.Model;

			emulator.RemoveAllMedia();
			emulator.ResetAll();

			emulator.SetCapabilitiesByModelAndCountry(
				(emulator.Region == BeeDevelopment.Sega8Bit.Region.Japanese && Model == HardwareModel.MasterSystem2) ? HardwareModel.MasterSystem : Model,
				Info != null ? Info.Country : Country.None
			);

			if (Info != null && Info.Type == RomInfo.RomType.Bios) {
				emulator.Bios.Memory = this.CreateMapper(Data);
				emulator.Bios.Enabled = true;
				emulator.CartridgeSlot.Enabled = false;
			} else {
				emulator.CartridgeSlot.Memory = this.CreateMapper(Data);
				emulator.Bios.Enabled = false;
				emulator.CartridgeSlot.Enabled = true;

				if (Model == HardwareModel.ColecoVision) {
					try {
#if !SILVERLIGHT
						string ColecoBiosRomPath = Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "COLECO.ROM");
						if (File.Exists(ColecoBiosRomPath)) {
							emulator.Bios.Memory = new Rom8();
							emulator.Bios.Memory.Load(FileEx.ReadAllBytes(ColecoBiosRomPath));
						}
#endif
					} catch { }
				}

			}
			return Info;

		}

		/// <summary>
		/// Loads and fixes data from a ROM file.
		/// </summary>
		/// <param name="filename">The file to load.</param>
		/// <param name="info">Outputs <see cref="RomInfo"/> if found.</param>
		/// <returns>The loaded and fixed ROM data.</returns>
		public byte[] LoadAndFixRomData(ref string filename, out RomInfo info) {
			info = null;
			var Data = ZipLoader.FindRom(ref filename);
			info = this.GetRomInfo(Data, filename);
			if (info != null) info.Fix(ref Data);
			return Data;
		}


		/// <summary>
		/// Loads and fixes data from a ROM file.
		/// </summary>
		/// <param name="filename">The file to load.</param>
		/// <returns>The loaded and fixed ROM data.</returns>
		public byte[] LoadAndFixRomData(ref string filename) {
			RomInfo Dud;
			return LoadAndFixRomData(ref filename, out Dud);
		}


		/// <summary>
		/// Identifies a particular <see cref="HardwareModel"/> from the file extension typically used for its ROM dumps.
		/// </summary>
		/// <param name="extension">The file extension to check for (with or without a leading dot).</param>
		/// <returns>The corresponding <see cref="HardwareModel"/> for the file extension.</returns>
		public static HardwareModel GetModelFromExtension(string extension) {
			if (string.IsNullOrEmpty(extension)) return HardwareModel.Default;
			if (extension[0] == '.') extension = extension.Substring(1);
			switch (extension.ToLowerInvariant()) {
				case "sms":
					return HardwareModel.MasterSystem2;
				case "gg":
					return HardwareModel.GameGear;
				case "sg":
				case "mv":  // These two are the "Othello Multivision",
				case "omv": // a console compatible with the SG-1000.
					return HardwareModel.SG1000;
				case "sc":
					return HardwareModel.SC3000;
				case "sf7":
					return HardwareModel.SF7000;
				case "rom":
				case "col":
					return HardwareModel.ColecoVision;
			}
			return HardwareModel.Default; // No idea!
		}
	}
}
