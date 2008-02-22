using System.Collections.Generic;
using System.IO;
using System;
using BeeDevelopment.Sega8Bit.Mappers;

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
		/// Creates an <see cref="ICartridgeMapper"/> instance from a ROM dump.
		/// </summary>
		/// <param name="data">The data to create the <see cref="ICartridgeMapper"/> instance from.</param>
		/// <returns>An <see cref="ICartridgeMapper"/> with ROM dump data loaded.</returns>
		/// <remarks>This method will attempt to guess the correct mapper to use.</remarks>
		public ICartridgeMapper CreateCartridgeMapper(byte[] data) {

			ICartridgeMapper Result = null;

			if (data.Length >= 0x8000) {
				ushort CodemastersChecksumA = BitConverter.ToUInt16(data, 0x7FE6);
				ushort CodemastersChecksumB = BitConverter.ToUInt16(data, 0x7FE8);
				if (CodemastersChecksumB == 0x10000 - CodemastersChecksumA) {
					Result = new Codemasters();
				}
			}


			RomInfo TryIdentifyHardware = this.GetRomInfo(data);
			if (TryIdentifyHardware != null) {
				if (TryIdentifyHardware.RomData != null && (TryIdentifyHardware.RomData.Model == HardwareModel.SG1000 || TryIdentifyHardware.RomData.Model == HardwareModel.SC3000)) {
					Result = new Ram();
				}
			}

			Result = Result ?? new Standard();

			Result.Load(data);
			Result.Reset();

			return Result;

		}

	}
}
