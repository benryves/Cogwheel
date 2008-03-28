using System;
using BeeDevelopment.Brazil;
namespace BeeDevelopment.Sega8Bit.Hardware {

	public partial class VideoDisplayProcessor {

		#region Types
		
		/// <summary>
		/// Defines the four mode bits.
		/// </summary>
		[Flags()]
		private enum ModeBits {
			/// <summary>
			/// No mode bits are set.
			/// </summary>
			None = 0x00,
			/// <summary>
			/// The M2 mode bit.
			/// </summary>
			M2 = 0x02,
			/// <summary>
			/// The M4 mode bit.
			/// </summary>
			M4 = 0x04,
			/// <summary>
			/// The M3 mode bit.
			/// </summary>
			M3 = 0x08,
			/// <summary>
			/// The M3 mode bit.
			/// </summary>
			M1 = 0x10,
		}

		#endregion

		/// <summary>
		/// Accesses the video registers.
		/// </summary>
		public byte[] Registers { get { return this.registers; } }
		private byte[] registers;

		#region Register bit manipulation and testing functions

		/// <summary>
		/// Gets a bit from a register.
		/// </summary>
		/// <param name="register">The register index to retrieve a bit from.</param>
		/// <param name="bitIndex">The index of the bit to retrieve.</param>
		/// <returns>True if the bit is set, false if it is reset.</returns>
		private bool GetBit(int register, int bitIndex) {
			return (Registers[register] & (1 << bitIndex)) != 0;
		}

		/// <summary>
		/// Sets a bit in a register.
		/// </summary>
		/// <param name="register">The index of the register to change.</param>
		/// <param name="bitIndex">The index of the bit to set.</param>
		private void SetBit(int register, int bitIndex) {
			Registers[register] |= (byte)(1 << bitIndex);
		}


		/// <summary>
		/// Resets a bit in a register.
		/// </summary>
		/// <param name="register">The index of the register to change.</param>
		/// <param name="bitIndex">The index of the bit to reset.</param>
		private void ResetBit(int register, int bitIndex) {
			Registers[register] &= (byte)~(1 << bitIndex);
		}


		/// <summary>
		/// Sets or resets a bit in a register.
		/// </summary>
		/// <param name="register">The index of the register to change.</param>
		/// <param name="bitIndex">The index of the bit to set or reset.</param>
		/// <param name="set">True to set the bit, false to reset it.</param>
		private void AdjustBit(int register, int bitIndex, bool set) {
			if (set) {
				this.SetBit(register, bitIndex);
			} else {
				this.ResetBit(register, bitIndex);
			}
		}

		#endregion

		#region Register $00 flags

		/// <summary>
		/// Set to disable vertical scrolling for columns 24-31.
		/// </summary>
		[StateNotSaved()]
		public bool InhibitScrollY {
			get { return GetBit(0x0, 7); }
			set { AdjustBit(0x0, 7, value); }
		}

		/// <summary>
		/// Set to disable horizontal scrolling for rows 0-1.
		/// </summary>
		[StateNotSaved()]
		public bool InhibitScrollX {
			get { return GetBit(0x0, 6); }
			set { AdjustBit(0x0, 6, value); }
		}

		/// <summary>
		/// Set to mask column 0 with border colour.
		/// </summary>
		[StateNotSaved()]
		public bool MaskColumn0 {
			get { return GetBit(0x0, 5); }
			set { AdjustBit(0x0, 5, value); }
		}

		/// <summary>
		/// (IE1) Set to enable raster line interrupts.
		/// </summary>
		[StateNotSaved()]
		public bool LineInterruptEnable {
			get { return GetBit(0x0, 4); }
			set { AdjustBit(0x0, 4, value); }
		}

		/// <summary>
		/// (EC) Set to shift sprites left by 8 pixels.
		/// </summary>
		[StateNotSaved()]
		public bool EarlyClock {
			get { return GetBit(0x0, 3); }
			set { AdjustBit(0x0, 3, value); }
		}

		/// <summary>
		/// (M4) Set to use mode 4, reset to use TMS9918 modes.
		/// </summary>
		[StateNotSaved()]
		public bool UseMode4 {
			get { return GetBit(0x0, 2); }
			set { AdjustBit(0x0, 2, value); }
		}

		/// <summary>
		/// (M3) Set to allow mode 4 screen height changes.
		/// </summary>
		[StateNotSaved()]
		public bool AllowMode4HeightChanges {
			get { return GetBit(0x0, 1); }
			set { AdjustBit(0x0, 1, value); }
		}

		/// <summary>
		/// Set to disable synch.
		/// </summary>
		[StateNotSaved()]
		public bool NoSynch {
			get { return GetBit(0x0, 0); }
			set { AdjustBit(0x0, 0, value); }
		}

		#endregion

		#region Register $01 flags

		/// <summary>
		/// (BLK) Set to make the display visible, reset to blank it.
		/// </summary>
		[StateNotSaved()]
		public bool DisplayVisible {
			get { return GetBit(0x1, 6); }
			set { AdjustBit(0x1, 6, value); }
		}

		/// <summary>
		/// (IE0) Set to enable frame interrupts.
		/// </summary>
		[StateNotSaved()]
		public bool FrameInterruptEnable {
			get { return GetBit(0x1, 5); }
			set { AdjustBit(0x1, 5, value); }
		}

		/// <summary>
		/// (M1) Set to use 224-line screen when in Mode 4.
		/// </summary>
		/// <remarks>M2 must also be set to allow the change.</remarks>
		[StateNotSaved()]
		public bool Use224LineMode {
			get { return GetBit(0x1, 4); }
			set { AdjustBit(0x1, 4, value); }
		}

		/// <summary>
		/// (M3) Set to use 240-line screen when in Mode 4.
		/// </summary>
		/// <remarks>M2 must also be set to allow the change.</remarks>
		[StateNotSaved()]
		public bool Use240LineMode {
			get { return GetBit(0x1, 3); }
			set { AdjustBit(0x1, 3, value); }
		}

		/// <summary>
		/// Set to join sprites (16x16 in TMS9918 modes, 8x16 in mode 4).
		/// </summary>
		[StateNotSaved()]
		public bool JoinSprites {
			get { return GetBit(0x1, 1); }
			set { AdjustBit(0x1, 1, value); }
		}

		/// <summary>
		/// Set to zoom sprites to double size.
		/// </summary>
		[StateNotSaved()]
		public bool ZoomSprites {
			get { return GetBit(0x1, 0); }
			set { AdjustBit(0x1, 0, value); }
		}

		#endregion


		/// <summary>
		/// Gets the current mode bits.
		/// </summary>
		private ModeBits CurrentModeBits {
			get { return (ModeBits)((Registers[0x0] & 0x06) | (Registers[0x1] & 0x18)); }
		}

		/// <summary>
		/// Gets the current screen resolution height.
		/// </summary>
		private int VerticalResolution {
			get {
				if (!this.SupportsExtendedResolutions) {
					return 192;
				} else {
					ModeBits M = this.CurrentModeBits;

					// If M2 is not set or M4 is not set, standard resolution.
					if ((M & ModeBits.M2) == 0 || (M & ModeBits.M4) == 0) return 192;

					// If M1 and M3 are both set, standard resolution.
					if ((M & (ModeBits.M1 | ModeBits.M3)) == (ModeBits.M1 | ModeBits.M3)) {
						return 192;
					} else if ((M & ModeBits.M1) == ModeBits.M1) {
						return 224;
					} else if ((M & ModeBits.M3) == ModeBits.M3) {
						return 240;
					} else {
						return 192;
					}
				}
			}
		}

		/// <summary>
		/// Gets the current <see cref="VideoDisplayProcessor"/> mode.
		/// </summary>
		[StateNotSaved()]
		public Mode CurrentMode {
			get {

				ModeBits MaskedFlags = this.CurrentModeBits;
				if (!this.SupportsMode4) MaskedFlags &= ~ModeBits.M4;

				switch (MaskedFlags) {

					case ModeBits.None:
						return Mode.Graphic1;
					case ModeBits.M1:
						return Mode.Text;
					case ModeBits.M2:
						return Mode.Graphic2;
					case ModeBits.M3:
						return Mode.Multicolour;

					case ModeBits.M1 | ModeBits.M2:
					case ModeBits.M2 | ModeBits.M3:
					case ModeBits.M1 | ModeBits.M2 | ModeBits.M3:
						return Mode.Invalid;

					case ModeBits.M4:
					case ModeBits.M4 | ModeBits.M2:
					case ModeBits.M4 | ModeBits.M3:
						return Mode.Mode4;

					case ModeBits.M4 | ModeBits.M1 | ModeBits.M2:
						return this.SupportsExtendedResolutions ?
							Mode.Mode4Resolution224 : Mode.Invalid; ;

					case ModeBits.M4 | ModeBits.M1 | ModeBits.M2 | ModeBits.M3:
						return this.SupportsExtendedResolutions ?
							Mode.Mode4 : Mode.Invalid;

					default:
						return Mode.Invalid;

				}
			}
		}

	}
}