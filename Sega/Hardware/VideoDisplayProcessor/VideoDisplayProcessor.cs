using System;
using BeeDevelopment.Brazil;
namespace BeeDevelopment.Sega8Bit.Hardware {
	/// <summary>
	/// Emulates a TMS9918A video display processor with Sega Master System and Sega Game Gear extensions.
	/// </summary>
	[Serializable()]
	public partial class VideoDisplayProcessor {

		#region Types

		/// <summary>
		/// Defines the various VDP modes.
		/// </summary>
		public enum Mode {
			/// <summary>
			/// TMS9918 "Graphic I" mode.
			/// </summary>
			Graphic1 = 0,
			/// <summary>
			/// TMS9918 "Text" mode.
			/// </summary>
			Text = 1,
			/// <summary>
			/// TMS9918 "Graphic II" mode.
			/// </summary>
			Graphic2 = 2,
			/// <summary>
			/// TMS9918 "Multicolor" mode.
			/// </summary>
			Multicolor = 4,
			/// <summary>
			/// Sega Master System and Game Gear extended mode 4.
			/// </summary>
			Mode4 = 8,
			/// <summary>
			/// Sega Master System and Game Gear extended mode 4 with an increased 224 line resolution.
			/// </summary>
			Mode4Resolution224 = 11,
			/// <summary>
			/// Sega Master System and Game Gear extended mode 4 with an increased 240 line resolution.
			/// </summary>
			Mode4Resolution240 = 14,
			/// <summary>
			/// Represents an invalid or unsupported display mode.
			/// </summary>
			Invalid = -1,
		}

		/// <summary>
		/// Defines the supported video systems.
		/// </summary>
		public enum VideoSystem {
			/// <summary>
			/// 60Hz NTSC (Japan, USA, Brazil).
			/// </summary>
			Ntsc,
			/// <summary>
			/// 50Hz PAL (Europe, Australia).
			/// </summary>
			Pal,
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the current video system.
		/// </summary>
		public VideoSystem System { get; set; }

		/// <summary>
		/// Gets the <see cref="Emulator"/> that contains this <see cref="VideoDisplayProcessor"/>.
		/// </summary>
		[StateNotSaved()]
		public Emulator Emulator { get; private set; }

		/// <summary>
		/// Gets the open shutter of the Sega 3D glasses during the active display from the last frame.
		/// </summary>
		[StateNotSaved()]
		public Emulator.GlassesShutter LastOpenGlassesShutter { get; private set; }

		#endregion

		#region Initialisation

		/// <summary>
		/// Reset the <see cref="VideoDisplayProcessor"/> to its default state.
		/// </summary>
		public void Reset() {

			// Clear out the VRAM.
			this.vram = new byte[0x4000]; // 16KB
			this.FastPixelColourIndex = new int[512 * 8 * 8];

			// Resets registers to default settings.
			this.registers = new byte[] {
				0x06, 0xA0, 0xFF, 0xFF, 0xFF, 0xFF, 0xFB, 0xF0,
				0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00 
			};

			// Clear colour RAM to black.
			this.colourRam = new int[32];

			// Clear interrupt pending flags.
			this.lineInterruptPending = false;
			this.frameInterruptPending = false;

			// Reset to bog standard SMS mode.
			this.SetCapabilitiesByModelAndVideoSystem(HardwareModel.MasterSystem2, VideoSystem.Ntsc); // That'll do as a default.

			// Reset I/O state.
			this.WaitingForSecond = false;
			this.accessMode = DataAccessMode.VideoRamReadWrite;
			this.readBuffer = 0x00;
			this.LatchedColourRamData = 0x00;

			// 60Hz smoothness at the cost of not supporting 240 line mode.
			this.System = VideoSystem.Ntsc;

			// Clear out the pixel buffer.
			this.PixelBuffer = new int[256 * 256];
			this.TempPixelBuffer = new int[256 * 256];
			this.BackdropBuffer = new int[256];

			this.spriteLayerEnabled = true;
			this.backgroundLayerEnabled = true;

			this.BeginFrame();
	
		}

		/// <summary>
		/// Creates an instance of the <see cref="VideoDisplayProcessor"/> and resets it.
		/// </summary>
		public VideoDisplayProcessor(Emulator emulator) {
			this.Emulator = emulator;
			this.SetupTimingCounters(); // Not fun.
			this.SetCapabilitiesByModelAndVideoSystem(HardwareModel.MasterSystem2, VideoSystem.Ntsc);
			this.Reset();
		}

		#endregion
	}
}