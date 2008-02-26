/* 
 * This source file relates to memory access.
 */
namespace BeeDevelopment.Sega8Bit {

	public partial class Emulator {

		private byte[] ram;
		/// <summary>
		/// Gets the work RAM of the emulator.
		/// </summary>
		public byte[] Ram { get { return this.ram; } }

		/// <summary>
		/// Gets or sets the <see cref="ICartridgeMapper"/> inserted in the console's cartridge slot.
		/// </summary>
		public Mappers.ICartridgeMapper Cartridge { get; set; }

		/// <summary>
		/// Gets a <see cref="MemoryCheatCollection"/> of cheats.
		/// </summary>
		public MemoryCheatCollection Cheats { get; private set; }

		/// <summary>
		/// Writes a byte to memory.
		/// </summary>
		/// <param name="address">The address to write to .</param>
		/// <param name="value">The data to write.</param>
		public override void WriteMemory(ushort address, byte value) {

			// Writes to RAM.
			if (address >= 0xC000) this.ram[address & 0x1FFF] = value;

			// Writes to cartridge.
			if (this.Cartridge != null) this.Cartridge.WriteMemory(address, value);

		}


		/// <summary>
		/// Reads a byte from memory.
		/// </summary>
		/// <param name="address">The address to read from.</param>
		/// <returns>The byte read from memory from address <paramref name="address"/>.</returns>
		public override byte ReadMemory(ushort address) {

			// Reads from RAM.
			if (address >= 0xC000) return this.ram[address & 0x1FFF];

			// Reads from cartridge slot.
			if (this.Cartridge != null) {

				byte Source = this.Cartridge.ReadMemory(address);

				if (!this.Cheats.Enabled) return Source;

				var Cheat = this.Cheats[address];

		
				if (Cheat != null && Cheat.Original == Source) {
					return Cheat.Replacement;
				} else {
					return Source;
				}
			}

			return 0xFF; // Default.
		}

		/// <summary>
		/// Resets all memory, including RAM and any inserted cartridges.
		/// </summary>
		public void ResetMemory() {
			this.ram = new byte[0x2000]; // 8KB RAM
			if (this.Cartridge != null) this.Cartridge.Reset();
		}

		

	}
}
