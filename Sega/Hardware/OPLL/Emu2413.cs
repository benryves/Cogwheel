using System;
using System.Collections.Generic;
using BeeDevelopment.Brazil;

namespace BeeDevelopment.Sega8Bit.Hardware {

	/// <summary>
	/// Emulates a YM2413 sound chip.
	/// </summary>
	public partial class Emu2413 {

		/// <summary>
		/// Gets the emulator that contains the <see cref="Emu2413"/>.
		/// </summary>
		[StateNotSaved()]
		public Emulator Emulator { get; private set; }

		private OPLL opll;
		/// <summary>
		/// Gets the running <see cref="OPLL"/> wrapper instance.
		/// </summary>
		[StateNotSaved()]
		public OPLL Opll { get { return this.opll; } }

		/// <summary>
		/// Creates an instance of the <see cref="Emu2413"/> emulator class.
		/// </summary>
		/// <param name="clockRate">The clock rate of the emulated YM2413.</param>
		/// <param name="sampleRate">The sound sample rate.</param>
		public Emu2413(Emulator emulator, int clockSpeed, int sampleRate) {
			this.Emulator = emulator;
			this.opll = OPLL_new((uint)clockSpeed, (uint)sampleRate);
		}

		/// <summary>
		/// Creates an instance of the <see cref="Emu2413"/> class with a 3.58MHz clock and 44.1kHz sample rate.
		/// </summary>
		public Emu2413(Emulator emulator)
			: this(emulator, 3579545, 44100) {
		}

		/// <summary>
		/// Resets the emulated YM2413.
		/// </summary>
		public void Reset() {
			OPLL_reset(this.opll);
			this.QueuedWrites = new Queue<QueuedWrite>();
			this.DetectionValue = 0;
		}

		/// <summary>
		/// Writes a byte to an address.
		/// </summary>
		/// <param name="address">The address of the YM2413 write.</param>
		/// <param name="value">The value to write.</param>
		public void WriteToAddress(int address, byte value) {
			OPLL_writeIO(this.opll, (uint)address, (uint)value);
		}

		/// <summary>
		/// Sets a register value directly.
		/// </summary>
		/// <param name="register">The YM2413 register to write to.</param>
		/// <param name="value">The value to write.</param>
		public void WriteToRegister(int register, byte value) {
			OPLL_writeReg(this.opll, (uint)register, (uint)value);
		}

		/// <summary>
		/// Generates a number of sound samples in 16-bit stereo format.
		/// </summary>
		/// <param name="samples">An array of <see cref="Int16"/> to store the sound samples.</param>
		public void GenerateSamples(short[] samples) {
			lock (this.Emulator) {
				lock (this.QueuedWrites) {
					int TotalCyclesExecuted = this.Emulator.ExpectedExecutedCycles;
					int ElapsedCycles = TotalCyclesExecuted - this.LastCpuClocks;
					for (int i = 0; i < samples.Length; i += 2) {
						int CorrespondingCycle = (int)(((UInt64)i * (UInt64)ElapsedCycles) / (UInt64)samples.Length);
						while (this.QueuedWrites.Count > 0 && (this.QueuedWrites.Peek().Time - this.LastCpuClocks) <= CorrespondingCycle) {
							this.QueuedWrites.Dequeue().Commit();
						}
						samples[i + 0] = samples[i + 1] = calc(this.opll);
					}					
					this.FlushQueuedWrites();
					this.LastCpuClocks = TotalCyclesExecuted;
				}
			}
		}
		/// <summary>
		/// Stores the number of CPU clocks the last time GenerateSamples was called.
		/// </summary>
		private int LastCpuClocks = 0;

		/// <summary>
		/// Gets or sets a byte value to detect the chip.
		/// </summary>
		public byte DetectionValue { get; set; }

		/// <summary>
		/// Forces the OPLL to update after changing state.
		/// </summary>
		internal void Update() {
			OPLL_forceRefresh(this.opll);
		}

	}

}