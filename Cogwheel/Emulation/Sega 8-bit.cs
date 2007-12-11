using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Emulation {

	/// <summary>
	/// Represents an 8-bit Sega machine.
	/// </summary>
    public enum MachineType {
		/// <summary>The SG-1000 (Sega Game 1000).</summary>
        Sg1000 = 1,
		/// <summary>The SC-3000 (Sega Computer 1000).</summary>
        Sc3000,
		/// <summary>The SF-7000 add-on.</summary>
        Sf7000,
		/// <summary>The Sega Master System.</summary>
        MasterSystem,
		/// <summary>The Sega Master System version 2.</summary>
        MasterSystem2,
		/// <summary>The Sega Game Gear.</summary>
        GameGear,
		/// <summary>The Sega Game Gear in Master System mode.</summary>
        GameGearMasterSystem,
    }

    public partial class Sega8Bit : Brazil.Z80A {
        
        private MachineType type = MachineType.MasterSystem2;

        /// <summary>Gets or sets the underlying machine type.</summary>
        public MachineType Machine {
            get {
                return this.type;
            }
            set {
                this.type = value;
                this.VideoProcessor.MachineType = value;
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
        /// Gets or sets the machine nationality.
        /// </summary>
		public bool IsJapanese { get; set; }

        #region Devices

		/// <summary>
		/// Provides access to the video display processor component of the hardware.
		/// </summary>
		public Devices.VideoDisplayProcessor VideoProcessor { get; private set; }

		/// <summary>
		/// Provides access to the programmable sound generator component of the hardware.
		/// </summary>
		public Devices.ProgrammableSoundGenerator SoundGenerator { get; private set; }

		public Devices.Sc3000PPI Sc3000PPI { get; private set; }
		public Devices.Sf7000PPI Sf7000PPI { get; private set; }

        /// <summary>Gets or sets the controller connected to port A.</summary>
		public Devices.Input.Controller ControllerPortA { get; set; }
        /// <summary>Gets or sets the controller connected to port B.</summary>
		public Devices.Input.Controller ControllerPortB { get; set; }

        /// <summary>Provides access to the keyboard.</summary>
        public Devices.Input.Keyboard ControllerKeyboard;

        /// <summary>Gets or sets the status of the Game Gear's start button.</summary>
        /// <remarks>This has no effect in Master System mode.  Use ButtonPause to set the status of the Master System's Pause button.</remarks>
		public bool ButtonStart { get; set; }

        /// <summary>Gets or sets the status of the Master System's reset button.</summary>
        /// <remarks>This has no effect in Game Gear or Master System 2 mode.</remarks>
		public bool ButtonReset { get; set; }

        /// <summary>Gets or sets the status of the Master System's pause button.</summary>
        /// <remarks>This has no effect in Game Gear mode. Use ButtonStart to set the status of the Game Gear's Start button.</remarks>
        public bool ButtonPause {
            set { this.PinNonMaskableInterrupt = value; }
            get { return this.PinNonMaskableInterrupt; }
        }

        #endregion

        #region Peripherals

		/// <summary>
		/// Provides access to information about the 3D glasses.
		/// </summary>
		public Peripherals.Glasses3D Glasses { get; private set; }

        #endregion

        /// <summary>
        /// Create a new insance of the 8-bit Sega machine emulator.
        /// </summary>
        public Sega8Bit() {

			// Sets region to Japanese.
			this.IsJapanese = true;

            // Set up hardware devices.
            this.VideoProcessor = new Devices.VideoDisplayProcessor(this);
            this.SoundGenerator = new Devices.ProgrammableSoundGenerator();
            this.Sc3000PPI = new Devices.Sc3000PPI();
            this.Sf7000PPI = new Devices.Sf7000PPI();
            this.ControllerPortA = new Devices.Input.Controller(this);
            this.ControllerPortB = new Devices.Input.Controller(this);
            this.ControllerKeyboard = new Cogwheel.Devices.Input.Keyboard(this);
            this.Glasses = new Cogwheel.Peripherals.Glasses3D();

            // Set capabilities
            this.Machine = MachineType.MasterSystem2;

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
            this.VideoProcessor.Reset();
            this.SoundGenerator.Reset();
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
            this.VideoProcessor.RasteriseLine();
        }

        public void RunFrame() {
            this.VideoProcessor.RunFramePending = false;
            while (!this.VideoProcessor.RunFramePending) {
                this.VideoProcessor.RasteriseLine();
                this.FetchExecute(228);
            }
        }
    }
}
