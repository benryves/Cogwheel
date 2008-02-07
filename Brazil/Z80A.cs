using System;
using System.Collections.Generic;
using System.Text;

namespace Brazil {
	/// <summary>
	/// ZiLOG Z80A CPU Emulator
	/// </summary>
	public partial class Z80A {

		int RunningCycles;

		public Z80A() {
			InitTables();
			Reset();
			RunningCycles = 0;
		}

		/// <summary>
		/// Reset the Z80 to its initial state
		/// </summary>
		public void Reset() {
			ResetRegisters();
			ResetInterrupts();
			this.TotalExecutedCycles = 0;
		}

		//public bool Logging = false;

	}
}
