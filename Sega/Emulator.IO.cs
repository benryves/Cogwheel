﻿/* 
 * This source file relates to hardware device I/O.
 */
using BeeDevelopment.Brazil;
using BeeDevelopment.Sega8Bit.Hardware;
using System;
namespace BeeDevelopment.Sega8Bit {
	public partial class Emulator {

		#region Devices

		private VideoDisplayProcessor video;
		/// <summary>
		/// Gets the <see cref="VideoDisplayProcessor"/>.
		/// </summary>
		[StateNotSaved()]
		public VideoDisplayProcessor Video {
			get { return this.video; }
			internal set { this.video = value; }
		}

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

		/// <summary>
		/// Gets or sets whether the emulator reponds to the PS/2 keyboard.
		/// </summary>
		public bool HasPS2Keyboard { get; set; }

		/// <summary>
		/// Gets or sets whether the emulator reponds to a serial port.
		/// </summary>
		public bool HasSerialPort { get; set; }

		/// <summary>
		/// Gets or sets whether the emulator reponds to a tape cassette.
		/// </summary>
		public bool HasCassetteRecorder { get; set; }

		/// <summary>
		/// Gets the primary <see cref="ProgrammablePeripheralInterface"/> used by the emulator in SC-3000 and SF-7000 mode.
		/// </summary>
		[StateNotSaved()]
		public ProgrammablePeripheralInterface MainPPI { get; internal set; }

		/// <summary>
		/// Gets the secondary <see cref="ProgrammablePeripheralInterface"/> used by the emulator in SF-7000 mode.
		/// </summary>
		[StateNotSaved()]
		public ProgrammablePeripheralInterface SecondaryPPI { get; private set; }

		/// <summary>
		/// Gets the <see cref="DebugConsole"/> used by the emulator.
		/// </summary>
		[StateNotSaved()]
		public DebugConsole DebugConsole { get; private set; }


		/// <summary>
		/// Gets the <see cref="FloppyDiskController"/> used by the emulator in SF-7000 mode.
		/// </summary>
		[StateNotSaved()]
		public FloppyDiskController FloppyDiskController { get; private set; }

#if EMU2413

		/// <summary>
		/// Gets the <see cref="YM2413"/> connected to the system.
		/// </summary>
		[StateNotSaved()]
		public Emu2413 FmSound { get; private set; }

		/// <summary>
		/// Gets or sets whether FM sound is enabled/available.
		/// </summary>
		public bool FmSoundEnabled { get; set; }

#endif

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

					switch (port & 0xE1) {
						case 0xA0:
							return this.video.ReadData();
						case 0xA1:
							return this.video.ReadControl();
						case 0xE0:
						case 0xE1:
							return this.ReadingColecoVisionJoysticks ? this.ColecoVisionPorts[port & 1].ReadJoystick() : this.ColecoVisionPorts[port & 1].ReadNumberPad();
					}
					break;

				case HardwareFamily.SC3000:


					switch (port & 0x61) {
						case 0x00:
						case 0x20:
							return this.video.ReadData();
						case 0x01:
						case 0x21:
							return this.video.ReadControl();
						case 0x40:
							return this.MainPPI.ReadPortA();
						case 0x41:
							return this.MainPPI.ReadPortB();
						case 0x80:
							//TODO: "Instruction referenced by R".
							break;
					}



					break;

				case HardwareFamily.SF7000:

					//TODO: Port mirroring.

					switch (port & 0xFF) {
						case 0xBE:
							return this.video.ReadData();
						case 0xBF:
							return this.video.ReadControl();
						case 0xDC:
							return this.MainPPI.ReadPortA();
						case 0xDD:
							return this.MainPPI.ReadPortB();
						case 0xDE:
							return this.MainPPI.ReadPortC();
						case 0xE0:
							return (byte)this.FloppyDiskController.ReadStatus();
						case 0xE1:
							return this.FloppyDiskController.ReadData();
						case 0xE4:
							if (this.FloppyDiskController.Int) {
								this.SecondaryPPI.PortAInput |= (byte)(1 << 0);
							} else {
								this.SecondaryPPI.PortAInput &= unchecked((byte)(~(1 << 0)));
							}
							if (this.FloppyDiskController.Index) {
								this.SecondaryPPI.PortAInput |= (byte)(1 << 2);
							} else {
								this.SecondaryPPI.PortAInput &= unchecked((byte)(~(1 << 2)));
							}
							return this.SecondaryPPI.ReadPortA();
						case 0xE5:
							return this.SecondaryPPI.ReadPortB();
						case 0xE6:
							return this.SecondaryPPI.ReadPortC();
						case 0xE8:
							throw new NotImplementedException("USARTD");
						case 0xE9:
							throw new NotImplementedException("USARTC");
						default:
							throw new NotImplementedException(string.Format("? io[{0:X2}]", port & 0xFF));
					}



				default: // Master System (default) I/O map.
					
					if (this.HasGameGearPorts && this.RespondsToGameGearPorts) {
						switch (port & 0xFF) {
							case 0x00:
								// D7: STT  - Start/Pause button (0 = on, 1 = off).
								// D6: NJAP - 0: Domestic (Japan), 1: Overseas.
								// D5: NNTS - 0: NTSC, 1: PAL.
								return (byte)(
									(this.StartButton ? 0x00 : 0x80) |
									(this.Region == Region.Japanese ? 0x00 : 0x40) |
									(this.video.System == VideoDisplayProcessor.VideoSystem.Ntsc ? 0x00 : 0x20)
								);
							case 0x01: return 0x7F;
							case 0x02: return this.Port2;
							case 0x03: return 0x00;
							case 0x04: return 0xFF;
							case 0x05: return 0x00;
							case 0x06: return 0xFF;
						}
					}

					byte Result = 0xFF;

					switch (port & 0xC1) {
						case 0x00:
						case 0x01:
							break;
						case 0x40: // VDP vertical retrace counter.
							Result =  this.video.VerticalCounter;
							break;
						case 0x41: // VDP horizontal counter.
							Result = this.video.HorizontalCounter;
							break;
						case 0x80: // VDP Data.
							Result = this.video.ReadData();
							break;
						case 0x81: // VDP Control.
							Result = this.video.ReadControl();
							break;
						case 0xC0: // I/O port A.
							Result = this.ReadSegaIOPortA();
							break;
						case 0xC1: // I/O port B.
							Result = this.ReadSegaIOPortB();
							break;
					}

#if EMU2413
					if ((port & 0xFF) == 0xF2 && this.FmSoundEnabled) {
						Result &= this.FmSound.DetectionValue;
					}
#endif

					return Result;
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

					switch (port & 0xE0) {
						case 0x80:
							this.ReadingColecoVisionJoysticks = false;
							break;
						case 0xA0:
							if ((port & 1) == 0) {
								this.video.WriteData(value);
							} else {
								this.video.WriteControl(value);
							}
							break;
						case 0xC0:
							this.ReadingColecoVisionJoysticks = true;
							break;
						case 0xE0:
							this.Sound.WriteQueued(value);
							break;
					}

					if ((port & 0xFF) == 0xFC) this.DebugConsole.WriteControl(value);
					if ((port & 0xFF) == 0xFD) this.DebugConsole.WriteData(value);

					break;

				case HardwareFamily.SC3000:

					if ((port & 0x20) == 0) {
						switch (port & 0x3) {
							case 0: this.MainPPI.WritePortA(value); break;
							case 1: this.MainPPI.WritePortB(value); break;
							case 2: this.MainPPI.WritePortC(value); break;
							case 3: this.MainPPI.WriteControl(value); break;
						}
						this.SC3000Keyboard.UpdateState();
					}

					if ((port & 0x40) == 0) {
						if ((port & 0x01) == 0) {
							this.video.WriteData(value);
						} else {
							this.video.WriteControl(value);
						}
					}

					if ((port & 0x80) == 0) {
						this.Sound.WriteQueued(value);
					}


					if ((port & 0xFF) == 0xFC) this.DebugConsole.WriteControl(value);
					if ((port & 0xFF) == 0xFD) this.DebugConsole.WriteData(value);

					break;
				
				case HardwareFamily.SF7000:

					//TODO: Port mirroring.

					switch (port & 0xFF) {
						case 0x7F:
							this.Sound.WriteQueued(value);
							break;
						case 0xBE:
							this.video.WriteData(value);
							break;
						case 0xBF:
							this.video.WriteControl(value);
							break;
						case 0xDC:
							this.MainPPI.PortAOutput = value;
							this.SC3000Keyboard.UpdateState();
							break;
						case 0xDD:
							this.MainPPI.PortBOutput = value;
							this.SC3000Keyboard.UpdateState();
							break;
						case 0xDE:
							this.MainPPI.PortCOutput = value;
							this.SC3000Keyboard.UpdateState();
							break;
						case 0xDF:
							this.MainPPI.WriteControl(value);
							this.SC3000Keyboard.UpdateState();
							break;
						case 0xE1:
							this.FloppyDiskController.WriteData(value);
							break;
						case 0xE4:
							this.SecondaryPPI.WritePortA(value);
							break;
						case 0xE5:
							this.SecondaryPPI.WritePortB(value);
							break;
						case 0xE6:
							this.SecondaryPPI.WritePortC(value);
							this.Bios.Enabled = (this.SecondaryPPI.PortCOutput & (1 << 6)) == 0;
							break;
						case 0xE7:
							this.SecondaryPPI.WriteControl(value);
							this.Bios.Enabled = (this.SecondaryPPI.PortCOutput & (1 << 6)) == 0;
							break;
						case 0xE8:
							throw new NotImplementedException("USARTD");
						case 0xE9:
							throw new NotImplementedException("USARTC");
						case 0xFC:
							this.DebugConsole.WriteControl(value);
							break;
						case 0xFD:
							this.DebugConsole.WriteData(value);
							break;
						default:
							throw new NotImplementedException(string.Format("io[{0:X2}]={1:X2}", port & 0xFF, value));
					}
					break;

				default: // Master System (default) I/O map.
					
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
								if ((this.Region != Region.Japanese) || ((port & 0xFF) == 0x3E)) { // Hacky fix for Super Tetris; see http://www.smspower.org/forums/viewtopic.php?t=12123
									this.ExpansionSlot.Enabled = (value & 0x80) == 0;
									this.CartridgeSlot.Enabled = (value & 0x40) == 0;
									this.CardSlot.Enabled = (value & 0x20) == 0;
									this.WorkRam.Enabled = (value & 0x10) == 0;
									this.Bios.Enabled = (value & 0x08) == 0;
								}
								break;

							case 0x01: // I/O port (controller ports) control.
								bool OldTh = this.SegaPorts[0].TH.State || this.SegaPorts[1].TH.State;
								this.SegaPorts[0].WriteState(value >> 0);
								this.SegaPorts[1].WriteState(value >> 2);
								if (!OldTh && (this.SegaPorts[0].TH.State || this.SegaPorts[1].TH.State)) {
									this.video.LatchHorizontalCounter();
								}

								if (this.HasPS2Keyboard) this.PS2Keyboard.UpdateState();
								if (this.HasSerialPort) this.SerialPort.UpdateState();
								if (this.HasCassetteRecorder) this.CassetteRecorder.UpdateState();
								break;

							case 0x40: // PSG.
							case 0x41:
								this.Sound.WriteQueued(value);
								break;

							case 0x80: // VDP Data.
								this.video.WriteData(value);
								break;

							case 0x81: // VDP Control.
								this.video.WriteControl(value);
								break;
						}


						switch (port & 0xFF) {
#if EMU2413
							case 0xF0:
							case 0xF1:
								if (this.FmSoundEnabled) this.FmSound.WriteQueued(port, value);
								break;
							case 0xF2:
								if (this.FmSoundEnabled) this.FmSound.DetectionValue = value;
								break;
#endif
							case 0xFC:
								this.DebugConsole.WriteControl(value);
								break;
							case 0xFD:
								this.DebugConsole.WriteData(value);
								break;
						}
					}

					break;
			}

		

		}

	}
}
