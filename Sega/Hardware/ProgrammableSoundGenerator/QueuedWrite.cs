using System;
using System.Collections.Generic;
using BeeDevelopment.Brazil;

namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class ProgrammableSoundGenerator {

		/// <summary>
		/// Specifies the destination of a queued write.
		/// </summary>
		enum WriteDestination {
			/// <summary>
			/// The write goes to the sound generator.
			/// </summary>
			SoundGenerator,
			/// <summary>
			/// The write goes to the stereo distribution hardware.
			/// </summary>
			StereoDistribution,
		}

		/// <summary>
		/// Represents a queued hardware write.
		/// </summary>
		class QueuedWrite {

			/// <summary>Gets the <see cref="ProgrammableSoundGenerator"/> that contains this queued write.</summary>
			public ProgrammableSoundGenerator Generator { get; private set; }

			/// <summary>
			/// Creates an instance of the <see cref="QueuedWrite"/>.
			/// </summary>
			/// <param name="generator">The <see cref="ProgrammableSoundGenerator"/> that contains this queued write.</param>
			public QueuedWrite(ProgrammableSoundGenerator generator) {
				this.Generator = generator;
			}

			/// <summary>
			/// Gets or sets the time of the write in CPU clock cycles.
			/// </summary>
			public int Time { get; set; }
			/// <summary>
			/// Gets or sets the value of the write.
			/// </summary>
			public byte Value { get; set; }
			/// <summary>
			/// Gets or sets the <see cref="WriteDestination"/> of the hardware write.
			/// </summary>
			public WriteDestination Destination { get; set; }

			/// <summary>
			/// Writes the value of the <see cref="QueuedWrite"/> immediately to the <see cref="ProgrammableSoundGenerator"/>.
			/// </summary>
			public void Commit() {
				switch (this.Destination) {
					case WriteDestination.SoundGenerator:
						this.Generator.WriteImmediate(this.Value);
						break;
					case WriteDestination.StereoDistribution:
						this.Generator.WriteStereoDistributionImmediate(this.Value);
						break;
				}
			}
		}

		private Queue<QueuedWrite> QueuedWrites;

		/// <summary>
		/// Queues a byte to later write to the <see cref="ProgrammableSoundGenerator"/>.
		/// </summary>
		/// <param name="value">The control byte to write.</param>
		/// <remarks>The writes are committed by the <see cref="CreateSamples"/> method.</remarks>
		public void WriteQueued(byte value) {
			this.QueuedWrites.Enqueue(new QueuedWrite(this) { 
				Time = this.Emulator.TotalExecutedCycles, 
				Value = value, 
				Destination = WriteDestination.SoundGenerator,
			});
		}

		/// <summary>
		/// Flushes all queued writes immediately to the device.
		/// </summary>
		public void FlushQueuedWrites() {
			while (this.QueuedWrites.Count > 0) {
				this.QueuedWrites.Dequeue().Commit();
			}
		}

	}
}