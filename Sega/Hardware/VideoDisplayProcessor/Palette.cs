using System;
namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class VideoDisplayProcessor {

		#region Properties

		/// <summary>
		/// Gets the colour RAM as 32-bit ARGB values. 
		/// </summary>
		/// <remarks>
		/// In hardware, the colour RAM is either represented as 6-bit or 12-bit RGB values.
		/// 
		/// For the sake of simplifying the programming, the colour RAM is updated automatically
		/// to contain 32-bit ARGB values regardless of the current hardware mode.
		/// 
		/// In certain modes the data in the colour RAM is ignored entirely when the display is rasterised.
		/// </remarks>
		public int[] ColourRam { 
			get { return this.colourRam; }
			set {
				if (value == null || value.Length != this.colourRam.Length) throw new InvalidOperationException();
				this.colourRam = value; 
			}
		}
		private int[] colourRam;

		/// <summary>
		/// Gets or sets the latched value written to colour RAM.
		/// </summary>
		/// <remarks>
		/// When writing to the Game Gear's colour RAM, writes to even addresses
		/// are stored in an internal buffer (and the colour RAM is not updated)
		/// until a subsequent write to an odd address, when both values are
		/// written simultaneously.
		/// </remarks>
		public byte LatchedColourRamData { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Writes data to the colour RAM.
		/// </summary>
		/// <param name="value">The data to write to colour RAM.</param>
		private void WriteColourRam(byte value) {

			if (this.extendedPalette && (this.address & 1) == 0) {

				// Latch the colour data, but don't actually bother writing it.
				this.LatchedColourRamData = value;

			} else {

				int PaletteIndex;
				int R, G, B;

				// Calculate the R, G and B components and palette index.
				// The R, G and B components are all scaled up to the full 0..255 range.
				// The redundant-looking additions (rather than just scaling) is to more evenly spread the results.
				// ie, a component of %01 doesn't become %01000000, but becomes %01010101.

				if (this.extendedPalette) {
					PaletteIndex = (address / 2) & 31;
					B = value & 0x0F; B += B * 16;
					G = this.LatchedColourRamData & 0xF0; G += G / 16;
					R = this.LatchedColourRamData & 0x0F; R += R * 16;
				} else {
					PaletteIndex = address & 31;
					B = (value >> 4) & 0x3; B += B * 4; B += B * 16;
					G = (value >> 2) & 0x3; G += G * 4; G += G * 16;
					R = (value >> 0) & 0x3; R += R * 4; R += R * 16;
				}


				// Finally, convert to 32-bit ARGB and dump into the ColourRam array.
				this.colourRam[PaletteIndex] = (int)(0xFF000000 + R * 0x10000 + G * 0x100 + B);

			}
		}

		#endregion

		#region Fixed Palettes

		/// <summary>
		/// Hard-coded fixed TMS9918 colour palette.
		/// </summary>
		private int[] FixedPaletteTMS9918 = new[] {
			unchecked((int)0x00000000),
			unchecked((int)0xFF000000),
			unchecked((int)0xFF47B73B),
			unchecked((int)0xFF7CCF6F),
			unchecked((int)0xFF5D4EFF),
			unchecked((int)0xFF8072FF),
			unchecked((int)0xFFB66247),
			unchecked((int)0xFF5DC8ED),
			unchecked((int)0xFFD76B48),
			unchecked((int)0xFFFB8F6C),
			unchecked((int)0xFFC3CD41),
			unchecked((int)0xFFD3DA76),
			unchecked((int)0xFF3E9F2F),
			unchecked((int)0xFFB664C7),
			unchecked((int)0xFFCCCCCC),
			unchecked((int)0xFFFFFFFF)
		};

		/// <summary>
		/// Hard-coded fixed Master System palette.
		/// </summary>
		/// <remarks>
		/// This is used when the Master System VDP is in a legacy video mode.
		/// It is a rough copy of the TMS9918 colours poorly mangled to 6-bit RGB,
		/// and as such is incredibly ugly.
		/// </remarks>
		private int[] FixedPaletteMasterSystem = new[] {
			unchecked((int)0x00000000),
			unchecked((int)0xFF000000),
			unchecked((int)0xFF00AA00),
			unchecked((int)0xFF00FF00),
			unchecked((int)0xFF000055),
			unchecked((int)0xFF0000FF),
			unchecked((int)0xFF550000),
			unchecked((int)0xFF00FFFF),
			unchecked((int)0xFFAA0000),
			unchecked((int)0xFFFF0000),
			unchecked((int)0xFF555500),
			unchecked((int)0xFFFFFF00),
			unchecked((int)0xFF005500),
			unchecked((int)0xFFFF00FF),
			unchecked((int)0xFF555555),
			unchecked((int)0xFFFFFFFF)
		};

		#endregion
	}
}