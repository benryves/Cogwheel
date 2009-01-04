using System;
using BeeDevelopment.Brazil;
using BeeDevelopment.Sega8Bit.Hardware;

namespace BeeDevelopment.Sega8Bit {

	/// <summary>
	/// Provides a basic Sega 8-bit emulator.
	/// </summary>
	[Serializable()]
	public partial class Emulator : Z80A {

		#region Public Properties

		/// <summary>
		/// Gets or sets the <see cref="Region"/> of the emulator.
		/// </summary>
		public Region Region { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="HardwareFamily"/> of the emulator.
		/// </summary>
		public HardwareFamily Family { get; set; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns the a <see cref="HardwareFamily"/> that a particular <see cref="HardwareModel"/> is a member of.
		/// </summary>
		/// <param name="model">The <see cref="HardwareModel"/> to find the family of.</param>
		/// <returns>The <see cref="HardwareFamily"/> that the <paramref="model"/> is a member of.</returns>
		public static HardwareFamily GetFamilyFromModel(HardwareModel model) {
			switch (model) {
				case HardwareModel.SG1000:
				case HardwareModel.MasterSystem:
				case HardwareModel.MasterSystem2:
				case HardwareModel.GameGear:
				case HardwareModel.GameGearMasterSystem:
					return HardwareFamily.MasterSystem;
				case HardwareModel.ColecoVision:
					return HardwareFamily.ColecoVision;
				case HardwareModel.SC3000:
				case HardwareModel.SF7000:
					return HardwareFamily.SC3000;
				default:
					return HardwareFamily.Default;
			}
		}


		/// <summary>
		/// Resets the emulator to its original state.
		/// </summary>
		public override void Reset() {
			base.Reset();
			this.RegisterSP = 0xDFF0;
		}

		/// <summary>
		/// Resets the state of all resettable hardware.
		/// </summary>
		public void ResetAll() {
			this.Reset();
			this.ResetMemory();
			this.ResetPorts();
			this.Video.Reset();
			this.Sound.Reset();
			this.MainPPI.Reset();
			this.DebugConsole.Reset();
#if EMU2413
			this.FmSound.Reset();
#endif
		}

		#endregion

		#region Construtor

		/// <summary>
		/// Creates an instance of the <see cref="Emulator"/> class.
		/// </summary>
		public Emulator() {
			this.Video = new VideoDisplayProcessor(this);
			this.Sound = new ProgrammableSoundGenerator(this);
			this.Cheats = new MemoryCheatCollection();
			this.MainPPI = new ProgrammablePeripheralInterface();
			this.DebugConsole = new DebugConsole(this);
#if EMU2413
			this.FmSound = new Emu2413();
#endif
			
			#region Memory

			this.CartridgeSlot = new MemoryDevice();
			this.CardSlot = new MemoryDevice();
			this.ExpansionSlot = new MemoryDevice();
			this.Bios = new MemoryDevice();
			this.WorkRam = new MemoryDevice() { Memory = new Mappers.Ram8() };

			#endregion

			this.ResetAll();
		}

		/// <summary>
		/// Runs the emulator for a single scanline.
		/// </summary>
		/// <returns>True if the last line of the active display has been rendered.</returns>
		public bool RunScanline() {
			this.FetchExecute(this.Video.CpuCyclesPerScanline);
			return this.Video.RasteriseLine();
		}


		/// <summary>
		/// Runs the emulator for a compete frame.
		/// </summary>
		public void RunFrame() {
			while (!this.RunScanline()) ;
		}



		/// <summary>
		/// Sets the capabilities of the <see cref="Emulator"/> based on a particular hardware version.
		/// </summary>
		/// <param name="model">The <see cref="HardwareModel"/> to base the capabilities on.</param>
		public void SetCapabilitiesByModelAndRegion(HardwareModel model, Region region) {

			// Family:
			this.Family = Emulator.GetFamilyFromModel(model);

			// Video:
			this.Video.SetCapabilitiesByModel(model);

			// Sound:
			switch (model) {
				case HardwareModel.SG1000:
				case HardwareModel.SC3000:
					this.Sound.SetNoiseByPreset(ProgrammableSoundGenerator.NoisePresets.SC3000);
					break;
				case HardwareModel.ColecoVision:
					this.Sound.SetNoiseByPreset(ProgrammableSoundGenerator.NoisePresets.BbcMicro);
					break;
				default:
					this.Sound.SetNoiseByPreset(ProgrammableSoundGenerator.NoisePresets.SegaMasterSystem);
					break;
			}

			this.HasGameGearPorts = (model == HardwareModel.GameGear || model == HardwareModel.GameGearMasterSystem);
			this.RespondsToGameGearPorts = (model == HardwareModel.GameGear);
			this.HasStartButton = (model == HardwareModel.GameGear);
			this.HasResetButton = (model == HardwareModel.MasterSystem || model == HardwareModel.MasterSystem2);
			this.HasPauseButton = !(model == HardwareModel.GameGear || model == HardwareModel.ColecoVision);

			for (int i = 0; i < 2; ++i) {
				this.SegaPorts[i].SupportsOutput = (model == HardwareModel.MasterSystem || model == HardwareModel.MasterSystem2) && region != Region.Japanese;
			}

			#region Memory

			switch (this.Family) {
				case HardwareFamily.ColecoVision:

					this.Bios.Accessibility = MemoryDevice.AccessibilityMode.Always;
					this.ExpansionSlot.Accessibility = MemoryDevice.AccessibilityMode.Always;
					this.WorkRam.Accessibility = MemoryDevice.AccessibilityMode.Always;
					this.CartridgeSlot.Accessibility = MemoryDevice.AccessibilityMode.Always;					
					this.CardSlot.Accessibility = MemoryDevice.AccessibilityMode.Never;

					// 1KB RAM (massive!)
					this.WorkRam.Memory = new Mappers.Ram1();

					break;

				default:

					// Make all memory devices available.
					this.CartridgeSlot.Accessibility = MemoryDevice.AccessibilityMode.Optional;
					this.CardSlot.Accessibility = MemoryDevice.AccessibilityMode.Optional;
					this.ExpansionSlot.Accessibility = MemoryDevice.AccessibilityMode.Optional;
					this.Bios.Accessibility = MemoryDevice.AccessibilityMode.Optional;
					this.WorkRam.Accessibility = MemoryDevice.AccessibilityMode.Optional;

					// If SMS2 or Game Gear - we have no card or expansion slots.
					if (model == HardwareModel.MasterSystem2 || model == HardwareModel.GameGear || model == HardwareModel.GameGearMasterSystem) {
						this.CardSlot.Accessibility = MemoryDevice.AccessibilityMode.Never;
						this.ExpansionSlot.Accessibility = MemoryDevice.AccessibilityMode.Never;
					}

					// If Game Gear, we cannot modify the state of the expansion, cartridge, card or IO.
					if (model == HardwareModel.GameGear || model == HardwareModel.GameGearMasterSystem) {
						this.CartridgeSlot.Accessibility = MemoryDevice.AccessibilityMode.Always;
						this.WorkRam.Accessibility = MemoryDevice.AccessibilityMode.Always;
					}

					// Set RAM size according to model.
					switch (model) {
						case HardwareModel.SG1000:
						case HardwareModel.SC3000:
							this.WorkRam.Memory = new Mappers.Ram2(); // 2KB RAM in the SG1000/SC3000.
							break;
						default:
							this.WorkRam.Memory = new Mappers.Ram8(); // 8KB RAM in everything else.
							break;
					}

					// Default enabled devices:
					this.CartridgeSlot.Enabled = false;
					this.CardSlot.Enabled = false;
					this.ExpansionSlot.Enabled = false;
					this.Bios.Enabled = true;
					this.WorkRam.Enabled = true;

					break;
			}

			#endregion

		}

		#endregion
	}
}