using System;
namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class ProgrammableSoundGenerator {

		private int[] countDown;
		/// <summary>
		/// Gets or sets the count-down timers for the four sound channels.
		/// </summary>
		public int[] CountDown {
			get { return this.countDown; }
			set {
				if (value.Length != 4) throw new ArgumentException("Count-down array must have four elements (one per channel).");
				this.countDown = value;
			}
		}


		private int[] levels;
		/// <summary>
		/// Gets or sets the level (amplitude) values for the four sound channels.
		/// </summary>
		public int[] Levels {
			get { return this.levels; }
			set {
				if (value.Length != 4) throw new ArgumentException("Levels array must have four elements (one per channel).");
				this.levels = value;
			}
		}


		private ushort shiftRegister;
		/// <summary>
		/// Gets or sets the value of the noise channel's shift register.
		/// </summary>
		public ushort ShiftRegister {
			get { return this.shiftRegister; }
			set { this.shiftRegister = value; }
		}

		private bool noiseTicking;
		public bool NoiseTicking {
			get { return this.noiseTicking; }
			set { this.noiseTicking = value; }
		}

		private int CycleStepper = 0;

		private static int CalculateParity(int val) {
			val ^= val >> 8;
			val ^= val >> 4;
			val ^= val >> 2;
			val ^= val >> 1;
			return val;
		}
		
		private int LastCpuClocks = 0;

		private static double[] LogarithmicScale = { 1.0d, 0.794335765d, 0.630970183d, 0.501174963d, 0.398113956d, 0.316232795d, 0.251197851d, 0.20044557d, 0.15848262d, 0.125888852d, 0.100009156d, 0.07943968d, 0.063081759d, 0.050111393d, 0.039796136d, 0.0d };

		/// <summary>
		/// Creates some sound samples.
		/// </summary>
		/// <param name="buffer">The buffer to write the sound samples to.</param>
		/// <remarks>
		/// The output samples are all 16-bit stereo at 44.1KHz.
		/// </remarks>
		public void CreateSamples(short[] buffer) {

			lock (this.Emulator) {
				lock (this.QueuedWrites) {
					
					int TotalCyclesExecuted = this.Emulator.ExpectedExecutedCycles;
					int ElapsedCycles = TotalCyclesExecuted - this.LastCpuClocks;

					for (int i = 0; i < buffer.Length; i += 2) {

						int CorrespondingCycle = (int)(((UInt64)i * (UInt64)ElapsedCycles) / (UInt64)buffer.Length);

						while (this.QueuedWrites.Count > 0 && (this.QueuedWrites.Peek().Time - this.LastCpuClocks) <= CorrespondingCycle) {
							this.QueuedWrites.Dequeue().Commit();
						}

						bool WhiteNoise = (this.toneRegisters[3] & 0x04) != 0;

						this.CycleStepper -= this.clockSpeed;

						int[] SampleTotal = new int[4];
						int[] SampleCount = new int[4];

						while (this.CycleStepper < 0) {
							for (int c = 0; c < 4; ++c) {

								if (this.countDown[c] != 0) --countDown[c];

								if (this.countDown[c] <= 0) {

									if (c != 3) {

										this.levels[c] = 1 - this.levels[c];
										this.countDown[c] = this.toneRegisters[c];
										if (this.countDown[c] == 0) this.levels[c] = 1;

									} else {

										switch (this.toneRegisters[c] & 0x3) {
											case 0x0: this.countDown[c] = 0x10; break;
											case 0x1: this.countDown[c] = 0x20; break;
											case 0x2: this.countDown[c] = 0x40; break;
											case 0x3: this.countDown[c] = this.toneRegisters[2]; break;
										}

										if (this.noiseTicking ^= true) {
											this.shiftRegister = (ushort)((this.shiftRegister >> 1) | ((WhiteNoise ? ProgrammableSoundGenerator.CalculateParity(this.shiftRegister & this.tappedBits) : this.shiftRegister & 1) << (this.ShiftRegisterWidth - 1)));
											this.levels[3] = this.shiftRegister & 1;
										}

									}
								}

								SampleTotal[c] += this.levels[c];
								++SampleCount[c];
							}
							CycleStepper += 44100;
						}

						var Mixer = new double[2];
						for (int c = 0; c < 4; c++) {
							double Level = this.levels[c];
							if (SampleCount[c] != 0) Level = (double)SampleTotal[c] / (double)SampleCount[c];
							double ScaledValue = Level * LogarithmicScale[this.volumeRegisters[c]];
							for (int StereoChannel = 0; StereoChannel < 2; ++StereoChannel) {
								if ((this.StereoDistribution & (1 << (c + 4 * (1 - StereoChannel)))) != 0) Mixer[StereoChannel] += ScaledValue;
							}
						}

						for (int StereoChannel = 0; StereoChannel < 2; ++StereoChannel) {
							int Level = (int)(32768d * Mixer[StereoChannel] * 0.25d);
							if (Level > short.MaxValue) {
								Level = short.MaxValue;
							} else if (Level < short.MinValue) {
								Level = short.MinValue;
							}
							buffer[i + StereoChannel] = (short)Level;
						}
					}

					this.FlushQueuedWrites();
					this.LastCpuClocks = TotalCyclesExecuted;

				}
			}
		}
	}
}
