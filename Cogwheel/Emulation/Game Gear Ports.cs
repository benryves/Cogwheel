using System;
using System.Collections.Generic;
using System.Text;

namespace Cogwheel.Emulation {
    public partial class Sega8Bit {

        private byte Port2 = 0;

        private void WriteGameGearPort(int port, byte value) {
            switch (port) {
                case 0x02:
                    Port2 = value;
                    break;
            }
        }

        private byte ReadGameGearPort(int port) {
            switch (port) {
                case 0x00: {
                    // D7: STT  - Start/Pause button (0 = on, 1 = off).
                    // D6: NJAP - 0: Domestic (Japan), 1: Overseas.
                    // D5: NNTS - 0: NTSC, 1: PAL.
                        return (byte)(
                            (this.ButtonStart ? 0x00 : 0x80) |
                            (this.IsJapanese ? 0x00 : 0x40) |
                            (this.VideoProcessor.VideoStandard == Cogwheel.Devices.VideoDisplayProcessor.VideoStandardType.NTSC ? 0x00 : 0x20)
                        );
                    }
                case 0x01: return 0x7F;
                case 0x02: return this.Port2;
                case 0x03: return 0x00;
                case 0x04: return 0xFF;
                case 0x05: return 0x00;
                case 0x06: return 0xFF;
            }
            return 0xFF;
        }

    }
}
