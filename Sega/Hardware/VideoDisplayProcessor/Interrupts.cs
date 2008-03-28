namespace BeeDevelopment.Sega8Bit.Hardware {
	public partial class VideoDisplayProcessor {

		/// <summary>
		/// Gets whether a line interrupt is pending or not.
		/// </summary>
		public bool LineInterruptPending { get { return this.lineInterruptPending; } set { this.lineInterruptPending = value; } }
		private bool lineInterruptPending;

		/// <summary>
		/// Gets whether a frame interrupt is pending or not.
		/// </summary>
		public bool FrameInterruptPending { get { return this.frameInterruptPending; } set { this.frameInterruptPending = value; } }
		private bool frameInterruptPending;

		/// <summary>
		/// Updates the state of the Z80's IRQ line.
		/// </summary>
		private void UpdateIrq() {
			this.Emulator.Interrupt =
				(this.LineInterruptEnable && this.lineInterruptPending) ||
				(this.FrameInterruptEnable && this.frameInterruptPending);
		}
	
	}
}