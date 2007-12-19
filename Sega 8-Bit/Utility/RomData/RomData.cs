using System;

namespace BeeDevelopment.Sega8Bit.Utility.RomData {
	
	/// <summary>
	/// Contains information about a dumped ROM.
	/// </summary>
	public class RomData : IComparable {

		#region Types

		/// <summary>
		/// Identifies a particular ROM type.
		/// </summary>
		public enum Categories {
			/// <summary>
			/// Commerical or unknown software.
			/// </summary>
			Normal,
			/// <summary>
			/// Homebrew software.
			/// </summary>
			Homebrew,
			/// <summary>
			/// A modified version of existing software.
			/// </summary>
			Hack,
			/// <summary>
			/// Translated software.
			/// </summary>
			Translation,
			/// <summary>
			/// A BIOS ROM image.
			/// </summary>
			Bios,
		}

		/// <summary>
		/// Represents the quality of the ROM dump.
		/// </summary>
		public enum Qualities {
			/// <summary>
			/// The ROM is in a usable condition.
			/// </summary>
			Good,
			/// <summary>
			/// The ROM might contain redundant data, such as a header or a footer, or duplicated data from overdumping.
			/// </summary>
			Dirty,
			/// <summary>
			/// The ROM image has some bad bytes that can be repaired.
			/// </summary>
			Damaged,
			/// <summary>
			/// The ROM is in such bad condition it cannot be used.
			/// </summary>
			Irreparable,
		}

		#endregion

		#region Private Fields

		private char Status;
		private string Tag;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the CRC-32 checksum of the ROM.
		/// </summary>
		public int Crc32 { get; private set; }

		/// <summary>
		/// Gets the descriptive name of the ROM.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets comments describing the ROM.
		/// </summary>
		public string Comments { get; private set; }

		/// <summary>
		/// Gets or sets the category of the ROM.
		/// </summary>
		public Categories Category { get; private set; }

		/// <summary>
		/// Gets the quality of the ROM dump (its condition).
		/// </summary>
		public Qualities Quality { get; private set; }

		/// <summary>
		/// Gets the data file containing the <see cref="RomData"/>.
		/// </summary>
		public RomDataFile DataFile { get; internal set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an instance of the <see cref="RomData"/> class from a <see cref="String"/>.
		/// </summary>
		/// <param name="s">The <see cref="String"/> to create an instance from.</param>
		/// <remarks>The <see cref="String"/> should be in the format used in a .romdata file.</remarks>
		public static RomData FromString(string s) {
			var Result = new RomData();
			var Components = s.Split('\t');

			Array.Resize(ref Components, 4);

			Result.Crc32 = Convert.ToInt32(Components[0], 16);

			Result.Name = Components[2];

			Result.Category = Categories.Normal;
			Result.Quality = Qualities.Good;

			Result.Status = char.ToUpperInvariant(((Components[1] ?? "").Trim() + " ")[0]);

			switch (Result.Status) {
				case 'D':
					Result.Category = Categories.Homebrew;
					break;
				case 'H':
					Result.Category = Categories.Hack;
					break;
				case 'I':
					Result.Category = Categories.Bios;
					break;
				case 'T':
					Result.Category = Categories.Translation;
					break;
				case 'B':
					Result.Quality = Qualities.Damaged;
					break;
				case 'V':
					Result.Quality = Qualities.Irreparable;
					break;
				case 'O':
				case 'F':
					Result.Quality = Qualities.Dirty;
					break;
			}

			Result.Tag = (Components[3] ?? "").Trim();

			if (Result.Quality == Qualities.Good) {
				Result.Comments = Components[3];
			}

			return Result;
			
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns a string representing the <see cref="RomData"/> instance.
		/// </summary>
		public override string ToString() {
			var Result = string.Format("{0} - {1} ({2})", this.Name, this.Category, this.Quality);
			if (this.DataFile != null && !string.IsNullOrEmpty(this.DataFile.System)) {
				Result += " [" + this.DataFile.System + "]";
			}
			return Result;
		}

		/// <summary>
		/// Fix a ROM file if possible.
		/// </summary>
		/// <param name="data">The data to fix.</param>
		/// <returns>True if the data was repaired in any way.</returns>
		/// <remarks>If the ROM was already in good condition then this function will still return false as no repairs were carried out.</remarks>
		public bool Fix(ref byte[] data) {
			switch (this.Status) {
				case 'B':
					foreach (var Patch in this.Tag.Split('&')) {
						var PatchComponents = Array.ConvertAll(Patch.Split('='), x => Convert.ToInt32(x, 16));
						data[Patch[0]] = (byte)PatchComponents[1];
					}
					return true;
				case 'O':
					Array.Resize(ref data, Convert.ToInt32(this.Tag, 16));
					return true;
				case 'F':
					var FooterSize = 64;
					var IsHeader = false;
					if (this.Tag.ToUpperInvariant() != "F" && this.Tag.Length>0) {
						IsHeader = char.ToUpperInvariant(this.Tag[0]) == 'H';
						FooterSize = Convert.ToInt32(IsHeader ? this.Tag.Substring(1) : this.Tag);
					}
					if (IsHeader) {
						var NewData = new byte[data.Length - FooterSize];
						Array.Copy(data, FooterSize, NewData, 0, NewData.Length);
						data = NewData;
					} else {
						Array.Resize(ref data, data.Length - FooterSize);
					}
					return true;
			}
			return false;
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares two <see cref="RomData"/> instances so that the most relevant one can be picked from a list of <see cref="RomData"/> instances.
		/// </summary>
		/// <param name="obj">The object to compare to.</param>
		public int CompareTo(object obj) {
			RomData that = (RomData)obj;

			if (that.Status != this.Status) {
				if (this.Status == 'E') {
					return +1;
				} else if (that.Status == 'E') {
					return -1;
				}
			}

			if (this.Quality != that.Quality) {
				return this.Quality.CompareTo(that.Quality);
			}

			return this.Name.CompareTo(that.Name);

		}

		#endregion
	}
}
