/* 
 * This source file relates to hardware device I/O.
 */
using BeeDevelopment.Sega8Bit.Hardware;
using BeeDevelopment.Brazil;
namespace BeeDevelopment.Sega8Bit {
	public partial class Emulator {

		#region Devices
		
		/// <summary>
		/// Gets the <see cref="VideoDisplayProcessor"/>.
		/// </summary>
		[StateNotSaved()]
		public VideoDisplayProcessor Video { get; internal set; }

		/// <summary>
		/// Gets the <see cref="ProgrammableSoundGenerator"/>.
		/// </summary>
		[StateNotSaved()]
		public ProgrammableSoundGenerator Sound { get; internal set; }

		/// <summary>
		/// Gets or sets whether the emulator has Game Gear ports.
		/// </summary>
		public bool HasGameGearPorts { get; set; }

		/// <summary>
		/// Gets or sets whether the emulator reponds to Game Gear port writes.
		/// </summary>
		public bool RespondsToGameGearPorts { get; set; }

		#endregion

		/// <summary>
		/// Game Gear port 2
		/// </summary>
		private byte Port2 = 0; //HACK: Find somewhere better for this.

		public bool ReadingColecoVisionJoysticks { get; set; }

		/// <summary>
		/// Reads a byte to from a hardware device.
		/// </summary>
		/// <param name="port">Address of the hardware port to read from.</param>
		/// <returns>A value read from the device at address <paramref name="port"/>.</param>
		public override byte ReadHardware(ushort port) {

			switch (this.Family) {

				case HardwareFamily.ColecoVision: // ColecoVision I/O map.

					switch (port & 0xFF) {
						case 0xBE:
							return this.Video.ReadData();
						case 0xBF:
							return this.Video.ReadControl();
						case 0xFC:
							return this.ReadingColecoVisionJoysticks ? this.ColecoVisionPorts[0].ReadJoystick() : this.ColecoVisionPorts[0].ReadNumberPad();
						case 0xFF:
							return this.ReadingColecoVisionJoysticks ? this.ColecoVisionPorts[1].ReadJoystick() : this.ColecoVisionPorts[1].ReadNumberPad();
					}
					break;

				default: // Sega (default) I/O map.
					
					if (this.HasGameGearPorts && this.RespondsToGameGearPorts) {
						switch (port & 0xFF) {
							case 0x00:
								// D7: STT  - Start/Pause button (0 = on, 1 = off).
								// D6: NJAP - 0: Domestic (Japan), 1: Overseas.
								// D5: NNTS - 0: NTSC, 1: PAL.
								return (byte)(
									(this.StartButton ? 0x00 : 0x80) |
									(this.Region == Region.Japanese ? 0x00 : 0x40) |
									(this.Video.System == VideoDisplayProcessor.VideoSystem.Ntsc ? 0x00 : 0x20)
								);
							case 0x01: return 0x7F;
							case 0x02: return this.Port2;
							case 0x03: return 0x00;
							case 0x04: return 0xFF;
							case 0x05: return 0x00;
							case 0x06: return 0xFF;
						}
					}

					switch (port & 0xC1) {
						case 0x00:
						case 0x01:
							return 0xFF;
						case 0x40: // VDP vertical retrace counter.
							return this.Video.VerticalCounter;
						case 0x41: // TODO: H counter.
							return this.Video.VerticalCounter;
						case 0x80: // VDP Data.
							return this.Video.ReadData();
						case 0x81: // VDP Control.
							return this.Video.ReadControl();
						case 0xC0: // I/O port A.
							return this.ReadSegaIOPortA();
						case 0xC1: // I/O port B.
							return this.ReadSegaIOPortB();
					}

					break;
			}



		
			return 0xFF;
		}

		/// <summary>
		/// Writes a byte to a hardware device.
		/// </summary>
		/// <param name="port">Address of the hardware port to write to.</param>
		/// <param name="value">The value to write to the device at address <paramref name="port"/>.</param>
		public override void WriteHardware(ushort port, byte value) {

			switch (this.Family) {
				case HardwareFamily.ColecoVision: // ColecoVision I/O map.

					switch (port & 0xFF) {
						case 0x80:
							this.ReadingColecoVisionJoysticks = false;
							break;
						case 0xC0:
							this.ReadingColecoVisionJoysticks = true;
							break;
						case 0xBE:
							this.Video.WriteData(value);
							break;
						case 0xBF:
							this.Video.WriteControl(value);
							break;
						case 0xFF:
							this.Sound.WriteQueued(value);
							break;

					}

					break;
				
				default: // Sega (default) I/O map.
					
					if (this.HasGameGearPorts && (port & 0xFF) < 7) {
						if (this.RespondsToGameGearPorts) {
							switch (port & 0xFF) {
								case 0x02:
									this.Port2 = value;
									break;
								case 0x06:
									this.Sound.WriteStereoDistributionQueued(value);
									break;
							}
						}
					} else {

						switch (port & 0xC1) {

							case 0x00: // Memory controller.
								this.ExpansionSlot.Enabled = (value & 0x80) == 0;
								this.CartridgeSlot.Enabled = (value & 0x40) == 0;
								this.CardSlot.Enabled = (value & 0x20) == 0;
								this.WorkRam.Enabled = (value & 0x10) == 0;
								this.Bios.Enabled = (value & 0x08) == 0;
								break;

							case 0x01: // I/O port (controller ports) control.
								this.SegaPorts[0].WriteState(value >> 0);
								this.SegaPorts[1].WriteState(value >> 2);
								break;

							case 0x40: // PSG.
							case 0x41:
								this.Sound.WriteQueued(value);
								break;

							case 0x80: // VDP Data.
								this.Video.WriteData(value);
								break;

							case 0x81: // VDP Control.
								this.Video.WriteControl(value);
								break;
						}
					}

					break;
			}

		

		}

	}
}
