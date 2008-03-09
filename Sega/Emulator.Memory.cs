/* 
 * This source file relates to memory access.
 */
namespace BeeDevelopment.Sega8Bit {

	public partial class Emulator {

		#region Enabled Ports

		/// <summary>Gets or sets whether the expansion slot is enabled.</summary>
		public bool ExpansionSlotEnabled { get; set; }

		/// <summary>Gets or sets whether the cartridge slot is enabled.</summary>
		public bool CartridgeSlotEnabled { get; set; }

		/// <summary>Gets or sets whether the card slot is enabled.</summary>
		public bool CardSlotEnabled { get; set; }

		/// <summary>Gets or sets whether the work RAM is enabled.</summary>
		public bool RamEnabled { get; set; }

		/// <summary>Gets or sets whether the BIOS ROM is enabled.</summary>
		public bool BiosEnabled { get; set; }

		#endregion

		private byte[] ram;
		/// <summary>
		/// Gets the work RAM of the emulator.
		/// </summary>
		public byte[] Ram { get { return this.ram; } }

		/// <summary>
		/// Gets or sets the <see cref="ICartridgeMapper"/> inserted in the console's cartridge slot.
		/// </summary>
		public Mappers.IMemoryMapper Cartridge { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="ICartridgeMapper"/> containing the console's ROM BIOS.
		/// </summary>
		public Mappers.IMemoryMapper Bios { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="ICartridgeMapper"/> inserted in the console's card slot.
		/// </summary>
		public Mappers.IMemoryMapper CardSlot { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="ICartridgeMapper"/> connected to the console's expansion slot.
		/// </summary>
		public Mappers.IMemoryMapper ExpansionSlot { get; set; }

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
			if (address >= 0xC000) {
				return this.ram[address & 0x1FFF];
			} else {


				// Reads from BIOS ROM.
				if (this.BiosEnabled && this.Bios != null) return this.Bios.ReadMemory(address);

				// Reads from card slot.
				if (this.CardSlotEnabled && this.CardSlot != null) return this.CardSlot.ReadMemory(address);

				// Reads from expansion slot.
				if (this.ExpansionSlotEnabled && this.ExpansionSlot != null) return this.ExpansionSlot.ReadMemory(address);

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
		}

		/// <summary>
		/// Resets all memory, including RAM and any inserted cartridges.
		/// </summary>
		public void ResetMemory() {
			this.ram = new byte[0x2000]; // 8KB RAM
			if (this.Cartridge != null) this.Cartridge.Reset();
			if (this.Bios != null) this.Bios.Reset();
			if (this.CardSlot != null) this.CardSlot.Reset();
			if (this.ExpansionSlot != null) this.ExpansionSlot.Reset();
			this.CartridgeSlotEnabled = true;
			this.BiosEnabled = false;
			this.CardSlotEnabled = false;
			this.ExpansionSlotEnabled = false;
			this.RamEnabled = true;
		}

		/// <summary>
		/// Removes all inserted media from the emulator.
		/// </summary>
		public void RemoveAllMedia() {
			this.Cartridge = null;
			this.Bios = null;
			this.CardSlot = null;
			this.ExpansionSlot = null;
		}

		

	}
}
