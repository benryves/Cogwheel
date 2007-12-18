using System;
using System.Collections.Generic;
using System.Text;

namespace BeeDevelopment.Sega8Bit.Devices {
    public partial class VideoDisplayProcessor {

        public bool FlagFrameInterruptPending;

        public bool FlagLineInterruptPending;

        public bool FlagSpriteOverflow;
        public bool FlagSpriteCollision;

        private int LineInterruptCounter;

        private int InvalidSpriteIndex;


        private void UpdateIRQ() {
            this.cpu.PinInterrupt =
                (VariableLineInterruptEnable && FlagLineInterruptPending) ||
                (VariableFrameInterruptEnable && FlagFrameInterruptPending);
            //Console.WriteLine(this.cpu.InterruptPin);
        }


    }
}
