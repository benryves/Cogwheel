using System;
namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class ProgrammableSoundGenerator {

		public int[] CountDown { get; set; }
		public int[] Levels { get; set; }

		private int CycleStepper = 0;

		private static int CalculateParity(int val) {
			val ^= val >> 8;
			val ^= val >> 4;
			val ^= val >> 2;
			val ^= val >> 1;
			return val;
		}

		public ushort ShiftRegister { get; set; }

		public bool NoiseTicking { get; set; }

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

						this.CycleStepper -= this.ClockSpeed;

						int[] SampleTotal = new int[4];
						int[] SampleCount = new int[4];

						while (this.CycleStepper < 0) {
							for (int c = 0; c < 4; ++c) {

								if (this.CountDown[c] != 0) --CountDown[c];

								if (this.CountDown[c] <= 0) {

									if (c != 3) {

										this.Levels[c] = 1 - this.Levels[c];
										this.CountDown[c] = this.toneRegisters[c];
										if (this.CountDown[c] == 0) this.Levels[c] = 1;

									} else {

										switch (this.toneRegisters[c] & 0x3) {
											case 0x0: this.CountDown[c] = 0x10; break;
											case 0x1: this.CountDown[c] = 0x20; break;
											case 0x2: this.CountDown[c] = 0x40; break;
											case 0x3: this.CountDown[c] = this.toneRegisters[2]; break;
										}

										if (this.NoiseTicking ^= true) {
											this.ShiftRegister = (ushort)((this.ShiftRegister >> 1) | ((WhiteNoise ? ProgrammableSoundGenerator.CalculateParity(this.ShiftRegister & this.TappedBits) : this.ShiftRegister & 1) << (this.ShiftRegisterWidth - 1)));
											this.Levels[3] = this.ShiftRegister & 1;
										}

									}
								}

								SampleTotal[c] += this.Levels[c];
								++SampleCount[c];
							}
							CycleStepper += 44100;
						}

						var Mixer = new double[2];
						for (int c = 0; c < 4; c++) {
							double Level = this.Levels[c];
							if (SampleCount[c] != 0) Level = (double)SampleTotal[c] / (double)SampleCount[c];
							double ScaledValue = Level * LogarithmicScale[this.volumeRegisters[c]];
							for (int StereoChannel = 0; StereoChannel < 2; ++StereoChannel) {
								if ((this.StereoDistribution & (1 << (c + 4 * (1 - StereoChannel)))) != 0) Mixer[StereoChannel] += ScaledValue;
							}
						}

						for (int StereoChannel = 0; StereoChannel < 2; ++StereoChannel) {
							buffer[i + StereoChannel] = (short)Math.Min(short.MaxValue, Math.Max(short.MinValue, (32768d * Mixer[StereoChannel] * 0.25d)));
						}
					}

					this.FlushQueuedWrites();
					this.LastCpuClocks = TotalCyclesExecuted;

				}
			}
		}
	}
}
