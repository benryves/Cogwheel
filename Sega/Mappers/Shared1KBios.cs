using System;

namespace BeeDevelopment.Sega8Bit.Mappers {

	/// <summary>
	/// A 1KB BIOS ROM that shares the address space with another mapper.
	/// </summary>
	[Serializable()]
	public class Shared1KBios : IMemoryMapper {

		#region Private Fields

		/// <summary>
		/// The entire BIOS ROM.
		/// </summary>
		private byte[] Memory;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the <see cref="IMemoryMapper"/> that shares the address space with the 1KB BIOS ROM.
		/// </summary>
		public IMemoryMapper SharedMapper { get; set; }

		#endregion

		#region Reading and Writing

		/// <summary>
		/// Reads a byte from memory.
		/// </summary>
		/// <param name="address">The address to read from.</param>
		/// <returns>The byte read from memory from address <paramref name="address"/>.</returns>
		public byte ReadMemory(ushort address) {
			if (address < this.Memory.Length) {
				return this.Memory[address];
			} else {
				return (this.SharedMapper != null) ? this.SharedMapper.ReadMemory(address) : (byte)0xFF;
			}
		}

		/// <summary>
		/// Writes a byte to memory.
		/// </summary>
		/// <param name="address">The address to write to .</param>
		/// <param name="value">The data to write.</param>
		public void WriteMemory(ushort address, byte value) {
			if (this.SharedMapper != null) this.SharedMapper.WriteMemory(address, value);
		}

		#endregion

		#region Initialisation

		/// <summary>
		/// Resets the mapper to its default state.
		/// </summary>
		public void Reset() {
			// Do nothing...
		}

		/// <summary>
		/// Loads ROM data.
		/// </summary>
		/// <param name="data">Data taken from a ROM dump.</param>
		public void Load(byte[] data) {
			Array.Copy(data,this.Memory,Math.Min(data.Length, this.Memory.Length));
		}

		/// <summary>
		/// Creates an instance of the <see cref="Shared1KBios"/> mapper.
		/// </summary>
		public Shared1KBios() {
			this.Memory = new byte[1024];
		}

		#endregion
	}
}
