using System;
namespace BeeDevelopment.Sega8Bit.Hardware {
	/// <summary>
	/// Emulates an 8255 Programmable Peripheral Interface (PPI).
	/// </summary>
	public partial class ProgrammablePeripheralInterface {


		/// <summary>Gets or sets the <see cref="PinDirection"/> of port A.</summary>
		public PinDirection PortADirection { get; set; }
		/// <summary>Gets or sets the <see cref="PinDirection"/> of port B.</summary>
		public PinDirection PortBDirection { get; set; }
		/// <summary>Gets or sets the <see cref="PinDirection"/> of the lower nybble of port A.</summary>
		public PinDirection PortCLowerDirection { get; set; }
		/// <summary>Gets or sets the <see cref="PinDirection"/> of the upper nybble of port A.</summary>
		public PinDirection PortCUpperDirection { get; set; }

		/// <summary>Gets or sets the state of port A's input.</summary>
		public byte PortAInput { get; set; }
		/// <summary>Gets or sets the state of port A's output.</summary>
		public byte PortAOutput { get; set; }
		/// <summary>Gets or sets the state of port B's input.</summary>
		public byte PortBInput { get; set; }
		/// <summary>Gets or sets the state of port B's output.</summary>
		public byte PortBOutput { get; set; }
		/// <summary>Gets or sets the state of port C's input.</summary>
		public byte PortCInput { get; set; }
		/// <summary>Gets or sets the state of port C's output.</summary>
		public byte PortCOutput { get; set; }

		/// <summary>
		/// Gets or sets the operating mode of group A.
		/// </summary>
		public int GroupAMode {
			get { return this.groupAMode; }
			set { if (value > 2 || value < 0) throw new InvalidOperationException("Group A mode must be 0, 1 or 2."); this.groupAMode = value; }
		}
		private int groupAMode;

		/// <summary>
		/// Gets or sets the operating mode of group B.
		/// </summary>
		public int GroupBMode {
			get { return this.groupBMode; }
			set { if (value > 1 || value < 0) throw new InvalidOperationException("Group B mode must be 0 or 1."); this.groupBMode = value; }
		}
		private int groupBMode;



		/// <summary>
		/// Resets the <see cref="ProgrammablePeripheralInterface"/> to its default state.
		/// </summary>
		public void Reset() {
			this.WriteControl(0x9B);
		}


	}
}
