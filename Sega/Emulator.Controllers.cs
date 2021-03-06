/* 
 * This source file relates to controller port I/O.
 */
using BeeDevelopment.Brazil;
using BeeDevelopment.Sega8Bit.Hardware.Controllers;
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
		/// Gets the <see cref="Hardware.Controllers.SC3000Keyboard"/> connected to the console.
		/// </summary>
		[StateNotSaved()]
		public SC3000Keyboard SC3000Keyboard { get; private set; }

		/// <summary>
		/// Gets the <see cref="Hardware.Controllers.PS2Keyboard"/> connected to the console.
		/// </summary>
		[StateNotSaved()]
		public PS2Keyboard PS2Keyboard { get; private set; }

		/// <summary>
		/// Gets the <see cref="Hardware.Controllers.SerialPort"/> connected to the console.
		/// </summary>
		[StateNotSaved()]
		public SerialPort SerialPort { get; private set; }

		/// <summary>
		/// Gets the <see cref="Hardware.Controllers.CassetteRecorder"/> connected to the console.
		/// </summary>
		[StateNotSaved()]
		public CassetteRecorder CassetteRecorder { get; private set; }

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
			this.SC3000Keyboard = new SC3000Keyboard(this);
			this.PS2Keyboard = new PS2Keyboard(this);
			this.SerialPort = new SerialPort(this);
			this.CassetteRecorder = new CassetteRecorder(this);
			this.ReadingColecoVisionJoysticks = false;
			this.ResetButton = false;
		}

		/// <summary>
		/// Reads I/O port A.
		/// </summary>
		/// <returns>The state of I/O port A.</returns>
		private byte ReadSegaIOPortA() {

			if (this.HasPS2Keyboard) this.PS2Keyboard.UpdateState();
			if (this.HasSerialPort) this.SerialPort.UpdateState();
			if (this.HasCassetteRecorder) this.CassetteRecorder.UpdateState();

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

			if (this.HasPS2Keyboard) this.PS2Keyboard.UpdateState();
			if (this.HasSerialPort) this.SerialPort.UpdateState();
			if (this.HasCassetteRecorder) this.CassetteRecorder.UpdateState();

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
