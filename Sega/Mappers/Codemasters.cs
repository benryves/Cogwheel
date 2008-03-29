using System;

namespace BeeDevelopment.Sega8Bit.Mappers {

	/// <summary>
	/// The Codemasters cartridge mapper.
	/// </summary>
	[Serializable()]
	public class Codemasters : Standard, IMemoryMapper {

		protected override void UpdateMapperFromBankNumbers() {
			for (int i = 0; i < 3; ++i) {
				this.MemoryModel[i] = this.pagedCartridgeRom[this.bankNumbers[i] % this.pagedCartridgeRom.Length];
			}
			
		}
		
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
					this.bankNumbers[SwitchedBank] = value % this.pagedCartridgeRom.Length;
					this.MemoryModel[SwitchedBank] = this.pagedCartridgeRom[this.bankNumbers[SwitchedBank]];
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
			this.cartridgeRam = new byte[0x4000];

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
			this.ProtectFirstKilobyte = false;
		}

		#endregion


	}
}
