using System;
using System.Collections.Generic;
using System.Text;

namespace Brazil {
    public partial class Z80A {

        private bool flipFlopIFF1 = false;
        private bool flipFlopIFF2 = false;
        private int interruptMode = 1;
        private bool pinInterrupt = false;
        private bool pinNonMaskableInterrupt = false;

        private bool NmiRequested = false;
        private bool IntRequested = false;


        public bool PinInterrupt {
            get { return pinInterrupt; }
            set { if (value && !pinInterrupt) IntRequested = true; pinInterrupt = value; }
        }

        public bool PinNonMaskableInterrupt {
            get { return pinNonMaskableInterrupt; }
            set { if (value && !pinNonMaskableInterrupt) NmiRequested = true; pinNonMaskableInterrupt = value; }
        }

        private void ResetInterrupts() {
            flipFlopIFF1 = false;
            flipFlopIFF2 = false;
            pinInterrupt = false;
            interruptMode = 1;
            PendingEI = false;
            NmiRequested = false;
            IntRequested = false;
        }

        public bool FlipFlopIFF1 {
            get { return this.flipFlopIFF1; }
            set { this.flipFlopIFF1 = value; }
        }

        public bool FlipFlopIFF2 {
            get { return this.flipFlopIFF2; }
            set { this.flipFlopIFF2 = value; }
        }

        public int InterruptMode {
            get { return this.interruptMode; }
            set { if (value < 0 || value > 2) throw new ArgumentOutOfRangeException(); this.interruptMode = value; }
        }

        public int InterruptPackedStatus {
            get {
                return (this.FlipFlopIFF1 ? 0x01 : 0)
                     + (this.FlipFlopIFF2 ? 0x02 : 0)
                     + (this.PinInterrupt ? 0x04 : 0)
                     + (this.PinNonMaskableInterrupt ? 0x08 : 0)
                     + (this.IntRequested ? 0x10 : 0)
                     + (this.NmiRequested ? 0x20 : 0)
                     + (this.PendingEI ? 0x40 : 0)
                     + (this.InterruptMode << 8);
            }
            set {
                this.FlipFlopIFF1 = (value & 0x01) != 0;
                this.FlipFlopIFF2 = (value & 0x02) != 0;
                this.PinInterrupt = (value & 0x04) != 0;
                this.PinNonMaskableInterrupt = (value & 0x08) != 0;
                this.IntRequested = (value & 0x10) != 0;
                this.NmiRequested = (value & 0x20) != 0;
                this.PendingEI = (value & 0x40) != 0;
                this.InterruptMode = (value >> 8) & 3;                
            }

        }


    }
}
