using System;

namespace BeeDevelopment.Brazil {
	public partial class Z80A {


		/// <summary>
		/// Gets or sets the state of the <c>IFF1</c> flag.
		/// </summary>
		public bool IFF1 { get; set; }

		/// <summary>
		/// Gets or sets the state of the <c>IFF2</c> flag.
		/// </summary>
		public bool IFF2 { get; set; }
		
		/// <summary>
		/// Gets or sets the state of the <see cref="INT"/> pin.
		/// </summary>
		public bool Interrupt { get; set; }

		/// <summary>
		/// Gets or sets the state of the <see cref="NMI"/> pin.
		/// </summary>
		public bool NonMaskableInterrupt {
			get { return this.nonMaskableInterrupt; }
			set { if (value && !this.nonMaskableInterrupt) this.NonMaskableInterruptPending = true; this.nonMaskableInterrupt = value; }
		}
		private bool nonMaskableInterrupt;

		/// <summary>
		/// Gets or sets whether a non-maskable interrupt (<c>NMI</c>) is pending.
		/// </summary>
		public bool NonMaskableInterruptPending { get; set; }

		/// <summary>
		/// Gets or sets the current interrupt mode.
		/// </summary>
		public int InterruptMode {
			get { return this.interruptMode; }
			set { if (value < 0 || value > 2) throw new ArgumentOutOfRangeException(); this.interruptMode = value; }
		}
		private int interruptMode;


		/// <summary>
		/// Gets or sets whether the device is halted.
		/// </summary>
		public bool Halted { get; set; }

		private void ResetInterrupts() {
			this.IFF1 = false;
			this.IFF2 = false;
			this.Interrupt = false;
			this.NonMaskableInterrupt = false;
			this.NonMaskableInterruptPending = false;
			this.InterruptMode = 1;
			this.Halted = false;
		}

		private void DisableInterrupts() {
			this.IFF1 = false;
			this.IFF2 = false;
			InstructionsUntilEI = 0;
		}

		/// <summary>
		/// Gets or sets the number of instructions to execute before interrupts are re-enabled.
		/// </summary>
		public int InstructionsUntilEI { get; set; }

		private void EnableInterrupts() {
			InstructionsUntilEI = 2;
		}

		private void Halt() {
			this.Halted = true;			
		}

	
	}
}
