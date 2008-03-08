/* 
 * This source file relates to hardware device I/O.
 */
using BeeDevelopment.Sega8Bit.Hardware;
namespace BeeDevelopment.Sega8Bit {
	public partial class Emulator {

		#region Devices
		
		/// <summary>
		/// Gets the <see cref="VideoDisplayProcessor"/>.
		/// </summary>
		public VideoDisplayProcessor Video { get; private set; }

		/// <summary>
		/// Gets the <see cref="ProgrammableSoundGenerator"/>.
		/// </summary>
		public ProgrammableSoundGenerator Sound { get; private set; }

		/// <summary>
		/// Gets or sets whether the emulator has Game Gear ports.
		/// </summary>
		public bool HasGameGearPorts { get; set; }

		#endregion

		/// <summary>
		/// Game Gear port 2
		/// </summary>
		private byte Port2 = 0; //HACK: Find somewhere better for this.

		/// <summary>
		/// Reads a byte to from a hardware device.
		/// </summary>
		/// <param name="port">Address of the hardware port to read from.</param>
		/// <returns>A value read from the device at address <paramref name="port"/>.</param>
		public override byte ReadHardware(ushort port) {

			if (this.HasGameGearPorts) {
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
					return this.ReadIOPortA();
				case 0xC1: // I/O port B.
					return this.ReadIOPortB();
			}
			return 0xFF;
		}

		/// <summary>
		/// Writes a byte to a hardware device.
		/// </summary>
		/// <param name="port">Address of the hardware port to write to.</param>
		/// <param name="value">The value to write to the device at address <paramref name="port"/>.</param>
		public override void WriteHardware(ushort port, byte value) {

			if (this.HasGameGearPorts) {
				switch (port & 0xFF) {
					case 0x02:
						this.Port2 = value;
						break;
				}
			}

			switch (port & 0xC1) { 

				case 0x00: // Memory controller.
					this.ExpansionSlotEnabled = (value & 0x80) == 0;
					this.CartridgeSlotEnabled = (value & 0x40) == 0;
					this.CardSlotEnabled = (value & 0x20) == 0;
					this.RamEnabled = (value & 0x10) == 0;
					this.BiosEnabled = (value & 0x08) == 0;
					break;

				case 0x01: // I/O port (controller ports) control.
					this.Ports[0].WriteState(value >> 0);
					this.Ports[1].WriteState(value >> 2);
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

	}
}
