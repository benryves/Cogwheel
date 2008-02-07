using System;
using System.Collections.Generic;
using System.Text;

namespace Brazil {
	public partial class Z80A {
		/// <summary>
		/// Override this method to allow the Z80 to read from memory.
		/// </summary>
		/// <param name="address">16-bit port address</param>
		/// <returns>Data from hardware at port requested</returns>
		public virtual byte ReadHardware(ushort port) {
			return 0;
		}

		/// <summary>
		/// Override this method to allow the Z80 to write to a hardware device.
		/// </summary>
		/// <param name="address">16-bit port address</param>
		/// <param name="value">The value of the byte to be written to the hardware at the requested port</param>
		public virtual void WriteHardware(ushort port, byte value) {

		}
	}
}
