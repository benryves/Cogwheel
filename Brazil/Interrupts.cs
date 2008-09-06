using System;

namespace BeeDevelopment.Brazil {
	public partial class Z80A {


		[StateNotSaved()]
		private bool iff1;
		/// <summary>
		/// Gets or sets the state of the <c>IFF1</c> flag.
		/// </summary>
		public bool IFF1 { get { return this.iff1; } set { this.iff1 = value; } }

		[StateNotSaved()]
		private bool iff2;
		/// <summary>
		/// Gets or sets the state of the <c>IFF2</c> flag.
		/// </summary>
		public bool IFF2 { get { return this.iff2; } set { this.iff2 = value; } }

		[StateNotSaved()]
		private bool interrupt;
		/// <summary>
		/// Gets or sets the state of the <see cref="INT"/> pin.
		/// </summary>
		public bool Interrupt { get { return this.interrupt; } set { this.interrupt = value; } }

		[StateNotSaved()]
		private bool nonMaskableInterrupt;
		/// <summary>
		/// Gets or sets the state of the <see cref="NMI"/> pin.
		/// </summary>
		public bool NonMaskableInterrupt {
			get { return this.nonMaskableInterrupt; }
			set { if (value && !this.nonMaskableInterrupt) this.NonMaskableInterruptPending = true; this.nonMaskableInterrupt = value; }
		}

		[StateNotSaved()]
		private bool nonMaskableInterruptPending;
		/// <summary>
		/// Gets or sets whether a non-maskable interrupt (<c>NMI</c>) is pending.
		/// </summary>
		public bool NonMaskableInterruptPending { get { return this.nonMaskableInterruptPending; } set { this.nonMaskableInterruptPending = value; } }

		[StateNotSaved()]
		private int interruptMode;
		/// <summary>
		/// Gets or sets the current interrupt mode.
		/// </summary>
		public int InterruptMode {
			get { return this.interruptMode; }
			set { if (value < 0 || value > 2) throw new ArgumentOutOfRangeException(); this.interruptMode = value; }
		}


		[StateNotSaved()]
		private bool halted;
		/// <summary>
		/// Gets or sets whether the device is halted.
		/// </summary>
		public bool Halted { get { return this.halted; } set { this.halted = value; } }

		private void ResetInterrupts() {
			this.IFF1 = false;
			this.IFF2 = false;
			this.Interrupt = false;
			this.NonMaskableInterrupt = false;
			this.NonMaskableInterruptPending = false;
			this.InterruptMode = 1;
			this.Halted = false;
		}

		private void Halt() {
			this.Halted = true;			
		}
	}
}
