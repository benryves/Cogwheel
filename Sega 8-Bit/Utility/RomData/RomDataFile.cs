using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BeeDevelopment.Sega8Bit.Utility.RomData {
	/// <summary>
	/// Represents a .romdata file containing <see cref="RomData"/> instances.
	/// </summary>
	public class RomDataFile : IEnumerable<RomData> {

		#region Public Properties

		/// <summary>
		/// Gets the name of the system.
		/// </summary>
		public string System { get; private set; }

		/// <summary>
		/// Gets the extensions that this data file refers to.
		/// </summary>
		public string[] Extensions { get; private set; }

		#endregion

		#region Private Fields

		private List<RomData> Entries;

		#endregion		

		#region Constructors
		
		private RomDataFile() {
			this.Entries = new List<RomData>(); 
		}

		/// <summary>
		/// Loads a <see cref="RomDataFile"/> from a file.
		/// </summary>
		/// <param name="filename">The name of the file to load the <see cref="RomDataFile"/> from.</param>
		/// <returns>A <see cref="RomDataFile"/> read from the file <paramref name="filename"/>.</returns>
		public static RomDataFile FromFile(string filename) {
			using (var DataFile = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
				return RomDataFile.FromStream(DataFile);
			}
		}

		/// <summary>
		/// Loads a <see cref="RomDataFile"/> from a <see cref="Stream"/>.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to load the <see cref="RomDataFile"/> from.</param>
		/// <returns>A <see cref="RomDataFile"/> read from the <paramref name="stream"/>.</returns>
		public static RomDataFile FromStream(Stream stream) {
			var Result = new RomDataFile();

			var DataFileReader = new StreamReader(stream);

			while (!DataFileReader.EndOfStream) {
				var Source = DataFileReader.ReadLine();
				if (Source.Trim() == "") continue;
				if (Source.StartsWith("!")) {
					if (Result.System == null) {
						Result.System = Source.Substring(1).Trim();
					} else if (Result.Extensions == null) {
						Result.Extensions = Source.Substring(1).Trim().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
					}
				} else {
					var NewData = RomData.FromString(Source);
					NewData.DataFile = Result;
					Result.Entries.Add(NewData);
				}
			}

			return Result;

		}

		#endregion

		#region IEnumerable<RomData> Members

		public IEnumerator<RomData> GetEnumerator() {
			return this.Entries.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.Entries.GetEnumerator();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Try and get a <see cref="RomData"/> from the <see cref="RomDataFile"/> by the CRC-32 checksum.
		/// </summary>
		/// <param name="crc">The CRC-32 checksum to search for.</param>
		/// <param name="romData">Outputs the <see cref="RomData"/> instance found matching the supplied <paramref name="crc"/>.</param>
		/// <returns>True if a matching CRC-32 was found, false otherwise.</returns>
		public bool TryGetRomData(int crc, out RomData romData) {
			romData = default(RomData);
			foreach (var PossibleRom in this) {
				if (PossibleRom.Crc32 == crc) {
					romData = PossibleRom;
					return true;
				}
			}
			return false;
		}

		#endregion
	}
}
