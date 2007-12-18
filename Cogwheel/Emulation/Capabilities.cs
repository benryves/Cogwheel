using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Emulation {
    public partial class Emulator {

        private enum MemoryModelType {
            Sg1000,
            Sf7000,
            SegaMasterSystem,
        }

        private enum HardwareModelType {
            Sg1000,
            Sc3000,
            Sf7000,
            SegaMasterSystem,
            SegaGameGear,
        }

        private MemoryModelType CapsMemoryModel = MemoryModelType.SegaMasterSystem;
        public int x() { return (CapsMemoryModel == MemoryModelType.SegaMasterSystem) ? 1 : 0; }

        private HardwareModelType CapsHardwareModel = HardwareModelType.SegaMasterSystem;

    }
}
