using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Devices {
    public partial class VDP {

        private Emulation.MachineType CurrentMachineType = Cogwheel.Emulation.MachineType.MasterSystem2;

        public Emulation.MachineType MachineType {
            set {
                
                this.CurrentMachineType = value;

                bool SmsClass = !(value == Emulation.MachineType.Sg1000 || value == Emulation.MachineType.Sc3000 || value == Cogwheel.Emulation.MachineType.Sf7000);

                this.CapSupportsLineInterrupts = SmsClass;
                this.CapSupportsMode4 = SmsClass;
                this.CapSupportsExtendedPalette = value == Emulation.MachineType.GameGear;
                this.CapSupportsExtendedResolutions = SmsClass && value != Emulation.MachineType.MasterSystem;
                this.CapCroppedScreen = value == Emulation.MachineType.GameGear || value == Emulation.MachineType.GameGearMasterSystem;
                this.CapFixedPalette = !(value == Emulation.MachineType.GameGear || value == Emulation.MachineType.GameGearMasterSystem);

                this.CapFixedPaletteIndex = SmsClass ? 1 : 0;

                this.CapMaxSpritesPerScaline = SmsClass ? 8 : 4;
                this.CapMaxZoomedSpritesPerScanline = SmsClass ? (value == Cogwheel.Emulation.MachineType.MasterSystem ? 4 : 8) : 0;
            }

            get {
                return this.CurrentMachineType;
            }
        }

        private bool CapSupportsLineInterrupts = true;
        private bool CapSupportsMode4 = true;

        private bool CapSupportsExtendedPalette = false;
        private bool CapSupportsExtendedResolutions = false;

        private bool CapCroppedScreen = false;

        private bool CapFixedPalette = false;

        private int CapFixedPaletteIndex = 0;

        private int CapMaxSpritesPerScaline = 8;
        private int CapMaxZoomedSpritesPerScanline = 8;

    }
}
