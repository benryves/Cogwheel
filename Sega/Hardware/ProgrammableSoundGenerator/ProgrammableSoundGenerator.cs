using System;
using System.Collections.Generic;
namespace BeeDevelopment.Sega8Bit.Hardware {

	/// <summary>
	/// Emulates an SN76489 programmable sound generator.
	/// </summary>
	public partial class ProgrammableSoundGenerator {

		/// <summary>
		/// Gets or sets the clock speed of the sound generator.
		/// </summary>
		public int ClockSpeed { get; set; }

		/// <summary>
		/// Gets the <see cref="Emulator"/> that contains the <see cref="ProgrammableSoundGenerator"/>.
		/// </summary>
		public Emulator Emulator { get; private set; }

		/// <summary>
		/// Creates an instance of a <see cref="ProgrammableSoundGenerator"/>
		/// </summary>
		/// <param name="emulator">The emulator that contains the <see cref="ProgrammableSoundGenerator"/>.</param>
		public ProgrammableSoundGenerator(Emulator emulator) {
			this.Emulator = emulator;
			this.ClockSpeed = 3579545 / 16;
			this.Reset();
		}

		/// <summary>
		/// Resets the <see cref="ProgrammableSoundGenerator"/> to its default state.
		/// </summary>
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
