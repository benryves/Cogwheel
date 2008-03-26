
using System;
namespace BeeDevelopment.Brazil {

	/// <summary>
	/// ZiLOG Z80A CPU Emulator
	/// </summary>
	[Serializable()]
	public partial class Z80A {

		/// <summary>
		/// Creates an instance of the <see cref="Z80A"/> emulator class.
		/// </summary>
		public Z80A() {
			InitialiseTables();
			Reset();
		}

		/// <summary>
		/// Reset the Z80 to its initial state
		/// </summary>
		public virtual void Reset() {
			this.ResetRegisters();
			this.ResetInterrupts();
			this.RunningCycles = 0;
			this.TotalExecutedCycles = 0;
		}
	}
}
