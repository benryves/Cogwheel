using System;
using System.Collections.Generic;
using BeeDevelopment.Brazil;

namespace BeeDevelopment.Sega8Bit.Mappers {

	/// <summary>
	/// The standard Sega cartridge mapper.
	/// </summary>
	[Serializable()]
	public class Standard : IMemoryMapper {

		#region Private Fields

		/// <summary>
		/// The entire cartridge ROM in its "normal" order.
		/// </summary>
		protected byte[][] pagedCartridgeRom;

		/// <summary>
		/// Gets or sets the contents of ROM.
		/// </summary>
		public byte[] Rom {
			get {
				var Result = new List<byte>(16 * 1024);
				foreach (var Page in this.pagedCartridgeRom) Result.AddRange(Page);
				return Result.ToArray();
			}
			set {
				if (value == null) throw new InvalidOperationException();
				this.Load(value);
			}
		}

		/// <summary>
		/// The entire cartridge RAM.
		/// </summary>
		protected byte[] cartridgeRam;

		/// <summary>
		/// Gets or sets the contents of RAM.
		/// </summary>
		public byte[] Ram {
			get { return this.cartridgeRam; }
			set {
				if (value == null || value.Length != this.cartridgeRam.Length) throw new InvalidOperationException();
				this.cartridgeRam = value;
			}
		}

		protected virtual void UpdateMapperFromBankNumbers() {
			this.MemoryModel[0] = this.pagedCartridgeRom[this.bankNumbers[0] % this.pagedCartridgeRom.Length];
			this.MemoryModel[1] = this.pagedCartridgeRom[this.bankNumbers[1] % this.pagedCartridgeRom.Length];
			this.MemoryModel[2] = this.RamEnabled ? this.cartridgeRam : this.pagedCartridgeRom[this.bankNumbers[2] % this.pagedCartridgeRom.Length];
		}

		protected int[] bankNumbers;
		/// <summary>
		/// Gets or sets the current ROM bank numbers.
		/// </summary>
		public int[] BankNumbers {
			get { return this.bankNumbers; }
			set {
				if (value == null || value.Length != this.bankNumbers.Length) throw new InvalidOperationException();
				this.bankNumbers = value;
				this.UpdateMapperFromBankNumbers();
			}
		}

		/// <summary>
		/// Maps a slot to a 16KB array of bytes.
		/// </summary>
		protected byte[][] MemoryModel;

		/// <summary>
		/// Bank number offset.
		/// </summary>
		public int BankNumberOffset { get; set; }

		/// <summary>
		/// Gets whether the on-cartridge RAM is enabled.
		/// </summary>
		public bool RamEnabled { get; set; }

		/// <summary>
		/// Gets or sets whether the first kilobyte should be protected or not.
		/// </summary>
		public bool ProtectFirstKilobyte { get; set; }

		/// <summary>
		/// Gets the CRC-32 checksum of the ROM contents.
		/// </summary>
		[StateNotSaved]
		public int Crc32 { get; private set; }

		#endregion


		#region Reading and Writing

		/// <summary>
		/// Reads a byte from memory.
		/// </summary>
		/// <param name="address">The address to read from.</param>
		/// <returns>The byte read from memory from address <paramref name="address"/>.</returns>
		public virtual byte ReadMemory(ushort address) {

			switch (address & 0xC000) {
				case 0x0000:
					if (address < 1024 && this.ProtectFirstKilobyte) {
						return this.pagedCartridgeRom[0][address];
					} else {
						return this.MemoryModel[0][address & 0x3FFF];
					}
				case 0x4000:
					return this.MemoryModel[1][address & 0x3FFF];
				case 0x8000:
					return this.MemoryModel[2][address & 0x3FFF];
				default:
					return 0xFF;
			}
		}

		/// <summary>
		/// Writes a byte to memory.
		/// </summary>
		/// <param name="address">The address to write to .</param>
		/// <param name="value">The data to write.</param>
		public virtual void WriteMemory(ushort address, byte value) {

			if (address == 0xFFFC) {

				this.BankNumberOffset = (value & 3) * 8;

				if ((value & 0x08) != 0) {
					// Cartridge RAM enabled
					this.RamEnabled = true;
					this.MemoryModel[2] = cartridgeRam;
				} else {
					// Cartridge ROM enabled
					this.RamEnabled = false;
					this.MemoryModel[2] = this.pagedCartridgeRom[this.bankNumbers[2]];
				}


			} else if (address > 0xFFFC) {

				int SwitchedBank = address - 0xFFFD;
				this.bankNumbers[SwitchedBank] = (value + BankNumberOffset) % this.pagedCartridgeRom.Length;

				if (!(SwitchedBank == 2 && this.RamEnabled)) {
					this.MemoryModel[SwitchedBank] = this.pagedCartridgeRom[this.bankNumbers[SwitchedBank]];
				}
			}

			if (this.RamEnabled && (address & 0xC000) == 0x8000) {
				this.cartridgeRam[address & 0x3FFF] = value;
			}

		}

		#endregion

		#region Initialisation

		/// <summary>
		/// Resets the mapper to its default state.
		/// </summary>
		public virtual void Reset() {

			// Clear cartridge RAM.
			this.cartridgeRam = new byte[0x4000];

			// Write default mapper values.
			this.WriteMemory(0xFFFC, 0);
			this.WriteMemory(0xFFFD, 0);
			this.WriteMemory(0xFFFE, 1);
			this.WriteMemory(0xFFFF, 2);
		}

		/// <summary>
		/// Loads ROM data.
		/// </summary>
		/// <param name="data">Data taken from a ROM dump.</param>
		public virtual void Load(byte[] data) {
			
			// First, calculate the smallest power-of-two > 2 pages that we'll offer.
			int PageCount = 2;
			while (PageCount * 0x4000 < data.Length) PageCount <<= 1;
			this.pagedCartridgeRom = new byte[PageCount][];

			// Now copy the pages over.
			for (int i = 0, StartCopy = 0; i < PageCount; ++i, StartCopy += 0x4000) {
				this.pagedCartridgeRom[i] = new byte[0x4000];
				if (StartCopy < data.Length) {
					Array.Copy(data, StartCopy, this.pagedCartridgeRom[i], 0, Math.Min(0x4000, data.Length - StartCopy));
				}
			}

			// Finally, reset the state of the ROM mapper.
			this.Reset();

			this.Crc32 = Zip.Crc32.Calculate(data);
		}

		/// <summary>
		/// Creates an instance of the <see cref="Standard"/> mapper.
		/// </summary>
		public Standard() {
			this.bankNumbers = new int[3]; // Three bank numbers.
			this.MemoryModel = new byte[4][]; // Four slots.
			for (int i = 0; i < 4; i++) this.MemoryModel[i] = new byte[0x4000]; // 16KB per slot.
			
			// Load a dummy ROM to reset state.
			this.Load(new byte[0]);
		}

		#endregion


	}
}
