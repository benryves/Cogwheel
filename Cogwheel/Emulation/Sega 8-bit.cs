using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Emulation {

    public enum MachineType {
        Sg1000 = 1,
        Sc3000,
        Sf7000,
        MasterSystem,
        MasterSystem2,
        GameGear,
        GameGearMasterSystem,
    }

    public partial class Sega8Bit : Brazil.Z80A {

        
        private MachineType type = MachineType.MasterSystem2;
        /// <summary>The machine type.</summary>
        public MachineType Type {
            get {
                return this.type;
            }
            set {
                this.type = value;
                this.VDP.MachineType = value;
                switch (value) {
                    case MachineType.Sg1000:
                    case MachineType.Sc3000:
                        this.CapsMemoryModel = MemoryModelType.Sg1000;
                        break;
                    case MachineType.MasterSystem:
                    case MachineType.MasterSystem2:
                    case MachineType.GameGear:
                    case MachineType.GameGearMasterSystem:
                        this.CapsMemoryModel = MemoryModelType.SegaMasterSystem;
                        break;
                    case MachineType.Sf7000:
                        this.CapsMemoryModel = MemoryModelType.Sf7000;
                        //this.MemoryBiosRom = Properties.Resources.SF7000_IPL_ROM;
                        break;
                }
                switch (value) {
                    case MachineType.Sg1000:
                        this.CapsHardwareModel = HardwareModelType.Sg1000;
                        break;
                    case MachineType.Sc3000:
                        this.CapsHardwareModel = HardwareModelType.Sc3000;
                        break;
                    case MachineType.Sf7000:
                        this.CapsHardwareModel = HardwareModelType.Sf7000;
                        break;
                    case MachineType.MasterSystem:
                    case MachineType.MasterSystem2:
                    case MachineType.GameGearMasterSystem:
                        this.CapsHardwareModel = HardwareModelType.SegaMasterSystem;
                        break;
                    case MachineType.GameGear:
                        this.CapsHardwareModel = HardwareModelType.SegaGameGear;
                        break;
                }
            }
        }

        /// <summary>
        /// Machine nationality.
        /// </summary>
        public bool IsJapanese = true;

        #region Devices

        public readonly Devices.VDP VDP;
        public readonly Devices.PSG PSG;
        public readonly Devices.Sc3000PPI Sc3000PPI;
        public readonly Devices.Sf7000PPI Sf7000PPI;

        /// <summary>The controller connected to port A.</summary>
        public Devices.Input.Controller ControllerPortA;
        /// <summary>The controller connected to port B.</summary>
        public Devices.Input.Controller ControllerPortB;

        /// <summary>An emulated keyboard.</summary>
        public Devices.Input.Keyboard ControllerKeyboard;

        /// <summary>Used to set the status of the Game Gear's start button.</summary>
        /// <remarks>This has no effect in Master System mode.  Use ButtonPause to set the status of the Master System's Pause button.</remarks>
        public bool ButtonStart;

        /// <summary>Used to set the status of the Master System's reset button.</summary>
        /// <remarks>This has no effect in Game Gear or Master System 2 mode.</remarks>
        public bool ButtonReset;

        /// <summary>Used to set the status of the Master System's pause button.</summary>
        /// <remarks>This has no effect in Game Gear mode. Use ButtonStart to set the status of the Game Gear's Start button.</remarks>
        public bool ButtonPause {
            set { this.PinNonMaskableInterrupt = value; }
            get { return this.PinNonMaskableInterrupt; }
        }

        #endregion

        #region Peripherals

        public readonly Peripherals.Glasses3D Glasses;

        #endregion

        /// <summary>
        /// Create a new insance of the Master System emulator.
        /// </summary>
        public Sega8Bit() {

            // Set up hardware devices.
            this.VDP = new Devices.VDP(this);
            this.PSG = new Devices.PSG();
            this.Sc3000PPI = new Devices.Sc3000PPI();
            this.Sf7000PPI = new Devices.Sf7000PPI();
            this.ControllerPortA = new Devices.Input.Controller(this);
            this.ControllerPortB = new Devices.Input.Controller(this);
            this.ControllerKeyboard = new Cogwheel.Devices.Input.Keyboard(this);
            this.Glasses = new Cogwheel.Peripherals.Glasses3D();

            // Set capabilities
            this.Type = MachineType.MasterSystem;

            // Reset everything.
            this.Reset();

        }


        /// <summary>
        /// Reset the machine and related devices.
        /// </summary>
        public new void Reset() {
            // Reset the CPU.
            base.Reset();

            // Set up some registers in SMS-friendly ways:
            this.RegisterSP = 0xDFF0;

            // Clear status of buttons.
            this.ButtonStart = false;
            this.ButtonReset = false;
            this.ButtonPause = false;
            if (this.ControllerPortA != null) this.ControllerPortA.State = Devices.Input.Controller.Pins.None;
            if (this.ControllerPortB != null) this.ControllerPortB.State = Devices.Input.Controller.Pins.None;
            this.PortAControl.SetBits(0x05);
            this.PortBControl.SetBits(0x05);
            // Reset the hardware devices.
            this.VDP.Reset();
            this.PSG.Reset();
            this.Sc3000PPI.Reset();
            this.Sf7000PPI.Reset();

            // Reset peripherals.
            this.Glasses.Reset();
            // Reset memory.
            this.ResetMemory();
            // Clear Game Genie codes.
            this.GameGenieClearCodes();
        }

        public void RunLine() {
            this.FetchExecute(228);
            this.VDP.RasteriseLine();
        }

        public void RunFrame() {
            this.VDP.RunFramePending = false;
            while (!this.VDP.RunFramePending) {
                this.VDP.RasteriseLine();
                this.FetchExecute(228);
            }
        }
    }
}
