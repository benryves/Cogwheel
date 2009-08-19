using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BeeDevelopment.Sega8Bit.Utility {
	public class VgmRecorder {

		#region Constants

		/// <summary>
		/// Represents a command that appears in the VGM file.
		/// </summary>
		private enum VgmCommand {
			/// <summary>Game Gear PSG stereo (0x4F dd), write dd to port 0x06.</summary>
			PsgStereoDistribution = 0x4F,
			/// <summary>PSG (SN76489/SN76496) (0x50 dd), write single value.</summary>
			Psg = 0x50,
			/// <summary>YM2413 (0x51 aa dd), write value dd to register aa.</summary>
			YM2413 = 0x51,
			/// <summary>Wait for a number of samples (0x61 nn nn), n can range from 0 to 65535.</summary>
			WaitSamples = 0x61,
			/// <summary>Wait 735 samples (1/60th of a second).</summary>
			Wait60thSecond = 0x62,
			/// <summary>Wait 882 samples (1/50th of a second).</summary>
			Wait50thSecond = 0x63,
			/// <summary>End of sound data.</summary>
			EndOfSound = 0x66,
		}

		/// <summary>
		/// Gets the VGM sample rate in Hertz.
		/// </summary>
		const int SampleRate = 44100;

		#endregion

		#region Properties

		private Stream stream;
		/// <summary>
		/// Gets the <see cref="Stream"/> to record to.
		/// </summary>
		public Stream Stream {
			get { return this.stream; }
		}

		private Emulator emulator;
		/// <summary>
		/// Gets the <see cref="Emulator"/> to record from.
		/// </summary>
		public Emulator Emulator {
			get { return this.emulator; }
		}

		#endregion

		#region Private Fields

		private BinaryWriter Writer = null;

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a new instance of the <see cref="VgmRecorder"/>.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to record to.</param>
		/// <param name="emulator">The <see cref="Emulator"/> to record from.</param>
		public VgmRecorder(Stream stream, Emulator emulator) {
			this.stream = stream;
			this.emulator = emulator;
			this.emulator.Sound.DataWritten += new EventHandler<DataWrittenEventArgs>(Sound_DataWritten);
			this.emulator.Sound.StereoDistributionDataWritten += new EventHandler<DataWrittenEventArgs>(Sound_StereoDistributionDataWritten);
		}


		#endregion

		#region Methods

		/// <summary>
		/// Start recording.
		/// </summary>
		public void Start() {
			if (this.Writer != null) throw new InvalidOperationException("Recording is already in progress.");
			this.LastExecutedCycles = null;
			this.TotalSamples = 0;
			this.Writer = new BinaryWriter(this.stream, Encoding.Unicode);
			// Write VGM header:
			this.stream.Seek(0, SeekOrigin.Begin);
			this.Writer.Write(Encoding.ASCII.GetBytes("Vgm "));              // Identification.
			this.Writer.Write((int)0);                                       // EOF offset (unpopulated).
			this.Writer.Write((int)0x150);                                   // Version 1.50
			this.Writer.Write((int)this.emulator.ClockSpeed);                // PSG clock speed.
			this.Writer.Write((int)0);                                       // YM2413 clock speed.
			this.Writer.Write((int)0);                                       // GD3 offset.
			this.Writer.Write((int)0);                                       // Total number of samples.
			this.Writer.Write((int)0);                                       // Loop offset.
			this.Writer.Write((int)0);                                       // Loop number of samples.
			this.Writer.Write((int)0);                                       // Rate scaling.
			this.Writer.Write((ushort)this.emulator.Sound.TappedBits);       // LFSR tapped bits.
			this.Writer.Write((byte)this.emulator.Sound.ShiftRegisterWidth); // LFSR width.
			this.Writer.Write((byte)0);                                      // (Reserved).
			this.Writer.Write((int)0);                                       // YM2612 clock speed.
			this.Writer.Write((int)0);                                       // YM2151 clock speed.
			this.Writer.Write((int)0xC);                                     // VGM data offset.
			this.Writer.Write(new byte[0x40 - this.stream.Position]);        // Padding to 0x40.
			
			// Write PSG pre-initialisation:
			for (int i = 0; i < 4; ++i) {
				this.Writer.Write((byte)VgmCommand.Psg);
				this.Writer.Write((byte)(0x80 | (i << 5) | 0x10 | (this.emulator.Sound.volumeRegisters[i] & 0x0F)));
				this.Writer.Write((byte)(0x80 | (i << 5) | 0x00 | (this.emulator.Sound.toneRegisters[i] & 0x0F)));
				this.Writer.Write((byte)((this.emulator.Sound.toneRegisters[i] >> 4) & 0x3F));
			}
			this.Writer.Write((byte)VgmCommand.PsgStereoDistribution);
			this.Writer.Write((byte)this.emulator.Sound.stereoDistribution);
		}

		/// <summary>
		/// Stop recording.
		/// </summary>
		public void Stop() {
			if (this.Writer == null) throw new InvalidOperationException("Recording hadn't been started.");
			this.Write(VgmCommand.EndOfSound);
			this.Writer.Flush();
			long EndPosition = this.stream.Position;
			this.stream.Seek(0x04, SeekOrigin.Begin);
			this.Writer.Write((int)(EndPosition - 4));
			this.stream.Seek(0x18, SeekOrigin.Begin);
			this.Writer.Write(this.TotalSamples);
			this.Writer = null;
		}

		#endregion

		private int? LastExecutedCycles;
		private int TotalSamples = 0;

		private void Sound_StereoDistributionDataWritten(object sender, DataWrittenEventArgs e) {
			if (this.Writer == null) return;
			this.Write(VgmCommand.PsgStereoDistribution, e.Data);
		}

		private void Sound_DataWritten(object sender, DataWrittenEventArgs e) {
			if (this.Writer == null) return;
			this.Write(VgmCommand.Psg, e.Data);
		}

		private void Write(VgmCommand command) {
			// First, we need to write the delta-time:
			int EmulatedCycles = this.emulator.TotalExecutedCycles;
			if (!this.LastExecutedCycles.HasValue) {
				this.LastExecutedCycles = EmulatedCycles;
			}
			int ElapsedCycles = EmulatedCycles - this.LastExecutedCycles.Value;
			int ElapsedSamples = (int)(((long)ElapsedCycles * (long)SampleRate) / this.emulator.ClockSpeed);
			this.TotalSamples += ElapsedSamples;
			if (ElapsedSamples > 0) {
				// There has been a time delay; we can write some more samples.
				if (ElapsedSamples <= 16) {
					// Waiting 1..16 samples can be done with a single byte.
					this.Writer.Write(0x70 + (ElapsedSamples - 1));
				} else {
					// Waiting n samples requires a more advanced delay.
					int ElapsedSamplesToWrite = ElapsedSamples;
					while (ElapsedSamplesToWrite > 0xFFFF) {
						this.Writer.Write((byte)VgmCommand.WaitSamples);
						this.Writer.Write((ushort)0xFFFF);
						ElapsedSamplesToWrite -= 0xFFFF;
						
					}
					if (ElapsedSamplesToWrite > 0) {
						this.Writer.Write((byte)VgmCommand.WaitSamples);
						this.Writer.Write((ushort)ElapsedSamplesToWrite);
					}
				}
			}
			this.LastExecutedCycles += (int)(((long)ElapsedSamples * (long)this.emulator.ClockSpeed) / SampleRate);
			// Now we need to write the command!
			this.Writer.Write((byte)command);
		}
		
		private void Write(VgmCommand command, byte value) {
			this.Write(command);
			this.Writer.Write(value);
		}

		private void Write(VgmCommand command, byte value1, byte value2) {
			this.Write(command);
			this.Writer.Write(value1);
			this.Writer.Write(value2);
		}

	}
}
