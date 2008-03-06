using BeeDevelopment.Brazil;
using BeeDevelopment.Sega8Bit.Hardware;

namespace BeeDevelopment.Sega8Bit {

	/// <summary>
	/// Provides a basic Sega 8-bit emulator.
	/// </summary>
	public partial class Emulator : Z80A {

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
		}

		/// <summary>
		/// Creates an instance of the <see cref="Emulator"/> class.
		/// </summary>
		public Emulator() {
			this.Video = new VideoDisplayProcessor(this);
			this.Sound = new ProgrammableSoundGenerator(this);
			this.Cheats = new MemoryCheatCollection();
			this.ResetAll();
		}

		/// <summary>
		/// Runs the emulator for a single scanline.
		/// </summary>
		/// <returns>True if the last line of the active display has been rendered.</returns>
		public bool RunScanline() {
			this.FetchExecute(228);
			return this.Video.RasteriseLine();
		}

		/// <summary>
		/// Runs the emulator for a compete frame.
		/// </summary>
		public void RunFrame() {
			while (!this.RunScanline()) ;
		}

		private Region region;
		/// <summary>
		/// Gets or sets the region of the <see cref="Emulator"/>.
		/// </summary>
		public Region Region {
			get { return this.region; }
			set { 
				this.region = value;
				for (int i = 0; i < 2; ++i) {
					this.Ports[i].Region = this.region;					
				}
			}
		}

		/// <summary>
		/// Sets the capabilities of the <see cref="Emulator"/> based on a particular hardware version.
		/// </summary>
		/// <param name="model">The <see cref="HardwareModel"/> to base the capabilities on.</param>
		public void SetCapabilitiesByModel(HardwareModel model) {
			this.Video.SetCapabilitiesByModel(model);
			this.HasGameGearPorts = (model == HardwareModel.GameGear);
			this.HasStartButton = (model == HardwareModel.GameGear);
			this.HasResetButton = (model == HardwareModel.MasterSystem || model == HardwareModel.MasterSystem2);
			this.HasPauseButton = (model != HardwareModel.GameGear);
		}

	}
}