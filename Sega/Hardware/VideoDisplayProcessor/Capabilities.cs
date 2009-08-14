namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class VideoDisplayProcessor {

		private bool supportsMode4;
		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> supports the Sega Master System and Game Gear extended mode 4.
		/// </summary>
		/// <remarks>The regular TMS9918A <see cref="VideoDisplayProcessor"/> (as used in the SG-1000 and SC-3000) did not support mode 4.</remarks>
		public bool SupportsMode4 {
			get { return this.supportsMode4; }
			set { this.supportsMode4 = value; }
		}

		private bool supportsLineInterrupts;
		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> supports line interrupts.
		/// </summary>
		public bool SupportsLineInterrupts {
			get { return this.supportsLineInterrupts; }
			set { this.supportsLineInterrupts = value; }
		}

		private bool supportsFixedPalette;
		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> supports a fixed palette.
		/// </summary>
		public bool SupportsFixedPalette {
			get { return this.supportsFixedPalette; }
			set { this.supportsFixedPalette = value; }
		}

		private bool supportsVariablePalette;
		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> supports a variable palette.
		/// </summary>
		public bool SupportsVariablePalette {
			get { return this.supportsVariablePalette; }
			set { this.supportsVariablePalette = value; }
		}

		private bool extendedPalette;
		/// <summary>
		/// Gets or sets whether the device is using an extended (12-bit) palette.
		/// </summary>
		public bool ExtendedPalette {
			get { return this.extendedPalette; }
			set { this.extendedPalette = value; }
		}

		private int spritesPerScanline;
		/// <summary>
		/// Gets or sets the maximum number of displayable sprites per scanline.
		/// </summary>
		public int SpritesPerScanline {
			get { return this.spritesPerScanline; }
			set { this.spritesPerScanline = value; }
		}

		private int zoomedSpritesPerScanline;
		/// <summary>
		/// Gets or sets the maximum number of zoomed sprites per scanline.
		/// </summary>
		public int ZoomedSpritesPerScanline {
			get { return this.zoomedSpritesPerScanline; }
			set { this.zoomedSpritesPerScanline = value; }
		}

		private bool supportsExtendedResolutions;
		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> supports the extended 224 and 240 line resolutions.
		/// </summary>
		public bool SupportsExtendedResolutions {
			get { return this.supportsExtendedResolutions; }
			set { this.supportsExtendedResolutions = value; }
		}

		private bool supportsMirroredNameTable;
		/// <summary>
		/// Gets or sets whether rows 0..15 should be repeated as rows 16..27 when bit 0 of register 2 is cleared.
		/// </summary>
		/// <remarks>This feature was only present in the original SMS VDP.</remarks>
		public bool SupportsMirroredNameTable {
			get { return this.supportsMirroredNameTable; }
			set { this.supportsMirroredNameTable = value; }
		}

		private ResizingModes resizingMode;
		/// <summary>
		/// Gets or sets the method used to resize the display mode.
		/// </summary>
		public ResizingModes ResizingMode {
			get { return this.resizingMode; }
			set { this.resizingMode = value; }
		}

		/// <summary>
		/// Defines the possible resizing modes.
		/// </summary>
		public enum ResizingModes {
			/// <summary>
			/// The video frames are output at their native resolution.
			/// </summary>
			Normal,
			/// <summary>
			/// The video frames are cropped to a 160x144 resolution.
			/// </summary>
			Cropped,
			/// <summary>
			/// The video frames are scaled to a 160x144 resolution.
			/// </summary>
			Scaled,
		}

		/// <summary>
		/// Defines the types of CPU interrupt pin that the <see cref="VideoDisplayProcessor"/> can be attached to.
		/// </summary>
		public enum InterruptPins {
			/// <summary>The device is not connected to any interrupt pins.</summary>
			None,
			/// <summary>The device is not connected to the CPU's maskable interrupt pin.</summary>
			Maskable,
			/// <summary>The device is not connected to the CPU's non-maskable interrupt pin.</summary>
			NonMaskable,
		}

		private InterruptPins interruptPin;
		/// <summary>Gets or sets the <see cref="InterruptPins"/> that the device is attached to.</summary>
		public InterruptPins InterruptPin {
			get { return this.interruptPin; }
			set { this.interruptPin = value; }
		}

		private FixedPaletteModes fixedPaletteMode;
		/// <summary>
		/// Gets or sets the palette mode used with the legacy TMS9918 video modes.
		/// </summary>
		public FixedPaletteModes FixedPaletteMode {
			get { return this.fixedPaletteMode; }
			set { this.fixedPaletteMode = value; }
		}

		/// <summary>
		/// Defines the possible fixed colour palette types.
		/// </summary>
		public enum FixedPaletteModes {
			/// <summary>
			/// The original TMS9918 fixed colour palette.
			/// </summary>
			Tms9918,
			/// <summary>
			/// The truncated Sega Master System fixed colour palette.
			/// </summary>
			MasterSystem,
		}

		/// <summary>
		/// Sets the capabilities of the <see cref="VideoDisplayProcessor"/> based on a particular hardware version.
		/// </summary>
		/// <param name="model">The <see cref="HardwareModel"/> to base the capabilities on.</param>
		public void SetCapabilitiesByModelAndVideoSystem(HardwareModel model, VideoSystem videoSystem) {

			// Set the video system (Game Gear is only available in NTSC):
			switch (model) {
				case HardwareModel.GameGear:
				case HardwareModel.GameGearMasterSystem:
					this.System = VideoSystem.Ntsc;
					break;
				default:
					this.System = videoSystem;
					break;
			}

			// Is it a Mark III or later?
			bool Mark3OrLater = !(model == HardwareModel.SG1000 || model == HardwareModel.SC3000 || model == HardwareModel.SF7000 || model == HardwareModel.ColecoVision);

			// General.
			this.supportsMode4 = Mark3OrLater;
			this.supportsExtendedResolutions = Mark3OrLater && model != HardwareModel.MasterSystem; // Original SMS VDP doesn't support the extended resolutions.
			this.supportsMirroredNameTable = model == HardwareModel.MasterSystem;

			// Sprites.
			this.spritesPerScanline = Mark3OrLater ? 8 : 4;
			this.zoomedSpritesPerScanline = (Mark3OrLater && model != HardwareModel.MasterSystem) ? 8 : 4; // A bug in the SMS1 VDP means only the first four sprites are zoomed horizontally!

			// Colour.
			this.extendedPalette = model == HardwareModel.GameGear;
			this.supportsVariablePalette = Mark3OrLater;
			this.supportsFixedPalette = !(model == HardwareModel.GameGear || model == HardwareModel.GameGearMasterSystem); // The Game Gear VDP loses any sort of fixed TMS9918A palette.
			this.fixedPaletteMode = Mark3OrLater ? FixedPaletteModes.MasterSystem : FixedPaletteModes.Tms9918;
			this.resizingMode = (model == HardwareModel.GameGear ? ResizingModes.Cropped : (model == HardwareModel.GameGearMasterSystem ? ResizingModes.Scaled : ResizingModes.Normal));

			// Interrupts:
			this.supportsLineInterrupts = Mark3OrLater;
			this.interruptPin = model == HardwareModel.ColecoVision ? InterruptPins.NonMaskable : InterruptPins.Maskable;

			// Timing:
			this.cpuCyclesPerScanline = 228;

		}
	}
}