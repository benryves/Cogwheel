using System;

namespace BeeDevelopment.Sega8Bit {
	/// <summary>
	/// Represents a byte of data that was written to an I/O device or memory.
	/// </summary>
	public class DataWrittenEventArgs : EventArgs {

		/// <summary>
		/// The address that the data was written to (if applicable).
		/// </summary>
		public int Address { get; private set; }

		/// <summary>
		/// The value that was written.
		/// </summary>
		public byte Data { get; private set; }

		/// <summary>
		/// Creates an instance of the <see cref="DataWrittenEventArgs"/> class.
		/// </summary>
		/// <param name="address">The address that the data was written to (if applicable).</param>
		/// <param name="data">The data that was written.</param>
		public DataWrittenEventArgs(int address, byte data) {
			this.Address = address;
			this.Data = data;
		}
	}
}
