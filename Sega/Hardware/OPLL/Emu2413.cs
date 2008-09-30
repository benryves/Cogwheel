using System;
using System.Runtime.InteropServices;

namespace BeeDevelopment.Sega8Bit.Hardware {

	/// <summary>
	/// Provides a managed wrapper for the emu2413 YM2413 emulator.
	/// </summary>
	public class Emu2413 : IDisposable {

		private IntPtr Handle;
		private bool IsDisposed = false;

		#region Properties

		/// <summary>
		/// Gets or sets a value used by the hardware to detect the presence of a YM2413.
		/// </summary>
		public byte DetectionValue { get; set; }

		/// <summary>
		/// Gets or sets the latched register.
		/// </summary>
		public byte LatchedRegister { get; set; }

		private byte[] registers;
		/// <summary>
		/// Gets or sets the YM2413 registers.
		/// </summary>
		public byte[] Registers {
			get {
				this.CheckNotDisposed();
				return this.registers;
			}
			set {
				this.CheckNotDisposed();
				if (value == null || value.Length != this.registers.Length) throw new InvalidOperationException();
				this.registers = value;
				this.Reset();
				for (int i = 0; i < this.registers.Length; ++i) this.WriteToRegister(i, this.registers[i]);
			}
		}

		#endregion



		#region Initialisation

		/// <summary>
		/// Creates an instance of the <see cref="Emu2413"/> emulator class.
		/// </summary>
		/// <param name="clockRate">The clock rate of the emulated YM2413.</param>
		/// <param name="sampleRate">The sound sample rate.</param>
		public Emu2413(int clockRate, int sampleRate) {
			this.Handle = OPLL.New(clockRate, sampleRate);
			this.Reset();
		}

		/// <summary>
		/// Creates an instance of the <see cref="Emu2413"/> class with a 3.58MHz clock and 44.1kHz sample rate.
		/// </summary>
		public Emu2413()
			: this(3579545, 44100) {
		}

		/// <summary>
		/// Resets the <see cref="Emu2413"/> emulator.
		/// </summary>
		public void Reset() {
			this.CheckNotDisposed();
			this.DetectionValue = 0;
			this.registers = new byte[64];
			OPLL.Reset(this.Handle);
		}

		#endregion

		#region Public Interface

		/// <summary>
		/// Writes a byte to an address.
		/// </summary>
		/// <param name="address">The address of the YM2413 write.</param>
		/// <param name="value">The value to write.</param>
		public void WriteToAddress(int address, byte value) {
			this.CheckNotDisposed();
			if ((address & 0x01) == 1) {
				this.registers[this.LatchedRegister] = value;
			} else {
				this.LatchedRegister = value;
			}
			OPLL.WriteIO(this.Handle, address, value);
		}

		/// <summary>
		/// Sets a register value directly.
		/// </summary>
		/// <param name="register">The YM2413 register to write to.</param>
		/// <param name="value">The value to write.</param>
		public void WriteToRegister(int register, byte value) {
			this.CheckNotDisposed();
			this.registers[register & 0x3F] = value;
			OPLL.WriteRegister(this.Handle, register, value);
		}

		/// <summary>
		/// Generates a single stereo sample.
		/// </summary>
		/// <param name="left">Outputs the level for the left channel.</param>
		/// <param name="right">Outputs the level for the right channel.</param>
		public void CalculateStereo(out int left, out int right) {
			this.CheckNotDisposed();
			var Samples = new int[2];
			OPLL.CalculateStereo(this.Handle, Samples);
			left = Samples[0]; right = Samples[1];
		}

		/// <summary>
		/// Generates a number of sound samples in 16-bit stereo format.
		/// </summary>
		/// <param name="samples">An array of <see cref="Int16"/> to store the sound samples.</param>
		public void GenerateSamples(short[] samples) {
			this.CheckNotDisposed();
			OPLL.GenerateSamples(this.Handle, samples.Length, samples);
		}

		#endregion

		#region Cleanup

		/// <summary>
		/// Bail out by throwing an <see cref="ObjectDisposedException"/> if the instance has been disposed.
		/// </summary>
		private void CheckNotDisposed() {
			if (this.IsDisposed) throw new ObjectDisposedException("Emu2413");
		}

		/// <summary>
		/// Disposes the <see cref="Emu2413"/> instance.
		/// </summary>
		public void Dispose() {
			if (!this.IsDisposed) {
				try {
					OPLL.Delete(this.Handle);
				} finally {
					this.IsDisposed = true;
				}
			}
		}

		/// <summary>
		/// Disposes the <see cref="Emu2413"/> instance.
		/// </summary>
		~Emu2413() {
			try {
				this.Dispose();
			} catch { }
		}

		#endregion

		

	}
}
