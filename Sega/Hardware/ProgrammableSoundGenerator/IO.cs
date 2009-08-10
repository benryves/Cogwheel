using System;
using System.Collections.Generic;

namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class ProgrammableSoundGenerator {


		public int LatchedChannel { get; set; }

		public enum LatchMode {
			Tone,
			Volume 
		}

		public LatchMode LatchedMode { get; set; }


		/// <summary>
		/// Writes a control byte to the <see cref="ProgrammableSoundGenerator"/>.
		/// </summary>
		/// <param name="value">The control byte to write.</param>
		public void WriteImmediate(byte value) {

			if ((value & 0x80) != 0x00) {
				// Latch/Data write.
				this.LatchedChannel = (value >> 5) & 0x3;
				this.LatchedMode = (LatchMode)((value >> 4) & 0x1);
				switch (this.LatchedMode) {
					case LatchMode.Tone:
						this.toneRegisters[this.LatchedChannel] &= 0xFFF0;
						this.toneRegisters[this.LatchedChannel] |= (ushort)(value & 0x0F);
						if (this.LatchedChannel == 3) this.shiftRegister = (ushort)(1 << (this.ShiftRegisterWidth - 1));
						break;
					case LatchMode.Volume:
						this.volumeRegisters[this.LatchedChannel] = (byte)(value & 0x0F);
						break;
				}
			} else {
				// Data write.
				switch (this.LatchedMode) {
					case LatchMode.Tone:
						this.toneRegisters[this.LatchedChannel] &= 0xF;
						this.toneRegisters[this.LatchedChannel] |= (ushort)((value & 0x3F) << 4);
						break;
					case LatchMode.Volume:
						this.volumeRegisters[this.LatchedChannel] = (byte)(value & 0xF);
						break;
				}
			}
		}
	}
}
