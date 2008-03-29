/* 
 * This source file relates to memory access.
 */
using BeeDevelopment.Brazil;
namespace BeeDevelopment.Sega8Bit {

	public partial class Emulator {

		#region Memory Devices

		/// <summary>Gets the <see cref="MemoryDevice"/> that represents the console's cartridge slot.</summary>
		[StateNotSaved()]
		public MemoryDevice CartridgeSlot { get; private set; }
		/// <summary>Gets the <see cref="MemoryDevice"/> that represents the console's card slot.</summary>
		[StateNotSaved()]
		public MemoryDevice CardSlot { get; private set; }
		/// <summary>Gets the <see cref="MemoryDevice"/> that represents the console's expansion slot.</summary>
		[StateNotSaved()]
		public MemoryDevice ExpansionSlot { get; private set; }
		/// <summary>Gets the <see cref="MemoryDevice"/> that represents the console's BIOS ROM.</summary>
		[StateNotSaved()]
		public MemoryDevice Bios { get; private set; }
		/// <summary>Gets the <see cref="MemoryDevice"/> that represents the console's work RAM.</summary>
		[StateNotSaved()]
		public MemoryDevice WorkRam { get; private set; }

		#endregion

		/// <summary>
		/// Gets a <see cref="MemoryCheatCollection"/> of cheats.
		/// </summary>
		[StateNotSaved()]
		public MemoryCheatCollection Cheats { get; private set; }

		/// <summary>
		/// Writes a byte to memory.
		/// </summary>
		/// <param name="address">The address to write to .</param>
		/// <param name="value">The data to write.</param>
		public override void WriteMemory(ushort address, byte value) {
			// Writes to RAM.
			if (address >= 0xC000) this.WorkRam.Write(address, value);
			// Writes to all other devices.
			this.CartridgeSlot.Write(address, value);
			this.CardSlot.Write(address, value);
			this.ExpansionSlot.Write(address, value);
			this.Bios.Write(address, value);

		}

		/// <summary>
		/// Reads a byte from memory.
		/// </summary>
		/// <param name="address">The address to read from.</param>
		/// <returns>The byte read from memory from address <paramref name="address"/>.</returns>
		public override byte ReadMemory(ushort address) {
			// Reads from RAM.
			if (address >= 0xC000) {
				return this.WorkRam.Read(address);
			} else {
				//HACK: This is a nasty kludge to prevent the Game Gear BIOS and catridge ROM from being AND-ed together.
				if (this.Bios.Memory != null && this.Bios.Memory is Mappers.Shared1KBios) {
					if (this.Bios.Enabled) {
						return this.Bios.Read(address);
					} else {
						return this.CartridgeSlot.Read(address);
					}
				} else {
					return (byte)(
						this.CartridgeSlot.Read(address) &
						this.CardSlot.Read(address) &
						this.ExpansionSlot.Read(address) &
						this.Bios.Read(address)
					);
				}
			}
		}

		/// <summary>
		/// Resets all memory, including RAM and any inserted cartridges.
		/// </summary>
		public void ResetMemory() {
			this.CartridgeSlot.Reset();
			this.CardSlot.Reset();
			this.ExpansionSlot.Reset();
			this.Bios.Reset();
			this.WorkRam.Reset();
		}

		/// <summary>
		/// Removes all inserted media from the emulator.
		/// </summary>
		public void RemoveAllMedia() {
			this.CartridgeSlot.Memory = null;
			this.Bios.Memory = null;
			this.CardSlot.Memory = null;
			this.ExpansionSlot.Memory = null;
		}

		

	}
}
