/* 
 * This source file relates to controller port I/O.
 */
using BeeDevelopment.Brazil;
namespace BeeDevelopment.Sega8Bit {
	public partial class Emulator {

		/// <summary>
		/// Gets the two <see cref="SegaControllerPort"/> instances connected to the console.
		/// </summary>
		[StateNotSaved()]
		public SegaControllerPort[] SegaPorts { get; private set; }

		/// <summary>
		/// Gets the two <see cref="ColecoVisionControllerPort"/> instances connected to the console.
		/// </summary>
		[StateNotSaved()]
		public ColecoVisionControllerPort[] ColecoVisionPorts { get; private set; }

		/// <summary>
		/// Resets the controller ports to their default state.
		/// </summary>
		public void ResetPorts() {
			this.SegaPorts = new SegaControllerPort[] {
				new SegaControllerPort(),
				new SegaControllerPort()
			};
			this.ColecoVisionPorts = new ColecoVisionControllerPort[] { 
				new ColecoVisionControllerPort(),
				new ColecoVisionControllerPort()
			};
			this.ReadingColecoVisionJoysticks = false;
			this.ResetButton = false;
		}

		/// <summary>
		/// Reads I/O port A.
		/// </summary>
		/// <returns>The state of I/O port A.</returns>
		private byte ReadSegaIOPortA() {
			return (byte)(
				(this.SegaPorts[0].Up.State ? 0x01 : 0x00) |
				(this.SegaPorts[0].Down.State ? 0x02 : 0x00) |
				(this.SegaPorts[0].Left.State ? 0x04 : 0x00) |
				(this.SegaPorts[0].Right.State ? 0x08 : 0x00) |
				(this.SegaPorts[0].TL.State ? 0x10 : 0x00) |
				(this.SegaPorts[0].TR.State ? 0x20 : 0x00) |
				(this.SegaPorts[1].Up.State ? 0x40 : 0x00) |
				(this.SegaPorts[1].Down.State ? 0x80 : 0x00)
			);
		}

		/// <summary>
		/// Reads I/O port B.
		/// </summary>
		/// <returns>The state of I/O port B.</returns>
		private byte ReadSegaIOPortB() {
			return (byte)(
				(this.SegaPorts[1].Left.State ? 0x01 : 0x00) |
				(this.SegaPorts[1].Right.State ? 0x02 : 0x00) |
				(this.SegaPorts[1].TL.State ? 0x04 : 0x00) |
				(this.SegaPorts[1].TR.State ? 0x08 : 0x00) |
				(this.ResetButton ? 0x00 : 0x10) |
				0x20 |
				(this.SegaPorts[0].TH.State ? 0x40 : 0x00) |
				(this.SegaPorts[1].TH.State ? 0x80 : 0x00)
			);
		}

		/// <summary>
		/// Gets or sets the state of the console's Reset button.
		/// </summary>
		/// <remarks>This is only available on the Master System hardware.</remarks>
		public bool ResetButton { get; set; }

		/// <summary>
		/// Gets or sets whether the console has a Reset button.
		/// </summary>
		public bool HasResetButton { get; set; }

		/// <summary>
		/// Gets or sets the state of the console's Pause button.
		/// </summary>
		/// <remarks>This is connected directly to the CPU's non-maskable interrupt pin.</remarks>
		public bool PauseButton {
			get { return this.NonMaskableInterrupt; }
			set { this.NonMaskableInterrupt = value; } 
		}

		/// <summary>
		/// Gets or sets whether the console has a Pause button.
		/// </summary>
		public bool HasPauseButton { get; set; }

		/// <summary>
		/// Gets or sets the state of the console's Start button.
		/// </summary>
		/// <remarks>This is only available on the Game Gear hardware.</remarks>
		public bool StartButton { get; set; }

		/// <summary>
		/// Gets or sets whether the console has a Start button.
		/// </summary>
		public bool HasStartButton { get; set; }

	}
}
