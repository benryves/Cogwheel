using System;
using System.Collections.Generic;
namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class ProgrammableSoundGenerator {

		public int ClockSpeed { get; set; }

		public Emulator Emulator { get; private set; }

		public ProgrammableSoundGenerator(Emulator emulator) {
			this.Emulator = emulator;
			this.ClockSpeed = 3579545 / 16;
			this.Reset();
		}

		public void Reset() {
			this.LatchedChannel = 0;
			this.LatchedMode = LatchMode.Tone;
			this.QueuedWrites = new Queue<KeyValuePair<int, byte>>(256);
			this.volumeRegisters = new byte[] { 0x0F, 0x0F, 0x0F, 0x0F };
			this.toneRegisters = new ushort[4];
			this.CountDown = new int[4];
			this.Levels = new int[] { 1, 1, 1, 1 };
			this.ShiftRegister = 0x8000;
		}

	}
}
