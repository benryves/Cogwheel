namespace BeeDevelopment.Sega8Bit.Mappers {

	/// <summary>
	/// Provides a base interface for cartridge mappers.
	/// </summary>
	public interface IMemoryMapper {

		/// <summary>
		/// Reads a byte from memory.
		/// </summary>
		/// <param name="address">The address to read from.</param>
		/// <returns>The byte read from memory from address <paramref name="address"/>.</returns>
		byte ReadMemory(ushort address);

		/// <summary>
		/// Writes a byte to memory.
		/// </summary>
		/// <param name="address">The address to write to .</param>
		/// <param name="value">The data to write.</param>
		void WriteMemory(ushort address, byte value);

		/// <summary>
		/// Resets the mapper to its default state.
		/// </summary>
		void Reset();

		/// <summary>
		/// Loads ROM data.
		/// </summary>
		/// <param name="data">Data taken from a ROM dump.</param>
		void Load(byte[] data);
	
	}
}
