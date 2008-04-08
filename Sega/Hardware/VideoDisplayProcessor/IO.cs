using System;
namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class VideoDisplayProcessor {

		#region Types

		/// <summary>
		/// Defines the different modes used to access data in the <see cref="VideoDisplayRegister"/>.
		/// </summary>
		public enum DataAccessMode {
			/// <summary>
			/// Data is be written to or read from video RAM.
			/// </summary>
			VideoRamReadWrite,
			/// <summary>
			/// Data is be written to video RAM only.
			/// </summary>
			VideoRamWrite,
			/// <summary>
			/// Data is be written to a <see cref="VideoDisplayProcessor"/> register.
			/// </summary>
			RegisterWrite,
			/// <summary>
			/// Data is be written to or read from the colour RAM.
			/// </summary>
			ColourRamWrite,
		}

		#endregion

		#region Properties

		/// <summary>
		/// Provides access to the video RAM.
		/// </summary>
		public byte[] VideoRam { 
			get { return this.vram; }
			set {
				if (value == null || value.Length != this.vram.Length) throw new InvalidOperationException();
				this.vram = value;
				for (ushort i = 0; i < this.vram.Length; ++i) this.CacheFastPixelColourIndex(i, this.vram[i]);
			}
		}
		private byte[] vram;

		/// <summary>
		/// Gets or sets the address of the internal data pointer.
		/// </summary>
		public ushort Address { get { return this.address; } set { this.address = value; } }
		private ushort address;

		/// <summary>
		/// Gets or sets whether the <see cref="VideoDisplayProcessor"/> is currently waiting for the second byte in a command word.
		/// </summary>
		public bool WaitingForSecond { get { return this.waitingForSecond; } set { this.waitingForSecond = value; } }
		private bool waitingForSecond;

		/// <summary>
		/// Gets or sets the current <see cref="DataAccessMode"/>.
		/// </summary>
		public DataAccessMode AccessMode { get { return this.accessMode; } set { this.accessMode = value; } }
		private DataAccessMode accessMode;

		/// <summary>
		/// Gets or sets the value in the data read buffer.
		/// </summary>
		public byte ReadBuffer { get { return this.readBuffer; } set { this.readBuffer = value; } }
		private byte readBuffer;

		/// <summary>
		/// Gets the current vertical counter position.
		/// </summary>
		public byte VerticalCounter { get; private set; }

		/// <summary>
		/// Gets the current horizontal counter position.
		/// </summary>
		public byte HorizontalCounter { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Write a byte to the <see cref="VideoDisplayProcessor"/> data port.
		/// </summary>
		/// <param name="value">The byte of data to write.</param>
		/// <remarks>
		/// The destination of the data (video RAM, palette or register) depends
		/// on the current <see cref="AccessMode"/>.
		/// </remarks>
		public void WriteData(byte value) {
			this.waitingForSecond = false;
			this.readBuffer = value;

			if (this.accessMode == DataAccessMode.ColourRamWrite) {

				// Writing to the palette (CRAM).
				if (this.SupportsVariablePalette) this.WriteColourRam(value);

			} else {

				// Write to video RAM.
				this.vram[this.address & 0x3FFF] = value;

				this.CacheFastPixelColourIndex(this.address, value);
			}
			++this.address;
		}

		private void CacheFastPixelColourIndex(ushort address, byte value) {
			int ModifiedTileSlice = ((address & 0x3FFF) / 4) * 8;
			int PlaneBitmask = 1 << (address & 3);
			int InverseBitmask = ~PlaneBitmask;
			for (int i = 0; i < 8; ++i) {
				if ((value & 0x80) == 0) {
					this.FastPixelColourIndex[ModifiedTileSlice] &= InverseBitmask;
				} else {
					this.FastPixelColourIndex[ModifiedTileSlice] |= PlaneBitmask;
				}
				++ModifiedTileSlice;
				value <<= 1;
			}
		}

		/// <summary>
		/// Read a byte from the <see cref="VideoDisplayProcessor"/> data port.
		/// </summary>
		/// <returns>The data read from the port.</returns>
		public byte ReadData() {
			this.waitingForSecond = false;
			byte b = this.readBuffer;
			this.readBuffer = vram[this.address++ & 0x3FFF];
			return b;
		}

		/// <summary>
		/// Write a byte of data to the <see cref="VideoDisplayProcessor"/> control port.
		/// </summary>
		/// <param name="data">The data to write to the port.</param>
		public void WriteControl(byte data) {

			if (this.waitingForSecond) {
				
				this.accessMode = (DataAccessMode)(data >> 6);

				this.address = (ushort)((this.address & 0x00FF) + (data << 8));

				switch (this.accessMode) {
					case DataAccessMode.VideoRamReadWrite: // 0
						this.readBuffer = this.vram[this.address & 0x3FFF];
						++this.address;
						break;
					case DataAccessMode.RegisterWrite: // 2
						this.registers[data & 0x0F] = (byte)(address & 0xFF);
						this.UpdateIrq();
						break;

				}
				this.waitingForSecond = false;

			} else {

				this.address = (ushort)((this.address & 0xFF00) + data);
				this.waitingForSecond = true;

			}
		}

		/// <summary>
		/// Read a byte from the <see cref="VideoDisplayProcessor"/> control port.
		/// </summary>
		/// <returns>Data read from the port.</returns>
		public byte ReadControl() {

			WaitingForSecond = false;
			byte b = (byte)(
				(this.frameInterruptPending ? 0x80 : 0x00) +
				(this.SpriteOverflow ? 0x40 : 0x00) +
				(this.SpriteCollision ? 0x20 : 0x00) +
				this.InvalidSpriteIndex
			);

			this.frameInterruptPending = false;
			this.lineInterruptPending = false;
			this.SpriteOverflow = false;
			this.SpriteCollision = false;

			this.UpdateIrq();

			return b;
		}

		/// <summary>
		/// Latches the value of the horizontal counter.
		/// </summary>
		public void LatchHorizontalCounter() {
			int ExecutedCycles = this.Emulator.TotalExecutedCycles - this.CpuCyclesAtStartOfScanline;
			int PixelsDrawn = (ExecutedCycles * 342) / this.CpuCyclesPerScanline;
			this.HorizontalCounter = (byte)((PixelsDrawn - 8) / 2);
			if (this.HorizontalCounter > 0x93) {
				this.HorizontalCounter += 0xE9 - 0x94;
			}
		}

		#endregion

	}
}