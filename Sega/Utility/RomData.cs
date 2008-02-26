using System;
using System.Collections.Generic;
using System.IO;

namespace BeeDevelopment.Sega8Bit.Utility {
	/// <summary>
	/// Provides methods for reading <c>.romdata</c> files.
	/// </summary>
	public class RomData {

		#region Properties

		/// <summary>
		/// Gets or sets the name of the hardware model.
		/// </summary>
		public string ModelName { get; set; }

		/// <summary>
		/// Gets or sets the associated extension for ROMs described by the <c>.romdata</c> file.
		/// </summary>
		public string[] Extensions { get; set; }

		/// <summary>
		/// Gets or sets the comments attributed to the data.
		/// </summary>
		public string Comments { get; set; }

		/// <summary>
		/// Provides access to a list of <see cref="RomInfo"/> instances defined in a <c>.romdata</c> file.
		/// </summary>
		public List<RomInfo> Roms { get; private set; }

		/// <summary>
		/// Gets or sets the hardware model that this <see cref="RomData"/> describes.
		/// </summary>
		public HardwareModel Model { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a <see cref="RomData"/> instance from a file.
		/// </summary>
		/// <param name="filename">The name of the <c>.romdata</c> file to load.</param>
		public RomData(string filename) {

			this.Roms = new List<RomInfo>(64);

			this.Model = HardwareModel.Default;

			using (var DataReader = new StreamReader(File.OpenRead(filename))) {
				string CurrentLine;
				while ((CurrentLine = DataReader.ReadLine()) != null) {

					CurrentLine = CurrentLine.Trim();


					if (string.IsNullOrEmpty(CurrentLine)) {

						// Ignore empty lines.
						continue;

					} else if (CurrentLine.StartsWith("!")) {

						// Handle lines starting with "!".

						if (this.ModelName == null) {
							this.ModelName = CurrentLine.Substring(1).Trim();
						} else if (this.Extensions == null) {

							this.Extensions = Array.ConvertAll(CurrentLine.Substring(1).ToLowerInvariant().Split('.'), Extension => "." + Extension.Trim());

							foreach (var Extension in this.Extensions) {
								switch (Extension) {
									case ".sms":
										this.Model = HardwareModel.MasterSystem2;
										break;
									case ".gg":
										this.Model = HardwareModel.GameGear;
										break;
									case ".sc":
										this.Model = HardwareModel.SC3000;
										break;
									case ".sg":
										this.Model = HardwareModel.SG1000;
										break;
								}
							}

						} else if (this.Comments == null) {
							this.Comments = CurrentLine.Substring(1).Trim();
						} else {
							this.Comments += Environment.NewLine + CurrentLine.Substring(1).Trim();
						}

					} else {

						// Gadzooks, 'tis a valid line of data.
						this.Roms.Add(new RomInfo(CurrentLine, this));

					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Returns a <see cref="RomInfo"/> instance describing the ROM identified by its CRC-32 checksum, or <c>null</c> if no ROM was found.
		/// </summary>
		/// <param name="crc">The CRC-32 checksum of the ROM to find.</param>
		/// <returns>A <see cref="RomInfo"/> instance describing the ROM, or <c>null</c> if no ROM was found.</returns>
		public RomInfo GetRomInfo(int crc) {
			foreach (var Rom in this.Roms) if (Rom.Crc32 == crc) return Rom;
			return null;
		}

		/// <summary>
		/// Returns a <see cref="RomInfo"/> instance describing the current ROM identified by its data, or <c>null</c> if no ROM was found.
		/// </summary>
		/// <param name="data">The data to search for.</param>
		/// <returns>A <see cref="RomInfo"/> instance describing the ROM, or <c>null</c> if no ROM was found.</returns>
		public RomInfo GetRomInfo(byte[] data) {
			return this.GetRomInfo(Zip.Crc32.Calculate(data));
		}


	}
}
