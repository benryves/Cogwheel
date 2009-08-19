using System;

namespace BeeDevelopment.Sega8Bit {
	/// <summary>
	/// Represents a byte of data that was written to an I/O device or memory.
	/// </summary>
	public class DataWrittenEventArgs : EventArgs {

		/// <summary>
		/// The value that was written.
		/// </summary>
		public byte Data { get; private set; }

		/// <summary>
		/// Creates an instance of the <see cref="DataWrittenEventArgs"/> class.
		/// </summary>
		/// <param name="data">The data that was written.</param>
		public DataWrittenEventArgs(byte data) {
			this.Data = data;
		}
	}
}
