using System;
using System.Collections.Generic;

namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class Emu2413 {

		#region Events

		/// <summary>
		/// An event that is triggered when data has been written to the <see cref="Emu2413"/>.
		/// </summary>
		public event EventHandler<DataWrittenEventArgs> DataWritten;

		/// <summary>
		/// An event that is triggered when data has been written to the <see cref="Emu2413"/>.
		/// </summary>
		/// <param name="e">The data that was written.</param>
		protected virtual void OnDataWritten(DataWrittenEventArgs e) {
			if (this.DataWritten != null) {
				this.DataWritten(this, e);
			}
		}

		#endregion

		/// <summary>
		/// Represents a queued hardware write.
		/// </summary>
		class QueuedWrite {

			/// <summary>Gets the <see cref="Emu2413"/> that contains this queued write.</summary>
			public Emu2413 Generator { get; private set; }

			/// <summary>
			/// Creates an instance of the <see cref="QueuedWrite"/>.
			/// </summary>
			/// <param name="generator">The <see cref="Emu2413"/> that contains this queued write.</param>
			public QueuedWrite(Emu2413 generator) {
				this.Generator = generator;
			}

			/// <summary>
			/// Gets or sets the time of the write in CPU clock cycles.
			/// </summary>
			public int Time { get; set; }

			/// <summary>
			/// Gets or sets the address of the write.
			/// </summary>
			public int Address { get; set; }
			
			/// <summary>
			/// Gets or sets the value of the write.
			/// </summary>
			public byte Value { get; set; }

			/// <summary>
			/// Writes the value of the <see cref="QueuedWrite"/> immediately to the <see cref="Emu2413"/>.
			/// </summary>
			public void Commit() {
				this.Generator.WriteToAddress(this.Address, this.Value);
			}
		}

		private Queue<QueuedWrite> QueuedWrites;

		/// <summary>
		/// Queues a byte to later write to the <see cref="Emu2413"/>.
		/// </summary>
		/// <param name="value">The control byte to write.</param>
		/// <remarks>The writes are committed by the <see cref="CreateSamples"/> method.</remarks>
		public void WriteQueued(int address, byte value) {
			this.OnDataWritten(new DataWrittenEventArgs(address, value));
			this.QueuedWrites.Enqueue(new QueuedWrite(this) {
				Time = this.Emulator.ExpectedExecutedCycles, 
				Address = address,
				Value = value, 
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