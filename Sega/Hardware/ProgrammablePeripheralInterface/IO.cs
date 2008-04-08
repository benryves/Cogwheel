using System;
namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class ProgrammablePeripheralInterface {

		public void WritePortA(byte value) { this.PortAOutput = value; }
		public void WritePortB(byte value) { this.PortBOutput = value; }
		public void WritePortC(byte value) { this.PortCOutput = value; }

		public byte ReadPortA() { return this.PortADirection == PinDirection.Input ? this.PortAInput : this.PortAOutput; }
		public byte ReadPortB() { return this.PortBDirection == PinDirection.Input ? this.PortBInput : this.PortBOutput; }
		public byte ReadPortC() {
			return (byte)(
				((this.PortCLowerDirection == PinDirection.Input ? this.PortCInput : this.PortCOutput) & 0x0F) |
				((this.PortCUpperDirection == PinDirection.Input ? this.PortCInput : this.PortCOutput) & 0xF0)
			);
		}

		/// <summary>Writes a byte to the control port.</summary>
		public void WriteControl(byte value) {

			if ((value & 0x80) != 0) {

				// Group A

				this.PortCUpperDirection = (PinDirection)((value >> 3) & 1);
				this.PortADirection = (PinDirection)((value >> 4) & 1);
				this.GroupAMode = Math.Min(2, (value >> 5) & 0x3);

				// Group B

				this.PortCLowerDirection = (PinDirection)((value >> 0) & 1);
				this.PortBDirection = (PinDirection)((value >> 1) & 1);
				this.GroupBMode = (value >> 2) & 1;

				// Reset all port outputs:
				this.PortAOutput = this.PortBOutput = this.PortCOutput = 0;

			} else {

				// Set port C pin value.
				byte Bitmask = (byte)(1 << ((value >> 1) & 7));
				if ((value & 1) == 0) {
					this.PortCOutput &= (byte)~Bitmask;
				} else {
					this.PortCOutput |= Bitmask;
				}

			}
		}
	}
}
