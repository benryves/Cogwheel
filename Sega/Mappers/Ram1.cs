using System;

namespace BeeDevelopment.Sega8Bit.Mappers {

	/// <summary>
	/// A simple 1KB of RAM.
	/// </summary>
	[Serializable()]
	public class Ram1 : IMemoryMapper {

		#region Private Fields

		/// <summary>
		/// The entire cartridge RAM.
		/// </summary>
		private byte[] memory;

		#endregion

		#region Public Properties

		/// <summary>Gets or sets the RAM data as a an array of bytes.</summary>
		public byte[] Memory {
			get { return this.memory; }
			set {
				if (value == null || value.Length != this.memory.Length) throw new InvalidOperationException();
				this.memory = value;
			}
		}

		#endregion

		#region Reading and Writing

		/// <summary>
		/// Reads a byte from memory.
		/// </summary>
		/// <param name="address">The address to read from.</param>
		/// <returns>The byte read from memory from address <paramref name="address"/>.</returns>
		public byte ReadMemory(ushort address) {
			return this.memory[address & 0x3FF];
		}

		/// <summary>
		/// Writes a byte to memory.
		/// </summary>
		/// <param name="address">The address to write to .</param>
		/// <param name="value">The data to write.</param>
		public void WriteMemory(ushort address, byte value) {
			this.memory[address & 0x3FF] = value;
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
			Array.Copy(data,this.memory,Math.Min(data.Length, this.memory.Length));
		}

		/// <summary>
		/// Creates an instance of the <see cref="Ram1"/> mapper.
		/// </summary>
		public Ram1() {
			this.memory = new byte[0x400];
		}

		#endregion
	}
}
