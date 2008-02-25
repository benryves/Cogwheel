using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace BeeDevelopment.Sega8Bit.Utility {
	/// <summary>
	/// Describes a ROM dump.
	/// </summary>
	public class RomInfo {

		#region Types

		/// <summary>
		/// Defines the type of the ROM.
		/// </summary>
		public enum RomType {
			/// <summary>
			/// No special type is associated with this ROM.
			/// </summary>
			None,
			/// <summary>
			/// The ROM is a homebrew demo.
			/// </summary>
			/// <remarks>Represented by a <c>D</c> in the <c>.romdata</c> file.</remarks>
			Demo,
			/// <summary>
			/// The ROM is a hack.
			/// </summary>
			/// <remarks>Represented by an <c>H</c> in the <c>.romdata</c> file.</remarks>
			Hack,
			/// <summary>
			/// The ROM is damaged, but repairable.
			/// </summary>
			/// <remarks>Represented by a <c>B</c> in the <c>.romdata</c> file.</remarks>
			Bad,
			/// <summary>
			/// The ROM has a header or a footer.
			/// </summary>
			/// <remarks>Represented by an <c>F</c> in the <c>.romdata</c> file.</remarks>
			HeaderedFootered,
			/// <summary>
			/// The ROM has been overdumped.
			/// </summary>
			/// <remarks>Represented by an <c>O</c> in the <c>.romdata</c> file.</remarks>
			Overdumped,
			/// <summary>
			/// The ROM is a translation.
			/// </summary>
			/// <remarks>Represented by a <c>T</c> in the <c>.romdata</c> file.</remarks>
			Translation,
			/// <summary>
			/// The ROM is very badly damaged, and cannot be repaired.
			/// </summary>
			/// <remarks>Represented by a <c>V</c> in the <c>.romdata</c> file.</remarks>
			VeryBad,
			/// <summary>
			/// The ROM has an incorrect extension.
			/// </summary>
			/// <remarks>Represented by an <c>E</c> in the <c>.romdata</c> file.</remarks>
			IncorrectExtension,
			/// <summary>
			/// The ROM is a BIOS image.
			/// </summary>
			/// <remarks>Represented by an <c>I</c> in the <c>.romdata</c> file.</remarks>
			Bios,
		}
		
		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the CRC-32 checksum of the ROM file.
		/// </summary>
		public int Crc32 { get; set; }

		/// <summary>
		/// Gets or sets a descriptive name of the ROM.
		/// </summary>
		public string FullName { get; set; }

		/// <summary>
		/// Gets or sets the author of the ROM.
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// Gets or sets comments attributed to the ROM.
		/// </summary>
		public string Comments { get; set; }

		/// <summary>
		/// Gets or sets data that can be used to patch a damaged ROM to correct it.
		/// </summary>
		public KeyValuePair<int, byte>[] CorrectivePatch { get; set; }

		/// <summary>
		/// Gets or sets the size of the ROM's header.
		/// </summary>
		public int HeaderSize { get; set; }

		/// <summary>
		/// Gets or sets the size of the ROM's footer.
		/// </summary>
		/// <remarks>
		/// A footer can be caused by overdumping.
		/// </remarks>
		public int FooterSize { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="RomType"/> of the ROM.
		/// </summary>
		public RomType Type { get; set; }

		/// <summary>
		/// Gets or sets the correct extension if the <see cref="RomType"/> is <c>IncorrectExtension</c>.
		/// </summary>
		public string CorrectedExtension { get; set; }

		/// <summary>
		/// Gets or sets the correct size if the <see cref="RomType"/> is <c>Overdumped</c>.
		/// </summary>
		public int? CorrectedSize { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="RomData"/> instance that defines this <see cref="RomInfo"/> instance.
		/// </summary>
		public RomData RomData { get; set;}

		/// <summary>
		/// Gets the title of the game, with any special information removed.
		/// </summary>
		public string Title {

			get {

				string[] StrippedInfo = new string[]{
                        "[BIOS]", "[Hack]", "[Proto]",
                        "(h)", "(f)",
                    };


				string s = this.FullName;
				foreach (string remove in StrippedInfo) s = s.Replace(remove, "");

				s = new Regex(@"\(bad([^\(]*?)\)").Replace(s, "");
				s = new Regex(@"\(first([^\(]*?)\)").Replace(s, "");
				s = new Regex(@"\(([^\(]*?)byte(.*?)\)").Replace(s, "");
				s = new Regex(@"\[([A-Z]{1}|[vV]([^\[].*?)|Proto|beta([^\[].*?))\]").Replace(s, "");
				s = new Regex(@"\((([^\(]*?)overdump)\)").Replace(s, "");

				foreach (Country C in Enum.GetValues(typeof(Country))) {
					s = new Regex(@"\(" + Countries.CountryToIdentifier(C) + @"\)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).Replace(s, "");
				}


				while (s.Contains("  ")) s = s.Replace("  ", " ");
				s = s.Trim();
				if (s.EndsWith(", The")) s = "The " + s.Replace(", The", "");
				return s.Trim();

			}
		}

		/// <summary>
		/// Gets or sets the country of origin of the ROM.
		/// </summary>
		public Country Country { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an empty instance of <see cref="RomInfo"/>.
		/// </summary>
		public RomInfo() { }

		/// <summary>
		/// Creates an instance of <see cref="RomInfo"/> from a line in a <c>.romdata</c> file.
		/// </summary>
		/// <param name="romDataDefinition">The line from the <c>.romdata</c> file to parse.</param>
		public RomInfo(string romDataDefinition)
			: this() {

			string[] Components = Array.ConvertAll(romDataDefinition.Split('\t'), s => s.Trim());
			if (Components.Length < 3) throw new InvalidDataException("Not enough fields in the ROM data definition.");
			
			this.Crc32 = Convert.ToInt32(Components[0], 16);

			switch (Components[1].ToUpperInvariant()) {
				case "": this.Type = RomType.None; break;
				case "D": this.Type = RomType.Demo; break;
				case "H": this.Type = RomType.Hack; break;
				case "B": this.Type = RomType.Bad; break;
				case "F": this.Type = RomType.HeaderedFootered; break;
				case "O": this.Type = RomType.Overdumped; break;
				case "T": this.Type = RomType.Translation; break;
				case "V": this.Type = RomType.VeryBad; break;
				case "E": this.Type = RomType.IncorrectExtension; break;
				case "I": this.Type = RomType.Bios; break;
				default: throw new InvalidDataException("Unsupported ROM type '" + Components[1] + "'.");
			}

			this.FullName = Components[2];

			// Find country.
			this.Country = Country.None;
			foreach (Country PossibleMatch in Enum.GetValues(typeof(Country))) {
				if (this.FullName.ToUpperInvariant().Contains("(" + Countries.CountryToIdentifier(PossibleMatch) + ")")) {
					this.Country = PossibleMatch;
				}
			}

			if (Components.Length > 3) {

				switch (this.Type) {
					case RomType.None:
					case RomType.VeryBad:
					case RomType.Hack:
						this.Comments = Components[3];
						break;
					case RomType.Demo:
					case RomType.Translation:
						this.Author = Components[3];
						break;
					case RomType.IncorrectExtension:
						this.CorrectedExtension = Components[3];
						break;
					case RomType.Bad:
						this.CorrectivePatch = Array.ConvertAll(Components[3].Split('&'), Patch => { var Values = Array.ConvertAll(Patch.Split('='), Value => Convert.ToInt32(Value, 16)); return new KeyValuePair<int, byte>(Values[0], (byte)Values[1]); });
						break;
					case RomType.HeaderedFootered:
						if (Components[3].Length > 0) {

							int Amount = 128;

							if (Components[3].Length > 1) {
								Amount = Convert.ToInt32(Components[3].Substring(1));
							}

							switch (char.ToUpperInvariant(Components[3][0])) {
								case 'F':
									this.FooterSize = Amount;
									break;
								case 'H':
									this.HeaderSize = Amount;
									break;
							}
						}
						break;
					case RomType.Overdumped:
						if (Components[3].Length > 0) {
							this.CorrectedSize = Convert.ToInt32(char.ToUpperInvariant(Components[3][0]) == 'O' ? Components[3].Substring(1) : Components[3], 16);
						}
						break;
				}

			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Returns a descriptive name for the ROM.
		/// </summary>
		public override string ToString() {
			return this.FullName;
		}

		/// <summary>
		/// Fix a ROM dump.
		/// </summary>
		/// <param name="data">The data to fix.</param>
		public void Fix(ref byte[] data) {
			
			// First up, check for ROMs that are either OK or unfixable:
			switch (this.Type) {
				case RomType.None:
				case RomType.Demo:
				case RomType.Hack:
				case RomType.IncorrectExtension:
				case RomType.Translation:
				case RomType.VeryBad:
				case RomType.Bios:
					return;
			}

			// Correct overdumping.
			if (this.Type == RomType.Overdumped && this.CorrectedSize.HasValue) {
				Array.Resize(ref data, this.CorrectedSize.Value);
			}

			// Correct footer.
			if (this.Type == RomType.HeaderedFootered && this.FooterSize != 0) {
				Array.Resize(ref data, data.Length - this.FooterSize);
			}

			// Correct header.
			if (this.Type == RomType.HeaderedFootered && this.HeaderSize != 0) {
				var NewData = new byte[data.Length - this.HeaderSize];
				Array.Copy(data, this.HeaderSize, NewData, 0, NewData.Length);
				data = NewData;
			}

			// Correct bad bytes.
			if (this.CorrectivePatch != null) {
				foreach (var Patch in this.CorrectivePatch) {
					if (Patch.Key > 0 && Patch.Key < data.Length) data[Patch.Key] = Patch.Value;
				}
			}

		}

		#endregion

	}
}
