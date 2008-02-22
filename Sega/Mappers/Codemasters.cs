using System;

namespace BeeDevelopment.Sega8Bit.Mappers {

	/// <summary>
	/// The Codemasters cartridge mapper.
	/// </summary>
	public class Codemasters : Standard, ICartridgeMapper {

		#region Reading and Writing

		/// <summary>
		/// Writes a byte to memory.
		/// </summary>
		/// <param name="address">The address to write to .</param>
		/// <param name="value">The data to write.</param>
		public override void WriteMemory(ushort address, byte value) {

			switch (address) {
				case 0x0000:
				case 0x4000:
				case 0x8000:
					int SwitchedBank = address / 0x4000;
					this.BankNumbers[SwitchedBank] = value % this.CartridgeRom.Length;
					this.MemoryModel[SwitchedBank] = this.CartridgeRom[this.BankNumbers[SwitchedBank]];
					break;
			}

		}

		#endregion

		#region Initialisation

		/// <summary>
		/// Resets the mapper to its default state.
		/// </summary>
		public override void Reset() {

			// Clear cartridge RAM.
			this.CartridgeRam = new byte[0x4000];

			// Write default mapper values.
			this.WriteMemory(0x0000, 0);
			this.WriteMemory(0x4000, 1);
			this.WriteMemory(0x8000, 2);
		}

		

		/// <summary>
		/// Creates an instance of the <see cref="Codemasters"/> mapper.
		/// </summary>
		public Codemasters()
			: base() {
		}

		#endregion


	}
}
