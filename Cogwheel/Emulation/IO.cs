using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Emulation {
    public partial class Sega8Bit {

        /// <summary>
        /// Write a byte to the SMS hardware.
        /// </summary>
        /// <param name="port">Port number to write to.</param>
        /// <param name="value">Data to write to requested port.</param>
        public override void WriteHardware(ushort port, byte value) {

            port &= 0xFF;

            switch (this.CapsHardwareModel) {
                case HardwareModelType.Sg1000:
                case HardwareModelType.Sc3000:
                case HardwareModelType.Sf7000: {

                        if (port >= 0xE0) {

                            Console.WriteLine("p[{0:X2}]={1:X2}", port, value);

                            if (this.type == MachineType.Sf7000) {
                                switch (port & 0x0D) {
                                    case 0x00:
                                        break;
                                    case 0x01:
                                        break;
                                    case 0x04:
                                    case 0x05:
                                        this.Sf7000PPI.Write((Devices.PPI.PortNumber)(port & 0x03), value);
                                        break;
                                    case 0x08:
                                        // USART
                                        break;
                                }
                                
                            }

                        } else {

                            if ((port & 0x20) == 0) {
                                this.Sc3000PPI.Write((Devices.PPI.PortNumber)(port & 0x03), value);
                            }

                            if ((port & 0x40) == 0) {
                                if ((port & 0x01) == 0) {
                                    VideoProcessor.WriteData(value);
                                } else {
                                    VideoProcessor.WriteControl(value);
                                }
                            }

                            if ((port & 0x80) == 0) {
                                SoundGenerator.WriteByteToPsg(value);
                            }

                        }
                    }
                    break;

                case HardwareModelType.SegaMasterSystem:
                case HardwareModelType.SegaGameGear: {

                        // Ignore the Game Gear specific ports for now.
                        if (port < 7 && (this.Machine == MachineType.GameGear || this.Machine == MachineType.GameGearMasterSystem)) {
                            if (this.CapsHardwareModel == HardwareModelType.SegaGameGear) {
                                WriteGameGearPort(port, value);
                            }
                            return;
                        }

                        switch (port & 0xC1) {

                            case 0x00:
                                // Memory controller
                                WriteToMemoryControl(value);
                                break;
                            case 0x01:
                                // I/O port control
                                PortAControl.SetBits(value >> 0);
                                PortBControl.SetBits(value >> 2);
                                break;
                            case 0x40:
                            case 0x41:
                                // PSG
                                this.SoundGenerator.BufferedWriteByteToPsg(this.TotalExecutedCycles, value);
                                break;

                            case 0x80:
                                // VDP (Data) 
                                this.VideoProcessor.WriteData(value);
                                break;
                            case 0x81:
                                // VDP (Control)
                                this.VideoProcessor.WriteControl(value);
                                break;
                        }
                    } break;
            }
        }

        public override byte ReadHardware(ushort port) {

            port &= 0xFF;

            switch (this.CapsHardwareModel) {
                case HardwareModelType.Sg1000:
                case HardwareModelType.Sc3000:
                case HardwareModelType.Sf7000: {

                        if (port >= 0xE0) {

                            if (this.type == MachineType.Sf7000) {
                                switch (port & 0x0D) {
                                    case 0x00:
                                    case 0x01:
                                        return 0x00;
                                    case 0x04:
                                    case 0x05:
                                        return 0x00;
                                    case 0x08:
                                        // USART
                                        return 0x00;
                                }

                            }
                        }

                        switch (port & 0x61) {
                            case 0x00:
                            case 0x20:
                                return VideoProcessor.ReadData();
                            case 0x01:
                            case 0x21:
                                return VideoProcessor.ReadControl();
                            case 0x40:
                                if (this.CapsHardwareModel == HardwareModelType.Sg1000 || this.Sc3000PPI.KeyboardRow == 7) {
                                    return PortA;
                                } else {
                                    return this.ControllerKeyboard.RowStatusByteA;
                                }
                            case 0x41:
                                if (this.CapsHardwareModel == HardwareModelType.Sg1000 || this.Sc3000PPI.KeyboardRow == 7) {
                                    return PortB;
                                } else {
                                    return this.ControllerKeyboard.RowStatusByteB;
                                }
                            case 0x80:
                                //TODO: "Instruction referenced by R".
                                break;
                        }

                    } break;


                case HardwareModelType.SegaMasterSystem:
                case HardwareModelType.SegaGameGear: {
                        if (port < 7 && this.CapsHardwareModel == HardwareModelType.SegaGameGear) {
                            return ReadGameGearPort(port);
                        } else {
                            switch (port & 0xC1) {
                                case 0x00:
                                case 0x01:
                                    return 0xFF;
                                case 0x40:
                                    return this.VideoProcessor.VCounter;
                                case 0x41:
                                    return this.VideoProcessor.VCounter; //TODO:Should be H counter
                                case 0x80:
                                    return this.VideoProcessor.ReadData();
                                case 0x81:
                                    return this.VideoProcessor.ReadControl();
                                case 0xC0:
                                    return PortA;
                                case 0xC1:
                                    return PortB;
                            }
                        }

                    }  break;
            }
            return 0xFF;
        }

        private byte PortA {
            get {
                int PortRead = ~(((int)ControllerPortA.State & 0x1F) | (((int)ControllerPortB.State & 0x03) << 6) | 0x20);
                PortRead |= (GetTr(PortAControl, ControllerPortA) ? 0x20 : 0x00);
                return (byte)PortRead;
            }
        }
        private byte PortB {
            get {
                int PortRead = ~((((int)ControllerPortB.State >> 2) & 0x7) | 0xC8);
                PortRead |= ButtonReset && Machine == MachineType.MasterSystem ? 0x00 : 0x10;
                PortRead |= GetTr(PortBControl, ControllerPortB) ? 0x08 : 0x00;
                PortRead |= GetTh(PortAControl, ControllerPortA) ? 0x40 : 0x00;
                PortRead |= GetTh(PortBControl, ControllerPortB) ? 0x80 : 0x00;
                return (byte)PortRead;
            }
        }

        #region I/O Port Control

        private bool GetTh(PortPinControl portControl, Devices.Input.Controller portController) {
            if (portControl.PinThIsInput) {
                return (portController.State & Cogwheel.Devices.Input.Controller.Pins.TH) == 0;
            } else {
                if (this.IsJapanese) {
                    return false;
                } else {
                    return portControl.PinThLevel;
                }
            }
        }

        private bool GetTr(PortPinControl portControl, Devices.Input.Controller portController) {
            if (portControl.PinTrIsInput) {
                return (portController.State & Cogwheel.Devices.Input.Controller.Pins.TR) == 0;
            } else {
                if (this.IsJapanese) {
                    return false;
                } else {
                    return portControl.PinTrLevel;
                }
            }
        }

        private PortPinControl PortAControl = new PortPinControl();
        private PortPinControl PortBControl = new PortPinControl();

        private struct PortPinControl {
            public bool PinTrIsInput;
            public bool PinThIsInput;
            public bool PinTrLevel;
            public bool PinThLevel;

            public void SetBits(int value) {
                PinTrIsInput = (value & 0x01) != 0;
                PinThIsInput = (value & 0x02) != 0;
                PinTrLevel = (value & 0x10) != 0;
                PinThLevel = (value & 0x20) != 0;
            }

        }

        #endregion



    }
}
