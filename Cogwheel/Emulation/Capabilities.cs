using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Cogwheel.Emulation {
    public partial class Sega8Bit {

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
